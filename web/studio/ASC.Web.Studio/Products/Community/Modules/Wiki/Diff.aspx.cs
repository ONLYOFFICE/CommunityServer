/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Web.Community.Wiki.Common;
using ASC.Web.UserControls.Wiki;
using System.Linq;
using System.Text;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.Community.Wiki
{
    public partial class Diff : WikiBasePage
    {
        protected int OldVer
        {
            get
            {
                int result;
                if (Request["ov"] == null || !int.TryParse(Request["ov"], out result))
                    return 0;

                return result;
            }
        }

        protected int NewVer
        {
            get
            {
                int result;
                if (Request["nv"] == null || !int.TryParse(Request["nv"], out result))
                    return 0;

                return result;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            UpdateBreadCrumb();

            if (!IsPostBack)
            {
                FindDiff();
            }

            cmdCancel.Text = WikiResource.cmdCancel;
        }

        private void FindDiff()
        {
            var pageName = PageNameUtil.Decode(WikiPage);

            //Page oldPage = PagesProvider.PagesHistGetByNameVersion(pageName, OldVer, TenantId);
            //Page newPage = PagesProvider.PagesHistGetByNameVersion(pageName, NewVer, TenantId);
            var oldPage = Wiki.GetPage(pageName, OldVer);
            var newPage = Wiki.GetPage(pageName, NewVer);

            var oldVersion = oldPage == null ? string.Empty : oldPage.Body;

            var newVersion = newPage == null ? string.Empty : newPage.Body;


            var f = DiffHelper.DiffText(oldVersion, newVersion, true, true, false);
            var aLines = oldVersion.Split('\n');
            var bLines = newVersion.Split('\n');

            var n = 0;
            var sb = new StringBuilder();
            foreach (var aItem in f)
            {
                // write unchanged lines
                while ((n < aItem.StartB) && (n < bLines.Length))
                {
                    WriteLine(n, null, bLines[n], sb);
                    n++;
                } // while

                // write deleted lines
                for (var m = 0; m < aItem.deletedA; m++)
                {
                    WriteLine(-1, "d", aLines[aItem.StartA + m], sb);
                } // for

                // write inserted lines
                while (n < aItem.StartB + aItem.insertedB)
                {
                    WriteLine(n, "i", bLines[n], sb);
                    n++;
                } // while
            } // while

            if (f.Length > 0 || (from bline in bLines where !bline.Trim().Equals(string.Empty) select bline).Count() > 0)
            {
                // write rest of unchanged lines
                while (n < bLines.Length)
                {
                    WriteLine(n, null, bLines[n], sb);
                    n++;
                } // while
            }
            litDiff.Text = sb.ToString();
        }

        private void UpdateBreadCrumb()
        {
            if (OldVer == 0 || NewVer == 0 || OldVer == NewVer)
            {
                Response.RedirectLC(ActionHelper.GetViewPagePath(this.ResolveUrlLC("PageHistoryList.aspx"), PageNameUtil.Decode(WikiPage)), this);
            }

            WikiMaster.CurrentPageCaption = string.Format(WikiResource.wikiDiffDescriptionFormat, OldVer, NewVer);
        }

        private void WriteLine(int nr, string typ, string aText, StringBuilder sb)
        {
            sb.Append(nr >= 0 ? "<li>" : "<br/>");

            sb.Append("<span style='width:100%'");
            if (typ != null)
            {
                sb.Append(" class=\"" + typ + "\"");
            }
            sb.AppendFormat(@">{0}</span>", Server.HtmlEncode(aText).Replace("\r", "").Replace(" ", "&nbsp;"));
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.RedirectLC(ActionHelper.GetViewPagePath(this.ResolveUrlLC("PageHistoryList.aspx"), PageNameUtil.Decode(WikiPage)), this);
        }
    }
}