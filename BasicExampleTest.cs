using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace SeleniumTest
{
    class BasicExampleTest
    {
        public static string username = "you@yourcompany.com";
        public static string authkey = "yourauthkey";

        static void Main(string[] args)
        {

            CBTAPI cbt = new CBTAPI(username, authkey);

            Dictionary<string, object> myCaps = new Dictionary<string, object>();
            myCaps.Add("name", "Basic Example test");
            myCaps.Add("recordVideo", "false");
            ICapabilities caps = cbt.GetCapabilitiesFromBrowserName("firefox", "latest", "Windows 10", myCaps);
            RemoteWebDriver driver = new RemoteWebDriver(new Uri("https://hub.crossbrowsertesting.com:443/wd/hub"), caps, TimeSpan.FromSeconds(180));

            try
            {
                driver.Navigate().GoToUrl("http://crossbrowsertesting.github.io/selenium_example_page.html");
                cbt.TakeSnapshot(driver.SessionId.ToString());
                cbt.SetScore(driver.SessionId.ToString(), "pass");
            } catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                cbt.SetScore(driver.SessionId.ToString(), "fail");
            } finally
            {
                driver.Quit();
                
            }   
        }
    }
}
