<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Welcome
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% var products = (ConfigurationManager.AppSettings["enabled_products"] ?? "").Split('|'); %>
    <div class="products">
        <div class="products-info">
            <h1>
                <span class="hdr">Welcome to onlyoffice api</span>
            </h1>
            <p>The ONLYOFFICE™ API is implemented as REST over HTTP using GET/POST/PUT/DELETE. All the resources, like posts or comments, have their own URLs and are designed to be manipulated in isolation.</p>
        </div>
        <% if (products.Length > 0) 
           { %>
        <div class="product-list">
            <% if (products.Contains("portals"))
                { %>
            <div class="product">
                <a href="<%= Url.Action("basic", "portals") %>">
                    <img src="<%= Url.Content("~/content/img/portals.png") %>">
                    Portals
                </a>
            </div>
            <% } %>
            <% if (products.Contains("editors"))
                { %>
            <div class="product">
                <a href="<%= Url.Action("basic", "editors") %>">
                    <img src="<%= Url.Content("~/content/img/editors.png") %>">
                    Editors
                </a>
            </div>
            <% } %>
            <% if (products.Contains("partners"))
                { %>
            <div class="product">
                <a href="<%= Url.Action("basic", "partners") %>">
                    <img src="<%= Url.Content("~/content/img/partners.png") %>">
                    Partners
                </a>
            </div>
            <% } %>
        </div>
        <% } %>
    </div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>