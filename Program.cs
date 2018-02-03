using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;

namespace MinerHashRateMonitor
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IList<Miner> miners = GetMinersList();

                using (IWebDriver chrome = new ChromeDriver())
                {
                    //chrome.Manage().Timeouts().ImplicitWait = new TimeSpan(0,0,5);
                    //Logger.Log(string.Empty);
                    Logger.Log("Session Started - " + DateTime.Now.ToString());
                    foreach (var miner in miners)
                    {
                        try
                        {
                            if (miner.IsOnline)
                            {
                                string rootUrl = string.Format("http://root:root1@{0}", miner.Ip);
                                string statusUrl = string.Format("http://{0}/cgi-bin/minerStatus.cgi", miner.Ip);
                                string rebootUrl = string.Format("http://{0}/reboot.html", miner.Ip);
                                INavigation navigator = chrome.Navigate();
                                navigator.GoToUrl(rootUrl);
                                navigator.GoToUrl(statusUrl);

                                //Logger.Log(string.Format("[{0}]",miner.Name));
                                IWebElement time_elapsed = chrome.FindElement(By.Id("ant_elapsed"));

                                if (!string.IsNullOrEmpty(time_elapsed.Text))
                                {
                                    int totalMinutes = ConvertToMinutes(time_elapsed.Text);
                                    //Logger.Log(string.Format("Total minutes elapsed - {0}", totalMinutes));
                                    if (totalMinutes >= 13)
                                    {
                                        IWebElement ant_ghs5s = chrome.FindElement(By.Id("ant_ghs5s"));
                                        IWebElement avg_hash = chrome.FindElement(By.Id("ant_ghsav"));

                                        if (!string.IsNullOrEmpty(avg_hash.Text))
                                        {
                                            var ant_ghs5s_value = (int)Convert.ToDecimal(ant_ghs5s.Text.Replace(",", string.Empty));
                                            var avg_hash_value = (int)Convert.ToDecimal(avg_hash.Text.Replace(",", string.Empty));
                                            //Logger.Log(string.Format("Average hashrate - {0}/ {1}", avg_hash_value, miner.HashRate));
                                            if (ant_ghs5s_value < miner.HashRate || avg_hash_value < miner.HashRate)
                                            {
                                                Logger.Log(string.Format("[{0}] - Below threashold {1}-{2}/{3}. Restarting...", miner.Name, ant_ghs5s_value, avg_hash_value, miner.HashRate));
                                                navigator.GoToUrl(rebootUrl);
                                                IWebElement reboot_button = chrome.FindElement(By.ClassName("cbi-button-save"));
                                                reboot_button.Click();
                                            }
                                            else
                                            {
                                                Logger.Log(string.Format("[{0}] - {1} @{2}h - OK", miner.Name, avg_hash_value, ConvertToHrs(totalMinutes)));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Logger.Log(string.Format("[{0}] - Warming Up @{1}m", miner.Name, totalMinutes));
                                    }
                                }
                                else
                                {
                                    Logger.Log(string.Format("[{0}] - Starting up.", miner.Name));
                                }
                            }
                            else
                            {
                                Logger.Log(string.Format("[{0}] - OFFLINE", miner.Name));
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Error:" + ex.Message);
                            Logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, false, true);
                        }
                    }
                }
                // Check internet connection statitics
                InternetManager.CheckInternetSpeed();
                // Session end
                Logger.Log(string.Empty, true);
            }
            catch (Exception ex)
            {
                Logger.Log("Error:" + ex.Message);
                Logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, false, true);
                Logger.Log(string.Empty, true);
            }
            finally
            {
                Logger.CleanupWebDriver();
            }

        }

        private static decimal ConvertToHrs(decimal totalMinutes)
        {
            decimal hrs = Math.Round(totalMinutes / 60, 1);
            return hrs;
        }

        private static IList<Miner> GetMinersList()
        {
            return new List<Miner>()
            {
                //new Miner() { Name = "S9-1", Type = MinerType.Bitcoin, Ip = "192.168.1.11", HashRate = 4500},
                new Miner() { Name = "S9-2", Type = MinerType.Bitcoin, Ip = "192.168.1.12", HashRate = 13000},
                new Miner() { Name = "S9-3", Type = MinerType.Bitcoin, Ip = "192.168.1.13", HashRate = 12000},
                new Miner() { Name = "T9-1", Type = MinerType.Bitcoin, Ip = "192.168.1.14", HashRate = 7000},
                new Miner() { Name = "T9-2", Type = MinerType.Bitcoin, Ip = "192.168.1.15", HashRate = 11000},
                new Miner() { Name = "L3-1", Type = MinerType.Litecoin, Ip = "192.168.1.16", HashRate = 500},
                new Miner() { Name = "L3-2", Type = MinerType.Litecoin, Ip = "192.168.1.17", HashRate = 450},
                new Miner() { Name = "L3-3", Type = MinerType.Litecoin, Ip = "192.168.1.18", HashRate = 450}
            };
        }
        public static int ConvertToMinutes(string time_elapsed)
        {
            int days = 0, hours = 0, minutes = 0;
            int length = time_elapsed.Length;
            int d_index = time_elapsed.IndexOf("d");
            int h_index = time_elapsed.IndexOf("h");
            int m_index = time_elapsed.IndexOf("m");

            if (d_index > 0)
            {
                days = Convert.ToInt32(time_elapsed.Substring(0, d_index));
            }

            if (h_index > 0)
            {
                if (d_index > 0)
                {
                    hours = Convert.ToInt32(time_elapsed.Substring(d_index + 1, h_index - (d_index + 1)));
                }
                else
                {
                    hours = Convert.ToInt32(time_elapsed.Substring(0, h_index));
                }
            }

            if (m_index > 0)
            {
                if (h_index > 0)
                {
                    minutes = Convert.ToInt32(time_elapsed.Substring(h_index + 1, m_index - (h_index + 1)));
                }
                else
                {
                    if (d_index > 0)
                    {
                        minutes = Convert.ToInt32(time_elapsed.Substring(d_index + 1, m_index - 2));
                    }
                    else
                    {
                        minutes = Convert.ToInt32(time_elapsed.Substring(0, m_index));
                    }
                }
            }

            return (days*24*60 + hours * 60 + minutes);

        }
    }
}
