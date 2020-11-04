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
using System.Globalization;
using System.Text;
using System.Linq;
using ASC.Collections;
using ASC.Notify.Recipients;

namespace ASC.Core.Users
{
    [Serializable]
    public sealed class UserInfo : IDirectRecipient, ICloneable
    {
        private readonly HttpRequestDictionary<GroupInfo[]> groupCache = new HttpRequestDictionary<GroupInfo[]>("UserInfo-Groups");
        private readonly HttpRequestDictionary<IEnumerable<Guid>> groupCacheId = new HttpRequestDictionary<IEnumerable<Guid>>("UserInfo-Groups1");

        public UserInfo()
        {
            Status = EmployeeStatus.Active;
            ActivationStatus = EmployeeActivationStatus.NotActivated;
            Contacts = new List<string>();
            LastModified = DateTime.UtcNow;
        }


        public Guid ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public DateTime? BirthDate { get; set; }

        public bool? Sex { get; set; }

        public EmployeeStatus Status { get; set; }

        public EmployeeActivationStatus ActivationStatus { get; set; }

        public DateTime? TerminatedDate { get; set; }

        public string Title { get; set; }

        public DateTime? WorkFromDate { get; set; }

        public string Email { get; set; }

        public List<string> Contacts { get; set; }

        public string Location { get; set; }

        public string Notes { get; set; }

        public bool Removed { get; set; }

        public DateTime LastModified { get; set; }

        public int Tenant { get; set; }

        public bool IsActive
        {
            get { return ActivationStatus.HasFlag(EmployeeActivationStatus.Activated); }
        }

        public string CultureName { get; set; }

        public string MobilePhone { get; set; }

        public MobilePhoneActivationStatus MobilePhoneActivationStatus { get; set; }

        public string Sid { get; set; } // LDAP user identificator

        public string SsoNameId { get; set; } // SSO SAML user identificator
        public string SsoSessionId { get; set; } // SSO SAML user session identificator

        public DateTime CreateDate { get; set; }

        public override string ToString()
        {
            return String.Format("{0} {1}", FirstName, LastName).Trim();
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var ui = obj as UserInfo;
            return ui != null && ID.Equals(ui.ID);
        }

        public CultureInfo GetCulture()
        {
            return string.IsNullOrEmpty(CultureName) ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(CultureName);
        }


        string[] IDirectRecipient.Addresses
        {
            get { return !string.IsNullOrEmpty(Email) ? new[] {Email} : new string[0]; }
        }

        public bool CheckActivation
        {
            get { return !IsActive; /*if user already active we don't need activation*/ }
        }

        string IRecipient.ID
        {
            get { return ID.ToString(); }
        }

        string IRecipient.Name
        {
            get { return ToString(); }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        internal GroupInfo[] GetGroups(IncludeType includeType, Guid? categoryId)
        {
            var groups = groupCache.Get(ID.ToString(), () => CoreContext.UserManager.GetUserGroups(ID, IncludeType.Distinct, null));

            if (categoryId.HasValue)
            {
                return groups.Where(r => r.CategoryID.Equals(categoryId.Value)).ToArray();
            }

            return groups;
        }

        internal IEnumerable<Guid> GetUserGroupsId()
        {
            return groupCacheId.Get(ID.ToString(), () => CoreContext.UserManager.GetUserGroupsGuids(ID));
        }

        internal void ResetGroupCache()
        {
            groupCache.Reset(ID.ToString());
        }

        public string ContactsToString()
        {
            if (Contacts.Count == 0) return null;
            var sBuilder = new StringBuilder();
            foreach (var contact in Contacts)
            {
                sBuilder.AppendFormat("{0}|", contact);
            }
            return sBuilder.ToString();
        }

        public UserInfo ContactsFromString(string contacts)
        {
            if (string.IsNullOrEmpty(contacts)) return this;
            Contacts.Clear();
            foreach (var contact in contacts.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries))
            {
                Contacts.Add(contact);
            }
            return this;
        }
    }
}