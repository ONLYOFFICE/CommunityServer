<%@ 
    Control
    Language="C#"
    Inherits="System.Web.Mvc.ViewUserControl"
%>

<% if (!Html.IfController("Help"))
   {
       Html.RenderPartial("SearchForm");
       Html.RenderAction("Navigation", (string)Html.GetCurrentController());
   } %>

<div class="treeheader">Help</div>
<ul class="treeview root">
    <li>
        <%=Html.MenuActionLink("F.A.Q.", "faq", "help", "selected")%>
    </li>
    <li>
        <%=Html.MenuActionLink("Filtering", "filters", "help", "selected")%>
    </li>
    <li>
        <%=Html.MenuActionLink("Batching", "batch", "help", "selected")%>
    </li>
</ul>

<a class="forum" href="http://dev.onlyoffice.org/" target="_blank">Forum</a>