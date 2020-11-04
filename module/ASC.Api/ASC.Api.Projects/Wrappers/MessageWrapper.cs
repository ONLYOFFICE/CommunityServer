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


using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "message", Namespace = "")]
    public class MessageWrapper : ObjectWrapperFullBase
    {
        [DataMember(Order = 14)]
        public SimpleProjectWrapper ProjectOwner { get; set; }

        [DataMember(Order = 20)]
        public string Text { get; set; }

        [DataMember]
        public bool CanCreateComment { get; set; }

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember(Order = 15)]
        public int CommentsCount { get; set; }


        private MessageWrapper()
        {
        }

        public MessageWrapper(ProjectApiBase projectApiBase, Message message)
        {
            Id = message.ID;
            if (message.Project != null)
            {
                ProjectOwner = new SimpleProjectWrapper(message.Project);
            }
            Title = message.Title;
            Text = message.Description;
            Created = (ApiDateTime)message.CreateOn;
            Updated = (ApiDateTime)message.LastModifiedOn;

            if (projectApiBase.Context.GetRequestValue("simple") != null)
            {
                CreatedById = message.CreateBy;
                UpdatedById = message.LastModifiedBy;
            }
            else
            {
                CreatedBy = projectApiBase.GetEmployeeWraper(message.CreateBy);
                if (message.CreateBy != message.LastModifiedBy)
                {
                    UpdatedBy = projectApiBase.GetEmployeeWraper(message.LastModifiedBy);
                }
            }

            CanEdit = projectApiBase.ProjectSecurity.CanEdit(message);
            CommentsCount = message.CommentsCount;
            Status = (int)message.Status;
            CanCreateComment = projectApiBase.ProjectSecurity.CanCreateComment(message);
        }


        public static MessageWrapper GetSample()
        {
            return new MessageWrapper
                {
                    Id = 10,
                    ProjectOwner = SimpleProjectWrapper.GetSample(),
                    Title = "Sample Title",
                    Text = "Hello, this is sample message",
                    Created = ApiDateTime.GetSample(),
                    CreatedBy = EmployeeWraper.GetSample(),
                    Updated = ApiDateTime.GetSample(),
                    UpdatedBy = EmployeeWraper.GetSample(),
                    CanEdit = true,
                    CommentsCount = 5
                };
        }
    }
}