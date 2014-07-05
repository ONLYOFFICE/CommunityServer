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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.UI;
using ASC.Core;
using ASC.Data.Storage;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/AuthorizationKeys/js/authorizationkeys.js"));
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "authorizationkeys_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebPath.GetPath("usercontrols/management/authorizationkeys/css/authorizationkeys.css") + "\">", false);
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
                authService.WithId(consumerKey.Name, consumerKey.Value);
            if (consumerSecret != null)
                authService.WithKey(consumerSecret.Name, consumerSecret.Value);
            return authService;
        }

        #region Ajax

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void SaveAuthKeys(List<AuthKey> authKeys)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            var config = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);
            var consumersSection = (ConsumerConfigurationSection)config.GetSection(ConsumerConfigurationSection.SectionName);

            //todo: keys to db
            foreach (var authKey in authKeys)
            {
                var consumersKey = consumersSection.Keys.GetKey(authKey.Name);
                if (consumersKey != null && consumersKey.Value != authKey.Value)
                {
                    consumersKey.Value = authKey.Value;
                }
            }

            config.Save(ConfigurationSaveMode.Modified);
        }

        #endregion
    }
}