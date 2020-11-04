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


using System.Collections.Generic;
using ASC.Core.Users;

namespace ASC.Web.Core.Users
{
    public class UserInfoComparer : IComparer<UserInfo>
    {
        public static readonly IComparer<UserInfo> Default = new UserInfoComparer(UserSortOrder.DisplayName, false);
        public static readonly IComparer<UserInfo> FirstName = new UserInfoComparer(UserSortOrder.FirstName, false);
        public static readonly IComparer<UserInfo> LastName = new UserInfoComparer(UserSortOrder.LastName, false);


        public UserSortOrder SortOrder { get; set; }
        public bool Descending { get; set; }


        public UserInfoComparer(UserSortOrder sortOrder)
            : this(sortOrder, false)
        {
        }

        public UserInfoComparer(UserSortOrder sortOrder, bool descending)
        {
            SortOrder = sortOrder;
            Descending = descending;
        }


        public int Compare(UserInfo x, UserInfo y)
        {
            int result = 0;
            switch (SortOrder)
            {
                case UserSortOrder.DisplayName:
                    result = UserFormatter.Compare(x, y, DisplayUserNameFormat.Default);
                    break;
                case UserSortOrder.FirstName:
                    result = UserFormatter.Compare(x, y, DisplayUserNameFormat.FirstLast);
                    break;
                case UserSortOrder.LastName:
                    result = UserFormatter.Compare(x, y, DisplayUserNameFormat.LastFirst);
                    break;
            }

            return !Descending ? result : -result;
        }
    }
}
