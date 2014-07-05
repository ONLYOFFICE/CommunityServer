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
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility.Skins;
using ASC.Data.Storage;
using ASC.Web.Studio.Core.Users;
using System.Web.UI.HtmlControls;

namespace ASC.Web.Studio.UserControls.Users
{
    public partial class UserSelector : System.Web.UI.UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserSelector/UserSelector.ascx"; }
        }

        public string Title { get; set; }

        public string SelectedUserListTitle { get; set; }

        public string UserListTitle { get; set; }

        public string CustomBottomHtml { get; set; }

        public List<Guid> SelectedUsers { get; set; }

        public List<Guid> DisabledUsers { get; set; }

        public string BehaviorID { get; set; }

        protected string _jsObjName;

        private List<UserGroup> _userGroups = new List<UserGroup>();

        protected string _selectorID = Guid.NewGuid().ToString().Replace('-', '_');

        public UserSelector()
        {
            SelectedUsers = new List<Guid>();
            DisabledUsers = new List<Guid>();
            UserListTitle = HttpUtility.HtmlDecode(CustomNamingPeople.Substitute<Resources.Resource>("Employees"));
            SelectedUserListTitle = Resources.Resource.Selected;
            Title = CustomNamingPeople.Substitute<Resources.Resource>("UserSelectDialogTitle");
            CustomBottomHtml = "";
        }

        private class UserGroup
        {
            public GroupInfo Group { get; set; }
            public List<UserInfo> Users { get; set; }

            public UserGroup()
            {
                Users = new List<UserInfo>();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _container.Options.IsPopup = true;


            _jsObjName = String.IsNullOrEmpty(BehaviorID) ? "__userSelector" + UniqueID : BehaviorID;

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/users/userselector/css/userselector.less"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/users/userselector/js/userselector.js"));

            var script = new StringBuilder();
            script.AppendFormat("var {0} = new ASC.Studio.UserSelector.UserSelectorPrototype('{1}', '{0}', {2});\n", _jsObjName, _selectorID, MobileDetector.IsMobile.ToString().ToLower());

            var noDepGroup = new UserGroup { Group = new GroupInfo { Name = "" } };
            foreach (var u in CoreContext.UserManager.GetUsers().SortByUserName())
            {
                if (CoreContext.UserManager.GetUserGroups(u.ID).Length == 0)
                {
                    noDepGroup.Users.Add(u);
                }
            }
            if (noDepGroup.Users.Count > 0)
            {
                noDepGroup.Users.RemoveAll(ui => DisabledUsers.Contains(ui.ID));
                _userGroups.Add(noDepGroup);
            }


            foreach (var g in CoreContext.GroupManager.GetGroups())
            {
                FillChildGroups(g);
            }
            _userGroups.Sort((ug1, ug2) => String.Compare(ug1.Group.Name, ug2.Group.Name));

            foreach (var ug in _userGroups)
            {
                var groupVarName = _jsObjName + "_ug_" + ug.Group.ID.ToString().Replace('-', '_');
                script.AppendFormat("var {0} = new ASC.Studio.UserSelector.UserGroupItem('{1}','{2}'); ", groupVarName, ug.Group.ID, ug.Group.Name.HtmlEncode().ReplaceSingleQuote());
                foreach (var u in ug.Users)
                {
                    var selected = SelectedUsers.Contains(u.ID);
                    script.AppendFormat(" {0}.Users.push(new ASC.Studio.UserSelector.UserItem('{1}','{2}',{3},{0},{4},'{5}')); ", groupVarName,
                                        u.ID,
                                        u.DisplayUserName().ReplaceSingleQuote().Replace(@"\", @"\\"),
                                        selected ? "true" : "false",
                                        selected ? "true" : "false",
                                        string.IsNullOrEmpty(u.Title) ? string.Empty : u.Title.HtmlEncode().ReplaceSingleQuote().Replace(@"\", @"\\"));
                }

                script.AppendFormat(" {0}.Groups.push({1}); ", _jsObjName, groupVarName);
            }

            Page.RegisterInlineScript(script.ToString(), onReady: false);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        private void FillChildGroups(GroupInfo groupInfo)
        {
            var users = new List<UserInfo>(CoreContext.UserManager.GetUsersByGroup(groupInfo.ID));
            users.RemoveAll(ui => (DisabledUsers.Find(dui => dui.Equals(ui.ID)) != Guid.Empty));
            users = users.SortByUserName();

            if (users.Count > 0)
            {
                var userGroup = new UserGroup { Group = groupInfo };
                _userGroups.Add(userGroup);
                foreach (var u in users)
                {
                    userGroup.Users.Add(u);
                }
            }
        }
    }
}