/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Forum;
using ASC.Specific;

namespace ASC.Api.Forums
{
    [DataContract(Name = "topic", Namespace = "")]
    public class ForumTopicWrapper : IApiSortableDate
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public string Title { get; set; }

        [DataMember(Order = 3)]
        public ApiDateTime Created { get; set; }

        private ApiDateTime _updated;

        [DataMember(Order = 3)]
        public ApiDateTime Updated
        {
            get { return _updated >= Created ? _updated : Created; }
            set { _updated = value; }
        }

        [DataMember(Order = 8)]
        public string Text { get; set; }

        [DataMember(Order = 9)]
        public EmployeeWraper UpdatedBy { get; set; }

        [DataMember(Order = 10)]
        public string ThreadTitile { get; set; }
               

        public ForumTopicWrapper(Topic topic)
        {
            Id = topic.ID;
            Title = topic.Title;
            Created = (ApiDateTime)topic.CreateDate;
            Text = ASC.Common.Utils.HtmlUtil.GetText(topic.RecentPostText, 160);
            Updated = (ApiDateTime)topic.RecentPostCreateDate;
            UpdatedBy = EmployeeWraper.Get(topic.RecentPostAuthorID);
            Status = topic.Status;
            Type = topic.Type;
            Tags = topic.Tags.Where(x => x.IsApproved).Select(x => x.Name).ToList();
            ThreadTitile = topic.ThreadTitle;
        }

        protected ForumTopicWrapper()
        {

        }

        [DataMember(Order = 30)]
        public TopicStatus Status { get; set; }
        [DataMember(Order = 30)]
        public TopicType Type { get; set; }

        [DataMember(Order = 100)]
        public List<string> Tags { get; set; }

        public static ForumTopicWrapper GetSample()
        {
            return new ForumTopicWrapper()
                       {
                           Created = ApiDateTime.GetSample(),
                           Updated = ApiDateTime.GetSample(),
                           Id = 10,
                           UpdatedBy = EmployeeWraper.GetSample(),
                           Text = "This is sample post",
                           Status = TopicStatus.Sticky,
                           Tags = new List<string> { "Tag1", "Tag2" },
                           Title = "Sample topic",
                           Type = TopicType.Informational
                       };
        }
    }
}