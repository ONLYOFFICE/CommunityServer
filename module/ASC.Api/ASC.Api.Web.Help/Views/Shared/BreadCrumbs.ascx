<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
    var breadcrumbs = ViewData["breadcrumbs"] as List<BreadCrumbsBuilder.BreadCrumb>;
%>
<%if (breadcrumbs != null && breadcrumbs.Count > 0)
  {%>
<p class="breadcrumb">
    <%=Html.ActionLink("Home","Index","Help") %>
    <span class="divider">→</span>
    <% for (int i = 0; i < breadcrumbs.Count-1; i++)
       {%>
           <a href="<%=breadcrumbs[i].Link%>"><%=breadcrumbs[i].Text%></a>
    <span class="divider">→</span>

    <%} %>
    <span><%=breadcrumbs[breadcrumbs.Count-1].Text%></span>
</p>
<% } %>
