/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Files.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Studio;
using System;
using System.Text;
using System.Web;

namespace ASC.Web.Files
{
    public partial class FileChoice : MainPage, IStaticBundle
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "filechoice.aspx"; }
        }

        public const string ParamFilterExt = "fileType";
        public const string MailMergeParam = "mailmerge";
        public const string RootParam = "root";
        public const string ThirdPartyParam = "thirdParty";
        public const string DocumentTypeParam = "documentType";

        protected string RequestExt
        {
            get { return (Request[ParamFilterExt] ?? "").Trim().ToLower(); }
        }

        private bool OnlyFolder
        {
            get { return !string.IsNullOrEmpty(Request["onlyFolder"]); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.Master.DisabledSidePanel = true;
            Master.Master.DisabledTopStudioPanel = true;
            Master.Master
                  .AddStaticStyles(GetStaticStyleSheet())
                  .AddStaticBodyScripts(GetStaticJavaScript());

            var fileSelector = (FileSelector) LoadControl(FileSelector.Location);
            fileSelector.IsFlat = true;
            fileSelector.OnlyFolder = OnlyFolder;
            CommonContainerHolder.Controls.Add(fileSelector);

            InitScript();
        }

        private void InitScript()
        {
            var script = new StringBuilder();

            FolderType folderType;
            if (Enum.TryParse(Request[RootParam], true, out folderType))
            {
                object rootId = null;
                switch (folderType)
                {
                    case FolderType.COMMON:
                        rootId = Classes.Global.FolderCommon;
                        break;
                    case FolderType.USER:
                        rootId = Classes.Global.FolderMy;
                        break;
                }
                if (rootId != null)
                    script.AppendFormat("jq(\"#fileSelectorTree > ul > li.tree-node:not([data-id=\\\"{0}\\\"])\").remove();", rootId);
            }

            if (!string.IsNullOrEmpty(RequestExt))
            {
                script.AppendFormat(";ASC.Files.FileSelector.filesFilter = ASC.Files.Constants.FilterType.ByExtension;"
                                    + "ASC.Files.FileSelector.filesFilterText = \"{0}\";",
                                    RequestExt.Replace("\"", "\\\""));
            }

            FilterType filter;
            if (Enum.TryParse(Request[DocumentTypeParam], out filter))
            {
                script.AppendFormat("ASC.Files.FileSelector.filesFilter = ASC.Files.Constants.FilterType[\"{0}\"] || ASC.Files.Constants.FilterType.None;", filter);
            }

            script.AppendFormat("ASC.Files.FileChoice.init(\"{0}\", ({1} == true), ({2} == true), ({3} == true));",
                                Request[FilesLinkUtility.FolderId],
                                OnlyFolder.ToString().ToLower(),
                                (!string.IsNullOrEmpty(Request[ThirdPartyParam])).ToString().ToLower(),
                                (!string.IsNullOrEmpty(Request[MailMergeParam])).ToString().ToLower());

            Page.RegisterInlineScript(script.ToString());
        }


        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                   new ScriptBundleData("fileschoice", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath,
                                  "common.js",
                                  "templatemanager.js",
                                  "servicemanager.js",
                                  "ui.js",
                                  "eventhandler.js",
                                  "anchormanager.js",
                                  "filechoice.js"
                       )
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                  "controls/emptyfolder/emptyfolder.js",
                                  "controls/fileselector/fileselector.js",
                                  "controls/tree/tree.js"
                       );
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                   new StyleBundleData("fileschoice", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath, "filechoice.css")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                  "controls/fileselector/fileselector.css",
                                  "controls/thirdparty/thirdparty.css",
                                  "controls/contentlist/contentlist.css",
                                  "controls/emptyfolder/emptyfolder.css",
                                  "controls/tree/tree.css"
                       );
        }
    }
}