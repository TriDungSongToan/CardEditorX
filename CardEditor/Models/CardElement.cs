namespace CardEditor.Models
{
    public class RuleItem
    {
        public ulong RuleCode { get; set; }
        public string RuleName { get; set; }
        public bool IsEnabled { get; set; } = true;

        public override string ToString()
        {
            return RuleName;
        }
    }

    public class TypeItem
    {
        public ulong TypeCode { get; set; }
        public string TypeName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public override string ToString()
        {
            return TypeName;
        }
    }

    public class RaceItem
    {
        public ulong RaceCode { get; set; }
        public string RaceName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public override string ToString()
        {
            return RaceName;
        }
    }

    public class CharItem
    {
        public ulong CharCode { get; set; }
        public string CharName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public override string ToString()
        {
            return CharName;
        }
    }

    public class AttributeItem
    {
        public ulong AttributeCode { get; set; }
        public string AttributeName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public override string ToString()
        {
            return AttributeName;
        }
    }

    public class SetCodeItem
    {
        public ulong SetCode { get; set; }
        public string SetName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public override string ToString()
        {
            return SetName;
        }
    }

    public class CategoryItem
    {
        public ulong CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public override string ToString()
        {
            return CategoryName;
        }
    }

    public class FlagItem
    {
        public ulong FlagCode { get; set; }
        public string FlagName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public override string ToString()
        {
            return FlagName;
        }
    }

    public class LevelItem
    {
        public ulong LevelCode { get; set; }
        public ulong LevelNum { get; set; }
        public bool IsEnabled { get; set; }
        public override string ToString()
        {
            return LevelNum.ToString();
        }
    }

    public class LinkArrowItem
    {
        public ulong LinkArrowCode { get; set; }
        public bool LinkArrowValue { get; set; }
    }

}
