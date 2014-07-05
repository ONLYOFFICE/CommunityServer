/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System.Web;
using ASC.Web.Projects.Classes;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects
{
    public partial class GanttChart : BasePage
    {
        protected override bool CanRead { get { return !Request.Browser.IsMobileDevice; } }

        protected override void PageLoad()
        {
            Master.DisabledHelpTour = true;
            Master.DisabledSidePanel = true;
            Master.DisabledPrjNavPanel = true;
            Master.DisabledEmptyScreens = true;

            _hintPopupTaskRemove.Options.IsPopup = true;
            _hintPopupMilestoneRemove.Options.IsPopup = true;
            _hintPopupMilestoneTasks.Options.IsPopup = true;
            _hintPopupTaskWithSubtasks.Options.IsPopup = true;
            _newLinkError.Options.IsPopup = true;
            _moveTaskOutMilestone.Options.IsPopup = true;
            _addNewLinkPopup.Options.IsPopup = true;

            _taskAction.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Tasks/TaskAction.ascx")));
            _milestoneAction.Controls.Add(LoadControl(PathProvider.GetFileStaticRelativePath("Milestones/MilestoneAction.ascx")));

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/products/projects/app_themes/default/css/common.css"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/products/projects/app_themes/default/css/ganttchart.css"));

            Title = HeaderStringHelper.GetPageTitle(ProjectResource.GanttGart);
        }
    }
}