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


using ASC.Common.Security.Authorizing;
using ASC.Notify.Recipients;
using System;

namespace ASC.Core.Users
{
    [Serializable]
    public class GroupInfo : IRole, IRecipientsGroup
    {
        public Guid ID { get; internal set; }

        public string Name { get; set; }

        public Guid CategoryID { get; set; }

        public GroupInfo Parent { get; internal set; }

        public string Sid { get; set; }

        public GroupInfo()
        {
        }

        public GroupInfo(Guid categoryID)
        {
            CategoryID = categoryID;
        }


        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return ID != Guid.Empty ? ID.GetHashCode() : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var g = obj as GroupInfo;
            if (g == null) return false;
            if (ID == Guid.Empty && g.ID == Guid.Empty) return ReferenceEquals(this, g);
            return g.ID == ID;
        }


        string IRecipient.ID
        {
            get { return ID.ToString(); }
        }

        string IRecipient.Name
        {
            get { return Name; }
        }

        public string AuthenticationType
        {
            get { return "ASC"; }
        }

        public bool IsAuthenticated
        {
            get { return false; }
        }
    }
}