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
    internal class SettingsActionsMapper
    {
        public static Dictionary<MessageAction, MessageMaps> GetMaps()
        {
            return new Dictionary<MessageAction, MessageMaps>
                {
                    {
                        MessageAction.LanguageSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "LanguageSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                    {
                        MessageAction.TimeZoneSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TimeZoneSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                    {
                        MessageAction.DnsSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "DnsSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                    {
                        MessageAction.TrustedMailDomainSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TrustedMailDomainSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                    {
                        MessageAction.PasswordStrengthSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PasswordStrengthSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                    {
                        MessageAction.TwoFactorAuthenticationSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TwoFactorAuthenticationSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                    {
                        MessageAction.AdministratorMessageSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "AdministratorMessageSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                    {
                        MessageAction.DefaultStartPageSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "DefaultStartPageSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                    {
                        MessageAction.ProductsListUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ProductsListUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.OwnerUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "OwnerChanged",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.AdministratorAdded, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "AdministratorAdded",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.UsersOpenedProductAccess, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "ProductAccessOpenedForUsers",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.GroupsOpenedProductAccess, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "ProductAccessOpenedForGroups",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.ProductAccessOpened, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "ProductAccessOpened",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.ProductAccessRestricted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "ProductAccessRestricted",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.ProductAddedAdministrator, new MessageMaps
                            {
                                ActionTypeTextResourceName = "CreateActionType",
                                ActionTextResourceName = "ProductAddedAdministrator",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.ProductDeletedAdministrator, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "ProductDeletedAdministrator",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.AdministratorDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "AdministratorDeleted",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.AdministratorOpenedFullAccess, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateAccessActionType",
                                ActionTextResourceName = "AdministratorOpenedFullAccess",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.GreetingSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "GreetingSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.TeamTemplateChanged, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TeamTemplateChanged",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.ColorThemeChanged, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "ColorThemeChanged",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.OwnerSentPortalDeactivationInstructions, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "OwnerSentPortalDeactivationInstructions",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.OwnerSentPortalDeleteInstructions, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "OwnerSentPortalDeleteInstructions",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.LoginHistoryReportDownloaded, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DownloadActionType",
                                ActionTextResourceName = "LoginHistoryReportDownloaded",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.AuditTrailReportDownloaded, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DownloadActionType",
                                ActionTextResourceName = "AuditTrailReportDownloaded",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.PortalDeactivated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PortalDeactivated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.PortalDeleted, new MessageMaps
                            {
                                ActionTypeTextResourceName = "DeleteActionType",
                                ActionTextResourceName = "PortalDeleted",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.OwnerSentChangeOwnerInstructions, new MessageMaps
                            {
                                ActionTypeTextResourceName = "SendActionType",
                                ActionTextResourceName = "OwnerSentChangeOwnerInstructions",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.SSOEnabled, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "SSOEnabled",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.SSODisabled, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "SSODisabled",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                };
        }
    }
}