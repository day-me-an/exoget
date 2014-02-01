using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Collections.Specialized;
using Exo.Media.Video;
using System.Threading;
using Exo.Media.image;
using DistribuJob.Client;
using DistribuJob.Client.Extracts;
using Exo.Collections;
using Exo.Web;
using System.Diagnostics;

namespace DistribuJob.Client.Processors
{
    class DirectshowExtractor : Processor
    {
        public DirectshowExtractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            /*using (DirectshowMediaExtractor directshowExtractor = new DirectshowMediaExtractor())
            {
                directshowExtractor.Open(job.FilePath);
                job.MediaExtract.Duration = (uint)directshowExtractor.Length.TotalSeconds;
            }*/

            using (DirectshowMediaExtractor directshowExtractor = new DirectshowMediaExtractor())
            {
                try
                {
                    directshowExtractor.Open(job.FilePath);
                }
                catch
                {
                    if (directshowExtractor != null)
                        directshowExtractor.Dispose();

                    job.Extract = null;

                    return;
                }

                job.MediaExtract.MediaType = directshowExtractor.HasVideo ? MediaType.Video : MediaType.Audio;

                if (job.MediaExtract.MediaType == MediaType.Audio)
                {
                    if (directshowExtractor != null)
                        directshowExtractor.Dispose();

                    return;
                }

                if (job.Format != DocumentFormat.MsMedia)
                {
                    job.MediaExtract.Bitrate = directshowExtractor.Bitrate;

                    if (job.readToEnd || job.Format == DocumentFormat.Realmedia)
                        job.MediaExtract.Duration = (uint)directshowExtractor.Length.TotalSeconds;

                    else if (job.ContentLength > 0 && job.MediaExtract.Bitrate > 0)
                        job.MediaExtract.Duration = (uint)(job.ContentLength / (job.MediaExtract.Bitrate / 8));
                }

                if (job.MediaExtract.MediaType == MediaType.Video)
                {
                    directshowExtractor.GetDimensions(out job.MediaExtract.width, out job.MediaExtract.height);

                    if (directshowExtractor.Length != TimeSpan.Zero
                        && job.MediaExtract.width > 0 && job.MediaExtract.width <= 1024
                        && job.MediaExtract.height > 0 && job.MediaExtract.height <= 768)
                    {
                        Image previewImage = null;

                        try
                        {
                            previewImage = directshowExtractor.GetFrame(1, directshowExtractor.Size);
                        }
                        catch
                        {
                            job.MediaExtract.width = 0;
                            job.MediaExtract.height = 0;
                        }

                        if (previewImage != null)
                            ImageManipulator.CreateImage(previewImage, out job.MediaExtract.image);
                    }
                    else
                    {
                        job.MediaExtract.width = 0;
                        job.MediaExtract.height = 0;
                    }
                }
            }
        }

        public override void QueueControl(Job job)
        {
            Console.WriteLine(job.Extract);

            if (job.Format == DocumentFormat.Realmedia)
                Dj.Queues.exifToolExtractor.Enqueue(job);

            else if (job.Format == DocumentFormat.MsMedia || job.MediaExtract.MediaType == MediaType.Audio)
                Dj.Queues.wmfExtractor.Enqueue(job);

            else
                Dj.Queues.exports.Enqueue(job);
        }
    }
}