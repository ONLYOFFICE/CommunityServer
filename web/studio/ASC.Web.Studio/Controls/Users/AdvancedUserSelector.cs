/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility.Skins;
using Newtonsoft.Json;
using Resources;

namespace ASC.Web.Studio.Controls.Users
{
    [ToolboxData("<{0}:AdvancedUserSelector runat=server></{0}:AdvancedUserSelector>")]
    public class AdvancedUserSelector : Control
    {
        #region Fields

        private readonly string _selectorID = Guid.NewGuid().ToString().Replace('-', '_');
        private string _jsObjName;
        private string _linkText = UserControlsCommonResource.LinkText;

        public static string WebApiBaseUrl
        {
            get { return VirtualPathUtility.ToAbsolute(ConfigurationManager.AppSettings["api.url"] ?? "/api/2.0"); }
        }

        #endregion

        #region Properties

        public int InputWidth { get; set; }
        public Guid SelectedUserId { get; set; }
        public bool IsLinkView { get; set; }
        public EmployeeType EmployeeType { get; set; }
        public bool IncludeLockedUsersIntoObject { get; set; }
        public String ParentContainerHtmlSelector { get; set; }

        public string LinkText
        {
            get { return _linkText; }
            set { _linkText = value; }
        }

        public string DefaultGroupText { get; set; }
        public string AdditionalFunction { get; set; }
        public List<UserInfo> UserList { get; set; }
        public List<Guid> DisabledUsers { get; set; }

        #endregion

        public AdvancedUserSelector()
        {
            InputWidth = 230;
        }

        #region Events

        public static void RegisterStartupScripts(Page page, AdvancedUserSelector advUsrSlr)
        {
            var script = new StringBuilder();
            var jsObjName = advUsrSlr._jsObjName;
            if (String.IsNullOrEmpty(jsObjName))
            {
                jsObjName = String.IsNullOrEmpty(advUsrSlr.ID)
                                ? "advancedUserSelector" + advUsrSlr.UniqueID.Replace('$', '_')
                                : advUsrSlr.ID;
            }

            if (!String.IsNullOrEmpty(advUsrSlr.ParentContainerHtmlSelector))
            {
                var selectedUserName =
                    advUsrSlr.SelectedUserId != Guid.Empty
                        ? CoreContext.UserManager.GetUsers(advUsrSlr.SelectedUserId).DisplayUserName().ReplaceSingleQuote()
                        : "";
                var selectedUserJson = JsonConvert.SerializeObject(new
                    {
                        id = advUsrSlr.SelectedUserId,
                        displayName = selectedUserName
                    });

                script.AppendFormat(@"jq(function(){{
                                        jq.tmpl('advUserSelectorTemplate', {{
                                                jsObjName: '{0}',
                                                InputWidth: {1},
                                                SelectedUser: {2},
                                                IsLinkView: {3},
                                                LinkText: '{4}'
                                                }}).appendTo('{5}');
                                    }});",
                                    jsObjName,
                                    advUsrSlr.InputWidth,
                                    selectedUserJson,
                                    advUsrSlr.IsLinkView.ToString().ToLower(),
                                    advUsrSlr.LinkText.HtmlEncode(),
                                    advUsrSlr.ParentContainerHtmlSelector);
            }

            script.AppendFormat(@" window.{0} = new ASC.Controls.AdvancedUserSelector.UserSelectorPrototype('{1}', '{0}', '&lt;{2}&gt;', '{3}', {4}, '{5}','{6}', {7});",
                                jsObjName,
                                advUsrSlr._selectorID,
                                UserControlsCommonResource.EmptyList,
                                UserControlsCommonResource.ClearFilter,
                                advUsrSlr.IsLinkView.ToString().ToLower(),
                                MobileDetector.IsMobile ? advUsrSlr._linkText.HtmlEncode().ReplaceSingleQuote() : "",
                                String.IsNullOrEmpty(advUsrSlr.DefaultGroupText) ? UserControlsCommonResource.AllDepartments : advUsrSlr.DefaultGroupText,
                                (int)advUsrSlr.EmployeeType);

            if (advUsrSlr.UserList != null && advUsrSlr.UserList.Count > 0)
            {
                if (advUsrSlr.DisabledUsers != null && advUsrSlr.DisabledUsers.Count > 0)
                {
                    advUsrSlr.UserList.RemoveAll(ui => advUsrSlr.DisabledUsers.Contains(ui.ID));
                }

                script.AppendFormat(" {0}.UserIDs = [", jsObjName);
                foreach (var u in advUsrSlr.UserList.SortByUserName())
                {
                    script.AppendFormat("'{0}',", u.ID);
                }
                if (advUsrSlr.UserList.Count > 0)
                {
                    script.Remove(script.Length - 1, 1);
                }

                script.Append("];");
            }

            if (advUsrSlr.DisabledUsers != null && advUsrSlr.DisabledUsers.Count > 0)
            {
                script.AppendFormat(" {0}.DisabledUserIDs = [", jsObjName);
                foreach (var u in advUsrSlr.DisabledUsers)
                {
                    script.AppendFormat("'{0}',", u);

                }
                script.Remove(script.Length - 1, 1);

                script.Append("];");
            }

            if (advUsrSlr.IncludeLockedUsersIntoObject)
            {
                var lockedUsers = CoreContext.UserManager.GetUsers(EmployeeStatus.Terminated);
                script.AppendFormat(" {0}.LockedUsers = [", jsObjName);
                foreach (var u in lockedUsers)
                {
                    script.AppendFormat("{{ID:\"{0}\",Name:\"{1}\",PhotoUrl:\"{2}\"}},",
                                        u.ID, u.DisplayUserName(), u.GetSmallPhotoURL());
                }
                if (lockedUsers.Length > 0)
                    script.Remove(script.Length - 1, 1);

                script.Append("];");
            }


            script.AppendFormat(" {0}.AllDepartmentsGroupName = '{1}';", jsObjName, UserControlsCommonResource.AllDepartments.HtmlEncode().ReplaceSingleQuote());

            if (!String.IsNullOrEmpty(advUsrSlr.AdditionalFunction))
            {
                script.AppendFormat(" {0}.AdditionalFunction = {1};", jsObjName, advUsrSlr.AdditionalFunction);
            }

            if (!Guid.Empty.Equals(advUsrSlr.SelectedUserId))
            {
                script.AppendFormat(" {0}.SelectedUserId = '{1}';", jsObjName, advUsrSlr.SelectedUserId);
            }
            else if (MobileDetector.IsMobile)
            {
                script.AppendFormat(" {0}.SelectedUserId = {0}.Me().find('option:first').attr('selected', 'selected').val();", jsObjName);
            }

            script.AppendFormat("window.{0}.Init();", jsObjName);
            page.RegisterInlineScript(script.ToString(), true);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnInit(e);
            _jsObjName = String.IsNullOrEmpty(ID) ? "advancedUserSelector" + UniqueID.Replace('$', '_') : ID;
            RegisterStartupScripts(Page, this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            if (String.IsNullOrEmpty(ParentContainerHtmlSelector))
            {
                var sb = new StringBuilder();
                sb.AppendFormat("<div id='{0}'>", _jsObjName);

                if (MobileDetector.IsMobile)
                {
                    sb.AppendFormat("<select class='comboBox' style='width:{0}px;' onchange='javascript:{1}.SelectUser(this);' >", InputWidth, _jsObjName);
                    sb.AppendFormat("<option style='max-width:300px;' value='{0}' {2}>{1}</option>",
                                    -1,
                                    _linkText.HtmlEncode(),
                                    Guid.Empty.Equals(SelectedUserId) ? "selected = 'selected'" : string.Empty);

                    var accounts = CoreContext.Authentication.GetUserAccounts().OrderBy(a => a.Name);

                    foreach (var account in accounts)
                    {
                        sb.AppendFormat("<option style='max-width:300px;' value='{0}' {2}>{1}</option>",
                                        account.ID,
                                        account.Name.HtmlEncode(),
                                        Equals(account.ID, SelectedUserId) ? "selected = 'selected'" : string.Empty);
                    }

                    sb.AppendFormat("</select>");
                }
                else
                {
                    var valueForInput = Guid.Empty.Equals(SelectedUserId) ? string.Empty : CoreContext.UserManager.GetUsers(SelectedUserId).DisplayUserName().ReplaceSingleQuote();

                    if (IsLinkView)
                    {
                        sb.AppendFormat("<span class='addUserLink' onclick='{0}.OnInputClick({0}, event);'><a class='link dotline'>{1}</a><span class='sort-down-black'></span></span>",
                                        _jsObjName,
                                        LinkText.HtmlEncode());
                    }
                    else
                    {
                        var peopleImgStyle = valueForInput.Trim() == string.Empty ? "display:none;" : "display:block;";
                        var searchImgStyle = valueForInput.Trim() == string.Empty ? "display:block;" : "display:none;";

                        sb.AppendFormat(@"
                            <table cellspacing='0' cellpadding='1' class='borderBase adv-userselector-inputContainer' width='{0}px'>
                                <tbody>
                                    <tr>
                                        <td width='16px'>
                                            <img align='absmiddle' src='{1}' id='peopleImg' style='margin:2px;{7}'/>
                                            <img align='absmiddle' src='{2}' id='searchImg' style='{8}'/>
                                        </td>
                                        <td>
                                            <input type='text' autocomplete='off'
                                                oninput='{3}.SuggestUser(event);' onpaste='{3}.SuggestUser(event);' onkeyup='{3}.SuggestUser(event);'
                                                onclick='{3}.OnInputClick({3}, event);' onkeydown='{3}.ChangeSelection(event);'
                                                class='textEdit inputUserName' style='width:100%;' value='{5}'/>
                                            <input class='loginhidden' name='login' value='{6}' type='hidden'/>
                                        </td>
                                        <td width='20px' onclick='{3}.OnInputClick({3}, event);'>
                                            <img align='absmiddle' src='{4}' style='cursor:pointer;'/>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>",
                                        InputWidth + 8,
                                        WebImageSupplier.GetAbsoluteWebPath("people_icon.png"),
                                        WebImageSupplier.GetAbsoluteWebPath("search.png"),
                                        _jsObjName,
                                        WebImageSupplier.GetAbsoluteWebPath("collapse_down_dark.png"),
                                        valueForInput,
                                        SelectedUserId,
                                        peopleImgStyle,
                                        searchImgStyle);
                    }

                    sb.AppendFormat("<div class='adv-userselector-DepsAndUsersContainer' {0}>", IsLinkView ? "style='height:230px'" : string.Empty);

                    if (IsLinkView)
                    {
                        sb.Append("<div style='margin-bottom: 10px;'>");
                        sb.Append("<div style='width:50%;'>");
                        sb.AppendFormat(@"
                            <table cellspacing='0' cellpadding='1' class='borderBase adv-userselector-inputContainer' width='100%' style='height: 18px;'>
                                <tbody>
                                    <tr>
                                        <td>
                                            <input type='text' autocomplete='off'
                                                oninput='javascript:{0}.SuggestUser(event);' onpaste='javascript:{0}.SuggestUser(event);' onkeyup='javascript:{0}.SuggestUser(event);'
                                                onclick='{0}.OnInputClick({0}, event);' onkeydown='{0}.ChangeSelection(event);'
                                                class='textEdit inputUserName' style='width:100%;' value='{1}'/>
                                        </td>
                                        <td width='16px'>
                                            <img align='absmiddle' src='{2}'
                                             onclick='{0}.ClearFilter();'
                                             style='cursor:pointer;' title='{3}'/>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>",
                                        _jsObjName,
                                        valueForInput,
                                        WebImageSupplier.GetAbsoluteWebPath("cross_grey.png"),
                                        UserControlsCommonResource.ClearFilter);

                        sb.AppendFormat("<input class='loginhidden' name='login' value='{0}' type='hidden'/>", SelectedUserId);
                        sb.Append("</div>");
                        sb.Append("</div>");
                    }

                    sb.Append("  <div class='adv-userselector-users'></div>");
                    sb.Append("  <div class='adv-userselector-deps'></div>");
                    sb.Append("</div>");
                }
                sb.Append("</div>");

                writer.Write(sb.ToString());
            }
        }

        #endregion
    }
}