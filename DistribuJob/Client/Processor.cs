using System;
using System.Diagnostics;
using System.Threading;
using Exo.Collections;

namespace DistribuJob.Client
{
    public abstract class Processor
    {
        protected readonly IQueue<Job> queue;
        private int instanceId = -1;
        private bool paused = false;
        public Job currentJob;
        protected TimeSpan processDur;

        public Processor(IQueue<Job> queue, int startDelay)
        {
            if (startDelay > 0)
                Thread.Sleep(startDelay);

            this.queue = queue;

            Thread t = new Thread(new ThreadStart(Run));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public Processor(IQueue<Job> queue)
            : this(queue, 0)
        {
        }

        public abstract void Process(Job job);
        public abstract void QueueControl(Job job);

        public void Run()
        {
            Thread.Sleep(300);
            Console.WriteLine("{0} started, queue count = {1}", Name, queue.Count);

            Thread.CurrentThread.Name = Name;

            for (; ; )
            {
                if (paused || queue.Count == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }

#if !DEBUG || DEBUG_WITH_IMPORT_EXPORT
                try
                {
#endif
                currentJob = queue.Dequeue();
#if !DEBUG || DEBUG_WITH_IMPORT_EXPORT
                }
                catch
                {
                    Thread.Sleep(100);
                    continue;
                }
#endif

                if (currentJob == null)
                {
                    Thread.Sleep(100);
                    continue;
                }

                Console.WriteLine("{0}: Processing {1}", Name, currentJob);

                DateTime start = DateTime.Now;

#if !DEBUG || DEBUG_WITH_IMPORT_EXPORT
                try
                {
#endif
                Process(currentJob);
#if !DEBUG || DEBUG_WITH_IMPORT_EXPORT
                }
                catch (Exception e)
                {
                    Trace.TraceError("{0} JobId={1} {2}",
                        Name,
                        currentJob.Id,
                        e);

                    Trace.Flush();

                    throw;
                }
#endif
                processDur = ((TimeSpan)(DateTime.Now - start));

                if (processDur > TimeSpan.FromSeconds(20))
                    Debug.Print("{0}: Slow Process {1} ({2})", Name, currentJob, processDur);

                DateTime queueStart = DateTime.Now;

                try
                {
                    QueueControl(currentJob);
                }
                catch (Exception e)
                {
                    Trace.TraceError("{0} JobId={1} {2}",
                        Name,
                        currentJob.Id,
                        e);

                    Trace.Flush();

                    throw;
                }

                TimeSpan queueDur = ((TimeSpan)(DateTime.Now - queueStart));

                if (queueDur.TotalSeconds > 5)
                {
                    Trace.TraceWarning("{0} slow QueueControl, {1}", Name, queueDur);
                    Trace.Flush();
                }

                Console.WriteLine("{0}: Done {1} ({2} ms)", Name, currentJob, ((TimeSpan)(DateTime.Now - start)).TotalMilliseconds);
            }
        }

        protected void ReQueueCurrentJob()
        {
            if (currentJob != null)
                queue.Enqueue(currentJob);
        }

        public override string ToString()
        {
            return Name;
        }

        public IQueue<Job> Queue
        {
            get { return queue; }
        }

        public virtual string Name
        {
            get { return this.GetType().Name + " - " + (instanceId != -1 ? instanceId : (instanceId = Dj.Processors[this.GetType()].IndexOf(this))); }
        }

        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }
    }
}
