/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using ASC.Core.Users;

namespace ASC.Core
{
    public interface IUserManagerClient
    {
        #region Users

        UserInfo[] GetUsers();

        UserInfo GetUsers(Guid userID);

        IEnumerable<UserInfo> GetUsers(IEnumerable<Guid> userIds);

        UserInfo GetUsers(int tenant, string login, string password);

        DateTime GetMaxUsersLastModified();

        UserInfo SaveUserInfo(UserInfo userInfo);

        void DeleteUser(Guid userID);

        void SaveUserPhoto(Guid userID, Guid moduleID, byte[] photo);

        byte[] GetUserPhoto(Guid userID, Guid moduleID);

        bool UserExists(Guid userID);

        UserInfo[] GetUsers(EmployeeStatus status);

        UserInfo[] GetUsers(EmployeeStatus status, EmployeeType type);

        string[] GetUserNames(EmployeeStatus status);

        UserInfo GetUserByUserName(string userName);

        UserInfo GetUserBySid(string sid);

        bool IsUserNameExists(string userName);

        UserInfo GetUserByEmail(string email);

        UserInfo[] Search(string text, EmployeeStatus status);

        UserInfo[] Search(string text, EmployeeStatus status, Guid groupId);


        GroupInfo[] GetUserGroups(Guid userID);

        GroupInfo[] GetUserGroups(Guid userID, IncludeType includeType);

        GroupInfo[] GetUserGroups(Guid userID, Guid categoryID);

        UserInfo[] GetUsersByGroup(Guid groupID);

        bool IsUserInGroup(Guid userID, Guid groupID);

        void AddUserIntoGroup(Guid userID, Guid groupID);

        void RemoveUserFromGroup(Guid userID, Guid groupID);

        #endregion

        #region Company

        UserInfo GetCompanyCEO();

        void SetCompanyCEO(Guid userId);

        GroupInfo[] GetDepartments();

        Guid GetDepartmentManager(Guid deparmentID);

        void SetDepartmentManager(Guid deparmentID, Guid userID);

        #endregion
    }
}