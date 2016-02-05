/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System.Web;
using ASC.Projects.Engine;
using ASC.Web.Files.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

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
            var mainContent = (MainContent)LoadControl(MainContent.Location);
            mainContent.FolderIDCurrentRoot = Project == null ? Files.Classes.Global.FolderProjects : EngineFactory.FileEngine.GetRoot(Project.ID);
            mainContent.TitlePage = ProjectsCommonResource.ModuleName;
            CommonContainerHolder.Controls.Add(mainContent);

            Master.DisabledEmptyScreens = true;

            Title = HeaderStringHelper.GetPageTitle(ProjectsFileResource.Files);

            Page.RegisterStyleControl("~/products/files/masters/styles.ascx");
            Page.RegisterBodyScriptsControl("~/products/files/masters/FilesScripts.ascx");
            Page.RegisterClientLocalizationScript(typeof(Files.Masters.ClientScripts.FilesLocalizationResources));
            Page.RegisterClientScript(typeof(Files.Masters.ClientScripts.FilesConstantsResources));
            Page.RegisterInlineScript("if (typeof ZeroClipboard != 'undefined') {ZeroClipboard.setMoviePath('" + CommonLinkUtility.ToAbsolute("~/js/flash/zeroclipboard/zeroclipboard10.swf") + "');}");
        }
    }
}