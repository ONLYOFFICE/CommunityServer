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
    public class SettingsActionsMapper : IProductActionMapper
    {
        public List<IModuleActionMapper> Mappers { get; }
        public ProductType Product { get; }

        public SettingsActionsMapper()
        {
            Product = ProductType.Settings;

            Mappers = new List<IModuleActionMapper>()
            {
                new GeneralActionMapper(),
                new ProductsActionMapper()
            };
        }
    }

    public class GeneralActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public GeneralActionMapper()
        {
            Module = ModuleType.General;
            var productType = ProductType.Settings;
            Actions = new MessageMapsDictionary(productType, Module)
            {
                {
                    ActionType.Update, new  MessageAction[]
                    {
                        MessageAction.LanguageSettingsUpdated, MessageAction.TimeZoneSettingsUpdated, MessageAction.DnsSettingsUpdated,
                        MessageAction.TrustedMailDomainSettingsUpdated,MessageAction.PasswordStrengthSettingsUpdated,MessageAction.TwoFactorAuthenticationSettingsUpdated,
                        MessageAction.AdministratorMessageSettingsUpdated,MessageAction.DefaultStartPageSettingsUpdated,
                    }
                }
            };

            Actions.Add(MessageAction.TwoFactorAuthenticationDisabled, new MessageMaps("TwoFactorAuthenticationSettingsDisabled", ActionType.Update, productType, Module));
            Actions.Add(MessageAction.TwoFactorAuthenticationEnabledBySms, new MessageMaps("TwoFactorAuthenticationSettingsEnabledBySms", ActionType.Update, productType, Module));
            Actions.Add(MessageAction.TwoFactorAuthenticationEnabledByTfaApp, new MessageMaps("TwoFactorAuthenticationSettingsEnabledByTfaApp", ActionType.Update, productType, Module));
        }
    }

    public class ProductsActionMapper : IModuleActionMapper
    {
        public ModuleType Module { get; }
        public IDictionary<MessageAction, MessageMaps> Actions { get; }

        public ProductsActionMapper()
        {
            Module = ModuleType.Products;
            var productType = ProductType.Projects;

            Actions = new MessageMapsDictionary(ProductType.Projects, Module)
            {
                {
                    ActionType.Update, new  MessageAction[]
                    {
                        MessageAction.ProductsListUpdated,
                        MessageAction.GreetingSettingsUpdated,MessageAction.TeamTemplateChanged,MessageAction.ColorThemeChanged,
                        MessageAction.OwnerSentPortalDeactivationInstructions, MessageAction.PortalDeactivated,
                        MessageAction.SSOEnabled,MessageAction.SSODisabled,MessageAction.PortalAccessSettingsUpdated,
                        MessageAction.DocumentServiceLocationSetting, MessageAction.AuthorizationKeysSetting,
                        MessageAction.FullTextSearchSetting, MessageAction.StartTransferSetting,
                        MessageAction.StartBackupSetting,MessageAction.LicenseKeyUploaded, MessageAction.StartStorageEncryption,
                        MessageAction.StartStorageDecryption, MessageAction.CookieSettingsUpdated,  MessageAction.MailServiceSettingsUpdated,
                        MessageAction.CustomNavigationSettingsUpdated,MessageAction.AuditSettingsUpdated,MessageAction.PrivacyRoomEnable,
                        MessageAction.PrivacyRoomDisable,
                    }
                },
                {
                    ActionType.Create, new  MessageAction[]
                    {
                        MessageAction.AdministratorAdded, MessageAction.ProductAddedAdministrator,
                    }
                },
                {
                    ActionType.UpdateAccess, new  MessageAction[]
                    {
                        MessageAction.ProductAccessOpened,MessageAction.ProductAccessRestricted,MessageAction.AdministratorDeleted, MessageAction.AdministratorOpenedFullAccess
                    }
                },
                {
                    ActionType.Delete, new  MessageAction[]
                    {
                        MessageAction.ProductDeletedAdministrator,MessageAction.PortalDeleted,
                    }
                },
                {
                    ActionType.Send, new  MessageAction[]
                    {
                        MessageAction.OwnerSentPortalDeleteInstructions, MessageAction.OwnerSentChangeOwnerInstructions,
                    }
                },
                {
                    ActionType.Download, new  MessageAction[]
                    {
                        MessageAction.LoginHistoryReportDownloaded, MessageAction.AuditTrailReportDownloaded
                    }
                },
            };

            Actions.Add(MessageAction.UsersOpenedProductAccess, new MessageMaps("ProductAccessOpenedForUsers", ActionType.UpdateAccess, productType, Module));
            Actions.Add(MessageAction.GroupsOpenedProductAccess, new MessageMaps("ProductAccessOpenedForGroups", ActionType.UpdateAccess, productType, Module));
            Actions.Add(MessageAction.OwnerUpdated, new MessageMaps("OwnerChanged", ActionType.Update, productType, Module));
        }
    }
}