/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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