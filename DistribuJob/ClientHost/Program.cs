using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ClientHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessStartInfo client = new ProcessStartInfo("dj.exe")
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = false
            };
            
            restart:
            Process clientProcess = Process.Start(client);
            clientProcess.StandardInput.Write((char)ConsoleKey.Enter);
            clientProcess.StandardInput.Write((char)ConsoleKey.Enter);

            clientProcess.WaitForExit();

            goto restart;
        }
    }
}
