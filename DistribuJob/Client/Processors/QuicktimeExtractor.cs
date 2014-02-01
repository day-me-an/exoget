using System;
using System.Collections.Generic;
using System.Text;
using Exo.Media;
using System.Drawing;
using QTOLibrary;
using Exo.Media.image;
using DistribuJob.Client;
using DistribuJob.Client.Extracts;
using Exo.Collections;
using Exo.Web;
using System.Drawing.Imaging;
using Exo.Misc;
using System.Diagnostics;

namespace DistribuJob.Client.Processors
{
    class QuicktimeExtractor : Processor
    {
        public QuicktimeExtractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            Quicktime quicktime;

        retry:
            try
            {
                quicktime = new Quicktime();
            }
            catch
            {
                System.Threading.Thread.Sleep(250);

                Trace.TraceError("{0}, {1}: failed to create new Quicktime()", Name, job);

                goto retry;
            }

            try
            {
                quicktime.Open(job.FilePath);

                job.MediaExtract.Bitrate = quicktime.GetBitrate();

                if (job.readToEnd)
                    job.MediaExtract.Duration = (uint)(quicktime.movie.Duration / quicktime.movie.TimeScale);

                else if (job.ContentLength > 0)
                    job.MediaExtract.Duration = (uint)(job.ContentLength / (job.MediaExtract.Bitrate / 8));

                job.MediaExtract.Title = TextUtil.Clean(quicktime.GetAnnotation(QTAnnotationsEnum.qtAnnotationFullName));
                job.MediaExtract.Author = TextUtil.Clean(quicktime.GetAnnotation(QTAnnotationsEnum.qtAnnotationArtist));
                job.MediaExtract.Album = TextUtil.Clean(quicktime.GetAnnotation(QTAnnotationsEnum.qtAnnotationAlbum));
                job.MediaExtract.Description = TextUtil.Clean(quicktime.GetAnnotation(QTAnnotationsEnum.qtAnnotationDescription) + " " + quicktime.GetAnnotation(QTAnnotationsEnum.qtAnnotationComments));
                job.MediaExtract.Genre = TextUtil.Clean(quicktime.GetAnnotation(QTAnnotationsEnum.qtAnnotationGenre));

                job.MediaExtract.Width = quicktime.movie.Width;
                job.MediaExtract.Height = quicktime.movie.Height;

                if (job.MediaExtract.Width > 0 && job.MediaExtract.Height > 0)
                {
                    job.MediaExtract.MediaType = MediaType.Video;

                    Image preview = quicktime.GetFrameAtPosition(0, quicktime.Width, quicktime.Height);

                    if (preview != null)
                        ImageManipulator.CreateImage(preview, out job.MediaExtract.image);
                }
                else
                    job.MediaExtract.MediaType = MediaType.Audio;
            }
            catch (Exception e)
            {
                job.Extract = null;

                Trace.TraceError("{0}, {1}:{2}", Name, job, e);
            }
            finally
            {
                if (quicktime != null)
                    quicktime.Dispose();

                if (job.MediaExtract.MediaType == MediaType.None)
                {
                    if (job.Format == DocumentFormat.Mpeg)
                        job.MediaExtract.MediaType = MediaType.Video;

                    else
                        job.MediaExtract.MediaType = MediaType.Audio;
                }
            }
        }

        public override void QueueControl(Job job)
        {
            Console.WriteLine(job.Extract);

            Dj.Queues.exports.Enqueue(job);
        }
    }
}
