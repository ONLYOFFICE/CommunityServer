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
using System.Security;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Community.Product;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Resources;
using ASC.Web.Community.Resources;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Community.Wiki
{
    public partial class PageHistoryList : WikiBasePage
    {
        protected string WikiPageTitle { get; set; }
        protected string WikiPageURL { get; set; }
        protected string WikiPageIn { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var page = Wiki.GetPage(PageNameUtil.Decode(WikiPage));

            if (Request["page"] == null || page == null)
                Response.RedirectLC("Default.aspx", this);

            var pageName = page.PageName.Equals(string.Empty) ? WikiResource.MainWikiCaption : page.PageName;

            WikiPageTitle = pageName;
            WikiPageIn = CommunityResource.InForParentPage;
            WikiPageURL = ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), page.PageName);
            //WikiPageIn = CommunityResource.InForParentPage;
            WikiMaster.CurrentPageCaption = WikiResource.wikiHistoryCaption;

            if (!IsPostBack)
            {
                cmdDiff.Text = WikiResource.cmdDiff;
                BindHistoryList();
            }
        }

        protected void cmdRevert_Click(object sender, EventArgs e)
        {
            if(CommunitySecurity.IsOutsider())
                throw new SecurityException();

            int ver;
            if (int.TryParse((sender as LinkButton).CommandName, out ver))
            {
                var page = Wiki.GetPage(PageNameUtil.Decode(WikiPage), ver);
                if (page != null)
                {
                    page.Date = TenantUtil.DateTimeNow();
                    page.UserID = SecurityContext.CurrentAccount.ID;
                    page.Version = Wiki.GetPageMaxVersion(page.PageName) + 1;

                    Wiki.SavePage(page);
                    Wiki.UpdateCategoriesByPageContent(page);

                    BindHistoryList();
                }
            }
        }

        protected new string GetPageViewLink(Page page)
        {
            return ActionHelper.GetViewPagePathWithVersion(this.ResolveUrlLC("Default.aspx"), page.PageName, page.Version);
        }

        protected new string GetPageName(Page page)
        {
            return string.IsNullOrEmpty(page.PageName) ? WikiResource.MainWikiCaption : page.PageName;
        }

        protected string GetAuthor(Page page)
        {
            return CoreContext.UserManager.GetUsers(page.UserID).RenderCustomProfileLink("", "linkMedium");
        }

        protected string GetDate(Page page)
        {
            return string.Format("{0} {1}", page.Date.ToString("t"), page.Date.ToString("d"));
        }

        protected void cmdDiff_Click(object sender, EventArgs e)
        {
            int oldVersion = 0, newVersion = 0;

            foreach (RepeaterItem item in rptPageHistory.Items)
            {
                if (oldVersion > 0 && newVersion > 0)
                    break;

                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    var rbOldDiff = (RadioButton)item.FindControl("rbOldDiff");
                    var rbNewDiff = (RadioButton)item.FindControl("rbNewDiff");
                    var litDiff = (Literal)item.FindControl("litDiff");

                    if (oldVersion == 0 && rbOldDiff.Checked)
                    {
                        oldVersion = Convert.ToInt32(litDiff.Text);
                    }

                    if (newVersion == 0 && rbNewDiff.Checked)
                    {
                        newVersion = Convert.ToInt32(litDiff.Text);
                    }
                }
            }

            Response.RedirectLC(string.Format(@"Diff.aspx?page={0}&ov={1}&nv={2}", WikiPage, oldVersion, newVersion), this);
        }

        private void BindHistoryList()
        {
            rptPageHistory.DataSource = Wiki.GetPageHistory(PageNameUtil.Decode(WikiPage));
            rptPageHistory.DataBind();
            cmdDiff.Visible = (rptPageHistory.DataSource as List<Page>).Count > 1;
        }
    }
}