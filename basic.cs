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
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace BasicTest
{
    class BasicTest
    {
        // put your username and authkey here:
        public static string username = "user@email.com";
        public static string authkey = "12345";

        static void Main(string[] args)
        {
            var cbtapi = new CBTApi();

            // Start by setting the capabilities
            var browserOptions = new OpenQA.Selenium.Chrome.ChromeOptions
            {
                PlatformName = "Windows 10",
                BrowserVersion = "latest"
            };

            var cloudOptions = new Dictionary<string, object>();
            cloudOptions.Add("username", username);
            cloudOptions.Add("password", authkey);
            browserOptions.AddAdditionalOption("cbt:options", cloudOptions);


            // Start the remote webdriver
            RemoteWebDriver driver = new RemoteWebDriver (new Uri ("http://hub.crossbrowsertesting.com:80/wd/hub"), browserOptions.ToCapabilities(), TimeSpan.FromSeconds(180));

            // wrap the rest of the test in a try-catch for error logging via the API
            try
            {
                // Maximize the window - DESKTOPS ONLY
                // driver.Manage().Window.Maximize();
                // Navigate to the URL
                driver.Navigate ().GoToUrl("http://crossbrowsertesting.github.io/selenium_example_page.html");
                // Check the title
                Console.WriteLine(driver.Title);
                Assert.AreEqual(driver.Title, "Selenium Test Example Page");
                // Uncomment this line to see what happens when an assertion fails!
                // Assert.AreEqual(driver.Title, "Selenium Test Example Cage");
                cbtapi.setScore (driver.SessionId.ToString(),"pass");
                driver.Quit();
            } catch (AssertionException ex) {

                var snapshotHash = cbtapi.takeSnapshot (driver.SessionId.ToString());
                cbtapi.setDescription (driver.SessionId.ToString(), snapshotHash, ex.ToString ());
                cbtapi.setScore (driver.SessionId.ToString(), "fail");
                Console.WriteLine ("caught the exception : " + ex);
                driver.Quit ();
                throw new AssertionException (ex.ToString ());
            }
        }
    }

    public class CBTApi {

        public string BaseURL = "https://crossbrowsertesting.com/api/v3/selenium";

        public string username = BasicTest.username;
        public string authkey = BasicTest.authkey;

        public string takeSnapshot(string sessionId){
            // returns the screenshot hash to be used in the setDescription method.
            // create the POST request object pointed at the snapshot endpoint
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BaseURL + "/" + sessionId + "/snapshots");
            Console.WriteLine(BaseURL+"/"+sessionId);
            request.Method = "POST";
            request.Credentials = new NetworkCredential(username, authkey);
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "HttpWebRequest";
            // Execute the request
            WebResponse response = request.GetResponse();
            // store the response
            String responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // parse out the snapshot Hash value
            Regex myregex = new Regex("(?<=\"hash\": \")((\\w|\\d)*)");
            String snapshotHash = myregex.Match(responseString).Value;
            Console.WriteLine (snapshotHash);
            //close our request stream
            response.Close();
            return snapshotHash;
        }

        public void setDescription(string sessionId, string snapshotHash, string description ){
            // encode the data to be written
            ASCIIEncoding encoding = new ASCIIEncoding ();
            var putData = encoding.GetBytes ("description=" + description);
            // create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create (BaseURL + "/" + sessionId + "/snapshots/" + snapshotHash);
            request.Method = "PUT";
            request.Credentials = new NetworkCredential (username, authkey);
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "HttpWebRequest";
            // write data to stream
            Stream newStream = request.GetRequestStream ();
            newStream.Write (putData, 0, putData.Length);
            newStream.Close ();
            WebResponse response = request.GetResponse ();
            response.Close();
        }

        public void setScore(string sessionId, string score ) {
            string url = BaseURL + "/" + sessionId;
            // encode the data to be written
            ASCIIEncoding encoding = new ASCIIEncoding();
            string data = "action=set_score&score="+ score;
            byte[] putdata = encoding.GetBytes (data);
            // Create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.Credentials = new NetworkCredential(username, authkey);
            request.ContentLength = putdata.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "HttpWebRequest";
            // Write data to stream
            Stream newStream = request.GetRequestStream ();
            newStream.Write (putdata, 0, putdata.Length);
            WebResponse response = request.GetResponse();
            response.Close();
        }
    }
}
