// Getting started: http://docs.seleniumhq.org/docs/03_webdriver.jsp
// API details: https://github.com/SeleniumHQ/selenium#selenium

// Quick start video tutorial using Visual Studio: https://www.youtube.com/watch?v=CxDkRJ1iHwE
// Step-by-step video turoial using Visual Studio: https://www.youtube.com/watch?v=uRJL0zu7U6k

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

namespace LoginFormTest
{
    class LoginFormTest
    {
        // put your username and authkey here:
        public static string username = "user@email.com";
        public static string authkey = "12345";

        static void Main(string[] args)
        {
            var cbtapi = new CBTApi();

            // Start by setting the capabilities
            var caps = new DesiredCapabilities();
            caps.SetCapability("name", "Login Form Example");
            caps.SetCapability("build", "1.0");
            // request latest chrome, try ff-latest for firefox
            caps.SetCapability("browser_api_name", "chrome-latest");
            caps.SetCapability("os_api_name", "Win10");
            caps.SetCapability("screen_resolution", "1366x768");
            caps.SetCapability("record_video", "true");
            caps.SetCapability("record_network", "true");

            caps.SetCapability("username", username);
            caps.SetCapability("password", authkey);

            // Start the remote webdriver
            RemoteWebDriver driver = new RemoteWebDriver(new Uri("http://hub.crossbrowsertesting.com:80/wd/hub"), caps, TimeSpan.FromSeconds(180));

            // wrap the rest of the test in a try-catch for error logging via the API
            try
            {
                // Maximize the window - DESKTOPS ONLY
                // driver.Manage().Window.Maximize();
                // Navigate to the URL
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
                cbtapi.setScore(driver.SessionId.ToString(), "pass");
                driver.Quit();
            }
            catch (AssertionException ex)
            {

                var snapshotHash = cbtapi.takeSnapshot(driver.SessionId.ToString());
                cbtapi.setDescription(driver.SessionId.ToString(), snapshotHash, ex.ToString());
                cbtapi.setScore(driver.SessionId.ToString(), "fail");
                Console.WriteLine("caught the exception : " + ex);
                driver.Quit();
                throw new AssertionException(ex.ToString());
            }
        }
    }

    public class CBTApi
    {

        public string BaseURL = "https://crossbrowsertesting.com/api/v3/selenium";

        public string username = LoginFormTest.username;
        public string authkey = LoginFormTest.authkey;

        public string takeSnapshot(string sessionId)
        {
            // returns the screenshot hash to be used in the setDescription method.
            // create the POST request object pointed at the snapshot endpoint
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BaseURL + "/" + sessionId + "/snapshots");
            Console.WriteLine(BaseURL + "/" + sessionId);
            request.Method = "POST";
            request.Credentials = new NetworkCredential(username, authkey);
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "HttpWebRequest";
            // Execute the request
            var response = (HttpWebResponse)request.GetResponse();
            // store the response
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            // close our request stream
            response.close();
            // parse out the snapshot Hash value 
            var myregex = new Regex("(?<=\"hash\": \")((\\w|\\d)*)");
            var snapshotHash = myregex.Match(responseString).Value;
            Console.WriteLine(snapshotHash);
            return snapshotHash;
        }

        public void setDescription(string sessionId, string snapshotHash, string description)
        {
            // encode the data to be written
            ASCIIEncoding encoding = new ASCIIEncoding();
            var putData = encoding.GetBytes("description=" + description);
            // create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BaseURL + "/" + sessionId + "/snapshots/" + snapshotHash);
            request.Method = "PUT";
            request.Credentials = new NetworkCredential(username, authkey);
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "HttpWebRequest";
            // write data to stream
            Stream newStream = request.GetRequestStream();
            newStream.Write(putData, 0, putData.Length);
            newStream.Close();
            WebResponse response = request.GetResponse();
        }

        public void setScore(string sessionId, string score)
        {
            string url = BaseURL + "/" + sessionId;
            // encode the data to be written
            ASCIIEncoding encoding = new ASCIIEncoding();
            string data = "action=set_score&score=" + score;
            byte[] putdata = encoding.GetBytes(data);
            // Create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.Credentials = new NetworkCredential(username, authkey);
            request.ContentLength = putdata.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "HttpWebRequest";
            // Write data to stream
            Stream newStream = request.GetRequestStream();
            newStream.Write(putdata, 0, putdata.Length);
        }
    }
}
