<%@ 
    Control
    Language="C#"
    Inherits="System.Web.Mvc.ViewUserControl"
%>

<a class="logo" href="<%= Url.Action("index","home") %>"></a>

<% 
    var products = (ConfigurationManager.AppSettings["enabled_products"] ?? "").Split('|');
    var subControllerObj = ViewContext.RequestContext.RouteData.Values["id"];
    var subControllerStr = subControllerObj != null ? subControllerObj.ToString() : string.Empty;
%>
<% if (products.Length > 0)
   { %>
<ul class="top-nav">
    <% if (products.Contains("portals"))
       { %>
    <li class="<%= Html.IfController("portals") || subControllerStr.Equals("portals", StringComparison.OrdinalIgnoreCase) ? "active" : "" %>">
        <a href="<%= Url.Action("basic", "portals") %>">Community Server</a>
    </li>
    <% } %>
    <% if (products.Contains("editors"))
       { %>
    <li class="<%= Html.IfController("editors") || subControllerStr.Equals("editors", StringComparison.OrdinalIgnoreCase) ? "active" : "" %>">
        <a href="<%= Url.Action("basic", "editors") %>">Document Server</a>
    </li>
    <% } %>
    <% if (products.Contains("partners"))
       { %>
    <li class="<%= Html.IfController("partners") || subControllerStr.Equals("partners", StringComparison.OrdinalIgnoreCase) ? "active" : "" %>">
        <a href="<%= Url.Action("basic", "partners") %>">Partners</a>
    </li>
    <% } %>
    <% if (products.Contains("apisystem"))
       { %>
    <li class="<%= Html.IfController("apisystem") || subControllerStr.Equals("apisystem", StringComparison.OrdinalIgnoreCase) ? "active" : "" %>">
        <a href="<%= Url.Action("basic", "apisystem") %>">Hosted Solution</a>
    </li>
    <% } %>
</ul>
<% } %>