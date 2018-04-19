/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Security.Cryptography;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Users;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;

namespace ASC.Web.Studio.Core.Notify
{
    public class StudioNotifyService
    {
        private readonly INotifyClient client;
        internal readonly StudioNotifySource source;

        private static string EMailSenderName { get { return ASC.Core.Configuration.Constants.NotifyEMailSenderSysName; } }

        public static StudioNotifyService Instance
        {
            get;
            private set;
        }


        static StudioNotifyService()
        {
            Instance = new StudioNotifyService();
        }


        private StudioNotifyService()
        {
            source = new StudioNotifySource();
            client = WorkContext.NotifyContext.NotifyService.RegisterClient(source);
        }

        public void RegisterSendMethod()
        {
            var cron = WebConfigurationManager.AppSettings["core.notify.cron"] ?? "0 0 5 ? * *";

            if (WebConfigurationManager.AppSettings["core.notify.tariff"] != "false")
            {
                if (TenantExtra.Enterprise)
                {
                    client.RegisterSendMethod(SendEnterpriseTariffLetters, cron);  // 5am every day
                }
                else if (TenantExtra.Hosted)
                {
                    client.RegisterSendMethod(SendHostedTariffLetters, cron);
                }
                else if (TenantExtra.Opensource)
                {
                    client.RegisterSendMethod(SendOpensourceTariffLetters, cron);
                }
                else
                {
                    client.RegisterSendMethod(SendSaasTariffLetters, cron);
                }
            }

            if (CoreContext.Configuration.Personal)
            {
                client.RegisterSendMethod(SendLettersPersonal, cron);
            }
            else
            {
                client.RegisterSendMethod(SendMsgWhatsNew, "0 0 * ? * *"); // every hour
            }
        }



        public void SendMsgWhatsNew(DateTime scheduleDate)
        {
            StudioWhatsNewService.Instance.SendMsgWhatsNew(scheduleDate, client);
        }

        public bool IsSubscribeToAdminNotify(Guid userID)
        {
            return source.GetSubscriptionProvider().IsSubscribed(Constants.ActionAdminNotify, ToRecipient(userID), null);
        }

        public void SubscribeToAdminNotify(Guid userID, bool subscribe)
        {
            var recipient = source.GetRecipientsProvider().GetRecipient(userID.ToString());
            if (recipient != null)
            {
                if (subscribe)
                {
                    source.GetSubscriptionProvider().Subscribe(Constants.ActionAdminNotify, null, recipient);
                }
                else
                {
                    source.GetSubscriptionProvider().UnSubscribe(Constants.ActionAdminNotify, null, recipient);
                }
            }
        }


        public void SendMsgToAdminAboutProfileUpdated()
        {
            client.SendNoticeAsync(Constants.ActionSelfProfileUpdated, null, null);
        }

        public void SendMsgToAdminFromNotAuthUser(string email, string message)
        {
            client.SendNoticeAsync(Constants.ActionUserMessageToAdmin, null, null,
                new TagValue(Constants.TagBody, message), new TagValue(Constants.TagUserEmail, email));
        }

        public void SendRequestTariff(bool license, string fname, string lname, string title, string email, string phone, string ctitle, string csize, string site, string message)
        {
            fname = (fname ?? "").Trim();
            if (string.IsNullOrEmpty(fname)) throw new ArgumentNullException("fname");
            lname = (lname ?? "").Trim();
            if (string.IsNullOrEmpty(lname)) throw new ArgumentNullException("lname");
            title = (title ?? "").Trim();
            email = (email ?? "").Trim();
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException("email");
            phone = (phone ?? "").Trim();
            if (string.IsNullOrEmpty(phone)) throw new ArgumentNullException("phone");
            ctitle = (ctitle ?? "").Trim();
            if (string.IsNullOrEmpty(ctitle)) throw new ArgumentNullException("ctitle");
            csize = (csize ?? "").Trim();
            if (string.IsNullOrEmpty(csize)) throw new ArgumentNullException("csize");
            site = (site ?? "").Trim();
            if (string.IsNullOrEmpty(site)) throw new ArgumentNullException("site");
            message = (message ?? "").Trim();

            var salesEmail = AdditionalWhiteLabelSettings.Instance.SalesEmail ?? SetupInfo.SalesEmail;

            var recipient = (IRecipient)(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), String.Empty, new[] { salesEmail }, false));

            client.SendNoticeToAsync(license ? Constants.ActionRequestLicense : Constants.ActionRequestTariff,
                                     null,
                                     new[] { recipient },
                                     new[] { "email.sender" },
                                     null,
                                     new TagValue(Constants.TagUserName, fname),
                                     new TagValue(Constants.TagUserLastName, lname),
                                     new TagValue(Constants.TagUserPosition, title),
                                     new TagValue(Constants.TagUserEmail, email),
                                     new TagValue(Constants.TagPhone, phone),
                                     new TagValue(Constants.TagWebsite, site),
                                     new TagValue(Constants.TagCompanyTitle, ctitle),
                                     new TagValue(Constants.TagCompanySize, csize),
                                     new TagValue(Constants.TagBody, message));
        }

        #region Voip

        public void SendToAdminVoipWarning(double balance)
        {
            client.SendNoticeAsync(Constants.ActionVoipWarning, null, null,
                new TagValue(Constants.TagBody, balance));
        }

        public void SendToAdminVoipBlocked()
        {
            client.SendNoticeAsync(Constants.ActionVoipBlocked, null, null);
        }

        #endregion

        #region User Password

        public void UserPasswordChange(UserInfo userInfo)
        {
            var hash = Hasher.Base64Hash(CoreContext.Authentication.GetUserPasswordHash(userInfo.ID));
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(userInfo.Email, ConfirmType.PasswordChange, hash);

            client.SendNoticeToAsync(
                CoreContext.Configuration.Personal ? Constants.ActionPersonalPasswordChange : Constants.ActionPasswordChange,
                        null,
                        RecipientFromEmail(new[] { userInfo.Email }, false),
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagUserName, SecurityContext.IsAuthenticated ? DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID) : ((HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)),
                        new TagValue(Constants.TagInviteLink, confirmationUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                        Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonChangePassword, confirmationUrl),
                        new TagValue(Constants.TagBody, string.Empty),
                        new TagValue(Constants.TagUserDisplayName, userInfo.DisplayUserName()),
                        new TagValue(CommonTags.Footer, CoreContext.Configuration.Personal ? "personal" : ""),
                        new TagValue(CommonTags.IsPersonal, CoreContext.Configuration.Personal ? "true" : "false"),
                        Constants.UnsubscribeLink);
        }

        #endregion

        #region User Email

        public void SendEmailChangeInstructions(UserInfo user, string email)
        {
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmailChange, SecurityContext.CurrentAccount.ID);

            client.SendNoticeToAsync(
                CoreContext.Configuration.Personal ? Constants.ActionPersonalEmailChange : Constants.ActionEmailChange,
                        null,
                        RecipientFromEmail(new[] { email }, false),
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagUserName, SecurityContext.IsAuthenticated ? DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID) : ((HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)),
                        new TagValue(Constants.TagInviteLink, confirmationUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                        Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonChangeEmail, confirmationUrl),
                        new TagValue(Constants.TagBody, string.Empty),
                        new TagValue(Constants.TagUserDisplayName, string.Empty),
                        new TagValue(CommonTags.Footer, CoreContext.Configuration.Personal ? "personal" : ""),
                        new TagValue(CommonTags.IsPersonal, CoreContext.Configuration.Personal ? "true" : "false"),
                        new TagValue(CommonTags.Culture, user.GetCulture().Name),
                        Constants.UnsubscribeLink);
        }

        public void SendEmailActivationInstructions(UserInfo user, string email)
        {
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmailActivation);

            client.SendNoticeToAsync(
                        Constants.ActionActivateEmail,
                        null,
                        RecipientFromEmail(new[] { email }, false),
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagUserName, SecurityContext.IsAuthenticated ? DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID) : ((HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)),
                        new TagValue(Constants.TagInviteLink, confirmationUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                        Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonActivateEmail, confirmationUrl),
                        new TagValue(Constants.TagBody, string.Empty),
                        new TagValue(Constants.TagUserDisplayName, (user.DisplayUserName() ?? string.Empty).Trim()),
                        new TagValue(CommonTags.Footer, CoreContext.Configuration.Personal ? "personal" : "common"),
                        new TagValue(CommonTags.IsPersonal, CoreContext.Configuration.Personal ? "true" : "false"),
                        Constants.UnsubscribeLink);
        }

        #endregion

        public void SendMsgMobilePhoneChange(UserInfo userInfo)
        {
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(userInfo.Email.ToLower(), ConfirmType.PhoneActivation);

            client.SendNoticeToAsync(
                Constants.ActionPhoneChange,
                null,
                RecipientFromEmail(new[] { userInfo.Email.ToLower() }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, confirmationUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonChangePhone, confirmationUrl),
                new TagValue(Constants.TagUserDisplayName, userInfo.DisplayUserName()));
        }


        public void UserHasJoin()
        {
            client.SendNoticeAsync(Constants.ActionUserHasJoin, null, null);
        }

        public void InviteUsers(string emailList, string inviteMessage, bool join, EmployeeType emplType)
        {
            if (string.IsNullOrWhiteSpace(emailList))
            {
                return;
            }

            foreach (var email in emailList.Split(new[] { " ", ",", ";", Environment.NewLine, "\n", "\n\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                SendInvite(new UserInfo() { Email = email }, inviteMessage, join, emplType);
            }
        }

        private void SendInvite(UserInfo user, string inviteMessage, bool join, EmployeeType emplType)
        {
            var inviteUrl = CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.EmpInvite, (int)emplType, SecurityContext.CurrentAccount.ID)
                            + String.Format("&firstname={0}&lastname={1}&emplType={2}",
                                            HttpUtility.UrlEncode(user.FirstName),
                                            HttpUtility.UrlEncode(user.LastName),
                                            (int)emplType);

            client.SendNoticeToAsync(
                        join ? Constants.ActionJoinUsers : Constants.ActionInviteUsers,
                        null,
                        RecipientFromEmail(new string[] { user.Email }, join),/*if it's invite - don't check activation status*/
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagUserName, SecurityContext.IsAuthenticated ? DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID) : ((HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)),
                        new TagValue(Constants.TagInviteLink, inviteUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                        Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonJoin, inviteUrl),
                        new TagValue(Constants.TagBody, inviteMessage ?? string.Empty),
                        new TagValue(CommonTags.Footer, "common"),
                        new TagValue(Constants.TagUserDisplayName, (user.DisplayUserName() ?? "").Trim()),
                        CreateSendFromTag());
        }

        public void UserInfoAddedAfterInvite(UserInfo newUserInfo)
        {
            if (!CoreContext.UserManager.UserExists(newUserInfo.ID)) return;

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var tariff = CoreContext.TenantManager.GetTenantQuota(tenant.TenantId);
            var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;

            INotifyAction notifyAction;
            var footer = "common";

            if (CoreContext.Configuration.Personal)
            {
                notifyAction = Constants.ActionPersonalAfterRegistration1;
                footer = "personal";
            }
            else if (TenantExtra.Enterprise)
            {
                notifyAction = defaultRebranding ? Constants.ActionEnterpriseUserWellcome : Constants.ActionEnterpriseWhitelabelUserWellcome;
            }
            else if (TenantExtra.Hosted)
            {
                notifyAction = defaultRebranding ? Constants.ActionHostedUserWellcome : Constants.ActionHostedWhitelabelUserWellcome;
            }
            else
            {
                if (tariff != null && tariff.Free)
                {
                    notifyAction = Constants.ActionFreeCloudUserWellcome;
                    footer = "freecloud";
                }
                else
                {
                    notifyAction = Constants.ActionSaasUserWellcome;
                }
            }

            var greenButtonText = TenantExtra.Enterprise
                                      ? WebstudioNotifyPatternResource.ButtonAccessYourPortal
                                      : WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;

            client.SendNoticeToAsync(
                notifyAction,
                null,
                RecipientFromEmail(new[] { newUserInfo.Email }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                new TagValue(Constants.TagUserEmail, newUserInfo.Email),
                new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                Constants.TagGreenButton(greenButtonText, CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/')),
                new TagValue(CommonTags.Footer, footer),
                new TagValue(CommonTags.IsPersonal, CoreContext.Configuration.Personal ? "true" : "false"),
                new TagValue(CommonTags.MasterTemplate, CoreContext.Configuration.Personal ? "HtmlMasterPersonal" : "HtmlMaster"),
                Constants.UnsubscribeLink);
        }

        public void GuestInfoAddedAfterInvite(UserInfo newUserInfo)
        {
            if (!CoreContext.UserManager.UserExists(newUserInfo.ID)) return;

            var tariff = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;

            INotifyAction notifyAction;

            if (TenantExtra.Enterprise)
            {
                notifyAction = defaultRebranding ? Constants.ActionEnterpriseGuestWellcome : Constants.ActionEnterpriseWhitelabelGuestWellcome;
            }
            else if (TenantExtra.Hosted)
            {
                notifyAction = defaultRebranding ? Constants.ActionHostedGuestWellcome : Constants.ActionHostedWhitelabelGuestWellcome;
            }
            else
            {
                notifyAction = tariff != null && tariff.Free ? Constants.ActionFreeCloudGuestWellcome : Constants.ActionSaasGuestWellcome;
            }

            var greenButtonText = TenantExtra.Enterprise
                                      ? WebstudioNotifyPatternResource.ButtonAccessYourPortal
                                      : WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;

            client.SendNoticeToAsync(
                notifyAction,
                null,
                RecipientFromEmail(new[] { newUserInfo.Email }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                new TagValue(Constants.TagUserEmail, newUserInfo.Email),
                new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                Constants.TagGreenButton(greenButtonText, CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/')));
        }

        public void UserInfoActivation(UserInfo newUserInfo)
        {
            if (newUserInfo.IsActive)
            {
                throw new ArgumentException("User is already activated!");
            }

            var tariff = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;

            INotifyAction notifyAction;
            var footer = "common";

            if (TenantExtra.Enterprise)
            {
                notifyAction = defaultRebranding ? Constants.ActionEnterpriseUserActivation : Constants.ActionEnterpriseWhitelabelUserActivation;
            }
            else if (TenantExtra.Hosted)
            {
                notifyAction = defaultRebranding ? Constants.ActionHostedUserActivation : Constants.ActionHostedWhitelabelUserActivation;
            }
            else
            {
                if (tariff != null && tariff.Free)
                {
                    notifyAction = Constants.ActionFreeCloudUserActivation;
                    footer = "freecloud";
                }
                else
                {
                    notifyAction = Constants.ActionSaasUserActivation;
                }
            }

            var confirmationUrl = GenerateActivationConfirmUrl(newUserInfo);

            client.SendNoticeToAsync(
                notifyAction,
                null,
                RecipientFromEmail(new[] { newUserInfo.Email.ToLower() }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, confirmationUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonJoin, confirmationUrl),
                new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                new TagValue(CommonTags.Footer, footer),
                new TagValue("noUnsubscribeLink", "true"),
                CreateSendFromTag());
        }

        public void GuestInfoActivation(UserInfo newUserInfo)
        {
            if (newUserInfo.IsActive)
            {
                throw new ArgumentException("User is already activated!");
            }

            var tariff = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;

            INotifyAction notifyAction;
            var footer = "common";

            if (TenantExtra.Enterprise)
            {
                notifyAction = defaultRebranding ? Constants.ActionEnterpriseGuestActivation : Constants.ActionEnterpriseWhitelabelGuestActivation;
            }
            else if (TenantExtra.Hosted)
            {
                notifyAction = defaultRebranding ? Constants.ActionHostedGuestActivation : Constants.ActionHostedWhitelabelGuestActivation;
            }
            else
            {
                if (tariff != null && tariff.Free)
                {
                    notifyAction = Constants.ActionFreeCloudGuestActivation;
                    footer = "freecloud";
                }
                else
                {
                    notifyAction = Constants.ActionSaasGuestActivation;
                }
            }

            var confirmationUrl = GenerateActivationConfirmUrl(newUserInfo);

            client.SendNoticeToAsync(
                notifyAction,
                null,
                RecipientFromEmail(new[] { newUserInfo.Email.ToLower() }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, confirmationUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonJoin, confirmationUrl),
                new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                new TagValue(CommonTags.Footer, footer),
                new TagValue("noUnsubscribeLink", "true"),
                CreateSendFromTag());
        }

        public void SendMsgProfileDeletion(string email)
        {
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.ProfileRemove);

            client.SendNoticeToAsync(
                Constants.ActionProfileDelete,
                null,
                RecipientFromEmail(new[] { email }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, confirmationUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonRemoveProfile, confirmationUrl));
        }

        public void SendMsgReassignsCompleted(Guid recipientId, UserInfo fromUser, UserInfo toUser)
        {
            client.SendNoticeToAsync(
                Constants.ActionReassignsCompleted,
                null,
                new[] { ToRecipient(recipientId) },
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagUserName, DisplayUserSettings.GetFullUserName(recipientId)),
                new TagValue(Constants.TagFromUserName, fromUser.DisplayUserName()),
                new TagValue(Constants.TagFromUserLink, GetUserProfileLink(fromUser.ID)),
                new TagValue(Constants.TagToUserName, toUser.DisplayUserName()),
                new TagValue(Constants.TagToUserLink, GetUserProfileLink(toUser.ID)));
        }

        public void SendMsgReassignsFailed(Guid recipientId, UserInfo fromUser, UserInfo toUser, string message)
        {
            client.SendNoticeToAsync(
                Constants.ActionReassignsFailed,
                null,
                new[] { ToRecipient(recipientId) },
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagUserName, DisplayUserSettings.GetFullUserName(recipientId)),
                new TagValue(Constants.TagFromUserName, fromUser.DisplayUserName()),
                new TagValue(Constants.TagFromUserLink, GetUserProfileLink(fromUser.ID)),
                new TagValue(Constants.TagToUserName, toUser.DisplayUserName()),
                new TagValue(Constants.TagToUserLink, GetUserProfileLink(toUser.ID)),
                new TagValue(Constants.TagMessage, message));
        }

        public void SendMsgRemoveUserDataCompleted(Guid recipientId, Guid fromUserId, string fromUserName, long docsSpace, long crmSpace, long mailSpace, long talkSpace)
        {
            client.SendNoticeToAsync(
                Constants.ActionRemoveUserDataCompleted,
                null,
                new[] { ToRecipient(recipientId) },
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagUserName, DisplayUserSettings.GetFullUserName(recipientId)),
                new TagValue(Constants.TagFromUserName, fromUserName.HtmlEncode()),
                new TagValue(Constants.TagFromUserLink, GetUserProfileLink(fromUserId)),
                new TagValue("DocsSpace", FileSizeComment.FilesSizeToString(docsSpace)),
                new TagValue("CrmSpace", FileSizeComment.FilesSizeToString(crmSpace)),
                new TagValue("MailSpace", FileSizeComment.FilesSizeToString(mailSpace)),
                new TagValue("TalkSpace", FileSizeComment.FilesSizeToString(talkSpace)));
        }

        public void SendMsgRemoveUserDataFailed(Guid recipientId, Guid fromUserId, string fromUserName, string message)
        {
            client.SendNoticeToAsync(
                Constants.ActionRemoveUserDataFailed,
                null,
                new[] { ToRecipient(recipientId) },
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagUserName, DisplayUserSettings.GetFullUserName(recipientId)),
                new TagValue(Constants.TagFromUserName, fromUserName.HtmlEncode()),
                new TagValue(Constants.TagFromUserLink, GetUserProfileLink(fromUserId)),
                new TagValue(Constants.TagMessage, message));
        }

        public void SendAdminWellcome(UserInfo newUserInfo)
        {
            if (!CoreContext.UserManager.UserExists(newUserInfo.ID)) return;

            if (!newUserInfo.IsActive)
                throw new ArgumentException("User is not activated yet!");

            var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;

            INotifyAction notifyAction;

            if (TenantExtra.Enterprise)
            {
                notifyAction = defaultRebranding ? Constants.ActionEnterpriseAdminWellcome : Constants.ActionEnterpriseWhitelabelAdminWellcome;
            }
            else if (TenantExtra.Hosted)
            {
                notifyAction = defaultRebranding ? Constants.ActionHostedAdminWellcome : Constants.ActionHostedWhitelabelAdminWellcome;
            }
            else
            {
                notifyAction = Constants.ActionSaasAdminWellcome;
            }

            var greenBtnText =
                TenantExtra.Enterprise
                    ? WebstudioNotifyPatternResource.ButtonAccessControlPanel
                    : WebstudioNotifyPatternResource.ButtonConfigureRightNow;

            var greenBtnLink =
                TenantExtra.Enterprise
                    ? CommonLinkUtility.GetFullAbsolutePath("~" + SetupInfo.ControlPanelUrl)
                    : CommonLinkUtility.GetAdministration(ManagementType.General);

            var tableItemText1 = string.Empty;
            var tableItemText2 = string.Empty;
            var tableItemText3 = string.Empty;
            var tableItemText4 = string.Empty;
            var tableItemText5 = string.Empty;

            var tableItemImg1 = string.Empty;
            var tableItemImg2 = string.Empty;
            var tableItemImg3 = string.Empty;
            var tableItemImg4 = string.Empty;
            var tableItemImg5 = string.Empty;

            var tableItemComment1 = string.Empty;
            var tableItemComment2 = string.Empty;
            var tableItemComment3 = string.Empty;
            var tableItemComment4 = string.Empty;
            var tableItemComment5 = string.Empty;


            if (!TenantExtra.Enterprise)
            {
                tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/tips-brand-100.png";
                tableItemText1 = WebstudioNotifyPatternResource.ItemBrandYourWebOffice;
                tableItemComment1 = WebstudioNotifyPatternResource.ItemBrandYourWebOfficeText;

                tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/tips-regional-setings-100.png";
                tableItemText2 = WebstudioNotifyPatternResource.ItemAdjustRegionalSettings;
                tableItemComment2 = WebstudioNotifyPatternResource.ItemAdjustRegionalSettingsText;


                tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/tips-customize-100.png";
                tableItemText3 = WebstudioNotifyPatternResource.ItemCustomizeWebOfficeInterface;
                tableItemComment3 = WebstudioNotifyPatternResource.ItemCustomizeWebOfficeInterfaceText;

                tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/tips-modules-100.png";
                tableItemText4 = WebstudioNotifyPatternResource.ItemModulesAndTools;
                tableItemComment4 = WebstudioNotifyPatternResource.ItemModulesAndToolsText;

                tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/tips-sms-secure-100.png";
                tableItemText5 = WebstudioNotifyPatternResource.ItemSecureAccess;
                tableItemComment5 = WebstudioNotifyPatternResource.ItemSecureAccessText;
            }

            client.SendNoticeToAsync(
                notifyAction,
                null,
                RecipientFromEmail(new[] { newUserInfo.Email.ToLower() }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                Constants.TagGreenButton(greenBtnText, greenBtnLink),
                Constants.TagTableTop(),
                Constants.TagTableItem(1, tableItemText1, string.Empty, tableItemImg1, tableItemComment1, string.Empty, string.Empty),
                Constants.TagTableItem(2, tableItemText2, string.Empty, tableItemImg2, tableItemComment2, string.Empty, string.Empty),
                Constants.TagTableItem(3, tableItemText3, string.Empty, tableItemImg3, tableItemComment3, string.Empty, string.Empty),
                Constants.TagTableItem(4, tableItemText4, string.Empty, tableItemImg4, tableItemComment4, string.Empty, string.Empty),
                Constants.TagTableItem(5, tableItemText5, string.Empty, tableItemImg5, tableItemComment5, string.Empty, string.Empty),
                Constants.TagTableBottom(),
                new TagValue(CommonTags.Footer, "common"),
                CreateSendFromTag());
        }

        #region Backup & Restore

        public void SendMsgBackupCompleted(Guid userId, string link)
        {
            client.SendNoticeToAsync(
                Constants.ActionBackupCreated,
                null,
                new[] {ToRecipient(userId)},
                new[] {EMailSenderName},
                null,
                new TagValue(Constants.TagOwnerName, CoreContext.UserManager.GetUsers(userId).DisplayUserName()));
        }

        public void SendMsgRestoreStarted(bool notifyAllUsers)
        {
            IRecipient[] users =
                notifyAllUsers
                    ? CoreContext.UserManager.GetUsers(EmployeeStatus.Active).Select(u => ToRecipient(u.ID)).ToArray()
                    : new[] { ToRecipient(CoreContext.TenantManager.GetCurrentTenant().OwnerId) };

            client.SendNoticeToAsync(
                Constants.ActionRestoreStarted,
                null,
                users,
                new[] { EMailSenderName },
                null);
        }

        public void SendMsgRestoreCompleted(bool notifyAllUsers)
        {
            var owner = CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant().OwnerId);

            IRecipient[] users =
                notifyAllUsers
                    ? CoreContext.UserManager.GetUsers(EmployeeStatus.Active).Select(u => ToRecipient(u.ID)).ToArray()
                    : new[] { ToRecipient(owner.ID) };

            client.SendNoticeToAsync(
                Constants.ActionRestoreCompleted,
                null,
                users,
                new[] {EMailSenderName},
                null,
                new TagValue(Constants.TagOwnerName, owner.DisplayUserName()));
        }

        #endregion

        #region Portal Deactivation & Deletion

        public void SendMsgPortalDeactivation(Tenant t, string d_url, string a_url)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionPortalDeactivate,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagActivateUrl, a_url),
                        new TagValue(Constants.TagDeactivateUrl, d_url), //TODO: Tag is deprecated and replaced by TagGreenButton
                        Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonDeactivatePortal, d_url),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }

        public void SendMsgPortalDeletion(Tenant t, string url, bool showAutoRenewText)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionPortalDelete,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagDeleteUrl, url), //TODO: Tag is deprecated and replaced by TagGreenButton
                        Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonDeletePortal, url),
                        new TagValue(Constants.TagAutoRenew, showAutoRenewText.ToString()),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }

        public void SendMsgPortalDeletionSuccess(Tenant t, TenantQuota tariff, string url)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);

            var notifyAction = tariff != null && tariff.Free ? Constants.ActionPortalDeleteSuccessFreeCloud : Constants.ActionPortalDeleteSuccess;

            client.SendNoticeToAsync(
                        notifyAction,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagFeedBackUrl, url), //TODO: Tag is deprecated and replaced by TagGreenButton
                        Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonLeaveFeedback, url),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }

        #endregion

        public void SendMsgDnsChange(Tenant t, string confirmDnsUpdateUrl, string portalAddress, string portalDns)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionDnsChange,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue("ConfirmDnsUpdate", confirmDnsUpdateUrl),//TODO: Tag is deprecated and replaced by TagGreenButton
                        Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonConfirmPortalAddressChange, confirmDnsUpdateUrl),
                        new TagValue("PortalAddress", AddHttpToUrl(portalAddress)),
                        new TagValue("PortalDns", AddHttpToUrl(portalDns ?? string.Empty)),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }


        public void SendMsgConfirmChangeOwner(Tenant t, string newOwnerName, string confirmOwnerUpdateUrl)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionConfirmOwnerChange,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue("ConfirmPortalOwnerUpdate", confirmOwnerUpdateUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                        Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonConfirmPortalOwnerUpdate, confirmOwnerUpdateUrl),
                        new TagValue(Constants.TagUserName, newOwnerName),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName()));
        }


        public void SendCongratulations(UserInfo u)
        {
            try
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var tariff = CoreContext.TenantManager.GetTenantQuota(tenant.TenantId);
                var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;

                INotifyAction notifyAction;
                var footer = "common";

                if (TenantExtra.Enterprise)
                {
                    notifyAction = defaultRebranding ? Constants.ActionEnterpriseAdminActivation : Constants.ActionEnterpriseWhitelabelAdminActivation;
                }
                else if (TenantExtra.Hosted)
                {
                    notifyAction = defaultRebranding ? Constants.ActionHostedAdminActivation : Constants.ActionHostedWhitelabelAdminActivation;
                }
                else if (TenantExtra.Opensource)
                {
                    notifyAction = Constants.ActionOpensourceAdminActivation;
                    footer = "opensource";
                }
                else
                {
                    if (tariff != null && tariff.Free)
                    {
                        notifyAction = Constants.ActionFreeCloudAdminActivation;
                        footer = "freecloud";
                    }
                    else
                    {
                        notifyAction = Constants.ActionSaasAdminActivation;
                    }
                }

                var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(u.Email, ConfirmType.EmailActivation);
                confirmationUrl += "&first=true";

                client.SendNoticeToAsync(
                    notifyAction,
                    null,
                    RecipientFromEmail(new[] { u.Email.ToLower() }, false),
                    new[] { EMailSenderName },
                    null,
                    new TagValue(Constants.TagUserName, u.DisplayUserName()),
                    new TagValue(Constants.TagUserEmail, u.Email),
                    new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                    new TagValue(Constants.TagSettingsLink, CommonLinkUtility.GetAdministration(ManagementType.General)),
                    new TagValue(Constants.TagInviteLink, confirmationUrl), //TODO: Tag is deprecated and replaced by TagGreenButton
                    Constants.TagGreenButton(WebstudioNotifyPatternResource.ButtonForConfirmation, confirmationUrl),
                    new TagValue(CommonTags.Footer, footer),
                    Constants.UnsubscribeLink);
            }
            catch (Exception error)
            {
                LogManager.GetLogger("ASC.Notify").Error(error);
            }
        }


        public void SendSaasTariffLetters(DateTime scheduleDate)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var now = scheduleDate.Date;
            const string dbid = "webstudio";

            log.Info("Start SendSaasTariffLetters");

            var activeTenants = CoreContext.TenantManager.GetTenants().ToList();

            if (activeTenants.Count <= 0)
            {
                log.Info("End SendSaasTariffLetters");
                return;
            }

            var monthQuotasIds = CoreContext.TenantManager.GetTenantQuotas()
                            .Where(r => !r.Trial && r.Visible && !r.Year && !r.Year3 && !r.Free && !r.NonProfit)
                            .Select(q => q.Id)
                            .ToArray();

            foreach (var tenant in activeTenants)
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);

                    var tariff = CoreContext.PaymentManager.GetTariff(tenant.TenantId);
                    var quota = CoreContext.TenantManager.GetTenantQuota(tenant.TenantId);

                    var duedate = tariff.DueDate.Date;
                    var delayDuedate = tariff.DelayDueDate.Date;

                    INotifyAction action = null;

                    var toadmins = false;
                    var tousers = false;
                    var toguests = false;
                    var toowner = false;

                    var footer = "common";

                    Func<string> greenButtonText = () => string.Empty;
                    var greenButtonUrl = string.Empty;
                    var feedbackUrl = string.Empty;

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

                        #region 2 days after registration to users SAAS TRIAL

                        if (tenant.CreatedDateTime.Date.AddDays(2) == now)
                        {
                            action = Constants.ActionSaasUserOrganizeWorkplace;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-01-50.png";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemAddFilesCreatWorkspace;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-02-50.png";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemTryOnlineDocEditor;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-06-50.png";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemShareDocuments;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-07-50.png";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemCoAuthoringDocuments;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-04-50.png";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemAddTeamlabMail;

                            tableItemImg6 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-03-50.png";
                            tableItemComment6 = () => WebstudioNotifyPatternResource.ItemUploadCrmContacts;

                            //tableItemImg7 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-05-50.png";
                            //tableItemComment7 = () => WebstudioNotifyPatternResource.ItemIntegrateIM;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonMoveRightNow;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/');
                        }

                        #endregion

                        #region 3 days after registration to admins SAAS TRIAL + only 1 user

                        else if (tenant.CreatedDateTime.Date.AddDays(3) == now && CoreContext.UserManager.GetUsers().Count() == 1)
                        {
                            action = Constants.ActionSaasAdminInviteTeammates;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonInviteRightNow;
                            greenButtonUrl = String.Format("{0}/products/people/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 5 days after registration to admins SAAS TRAIL + without activity in 1 or more days

                        else if (tenant.CreatedDateTime.Date.AddDays(5) == now)
                        {
                            List<DateTime> datesWithActivity;

                            var query = new SqlQuery("feed_aggregate")
                                .Select(new SqlExp("cast(created_date as date) as short_date"))

                                .Where("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                                .Where(Exp.Le("created_date", now.AddDays(-1)))
                                .GroupBy("short_date");

                            using (var db = new DbManager(dbid))
                            {
                                datesWithActivity = db
                                    .ExecuteList(query)
                                    .ConvertAll(r => Convert.ToDateTime(r[0]));
                            }

                            if (datesWithActivity.Count < 5)
                            {
                                action = Constants.ActionSaasAdminWithoutActivity;
                                toadmins = true;
                            }
                        }

                        #endregion

                        #region 7 days after registration to admins and users SAAS TRIAL

                        else if (tenant.CreatedDateTime.Date.AddDays(7) == now)
                        {
                            action = Constants.ActionSaasAdminUserDocsTips;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-01-100.png";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemSaasDocsTips1;
                            tableItemLearnMoreUrl1 = "https://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/CollaborativeEditing.aspx";
                            tableItemLearnMoreText1 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-02-100.png";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemSaasDocsTips2;
                            tableItemLearnMoreUrl2 = "http://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/UsageInstructions/ViewDocInfo.aspx";
                            tableItemLearnMoreText2 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-07-100.png";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemSaasDocsTips3;
                            tableItemLearnMoreUrl3 = "https://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/Review.aspx";
                            tableItemLearnMoreText3 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-03-100.png";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemSaasDocsTips4;
                            tableItemLearnMoreUrl4 = "https://helpcenter.onlyoffice.com/gettingstarted/documents.aspx#SharingDocuments_block";
                            tableItemLearnMoreText4 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-06-100.png";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemSaasDocsTips5;
                            tableItemLearnMoreUrl5 = "http://helpcenter.onlyoffice.com/tipstricks/add-resource.aspx";
                            tableItemLearnMoreText5 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg6 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-08-100.png";
                            tableItemComment6 = () => WebstudioNotifyPatternResource.ItemSaasDocsTips6;
                            tableItemLearnMoreUrl6 = "http://www.onlyoffice.com/desktop.aspx";
                            tableItemLearnMoreText6 = () => WebstudioNotifyPatternResource.ButtonDownloadNow;

                            tableItemImg7 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-05-100.png";
                            tableItemComment7 = () => WebstudioNotifyPatternResource.ItemSaasDocsTips7;
                            tableItemLearnMoreUrl7 = "https://itunes.apple.com/us/app/onlyoffice-documents/id944896972";
                            tableItemLearnMoreText7 = () => WebstudioNotifyPatternResource.ButtonGoToAppStore;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;
                            greenButtonUrl = String.Format("{0}/products/files/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 10 days after registration to admins and users SAAS TRIAL

                        else if (tenant.CreatedDateTime.Date.AddDays(10) == now)
                        {
                            action = Constants.ActionSaasAdminUserPowerfulTips;
                            toadmins = true;
                            tousers = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSendRequest;
                            greenButtonUrl = "mailto:sales@onlyoffice.com";
                        }

                        #endregion

                        #region 2 weeks after registration to admins and users SAAS TRIAL

                        else if (tenant.CreatedDateTime.Date.AddDays(14) == now)
                        {
                            action = Constants.ActionSaasAdminUserMailTips;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-01-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.ItemFeatureMailGroups;
                            tableItemUrl1 = "https://helpcenter.onlyoffice.com/tipstricks/alias-groups.aspx";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemFeatureMailGroupsText;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-02-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.ItemFeatureMailboxAliases;
                            tableItemUrl2 = "https://helpcenter.onlyoffice.com/tipstricks/alias-groups.aspx";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemFeatureMailboxAliasesText;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-03-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.ItemFeatureEmailSignature;
                            tableItemUrl3 = "https://helpcenter.onlyoffice.com/gettingstarted/mail.aspx#SendingReceivingMessages_block__addingSignature";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemFeatureEmailSignatureText;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-04-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.ItemFeatureLinksVSAttachments;
                            tableItemUrl4 = "https://helpcenter.onlyoffice.com/gettingstarted/mail.aspx#SendingReceivingMessages_block";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemFeatureLinksVSAttachmentsText;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-05-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.ItemFeatureFolderForAtts;
                            tableItemUrl5 = "https://helpcenter.onlyoffice.com/gettingstarted/mail.aspx#SendingReceivingMessages_block__emailIn";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemFeatureFolderForAttsText;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessMail;
                            greenButtonUrl = String.Format("{0}/addons/mail/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 3 weeks after registration to admins and users SAAS TRIAL

                        else if (tenant.CreatedDateTime.Date.AddDays(21) == now)
                        {
                            action = Constants.ActionSaasAdminUserCrmTips;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/crm-01-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.ItemWebToLead;
                            tableItemUrl1 = "https://helpcenter.onlyoffice.com/tipstricks/website-contact-form.aspx";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemWebToLeadText;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/crm-02-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.ItemARM;
                            tableItemUrl2 = "https://helpcenter.onlyoffice.com/gettingstarted/crm.aspx#AddingContacts_block";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemARMText;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/crm-03-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.ItemCustomization;
                            tableItemUrl3 = "https://helpcenter.onlyoffice.com/gettingstarted/crm.aspx#ChangingCRMSettings_block";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemCustomizationText;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/crm-04-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.ItemLinkWithProjects;
                            tableItemUrl4 = "https://helpcenter.onlyoffice.com/guides/link-with-project.aspx";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemLinkWithProjectsText;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/crm-05-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.ItemMailIntegration;
                            tableItemUrl5 = "https://helpcenter.onlyoffice.com/gettingstarted/mail.aspx#IntegratingwithCRM_block";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemMailIntegrationText;

                            tableItemImg6 = "http://cdn.teamlab.com/media/newsletters/images/crm-06-100.png";
                            tableItemText6 = () => WebstudioNotifyPatternResource.ItemVoIP;
                            tableItemUrl6 = "https://helpcenter.onlyoffice.com/gettingstarted/crm.aspx#VoIP_block";
                            tableItemComment6 = () => WebstudioNotifyPatternResource.ItemVoIPText;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessCRMSystem;
                            greenButtonUrl = String.Format("{0}/products/crm/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 4 weeks after registration to admins and users SAAS TRIAL

                        else if (tenant.CreatedDateTime.Date.AddDays(28) == now)
                        {
                            action = Constants.ActionSaasAdminUserTeamTips;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-01-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.ItemFeatureCommunity;
                            tableItemUrl1 = "http://helpcenter.onlyoffice.com/gettingstarted/community.aspx";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemFeatureCommunityText;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-02-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.ItemFeatureGanttChart;
                            tableItemUrl2 = "http://helpcenter.onlyoffice.com/guides/gantt-chart.aspx";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemFeatureGanttChartText;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-03-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.ItemFeatureProjectDiscussions;
                            tableItemUrl3 = "http://helpcenter.onlyoffice.com/gettingstarted/projects.aspx#LeadingDiscussion_block";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemFeatureProjectDiscussionsText;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-04-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.ItemFeatureDocCoAuthoring;
                            tableItemUrl4 = "http://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/CollaborativeEditing.aspx";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemFeatureDocCoAuthoringText;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-05-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.ItemFeatureTalk;
                            tableItemUrl5 = "http://helpcenter.onlyoffice.com/gettingstarted/talk.aspx";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemFeatureTalkText;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;
                            greenButtonUrl = CommonLinkUtility.GetAdministration(ManagementType.ProductsAndInstruments);
                        }

                        #endregion

                        #endregion

                        #region Trial warning letters

                        #region 5 days before SAAS TRIAL ends to admins

                        else if (duedate != DateTime.MaxValue && duedate.AddDays(-5) == now)
                        {
                            action = Constants.ActionSaasAdminTrialWarningBefore5;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                        }

                        #endregion

                        #region SAAS TRIAL expires today to admins

                        else if (duedate == now)
                        {
                            action = Constants.ActionSaasAdminTrialWarning;
                            toadmins = true;
                        }

                        #endregion

                        #region 5 days after SAAS TRIAL expired to admins

                        else if (duedate != DateTime.MaxValue && duedate.AddDays(5) == now && tenant.VersionChanged <= tenant.CreatedDateTime)
                        {
                            action = Constants.ActionSaasAdminTrialWarningAfter5;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSendRequest; //WebstudioNotifyPatternResource.ButtonExtendTrialButton;
                            greenButtonUrl = "mailto:sales@onlyoffice.com";
                        }

                        #endregion

                        #region 30 days after SAAS TRIAL expired + only 1 user

                        else if (duedate != DateTime.MaxValue && duedate.AddDays(30) == now && CoreContext.UserManager.GetUsers().Count() == 1)
                        {
                            action = Constants.ActionSaasAdminTrialWarningAfter30;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSignUpPersonal;
                            greenButtonUrl = "https://personal.onlyoffice.com";
                        }

                        #endregion

                        #region 6 months after SAAS TRIAL expired

                        else if (duedate != DateTime.MaxValue && duedate.AddMonths(6) == now)
                        {
                            action = Constants.ActionSaasAdminTrialWarningAfterHalfYear;
                            toowner = true;

                            var owner = CoreContext.UserManager.GetUsers(tenant.OwnerId);
                            feedbackUrl = SetupInfo.TeamlabSiteRedirect + "/remove-portal-feedback-form.aspx#" +
                                          Convert.ToBase64String(
                                              System.Text.Encoding.UTF8.GetBytes("{\"firstname\":\"" + owner.FirstName +
                                                                                 "\",\"lastname\":\"" + owner.LastName +
                                                                                 "\",\"alias\":\"" + tenant.TenantAlias +
                                                                                 "\",\"email\":\"" + owner.Email + "\"}")); 
                        }
                        else if (duedate != DateTime.MaxValue && duedate.AddMonths(6).AddDays(7) <= now)
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

                        #region 7 days before SAAS PAID expired to admins

                        if (tariff.State == TariffState.Paid && duedate != DateTime.MaxValue && duedate.AddDays(-7) == now)
                        {
                            action = Constants.ActionSaasAdminPaymentWarningBefore7;
                            toadmins = true;
                        }

                        #endregion

                        #region SAAS PAID expires today to admins

                        else if (tariff.State >= TariffState.Paid && duedate == now)
                        {
                            action = Constants.ActionSaasAdminPaymentWarning;
                            toadmins = true;
                        }

                        #endregion

                        #region 3 days after SAAS PAID expired on delay to admins

                        else if (tariff.State == TariffState.Delay && duedate != DateTime.MaxValue && duedate.AddDays(3) == now)
                        {
                            action = Constants.ActionSaasAdminPaymentWarningAfter3;
                            toadmins = true;
                        }

                        #endregion

                        #region payment delay expires today to admins

                        else if (tariff.State >= TariffState.Delay && delayDuedate == now)
                        {
                            action = Constants.ActionSaasAdminPaymentWarningDelayDue;
                            toadmins = true;
                        }

                        #endregion

                        #region 2 weeks after 3 times SAAS PAID to admins

                        else if (tariff.State == TariffState.Paid)
                        {
                            try
                            {
                                DateTime lastDatePayment;

                                var query = new SqlQuery("tenants_tariff")
                                    .Select("max(create_on)")
                                    .Where(Exp.Eq("tenant", tenant.TenantId) & Exp.In("tariff", monthQuotasIds))
                                    .Having(Exp.Sql("count(*) >= 3"));

                                using (var db = new DbManager(dbid))
                                {
                                    lastDatePayment = db.ExecuteScalar<DateTime>(query);
                                }

                                if (lastDatePayment != DateTime.MinValue && lastDatePayment.AddDays(14) == now)
                                {
                                    action = Constants.ActionSaasAdminPaymentAfterMonthlySubscriptions;
                                    toadmins = true;
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error(e);
                            }
                        }

                        #endregion

                        #endregion
                    }


                    if (action == null) continue;

                    var users = toowner
                                    ? new List<UserInfo> { CoreContext.UserManager.GetUsers(tenant.OwnerId) }
                                    : GetRecipients(toadmins, tousers, toguests);

                    foreach (var u in users)
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;
                        var rquota = TenantExtra.GetRightQuota() ?? TenantQuota.Default;

                        client.SendNoticeToAsync(
                            action,
                            null,
                            new[] { ToRecipient(u.ID) },
                            new[] { EMailSenderName },
                            null,
                            new TagValue(Constants.TagUserName, u.DisplayUserName()),
                            new TagValue(Constants.TagPricingPage, CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx")),
                            new TagValue(Constants.TagActiveUsers, CoreContext.UserManager.GetUsers().Count()),
                            new TagValue(Constants.TagPrice, rquota.Price),//TODO: use price partner
                            new TagValue(Constants.TagPricePeriod, rquota.Year3 ? UserControlsCommonResource.TariffPerYear3 : rquota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth),
                            new TagValue(Constants.TagDueDate, duedate.ToLongDateString()),
                            new TagValue(Constants.TagDelayDueDate, (delayDuedate != DateTime.MaxValue ? delayDuedate : duedate).ToLongDateString()),
                            Constants.TagBlueButton(WebstudioNotifyPatternResource.ButtonRequestCallButton, "http://www.onlyoffice.com/call-back-form.aspx"),
                            Constants.TagGreenButton(greenButtonText(), greenButtonUrl),
                            Constants.TagTableTop(),
                            Constants.TagTableItem(1, tableItemText1(), tableItemUrl1, tableItemImg1, tableItemComment1(), tableItemLearnMoreText1(), tableItemLearnMoreUrl1),
                            Constants.TagTableItem(2, tableItemText2(), tableItemUrl2, tableItemImg2, tableItemComment2(), tableItemLearnMoreText2(), tableItemLearnMoreUrl2),
                            Constants.TagTableItem(3, tableItemText3(), tableItemUrl3, tableItemImg3, tableItemComment3(), tableItemLearnMoreText3(), tableItemLearnMoreUrl3),
                            Constants.TagTableItem(4, tableItemText4(), tableItemUrl4, tableItemImg4, tableItemComment4(), tableItemLearnMoreText4(), tableItemLearnMoreUrl4),
                            Constants.TagTableItem(5, tableItemText5(), tableItemUrl5, tableItemImg5, tableItemComment5(), tableItemLearnMoreText5(), tableItemLearnMoreUrl5),
                            Constants.TagTableItem(6, tableItemText6(), tableItemUrl6, tableItemImg6, tableItemComment6(), tableItemLearnMoreText6(), tableItemLearnMoreUrl6),
                            Constants.TagTableItem(7, tableItemText7(), tableItemUrl7, tableItemImg7, tableItemComment7(), tableItemLearnMoreText7(), tableItemLearnMoreUrl7),
                            Constants.TagTableBottom(),
                            new TagValue(CommonTags.Footer, string.IsNullOrEmpty(tenant.PartnerId) ? footer : string.Empty),
                            new TagValue(Constants.TagFeedBackUrl, feedbackUrl));
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendSaasTariffLetters");
        }

        public void SendEnterpriseTariffLetters(DateTime scheduleDate)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var now = scheduleDate.Date;
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

                    var duedate = tariff.DueDate.Date;
                    var delayDuedate = tariff.DelayDueDate.Date;

                    INotifyAction action = null;

                    var toadmins = false;
                    var tousers = false;
                    var toguests = false;

                    var footer = "common";

                    Func<string> greenButtonText = () => string.Empty;
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

                        #region 2 days after registration to users ENTERPRISE TRIAL + defaultRebranding

                        if (tenant.CreatedDateTime.Date.AddDays(2) == now)
                        {
                            action = Constants.ActionEnterpriseUserOrganizeWorkplace;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-01-50.png";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemAddFilesCreatWorkspace;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-02-50.png";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemTryOnlineDocEditor;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-06-50.png";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemShareDocuments;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-07-50.png";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemCoAuthoringDocuments;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-03-50.png";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemUploadCrmContacts;

                            tableItemImg6 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-04-50.png";
                            tableItemComment6 = () => WebstudioNotifyPatternResource.ItemAddTeamlabMail;

                            //tableItemImg7 = "http://cdn.teamlab.com/media/newsletters/images/move-to-cloud-05-50.png";
                            //tableItemComment7 = () => WebstudioNotifyPatternResource.ItemIntegrateIM;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYourPortal;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/');
                        }

                        #endregion

                        #region 3 days after registration to admins ENTERPRISE TRIAL + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(3) == now)
                        {
                            action = Constants.ActionEnterpriseAdminCustomizePortal;
                            toadmins = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/tips-brand-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.ItemBrandYourWebOffice;
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemBrandYourWebOfficeText;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/tips-regional-setings-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.ItemAdjustRegionalSettings;
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemAdjustRegionalSettingsText;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/tips-customize-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.ItemCustomizeWebOfficeInterface;
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemCustomizeWebOfficeInterfaceText;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/tips-modules-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.ItemModulesAndTools;
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemModulesAndToolsText;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/tips-3rdparty-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.Item3rdParty;
                            tableItemComment5 = () => WebstudioNotifyPatternResource.Item3rdPartyText;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonConfigureRightNow;
                            greenButtonUrl = CommonLinkUtility.GetAdministration(ManagementType.General);
                        }

                        #endregion

                        #region 4 days after registration to admins ENTERPRISE TRIAL + only 1 user + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(4) == now && CoreContext.UserManager.GetUsers().Count() == 1)
                        {
                            action = Constants.ActionEnterpriseAdminInviteTeammates;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonInviteRightNow;
                            greenButtonUrl = String.Format("{0}/products/people/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 5 days after registration to admins ENTERPRISE TRAIL + without activity in 1 or more days + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(5) == now)
                        {
                            List<DateTime> datesWithActivity;

                            var query = new SqlQuery("feed_aggregate")
                                .Select(new SqlExp("cast(created_date as date) as short_date"))
                                .Where("tenant", CoreContext.TenantManager.GetCurrentTenant().TenantId)
                                .Where(Exp.Le("created_date", now.AddDays(-1)))
                                .GroupBy("short_date");

                            using (var db = new DbManager(dbid))
                            {
                                datesWithActivity = db
                                    .ExecuteList(query)
                                    .ConvertAll(r => Convert.ToDateTime(r[0]));
                            }

                            if (datesWithActivity.Count < 5)
                            {
                                action = Constants.ActionEnterpriseAdminWithoutActivity;
                                toadmins = true;
                            }
                        }

                        #endregion

                        #region 7 days after registration to admins and users ENTERPRISE TRIAL + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(7) == now)
                        {
                            action = Constants.ActionEnterpriseAdminUserDocsTips;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-01-100.png";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemEnterpriseDocsTips1;
                            tableItemLearnMoreUrl1 = "https://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/CollaborativeEditing.aspx";
                            tableItemLearnMoreText1 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-02-100.png";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemEnterpriseDocsTips2;
                            tableItemLearnMoreUrl2 = "http://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/UsageInstructions/ViewDocInfo.aspx";
                            tableItemLearnMoreText2 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-07-100.png";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemEnterpriseDocsTips3;
                            tableItemLearnMoreUrl3 = "https://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/Review.aspx";
                            tableItemLearnMoreText3 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-03-100.png";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemEnterpriseDocsTips4;
                            tableItemLearnMoreUrl4 = "https://helpcenter.onlyoffice.com/gettingstarted/documents.aspx#SharingDocuments_block";
                            tableItemLearnMoreText4 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-04-100.png";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemEnterpriseDocsTips5;
                            tableItemLearnMoreUrl5 = "https://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/UsageInstructions/UseMailMerge.aspx";
                            tableItemLearnMoreText5 = () => WebstudioNotifyPatternResource.ButtonGoToAppStore;

                            tableItemImg6 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-08-100.png";
                            tableItemComment6 = () => WebstudioNotifyPatternResource.ItemEnterpriseDocsTips6;
                            tableItemLearnMoreUrl6 = "http://www.onlyoffice.com/desktop.aspx";
                            tableItemLearnMoreText6 = () => WebstudioNotifyPatternResource.ButtonDownloadNow;

                            tableItemImg7 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-05-100.png";
                            tableItemComment7 = () => WebstudioNotifyPatternResource.ItemEnterpriseDocsTips7;
                            tableItemLearnMoreUrl7 = "https://itunes.apple.com/us/app/onlyoffice-documents/id944896972";
                            tableItemLearnMoreText7 = () => WebstudioNotifyPatternResource.ButtonGoToAppStore;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;
                            greenButtonUrl = String.Format("{0}/products/files/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 10 days after registration to admins and users ENTERPRISE TRIAL + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(10) == now)
                        {
                            action = Constants.ActionEnterpriseAdminUserPowerfulTips;
                            toadmins = true;
                            tousers = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;
                            greenButtonUrl = String.Format("{0}/products/files/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 2 weeks after registration to admins and users ENTERPRISE TRIAL + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(14) == now)
                        {
                            action = Constants.ActionEnterpriseAdminUserMailTips;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-01-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.ItemFeatureMailGroups;
                            tableItemUrl1 = "https://helpcenter.onlyoffice.com/tipstricks/alias-groups.aspx";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemFeatureMailGroupsText;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-02-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.ItemFeatureMailboxAliases;
                            tableItemUrl2 = "https://helpcenter.onlyoffice.com/tipstricks/alias-groups.aspx";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemFeatureMailboxAliasesText;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-03-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.ItemFeatureEmailSignature;
                            tableItemUrl3 = "https://helpcenter.onlyoffice.com/gettingstarted/mail.aspx#SendingReceivingMessages_block__addingSignature";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemFeatureEmailSignatureText;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-04-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.ItemFeatureLinksVSAttachments;
                            tableItemUrl4 = "https://helpcenter.onlyoffice.com/gettingstarted/mail.aspx#SendingReceivingMessages_block";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemFeatureLinksVSAttachmentsText;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/mail-exp-05-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.ItemFeatureFolderForAtts;
                            tableItemUrl5 = "https://helpcenter.onlyoffice.com/gettingstarted/mail.aspx#SendingReceivingMessages_block__emailIn";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemFeatureFolderForAttsText;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessMail;
                            greenButtonUrl = String.Format("{0}/addons/mail/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 3 weeks after registration to admins and users ENTERPRISE TRIAL + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(21) == now)
                        {
                            action = Constants.ActionEnterpriseAdminUserCrmTips;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/crm-01-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.ItemWebToLead;
                            tableItemUrl1 = "https://helpcenter.onlyoffice.com/tipstricks/website-contact-form.aspx";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemWebToLeadText;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/crm-02-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.ItemARM;
                            tableItemUrl2 = "https://helpcenter.onlyoffice.com/gettingstarted/crm.aspx#AddingContacts_block";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemARMText;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/crm-03-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.ItemCustomization;
                            tableItemUrl3 = "https://helpcenter.onlyoffice.com/gettingstarted/crm.aspx#ChangingCRMSettings_block";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemCustomizationText;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/crm-04-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.ItemLinkWithProjects;
                            tableItemUrl4 = "https://helpcenter.onlyoffice.com/guides/link-with-project.aspx";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemLinkWithProjectsText;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/crm-05-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.ItemMailIntegration;
                            tableItemUrl5 = "https://helpcenter.onlyoffice.com/gettingstarted/mail.aspx#IntegratingwithCRM_block";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemMailIntegrationText;

                            tableItemImg6 = "http://cdn.teamlab.com/media/newsletters/images/crm-06-100.png";
                            tableItemText6 = () => WebstudioNotifyPatternResource.ItemVoIP;
                            tableItemUrl6 = "https://helpcenter.onlyoffice.com/gettingstarted/crm.aspx#VoIP_block";
                            tableItemComment6 = () => WebstudioNotifyPatternResource.ItemVoIPText;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessCRMSystem;
                            greenButtonUrl = String.Format("{0}/products/crm/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                        }

                        #endregion

                        #region 4 weeks after registration to admins and users ENTERPRISE TRIAL + defaultRebranding

                        else if (tenant.CreatedDateTime.Date.AddDays(28) == now)
                        {
                            action = Constants.ActionEnterpriseAdminUserTeamTips;
                            toadmins = true;
                            tousers = true;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-01-100.png";
                            tableItemText1 = () => WebstudioNotifyPatternResource.ItemFeatureCommunity;
                            tableItemUrl1 = "http://helpcenter.onlyoffice.com/gettingstarted/community.aspx";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemFeatureCommunityText;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-02-100.png";
                            tableItemText2 = () => WebstudioNotifyPatternResource.ItemFeatureGanttChart;
                            tableItemUrl2 = "http://helpcenter.onlyoffice.com/guides/gantt-chart.aspx";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemFeatureGanttChartText;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-03-100.png";
                            tableItemText3 = () => WebstudioNotifyPatternResource.ItemFeatureProjectDiscussions;
                            tableItemUrl3 = "http://helpcenter.onlyoffice.com/gettingstarted/projects.aspx#LeadingDiscussion_block";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemFeatureProjectDiscussionsText;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-04-100.png";
                            tableItemText4 = () => WebstudioNotifyPatternResource.ItemFeatureDocCoAuthoring;
                            tableItemUrl4 = "http://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/CollaborativeEditing.aspx";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemFeatureDocCoAuthoringText;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/collaboration-05-100.png";
                            tableItemText5 = () => WebstudioNotifyPatternResource.ItemFeatureTalk;
                            tableItemUrl5 = "http://helpcenter.onlyoffice.com/gettingstarted/talk.aspx";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemFeatureTalkText;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;
                            greenButtonUrl = CommonLinkUtility.GetAdministration(ManagementType.ProductsAndInstruments);
                        }

                        #endregion

                        #endregion

                        #region Trial warning letters

                        #region 7 days before ENTERPRISE TRIAL ends to admins + defaultRebranding

                        else if (duedate != DateTime.MaxValue && duedate.AddDays(-7) == now)
                        {
                            action = Constants.ActionEnterpriseAdminTrialWarningBefore7;
                            toadmins = true;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = "http://www.onlyoffice.com/enterprise-edition.aspx";
                        }

                        #endregion

                        #region ENTERPRISE TRIAL expires today to admins + defaultRebranding

                        else if (duedate == now)
                        {
                            action = Constants.ActionEnterpriseAdminTrialWarning;
                            toadmins = true;
                        }

                        #endregion

                        #endregion
                    }
                    else if (tariff.State == TariffState.Paid)
                    {
                        #region Payment warning letters

                        #region 7 days before ENTERPRISE PAID expired to admins

                        if (duedate != DateTime.MaxValue && duedate.AddDays(-7) == now)
                        {
                            action = defaultRebranding
                                         ? Constants.ActionEnterpriseAdminPaymentWarningBefore7
                                         : Constants.ActionEnterpriseWhitelabelAdminPaymentWarningBefore7;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                        }

                        #endregion

                        #region ENTERPRISE PAID expires today to admins

                        else if (duedate == now)
                        {
                            action = defaultRebranding
                                         ? Constants.ActionEnterpriseAdminPaymentWarning
                                         : Constants.ActionEnterpriseWhitelabelAdminPaymentWarning;
                            toadmins = true;
                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonSelectPricingPlans;
                            greenButtonUrl = CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx");
                        }

                        #endregion

                        #endregion
                    }


                    if (action == null) continue;

                    var users = GetRecipients(toadmins, tousers, toguests);

                    foreach (var u in users)
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        var rquota = TenantExtra.GetRightQuota() ?? TenantQuota.Default;

                        client.SendNoticeToAsync(
                            action,
                            null,
                            new[] {ToRecipient(u.ID)},
                            new[] {EMailSenderName},
                            null,
                            new TagValue(Constants.TagUserName, u.DisplayUserName()),
                            new TagValue(Constants.TagPricingPage, CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx")),
                            new TagValue(Constants.TagActiveUsers, CoreContext.UserManager.GetUsers().Count()),
                            new TagValue(Constants.TagPrice, rquota.Price), //TODO: use price partner
                            new TagValue(Constants.TagPricePeriod, rquota.Year3 ? UserControlsCommonResource.TariffPerYear3 : rquota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth),
                            new TagValue(Constants.TagDueDate, duedate.ToLongDateString()),
                            new TagValue(Constants.TagDelayDueDate, (delayDuedate != DateTime.MaxValue ? delayDuedate : duedate).ToLongDateString()),
                            Constants.TagBlueButton(WebstudioNotifyPatternResource.ButtonRequestCallButton, "http://www.onlyoffice.com/call-back-form.aspx"),
                            Constants.TagGreenButton(greenButtonText(), greenButtonUrl),
                            Constants.TagTableTop(),
                            Constants.TagTableItem(1, tableItemText1(), tableItemUrl1, tableItemImg1, tableItemComment1(), tableItemLearnMoreText1(), tableItemLearnMoreUrl1),
                            Constants.TagTableItem(2, tableItemText2(), tableItemUrl2, tableItemImg2, tableItemComment2(), tableItemLearnMoreText2(), tableItemLearnMoreUrl2),
                            Constants.TagTableItem(3, tableItemText3(), tableItemUrl3, tableItemImg3, tableItemComment3(), tableItemLearnMoreText3(), tableItemLearnMoreUrl3),
                            Constants.TagTableItem(4, tableItemText4(), tableItemUrl4, tableItemImg4, tableItemComment4(), tableItemLearnMoreText4(), tableItemLearnMoreUrl4),
                            Constants.TagTableItem(5, tableItemText5(), tableItemUrl5, tableItemImg5, tableItemComment5(), tableItemLearnMoreText5(), tableItemLearnMoreUrl5),
                            Constants.TagTableItem(6, tableItemText6(), tableItemUrl6, tableItemImg6, tableItemComment6(), tableItemLearnMoreText6(), tableItemLearnMoreUrl6),
                            Constants.TagTableItem(7, tableItemText7(), tableItemUrl7, tableItemImg7, tableItemComment7(), tableItemLearnMoreText7(), tableItemLearnMoreUrl7),
                            Constants.TagTableBottom(),
                            new TagValue(CommonTags.Footer, string.IsNullOrEmpty(tenant.PartnerId) ? footer : string.Empty));
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendTariffEnterpriseLetters");
        }

        public void SendHostedTariffLetters(DateTime scheduleDate)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var now = scheduleDate.Date;

            log.Info("Start SendHostedTariffLetters");

            var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;

            var activeTenants = CoreContext.TenantManager.GetTenants();

            if (activeTenants.Count <= 0)
            {
                log.Info("End SendHostedTariffLetters");
                return;
            }

            foreach (var tenant in activeTenants)
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant.TenantId);

                    var tariff = CoreContext.PaymentManager.GetTariff(tenant.TenantId);
                    var quota = CoreContext.TenantManager.GetTenantQuota(tenant.TenantId);

                    var duedate = tariff.DueDate.Date;
                    var delayDuedate = tariff.DelayDueDate.Date;

                    INotifyAction action = null;

                    var toadmins = false;
                    var tousers = false;
                    var toguests = false;

                    var footer = "common";

                    Func<string> greenButtonText = () => string.Empty;
                    var greenButtonUrl = string.Empty;

                    Func<string> tableItemText1 = () => string.Empty;
                    Func<string> tableItemText2 = () => string.Empty;
                    Func<string> tableItemText3 = () => string.Empty;
                    Func<string> tableItemText4 = () => string.Empty;
                    Func<string> tableItemText5 = () => string.Empty;

                    var tableItemUrl1 = string.Empty;
                    var tableItemUrl2 = string.Empty;
                    var tableItemUrl3 = string.Empty;
                    var tableItemUrl4 = string.Empty;
                    var tableItemUrl5 = string.Empty;

                    var tableItemImg1 = string.Empty;
                    var tableItemImg2 = string.Empty;
                    var tableItemImg3 = string.Empty;
                    var tableItemImg4 = string.Empty;
                    var tableItemImg5 = string.Empty;

                    Func<string> tableItemComment1 = () => string.Empty;
                    Func<string> tableItemComment2 = () => string.Empty;
                    Func<string> tableItemComment3 = () => string.Empty;
                    Func<string> tableItemComment4 = () => string.Empty;
                    Func<string> tableItemComment5 = () => string.Empty;

                    Func<string> tableItemLearnMoreText1 = () => string.Empty;
                    Func<string> tableItemLearnMoreText2 = () => string.Empty;
                    Func<string> tableItemLearnMoreText3 = () => string.Empty;
                    Func<string> tableItemLearnMoreText4 = () => string.Empty;
                    Func<string> tableItemLearnMoreText5 = () => string.Empty;

                    var tableItemLearnMoreUrl1 = string.Empty;
                    var tableItemLearnMoreUrl2 = string.Empty;
                    var tableItemLearnMoreUrl3 = string.Empty;
                    var tableItemLearnMoreUrl4 = string.Empty;
                    var tableItemLearnMoreUrl5 = string.Empty;


                    #region After registration letters

                    #region 3 days after registration to admins + only 1 user + defaultRebranding

                    if (tenant.CreatedDateTime.Date.AddDays(3) == now && CoreContext.UserManager.GetUsers().Count() == 1 && defaultRebranding)
                    {
                        action = Constants.ActionHostedAdminInviteTeammates;
                        toadmins = true;

                        greenButtonText = () => WebstudioNotifyPatternResource.ButtonInviteRightNow;
                        greenButtonUrl = String.Format("{0}/products/people/", CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'));
                    }

                    #endregion

                    #endregion
                    

                    if (action == null) continue;

                    var users = GetRecipients(toadmins, tousers, toguests);

                    foreach (var u in users)
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        var rquota = TenantExtra.GetRightQuota() ?? TenantQuota.Default;

                        client.SendNoticeToAsync(
                            action,
                            null,
                            new[] { ToRecipient(u.ID) },
                            new[] { EMailSenderName },
                            null,
                            new TagValue(Constants.TagUserName, u.DisplayUserName()),
                            new TagValue(Constants.TagPricingPage, CommonLinkUtility.GetFullAbsolutePath("~/tariffs.aspx")),
                            new TagValue(Constants.TagActiveUsers, CoreContext.UserManager.GetUsers().Count()),
                            new TagValue(Constants.TagPrice, rquota.Price), //TODO: use price partner
                            new TagValue(Constants.TagPricePeriod, rquota.Year3 ? UserControlsCommonResource.TariffPerYear3 : rquota.Year ? UserControlsCommonResource.TariffPerYear : UserControlsCommonResource.TariffPerMonth),
                            new TagValue(Constants.TagDueDate, duedate.ToLongDateString()),
                            new TagValue(Constants.TagDelayDueDate, (delayDuedate != DateTime.MaxValue ? delayDuedate : duedate).ToLongDateString()),
                            Constants.TagBlueButton(WebstudioNotifyPatternResource.ButtonRequestCallButton, "http://www.onlyoffice.com/call-back-form.aspx"),
                            Constants.TagGreenButton(greenButtonText(), greenButtonUrl),
                            Constants.TagTableTop(),
                            Constants.TagTableItem(1, tableItemText1(), tableItemUrl1, tableItemImg1, tableItemComment1(), tableItemLearnMoreText1(), tableItemLearnMoreUrl1),
                            Constants.TagTableItem(2, tableItemText2(), tableItemUrl2, tableItemImg2, tableItemComment2(), tableItemLearnMoreText2(), tableItemLearnMoreUrl2),
                            Constants.TagTableItem(3, tableItemText3(), tableItemUrl3, tableItemImg3, tableItemComment3(), tableItemLearnMoreText3(), tableItemLearnMoreUrl3),
                            Constants.TagTableItem(4, tableItemText4(), tableItemUrl4, tableItemImg4, tableItemComment4(), tableItemLearnMoreText4(), tableItemLearnMoreUrl4),
                            Constants.TagTableItem(5, tableItemText5(), tableItemUrl5, tableItemImg5, tableItemComment5(), tableItemLearnMoreText5(), tableItemLearnMoreUrl5),
                            Constants.TagTableBottom(),
                            new TagValue(CommonTags.Footer, string.IsNullOrEmpty(tenant.PartnerId) ? footer : string.Empty));
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendHostedTariffLetters");
        }

        public void SendOpensourceTariffLetters(DateTime scheduleDate)
        {
            var log = LogManager.GetLogger("ASC.Notify");
            var now = scheduleDate.Date;

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

                    INotifyAction action = null;

                    Func<string> greenButtonText = () => string.Empty;
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


                    #region After registration letters

                    #region 7 days after registration to admins

                        if (tenant.CreatedDateTime.Date.AddDays(7) == now)
                        {
                            action = Constants.ActionOpensourceAdminSecurityTips;

                            greenButtonText = () => WebstudioNotifyPatternResource.ButtonStartFreeTrial;
                            greenButtonUrl = "https://www.onlyoffice.com/enterprise-edition-free.aspx";
                        }

                    #endregion

                    #region 3 weeks after registration to admins

                        else if (tenant.CreatedDateTime.Date.AddDays(21) == now)
                        {
                            action = Constants.ActionOpensourceAdminDocsTips;

                            tableItemImg1 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-01-100.png";
                            tableItemComment1 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips1;
                            tableItemLearnMoreUrl1 = "https://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/CollaborativeEditing.aspx";
                            tableItemLearnMoreText1 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg2 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-02-100.png";
                            tableItemComment2 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips2;
                            tableItemLearnMoreUrl2 = "http://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/UsageInstructions/ViewDocInfo.aspx";
                            tableItemLearnMoreText2 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg3 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-07-100.png";
                            tableItemComment3 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips3;
                            tableItemLearnMoreUrl3 = "https://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/HelpfulHints/Review.aspx";
                            tableItemLearnMoreText3 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg4 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-03-100.png";
                            tableItemComment4 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips4;
                            tableItemLearnMoreUrl4 = "https://helpcenter.onlyoffice.com/gettingstarted/documents.aspx#SharingDocuments_block";
                            tableItemLearnMoreText4 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg5 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-04-100.png";
                            tableItemComment5 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips5;
                            tableItemLearnMoreUrl5 = "http://helpcenter.onlyoffice.com/ONLYOFFICE-Editors/ONLYOFFICE-Document-Editor/UsageInstructions/UseMailMerge.aspx";
                            tableItemLearnMoreText5 = () => WebstudioNotifyPatternResource.LinkLearnMore;

                            tableItemImg6 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-08-100.png";
                            tableItemComment6 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips6;
                            tableItemLearnMoreUrl6 = "http://www.onlyoffice.com/desktop.aspx";
                            tableItemLearnMoreText6 = () => WebstudioNotifyPatternResource.ButtonDownloadNow;

                            tableItemImg7 = "http://cdn.teamlab.com/media/newsletters/images/tips-documents-05-100.png";
                            tableItemComment7 = () => WebstudioNotifyPatternResource.ItemOpensourceDocsTips7;
                            tableItemLearnMoreUrl7 = "https://itunes.apple.com/us/app/onlyoffice-documents/id944896972";
                            tableItemLearnMoreText7 = () => WebstudioNotifyPatternResource.ButtonGoToAppStore;
                        }

                        #endregion

                    #endregion


                    if (action == null) continue;

                    var users = GetRecipients(true, false, false);

                    foreach (var u in users)
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        client.SendNoticeToAsync(
                            action,
                            null,
                            new[] { ToRecipient(u.ID) },
                            new[] { EMailSenderName },
                            null,
                            new TagValue(Constants.TagUserName, u.DisplayUserName()),
                            Constants.TagGreenButton(greenButtonText(), greenButtonUrl),
                            Constants.TagTableTop(),
                            Constants.TagTableItem(1, tableItemText1(), tableItemUrl1, tableItemImg1, tableItemComment1(), tableItemLearnMoreText1(), tableItemLearnMoreUrl1),
                            Constants.TagTableItem(2, tableItemText2(), tableItemUrl2, tableItemImg2, tableItemComment2(), tableItemLearnMoreText2(), tableItemLearnMoreUrl2),
                            Constants.TagTableItem(3, tableItemText3(), tableItemUrl3, tableItemImg3, tableItemComment3(), tableItemLearnMoreText3(), tableItemLearnMoreUrl3),
                            Constants.TagTableItem(4, tableItemText4(), tableItemUrl4, tableItemImg4, tableItemComment4(), tableItemLearnMoreText4(), tableItemLearnMoreUrl4),
                            Constants.TagTableItem(5, tableItemText5(), tableItemUrl5, tableItemImg5, tableItemComment5(), tableItemLearnMoreText5(), tableItemLearnMoreUrl5),
                            Constants.TagTableItem(6, tableItemText6(), tableItemUrl6, tableItemImg6, tableItemComment6(), tableItemLearnMoreText6(), tableItemLearnMoreUrl6),
                            Constants.TagTableItem(7, tableItemText7(), tableItemUrl7, tableItemImg7, tableItemComment7(), tableItemLearnMoreText7(), tableItemLearnMoreUrl7),
                            Constants.TagTableBottom(),
                            new TagValue(CommonTags.Footer, "opensource"),
                            Constants.UnsubscribeLink);
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }

            log.Info("End SendOpensourceTariffLetters");
        }


        private IEnumerable<UserInfo> GetRecipients(bool toadmins, bool tousers, bool toguests)
        {
            IEnumerable<UserInfo> users = new List<UserInfo>();

            if (toadmins && tousers && toguests)
            {
                users = CoreContext.UserManager.GetUsers();
            }
            else if (toadmins && !tousers && !toguests)
            {
                users = CoreContext.UserManager.GetUsersByGroup(ASC.Core.Users.Constants.GroupAdmin.ID);
            }
            else if (!toadmins && tousers && !toguests)
            {
                users = CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.User)
                    .Where(u => !CoreContext.UserManager.IsUserInGroup(u.ID, ASC.Core.Users.Constants.GroupAdmin.ID))
                    .ToArray();
            }
            else if (!toadmins && !tousers && toguests)
            {
                users = CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.Visitor);
            }
            else if (toadmins && tousers && !toguests)
            {
                users = CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.User);
            }
            else if (!toadmins && tousers && toguests)
            {
                users = CoreContext.UserManager.GetUsers()
                    .Where(u => !CoreContext.UserManager.IsUserInGroup(u.ID, ASC.Core.Users.Constants.GroupAdmin.ID))
                    .ToArray();
            }
            else if (toadmins && !tousers && toguests)
            {
                var admins = CoreContext.UserManager.GetUsersByGroup(ASC.Core.Users.Constants.GroupAdmin.ID);
                var guests = CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.Visitor);

                users = admins.Concat(guests);
            }

            return users;
        }


        #region Personal

        public void SendLettersPersonal(DateTime scheduleDate)
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

                    foreach (var user in users)
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

                        switch (dayAfterRegister)
                        {
                            case 7:
                                action = Constants.ActionPersonalAfterRegistration7;
                                break;
                            case 14:
                                action = Constants.ActionPersonalAfterRegistration14;
                                break;
                            case 21:
                                action = Constants.ActionPersonalAfterRegistration21;
                                break;
                            case 28:
                                action = Constants.ActionPersonalAfterRegistration28;
                                greenButtonText = () => WebstudioNotifyPatternResource.ButtonStartFreeTrial;
                                greenButtonUrl = "https://www.onlyoffice.com/enterprise-edition-free.aspx";
                                break;
                            default:
                                continue;

                        }

                        log.InfoFormat(@"Send letter personal '{1}'  to {0} culture {2}. tenant id: {3} user culture {4} create on {5} now date {6}",
                              user.Email, action.Name, culture, tenant.TenantId, user.GetCulture(), user.CreateDate, scheduleDate.Date);

                        sendCount++;

                        client.SendNoticeToAsync(
                          action,
                          null,
                          RecipientFromEmail(new[] { user.Email.ToLower() }, true),
                          new[] { EMailSenderName },
                          null,
                          Constants.TagPersonalHeaderStart,
                          Constants.TagPersonalHeaderEnd,
                          Constants.TagGreenButton(greenButtonText(), greenButtonUrl),
                          new TagValue(CommonTags.Footer, "personal"),
                          new TagValue(CommonTags.IsPersonal, "true"));
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

        public void SendInvitePersonal(string email, string additionalMember = "")
        {
            var newUserInfo = CoreContext.UserManager.GetUserByEmail(email);
            if (CoreContext.UserManager.UserExists(newUserInfo.ID)) return;

            var confirmUrl = CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmpInvite, (int)EmployeeType.User)
                             + "&emplType=" + (int)EmployeeType.User
                             + "&lang=" + Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
                             + additionalMember;

            client.SendNoticeToAsync(
                Constants.ActionPersonalConfirmation,
                null,
                RecipientFromEmail(new[] { email }, false),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, confirmUrl),
                new TagValue(CommonTags.Footer, "personal"),
                new TagValue(CommonTags.IsPersonal, "true"),
                Constants.UnsubscribeLink,
                new TagValue(CommonTags.Culture, Thread.CurrentThread.CurrentUICulture.Name));
        }

        public void SendUserWelcomePersonal(UserInfo newUserInfo)
        {
            client.SendNoticeToAsync(
                Constants.ActionPersonalAfterRegistration1,
                null,
                RecipientFromEmail(new[] { newUserInfo.Email.ToLower() }, true),
                new[] { EMailSenderName },
                null,
                new TagValue(Constants.TagInviteLink, GenerateActivationConfirmUrl(newUserInfo)),
                new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                new TagValue(CommonTags.Footer, "personal"),
                new TagValue(CommonTags.IsPersonal, "true"),
                new TagValue(CommonTags.MasterTemplate, "HtmlMasterPersonal"),
                Constants.UnsubscribeLink,
                CreateSendFromTag());
        }

        #endregion

        #region Migration Portal

        public void MigrationPortalStart(string region, bool notify)
        {
            MigrationNotify(Constants.ActionMigrationPortalStart, region, string.Empty, notify);
        }

        public void MigrationPortalSuccess(string region, string url, bool notify)
        {
            MigrationNotify(Constants.ActionMigrationPortalSuccess, region, url, notify);
        }

        public void MigrationPortalError(string region, string url, bool notify)
        {
            MigrationNotify(!string.IsNullOrEmpty(region) ? Constants.ActionMigrationPortalError : Constants.ActionMigrationPortalServerFailure, region, url, notify);
        }

        private void MigrationNotify(INotifyAction action, string region, string url, bool notify)
        {
            var users = CoreContext.UserManager.GetUsers()
                .Where(u => notify ? u.ActivationStatus == EmployeeActivationStatus.Activated : u.IsOwner())
                .Select(u => ToRecipient(u.ID));

            if (users.Any())
            {
                client.SendNoticeToAsync(
                    action,
                    null,
                    users.ToArray(),
                    new[] { EMailSenderName },
                    null,
                    new TagValue(Constants.TagRegionName, TransferResourceHelper.GetRegionDescription(region)),
                    new TagValue(Constants.TagPortalUrl, url));
            }
        }

        public void PortalRenameNotify(String oldVirtualRootPath)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            var users = CoreContext.UserManager.GetUsers()
                        .Where(u => u.ActivationStatus == EmployeeActivationStatus.Activated);


            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);

                    foreach (var u in users)
                    {
                        var culture = string.IsNullOrEmpty(u.CultureName) ? tenant.GetCulture() : u.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        client.SendNoticeToAsync(
                            Constants.ActionPortalRename,
                            null,
                            new[] { ToRecipient(u.ID) },
                            new[] { EMailSenderName },
                            null,
                            new TagValue(Constants.TagPortalUrl, oldVirtualRootPath),
                            new TagValue(Constants.TagUserDisplayName, u.DisplayUserName()));
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ASC.Notify").Error(ex);
                }
                finally
                {

                }
            });
        }

        #endregion

        #region Helpers

        private IRecipient ToRecipient(Guid userID)
        {
            return source.GetRecipientsProvider().GetRecipient(userID.ToString());
        }

        private IDirectRecipient[] RecipientFromEmail(string[] emails, bool checkActivation)
        {
            return (emails ?? new string[0])
                .Select(e => new DirectRecipient(e, null, new[] { e }, checkActivation))
                .ToArray();
        }

        private static TagValue CreateSendFromTag()
        {
            return new TagValue(CommonTags.SendFrom,
                SecurityContext.IsAuthenticated && SecurityContext.CurrentAccount is IUserAccount ?
                    DisplayUserSettings.GetFullUserName(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID), false).Replace(">", "&#62").Replace("<", "&#60") :
                    CoreContext.TenantManager.GetCurrentTenant().Name);
        }

        private string GetMyStaffLink()
        {
            return CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetMyStaff());
        }

        private string GetUserProfileLink(Guid userId)
        {
            return CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(userId));
        }

        private string AddHttpToUrl(string url)
        {
            var httpPrefix = Uri.UriSchemeHttp + Uri.SchemeDelimiter;
            return !string.IsNullOrEmpty(url) && !url.StartsWith(httpPrefix) ? httpPrefix + url : url;
        }

        private static string GenerateActivationConfirmUrl(UserInfo user)
        {
            var confirmUrl = CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.Activation);

            return confirmUrl + String.Format("&uid={0}&firstname={1}&lastname={2}",
                                              SecurityContext.CurrentAccount.ID,
                                              HttpUtility.UrlEncode(user.FirstName),
                                              HttpUtility.UrlEncode(user.LastName));
        }

        #endregion
    }
}