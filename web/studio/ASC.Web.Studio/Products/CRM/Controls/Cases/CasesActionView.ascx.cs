/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Text;

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

        private const string ErrorCookieKey = "save_cases_error";

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

        #region Save Or Update Case

        protected void SaveOrUpdateCase(Object sender, CommandEventArgs e)
        {
            try
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
                                      : "cases.aspx?action=manage", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.CRM").Error(ex);
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
            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.CasesActionView.init(""{0}"");",
                ErrorCookieKey);

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}