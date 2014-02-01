using System.Diagnostics;
using System.IO;
using Exo.Collections;

namespace DistribuJob.Client
{
    class ExportQueue : BlockingQueue<Job>
    {
        public override void Enqueue(Job item)
        {
            if (item.Format > 0 && File.Exists(item.FilePath))
            {
            retryDelete:
                try
                {
                    File.Delete(item.FilePath);
                }
                catch
                {
                    System.Threading.Thread.Sleep(250);

                    Trace.TraceWarning("cannot delete {0}", item);
                    goto retryDelete;
                }
            }

            /*lock (Fetcher.spaceReserveLock)
            {
                Fetcher.reservedDiskSpace -= item.readLimit;
            }*/

            base.Enqueue(item);
        }
    }
}