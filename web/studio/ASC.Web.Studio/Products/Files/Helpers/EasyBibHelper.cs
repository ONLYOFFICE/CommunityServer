/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using ASC.Common.Logging;
using ASC.Core.Common.Configuration;
using ASC.FederatedLogin.Helpers;
using ASC.Web.Files.Classes;
using Newtonsoft.Json.Linq;

namespace ASC.Web.Files.Helpers
{
    public class EasyBibHelper : Consumer
    {
        public static ILog Log = Global.Logger;

        static string   searchBookUrl = "https://worldcat.citation-api.com/query?search=",
                        searchJournalUrl = "https://crossref.citation-api.com/query?search=",
                        searchWebSiteUrl = "https://web.citation-api.com/query?search=",
                        easyBibStyles = "https://api.citation-api.com/2.1/rest/styles";
        
        public enum EasyBibSource
        {
            book = 0,
            journal = 1,
            website = 2
        }

        public string AppKey
        {
            get { return this["easyBibappkey"]; }
        }

        public EasyBibHelper()
        {
            
        }

        public EasyBibHelper(string name, int order,  Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
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
                var easyBibappkey = ConsumerFactory.Get<EasyBibHelper>().AppKey;

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