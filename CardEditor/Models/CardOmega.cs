using System;
using System.Collections.Generic;

namespace CardEditor.Models
{
    [Serializable]
    public class CardOmega
    {
        public ulong id { get; set; }

        // Bảng texts
        public string name { get; set; } = string.Empty;
        public string desc { get; set; } = string.Empty;
        public string str1 { get; set; } = string.Empty;
        public string str2 { get; set; } = string.Empty;
        public string str3 { get; set; } = string.Empty;
        public string str4 { get; set; } = string.Empty;
        public string str5 { get; set; } = string.Empty;
        public string str6 { get; set; } = string.Empty;
        public string str7 { get; set; } = string.Empty;
        public string str8 { get; set; } = string.Empty;
        public string str9 { get; set; } = string.Empty;
        public string str10 { get; set; } = string.Empty;
        public string str11 { get; set; } = string.Empty;
        public string str12 { get; set; } = string.Empty;
        public string str13 { get; set; } = string.Empty;
        public string str14 { get; set; } = string.Empty;
        public string str15 { get; set; } = string.Empty;
        public string str16 { get; set; } = string.Empty;

        // Bảng datas
        public ulong ot { get; set; } = 0;
        public ulong alias { get; set; } = 0;
        public byte[]? setcode { get; set; }
        public ulong type { get; set; } = 0;
        public long atk { get; set; } = 0;
        public long def { get; set; } = 0;
        public ulong level { get; set; } = 0;
        public ulong race { get; set; } = 0;
        public ulong attribute { get; set; } = 0;
        public ulong category { get; set; } = 0;    // Omega Flag
        public ulong genre { get; set; } = 0;       // Omega Category
        public byte[]? script { get; set; }
        public byte[]? support { get; set; }

        public void UpdateFrom(CardOmega src, params Action<CardOmega, CardOmega>[] updaters)
        {
            if (src == null || updaters == null) return;

            foreach (var u in updaters)
                u(this, src);
        }
        public void UpdateFrom(CardOmega src, IEnumerable<Action<CardOmega, CardOmega>> updaters)
        {
            if (src == null || updaters == null) return;
            foreach (var u in updaters)
                u(this, src);
        }
    }
}
