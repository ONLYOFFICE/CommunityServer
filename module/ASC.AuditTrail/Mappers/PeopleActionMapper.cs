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


using System.Collections.Generic;

using ASC.AuditTrail.Types;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    public class PeopleActionMapper : IProductActionMapper
    {
        public List<IModuleActionMapper> Mappers { get; }
        public ProductType Product { get; }

        public PeopleActionMapper()
        {
            Product = ProductType.People;

            Mappers = new List<IModuleActionMapper>()
            {
                new UsersActionMapper(),
                new GroupsActionMapper()
            };
        }
    }

    public class UsersActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public UsersActionMapper()
        {
            Module = ModuleType.Users;

            Actions = new MessageMapsDictionary(ProductType.People, Module)
            {
                {
                    EntryType.User, new Dictionary<ActionType, MessageAction[]>()
                    {
                        { ActionType.Create,  new[] { MessageAction.UserCreated, MessageAction.GuestCreated, MessageAction.UserCreatedViaInvite, MessageAction.GuestCreatedViaInvite }  },
                        {
                            ActionType.Update,  new[]
                            {
                                MessageAction.UserActivated, MessageAction.GuestActivated, MessageAction.UserUpdated,
                                MessageAction.UserUpdatedMobileNumber, MessageAction.UserUpdatedLanguage, MessageAction.UserAddedAvatar,
                                MessageAction.UserUpdatedAvatarThumbnails, MessageAction.UserUpdatedEmail, MessageAction.UsersUpdatedType,
                                MessageAction.UsersUpdatedStatus, MessageAction.UsersSentActivationInstructions,
                            }
                        },
                        { ActionType.Delete, new[] { MessageAction.UserDeletedAvatar, MessageAction.UserDeleted, MessageAction.UsersDeleted, MessageAction.UserDataRemoving } },
                        { ActionType.Import, new[] { MessageAction.UserImported, MessageAction.GuestImported } },
                        { ActionType.Logout, new[] { MessageAction.UserLogoutActiveConnections, MessageAction.UserLogoutActiveConnection, MessageAction.UserLogoutActiveConnectionsForUser } },
                    },
                    new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Reassigns, MessageAction.UserDataReassigns }
                    }
                },
                { MessageAction.UserLinkedSocialAccount, ActionType.Link },
                { MessageAction.UserUnlinkedSocialAccount, ActionType.Unlink },
                {
                    ActionType.Send, new[] { MessageAction.UserSentActivationInstructions, MessageAction.UserSentDeleteInstructions, MessageAction.SentInviteInstructions }
                },
                { MessageAction.UserUpdatedPassword, ActionType.Update }
            };

            Actions.Add(MessageAction.UserSentEmailChangeInstructions, new MessageMaps("UserSentEmailInstructions", ActionType.Send, ProductType.People, Module, EntryType.User));
            Actions.Add(MessageAction.UserSentPasswordChangeInstructions, new MessageMaps("UserSentPasswordInstructions", ActionType.Send, ProductType.People, Module, EntryType.User));
            Actions.Add(MessageAction.UserConnectedTfaApp, new MessageMaps("UserTfaGenerateCodes", ActionType.Link, ProductType.People, Module, EntryType.User));
            Actions.Add(MessageAction.UserDisconnectedTfaApp, new MessageMaps("UserTfaDisconnected", ActionType.Delete, ProductType.People, Module, EntryType.User));
        }
    }

    public class GroupsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public GroupsActionMapper()
        {
            Module = ModuleType.Groups;

            Actions = new MessageMapsDictionary(ProductType.People, Module)
            {
                {
                    EntryType.Group, new Dictionary<ActionType, MessageAction>()
                    {
                        { ActionType.Create, MessageAction.GroupCreated },
                        { ActionType.Update, MessageAction.GroupUpdated },
                        { ActionType.Delete, MessageAction.GroupDeleted }
                    }
                }
            };
        }
    }
}