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
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Studio.Core.HelpCenter;
using ASC.Web.Studio.UserControls.Common.VideoGuides;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Common.HelpCenter
{
    public partial class HelpCenter : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/HelpCenter/HelpCenter.ascx"; }
        }

        public bool IsSideBar { get; set; }
        public Guid ModuleId { get; set; }

        protected List<HelpCenterItem> HelpCenterItems { get; set; }
        protected String HelpLink { get; set; }
        protected String HelpLinkBlock { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
            {
                return;
            }

            Page.RegisterStyle("~/UserControls/Common/HelpCenter/css/help-center.less");

            string module;
            string mainLink;

            if (Page is Studio.Management)
            {
                module = "configuration.aspx";
                mainLink = CommonLinkUtility.GetAdministration(ManagementType.HelpCenter);
            }
            else
            {
                var currentModule = GetProduct();
                if (currentModule == null || string.IsNullOrEmpty(currentModule.HelpURL)) return;

                module = currentModule.ProductClassName + ".aspx";
                mainLink = VirtualPathUtility.ToAbsolute(currentModule.HelpURL);
            }

            const string index = "#help";
            HelpLink = mainLink + index;
            HelpLinkBlock = mainLink + index + "=";

            HelpCenterItems = HelpCenterHelper.GetHelpCenter(module, HelpLinkBlock);

            if (IsSideBar)
            {
                var videoGuidesControl = (VideoGuidesControl)LoadControl(VideoGuidesControl.Location);
                if (HelpCenterItems == null)
                {
                    VideoGuides.Controls.Add(videoGuidesControl);
                }
                else
                {
                    videoGuidesControl.IsSubItem = true;
                    VideoGuidesSubItem.Controls.Add(videoGuidesControl);
                }
            }

            if (MediaViewersPlaceHolder != null)
            {
                MediaViewersPlaceHolder.Controls.Add(LoadControl(MediaPlayer.Location));
            }
        }

        private IWebItem GetProduct()
        {
            var currentModule = WebItemManager.Instance[ModuleId] ?? CommonLinkUtility.GetWebItemByUrl(Context.Request.Url.AbsoluteUri);

            if (currentModule is IAddon)
                return currentModule;

            var product = currentModule as IProduct;
            if (product != null)
                return product;

            if (currentModule == null) return null;

            IModule module;
            CommonLinkUtility.GetLocationByUrl(CommonLinkUtility.GetFullAbsolutePath(currentModule.StartURL), out product, out module);
            return product;
        }


        public static string RenderControlToString()
        {
            return RenderControlToString(null);
        }

        public static string RenderControlToString(Guid? moduleId)
        {
            var cntrl = new HelpCenter();
            cntrl = (HelpCenter)cntrl.LoadControl(Location);
            if (moduleId.HasValue)
            {
                cntrl.ModuleId = moduleId.Value;
            }

            var page = new Page();
            page.Controls.Add(cntrl);

            var writer = new StringWriter();
            HttpContext.Current.Server.Execute(page, writer, false);
            var renderedControl = writer.ToString();
            writer.Close();

            return renderedControl;
        }
    }
}