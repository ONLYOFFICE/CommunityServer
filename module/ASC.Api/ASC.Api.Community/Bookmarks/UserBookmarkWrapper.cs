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
using System.Text;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Specific;
using ASC.Bookmarking.Pojo;

namespace ASC.Api.Bookmarks
{
    [DataContract(Name = "userbookmark", Namespace = "")]
    public class UserBookmarkWrapper
    {
        [DataMember(Order = 0)]
        public long BookmarkId { get; set; }

        [DataMember(Order = 1)]
        public EmployeeWraper CreatedBy { get; set; }

        [DataMember(Order = 1)]
        public ApiDateTime Created { get; set; }

        [DataMember(Order = 2)]
        public string Title { get; set; }

        [DataMember(Order = 3)]
        public string Description { get; set; }

        public UserBookmarkWrapper(UserBookmark userBookmark)
        {
            BookmarkId = userBookmark.BookmarkID;
            CreatedBy = EmployeeWraper.Get(Core.CoreContext.UserManager.GetUsers(userBookmark.UserID));
            Created = (ApiDateTime) userBookmark.DateAdded;
            Title = userBookmark.Name;
            Description = userBookmark.Description;
        }
    }
}
