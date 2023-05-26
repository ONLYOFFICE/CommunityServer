/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Notify
{
    public class StudioNotifyService
    {
        private readonly INotifyClient client;
        private ILog Log = LogManager.GetLogger("ASC.Notify");

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
            client = WorkContext.NotifyContext.NotifyService.RegisterClient(StudioNotifyHelper.NotifySource);
        }


        #region Periodic Notify

        public void RegisterSendMethod()
        {
            var cron = ConfigurationManagerExtension.AppSettings["core.notify.cron"] ?? "0 0 5 ? * *"; // 5am every day

            if (ConfigurationManagerExtension.AppSettings["core.notify.tariff"] != "false")
            {
                if (TenantExtra.Enterprise)
                {
                    client.RegisterSendMethod(SendEnterpriseTariffLetters, cron);
                }
                else if (TenantExtra.Opensource)
                {
                    client.RegisterSendMethod(SendOpensourceTariffLetters, cron);
                }
                else if (TenantExtra.Saas)
                {
                    if (CoreContext.Configuration.Personal)
                    {
                        if (!CoreContext.Configuration.CustomMode)
                        {
                            client.RegisterSendMethod(SendLettersPersonal, cron);
                        }
                    }
                    else
                    {
                        client.RegisterSendMethod(SendSaasTariffLetters, cron);
                    }
                }
            }

            if (!CoreContext.Configuration.Personal)
            {
                client.RegisterSendMethod(SendMsgWhatsNew, "0 0 * ? * *"); // every hour
            }
        }

        public void SendSaasTariffLetters(DateTime scheduleDate)
        {
            StudioPeriodicNotify.SendSaasLetters(client, EMailSenderName, scheduleDate);
        }

        public void SendEnterpriseTariffLetters(DateTime scheduleDate)
        {
            StudioPeriodicNotify.SendEnterpriseLetters(client, EMailSenderName, scheduleDate);
        }

        public void SendOpensourceTariffLetters(DateTime scheduleDate)
        {
            StudioPeriodicNotify.SendOpensourceLetters(client, EMailSenderName, scheduleDate);
        }

        public void SendLettersPersonal(DateTime scheduleDate)
        {
            StudioPeriodicNotify.SendPersonalLetters(client, EMailSenderName, scheduleDate);
        }

        public void SendMsgWhatsNew(DateTime scheduleDate)
        {
            StudioWhatsNewNotify.SendMsgWhatsNew(scheduleDate, client);
        }

        #endregion


        public void SendMsgToAdminAboutProfileUpdated()
        {
            client.SendNoticeAsync(Actions.SelfProfileUpdated, null);
        }

        public void SendMsgToAdminFromNotAuthUser(string email, string message)
        {
            client.SendNoticeAsync(Actions.UserMessageToAdmin, null,
                new TagValue(Tags.Body, message), new TagValue(Tags.UserEmail, email));
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
            if (string.IsNullOrEmpty(site) && !CoreContext.Configuration.CustomMode) throw new ArgumentNullException("site");
            message = (message ?? "").Trim();
            if (string.IsNullOrEmpty(message) && !CoreContext.Configuration.CustomMode) throw new ArgumentNullException("message");

            var salesEmail = AdditionalWhiteLabelSettings.Instance.SalesEmail ?? SetupInfo.SalesEmail;

            var recipient = (IRecipient)(new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), String.Empty, new[] { salesEmail }, false));

            client.SendNoticeToAsync(license ? Actions.RequestLicense : Actions.RequestTariff,
                                     null,
                                     new[] { recipient },
                                     new[] { "email.sender" },
                                     new TagValue(Tags.UserName, fname),
                                     new TagValue(Tags.UserLastName, lname),
                                     new TagValue(Tags.UserPosition, title),
                                     new TagValue(Tags.UserEmail, email),
                                     new TagValue(Tags.Phone, phone),
                                     new TagValue(Tags.Website, site),
                                     new TagValue(Tags.CompanyTitle, ctitle),
                                     new TagValue(Tags.CompanySize, csize),
                                     new TagValue(Tags.Body, message));
        }

        #region Voip

        public void SendToAdminVoipWarning(double balance)
        {
            client.SendNoticeAsync(Actions.VoipWarning, null,
                new TagValue(Tags.Body, balance));
        }

        public void SendToAdminVoipBlocked()
        {
            client.SendNoticeAsync(Actions.VoipBlocked, null);
        }



        #endregion
        #region User Password

        public void UserPasswordChange(UserInfo userInfo)
        {
            var auditEventDate = DateTime.UtcNow;

            String hash = auditEventDate.ToString("s");

            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(userInfo.Email, ConfirmType.PasswordChange, hash);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonChangePassword;

            var action = CoreContext.Configuration.Personal
                             ? (CoreContext.Configuration.CustomMode ? Actions.PersonalCustomModeEmailChangeV115 : Actions.PersonalPasswordChangeV115)
                             : Actions.PasswordChangeV115;

            client.SendNoticeToAsync(
                        action,
                        null,
                        StudioNotifyHelper.RecipientFromEmail(userInfo.Email, false),
                        new[] { EMailSenderName },
                        TagValues.GreenButton(greenButtonText, confirmationUrl));

            var displayUserName = userInfo.DisplayUserName(false);

            MessageService.Send(HttpContext.Current.Request, auditEventDate, MessageAction.UserSentPasswordChangeInstructions, MessageTarget.Create(userInfo.ID), displayUserName);
        }

        #endregion

        #region User Email

        public void SendEmailChangeInstructions(UserInfo user, string email)
        {
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmailChange, SecurityContext.CurrentAccount.ID);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonChangeEmail;

            var action = CoreContext.Configuration.Personal
                             ? (CoreContext.Configuration.CustomMode ? Actions.PersonalCustomModeEmailChangeV115 : Actions.PersonalEmailChangeV115)
                             : Actions.EmailChangeV115;

            client.SendNoticeToAsync(
                        action,
                        null,
                        StudioNotifyHelper.RecipientFromEmail(email, false),
                        new[] { EMailSenderName },
                        TagValues.GreenButton(greenButtonText, confirmationUrl),
                        new TagValue(CommonTags.Culture, user.GetCulture().Name));
        }

        public void SendEmailActivationInstructions(UserInfo user, string email)
        {
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmailActivation);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonActivateEmail;

            client.SendNoticeToAsync(
                        Actions.ActivateEmail,
                        null,
                        StudioNotifyHelper.RecipientFromEmail(email, false),
                        new[] { EMailSenderName },
                        new TagValue(Tags.InviteLink, confirmationUrl),
                        TagValues.GreenButton(greenButtonText, confirmationUrl),
                        new TagValue(Tags.UserDisplayName, (user.DisplayUserName() ?? string.Empty).Trim()));
        }

        #endregion

        #region MailServer

        public void SendMailboxCreated(List<string> toEmails, string username, string address)
        {
            SendMailboxCreated(toEmails, username, address, null, null, -1, -1, null);
        }

        public void SendMailboxCreated(List<string> toEmails, string username, string address, string server,
            string encyption, int portImap, int portSmtp, string login, bool skipSettings = false)
        {
            var tags = new List<ITagValue>
            {
                new TagValue(Tags.UserName, username ?? string.Empty),
                new TagValue(Tags.Address, address ?? string.Empty)
            };

            if (!skipSettings)
            {
                var link = string.Format("{0}/addons/mail/#accounts/changepwd={1}",
                    CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/'), address);

                tags.Add(new TagValue(Tags.MyStaffLink, link));
                tags.Add(new TagValue(Tags.Server, server));
                tags.Add(new TagValue(Tags.Encryption, encyption ?? string.Empty));
                tags.Add(new TagValue(Tags.ImapPort, portImap.ToString(CultureInfo.InvariantCulture)));
                tags.Add(new TagValue(Tags.SmtpPort, portSmtp.ToString(CultureInfo.InvariantCulture)));
                tags.Add(new TagValue(Tags.Login, login));
            }

            client.SendNoticeToAsync(
                skipSettings
                    ? Actions.MailboxWithoutSettingsCreated
                    : Actions.MailboxCreated,
                null,
                StudioNotifyHelper.RecipientFromEmail(toEmails, false),
                new[] { EMailSenderName },
                tags.ToArray());
        }

        public void SendMailboxPasswordChanged(List<string> toEmails, string username, string address)
        {
            client.SendNoticeToAsync(
                Actions.MailboxPasswordChanged,
                null,
                StudioNotifyHelper.RecipientFromEmail(toEmails, false),
                new[] { EMailSenderName },
                new TagValue(Tags.UserName, username ?? string.Empty),
                new TagValue(Tags.Address, address ?? string.Empty));
        }

        #endregion

        public void SendMsgMobilePhoneChange(UserInfo userInfo)
        {
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(userInfo.Email.ToLower(), ConfirmType.PhoneActivation);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonChangePhone;

            client.SendNoticeToAsync(
                Actions.PhoneChange,
                null,
                StudioNotifyHelper.RecipientFromEmail(userInfo.Email, false),
                new[] { EMailSenderName },
                TagValues.GreenButton(greenButtonText, confirmationUrl));
        }

        public void SendMsgTfaReset(UserInfo userInfo)
        {
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(userInfo.Email.ToLower(), ConfirmType.TfaActivation);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonChangeTfa;

            client.SendNoticeToAsync(
                Actions.TfaChange,
                null,
                StudioNotifyHelper.RecipientFromEmail(userInfo.Email, false),
                new[] { EMailSenderName },
                TagValues.GreenButton(greenButtonText, confirmationUrl));
        }


        public void UserHasJoin()
        {
            if (!CoreContext.Configuration.Personal)
            {
                client.SendNoticeAsync(Actions.UserHasJoin, null);
            }
        }

        public void SendJoinMsg(string email, EmployeeType emplType)
        {
            var inviteUrl = CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmpInvite, (int)emplType)
                            + String.Format("&emplType={0}", (int)emplType);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonJoin;

            client.SendNoticeToAsync(
                        Actions.JoinUsers,
                        null,
                        StudioNotifyHelper.RecipientFromEmail(email, true),
                        new[] { EMailSenderName },
                        new TagValue(Tags.InviteLink, inviteUrl),
                        TagValues.GreenButton(greenButtonText, inviteUrl));
        }

        public void UserInfoAddedAfterInvite(UserInfo newUserInfo)
        {
            if (!CoreContext.UserManager.UserExists(newUserInfo.ID)) return;

            INotifyAction notifyAction;
            var footer = "social";

            if (CoreContext.Configuration.Personal)
            {
                if (CoreContext.Configuration.CustomMode)
                {
                    notifyAction = Actions.PersonalCustomModeAfterRegistration1;
                    footer = "personalCustomMode";
                }
                else
                {
                    notifyAction = Actions.PersonalAfterRegistration1;
                    footer = "personal";
                }
            }
            else if (TenantExtra.Enterprise)
            {
                var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;
                notifyAction = defaultRebranding
                                   ? Actions.EnterpriseUserWelcomeV10
                                   : CoreContext.Configuration.CustomMode
                                         ? Actions.EnterpriseWhitelabelUserWelcomeCustomMode
                                         : Actions.EnterpriseWhitelabelUserWelcomeV10;
                footer = null;
            }
            else if (TenantExtra.Opensource)
            {
                notifyAction = Actions.OpensourceUserWelcomeV11;
                footer = "opensource";
            }
            else
            {
                notifyAction = Actions.SaasUserWelcomeV115;
            }

            Func<string> greenButtonText = () => TenantExtra.Enterprise
                                      ? WebstudioNotifyPatternResource.ButtonAccessYourPortal
                                      : WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;

            client.SendNoticeToAsync(
                notifyAction,
                null,
                StudioNotifyHelper.RecipientFromEmail(newUserInfo.Email, false),
                new[] { EMailSenderName },
                new TagValue(Tags.UserName, newUserInfo.FirstName.HtmlEncode()),
                new TagValue(Tags.MyStaffLink, GetMyStaffLink()),
                TagValues.GreenButton(greenButtonText, CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/')),
                new TagValue(CommonTags.Footer, footer),
                new TagValue(CommonTags.MasterTemplate, CoreContext.Configuration.Personal ? "HtmlMasterPersonal" : "HtmlMaster"));
        }

        public void GuestInfoAddedAfterInvite(UserInfo newUserInfo)
        {
            if (!CoreContext.UserManager.UserExists(newUserInfo.ID)) return;

            INotifyAction notifyAction;
            var footer = "social";

            if (TenantExtra.Enterprise)
            {
                var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;
                notifyAction = defaultRebranding ? Actions.EnterpriseGuestWelcomeV10 : Actions.EnterpriseWhitelabelGuestWelcomeV10;
                footer = null;
            }
            else if (TenantExtra.Opensource)
            {
                notifyAction = Actions.OpensourceGuestWelcomeV11;
                footer = "opensource";
            }
            else
            {
                notifyAction = Actions.SaasGuestWelcomeV115;
            }

            Func<string> greenButtonText = () => TenantExtra.Enterprise
                                      ? WebstudioNotifyPatternResource.ButtonAccessYourPortal
                                      : WebstudioNotifyPatternResource.ButtonAccessYouWebOffice;

            client.SendNoticeToAsync(
                notifyAction,
                null,
                StudioNotifyHelper.RecipientFromEmail(newUserInfo.Email, false),
                new[] { EMailSenderName },
                new TagValue(Tags.UserName, newUserInfo.FirstName.HtmlEncode()),
                new TagValue(Tags.MyStaffLink, GetMyStaffLink()),
                TagValues.GreenButton(greenButtonText, CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/')),
                new TagValue(CommonTags.Footer, footer));
        }

        public void UserInfoActivation(UserInfo newUserInfo)
        {
            if (newUserInfo.IsActive)
                throw new ArgumentException("User is already activated!");

            INotifyAction notifyAction;
            var footer = "social";

            if (TenantExtra.Enterprise)
            {
                var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;
                notifyAction = defaultRebranding ? Actions.EnterpriseUserActivationV10 : Actions.EnterpriseWhitelabelUserActivationV10;
                footer = null;
            }
            else if (TenantExtra.Opensource)
            {
                notifyAction = Actions.OpensourceUserActivationV11;
                footer = "opensource";
            }
            else
            {
                notifyAction = Actions.SaasUserActivationV115;
            }

            var confirmationUrl = GenerateActivationConfirmUrl(newUserInfo);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccept;

            client.SendNoticeToAsync(
                notifyAction,
                null,
                StudioNotifyHelper.RecipientFromEmail(newUserInfo.Email, false),
                new[] { EMailSenderName },
                new TagValue(Tags.ActivateUrl, confirmationUrl),
                TagValues.GreenButton(greenButtonText, confirmationUrl),
                new TagValue(Tags.UserName, newUserInfo.FirstName.HtmlEncode()),
                new TagValue(CommonTags.Footer, footer));
        }

        public void GuestInfoActivation(UserInfo newUserInfo)
        {
            if (newUserInfo.IsActive)
                throw new ArgumentException("User is already activated!");

            INotifyAction notifyAction;
            var footer = "social";

            if (TenantExtra.Enterprise)
            {
                var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;
                notifyAction = defaultRebranding ? Actions.EnterpriseGuestActivationV10 : Actions.EnterpriseWhitelabelGuestActivationV10;
                footer = null;
            }
            else if (TenantExtra.Opensource)
            {
                notifyAction = Actions.OpensourceGuestActivationV11;
                footer = "opensource";
            }
            else
            {
                notifyAction = Actions.SaasGuestActivationV115;
            }

            var confirmationUrl = GenerateActivationConfirmUrl(newUserInfo);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonAccept;

            client.SendNoticeToAsync(
                notifyAction,
                null,
                StudioNotifyHelper.RecipientFromEmail(newUserInfo.Email, false),
                new[] { EMailSenderName },
                new TagValue(Tags.ActivateUrl, confirmationUrl),
                TagValues.GreenButton(greenButtonText, confirmationUrl),
                new TagValue(Tags.UserName, newUserInfo.FirstName.HtmlEncode()),
                new TagValue(CommonTags.Footer, footer));
        }

        public void SendMsgProfileDeletion(UserInfo user)
        {
            var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.ProfileRemove);

            Func<string> greenButtonText = () => CoreContext.Configuration.Personal ? WebstudioNotifyPatternResource.ButtonConfirmTermination : WebstudioNotifyPatternResource.ButtonRemoveProfile;

            var action = CoreContext.Configuration.Personal
                             ? (CoreContext.Configuration.CustomMode ? Actions.PersonalCustomModeProfileDelete : Actions.PersonalProfileDelete)
                             : Actions.ProfileDelete;

            client.SendNoticeToAsync(
                action,
                null,
                StudioNotifyHelper.RecipientFromEmail(user.Email, false),
                new[] { EMailSenderName },
                TagValues.GreenButton(greenButtonText, confirmationUrl),
                new TagValue(CommonTags.Culture, user.GetCulture().Name));
        }

        public void SendMsgProfileHasDeletedItself(UserInfo user)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var admins = CoreContext.UserManager.GetUsers()
                        .Where(u => WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, u.ID));

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);

                    foreach (var admin in admins)
                    {
                        var culture = string.IsNullOrEmpty(admin.CultureName) ? tenant.GetCulture() : admin.GetCulture();
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;

                        client.SendNoticeToAsync(
                            Actions.ProfileHasDeletedItself,
                            null,
                            new IRecipient[] { admin },
                            new[] { EMailSenderName },
                            new TagValue(Tags.FromUserName, user.DisplayUserName()),
                            new TagValue(Tags.FromUserLink, GetUserProfileLink(user.ID)));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });

        }

        public void SendMsgReassignsCompleted(Guid recipientId, UserInfo fromUser, UserInfo toUser)
        {
            client.SendNoticeToAsync(
                Actions.ReassignsCompleted,
                null,
                new[] { StudioNotifyHelper.ToRecipient(recipientId) },
                new[] { EMailSenderName },
                new TagValue(Tags.UserName, DisplayUserSettings.GetFullUserName(recipientId)),
                new TagValue(Tags.FromUserName, fromUser.DisplayUserName()),
                new TagValue(Tags.FromUserLink, GetUserProfileLink(fromUser.ID)),
                new TagValue(Tags.ToUserName, toUser.DisplayUserName()),
                new TagValue(Tags.ToUserLink, GetUserProfileLink(toUser.ID)));
        }

        public void SendMsgReassignsFailed(Guid recipientId, UserInfo fromUser, UserInfo toUser, string message)
        {
            client.SendNoticeToAsync(
                Actions.ReassignsFailed,
                null,
                new[] { StudioNotifyHelper.ToRecipient(recipientId) },
                new[] { EMailSenderName },
                new TagValue(Tags.UserName, DisplayUserSettings.GetFullUserName(recipientId)),
                new TagValue(Tags.FromUserName, fromUser.DisplayUserName()),
                new TagValue(Tags.FromUserLink, GetUserProfileLink(fromUser.ID)),
                new TagValue(Tags.ToUserName, toUser.DisplayUserName()),
                new TagValue(Tags.ToUserLink, GetUserProfileLink(toUser.ID)),
                new TagValue(Tags.Message, message));
        }

        public void SendMsgRemoveUserDataCompleted(Guid recipientId, Guid fromUserId, string fromUserName, long docsSpace, long crmSpace, long mailSpace, long talkSpace)
        {
            client.SendNoticeToAsync(
                CoreContext.Configuration.CustomMode ? Actions.RemoveUserDataCompletedCustomMode : Actions.RemoveUserDataCompleted,
                null,
                new[] { StudioNotifyHelper.ToRecipient(recipientId) },
                new[] { EMailSenderName },
                new TagValue(Tags.UserName, DisplayUserSettings.GetFullUserName(recipientId)),
                new TagValue(Tags.FromUserName, fromUserName.HtmlEncode()),
                new TagValue(Tags.FromUserLink, GetUserProfileLink(fromUserId)),
                new TagValue("DocsSpace", FileSizeComment.FilesSizeToString(docsSpace)),
                new TagValue("CrmSpace", FileSizeComment.FilesSizeToString(crmSpace)),
                new TagValue("MailSpace", FileSizeComment.FilesSizeToString(mailSpace)),
                new TagValue("TalkSpace", FileSizeComment.FilesSizeToString(talkSpace)));
        }

        public void SendMsgRemoveUserDataFailed(Guid recipientId, Guid fromUserId, string fromUserName, string message)
        {
            client.SendNoticeToAsync(
                Actions.RemoveUserDataFailed,
                null,
                new[] { StudioNotifyHelper.ToRecipient(recipientId) },
                new[] { EMailSenderName },
                new TagValue(Tags.UserName, DisplayUserSettings.GetFullUserName(recipientId)),
                new TagValue(Tags.FromUserName, fromUserName.HtmlEncode()),
                new TagValue(Tags.FromUserLink, GetUserProfileLink(fromUserId)),
                new TagValue(Tags.Message, message));
        }

        public void SendAdminWelcome(UserInfo newUserInfo)
        {
            if (!CoreContext.UserManager.UserExists(newUserInfo.ID)) return;

            if (!newUserInfo.IsActive)
                throw new ArgumentException("User is not activated yet!");

            INotifyAction notifyAction;
            var tagValues = new List<ITagValue>();

            if (TenantExtra.Enterprise)
            {
                var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;
                notifyAction = defaultRebranding ? Actions.EnterpriseAdminWelcomeV10 : Actions.EnterpriseWhitelabelAdminWelcomeV10;

                tagValues.Add(TagValues.GreenButton(() => WebstudioNotifyPatternResource.ButtonAccessControlPanel, CommonLinkUtility.GetFullAbsolutePath(SetupInfo.ControlPanelUrl)));
                tagValues.Add(new TagValue(Tags.ControlPanelUrl, CommonLinkUtility.GetFullAbsolutePath(SetupInfo.ControlPanelUrl).TrimEnd('/')));
            }
            else if (TenantExtra.Opensource)
            {
                notifyAction = Actions.OpensourceAdminWelcomeV11;
                tagValues.Add(new TagValue(CommonTags.Footer, "opensource"));
                tagValues.Add(new TagValue(Tags.ControlPanelUrl, CommonLinkUtility.GetFullAbsolutePath(SetupInfo.ControlPanelUrl).TrimEnd('/')));
            }
            else
            {
                notifyAction = Actions.SaasAdminWelcomeV115;
                //tagValues.Add(TagValues.GreenButton(() => WebstudioNotifyPatternResource.ButtonConfigureRightNow, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetAdministration(ManagementType.General))));
                tagValues.Add(new TagValue(CommonTags.Footer, "common"));
                tagValues.Add(new TagValue(Tags.PricingPage, CommonLinkUtility.GetFullAbsolutePath("~/Tariffs.aspx")));
            }

            tagValues.Add(new TagValue(Tags.UserName, newUserInfo.FirstName.HtmlEncode()));

            client.SendNoticeToAsync(
                notifyAction,
                null,
                StudioNotifyHelper.RecipientFromEmail(newUserInfo.Email, false),
                new[] { EMailSenderName },
                tagValues.ToArray());
        }

        #region Backup & Restore

        public void SendMsgBackupCompleted(Guid userId, string link)
        {
            client.SendNoticeToAsync(
                Actions.BackupCreated,
                null,
                new[] { StudioNotifyHelper.ToRecipient(userId) },
                new[] { EMailSenderName },
                new TagValue(Tags.OwnerName, CoreContext.UserManager.GetUsers(userId).DisplayUserName()));
        }

        public void SendMsgRestoreStarted(bool notifyAllUsers)
        {
            var owner = CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant().OwnerId);
            var users =
                notifyAllUsers
                    ? StudioNotifyHelper.RecipientFromEmail(CoreContext.UserManager.GetUsers(EmployeeStatus.Active).Where(r => r.ActivationStatus == EmployeeActivationStatus.Activated).Select(u => u.Email).ToList(), false)
                    : owner.ActivationStatus == EmployeeActivationStatus.Activated ? StudioNotifyHelper.RecipientFromEmail(owner.Email, false) : new IDirectRecipient[0];

            client.SendNoticeToAsync(
                Actions.RestoreStarted,
                null,
                users,
                new[] { EMailSenderName });
        }

        public void SendMsgRestoreCompleted(bool notifyAllUsers)
        {
            var users = notifyAllUsers
                    ? CoreContext.UserManager.GetUsers(EmployeeStatus.Active)
                    : new[] { CoreContext.UserManager.GetUsers(CoreContext.TenantManager.GetCurrentTenant().OwnerId) };

            foreach (var user in users)
            {
                var hash = CoreContext.Authentication.GetUserPasswordStamp(user.ID).ToString("s");
                var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.PasswordChange, hash);

                Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonSetPassword;

                client.SendNoticeToAsync(
                    Actions.RestoreCompletedV115,
                    null,
                    new IRecipient[] { user },
                    new[] { EMailSenderName },
                    TagValues.GreenButton(greenButtonText, confirmationUrl));
            }
        }

        #endregion

        #region Portal Deactivation & Deletion

        public void SendMsgPortalDeactivation(Tenant t, string deactivateUrl, string activateUrl)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonDeactivatePortal;

            client.SendNoticeToAsync(
                        Actions.PortalDeactivate,
                        null,
                        new IRecipient[] { u },
                        new[] { EMailSenderName },
                        new TagValue(Tags.ActivateUrl, activateUrl),
                        TagValues.GreenButton(greenButtonText, deactivateUrl),
                        new TagValue(Tags.OwnerName, u.DisplayUserName()));
        }

        public void SendMsgPortalDeletion(Tenant t, string url, bool showAutoRenewText)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonDeletePortal;

            client.SendNoticeToAsync(
                        Actions.PortalDelete,
                        null,
                        new IRecipient[] { u },
                        new[] { EMailSenderName },
                        TagValues.GreenButton(greenButtonText, url),
                        new TagValue(Tags.AutoRenew, showAutoRenewText.ToString()),
                        new TagValue(Tags.OwnerName, u.DisplayUserName()));
        }

        public void SendMsgPortalDeletionSuccess(UserInfo owner, string url)
        {
            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonLeaveFeedback;

            client.SendNoticeToAsync(
                        Actions.PortalDeleteSuccessV115,
                        null,
                        new IRecipient[] { owner },
                        new[] { EMailSenderName },
                        TagValues.GreenButton(greenButtonText, url),
                        new TagValue(Tags.OwnerName, owner.DisplayUserName()));
        }

        #endregion

        public void SendMsgDnsChange(Tenant t, string confirmDnsUpdateUrl, string portalAddress, string portalDns)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);

            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonConfirmPortalAddressChange;

            client.SendNoticeToAsync(
                        Actions.DnsChange,
                        null,
                        new IRecipient[] { u },
                        new[] { EMailSenderName },
                        new TagValue("ConfirmDnsUpdate", confirmDnsUpdateUrl),//TODO: Tag is deprecated and replaced by TagGreenButton
                        TagValues.GreenButton(greenButtonText, confirmDnsUpdateUrl),
                        new TagValue("PortalAddress", AddHttpToUrl(portalAddress)),
                        new TagValue("PortalDns", AddHttpToUrl(portalDns ?? string.Empty)),
                        new TagValue(Tags.OwnerName, u.DisplayUserName()));
        }


        public void SendMsgConfirmChangeOwner(UserInfo owner, UserInfo newOwner, string confirmOwnerUpdateUrl)
        {
            Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonConfirmPortalOwnerUpdate;

            client.SendNoticeToAsync(
                Actions.ConfirmOwnerChange,
                null,
                new IRecipient[] { owner },
                new[] { EMailSenderName },
                TagValues.GreenButton(greenButtonText, confirmOwnerUpdateUrl),
                new TagValue(Tags.UserName, newOwner.DisplayUserName()),
                new TagValue(Tags.OwnerName, owner.DisplayUserName()));
        }


        public void SendCongratulations(UserInfo u)
        {
            try
            {
                INotifyAction notifyAction;
                var footer = "common";

                if (TenantExtra.Enterprise)
                {
                    var defaultRebranding = MailWhiteLabelSettings.Instance.IsDefault;
                    notifyAction = defaultRebranding ? Actions.EnterpriseAdminActivationV10 : Actions.EnterpriseWhitelabelAdminActivationV10;
                    footer = null;
                }
                else if (TenantExtra.Opensource)
                {
                    notifyAction = Actions.OpensourceAdminActivationV11;
                    footer = "opensource";
                }
                else
                {
                    notifyAction = Actions.SaasAdminActivationV115;
                }

                var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(u.Email, ConfirmType.EmailActivation);
                confirmationUrl += "&first=true";

                Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonConfirm;

                client.SendNoticeToAsync(
                    notifyAction,
                    null,
                    StudioNotifyHelper.RecipientFromEmail(u.Email, false),
                    new[] { EMailSenderName },
                    new TagValue(Tags.UserName, u.FirstName.HtmlEncode()),
                    new TagValue(Tags.MyStaffLink, GetMyStaffLink()),
                    new TagValue(Tags.ActivateUrl, confirmationUrl),
                    TagValues.GreenButton(greenButtonText, confirmationUrl),
                    new TagValue(CommonTags.Footer, footer));
            }
            catch (Exception error)
            {
                Log.Error(error);
            }
        }

        #region Personal

        public void SendInvitePersonal(string email, string additionalMember = "")
        {
            var newUserInfo = CoreContext.UserManager.GetUserByEmail(email);
            if (CoreContext.UserManager.UserExists(newUserInfo.ID)) return;

            var lang = CoreContext.Configuration.CustomMode
                           ? "ru-RU"
                           : Thread.CurrentThread.CurrentUICulture.Name;

            var culture = SetupInfo.GetPersonalCulture(lang);

            var confirmUrl = CommonLinkUtility.GetConfirmationUrl(email, ConfirmType.EmpInvite, (int)EmployeeType.User)
                             + "&emplType=" + (int)EmployeeType.User
                             + "&lang=" + culture.Key
                             + additionalMember;

            client.SendNoticeToAsync(
                CoreContext.Configuration.CustomMode ? Actions.PersonalCustomModeConfirmation : Actions.PersonalConfirmation,
                null,
                StudioNotifyHelper.RecipientFromEmail(email, false),
                new[] { EMailSenderName },
                new TagValue(Tags.InviteLink, confirmUrl),
                new TagValue(CommonTags.Footer, CoreContext.Configuration.CustomMode ? "personalCustomMode" : "personal"),
                new TagValue(CommonTags.Culture, Thread.CurrentThread.CurrentUICulture.Name));
        }

        public void SendUserWelcomePersonal(UserInfo newUserInfo)
        {
            client.SendNoticeToAsync(
                CoreContext.Configuration.CustomMode ? Actions.PersonalCustomModeAfterRegistration1 : Actions.PersonalAfterRegistration1,
                null,
                StudioNotifyHelper.RecipientFromEmail(newUserInfo.Email, true),
                new[] { EMailSenderName },
                new TagValue(CommonTags.Footer, CoreContext.Configuration.CustomMode ? "personalCustomMode" : "personal"),
                new TagValue(CommonTags.MasterTemplate, "HtmlMasterPersonal"));
        }

        #endregion

        #region Migration Portal

        public void MigrationPortalStart(string region, bool notify)
        {
            MigrationNotify(Actions.MigrationPortalStart, region, string.Empty, notify);
        }

        public void MigrationPortalSuccess(string region, string url, bool notify, int toTenantId)
        {
            MigrationNotify(Actions.MigrationPortalSuccessV115, region, url, notify, toTenantId);
        }

        public void MigrationPortalError(string region, string url, bool notify)
        {
            MigrationNotify(!string.IsNullOrEmpty(region) ? Actions.MigrationPortalError : Actions.MigrationPortalServerFailure, region, url, notify);
        }

        private void MigrationNotify(INotifyAction action, string region, string url, bool notify, int? toTenantId = null)
        {
            var users = CoreContext.UserManager.GetUsers()
               .Where(u => notify ? u.ActivationStatus.HasFlag(EmployeeActivationStatus.Activated) : u.IsOwner());

            if (users.Any())
            {
                var args = CreateArgs(region, url);
                if (action == Actions.MigrationPortalSuccessV115)
                {
                    foreach (var user in users)
                    {
                        var currentArgs = new List<ITagValue>(args);

                        var newTenantId = toTenantId.HasValue ? toTenantId.Value : CoreContext.TenantManager.GetCurrentTenant().TenantId;
                        var hash = CoreContext.Authentication.GetUserPasswordStamp(user.ID).ToString("s");
                        var confirmationUrl = url + "/" + CommonLinkUtility.GetConfirmationUrlRelative(newTenantId, user.Email, ConfirmType.PasswordChange, hash);

                        Func<string> greenButtonText = () => WebstudioNotifyPatternResource.ButtonSetPassword;
                        currentArgs.Add(TagValues.GreenButton(greenButtonText, confirmationUrl));

                        client.SendNoticeToAsync(
                            action,
                            null,
                            new IRecipient[] { user },
                            new[] { EMailSenderName },
                            currentArgs.ToArray());
                    }
                }
                else
                {
                    client.SendNoticeToAsync(
                        action,
                        null,
                        users.Select(u => StudioNotifyHelper.ToRecipient(u.ID)).ToArray(),
                        new[] { EMailSenderName },
                        args.ToArray());
                }
            }
        }
        private List<ITagValue> CreateArgs(string region, string url)
        {
            var args = new List<ITagValue>()
                    {
                        new TagValue(Tags.RegionName, TransferResourceHelper.GetRegionDescription(region)),
                        new TagValue(Tags.PortalUrl, url)
                    };

            if (!string.IsNullOrEmpty(url))
            {
                args.Add(new TagValue(CommonTags.VirtualRootPath, url));
                args.Add(new TagValue(CommonTags.ProfileUrl, url + CommonLinkUtility.GetMyStaff()));
                args.Add(new TagValue(CommonTags.LetterLogo, TenantLogoManager.GetLogoDark(true)));
            }
            return args;
        }

        public void PortalRenameNotify(String oldVirtualRootPath)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            var users = CoreContext.UserManager.GetUsers()
                        .Where(u => u.ActivationStatus.HasFlag(EmployeeActivationStatus.Activated));

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
                            Actions.PortalRename,
                            null,
                            new[] { StudioNotifyHelper.ToRecipient(u.ID) },
                            new[] { EMailSenderName },
                            new TagValue(Tags.PortalUrl, oldVirtualRootPath),
                            new TagValue(Tags.UserDisplayName, u.DisplayUserName()));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });
        }

        #endregion

        #region Helpers

        private static string GetMyStaffLink()
        {
            return CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetMyStaff());
        }

        private static string GetUserProfileLink(Guid userId)
        {
            return CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(userId));
        }

        private static string AddHttpToUrl(string url)
        {
            var httpPrefix = Uri.UriSchemeHttp + Uri.SchemeDelimiter;
            return !string.IsNullOrEmpty(url) && !url.StartsWith(httpPrefix) ? httpPrefix + url : url;
        }

        private static string GenerateActivationConfirmUrl(UserInfo user)
        {
            var confirmUrl = CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.Activation);

            if (HttpContext.Current == null)
            {
                confirmUrl = CorrectUrlScheme(confirmUrl);
            }

            return confirmUrl + String.Format("&uid={0}&firstname={1}&lastname={2}",
                                              SecurityContext.CurrentAccount.ID,
                                              HttpUtility.UrlEncode(user.FirstName),
                                              HttpUtility.UrlEncode(user.LastName));
        }

        private static string CorrectUrlScheme(string url)
        {
            try
            {
                var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
                var cachedRewriteUrlStr = Common.Caching.AscCache.Default.Get<string>("REWRITE_URL" + tenantId);

                if (string.IsNullOrEmpty(cachedRewriteUrlStr))
                {
                    return url;
                }

                var cachedRewriteUriScheme = new Uri(cachedRewriteUrlStr).Scheme;
                var uriBuilder = new UriBuilder(url);

                if (uriBuilder.Scheme == cachedRewriteUriScheme)
                {
                    return url;
                }

                var hadDefaultPort = uriBuilder.Uri.IsDefaultPort;

                uriBuilder.Scheme = cachedRewriteUriScheme;
                uriBuilder.Port = hadDefaultPort ? -1 : uriBuilder.Port;

                return uriBuilder.ToString();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Notify").Error(ex);
                return url;
            }
        }

        #endregion

        public void SendRegData(UserInfo u)
        {
            try
            {
                if (!TenantExtra.Saas || !CoreContext.Configuration.CustomMode) return;

                var salesEmail = AdditionalWhiteLabelSettings.Instance.SalesEmail ?? SetupInfo.SalesEmail;

                if (string.IsNullOrEmpty(salesEmail)) return;

                var recipient = new DirectRecipient(salesEmail, null, new[] { salesEmail }, false);

                client.SendNoticeToAsync(
                    Actions.SaasCustomModeRegData,
                    null,
                    new IRecipient[] { recipient },
                    new[] { EMailSenderName },
                    new TagValue(Tags.UserName, u.FirstName.HtmlEncode()),
                    new TagValue(Tags.UserLastName, u.LastName.HtmlEncode()),
                    new TagValue(Tags.UserEmail, u.Email.HtmlEncode()),
                    new TagValue(Tags.Phone, u.MobilePhone != null ? u.MobilePhone.HtmlEncode() : "-"),
                    new TagValue(Tags.Date, u.CreateDate.ToShortDateString() + " " + u.CreateDate.ToShortTimeString()),
                    new TagValue(CommonTags.Footer, null),
                    TagValues.WithoutUnsubscribe());
            }
            catch (Exception error)
            {
                Log.Error(error);
            }
        }

        #region Storage encryption

        public void SendStorageEncryptionStart(string serverRootPath)
        {
            SendStorageEncryptionNotify(Actions.StorageEncryptionStart, false, serverRootPath);
        }

        public void SendStorageEncryptionSuccess(string serverRootPath)
        {
            SendStorageEncryptionNotify(Actions.StorageEncryptionSuccess, false, serverRootPath);
        }

        public void SendStorageEncryptionError(string serverRootPath)
        {
            SendStorageEncryptionNotify(Actions.StorageEncryptionError, true, serverRootPath);
        }

        public void SendStorageDecryptionStart(string serverRootPath)
        {
            SendStorageEncryptionNotify(Actions.StorageDecryptionStart, false, serverRootPath);
        }

        public void SendStorageDecryptionSuccess(string serverRootPath)
        {
            SendStorageEncryptionNotify(Actions.StorageDecryptionSuccess, false, serverRootPath);
        }

        public void SendStorageDecryptionError(string serverRootPath)
        {
            SendStorageEncryptionNotify(Actions.StorageDecryptionError, true, serverRootPath);
        }

        private void SendStorageEncryptionNotify(INotifyAction action, bool notifyAdminsOnly, string serverRootPath)
        {
            var users = notifyAdminsOnly
                    ? CoreContext.UserManager.GetUsersByGroup(Constants.GroupAdmin.ID)
                    : CoreContext.UserManager.GetUsers().Where(u => u.ActivationStatus.HasFlag(EmployeeActivationStatus.Activated));

            foreach (var u in users)
            {
                client.SendNoticeToAsync(
                    action,
                    null,
                    new[] { StudioNotifyHelper.ToRecipient(u.ID) },
                    new[] { EMailSenderName },
                    new TagValue(Tags.UserName, u.FirstName.HtmlEncode()),
                    new TagValue(Tags.PortalUrl, serverRootPath),
                    new TagValue(Tags.ControlPanelUrl, GetControlPanelUrl(serverRootPath)));
            }
        }

        private string GetControlPanelUrl(string serverRootPath)
        {
            var controlPanelUrl = SetupInfo.ControlPanelUrl;

            if (string.IsNullOrEmpty(controlPanelUrl))
                return string.Empty;

            if (controlPanelUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) ||
                controlPanelUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                return controlPanelUrl;

            return serverRootPath + "/" + controlPanelUrl.TrimStart('~', '/').TrimEnd('/');
        }

        #endregion
    }
}