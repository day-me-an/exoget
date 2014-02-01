using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Exo.Collections;
using Exo.Misc;
using System.Diagnostics;
using DistribuJob.Client.Extracts;

namespace DistribuJob.Client.Processors
{
    class Id3Extractor : Processor
    {
        public Id3Extractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            throw new NotImplementedException();

            /*TagLib.Flac.File mp3File;

            try
            {
                mp3File = new TagLib.Flac.File(job.FilePath);

                if (job.MediaExtract.MediaType == Exo.Web.MediaType.None)
                    job.MediaExtract.MediaType = Exo.Web.MediaType.Audio;

                job.MediaExtract.Bitrate = (uint)(mp3File.Properties.AudioBitrate * 1000);
                job.MediaExtract.Duration = job.MediaExtract.Bitrate > 0 ? (uint)(job.ContentLength / (job.MediaExtract.Bitrate / 8)) : 0;

                job.MediaExtract.Title = TextUtil.Clean(mp3File.Tag.Title);
                job.MediaExtract.Description = TextUtil.Clean(mp3File.Tag.Comment);
                job.MediaExtract.Transcript = TextUtil.Clean(mp3File.Tag.Lyrics);
                job.MediaExtract.Author = TextUtil.Clean(mp3File.Tag.FirstArtist);
                job.MediaExtract.Album = TextUtil.Clean(mp3File.Tag.Album);
                job.MediaExtract.Genre = TextUtil.Clean(mp3File.Tag.FirstGenre);
                job.MediaExtract.Year = Convert.ToString(mp3File.Tag.Year);

                if (mp3File.Tag.Pictures.Length > 0)
                {
                    ImageInfo image;

                    if (ImageManipulator.CreateImage(mp3File.Tag.Pictures[0].Data.Data, out image))
                        job.MediaExtract.Image = image;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning("TagLib could not load ({0}, {1}, {2}): {3}", job.Id, job.Uri, job.Format, e);

                job.Extract = null;
            }*/
        }

        public override void QueueControl(Job job)
        {
            Console.WriteLine(job.HasExtract ? job.MediaExtract : null);

            Dj.Queues.exports.Enqueue(job);
        }
    }
}
