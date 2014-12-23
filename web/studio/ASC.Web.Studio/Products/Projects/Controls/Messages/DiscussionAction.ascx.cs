/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
using ASC.Web.Studio.Utility.HtmlUtility;
using AjaxPro;

namespace ASC.Web.Projects.Controls.Messages
{
    [AjaxNamespace("AjaxPro.DiscussionAction")]
    public partial class DiscussionAction : BaseUserControl
    {       
        public Project Project { get { return Page.Project; } }
        public Message Discussion { get; set; }
        public UserInfo Author { get; set; }
        public string Text { get; set; }
        public int ProjectFolderId { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof(DiscussionAction), Page);
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/usercontrols/common/ckeditor/ckeditor-connector.js"));

            Text = "";
            Author = CoreContext.UserManager.GetUsers(Page.Participant.ID);

            if (Discussion != null)
            {
                discussionTitle.Text = Discussion.Title;
                Text = Discussion.Content;
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

        [AjaxMethod]
        public string GetDiscussionPreview(string html)
        {
            return HtmlUtility.GetFull(html);
        }
    }
}
