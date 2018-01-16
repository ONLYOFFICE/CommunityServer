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
                ASC.CRM.Core.Entities.Invoice targetInvoice = Global.DaoFactory.GetInvoiceDao().GetByID(invoiceID);

                if (targetInvoice == null || !CRMSecurity.CanAccessTo(targetInvoice))
                    Response.Redirect(PathProvider.StartURL() + "invoices.aspx");

                if (String.Compare(UrlParameters.Action, "edit", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!CRMSecurity.CanEdit(targetInvoice))
                        Response.Redirect(PathProvider.StartURL() + "invoices.aspx");

                    ExecInvoiceActionView(targetInvoice, InvoiceActionType.Edit);
                }
                else if (String.Compare(UrlParameters.Action, "duplicate", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ExecInvoiceActionView(targetInvoice, InvoiceActionType.Duplicate);
                }
                else if (String.Compare(UrlParameters.Action, "pdf", StringComparison.OrdinalIgnoreCase) == 0 && Global.CanDownloadInvoices)
                {
                    if (!Global.CanDownloadInvoices)
                        Response.Redirect(PathProvider.StartURL() + "invoices.aspx");

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
            var pdfFile = Global.GetInvoicePdfExistingOrCreate(targetInvoice);

            MessageService.Send(HttpContext.Current.Request, MessageAction.InvoiceDownloaded, MessageTarget.Create(targetInvoice.ID), targetInvoice.Number);

            Response.Redirect(CommonLinkUtility.GetFullAbsolutePath(pdfFile.FileDownloadUrl));
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