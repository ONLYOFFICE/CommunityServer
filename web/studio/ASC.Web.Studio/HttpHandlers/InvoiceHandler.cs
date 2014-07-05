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
using System.Web;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;
using log4net;
using MimeMapping = System.Web.MimeMapping;

namespace ASC.Web.Studio.HttpHandlers
{
    public class InvoiceHandler : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InvoiceHandler));


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