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
using ASC.Common.Logging;
using ASC.CRM.Core.Dao;
using ASC.ElasticSearch;
using ASC.Web.CRM.Core;
using ASC.Web.CRM.Core.Search;
using Autofac;

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
                cancelButton.Attributes.Add("href", String.Format("Cases.aspx?id={0}", TargetCase.ID));
                RegisterClientScriptHelper.DataListContactTab(Page, TargetCase.ID, EntityType.Case);
            }
            else
            {
                saveCaseButton.Text = CRMCasesResource.CreateThisCaseButton;
                saveAndCreateCaseButton.Text = CRMCasesResource.AddThisAndCreateCaseButton;
                cancelButton.Attributes.Add("href",
                                            Request.UrlReferrer != null && String.CompareOrdinal(Request.UrlReferrer.PathAndQuery, Request.Url.PathAndQuery) != 0
                                                ? Request.UrlReferrer.OriginalString
                                                : "Cases.aspx");
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

        protected void SaveOrUpdateCase(object sender, CommandEventArgs e)
        {
            try
            {
                using (var scope = DIHelper.Resolve())
                {
                    var daoFactory = scope.Resolve<DaoFactory>();
                    int caseID;

                    if (TargetCase != null)
                    {
                        caseID = TargetCase.ID;
                        TargetCase.Title = Request["caseTitle"];
                        daoFactory.CasesDao.UpdateCases(TargetCase);
                        FactoryIndexer<CasesWrapper>.UpdateAsync(TargetCase);
                        MessageService.Send(HttpContext.Current.Request, MessageAction.CaseUpdated, MessageTarget.Create(TargetCase.ID), TargetCase.Title);
                        SetPermission(TargetCase);
                    }
                    else
                    {
                        caseID = daoFactory.CasesDao.CreateCases(Request["caseTitle"]);
                        var newCase = daoFactory.CasesDao.GetByID(caseID);
                        FactoryIndexer<CasesWrapper>.IndexAsync(newCase);
                        MessageService.Send(HttpContext.Current.Request, MessageAction.CaseCreated, MessageTarget.Create(newCase.ID), newCase.Title);
                        SetPermission(newCase);
                    }


                    daoFactory.CasesDao.SetMembers(caseID,
                        !String.IsNullOrEmpty(Request["memberID"])
                            ? Request["memberID"].Split(',').Select(
                                id => Convert.ToInt32(id)).ToArray()
                            : new List<int>().ToArray());


                    var assignedTags = Request["baseInfo_assignedTags"];
                    if (assignedTags != null)
                    {
                        var oldTagList = daoFactory.TagDao.GetEntityTags(EntityType.Case, caseID);
                        foreach (var tag in oldTagList)
                        {
                            daoFactory.TagDao.DeleteTagFromEntity(EntityType.Case, caseID, tag);
                        }
                        if (assignedTags != string.Empty)
                        {
                            var tagListInfo = JObject.Parse(assignedTags)["tagListInfo"].ToArray();
                            var newTagList = tagListInfo.Select(t => t.ToString()).ToArray();
                            daoFactory.TagDao.SetTagToEntity(EntityType.Case, caseID, newTagList);
                        }
                    }

                    foreach (var customField in Request.Form.AllKeys)
                    {
                        if (!customField.StartsWith("customField_")) continue;
                        int fieldID = Convert.ToInt32(customField.Split('_')[1]);
                        string fieldValue = Request.Form[customField];

                        if (String.IsNullOrEmpty(fieldValue) && TargetCase == null)
                            continue;

                        daoFactory.CustomFieldDao.SetFieldValue(EntityType.Case, caseID, fieldID, fieldValue);
                    }

                    Response.Redirect(
                        string.Compare(e.CommandArgument.ToString(), "0", StringComparison.OrdinalIgnoreCase) == 0
                            ? string.Format("Cases.aspx?id={0}", caseID)
                            : "Cases.aspx?action=manage", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.CRM").Error(ex);
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

            var usersWhoHasAccess = new List<string> {CustomNamingPeople.Substitute<CRMCommonResource>("CurrentUser")};

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
                        Services.NotifyService.NotifyClient.Instance.SendAboutSetAccess(EntityType.Case, caseItem.ID, DaoFactory,selectedUserList.ToArray());
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