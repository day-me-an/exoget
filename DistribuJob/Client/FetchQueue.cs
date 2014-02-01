using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Exo.Collections;
using SiteServer = DistribuJob.Client.Net.Server;

namespace DistribuJob.Client
{
    public class FetchQueue : IQueue<Job>
    {
        private readonly Dictionary<SiteServer, Queue<Job>> serverToJobs = new Dictionary<SiteServer, Queue<Job>>();
        private readonly List<SiteServer> serverOrder = new List<SiteServer>();

        private object queueLock = new object();

        private volatile int count = 0;

        public FetchQueue()
        {
        }

        #region IQueue<Job> Members

        public void Enqueue(Job item)
        {
            item.Path = Exo.Misc.TextUtil.DecodeHtmlEntities(item.Path);

            SiteServer server = item.Server;
            Queue<Job> jobQueue;

            lock (queueLock)
            {
                if (!serverToJobs.TryGetValue(server, out jobQueue))
                {
                    jobQueue = new Queue<Job>();
                    serverToJobs.Add(server, jobQueue);
                    serverOrder.Add(server);
                }

                jobQueue.Enqueue(item);
                count++;
            }
        }

        public void Enqueue(Job[] items)
        {
            foreach (Job item in /*Exo.Array.Shuffle<Job>(*/items/*)*/)
                Enqueue(item);
        }

        public Job Dequeue()
        {
            Queue<Job> jobs;
            Job job;

            lock (queueLock)
            {
                List<SiteServer>.Enumerator enumerator = serverOrder.GetEnumerator();

                for (int i = 0; ; i++)
                {
                    if (enumerator.MoveNext())
                    {
                        SiteServer server = enumerator.Current;
                        jobs = serverToJobs[server];

                        serverOrder.Remove(server);

                        if (jobs.Count > 0)
                        {
                            serverOrder.Add(server);

                            break;
                        }
                        else
                            serverToJobs.Remove(server);
                    }
                    else if (i % serverOrder.Count == 0)
                        Thread.Sleep(100);
                }

                job = jobs.Dequeue();

                count--;
            }

            return job;
        }

        public Job[] DequeueToArray(int count)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Job[] DequeueAllToArray()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ICollection<Job> Members

        public void Add(Job item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Clear()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Contains(Job item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CopyTo(Job[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Job item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable<Job> Members

        public IEnumerator<Job> GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        public void MoveAllJobs(SiteServer server, IQueue<Job> queue, FetchStatus fetchStatus)
        {
            Queue<Job> jobs;

            lock (queueLock)
            {
                serverToJobs.TryGetValue(server, out jobs);
            }

            if (jobs == null)
                return;

            lock (queueLock)
            {
                serverToJobs.Remove(server);
                serverOrder.Remove(server);

                foreach (Job job in jobs)
                {
                    job.FetchStatus = fetchStatus;

                    queue.Enqueue(job);

                    count--;
                }
            }
        }

        public void MoveAllJobs(SiteServer server, IQueue<Job> queue)
        {
            lock (queueLock)
            {
                Queue<Job> jobs;
                serverToJobs.TryGetValue(server, out jobs);

                if (jobs == null)
                    return;

                serverToJobs.Remove(server);

                foreach (Job job in jobs)
                {
                    queue.Enqueue(job);

                    count--;
                }
            }
        }
    }
}
