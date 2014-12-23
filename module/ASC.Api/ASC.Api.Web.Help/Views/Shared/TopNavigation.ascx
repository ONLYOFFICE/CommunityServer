<%@ 
    Control
    Language="C#"
    Inherits="System.Web.Mvc.ViewUserControl"
%>

<a class="logo" href="<%=Url.Action("index","help") %>"></a>

<% var products = (ConfigurationManager.AppSettings["enabled_products"] ?? "").Split('|'); %>
<% if (products.Length > 0)
   { %>
<ul class="top-nav">
    <% if (products.Contains("portals"))
       { %>
    <li class="<%= Html.IfController("portals") ? "active" : "" %>">
        <a href="<%= Url.Action("basic", "portals") %>">Portals</a>
    </li>
    <% } %>
    <% if (products.Contains("editors"))
       { %>
    <li class="<%= Html.IfController("editors") ? "active" : "" %>">
        <a href="<%= Url.Action("basic", "editors") %>">Editors</a>
    </li>
    <% } %>
    <% if (products.Contains("partners"))
       { %>
    <li class="<%= Html.IfController("partners") ? "active" : "" %>">
        <a href="<%= Url.Action("basic", "partners") %>">Partners</a>
    </li>
    <% } %>
</ul>
<% } %>