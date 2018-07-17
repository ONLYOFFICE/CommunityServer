using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using ASC.FederatedLogin.Helpers;
using ASC.Thrdparty.Configuration;
using ASC.Web.Files.Classes;
using Newtonsoft.Json.Linq;
using log4net;

namespace ASC.Web.Files.Helpers
{
    public class EasyBibHelper
    {
        public static ILog Log = Global.Logger;

        static string   searchBookUrl = "https://worldcat.citation-api.com/query?search=",
                        searchJournalUrl = "https://crossref.citation-api.com/query?search=",
                        searchWebSiteUrl = "https://web.citation-api.com/query?search=",
                        easyBibStyles = "http://easybib-csl.herokuapp.com/1.0/styles";
        
        public enum EasyBibSource
        {
            book = 0,
            journal = 1,
            website = 2
        }

        public static string GetEasyBibCitationsList(int source,  string data)
        {
            var uri = "";
            switch (source)
            {
                case 0:
                    uri = searchBookUrl;
                    break;
                case 1:
                    uri = searchJournalUrl;
                    break;
                case 2:
                    uri = searchWebSiteUrl;
                    break;
                default:
                    break;
            }
            uri += data;

            const string method = "GET";
            var headers = new Dictionary<string, string>(){};
            try
            {
                return RequestHelper.PerformRequest(uri, "", method, "", headers);
            }
            catch (Exception)
            {
                return "error";
            }

        }

        public static string GetEasyBibStyles()
        {
             
            const string method = "GET";
            var headers = new Dictionary<string, string>(){};
            try
            {
                return RequestHelper.PerformRequest(easyBibStyles, "", method, "", headers);
            }
            catch (Exception)
            {
                return "error";
            }
        }
        public static object GetEasyBibCitation(string data)
        {
            try
            {
                var easyBibappkey = KeyStorage.Get("easyBibappkey");

                var jsonBlogInfo = JObject.Parse(data);
                jsonBlogInfo.Add("key", easyBibappkey);
                var citationData = jsonBlogInfo.ToString();

                var uri = "https://api.citation-api.com/2.0/rest/cite";
                const string contentType = "application/json";
                const string method = "POST";
                var body = citationData;
                var headers = new Dictionary<string, string>() { };

                return RequestHelper.PerformRequest(uri, contentType, method, body, headers);
                
            }
            catch (Exception)
            {
                return null;
                throw;
            }
            
        }
    }
}