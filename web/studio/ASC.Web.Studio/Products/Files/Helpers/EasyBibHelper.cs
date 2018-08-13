/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


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