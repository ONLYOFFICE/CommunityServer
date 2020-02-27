/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

namespace ASC.Mail.Core.Entities
{
    public class MailboxServer : IEquatable<MailboxServer>
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public string Type { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string SocketType { get; set; }

        private string _username;
        public string Username {
            get { return _username; }
            set { _username = value ?? ""; }
        }

        private string _auth;
        public string Authentication {
            get { return _auth; }
            set {
                _auth = value ?? "";
            }
        }
        public bool IsUserData { get; set; }

        #region Equality

        public bool Equals(MailboxServer other)
        {
            if (other == null) return false;
            return string.Equals(Type, other.Type, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(Hostname, other.Hostname, StringComparison.InvariantCultureIgnoreCase) &&
                Port == other.Port &&
                string.Equals(SocketType, other.SocketType, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(Username, other.Username, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(Authentication, other.Authentication, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as MailboxServer);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Hostname.GetHashCode() ^ Port.GetHashCode() ^ SocketType.GetHashCode() ^
                   Username.GetHashCode() ^ Authentication.GetHashCode();
        }

        #endregion
    }
}
