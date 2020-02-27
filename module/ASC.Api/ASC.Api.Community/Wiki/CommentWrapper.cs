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
using System.Text;
using ASC.Specific;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Api.Employee;

namespace ASC.Api.Wiki.Wrappers
{
    [DataContract(Name = "comment", Namespace = "")]
    public class CommentWrapper
    {
        [DataMember(Order = 0)]
        public Guid Id { get; set; }

        [DataMember(Order = 1)]
        public Guid ParentId { get; set; }

        [DataMember(Order = 2)]
        public string Page { get; set; }

        [DataMember(Order = 3)]
        public string Content { get; set; }

        [DataMember(Order = 4)]
        public EmployeeWraper Author { get; set; }

        [DataMember(Order = 4)]
        public ApiDateTime LastModified { get; set; }

        [DataMember(Order = 5)]
        public bool Inactive { get; set; }

        public CommentWrapper(Comment comment)
        {
            Id = comment.Id;
            ParentId = comment.ParentId;
            Page = comment.PageName;
            Content = comment.Body;
            Author = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(comment.UserId));
            LastModified = (ApiDateTime)comment.Date;
            Inactive = comment.Inactive;
        }

        public CommentWrapper()
        {
            
        }

        public static CommentWrapper GetSample()
        {
            return new CommentWrapper
                       {
                           Author = EmployeeWraper.GetSample(),
                           Content = "Comment content",
                           Id = Guid.Empty,
                           Page = "Some page",
                           Inactive = false,
                           LastModified = ApiDateTime.GetSample(),
                           ParentId = Guid.Empty
                       };
        }
    }
}
