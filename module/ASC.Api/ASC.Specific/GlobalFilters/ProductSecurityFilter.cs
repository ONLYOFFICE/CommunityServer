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
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Web;
using ASC.Api;
using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Web.Core;

namespace ASC.Specific.GlobalFilters
{
    public class ProductSecurityFilter : ApiCallFilter
    {
        private static readonly IDictionary<string, Guid> products;
        private readonly ILog log;


        static ProductSecurityFilter()
        {
            var blog = new Guid("6a598c74-91ae-437d-a5f4-ad339bd11bb2");
            var bookmark = new Guid("28b10049-dd20-4f54-b986-873bc14ccfc7");
            var forum = new Guid("853b6eb9-73ee-438d-9b09-8ffeedf36234");
            var news = new Guid("3cfd481b-46f2-4a4a-b55c-b8c0c9def02c");
            var wiki = new Guid("742cf945-cbbc-4a57-82d6-1600a12cf8ca");
            var photo = new Guid("9d51954f-db9b-4aed-94e3-ed70b914e101");

            products = new Dictionary<string, Guid>
                {
                    { "blog", blog },
                    { "bookmark", bookmark },
                    { "event", news },
                    { "forum", forum },
                    { "photo", photo },
                    { "wiki", wiki },
                    { "birthdays", WebItemManager.BirthdaysProductID },
                    { "community", WebItemManager.CommunityProductID },
                    { "crm", WebItemManager.CRMProductID },
                    { "files", WebItemManager.DocumentsProductID },
                    { "project", WebItemManager.ProjectsProductID },
                    { "calendar", WebItemManager.CalendarProductID },
                    { "mail", WebItemManager.MailProductID },
                };
        }


        public ProductSecurityFilter(ILog log)
        {
            this.log = log;
        }


        public override void PreMethodCall(IApiMethodCall method, ApiContext context, IEnumerable<object> arguments)
        {
            if (context.RequestContext.RouteData.DataTokens.ContainsKey(DataTokenConstants.CheckPayment)
                && !(bool)context.RequestContext.RouteData.DataTokens[DataTokenConstants.CheckPayment])
            {
                log.Debug("Payment is not required");
            }
            else
            {
                var header = context.RequestContext.HttpContext.Request.Headers["Payment-Info"];
                bool flag;
                if (string.IsNullOrEmpty(header) || (bool.TryParse(header, out flag) && flag))
                {
                    var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                    if (tenant == null)
                    {
                        var hostname = string.Empty;
                        try
                        {
                            hostname = HttpContext.Current.Request.GetUrlRewriter().Host;
                        }
                        catch
                        {
                        }
                        throw new System.Security.SecurityException(string.Format("Portal {0} not found.", hostname));
                    }

                    var tenantStatus = tenant.Status;
                    if (tenantStatus == TenantStatus.Transfering)
                    {
                        context.RequestContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                        context.RequestContext.HttpContext.Response.StatusDescription = HttpStatusCode.ServiceUnavailable.ToString();
                        log.WarnFormat("Portal {0} is transfering to another region", context.RequestContext.HttpContext.Request.Url);
                    }

                    var tariff = CoreContext.PaymentManager.GetTariff(tenant.TenantId);
                    if (tenantStatus != TenantStatus.Active || tariff.State >= TariffState.NotPaid)
                    {
                        context.RequestContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.PaymentRequired;
                        context.RequestContext.HttpContext.Response.StatusDescription = HttpStatusCode.PaymentRequired.ToString();
                        log.WarnFormat("Payment Required {0}.", context.RequestContext.HttpContext.Request.Url);
                    }
                }
            }

            if (!SecurityContext.IsAuthenticated) return;

            var pid = FindProduct(method);
            if (pid != Guid.Empty)
            {
                if (CallContext.GetData("asc.web.product_id") == null)
                {
                    CallContext.SetData("asc.web.product_id", pid);
                }
                if (!WebItemSecurity.IsAvailableForMe(pid))
                {
                    context.RequestContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.RequestContext.HttpContext.Response.StatusDescription = HttpStatusCode.Forbidden.ToString();
                    log.WarnFormat("Product {0} denied for user {1}", method.Name, SecurityContext.CurrentAccount);
                }
            }
        }


        private static Guid FindProduct(IApiMethodCall method)
        {
            if (method == null || string.IsNullOrEmpty(method.Name))
            {
                return default(Guid);
            }
            if (method.Name == "community" && !string.IsNullOrEmpty(method.RoutingUrl))
            {
                var module = method.RoutingUrl.Split('/')[0];
                if (products.ContainsKey(module))
                {
                    return products[module];
                }
            }
            if (products.ContainsKey(method.Name))
            {
                return products[method.Name];
            }
            return default(Guid);
        }
    }
}