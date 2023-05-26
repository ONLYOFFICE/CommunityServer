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
                    Actions.RestoreStarted,
                    Actions.RestoreCompletedV115,
                    Actions.PortalDeactivate,
                    Actions.PortalDelete,
                    Actions.PortalDeleteSuccessV115,
                    Actions.DnsChange,
                    Actions.ConfirmOwnerChange,
                    Actions.EmailChangeV115,
                    Actions.PasswordChangeV115,
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
                    Actions.MigrationPortalSuccessV115,
                    Actions.MigrationPortalError,
                    Actions.MigrationPortalServerFailure,

                    Actions.UserMessageToAdmin,

                    Actions.VoipWarning,
                    Actions.VoipBlocked,

                    Actions.SaasAdminActivationV115,
                    Actions.EnterpriseAdminActivationV10,
                    Actions.EnterpriseWhitelabelAdminActivationV10,
                    Actions.OpensourceAdminActivationV11,

                    Actions.SaasAdminWelcomeV115,
                    Actions.EnterpriseAdminWelcomeV10,
                    Actions.EnterpriseWhitelabelAdminWelcomeV10,
                    Actions.OpensourceAdminWelcomeV11,

                    Actions.SaasUserActivationV115,
                    Actions.EnterpriseUserActivationV10,
                    Actions.EnterpriseWhitelabelUserActivationV10,
                    Actions.OpensourceUserActivationV11,

                    Actions.SaasUserWelcomeV115,
                    Actions.EnterpriseUserWelcomeV10,
                    Actions.EnterpriseWhitelabelUserWelcomeV10,
                    Actions.EnterpriseWhitelabelUserWelcomeCustomMode,
                    Actions.OpensourceUserWelcomeV11,

                    Actions.SaasGuestActivationV115,
                    Actions.EnterpriseGuestActivationV10,
                    Actions.EnterpriseWhitelabelGuestActivationV10,
                    Actions.OpensourceGuestActivationV11,

                    Actions.SaasGuestWelcomeV115,
                    Actions.EnterpriseGuestWelcomeV10,
                    Actions.EnterpriseWhitelabelGuestWelcomeV10,
                    Actions.OpensourceGuestWelcomeV11,

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

                    Actions.SaasAdminUserDocsTipsV115,
                    Actions.SaasAdminComfortTipsV115,
                    Actions.SaasAdminUserAppsTipsV115,

                    Actions.SaasAdminTrialWarningBefore5V115,
                    Actions.SaasAdminTrialWarningV115,
                    Actions.SaasAdminTrialWarningAfter1V115,
                    Actions.SaasAdminTrialWarningAfterHalfYearV115,

                    Actions.SaasAdminPaymentWarningEvery2MonthsV115,

                    Actions.SaasAdminModulesV115,

                    Actions.OpensourceAdminDocsTipsV11,
                    Actions.OpensourceUserDocsTipsV11,

                    Actions.PersonalActivate,
                    Actions.PersonalAfterRegistration1,
                    Actions.PersonalAfterRegistration7,
                    Actions.PersonalAfterRegistration14,
                    Actions.PersonalAfterRegistration21,
                    Actions.PersonalAfterRegistration28,
                    Actions.PersonalConfirmation,
                    Actions.PersonalPasswordChangeV115,
                    Actions.PersonalEmailChangeV115,
                    Actions.PersonalProfileDelete,

                    Actions.MailboxCreated,
                    Actions.MailboxWithoutSettingsCreated,

                    Actions.MailboxCreated,
                    Actions.MailboxWithoutSettingsCreated,

                    Actions.PersonalCustomModeAfterRegistration1,
                    Actions.PersonalCustomModeConfirmation,
                    Actions.PersonalCustomModePasswordChangeV115,
                    Actions.PersonalCustomModeEmailChangeV115,
                    Actions.PersonalCustomModeProfileDelete,

                    Actions.SaasCustomModeRegData,

                    Actions.StorageEncryptionStart,
                    Actions.StorageEncryptionSuccess,
                    Actions.StorageEncryptionError,
                    Actions.StorageDecryptionStart,
                    Actions.StorageDecryptionSuccess,
                    Actions.StorageDecryptionError
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
