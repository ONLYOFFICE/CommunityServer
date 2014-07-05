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
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Projects.Core.Domain;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "comment", Namespace = "")]
    public class CommentWrapper : IApiSortableDate
    {
        [DataMember(Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Order = 12)]
        public Guid ParentId { get; set; }

        [DataMember(Order = 10)]
        public string Text { get; set; }

        [DataMember(Order = 50)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 9)]
        public EmployeeWraper CreatedBy { get; set; }

        [DataMember(Order = 10)]
        public string ProjectTitle { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = false)]
        public ApiDateTime Updated { get; set; }

        [DataMember(Order = 13)]
        public bool Inactive { get; set; }

        private CommentWrapper()
        {
        }

        public CommentWrapper(Comment comment)
        {
            Id = comment.ID;
            ParentId = comment.Parent;
            Text = comment.Content;
            Created = Updated = (ApiDateTime)comment.CreateOn;
            CreatedBy = EmployeeWraper.Get(comment.CreateBy);
            Inactive = comment.Inactive;
        }


        public static CommentWrapper GetSample()
        {
            return new CommentWrapper
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.NewGuid(),
                    Text = "comment text",
                    Created = (ApiDateTime)DateTime.UtcNow,
                    CreatedBy = EmployeeWraper.GetSample(),
                    Updated = (ApiDateTime)DateTime.Now,
                    Inactive = false
                };
        }
    }
}