using System.Threading.Tasks;

namespace CardEditor.Models
{
    public interface ISaveable
    {
        bool IsSaved { get; set; }
        Task<bool> Save();
    }
}
