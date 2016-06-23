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


using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Skins;
using System.Web;
using ASC.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Controls.Users
{
    [ToolboxData("<{0}:EmployeeUserCard runat=\"server\"/>")]
    public class EmployeeUserCard : Control
    {

        private string _CssClass = string.Empty;
        public string CssClass
        {
            get
            {
                if (ViewState["CssClass"] == null || ViewState["CssClass"].ToString().Equals(string.Empty))
                {
                    return _CssClass;
                }
                return ViewState["CssClass"].ToString();
            }
            set
            {
                ViewState["CssClass"] = value;
            }
        }

        public string EmployeeUrl
        {
            get
            {
                if (ViewState["EmployeeUrl"] == null || ViewState["EmployeeUrl"].ToString().Equals(string.Empty))
                {
                    return string.Empty; ;
                }
                return ViewState["EmployeeUrl"].ToString();
            }
            set
            {
                ViewState["EmployeeUrl"] = value;
            }
        }

        public Unit Height
        {
            get
            {
                if (ViewState["Height"] == null || ViewState["Height"].ToString().Equals(string.Empty))
                {
                    return Unit.Parse("122px");
                }
                return Unit.Parse(ViewState["Height"].ToString());
            }
            set
            {
                ViewState["Height"] = value.ToString();
            }
        }


        public Unit Width
        {
            get
            {
                if (ViewState["Width"] == null || ViewState["Width"].ToString().Equals(string.Empty))
                {
                    return Unit.Parse("352px");
                }
                return Unit.Parse(ViewState["Width"].ToString());
            }
            set
            {
                ViewState["Width"] = value.ToString();
            }
        }

        public UserInfo EmployeeInfo { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if (EmployeeInfo != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<div {1} style=\"width: {0}px;overflow:hidden;\"><table cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\" >", Width.Value,
                    EmployeeInfo.ActivationStatus == EmployeeActivationStatus.Pending ? "class=\"pending\"" : "");

                sb.Append("<tr valign='top'>");
                sb.Append("<td align=\"left\" style=\"width:56px; padding-right:10px;\">");
                sb.AppendFormat("<a class=\"borderBase\" {1} href=\"{0}\">", EmployeeUrl, "style=\"position:relative;  text-decoration:none; display:block; height:48px; width:48px;\"");
                sb.Append("<img align=\"center\" alt=\"\" style='display:block;margin:0; position:relative;' border=0 src=\"" + EmployeeInfo.GetMediumPhotoURL() + "\"/>");
                if (EmployeeInfo.ActivationStatus == EmployeeActivationStatus.Pending)
                {
                    sb.Append("<div class=\"pendingInfo borderBase tintMedium\"><div>" + Resources.Resource.PendingTitle + "</div></div>");
                }
                sb.Append("</a>");
                sb.Append("</td>");

                sb.Append("<td>");
                if (!EmployeeInfo.ID.Equals(ASC.Core.Users.Constants.LostUser.ID))
                {
                    sb.Append("<div>");
                    sb.AppendFormat("<a class=\"link header-base middle bold\" data-id=\"{2}\" href=\"{0}\" title=\"{1}\">{1}</a>", EmployeeUrl, EmployeeInfo.DisplayUserName(), EmployeeInfo.ID);
                    sb.Append("</div>");

                    //department
                    sb.Append("<div style=\"padding-top: 6px;\">");
                    if (EmployeeInfo.Status != EmployeeStatus.Terminated)
                    {
                        var removecoma = false;
                        foreach (var g in CoreContext.UserManager.GetUserGroups(EmployeeInfo.ID))
                        {
                            sb.AppendFormat("<a class=\"link\" href=\"{0}\">", CommonLinkUtility.GetDepartment(g.ID));
                            sb.Append(HttpUtility.HtmlEncode(g.Name));
                            sb.Append("</a>, ");
                            removecoma = true;
                        }
                        if (removecoma)
                        {
                            sb.Remove(sb.Length - 2, 2);
                        }
                    }
                    sb.Append("&nbsp;</div>");

                    sb.Append("<div style=\"padding-top: 6px;\">");
                    sb.Append(HttpUtility.HtmlEncode(EmployeeInfo.Title) ?? "");
                    sb.Append("&nbsp;</div>");
                }

                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("</table></div>");
                writer.Write(sb.ToString());
            }

            base.Render(writer);
        }
    }
}