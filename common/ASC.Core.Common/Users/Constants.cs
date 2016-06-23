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
using System.Configuration;
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

        public static readonly UserInfo OutsideUser = new UserInfo
        {
            ID = new Guid("{E78F4C20-2F3B-4A9D-AD13-5F298BD5A3BA}"),
            FirstName = "Outside",
            LastName = "Outside",
            ActivationStatus = EmployeeActivationStatus.Activated
        };

        public static readonly UserInfo NamingPoster = new UserInfo
        {
            ID = new Guid("{17097D73-2D1E-4B36-AA07-AEB34AF993CD}"),
            FirstName = ConfigurationManager.AppSettings["core.system.poster.name"] ?? "ONLYOFFICE Poster",
            LastName = String.Empty,
            ActivationStatus = EmployeeActivationStatus.Activated
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