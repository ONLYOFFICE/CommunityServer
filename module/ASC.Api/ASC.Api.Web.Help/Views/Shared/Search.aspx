<%@ Import Namespace="ASC.Api.Web.Help.DocumentGenerator" %>
<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<Dictionary<MsDocEntryPoint, Dictionary<MsDocEntryPointMethod, string>>>"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
<% if (string.IsNullOrEmpty(ViewData["query"] as string))
   { %>
Search
<% }
   else
   { %>
Search - <%= ViewData["query"] %>
<% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% var model = Model as Dictionary<MsDocEntryPoint, Dictionary<MsDocEntryPointMethod, string>>;
       var result = ViewData["result"] as List<SearchResult> ?? new List<SearchResult>(); %>
    <% if (model.Any(x => x.Value.Any()) || result.Any())
       { %>
    <h1>
        <span class="hdr">Search results</span>
    </h1>
    <table class="table hover">
        <colgroup>
            <col style="width: 10%"/>
            <col style="width: 20%"/>
            <col style="width: 25%"/>
            <col/>
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Module</td>
                <td>Name</td>
                <td>Resource</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <% foreach (var methodGroup in model.Where(x => x.Value.Any()))
               {
                   var groupUrl = Url.DocUrl(methodGroup.Key, null, Html.GetCurrentController()); 
                   
                   foreach (var method in methodGroup.Value)
                   {
                       var methodUrl = Url.DocUrl(methodGroup.Key, method.Key, Html.GetCurrentController()); 
                        %>
            <tr class="tablerow">
                <td>
                    <% if (!string.IsNullOrEmpty(methodGroup.Key.Name))
                       { %>
                    <a class="underline" href="<%= groupUrl %>"><%= methodGroup.Key.Name %></a>
                    <% } %>
                </td>
                <td>
                    <a class="underline" href="<%= methodUrl %>">
                        <%= !string.IsNullOrEmpty(method.Key.FunctionName) ? method.Key.FunctionName : method.Key.ShortName %>
                    </a>
                </td>
                <td>
                    <a class="underline" href="<%= methodUrl %>">
                        <span class="uppercase"><%= method.Key.HttpMethod %></span>&nbsp;<%= method.Key.Path %>
                    </a>
                </td>
                <td>
                    <% if (string.IsNullOrEmpty(method.Value))
                       { %>
                    <%= Highliter.HighliteString(method.Key.Summary, ViewData["query"] as string) %>
                    <% }
                       else
                       { %>
                    <%= Highliter.HighliteSearchString(method.Value) %>
                    <% } %>
                </td>
            </tr>
            <% }
               }

               foreach (var res in result)
               { %>
            <tr class="tablerow">
                <td><%= res.Module %></td>
                <td>
                    <a class="underline" href="<%= res.Url %>"><%= res.Name %></a>
                </td>
                <td>
                    <a class="underline" href="<%= res.Url %>"><%= res.Resource %></a>
                </td>
                <td><%= res.Description %></td>
            </tr>
            <% } %>
        </tbody>
    </table>
    <% }
       else
       { %>
    <h1>
        <span class="hdr">No results found</span>
    </h1>
    <% } %>
</asp:Content>
