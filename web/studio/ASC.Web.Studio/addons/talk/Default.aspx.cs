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
using System.Configuration;
using System.Web;
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.Talk.Resources;

namespace ASC.Web.Talk
{
    public partial class DefaultTalk : MainPage
    {
        private TalkConfiguration cfg;

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var isEnabledTalk = ConfigurationManagerExtension.AppSettings["web.talk"] ?? "false";
            if (isEnabledTalk != "true")
            {
                Response.Redirect(CommonLinkUtility.GetDefault());
            }

            Page.RegisterStyle("~/addons/talk/css/default/talk.overview.css");

            Title = HeaderStringHelper.GetPageTitle(TalkResource.ProductName);
            Master.DisabledSidePanel = true;

            cfg = new TalkConfiguration();

            HelpLink = CommonLinkUtility.GetHelpLink();
        }

        public string ServerAddress
        {
            get { return cfg.ServerAddress; }
        }

        public string ServerName
        {
            get { return cfg.ServerName; }
        }

        public string ServerPort
        {
            get { return cfg.ServerPort; }
        }

        public string UserName
        {
            get { return cfg.UserName; }
        }

        public string JID
        {
            get { return cfg.Jid; }
        }
    }
}