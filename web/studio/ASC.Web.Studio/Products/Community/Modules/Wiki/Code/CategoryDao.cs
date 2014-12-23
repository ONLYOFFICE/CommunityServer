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