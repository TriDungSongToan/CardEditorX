using System;

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
