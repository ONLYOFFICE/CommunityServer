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
using ASC.Web.Community.Forum.Resources;
using ASC.Web.Studio;
using ASC.Web.UserControls.Forum;
using ASC.Web.UserControls.Forum.Common;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Community.Forum
{
    public partial class NewPost : MainPage
    {
        private NewPostControl _newPostControl;

        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.NewPost);

            _newPostControl = LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/NewPostControl.ascx") as NewPostControl;
            _newPostControl.SettingsID = ForumManager.Settings.ID;
            _newPostHolder.Controls.Add(_newPostControl);
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            var caption =
                (Master as ForumMasterPage).CurrentPageCaption =
                _newPostControl.PostType == NewPostType.Post
                    ? ForumResource.NewPostButton
                    : _newPostControl.PostType == NewPostType.Topic
                          ? ForumResource.NewTopicButton :
                          ForumResource.NewPollButton;

            (Master as ForumMasterPage).CurrentPageCaption = caption;
            Title = HeaderStringHelper.GetPageTitle(caption);
        }
    }
}