/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;
using ASC.Web.Core.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Customization, Location, SortOrder = 100)]
    [AjaxNamespace("ColorThemeController")]
    public partial class ColorThemes : UserControl
    {
        public const string Location = "~/UserControls/Management/ColorThemes/ColorThemes.ascx";

        protected List<PortalColorTheme> ColorThemesList { get; set; }

        protected string ChosenTheme { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/Management/ColorThemes/js/colorthemes.js"));
            Page.RegisterStyleControl(ResolveUrl("~/usercontrols/management/ColorThemes/css/colorthemes.css"));

            ChosenTheme = ColorThemesSettings.GetColorThemesSettings();

            ColorThemesList = new List<PortalColorTheme>
                {
                    new PortalColorTheme
                        {
                            Title = Resource.ColorThemeDefault,
                            Value = "default"
                        },
                    new PortalColorTheme
                        {
                            Title = Resource.ColorThemePureOrange,
                            Value = "pure-orange"
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
        }

        [AjaxMethod]
        public void SaveColorTheme(string theme)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            ColorThemesSettings.SaveColorTheme(theme);
            MessageService.Send(HttpContext.Current.Request, MessageAction.ColorThemeChanged);
        }

        public class PortalColorTheme
        {
            public string Value { get; set; }
            public string Title { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}