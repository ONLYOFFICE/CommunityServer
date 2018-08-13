/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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