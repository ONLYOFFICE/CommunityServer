<%@ Page Language="C#" MasterPageFile="~/Masters/basetemplate.master" AutoEventWireup="true" EnableViewState="false" CodeBehind="Auth.aspx.cs" Inherits="ASC.Web.Studio.Auth" Title="ONLYOFFICE™" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="Resources" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <% if (CoreContext.Configuration.Personal)
       { %>
    <asp:PlaceHolder runat="server" ID="AutorizeDocuments"></asp:PlaceHolder>
    <% }
       else
       { %>
    <div class="auth-form-page">
        <div id="GreetingBlock" class="authForm <%= withHelpBlock ? "" : "help-block-none" %>">
            <%--header and logo--%>
            <div class="header">
                <img class="logo" src="<%= LogoPath %>" alt="<%= CoreContext.TenantManager.GetCurrentTenant().Name.HtmlEncode() %>" />
                <h1 class="header-base big blue-text"><%= CoreContext.TenantManager.GetCurrentTenant().Name.HtmlEncode() %></h1>
            </div>

            <asp:PlaceHolder runat="server" ID="AuthorizeHolder"></asp:PlaceHolder>

            <div class="help-block-signin">
                <asp:PlaceHolder runat="server" ID="CommunitationsHolder"></asp:PlaceHolder>
            </div>
        </div>
    </div>
    <% } %>
    <% if (!string.IsNullOrEmpty(SetupInfo.UserVoiceURL))
       { %>
    <script type="text/javascript" src="<%= SetupInfo.UserVoiceURL %>"></script>
    <% } %>
</asp:Content>

<asp:Content ContentPlaceHolderID="FooterContent" runat="server">
    <% if (!CoreContext.Configuration.Personal)
       { %>
    <div class="footerAuth">
        <%= Resource.PoweredBy %>
        <a href="http://www.onlyoffice.com/" title="www.onlyoffice.com" class="link underline" target="_blank">www.onlyoffice.com</a>
         <%if (IsAutorizePartner.HasValue && Partner != null) { %>
            <span class="float-right">
                <%= IsAutorizePartner.Value ? (Partner.DisplayName ?? Partner.CompanyName).HtmlEncode() + " • <a class=\"link\" href=\"" + (Partner.Url.StartsWith("http:") || Partner.Url.StartsWith("https:") ? Partner.Url : string.Concat("http://", Partner.Url)) +"\" target=\"_blank\">" + Partner.Url + "</a>" 
            : Resource.HostedNonAuthorizedVersion%></span>
         <% } %>
    </div>
    <% } %>
</asp:Content>
