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
using System.Linq;
using System.Web.UI.WebControls;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.CRM.Core;
using ASC.Web.CRM.Controls.Common;
using ASC.Core;
using ASC.Web.Studio.Core.Users;
using Newtonsoft.Json.Linq;
using System.Web;

namespace ASC.Web.CRM.Controls.Cases
{
    public partial class CasesActionView : BaseUserControl
    {
        #region Properies

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Cases/CasesActionView.ascx"); }
        }

        public ASC.CRM.Core.Entities.Cases TargetCase { get; set; }

        protected bool HavePermission { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (TargetCase != null)
            {
                HavePermission = CRMSecurity.IsAdmin || TargetCase.CreateBy == SecurityContext.CurrentAccount.ID;
            }
            else
            {
                HavePermission = true;
            }

            if (IsPostBack) return;

            if (TargetCase != null)
            {
                saveCaseButton.Text = CRMCasesResource.SaveChanges;
                cancelButton.Attributes.Add("href", String.Format("cases.aspx?id={0}", TargetCase.ID));
                RegisterClientScriptHelper.DataListContactTab(Page, TargetCase.ID, EntityType.Case);
            }
            else
            {
                saveCaseButton.Text = CRMCasesResource.CreateThisCaseButton;
                saveAndCreateCaseButton.Text = CRMCasesResource.AddThisAndCreateCaseButton;
                cancelButton.Attributes.Add("href",
                                            Request.UrlReferrer != null && String.CompareOrdinal(Request.UrlReferrer.PathAndQuery, Request.Url.PathAndQuery) != 0
                                                ? Request.UrlReferrer.OriginalString
                                                : "cases.aspx");
            }

            RegisterClientScriptHelper.DataCasesActionView(Page, TargetCase);

            if (HavePermission)
            {
                InitPrivatePanel();
            }
            RegisterScript();
        }

        #endregion

        #region Methods

        public String GetCaseTitle()
        {
            return TargetCase == null ? String.Empty : TargetCase.Title.HtmlEncode();
        }

        protected void InitPrivatePanel()
        {
            var cntrlPrivatePanel = (PrivatePanel)LoadControl(PrivatePanel.Location);

            cntrlPrivatePanel.CheckBoxLabel = CRMCasesResource.PrivatePanelCheckBoxLabel;

            if (TargetCase != null)
            {
                cntrlPrivatePanel.IsPrivateItem = CRMSecurity.IsPrivate(TargetCase);
                if (cntrlPrivatePanel.IsPrivateItem)
                    cntrlPrivatePanel.SelectedUsers = CRMSecurity.GetAccessSubjectTo(TargetCase);
            }

            var usersWhoHasAccess = new List<string> {CustomNamingPeople.Substitute<CRMCommonResource>("CurrentUser").HtmlEncode()};

            cntrlPrivatePanel.UsersWhoHasAccess = usersWhoHasAccess;
            cntrlPrivatePanel.DisabledUsers = new List<Guid> {SecurityContext.CurrentAccount.ID};
            phPrivatePanel.Controls.Add(cntrlPrivatePanel);
        }

        protected void SaveOrUpdateCase(Object sender, CommandEventArgs e)
        {
            int caseID;

            if (TargetCase != null)
            {
                caseID = TargetCase.ID;
                TargetCase.Title = Request["caseTitle"];
                Global.DaoFactory.GetCasesDao().UpdateCases(TargetCase);
                MessageService.Send(HttpContext.Current.Request, MessageAction.CaseUpdated, TargetCase.Title);
                SetPermission(TargetCase);
            }
            else
            {
                caseID = Global.DaoFactory.GetCasesDao().CreateCases(Request["caseTitle"]);
                var newCase = Global.DaoFactory.GetCasesDao().GetByID(caseID);
                MessageService.Send(HttpContext.Current.Request, MessageAction.CaseCreated, newCase.Title);
                SetPermission(newCase);
            }


            Global.DaoFactory.GetCasesDao().SetMembers(caseID,
                                                       !String.IsNullOrEmpty(Request["memberID"])
                                                           ? Request["memberID"].Split(',').Select(
                                                               id => Convert.ToInt32(id)).ToArray()
                                                           : new List<int>().ToArray());


            var assignedTags = Request["baseInfo_assignedTags"];
            if (assignedTags != null)
            {
                var oldTagList = Global.DaoFactory.GetTagDao().GetEntityTags(EntityType.Case, caseID);
                foreach (var tag in oldTagList)
                {
                    Global.DaoFactory.GetTagDao().DeleteTagFromEntity(EntityType.Case, caseID, tag);
                }
                if (assignedTags != string.Empty)
                {
                    var tagListInfo = JObject.Parse(assignedTags)["tagListInfo"].ToArray();
                    var newTagList = tagListInfo.Select(t => t.ToString()).ToArray();
                    Global.DaoFactory.GetTagDao().SetTagToEntity(EntityType.Case, caseID, newTagList);
                }
            }

            foreach (var customField in Request.Form.AllKeys)
            {
                if (!customField.StartsWith("customField_")) continue;
                int fieldID = Convert.ToInt32(customField.Split('_')[1]);
                string fieldValue = Request.Form[customField];

                if (String.IsNullOrEmpty(fieldValue) && TargetCase == null)
                    continue;

                Global.DaoFactory.GetCustomFieldDao().SetFieldValue(EntityType.Case, caseID, fieldID, fieldValue);
            }

            Response.Redirect(string.Compare(e.CommandArgument.ToString(), "0", StringComparison.OrdinalIgnoreCase) == 0
                                  ? string.Format("cases.aspx?id={0}", caseID)
                                  : "cases.aspx?action=manage");
        }

        protected void SetPermission(ASC.CRM.Core.Entities.Cases caseItem)
        {
            if (HavePermission)
            {

                var isPrivate = false;
                var notifyPrivateUsers = false;

                bool value;
                if (bool.TryParse(Request.Form["isPrivateCase"], out value))
                {
                    isPrivate = value;
                }
                if (bool.TryParse(Request.Form["notifyPrivateUsers"], out value))
                {
                    notifyPrivateUsers = value;
                }

                if (isPrivate)
                {
                    var selectedUserList = Request["selectedUsersCase"]
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(item => new Guid(item)).ToList();

                    if (notifyPrivateUsers)
                    {
                        Services.NotifyService.NotifyClient.Instance.SendAboutSetAccess(EntityType.Case, caseItem.ID, selectedUserList.ToArray());
                    }

                    selectedUserList.Add(SecurityContext.CurrentAccount.ID);
                    CRMSecurity.SetAccessTo(caseItem, selectedUserList);
                }
                else
                {
                    CRMSecurity.MakePublic(caseItem);
                }
            }
        }

        private void RegisterScript()
        {
            const string script = "ASC.CRM.CasesActionView.init();";

            Page.RegisterInlineScript(script);
        }

        #endregion
    }
}