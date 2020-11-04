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

namespace ASC.Core
{
    public class Group
    {
        public Guid Id
        {
            get;
            set;
        }

        public Guid ParentId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Guid CategoryId
        {
            get;
            set;
        }

        public bool Removed
        {
            get;
            set;
        }

        public DateTime LastModified
        {
            get;
            set;
        }

        public int Tenant
        {
            get;
            set;
        }

        public string Sid
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var g = obj as Group;
            return g != null && g.Id == Id;
        }
    }
}
