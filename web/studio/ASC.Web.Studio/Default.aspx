<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" EnableViewState="false" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Studio._Default" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <div id="GreetingBlock" class="<%= (7 <= ProductsCount && ProductsCount <= 10) ? "five-column-block" : "greating-block" %>">
        <div class="greating-modules-block">
            <% if (_showDocs != null) { %>
            <div class="docs-default-page clearFix">
                <a class="link docs-default-logo" href="<%= VirtualPathUtility.ToAbsolute(_showDocs.StartURL) %>">
                    <span class="inner">
                        <span class="title">
                            <%=_showDocs.Name %>
                        </span>
                        <span class="description">
                            <%= _showDocs.Description %>
                        </span>
                    </span>
                </a>
            </div>
            <% } %>

            <div class="default-list-products">
            <% foreach (var product in defaultListProducts) %>
            <% {
                   var productStartUrl = VirtualPathUtility.ToAbsolute(product.StartURL);
                   var productLabel = HttpUtility.HtmlEncode(product.Name);
                %>
                <a class="link header product" href="<%= productStartUrl %>">
                    <img alt="<%= productLabel %>" src="<%= product.GetLargeIconAbsoluteURL() + "?" + ResetCacheKey %>" />
                    <span class="title">
                        <%= productLabel %>
                    </span>
                </a>
            <% } %>
            <% if (TenantExtra.EnableControlPanel)
               { %>
                <a class="link header product" href="<%= SetupInfo.ControlPanelUrl %>" target="_blank">
                    <img alt="<%= Resource.ControlPanelLabel %>" src="<%= ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("icon-controlpanel.svg") + "?" + ResetCacheKey %>" />
                    <span class="title">
                        <%= Resource.ControlPanelLabel %>
                    </span>
                </a>
            <% } %>
            <% foreach (var item in CustomNavigationItems) { %>
                <a class="link header product" href="<%= item.Url.HtmlEncode() %>" target="_blank">
                    <img alt="<%= item.Label.HtmlEncode() %>" src="<%= item.BigImg + "?" + ResetCacheKey %>" />
                    <span class="title">
                        <%= item.Label.HtmlEncode() %>
                    </span>
                </a>
            <% } %>
            </div>
        </div>
    </div>

    <asp:PlaceHolder ID="WelcomePanelHolder" runat="server" />

    <% if (!string.IsNullOrEmpty(SetupInfo.UserVoiceURL))
       { %>
    <script type="text/javascript" src="<%= SetupInfo.UserVoiceURL %>"></script>
    <% } %>
</asp:Content>
