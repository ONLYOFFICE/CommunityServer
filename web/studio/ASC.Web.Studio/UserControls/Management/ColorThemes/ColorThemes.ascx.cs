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
using System.Web;
using System.Web.UI;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Customization, Location, SortOrder = 100)]
    public partial class ColorThemes : UserControl
    {
        public const string Location = "~/UserControls/Management/ColorThemes/ColorThemes.ascx";

        protected List<PortalColorTheme> ColorThemesList { get; set; }

        protected string ChosenTheme { get; set; }

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Management/ColorThemes/js/colorthemes.js")
                .RegisterStyle("~/UserControls/Management/ColorThemes/css/colorthemes.css");

            ChosenTheme = ColorThemesSettings.GetColorThemesSettings();

            ColorThemesList = new List<PortalColorTheme>
                {
                    new PortalColorTheme
                        {
                            Title = Resource.ColorThemePureOrange,
                            Value = "pure-orange"
                        },
                    new PortalColorTheme
                        {
                            Title = Resource.ColorThemeDefault,
                            Value = "default"
                        },
                    new PortalColorTheme
                        {
                            Title = Resource.ColorThemeDarkGreen,
                            Value = "dark-green"
                        },
                    new PortalColorTheme
                        {
                            Title = Resource.ColorThemeDeepBlue,
                            Value = "deep-blue"
                        },
                    new PortalColorTheme
                        {
                            Title = Resource.ColorThemeWildPink,
                            Value = "wild-pink"
                        },
                    new PortalColorTheme
                        {
                            Title = Resource.ColorThemeBrightBlue,
                            Value = "bright-blue"
                        }
                };

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        public class PortalColorTheme
        {
            public string Value { get; set; }
            public string Title { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}