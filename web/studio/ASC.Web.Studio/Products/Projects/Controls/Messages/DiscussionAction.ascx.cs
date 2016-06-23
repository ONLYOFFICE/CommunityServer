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


using System;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Core.Users;

using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Resources;


namespace ASC.Web.Projects.Controls.Messages
{
    public partial class DiscussionAction : BaseUserControl
    {       
        public Project Project { get { return Page.Project; } }
        public Message Discussion { get; set; }
        public UserInfo Author { get; set; }
        public string Text { get; set; }
        public int ProjectFolderId { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/usercontrols/common/ckeditor/ckeditor-connector.js");

            Text = "";
            Author = CoreContext.UserManager.GetUsers(Page.Participant.ID);

            if (Discussion != null)
            {
                discussionTitle.Text = Discussion.Title;
                Text = Discussion.Description;
            }
        }

        protected string GetPageTitle()
        {
            return Discussion == null ? MessageResource.CreateDiscussion : MessageResource.EditMessage;
        }

        protected string GetDiscussionAction()
        {
            var innerHTML = new StringBuilder();
            var discussionId = Discussion == null ? -1 : Discussion.ID;
            var action = Discussion == null ? MessageResource.AddDiscussion : ProjectsCommonResource.SaveChanges;

            innerHTML.AppendFormat("<a id='discussionActionButton' class='button blue big' discussionId='{0}'>{1}</a>", 
                                    discussionId, action);
            innerHTML.AppendFormat(" <span class=\"splitter-buttons\"></span>");
            innerHTML.AppendFormat("<a id='discussionPreviewButton' class='button blue big {5}' authorName='{0}' authorAvatarUrl='{1}' authorTitle='{2}' authorPageUrl='{3}'>{4}</a>",
                Author.DisplayUserName(), Author.GetBigPhotoURL(), Author.Title.HtmlEncode(), Author.GetUserProfilePageURL(), ProjectsCommonResource.Preview, string.IsNullOrEmpty(Text) ? "disable" : "");
            innerHTML.AppendFormat(" <span class=\"splitter-buttons\"></span>");
            innerHTML.AppendFormat("<a id='discussionCancelButton' class='button gray big'>{0}</a>",
                                   ProjectsCommonResource.Cancel);

            return innerHTML.ToString();
        }

        protected bool CanReadDiscussion(Guid id)
        {
            return ProjectSecurity.CanRead(Discussion, id);
        }
    }
}
