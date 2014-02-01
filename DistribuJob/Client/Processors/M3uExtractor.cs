using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Exo.Collections;

namespace DistribuJob.Client.Processors
{
    class M3uExtractor : Processor
    {
        public M3uExtractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            List<string> links = new List<string>();

            using (StreamReader sr = new StreamReader(job.FileReadStream))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.StartsWith("#"))
                        continue;

                    links.Add(line);
                }
            }

            //job.MediaPlaylistExtract.MediaExtractLinks = links.ToArray();
        }

        public override void QueueControl(Job job)
        {
            Dj.Queues.exports.Enqueue(job);
        }
    }
}