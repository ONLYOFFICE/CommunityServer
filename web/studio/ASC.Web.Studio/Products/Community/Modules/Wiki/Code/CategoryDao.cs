/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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