using System;
using System.Runtime.Serialization;

namespace CardEditor.Enums.MasterDuel
{
    public enum CardType
    {
        Monster,
        Spell,
        Trap
    }
    public enum MonsterType
    {
        Effect,
        Flip,
        Fusion,
        Gemini,
        Link,
        Normal,
        Pendulum,
        Ritual,
        Spirit,
        Synchro,
        Toon,
        Tuner,
        Union,
        Xyz
    }
    public enum Race
    {
        Aqua, Beast,
        [EnumMember(Value = "Beast-Warrior")] BeastWarrior,
        Continuous, Counter,
        [EnumMember(Value = "Creator-God")] CreatorGod,
        Cyberse, Dinosaur,
        [EnumMember(Value = "Divine-Beast")] DivineBeast,
        Dragon, Equip, Fairy, Field, Fiend, Fish, Illusion, Insect,
        Machine, Normal, Plant, Psychic, Pyro,
        [EnumMember(Value = "Quick-Play")] QuickPlay,
        Reptile, Ritual, Rock,
        [EnumMember(Value = "Sea Serpent")] SeaSerpent,
        Spellcaster, Thunder, Warrior,
        [EnumMember(Value = "Winged Beast")] WingedBeast,
        Wyrm, Zombie
    }
    public enum Attribute
    {
        DARK,
        DIVINE,
        EARTH,
        FIRE,
        LIGHT,
        WATER,
        WIND
    }
    public enum LinkArrow
    {
        Bottom,
        [EnumMember(Value = "Bottom-Left")] BottomLeft,
        [EnumMember(Value = "Bottom-Right")] BottomRight,
        Left,
        Right,
        Top,
        [EnumMember(Value = "Top-Left")] TopLeft,
        [EnumMember(Value = "Top-Right")] TopRight
    }
    [Flags]
    public enum Rarity
    {
        N = 1,
        R = 2,
        SR = 4,
        UR = 8
    }
}
