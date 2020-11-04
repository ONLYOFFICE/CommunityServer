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


using System.Text;

namespace ASC.Web.Studio.UserControls.Common.ViewSwitcher
{
    public class ViewSwitcherLinkItem : ViewSwitcherBaseItem
    {
        private string _linkCssClass;

        public string LinkCssClass
        {
            get
            {
                if (string.IsNullOrEmpty(this._linkCssClass))
                    return "linkAction";
                return _linkCssClass;
            }
            set { _linkCssClass = value; }
        }

        public bool ActiveItemIsLink { get; set; }

        public override string GetLink()
        {
            var sb = new StringBuilder();
            if (!ActiveItemIsLink)
            {
                if (!IsSelected)
                {
                    sb.AppendFormat("<a href=\"{0}\" class='{1}'>{2}</a>", SortUrl, LinkCssClass, SortLabel);
                }
                else
                {
                    sb.Append(SortLabel);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_linkCssClass))
                    sb.AppendFormat("<a href=\"{0}\" class='{1}'>{2}</a>", SortUrl, LinkCssClass, SortLabel);
                else
                {
                    sb.AppendFormat(IsSelected
                                        ? "<a href=\"{0}\" class='{1}' style='font-weight:bold;'>{2}</a>"
                                        : "<a href=\"{0}\" class='{1}'>{2}</a>",
                                    SortUrl, LinkCssClass, SortLabel);
                }

            }
            return sb.ToString();
        }

        public ViewSwitcherLinkItem()
        {
            ActiveItemIsLink = false;
        }
    }
}