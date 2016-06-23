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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Data.Storage;
using ASC.MessagingSystem;
using ASC.Thrdparty.Configuration;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.ThirdPartyAuthorization, Location)]
    [AjaxNamespace("AuthorizationKeys")]
    public partial class AuthorizationKeys : UserControl
    {
        public const string Location = "~/UserControls/Management/AuthorizationKeys/AuthorizationKeys.ascx";

        private List<AuthService> _authServiceList;

        protected List<AuthService> AuthServiceList
        {
            get { return _authServiceList ?? (_authServiceList = GetAuthServices().ToList()); }
        }

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts("~/usercontrols/management/AuthorizationKeys/js/authorizationkeys.js");
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "authorizationkeys_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebPath.GetPath("usercontrols/management/authorizationkeys/css/authorizationkeys.css") + "\">", false);

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        private static IEnumerable<AuthService> GetAuthServices()
        {
            return from KeyElement keyElement in ConsumerConfigurationSection.GetSection().Keys
                   where keyElement.Type != KeyElement.KeyType.Default && !string.IsNullOrEmpty(keyElement.ConsumerName)
                   group keyElement by keyElement.ConsumerName
                   into keyGroup
                   let consumerKey = keyGroup.FirstOrDefault(key => key.Type == KeyElement.KeyType.Key)
                   let consumerSecret = keyGroup.FirstOrDefault(key => key.Type == KeyElement.KeyType.Secret)
                   select ToAuthService(keyGroup.Key, consumerKey, consumerSecret);
        }

        private static AuthService ToAuthService(string consumerName, KeyElement consumerKey, KeyElement consumerSecret)
        {
            var authService = new AuthService(consumerName);
            if (consumerKey != null)
                authService.WithId(consumerKey.Name, KeyStorage.Get(consumerKey.Name));
            if (consumerSecret != null)
                authService.WithKey(consumerSecret.Name, KeyStorage.Get(consumerSecret.Name));
            return authService;
        }

        #region Ajax

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void SaveAuthKeys(List<AuthKey> authKeys)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            foreach (var authKey in authKeys.Where(authKey => KeyStorage.Get(authKey.Name) != authKey.Value))
            {
                KeyStorage.Set(authKey.Name, authKey.Value);
            }

            MessageService.Send(HttpContext.Current.Request, MessageAction.AuthorizationKeysSetting);
        }

        #endregion
    }
}