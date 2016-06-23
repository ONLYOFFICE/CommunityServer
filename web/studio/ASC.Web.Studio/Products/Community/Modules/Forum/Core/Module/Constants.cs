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
using ASC.Common.Security.Authorizing;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using Action = ASC.Common.Security.Authorizing.Action;

namespace ASC.Forum.Module
{
    public sealed class Constants
    {
        public static readonly Action TopicCreateAction = new Action(
                                                        new Guid("{49AE8915-2B30-4348-AB74-B152279364FB}"),
                                                        "Add topic"
                                                    );

        public static readonly Action PollCreateAction = new Action(
                                                        new Guid("{13E30B51-5B4D-40a5-8575-CB561899EEB1}"),
                                                        "Add poll"
                                                    );

        public static readonly Action TopicStickyAction = new Action(
                                                        new Guid("{6D50336A-0418-41c0-AF56-282F8AF39C59}"),
                                                        "Attach/Detach topic"
                                                    );
        public static readonly Action TopicCloseAction = new Action(
                                                       new Guid("{C333B9B9-EE06-4b11-8ED0-322B8D2ADCB6}"),
                                                       "Open/Close topic"
                                                   );
        public static readonly Action TopicEditAction = new Action(
                                                       new Guid("{43C7CB5E-38D0-495d-ABD7-E50CFEEA7DD2}"),
                                                       "Edit topic"
                                                   );
        public static readonly Action TopicDeleteAction = new Action(
                                                       new Guid("{EA5CC1C4-AFE1-42d1-94C7-6A45154FC2B7}"),
                                                       "Remove topic"
                                                   );
        public static readonly Action PollVoteAction = new Action(
                                                       new Guid("{E37239BD-C5B5-4f1e-A9F8-3CEEAC209615}"),
                                                       "Vote"
                                                   );
        public static readonly Action PostCreateAction = new Action(
                                                       new Guid("{63E9F35F-6BB5-4fb1-AFAA-E4C2F4DEC9BD}"),
                                                       "Add post"
                                                   );
        public static readonly Action ReadPostsAction = new Action(
                                                       new Guid("{E0759A42-47F0-4763-A26A-D5AA665BEC35}"),
                                                       "Read post"
                                                   );

        public static readonly Action PostEditAction = new Action(
                                                       new Guid("{D7CDB020-288B-41e5-A857-597347618533}"),
                                                       "Edit post"
                                                   );
        public static readonly Action PostDeleteAction = new Action(
                                                       new Guid("{662F3DB7-9BC8-42cf-84DA-2765F563E9B0}"),
                                                       "Remove post"
                                                   );
        public static readonly Action PostApproveAction = new Action(
                                                       new Guid("{D11EBCB9-0E6E-45e6-A6D0-99C41D687598}"),
                                                       "Confirm post"
                                                   );
        public static readonly Action TagCreateAction = new Action(
                                                      new Guid("{9018C001-24C2-44bf-A1DB-D1121A570E74}"),
                                                      "Add tag"
                                                  );
        public static readonly Action AttachmentCreateAction = new Action(
                                                      new Guid("{D1F3B53D-D9E2-4259-80E7-D24380978395}"),
                                                      "Attach file"
                                                  );
        public static readonly Action AttachmentDeleteAction = new Action(
                                                      new Guid("{C62A9E8D-B24C-4513-90AA-7FF0F8BA38EB}"),
                                                      "Remove attached file"
                                                  );

        public static readonly Action ForumManagementAction = new Action(
                                                      new Guid("{73E324F2-F2A6-4548-BF3F-5884F3713E9C}"),
                                                      "Forum administration"
                                                  );

        /// <summary>
        /// admin
        /// </summary>
        public static readonly Action TagManagementAction = new Action(
                                                      new Guid("{1F3BB856-CDF3-479b-B9B9-651871F25105}"),
                                                      "Manage tags"
                                                  );

        public static INotifyAction NewPostInTopic = new NotifyAction("new post in topic", "new post in topic");

        public static INotifyAction NewPostInThread = new NotifyAction("new post in thread", "new post in thread");

        public static INotifyAction NewPostByTag = new NotifyAction("new post by tag", "new post by tag");

        public static INotifyAction NewTopicInForum = new NotifyAction("new topic in forum", "new topic in forum");

        public static string TagTopicTitle = "TopicTitle";

        public static string TagThreadTitle = "ThreadTitle";

        public static string TagTopicURL = "TopicURL";

        public static string TagPostURL = "PostURL";

        public static string TagThreadURL = "ThreadURL";

        public static string TagPostText = "PostText";

        public static string TagUserName = "UserName";

        public static string TagTagName = "TagName";

        public static string TagTagURL = "TagURL";

        public static string TagDate = "Date";

        public static string TagUserURL = "UserURL";
    }
}
