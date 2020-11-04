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
            get { return "~/Products/CRM/Default.aspx"; }
        }

        protected string HelpLink { get; set; }

        #endregion

        #region Events

        protected override void PageLoad()
        {
            HelpLink = CommonLinkUtility.GetHelpLink();
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

            TabsHolder.Visible = true;

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