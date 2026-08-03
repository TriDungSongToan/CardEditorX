using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CardEditor.Models
{
    public class CharacterItem
    {
        public string Character { get; set; } = string.Empty;
        public CharacterMetadata Metadata { get; set; } = new CharacterMetadata();
        public string Description { get; set; } = string.Empty;
    }
    public class CharacterDataFile
    {
        public int Version { get; set; } = 1;
        public List<CharacterItem> Items { get; set; } = new();
    }
    public class CharacterFilter
    {
        public string SearchText { get; set; } // match Description
        public CharacterGroup? Group { get; set; }
        public string SubCategory { get; set; }
        public List<string> Tags { get; set; } = new(); // filter tag name
    }
    public enum CharacterGroup
    {
        All,
        Punctuation,
        Math,
        Technical,
        Currency,
        Arrow,
        Number,
        Symbol,
        Geometry,
        Icon,
        UI,
        BoxDrawing,
        Language,
        Japanese,
        Music,
        Measurement,
        Emoji
    }
    public class CharacterMetadata
    {
        public CharacterGroup Group { get; set; }
        public string SubCategory { get; set; } = string.Empty;
        public List<TagItem> Tags { get; set; } = new List<TagItem>();
    }
    public class TagItem
    {
        public string Name { get; set; } = string.Empty;
        [JsonIgnore]
        public bool IsEnabled { get; set; } = true;
        public override string ToString()
        {
            return Name;
        }
    }
}
