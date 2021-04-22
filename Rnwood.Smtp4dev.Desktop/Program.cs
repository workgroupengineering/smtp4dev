using Chromely;
using Chromely.Core;
using Chromely.Core.Configuration;
using Gtk;
using Medallion.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace Rnwood.Smtp4dev.Desktop
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //This is used by Chromely to launch child processes
            if (args.Any(a => a.StartsWith("--type=")))
            {
                DesktopApp.Run(args, new Uri("about:blank"));
                return;
            }

            string workingDir = "../../../../Rnwood.Smtp4dev";
            string mainModule = "../../../../Rnwood.Smtp4dev/bin/Debug/netcoreapp3.1/Rnwood.Smtp4dev.exe";

            mainModule = Path.GetFullPath(mainModule);

            Thread outputThread = null;

            Environment.SetEnvironmentVariable("SMTP4DEV_NOHELP", "true");

            using (Command serverProcess = Command.Run(mainModule, new[] { $"--parentprocessid={Process.GetCurrentProcess().Id}", "--urls=http://127.0.0.1:0" }, o => o.DisposeOnExit(false).WorkingDirectory(workingDir)))
            {
                try
                {
                    IEnumerator<string> serverOutput = serverProcess.GetOutputAndErrorLines().GetEnumerator();

                    Uri baseUrl = null;

                    StringBuilder startupOutput = new StringBuilder();

                    while (baseUrl == null && serverOutput.MoveNext())
                    {
                        string newLine = serverOutput.Current;
                        startupOutput.AppendLine(newLine);
                        Console.WriteLine(newLine);

                        if (newLine.StartsWith("Now listening on: http://"))
                        {
                            int portNumber = int.Parse(Regex.Replace(newLine, @".*http://[^\s]+:(\d+)", "$1"));
                            baseUrl = new Uri($"http://localhost:{portNumber}");
                        }
                    }

                    if (baseUrl == null)
                    {
                        ShowFatalError("Failed to start smtp4dev", "Output from startup:\n\n" + startupOutput.ToString());
                    }

                    outputThread = new Thread(() =>
                    {
                        while (serverOutput.MoveNext())
                        {
                            string newLine = serverOutput.Current;
                            Console.WriteLine(newLine);
                        }
                    });
                    outputThread.Start();


                    DesktopApp.Run(args, baseUrl);

                }
                finally
                {
                    bool signalSent = serverProcess.TrySignalAsync(CommandSignal.ControlC).Result;

                    if (!signalSent)
                    {
                        serverProcess.Kill();
                    }

                    serverProcess.Process.WaitForExit();

                    outputThread.Join();

                    DesktopApp.Exit();
                }
            }

        }

        private static void ShowFatalError(string title, string details)
        {
            Console.Error.WriteLine(title);
            Console.Error.WriteLine(details);

            Application.Init();
            MessageDialog errDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, $"<big><b>{title}</b></big>");
            errDialog.SetPosition(Gtk.WindowPosition.Center);
            errDialog.SetIconFromFile("icon.ico");
            errDialog.Title = "smtp4dev";
            errDialog.SecondaryText = details;
            errDialog.UseMarkup = true;
            errDialog.Response += (s, ea) => Application.Quit();
            errDialog.Show();
            Application.Run();
            Environment.Exit(1);
        }

        private static void ErrDialog_Response(object o, ResponseArgs args)
        {
            throw new NotImplementedException();
        }
    }

         

}
