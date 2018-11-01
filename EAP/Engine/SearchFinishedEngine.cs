using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace EAP.Engine
{
    public class SearchFinishedEngine : AbstractEngine
    {
        public SearchFinishedEngine(PrintWindow window, Stop stop, ManualResetEvent pause, CancellationToken tokenSource) : base(window, stop, pause, tokenSource)
        {
        }

        public override void Start()
        {
            PrintServer PrintS = new PrintServer();
            PrintQueue queue = new PrintQueue(PrintS, _window.PrintInformation.PrinterSettings.PrinterName);
            bool trouve;
            List<Document> tempDocToRemove;
            List<Document> tempDocInQueue;
            do
            {
                tempDocToRemove = new List<Document>();
                tempDocInQueue = _window.DocumentsInQueue.ToList();
                queue.Refresh();
                foreach (Document theDoc in tempDocInQueue)
                {
                    trouve = false;
                    try
                    {
                        using (PrintJobInfoCollection jobinfo = queue.GetPrintJobInfoCollection())
                        {
                            foreach (PrintSystemJobInfo job in jobinfo) using (job)
                                {
                                    if (job.Name.Contains(theDoc.Name))
                                    {
                                        trouve = true;
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
                    if (trouve == false)
                    {
                        tempDocToRemove.Add(theDoc);
                        SetStatus(theDoc, State.Printed);
                    }
                }
                foreach (Document theDoc in tempDocToRemove)
                {
                    _window.DocumentsInQueue.Remove(theDoc);
                }
            } while (!_token.IsCancellationRequested || _stopWindow.Stopped);
            PrintS.Dispose();
            queue.Dispose();
        }

        public override void Finished()
        {
            //Not used
        }
    }
}
