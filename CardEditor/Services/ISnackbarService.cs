using System;
using MaterialDesignThemes.Wpf;

namespace CardEditor.Services
{
    public interface ISnackbarService
    {
        void ShowMessage(string message);
        void ShowMessage(string message, string actionContent, Action actionCallback);
    }

    public class SnackbarService : ISnackbarService
    {
        private readonly SnackbarMessageQueue _messageQueue;
        public SnackbarService(SnackbarMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public void ShowMessage(string message)
        {
            _messageQueue.Enqueue(message);
        }

        public void ShowMessage(string message, string actionContent, Action actionCallback)
        {
            _messageQueue.Enqueue(message, actionContent, actionCallback);
        }
    }
}
