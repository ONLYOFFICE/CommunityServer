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
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Engine;
using ASC.Notify.Patterns;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;
using log4net;

namespace ASC.Web.Studio.Core.Notify
{
    public static class NotifyConfiguration
    {
        private static bool configured = false;
        private static object locker = new object();
        private static readonly Regex urlReplacer = new Regex(@"(<a [^>]*href=(('(?<url>[^>']*)')|(""(?<url>[^>""]*)""))[^>]*>)|(<img [^>]*src=(('(?<url>(?!data:)[^>']*)')|(""(?<url>(?!data:)[^>""]*)""))[^/>]*/?>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex textileLinkReplacer = new Regex(@"""(?<text>[\w\W]+?)"":""(?<link>[^""]+)""", RegexOptions.Singleline | RegexOptions.Compiled);

        public static void Configure()
        {
            lock (locker)
            {
                if (!configured)
                {
                    configured = true;

                    WorkContext.NotifyContext.NotifyClientRegistration += NotifyClientRegisterCallback;
                    WorkContext.NotifyContext.NotifyEngine.BeforeTransferRequest += BeforeTransferRequest;
                }
            }
        }

        public static void RegisterSendMethods()
        {
            StudioNotifyService.Instance.RegisterSendMethod();
        }


        private static void NotifyClientRegisterCallback(Context context, INotifyClient client)
        {
            var absoluteUrl = new SendInterceptorSkeleton(
                "Web.UrlAbsoluter",
                InterceptorPlace.MessageSend,
                InterceptorLifetime.Global,
                (r, p) =>
                {
                    if (r != null && r.CurrentMessage != null && r.CurrentMessage.ContentType == Pattern.HTMLContentType)
                    {
                        var body = r.CurrentMessage.Body;

                        body = urlReplacer.Replace(body, m =>
                        {
                            var url = m.Groups["url"].Value;
                            var ind = m.Groups["url"].Index - m.Index;
                            return string.IsNullOrEmpty(url) && ind > 0 ?
                                m.Value.Insert(ind, CommonLinkUtility.GetFullAbsolutePath(string.Empty)) :
                                m.Value.Replace(url, CommonLinkUtility.GetFullAbsolutePath(url));
                        });

                        body = textileLinkReplacer.Replace(body, m =>
                        {
                            var url = m.Groups["link"].Value;
                            var ind = m.Groups["link"].Index - m.Index;
                            return string.IsNullOrEmpty(url) && ind > 0 ?
                                m.Value.Insert(ind, CommonLinkUtility.GetFullAbsolutePath(string.Empty)) :
                                m.Value.Replace(url, CommonLinkUtility.GetFullAbsolutePath(url));
                        });

                        r.CurrentMessage.Body = body;
                    }
                    return false;
                });
            client.AddInterceptor(absoluteUrl);

            var securityAndCulture = new SendInterceptorSkeleton(
                "ProductSecurityInterceptor",
                 InterceptorPlace.DirectSend,
                 InterceptorLifetime.Global,
                 (r, p) =>
                 {
                     try
                     {
                         // culture
                         var u = ASC.Core.Users.Constants.LostUser;

                         if (32 <= r.Recipient.ID.Length)
                         {
                             var guid = default(Guid);
                             try
                             {
                                 guid = new Guid(r.Recipient.ID);
                             }
                             catch (FormatException) { }
                             catch (OverflowException) { }

                             if (guid != default(Guid))
                             {
                                 u = CoreContext.UserManager.GetUsers(guid);
                             }
                         }

                         if (ASC.Core.Users.Constants.LostUser.Equals(u))
                         {
                             u = CoreContext.UserManager.GetUserByEmail(r.Recipient.ID);
                         }

                         if (ASC.Core.Users.Constants.LostUser.Equals(u))
                         {
                             u = CoreContext.UserManager.GetUserByUserName(r.Recipient.ID);
                         }

                         if (!ASC.Core.Users.Constants.LostUser.Equals(u))
                         {
                             var culture = !string.IsNullOrEmpty(u.CultureName) ? u.GetCulture() : CoreContext.TenantManager.GetCurrentTenant().GetCulture();
                             Thread.CurrentThread.CurrentCulture = culture;
                             Thread.CurrentThread.CurrentUICulture = culture;

                             // security
                             var tag = r.Arguments.Find(a => a.Tag == CommonTags.ModuleID);
                             var productId = tag != null ? (Guid)tag.Value : Guid.Empty;
                             if (productId == Guid.Empty)
                             {
                                 tag = r.Arguments.Find(a => a.Tag == CommonTags.ProductID);
                                 productId = tag != null ? (Guid)tag.Value : Guid.Empty;
                             }
                             if (productId == Guid.Empty)
                             {
                                 productId = (Guid)(CallContext.GetData("asc.web.product_id") ?? Guid.Empty);
                             }
                             if (productId != Guid.Empty && productId != new Guid("f4d98afdd336433287783c6945c81ea0") /* ignore people product */)
                             {
                                 return !WebItemSecurity.IsAvailableForUser(productId.ToString(), u.ID);
                             }
                         }
                     }
                     catch (Exception error)
                     {
                         LogManager.GetLogger(typeof(NotifyConfiguration)).Error(error);
                     }
                     return false;
                 });
            client.AddInterceptor(securityAndCulture);
        }

        private static string GetPartnerInfo()
        {
            var partner = CoreContext.PaymentManager.GetApprovedPartner();
            if (partner == null || !partner.CustomEmailSignature) return string.Empty;

            var footerStart = "<table cellspacing=\"0\" cellpadding=\"0\" style=\"margin: 0; border-spacing: 0; empty-cells: show;\">";
            footerStart += "<tbody>";
            footerStart += "<tr style=\"height: 10px\">";
            footerStart += "<td colspan=\"2\" style=\"width: 40px; background-color: #09669C;\"><div style=\"width: 40px; height: 10px;\">&nbsp;</div></td>";
            footerStart += "<td style=\"width: 600px; background-color: #fff;\"><div style=\"width: 600px; height: 10px;\">&nbsp;</div></td>";
            footerStart += "<td colspan=\"2\" style=\"width: 40px; background-color: #09669C;\"><div style=\"width: 40px; height: 10px;\">&nbsp;</div></td>";
            footerStart += "</tr>";
            footerStart += "<tr style=\"height: 10px\">";
            footerStart += "<td colspan=\"5\" style=\"width: 680px; background-color: #09669C;\"><div style=\"width: 680px; height: 10px;\">&nbsp;</div></td>";
            footerStart += "</tr>";
            footerStart += "<tr style=\"height: 10px\">";
            footerStart += "<td style=\"width: 33px; background-color: #09669C;\"><div style=\"width: 33px; height: 10px;\">&nbsp;</div></td>";
            footerStart += "<td style=\"width: 7px; background-color: #09669C;\"><div style=\"width: 7px; height: 10px;\">&nbsp;</div></td>";
            footerStart += "<td style=\"width: 600px; background-color: #fff;\"><div style=\"width: 600px; height: 10px;\">&nbsp;</div></td>";
            footerStart += "<td style=\"width: 7px; background-color: #09669C;\"><div style=\"width: 7px; height: 10px;\">&nbsp;</div></td>";
            footerStart += "<td style=\"width: 33px; background-color: #09669C;\"><div style=\"width: 33px; height: 10px;\">&nbsp;</div></td>";
            footerStart += "</tr>";
            footerStart += "</tbody>";
            footerStart += "</table>";
            footerStart += "<table cellspacing=\"0\" cellpadding=\"0\" style=\"margin: 0; border-spacing: 0; empty-cells: show;\">";
            footerStart += "<tbody>";
            footerStart += "<tr style=\"color: #333;\">";
            footerStart += "<td style=\"width: 33px; background-color: #09669C;\"><div style=\"width: 33px;\"> </div></td>";
            footerStart += "<td style=\"width: 7px; background-color: #f5942d\"><div style=\"width: 7px;\"> </div></td>";
            footerStart += "<td style=\"width: 600px; background-color: #fff;\">";
            footerStart += "<div style=\"width: 540px; padding: 5px 30px 10px; overflow: hidden;\">";
            footerStart += "<table cellspacing=\"0\" cellpadding=\"0\">";
            footerStart += "<tbody>";
            footerStart += "<tr>";

            var footerEnd = "</tr>";
            footerEnd += "</tbody>";
            footerEnd += "</table>";
            footerEnd += "</div>";
            footerEnd += "</td>";
            footerEnd += "<td style=\"width: 7px; background-color: #f5942d\"><div style=\"width: 7px;\"> </div></td>";
            footerEnd += "<td style=\"width: 33px; background-color: #09669C;\"><div style=\"width: 33px;\"> </div></td>";
            footerEnd += "</tr>";
            footerEnd += "</tbody>";
            footerEnd += "</table>";

            var partnerInfo = string.Empty;
            if ((partner.DisplayType == PartnerDisplayType.All || partner.DisplayType == PartnerDisplayType.LogoOnly) && !string.IsNullOrEmpty(partner.LogoUrl))
            {
                partnerInfo += "<td rowspan=\"2\" align=\"center\" style=\"width:180px; max-width:180px;\"><img src=\"" + partner.LogoUrl + "\" style=\"max-width:180px;\" /></td>";
            }

            partnerInfo += "<td colspan=\"3\" style=\"padding-left: 10px;\">";

            if ((partner.DisplayType == PartnerDisplayType.All || partner.DisplayType == PartnerDisplayType.DisplayNameOnly) && !string.IsNullOrEmpty(partner.DisplayName))
            {
                partnerInfo += "<div style=\"font-size: 22px;\">" + partner.DisplayName + "</div>";
            }
            partnerInfo += "<i style=\"color: #808080; font-size: 13px;\">" + WebstudioPatternResource.TextForPartnerFooter + "</i>";
            partnerInfo += "</td></tr><tr>";

            if (!string.IsNullOrEmpty(partner.Address) || !string.IsNullOrEmpty(partner.SupportPhone) ||
                !string.IsNullOrEmpty(partner.Phone) || !string.IsNullOrEmpty(partner.Url))
            {
                partnerInfo += "<td style=\"width:180px; padding:8px 0 0 10px; vertical-align: top;\">";

                if (!string.IsNullOrEmpty(partner.Address))
                {
                    partnerInfo += "<div style=\"font-size: 12px; padding-bottom: 5px;\">" + partner.Address + "</div>";
                }
                if (!string.IsNullOrEmpty(partner.SupportPhone))
                {
                    partnerInfo += "<div style=\"font-size: 12px;\">" + partner.SupportPhone + "</div>";
                }
                else if (!string.IsNullOrEmpty(partner.Phone))
                {
                    partnerInfo += "<div style=\"font-size: 12px;\">" + partner.Phone + "</div>";
                }
                if (!string.IsNullOrEmpty(partner.Url))
                {
                    partnerInfo += "<a style=\"font-size:12px; max-width: 180px; overflow: hidden; text-overflow: ellipsis; display:inline-block;\" target=\"_blank\" href=\"" + partner.Url + "\">" + partner.Url + "</a>";
                }
                partnerInfo += "</td>";
            }
            if (!string.IsNullOrEmpty(partner.SupportEmail) || !string.IsNullOrEmpty(partner.SalesEmail) || !string.IsNullOrEmpty(partner.Email))
            {
                partnerInfo += "<td style=\"width:180px; padding:8px 0 0 10px; vertical-align: top;\">";
                if (!string.IsNullOrEmpty(partner.SupportEmail) || !string.IsNullOrEmpty(partner.SalesEmail))
                {
                    if (!string.IsNullOrEmpty(partner.SalesEmail))
                    {
                        partnerInfo += "<p style=\"font-size:12px; color:#808080; margin:0; padding:0;\">" + WebstudioPatternResource.SalesDepartment + ":</p>";
                        partnerInfo += "<a style=\"font-size:12px; max-width: 180px; overflow: hidden; text-overflow: ellipsis; display:inline-block; margin-bottom: 8px;\" href=\"mailto:" + partner.SalesEmail + "\">" + partner.SalesEmail + "</a>";
                    }
                    if (!string.IsNullOrEmpty(partner.SupportEmail))
                    {
                        partnerInfo += "<p style=\"font-size:12px; color:#808080; margin:0; padding:0;\">" + WebstudioPatternResource.TechnicalSupport + ":</p>";
                        partnerInfo += "<a style=\"font-size:12px; max-width: 180px; overflow: hidden; text-overflow: ellipsis; display:inline-block;\" href=\"mailto:" + partner.SupportEmail + "\">" + partner.SupportEmail + "</a>";
                    }
                }
                else if (!string.IsNullOrEmpty(partner.Email))
                {
                    partnerInfo += "<a style=\"font-size:12px; max-width: 180px; overflow: hidden; text-overflow: ellipsis; display:inline-block;\" href=\"mailto:" + partner.Email + "\">" + partner.Email + "</a><br />";
                }
                partnerInfo += "</td>";
            }
            partnerInfo = footerStart + partnerInfo + footerEnd;
            return partnerInfo;
        }


        private static void BeforeTransferRequest(NotifyEngine sender, NotifyRequest request)
        {
            var aid = Guid.Empty;
            var aname = string.Empty;
            if (SecurityContext.IsAuthenticated)
            {
                aid = SecurityContext.CurrentAccount.ID;
                if (CoreContext.UserManager.UserExists(aid))
                {
                    aname = CoreContext.UserManager.GetUsers(aid).DisplayUserName(false)
                        .Replace(">", "&#62")
                        .Replace("<", "&#60");
                }
            }

            IProduct product;
            IModule module;
            CommonLinkUtility.GetLocationByRequest(out product, out module);
            if (product == null && CallContext.GetData("asc.web.product_id") != null)
            {
                product = WebItemManager.Instance[(Guid)CallContext.GetData("asc.web.product_id")] as IProduct;
            }

            request.Arguments.Add(new TagValue(CommonTags.AuthorID, aid));
            request.Arguments.Add(new TagValue(CommonTags.AuthorName, aname));
            request.Arguments.Add(new TagValue(CommonTags.AuthorUrl, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(aid))));
            request.Arguments.Add(new TagValue(CommonTags.VirtualRootPath, CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/')));
            request.Arguments.Add(new TagValue(CommonTags.ProductID, product != null ? product.ID : Guid.Empty));
            request.Arguments.Add(new TagValue(CommonTags.ModuleID, module != null ? module.ID : Guid.Empty));
            request.Arguments.Add(new TagValue(CommonTags.ProductUrl, CommonLinkUtility.GetFullAbsolutePath(product != null ? product.StartURL : "~")));
            request.Arguments.Add(new TagValue(CommonTags.DateTime, TenantUtil.DateTimeNow()));
            request.Arguments.Add(new TagValue(CommonTags.Helper, new PatternHelper()));
            request.Arguments.Add(new TagValue(CommonTags.RecipientID, Context.SYS_RECIPIENT_ID));
            request.Arguments.Add(new TagValue(CommonTags.RecipientSubscriptionConfigURL, CommonLinkUtility.GetMyStaff()));
            request.Arguments.Add(new TagValue("Partner", GetPartnerInfo()));

            if (!request.Arguments.Any(x => CommonTags.SendFrom.Equals(x.Tag)))
            {
                request.Arguments.Add(new TagValue(CommonTags.SendFrom, CoreContext.TenantManager.GetCurrentTenant().Name));
            }
        }
    }
}
