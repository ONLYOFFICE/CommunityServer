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
using ASC.Common.Data.Sql.Expressions;

namespace ASC.Web.UserControls.Wiki.Data
{
    class CategoryDao : BaseDao
    {
        public CategoryDao(string dbid, int tenant)
            : base(dbid, tenant)
        {
        }


        public List<Category> GetCategories()
        {
            var q = Query("wiki_categories").Select("CategoryName").GroupBy(1);

            return db.ExecuteList(q)
                .ConvertAll(r => ToCategory(r));
        }

        public List<Category> GetCategories(string pagename, bool onlyWithThisPage)
        {
            var q = Query("wiki_categories c2").Select("c2.CategoryName").Where("c2.PageName", pagename);
            if (onlyWithThisPage)
            {
                q = Query("wiki_categories c1").Select("c1.CategoryName")
                    .InnerJoin(q, "t", Exp.EqColumns("c1.CategoryName", "t.CategoryName"))
                    .GroupBy(1)
                    .Having(Exp.Eq("count(*)", 1));
            }

            return db.ExecuteList(q)
                .ConvertAll(r => ToCategory(r));
        }

        public Category SaveCategory(Category category)
        {
            if (category == null) throw new ArgumentNullException("category");
            if (string.IsNullOrEmpty(category.CategoryName) || string.IsNullOrEmpty(category.PageName)) return category;

            var i = Insert("wiki_categories")
                .InColumnValue("CategoryName", category.CategoryName)
                .InColumnValue("PageName", category.PageName);
            db.ExecuteNonQuery(i);
            return category;
        }

        public void RemoveCategories(string pagename)
        {
            var d = Delete("wiki_categories").Where("PageName", pagename);
            db.ExecuteNonQuery(d);
        }


        private Category ToCategory(object[] r)
        {
            return new Category
            {
                CategoryName = (string)r[0],
                PageName = 1 < r.Length ? (string)r[1] : string.Empty,
            };
        }
    }
}