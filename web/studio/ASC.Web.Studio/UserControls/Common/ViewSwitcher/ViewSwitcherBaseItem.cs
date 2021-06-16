/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using System.Text;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Common.ViewSwitcher
{
    public abstract class ViewSwitcherBaseItem : Control
    {
        public string SortUrl { get; set; }

        public string SortLabel { get; set; }

        public bool IsSelected { get; set; }

        internal bool IsLast { get; set; }

        public string DivID { get; set; }

        public string AdditionalHtml { get; set; }

        public string HintText { get; set; }

        public string GetSortLink
        {
            get
            {
                var idString = string.Empty;
                if (!string.IsNullOrEmpty(DivID))
                {
                    idString = string.Format(" id='{0}' ", DivID);
                }
                var cssClass = "viewSwitcherItem";
                if (IsSelected)
                {
                    cssClass = "viewSwithcerSelectedItem";
                }
                var sb = new StringBuilder();
                sb.AppendFormat("<div {0} class='{1}'>{2}{3}</div>", idString, cssClass, GetLink(), AdditionalHtml ?? string.Empty);
                return sb.ToString();
            }
        }

        public abstract string GetLink();
    }
}