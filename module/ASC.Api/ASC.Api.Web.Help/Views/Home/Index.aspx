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
            <p>
                ONLYOFFICE™ API describes the main methods that allow you to interact with different ONLYOFFICE™ components.
                <br />
                Please select the necessary section below to learn more about which API methods are available for it.
            </p>
        </div>
        <% if (products.Length > 0) 
           { %>
        <div class="product-list clearfix">
            <% if (products.Contains("portals"))
                { %>
            <div class="product">
                <a href="<%= Url.Action("basic", "portals") %>">
                    <img src="<%= Url.Content("~/content/img/portals.png") %>" alt="Community Server">
                    Community Server
                </a>
            </div>
            <% } %>
            <% if (products.Contains("editors"))
                { %>
            <div class="product">
                <a href="<%= Url.Action("basic", "editors") %>">
                    <img src="<%= Url.Content("~/content/img/editors.png") %>" alt="Document Server">
                    Document Server
                </a>
            </div>
            <% } %>
            <% if (products.Contains("partners"))
                { %>
            <div class="product">
                <a href="<%= Url.Action("basic", "partners") %>">
                    <img src="<%= Url.Content("~/content/img/partners.png") %>" alt="Partners">
                    Partners
                </a>
            </div>
            <% } %>
            <% if (products.Contains("apisystem"))
                { %>
            <div class="product">
                <a href="<%= Url.Action("basic", "apisystem") %>">
                    <img src="<%= Url.Content("~/content/img/partners.png") %>" alt="Hosted Solution">
                    Hosted Solution
                </a>
            </div>
            <% } %>
        </div>
        <% } %>
    </div>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>