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

using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.WebZones;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;

namespace ASC.Web.CRM.Configuration
{
    [WebZone(WebZoneType.CustomProductList)]
    public class VoipModule : IAddon, IRenderCustomNavigation
    {
        public Guid ID
        {
            get { return WebItemManager.VoipModuleID; }
        }

        public string Name
        {
            get { return CRMVoipResource.VoipModuleTitle; }
        }

        public string Description
        {
            get { return CRMVoipResource.VoipModuleDescription; }
        }

        public string StartURL
        {
            get { return PathProvider.StartURL() + "Settings.aspx?type=voip.common&sysname=/modules/voip"; }
        }

        public string WarmupURL { get; set; }

        public string HelpURL
        {
            get { return null; }
        }

        public string ProductClassName { get { return "voip"; } }

        public bool Visible { get { return SetupInfo.VoipEnabled; } }

        public AddonContext Context { get; private set; }

        public void Init()
        {
            Context = new AddonContext
            {
                DefaultSortOrder = 90,
                IconFileName = "voip_logo.png",
                CanNotBeDisabled = true
            };
        }

        public void Shutdown()
        {

        }

        WebItemContext IWebItem.Context
        {
            get { return Context; }
        }

        public Control LoadCustomNavigationControl(Page page)
        {
            return null;
        }

        public string RenderCustomNavigation(Page page)
        {
            try
            {
                if (!VoipNumberData.CanMakeOrReceiveCall) return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            page.RegisterBodyScripts("~/js/asc/core/voip.navigationitem.js");

            return
                string.Format(@"<li class=""top-item-box voip display-none"">
                                  <a class=""voipActiveBox inner-text"" title=""{0}"">
                                      <svg><use base=""{1}"" href=""/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenuphone""></use></svg>
                                      <span class=""inner-label"">{2}</span>
                                  </a>
                                </li>",
                              "VoIP",
                              WebPath.GetPath("/"),
                              0);
        }
    }
}