using CardEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardEditor.Helpers
{
    public static class GetPenScaleHelp
    {
        public static PenScale GetPenScale(ulong level)
        {
            int right = (int)((level >> 16) & 0xFF);
            int left = (int)((level >> 24) & 0xFF);

            return new PenScale
            {
                LeftScale = left,
                RightScale = right,
            };
        }
    }
}
