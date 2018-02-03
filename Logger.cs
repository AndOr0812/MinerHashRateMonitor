using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MinerHashRateMonitor
{
    public class Logger
    {
        private static StringBuilder _messageBuffer = new StringBuilder();
        public static void Log(string message, bool sessionEnd = false, bool IsError = false)
        {
            if (IsError)
            {
                WriteError(message);
            }
            else
            {
                _messageBuffer.Append(message + Environment.NewLine);
                if (sessionEnd)
                {
                    WriteLog(_messageBuffer);
                    _messageBuffer.Clear();
                }
            }
        }

        private static void WriteError(string message)
        {
            File.AppendAllText("error.log", Environment.NewLine + DateTime.Now.ToString()+ Environment.NewLine + message + Environment.NewLine);
        }
        private static void WriteLog(StringBuilder message)
        {
            string _logfilename = string.Empty;
            bool _isNew = GetFileName(out _logfilename);

            if (!_isNew)
            {
                message.Append(File.ReadAllText(_logfilename));
            }

            File.WriteAllText(_logfilename, message.ToString());
            File.Copy(_logfilename, "status.log", true);
        }

        private static bool GetFileName(out string _logfilename)
        {
            _logfilename = $"log\\status_{DateTime.Today.ToShortDateString().Replace(@"/", string.Empty)}.log";
            if (!File.Exists(_logfilename))
            {
                File.Create(_logfilename).Close();
                return true;
            }

            return false;
        }

        public static void CleanupWebDriver()
        {
            try
            {
                Process[] webdrivers = Process.GetProcessesByName("chromedriver");                

                foreach (var webdriver in webdrivers)
                {
                    try
                    {
                        webdriver.Kill();
                    }
                    catch { }
                }

                if(webdrivers!=null &&  webdrivers.Length > 0)
                {
                    WriteError("CleanupWebDriver method invoked: webdrivers-" + webdrivers.Length);
                }

                Process[] chromeBrowsers = Process.GetProcessesByName("chrome");

                foreach (var chrome in chromeBrowsers)
                {
                    try
                    {                       
                        chrome.Kill();
                    }
                    catch { }
                }

                if (chromeBrowsers != null && chromeBrowsers.Length > 0)
                {
                    WriteError("CleanupWebDriver method invoked: chromeBrowsers-" + chromeBrowsers.Length);
                }

            }
            catch (Exception ex)
            {
                WriteError(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
