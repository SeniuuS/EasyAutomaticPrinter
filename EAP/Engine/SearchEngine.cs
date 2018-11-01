using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace EAP.Engine
{
    public class SearchEngine : AbstractEngine
    {
        private static ILog log = LogManager.GetLogger(typeof(SearchEngine));

        private bool _inQueue = false;
        public bool InQueue {
            get
            {
                return _inQueue;
            }
        }

        private Document _document;

        public SearchEngine(Document document, PrintWindow window, Stop stop, ManualResetEvent reset, CancellationToken token) : base(window, stop, reset, token)
        {
            _document = document;
        }

        public override void Start()
        {
            _wait.Reset();
            _inQueue = false;
            PrintServer PrintS = new PrintServer();
            PrintQueue queue = new PrintQueue(PrintS, _window.PrintInformation.PrinterSettings.PrinterName);
            int trial = 0;
            do
            {
                trial++;
                queue.Refresh();
                if (trial >= 3000 && trial < 6000)
                {
                    RefreshList(_document);
                    _window.Dispatcher.Invoke(new Action(() => { _stopWindow.skipVisibility(System.Windows.Visibility.Visible); }));
                    SetStatus(_document, State.Searching);
                }
                else if (trial >= 6000)
                    SetStatus(_document, State.StillSearching);
                try
                {
                    if (queue.NumberOfJobs > 0)
                    {
                        using (PrintSystemJobInfo job = queue.GetPrintJobInfoCollection().Last())
                        {
                            if (job.Name.Contains(_document.Name))
                            {
                                _inQueue = true;
                                SetStatus(_document, State.InQueue);
                                log.Info(_document.Name + " has been queued");
                            }
                        }
                    }
                }
                catch (NullReferenceException)
                {

                }
                catch (RuntimeWrappedException)
                {

                }
                catch (InvalidOperationException)
                {

                }
            } while (!_inQueue && trial < 10000 && !_token.IsCancellationRequested && !_stopWindow.Skip);
            PrintS.Dispose();
            queue.Dispose();

            if (trial >= 10000)
            {
                SetStatus(_document, State.Error);
                log.Error(_document.Name + " has made an error");
                if (!_window.Configuration.AutoRetry)
                {
                    System.Media.SystemSounds.Beep.Play();
                    _window.Dispatcher.Invoke(new Action(() => {
                        _stopWindow.retryVisibility(System.Windows.Visibility.Visible);
                        _window.Activate();
                        _stopWindow.Activate();
                    }));
                    _stopWindow.Retry.Reset();
                    _stopWindow.Retry.WaitOne();
                }
            }
            if (_stopWindow.Skip)
            {
                SetStatus(_document, State.Skipped);
                log.Info(_document.Name + " has been skipped");
            }
        }

        public override void Finished()
        {
            _stopWindow.retryVisibility(System.Windows.Visibility.Collapsed);
            _stopWindow.skipVisibility(System.Windows.Visibility.Collapsed);
            _wait.Set();
        }
    }
}
