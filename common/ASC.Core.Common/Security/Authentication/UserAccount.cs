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
using ASC.Common.Security.Authentication;
using ASC.Core.Users;

namespace ASC.Core.Security.Authentication
{
    [Serializable]
    class UserAccount : MarshalByRefObject, IUserAccount
    {
        public Guid ID { get; private set; }

        public string Name { get; private set; }

        public string AuthenticationType { get { return "ASC"; } }

        public bool IsAuthenticated { get { return true; } }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string Title { get; private set; }

        public int Tenant { get; private set; }

        public string Email { get; private set; }


        public UserAccount(UserInfo info, int tenant)
        {
            ID = info.ID;
            Name = UserFormatter.GetUserName(info);
            FirstName = info.FirstName;
            LastName = info.LastName;
            Title = info.Title;
            Tenant = tenant;
            Email = info.Email;            
        }


        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            var a = obj as IUserAccount;
            return a != null && ID.Equals(a.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}