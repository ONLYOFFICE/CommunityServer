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
                    {
                        MessageAction.PortalAccessSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PortalAccessSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.DocumentServiceLocationSetting, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "DocumentServiceLocationSetting",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.AuthorizationKeysSetting, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "AuthorizationKeysSetting",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.FullTextSearchSetting, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "FullTextSearchSetting",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.StartTransferSetting, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "StartTransferSetting",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.StartBackupSetting, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "StartBackupSetting",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.LicenseKeyUploaded, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "LicenseKeyUploaded",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                };
        }
    }
}