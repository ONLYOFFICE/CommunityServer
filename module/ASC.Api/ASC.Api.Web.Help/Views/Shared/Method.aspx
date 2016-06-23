<%@ Import Namespace="ASC.Api.Web.Help.Models" %>
<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<MethodViewModel>"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <% var method = (Model as MethodViewModel).Method; %>
    <%= !string.IsNullOrEmpty(method.ShortName) ? method.ShortName : method.Summary %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% var model = Model as MethodViewModel;
       var method = model.Method; %>
    <h1>
        <a class="up" href="<%= Url.DocUrl(model.Section.Name, string.IsNullOrEmpty(method.Category) ? null : method.Category, null, null, Html.GetCurrentController()) %>"></a>
        <span class="hdr"><%= method.HttpMethod + " " + method.Path %></span>
        <% if (method.Authentification)
          { %>
        <span class="auth">This function requires authentication</span>
        <% } %>
    </h1>
    
    <% if (!string.IsNullOrEmpty(method.Summary))
          { %>
    <div class="header-gray">Description</div>
    <p class="dscr"><%= method.Summary %></p>
    <% } %>
    
    <div class="header-gray">Parameters</div>
    <% if (method.Params.Any(x => x.Visible))
       { %>
    <table class="table">
        <colgroup>
            <col style="width: 20%"/>
            <col/>
            <col style="width: 110px"/>
            <col style="width: 20%"/>
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Name</td>
                <td>Description</td>
                <td>Type</td>
                <td>Example</td>
            </tr>
        </thead>
        <tbody>
        <% foreach (var param in method.Params.OrderByDescending(x => x.Method).Where(x => x.Visible))
           {
               var paramModel = ClassNamePluralizer.ToHumanName(param.Type); %>
            <tr class="tablerow">
                <td>
                    <%= param.Name %>
                    <div class="infotext">sent in <%= param.Method %></div>
                </td>
                <td>
                    <%= param.Description %>
                    <% if (ClassNamePluralizer.IsOptional(param.Type) || param.IsOptional) { %>
                    <div class="infotext">optional</div><% } %>
                </td>
                <td>
                    <%= paramModel.Description %>
                    <% if (ClassNamePluralizer.IsCollection(param.Type) || paramModel.IsCollection) { %>
                    <div class="infotext">collection</div><% } %>
                </td>
                <td>
                    <% if (!string.IsNullOrEmpty(paramModel.Example)) { %><%= paramModel.Example %><% } %>
                    <% if (!string.IsNullOrEmpty(paramModel.Note)) { %>
                    <div class="infotext"><%= paramModel.Note %></div><% } %>
                </td>
            </tr>
        <% } %>
        </tbody>
    </table>
    <% } else { %>
    <p>This method doesn't have any parameters</p>
    <%} %>

    <%if (!string.IsNullOrEmpty(method.Remarks))
      { %>
    <div class="header-gray">Remark</div>
    <p><%= method.Remarks %></p>
    <% } %>
    
    <%if (!string.IsNullOrEmpty(method.Notes))
      { %>
    <div class="header-gray">Notes</div>
    <p><%= method.Notes %></p>
    <% } %>
    
    <%if (!string.IsNullOrEmpty(method.Example))
      { %>
    <div class="header-gray">Example</div>
    <pre><%= method.Example%></pre>
    <% } %>

    <div class="header-gray">
        Returns
        <span id="clipLink">Get link to this headline</span>
        <a id="returns"></a>
    </div>
    <p><%= method.Returns %></p>
    
    <% if (method.Response.Any())
       { %>
    <div class="header-gray">Example Response</div>
    <% foreach (var output in method.Response.First().Outputs)
       { %>
        <p><%= Html.Encode(output.Key) %></p>
        <pre><%= Html.Encode(output.Value) %></pre>
    <% }
       } %>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptPlaceholder" runat="server">
</asp:Content>
