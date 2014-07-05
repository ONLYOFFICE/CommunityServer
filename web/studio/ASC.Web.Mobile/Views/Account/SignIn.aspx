<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ASC.Web.Mobile.Models.LoginModel>" %>

<asp:Content ID="Head" ContentPlaceHolderID="Head" runat="server" />

<asp:Content ID="Script" ContentPlaceHolderID="Script" runat="server">
</asp:Content>

<asp:Content ID="Body" ContentPlaceHolderID="Body" runat="server">
  <div class="ui-page page-auth ui-footer ui-page-active" data-page-id="pgauth-auth">
    <%if (!Model.IsAuthentificated)
    {%>
      <div class="ui-content">
        <a href="http://<%=Model.TenantAddress%>" class="ui-logo">
          <img alt="<%=Model.CompanyInfo%>" src="<%=Model.CompanyLogo ?? Url.Content("~/content/images/logo-teamlab.png")%>" />
          <span><%=Model.CompanyInfo%></span>
        </a>
        <%Html.EnableClientValidation();%>
        <%using (Html.BeginForm())
        {%>
          
          <% if (!Model.ByThirdParty)
             { %>
          <div class="auth-item-container">
            <label class="auth-label login-label" for="Email"><%= Resources.MobileResource.LblEmail %>:</label>
            <%= Html.TextBoxFor(m => m.Email,
                                new
                                    {
                                        type = "email",
                                        title = "Email",
                                        @class = "input-text",
                                        value = Model.Email
                                    }) %>
            <%= Html.ValidationMessageFor(m => m.Email) %>
          </div>
          <div class="auth-item-container">
            <label class="auth-label password-label" for="Password"><%= Resources.MobileResource.LblPassword %>:</label>
            <%= Html.PasswordFor(m => m.Password, new { title = "Your password", @class = "input-password", value = Model.Password }) %>
            <%= Html.ValidationMessageFor(m => m.Password) %>
            <%= Html.ValidationMessage("InvalidPassword") %>
          </div>
          <% } %>

          <% if (Model.RequestCode)
             { %>
          <div class="auth-item-container">
            <label class="auth-label code-label" for="Code"><%= Resources.MobileResource.LblCode %>:</label>
            <%= Html.TextBoxFor(m => m.Code, new { title = "Sms code", @class = "input-code" }) %>
            <%= Html.ValidationMessageFor(m => m.Code) %>
            <%= Html.ValidationMessage("InvalidCode") %>
          </div>
          <% } %>

          <div class="auth-item-container">
            <%=Html.AntiForgeryToken()%>
            <% if (Model.RequestCode)
               { %>
            <button class="ui-btn sign-in" ><%= Resources.MobileResource.LblLogin %></button>
            <br/>
            <button class="ui-btn sign-in" name="Resend" value="Resend" ><%= Resources.MobileResource.LblResendSms %></button>
            <% }
               else
               { %>
            <button class="ui-btn sign-in" name="SendCode" value="SendCode" ><%= Resources.MobileResource.LblLogin %></button>
            <% } %>
          </div>
          <div class="auth-item-container auth-item-container-auth-link">
            <a class="auth-link auth-link-linkedid" href="/login.ashx?auth=linkedin&mode=Redirect&min=true&returnurl=<%=HttpUtility.UrlEncode(new Uri(Request.GetUrlRewriter(),Url.RouteUrl("Login")).ToString())%>">&nbsp;</a>
            <a class="auth-link auth-link-twitter" href="/login.ashx?auth=twitter&mode=Redirect&min=true&returnurl=<%=HttpUtility.UrlEncode(new Uri(Request.GetUrlRewriter(),Url.RouteUrl("Login")).ToString())%>">&nbsp;</a>
            <a class="auth-link auth-link-facebook" href="/login.ashx?auth=facebook&mode=Redirect&min=true&returnurl=<%=HttpUtility.UrlEncode(new Uri(Request.GetUrlRewriter(),Url.RouteUrl("Login")).ToString())%>">&nbsp;</a>
            <a class="auth-link auth-link-google" href="/login.ashx?auth=google&mode=Redirect&min=true&returnurl=<%=HttpUtility.UrlEncode(new Uri(Request.GetUrlRewriter(),Url.RouteUrl("Login")).ToString())%>">&nbsp;</a>
          </div>
        <%
        }%>
    <%}
    else
    {%>
      <div class="ui-content">
        <a href="http://<%=Model.TenantAddress%>" class="ui-logo">
          <img src="<%=Model.UserPhoto%>" alt="<%=Model.UserInfo.FirstName+" "+Model.UserInfo.LastName%>" />
          <span><%=Model.UserInfo.FirstName + " " + Model.UserInfo.LastName%></span>
        </a>
        <form action="<%=Url.RouteUrl("Default", new { controller = "Account", action = "SignOut" })%>">
          <div class="auth-item-container">
            <button class="ui-btn sign-out" type="submit"><%=Resources.MobileResource.LblLogout%></button>
          </div>
        </form>
      </div>
    <%
    }%>
  </div>
  <div class="ui-footer">
    <div class="bottom-menu">
      <a class="copyrights" href="http://www.onlyoffice.com"><%=Resources.MobileResource.LblCopyrights%></a>
      <span class="sep">|</span>
      <a class="standart-vertion target-standart" href="/"><%=Resources.MobileResource.LblStandartVersion%></a>
    </div>
  </div>
</asp:Content>

<asp:Content ID="JsTemplate" ContentPlaceHolderID="JsTemplate" runat="server" />
