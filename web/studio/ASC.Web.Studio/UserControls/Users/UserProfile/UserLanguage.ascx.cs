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


using System;
using System.Web;
using System.Web.UI;
using ASC.MessagingSystem;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Utility;
using AjaxPro;
using ASC.Core;
using ASC.Web.Studio.Core;
using Resources;

namespace ASC.Web.Studio.UserControls.Users
{
    [AjaxNamespace("AjaxPro.UserLangController")]
    public partial class UserLanguage : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/UserLanguage.ascx"; }
        }

        protected string HelpLink { get; set; }

        protected bool ShowHelper { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterStyle(ResolveUrl("~/UserControls/Users/UserProfile/css/userlanguages.less"))
                .RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/UserControls/Users/UserProfile/js/userlanguage.js"));

            HelpLink = CommonLinkUtility.GetHelpLink();

            ShowHelper = !(CoreContext.Configuration.Standalone && !CompanyWhiteLabelSettings.Instance.IsDefault);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveUserLanguageSettings(string lng)
        {
            try
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var curLng = user.CultureName;
                
                var changelng = false;
                if (SetupInfo.EnabledCultures.Find(c => String.Equals(c.Name, lng, StringComparison.InvariantCultureIgnoreCase)) != null)
                {
                    if (curLng != lng)
                    {
                        user.CultureName = lng;
                        changelng = true;

                        try
                        {
                            CoreContext.UserManager.SaveUserInfo(user);
                        }
                        catch (Exception ex)
                        {
                            user.CultureName = curLng;
                            throw ex;
                        }

                        MessageService.Send(HttpContext.Current.Request, MessageAction.UserUpdatedLanguage);
                    }
                }
                
                return new {Status = changelng ? 1 : 2, Message = Resource.SuccessfullySaveSettingsMessage};
            }
            catch(Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }
    }
}