<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserProfileEditControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfile.UserProfileEditControl" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Core.SMS" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="Resources" %>
<%@ Import Namespace="ASC.Core.Users" %>

<div id="userProfileEditPage" class="containerBodyBlock">
    <div class="clearFix profile-title header-with-menu">
        <span id="titleEditProfile" class="header text-overflow"><%= GetTitle() %></span>
        <% if ((IsAdmin() || UserInfo.IsMe()) && UserInfo.IsLDAP())
        { %>
        <span class="ldap-lock-big" title="<%= Resource.LdapUsersListLockTitle %>"></span>
        <% } %>
    </div>
    <div class="profile-action-content clearFix">
        <div class="profile-photo-block">
            <div class="profile-user-photo">
                <div id="userProfilePhoto" class="profile-action-usericon">
                    <img src="<%= GetPhotoPath() %>" alt="" />
                </div>
                <div id="loadPhotoImage" class="action-block grey-phone">
                    <span class="bold">
                        <%= IsPageEditProfileFlag ? Resource.EditImage : Resource.AddImage %>
                    </span>
                </div>
                <% if (IsPageEditProfileFlag && IsAdmin())
                   { %>
                <div class="profile-role <%= Role.Class %>" title="<%= Role.Title%>"></div>
                <% } %>
            </div>
        </div>
        <table class="profile-action-userdata">
            <%--Type--%>
            <% if(!isPersonal){ %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= Resource.UserType %>:</td>
                <td class="userdata-value user-type">
                    <% if (CanAddUser)
                       { %>

                    <% if (IsVisitor && !CanEditType)
                       { %>
                    <span id="userTypeField" data-type="collaborator" class="link dotline nochange">
                        <%= CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode() %>
                    </span>
                    <% }
                       else
                       { %>
                    <% if (CanEditType)
                       { %>
                    <select id="userType" class="user-type-selector float-left" <%= CanEditType ? "" : "disabled = 'disabled'" %>>
                        <option class="optionItem" value="collaborator" <%= IsVisitor ? "selected='selected'" : "" %>><%= CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode() %></option>
                        <option class="optionItem" value="user" <%= IsVisitor ? "" : "selected='selected'" %>><%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %></option>
                    </select>
                    <% }
                       else
                       { %>
                    <span id="userTypeField" data-type="user" class="link dotline nochange">
                        <%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %>
                    </span>
                    <% } %>
                    <% } %>

                    <% }
                       else
                       { %>

                    <% if (IsVisitor || !IsPageEditProfileFlag)
                       { %>
                    <span id="userTypeField" data-type="collaborator" class="link dotline nochange">
                        <%= CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode() %>
                    </span>
                    <% }
                       else
                       { %>
                    <% if (CanEditType)
                       { %>
                    <select id="userType" class="user-type-selector" <%= CanEditType ? "" : "disabled='disabled'" %>>
                        <option class="optionItem" value="collaborator" <%= IsVisitor ? "selected='selected'" : "" %>>
                            <%= CustomNamingPeople.Substitute<Resource>("Guest").HtmlEncode() %>
                        </option>
                        <option class="optionItem" value="user" <%= IsVisitor ? "" : "selected='selected'" %>>
                            <%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %>
                        </option>
                    </select>
                    <% }
                       else
                       { %>
                    <span id="userTypeField" data-type="user" class="link dotline nochange">
                        <%= CustomNamingPeople.Substitute<Resource>("User").HtmlEncode() %>
                    </span>
                    <% } %>
                    <% } %>

                    <% } %>

                    <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'AnswerForProjectTeam'});"></div>
                    <div class="popup_helper" id="AnswerForProjectTeam">

                        <div id="collaboratorCanBlock" class="can-description-block <%= IsVisitor ? "" : "display-none" %>">
                            <%= CustomNamingPeople.Substitute(String.Format(Resource.CollaboratorCanDescribe.HtmlEncode(), "<p>", "</p><ul><li>", "</li><li>", "</li><li>", "</li></ul>")) %>
                        </div>
                        <div id="userCanBlock" class="can-description-block <%= IsVisitor ? "display-none" : "" %>">
                            <%= CustomNamingPeople.Substitute(String.Format(Resource.UserCanDescribe.HtmlEncode(), "<p>", "</p><ul><li>", "</li><li>", "</li><li>", "</li><li>", "</li></ul>")) %>
                        </div>

                    </div>
                </td>
            </tr>
            <% } %>
            <%--FirstName--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text requiredTitle"><%= Resource.FirstName %>:</td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profileFirstName" class="textEdit" value="<%= GetFirstName() %>" autocomplete="off" <%= IsLDAP ? "disabled title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : "" %> />
                </td>
            </tr>
            <%--LastName--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text requiredTitle"><%= Resource.LastName %>:</td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profileSecondName" class="textEdit" value="<%= GetLastName() %>" autocomplete="off" <%= IsLDAP ? "disabled title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : "" %>/>
                </td>
            </tr>
            <%--Email--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text">
                    <span class="requiredTitle"><%= IsLDAP ? Resource.Login : Resource.Email %>:</span> 
                       <% if (IsLDAP)
                       { %>
                            <div class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'LoginEmailInfo'});"></div>
                            <div class="popup_helper" id="LoginEmailInfo">
                                <p>
                                    <%= Resource.LoginDescription %>
                                </p>
                            </div>
                     <% } %>
                </td>
                <td class="userdata-value requiredField">
                    <input type="email" id="profileEmail" value="<%= GetEmail() %>" autocomplete="off" class="textEdit" <%= IsPageEditProfileFlag && !(CoreContext.Configuration.Personal && CoreContext.Configuration.Standalone) || IsLDAP ? "disabled" : "" %> <%= IsLDAP ? " title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : "" %> />
                </td>
            </tr>
            <%--Department--%>
            <% if ((IsAdmin() || Departments.Length != 0) && !isPersonal){ %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text">
                    <%= CustomNamingPeople.Substitute<Resource>("Department").HtmlEncode() %>:
                </td>
                <td id="departmentsField" class="userdata-value">
                    <% if (IsAdmin())
                       { %>
                    <ul class="departments-list advanced-selector-list-results"></ul>
                    <div><span id="chooseGroupsSelector" class="link dotline plus"><%= CustomNamingPeople.Substitute<Resource>("BindDepartmentButton").HtmlEncode() %></span></div>
                    <% }
                       else
                       { %>

                    <% foreach (var department in Departments)
                       { %>
                    <div class="field-value"><%= department.Name %></div>
                    <% } %>

                    <% } %>
                        
                </td>
            </tr>
            <% } %>
            <%--Position--%>
            <% if(!isPersonal){ %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= CustomNamingPeople.Substitute<Resource>("UserPost").HtmlEncode() %>:</td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profilePosition" <%= IsAdmin() && !IsLDAP ? "" : "disabled = 'disabled'"%> <%= IsLDAP ? "title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : "" %> class="textEdit" value="<%= GetPosition() %>" autocomplete="off" />
                    <span class="requiredErrorText"><%= Resource.ErrorMessageLongField64 %></span>
                </td>
            </tr>
            <%} %>
            <%--Phone--%>
            <% if (StudioSmsNotificationSettings.IsVisibleSettings && IsPageEditProfileFlag && !String.IsNullOrEmpty(Phone))
               { %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= Resource.MobilePhone %>:</td>
                <td class="userdata-value">
                    <div>+<%= Phone %></div>
                </td>
            </tr>
            <% } %>
            <%--Sex--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= Resource.Sex %>:</td>
                <td class="userdata-value">
                    <select id="userdataSex" data-value="<%= IsPageEditProfileFlag ? ProfileGender : "-1" %>">
                        <option class="optionItem" value="-1"><%= UserControlsCommonResource.LblSelect %></option>
                        <option class="optionItem" value="1"><%= Resource.MaleSexStatus %></option>
                        <option class="optionItem" value="0"><%= Resource.FemaleSexStatus %></option>
                    </select>
                </td>
            </tr>
            <%--Registration Date--%>
            <% if(!isPersonal){ %>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= CustomNamingPeople.Substitute<Resource>("WorkFromDate").HtmlEncode() %>:</td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profileRegistrationDate" class="textCalendar textEditCalendar" value="<%= GetWorkFromDate() %>" data-value="<%= GetWorkFromDate() %>" />
                    <span class="requiredErrorText"><%= Resource.ErrorNotCorrectDate %></span>
                </td>
            </tr>
            <%} %>
            <%--Birth Date--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= Resource.Birthdate %>:</td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profileBirthDate" class="textCalendar textEditCalendar" value="<%= GetBirthDate() %>" data-value="<%= GetBirthDate() %>" />
                    <span class="requiredErrorText"><%= Resource.ErrorNotCorrectDate %></span>
                </td>
            </tr>
            <%--Location--%>
            <tr class="userdata-field">
                <td class="userdata-title describe-text"><%= Resource.Location %>:</td>
                <td class="userdata-value requiredField">
                    <input type="text" id="profilePlace" class="textEdit" value="<%= GetPlace() %>" autocomplete="off" <%= IsLDAP ? "disabled title=\"" + Resource.LdapUserEditCanOnlyAdminTitle + "\"" : "" %> />
                    <span class="requiredErrorText"><%= Resource.ErrorMessageLongField255 %></span>
                </td>
            </tr>
        </table>
    </div>
    <%--Comment--%>
     <% if(!isPersonal){ %>
    <div id="commentTab" class="tabs-section">
        <span class="header-base"><%= Resource.Comments %></span>
        <span id="switcherCommentButton" class="toggle-button" data-switcher="0"
            data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>
    <div id="commentContainer" class="tabs-content">
        <textarea id="profileComment" class="textEdit" rows="4"><%= GetComment() %></textarea>
    </div>
    <%} %>
    <%--Contacts--%>
    <div id="contactInfoTab" class="tabs-section">
        <span class="header-base"><%= Resource.ContactInformation %></span>
        <span id="switcherContactInfoButton" class="toggle-button" data-switcher="0"
            data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>
    <div id="contactInfoContainer" class="tabs-content contacts-group">

        <div class="field-with-actions default">
            <select class="group-field">
                <option class="optionItem mail" value="mail" selected="selected"><%= Resource.TitleEmail %></option>
                <option class="optionItem phone" value="phone"><%= Resource.TitlePhone %></option>
                <option class="optionItem mobphone" value="mobphone"><%= Resource.TitleMobphone %></option>
                <option class="optionItem gmail" value="gmail"><%= Resource.TitleGooglemail %></option>
                <option class="optionItem skype" value="skype"><%= Resource.TitleSkype %></option>
                <option class="optionItem msn" value="msn"><%= Resource.TitleMsn %></option>
                <option class="optionItem icq" value="icq"><%= Resource.TitleIcq %></option>
                <option class="optionItem jabber" value="jabber"><%= Resource.TitleJabber %></option>
                <option class="optionItem aim" value="aim"><%= Resource.TitleAim %></option>
            </select>
            <a class="delete-field icon-link trash"></a>
            <input type="text" class="textEdit" value="" autocomplete="off" />
        </div>
        <a class="add-new-field link dotline plus">
            <%= Resource.BtnAddNewContact %></a>
    </div>
    <%--Social Nets--%>
    <div id="SocialTab" class="tabs-section">
        <span class="header-base"><%= Resource.SocialProfiles %></span>
        <span id="switcherSocialButton" class="toggle-button" data-switcher="0"
            data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>
    <div id="socialContainer" class="tabs-content contacts-group">
        <div class="field-with-actions default">
            <select class="group-field">
                <option class="optionItem facebook" value="facebook"><%= Resource.TitleFacebook %></option>
                <option class="optionItem livejournal" value="livejournal"><%= Resource.TitleLiveJournal %></option>
                <option class="optionItem myspace" value="myspace"><%= Resource.TitleMyspace %></option>
                <option class="optionItem twitter" value="twitter"><%= Resource.TitleTwitter %></option>
                <option class="optionItem blogger" value="blogger"><%= Resource.TitleBlogger %></option>
                <option class="optionItem yahoo" value="yahoo"><%= Resource.TitleYahoo %></option>
            </select>
            <a class="delete-field icon-link trash"></a>
            <input type="text" class="textEdit" value="" placeholder="<%= Resource.HintForSocialAccounts %>" autocomplete="off" />
        </div>
        <a class="add-new-field link dotline plus">
            <%= UserControlsCommonResource.AddNewSocialProfile %></a>
    </div>
    <div id="profileActionsContainer" class="big-button-container">
        <a id="profileActionButton" class="button blue big"><%= GetTextButton() %></a>
        <span class="splitter-buttons"></span>
        <a id="cancelProfileAction" data-url="<%= ASC.Web.Studio.Utility.CommonLinkUtility.GetUserProfile(UserInfo.ID) %>" class="button gray big">
            <%= UserControlsCommonResource.CancelButton %>
        </a>
    </div>
</div>
<asp:PlaceHolder ID="loadPhotoWindow" runat="server"></asp:PlaceHolder>
