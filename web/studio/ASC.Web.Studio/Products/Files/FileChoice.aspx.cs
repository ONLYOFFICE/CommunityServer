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
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "FileChoice.aspx"; }
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
                //todo: obsolete since DS v5.3
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

            var originForPost = "*";
            if (FromEditor && !FilesLinkUtility.DocServiceApiUrl.StartsWith("/"))
            {
                var origin = new Uri(FilesLinkUtility.DocServiceApiUrl ?? "");
                originForPost = origin.Scheme + "://" + origin.Host + ":" + origin.Port;
            }

            script.AppendFormat("ASC.Files.FileChoice.init(\"{0}\", ({1} == true), \"{2}\", ({3} == true), \"{4}\");",
                                (Request[FilesLinkUtility.FolderId] ?? "").Replace("\"", "\\\""),
                                OnlyFolder.ToString().ToLower(),
                                (Request[ThirdPartyParam] ?? "").ToLower().Replace("\"", "\\\""),
                                FromEditor.ToString().ToLower(),
                                originForPost);

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