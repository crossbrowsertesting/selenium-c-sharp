using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;

namespace SeleniumTest
{
    class LoginFormExampleTest
    {
        public static string username = "you@yourcompany.com";
        public static string authkey = "yourauthkey";

        static void Main(string[] args)
        {

            CBTAPI cbt = new CBTAPI(username, authkey);

            Dictionary<string, object> myCaps = new Dictionary<string, object>();
            myCaps.Add("name", "Login Form Example Test");
            myCaps.Add("recordVideo", "false");
            ICapabilities caps = cbt.GetCapabilitiesFromBrowserName("internet explorer", "11", "Windows 10", myCaps);
            RemoteWebDriver driver = new RemoteWebDriver(new Uri("https://hub.crossbrowsertesting.com:443/wd/hub"), caps, TimeSpan.FromSeconds(180));

            try
            {
                driver.Navigate().GoToUrl("http://crossbrowsertesting.github.io/login-form.html");
                // Check the title
                Console.WriteLine("Entering username");
                driver.FindElementByName("username").SendKeys("tester@crossbrowsertesting.com");

                // then by entering the password
                Console.WriteLine("Entering password");
                driver.FindElementByName("password").SendKeys("test123");

                // then by clicking the login button
                Console.WriteLine("Logging in");
                driver.FindElementByCssSelector("div.form-actions > button").Click();

                // let's wait here to ensure that the page has loaded completely
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(drv => driver.FindElement(By.XPath("//*[@id=\"logged-in-message\"]/h2")));

                // Let's assert that the welcome message is present on the page. 
                // if not, an exception will be raised and we'll set the score to fail in the catch block.
                string welcomeMessage = driver.FindElementByXPath("//*[@id=\"logged-in-message\"]/h2").Text;
                Assert.AreEqual("Welcome tester@crossbrowsertesting.com", welcomeMessage);
                cbt.TakeSnapshot(driver.SessionId.ToString());
                cbt.SetScore(driver.SessionId.ToString(), "pass");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                cbt.SetScore(driver.SessionId.ToString(), "fail");
            }
            finally
            {
                driver.Quit();

            }
        }
    }
}