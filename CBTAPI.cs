using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using OpenQA.Selenium;
using RestSharp;
using RestSharp.Authenticators;

namespace SeleniumTest
{
    public class CBTAPI
    {
        public string apiBaseUrl = "https://crossbrowsertesting.com/api/v3";
        private RestClient client;
        private string username;
        private string authkey;

        public CBTAPI(string username, string authkey)
        {
            this.client = new RestClient(apiBaseUrl);
            this.username = username;
            this.authkey = authkey;
            this.client.Authenticator = new HttpBasicAuthenticator(this.username, this.authkey);
        }

        public ICapabilities GetCapabilitiesFromBrowserName(string browserName, string browserVersion, string platformName, Dictionary<string, object> optionalCapabilities = null)
        {
            optionalCapabilities = optionalCapabilities ?? new Dictionary<string, object>();
            DriverOptions browserOptions;
            switch (browserName)
            {
                case "firefox":
                    browserOptions = new OpenQA.Selenium.Firefox.FirefoxOptions();
                    break;
                case "safari":
                    browserOptions = new OpenQA.Selenium.Safari.SafariOptions();
                    break;
                case "internet explorer":
                    browserOptions = new OpenQA.Selenium.IE.InternetExplorerOptions();
                    break;
                case "microsoft edge":
                    browserOptions = new OpenQA.Selenium.Edge.EdgeOptions();
                    break;
                default:
                    browserOptions = new OpenQA.Selenium.Chrome.ChromeOptions();
                    break;
            }

            browserOptions.BrowserVersion = browserVersion;
            browserOptions.PlatformName = platformName;
            var cloudOptions = new Dictionary<string, object>();
            foreach (var optionalCapability in optionalCapabilities)
            {
                cloudOptions.Add(optionalCapability.Key, optionalCapability.Value );
            }

            cloudOptions.Add("username", this.username);
            cloudOptions.Add("password", this.authkey);
            browserOptions.AddAdditionalOption("cbt:options", cloudOptions);
            return browserOptions.ToCapabilities();
        }

        private void makeRequest(string urlPath, RestSharp.Method requestMethod, string optionalJsonData = null)
        {

            var request = new RestRequest(urlPath, requestMethod, DataFormat.Json);
            if (optionalJsonData != null)
            {
                request.AddJsonBody(optionalJsonData);
            }
            var res = client.Execute(request);
            Console.WriteLine(res.Content.ToString());
        }

        public void SetScore(string sessionId, string score)
        {
            Dictionary<string, string> putDataDict = new Dictionary<string, string>();
            putDataDict.Add("action", "set_score");
            putDataDict.Add("score", score);
            string putData = JsonConvert.SerializeObject(putDataDict);
            makeRequest("/selenium/" + sessionId, RestSharp.Method.PUT, putData);
        }

        public void TakeSnapshot(string sessionId)
        {
            makeRequest("/selenium/" + sessionId + "/snapshots", RestSharp.Method.POST);
        }
    }
}
