using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    public class CustomNotification : NotificationBase, INotifyPropertyChanged
    {
        private CustomDisplayPart _displayPart;

        public override NotificationDisplayPart DisplayPart => _displayPart ??= new CustomDisplayPart(this);

        public new MessageOptions Options { get; }

        private string _header;
        private string _message;

        public CustomNotification(string header, string message, MessageOptions options) : base(message, options)
        {
            Header = header;
            Message = message;
            Options = options;
        }

        public new string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public string Header { get => _header; set { _header = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
