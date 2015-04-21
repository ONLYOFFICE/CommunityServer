<%@ Import Namespace="ASC.Api.Web.Help.DocumentGenerator" %>
<%@ 
    Control
    Language="C#"
    Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<MsDocEntryPoint>>"
%>

<% var entryPoints = Model as List<MsDocEntryPoint>; %>

<div class="treeheader">Get Started</div>
<ul class="treeview root">
    <li>
        <%=Html.MenuActionLink("Basic concepts", "basic", "portals", "selected")%>
    </li>
    <li>
        <%=Html.MenuActionLink("Passing authentication", "auth", "portals", "selected")%>
    </li>
</ul>

<% if (entryPoints != null && entryPoints.Any())
   { %>
<div class="treeheader">Portal api methods</div>
<ul class="treeview root" id="sideNav">
    <% foreach (var entryPoint in entryPoints.OrderBy(x => x.Name).ToList())
       { %>
    <li>
        <a href="<%= Url.DocUrl(entryPoint, null, "portals") %>"><%= entryPoint.Name %></a>
        <% var categories = entryPoint.Methods.Where(x => !string.IsNullOrEmpty(x.Category)).GroupBy(x => x.Category).OrderBy(x => x.Key).ToList(); %>
        <% if (categories.Any()) { %>
        <ul class="treeview">
        <% foreach (var category in categories)
           { %>
            <li>
                <a href="<%= Url.DocUrl(entryPoint.Name, category.Key, null, null, "portals") %>"><%= category.Key %></a>
            </li>
        <% } %>
        </ul>
        <% } %>
    </li>
    <% } %>
</ul>
<% } %>

<div class="treeheader">Help</div>
<ul class="treeview root">
    <li>
        <%=Html.MenuActionLink("F.A.Q.", "faq", "portals", "selected")%>
    </li>
    <li>
        <%=Html.MenuActionLink("Filtering", "filters", "portals", "selected")%>
    </li>
    <li>
        <%=Html.MenuActionLink("Batching", "batch", "portals", "selected")%>
    </li>
</ul>

<a class="forum" href="http://dev.onlyoffice.org/" target="_blank">Forum</a>