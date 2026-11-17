using Microsoft.Data.Sqlite;
using ScriptSupport.Stores;
using ScriptSupport.Interfaces;
using ScriptSupport.Environment;
using ScriptSupport.Localization;
using CMess = ScriptSupport.Localization.Language;

namespace ScriptSupport.Services
{
    public class GenesysService : IGenesysInterface
    {
        private readonly AppEnvironment _env;
        private readonly CardStore _cardStore;

        public GenesysService(AppEnvironment env, CardStore cardStore)
        {
            _env = env;
            _cardStore = cardStore;
        }

        public async Task<(bool Success, string Message)> LoadGenesysDB()
        {
            var (data, message) = await LoadGenesysCardData();
            if (data == null) return (false, message);

            if (data.Count > 0) _cardStore.SetGenesysData(data);
            return (true, string.Empty);
        }
        public async Task<(Dictionary<ulong, int>?, string)> LoadGenesysCardData()
        {
            string genesysDBPath = _env.GenesysDBPath;
            if (string.IsNullOrEmpty(genesysDBPath) || !System.IO.File.Exists(genesysDBPath))
                return (null, CMess.fileNotExit.ToText());

            try
            {
                var result = new Dictionary<ulong, int>();
                var connectionString = $"Data Source={_env.GenesysDBPath};";

                await using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT id, point FROM GenesysCard";

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ulong id = reader.IsDBNull(0) ? 0UL : Convert.ToUInt64(reader.GetValue(0));
                    int point = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
                    result[id] = point;
                }

                return (result, string.Empty);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public int? GetGPoint(ulong id)
        {
            return _cardStore.GenesysData.TryGetValue(id, out int value)
                ? value : null;
        }
    }
}
