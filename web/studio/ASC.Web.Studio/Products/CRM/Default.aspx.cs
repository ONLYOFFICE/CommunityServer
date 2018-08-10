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
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Contacts;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;
using ASC.CRM.Core;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Configuration;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.CRM
{
    public partial class Contacts : BasePage
    {
        #region Property

        public static String Location
        {
            get { return "~/products/crm/default.aspx"; }
        }

        #endregion

        #region Events

        protected override void PageLoad()
        {
            InitControls();
            ProductEntryPoint.ConfigurePortal();
        }

        #endregion

        #region Methods

        protected void InitControls()
        {
            int contactID;

            if (int.TryParse(UrlParameters.ID, out contactID))
            {
                var targetContact = DaoFactory.ContactDao.GetByID(contactID);

                if (targetContact == null || !CRMSecurity.CanAccessTo(targetContact))
                {
                    Response.Redirect(PathProvider.StartURL());
                }

                if (string.Compare(UrlParameters.Action, "manage", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (CRMSecurity.CanEdit(targetContact))
                    {
                        ExecContactActionView(targetContact);
                    }
                    else
                    {
                        Response.Redirect(PathProvider.StartURL());
                    }
                }
                else
                {
                    ExecContactDetailsView(targetContact);
                }

                _ctrlContactID.Value = targetContact.ID.ToString();
            }
            else
            {
                if (string.Compare(UrlParameters.Action, "manage", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ExecContactActionView(null);
                }
                else if (string.Compare(UrlParameters.Action, "import", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ExecImportView();
                }
                else
                {
                    ExecListContactView();
                }
            }
        }

        protected void ExecImportView()
        {
            var importViewControl = (ImportFromCSVView)LoadControl(ImportFromCSVView.Location);
            importViewControl.EntityType = EntityType.Contact;

            CommonContainerHolder.Controls.Add(importViewControl);

            Master.CurrentPageCaption = CRMContactResource.ImportContacts;
            Title = HeaderStringHelper.GetPageTitle(CRMContactResource.ImportContacts);
        }

        protected void ExecContactDetailsView(Contact targetContact)
        {
            var contactDetailsViewControl = (ContactDetailsView)LoadControl(ContactDetailsView.Location);
            contactDetailsViewControl.TargetContact = targetContact;

            CommonContainerHolder.Controls.Add(contactDetailsViewControl);

            var title = targetContact.GetTitle();

            Master.CurrentPageCaption = title;

            Master.CommonContainerHeader = Global.RenderItemHeaderWithMenu(title.HtmlEncode(), targetContact is Company ? EntityType.Company : EntityType.Person, false, CRMSecurity.CanEdit(targetContact));

            Title = HeaderStringHelper.GetPageTitle(title);
        }

        protected void ExecListContactView()
        {
            CommonContainerHolder.Controls.Add(LoadControl(ListContactView.Location));
            Title = HeaderStringHelper.GetPageTitle(Master.CurrentPageCaption ?? CRMContactResource.AllContacts);
            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
        }

        protected void ExecContactActionView(Contact targetContact)
        {
            var contactActionViewControl = (ContactActionView)LoadControl(ContactActionView.Location);
            contactActionViewControl.TargetContact = targetContact;

            if (targetContact == null)
            {
                if (string.Compare(UrlParameters.Type, "people", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    contactActionViewControl.TypeAddedContact = "company";
                    contactActionViewControl.SaveContactButtonText = CRMContactResource.AddThisCompanyButton;
                    contactActionViewControl.SaveAndCreateContactButtonText = CRMContactResource.AddThisAndCreateCompanyButton;

                    contactActionViewControl.AjaxProgressText = CRMContactResource.AddingCompany;
                    Master.CurrentPageCaption = CRMContactResource.BreadCrumbsAddCompany;
                    Title = HeaderStringHelper.GetPageTitle(CRMContactResource.BreadCrumbsAddCompany);
                }
                else
                {
                    contactActionViewControl.TypeAddedContact = "people";
                    contactActionViewControl.SaveContactButtonText = CRMContactResource.AddThisPersonButton;
                    contactActionViewControl.SaveAndCreateContactButtonText = CRMContactResource.AddThisAndCreatePeopleButton;

                    contactActionViewControl.AjaxProgressText = CRMContactResource.AddingPersonProgress;
                    Master.CurrentPageCaption = CRMContactResource.BreadCrumbsAddPerson;
                    Title = HeaderStringHelper.GetPageTitle(CRMContactResource.BreadCrumbsAddPerson);
                }
            }
            else
            {
                var contactTitle = targetContact.GetTitle();

                contactActionViewControl.SaveAndCreateContactButtonText = string.Compare(UrlParameters.Type, "people", StringComparison.OrdinalIgnoreCase) != 0 ? CRMContactResource.SaveThisAndCreateCompanyButton : CRMContactResource.SaveThisAndCreatePeopleButton;

                contactActionViewControl.SaveContactButtonText = CRMContactResource.SaveChanges;
                contactActionViewControl.AjaxProgressText = CRMContactResource.SavingChangesProgress;

                if (targetContact is Company)
                {
                    contactActionViewControl.TypeAddedContact = "company";
                    var headerTitle = String.Format(CRMContactResource.EditCompany, contactTitle);
                    Master.CurrentPageCaption = headerTitle;
                    Title = HeaderStringHelper.GetPageTitle(headerTitle);
                }
                else
                {
                    contactActionViewControl.TypeAddedContact = "people";
                    var headerTitle = String.Format(CRMContactResource.EditPerson, contactTitle);
                    Master.CurrentPageCaption = headerTitle;
                    Title = HeaderStringHelper.GetPageTitle(headerTitle);
                }
            }

            CommonContainerHolder.Controls.Add(contactActionViewControl);
        }

        #endregion
    }
}