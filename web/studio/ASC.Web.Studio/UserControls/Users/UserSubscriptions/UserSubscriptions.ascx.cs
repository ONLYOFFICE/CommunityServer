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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Core.Users;
using ASC.Web.Studio.Core;
using AjaxPro;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Core;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Users
{
    [AjaxNamespace("AjaxPro.SubscriptionManager")]
    public partial class UserSubscriptions : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserSubscriptions/UserSubscriptions.ascx"; }
        }

        protected UserInfo CurrentUser;

        protected bool IsAdmin;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            CurrentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            IsAdmin = CurrentUser.IsAdmin();
        }

        #region Init Notify by comboboxes

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            try
            {
                Page.RegisterBodyScripts("~/UserControls/Users/UserSubscriptions/js/subscription_manager.js")
                    .RegisterInlineScript("CommonSubscriptionManager.InitNotifyByComboboxes();")
                    .RegisterStyle("~/UserControls/Users/UserSubscriptions/css/subscriptions.less");
            }
            catch
            {
            }
        }

        #endregion

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object GetAllSubscriptions()
        {
            var result = new { Items = new List<object>() };
            var isFirst = true;
            foreach (var item in WebItemManager.Instance.GetItems(Web.Core.WebZones.WebZoneType.All)
                                               .FindAll(i => i.Context != null && i.Context.SubscriptionManager is IProductSubscriptionManager))
            {
                try
                {
                    result.Items.Add(GetItemSubscriptions(item, isFirst));
                    isFirst = false;
                }
                catch (Exception)
                {
                }
            }
            return result;
        }

        private object GetItemSubscriptions(IWebItem webItem, bool isOpen)
        {
            var isEmpty = true;
            var canUnsubscribe = false;

            var groups = new List<object>();
            var types = new List<object>();
            var itemType = 1;

            var recipient = GetCurrentRecipient();

            var productSubscriptionManager = webItem.Context.SubscriptionManager as IProductSubscriptionManager;
            if (productSubscriptionManager.GroupByType == GroupByType.Modules)
            {
                foreach (var subItem in WebItemManager.Instance.GetSubItems(webItem.ID))
                {
                    if (subItem.Context == null || subItem.Context.SubscriptionManager == null)
                        continue;

                    var subscriptionTypes = subItem.Context.SubscriptionManager.GetSubscriptionTypes();
                    if (subscriptionTypes == null || subscriptionTypes.Count == 0)
                        continue;
                    else
                        subscriptionTypes = subscriptionTypes.FindAll(type => (type.IsEmptySubscriptionType != null && !type.IsEmptySubscriptionType(webItem.ID, subItem.ID, type.ID)));

                    if (subscriptionTypes == null || subscriptionTypes.Count == 0)
                        continue;

                    var group = new
                        {
                            Id = subItem.ID,
                            ImageUrl = subItem.GetIconAbsoluteURL(),
                            Name = subItem.Name.HtmlEncode(),
                            Types = new List<object>()
                        };

                    foreach (var type in subscriptionTypes)
                    {
                        var t = new
                            {
                                Id = type.ID,
                                Name = type.Name.HtmlEncode(),
                                Single = type.Single,
                                IsSubscribed = type.CanSubscribe ? subItem.Context.SubscriptionManager.SubscriptionProvider.IsSubscribed(type.NotifyAction, recipient, null) : true
                            };
                        if (t.IsSubscribed)
                            canUnsubscribe = true;

                        group.Types.Add(t);
                    }

                    groups.Add(group);
                    isEmpty = false;
                }

            }
            else if (productSubscriptionManager.GroupByType == GroupByType.Groups)
            {
                var subscriptionTypes = productSubscriptionManager.GetSubscriptionTypes();
                var subscriptionGroups = productSubscriptionManager.GetSubscriptionGroups();
                if (subscriptionTypes != null && subscriptionGroups != null)
                {
                    foreach (var gr in subscriptionGroups)
                    {
                        var sTypes = subscriptionTypes.FindAll(type => (type.IsEmptySubscriptionType != null && !type.IsEmptySubscriptionType(webItem.ID, gr.ID, type.ID)));
                        if (sTypes == null || sTypes.Count == 0)
                            continue;

                        var group = new
                            {
                                Id = gr.ID,
                                ImageUrl = "",
                                Name = gr.Name.HtmlEncode(),
                                Types = new List<object>()
                            };

                        foreach (var type in sTypes)
                        {
                            var t = new
                                {
                                    Id = type.ID,
                                    Name = type.Name.HtmlEncode(),
                                    Single = type.Single,
                                    IsSubscribed = type.CanSubscribe ? productSubscriptionManager.SubscriptionProvider.IsSubscribed(type.NotifyAction, recipient, null) : true
                                };

                            if (t.IsSubscribed)
                                canUnsubscribe = true;

                            group.Types.Add(t);
                        }

                        groups.Add(group);
                        isEmpty = false;
                    }
                }

            }
            else if (productSubscriptionManager.GroupByType == GroupByType.Simple)
            {
                itemType = 0;
                var subscriptionTypes = productSubscriptionManager.GetSubscriptionTypes();
                if (subscriptionTypes != null)
                {
                    foreach (var type in subscriptionTypes)
                    {
                        if (type.IsEmptySubscriptionType != null && type.IsEmptySubscriptionType(webItem.ID, webItem.ID, type.ID))
                            continue;

                        var t = new
                            {
                                Id = type.ID,
                                Name = type.Name.HtmlEncode(),
                                Single = type.Single,
                                IsSubscribed = !type.CanSubscribe || !productSubscriptionManager.SubscriptionProvider.IsUnsubscribe((IDirectRecipient)recipient, type.NotifyAction, null)
                            };
                        if (t.IsSubscribed)
                            canUnsubscribe = true;

                        types.Add(t);
                        isEmpty = false;
                    }
                }
            }

            return new
                {
                    Id = webItem.ID,
                    LogoUrl = webItem.GetIconAbsoluteURL(),
                    Name = HttpUtility.HtmlEncode(webItem.Name),
                    IsEmpty = isEmpty,
                    IsOpen = isOpen,
                    CanUnSubscribe = canUnsubscribe,
                    NotifyType = GetNotifyByMethod(webItem.ID),
                    Groups = groups,
                    Types = types,
                    Type = itemType,
                    Class = webItem.ProductClassName
                };
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object RenderGroupItemSubscriptions(Guid itemId, Guid subItemId, Guid subscriptionTypeId)
        {
            try
            {
                SubscriptionType type = null;

                var productSubscriptionManager = WebItemManager.Instance[itemId].Context.SubscriptionManager as IProductSubscriptionManager;

                ISubscriptionManager subscriptionManager = productSubscriptionManager;

                if (productSubscriptionManager.GroupByType == GroupByType.Modules)
                    subscriptionManager = WebItemManager.Instance[subItemId].Context.SubscriptionManager;

                var subscriptionTypes = subscriptionManager.GetSubscriptionTypes();
                if (subscriptionTypes != null && subscriptionTypes.Count != 0)
                {
                    type = (from s in subscriptionTypes
                            where s.ID.Equals(subscriptionTypeId)
                            select s).Single<SubscriptionType>();
                }

                var result = new { Status = 1, ItemId = itemId, SubItemId = subItemId, TypeId = subscriptionTypeId, Objects = new List<object>() };

                if (type.Single || type.CanSubscribe)
                    return result;

                if (type.IsEmptySubscriptionType != null && type.IsEmptySubscriptionType(itemId, subItemId, subscriptionTypeId))
                    return result;

                if (type.GetSubscriptionObjects == null)
                    return result;

                var typeObjects = type.GetSubscriptionObjects(itemId, subItemId, subscriptionTypeId);
                if (typeObjects == null || typeObjects.Count == 0)
                    return result;

                typeObjects.Sort((s1, s2) => String.Compare(s1.Name, s2.Name, true));

                foreach (var subscription in typeObjects)
                {
                    result.Objects.Add(new
                        {
                            Id = subscription.ID,
                            Name = HttpUtility.HtmlEncode(subscription.Name),
                            Url = String.IsNullOrEmpty(subscription.URL) ? "" : subscription.URL,
                            IsSubscribed = subscriptionManager.SubscriptionProvider.IsSubscribed(type.NotifyAction,
                                                                                                                       GetCurrentRecipient(),
                                                                                                                       subscription.ID)
                        });
                }

                return result;
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }

        }

        #region what's new

        protected string RenderWhatsNewSubscriptionState()
        {
            return RenderWhatsNewSubscriptionState(StudioNotifyHelper.IsSubscribedToNotify(CurrentUser, Actions.SendWhatsNew));
        }

        protected string RenderWhatsNewSubscriptionState(bool isSubscribe)
        {
            if (isSubscribe)
                return "<a class=\"on_off_button on\" href=\"javascript:CommonSubscriptionManager.SubscribeToWhatsNew();\" title=\"" + Resources.Resource.UnsubscribeButton + "\"></a>";
            else
                return "<a class=\"on_off_button off\" href=\"javascript:CommonSubscriptionManager.SubscribeToWhatsNew();\" title=\"" + Resources.Resource.SubscribeButton + "\"></a>";
        }

        protected string RenderWhatsNewNotifyByCombobox()
        {
            var notifyBy = ConvertToNotifyByValue(StudioSubscriptionManager.Instance, Actions.SendWhatsNew);

            return string.Format("<span class=\"subsSelector subs-notice-text\" data-notify=\"{0}\" data-function=\"SetWhatsNewNotifyByMethod\"></span>", (int)notifyBy);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SetWhatsNewNotifyByMethod(int notifyBy)
        {
            try
            {
                var resp = new AjaxResponse();
                var notifyByList = ConvertToNotifyByList((NotifyBy)notifyBy);
                SetNotifyBySubsriptionTypes(notifyByList, StudioSubscriptionManager.Instance, Actions.SendWhatsNew);
                return resp;
            }
            catch
            {
                return null;
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeToWhatsNew()
        {
            var resp = new AjaxResponse { rs1 = "0" };
            try
            {
                var recipient = StudioNotifyHelper.ToRecipient(SecurityContext.CurrentAccount.ID);

                var isSubscribed = StudioNotifyHelper.IsSubscribedToNotify(recipient, Actions.SendWhatsNew);

                StudioNotifyHelper.SubscribeToNotify(recipient, Actions.SendWhatsNew, !isSubscribed);

                resp.rs2 = RenderWhatsNewSubscriptionState(!isSubscribed);

                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs2 = e.Message.HtmlEncode();
            }

            return resp;

        }

        #endregion

        #region tips&tricks

        protected string RenderTipsAndTricksSubscriptionState()
        {
            return RenderTipsAndTricksSubscriptionState(StudioNotifyHelper.IsSubscribedToNotify(CurrentUser, Actions.PeriodicNotify));
        }

        protected string RenderTipsAndTricksSubscriptionState(bool isSubscribe)
        {
            if (isSubscribe)
                return "<a class=\"on_off_button on\" href=\"javascript:CommonSubscriptionManager.SubscribeToTipsAndTricks();\" title=\"" + Resources.Resource.UnsubscribeButton + "\"></a>";
            else
                return "<a class=\"on_off_button off\" href=\"javascript:CommonSubscriptionManager.SubscribeToTipsAndTricks();\" title=\"" + Resources.Resource.SubscribeButton + "\"></a>";
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeToTipsAndTricks()
        {
            var resp = new AjaxResponse { rs1 = "0" };
            try
            {
                var recipient = StudioNotifyHelper.ToRecipient(SecurityContext.CurrentAccount.ID);

                var isSubscribe = StudioNotifyHelper.IsSubscribedToNotify(recipient, Actions.PeriodicNotify);

                StudioNotifyHelper.SubscribeToNotify(recipient, Actions.PeriodicNotify, !isSubscribe);

                resp.rs2 = RenderTipsAndTricksSubscriptionState(!isSubscribe);

                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs2 = e.Message.HtmlEncode();
            }

            return resp;
        }

        #endregion

        #region spam

        protected bool IsVisibleSpamSubscription()
        {
            return TenantExtra.Saas && SetupInfo.IsVisibleSettings("SpamSubscription");
        }

        private const string TeamlabSiteDbId = "teamlabsite";

        private const string TemplateUnsubscribeTable = "template_unsubscribe";

        private static IDbManager GetDb()
        {
            return DbManager.FromHttpContext(TeamlabSiteDbId);
        }

        private static void UnsubscribeFromSpam(string email)
        {
            using (var db = GetDb())
            {
                var query = new SqlInsert(TemplateUnsubscribeTable, true)
                    .InColumnValue("email", email.ToLowerInvariant());

                db.ExecuteScalar<int>(query);
            }
        }

        private static void SubscribeToSpam(string email)
        {
            using (var db = GetDb())
            {
                db.ExecuteScalar<int>(new SqlDelete(TemplateUnsubscribeTable).Where("email", email.ToLowerInvariant()));
            }
        }

        private static bool IsSubscribedToSpam(string email)
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(TemplateUnsubscribeTable)
                    .SelectCount()
                    .Where("email", email);

                return db.ExecuteScalar<int>(query) == 0;
            }
        }

        protected string RenderSpamSubscriptionState()
        {
            var isSubscribed = IsSubscribedToSpam(CurrentUser.Email);

            return RenderSpamSubscriptionState(isSubscribed);
        }

        protected string RenderSpamSubscriptionState(bool isSubscribed)
        {
            if (isSubscribed)
                return "<a class=\"on_off_button on\" href=\"javascript:CommonSubscriptionManager.SubscribeToSpam();\" title=\"" + Resources.Resource.UnsubscribeButton + "\"></a>";
            else
                return "<a class=\"on_off_button off\" href=\"javascript:CommonSubscriptionManager.SubscribeToSpam();\" title=\"" + Resources.Resource.SubscribeButton + "\"></a>";
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeToSpam()
        {
            var resp = new AjaxResponse { rs1 = "0" };
            try
            {
                if (!IsVisibleSpamSubscription())
                    throw new MissingMethodException();

                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                var isSubscribed = IsSubscribedToSpam(user.Email);

                if (isSubscribed)
                {
                    UnsubscribeFromSpam(user.Email);
                }
                else
                {
                    SubscribeToSpam(user.Email);
                }

                resp.rs2 = RenderTipsAndTricksSubscriptionState(!isSubscribed);

                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs2 = e.Message.HtmlEncode();
            }

            return resp;
        }

        #endregion

        #region admin notifies

        protected string RenderAdminNotifySubscriptionState()
        {
            return RenderAdminNotifySubscriptionState(StudioNotifyHelper.IsSubscribedToNotify(CurrentUser, Actions.AdminNotify));
        }

        protected string RenderAdminNotifySubscriptionState(bool isSubscribe)
        {
            if (isSubscribe)
                return "<a class=\"on_off_button on\" href=\"javascript:CommonSubscriptionManager.SubscribeToAdminNotify();\" title=\"" + Resources.Resource.UnsubscribeButton + "\"></a>";
            else
                return "<a class=\"on_off_button off\" href=\"javascript:CommonSubscriptionManager.SubscribeToAdminNotify();\" title=\"" + Resources.Resource.SubscribeButton + "\"></a>";
        }

        protected string RenderAdminNotifyNotifyByCombobox()
        {
            var notifyBy = ConvertToNotifyByValue(StudioSubscriptionManager.Instance, Actions.AdminNotify);

            return string.Format("<span class=\"subsSelector subs-notice-text\" data-notify=\"{0}\" data-function=\"SetAdminNotifyNotifyByMethod\"></span>", (int)notifyBy);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SetAdminNotifyNotifyByMethod(int notifyBy)
        {
            try
            {
                var resp = new AjaxResponse();
                var notifyByList = ConvertToNotifyByList((NotifyBy)notifyBy);
                SetNotifyBySubsriptionTypes(notifyByList, StudioSubscriptionManager.Instance, Actions.AdminNotify);
                return resp;
            }
            catch
            {
                return null;
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeToAdminNotify()
        {
            var resp = new AjaxResponse { rs1 = "0" };
            try
            {
                var recipient = StudioNotifyHelper.ToRecipient(SecurityContext.CurrentAccount.ID);

                var isSubscribe = StudioNotifyHelper.IsSubscribedToNotify(recipient, Actions.AdminNotify);

                StudioNotifyHelper.SubscribeToNotify(recipient, Actions.AdminNotify, !isSubscribe);

                resp.rs2 = RenderAdminNotifySubscriptionState(!isSubscribe);

                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs2 = e.Message.HtmlEncode();
            }

            return resp;

        }

        #endregion


        private IRecipient GetCurrentRecipient()
        {
            return new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), SecurityContext.CurrentAccount.Name);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeObject(Guid itemId, Guid subItemID, Guid subscriptionTypeID, string objID, bool subscribe)
        {
            var resp = new AjaxResponse { rs2 = itemId.ToString(), rs3 = subItemID.ToString(), rs4 = subscriptionTypeID.ToString() };

            try
            {
                ISubscriptionManager subscriptionManager = null;
                var item = WebItemManager.Instance[itemId];
                var productSubscriptionManager = item.Context.SubscriptionManager as IProductSubscriptionManager;

                if (productSubscriptionManager.GroupByType == GroupByType.Groups || productSubscriptionManager.GroupByType == GroupByType.Simple)
                    subscriptionManager = productSubscriptionManager;

                else if (productSubscriptionManager.GroupByType == GroupByType.Modules)
                    subscriptionManager = WebItemManager.Instance[subItemID].Context.SubscriptionManager;

                if (subscriptionManager != null)
                {
                    var types = subscriptionManager.GetSubscriptionTypes();
                    var type = types.Find(t => t.ID.Equals(subscriptionTypeID));

                    resp.rs5 = objID;
                    if (subscribe)
                        subscriptionManager.SubscriptionProvider.Subscribe(type.NotifyAction, objID, GetCurrentRecipient());
                    else
                        subscriptionManager.SubscriptionProvider.UnSubscribe(type.NotifyAction, objID, GetCurrentRecipient());
                }
                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs6 = "<div class='errorBox'>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
            }
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object UnsubscribeType(Guid itemId, Guid subItemId, Guid subscriptionTypeID)
        {
            try
            {
                var item = WebItemManager.Instance[itemId];
                var productSubscriptionManager = item.Context.SubscriptionManager as IProductSubscriptionManager;

                if (productSubscriptionManager.GroupByType == GroupByType.Groups)
                {
                    var type = productSubscriptionManager.GetSubscriptionTypes().Find(t => t.ID.Equals(subscriptionTypeID));
                    if (type != null && type.CanSubscribe)
                        productSubscriptionManager.SubscriptionProvider.UnSubscribe(type.NotifyAction, null, GetCurrentRecipient());
                    else
                    {
                        var objs = productSubscriptionManager.GetSubscriptionObjects(subItemId);
                        objs.RemoveAll(o => !o.SubscriptionGroup.ID.Equals(subItemId) || !o.SubscriptionType.ID.Equals(subscriptionTypeID));

                        var subscriptionProvider = productSubscriptionManager.SubscriptionProvider;
                        var currentRecipient = GetCurrentRecipient();

                        foreach (var o in objs)
                            subscriptionProvider.UnSubscribe(o.SubscriptionType.NotifyAction, o.ID, currentRecipient);
                    }
                }

                else if (productSubscriptionManager.GroupByType == GroupByType.Modules
                         || productSubscriptionManager.GroupByType == GroupByType.Simple)
                {
                    var subItem = WebItemManager.Instance[subItemId];
                    if (subItem != null && subItem.Context != null && subItem.Context.SubscriptionManager != null)
                    {
                        var subscriptionType = subItem.Context.SubscriptionManager.GetSubscriptionTypes().Find(st => st.ID.Equals(subscriptionTypeID));
                        if (subscriptionType.CanSubscribe)
                            subItem.Context.SubscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, null, GetCurrentRecipient());
                        else
                            subItem.Context.SubscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, GetCurrentRecipient());
                    }
                }

                var data = GetItemSubscriptions(item, true);

                return new { Status = 1, Message = "", Data = data, SubItemId = subItemId, SubscriptionTypeID = subscriptionTypeID };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = HttpUtility.HtmlEncode(e.Message) };
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SubscribeType(Guid itemId, Guid subItemId, Guid subscriptionTypeID)
        {
            try
            {
                var item = WebItemManager.Instance[itemId];

                ISubscriptionManager subscriptionManager = null;
                var productSubscriptionManager = item.Context.SubscriptionManager as IProductSubscriptionManager;

                if (productSubscriptionManager.GroupByType == GroupByType.Modules
                    || productSubscriptionManager.GroupByType == GroupByType.Simple)
                {
                    var subItem = WebItemManager.Instance[subItemId];
                    subscriptionManager = subItem.Context.SubscriptionManager;
                }
                else
                    subscriptionManager = productSubscriptionManager;

                var types = subscriptionManager.GetSubscriptionTypes();
                if (types != null)
                {
                    var type = types.Find(t => t.ID.Equals(subscriptionTypeID) && t.CanSubscribe);
                    if (type != null)
                        subscriptionManager.SubscriptionProvider.Subscribe(type.NotifyAction, null, GetCurrentRecipient());
                }

                var data = GetItemSubscriptions(item, true);
                return new { Status = 1, Message = "", Data = data, SubItemId = subItemId, SubscriptionTypeID = subscriptionTypeID };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = HttpUtility.HtmlEncode(e.Message) };
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object UnsubscribeProduct(Guid itemId)
        {
            try
            {
                var item = WebItemManager.Instance[itemId];
                var productSubscriptionManager = item.Context.SubscriptionManager as IProductSubscriptionManager;
                if (productSubscriptionManager.GroupByType == GroupByType.Modules)
                {
                    foreach (var subItem in WebItemManager.Instance.GetSubItems(item.ID))
                    {
                        if (subItem.Context != null && subItem.Context.SubscriptionManager != null)
                        {
                            foreach (var subscriptionType in subItem.Context.SubscriptionManager.GetSubscriptionTypes())
                            {
                                if (subscriptionType.CanSubscribe)
                                    subItem.Context.SubscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, null, GetCurrentRecipient());
                                else
                                    subItem.Context.SubscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, GetCurrentRecipient());
                            }
                        }
                    }
                }
                else if (productSubscriptionManager.GroupByType == GroupByType.Groups
                         || productSubscriptionManager.GroupByType == GroupByType.Simple)
                {
                    foreach (var subscriptionType in productSubscriptionManager.GetSubscriptionTypes())
                    {
                        if (subscriptionType.CanSubscribe)
                            productSubscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, null, GetCurrentRecipient());
                        else
                            productSubscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, GetCurrentRecipient());
                    }
                }

                var data = GetItemSubscriptions(item, true);
                return new { Status = 1, Message = "", Data = data };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = HttpUtility.HtmlEncode(e.Message) };
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SetNotifyByMethod(Guid productID, int notifyBy)
        {
            var resp = new AjaxResponse { rs2 = productID.ToString() };
            try
            {
                var notifyByList = ConvertToNotifyByList((NotifyBy)notifyBy);

                var productSubscriptionManager = WebItemManager.Instance[productID].Context.SubscriptionManager as IProductSubscriptionManager;
                if (productSubscriptionManager.GroupByType == GroupByType.Modules)
                {
                    foreach (var item in WebItemManager.Instance.GetSubItems(productID))
                    {
                        if (item.Context != null && item.Context.SubscriptionManager != null)
                            SetNotifyBySubsriptionTypes(notifyByList, item.Context.SubscriptionManager);
                    }
                }
                else if (productSubscriptionManager.GroupByType == GroupByType.Groups
                         || productSubscriptionManager.GroupByType == GroupByType.Simple)
                {
                    SetNotifyBySubsriptionTypes(notifyByList, productSubscriptionManager);
                }
                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs6 = "<div class='errorBox'>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
            }

            return null;
        }

        private static IList<string> ConvertToNotifyByList(NotifyBy notifyBy)
        {
            IList<string> notifyByList = new List<string>();

            NotifyByBindings.Keys.Where(n => notifyBy.HasFlag(n)).ToList().ForEach(n => notifyByList.Add(NotifyByBindings[n]));

            return notifyByList;
        }

        [Flags]
        public enum NotifyBy
        {
            None = 0,
            Email = 1,
            TMTalk = 2,
            Push = 4,
            Telegram = 8,
            SignalR = 16
        }

        protected static readonly Dictionary<NotifyBy, string> NotifyByBindings = new Dictionary<NotifyBy, string>()
        {
            { NotifyBy.Email, ASC.Core.Configuration.Constants.NotifyEMailSenderSysName },
            { NotifyBy.TMTalk, ASC.Core.Configuration.Constants.NotifyMessengerSenderSysName },
            { NotifyBy.Telegram, ASC.Core.Configuration.Constants.NotifyTelegramSenderSysName }
        };
        protected static readonly Dictionary<string, NotifyBy> NotifyByBindingsReverse = NotifyByBindings.ToDictionary(kv => kv.Value, kv => kv.Key);

        protected string GetNotifyLabel(NotifyBy notify)
        {
            try
            {
                return Resources.Resource.ResourceManager.GetString("NotifyBy" + notify.ToString());
            }
            catch
            {
                return "Unknown";
            }
        }

        private void SetNotifyBySubsriptionTypes(IList<string> notifyByList, ISubscriptionManager subscriptionManager)
        {
            var subscriptionTypes = subscriptionManager.GetSubscriptionTypes();
            if (subscriptionTypes != null)
            {
                foreach (var type in subscriptionTypes)
                    SetNotifyBySubsriptionTypes(notifyByList, subscriptionManager, type.NotifyAction);
            }
        }

        private void SetNotifyBySubsriptionTypes(IList<string> notifyByList, ISubscriptionManager subscriptionManager, INotifyAction action)
        {
            subscriptionManager
                .SubscriptionProvider
                .UpdateSubscriptionMethod(
                    action,
                    GetCurrentRecipient(),
                    notifyByList.ToArray());
        }

        private NotifyBy GetNotifyByMethod(Guid itemID)
        {
            var productSubscriptionManager = WebItemManager.Instance[itemID].Context.SubscriptionManager as IProductSubscriptionManager;
            if (productSubscriptionManager == null)
                return 0;

            if (productSubscriptionManager.GroupByType == GroupByType.Modules)
            {
                foreach (var item in WebItemManager.Instance.GetSubItems(itemID))
                {
                    if (item.Context == null || item.Context.SubscriptionManager == null)
                        continue;

                    foreach (var s in item.Context.SubscriptionManager.GetSubscriptionTypes())
                        return ConvertToNotifyByValue(item.Context.SubscriptionManager, s);
                }
            }
            else if (productSubscriptionManager.GroupByType == GroupByType.Groups
                     || productSubscriptionManager.GroupByType == GroupByType.Simple)
            {
                foreach (var s in productSubscriptionManager.GetSubscriptionTypes())
                    return ConvertToNotifyByValue(productSubscriptionManager, s);
            }
            return 0;
        }

        private NotifyBy ConvertToNotifyByValue(ISubscriptionManager subscriptionManager, SubscriptionType s)
        {
            return ConvertToNotifyByValue(subscriptionManager, s.NotifyAction);
        }

        private NotifyBy ConvertToNotifyByValue(ISubscriptionManager subscriptionManager, INotifyAction action)
        {
            var notifyByArray = subscriptionManager.SubscriptionProvider.GetSubscriptionMethod(action, GetCurrentRecipient()).ToList();

            var notify = NotifyBy.None;

            notifyByArray.ForEach(n => notify |= NotifyByBindingsReverse[n]);

            return notify;
        }
    }
}