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
using System.Web.UI;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Core;
using AjaxPro;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Web;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.PortalSecurity, Location, SortOrder = 400)]
    [AjaxNamespace("CookieSettingsController")]
    public partial class CookieSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/CookieSettings/CookieSettings.ascx";

        protected bool Enabled { get; set; }

        protected int LifeTime { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Enabled = SetupInfo.IsVisibleSettings("CookieSettings");
            
            if (!Enabled) return;

            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts("~/UserControls/Management/CookieSettings/js/cookiesettings.js");

            LifeTime = CookiesManager.GetLifeTime();
        }

        [AjaxMethod]
        public object Save(int lifeTime)
        {
            try
            {
                if (lifeTime > 0)
                {
                    CookiesManager.SetLifeTime(lifeTime);

                    MessageService.Send(HttpContext.Current.Request, MessageAction.CookieSettingsUpdated);
                }

                return new
                    {
                        Status = 1,
                        Message = Resources.Resource.SuccessfullySaveSettingsMessage
                    };
            }
            catch(Exception e)
            {
                return new
                    {
                        Status = 0,
                        Message = e.Message.HtmlEncode()
                    };
            }
        }

        [AjaxMethod]
        public object Restore()
        {
            try
            {
                CookiesManager.SetLifeTime(0);

                MessageService.Send(HttpContext.Current.Request, MessageAction.CookieSettingsUpdated);

                return new
                {
                    Status = 1,
                    Message = Resources.Resource.SuccessfullySaveSettingsMessage
                };
            }
            catch (Exception e)
            {
                return new
                    {
                        Status = 0,
                        Message = e.Message.HtmlEncode()
                    };
            }
        }
    }
}