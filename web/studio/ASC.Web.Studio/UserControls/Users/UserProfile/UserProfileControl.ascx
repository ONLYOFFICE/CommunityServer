<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserProfileControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserProfileControl" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Users.UserProfile" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="studio_userProfileCardInfo" data-id="<%= UserInfo.ID %>"></div>
<div class="userProfileCard clearFix<%= (UserInfo.ActivationStatus == EmployeeActivationStatus.Pending) ? " pending" : "" %>">
    <div class="additionInfo">
        <div class="profile-user-photo">
            <div id="userProfilePhoto" class="profile-action-usericon">
                <img alt="" src="<%= MainImgUrl %>" />
            </div>
            <% if (UserInfo.IsMe() || IsAdmin)
               { %>
            <div id="loadPhotoImage" class="action-block grey-phone">
                <span class="bold">
                    <%= Resource.EditImage %>
                </span>
            </div>
            <% } %>

            <div class="profile-status blocked" data-visible="<%= UserInfo.Status == EmployeeStatus.Terminated ? "" : "hidden" %>">
                <div>
                    <%= Resource.DisabledEmployeeTitle %>
                    <br />
                    <%= UserInfo.TerminatedDate == null ? string.Empty : UserInfo.TerminatedDate.Value.ToShortDateString() %>
                </div>
            </div>

            <% if (UserInfo.ActivationStatus == EmployeeActivationStatus.Pending || UserInfo.ActivationStatus == EmployeeActivationStatus.NotActivated)
               { %>

            <div class="profile-status pending" data-visible="<%= UserInfo.Status == EmployeeStatus.Terminated ? "hidden" : "" %>">
                <% if (UserInfo.ActivationStatus == EmployeeActivationStatus.Pending)
                   { %>
                <div onclick="EmailOperationManager.ShowResendInviteWindow('<%= UserInfo.Email %>','<%= UserInfo.ID %>',<%= IsAdmin.ToString().ToLower() %>);return false;">
                    <%= Resource.PendingTitle %>
                </div>
                <% }
                   else if (UserInfo.ActivationStatus == EmployeeActivationStatus.NotActivated)
                   { %>
                <div onclick="EmailOperationManager.ShowEmailActivationWindow('<%= UserInfo.Email %>','<%= UserInfo.ID %>', true);return false;">
                    <%= Resource.PendingTitle %>
                </div>
                <% } %>
            </div>

            <% } %>
            <% if (IsAdmin)
               { %>
            <div class="profile-role <%= Role.Class %>" title="<%= Role.Title%>"></div>
            <% } %>
        </div>
    </div>

    <div class="userInfo">
        <div class="profile-info">
            <table class="info">
                <tr>
                    <td>
                        <div class="left-column">
                            <div class="field">
                                <span class="field-title describe-text"><%= Resource.UserType %>:</span>
                                <span id="typeUserProfile" class="field-value"><%= CustomNamingPeople.Substitute<Resource>(UserInfo.IsVisitor() ? "Guest" : "User").HtmlEncode() %></span>
                            </div>
                            <asp:PlaceHolder ID="_phEmailControlsHolder" runat="server"></asp:PlaceHolder>

                            <asp:Repeater ID="DepartmentsRepeater" runat="server" ItemType="ASC.Core.Users.GroupInfo">
                                <HeaderTemplate>
                                    <div class="field">
                                        <span class="field-title describe-text"><%= CustomNamingPeople.Substitute<Resource>("Department").HtmlEncode() %>:</span>
                                        <span id="groupsUserProfile" class="field-value">
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <%if (IsVisitor)
                                      { %>
                                    <span><%# Item.Name.HtmlEncode() %></span>
                                    <%}
                                      else
                                      { %>
                                    <a class="link dotline" href="<%# CommonLinkUtility.GetDepartment(Item.ID) %>"><%# Item.Name.HtmlEncode() %></a>
                                    <%} %>
                                </ItemTemplate>
                                <SeparatorTemplate>, </SeparatorTemplate>
                                <FooterTemplate>
                                    </span>
                                    </div>
                                </FooterTemplate>
                            </asp:Repeater>

                            <% if (!String.IsNullOrEmpty(UserInfo.Title))
                               { %>
                            <div class="field">
                                <span class="field-title describe-text"><%= CustomNamingPeople.Substitute<Resource>("UserPost").HtmlEncode() %>:</span>
                                <span id="titleUserProfile" class="field-value"><%= HttpUtility.HtmlEncode(UserInfo.Title) %></span>
                            </div>
                            <% } %>
                            <% if (Actions.AllowEdit && UserInfo.Status != EmployeeStatus.Terminated && UserInfo.ActivationStatus == EmployeeActivationStatus.Activated)
                               { %>
                            <div class="field">
                                <span class="field-title describe-text"><%= Resource.Password %>:</span>
                                <span id="passwordUserProfile" class="field-value">********</span>
                                <a onclick="PasswordTool.ShowPwdReminderDialog('1','<%= UserInfo.Email %>'); return false;" class="baseLinkAction">&nbsp;</a>
                            </div>
                            <% if (UserInfo.IsMe())
                               { %>
                            <asp:PlaceHolder ID="_phLanguage" runat="server"></asp:PlaceHolder>
                            <% } %>
                            <% } %>

                            <% if (ShowPrimaryMobile)
                               { %>
                            <div class="field">
                                <span class="field-title describe-text"><%= Resource.MobilePhone %>:</span>
                                <span id="phoneUserProfile" class="field-value">
                                    <% if (!String.IsNullOrEmpty(UserInfo.MobilePhone))
                                       { %>
                                    <span class="primarymobile">+<%= UserInfo.MobilePhone %></span>
                                    <% } %>
                                    <% if (UserInfo.IsMe() || Actions.AllowAddOrDelete)
                                       { %>
                                    <a onclick="ASC.Controls.UserMobilePhoneManager.openDialog();" class="baseLinkAction" title="<%= Resource.MobilePhoneChange %>"></a>
                                    <% } %>
                                </span>
                            </div>

                            <asp:PlaceHolder runat="server" ID="ChangeMobileHolder"></asp:PlaceHolder>
                            <% } %>
                        </div>
                        <div class="right-column">
                            <% if (UserInfo.Sex.HasValue)
                               { %>
                            <div class="field">
                                <span class="field-title describe-text"><%= Resource.Sex %>:</span>
                                <span id="sexUserProfile" class="field-value"><%= (UserInfo.Sex.HasValue ? UserInfo.Sex.Value ? Resource.MaleSexStatus : Resource.FemaleSexStatus : string.Empty) %></span>
                            </div>
                            <% } %>
                            <% if (UserInfo.WorkFromDate.HasValue)
                               { %>
                            <div class="field">
                                <span class="field-title describe-text"><%= CustomNamingPeople.Substitute<Resource>("WorkFromDate").HtmlEncode() %>:</span>
                                <span id="workFromUserProfile" class="field-value"><%= UserInfo.WorkFromDate == null ? string.Empty : UserInfo.WorkFromDate.Value.ToShortDateString() %></span>
                            </div>
                            <% } %>
                            <% if (UserInfo.BirthDate.HasValue)
                               { %>
                            <div class="field birthday">
                                <span class="field-title describe-text"><%= Resource.Birthdate %>:</span>
                                <span id="birthdayUserProfile" class="field-value"><%= UserInfo.BirthDate == null ? string.Empty : UserInfo.BirthDate.Value.ToShortDateString() %></span>
                                <% if (HappyBirthday >= 0 && HappyBirthday < 4)
                                   { %>
                                <span class="birthday-fest">
                                    <%= BirthDayText %>
                                </span>
                                <% if ((HappyBirthday == 0) && !UserInfo.IsMe() && (UserInfo.ActivationStatus == EmployeeActivationStatus.Activated) && (UserInfo.Status == EmployeeStatus.Active))
                                   { %>
                                <a target="_blank" href="<%= VirtualPathUtility.ToAbsolute("~/addons/mail/#composeto/email=" + UserInfo.Email.ToLower()) %>"
                                    class="button gray"><%= Resource.CongratulateBirthday %></a>
                                <% }
                                   } %>
                            </div>
                            <% } %>
                            <% if (!String.IsNullOrEmpty(UserInfo.Location))
                               { %>
                            <div class="field">
                                <span class="field-title describe-text"><%= Resource.Location %>:</span>
                                <span id="locationUserProfile" class="field-value"><%= HttpUtility.HtmlEncode(UserInfo.Location) %></span>
                            </div>
                            <% } %>
                            <% if (AffiliateHelper.ButtonAvailable(UserInfo))
                               { %>
                            <div class="field">
                                <span class="field-title describe-text"><%= Resource.AffilliateStatus %>:</span>
                                <span id="affilliateStatusUserProfile" class="field-value">
                                    <span id="joinToAffilliate" class="button gray"><%= Resource.JoinToAffilliateProgram %></span>
                                    <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'HelpJoinToAffilliate'});"></span>
                                    <br />
                                    <span id="errorAffilliate" class="errorText"></span>
                                </span>
                                <div class="popup_helper" id="HelpJoinToAffilliate">
                                    <p><%= Resource.HelpJoinToAffilliate %></p>
                                </div>
                            </div>
                            <% } %>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <% if (ShowSocialLogins && AccountLinkControl.IsNotEmpty)
           { %>
        <div class="user-block social-logins">
            <div class="tabs-section">
                <span class="header-base"><%= Resource.LoginSocialNetworks %></span>
                <span id="switcherAccountLinks" class="toggle-button"
                    data-switcher="0" data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
                    <%= Resource.Hide %>
                </span>
            </div>
            <asp:PlaceHolder runat="server" ID="_accountPlaceholder"></asp:PlaceHolder>
        </div>
        <% } %>
    </div>
</div>

<% if (!String.IsNullOrEmpty(UserInfo.Notes.HtmlEncode()))
   { %>
<div class="user-block profile-comment">
    <div class="tabs-section">
        <span class="header-base"><%= Resource.Comments %></span>
        <span id="switcherCommentButton" class="toggle-button"
            data-switcher="0" data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>
    <span id="commentContainer" class="inner-text tabs-content"><%= UserInfo.Notes.HtmlEncode().Replace("\n", "<br/>") %></span>
</div>
<% } %>

<% if (UserProfileHelper.Phones.Count > 0 || UserProfileHelper.Emails.Count > 0 || UserProfileHelper.Messengers.Count > 0)
   { %>
<div class="user-block profile-contacts">
    <div class="tabs-section">
        <span class="header-base"><%= Resource.ContactInformation %></span>
        <span id="switcherContactsPhoneButton" class="toggle-button"
            data-switcher="0" data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>
    <ul id="contactsPhoneContainer" class="contacts tabs-content">
        <% if (UserProfileHelper.Phones.Count > 0)
           { %>
        <li class="contact">
            <asp:Repeater ID="ContactPhones" ItemType="ASC.Web.Studio.Core.Users.MyContact" runat="server">
                <ItemTemplate>
                    <div class="profile-contact <%# Item.classname %>">
                        <%# Item.link%>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </li>
        <% } %>
        <% if (UserProfileHelper.Emails.Count > 0)
           { %>
        <li class="contact">
            <asp:Repeater ID="ContactEmails" ItemType="ASC.Web.Studio.Core.Users.MyContact" runat="server">
                <ItemTemplate>
                    <div class="profile-contact <%# Item.classname %>">
                        <%# Item.link %>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </li>
        <% } %>
        <% if (UserProfileHelper.Messengers.Count > 0)
           { %>
        <li class="contact">
            <asp:Repeater ID="ContactMessengers" ItemType="ASC.Web.Studio.Core.Users.MyContact" runat="server">
                <ItemTemplate>
                    <div class="profile-contact <%# Item.classname %>">
                        <%# Item.link %>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </li>
        <% } %>
        <li class="clear"></li>
    </ul>
</div>
<% } %>
<% if (UserProfileHelper.Contacts.Count > 0)
   { %>
<div class="user-block social-links">
    <div class="tabs-section">
        <span class="header-base"><%= Resource.SocialProfiles %></span>
        <span id="switcherContactsSocialButton" class="toggle-button"
            data-switcher="0" data-showtext="<%= Resource.Show %>" data-hidetext="<%= Resource.Hide %>">
            <%= Resource.Hide %>
        </span>
    </div>
    <ul id="contactsSocialContainer" class="contacts">
        <asp:Repeater ID="ContactSoccontacts" ItemType="ASC.Web.Studio.Core.Users.MyContact" runat="server">
            <ItemTemplate>
                <li class="contact">
                    <div class="profile-contact <%# Item.classname %>">
                        <%# Item.link %>
                    </div>
                </li>
            </ItemTemplate>
        </asp:Repeater>
        <li class="clear"></li>
    </ul>
</div>
<% } %>

<asp:PlaceHolder runat="server" ID="_editControlsHolder"></asp:PlaceHolder>

<asp:PlaceHolder ID="userEmailChange" runat="server"></asp:PlaceHolder>

<div id="studio_deleteProfileDialog" class="display-none">
    <sc:Container runat="server" ID="_deleteProfileContainer">
        <Header>
            <%= Resource.DeleteProfileTitle%>
        </Header>
        <Body>
            <div id="remove_content">
                <div><%= Resource.DeleteProfileInfo%></div>
                <a target="_blank" href="<%=VirtualPathUtility.ToAbsolute("~/addons/mail/#composeto/email=" + UserInfo.Email.ToLower())%>" class="link blue underline email"><%= UserInfo.Email %></a>
            </div>
            <div class="clearFix middle-button-container">
                <a href="javascript:void(0);" class="button blue middle" onclick="SendInstrunctionsToRemoveProfile();"><%= Resource.SendButton %></a>
                <span class="splitter-buttons"></span>
                <a href="javascript:void(0);" class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();"><%= Resource.CloseButton %></a>
            </div>
        </Body>
    </sc:Container>
</div>
<asp:PlaceHolder ID="loadPhotoWindow" runat="server"></asp:PlaceHolder>
