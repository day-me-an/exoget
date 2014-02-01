using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using DistribuJob.Client.Extracts;
using DistribuJob.Client.Net;
using DistribuJob.Client.Properties;
using Exo.Collections;
using Exo.IO;
using Exo.Misc;
using Exo.Web;

namespace DistribuJob.Client.Processors
{
    partial class Fetcher : Processor
    {
        private const int ReadLimitText = 2097152; // 2mb 512kb=524288
        private const int ReadLimitMedia = 629145;
        private const int ReadLimitImage = 3145728; // 3mb
        private const int Mp3TestLength = 4096;
        private const int Mp3VbrReadLimit = 524416;
        private const int Mp3ApicReadLimit = 98304; // 96kb

        private static readonly byte[]
            id3HeaderBytes =
            { 
                (byte)'I',
                (byte)'D',
                (byte)'3'
            },
            xingHeaderBytes =
            {
                (byte)'X',
                (byte)'i',
                (byte)'n',
                (byte)'g'
            },
            apicHeaderBytes =
            {
                (byte)'A',
                (byte)'P',
                (byte)'I',
                (byte)'C'
            },
            jpgHeaderBytes =
            {
                (byte)'J',
                (byte)'P',
                (byte)'G'
            };

        public readonly bool isSlowFetcher;

        public Fetcher(IQueue<Job> queue, int startDelay, bool isSlowFetcher)
            : base(queue, startDelay)
        {
            this.isSlowFetcher = isSlowFetcher;
        }

        public override void Process(Job job)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(job.Uri);

            httpRequest.KeepAlive = false;
            httpRequest.Timeout = job.Timeout;
            httpRequest.ReadWriteTimeout = job.Timeout;
            httpRequest.UserAgent = String.Format(Settings.Default.WebUseragent, System.Windows.Forms.Application.ProductVersion);
            httpRequest.Referer = job.Server.Uri.ToString();

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
            Stream responseStream = null, fileStream = null;

            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                job.FetchStatus = FetchStatus.Success;

                if (!job.IsFetched)
                {
                    if (httpResponse.ContentType != null)
                    {
                        string contentType;

                        if (httpResponse.ContentType.IndexOf(';') != -1)
                            contentType = httpResponse.ContentType.Split(';')[0];

                        else
                            contentType = httpResponse.ContentType;

                        job.Format = UriUtil.GetFormatFromMimetype(contentType);
                    }
                    else
                        job.Format = UriUtil.GetFormatFromUri(job.Uri);

                    if (job.Format == DocumentFormat.Uncertain)
                        job.Format = UriUtil.GetFormatFromUri(job.Uri);

                    if (job.Format == DocumentFormat.Unknown)
                    {
                        job.FetchStatus = FetchStatus.ErrorFormatUnknown;
                        httpResponse.Close();

                        return;
                    }

                    if (httpResponse.CharacterSet != null)
                        Exo.Misc.ExoUtil.TryGetEncoding(httpResponse.CharacterSet, out job.encoding);

#region Redirection
                    if (httpResponse.ResponseUri != job.Uri)
                    {
                        /*
                         * hack for artificial media sites (myspace, youtube etc) that redirect when an item has been removed
                         */
                        if (job.ServerId == 653474)
                        {
                            job.FetchStatus = FetchStatus.ErrorHttpNotFound;
                            return;
                        }
                        /*
                         * end hack
                         */

                        if (httpResponse.ResponseUri.Authority != job.Uri.Authority)
                        {
                            uint responseServerId;

                            if (!Dj.Djs.TryGetServerId(httpResponse.ResponseUri, out responseServerId))
                            {
                                job.FetchStatus = FetchStatus.ErrorIllegalRedirect;
                                httpResponse.Close();

                                return;
                            }
                            else
                                job.ServerId = responseServerId;
                        }

                        Uri redirectedUri = httpResponse.ResponseUri;

                        if (!UriUtil.TryValidateUri(ref redirectedUri, job.Server.Uri, job.Uri))
                        {
                            job.FetchStatus = FetchStatus.ErrorIllegalRedirect;

                            throw new InvalidDataException(String.Format("Redirect to uri {0} from {1} was invalid", redirectedUri, job.Uri));
                        }

                        if (!redirectedUri.IsAbsoluteUri)
                        {
                            redirectedUri = new Uri(job.Server.Uri, redirectedUri);
                        }

                        job.Uri = redirectedUri;
                    }
#endregion
                }

                responseStream = httpResponse.GetResponseStream();

                if (job.Format == DocumentFormat.MpegAudio3 && !job.HasFetchRanges)
                {
                    byte[] readTestBytes = new byte[Mp3TestLength];
                    int readTextRead = responseStream.Read(readTestBytes, 0, readTestBytes.Length);

                    if (readTextRead < readTestBytes.Length)
                        Array.Resize<byte>(ref readTestBytes, readTextRead);

                    if (Exo.Array.Contains(xingHeaderBytes, readTestBytes))
                    {
                        job.MediaExtract.isVbr = true;
                        job.readLimit = Mp3VbrReadLimit;
                    }

                    if (httpResponse.Headers["Accept-Ranges"] != "none" && !Exo.Array.Contains(id3HeaderBytes, readTestBytes, 0, 3))
                    {
                        job.MediaExtract.id3Type = Id3Type.Id3_1;

                        int firstPartReadLimit = job.MediaExtract.isVbr ? Mp3VbrReadLimit : 20480;

                        byte[] firstPart = new byte[firstPartReadLimit];
                        Array.ConstrainedCopy(readTestBytes, 0, firstPart, 0, readTestBytes.Length);

                        int firstPartRead = responseStream.Read(firstPart, readTestBytes.Length, firstPart.Length - readTestBytes.Length);

                        if (firstPartRead < firstPart.Length - readTestBytes.Length)
                            Array.Resize<byte>(ref firstPart, readTestBytes.Length + firstPartRead);

                        job.pendingBytesAtBeginning = firstPart;

                        if (job.pendingBytesAtBeginning.Length < httpResponse.ContentLength)
                        {
                            job.ContentLength = httpResponse.ContentLength;

                            if (job.Uri.Query == null)
                                job.LastModifiedDate = httpResponse.LastModified;

                            job.fetchRanges = new int[] { -128 };
                            job.readLimit = 128;

                            httpResponse.Close();
                            Process(job);

                            return;
                        }
                    }
                    else
                    {
                        job.MediaExtract.id3Type = Id3Type.Id3_2;
                        job.pendingBytesAtBeginning = readTestBytes;

                        if (job.MediaExtract.isVbr)
                            job.readLimit = Mp3VbrReadLimit;

                        else if (Exo.Array.Contains(apicHeaderBytes, readTestBytes) || Exo.Array.Contains(jpgHeaderBytes, readTestBytes))
                            job.readLimit = Mp3ApicReadLimit;
                    }
                }

                if (!job.HasFetchRanges)
                {
                    job.ContentLength = httpResponse.ContentLength;

                    if (job.Uri.Query == null)
                        job.LastModifiedDate = httpResponse.LastModified;
                }

                fileStream = new BufferedStream(new FileStream(job.FilePath, FileMode.CreateNew, FileAccess.Write));

                int readCount = 0, readByte = 0;

                job.readLimit = GetReadLimit(job);

                /*if (job.ContentLength > 0 && job.ContentLength <= job.readLimit)
                    bytesToReserve = (int)job.ContentLength;

                else
                    bytesToReserve = job.readLimit;

                lock (spaceReserveLock)
                {
                    reservedDiskSpace += bytesToReserve;
                }

                while (reservedDiskSpace >= 31000000)
                    System.Threading.Thread.Sleep(50);*/

                if (job.HasPendingBytesAtBeginning)
                {
                    fileStream.Write(job.pendingBytesAtBeginning, 0, job.pendingBytesAtBeginning.Length);
                    readCount += job.pendingBytesAtBeginning.Length;
                }

                if (job.HasFetchRanges && job.fetchRanges.Length > 1)
                {
                    for (int i = 0; i < job.fetchRanges.Length; i++)
                        job.readLimit += 250;

                    string boundary = httpResponse.ContentType.Substring(httpResponse.ContentType.IndexOf("; boundary=") + 11);

                    responseStream = new MultipartStream(responseStream, new UTF8Encoding().GetBytes(boundary));

                    do
                    {
                        while ((readByte = responseStream.ReadByte()) != -1)
                        {
                            if (readByte == '\r'
                                && responseStream.ReadByte() == '\n'
                                && responseStream.ReadByte() == '\r'
                                && responseStream.ReadByte() == '\n')
                            {
                                break;
                            }
                        }

                        while ((readByte = responseStream.ReadByte()) != -1 && readCount++ <= job.readLimit)
                            fileStream.WriteByte((byte)readByte);

                    } while (((MultipartStream)responseStream).NextPart());
                }
                else
                {
                    responseStream = new BufferedStream(responseStream);

                    while (!job.readToEnd && readCount++ <= job.readLimit)
                    {
                        readByte = responseStream.ReadByte();

                        if (readByte != -1)
                            fileStream.WriteByte((byte)readByte);

                        else
                            job.readToEnd = true;
                    }
                }

                /*if (readCount < job.readLimit)
                {
                    lock (spaceReserveLock)
                    {
                        reservedDiskSpace -= (job.readLimit - readCount);
                        job.readLimit = readCount;
                    }
                }*/

                if (job.ContentLength <= 0 && job.readToEnd)
                    job.ContentLength = readCount;
            }
            catch (WebException e)
            {
                if (Paused)
                {
                    if (httpResponse != null)
                        httpResponse.Close();

                    if (responseStream != null)
                        responseStream.Dispose();

                    if (fileStream != null)
                        fileStream.Dispose();

                    ReQueueCurrentJob();

                    return;
                }

                switch (e.Status)
                {
                    case WebExceptionStatus.Timeout:
                        {
                            job.FetchStatus = FetchStatus.ErrorIOTimedOut;

                            job.Server.consecutiveTimeouts++;

                            if (job.Server.consecutiveTimeouts > Settings.Default.Server_MaxConsecutiveTimeouts)
                            {
                                Dj.Queues.fetcher.MoveAllJobs(job.Server, Dj.Queues.exports, FetchStatus.ErrorIOTimedOut);
                                Dj.Servers.SetServerStatus(job.ServerId, ServerStatus.ErrorIOTimedout);
                            }

                            break;
                        }

                    case WebExceptionStatus.NameResolutionFailure:
                        {
                            job.FetchStatus = FetchStatus.ErrorIODnsResolutionFailed;

                            job.Server.consecutiveLookupFailures++;

                            if (job.Server.consecutiveLookupFailures > Settings.Default.Server_MaxConsecutiveLookupFailures)
                            {
                                Dj.Queues.fetcher.MoveAllJobs(job.Server, Dj.Queues.exports, FetchStatus.ErrorIODnsResolutionFailed);
                                Dj.Servers.SetServerStatus(job.ServerId, ServerStatus.ErrorIODnsResolutionFailed);
                            }

                            break;
                        }

                    default:
                        {
                            if (e.Response != null)
                            {
                                switch (((HttpWebResponse)e.Response).StatusCode)
                                {
                                    case HttpStatusCode.NotModified:
                                        {
                                            if (job.HasLastModified)
                                            {
                                                job.FetchStatus = FetchStatus.WarningHttpNotModified;
                                                break;
                                            }
                                            else
                                                goto default;
                                        }

                                    case HttpStatusCode.NotFound:
                                        job.FetchStatus = FetchStatus.ErrorHttpNotFound;
                                        break;

                                    default:
                                        job.FetchStatus = FetchStatus.ErrorHttpOther;
                                        break;
                                }
                            }
                            else
                                job.FetchStatus = FetchStatus.ErrorIOOther;

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                if (Paused)
                {
                    if (httpResponse != null)
                        httpResponse.Close();

                    if (responseStream != null)
                        responseStream.Dispose();

                    if (fileStream != null)
                        fileStream.Dispose();

                    ReQueueCurrentJob();

                    return;
                }

                Trace.TraceError("{0}, {1}, {2}: {3}", job.Id, job.Format, job.Uri, e.ToString());

                if (job.FetchStatus == FetchStatus.NotFetched
                    || job.FetchStatus == FetchStatus.Success
                    || job.FetchStatus == FetchStatus.WarningHttpNotModified)
                {
                    job.FetchStatus = FetchStatus.ErrorUnknown;
                }
            }
            finally
            {
                if (httpResponse != null)
                    httpResponse.Close();

                if (responseStream != null)
                    responseStream.Dispose();

                if (fileStream != null)
                    fileStream.Dispose();

                if (job.FetchStatus != FetchStatus.Success && job.Format > 0)
                {
                    File.Delete(job.FilePath);
                }
            }
        }

        private int GetReadLimit(Job job)
        {
            if (job.HasReadLimit)
                return job.readLimit;

            else
            {
                switch (job.Format)
                {
                    case DocumentFormat.MpegAudio3:
                        return 20480; //32768 //4096 //12288

                    /*case DocumentFormat.MsMedia:
                        return 531072;*/

                    case DocumentFormat.Mpeg:
                    case DocumentFormat.MsMedia:
                    case DocumentFormat.Avi:
                    case DocumentFormat.Realmedia:
                        return 131072; //131072

                    case DocumentFormat.Quicktime:
                    case DocumentFormat.Mpeg4:
                        return 504800;

                    case DocumentFormat.OggVorbis:
                        return 131072;

                    case DocumentFormat.Flac:
                        return 531072;

                    default:
                        {
                            if (job.Type == DocumentType.Image)
                                return ReadLimitImage;

                            return job.Type == DocumentType.Media ? ReadLimitMedia : ReadLimitText;
                        }
                }
            }
        }

        public static void ChangeAllFetchersState(bool pause)
        {
            foreach (Processor processor in Dj.Processors[typeof(Fetcher)])
                processor.Paused = pause;
        }
    }
}