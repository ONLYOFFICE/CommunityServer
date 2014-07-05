/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
                };
        }
    }
}