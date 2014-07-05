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

#region Import

using System;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;
using ASC.CRM.Core;
using ASC.Web.CRM.Controls.Invoices;

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
        }

        protected void ExecInvoiceDetailsView(ASC.CRM.Core.Entities.Invoice targetInvoice)
        {
            var invoiceDetailsView = (InvoiceDetailsView)LoadControl(InvoiceDetailsView.Location);

            invoiceDetailsView.TargetInvoice = targetInvoice;
            CommonContainerHolder.Controls.Add(invoiceDetailsView);

            var title = targetInvoice.Number.HtmlEncode();

            Master.CurrentPageCaption = title;
            Master.CommonContainerHeader = Global.RenderItemHeaderWithMenu(title, EntityType.Invoice, false, true);
            Title = HeaderStringHelper.GetPageTitle(title);
        }

        protected void ExecInvoicePdfView(ASC.CRM.Core.Entities.Invoice targetInvoice)
        {
            var pdfFile = Global.GetInvoicePdfExistingOrCreate(targetInvoice);
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