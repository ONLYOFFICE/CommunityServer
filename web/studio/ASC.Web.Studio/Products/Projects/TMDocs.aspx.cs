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


using ASC.Projects.Engine;
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
            var mainContent = (MainContent) LoadControl(MainContent.Location);
            mainContent.FolderIDCurrentRoot = Project == null ? Global.FolderProjects : EngineFactory.FileEngine.GetRoot(Project.ID);
            mainContent.TitlePage = ProjectsCommonResource.ModuleName;
            CommonContainerHolder.Controls.Add(mainContent);

            Title = HeaderStringHelper.GetPageTitle(ProjectsFileResource.Files);

            Page
                .RegisterStyle(PathProvider.GetFileStaticRelativePath("common.css"))
                .RegisterStyle(r => FilesLinkUtility.FilesBaseAbsolutePath + r,
                               "controls/maincontent/maincontent.css",
                               "controls/contentlist/contentlist.css",
                               "controls/accessrights/accessrights.css",
                               "controls/fileviewer/fileviewer.css",
                               "controls/thirdparty/thirdparty.css",
                               "controls/convertfile/convertfile.css",
                               "controls/emptyfolder/emptyfolder.css",
                               "controls/chunkuploaddialog/chunkuploaddialog.css",
                               "controls/tree/treebuilder.css",
                               "controls/tree/tree.css"
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
                                     "controls/createmenu/createmenu.js",
                                     "controls/fileviewer/fileviewer.js",
                                     "controls/convertfile/convertfile.js",
                                     "controls/emptyfolder/emptyfolder.js",
                                     "controls/chunkuploaddialog/chunkuploadmanager.js",
                                     "controls/tree/treebuilder.js",
                                     "controls/tree/tree.js"
                )
                .RegisterClientScript(new Files.Masters.ClientScripts.FilesLocalizationResources())
                .RegisterClientScript(new Files.Masters.ClientScripts.FilesConstantsResources());
        }
    }
}