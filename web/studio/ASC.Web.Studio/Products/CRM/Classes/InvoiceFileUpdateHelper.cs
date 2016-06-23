/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Core;
using ASC.Web.CRM.Controls.Invoices;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace ASC.Web.CRM.Classes
{
    public class InvoiceFileUpdateHelper
    {
        private void UpdateInvoiceFileID(object parameters)
        {
            var obj = (NewThreadParams)parameters;

            var tenant = CoreContext.TenantManager.GetTenant(obj.TenantId);

            CoreContext.TenantManager.SetCurrentTenant(tenant);

            SecurityContext.AuthenticateMe(obj.CurrentUser);

            HttpContext.Current = obj.Ctx;

            log4net.LogManager.GetLogger("ASC.CRM").DebugFormat("Thread for updating file for Invoice with ID = {0} is working...", obj.InvoiceId);

            PdfCreator.CreateAndSaveFile(obj.InvoiceId);
        }


        public void UpdateInvoiceFileIDInThread(int invoiceID)
        {
            log4net.LogManager.GetLogger("ASC.CRM").DebugFormat("Create thread for updating file for Invoice with ID = {0}", invoiceID);
            var th = new Thread(UpdateInvoiceFileID);
            th.Start(new NewThreadParams
                {
                    Ctx = HttpContext.Current,
                    Url = HttpContext.Current.Request.Url,
                    TenantId = TenantProvider.CurrentTenantID,
                    CurrentUser = SecurityContext.CurrentAccount.ID,
                    InvoiceId = invoiceID
                });
        }
    }
}