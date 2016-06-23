<%@ 
    Control
    Language="C#"
    Inherits="System.Web.Mvc.ViewUserControl"
%>

<div class="treeheader">Get Started</div>
<ul class="treeview root">
    <li>
        <%= Html.MenuActionLink("Basic concepts", "basic", "partners", "selected") %>
    </li>
    <li>
        <%= Html.MenuActionLink("Authentication", "authentication", "partners", "selected") %>
    </li>
</ul>

<div class="treeheader">Methods</div>
<ul class="treeview root" id="sideNav">
    <li>
        <%= Html.ActionLink("Partners", "index", new {id = "partners"}) %>
    </li>
    <li>
        <%= Html.ActionLink("Clients and payments", "index", new {id = "clients"}) %>
    </li>
    <li>
        <%= Html.ActionLink("Portals", "index", new {id = "portals"}) %>
    </li>
    <li>
        <%= Html.ActionLink("Keys", "index", new {id = "keys"}) %>
    </li>
    <li>
        <%= Html.ActionLink("Invoices", "index", new {id = "invoices"}) %>
    </li>
</ul>

<a class="forum" href="http://dev.onlyoffice.org/viewforum.php?f=9" target="_blank">Forum</a>