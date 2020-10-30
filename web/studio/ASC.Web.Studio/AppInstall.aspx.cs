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
using System.Web;
using System.Collections.Generic;
using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;


namespace ASC.Web.Studio
{
    public partial class AppInstall : MainPage
    {
        public List<AppInstallItemGroup> Data;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (CoreContext.Configuration.CustomMode)
                Response.Redirect(CommonLinkUtility.GetDefault());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.DisabledSidePanel = true;
            Page.RegisterStyle("~/skins/default/app_install.less");

            Data = new List<AppInstallItemGroup>
            {
                new AppInstallItemGroup(Resource.ForYourPC)
                {
                    Items = new List<AppInstallItem>
                    {
                        new AppInstallItemPC("Windows"),
                        new AppInstallItemPC("Linux" ),
                        new AppInstallItemPC("Mac OS") { Title = "Mac" }
                    }
                },
                new AppInstallItemGroup(Resource.ForMobile)
                {
                    Items = new List<AppInstallItem>
                    {
                        new AppInstallItem
                        {
                            Href = SetupInfo.DownloadForIosDocuments,
                            OS = "iOS",
                            Text = Resource.DocumentsForDevices,
                            Icon = "Documents",
                            ButtonStore = "ButtonForApps AppStore"
                        },
                        new AppInstallItem
                        {
                            Href = SetupInfo.DownloadForIosProjects,
                            OS = "iOS",
                            Text = Resource.ProjectsForDevices,
                            Icon = "Project",
                            ButtonStore = "ButtonForApps AppStore"
                        },
                        new AppInstallItem
                        {
                            Href = SetupInfo.DownloadForAndroidDocuments,
                            OS = "Android",
                            Text = Resource.DocumentsForDevices,
                            Icon = "Documents",
                            ButtonStore = "ButtonForApps GooglePlay"
                        }
                    }
                }
            };
        }

    }

    public class AppInstallItem
    {
        private string title;
        public string Title
        {
            get { return title ?? OS; }
            set { title = value; }
        }

        public string Href { get; set; }
        public string OS { get; set; }

        private string text;
        public string Text
        {
            get { return string.Format(text, OS); }
            set { text = value; }
        }

        public string Icon { get; set; }
        public string ButtonStore { get; set; }
        public string ButtonText { get; set; }
    }

    public class AppInstallItemPC : AppInstallItem
    {
        public AppInstallItemPC(string os)
        {
            OS = os;
            Href = SetupInfo.DownloadForDesktopUrl;
            Text = Resource.DesktopEditorsFor;
            Icon = "PC";
            ButtonStore = "button DesctopDownload";
            ButtonText = Resource.GetItNow;
        }
    }

    public class AppInstallItemGroup
    {
        public string Caption { get; private set; }
        public List<AppInstallItem> Items { get; set; }

        public AppInstallItemGroup(string caption)
        {
            Caption = caption;
        }
    }
}