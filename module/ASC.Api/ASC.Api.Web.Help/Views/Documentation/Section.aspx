<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ASC.Api.Web.Help.DocumentGenerator.MsDocEntryPoint>" ContentType="text/html"%>

<%@ Import Namespace="ASC.Api.Web.Help.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Section - <%=Model.Name %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
        <div>
            <h1>
                <%=Model.Name%></h1>
            <p>
                <%=Model.Summary %></p>
            <p>
                <%=Model.Remarks%></p>
            <p>
                <%=Model.Example%></p>
        </div>
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
            <% foreach (var methodGroup in Model.Methods.OrderBy(x => x.HttpMethod, new HttpMethodOrderComarer()).ThenBy(x => x.Path.Length).GroupBy(x => x.Category))
               {
                   if (!string.IsNullOrEmpty(methodGroup.Key))
                   {%>
                   <tr>
                   <td colspan="2"><strong><%=methodGroup.Key %></strong></td>
                   </tr>
               <% } %>
                  <% foreach (var method in methodGroup)
                   {

            %>
            <tr>
                <td class="views-field-title">
                    <%=Html.DocMethodLink(Model, method)%>
                </td>
                <td class="views-field-body">
                    <%=method.Summary%>
                </td>
            </tr>
            <% }
          }%>
        </tbody>
    </table>
</asp:Content>
