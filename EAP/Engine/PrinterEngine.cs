using EAP.Config;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace EAP.Engine
{
    public class PrinterEngine : AbstractEngine
    {
        private static ILog log = LogManager.GetLogger(typeof(PrinterEngine));

        private CancellationTokenSource _finishedToken;
        private CancellationTokenSource _searchDocToken;
        private TaskScheduler _context;

        private List<string> _messages = new List<string>();
        
        public PrinterEngine(PrintWindow window, Stop stop, ManualResetEvent wait, CancellationToken tokenSource, TaskScheduler context) : base(window, stop, wait, tokenSource)
        {
            _context = context;
        }

        public override void Start()
        {
            log.Info("SESSION START");
            log.Info("Selected Printer : " + _window.PrintInformation.PrinterSettings.PrinterName);

            int j = 0;

            _finishedToken = new CancellationTokenSource();
            CancellationToken token = _finishedToken.Token;
            SearchFinishedEngine finishedEngine = new SearchFinishedEngine(_window, _stopWindow, _wait, token);
            Task.Factory.StartNew(() => { finishedEngine.Start(); }, token).ContinueWith((task) => { finishedEngine.Finished(); }, _context);
            _stopWindow.SearchDocToken = _finishedToken;

            foreach (Document document in _window.Documents)
            {
                _stopWindow.Skip = false;
                _stopWindow.Pause.WaitOne();
                _token.ThrowIfCancellationRequested();

                DoWorkDocument(ref j, document);

                _window.Dispatcher.Invoke(new Action(() => { _stopWindow.ActualizationActualNumber(j); }));
            }

            if (j.ToString() == _window.Documents.Count.ToString())
            {
                _messages.Add("All the documents (i.e. " + j + ") have been queued and will be printed.");
            }
            else
            {
                if (_stopWindow.Stopped)
                {
                    _messages.Add("Printing stopped.\n" + (j) + " documents may have been queued.\n(Depends if the last document has been queued.)");
                }
                else
                {
                    _messages.Add("Printing error.\n" + (j) + " documents have been queued.");
                }
            }
        }

        public void DoWorkDocument(ref int j ,Document document)
        {
            bool retry = false;
            do
            {
                _stopWindow.Pause.WaitOne();
                if (_token.IsCancellationRequested)
                {
                    SetStatus(document, State.Stopped);
                    _token.ThrowIfCancellationRequested();
                }
                RefreshList(document);

                if (retry)
                    SetStatus(document, State.ReSending);
                else
                    SetStatus(document, State.Sending);

                retry = false;

                _searchDocToken = new CancellationTokenSource();
                CancellationToken token = _searchDocToken.Token;

                SearchEngine engine = new SearchEngine(document, _window, _stopWindow, _wait, token);
                Task.Factory.StartNew(() => { engine.Start(); }, token).ContinueWith((task) => { engine.Finished(); }, _context);

                LaunchPrint(document);

                if (engine.InQueue)
                {
                    j++;
                    _window.DocumentsInQueue.Add(document);
                }
                else if (!_stopWindow.Skip)
                {
                    retry = true;
                }

            } while (retry);
        }

        private void LaunchPrint(Document document)
        {
            if (Configuration.DocumentExtension.Contains(document.Type))
            {
                PrintDocument(document);
            }else if (Configuration.ImageExtension.Contains(document.Type))
            {
                PrintImage(document);
            }
        }

        private void PrintDocument(Document document)
        {
            Process proc = new Process();
            ProcessStartInfo info = new ProcessStartInfo(@document.Path);
            info.Verb = "PrintTo";
            info.Arguments = "\"" + _window.PrintInformation.PrinterSettings.PrinterName + "\"";
            info.WindowStyle = ProcessWindowStyle.Minimized;
            proc.StartInfo = info;
            try
            {
                proc.Start();
            }
            catch (Win32Exception)
            {
                log.Error(document.Name + " has made an error code 10 :  No process associated.");
                SetStatus(document, State.AssocError10);
                _searchDocToken.Cancel();
                _wait.WaitOne();
            }

            _wait.WaitOne();

            if (document.Type.ToLower().Equals("pdf"))
            {
                try
                {
                    proc.WaitForInputIdle();
                    Thread.Sleep(3000);
                    if (false == proc.CloseMainWindow())
                        proc.Kill();
                }
                catch (InvalidOperationException)
                {

                }
            }
            else
            {
                try
                {
                    proc.WaitForExit();
                }
                catch (InvalidOperationException)
                {

                }
            }
            proc.Dispose();
        }

        private void PrintImage(Document document)
        {
            PrintDocument printDocument = new PrintDocument();
            ManualResetEvent waitPrint = new ManualResetEvent(false);
            try
            {
                PrintController printController = new StandardPrintController();
                printDocument.PrintController = printController;
                printDocument.DefaultPageSettings.PrinterSettings.PrinterName = _window.PrintInformation.PrinterSettings.PrinterName;
                printDocument.DocumentName = document.Name;
                printDocument.DefaultPageSettings.Landscape = true;
                Image image = Image.FromFile(@document.Path);
                if (((double)image.Width / (double)image.Height) < 1)
                {
                    printDocument.DefaultPageSettings.Landscape = false;
                }
                image.Dispose();
                printDocument.PrintPage += (sender, args) =>
                {
                    Rectangle m;
                    if (_window.Configuration.ImageMarginBool)
                        m = args.MarginBounds;
                    else
                        m = args.PageBounds;

                    Image i = Image.FromFile(@document.Path);
                    if ((double)i.Width / (double)i.Height > (double)m.Width / (double)m.Height) // image is wider
                    {
                        m.Height = (int)((double)i.Height / (double)i.Width * (double)m.Width);
                    }
                    else
                    {
                        m.Width = (int)((double)i.Width / (double)i.Height * (double)m.Height);
                    }
                    args.Graphics.DrawImage(i, m);
                    i.Dispose();
                };
                printDocument.EndPrint += (sen, arg) =>
                {
                    waitPrint.Set();
                };
                waitPrint.Reset();
                printDocument.Print();
            }
            catch (OutOfMemoryException)
            {
                log.Error(document.Name + " has made an error code 11 : Image corrupted.");
                SetStatus(document, State.AssocError11);
                _searchDocToken.Cancel();
            }
            _wait.WaitOne();
            waitPrint.WaitOne();
            printDocument.Dispose();
        }

        public override void Finished()
        {
            if (_window.Documents.Count != 0)
            {
                _finishedToken.Cancel();
                log.Info("SESSION ENDING");
                _stopWindow.Close();
            }
            foreach (string message in _messages)
            {
                MessageBox.Show(message);
            }
        }
    }
}
