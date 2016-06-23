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