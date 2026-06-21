using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardEditor.Events
{
    public class ScrollEventArgs : EventArgs
    {
        public double VerticalOffset { get; set; }
        public double VerticalScrollPercentage { get; set; }
        public int? FirstVisibleLine { get; set; }

        public ScrollEventArgs(double verticalOffset, double percentage, int? firstVisibleLine = null)
        {
            VerticalOffset = verticalOffset;
            VerticalScrollPercentage = percentage;
            FirstVisibleLine = firstVisibleLine;
        }
    }
}
