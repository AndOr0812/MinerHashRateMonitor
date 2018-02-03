using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinerHashRateMonitor
{
    class InternetManager
    {
        public static void CheckInternetSpeed()
        {
            using (IWebDriver chrome = new ChromeDriver())
            {
                if (!CheckInternetConnection())
                {
                    Logger.Log("NO INTERNET: Restarting router...");
                    RestartRouter(chrome);
                }
                else
                {
                    try
                    {

                        string speedTestUrl = "http://speedtest.net/";
                        INavigation navigator = chrome.Navigate();
                        navigator.GoToUrl(speedTestUrl);
                        if (chrome.Url != speedTestUrl)
                        {
                            navigator.GoToUrl(speedTestUrl);
                        }
                        Thread.Sleep(5000);
                        IWebElement broadbandOperator = chrome.FindElements(By.ClassName("result-label"))[5];

                        if (broadbandOperator != null && broadbandOperator.Text == "BSNL")
                        {
                            ChangeWiFiSetting(true);
                            ulong speedCheckCounter = Convert.ToUInt64(Environment.GetEnvironmentVariable("SpeedCheckCounter", EnvironmentVariableTarget.Machine) ?? "0");
                            speedCheckCounter = speedCheckCounter >= 1000 ? 0 : speedCheckCounter;
                            // Check internet speed every alternate round to save some data
                            if (speedCheckCounter % 2 == 0)
                            {
                                // Check internet speed
                                IWebElement goButton = chrome.FindElement(By.ClassName("start-text"));
                                goButton.Click();
                                // Lets wait for 60 sec before fetching speed test results
                                Thread.Sleep(45000);
                                IWebElement pingTime = chrome.FindElement(By.ClassName("ping-speed"));
                                IWebElement download = chrome.FindElement(By.ClassName("download-speed"));
                                IWebElement upload = chrome.FindElement(By.ClassName("upload-speed"));

                                // Raw values
                                // Logger.Log(string.Format("Speed Check results: Ping-{0} Download-{1} Upload-{2}", pingTime.Text, download.Text, upload.Text));

                                if (pingTime == null || string.IsNullOrEmpty(pingTime.Text))
                                {
                                    // Restart router
                                    Logger.Log("PING ERROR: Restarting router...");
                                    RestartRouter(chrome);
                                }
                                else
                                {
                                    // record ping time , Download /Upload speed                           
                                    int pingMS = string.IsNullOrWhiteSpace(pingTime.Text) ? 0 : Convert.ToInt32(pingTime.Text);
                                    double downloadMBPS = string.IsNullOrWhiteSpace(download?.Text) ? 0 : Convert.ToDouble(download.Text);
                                    double uploadMBPS = string.IsNullOrWhiteSpace(upload?.Text) ? 0 : Convert.ToDouble(upload.Text);

                                    if (pingMS > 250 || downloadMBPS < 0.5 || uploadMBPS < 0.3)
                                    {
                                        Logger.Log(string.Format("BSNL SLOW: Ping-{0} Download-{1} Upload-{2}", pingTime.Text, download.Text, upload.Text));
                                        // Restart router 
                                        Logger.Log("BSNL SLOW: Restarting router...");
                                        RestartRouter(chrome);
                                    }
                                    else
                                    {
                                        Logger.Log(string.Format("BSNL OK: Ping-{0} Download-{1} Upload-{2}", pingTime.Text, download.Text, upload.Text));
                                    }
                                }
                            }
                            speedCheckCounter++;
                            Environment.SetEnvironmentVariable("SpeedCheckCounter", speedCheckCounter.ToString(), EnvironmentVariableTarget.Machine);


                        }
                        else
                        {
                            if (broadbandOperator.Text == "Vodafone")
                            {
                                ChangeWiFiSetting(false);
                            }
                            else
                            {
                                ChangeWiFiSetting(true);
                            }

                            Logger.Log("OPERATOR CHANGED: " + broadbandOperator.Text);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message);
                        Logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, false, true);
                    }
                }
            }
        }

        private static bool CheckInternetConnection()
        {
            try
            {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send("www.google.com", 5000);
                if (reply != null && reply.Status == IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, false, true);
                return false;
            }

            return false;
        }

        private static void RestartRouter(IWebDriver chrome)
        {
            try
            {
                string routerLoginUrl = "http://192.168.1.1/login.htm";
                string routerRebootUrl = "http://192.168.1.1/reboot.htm";
                INavigation navigator = chrome.Navigate();
                navigator.GoToUrl(routerLoginUrl);
                IWebElement username = chrome.FindElement(By.Id("username"));
                IWebElement password = chrome.FindElement(By.Id("password"));
                IWebElement loginBtn = chrome.FindElement(By.Id("loginBtn"));
                username.Clear();
                password.Clear();
                username.SendKeys("admin");
                password.SendKeys("11@aDMINoNE");

                loginBtn.Click();
                navigator.GoToUrl(routerRebootUrl);

                IWebElement rebootButtom = chrome.FindElement(By.Name("save"));
                rebootButtom.Click();
                Logger.Log("Router restarted");

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                Logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, false, true);
            }
        }

        private static void ChangeWiFiSetting(bool enable)
        {
            try
            {
                using (IWebDriver chrome = new ChromeDriver())
                {
                    string routerLoginUrl = "http://192.168.1.1/login.htm";
                    string wifiUrl = "http://192.168.1.1/wlbasic.htm?v=1511371785000";
                    INavigation navigator = chrome.Navigate();
                    navigator.GoToUrl(routerLoginUrl);
                    IWebElement username = chrome.FindElement(By.Id("username"));
                    IWebElement password = chrome.FindElement(By.Id("password"));
                    IWebElement loginBtn = chrome.FindElement(By.Id("loginBtn"));
                    username.Clear();
                    password.Clear();
                    username.SendKeys("admin");
                    password.SendKeys("11@aDMINoNE");

                    loginBtn.Click();
                    navigator.GoToUrl(wifiUrl);

                    IWebElement disableWiFiCheckbox = chrome.FindElement(By.Name("wlanDisabled"));
                    IWebElement saveButton = chrome.FindElement(By.Name("save"));

                    if (!enable)
                    {
                        if (!disableWiFiCheckbox.Selected)
                        {
                            disableWiFiCheckbox.Click();
                            saveButton.Click();
                            Logger.Log("WiFi " + "Disabled");
                        }
                    }
                    else
                    {
                        if (disableWiFiCheckbox.Selected)
                        {
                            disableWiFiCheckbox.Click();
                            saveButton.Click();
                            Logger.Log("WiFi " + "Enabled");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                Logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, false, true);
            }
        }
    }
}
