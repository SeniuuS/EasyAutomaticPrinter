using System.Collections.Generic;
using System.ComponentModel;

namespace EAP
{
    public enum State
    {
        Ready,
        Sending,
        InQueue,
        Error,
        ReSending,
        Searching,
        StillSearching,
        Skipped,
        Stopped,
        AssocError10,
        AssocError11,
        Printed
    }

    public class Document : INotifyPropertyChanged
    {
        public static Dictionary<State, string> StateDescription = new Dictionary<State, string>()
        {
            { State.Ready, "Ready" },
            { State.Sending, "Sending to queue" },
            { State.InQueue, "In queue" },
            { State.Error, "Error" },
            { State.ReSending, "Error detected, resending to queue" },
            { State.Searching, "Searching the document in the print queue" },
            { State.StillSearching, "The document has still not been found, still searching" },
            { State.Skipped, "The document has been skipped" },
            { State.Stopped, "Printing stopped" },
            { State.AssocError10, "Error Code : 10. See Log File if activated." },
            { State.AssocError11, "Error Code : 11. See Log File if activated." },
            { State.Printed, "Printed" },
        };

        public string Name { get; }
        public string Path { get; }
        public string Type { get; }
        
        private string _status;
        public string Status {
            get { return _status; }
            set
            {
                if(_status != value)
                {
                    _status = value;
                    NotifyPropertyChanged("Status");
                }
            }
        }

        public int Number { get; set; }

        public Document(string name, string path, string type)
        {
            Name = name;
            Path = path;
            Type = type;
            _status = StateDescription[State.Ready];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propStat)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propStat));
            }
        }
    }
}
