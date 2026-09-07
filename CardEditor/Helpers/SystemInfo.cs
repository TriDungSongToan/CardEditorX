using System;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace CardEditor.Helpers
{
    public static class SystemInfoHelper
    {
        #region Operating System
        public static string OperatingSystemA
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                    {
                        if (key == null) return "Windows";
                        string productName = key.GetValue("ProductName") as string ?? "Windows";
                        string edition = GetEdition(productName);
                        string version = GetWindowsVersion(key);
                        string build = key.GetValue("CurrentBuild") as string
                            ?? key.GetValue("CurrentBuildNumber") as string
                            ?? "Unknown";
                        string architecture = GetArchitecture();
                        return $"{version} {edition} {architecture} (Build {build})";
                    }
                }
                catch
                {
                    return "Windows";
                }
            }
        }
        public static string OperatingSystem
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                    {
                        if (key == null) return "Unknown";
                        string productName = key.GetValue("ProductName") as string ?? "Windows";
                        string build = key.GetValue("CurrentBuild") as string ?? key.GetValue("CurrentBuildNumber") as string ?? "Unknown";
                        string architecture = GetArchitecture();
                        return $"{productName} {architecture} (Build {build})";
                    }
                }
                catch
                {
                    return "Unknown";
                }
            }
        }
        private static string GetWindowsVersion(RegistryKey key)
        {
            string build = key.GetValue("CurrentBuild") as string ?? key.GetValue("CurrentBuildNumber") as string;

            if (int.TryParse(build, out int buildNumber))
            {
                if (buildNumber >= 22000) return "Windows 11";
                if (buildNumber >= 10240) return "Windows 10";
            }
            string productName = key.GetValue("ProductName") as string ?? "Windows";

            if (productName.IndexOf("Windows 11", StringComparison.OrdinalIgnoreCase) >= 0) return "Windows 11";
            if (productName.IndexOf("Windows 10", StringComparison.OrdinalIgnoreCase) >= 0) return "Windows 10";
            return "Windows";
        }
        private static string GetEdition(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName)) return string.Empty;
            string edition = productName.Trim();

            if (edition.StartsWith("Windows 11 ", StringComparison.OrdinalIgnoreCase))
            {
                edition = edition.Substring("Windows 11 ".Length);
            }
            else if (edition.StartsWith("Windows 10 ", StringComparison.OrdinalIgnoreCase))
            {
                edition = edition.Substring("Windows 10 ".Length);
            }
            return edition.Trim();
        }
        #endregion

        #region Architecture
        public static string GetArchitecture()
        {
            // When a 32-bit application runs on 64-bit Windows,
            // PROCESSOR_ARCHITEW6432 contains the real OS architecture.
            string architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
            if (!string.IsNullOrEmpty(architecture)) return ConvertArchitecture(architecture);
            architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ConvertArchitecture(architecture);
        }
        private static string ConvertArchitecture(string architecture)
        {
            if (string.IsNullOrEmpty(architecture)) return "Unknown";

            switch (architecture.ToUpperInvariant())
            {
                case "AMD64":
                case "X64":
                    return "64-bit";

                case "ARM64":
                    return "ARM64";

                case "ARM":
                    return "ARM";

                case "X86":
                    return "32-bit";

                default:
                    return architecture;
            }
        }
        #endregion

        #region Processor
        public static string Processor
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                    {
                        string processor = key?.GetValue("ProcessorNameString") as string;
                        return processor?.Trim() ?? "Unknown";
                    }
                }
                catch
                {
                    return "Unknown";
                }
            }
        }
        public static int LogicalProcessorCount
        {
            get { return Environment.ProcessorCount; }
        }
        #endregion

        #region Memory
        public static string Memory
        {
            get
            {
                ulong bytes = GetTotalPhysicalMemory();

                if (bytes == 0) return "Unknown";
                double gb = bytes / (1024.0 * 1024.0 * 1024.0);
                return $"{gb:0.##} GB RAM";
            }
        }
        private static ulong GetTotalPhysicalMemory()
        {
            try
            {
                MEMORYSTATUSEX status = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(status)) return status.ullTotalPhys;
            }
            catch { }
            return 0;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
        [StructLayout(LayoutKind.Sequential)]
        private sealed class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        #endregion
    }
}
