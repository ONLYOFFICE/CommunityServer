<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<ASC.Api.Web.Help.DocumentGenerator.MsDocEntryPoint>>" %>
<ul class="nav">
    <li class="dropdown <%=!Request.Url.PathAndQuery.Contains("docs/")?"active":""%>"><a href="<%=Url.Action("Index", "Help")%>" class="dropdown-toggle">Help</a>
        <ul class="dropdown-menu">
            <li>
                <%=Html.MenuActionLink("Basic concepts", "Basic", "Help", "active")%></li>
            <li>
                <%=Html.MenuActionLink("Authentication", "Authentication", "Help", "active")%></li>
            <li>
                <%=Html.MenuActionLink("F.A.Q.", "Faq", "Help", "active")%></li>
            <li>
                <%=Html.MenuActionLink("Filtering", "Filters", "Help", "active")%></li>
            <li>
                <%=Html.MenuActionLink("Batching", "Batch", "Help", "active")%></li>
        </ul>
    </li>
    <li class="dropdown <%=Request.Url.PathAndQuery.Contains("docs/")?"active":""%>"><a href="#" class="dropdown-toggle">Methods</a>
        <ul class="dropdown-menu">
            <% foreach (var entryPoint in Model)
               {%>
            <li><a href="<%=Url.DocUrl(entryPoint)%>"><%=entryPoint.Name%></a></li>
            <% } %>
        </ul>
    </li>
</ul>
