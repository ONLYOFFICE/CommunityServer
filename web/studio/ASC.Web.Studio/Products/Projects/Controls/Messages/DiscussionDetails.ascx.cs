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


using System;
using System.Globalization;
using System.Linq;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Mobile;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.UserControls.Common.Attachments;

namespace ASC.Web.Projects.Controls.Messages
{
    public partial class DiscussionDetails : BaseCommentControl<Message>
    {
        public Message Discussion { get; set; }

        public Project Project
        {
            get { return Page.Project; }
        }

        public UserInfo Author { get; set; }

        public bool CanReadFiles { get; set; }
        public bool CanEditFiles { get; set; }
        public bool CanEdit { get; set; }

        public int FilesCount { get; private set; }
        public int ParticipiantCount { get; private set; }
        public int ProjectFolderId { get; set; }

        protected bool FilesAvailable { get; set; }
        protected bool CommentsAvailable { get; set; }

        protected string CommentsCountTitle { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            _hintPopup.Options.IsPopup = true;

            ParticipiantCount = Page.EngineFactory.MessageEngine.GetSubscribers(Discussion).Count;

            CanEdit = ProjectSecurity.CanEdit(Discussion);
            CanReadFiles = ProjectSecurity.CanReadFiles(Discussion.Project);
            CanEditFiles = ProjectSecurity.IsInTeam(Project) && Discussion.Status == MessageStatus.Open;
            Author = CoreContext.UserManager.GetUsers(Discussion.CreateBy);
            FilesCount = Page.EngineFactory.MessageEngine.GetFiles(Discussion).Count();
            FilesAvailable = CanReadFiles && !MobileDetector.IsMobile && (CanEditFiles || FilesCount > 0);
            CommentsAvailable = Discussion.Status == MessageStatus.Open || Discussion.CommentsCount > 0;

            CommentsCountTitle = Discussion.CommentsCount != 0 ? Discussion.CommentsCount.ToString(CultureInfo.InvariantCulture) : "";

            if (FilesAvailable)
            {
                LoadDiscussionFilesControl();
            }

            if (Discussion.Status == MessageStatus.Archived)
            {
                Page.EssenceStatus = MessageResource.ArchiveDiscussionStatus;
            }

            if (CommentsAvailable)
            {
                InitCommentBlock(discussionComments, Discussion);
            }
            else
            {
                discussionComments.Visible = false;
            }
        }

        #region LoadControls

        private void LoadDiscussionFilesControl()
        {
            ProjectFolderId = (int)Page.EngineFactory.FileEngine.GetRoot(Project.ID);

            var discussionFilesControl = (Attachments)LoadControl(Attachments.Location);
            discussionFilesControl.EmptyScreenVisible = false;
            discussionFilesControl.EntityType = "message";
            discussionFilesControl.ModuleName = "projects";
            discussionFilesControl.CanAddFile = CanEditFiles;
            discussionFilesPlaceHolder.Controls.Add(discussionFilesControl);
        }

        #endregion

        #region Tabs

        protected string GetTabTitle(int count, string defaultTitle)
        {
            return count > 0 ? string.Format("{0} ({1})", defaultTitle, count) : defaultTitle;
        }

        #endregion
    }
}