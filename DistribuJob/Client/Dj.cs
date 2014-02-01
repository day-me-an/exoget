using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Windows.Forms;
using DistribuJob.Client.Net;
using DistribuJob.Client.Processors;
using DistribuJob.Client.Properties;
using DistribuJob.Server;
using Exo.Collections;
using Exo.Misc;
using Exo.Remoting;
using Timer = System.Threading.Timer;

namespace DistribuJob.Client
{
    public class Dj
    {
        public static class Queues
        {
            public static readonly IQueue<Job> 
                htmlExtractor = new BlockingQueue<Job>(100),
                feedExtractor = new BlockingQueue<Job>(256),
                playlistExtractor = new BlockingQueue<Job>(),
                id3Extractor = new BlockingQueue<Job>(),
                wmfExtractor = new BlockingQueue<Job>(50),
                directshowExtractor = new BlockingQueue<Job>(4),
                quicktimeExtractor = new BlockingQueue<Job>(10),
                //realmediaExtractor = new BlockingQueue<Job>(4),
                swfExtractor = new BlockingQueue<Job>(),
                imageManipulator = new BlockingQueue<Job>(),
                exifToolExtractor = new BlockingQueue<Job>(4),
                exports = new ExportQueue();

            public static readonly FetchQueue
                fetcher = new FetchQueue(),
                fetcherSlow = new FetchQueue();
        }

        private static Djs djs;
        private static readonly Servers servers = new Servers();
        private static readonly ProcessorDictionary processors = new ProcessorDictionary();

        private static Timer importJobsTimer, exportJobsTimer, checkInternetAvailabilityTimer;

        private static object importJobsLock = new object(), exportJobsLock = new object(), checkInternetAvailabilityLock = new object();

        private delegate void FirstImportHandler();
        private static event FirstImportHandler FirstImport;
        private static bool doneFirstImport;

        private static DateTime lastExportTime = DateTime.Now;
        private static bool paused = false;

        static Dj()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnApplicationExit);

#if !DEBUG
            Console.Title = "DistribuJob Client " + Application.ProductVersion;
            Console.WriteLine(">>> Running Release Version <<<");
#else
            Console.Title = "DistribuJob Client " + Application.ProductVersion + " (DEBUG)";
            Console.WriteLine(">>> Running Debug Version <<<");
#endif
            Console.ReadLine();

            try
            {
                IClientChannelSinkProvider clientSinkProvider = new BinaryClientFormatterSinkProvider();
                clientSinkProvider.Next = new CompressionClientSinkProvider(Dns.GetHostName() != "C31370-85321");

                TcpChannel channel = new TcpChannel(null, clientSinkProvider, null);
                ChannelServices.RegisterChannel(channel, false);

                djs = (Djs)Activator.GetObject(typeof(Djs), "tcp://207.218.202.18:8001/Djs");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Could not connect to DistribuJob Sever:\n" + ex);
                throw;
            }
        }

        [MTAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Exo\DistribuJob Client";

#region Debug Init
            TraceListener debugTraceListenerConsole = new TextWriterTraceListener(Console.Out);
            debugTraceListenerConsole.TraceOutputOptions = TraceOptions.DateTime;
            Debug.Listeners.Add(debugTraceListenerConsole);

            TraceListener debugTraceListenerFile = new DelimitedListTraceListener(@"debug\djc debug " + DateTime.Now.ToString("yy-MM-HH-mm-ss") + ".log");
            debugTraceListenerFile.TraceOutputOptions = TraceOptions.DateTime;
            Debug.Listeners.Add(debugTraceListenerFile);

            Debug.AutoFlush = true;
#endregion

#region Trace Init
            TraceListener traceTraceListenerConsole = new TextWriterTraceListener(Console.Out);
            traceTraceListenerConsole.TraceOutputOptions = TraceOptions.DateTime;
            Trace.Listeners.Add(traceTraceListenerConsole);

            TraceListener traceTraceListenerFile = new DelimitedListTraceListener(@"trace\djc trace " + DateTime.Now.ToString("yy-MM-HH-mm-ss") + ".log");
            traceTraceListenerFile.TraceOutputOptions = TraceOptions.DateTime;
            Trace.Listeners.Add(traceTraceListenerConsole);
            Trace.AutoFlush = true;
#endregion

            if (Directory.Exists(@"b:\docs"))
                Directory.Delete(@"b:\docs", true);

            Directory.CreateDirectory(@"b:\docs");

            HttpWebRequest.DefaultCachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.BypassCache);
            //HttpWebRequest.DefaultWebProxy = new WebProxy("127.0.0.1", 8888);
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.MaxServicePoints = 0;
            ServicePointManager.MaxServicePointIdleTime = 5000;

            //checkInternetAvailabilityTimer = new Timer(new TimerCallback(CheckInternetAvailability), checkInternetAvailabilityLock, 2000, 5000);

#if !DEBUG || DEBUG_WITH_IMPORT_EXPORT
            FirstImport += StartProcessors;

            importJobsTimer = new Timer(new TimerCallback(ImportJobs), importJobsLock, 0, 5000);
            exportJobsTimer = new Timer(new TimerCallback(ExportJobs), exportJobsLock, 0, 5000);
#else
            StartProcessors();
#endif

#if !DEBUG_WITH_IMPORT_EXPORT && DEBUG
            Job job = new Job(840944);

            //http://www2.b3ta.com/mp3/Gimme%20Gimme%20Gimme%20L.S.D.mp3
            //http://www.youtube.com/watch?v=yuCArfy_fy0
            //http://www.holylemon.com/BritishFighting.html
            //http://www.esato.com/ringtones/polyphonic/index.php/c=50+Cent,id=19806
            //http://www.leftwingrecords.com/mp3s.html
            //http://www.the-scribe.com/ac/articles/pledge.htm
            //http://anthro.palomar.edu/adapt/adapt_4.htm
            //http://www.the-scribe.com/ac/articles/pledge.htm

            job.Uri = new Uri("http://img671.libsyn.com/img671/8573146227e0685b61b35b93a4daa7e4/468c5565/1830/7328/episode_9.mp3", UriKind.Absolute);
            job.ServerId = djs.GetServerId(job.Uri);

            Queues.fetcher.Enqueue(job);
#endif
            for (; ; )
                Thread.Sleep(1000);

            /*
            ConsoleKeyInfo cki;

            do
            {
                cki = Console.ReadKey();

                if (cki.Modifiers == ConsoleModifiers.Control)
                {
                    switch (cki.Key)
                    {
                        case ConsoleKey.I:
                            ImportJobs(importJobsLock);
                            break;

                        case ConsoleKey.E:
                            ExportJobs();
                            break;

                        case ConsoleKey.P:
                            Fetcher.ChangeAllFetchersState(!paused);
                            paused = !paused;
                            break;

                        case ConsoleKey.H:
                            Console.WriteLine("Hello");
                            break;
                    }
                }

            } while (cki.Key != ConsoleKey.Escape);
            */
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.ExceptionObject);
        }

        private static void StartProcessors()
        {
#if DEBUG_WITH_IMPORT_EXPORT || !DEBUG
            for (ushort i = 0; i < Settings.Default.Processor_Fetcher_Threads; i++)
#endif
                processors.Add(new Fetcher(Queues.fetcher, 0, false));

#if DEBUG_WITH_IMPORT_EXPORT || !DEBUG
            for (ushort i = 0; i < Settings.Default.Processor_Fetcher_Slow_Threads; i++)
#endif
                processors.Add(new Fetcher(Queues.fetcherSlow, 0, true));

            processors.Add(new HtmlExtractor(Queues.htmlExtractor, 0));
            processors.Add(new FeedExtractor(Queues.feedExtractor));
            processors.Add(new Id3Extractor(Queues.id3Extractor));
            processors.Add(new PlaylistExtractor(Queues.playlistExtractor));

            for (ushort i = 0; i < 1; i++)
                processors.Add(new DirectshowExtractor(Queues.directshowExtractor));
            
            processors.Add(new WindowsMetadataExtractor(Queues.wmfExtractor));
            
            //processors.Add(new RealmediaExtractor(Queues.realmediaExtractor));

            for (ushort i = 0; i < 3; i++)
                processors.Add(new ExifToolExtractor(Queues.exifToolExtractor));
            
            processors.Add(new QuicktimeExtractor(Queues.quicktimeExtractor));
            processors.Add(new ImageManipulator(Queues.imageManipulator));
        }

        private static void ImportJobs(object state)
        {
            lock (importJobsLock)
            {
                if (Queues.fetcher.Count < Settings.Default.MinImports)
                {
                    Debug.Print("importing");

                    Job[] jobs = djs.Import(Settings.Default.MaxImports - Queues.fetcher.Count);

                    if (jobs == null || jobs.Length == 0)
                        return;

                    Queues.fetcher.Enqueue(jobs);

                    Debug.Print("Imported Count: {0}", jobs.Length);

                    if (!doneFirstImport)
                    {
                        doneFirstImport = true;
                        FirstImport();
                    }

#if DEBUG && !DEBUG_WITH_IMPORT_EXPORT
                    importJobsTimer.Dispose();
#endif
                }
            }
        }

        private static void ExportJobs(object state)
        {
            lock (exportJobsLock)
            {
                if (Queues.exports.Count >= Settings.Default.MaxExports
                    || (Queues.exports.Count > 0 && lastExportTime != null && (DateTime.Now - lastExportTime) >= Settings.Default.MaxExportIdle))
                {
                    ExportJobs();
                }
            }
        }

        private static void ExportJobs()
        {
            if (Queues.exports.Count > 0)
            {
                Debug.Print("export start");

                Job[] jobs = Queues.exports.DequeueAllToArray();

                djs.Export(jobs);
                lastExportTime = DateTime.Now;

                Debug.Print("Exported {0}",
                    jobs.Length);
            }
        }

        private static void CheckInternetAvailability(object state)
        {
            lock (state)
            {
                bool isConnected = ExoUtil.IsConnectedToInternet();

                if (!isConnected)
                    Trace.TraceInformation("Internet connection lost");

                List<Processor> fetchers = processors[typeof(Fetcher)];

                if (!paused && fetchers != null)
                    Fetcher.ChangeAllFetchersState(!isConnected);
            }
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            Console.WriteLine("####################### Application Exit #######################");

            ExportJobs();
        }

        public static Djs Djs
        {
            get { return djs; }
        }

        public static ProcessorDictionary Processors
        {
            get { return processors; }
        }

        public static Servers Servers
        {
            get { return servers; }
        }
    }
}