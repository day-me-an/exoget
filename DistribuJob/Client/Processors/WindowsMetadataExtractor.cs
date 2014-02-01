using System;
using DistribuJob.Client.Extracts;
using Exo.Collections;
using Exo.Extensions;
using Exo.Media;
using Exo.Misc;
using Exo.Web;

namespace DistribuJob.Client.Processors
{
    class WindowsMetadataExtractor : Processor
    {
        public WindowsMetadataExtractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            WmfExtractor wmf = null;

            try
            {
                wmf = new WmfExtractor();
                wmf.Open(job.FilePath);

                if (job.MediaExtract.MediaType == MediaType.None)
                    job.MediaExtract.MediaType = wmf.GetAttributeAsBool("HasVideo") ? MediaType.Video : MediaType.Audio;

                job.MediaExtract.Bitrate = wmf.GetAttributeAsUInt("Bitrate");

                if (job.readToEnd)
                    job.MediaExtract.Duration = wmf.GetAttributeAsUInt("Duration") / 10000000;

                else if (job.ContentLength > 0)
                    job.MediaExtract.Duration = (uint)(job.ContentLength / (job.MediaExtract.Bitrate / 8));

                job.MediaExtract.Title = wmf.GetAttributeAsString("Title").Clean();

                string desc = wmf.GetAttributeAsString("Description");

                if (!String.IsNullOrEmpty(desc))
                    job.MediaExtract.Description = desc.Clean();

                job.MediaExtract.Transcript = wmf.GetAttributeAsString("WM/Lyrics").Clean();
                job.MediaExtract.Author = wmf.GetAttributeAsString("Author").Clean();
                job.MediaExtract.Album = wmf.GetAttributeAsString("WM/AlbumTitle").Clean();
                job.MediaExtract.Year = wmf.GetAttributeAsString("WM/Year").Clean();
                job.MediaExtract.Genre = wmf.GetAttributeAsString("WM/Genre").Clean();
                job.MediaExtract.Copyright = wmf.GetAttributeAsString("Copyright").Clean();

                byte[] wmPictureBytes = wmf.GetAttribute("WM/Picture");
                ImageInfo image;

                if (wmPictureBytes != null && ImageManipulator.CreateImage(wmPictureBytes, out image))
                    job.MediaExtract.Image = image;
            }
            catch
            {
                job.Extract = null;
            }
            finally
            {
                if (wmf != null)
                    wmf.Close();

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
