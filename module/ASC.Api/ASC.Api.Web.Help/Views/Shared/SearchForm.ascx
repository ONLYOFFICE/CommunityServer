<%@ 
    Control
    Language="C#"
    Inherits="System.Web.Mvc.ViewUserControl"
%>

<form class="search-box" action="<%= Url.Action("search", (string) Html.GetCurrentController()) %>" method="GET">
    <div class="search-input">
        <input type="text" name="query" placeholder="search" value="<%=ViewData["query"]%>" />
        <span class="search-clear"></span>
    </div>
    <a class="btn"></a>
</form>