<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Collections.Generic.Dictionary<ASC.Api.Web.Help.DocumentGenerator.MsDocEntryPoint, System.Collections.Generic.Dictionary<ASC.Api.Web.Help.DocumentGenerator.MsDocEntryPointMethod, string>>>" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%if (string.IsNullOrEmpty(ViewData["query"] as string))
      { %>
Function list      
    <% }
      else
      {%>
Search - <%=ViewData["query"]%>
      <%} %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%if (Model.Any(x => x.Value.Any()))
      {%>
    <h2>
        Search results</h2>
    <table class="views-table cols-9 zebra-striped">
        <thead>
            <tr>
                <th class="views-field-title">
                    Resource
                </th>
                <th class="views-field-body">
                    Description
                </th>
            </tr>
        </thead>
        <tbody>
            <% foreach (var methodGroup in Model.Where(x => x.Value.Any()))
               {
                   if (!string.IsNullOrEmpty(methodGroup.Key.Name))
                   {%>
            <tr>
                <td colspan="2">
                    <strong>
                        <%=methodGroup.Key.Name%></strong>
                </td>
            </tr>
            <% } %>
            <% foreach (var method in methodGroup.Value)
               { %>
            <tr>
                <td class="views-field-title">
                    <%=Html.DocMethodLink(methodGroup.Key, method.Key)%>
                </td>
                <td class="views-field-body">
                    <%if (string.IsNullOrEmpty(method.Value))
                      {%>
                    <%= Highliter.HighliteString(method.Key.Summary, ViewData["query"] as string) %>
                    <% }
                      else
                      {%>
                    <%= Highliter.HighliteSearchString(method.Value) %>
                    <%} %>
                </td>
            </tr>
            <% }
}%>
        </tbody>
    </table>
    <% }
      else
      {%>
    <h2>
        No results found</h2>
    <%}%>
</asp:Content>
