<%@ 
    Control
    Language="C#"
    Inherits="System.Web.Mvc.ViewUserControl"
%>

<% 
    var subController = ViewContext.RequestContext.RouteData.Values["id"];

    if (!Html.IfController("Help"))
    {
        Html.RenderPartial("SearchForm");
        Html.RenderAction("Navigation", (string) Html.GetCurrentController());
        subController = Html.GetCurrentController();
    }
    else if (subController != null)
    {
        var products = (ConfigurationManager.AppSettings["enabled_products"] ?? "").Split('|');
        if (products.Contains(subController.ToString()))
        {
            Html.RenderPartial("SearchForm");
            Html.RenderAction("Navigation", subController.ToString());
        }
        else
        {
            subController = null;
        }
    }
%>

<div class="treeheader">Help</div>
<ul class="treeview root">
    <li>
        <%=Html.MenuActionLink("F.A.Q.", "faq", "help", "selected", new { id = subController })%>
    </li>
    <li>
        <%=Html.MenuActionLink("Filtering", "filters", "help", "selected", new { id = subController })%>
    </li>
    <li>
        <%=Html.MenuActionLink("Batching", "batch", "help", "selected", new { id = subController })%>
    </li>
</ul>

<a class="forum" href="http://dev.onlyoffice.org/" target="_blank">Forum</a>