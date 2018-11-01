using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EAP.Engine
{
    public abstract class AbstractEngine : IEngine
    {
        protected PrintWindow _window;
        protected Stop _stopWindow;
        protected ManualResetEvent _wait;
        protected CancellationToken _token;

        protected AbstractEngine(PrintWindow window, Stop stop, ManualResetEvent wait, CancellationToken token)
        {
            _stopWindow = stop;
            _window = window;
            _wait = wait;
            _token = token;
        }
        
        protected void SetStatus(Document document, State state)
        {
            _window.Dispatcher.Invoke(new Action(() => { document.Status = Document.StateDescription[state]; }));
        }

        protected void RefreshList(Document document)
        {
            _window.Dispatcher.Invoke(new Action(() => { _window.RefreshList(document); }));
        }

        public abstract void Start();

        public abstract void Finished();
    }
}
