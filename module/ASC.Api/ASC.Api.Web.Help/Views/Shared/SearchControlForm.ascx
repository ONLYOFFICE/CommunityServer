<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
    <form class="" action="<%=Url.Action("Search","Documentation") %>" method="GET">
        <input type="text" class="span9" name="query" placeholder="Search" value="<%=ViewData["query"]%>" />
        <input class="btn primary" type="submit" value="Search"/>
    </form>

