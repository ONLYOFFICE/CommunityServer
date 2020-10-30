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


#region Usings

using System;
using System.Web;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Community.Product;
using ASC.Core;
using ASC.Web.Studio.Core;
using System.Text;
using Newtonsoft.Json.Linq;


#endregion

namespace ASC.Web.Community.Controls
{
    public partial class ButtonSidePanel : System.Web.UI.UserControl
    {
        public static String Location = VirtualPathUtility.ToAbsolute("~/Products/Community/Controls/ButtonSidePanel.ascx");

        protected bool IsBlogsAvailable { get; set; }
        protected bool IsEventsAvailable { get; set; }
        protected bool IsForumsAvailable { get; set; }
        protected bool IsBookmarksAvailable { get; set; }
        protected bool IsWikiAvailable { get; set; }
        protected bool IsBirthdaysAvailable { get; set; }

        protected bool IsAdmin { get; set; }
        protected bool IsVisitor { get; set; }
        protected bool IsFullAdministrator { get; set; }

        protected int TopicID
        {
            get
            {
                int result;
                return int.TryParse(Request["t"], out result) ? result : 0;
            }
        }

        protected int ForumID
        {
            get
            {
                int result;
                return int.TryParse(Request["f"], out result) ? result : 0;
            }
        }

        protected bool InAParticularTopic { get; set; }

        protected bool MakeCreateNewTopic { get; set; }

        protected bool ForumsHasThreadCategories { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            IsAdmin = WebItemSecurity.IsProductAdministrator(CommunityProduct.ID, SecurityContext.CurrentAccount.ID);
            IsVisitor = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor();
            IsFullAdministrator = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();

            InitPermission();
        }

        private void InitPermission()
        {           
            foreach (var module in WebItemManager.Instance.GetSubItems(CommunityProduct.ID))
            {
                switch (module.GetSysName())
                {
                    case "community-blogs":
                        IsBlogsAvailable = true;
                        break;
                    case "community-news":
                        IsEventsAvailable = true;
                        break;
                    case "community-forum":
                        InitForumsData();
                        break;
                    case "community-bookmarking":
                        IsBookmarksAvailable = true;
                        break;
                    case "community-wiki":
                        IsWikiAvailable = true;
                        break;
                    case "community-birthdays":
                        IsBirthdaysAvailable = true;
                        break;
                }
            }
        }

        private void InitForumsData()
        {
            IsForumsAvailable = true;

            var currentPath = HttpContext.Current.Request.Path;

            if (currentPath.IndexOf("Modules/Forum/Posts.aspx", StringComparison.OrdinalIgnoreCase) > 0)
            {
                InAParticularTopic = true;
                MakeCreateNewTopic = true;
            }

            var apiServer = new Api.ApiServer();
            var apiResponse = apiServer.GetApiResponse(String.Format("{0}community/forum/count.json", SetupInfo.WebApiBaseUrl), "GET");
            var obj = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(apiResponse)));
            var count = 0;
            if (Int32.TryParse(obj["response"].ToString(), out count))
                ForumsHasThreadCategories = count > 0;

            if (InAParticularTopic && TopicID > 0)
            {
                apiServer = new Api.ApiServer();
                apiResponse = apiServer.GetApiResponse(String.Format("{0}community/forum/topic/{1}.json", SetupInfo.WebApiBaseUrl, TopicID), "GET");
                obj = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(apiResponse)));
                if(obj["response"]!=null)
                {
                    obj = JObject.Parse(obj["response"].ToString());
                    var status = 0;
                    if (Int32.TryParse(obj["status"].ToString(), out status))
                        MakeCreateNewTopic = status != 1 && status != 3;
                }
            }
        }

    }
}
