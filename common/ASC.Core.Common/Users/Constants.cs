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
using ASC.Common.Security.Authorizing;
using Action = ASC.Common.Security.Authorizing.Action;
using AuthConst = ASC.Common.Security.Authorizing.Constants;

namespace ASC.Core.Users
{
    public sealed class Constants
    {
        #region system group and category groups

        public static readonly Guid SysGroupCategoryId = new Guid("{7717039D-FBE9-45ad-81C1-68A1AA10CE1F}");

        public static readonly GroupInfo GroupEveryone = new GroupInfo(SysGroupCategoryId)
        {
            ID = AuthConst.Everyone.ID,
            Name = AuthConst.Everyone.Name,
        };

        public static readonly GroupInfo GroupVisitor = new GroupInfo(SysGroupCategoryId)
        {
            ID = AuthConst.Visitor.ID,
            Name = AuthConst.Visitor.Name,
        };

        public static readonly GroupInfo GroupUser = new GroupInfo(SysGroupCategoryId)
        {
            ID = AuthConst.User.ID,
            Name = AuthConst.User.Name,
        };

        public static readonly GroupInfo GroupAdmin = new GroupInfo(SysGroupCategoryId)
        {
            ID = AuthConst.Admin.ID,
            Name = AuthConst.Admin.Name,
        };

        public static readonly GroupInfo[] BuildinGroups = new[]
        {
            GroupEveryone,
            GroupVisitor,
            GroupUser,
            GroupAdmin,
        };


        public static readonly UserInfo LostUser = new UserInfo
        {
            ID = new Guid("{4A515A15-D4D6-4b8e-828E-E0586F18F3A3}"),
            FirstName = "Unknown",
            LastName = "Unknown",
            ActivationStatus = EmployeeActivationStatus.NotActivated
        };

        public static readonly GroupInfo LostGroupInfo = new GroupInfo
        {
            ID = new Guid("{74B9CBD1-2412-4e79-9F36-7163583E9D3A}"),
            Name = "Unknown"
        };
                
        #endregion


        #region authorization rules module to work with users

        public static readonly Action Action_EditUser = new Action(
            new Guid("{EF5E6790-F346-4b6e-B662-722BC28CB0DB}"),
            "Edit user information");

        public static readonly Action Action_AddRemoveUser = new Action(
            new Guid("{D5729C6F-726F-457e-995F-DB0AF58EEE69}"),
            "Add/Remove user");

        public static readonly Action Action_EditGroups = new Action(
            new Guid("{1D4FEEAC-0BF3-4aa9-B096-6D6B104B79B5}"),
            "Edit categories and groups");

        #endregion
    }
}