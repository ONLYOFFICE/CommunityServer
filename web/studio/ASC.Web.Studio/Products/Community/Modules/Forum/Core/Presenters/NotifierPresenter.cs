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

using System;
using System.Collections.Generic;
using ASC.Common.Web;
using ASC.Core.Common.Notify;
using ASC.Forum.Module;
using ASC.Notify.Patterns;

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
