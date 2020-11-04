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


using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Community.News.Common;
using System;


namespace ASC.Web.Community.News.Controls
{
    

    public class BreadcrumbsControl : WebControl
    {
        // Fields
        private IList<BreadcrumbPath> breadCrumbPath = new List<BreadcrumbPath>();

        // Methods
        public void AddBreadcrumb(string name, Uri url)
        {
            this.breadCrumbPath.Add(new BreadcrumbPath(name, url.ToString()));
        }

        

        protected override void RenderContents(HtmlTextWriter writer)
        {
            base.RenderContents(writer);
            writer.Write(@"<div style=""padding:0px 0px 8px 0px;"">");
            for (int i = 0; i < (this.breadCrumbPath.Count - 1); i++)
            {
                if(i > 0)
                {
                    writer.Write(@"<span class=""textBase""> > </span>");
                }
                BreadcrumbPath path = this.breadCrumbPath[i];
                writer.Write(@"<a class=""breadCrumbs"" title=""{0}"" href=""{1}"">{0}</a>",path.Name, path.Link);
            }
            writer.Write(@"</div>");
            writer.Write(@"<div>{0}</div>", this.breadCrumbPath[this.breadCrumbPath.Count - 1].Name);

        }

        
    }
}