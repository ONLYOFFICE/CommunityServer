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
                           BlogId = row.Get<int>("blog_id"),
                           AutoIncrementID = row.Get<int>("post_id")
                       };
        }
    }
}
