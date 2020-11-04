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
using System.Globalization;
using System.Web.UI;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core.HelpCenter;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Common.VideoGuides
{
    public partial class VideoGuidesControl : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/VideoGuides/VideoGuides.ascx"; }
        }

        public bool IsSubItem;

        public bool DisableVideo { get; set; }
        protected List<VideoGuideItem> VideoGuideItems { get; set; }
        protected string AllVideoLink { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            // Mirgate to CommonBodyScripts.ascx.cs
            // Page.RegisterBodyScripts("~/UserControls/Common/VideoGuides/js/videoguides.js");
            RenderVideoHandlers();
        }

        protected void RenderVideoHandlers()
        {
            var settings = AdditionalWhiteLabelSettings.Instance;

            if (!settings.VideoGuidesEnabled || String.IsNullOrEmpty(settings.VideoGuidesUrl))
            {
                DisableVideo = true;
                return;
            }

            AllVideoLink = CommonLinkUtility.GetRegionalUrl(settings.VideoGuidesUrl, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            
            VideoGuideItems = HelpCenterHelper.GetVideoGuides();

            if (VideoGuideItems.Count > 0)
            {
                AjaxPro.Utility.RegisterTypeForAjax(typeof(UserVideoSettings));
            }
        }
    }
}