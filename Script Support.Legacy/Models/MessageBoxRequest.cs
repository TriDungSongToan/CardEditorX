using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSupport.Legacy.Models
{
    public class MessageBoxRequest
    {
        public string Title { get; set; }
        public CMSG.MessageBoxIconType IconType { get; set; }
        public string Message { get; set; }
        public string[] Buttons { get; set; }
#pragma warning disable CS8632
        public TaskCompletionSource<int>? ResponseSource { get; set; }
#pragma warning restore CS8632
        public Task<int> ResponseTask => ResponseSource?.Task ?? Task.FromResult(-1);
    }
}