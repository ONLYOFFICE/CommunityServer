/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.People.Core;

namespace ASC.Web.People.Classes
{
    public sealed class ProfileHelper
    {
        private Guid _userID;
        private UserInfo _ownInfo;
        private UserInfo _userInfo;

        public MyUserProfile userProfile { get; set; }
        public UserInfo userInfo
        {
            get
            {
                return _userInfo;
            }
        }

        public bool canEdit
        {
            get
            {
                return _ownInfo != null && (_ownInfo.ID == _userInfo.ID || _ownInfo.IsAdmin() || _ownInfo.IsOwner());
            }
        }

        public bool isMe
        {
            get
            {
                return _ownInfo != null && (_ownInfo.ID == _userInfo.ID);
            }
        }

        public ProfileHelper() : this(SecurityContext.CurrentAccount.ID.ToString()) { }

        public ProfileHelper(string id)
        {
            if (SecurityContext.IsAuthenticated)
            {
                id = String.IsNullOrEmpty(id) ? SecurityContext.CurrentAccount.ID.ToString() : id;
                _ownInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            }

            if (!String.IsNullOrEmpty(id))
            {
                _userInfo = CoreContext.UserManager.GetUserByUserName(id);
            }

            if (_userInfo == null || _userInfo == Constants.LostUser)
            {
                if (!String.IsNullOrEmpty(id))
                {
                    try
                    {
                        _userID = new Guid(id);
                    }
                    catch
                    {
                        _userID = SecurityContext.CurrentAccount.ID;
                    }
                }

                if (!CoreContext.UserManager.UserExists(_userID))
                {
                    _userID = SecurityContext.CurrentAccount.ID;
                }
                _userInfo = CoreContext.UserManager.GetUsers(_userID);
            }
            else
            {
                _userID = _userInfo.ID;
            }

            userProfile = new MyUserProfile(_userID);
        }

        public string GetProfileLink()
        {
            return VirtualPathUtility.ToAbsolute(PeopleProduct.ProductPath + "profile.aspx") +"?user=" + userProfile.UserName;
        }

        public string GetActivityLink()
        {
            return VirtualPathUtility.ToAbsolute(PeopleProduct.ProductPath + "activity.aspx") + "?user=" + userProfile.UserName;
        }

        public string GetSubscriptionsLink()
        {
            return VirtualPathUtility.ToAbsolute(PeopleProduct.ProductPath + "subscriptions.aspx") + "?user=" + userProfile.UserName;
        }

        public string GetCustomizationLink()
        {
            return VirtualPathUtility.ToAbsolute(PeopleProduct.ProductPath + "customization.aspx") + "?user=" + userProfile.UserName;
        }
    }
}
