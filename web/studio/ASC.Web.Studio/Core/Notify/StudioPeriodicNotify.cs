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
using System.Globalization;
using System.Linq;
using System.Threading;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.Core.Notify
{
    public class StudioPeriodicNotify
    {
        public static void SendSaasLetters(INotifyClient client, string senderName, DateTime scheduleDate)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var nowDate = scheduleDate.Date;
            const string dbid = "webstudio";

            log.Info("Start SendSaasTariffLetters");

            var activeTenants = CoreContext.TenantManager.GetTenants().ToList();

            if (activeTenants.Count <= 0)
            {
                log.Info("End SendSaasTariffLetters");
                return;
            }

            foreach (var tenant in activeTenants)
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);

                    var tariff = CoreContext.PaymentManager.GetTariff(tenant.TenantId);
                    var quota = CoreContext.TenantManager.GetTenantQuota(tenant.TenantId);

                    var createdDate = tenant.CreatedDateTime.Date;

                    var dueDateIsNotMax = tariff.DueDate != DateTime.MaxValue;
                    var dueDate = tariff.DueDate.Date;

                    var delayDueDateIsNotMax = tariff.DelayDueDate != DateTime.MaxValue;
                    var delayDueDate = tariff.DelayDueDate.Date;

                    INotifyAction action = null;
                    var paymentMessage = true;

                    var toadmins = false;
                    var tousers = false;
                    var toowner = false;

                    var coupon = string.Empty;

                    Func<string> greenButtonText = () => string.Empty;
                    Func<string> blueButtonText = () => WebstudioNotifyPatternResource.ButtonRequestCallButton;
                    var greenButtonUrl = string.Empty;

                    Func<string> tableItemText1 = () => string.Empty;
                    Func<string> tableItemText2 = () => string.Empty;
                    Func<string> tableItemText3 = () => string.Empty;
                    Func<string> tableItemText4 = () => string.Empty;
                    Func<string> tableItemText5 = () => string.Empty;
                    Func<string> tableItemText6 = () => string.Empty;
                    Func<string> tableItemText7 = () => string.Empty;

                    var tableItemUrl1 = string.Empty;
                    var tableItemUrl2 = string.Empty;
                    var tableItemUrl3 = string.Empty;
                    var tableItemUrl4 = string.Empty;
                    var tableItemUrl5 = string.Empty;
                    var tableItemUrl6 = string.Empty;
                    var tableItemUrl7 = string.Empty;

                    var tableItemImg1 = string.Empty;
                    var tableItemImg2 = string.Empty;
                    var tableItemImg3 = string.Empty;
                    var tableItemImg4 = string.Empty;
                    var tableItemImg5 = string.Empty;
                    var tableItemImg6 = string.Empty;
                    var tableItemImg7 = string.Empty;

                    Func<string> tableItemComment1 = () => string.Empty;
                    Func<string> tableItemComment2 = () => string.Empty;
                    Func<string> tableItemComment3 = () => string.Empty;
                    Func<string> tableItemComment4 = () => string.Empty;
                    Func<string> tableItemComment5 = () => string.Empty;
                    Func<string> tableItemComment6 = () => string.Empty;
                    Func<string> tableItemComment7 = () => string.Empty;

                    Func<string> tableItemLearnMoreText1 = () => string.Empty;
                    Func<string> tableItemLearnMoreText2 = () => string.Empty;
                    Func<string> tableItemLearnMoreText3 = () => string.Empty;
                    Func<string> tableItemLearnMoreText4 = () => string.Empty;
                    Func<string> tableItemLearnMoreText5 = () => string.Empty;
                    Func<string> tableItemLearnMoreText6 = () => string.Empty;
                    Func<string> tableItemLearnMoreText7 = () => string.Empty;

                    var tableItemLearnMoreUrl1 = string.Empty;
                    var tableItemLearnMoreUrl2 = string.Empty;
                    var tableItemLearnMoreUrl3 = string.Empty;
                    var tableItemLearnMoreUrl4 = string.Empty;
                    var tableItemLearnMoreUrl5 = string.Empty;
                    var tableItemLearnMoreUrl6 = string.Empty;
                    var tableItemLearnMoreUrl7 = string.Empty;


                    if (quota.Trial)
                    {
                        #region After registration letters

                        #region 3 days after registration to admins SAAS TRIAL + only 1 user

                        if (createdDate.AddDays(3) == nowDate && CoreContext.UserManager.GetUsers().Count() == 1)
                        {
                            action = Actions.SaasAdminInviteTeammatesV10;
                            paymentMessage = false;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonInviteRightNow;
                            greenButtonUrl = String.Format("{0}/Products/People/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 5 days after registration to admins SAAS TRAIL + without activity in 1 or more days

                        else if (createdDate.AddDays(5) == nowDate)
                        {
                            List<DateTime> datesWithActivity;

                            var query = new SqlQuery("feed_aggregate")
                                .Select(new SqlExp("cast(created_date as date) as short_date"))

                                .Where("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                                .Where(Exp.Le("created_date", nowDate.AddDays(-1)))
                                .GroupBy("short_date");

                            using (var db = new DbManager(dbid))
                            {
                                datesWithActivity = db
                                    .ExecuteList(query)
                                    .ConvertAll(r => Convert.ToDateTime(r[0]));
                            }

                            if (datesWithActivity.Count < 5)
                            {
                                action = Actions.SaasAdminWithoutActivityV10;
                                paymentMessage = false;
                                toadmins = true;
                            }
                        }

                        #endregion

                        #region 7 days after registration to admins and users SAAS TRIAL

                        else if (createdDate.AddDays(7) == nowDate)
                        {
                            action = Actions.SaasAdminUserDocsTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-formatting-100.png");
                            tableItemText1 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_formatting_hdr;
                            tableItemComment1 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_formatting;

                            if (!CoreContext.Configuration.CustomMode)
                            {
                                tableItemLearnMoreUrl1 = StudioNotifyHelper.Helplink + "/onlyoffice-editors/index.aspx";
                                tableItemLearnMoreText1 = () => WebstudioNotifyPatternResource.LinkLearnMore;
                            }

                            tableItemImg2 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-share-100.png");
                            tableItemText2 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_share_hdr;
                            tableItemComment2 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_share;

                            tableItemImg3 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-coediting-100.png");
                            tableItemText3 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_coediting_hdr;
                            tableItemComment3 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_coediting;

                            tableItemImg4 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-review-100.png");
                            tableItemText4 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_review_hdr;
                            tableItemComment4 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_review;

                            tableItemImg5 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-3rdparty-100.png");
                            tableItemText5 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_3rdparty_hdr;
                            tableItemComment5 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_3rdparty;

                            tableItemImg6 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-attach-100.png");
                            tableItemText6 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_attach_hdr;
                            tableItemComment6 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_attach;

                            tableItemImg7 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-apps-100.png");
                            tableItemText7 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_apps_hdr;
                            tableItemComment7 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_apps;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;
                            greenButtonUrl = String.Format("{0}/Products/Files/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 14 days after registration to admins and users SAAS TRIAL

                        else if (createdDate.AddDays(14) == nowDate)
                        {
                            action = Actions.SaasAdminUserComfortTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;
                        }

                        #endregion

                        #region 21 days after registration to admins and users SAAS TRIAL

                        else if (createdDate.AddDays(21) == nowDate)
                        {
                            action = Actions.SaasAdminUserAppsTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;
                        }

                        #endregion

                        #endregion

                        #region Trial warning letters

                        #region 5 days before SAAS TRIAL ends to admins

                        else if (dueDateIsNotMax && dueDate.AddDays(-5) == nowDate)
                        {
                            toadmins = true;
                            action = Actions.SaasAdminTrialWarningBefore5V10;
                            coupon = "PortalCreation10%";

                            if (string.IsNullOrEmpty(coupon))
                            {
                                try
                                {
                                    log.InfoFormat("start CreateCoupon to {0}", tenant.TenantAlias);

                                    coupon = SetupInfo.IsSecretEmail(CoreContext.UserManager.GetUsers(tenant.OwnerId).Email)
                                                ? tenant.TenantAlias
                                                : CouponManager.CreateCoupon();

                                    log.InfoFormat("end CreateCoupon to {0} coupon = {1}", tenant.TenantAlias, coupon);
                                }
                                catch (AggregateException ae)
                                {
                                    foreach (var ex in ae.InnerExceptions)
                                        log.Error(ex);
                                }
                                catch (Exception ex)
                                {
                                    log.Error(ex);
                                }
                            }

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonUseDiscount;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/Tariffs.aspx");
                        }

                        #endregion

                        #region SAAS TRIAL expires today to admins

                        else if (dueDate == nowDate)
                        {
                            action = Actions.SaasAdminTrialWarningV10;
                            toadmins = true;
                        }

                        #endregion

                        #region 5 days after SAAS TRIAL expired to admins

                        else if (dueDateIsNotMax && dueDate.AddDays(5) == nowDate && tenant.VersionChanged <= tenant.CreatedDateTime)
                        {
                            action = Actions.SaasAdminTrialWarningAfter5V10;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSendRequest;
                            greenButtonUrl = "mailto:sales@onlyoffice.com";
                        }

                        #endregion

                        #region 30 days after SAAS TRIAL expired + only 1 user

                        else if (dueDateIsNotMax && dueDate.AddDays(30) == nowDate && CoreContext.UserManager.GetUsers().Count() == 1)
                        {
                            action = Actions.SaasAdminTrialWarningAfter30V10;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonStartNow;
                            greenButtonUrl = "https://personal.onlyoffice.com";
                        }

                        #endregion

                        #region 6 months after SAAS TRIAL expired

                        else if (dueDateIsNotMax && dueDate.AddMonths(6) == nowDate)
                        {
                            action = Actions.SaasAdminTrialWarningAfterHalfYearV10;
                            toowner = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonLeaveFeedback;

                            var owner = CoreContext.UserManager.GetUsers(tenant.OwnerId);
                            greenButtonUrl = SetupInfo.TeamlabSiteRedirect + "/remove-portal-feedback-form.aspx#" +
                                          System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(
                                              System.Text.Encoding.UTF8.GetBytes("{\"firstname\":\"" + owner.FirstName +
                                                                                 "\",\"lastname\":\"" + owner.LastName +
                                                                                 "\",\"alias\":\"" + tenant.TenantAlias +
                                                                                 "\",\"email\":\"" + owner.Email + "\"}"))); 
                        }
                        else if (dueDateIsNotMax && dueDate.AddMonths(6).AddDays(7) <= nowDate)
                        {
                            CoreContext.TenantManager.RemoveTenant(tenant.TenantId, true);

                            if (!String.IsNullOrEmpty(ApiSystemHelper.ApiCacheUrl))
                            {
                                ApiSystemHelper.RemoveTenantFromCache(tenant.TenantAlias);
                            }
                        }

                        #endregion

                        #endregion
                    }
                    else if (tariff.State >= TariffState.Paid)
                    {
                        #region Payment warning letters

                        #region 5 days before SAAS PAID expired to admins

                        if (tariff.State == TariffState.Paid && dueDateIsNotMax && dueDate.AddDays(-5) == nowDate)
                        {
                            action = Actions.SaasAdminPaymentWarningBefore5V10;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonRenewNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/Tariffs.aspx");
                        }

                        #endregion

                        #region 1 day after SAAS PAID expired to admins

                        else if (tariff.State >= TariffState.Paid && dueDateIsNotMax && dueDate.AddDays(1) == nowDate)
                        {
                            action = Actions.SaasAdminPaymentWarningAfter1V10;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonRenewNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/Tariffs.aspx");
                        }

                        #endregion

                        #region  2 weeks before monthly SAAS PAID tariff expired to admins

                        else if (tariff.State == TariffState.Paid && !quota.Year && !quota.Year3 && dueDateIsNotMax && dueDate.AddDays(-14) == nowDate)
                        {
                            action = Actions.SaasAdminPaymentAfterMonthlySubscriptionsV10;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonBuyNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/Tariffs.aspx");
                        }

                        #endregion

                        #region 6 months after SAAS PAID expired

                        else if (tariff.State == TariffState.NotPaid && dueDateIsNotMax && dueDate.AddMonths(6) == nowDate)
                        {
                            action = Actions.SaasAdminTrialWarningAfterHalfYearV10;
                            toowner = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonLeaveFeedback;

                            var owner = CoreContext.UserManager.GetUsers(tenant.OwnerId);
                            greenButtonUrl = SetupInfo.TeamlabSiteRedirect + "/remove-portal-feedback-form.aspx#" +
                                          System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(
                                              System.Text.Encoding.UTF8.GetBytes("{\"firstname\":\"" + owner.FirstName +
                                                                                 "\",\"lastname\":\"" + owner.LastName +
                                                                                 "\",\"alias\":\"" + tenant.TenantAlias +
                                                                                 "\",\"email\":\"" + owner.Email + "\"}")));
                        }
                        else if (tariff.State == TariffState.NotPaid && dueDateIsNotMax && dueDate.AddMonths(6).AddDays(7) <= nowDate)
                        {
                            CoreContext.TenantManager.RemoveTenant(tenant.TenantId, true);

                            if (!String.IsNullOrEmpty(ApiSystemHelper.ApiCacheUrl))
                            {
                                ApiSystemHelper.RemoveTenantFromCache(tenant.TenantAlias);
                            }
                        }

                        #endregion

                        #endregion
                    }


                    if (action == null) continue;

                    var users = toowner
                                    ? new List<UserInfo> { CoreContext.UserManager.GetUsers(tenant.OwnerId) }
                                    : StudioNotifyHelper.GetRecipients(toadmins, tousers, false);


                    var analytics = StudioNotifyHelper.GetNotifyAnalytics(tenant.TenantId, action, toowner, toadmins, tousers, false);

                    foreach (var u in users.Where(u => paymentMessage || StudioNotifyHelper.IsSubscribedToNotify(u, Actions.PeriodicNotify)))
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;
                        var rquota = TenantExtra.GetRightQuota() ?? TenantQuota.Default;

                        client.SendNoticeToAsync(
                            action,
                            null,
                            new[] { StudioNotifyHelper.ToRecipient(u.ID) },
                            new[] { senderName },
                            null,
                            new TagValue(Tags.UserName, u.FirstName.HtmlEncode()),
                            new TagValue(Tags.PricingPage, CommonLinkUtility.GetFullAbsolutePath("~/Tariffs.aspx")),
                            new TagValue(Tags.ActiveUsers, CoreContext.UserManager.GetUsers().Count()),
                            new TagValue(Tags.Price, rquota.Price),
                            new TagValue(Tags.PricePeriod, rquota.Year3 ? UserControlsCommonResource.TariffPerYear3 : rquota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth),
                            new TagValue(Tags.DueDate, dueDate.ToLongDateString()),
                            new TagValue(Tags.DelayDueDate, (delayDueDateIsNotMax ? delayDueDate : dueDate).ToLongDateString()),
                            TagValues.BlueButton(blueButtonText, "http://www.onlyoffice.com/call-back-form.aspx"),
                            TagValues.GreenButton(greenButtonText, greenButtonUrl),
                            TagValues.TableTop(),
                            TagValues.TableItem(1, tableItemText1, tableItemUrl1, tableItemImg1, tableItemComment1, tableItemLearnMoreText1, tableItemLearnMoreUrl1),
                            TagValues.TableItem(2, tableItemText2, tableItemUrl2, tableItemImg2, tableItemComment2, tableItemLearnMoreText2, tableItemLearnMoreUrl2),
                            TagValues.TableItem(3, tableItemText3, tableItemUrl3, tableItemImg3, tableItemComment3, tableItemLearnMoreText3, tableItemLearnMoreUrl3),
                            TagValues.TableItem(4, tableItemText4, tableItemUrl4, tableItemImg4, tableItemComment4, tableItemLearnMoreText4, tableItemLearnMoreUrl4),
                            TagValues.TableItem(5, tableItemText5, tableItemUrl5, tableItemImg5, tableItemComment5, tableItemLearnMoreText5, tableItemLearnMoreUrl5),
                            TagValues.TableItem(6, tableItemText6, tableItemUrl6, tableItemImg6, tableItemComment6, tableItemLearnMoreText6, tableItemLearnMoreUrl6),
                            TagValues.TableItem(7, tableItemText7, tableItemUrl7, tableItemImg7, tableItemComment7, tableItemLearnMoreText7, tableItemLearnMoreUrl7),
                            TagValues.TableBottom(),
                            new TagValue(CommonTags.Footer, u.IsAdmin() ? "common" : "social"),
                            new TagValue(CommonTags.Analytics, analytics),
                            new TagValue(Tags.Coupon, coupon));
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendSaasTariffLetters");
        }

        public static void SendEnterpriseLetters(INotifyClient client, string senderName, DateTime scheduleDate)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var nowDate = scheduleDate.Date;
            const string dbid = "webstudio";

            log.Info("Start SendTariffEnterpriseLetters");

            var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;

            var activeTenants = CoreContext.TenantManager.GetTenants();

            if (activeTenants.Count <= 0)
            {
                log.Info("End SendTariffEnterpriseLetters");
                return;
            }

            foreach (var tenant in activeTenants)
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);

                    var tariff = CoreContext.PaymentManager.GetTariff(tenant.TenantId);
                    var quota = CoreContext.TenantManager.GetTenantQuota(tenant.TenantId);

                    var createdDate = tenant.CreatedDateTime.Date;

                    var actualEndDate = (tariff.DueDate != DateTime.MaxValue ? tariff.DueDate : tariff.LicenseDate);
                    var dueDateIsNotMax = actualEndDate != DateTime.MaxValue;
                    var dueDate = actualEndDate.Date;

                    var delayDueDateIsNotMax = tariff.DelayDueDate != DateTime.MaxValue;
                    var delayDueDate = tariff.DelayDueDate.Date;

                    INotifyAction action = null;
                    var paymentMessage = true;

                    var toadmins = false;
                    var tousers = false;

                    Func<string> greenButtonText = () => string.Empty;
                    Func<string> blueButtonText = () => WebstudioNotifyPatternResource.ButtonRequestCallButton;
                    var greenButtonUrl = string.Empty;

                    Func<string> tableItemText1 = () => string.Empty;
                    Func<string> tableItemText2 = () => string.Empty;
                    Func<string> tableItemText3 = () => string.Empty;
                    Func<string> tableItemText4 = () => string.Empty;
                    Func<string> tableItemText5 = () => string.Empty;
                    Func<string> tableItemText6 = () => string.Empty;
                    Func<string> tableItemText7 = () => string.Empty;

                    var tableItemUrl1 = string.Empty;
                    var tableItemUrl2 = string.Empty;
                    var tableItemUrl3 = string.Empty;
                    var tableItemUrl4 = string.Empty;
                    var tableItemUrl5 = string.Empty;
                    var tableItemUrl6 = string.Empty;
                    var tableItemUrl7 = string.Empty;

                    var tableItemImg1 = string.Empty;
                    var tableItemImg2 = string.Empty;
                    var tableItemImg3 = string.Empty;
                    var tableItemImg4 = string.Empty;
                    var tableItemImg5 = string.Empty;
                    var tableItemImg6 = string.Empty;
                    var tableItemImg7 = string.Empty;

                    Func<string> tableItemComment1 = () => string.Empty;
                    Func<string> tableItemComment2 = () => string.Empty;
                    Func<string> tableItemComment3 = () => string.Empty;
                    Func<string> tableItemComment4 = () => string.Empty;
                    Func<string> tableItemComment5 = () => string.Empty;
                    Func<string> tableItemComment6 = () => string.Empty;
                    Func<string> tableItemComment7 = () => string.Empty;

                    Func<string> tableItemLearnMoreText1 = () => string.Empty;
                    Func<string> tableItemLearnMoreText2 = () => string.Empty;
                    Func<string> tableItemLearnMoreText3 = () => string.Empty;
                    Func<string> tableItemLearnMoreText4 = () => string.Empty;
                    Func<string> tableItemLearnMoreText5 = () => string.Empty;
                    Func<string> tableItemLearnMoreText6 = () => string.Empty;
                    Func<string> tableItemLearnMoreText7 = () => string.Empty;

                    var tableItemLearnMoreUrl1 = string.Empty;
                    var tableItemLearnMoreUrl2 = string.Empty;
                    var tableItemLearnMoreUrl3 = string.Empty;
                    var tableItemLearnMoreUrl4 = string.Empty;
                    var tableItemLearnMoreUrl5 = string.Empty;
                    var tableItemLearnMoreUrl6 = string.Empty;
                    var tableItemLearnMoreUrl7 = string.Empty;


                    if (quota.Trial && defaultRebranding)
                    {
                        #region After registration letters

                        #region 1 day after registration to admins ENTERPRISE TRIAL + defaultRebranding

                        if (createdDate.AddDays(1) == nowDate)
                        {
                            action = Actions.EnterpriseAdminCustomizePortalV10;
                            paymentMessage = false;
                            toadmins = true;

                            tableItemImg1 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-brand-100.png");
                            tableItemText1 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_brand_hdr;
                            tableItemComment1 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_brand;

                            tableItemImg2 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-regional-100.png");
                            tableItemText2 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_regional_hdr;
                            tableItemComment2 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_regional;

                            tableItemImg3 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-customize-100.png");
                            tableItemText3 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_customize_hdr;
                            tableItemComment3 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_customize;

                            tableItemImg4 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-modules-100.png");
                            tableItemText4 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_modules_hdr;
                            tableItemComment4 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_modules;

                            tableItemImg5 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-3rdparty-100.png");
                            tableItemText5 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_3rdparty_hdr;
                            tableItemComment5 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_3rdparty;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonConfigureRightNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetAdministration(ManagementType.General));
                        }

                        #endregion

                        #region 4 days after registration to admins ENTERPRISE TRIAL + only 1 user + defaultRebranding

                        else if (createdDate.AddDays(4) == nowDate && CoreContext.UserManager.GetUsers().Count() == 1)
                        {
                            action = Actions.EnterpriseAdminInviteTeammatesV10;
                            paymentMessage = false;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonInviteRightNow;
                            greenButtonUrl = String.Format("{0}/Products/People/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 5 days after registration to admins ENTERPRISE TRAIL + without activity in 1 or more days + defaultRebranding

                        else if (createdDate.AddDays(5) == nowDate)
                        {
                            List<DateTime> datesWithActivity;

                            var query = new SqlQuery("feed_aggregate")
                                .Select(new SqlExp("cast(created_date as date) as short_date"))
                                .Where("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                                .Where(Exp.Le("created_date", nowDate.AddDays(-1)))
                                .GroupBy("short_date");

                            using (var db = new DbManager(dbid))
                            {
                                datesWithActivity = db
                                    .ExecuteList(query)
                                    .ConvertAll(r => Convert.ToDateTime(r[0]));
                            }

                            if (datesWithActivity.Count < 5)
                            {
                                action = Actions.EnterpriseAdminWithoutActivityV10;
                                paymentMessage = false;
                                toadmins = true;
                            }
                        }

                        #endregion

                        #region 7 days after registration to admins and users ENTERPRISE TRIAL + defaultRebranding

                        else if (createdDate.AddDays(7) == nowDate)
                        {
                            action = Actions.EnterpriseAdminUserDocsTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-formatting-100.png");
                            tableItemText1 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_formatting_hdr;
                            tableItemComment1 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_formatting;

                            if (!CoreContext.Configuration.CustomMode)
                            {
                                tableItemLearnMoreUrl1 = StudioNotifyHelper.Helplink + "/onlyoffice-editors/index.aspx";
                                tableItemLearnMoreText1 = () => WebstudioNotifyPatternResource.LinkLearnMore;
                            }

                            tableItemImg2 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-share-100.png");
                            tableItemText2 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_share_hdr;
                            tableItemComment2 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_share;

                            tableItemImg3 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-coediting-100.png");
                            tableItemText3 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_coediting_hdr;
                            tableItemComment3 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_coediting;

                            tableItemImg4 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-review-100.png");
                            tableItemText4 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_review_hdr;
                            tableItemComment4 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_review;

                            tableItemImg5 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-3rdparty-100.png");
                            tableItemText5 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_3rdparty_hdr;
                            tableItemComment5 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_3rdparty;

                            tableItemImg6 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-attach-100.png");
                            tableItemText6 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_attach_hdr;
                            tableItemComment6 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_attach;

                            tableItemImg7 = StudioNotifyHelper.GetNotificationImageUrl("tips-documents-apps-100.png");
                            tableItemText7 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_apps_hdr;
                            tableItemComment7 = () => WebstudioNotifyPatternResource.pattern_saas_admin_user_docs_tips_v10_item_apps;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;
                            greenButtonUrl = String.Format("{0}/Products/Files/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 21 days after registration to admins and users ENTERPRISE TRIAL + defaultRebranding

                        else if (createdDate.AddDays(21) == nowDate)
                        {
                            action = Actions.EnterpriseAdminUserAppsTipsV10;
                            paymentMessage = false;
                            toadmins = true;
                            tousers = true;
                        }

                        #endregion

                        #endregion

                        #region Trial warning letters

                        #region 7 days before ENTERPRISE TRIAL ends to admins + defaultRebranding

                        else if (dueDateIsNotMax && dueDate.AddDays(-7) == nowDate)
                        {
                            action = Actions.EnterpriseAdminTrialWarningBefore7V10;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = "http://www.onlyoffice.com/enterprise-edition.aspx";
                        }

                        #endregion

                        #region ENTERPRISE TRIAL expires today to admins + defaultRebranding

                        else if (dueDate == nowDate)
                        {
                            action = Actions.EnterpriseAdminTrialWarningV10;
                            toadmins = true;
                        }

                        #endregion

                        #endregion
                    }
                    else if (quota.Trial && !defaultRebranding)
                    {
                        #region After registration letters

                        #region 1 day after registration to admins ENTERPRISE TRIAL + !defaultRebranding

                        if (createdDate.AddDays(1) == nowDate)
                        {
                            action = Actions.EnterpriseWhitelabelAdminCustomizePortalV10;
                            paymentMessage = false;
                            toadmins = true;

                            tableItemImg1 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-brand-100.png");
                            tableItemText1 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_brand_hdr;
                            tableItemComment1 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_brand;

                            tableItemImg2 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-regional-100.png");
                            tableItemText2 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_regional_hdr;
                            tableItemComment2 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_regional;

                            tableItemImg3 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-customize-100.png");
                            tableItemText3 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_customize_hdr;
                            tableItemComment3 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_customize;

                            tableItemImg4 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-modules-100.png");
                            tableItemText4 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_modules_hdr;
                            tableItemComment4 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_modules;

                            if (!CoreContext.Configuration.CustomMode)
                            {
                                tableItemImg5 = StudioNotifyHelper.GetNotificationImageUrl("tips-customize-3rdparty-100.png");
                                tableItemText5 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_3rdparty_hdr;
                                tableItemComment5 = () => WebstudioNotifyPatternResource.pattern_enterprise_admin_customize_portal_v10_item_3rdparty;
                            }

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonConfigureRightNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetAdministration(ManagementType.General));
                        }

                        #endregion

                        #endregion
                    }
                    else if (tariff.State == TariffState.Paid)
                    {
                        #region Payment warning letters

                        #region 7 days before ENTERPRISE PAID expired to admins

                        if (dueDateIsNotMax && dueDate.AddDays(-7) == nowDate)
                        {
                            action = defaultRebranding
                                         ? Actions.EnterpriseAdminPaymentWarningBefore7V10
                                         : Actions.EnterpriseWhitelabelAdminPaymentWarningBefore7V10;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/Tariffs.aspx");
                        }

                        #endregion

                        #region ENTERPRISE PAID expires today to admins

                        else if (dueDate == nowDate)
                        {
                            action = defaultRebranding
                                         ? Actions.EnterpriseAdminPaymentWarningV10
                                         : Actions.EnterpriseWhitelabelAdminPaymentWarningV10;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/Tariffs.aspx");
                        }

                        #endregion

                        #endregion
                    }


                    if (action == null) continue;

                    var users = StudioNotifyHelper.GetRecipients(toadmins, tousers, false);

                    foreach (var u in users.Where(u => paymentMessage || StudioNotifyHelper.IsSubscribedToNotify(u, Actions.PeriodicNotify)))
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        var rquota = TenantExtra.GetRightQuota() ?? TenantQuota.Default;

                        client.SendNoticeToAsync(
                            action,
                            null,
                            new[] { StudioNotifyHelper.ToRecipient(u.ID) },
                            new[] { senderName },
                            null,
                            new TagValue(Tags.UserName, u.FirstName.HtmlEncode()),
                            new TagValue(Tags.PricingPage, CommonLinkUtility.GetFullAbsolutePath("~/Tariffs.aspx")),
                            new TagValue(Tags.ActiveUsers, CoreContext.UserManager.GetUsers().Count()),
                            new TagValue(Tags.Price, rquota.Price),
                            new TagValue(Tags.PricePeriod, rquota.Year3 ? UserControlsCommonResource.TariffPerYear3 : rquota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth),
                            new TagValue(Tags.DueDate, dueDate.ToLongDateString()),
                            new TagValue(Tags.DelayDueDate, (delayDueDateIsNotMax ? delayDueDate : dueDate).ToLongDateString()),
                            TagValues.BlueButton(blueButtonText, "http://www.onlyoffice.com/call-back-form.aspx"),
                            TagValues.GreenButton(greenButtonText, greenButtonUrl),
                            TagValues.TableTop(),
                            TagValues.TableItem(1, tableItemText1, tableItemUrl1, tableItemImg1, tableItemComment1, tableItemLearnMoreText1, tableItemLearnMoreUrl1),
                            TagValues.TableItem(2, tableItemText2, tableItemUrl2, tableItemImg2, tableItemComment2, tableItemLearnMoreText2, tableItemLearnMoreUrl2),
                            TagValues.TableItem(3, tableItemText3, tableItemUrl3, tableItemImg3, tableItemComment3, tableItemLearnMoreText3, tableItemLearnMoreUrl3),
                            TagValues.TableItem(4, tableItemText4, tableItemUrl4, tableItemImg4, tableItemComment4, tableItemLearnMoreText4, tableItemLearnMoreUrl4),
                            TagValues.TableItem(5, tableItemText5, tableItemUrl5, tableItemImg5, tableItemComment5, tableItemLearnMoreText5, tableItemLearnMoreUrl5),
                            TagValues.TableItem(6, tableItemText6, tableItemUrl6, tableItemImg6, tableItemComment6, tableItemLearnMoreText6, tableItemLearnMoreUrl6),
                            TagValues.TableItem(7, tableItemText7, tableItemUrl7, tableItemImg7, tableItemComment7, tableItemLearnMoreText7, tableItemLearnMoreUrl7),
                            TagValues.TableBottom());
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendTariffEnterpriseLetters");
        }

        public static void SendOpensourceLetters(INotifyClient client, string senderName, DateTime scheduleDate)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var nowDate = scheduleDate.Date;

            log.Info("Start SendOpensourceTariffLetters");

            var activeTenants = CoreContext.TenantManager.GetTenants();

            if (activeTenants.Count <= 0)
            {
                log.Info("End SendOpensourceTariffLetters");
                return;
            }

            foreach (var tenant in activeTenants)
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);

                    var createdDate = tenant.CreatedDateTime.Date;

                    #region After registration letters

                    #region 5 days after registration to admins and users

                    if (createdDate.AddDays(5) == nowDate)
                    {
                        var users = StudioNotifyHelper.GetRecipients(true, true, false);

                        foreach (var u in users.Where(u => StudioNotifyHelper.IsSubscribedToNotify(u, Actions.PeriodicNotify)))
                        {
                            var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                            Thread.CurrentThread.CurrentCulture = culture;
                            Thread.CurrentThread.CurrentUICulture = culture;

                            client.SendNoticeToAsync(
                                u.IsAdmin() ? Actions.OpensourceAdminDocsTipsV11 : Actions.OpensourceUserDocsTipsV11,
                                null,
                                new[] { StudioNotifyHelper.ToRecipient(u.ID) },
                                new[] { senderName },
                                null,
                                new TagValue(Tags.UserName, u.DisplayUserName()),
                                new TagValue(CommonTags.Footer, "opensource"));
                        }
                    }

                    #endregion

                    #endregion
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendOpensourceTariffLetters");
        }

        public static void SendPersonalLetters(INotifyClient client, string senderName, DateTime scheduleDate)
        {
            var log = LogManager.GetLogger("ASC.Notify");

            log.Info("Start SendLettersPersonal...");

            foreach (var tenant in CoreContext.TenantManager.GetTenants())
            {
                try
                {
                    Func<string> greenButtonText = () => string.Empty;
                    var greenButtonUrl = string.Empty;

                    int sendCount = 0;

                    CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);

                    log.InfoFormat("Current tenant: {0}", tenant.TenantId);

                    var users = CoreContext.UserManager.GetUsers(EmployeeStatus.Active);

                    foreach (var user in users.Where(u => StudioNotifyHelper.IsSubscribedToNotify(u, Actions.PeriodicNotify)))
                    {
                        INotifyAction action;

                        SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(user.ID));

                        var culture = tenant.GetCulture();
                        if (!string.IsNullOrEmpty(user.CultureName))
                        {
                            try
                            {
                                culture = user.GetCulture();
                            }
                            catch (CultureNotFoundException exception)
                            {

                                log.Error(exception);
                            }
                        }

                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        var dayAfterRegister = (int)scheduleDate.Date.Subtract(user.CreateDate.Date).TotalDays;

                        if (CoreContext.Configuration.CustomMode)
                        {
                            switch (dayAfterRegister)
                            {
                                case 7:
                                    action = Actions.PersonalCustomModeAfterRegistration7;
                                    break;
                                default:
                                    continue;
                            }
                        }
                        else
                        {

                            switch (dayAfterRegister)
                            {
                                case 7:
                                    action = Actions.PersonalAfterRegistration7;
                                    break;
                                case 14:
                                    action = Actions.PersonalAfterRegistration14;
                                    break;
                                case 21:
                                    action = Actions.PersonalAfterRegistration21;
                                    break;
                                case 28:
                                    action = Actions.PersonalAfterRegistration28;
                                    greenButtonText = () => WebstudioNotifyPatternResource.ButtonStartFreeTrial;
                                    greenButtonUrl = "https://www.onlyoffice.com/enterprise-edition-free.aspx";
                                    break;
                                default:
                                    continue;
                            }
                        }

                        if (action == null) continue;

                        log.InfoFormat(@"Send letter personal '{1}' to {0} culture {2}. tenant id: {3} user culture {4} create on {5} now date {6}",
                              user.Email, action.ID, culture, tenant.TenantId, user.GetCulture(), user.CreateDate, scheduleDate.Date);

                        sendCount++;

                        client.SendNoticeToAsync(
                          action,
                          null,
                          StudioNotifyHelper.RecipientFromEmail(user.Email, true),
                          new[] { senderName },
                          null,
                          TagValues.PersonalHeaderStart(),
                          TagValues.PersonalHeaderEnd(),
                          TagValues.GreenButton(greenButtonText, greenButtonUrl),
                          new TagValue(CommonTags.Footer, CoreContext.Configuration.CustomMode ? "personalCustomMode" : "personal"));
                    }

                    log.InfoFormat("Total send count: {0}", sendCount);
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendLettersPersonal.");
        }

        public static bool ChangeSubscription(Guid userId)
        {
            var recipient = StudioNotifyHelper.ToRecipient(userId);

            var isSubscribe = StudioNotifyHelper.IsSubscribedToNotify(recipient, Actions.PeriodicNotify);

            StudioNotifyHelper.SubscribeToNotify(recipient, Actions.PeriodicNotify, !isSubscribe);

            return !isSubscribe;
        }
    }
}