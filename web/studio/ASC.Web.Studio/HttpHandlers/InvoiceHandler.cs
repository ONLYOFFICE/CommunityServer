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
using System.Linq;
using System.Web;
using ASC.Common.Logging;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;
using MimeMapping = System.Web.MimeMapping;

namespace ASC.Web.Studio.HttpHandlers
{
    public class InvoiceHandler : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger("ASC");


        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (!SecurityContext.IsAuthenticated && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin())
                {
                    throw new HttpException(403, "Access denied.");
                }

                var pid = context.Request.QueryString["pid"];
                if (string.IsNullOrEmpty(pid))
                {
                    throw new HttpException(400, "Bad request.");
                }

                if (CoreContext.PaymentManager.GetTariffPayments(TenantProvider.CurrentTenantID).All(p => p.CartId != pid))
                {
                    throw new HttpException(403, "Access denied.");
                }

                var invoice = CoreContext.PaymentManager.GetPaymentInvoice(pid);
                if (invoice == null || string.IsNullOrEmpty(invoice.Sale))
                {
                    throw new HttpException(404, "Not found.");
                }

                var pdf = Convert.FromBase64String(invoice.Sale);

                context.Response.Clear();
                context.Response.ContentType = MimeMapping.GetMimeMapping(".pdf");
                context.Response.AddHeader("Content-Disposition", "inline; filename=\"" + pid + ".pdf\"");
                context.Response.AddHeader("Content-Length", pdf.Length.ToString());

                for (int i = 0, count = 1024; i < pdf.Length; i += count)
                {
                    context.Response.OutputStream.Write(pdf, i, Math.Min(count, pdf.Length - i));
                }
                context.Response.Flush();
            }
            catch (HttpException he)
            {
                context.Response.StatusCode = he.GetHttpCode();
                context.Response.Write(HttpUtility.HtmlEncode(he.Message));
            }
            catch (Exception error)
            {
                log.ErrorFormat("Url: {0} {1}", context.Request.Url, error);
                context.Response.StatusCode = 500;
                context.Response.Write(HttpUtility.HtmlEncode(error.Message));
            }
        }
    }
}