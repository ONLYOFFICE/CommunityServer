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


using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using System.Web;

namespace ASC.Web.Projects
{
    public partial class TMDocs : BasePage
    {
        protected override bool CanRead
        {
            get { return !RequestContext.IsInConcreteProject || ProjectSecurity.CanReadFiles(Project); }
        }

        protected override void PageLoad()
        {
            FilterHolder.Controls.Add(LoadControl(MainContentFilter.Location));

            var mainContent = (MainContent) LoadControl(MainContent.Location);
            mainContent.NoMediaViewers = true;
            mainContent.FolderIDCurrentRoot = Project == null ? Global.FolderProjects : EngineFactory.FileEngine.GetRoot(Project.ID);
            mainContent.TitlePage = ProjectsCommonResource.ModuleName;
            CommonContainerHolder.Controls.Add(mainContent);

            Title = HeaderStringHelper.GetPageTitle(ProjectsFileResource.Files);

            Page
                .RegisterStyle(PathProvider.GetFileStaticRelativePath("common.css"))
                .RegisterStyle(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                               "Controls/MainContent/maincontent.css",
                               "Controls/ContentList/contentlist.css",
                               "Controls/AccessRights/accessrights.css",
                               "Controls/ThirdParty/thirdparty.css",
                               "Controls/ConvertFile/convertfile.css",
                               "Controls/ConvertFile/confirmconvert.css",
                               "Controls/EmptyFolder/emptyfolder.css",
                               "Controls/ChunkUploadDialog/chunkuploaddialog.css",
                               "Controls/Tree/treebuilder.css",
                               "Controls/Tree/tree.css"
                )
                .RegisterBodyScripts(ResolveUrl,
                                     "~/js/third-party/jquery/jquery.mousewheel.js",
                                     "~/js/third-party/jquery/jquery.uri.js",
                                     "~/js/third-party/sorttable.js",
                                     "~/js/uploader/jquery.fileupload.js"
                )
                .RegisterBodyScripts(PathProvider.GetFileStaticRelativePath,
                                     "auth.js",
                                     "common.js",
                                     "filter.js",
                                     "templatemanager.js",
                                     "servicemanager.js",
                                     "ui.js",
                                     "mousemanager.js",
                                     "markernew.js",
                                     "actionmanager.js",
                                     "anchormanager.js",
                                     "foldermanager.js",
                                     "eventhandler.js",
                                     "socketmanager.js"
                )
                .RegisterBodyScripts(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                                     "Controls/CreateMenu/createmenu.js",
                                     "Controls/ConvertFile/convertfile.js",
                                     "Controls/ConvertFile/confirmconvert.js",
                                     "Controls/EmptyFolder/emptyfolder.js",
                                     "Controls/ChunkUploadDialog/chunkuploadmanager.js",
                                     "Controls/Tree/treebuilder.js",
                                     "Controls/Tree/tree.js"
                )
                .RegisterClientScript(new Files.Masters.ClientScripts.FilesLocalizationResources())
                .RegisterClientScript(new Files.Masters.ClientScripts.FilesConstantsResources());
        }
    }
}