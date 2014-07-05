/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
                    if (cfg.FromTeamlabToOnlyOffice == "true" && from != null && from.EndsWith(cfg.ToServerInJid))
                    {
                        int place = from.LastIndexOf(cfg.ToServerInJid);
                        if (place >= 0)
                        {
                            from = from.Remove(place, cfg.ToServerInJid.Length).Insert(place, cfg.FromServerInJid);
                        }
                    }
                    if (cfg.FromTeamlabToOnlyOffice == "true" && to != null && to.EndsWith(cfg.ToServerInJid))
                    {
                        int place = to.LastIndexOf(cfg.ToServerInJid);
                        if (place >= 0)
                        {
                            to = to.Remove(place, cfg.ToServerInJid.Length).Insert(place, cfg.FromServerInJid);
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
