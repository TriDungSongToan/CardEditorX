using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

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
