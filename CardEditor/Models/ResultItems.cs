using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardEditor.Models
{
    public class ResultItem
    {
        public bool Succeeded { get; set; }
        public int FilteredCount { get; set; }
        public int TotalCount { get; set; }
        public string Message { get; set; }

    }
}
