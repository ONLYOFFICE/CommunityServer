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

namespace ASC.Common.Security.Authorizing
{
    [Serializable]
    public sealed class Role : IRole
    {
        public const string Everyone = "Everyone";
        public const string Visitors = "Visitors";
        public const string Users = "Users";
        public const string Administrators = "Administrators";
        public const string System = "System";


        public Guid ID { get; internal set; }

        public string Name { get; internal set; }

        public string AuthenticationType
        {
            get { return "ASC"; }
        }

        public bool IsAuthenticated
        {
            get { return false; }
        }


        public Role(Guid id, string name)
        {
            if (id == Guid.Empty) throw new ArgumentException("id");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            ID = id;
            Name = name;
        }


        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var r = obj as Role;
            return r != null && r.ID == ID;
        }

        public override string ToString()
        {
            return string.Format("Role: {0}", Name);
        }
    }
}