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

            CanEdit = ProjectSecurity.CanEdit(message);
            CommentsCount = message.CommentsCount;
            Status = (int)message.Status;
            CanCreateComment = ProjectSecurity.CanCreateComment(message);
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