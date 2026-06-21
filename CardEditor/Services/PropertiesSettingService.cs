using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.Localization;

namespace CardEditor.Services
{
    public static class PropertiesSettingService
    {
        public static bool GetBoolSetting(string keyName, bool defaultValue = false)
        {
            try
            {
                var settingValue = Properties.Settings.Default[keyName];
                if (settingValue != null && settingValue is bool boolValue)
                {
                    return boolValue;
                }
            }
            catch
            {
                ///
            }
            return defaultValue;
        }
        public static string GetStringSetting(string keyName, string defaultValue = "")
        {
            try
            {
                var settingValue = Properties.Settings.Default[keyName];
                if (settingValue != null && settingValue is string stringValue)
                {
                    return stringValue;
                }
            }
            catch
            {
                ///
            }
            return defaultValue;
        }
        public static int GetIntSetting(string keyName, int defaultValue = 14, bool limit = false, int maxValue = 0)
        {
            try
            {
                var settingValue = Properties.Settings.Default[keyName];
                if (settingValue != null && settingValue is int intValue)
                {
                    if (intValue <= 0) return defaultValue;
                    if (limit && maxValue > 0)
                    {
                        return intValue <= maxValue ? intValue : defaultValue;
                    }
                    return intValue;
                }
            }
            catch
            {
                ///
            }
            return defaultValue;
        }
    }
}
