/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Core.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;

namespace ASC.Web.Studio.Core.Notify
{
    class StudioNotifySource : NotifySource
    {
        public StudioNotifySource()
            : base("asc.web.studio")
        {
        }


        protected override IActionProvider CreateActionProvider()
        {
            return new ConstActionProvider(
                    Actions.SelfProfileUpdated,
                    Actions.JoinUsers,
                    Actions.SendWhatsNew,
                    Actions.UserHasJoin,
                    Actions.BackupCreated,
                    Actions.PortalDeactivate,
                    Actions.PortalDelete,
                    Actions.PortalDeleteSuccessV10,
                    Actions.DnsChange,
                    Actions.ConfirmOwnerChange,
                    Actions.EmailChange,
                    Actions.PasswordChange,
                    Actions.ActivateEmail,
                    Actions.ProfileDelete,
                    Actions.ProfileHasDeletedItself,
                    Actions.ReassignsCompleted,
                    Actions.ReassignsFailed,
                    Actions.RemoveUserDataCompleted,
                    Actions.RemoveUserDataCompletedCustomMode,
                    Actions.RemoveUserDataFailed,
                    Actions.PhoneChange,
                    Actions.MigrationPortalStart,
                    Actions.MigrationPortalSuccess,
                    Actions.MigrationPortalError,
                    Actions.MigrationPortalServerFailure,

                    Actions.UserMessageToAdmin,

                    Actions.VoipWarning,
                    Actions.VoipBlocked,

                    Actions.SaasAdminActivationV10,
                    Actions.EnterpriseAdminActivationV10,
                    Actions.EnterpriseWhitelabelAdminActivationV10,
                    Actions.OpensourceAdminActivation,

                    Actions.SaasAdminWelcomeV10,
                    Actions.EnterpriseAdminWelcomeV10,
                    Actions.EnterpriseWhitelabelAdminWelcomeV10,

                    Actions.SaasUserActivationV10,
                    Actions.EnterpriseUserActivationV10,
                    Actions.EnterpriseWhitelabelUserActivationV10,

                    Actions.SaasUserWelcomeV10,
                    Actions.EnterpriseUserWelcomeV10,
                    Actions.EnterpriseWhitelabelUserWelcomeV10,
                    Actions.EnterpriseWhitelabelUserWelcomeCustomMode,

                    Actions.SaasGuestActivationV10,
                    Actions.EnterpriseGuestActivationV10,
                    Actions.EnterpriseWhitelabelGuestActivationV10,

                    Actions.SaasGuestWelcomeV10,
                    Actions.EnterpriseGuestWelcomeV10,
                    Actions.EnterpriseWhitelabelGuestWelcomeV10,

                    Actions.EnterpriseAdminCustomizePortalV10,
                    Actions.EnterpriseWhitelabelAdminCustomizePortalV10,
                    Actions.EnterpriseAdminInviteTeammatesV10,
                    Actions.EnterpriseAdminWithoutActivityV10,
                    Actions.EnterpriseAdminUserDocsTipsV10,
                    Actions.EnterpriseAdminUserAppsTipsV10,

                    Actions.EnterpriseAdminTrialWarningBefore7V10,
                    Actions.EnterpriseAdminTrialWarningV10,

                    Actions.EnterpriseAdminPaymentWarningBefore7V10,
                    Actions.EnterpriseWhitelabelAdminPaymentWarningBefore7V10,
                    Actions.EnterpriseAdminPaymentWarningV10,
                    Actions.EnterpriseWhitelabelAdminPaymentWarningV10,

                    Actions.SaasAdminInviteTeammatesV10,
                    Actions.SaasAdminWithoutActivityV10,
                    Actions.SaasAdminUserDocsTipsV10,
                    Actions.SaasAdminUserComfortTipsV10,
                    Actions.SaasAdminUserAppsTipsV10,

                    Actions.SaasAdminTrialWarningBefore5V10,
                    Actions.SaasAdminTrialWarningV10,
                    Actions.SaasAdminTrialWarningAfter5V10,
                    Actions.SaasAdminTrialWarningAfter30V10,
                    Actions.SaasAdminTrialWarningAfterHalfYearV10,

                    Actions.SaasAdminPaymentWarningBefore5V10,
                    Actions.SaasAdminPaymentWarningAfter1V10,

                    Actions.SaasAdminPaymentAfterMonthlySubscriptionsV10,

                    Actions.OpensourceAdminSecurityTips,
                    Actions.OpensourceAdminDocsTips,

                    Actions.PersonalActivate,
                    Actions.PersonalAfterRegistration1,
                    Actions.PersonalAfterRegistration7,
                    Actions.PersonalAfterRegistration14,
                    Actions.PersonalAfterRegistration21,
                    Actions.PersonalAfterRegistration28,
                    Actions.PersonalConfirmation,
                    Actions.PersonalPasswordChange,
                    Actions.PersonalEmailChange,
                    Actions.PersonalProfileDelete,

                    Actions.MailboxCreated,
                    Actions.MailboxWithoutSettingsCreated,

                    Actions.MailboxCreated,
                    Actions.MailboxWithoutSettingsCreated,

                    Actions.PersonalCustomModeAfterRegistration1,
                    Actions.PersonalCustomModeAfterRegistration7,
                    Actions.PersonalCustomModeConfirmation,
                    Actions.PersonalCustomModePasswordChange,
                    Actions.PersonalCustomModeEmailChange,
                    Actions.PersonalCustomModeProfileDelete
                );
        }

        protected override IPatternProvider CreatePatternsProvider()
        {
            return new XmlPatternProvider2(WebPatternResource.webstudio_patterns);
        }

        protected override ISubscriptionProvider CreateSubscriptionProvider()
        {
            return new AdminNotifySubscriptionProvider(base.CreateSubscriptionProvider());
        }


        private class AdminNotifySubscriptionProvider : ISubscriptionProvider
        {
            private readonly ISubscriptionProvider provider;


            public AdminNotifySubscriptionProvider(ISubscriptionProvider provider)
            {
                this.provider = provider;
            }

            public object GetSubscriptionRecord(INotifyAction action, IRecipient recipient, string objectID)
            {
                return provider.GetSubscriptionRecord(GetAdminAction(action), recipient, objectID);
            }

            public string[] GetSubscriptions(INotifyAction action, IRecipient recipient, bool checkSubscription = true)
            {
                return provider.GetSubscriptions(GetAdminAction(action), recipient, checkSubscription);
            }

            public void Subscribe(INotifyAction action, string objectID, IRecipient recipient)
            {
                provider.Subscribe(GetAdminAction(action), objectID, recipient);
            }

            public void UnSubscribe(INotifyAction action, IRecipient recipient)
            {
                provider.UnSubscribe(GetAdminAction(action), recipient);
            }

            public void UnSubscribe(INotifyAction action)
            {
                provider.UnSubscribe(GetAdminAction(action));
            }

            public void UnSubscribe(INotifyAction action, string objectID)
            {
                provider.UnSubscribe(GetAdminAction(action), objectID);
            }

            public void UnSubscribe(INotifyAction action, string objectID, IRecipient recipient)
            {
                provider.UnSubscribe(GetAdminAction(action), objectID, recipient);
            }

            public void UpdateSubscriptionMethod(INotifyAction action, IRecipient recipient, params string[] senderNames)
            {
                provider.UpdateSubscriptionMethod(GetAdminAction(action), recipient, senderNames);
            }

            public IRecipient[] GetRecipients(INotifyAction action, string objectID)
            {
                return provider.GetRecipients(GetAdminAction(action), objectID);
            }

            public string[] GetSubscriptionMethod(INotifyAction action, IRecipient recipient)
            {
                return provider.GetSubscriptionMethod(GetAdminAction(action), recipient);
            }

            public bool IsUnsubscribe(IDirectRecipient recipient, INotifyAction action, string objectID)
            {
                return provider.IsUnsubscribe(recipient, action, objectID);
            }

            private INotifyAction GetAdminAction(INotifyAction action)
            {
                if (Actions.SelfProfileUpdated.ID == action.ID ||
                    Actions.UserHasJoin.ID == action.ID ||
                    Actions.UserMessageToAdmin.ID == action.ID ||
                    Actions.VoipWarning.ID == action.ID ||
                    Actions.VoipBlocked.ID == action.ID
                    )
                {
                    return Actions.AdminNotify;
                }
                else
                {
                    return action;
                }
            }
        }
    }
}
