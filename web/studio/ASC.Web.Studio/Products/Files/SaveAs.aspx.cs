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
using System.Text;
using System.Web;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Resources;
using ASC.Web.Studio;
using Global = ASC.Web.Files.Classes.Global;

namespace ASC.Web.Files
{
    public partial class SaveAs : MainPage, IStaticBundle
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "saveas.aspx"; }
        }

        public static string GetUrl
        {
            get { return Location + string.Format("?{0}={{{0}}}&{1}={{{1}}}", FilesLinkUtility.FileTitle, FilesLinkUtility.FileUri); }
        }

        public string RequestFileTitle
        {
            get { return Global.ReplaceInvalidCharsAndTruncate(Request[FilesLinkUtility.FileTitle]); }
        }

        public string RequestUrl
        {
            get { return Request[FilesLinkUtility.FileUri]; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.Master.DisabledSidePanel = true;
            Master.Master.DisabledTopStudioPanel = true;
            Master.Master
                  .AddStaticStyles(GetStaticStyleSheet())
                  .AddStaticBodyScripts(GetStaticJavaScript());

            var fileSelector = (FileSelector)LoadControl(FileSelector.Location);
            fileSelector.IsFlat = true;
            fileSelector.OnlyFolder = true;
            fileSelector.SuccessButton = FilesCommonResource.ButtonSave;
            CommonContainerHolder.Controls.Add(fileSelector);

            InitScript();
        }

        private void InitScript()
        {
            var script = new StringBuilder();

            script.Append("ASC.Files.FileChoice.init();");
            Page.RegisterInlineScript(script.ToString());
        }


        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                   new ScriptBundleData("filessaveas", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath,
                                  "common.js",
                                  "templatemanager.js",
                                  "servicemanager.js",
                                  "ui.js",
                                  "eventhandler.js",
                                  "saveas.js"
                       )
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                  "Controls/EmptyFolder/emptyfolder.js",
                                  "Controls/FileSelector/fileselector.js",
                                  "Controls/Tree/tree.js"
                       );
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                   new StyleBundleData("filessaveas", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath, "saveas.css")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                  "Controls/FileSelector/fileselector.css",
                                  "Controls/ThirdParty/thirdparty.css",
                                  "Controls/ContentList/contentlist.css",
                                  "Controls/EmptyFolder/emptyfolder.css",
                                  "Controls/Tree/tree.css"
                       );
        }
    }
}