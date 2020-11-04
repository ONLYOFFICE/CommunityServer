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
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using System.Linq;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;
using ASC.Web.CRM.Controls.Common;
using ASC.Core;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Studio.Controls.Users;
using ASC.Web.Studio.Core.Users;
using ASC.Core.Tenants;
using ASC.Core.Users;
using System.Text;
using ASC.Common.Logging;
using ASC.CRM.Core.Dao;
using ASC.Web.CRM.Core;
using Autofac;

#endregion

namespace ASC.Web.CRM.Controls.Deals
{
    public partial class DealActionView : BaseUserControl
    {
        #region Members

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Deals/DealActionView.ascx"); }
        }

        public Deal TargetDeal { get; set; }

        protected bool HavePermission { get; set; }

        private const string ErrorCookieKey = "save_opportunity_error";

        #endregion

        #region Events

        public bool IsSelectedBidCurrency(String abbreviation)
        {
            return TargetDeal != null ?
                       String.Compare(abbreviation, TargetDeal.BidCurrency) == 0 :
                       String.Compare(abbreviation, Global.TenantSettings.DefaultCurrency.Abbreviation, true) == 0;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (TargetDeal != null)
            {
                HavePermission = CRMSecurity.IsAdmin || TargetDeal.CreateBy == SecurityContext.CurrentAccount.ID;
            }
            else
            {
                HavePermission = true;
            }

            if (TargetDeal == null)
            {
                saveDealButton.Text = CRMDealResource.AddThisDealButton;
                saveAndCreateDealButton.Text = CRMDealResource.AddThisAndCreateDealButton;

                cancelButton.Attributes.Add("href",
                                            Request.UrlReferrer != null && Request.Url != null && String.Compare(Request.UrlReferrer.PathAndQuery, Request.Url.PathAndQuery) != 0
                                                ? Request.UrlReferrer.OriginalString
                                                : "Deals.aspx");
            }
            else
            {
                saveDealButton.Text = CRMCommonResource.SaveChanges;

                cancelButton.Attributes.Add("href", String.Format("Deals.aspx?id={0}", TargetDeal.ID));

                RegisterClientScriptHelper.DataListContactTab(Page, TargetDeal.ID, EntityType.Opportunity);
            }

            RegisterClientScriptHelper.DataDealActionView(Page, TargetDeal);

            if (HavePermission)
            {
                InitPrivatePanel();
            }
            RegisterScript();
        }

        protected void InitPrivatePanel()
        {
            var cntrlPrivatePanel = (PrivatePanel)LoadControl(PrivatePanel.Location);

            cntrlPrivatePanel.CheckBoxLabel = CRMDealResource.PrivatePanelCheckBoxLabel;

            if (TargetDeal != null)
            {
                cntrlPrivatePanel.IsPrivateItem = CRMSecurity.IsPrivate(TargetDeal);
                if (cntrlPrivatePanel.IsPrivateItem)
                    cntrlPrivatePanel.SelectedUsers = CRMSecurity.GetAccessSubjectTo(TargetDeal);
            }

            var usersWhoHasAccess = new List<string> { CustomNamingPeople.Substitute<CRMCommonResource>("CurrentUser"), CRMDealResource.ResponsibleDeal };

            cntrlPrivatePanel.UsersWhoHasAccess = usersWhoHasAccess;
            cntrlPrivatePanel.DisabledUsers = new List<Guid> { SecurityContext.CurrentAccount.ID };
            phPrivatePanel.Controls.Add(cntrlPrivatePanel);

        }

        #endregion

        #region Save Or Update Deal

        protected void SaveOrUpdateDeal(Object sender, CommandEventArgs e)
        {
            try
            {
                using (var scope = DIHelper.Resolve())
                {
                    var dao = scope.Resolve<DaoFactory>();
                    int dealID;

                    var _isPrivate = false;
                    var _selectedUsersWithoutCurUsr = new List<Guid>();

                    #region BaseInfo

                    var deal = new Deal
                    {
                        Title = Request["nameDeal"],
                        Description = Request["descriptionDeal"],
                        DealMilestoneID = Convert.ToInt32(Request["dealMilestone"])
                    };

                    int contactID;
                    if (int.TryParse(Request["selectedContactID"], out contactID))
                        deal.ContactID = contactID;

                    int probability;
                    if (int.TryParse(Request["probability"], out probability))
                        deal.DealMilestoneProbability = probability;

                    if (deal.DealMilestoneProbability < 0) deal.DealMilestoneProbability = 0;
                    if (deal.DealMilestoneProbability > 100) deal.DealMilestoneProbability = 100;

                    deal.BidCurrency = Request["bidCurrency"];

                    if (String.IsNullOrEmpty(deal.BidCurrency))
                        deal.BidCurrency = Global.TenantSettings.DefaultCurrency.Abbreviation;
                    else
                        deal.BidCurrency = deal.BidCurrency.ToUpper();

                    if (!String.IsNullOrEmpty(Request["bidValue"]))
                    {
                        decimal bidValue;
                        if (!decimal.TryParse(Request["bidValue"], out bidValue))
                            bidValue = 0;

                        deal.BidValue = bidValue;
                        deal.BidType = (BidType) Enum.Parse(typeof(BidType), Request["bidType"]);

                        if (deal.BidType != BidType.FixedBid)
                        {
                            int perPeriodValue;

                            if (int.TryParse(Request["perPeriodValue"], out perPeriodValue))
                                deal.PerPeriodValue = perPeriodValue;
                        }
                    }
                    else
                    {
                        deal.BidValue = 0;
                        deal.BidType = BidType.FixedBid;
                    }

                    DateTime expectedCloseDate;

                    if (!DateTime.TryParse(Request["expectedCloseDate"], out expectedCloseDate))
                        expectedCloseDate = DateTime.MinValue;
                    deal.ExpectedCloseDate = expectedCloseDate;

                    deal.ResponsibleID = new Guid(Request["responsibleID"]);

                    #endregion

                    #region Validation

                    CRMSecurity.DemandCreateOrUpdate(deal);

                    if (HavePermission)
                    {
                        var CurrentAccountID = SecurityContext.CurrentAccount.ID;

                        bool value;
                        if (bool.TryParse(Request.Form["isPrivateDeal"], out value))
                        {
                            _isPrivate = value;
                        }

                        if (_isPrivate)
                        {
                            _selectedUsersWithoutCurUsr = Request.Form["selectedPrivateUsers"]
                                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(item => new Guid(item)).Where(i => i != CurrentAccountID).Distinct().ToList();

                            foreach (var uID in _selectedUsersWithoutCurUsr)
                            {
                                var usr = CoreContext.UserManager.GetUsers(uID);
                                if (usr.IsVisitor()) throw new ArgumentException();
                            }

                            if (deal.ResponsibleID != CurrentAccountID)
                            {
                                _selectedUsersWithoutCurUsr.Add(deal.ResponsibleID);

                                var responsible = CoreContext.UserManager.GetUsers(deal.ResponsibleID);
                                if (responsible.IsVisitor())
                                    throw new ArgumentException(CRMErrorsResource.ResponsibleCannotBeVisitor);
                            }
                        }
                    }


                    #endregion

                    var dealMilestone = dao.DealMilestoneDao.GetByID(deal.DealMilestoneID);
                    if (TargetDeal == null)
                    {
                        if (dealMilestone.Status != DealMilestoneStatus.Open)
                            deal.ActualCloseDate = TenantUtil.DateTimeNow();

                        dealID = dao.DealDao.CreateNewDeal(deal);
                        deal.ID = dealID;
                        deal.CreateBy = SecurityContext.CurrentAccount.ID;
                        deal.CreateOn = TenantUtil.DateTimeNow();
                        deal = dao.DealDao.GetByID(dealID);

                        SetPermission(deal, _isPrivate, _selectedUsersWithoutCurUsr);

                        if (deal.ResponsibleID != Guid.Empty && deal.ResponsibleID != SecurityContext.CurrentAccount.ID)
                        {
                            NotifyClient.Instance.SendAboutResponsibleForOpportunity(deal);
                        }
                        MessageService.Send(HttpContext.Current.Request, MessageAction.OpportunityCreated,
                            MessageTarget.Create(deal.ID), deal.Title);
                    }
                    else
                    {
                        dealID = TargetDeal.ID;
                        deal.ID = TargetDeal.ID;
                        deal.ActualCloseDate = TargetDeal.ActualCloseDate;

                        if (TargetDeal.ResponsibleID != Guid.Empty && TargetDeal.ResponsibleID != deal.ResponsibleID)
                            NotifyClient.Instance.SendAboutResponsibleForOpportunity(deal);


                        if (TargetDeal.DealMilestoneID != deal.DealMilestoneID)
                            deal.ActualCloseDate = dealMilestone.Status != DealMilestoneStatus.Open
                                ? TenantUtil.DateTimeNow()
                                : DateTime.MinValue;

                        dao.DealDao.EditDeal(deal);
                        deal = dao.DealDao.GetByID(dealID);
                        SetPermission(deal, _isPrivate, _selectedUsersWithoutCurUsr);
                        MessageService.Send(HttpContext.Current.Request, MessageAction.OpportunityUpdated,
                            MessageTarget.Create(deal.ID), deal.Title);
                    }

                    #region Members

                    var dealMembers = !String.IsNullOrEmpty(Request["selectedMembersID"])
                        ? Request["selectedMembersID"].Split(new[] {','}).Select(
                            id => Convert.ToInt32(id)).Where(id => id != deal.ContactID).ToList()
                        : new List<int>();

                    var dealMembersContacts =
                        dao.ContactDao.GetContacts(dealMembers.ToArray()).Where(CRMSecurity.CanAccessTo).ToList();
                    dealMembers = dealMembersContacts.Select(m => m.ID).ToList();

                    if (deal.ContactID > 0)
                        dealMembers.Add(deal.ContactID);

                    dao.DealDao.SetMembers(dealID, dealMembers.ToArray());

                    #endregion

                    #region CustomFields

                    foreach (var customField in Request.Form.AllKeys)
                    {
                        if (!customField.StartsWith("customField_")) continue;

                        var fieldID = Convert.ToInt32(customField.Split('_')[1]);

                        var fieldValue = Request.Form[customField];

                        if (String.IsNullOrEmpty(fieldValue) && TargetDeal == null)
                            continue;

                        dao.CustomFieldDao.SetFieldValue(EntityType.Opportunity, dealID, fieldID, fieldValue);

                    }

                    #endregion

                    string redirectUrl;
                    if (TargetDeal == null && UrlParameters.ContactID != 0)
                    {
                        redirectUrl =
                            string.Format(
                                e.CommandArgument.ToString() == "1"
                                    ? "Deals.aspx?action=manage&contactID={0}"
                                    : "Default.aspx?id={0}#deals", UrlParameters.ContactID);
                    }
                    else
                    {
                        redirectUrl = e.CommandArgument.ToString() == "1"
                            ? "Deals.aspx?action=manage"
                            : string.Format("Deals.aspx?id={0}", dealID);
                    }

                    Response.Redirect(redirectUrl, false);
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

        protected void SetPermission(Deal deal, bool isPrivate, List<Guid> selectedUsersWithoutCurUsr)
        {
            if (HavePermission)
            {
                var notifyPrivateUsers = false;

                bool value;
                if (bool.TryParse(Request.Form["notifyPrivateUsers"], out value))
                {
                    notifyPrivateUsers = value;
                }

                if (isPrivate)
                {
                    if (notifyPrivateUsers)
                        Services.NotifyService.NotifyClient.Instance.SendAboutSetAccess(EntityType.Opportunity, deal.ID, DaoFactory, selectedUsersWithoutCurUsr.ToArray());

                    selectedUsersWithoutCurUsr.Add(SecurityContext.CurrentAccount.ID);

                    CRMSecurity.SetAccessTo(deal, selectedUsersWithoutCurUsr);
                }
                else
                {
                    CRMSecurity.MakePublic(deal);
                }
            }
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"ASC.CRM.DealActionView.init(""{0}"", ""{1}"");",
                TenantUtil.DateTimeNow().ToString(DateTimeExtension.DateFormatPattern),
                ErrorCookieKey);

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}