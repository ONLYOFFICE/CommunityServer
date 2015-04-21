/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
