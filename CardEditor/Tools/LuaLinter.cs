using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CardEditor.Tools
{
    public class LuaLinter
    {
        private string toolsPath = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tools");
        public string RunLinter(string luaFilePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = System.IO.Path.Combine(toolsPath, "luacheck.exe"),
                Arguments = $"\"{luaFilePath}\" --no-color --formatter plain",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return string.IsNullOrEmpty(output) ? error : output;
            }
        }

        public string FormatLuaCode(string luaCode)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Path.Combine(toolsPath, "lua-format.exe"),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true, // Bổ sung để đọc lỗi
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8, // Đọc kết quả UTF-8
                    StandardErrorEncoding = Encoding.UTF8  // Đọc lỗi UTF-8
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();

                    using (StreamWriter sw = new StreamWriter(process.StandardInput.BaseStream, Encoding.UTF8)) // Ghi UTF-8
                    {
                        sw.Write(luaCode);
                        sw.Close(); // Đóng input để lua-format biết đã nhận xong dữ liệu
                    }

                    string formattedCode = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(errors))
                        return $"Lỗi định dạng: {errors.Trim()}";

                    return formattedCode;
                }
            }
            catch (Exception ex)
            {
                return $"Lỗi định dạng: {ex.Message}";
            }
        }

        public string FormatLuaWithStylua(string luaCode)
        {
            try
            {
                string exePath = System.IO.Path.Combine(toolsPath, "stylua.exe");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "--stdin",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    using (StreamWriter sw = new StreamWriter(process.StandardInput.BaseStream, Encoding.UTF8))
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.Write(luaCode);
                        }
                    }

                    string formattedCode = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(errors))
                        return $"Lỗi định dạng: {errors}";

                    return formattedCode;
                }
            }
            catch (Exception ex)
            {
                return $"Lỗi định dạng: {ex.Message}";
            }
        }

    }
}
