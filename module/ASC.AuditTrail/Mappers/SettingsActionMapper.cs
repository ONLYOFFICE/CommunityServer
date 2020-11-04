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
                        MessageAction.TwoFactorAuthenticationDisabled, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TwoFactorAuthenticationSettingsDisabled",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                                        {
                        MessageAction.TwoFactorAuthenticationEnabledBySms, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TwoFactorAuthenticationSettingsEnabledBySms",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "GeneralModule"
                            }
                    },
                                        {
                        MessageAction.TwoFactorAuthenticationEnabledByTfaApp, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "TwoFactorAuthenticationSettingsEnabledByTfaApp",
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
                    {
                        MessageAction.StartStorageEncryption, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "StartStorageEncryption",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.StartStorageDecryption, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "StartStorageDecryption",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.CookieSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CookieSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.MailServiceSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "MailServiceSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.CustomNavigationSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "CustomNavigationSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.AuditSettingsUpdated, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "AuditSettingsUpdated",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.PrivacyRoomEnable, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PrivacyRoomEnable",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    },
                    {
                        MessageAction.PrivacyRoomDisable, new MessageMaps
                            {
                                ActionTypeTextResourceName = "UpdateActionType",
                                ActionTextResourceName = "PrivacyRoomDisable",
                                ProductResourceName = "SettingsProduct",
                                ModuleResourceName = "ProductsModule"
                            }
                    }
                };
        }
    }
}