/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ASC.Files.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Resources;
using ASC.Web.Studio;

namespace ASC.Web.Files
{
    public partial class FileChoice : MainPage, IStaticBundle
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "filechoice.aspx"; }
        }

        public static string GetUrlForEditor
        {
            get {return Location + string.Format("?{0}=true&{1}={{{1}}}&{2}={{{2}}}",
                FromEditorParam,
                FilterExtParam,
                FileTypeParam);}
        }

        public static string GetUrl(string ext = null,
            bool fromEditor = false,
            FolderType? root = null,
            bool? thirdParty = null,
            FilterType? filterType = null,
            bool multiple = false,
            string successButton = null)
        {
            var args = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(ext = (ext ?? "").Trim().ToLower())) args.Add(FilterExtParam, ext);
            if (fromEditor) args.Add(FromEditorParam, "true");
            if (root.HasValue) args.Add(RootParam, root.ToString());
            if (thirdParty.HasValue) args.Add(ThirdPartyParam, thirdParty.Value.ToString().ToLower());
            if (filterType.HasValue) args.Add(FileTypeParam, filterType.Value.ToString());
            if (multiple) args.Add(MultiSelectParam, "true");
            if (!string.IsNullOrEmpty(successButton = (successButton ?? "").Trim())) args.Add(SuccessButtonParam, successButton);

            return Location + "?" + string.Join("&", args.Select(arg => HttpUtility.HtmlEncode(arg.Key) + "=" + HttpUtility.HtmlEncode(arg.Value)));
        }

        public const string FilterExtParam = "fileExt";
        public const string FromEditorParam = "editor";
        public const string RootParam = "root";
        public const string ThirdPartyParam = "thirdParty";
        public const string FileTypeParam = "documentType";
        public const string MultiSelectParam = "multiple";
        public const string SuccessButtonParam = "ok";

        protected string RequestExt
        {
            get
            {
                var ext = (Request[FilterExtParam] ?? "").Trim().ToLower();
                //todo: for DS 5.2 version. remove
                return
                    ext == "{" + FilterExtParam.ToLower() + "}"
                        ? "xlsx"
                        : ext;
            }
        }

        protected FilterType RequestType
        {
            get
            {
                FilterType filter;
                return Enum.TryParse(Request[FileTypeParam], out filter) ? filter : FilterType.None;
            }
        }

        protected bool FromEditor
        {
            get { return !string.IsNullOrEmpty(Request[FromEditorParam]); }
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
            fileSelector.Multiple = (Request[MultiSelectParam] ?? "").Trim().ToLower() == "true";
            var successButton = (Request[SuccessButtonParam] ?? "").Trim();
            if (!string.IsNullOrEmpty(successButton)) fileSelector.SuccessButton = successButton;
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
                    case FolderType.Projects:
                        rootId = Classes.Global.FolderProjects;
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

            if (RequestType != FilterType.None)
            {
                script.AppendFormat("ASC.Files.FileSelector.filesFilter = ASC.Files.Constants.FilterType[\"{0}\"] || ASC.Files.Constants.FilterType.None;", RequestType);
            }

            script.AppendFormat("ASC.Files.FileChoice.init(\"{0}\", ({1} == true), \"{2}\", ({3} == true));",
                                (Request[FilesLinkUtility.FolderId] ?? "").Replace("\"", "\\\""),
                                OnlyFolder.ToString().ToLower(),
                                (Request[ThirdPartyParam] ?? "").ToLower().Replace("\"", "\\\""),
                                FromEditor.ToString().ToLower());

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
                                  "Controls/EmptyFolder/emptyfolder.js",
                                  "Controls/FileSelector/fileselector.js",
                                  "Controls/Tree/tree.js"
                       );
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                   new StyleBundleData("fileschoice", "files")
                       .AddSource(PathProvider.GetFileStaticRelativePath, "filechoice.css")
                       .AddSource(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                  "Controls/FileSelector/fileselector.css",
                                  "Controls/ThirdParty/thirdparty.css",
                                  "Controls/ContentList/contentlist.css",
                                  "Controls/EmptyFolder/emptyfolder.css",
                                  "Controls/Tree/tree.css"
                       );
        }

        public string GetTypeString(FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.ArchiveOnly:
                    return FilesUCResource.ButtonFilterArchive;
                case FilterType.DocumentsOnly:
                    return FilesUCResource.ButtonFilterDocument;
                case FilterType.ImagesOnly:
                    return FilesUCResource.ButtonFilterImage;
                case FilterType.PresentationsOnly:
                    return FilesUCResource.ButtonFilterPresentation;
                case FilterType.SpreadsheetsOnly:
                    return FilesUCResource.ButtonFilterSpreadsheet;
                case FilterType.MediaOnly:
                    return FilesUCResource.ButtonFilterMedia;
            }
            return string.Empty;
        }
    }
}