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
using System.Collections.Generic;
using System.Linq;
using ASC.Core.Tenants;

namespace ASC.Web.UserControls.Wiki.Data
{
    class CommentDao : BaseDao
    {
        public CommentDao(string dbid, int tenant)
            : base(dbid, tenant)
        {
        }


        public List<Comment> GetComments(string pageName)
        {
            var q = Query("wiki_comments")
                .Select("Id", "ParentId", "PageName", "Body", "UserId", "Date", "Inactive")
                .Where("PageName", pageName)
                .OrderBy("Date", true);

            return db.ExecuteList(q)
                .ConvertAll(r => ToComment(r));
        }

        public Comment GetComment(Guid id)
        {
            var q = Query("wiki_comments")
                .Select("Id", "ParentId", "PageName", "Body", "UserId", "Date", "Inactive")
                .Where("Id", id.ToString());

            return db.ExecuteList(q)
                .ConvertAll(r => ToComment(r))
                .SingleOrDefault();
        }

        public Comment SaveComment(Comment comment)
        {
            if (comment == null) throw new NotImplementedException("comment");

            if (comment.Id == Guid.Empty) comment.Id = Guid.NewGuid();
            var i = Insert("wiki_comments")
                .InColumnValue("id", comment.Id.ToString())
                .InColumnValue("ParentId", comment.ParentId.ToString())
                .InColumnValue("PageName", comment.PageName)
                .InColumnValue("Body", comment.Body)
                .InColumnValue("UserId", comment.UserId.ToString())
                .InColumnValue("Date", TenantUtil.DateTimeToUtc(comment.Date))
                .InColumnValue("Inactive", comment.Inactive);

            db.ExecuteNonQuery(i);

            return comment;
        }

        public void RemoveComment(Guid id)
        {
            var d = Delete("wiki_comments").Where("id", id.ToString());
            db.ExecuteNonQuery(d);
        }


        private Comment ToComment(object[] r)
        {
            return new Comment
            {
                Id = new Guid((string)r[0]),
                ParentId = new Guid((string)r[1]),
                PageName = (string)r[2],
                Body = (string)r[3],
                UserId = new Guid((string)r[4]),
                Date = TenantUtil.DateTimeFromUtc((DateTime)r[5]),
                Inactive = Convert.ToBoolean(r[6]),
            };
        }
    }
}