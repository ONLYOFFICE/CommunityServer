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

namespace ASC.Common.Security.Authorizing
{
    public sealed class Constants
    {
        public static readonly Role Admin = new Role(new Guid("cd84e66b-b803-40fc-99f9-b2969a54a1de"), "Admin");

        public static readonly Role Everyone = new Role(new Guid("c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e"), "Everyone");


        public static readonly Role User = new Role(new Guid("abef62db-11a8-4673-9d32-ef1d8af19dc0"), "User");

        public static readonly Role Visitor = new Role(new Guid("aced04fa-dd96-4b35-af3e-346bf1eb972d"), "Visitor");


        public static readonly Role Member = new Role(new Guid("ba74ca02-873f-43dc-8470-8620c156bc67"), "Member");

        public static readonly Role Owner = new Role(new Guid("bba32183-a14d-48ed-9d39-c6b4d8925fbf"), "Owner");

        public static readonly Role Self = new Role(new Guid("5d5b7260-f7f7-49f1-a1c9-95fbb6a12604"), "Self");
    }
}