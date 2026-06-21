using System.Collections.Generic;

namespace CardEditor.Helpers
{
    public static class MatchesSetcode
    {
        public static bool MatchesSetCode_OR(ulong cardSetCode, ulong fillerSetCode)
        {
            if (fillerSetCode == 0) return true;

            for (int i = 0; i < 4; i++)
            {
                ushort fPart = (ushort)((fillerSetCode >> (i * 16)) & 0xFFFF);
                if (fPart == 0) continue;

                for (int j = 0; j < 4; j++)
                {
                    ushort cPart = (ushort)((cardSetCode >> (j * 16)) & 0xFFFF);
                    if (cPart == 0) continue;

                    if (fPart == cPart)
                        return true;
                }
            }
            return false;
        }

        public static bool MatchesSetCode_AND(ulong cardSetCode, ulong fillerSetCode)
        {
            if (fillerSetCode == 0) return true;

            var cardParts = new HashSet<ushort>();
            for (int i = 0; i < 4; i++)
            {
                ushort part = (ushort)((cardSetCode >> (i * 16)) & 0xFFFF);
                if (part != 0) cardParts.Add(part);
            }

            for (int i = 0; i < 4; i++)
            {
                ushort fPart = (ushort)((fillerSetCode >> (i * 16)) & 0xFFFF);
                if (fPart == 0) continue;

                if (!cardParts.Contains(fPart)) return false;
            }

            return true;

            //var cardParts = new List<ushort>();
            //for (int i = 0; i < 4; i++)
            //{
            //    ushort part = (ushort)((cardSetCode >> (i * 16)) & 0xFFFF);
            //    if (part != 0)
            //        cardParts.Add(part);
            //}

            //var fillerParts = new List<ushort>();
            //for (int i = 0; i < 4; i++)
            //{
            //    ushort part = (ushort)((fillerSetCode >> (i * 16)) & 0xFFFF);
            //    if (part != 0)
            //        fillerParts.Add(part);
            //}

            //foreach (ushort fPart in fillerParts)
            //{
            //    if (!cardParts.Contains(fPart))
            //        return false;
            //}

            //return true;
        }
    }
}
