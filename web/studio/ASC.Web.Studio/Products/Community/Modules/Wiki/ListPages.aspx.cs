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
using Newtonsoft.Json;

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
                InitPageData();
            }
        }

        private void InitPageData()
        {
            if (isByUser || isShowNew || isShowFresh)
            {
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


                Page.RegisterInlineScript(String.Format(" wikiPages = {0}; ASC.Community.Wiki.InitListPages();",
                                               JsonConvert.SerializeObject(dataSource.ConvertAll(p =>
                                                   new {
                                                       PageName = GetPageName(p),
                                                       ID = p.ID,
                                                       PageLink = GetPageViewLink(p),
                                                       Author = CoreContext.UserManager.GetUsers(p.UserID).RenderCustomProfileLink("", "linkMedium"),
                                                       PageDate = GetDate(p)
                                                   }))
                                               ), onReady: true);


                var emptyScreenControl = new EmptyScreenControl
                    {
                        ID = "wikiListPagesEmpty",
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("wikilogo150.png", WikiManager.ModuleId),
                        Header = emptyScreenCaption,
                        Describe = emptyScreenText,
                        CssClass = "display-none"
                    };

                if (CommunitySecurity.CheckPermissions(Community.Wiki.Common.Constants.Action_AddPage))
                {
                    emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='Default.aspx?action=New'>{0}</a>", WikiResource.menu_AddNewPage);
                }

                phListEmptyScreen.Controls.Add(emptyScreenControl);

            }
            else
            {
                List<Page> result;
                result = isShowCat ? Wiki.GetPages(categoryName) : Wiki.GetPages();

                result.RemoveAll(pemp => string.IsNullOrEmpty(pemp.PageName));


                Page.RegisterInlineScript(String.Format(" wikiCategoryAlfaList = '{0}'; wikiPages = {1}; ASC.Community.Wiki.InitListPagesByLetter();",
                                                WikiResource.wikiCategoryAlfaList,
                                                JsonConvert.SerializeObject(result.ConvertAll(p => new { PageName = GetPageName(p), ID = p.ID, PageLink = GetPageViewLink(p) }))
                                                ), onReady: true);


                var emptyScreenControl = new EmptyScreenControl
                    {
                        ID = "wikiListPagesByLetterEmpty",
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("wikilogo150.png", WikiManager.ModuleId),
                        Header = WikiResource.EmptyScreenWikiIndexCaption,
                        Describe = WikiResource.EmptyScreenWikiIndexText,
                        CssClass = "display-none"
                    };

                if (CommunitySecurity.CheckPermissions(Community.Wiki.Common.Constants.Action_AddPage))
                {
                    emptyScreenControl.ButtonHTML = String.Format("<a class='link underline blue plus' href='Default.aspx?action=New'>{0}</a>", WikiResource.menu_AddNewPage);
                }

                phListEmptyScreen.Controls.Add(emptyScreenControl);
            }
        }


        protected string GetDate(Page page)
        {
            return string.Format("{0} {1}", page.Date.ToString("t"), page.Date.ToString("d"));
        }
    }
}