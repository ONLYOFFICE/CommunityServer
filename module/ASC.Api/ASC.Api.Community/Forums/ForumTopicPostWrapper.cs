/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
    [DataContract(Name = "post", Namespace = "")]
    public class ForumTopicPostWrapper : IApiSortableDate
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public string Subject { get; set; }

        [DataMember(Order = 2)]
        public string Text { get; set; }

        [DataMember(Order = 3)]
        public ApiDateTime Created { get; set; }

        private ApiDateTime updated;

        [DataMember(Order = 3)]
        public ApiDateTime Updated
        {
            get { return updated >= Created ? updated : Created; }
            set { updated = value; }
        }

        [DataMember(Order = 9)]
        public EmployeeWraper CreatedBy { get; set; }

        [DataMember(Order = 10)]
        public string ThreadTitle { get; set; }

        [DataMember(Order = 100, EmitDefaultValue = false)]
        public List<ForumTopicPostAttachmentWrapper> Attachments { get; set; }

        public ForumTopicPostWrapper(Post post)
        {
            Id = post.ID;
            Text = post.Text;
            Created = (ApiDateTime)post.CreateDate;
            Updated = (ApiDateTime)post.EditDate;
            Subject = post.Subject;
            CreatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(post.PosterID));
            Attachments = post.Attachments.Select(x => new ForumTopicPostAttachmentWrapper(x)).ToList();
        }

        private ForumTopicPostWrapper()
        {
        }

        public static ForumTopicPostWrapper GetSample()
        {
            return new ForumTopicPostWrapper
                {
                    Id = 123,
                    CreatedBy = EmployeeWraper.GetSample(),
                    Created = ApiDateTime.GetSample(),
                    Updated = ApiDateTime.GetSample(),
                    Subject = "Sample subject",
                    Text = "Post text",
                    Attachments = new List<ForumTopicPostAttachmentWrapper> { ForumTopicPostAttachmentWrapper.GetSample() }
                };
        }
    }
}