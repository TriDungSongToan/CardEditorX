using System;

namespace CardEditor.Behaviors
{
    public class OpenFolderDialogEventArgs : EventArgs
    {
        public Action<(bool Success, string PathOrErrorMessage)> Callback { get; }

        public string Message { get; }

        public OpenFolderDialogEventArgs(string message, Action<(bool, string)> callback)
        {
            Message = message;
            Callback = callback;
        }
    }
}
