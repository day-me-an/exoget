using System;
using System.Collections.Generic;
using System.Text;
using librmffNET;
using DistribuJob.Client;
using DistribuJob.Client.Extracts;
using Exo.Collections;

namespace DistribuJob.Client.Processors
{
    class RealmediaExtractor : Processor
    {
        public RealmediaExtractor(IQueue<Job> queue)
            : base(queue)
        {
        }

        public override void Process(Job job)
        {
            RealMediaFile rmFile = new RealMediaFile(job.FilePath, OpenMode.Reading);
            rmFile.ReadHeaders();

            job.MediaExtract.Title = rmFile.Title;
            job.MediaExtract.Description = rmFile.Comment;
            job.MediaExtract.Author = rmFile.Author;
            job.MediaExtract.Copyright = rmFile.Copyright;

            //if (job.MediaExtract.Duration > 0 && job.ContentLength > 0)
            //job.MediaExtract.Bitrate = ((uint)job.ContentLength * 8) / job.MediaExtract.Duration;
        }

        public override void QueueControl(Job job)
        {
            Console.WriteLine(job.Extract);

            Dj.Queues.exports.Enqueue(job);
        }
    }
}
