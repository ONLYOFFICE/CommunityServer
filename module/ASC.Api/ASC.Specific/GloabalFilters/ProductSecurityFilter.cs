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

using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Api.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Web.Core;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace ASC.Specific.GloabalFilters
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
            var birthdays = new Guid("37620ae5-c40b-45ce-855a-39dd7d76a1fa");
            var community = new Guid("ea942538-e68e-4907-9394-035336ee0ba8");
            var crm = new Guid("6743007c-6f95-4d20-8c88-a8601ce5e76d");
            var files = new Guid("e67be73d-f9ae-4ce1-8fec-1880cb518cb4");
            var project = new Guid("1e044602-43b5-4d79-82f3-fd6208a11960");
            var calendar = new Guid("32d24cb5-7ece-4606-9c94-19216ba42086");
            var mail = new Guid("2a923037-8b2d-487b-9a22-5ac0918acf3f");
            products = new Dictionary<string, Guid>
            {
                { "blog", blog },
                { "bookmark", bookmark },
                { "event", news },
                { "forum", forum },
                { "photo", photo },
                { "wiki", wiki },
                { "birthdays", birthdays },
                { "community", community },
                { "crm", crm },
                { "files", files },
                { "project", project },
                { "calendar", calendar },
                { "mail", mail },
            };
        }


        public ProductSecurityFilter()
        {
            log = ServiceLocator.Current.GetInstance<ILog>();
        }


        public override void PreMethodCall(IApiMethodCall method, ApiContext context, IEnumerable<object> arguments)
        {
            var header = context.RequestContext.HttpContext.Request.Headers["Payment-Info"];
            var flag = true;
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
                    catch { }
                    throw new System.Security.SecurityException(string.Format("Portal {0} not found.", hostname));
                }

                var tenantStatus = tenant.Status;
                if (tenantStatus == TenantStatus.Transfering)
                {
                    context.RequestContext.HttpContext.Response.StatusCode = 503;
                    context.RequestContext.HttpContext.Response.StatusDescription = "Service Unavailable";
                    log.Warn("Portal {0} is transfering to another region", context.RequestContext.HttpContext.Request.Url);
                }

                var tariff = CoreContext.PaymentManager.GetTariff(tenant.TenantId);
                if (tenantStatus != TenantStatus.Active || tariff.State == TariffState.NotPaid)
                {
                    context.RequestContext.HttpContext.Response.StatusCode = 402;
                    context.RequestContext.HttpContext.Response.StatusDescription = "Payment Required.";
                    log.Warn("Payment Required {0}.", context.RequestContext.HttpContext.Request.Url);
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
                if (!WebItemSecurity.IsAvailableForUser(pid.ToString(), SecurityContext.CurrentAccount.ID))
                {
                    context.RequestContext.HttpContext.Response.StatusCode = 403;
                    context.RequestContext.HttpContext.Response.StatusDescription = "Access denied.";
                    log.Warn("Product {0} denied for user {1}", method.Name, SecurityContext.CurrentAccount);
                }
            }
        }


        private Guid FindProduct(IApiMethodCall method)
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
