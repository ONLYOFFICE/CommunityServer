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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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

        ///<example>38c0f464-f1e7-493e-8d95-dc4ee8ee834a</example>
        public Guid ID { get; set; }

        ///<example>FirstName</example>
        public string FirstName { get; set; }

        ///<example>LastName</example>
        public string LastName { get; set; }

        ///<example>UserName</example>
        public string UserName { get; set; }

        ///<example>2019-07-26T00:00:00</example>
        public DateTime? BirthDate { get; set; }

        ///<example>true</example>
        public bool? Sex { get; set; }

        ///<example type="int">1</example>
        public EmployeeStatus Status { get; set; }

        ///<example type="int">1</example>
        public EmployeeActivationStatus ActivationStatus { get; set; }

        ///<example>2019-07-26T00:00:00</example>
        public DateTime? TerminatedDate { get; set; }

        ///<example>Title</example>
        public string Title { get; set; }

        ///<example>2019-07-26T00:00:00</example>
        public DateTime? WorkFromDate { get; set; }

        ///<example>Email</example>
        public string Email { get; set; }

        ///<example>Contacts</example>
        ///<collection>list</collection>
        public List<string> Contacts { get; set; }

        ///<example>Location</example>
        public string Location { get; set; }

        ///<example>Notes</example>
        public string Notes { get; set; }

        ///<example>false</example>
        public bool Removed { get; set; }

        ///<example>2019-07-26T00:00:00</example>
        public DateTime LastModified { get; set; }

        ///<example type="int">1</example>
        public int Tenant { get; set; }

        ///<example>072A8452-ADF3-40AA-B359-22DA7B8923E2</example>
        public Guid? Lead {get; set;}

        ///<example>false</example>
        public bool IsActive
        {
            get { return ActivationStatus.HasFlag(EmployeeActivationStatus.Activated); }
        }

        ///<example>CultureName</example>
        public string CultureName { get; set; }

        ///<example>MobilePhone</example>
        public string MobilePhone { get; set; }

        ///<example type="int">1</example>
        public MobilePhoneActivationStatus MobilePhoneActivationStatus { get; set; }

        ///<example>Sid</example>
        public string Sid { get; set; } // LDAP user identificator

        ///<example>LdapQouta</example>
        public long LdapQouta { get; set; } // LDAP user quota attribute

        ///<example>SsoNameId</example>
        public string SsoNameId { get; set; } // SSO SAML user identificator

        ///<example>SsoSessionId</example>
        public string SsoSessionId { get; set; } // SSO SAML user session identificator

        ///<example>2019-07-26T00:00:00</example>
        public DateTime CreateDate { get; set; }

        ///<example>UsedSpace</example>
        public long UsedSpace { get; set; }

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
            get { return !string.IsNullOrEmpty(Email) ? new[] { Email } : new string[0]; }
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
            var groups = groupCache.Get(ID.ToString(), () => {
                if (CoreContext.Configuration.Personal)
                {
                    return new GroupInfo[2] { Constants.GroupUser, Constants.GroupEveryone };
                }
                return CoreContext.UserManager.GetUserGroups(ID, IncludeType.Distinct, null);
            });

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
            foreach (var contact in contacts.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Contacts.Add(contact);
            }
            return this;
        }
    }
}