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


#region Import

using System;
using System.Web;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;
using ASC.CRM.Core;
using ASC.Web.CRM.Controls.Invoices;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

#endregion

namespace ASC.Web.CRM
{
    public partial class Invoices : BasePage
    {
        #region Properies

        #endregion

        #region Events

        protected override void PageLoad()
        {
            int invoiceID;

            if (int.TryParse(UrlParameters.ID, out invoiceID))
            {
                ASC.CRM.Core.Entities.Invoice targetInvoice = DaoFactory.InvoiceDao.GetByID(invoiceID);

                if (targetInvoice == null || !CRMSecurity.CanAccessTo(targetInvoice))
                    Response.Redirect(PathProvider.StartURL() + "Invoices.aspx");

                if (String.Compare(UrlParameters.Action, "edit", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!CRMSecurity.CanEdit(targetInvoice))
                        Response.Redirect(PathProvider.StartURL() + "Invoices.aspx");

                    ExecInvoiceActionView(targetInvoice, InvoiceActionType.Edit);
                }
                else if (String.Compare(UrlParameters.Action, "duplicate", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ExecInvoiceActionView(targetInvoice, InvoiceActionType.Duplicate);
                }
                else if (String.Compare(UrlParameters.Action, "pdf", StringComparison.OrdinalIgnoreCase) == 0 && Global.CanDownloadInvoices)
                {
                    if (!Global.CanDownloadInvoices)
                        Response.Redirect(PathProvider.StartURL() + "Invoices.aspx");

                    ExecInvoicePdfView(targetInvoice);
                }
                else
                {
                    ExecInvoiceDetailsView(targetInvoice);
                }
            }
            else
            {
                if (String.Compare(UrlParameters.Action, "create", StringComparison.OrdinalIgnoreCase) == 0) {
                    ExecInvoiceActionView(null, InvoiceActionType.Create);
                }
                //else if (String.Compare(UrlParameters.Action, "import", true) == 0) {
                //    ExecImportView();
                //}
                else {
                    ExecListInvoiceView();
                }
            }
        }

        #endregion

        #region Methods

        protected void ExecListInvoiceView()
        {
            CommonContainerHolder.Controls.Add(LoadControl(ListInvoiceView.Location));
            Title = HeaderStringHelper.GetPageTitle(Master.CurrentPageCaption ?? CRMInvoiceResource.AllInvoices);
            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
        }

        protected void ExecInvoiceDetailsView(ASC.CRM.Core.Entities.Invoice targetInvoice)
        {
            var invoiceDetailsView = (InvoiceDetailsView)LoadControl(InvoiceDetailsView.Location);

            invoiceDetailsView.TargetInvoice = targetInvoice;
            CommonContainerHolder.Controls.Add(invoiceDetailsView);

            var title = targetInvoice.Number;

            Master.CurrentPageCaption = title;
            Master.CommonContainerHeader = Global.RenderItemHeaderWithMenu(title.HtmlEncode(), EntityType.Invoice, false, true);
            Title = HeaderStringHelper.GetPageTitle(title);
        }

        protected void ExecInvoicePdfView(ASC.CRM.Core.Entities.Invoice targetInvoice)
        {
            var pdfFile = Global.GetInvoicePdfExistingOrCreate(targetInvoice, DaoFactory);

            MessageService.Send(HttpContext.Current.Request, MessageAction.InvoiceDownloaded, MessageTarget.Create(targetInvoice.ID), targetInvoice.Number);

            Response.Redirect(CommonLinkUtility.GetFullAbsolutePath(pdfFile.DownloadUrl));
        }

        protected void ExecInvoiceActionView(ASC.CRM.Core.Entities.Invoice targetInvoice, InvoiceActionType actionType)
        {
            var invoiceActionViewControl = (InvoiceActionView)LoadControl(InvoiceActionView.Location);

            invoiceActionViewControl.ActionType = actionType;
            invoiceActionViewControl.TargetInvoice = targetInvoice;

            CommonContainerHolder.Controls.Add(invoiceActionViewControl);

            string headerTitle;

            switch (actionType)
            {
                case InvoiceActionType.Create:
                    headerTitle = CRMInvoiceResource.CreateNewInvoice;
                    break;
                case InvoiceActionType.Edit:
                    headerTitle = String.Format(CRMInvoiceResource.EditInvoiceHeader, targetInvoice.Number);
                    break;
                case InvoiceActionType.Duplicate:
                    headerTitle = String.Format(CRMInvoiceResource.DuplicateInvoiceHeader, targetInvoice.Number);
                    break;
                default:
                    headerTitle = CRMInvoiceResource.CreateNewInvoice;
                    break;
            }

            Master.CurrentPageCaption = headerTitle;
            Title = HeaderStringHelper.GetPageTitle(headerTitle);
        }

        #endregion

    }
}