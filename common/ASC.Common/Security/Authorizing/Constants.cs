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