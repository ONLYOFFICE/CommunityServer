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

#region Usings

using System;
using System.Data;
using ASC.Common.Data;
using ASC.Blogs.Core.Domain;

#endregion

namespace ASC.Blogs.Core.Data
{
    static class RowMappers
    {
        public static Blog ToBlog(IDataRecord row)
        {
            return new Blog
                       {
                           BlogID = row.Get<int>("id"),
                           Name = row.Get<string>("name"),
                           UserID = row.Get<Guid>("user_id"),
                           GroupID = row.Get<Guid>("group_id"),
                       };
        }

        public static string ToString(IDataRecord row)
        {
            return row.Get<string>("name");
        }

        public static Tag ToTag(IDataRecord row)
        {
            return new Tag
                       {
                           Content = row.Get<string>("name"),
                           PostId = row.Get<Guid>("post_id"),
                       };
        }

        public static Comment ToComment(IDataRecord row)
        {
            return new Comment
                       {
                           ID = row.Get<Guid>("id"),
                           PostId = row.Get<Guid>("post_id"),
                           Content = row.Get<string>("content"),
                           UserID = row.Get<Guid>("created_by"),
                           Datetime = ASC.Core.Tenants.TenantUtil.DateTimeFromUtc(row.Get<DateTime>("created_when")),
                           ParentId = row.Get<Guid>("parent_id"),
                           Inactive = row.Get<int>("inactive") > 0
                       };
        }

        public static Post ToPost(IDataRecord row, bool withContent)
        {
            return new Post
                       {
                           ID = row.Get<Guid>("id"),
                           Title = row.Get<string>("title"),
                           Content = withContent ? row.Get<string>("content") : null,
                           UserID = row.Get<Guid>("created_by"),
                           Datetime = ASC.Core.Tenants.TenantUtil.DateTimeFromUtc(row.Get<DateTime>("created_when")),
                           BlogId = row.Get<int>("blog_id")
                       };
        }
    }
}
