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
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Contacts;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;
using ASC.CRM.Core;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Configuration;

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
                var targetContact = Global.DaoFactory.GetContactDao().GetByID(contactID);

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

            var title = targetContact.GetTitle().HtmlEncode();

            Master.CurrentPageCaption = title;

            Master.CommonContainerHeader = Global.RenderItemHeaderWithMenu(title, targetContact is Company ? EntityType.Company : EntityType.Person, false, CRMSecurity.CanEdit(targetContact));

            Title = HeaderStringHelper.GetPageTitle(title);
        }

        protected void ExecListContactView()
        {
            CommonContainerHolder.Controls.Add(LoadControl(ListContactView.Location));
            Title = HeaderStringHelper.GetPageTitle(Master.CurrentPageCaption ?? CRMContactResource.AllContacts);
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