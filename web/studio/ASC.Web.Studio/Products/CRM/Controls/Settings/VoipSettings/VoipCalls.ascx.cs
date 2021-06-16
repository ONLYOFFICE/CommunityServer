/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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

using ASC.CRM.Core;
using ASC.Data.Storage;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.PublicResources;

namespace ASC.Web.CRM.Controls.Settings
{
    public partial class VoipCalls : BaseUserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/VoIPSettings/VoipCalls.ascx"); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!((CRMSecurity.IsAdmin || VoipNumberData.CanMakeOrReceiveCall) && VoipNumberData.Allowed))
            {
                Response.Redirect(PathProvider.StartURL() + "Settings.aspx");
            }

            var emptyScreenControl = new EmptyScreenControl
            {
                ID = "voip-calls-empty-list-box",
                ImgSrc = WebPath.GetPath("UserControls/Feed/images/empty_screen_feed.png"),
                Header = UserControlsCommonResource.VoipCallsNotFound,
                Describe = UserControlsCommonResource.VoipCallsNotFoundDescription
            };
            controlsHolder.Controls.Add(emptyScreenControl);

            var emptyScreenFilterControl = new EmptyScreenControl
            {
                ID = "voip-calls-empty-filter-box",
                ImgSrc = WebPath.GetPath("UserControls/Feed/images/empty_filter.png"),
                Header = UserControlsCommonResource.FilterNoVoipCalls,
                Describe = UserControlsCommonResource.FilterNoVoipCallsDescription,
                ButtonHTML = string.Format("<a href='javascript:void(0)' class='baseLinkAction clearFilterButton'>{0}</a>",
                                           UserControlsCommonResource.ResetFilter)
            };
            controlsHolder.Controls.Add(emptyScreenFilterControl);

            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("voip.calls.js"));
        }
    }
}