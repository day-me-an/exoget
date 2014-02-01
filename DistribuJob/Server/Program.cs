using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace DistribuJob.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            RemotingConfiguration.Configure("Djs.exe.config", false);

            Environment.CurrentDirectory = @"C:\Program Files\exo\DistribuJob Server";

            Debug.AutoFlush = true;
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            // until connector 5.0.8
            //Debug.Listeners.Add(new DelimitedListTraceListener("debug.log"));

            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.Listeners.Add(new DelimitedListTraceListener("/traces/trace_" + DateTime.Now.ToString("yy-MM-HH-mm-ss") + ".log"));

#if !DEBUG
            Console.Title = "DistribuJob Server " + Application.ProductVersion;
#else
            Console.Title = "DistribuJob Server " + Application.ProductVersion + " (DEBUG)";
#endif

            Console.WriteLine("Press enter twice to stop the server...");
            Console.ReadLine();
            Console.ReadLine();
        }
    }
}