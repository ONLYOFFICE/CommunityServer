<%@ Page Language="C#" MasterPageFile="~/Masters/basetemplate.master" AutoEventWireup="true" EnableViewState="false" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Studio._Default" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <div id="GreetingBlock" class="greating-block">
        <div class="greating-modules-block">
            <% if (_showDocs != null) { %>
            <div class="docs-default-page">
                <a class="docs-default-logo" href="<%= VirtualPathUtility.ToAbsolute(_showDocs.StartURL) %>"></a>
                <h2 class="title">
                    <a class="link header" href="<%= VirtualPathUtility.ToAbsolute(_showDocs.StartURL) %>">
                        <%=_showDocs.Name %></a>
                </h2>
                <span class="description">
                    <%= (CurrentUser.IsAdmin()) ? _showDocs.ExtendedDescription : _showDocs.Description %>
                </span>
            </div>
            <div class="clearFix"></div>
            <% } %>

            <div class="default-list-products">
            <% foreach (var product in defaultListProducts) %>
            <% {
                   var productStartUrl = VirtualPathUtility.ToAbsolute(product.StartURL);
                   var productLabel = GetProductLabel(product);
                %>
                <div class="product clearFix">
                        <a class="image-link" href="<%= productStartUrl %>">
                            <img alt="<%= productLabel %>" src="<%= product.GetLargeIconAbsoluteURL() %>" /></a>
                        <h2 class="title">
                            <a class="link header" href="<%= productStartUrl %>">
                                <%= productLabel %>
                            </a>
                        </h2>
                    </div>
            <% } %>
            <% if (TenantExtra.EnableControlPanel)
               { %>
                <div class="product clearFix">
                    <a class="image-link" href="<%= SetupInfo.ControlPanelUrl %>" target="_blank">
                        <img alt="<%= Resource.ControlPanelLabel %>" src="<%= ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("icon-controlpanel.png") %>" />
                    </a>
                    <h2 class="title">
                        <a class="link header" href="<%= SetupInfo.ControlPanelUrl %>" target="_blank">
                            <%= Resource.ControlPanelLabel %>
                        </a>
                    </h2>
                </div>
            <% } %>
            </div>
        </div>
    </div>

    <% if (!string.IsNullOrEmpty(SetupInfo.UserVoiceURL))
       { %>
    <script type="text/javascript" src="<%= SetupInfo.UserVoiceURL %>"></script>
    <% } %>
</asp:Content>
