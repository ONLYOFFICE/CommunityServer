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