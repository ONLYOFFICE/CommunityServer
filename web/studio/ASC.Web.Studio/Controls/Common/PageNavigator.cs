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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Resources;

namespace ASC.Web.Studio.Controls.Common
{
    [Themeable(true)]
    [ToolboxData("<{0}:PageNavigator runat=server></{0}:PageNavigator>")]
    public class PageNavigator : WebControl, ICloneable
    {
        private string _jsObjName;

        public int CurrentPageNumber { get; set; }

        public int EntryCountOnPage { get; set; }

        public int EntryCount { get; set; }

        public bool VisibleOnePage { get; set; }

        public int VisiblePageCount { get; set; }

        public string PageUrl { get; set; }

        public string ParamName { get; set; }

        public bool AutoDetectCurrentPage { get; set; }

        public PageNavigator()
        {
            ParamName = "p";
            VisiblePageCount = 10;
            VisibleOnePage = false;
            AutoDetectCurrentPage = false;
            EntryCountOnPage = 1;
            PageUrl = "";
        }

        private int _page_amount;
        private int _start_page;
        private int _end_page;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _jsObjName = String.IsNullOrEmpty(ID) ? "pageNavigator" + UniqueID.Replace('$', '_') : ID;

            if (HttpContext.Current != null && HttpContext.Current.Request != null && AutoDetectCurrentPage)
            {
                if (!String.IsNullOrEmpty(HttpContext.Current.Request[this.ParamName]))
                {
                    try
                    {
                        CurrentPageNumber = Convert.ToInt32(HttpContext.Current.Request[this.ParamName]);
                    }
                    catch
                    {
                        CurrentPageNumber = 0;
                    }
                }
            }

            if (CurrentPageNumber <= 0)
                CurrentPageNumber = 1;

            var scriptNav = String.Format(@"window.{0} = new ASC.Controls.PageNavigator.init('{0}', 'body', '{1}', {2}, {3}, '{4}', '{5}');",
                                          _jsObjName,
                                          EntryCountOnPage,
                                          VisiblePageCount,
                                          CurrentPageNumber,
                                          UserControlsCommonResource.PreviousPage,
                                          UserControlsCommonResource.NextPage);

            Page.RegisterInlineScript(scriptNav, onReady: false);


            _page_amount = Convert.ToInt32(Math.Ceiling(EntryCount/(EntryCountOnPage*1.0)));
            _start_page = CurrentPageNumber - 1 - VisiblePageCount/2;

            if (_start_page + VisiblePageCount > _page_amount)
                _start_page = _page_amount - VisiblePageCount;

            if (_start_page < 0)
                _start_page = 0;

            _end_page = _start_page + VisiblePageCount;

            if (_end_page > _page_amount)
                _end_page = _page_amount;

            if ((_page_amount == 1 && VisibleOnePage) || _start_page >= _end_page || _end_page - _start_page <= 1)
                return;

            var spliter = "&";
            if (PageUrl.IndexOf("?") == -1)
            {
                spliter = "&";
            }

            var isFirst = (CurrentPageNumber == 1);
            var isLast = (CurrentPageNumber == _page_amount);

            var prevURL = PageUrl + spliter + ParamName + "=" + (CurrentPageNumber - 1).ToString();
            var nextURL = PageUrl + spliter + ParamName + "=" + (CurrentPageNumber + 1).ToString();

            var script = @"document.onkeydown = function(e)
                            {
                                var code;
                                if (!e) var e = window.event;
                                if (e.keyCode) code = e.keyCode;
                                else if (e.which) code = e.which;" +

                         ((!isFirst) ?
                              @"if ((code == 37) && (e.ctrlKey == true))
                                {
                                    window.open('" + prevURL + @"','_self');
                                }" : "") +

                         ((!isLast) ?
                              @"if ((code == 39) && (e.ctrlKey == true))
                                {
                                    window.open('" + nextURL + @"','_self');
                                }" : "") +
                         @"}; ";

            Page.RegisterInlineScript(script, onReady: false);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            var sb = new StringBuilder();

            if (_page_amount == 1 && VisibleOnePage)
            {
                sb.Append("<div class='pagerNavigationLinkBox'>");
                sb.Append("<span class='pagerCurrentPosition'>");
                sb.Append(1);
                sb.Append("</span>");

                sb.Append("</div>");

                writer.Write(sb.ToString());
                return;
            }

            if (_start_page >= _end_page || _end_page - _start_page <= 1)
                return;

            sb.Append("<div class='pagerNavigationLinkBox'>");

            var spliter = "&";
            if (PageUrl.IndexOf("?") == -1)
            {
                spliter = "&";
            }
            string url = PageUrl;

            if (CurrentPageNumber != 1)
            {
                url = PageUrl + spliter + ParamName + "=" + Convert.ToString(CurrentPageNumber - 1);
                sb.Append("<a class='pagerPrevButtonCSSClass' href=\"" + url + "\">" + UserControlsCommonResource.PreviousPage + "</a>");
            }

            for (var i = _start_page; i < _end_page && _end_page - _start_page > 1; i++)
            {
                if (i == _start_page && i != 0)
                {
                    url = PageUrl + spliter + ParamName + "=" + Convert.ToString(1);
                    sb.Append("<a class='pagerNavigationLinkCSSClass' href=\"" + url + "\">1</a>");
                    if (i != 1)
                        sb.Append("<span class='splitter'>...</span>");
                }
                if ((CurrentPageNumber - 1) == i)
                {
                    sb.Append("<span class='pagerCurrentPosition'>");
                    sb.Append(CurrentPageNumber);
                    sb.Append("</span>");
                }
                else
                {
                    url = PageUrl + spliter + ParamName + "=" + Convert.ToString((i + 1));
                    sb.Append("<a class='pagerNavigationLinkCSSClass' href=\"" + url + "\">" + (i + 1) + "</a>");
                }
                if (i == _end_page - 1 && i != _page_amount - 1)
                {
                    url = PageUrl + spliter + ParamName + "=" + Convert.ToString((_page_amount));
                    if (i != _page_amount - 2)
                        sb.Append("<span class='splitter'>...</span>");
                    sb.Append("<a class='pagerNavigationLinkCSSClass' href=\"" + url + "\">" + _page_amount + "</a>");
                }
            }

            if (CurrentPageNumber != _page_amount && _page_amount != 1)
            {
                url = PageUrl + spliter + ParamName + "=" + Convert.ToString(CurrentPageNumber + 1);
                sb.Append("<a class='pagerNextButtonCSSClass' href=\"" + url + "\">" + UserControlsCommonResource.NextPage + "</a>");
            }

            sb.Append("</div>");
            writer.Write(sb.ToString());
        }

        #region ICloneable Members

        public object Clone()
        {
            return new PageNavigator
                {
                    CurrentPageNumber = this.CurrentPageNumber,
                    EntryCountOnPage = this.EntryCountOnPage,
                    EntryCount = this.EntryCount,
                    VisibleOnePage = this.VisibleOnePage,
                    VisiblePageCount = this.VisiblePageCount,
                    PageUrl = this.PageUrl,
                    ParamName = this.ParamName,
                };
        }

        #endregion
    }
}