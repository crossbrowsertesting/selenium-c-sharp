using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using NUnit.Framework;

namespace SeleniumTest
{
    class ToDoExampleTest
    {
        public static string username = "you@yourcompany.com";
        public static string authkey = "yourauthkey";

        static void Main(string[] args)
        {

            CBTAPI cbt = new CBTAPI(username, authkey);

            Dictionary<string, object> myCaps = new Dictionary<string, object>();
            myCaps.Add("name", "My test");
            myCaps.Add("recordVideo", "false");
            ICapabilities caps = cbt.GetCapabilitiesFromBrowserName("chrome", "latest", "Windows 10", myCaps);
            RemoteWebDriver driver = new RemoteWebDriver(new Uri("https://hub.crossbrowsertesting.com:443/wd/hub"), caps, TimeSpan.FromSeconds(180));

            try
            { 
                driver.Navigate().GoToUrl("http://crossbrowsertesting.github.io/todo-app.html");
                // Check the title
                Console.WriteLine("Clicking Checkbox");
                driver.FindElementByName("todo-4").Click();
                Console.WriteLine("Clicking Checkbox");
                driver.FindElementByName("todo-5").Click();

                // If both clicks worked, then te following List should have length 2
                IList<IWebElement> elems = driver.FindElementsByClassName("done-true");

                Console.WriteLine("Entering Text");
                driver.FindElementById("todotext").SendKeys("Run your first Selenium Test");
                driver.FindElementById("addbutton").Click();


                // lets also assert that the new todo we added is in the list
                string spanText = driver.FindElementByXPath("/html/body/div/div/div/ul/li[6]/span").Text;
                Console.WriteLine("Archiving old todos");
                driver.FindElementByLinkText("archive").Click();

                elems = driver.FindElementsByClassName("done-false");
                Assert.AreEqual(4, elems.Count);
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
