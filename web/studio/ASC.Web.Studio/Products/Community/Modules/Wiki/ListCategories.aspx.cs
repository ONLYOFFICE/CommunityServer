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

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Community.Product;

namespace ASC.Web.Community.Wiki
{
    
    public class CategoryInfo
    {
        public string CategoryName { get; set; }
        public string CategoryUrl { get; set; }

    }

    public class CategoryDictionary
    {
        public CategoryDictionary()
        {
            Categories = new List<CategoryInfo>();
        }
        public string HeadName { get; set; }
        public List<CategoryInfo> Categories { get; set; }
    }

    public partial class ListCategories : WikiBasePage
    {

        protected bool HasCategories { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindRepeater();
            }
        }

        private void BindRepeater()
        {
            var dictList = new List<CategoryDictionary>();
            var calList = Wiki.GetCategories();

            List<string> letters = new List<string>(WikiResource.wikiCategoryAlfaList.Split(','));
            string otherSymbol = string.Empty;
            if (letters.Count > 0)
            {
                otherSymbol = letters[0];
                letters.Remove(otherSymbol);
            }


            string firstLetter;
            CategoryDictionary catDic;
            foreach (Category cat in calList)
            {
                if (string.IsNullOrEmpty(cat.CategoryName))
                    continue;

                firstLetter = new string(cat.CategoryName[0], 1);

                if (!letters.Exists(lt => lt.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    firstLetter = otherSymbol;
                }

                if (!dictList.Exists(dl => dl.HeadName.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    catDic = new CategoryDictionary();
                    catDic.HeadName = firstLetter;
                    catDic.Categories.Add(GetCategoryInfo(cat.CategoryName));
                    dictList.Add(catDic);
                }
                else
                {
                    catDic = dictList.Find(dl => dl.HeadName.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase));
                    catDic.Categories.Add(GetCategoryInfo(cat.CategoryName));
                }
            }

            dictList.Sort(SortCatDict);

            int countAll = dictList.Count * 3 + calList.Count; //1 letter is like 2 links to category
            int perColumn = (int)(Math.Round((decimal)countAll / 3));

            List<List<CategoryDictionary>> mainDictList = new List<List<CategoryDictionary>>();

            int index = 0, lastIndex = 0, count = 0;

            CategoryDictionary cd;
            for (int i = 0; i < dictList.Count; i++)
            {
                cd = dictList[i];

                count += 3;
                count += cd.Categories.Count;
                index++;
                if (count >= perColumn || i == dictList.Count - 1)
                {
                    count = count - perColumn;
                    mainDictList.Add(dictList.GetRange(lastIndex, index - lastIndex));
                    lastIndex = index;
                }

            }


            HasCategories = mainDictList.Count > 0;

            if (HasCategories)
            {
                rptCategoryList.DataSource = mainDictList;
                rptCategoryList.DataBind();
            }
            else
            {
                var emptyScreenControl = new EmptyScreenControl
                {
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("WikiLogo150.png", WikiManager.ModuleId),
                    Header = WikiResource.EmptyScreenWikiCategoriesCaption,
                    Describe = WikiResource.EmptyScreenWikiCategoriesText
                };

                if (CommunitySecurity.CheckPermissions(Community.Wiki.Common.Constants.Action_AddPage))
                {
                    emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='default.aspx?action=New'>{0}</a>", WikiResource.menu_AddNewPage);
                }

                EmptyContent.Controls.Add(emptyScreenControl);
            }

        }

        private CategoryInfo GetCategoryInfo(string catName)
        {
            var name = PageNameUtil.NormalizeNameCase(catName);
            return new CategoryInfo()
            {
                
                CategoryName = HttpUtility.HtmlDecode(name).HtmlEncode(),
                CategoryUrl = ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), catName, ASC.Web.UserControls.Wiki.Constants.WikiCategoryKeyCaption)
            };
        }

        private int SortCatDict(CategoryDictionary cd1, CategoryDictionary cd2)
        {
            return cd1.HeadName.CompareTo(cd2.HeadName);
        }
    }
}
