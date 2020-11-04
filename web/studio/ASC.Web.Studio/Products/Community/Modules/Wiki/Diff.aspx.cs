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