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
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Web.Studio.Core.Statistic
{
    public class UserVisit
    {
        public virtual int TenantID { get; set; }

        public virtual DateTime VisitDate { get; set; }

        public virtual DateTime? FirstVisitTime { get; set; }

        public virtual DateTime? LastVisitTime { get; set; }

        public virtual Guid UserID { get; set; }

        public virtual UserInfo User
        {
            get { return CoreContext.UserManager.GetUsers(UserID); }
        }

        public virtual Guid ProductID { get; set; }

        public virtual int VisitCount { get; set; }
    }
}