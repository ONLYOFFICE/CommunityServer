/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
