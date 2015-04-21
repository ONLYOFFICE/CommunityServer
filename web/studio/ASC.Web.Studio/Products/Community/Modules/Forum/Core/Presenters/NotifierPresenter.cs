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
using System.Collections.Generic;
using ASC.Common.Web;
using ASC.Core.Common.Notify;
using ASC.Forum.Module;
using ASC.Notify.Patterns;
using ASC.Web.Studio.Core.Notify;

namespace ASC.Forum
{
    internal class NotifierPresenter : PresenterTemplate<INotifierView>
    {
        protected override void RegisterView()
        {
            _view.SendNotify += new EventHandler<NotifyEventArgs>(SendNotifyHandler);
        }

        private void SendNotifyHandler(object sender, NotifyEventArgs e)
        {
            
            if (String.Equals(e.NotifyAction.ID, Constants.NewPostInTopic.ID, StringComparison.InvariantCultureIgnoreCase))
            {

                ForumNotifyClient.NotifyClient.SendNoticeAsync(Constants.NewPostInTopic, e.ObjectID, null,
                                                               new TagValue(Constants.TagDate, e.Date),
                                                               new TagValue(Constants.TagThreadTitle, e.ThreadTitle),
                                                               new TagValue(Constants.TagTopicTitle, e.TopicTitle),
                                                               new TagValue(Constants.TagTopicURL, e.TopicURL),
                                                               new TagValue(Constants.TagPostURL, e.PostURL),
                                                               new TagValue(Constants.TagThreadURL, e.ThreadURL),
                                                               new TagValue(Constants.TagPostText, e.PostText),
                                                               new TagValue(Constants.TagUserURL, e.UserURL),
                                                               new TagValue(Constants.TagUserName, e.Poster.ToString()),
                                                               ReplyToTagProvider.Comment("forum.topic", e.TopicId.ToString(), e.PostId.ToString()));


            }


            else if (String.Equals(e.NotifyAction.ID, Constants.NewPostInThread.ID, StringComparison.InvariantCultureIgnoreCase))
            {
                ForumNotifyClient.NotifyClient.SendNoticeAsync(Constants.NewPostInThread, e.ObjectID, null,
                                                               new TagValue(Constants.TagDate, e.Date),
                                                               new TagValue(Constants.TagThreadTitle, e.ThreadTitle),
                                                               new TagValue(Constants.TagTopicTitle, e.TopicTitle),
                                                               new TagValue(Constants.TagTopicURL, e.TopicURL),
                                                               new TagValue(Constants.TagPostURL, e.PostURL),
                                                               new TagValue(Constants.TagThreadURL, e.ThreadURL),
                                                               new TagValue(Constants.TagPostText, e.PostText),
                                                               new TagValue(Constants.TagUserURL, e.UserURL),
                                                               new TagValue(Constants.TagUserName, e.Poster.ToString()),
                                                               ReplyToTagProvider.Comment("forum.topic", e.TopicId.ToString(), e.PostId.ToString()));


            }

            else if (String.Equals(e.NotifyAction.ID, Constants.NewPostByTag.ID, StringComparison.InvariantCultureIgnoreCase))
            {   
                ForumNotifyClient.NotifyClient.SendNoticeAsync(Constants.NewPostByTag, e.ObjectID, null,
                                                               new TagValue(Constants.TagDate, e.Date),
                                                                new TagValue(Constants.TagThreadTitle, e.ThreadTitle),
                                                                new TagValue(Constants.TagTopicTitle, e.TopicTitle),
                                                                new TagValue(Constants.TagTopicURL, e.TopicURL),
                                                                new TagValue(Constants.TagPostURL, e.PostURL),
                                                                new TagValue(Constants.TagThreadURL, e.ThreadURL),
                                                                new TagValue(Constants.TagPostText, e.PostText),
                                                                new TagValue(Constants.TagUserURL, e.UserURL),
                                                                new TagValue(Constants.TagUserName, e.Poster.ToString()));

            }

            else if (String.Equals(e.NotifyAction.ID, Constants.NewTopicInForum.ID, StringComparison.InvariantCultureIgnoreCase))
            {   
                ForumNotifyClient.NotifyClient.SendNoticeAsync(Constants.NewTopicInForum, e.ObjectID, null,
                                                                new TagValue(Constants.TagDate, e.Date),
                                                                new TagValue(Constants.TagThreadTitle, e.ThreadTitle),
                                                                new TagValue(Constants.TagTopicTitle, e.TopicTitle),
                                                                new TagValue(Constants.TagTopicURL, e.TopicURL),
                                                                new TagValue(Constants.TagPostURL, e.PostURL),
                                                                new TagValue(Constants.TagThreadURL, e.ThreadURL),
                                                                new TagValue(Constants.TagPostText, e.PostText),
                                                                new TagValue(Constants.TagTagName, e.TagName),
                                                                new TagValue(Constants.TagTagURL, e.TagURL),
                                                                new TagValue(Constants.TagUserURL, e.UserURL),
                                                                new TagValue(Constants.TagUserName, e.Poster.ToString()),
                                                                ReplyToTagProvider.Comment("forum.topic", e.TopicId.ToString()));


            }
        }
    }
}
