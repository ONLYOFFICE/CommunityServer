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
using System.Web.UI;
using ASC.Web.Studio.Controls.Common;
using System.Web;

namespace ASC.Web.Community.Bookmarking.Util
{
    public class BookmarkingNavigationItem : NavigationItem
    {
        public string BookmarkingClientID { get; set; }

        public bool DisplayOnPage { get; set; }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (String.IsNullOrEmpty(BookmarkingClientID))
            {
                base.RenderContents(writer);
                return;
            }
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(URL))
            {
                var display = DisplayOnPage ? "block" : "none";

                writer.Write(
                    String.Format("<a href=\"{0}\" title=\"{1}\" id='{2}' style='display:{3}' class='linkAction'>",
                                  ResolveUrl(URL), HttpUtility.HtmlEncode(Description), BookmarkingClientID, display
                        ));
                writer.Write(HttpUtility.HtmlEncode(Name));
                writer.Write("</a>");
            }
            else
                base.RenderContents(writer);
        }
    }
}