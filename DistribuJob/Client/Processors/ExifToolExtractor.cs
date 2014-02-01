using System;
using System.Collections.Generic;
using System.Text;
using Exo.Collections;
using Exo.Media;
using Exo.Misc;

namespace DistribuJob.Client.Processors
{
    public class ExifToolExtractor : Processor
    {
        public ExifToolExtractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            ExifToolWrapper target = new ExifToolWrapper(job.FilePath);

            if (target.HasProperty("Avg Bit Rate"))
            {
                uint value;

                if (target.TryGetPropertyUInt32("Avg Bit Rate", out value))
                    job.MediaExtract.Bitrate = value;
            }
            else if (target.HasProperty("Bytes Per Minute"))
            {
                uint value;

                if (target.TryGetPropertyUInt32("Bytes Per Minute", out value))
                    job.MediaExtract.Bitrate = (value * 8) / 60;
            }

            if (target.HasProperty("Duration"))
            {
                uint value;

                if (target.TryGetPropertyUInt32("Duration", out value))
                    job.MediaExtract.Duration = value;
            }

            if (target.HasProperty("Title"))
            {
                string value;

                if (target.TryGetProperty("Title", out value))
                    job.MediaExtract.Title = TextUtil.Clean(value);
            }

            if (target.HasProperty("Author"))
            {
                string value;

                if (target.TryGetProperty("Author", out value))
                    job.MediaExtract.Author = TextUtil.Clean(value);
            }
            else if (target.HasProperty("Artist"))
            {
                string value;

                if (target.TryGetProperty("Artist", out value))
                    job.MediaExtract.Author = TextUtil.Clean(value);
            }

            if (target.HasProperty("Copyright"))
            {
                string value;

                if (target.TryGetProperty("Copyright", out value))
                    job.MediaExtract.Copyright = TextUtil.Clean(value);
            }

            if (target.HasProperty("Genre"))
            {
                string value;

                if (target.TryGetProperty("Genre", out value))
                    job.MediaExtract.Genre = TextUtil.Clean(value);
            }

            if (target.HasProperty("Album"))
            {
                string value;

                if (target.TryGetProperty("Album", out value))
                    job.MediaExtract.Album = TextUtil.Clean(value);
            }

            if (target.HasProperty("Abstract"))
            {
                string value;

                if (target.TryGetProperty("Abstract", out value))
                    job.MediaExtract.Description = TextUtil.Clean(value);
            }

            if (target.HasProperty("Keywords"))
            {
                string value;

                if (target.TryGetProperty("Keywords", out value))
                {
                    string[] keywords = value.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                    job.MediaExtract.Keywords.AddRange(keywords);
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
