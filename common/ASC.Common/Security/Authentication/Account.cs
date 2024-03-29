/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

namespace ASC.Common.Security.Authentication
{
    [Serializable]
    public class Account : IAccount
    {
        public Account(Guid id, string name, bool authenticated)
        {
            ID = id;
            Name = name;
            IsAuthenticated = authenticated;
        }

        #region IAccount Members

        public Guid ID { get; private set; }

        public string Name { get; private set; }


        public object Clone()
        {
            return MemberwiseClone();
        }

        public string AuthenticationType
        {
            get { return "ASC"; }
        }

        public virtual bool IsAuthenticated
        {
            get;
            private set;
        }

        #endregion

        public override bool Equals(object obj)
        {
            var a = obj as IAccount;
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