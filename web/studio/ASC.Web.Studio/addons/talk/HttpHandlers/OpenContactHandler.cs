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
using System.Text;
using System.Web;
using System.Xml.Linq;
using ASC.Web.Core.Jabber;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Talk.HttpHandlers
{
    public class OpenContactHandler : IHttpHandler
    {
        private const string FROM = "from";
        private const string TO = "to";
        private const string FROM_TENANT = "fromtenant";
        private static TalkConfiguration cfg = new TalkConfiguration();

        public bool IsReusable
        {
            // To enable pooling, return true here.
            // This keeps the handler in memory.
            get { return false; }
        }

        private readonly static JabberServiceClient _jabberServiceClient = new JabberServiceClient();

        public void ProcessRequest(HttpContext context)
        {
            var response = String.Empty;
            if (context.Request.Url != null && !string.IsNullOrEmpty(context.Request.Url.Query))
            {
                bool fromTenant = false;
                var to = string.Empty;
                var from = string.Empty;
                foreach (var query in context.Request.Url.Query.Trim().Trim('?').Split('&'))
                {
                    var el = query.Split('=');
                    switch (el[0].ToLower())
                    {
                        case FROM:
                            from = el[1];
                            break;
                        case TO:
                            to = el[1];
                            break;
                        case FROM_TENANT:
                            fromTenant = Convert.ToBoolean(el[1]);
                            break;
                    }
                }

                if (!String.IsNullOrEmpty(to) && !String.IsNullOrEmpty(from))
                {
                    if (cfg.ReplaceDomain && from != null && from.EndsWith(cfg.ReplaceToDomain))
                    {
                        int place = from.LastIndexOf(cfg.ReplaceToDomain);
                        if (place >= 0)
                        {
                            from = from.Remove(place, cfg.ReplaceToDomain.Length).Insert(place, cfg.ReplaceFromDomain);
                        }
                    }
                    if (cfg.ReplaceDomain && to != null && to.EndsWith(cfg.ReplaceToDomain))
                    {
                        int place = to.LastIndexOf(cfg.ReplaceToDomain);
                        if (place >= 0)
                        {
                            to = to.Remove(place, cfg.ReplaceToDomain.Length).Insert(place, cfg.ReplaceFromDomain);
                        }
                    }
                    try
                    {
                        _jabberServiceClient.SendCommand(TenantProvider.CurrentTenantID, from, to, "open", fromTenant);
                    }
                    catch (Exception ex)
                    {
                        response = ex.Message;
                    }
                }
                else
                {
                    response = "Invalid param";
                }
            }

            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.ContentType = "application/xml";
            context.Response.Charset = Encoding.UTF8.WebName;
            var xml = new XDocument(new XElement("response", response)).ToString();
            context.Response.Write(Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(xml))));
        }
    }
}
