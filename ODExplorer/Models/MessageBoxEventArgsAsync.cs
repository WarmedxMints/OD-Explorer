using System;
using System.Threading.Tasks;
using System.Windows;

namespace ODExplorer.Models
{
    public sealed class MessageBoxEventArgsAsync(string title, string message, MessageBoxButton buttons, Func<Task>? callbackYes = null, Func<Task>? callbackNo = null)
    {
        public string Title { get; } = title;
        public string Message { get; } = message;
        public MessageBoxButton Buttons { get; } = buttons;
        public Func<Task>? CallbackYes { get; } = callbackYes;
        public Func<Task>? CallbackNo { get; } = callbackNo;
    }
}
