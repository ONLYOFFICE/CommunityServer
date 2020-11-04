<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmInviteActivation.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Management.ConfirmInviteActivation" %>
<%@ Import Namespace="ASC.Web.Core.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<asp:PlaceHolder runat="server" ID="_confirmHolder">
    <div class="confirmBlock <%= isPersonal ? "confirm-personal" : "" %>">
        <div class="confirm-block-title header-base">
            <% if (_type == ConfirmType.EmpInvite)
               { if(isPersonal) {%>
            <h2 class="default-personal-popup_h"><%= Resource.ConfirmPersonalJoin %></h2>
            <div class="default-personal-popup_line"></div>
            <div class="default-personal-popup_text"><%= Resource.ConfirmPersonalText %></div>
            <%} else{ %>
                <%= Resource.YouDecidedToJoinThisPortal %>
            <% }
                }
               else
               { %>
                <%= Resource.InviteTitle %>
                <div class="subTitle">
                    <%= String.IsNullOrEmpty(_email) ? Resource.InvitePublicSubTitle : Resource.InviteSubTitle %>
                </div>
            <% } %>
        </div>
        <div>
            <div id="registrationForm" class="rightPart">

                <%--Email--%>
                <% if (String.IsNullOrEmpty(_email))
                   { %>
                <div class="property">
                    <div class="name">
                        <%= Resource.Email %>
                    </div>
                    <div class="value">
                        <input type="text" maxlength="64" id="studio_confirm_Email" name="emailInput" class="textEdit" value="<%= GetEmailAddress() %>" />
                    </div>
                </div>
                <% } %>
                
                <%--FirstName--%>
                <div class="property">
                    <div class="name">
                        <%= Resource.FirstName%>
                    </div>
                    <div class="value">
                        <input type="text" maxlength="64" id="studio_confirm_FirstName" value="<%= GetFirstName() %>" name="firstnameInput"
                            class="textEdit" />
                    </div>
                </div>
                <%--LastName--%>
                <div class="property">
                    <div class="name">
                        <%= Resource.LastName%>
                    </div>
                    <div class="value">
                        <input type="text" maxlength="64" id="studio_confirm_LastName" value="<%= GetLastName() %>" name="lastnameInput"
                            class="textEdit" />
                    </div>
                </div>

                <%--Pwd--%>
                <div class="property">
                    <div class="name">
                        <%= isPersonal ? Resource.Password : Resource.InvitePassword %>
                        <span class="HelpCenterSwitcher" onclick="jq(this).helper({ BlockHelperID: 'InvitePasswordHelp'});"></span>
                    </div>
                    <div class="value">
                        <input type="password" id="studio_confirm_pwd" value="" class="textEdit" autocomplete="off"
                            data-maxlength="<%= PasswordSettings.MaxLength %>"
                            data-regex="<%: PasswordSettings.GetPasswordRegex(PasswordSettings.Load()) %>"
                            data-help="<%= ASC.Web.Studio.Core.Users.UserManagerWrapper.GetPasswordHelpMessage() %>" />
                        <input type="hidden" id="passwordHash" name="passwordHash" />
                    </div>
                    <div class="popup_helper" id="InvitePasswordHelp">
                        <%= UserManagerWrapper.GetPasswordHelpMessage() %>
                        </div>
                </div>

                <div class="big-button-container">
                    <% if (!String.IsNullOrEmpty(_errorMessage))
                       { %>
                    <div class="errorText">
                        <%= _errorMessage %>
                    </div>
                    <% } %>
                    <a id="buttonConfirmInvite" class="button blue huge" >
                        <%= Resource.LoginRegistryButton%></a>
                </div>
                 <asp:PlaceHolder runat="server" ID="thrdParty" Visible="false"></asp:PlaceHolder>
            </div>
            <% if (!isPersonal){ %>
            <div class="leftPart">
                <div class="borderBase tintMedium portalInfo">
                    <a href="Auth.aspx">
                        <img class="logo" src="/TenantLogo.ashx?logotype=2" border="0" alt="" />
                    </a>
                    <div class="header-base big blue-text">
                        <%= ASC.Core.CoreContext.TenantManager.GetCurrentTenant().Name.HtmlEncode() %></div>
                    <div class="user borderBase">
                        <img class="avatar borderBase" src="<%= _userAvatar %>" alt="" />
                        <div class="name">
                            <div class="header-base-small">
                                <%= _userName %></div>
                            <div class="describe-text">
                                <%= _userPost %></div>
                        </div>
                    </div>
                </div>
                <div class="description">
                    <%= String.Format(Resource.InviteDescription.HtmlEncode(), "<b>", "</b>") %>
                </div>
            </div>
            <%} %>
        </div>
    </div>
</asp:PlaceHolder>