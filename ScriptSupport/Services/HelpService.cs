using ScriptSupport.Stores;
using ScriptSupport.Interfaces;
using ScriptSupport.Environment;

namespace ScriptSupport.Services
{
    public class HelpService : IHelpInterface
    {
        private readonly AppEnvironment _appEnvironment;
        private readonly ConfigStore _configStore;

        public HelpService(AppEnvironment appEnvironment, ConfigStore configStore)
        {
            _appEnvironment = appEnvironment;
            _configStore = configStore;
        }

        #region Build Url
        public string BuildHelpUrl()
        {
            string lang = _configStore.UserSetting.Language;
            string url = string.Empty;

            if (string.IsNullOrEmpty(lang))
            {
                url = $@"https://github.com/TriDungSongToan/CardEditorX/tree/Legacy/ScriptSupport/Docs/UserGuide";
            }
            else
            {
                string repoUrl = $@"https://github.com/TriDungSongToan/CardEditorX/blob/Legacy/ScriptSupport";
                string startedUrl = $@"Docs/UserGuide/{lang}/GettingStarted.md";

                url = $@"{repoUrl}/{startedUrl}";
            }
            return url;
        }
        public string BuildConfigHelpUrl()
        {
            string lang = _configStore.UserSetting.Language;
            if (string.IsNullOrEmpty(lang)) lang = "English";

            string repoUrl = $@"https://github.com/TriDungSongToan/CardEditorX/blob/Legacy/ScriptSupport";
            string confUrl = $@"Docs/UserGuide/{lang}/Settings/Configuration.md";

            return $@"{repoUrl}/{confUrl}";
        }
        #endregion
    }
}
