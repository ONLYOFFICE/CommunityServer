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


using System.Collections.Generic;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    internal class PeopleActionMapper
    {
        public static Dictionary<MessageAction, MessageMaps> GetMaps()
        {
            return new Dictionary<MessageAction, MessageMaps>
                {
                    {
                        MessageAction.UserCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "UserCreated",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.GuestCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "GuestCreated",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserCreatedViaInvite, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "UserCreatedViaInvite",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.GuestCreatedViaInvite, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "GuestCreatedViaInvite",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserActivated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UserActivated",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.GuestActivated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "GuestActivated",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UserUpdated",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },                    
                    {
                        MessageAction.UserUpdatedMobileNumber, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UserUpdatedMobileNumber",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserUpdatedLanguage, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UserUpdatedLanguage",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserAddedAvatar, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UserAddedAvatar",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserDeletedAvatar, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "UserDeletedAvatar",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserUpdatedAvatarThumbnails, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UserUpdatedAvatarThumbnails",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserLinkedSocialAccount, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "UserLinkedSocialAccount",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserUnlinkedSocialAccount, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UnlinkActionType",
                                ActionTextResourceName = "UserUnlinkedSocialAccount",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserSentActivationInstructions, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "UserSentActivationInstructions",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserSentEmailChangeInstructions, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "UserSentEmailInstructions",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserSentPasswordChangeInstructions, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "UserSentPasswordInstructions",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserSentDeleteInstructions, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "UserSentDeleteInstructions",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserUpdatedEmail, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UserUpdatedEmail",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserUpdatedPassword, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UserUpdatedPassword",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "UserDeleted",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UsersUpdatedType, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UsersUpdatedType",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UsersUpdatedStatus, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "UsersUpdatedStatus",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UsersSentActivationInstructions, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "UsersSentActivationInstructions",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UsersDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "UsersDeleted",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.SentInviteInstructions, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "SentInviteInstructions",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserImported, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ImportActionType",
                                ActionTextResourceName = "UserImported",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.GuestImported, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ImportActionType",
                                ActionTextResourceName = "GuestImported",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.GroupCreated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "GroupCreated",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "GroupsModule"
                            }
                    },
                    {
                        MessageAction.GroupUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "GroupUpdated",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "GroupsModule"
                            }
                    },
                    {
                        MessageAction.GroupDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "GroupDeleted",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "GroupsModule"
                            }
                    },
                    {
                        MessageAction.UserDataReassigns, new MessageMaps
                            {
                                ActionTypeTextResourceName = "ReassignsActionType",
                                ActionTextResourceName = "UserDataReassigns",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserDataRemoving, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "UserDataRemoving",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserConnectedTfaApp, new MessageMaps
                            {
                                ActionTypeTextResourceName = "LinkActionType",
                                ActionTextResourceName = "UserTfaGenerateCodes",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    },
                    {
                        MessageAction.UserDisconnectedTfaApp, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "UserTfaDisconnected",
                                ProductResourceName = "PeopleProduct",
                                ModuleResourceName = "UsersModule"
                            }
                    }
                };
        }
    }
}