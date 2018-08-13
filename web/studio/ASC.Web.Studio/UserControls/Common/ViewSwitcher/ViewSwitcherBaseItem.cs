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