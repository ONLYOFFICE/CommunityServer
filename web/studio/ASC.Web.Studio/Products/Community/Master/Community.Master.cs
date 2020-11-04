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
            //Page.RegisterStyleControl(LoadControl(VirtualPathUtility.ToAbsolute("~/Products/Community/Master/Styles.ascx")));
            Page.RegisterBodyScripts(ResolveUrl, "~/Products/Community/js/common.js");

            CommunityCreateButtonContent.Controls.Add(LoadControl(ButtonSidePanel.Location));
            _sideNavigation.Controls.Add(LoadControl(NavigationSidePanel.Location));

            Master.AddClientScript(
                new ClientScripts.ClientLocalizationResources(),
                new ClientScripts.ClientTemplateResources());
        }

        //protected string GetFileStaticRelativePath(String fileName)
        //{
        //    if (fileName.EndsWith(".js"))
        //    {
        //        return ResolveUrl("~/Products/Community/js/" + fileName);
        //    }
        //    if (fileName.EndsWith(".ascx"))
        //    {
        //        return VirtualPathUtility.ToAbsolute("~/Products/Community/Controls/" + fileName);
        //    }
        //    if (fileName.EndsWith(".css"))
        //    {
        //        return ResolveUrl("~/Products/Community/App_Themes/default/" + fileName);
        //    }
        //    if (fileName.EndsWith(".png") || fileName.EndsWith(".gif") || fileName.EndsWith(".jpg"))
        //    {
        //        return WebPath.GetPath("/Products/Community/App_Themes/default/images/" + fileName);
        //    }
        //    return fileName;
        //}
    }
}