using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSupport.Legacy.Services
{
    public class GetInfoService
    {
        public static long GetLinkArrowValue(bool topleft, bool top, bool topright, bool left, bool right, bool botleft, bool bot, bool botright)
        {
            long value = 0;

            if (topleft) value |= 0x40;    // ↖
            if (top) value |= 0x80;        // ↑
            if (topright) value |= 0x100;  // ↗
            if (left) value |= 0x8;        // ←
            if (right) value |= 0x20;      // →
            if (botleft) value |= 0x1;     // ↙
            if (bot) value |= 0x2;         // ↓
            if (botright) value |= 0x4;    // ↘

            return value;
        }

        public static long GetDefValue(long linkarrow, long? deffromtext, bool notHasATK)
        {
            long linkarrowBits = linkarrow & 0x1FFL;
            long hasDefFlag = deffromtext.HasValue ? (1L << 31) : 0L;
            long notHasATKBit = notHasATK ? (1L << 4) : 0L;
            long defBits = 0;

            if (deffromtext.HasValue)
            {
                long def = deffromtext.Value;

                const long minDef = -(1L << 21);
                const long maxDef = (1L << 21) - 1;

                if (def < minDef) def = minDef;
                else if (def > maxDef) def = maxDef;

                defBits = (def & 0x3FFFFFL) << 9;
            }
            return linkarrowBits | defBits | hasDefFlag | notHasATKBit;
        }

        public static (long linkarrow, long? deffromtext, bool notHasATK) DecodeDef(long encodedDEF)
        {
            // Bit 0~3: Link Arrows
            // Bit 4: Not Has ATK (0: Có ATK, 1: Không có ATK)
            // Bit 5~8: Link Arrows
            // Bit 9~30: DEF Value
            // Bit 30: Dấu hiệu DEF (0: Dương, 1: Âm)
            // Bit 31: Has DEF (0: Không có DEF, 1: Có DEF)

            long linkarrow = encodedDEF & 0x1FFL;

            bool hasDef = (encodedDEF & (1L << 31)) != 0;
            long? deffromtext = null;

            if (hasDef)
            {
                long defBits = (encodedDEF >> 9) & 0x3FFFFFL;
                if ((defBits & (1L << 21)) != 0)
                {
                    defBits |= ~0x3FFFFFL;
                }
                deffromtext = defBits;
            }
            bool notHasATK = (encodedDEF & (1L << 4)) != 0;
            return (linkarrow, deffromtext, notHasATK);
        }
    }
}
