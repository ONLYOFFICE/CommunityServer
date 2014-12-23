/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
                    Constants.ActionPasswordChanged,

                    Constants.ActionYouAddedAfterInvite,
                    Constants.ActionYouAddedLikeGuest,

                    Constants.ActionSelfProfileUpdated,
                    Constants.ActionSendPassword,
                    Constants.ActionInviteUsers,
                    Constants.ActionJoinUsers,
                    Constants.ActionSendWhatsNew,
                    Constants.ActionUserHasJoin,
                    Constants.ActionBackupCreated,
                    Constants.ActionPortalDeactivate,
                    Constants.ActionPortalDelete,
                    Constants.ActionDnsChange,
                    Constants.ActionConfirmOwnerChange,
                    Constants.ActionActivateUsers,
                    Constants.ActionActivateGuests,
                    Constants.ActionEmailChange,
                    Constants.ActionPasswordChange,
                    Constants.ActionActivateEmail,
                    Constants.ActionProfileDelete,
                    Constants.ActionPhoneChange,
                    Constants.ActionMigrationPortalStart,
                    Constants.ActionMigrationPortalSuccess,
                    Constants.ActionMigrationPortalError,
                    Constants.ActionMigrationPortalServerFailure,

                    Constants.ActionUserMessageToAdmin,
                    Constants.ActionCongratulations,

                    Constants.ActionSmsBalance,
                    Constants.ActionVoipWarning,
                    Constants.ActionVoipBlocked
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
                if (Constants.ActionSelfProfileUpdated.ID == action.ID ||
                    Constants.ActionUserHasJoin.ID == action.ID ||
                    Constants.ActionUserMessageToAdmin.ID == action.ID ||
                    Constants.ActionSmsBalance.ID == action.ID ||
                    Constants.ActionVoipWarning.ID == action.ID ||
                    Constants.ActionVoipBlocked.ID == action.ID
                    )
                {
                    return Constants.ActionAdminNotify;
                }
                else
                {
                    return action;
                }
            }
        }
    }
}
