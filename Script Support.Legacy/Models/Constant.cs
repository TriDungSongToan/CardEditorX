using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSupport.Legacy.Models
{
    [Flags]
    public enum CardType : ulong
    {
        None = 0x0,
        Monster = 0x1,
        Spell = 0x2,
        Trap = 0x4,
        NotUsed1 = 0x8,
        Normal = 0x10,
        Effect = 0x20,
        Fusion = 0x40,
        Ritual = 0x80,
        NotUsed2 = 0x100,
        Spirit = 0x200,
        Union = 0x400,
        Gemini = 0x800,
        Tuner = 0x1000,
        Synchro = 0x2000,
        Token = 0x4000,
        Maximum = 0x8000,
        QuickPlay = 0x10000,
        Continuous = 0x20000,
        Equip = 0x40000,
        Field = 0x80000,
        Counter = 0x100000,
        Flip = 0x200000,
        Toon = 0x400000,
        eXceed = 0x800000,
        Pendulum = 0x1000000,
        SPSummon = 0x2000000,
        Link = 0x4000000,
        Skill = 0x8000000,
        Action = 0x10000000,
        Plus = 0x20000000,
        Minor = 0x40000000,
        Armor = 0x80000000
    }
    [Flags]
    public enum CardRule : ulong
    {
        None = 0x0,
        OCG = 0x1,
        TCG = 0x2,
        Anime = 0x4,
        Illegal = 0x8,
        VideoGame = 0x10,
        Custom = 0x20,
        SpeedDuel = 0x40,
        NA1 = 0x80,
        PreRelease = 0x100,
        Rush = 0x200,
        Legend = 0x400,
        NA2 = 0x800,
        Hidden = 0x1000
    }
}