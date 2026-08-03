using YamlDotNet.Serialization;

namespace CardEditor.Models
{
    public class OfficialData
    {
        [YamlMember(Alias = "password")]
        public ulong Password { get; set; } = 0;

        [YamlMember(Alias = "konami_id")]
        public ulong KonamiID { get; set; } = 0;
    }
    public class RushData
    {
        [YamlMember(Alias = "konami_id")]
        public ulong KonamiID { get; set; } = 0;

        [YamlMember(Alias = "name")]
        public NameData Name { get; set; } = new();
    }
    public class NameData
    {
        [YamlMember(Alias = "en")]
        public string En { get; set; } = string.Empty;
    }
}
