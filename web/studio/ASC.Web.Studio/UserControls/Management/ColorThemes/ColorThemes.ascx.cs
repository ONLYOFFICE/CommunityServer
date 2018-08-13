/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
            Page.RegisterBodyScripts("~/usercontrols/Management/ColorThemes/js/colorthemes.js")
                .RegisterStyle("~/usercontrols/management/ColorThemes/css/colorthemes.css");

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