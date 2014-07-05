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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.UI.WebControls;
using ASC.CRM.Core.Entities;
using ASC.Core;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.CRM.Core;
using ASC.Web.Studio.Core;
using System.Web;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json;

namespace ASC.Web.CRM.Controls.Invoices
{
    public partial class InvoiceActionView : BaseUserControl
    {
        #region Properies

        public static string Location { get { return PathProvider.GetFileStaticRelativePath("Invoices/InvoiceActionView.ascx"); } }

        public InvoiceActionType ActionType { get; set; }

        public Invoice TargetInvoice { get; set; }

        private const string ErrorCookieKey = "save_invoice_error";

        protected string InvoicesNumber
        {
            get
            {
                var number = ActionType == InvoiceActionType.Edit ? TargetInvoice.Number : Global.DaoFactory.GetInvoiceDao().GetNewInvoicesNumber();
                return number.HtmlEncode();
            }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            bool havePermission;

            if (ActionType == InvoiceActionType.Duplicate)
            {
                havePermission = TargetInvoice != null && CRMSecurity.CanAccessTo(TargetInvoice);
            }
            else
            {
                havePermission = TargetInvoice == null || CRMSecurity.CanEdit(TargetInvoice);
            }

            if (!havePermission)
            {
                Response.Redirect(PathProvider.StartURL() + "invoices.aspx");
            }

            RegisterClientScriptHelper.DataInvoicesActionView(Page, TargetInvoice);

            InitCountriesCombobox();

            InitActionButtons();

            RegisterScript();
        }

        protected void SaveOrUpdateInvoice(Object sender, CommandEventArgs e)
        {
            try
            {
                var dao = Global.DaoFactory;

                var invoice = GetInvoice();
                var billingAddressID = Convert.ToInt32(Request["billingAddressID"]);
                var deliveryAddressID = Convert.ToInt32(Request["deliveryAddressID"]);


                var messageAction = invoice.ID == 0 ? MessageAction.InvoiceCreated : MessageAction.InvoiceUpdated;
                invoice.ID = dao.GetInvoiceDao().SaveOrUpdateInvoice(invoice);
                MessageService.Send(HttpContext.Current.Request, messageAction, invoice.Number);

                var invoiceLines = GetInvoiceLines();

                foreach (var line in invoiceLines)
                {
                    line.InvoiceID = invoice.ID;
                    line.ID = dao.GetInvoiceLineDao().SaveOrUpdateInvoiceLine(line);
                }

                RemoveUnusedLines(invoice.ID, invoiceLines);

                dao.GetInvoiceDao().UpdateInvoiceJsonData(invoice, billingAddressID, deliveryAddressID);

                if (Global.CanDownloadInvoices)
                {
                    var th = new Thread(UpdateInvoiceFileID);
                    th.Start(new NewThreadParams
                        {
                            Ctx = HttpContext.Current,
                            Url = HttpContext.Current.Request.Url,
                            TenantId = TenantProvider.CurrentTenantID,
                            CurrentUser = SecurityContext.CurrentAccount.ID,
                            InvoiceId = invoice.ID
                        });
                }

                string redirectUrl;
                if (ActionType == InvoiceActionType.Create && UrlParameters.ContactID != 0)
                {
                    redirectUrl = string.Format("default.aspx?id={0}#invoices", UrlParameters.ContactID);
                }
                else
                {
                    if (e.CommandArgument.ToString() == "1")
                    {
                        redirectUrl = "invoices.aspx?action=create";
                    }
                    else
                    {
                        redirectUrl = string.Format("invoices.aspx?id={0}", invoice.ID);
                    }
                }

                Response.Redirect(redirectUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                var cookie = HttpContext.Current.Request.Cookies.Get(ErrorCookieKey);
                if (cookie == null)
                {
                    cookie = new HttpCookie(ErrorCookieKey)
                        {
                            Value = ex.Message
                        };
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }
        }

        #endregion

        #region Methods

        private void InitCountriesCombobox()
        {
            var country = new List<string> { CRMJSResource.ChooseCountry };
            country.AddRange(Global.GetCountryListExt());

            invoiceContactCountry.DataSource = country;
            invoiceContactCountry.DataBind();
        }

        private void InitActionButtons()
        {
            saveButton.OnClientClick = String.Format("return ASC.CRM.InvoiceActionView.submitForm('{0}');", saveButton.UniqueID);
            saveAndCreateNewButton.OnClientClick = String.Format("return ASC.CRM.InvoiceActionView.submitForm('{0}');", saveAndCreateNewButton.UniqueID);

            if (ActionType == InvoiceActionType.Create)
            {
                saveButton.Text = CRMInvoiceResource.AddThisInvoiceButton;
                saveAndCreateNewButton.Text = CRMInvoiceResource.AddAndCreateNewInvoiceButton;
                cancelButton.Attributes.Add("href",
                                            Request.UrlReferrer != null && Request.Url != null && String.Compare(Request.UrlReferrer.PathAndQuery, Request.Url.PathAndQuery) != 0
                                                ? Request.UrlReferrer.OriginalString
                                                : "invoices.aspx");
            }

            if (ActionType == InvoiceActionType.Edit)
            {
                saveButton.Text = CRMCommonResource.SaveChanges;
                cancelButton.Attributes.Add("href", String.Format("invoices.aspx?id={0}", TargetInvoice.ID));
            }

            if (ActionType == InvoiceActionType.Duplicate)
            {
                saveButton.Text = CRMInvoiceResource.DuplicateInvoiceButton;
                saveAndCreateNewButton.Text = CRMInvoiceResource.DuplicateAndCreateNewInvoiceButton;
                cancelButton.Attributes.Add("href", String.Format("invoices.aspx?id={0}", TargetInvoice.ID));
            }
        }

        private void RegisterScript()
        {
            Page.RegisterClientScript(typeof(Masters.ClientScripts.ExchangeRateViewData));

            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.InvoiceActionView.init({0}, ""{1}"");",
                (int)ContactSelectorTypeEnum.All,
                ErrorCookieKey
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        private Invoice GetInvoice()
        {
            var dao = Global.DaoFactory;

            var invoice = new Invoice();

            if (ActionType == InvoiceActionType.Edit)
            {
                invoice.ID = TargetInvoice.ID;
                invoice.Number = TargetInvoice.Number;
                invoice.FileID = TargetInvoice.FileID;
            }
            else
            {
                invoice.Number = Request["invoiceNumber"];
                if (dao.GetInvoiceDao().IsExist(invoice.Number))
                    throw new Exception("invoice with same number is already exist");
            }

            DateTime issueDate;
            if (!DateTime.TryParse(Request["invoiceIssueDate"], out issueDate))
                throw new Exception("invalid issueDate");
            invoice.IssueDate = issueDate;

            invoice.ContactID = Convert.ToInt32(Request["invoiceContactID"]);
            if (invoice.ContactID <= 0) throw new Exception("conctact is null");
            var contact = dao.GetContactDao().GetByID(invoice.ContactID);
            if (contact == null || !CRMSecurity.CanAccessTo(contact))
            {
                throw new Exception("conctact is null");
            }

            invoice.ConsigneeID = Convert.ToInt32(Request["invoiceConsigneeID"]);
            if (invoice.ConsigneeID > 0)
            {
                var consignee = dao.GetContactDao().GetByID(invoice.ConsigneeID);
                if (consignee == null || !CRMSecurity.CanAccessTo(consignee))
                {
                    throw new Exception("consignee is null");
                }
            }
            else
            {
                invoice.ConsigneeID = 0;
            }


            invoice.EntityType = EntityType.Opportunity;

            invoice.EntityID = Convert.ToInt32(Request["invoiceOpportunityID"]);
            if (invoice.EntityID > 0)
            {
                var deal = dao.GetDealDao().GetByID(invoice.EntityID);
                if (deal == null || !CRMSecurity.CanAccessTo(deal))
                    throw new Exception("opportunity is null");

                var dealMembers = dao.GetDealDao().GetMembers(invoice.EntityID);
                if (!dealMembers.Contains(invoice.ContactID))
                    throw new Exception("contact doesn't have this opportunity");
            }

            DateTime dueDate;
            if (!DateTime.TryParse(Request["invoiceDueDate"], out dueDate))
                throw new Exception("invalid dueDate");
            if (issueDate > dueDate)
                throw new Exception("Issue date more than due date");
            invoice.DueDate = dueDate;

            invoice.Language = Request["invoiceLanguage"];
            if (string.IsNullOrEmpty(invoice.Language) || SetupInfo.EnabledCultures.All(c => c.Name != invoice.Language))
                throw new Exception("language is null");

            invoice.Currency = Request["invoiceCurrency"];
            if (string.IsNullOrEmpty(invoice.Currency))
            {
                throw new Exception("currency is null");
            }
            else
            {
                invoice.Currency = invoice.Currency.ToUpper();
                if (CurrencyProvider.Get(invoice.Currency) == null)
                {
                    throw new Exception("currency is null");
                }
            }

            invoice.ExchangeRate = Convert.ToDecimal(Request["invoiceExchangeRate"], new CultureInfo("en-US"));
            if (invoice.ExchangeRate <= 0)
                throw new Exception("empty exchange rate");

            invoice.PurchaseOrderNumber = Request["invoicePurchaseOrderNumber"];

            invoice.Terms = Request["invoiceTerms"];
            if (string.IsNullOrEmpty(invoice.Terms))
                throw new Exception("terms is null");

            invoice.Description = Request["invoiceDescription"];

            invoice.Status = InvoiceStatus.Draft;

            invoice.TemplateType = InvoiceTemplateType.Eur;

            return invoice;
        }

        private List<InvoiceLine> GetInvoiceLines()
        {
            var dao = Global.DaoFactory;

            var invoiceLines = new List<InvoiceLine>();

            if (!Request.Form.AllKeys.Any(x => x.StartsWith("iLineItem_")))
                throw new Exception("invoiceItems list is empty");

            foreach (var customField in Request.Form.AllKeys)
            {
                if (!customField.StartsWith("iLineItem_")) continue;

                var id = Convert.ToInt32(customField.Split('_')[1]);
                var sortOrder = Convert.ToInt32(customField.Split('_')[2]);

                var invoiceItemID = Convert.ToInt32(Request["iLineItem_" + id + "_" + sortOrder]);
                var invoiceTax1ID = Convert.ToInt32(Request["iLineTax1_" + id + "_" + sortOrder]);
                var invoiceTax2ID = Convert.ToInt32(Request["iLineTax2_" + id + "_" + sortOrder]);

                if (!dao.GetInvoiceItemDao().IsExist(invoiceItemID))
                    throw new Exception("invoiceItem is null");

                if (invoiceTax1ID > 0 && !dao.GetInvoiceTaxDao().IsExist(invoiceTax1ID))
                    throw new Exception("invoiceTax is null");

                if (invoiceTax2ID > 0 && !dao.GetInvoiceTaxDao().IsExist(invoiceTax2ID))
                    throw new Exception("invoiceTax is null");

                var line = new InvoiceLine
                {
                    ID = id,
                    InvoiceItemID = invoiceItemID,
                    InvoiceTax1ID = invoiceTax1ID,
                    InvoiceTax2ID = invoiceTax2ID,
                    Description = Request["iLineDescription_" + id + "_" + sortOrder],
                    Quantity = Convert.ToInt32(Request["iLineQuantity_" + id + "_" + sortOrder]),
                    Price = Convert.ToDecimal(Request["iLinePrice_" + id + "_" + sortOrder], new CultureInfo("en-US")),
                    Discount = Convert.ToInt32(Request["iLineDiscount_" + id + "_" + sortOrder]),
                    SortOrder = sortOrder
                };

                invoiceLines.Add(line);
            }

            return invoiceLines;
        }

        private void RemoveUnusedLines(int invoiceID, List<InvoiceLine> lines)
        {
            var dao = Global.DaoFactory;
            var oldLines = dao.GetInvoiceLineDao().GetInvoiceLines(invoiceID);

            foreach (var line in oldLines)
            {
                var contains = lines.Any(x => x.ID == line.ID);
                if (!contains)
                {
                    dao.GetInvoiceLineDao().DeleteInvoiceLine(line.ID);
                }
            }
        }


        private void UpdateInvoiceFileID(object parameters)
        {
            var obj = (NewThreadParams)parameters;

            var tenant = CoreContext.TenantManager.GetTenant(obj.TenantId);

            CoreContext.TenantManager.SetCurrentTenant(tenant);

            SecurityContext.AuthenticateMe(obj.CurrentUser);

            HttpContext.Current = obj.Ctx;

            PdfCreator.CreateAndSaveFile(obj.InvoiceId);
        }

        #endregion
    }

    public class NewThreadParams
    {
        public int TenantId { get; set; }
        public int InvoiceId { get; set; }
        public HttpContext Ctx { get; set; }
        public Uri Url { get; set; }
        public Guid CurrentUser { get; set; }
    }
}