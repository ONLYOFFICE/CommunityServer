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
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.UserControls.Wiki;
using ASC.Web.Community.Product;

namespace ASC.Web.Community.Wiki
{
    public partial class ListPages : WikiBasePage
    {
        private const int MaxNewResults = 25;

        public string categoryName
        {
            get { return Request["cat"]; }
        }

        protected bool isShowCat
        {
            get { return !isByUser && !string.IsNullOrEmpty(categoryName); }
        }

        protected bool isShowNew
        {
            get { return !isShowCat && Request["n"] != null; }
        }

        protected bool isShowFresh
        {
            get { return !isShowNew && Request["f"] != null; }
        }

        protected bool isByUser
        {
            get { return !byUserID.Equals(Guid.Empty); }
        }

        private Guid? m_byUserID;

        protected Guid byUserID
        {
            get
            {
                if (m_byUserID == null)
                {
                    try
                    {
                        m_byUserID = new Guid(Request["uid"]);
                    }
                    catch
                    {
                        m_byUserID = Guid.Empty;
                    }
                }

                return m_byUserID.Value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (isShowCat)
            {
                WikiMaster.CurrentPageCaption = string.Format(WikiResource.menu_ListPagesCatFormat, categoryName);
            }

            if (!IsPostBack)
            {
                BindRepeater();
            }
        }

        private void BindRepeater()
        {
            phListResult.Visible = phTableResult.Visible = false;
            if (isByUser || isShowNew || isShowFresh)
            {
                phListResult.Visible = true;
                List<Page> dataSource;
                var emptyScreenCaption = string.Empty;
                var emptyScreenText = string.Empty;

                if (isByUser)
                {
                    dataSource = Wiki.GetPages(byUserID);
                }
                else if (isShowNew)
                {
                    dataSource = Wiki.GetNewPages(MaxNewResults);
                    emptyScreenCaption = WikiResource.EmptyScreenWikiNewPagesCaption;
                    emptyScreenText = WikiResource.EmptyScreenWikiNewPagesText;
                }
                else
                {
                    dataSource = Wiki.GetRecentEditedPages(MaxNewResults);
                    emptyScreenCaption = WikiResource.EmptyScreenWikiRecentlyEditedCaption;
                    emptyScreenText = WikiResource.EmptyScreenWikiRecentlyEditedText;
                }

                //foreach (Pages p in dataSource)
                //{
                //    p.PageName = HttpUtility.HtmlEncode(p.PageName);
                //}

                if (dataSource.Count > 0)
                {
                    rptPageList.DataSource = dataSource;
                    rptPageList.DataBind();
                }
                else
                {
                    var emptyScreenControl = new EmptyScreenControl
                        {
                            ImgSrc = WebImageSupplier.GetAbsoluteWebPath("WikiLogo150.png", WikiManager.ModuleId),
                            Header = emptyScreenCaption,
                            Describe = emptyScreenText
                        };

                    if (CommunitySecurity.CheckPermissions(Community.Wiki.Common.Constants.Action_AddPage))
                    {
                        emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='default.aspx?action=New'>{0}</a>", WikiResource.menu_AddNewPage);
                    }

                    phListResult.Controls.Add(emptyScreenControl);
                }
            }
            else
            {
                phTableResult.Visible = true;
                List<Page> result;
                result = isShowCat ? Wiki.GetPages(categoryName) : Wiki.GetPages();

                result.RemoveAll(pemp => string.IsNullOrEmpty(pemp.PageName));

                var letters = new List<string>(WikiResource.wikiCategoryAlfaList.Split(','));

                var otherSymbol = string.Empty;
                if (letters.Count > 0)
                {
                    otherSymbol = letters[0];
                    letters.Remove(otherSymbol);
                }

                var dictList = new List<PageDictionary>();
                foreach (var page in result)
                {
                    page.PageName = HttpUtility.HtmlEncode(page.PageName);

                    var firstLetter = new string(page.PageName[0], 1);

                    if (!letters.Exists(lt => lt.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        firstLetter = otherSymbol;
                    }

                    PageDictionary pageDic;
                    if (!dictList.Exists(dl => dl.HeadName.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        pageDic = new PageDictionary { HeadName = firstLetter };
                        pageDic.Pages.Add(page);
                        dictList.Add(pageDic);
                    }
                    else
                    {
                        pageDic = dictList.Find(dl => dl.HeadName.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase));
                        pageDic.Pages.Add(page);
                    }
                }

                dictList.Sort(SortPageDict);

                var countAll = dictList.Count*3 + result.Count; //1 letter is like 2 links to category
                var perColumn = (int)(Math.Round((decimal)countAll/3));

                var mainDictList = new List<List<PageDictionary>>();

                int index = 0, lastIndex = 0, count = 0;

                for (int i = 0; i < dictList.Count; i++)
                {
                    var p = dictList[i];

                    count += 3;
                    count += p.Pages.Count;
                    index++;
                    if (count >= perColumn || i == dictList.Count - 1)
                    {
                        count = count - perColumn;
                        mainDictList.Add(dictList.GetRange(lastIndex, index - lastIndex));
                        lastIndex = index;
                    }
                }

                if (mainDictList.Count > 0)
                {
                    rptMainPageList.DataSource = mainDictList;
                    rptMainPageList.DataBind();
                }
                else
                {
                    var emptyScreenControl = new EmptyScreenControl
                        {
                            ImgSrc = WebImageSupplier.GetAbsoluteWebPath("WikiLogo150.png", WikiManager.ModuleId),
                            Header = WikiResource.EmptyScreenWikiIndexCaption,
                            Describe = WikiResource.EmptyScreenWikiIndexText
                        };

                    if (CommunitySecurity.CheckPermissions(Community.Wiki.Common.Constants.Action_AddPage))
                    {
                        emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='default.aspx?action=New'>{0}</a>", WikiResource.menu_AddNewPage);
                    }

                    phTableResult.Controls.Add(emptyScreenControl);
                }
            }
        }

        private int SortPageDict(PageDictionary cd1, PageDictionary cd2)
        {
            return cd1.HeadName.CompareTo(cd2.HeadName);
        }

        //protected void cmdDelete_Click(object sender, EventArgs e)
        //{
        //    PagesProvider.PagesDelete((sender as LinkButton).CommandName);
        //    BindRepeater();
        //}



        //protected string GetPageEditLink(Pages page)
        //{
        //    return ActionHelper.GetEditPagePath(this.ResolveUrlLC("Default.aspx"), page.PageName);
        //}

        //protected string GetPageInfo(Pages page)
        //{
        //    if (page.UserID.Equals(Guid.Empty))
        //    {
        //        return string.Empty;
        //    }


        //    return ProcessVersionInfo(page.PageName, page.UserID, page.Date, page.Version, false);
        //}

        protected string GetAuthor(Page page)
        {
            return CoreContext.UserManager.GetUsers(page.UserID).RenderCustomProfileLink(ASC.Web.Community.Product.CommunityProduct.ID, "", "linkMedium");
        }

        protected string GetDate(Page page)
        {
            return string.Format("{0} {1}", page.Date.ToString("t"), page.Date.ToString("d"));
        }
    }
}