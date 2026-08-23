using System.Threading;
using System.Threading.Tasks;
using CardEditor.Models;

namespace CardEditor.Interfaces
{
    public interface IRegulationInterface
    {
        Task<(bool Success, string Error, YamiYugiRegulation Data)> LoadRegulation(string url,
            CancellationToken cancellationToken = default);

        Task<(bool Success, string Error, string filePath)> CreateBanListRegulation(
            YamiYugiRegulation regulation, string banlistName, bool whiteList, string filePath);
    }
}
