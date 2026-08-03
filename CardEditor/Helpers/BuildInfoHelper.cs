using System;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace CardEditor.Helpers
{
    public static class BuildInfoHelper
    {
        public static DateTime ReleaseDateUtc
        {
            get
            {
                var raw = Assembly.GetEntryAssembly()
                    ?.GetCustomAttributes<AssemblyMetadataAttribute>()
                    .FirstOrDefault(x => x.Key == "BuildTime")
                    ?.Value;

                if (!DateTime.TryParse(
                        raw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                        out var dt))
                {
                    dt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                }

                return dt; // đã chắc chắn là UTC, không cần ToUniversalTime() nữa
            }
        }
        public static DateTime ReleaseDateLocal => ReleaseDateUtc.ToLocalTime();
    }
}
