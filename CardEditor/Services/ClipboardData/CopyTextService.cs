using System;
using System.Windows;
using CardEditor.Models;

namespace CardEditor.Services.ClipboardData
{
    public class CopyTextService
    {
        public static SetClipboardResult CopyTextToClipboard(string text)
        {
            SetClipboardResult result = new();
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    result.Result = false;
                    result.Messenger = "Text is empty.";
                }
                Clipboard.SetText(text);
                result.Result = true;
                result.Messenger = string.Empty;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
    }
}
