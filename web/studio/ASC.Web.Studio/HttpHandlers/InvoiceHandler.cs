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