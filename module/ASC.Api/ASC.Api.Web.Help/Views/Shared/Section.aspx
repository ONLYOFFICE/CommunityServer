<%@ Import Namespace="ASC.Api.Web.Help.Models" %>
<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<SectionViewModel>"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <% var model = Model as SectionViewModel; %>
    <%= string.IsNullOrEmpty(model.Category) ? model.Section.Name : model.Category %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% var model = Model as SectionViewModel; %>

    <% if (string.IsNullOrEmpty(model.Category))
        { %>
    <h1>
        <span class="hdr"><%= model.Section.Name %></span>
    </h1>
    <% if (!string.IsNullOrEmpty(model.Section.Summary))
        { %><p><%= model.Section.Summary %></p><% } %>
    <% if (!string.IsNullOrEmpty(model.Section.Remarks))
        { %><p><%= model.Section.Remarks %></p><% } %>
    <% if (!string.IsNullOrEmpty(model.Section.Example))
        { %><p><%= model.Section.Example %></p><% } %>
    <% }
        else
        { %>
    <h1>
        <a class="up" href="<%= Url.DocUrl(model.Section, null, Html.GetCurrentController()) %>"></a>
        <span class="hdr"><%= model.Category %></span>
    </h1>
    <% } %>

    <table class="table hover">
        <colgroup>
            <col style="width: 25%"/>
            <col style="width: 25%"/>
            <col/>
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Name</td>
                <td>Resource</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <% foreach (var method in model.Methods.OrderBy(x => x.HttpMethod, new HttpMethodOrderComarer()).ThenBy(x => x.Path.Length))
               {
                   var url = Url.DocUrl(model.Section, method, Html.GetCurrentController()); %>
            <tr class="tablerow">
                <td>
                    <a class="underline" href="<%= url %>">
                        <%= !string.IsNullOrEmpty(method.FunctionName) ? method.FunctionName : method.ShortName %>
                    </a>
                </td>
                <td>
                    <a class="underline" href="<%= url %>">
                        <span class="uppercase"><%= method.HttpMethod %></span>&nbsp;<%= method.Path %>
                    </a>
                </td>
                <td>
                    <%= method.Summary %>
                </td>
            </tr>
            <% } %>
        </tbody>
    </table>
</asp:Content>
