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
using System.Text;
using System.Web.UI;
using ASC.Web.Community.Controls;
using ASC.Data.Storage;
using System.Web;
using ASC.Web.Community.Resources;

namespace ASC.Web.Community
{
    public partial class CommunityMasterPage : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts(GetFileStaticRelativePath("common.js"));
            Page.RegisterStyleControl(GetFileStaticRelativePath("community_common.css"));

            _sideNavigation.Controls.Add(LoadControl(NavigationSidePanel.Location));

            if (!(Page is _Default))
            {
                var script = new StringBuilder();
                script.Append("window.ASC=window.ASC||{};");
                script.Append("window.ASC.Community=window.ASC.Community||{};");
                script.Append("window.ASC.Community.Resources={};");
                script.AppendFormat("window.ASC.Community.Resources.HelpTitleAddNew=\"{0}\";", CommunityResource.HelpTitleAddNew);
                script.AppendFormat("window.ASC.Community.Resources.HelpContentAddNew=\"{0}\";", CommunityResource.HelpContentAddNew);
                script.AppendFormat("window.ASC.Community.Resources.HelpTitleSettings=\"{0}\";", CommunityResource.HelpTitleSettings);
                script.AppendFormat("window.ASC.Community.Resources.HelpContentSettings=\"{0}\";", CommunityResource.HelpContentSettings);
                script.AppendFormat("window.ASC.Community.Resources.HelpTitleNavigateRead=\"{0}\";", CommunityResource.HelpTitleNavigateRead);
                script.AppendFormat("window.ASC.Community.Resources.HelpContentNavigateRead=\"{0}\";", CommunityResource.HelpContentNavigateRead);
                script.AppendFormat("window.ASC.Community.Resources.HelpTitleSwitchModules=\"{0}\";", CommunityResource.HelpTitleSwitchModules);
                script.AppendFormat("window.ASC.Community.Resources.HelpContentSwitchModules=\"{0}\";", CommunityResource.HelpContentSwitchModules);

                Page.RegisterInlineScript(script.ToString());
            }
            else
            {
                Master.DisabledHelpTour = true;
            }
        }

        protected string GetFileStaticRelativePath(String fileName)
        {
            if (fileName.EndsWith(".js"))
            {
                return ResolveUrl("~/products/community/js/" + fileName);
            }
            if (fileName.EndsWith(".ascx"))
            {
                return VirtualPathUtility.ToAbsolute("~/products/community/controls/" + fileName);
            }
            if (fileName.EndsWith(".css"))
            {
                return ResolveUrl("~/products/community/app_themes/default/" + fileName);
            }
            if (fileName.EndsWith(".png") || fileName.EndsWith(".gif") || fileName.EndsWith(".jpg"))
            {
                return WebPath.GetPath("/products/community/app_themes/default/images/" + fileName);
            }
            return fileName;
        }
    }
}