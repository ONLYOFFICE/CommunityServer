/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Core;
using ASC.Notify.Recipients;
using ASC.Web.Community.News.Code.Module;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;

namespace ASC.Web.Community.News
{
    public partial class NewsMaster : System.Web.UI.MasterPage
    {
        public string SearchText { get; set; }

        public string CurrentPageCaption
        {
            get { return MainNewsContainer.CurrentPageCaption; }
            set { MainNewsContainer.CurrentPageCaption = value; }
        }

        private static IDirectRecipient IAmAsRecipient
        {
            get { return (IDirectRecipient)NewsNotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString()); }
        }

        protected Guid RequestedUserId
        {
            get
            {
                var result = Guid.Empty;
                try
                {
                    result = new Guid(Request["uid"]);
                }
                catch
                {
                }

                return result;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle("~/Products/Community/App_Themes/dark/dark-newsstylesheet.less");
            }
            else
            {
                Page.RegisterStyle("~/Products/Community/Modules/News/App_Themes/default/newsstylesheet.less");
            }
            Page.RegisterBodyScripts("~/Products/Community/Modules/News/js/news.js");

            SearchText = "";
            if (!string.IsNullOrEmpty(Request["search"]))
            {
                SearchText = Request["search"];
            }
        }

        public void SetInfoMessage(string message, InfoType type)
        {
            MainNewsContainer.Options.InfoType = type;
            MainNewsContainer.Options.InfoMessageText = message;
        }
    }
}