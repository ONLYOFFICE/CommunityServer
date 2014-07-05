<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EmailAndPassword.ascx.cs" Inherits="ASC.Web.Studio.UserControls.FirstTime.EmailAndPassword" %>

<div id="requiredStep" class="clearFix">

    <div class="passwordBlock">
        <div class="header-base"><%=Resources.Resource.Password %></div>
        <div class="clearFix">
            <div class="pwd clearFix">
                <div class="label">
                    <%= Resources.Resource.EmailAndPasswordTypePassword %> <span class="info"><%= Resources.Resource.EmailAndPasswordTypePasswordRecommendations %></span><span>*</span>
                </div>
                <div class="float-left">
                    <input type="password" id="newPwd" class="textEdit" maxlength="30" />
                </div>
            </div>
            <div class="pwd">
                <div class="label">
                    <%= Resources.Resource.EmailAndPasswordConfirmPassword %><span>*</span>
                </div>
                <div>
                    <input type="password" id="confPwd" class="textEdit" maxlength="30" />
                </div>
            </div>
            <% if (IsVisiblePromocode)
               { %>
            <div class="promocode">
                <div class="label"><%= Resources.Resource.EmailAndPasswordPromocode %></div>
                <div>
                    <input id="promocode_input" class="textEdit" maxlength="30" />
                </div>
            </div>
            <% } %>
        </div>
    </div>

    <div class="portal">
        <div class="header-base"><%=Resources.Resource.WizardRegistrationSettings %></div>
        <div class="emailBlock">
            <span class="info">
                <%= Resources.Resource.EmailAndPasswordRegEmail %>
            </span>
            <span class="email">
                <span class="emailAddress">
                    <%= this.GetEmail() %>
                </span>
                <span class="changeEmail">
                    <span id="dvChangeMail"><a class="info link dotline blue" onclick="ASC.Controls.EmailAndPasswordManager.ShowChangeEmailAddress();"><%= Resources.Resource.EmailAndPasswordTypeChangeIt %></a></span>
                </span>
            </span>
        </div>
        <div class="domainBlock">
            <span class="info">
                <%= Resources.Resource.EmailAndPasswordDomain%>
            </span>
            <span class="domainname">
                <%= _curTenant.TenantDomain %>
            </span>
        </div>
        <div class="header-base"><%=Resources.Resource.WizardGenTimeLang%></div>
        <asp:PlaceHolder ID="_dateandtimeHolder" runat="server"></asp:PlaceHolder>
    </div>
</div>
