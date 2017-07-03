/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ASC.Notify.Recipients;

namespace ASC.Core.Users
{
    [Serializable]
    public sealed class UserInfo : IDirectRecipient, ICloneable
    {
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
            get { return ActivationStatus == EmployeeActivationStatus.Activated; }
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


        internal string ContactsToString()
        {
            if (Contacts.Count == 0) return null;
            var sBuilder = new StringBuilder();
            foreach (var contact in Contacts)
            {
                sBuilder.AppendFormat("{0}|", contact);
            }
            return sBuilder.ToString();
        }

        internal UserInfo ContactsFromString(string contacts)
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