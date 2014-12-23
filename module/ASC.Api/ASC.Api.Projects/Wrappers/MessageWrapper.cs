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
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "message", Namespace = "")]
    public class MessageWrapper : IApiSortableDate
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 14)]
        public SimpleProjectWrapper ProjectOwner { get; set; }

        [DataMember(Order = 9)]
        public string Title { get; set; }

        [DataMember(Order = 10)]
        public string Text { get; set; }

        [DataMember(Order = 11)]
        public MessageStatus Status { get; set; }

        [DataMember(Order = 50)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 51)]
        public EmployeeWraper CreatedBy { get; set; }

        private ApiDateTime updated;

        [DataMember(Order = 50)]
        public ApiDateTime Updated
        {
            get { return updated >= Created ? updated : Created; }
            set { updated = value; }
        }

        [DataMember(Order = 41)]
        public EmployeeWraper UpdatedBy { get; set; }

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember(Order = 15)]
        public int CommentsCount { get; set; }


        private MessageWrapper()
        {
        }

        public MessageWrapper(Message message)
        {
            Id = message.ID;
            if (message.Project != null)
            {
                ProjectOwner = new SimpleProjectWrapper(message.Project);
            }
            Title = message.Title;
            Text = message.Content;
            Created = (ApiDateTime)message.CreateOn;
            CreatedBy = new EmployeeWraperFull(CoreContext.UserManager.GetUsers(message.CreateBy));
            Updated = (ApiDateTime)message.LastModifiedOn;
            if (message.CreateBy != message.LastModifiedBy)
            {
                UpdatedBy = EmployeeWraper.Get(message.LastModifiedBy);
            }
            CanEdit = ProjectSecurity.CanEdit(message);
            CommentsCount = message.CommentsCount;
            Status = message.Status;
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