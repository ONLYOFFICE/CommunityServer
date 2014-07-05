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

using ASC.MessagingSystem;
using AjaxPro;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Users;
using ASC.SingleSignOn.Common;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using System;
using System.Security;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Management.SingleSignOnSettings
{
    [ManagementControl(ManagementType.SingleSignOnSettings, Location)]
    [AjaxNamespace("SsoSettingsController")]
    public partial class SingleSignOnSettings : UserControl
    {
        protected const string Location = "~/userControls/Management/SingleSignOnSettings/SingleSignOnSettings.ascx";
        protected SsoSettings Settings;
        private static ILog _log = LogManager.GetLogger(typeof(SingleSignOnSettings));

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(SingleSignOnSettings), Page);
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/singlesignonsettings/js/singlesignonsettings.js"));
            Settings = SettingsManager.Instance.LoadSettings<SsoSettings>(TenantProvider.CurrentTenantID);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string SaveSettings(string serializeSettings)
        {
            if (Context.User != null && Context.User.Identity != null)
            {
                var userInfo = CoreContext.UserManager.GetUsers(((IUserAccount)Context.User.Identity).ID);
                if (userInfo != Constants.LostUser && userInfo.IsAdmin())
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(serializeSettings))
                        {
                            _log.ErrorFormat("SSO settings are null or empty.");
                            return Resource.SsoSettingsAreEmpty;
                        }
                        Settings = (SsoSettings)JavaScriptDeserializer.DeserializeFromJson(serializeSettings, typeof(SsoSettings));
                        if (Settings == null)
                        {
                            _log.ErrorFormat("Wrong SSO settings were received from client.");
                            return Resource.SsoSettingsWrongSerialization;
                        }

                        var messageAction = Settings.EnableSso ? MessageAction.SSOEnabled : MessageAction.SSODisabled;
                        if (Settings.EnableSso)
                        {
                            if (!(string.IsNullOrWhiteSpace(Settings.Issuer) || CheckUri(Settings.Issuer)))
                            {
                                _log.ErrorFormat("Wrong Issuer URL: {0}", Settings.Issuer);
                                return string.Format(Resource.SsoSettingsWrongURL, Settings.Issuer);
                            }
                            if (!(string.IsNullOrWhiteSpace(Settings.SsoEndPoint) || CheckUri(Settings.SsoEndPoint)))
                            {
                                _log.ErrorFormat("Wrong SsoEndPoint URL: {0}", Settings.SsoEndPoint);
                                return string.Format(Resource.SsoSettingsWrongURL, Settings.SsoEndPoint);
                            }
                            if (!string.IsNullOrWhiteSpace(Settings.SloEndPoint) && !CheckUri(Settings.SloEndPoint))
                            {
                                _log.ErrorFormat("Wrong SloEndPoint URL: {0}", Settings.SloEndPoint);
                                return string.Format(Resource.SsoSettingsWrongURL, Settings.SloEndPoint);
                            }
                            if (string.IsNullOrWhiteSpace(Settings.PublicKey))
                            {
                                _log.ErrorFormat("Wrong PublicKey: {0}", Settings.PublicKey);
                                return Resource.SsoSettingsWrongPublicKey;
                            }
                            if (Settings.TokenType != TokenTypes.SAML && Settings.TokenType != TokenTypes.JWT)
                            {
                                _log.ErrorFormat("Wrong token type: {0}", Settings.TokenType);
                                return Resource.SsoSettingsWrongTokenType;
                            }
                            if ((Settings.ValidationType != ValidationTypes.HMAC_SHA256 && Settings.ValidationType != ValidationTypes.RSA_SHA256
                                 && Settings.ValidationType != ValidationTypes.X509))
                            {
                                _log.ErrorFormat("Wrong validaion type: {0}", Settings.ValidationType);
                                return Resource.SsoSettingsWrongValidationType;
                            }
                            if (Settings.TokenType == TokenTypes.SAML && Settings.ValidationType != ValidationTypes.X509)
                            {
                                Settings.ValidationType = ValidationTypes.X509;
                            }
                        }

                        if (!SettingsManager.Instance.SaveSettings(Settings, TenantProvider.CurrentTenantID))
                        {
                            _log.ErrorFormat("Can't save SSO settings.");
                            return Resource.SsoSettingsCantSaveSettings;
                        }

                        MessageService.Send(HttpContext.Current.Request, messageAction);
                    }
                    catch(Exception e)
                    {
                        _log.ErrorFormat("Save SSO setting error: {0}.", e);
                        return Resource.SsoSettingsUnexpectedError;
                    }
                    return string.Empty;
                }
            }
            _log.ErrorFormat("Insufficient Access Rights by saving sso settings!");
            throw new SecurityException();
        }

        private bool CheckUri(string uriName)
        {
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}