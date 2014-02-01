using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;

namespace DistribuJobClient.Processors
{
    class Fetcher2
    {
        private const byte MAX_ACTIVE_FETCHES = 4;

        private volatile byte activeFetches = 0;

        public void Process()
        {
            while (true)
            {
                if (activeFetches <= 4)
                {
                    Job job = Dj.Queues.fetch.Dequeue();

                    job.httpRequest = (HttpWebRequest)WebRequest.Create(job.Uri);
                    job.httpRequest.BeginGetRequestStream(new AsyncCallback(DoItem), job);

                    activeFetches++;

                } else
                    Thread.Sleep(50);
            }
        }

        private void DoItem(IAsyncResult result)
        {
            Job job = result.AsyncState as Job;

            activeFetches--;
        }

        private void Fetch(Job job)
        {
            HttpWebRequest httpRequest = job.httpRequest;

            if (!job.HasFetchRanges && job.HasLastModified)
                httpRequest.IfModifiedSince = job.LastModifiedDate;

            if (job.fetchRanges != null)
            {
                foreach (int fetchRange in job.fetchRanges)
                {
                    if (fetchRange > 0)
                        httpRequest.AddRange(0, fetchRange);

                    else
                        httpRequest.AddRange(fetchRange);
                }

                Console.WriteLine(httpRequest.Headers["Range"]);
            }

            HttpWebResponse httpResponse = null;
            Stream responseStream = null;
            Stream fileStream = null;

            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                Console.WriteLine("Response uri: " + httpResponse.ResponseUri);
                job.Uri = httpResponse.ResponseUri;

                job.fetchStatus = Job.FetchStatus.SUCCESS;

                if (job.format == null)
                {
                    if (httpResponse.ContentType != null)
                        job.format = FormatFinder.GetFormatFromMimetype(httpResponse.ContentType.Split(';')[0]);

                    else
                        job.format = FormatFinder.GetFormatFromUri(job.Uri);

                    if (job.format == Job.Format.UNCERTAIN)
                        job.format = FormatFinder.GetFormatFromUri(job.Uri);

                    if (job.format == Job.Format.UNKNOWN)
                    {
                        job.fetchStatus = Job.FetchStatus.ERROR_UNKNOWN_FORMAT;
                        httpResponse.Close();
                        return;
                    }
                }

                responseStream = httpResponse.GetResponseStream();

                if (job.format == Job.Format.MP3 && !job.HasFetchRanges)
                {
                    byte[] readTestBytes = new byte[4096];
                    responseStream.Read(readTestBytes, 0, readTestBytes.Length);

                    if (ExoArray.Contains(xingHeaderBytes, readTestBytes) || ExoArray.Contains(infoHeaderBytes, readTestBytes))
                        job.MediaExtract.isVbr = true;

                    Console.WriteLine("isVbr: " + job.MediaExtract.isVbr);

                    if (!ExoArray.Contains(id3HeaderBytes, readTestBytes, 0, 3))
                    {
                        if (job.MediaExtract.isVbr)
                        {
                            job.fetchRanges = new int[] { 524288, -128 };
                            job.readLimit = 524416;

                        } else
                        {
                            job.fetchRanges = new int[] { 4096, -128 };
                            job.readLimit = 4224;
                        }

                        job.MediaExtract.id3Type = MediaExtract.Id3Type.ID3_1;
                        job.contentLength = httpResponse.ContentLength;

                        if (job.Uri.Query == null)
                            job.LastModifiedDate = httpResponse.LastModified;

                        httpResponse.Close();
                        Fetch(job);
                        return;

                    } else
                    {
                        job.MediaExtract.id3Type = MediaExtract.Id3Type.ID3_2;
                        job.pendingBytesAtBeginning = readTestBytes;

                        if (ExoArray.Contains(apicHeaderBytes, readTestBytes) || job.MediaExtract.isVbr)
                            job.readLimit = 32768;
                    }
                }

                if (!job.HasFetchRanges)
                {
                    job.contentLength = httpResponse.ContentLength;

                    if (job.Uri.Query == null)
                        job.LastModifiedDate = httpResponse.LastModified;
                }

                fileStream = new BufferedStream(new FileStream(job.FilePath, FileMode.CreateNew));

                uint readLimit = GetReadLimit(job);
                uint read = 0;
                int b;

                if (job.HasFetchRanges)
                {
                    for (int i = 0; i < job.fetchRanges.Length; i++)
                        readLimit += 250;

                    string boundary = httpResponse.ContentType.Substring(httpResponse.ContentType.IndexOf("; boundary=") + 11);
                    Console.WriteLine(boundary);

                    responseStream = new MultipartStream(responseStream, new UnicodeEncoding().GetBytes(boundary));

                    do
                    {
                        while ((b = responseStream.ReadByte()) != -1)
                            if (b == '\r' && responseStream.ReadByte() == '\n' && responseStream.ReadByte() == '\r' && responseStream.ReadByte() == '\n')
                                break;

                        while ((b = responseStream.ReadByte()) != -1 && read++ <= readLimit)
                            fileStream.WriteByte((byte)b);

                    } while (((MultipartStream)responseStream).nextStream());

                } else
                {
                    if (job.HasPendingBytesAtBeginning)
                    {
                        fileStream.Write(job.pendingBytesAtBeginning, 0, job.pendingBytesAtBeginning.Length);
                        read += (uint)job.pendingBytesAtBeginning.Length;
                    }

                    //if (readLimit > 0 && readLimit <= MIN_THROTTLING_LENGTH)
                    //{
                    responseStream = new BufferedStream(responseStream);

                    while ((b = responseStream.ReadByte()) != -1 && read++ <= readLimit)
                        fileStream.WriteByte((byte)b);

                    /*} else
                    {
                        while ((b = responseStream.ReadByte()) != -1 && throttle.enforce(1) && throttle.byteCount <= readLimit)
                            fileStream.WriteByte((byte)b);

                        read = (uint)throttle.byteCount;
                    }*/
                }

                if (job.contentLength == -1)
                    job.contentLength = read;

            } catch (WebException e)
            {
                if (Paused)
                {
                    ReQueueCurrentJob();
                    return;
                }

                switch (e.Status)
                {
                    case WebExceptionStatus.Timeout:
                        job.fetchStatus = Job.FetchStatus.ERROR_IO_TIMED_OUT;
                        return;

                    default:
                        {
                            if (e.Response != null)
                            {
                                switch (((HttpWebResponse)e.Response).StatusCode)
                                {
                                    case HttpStatusCode.NotModified:
                                        job.fetchStatus = Job.FetchStatus.ERROR_HTTP_NOT_MODIFIED;
                                        break;

                                    case HttpStatusCode.NotFound:
                                        job.fetchStatus = Job.FetchStatus.ERROR_HTTP_NOT_FOUND;
                                        break;

                                    /*case HttpStatusCode.Moved:
                                    case HttpStatusCode.MovedPermanently:
                                    case HttpStatusCode.Redirect:
                                    case HttpStatusCode.RedirectKeepVerb:
                                    case HttpStatusCode.RedirectMethod:
                                        break;*/

                                    default:
                                        Console.WriteLine(e);
                                        job.fetchStatus = Job.FetchStatus.ERROR_HTTP_OTHER;
                                        break;
                                }

                            } else
                                job.fetchStatus = Job.FetchStatus.ERROR_IO_OTHER;

                            return;
                        }
                }

            } catch (Exception e)
            {
                //if (Paused)
                //{
                    //ReQueueCurrentJob();
                    return;
                //}

                Console.WriteLine(e);

                job.fetchStatus = Job.FetchStatus.ERROR_UNKNOWN;
                return;

            } finally
            {
                if (httpResponse != null)
                    httpResponse.Close();

                if (responseStream != null)
                    responseStream.Dispose();

                if (fileStream != null)
                    fileStream.Dispose();
            }
        }
    }
}
