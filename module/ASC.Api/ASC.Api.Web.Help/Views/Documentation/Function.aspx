<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ASC.Api.Web.Help.Models.SectionMethodViewModel>" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Function -
    <%=!string.IsNullOrEmpty(Model.Method.ShortName)?Model.Method.ShortName:Model.Method.Summary%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%=Model.Method.HttpMethod + " " + Highliter.HighliteParams(Model.Method.Path)%>
        <%if (Model.Method.Authentification)
          {%>
        <a class="auth tip" title="This function requires authentication" href="<%=Url.Action("Authentication", "Help")%>">authentication</a>
        <% } %>
    </h1>
    <p class="summary">
        <%=Model.Method.Summary%>
    </p>
    <%if (Model.Method.Params.Any(x => x.Visible))
      {%>
    <h2>
        Parameters</h2>
    <div>
        <% foreach (var param in Model.Method.Params.OrderByDescending(x => x.Method).Where(x => x.Visible))
           {%>
        <div class="parameter" id="api-param-<%=param.Name%>">
            <span class="param"><strong>
                <%=param.Name%></strong> <span>Sent in
                    <%=param.Method%></span></span>
            <p>
                <%if (ClassNamePluralizer.IsOptional(param.Type) || param.IsOptional)
                  {%>
                <span class="label notice">Optional</span>
                <% } %>
                <%=param.Description%></p>
            <%Html.RenderPartial("ParamTypeControl", ClassNamePluralizer.ToHumanName(param.Type)); %>
        </div>
        <% } %>
    </div>
    <% }
      else
      {%>
    <p class="alert-message info">
        This method doesn't have any parameters</p>
    <%} %>
    <%if (!string.IsNullOrEmpty(Model.Method.Remarks))
      { %>
    <h3>
        Remark</h3>
    <div class="remarks alert-message block-message warning">
        <%=Model.Method.Remarks %></div>
    <% } %>
    <%if (!string.IsNullOrEmpty(Model.Method.Example))
      { %>
    <h3>
        Example</h3>
    <pre class="example"><%=Model.Method.Example%></pre>
    <% } %>
    <p>
        <h3>
            Returns:</h3>
        <%=Model.Method.Returns %></p>
    <% if (Model.Method.Response.Any())
       {%>
    <h2>
        Example Response</h2>
        <ul class="tabs">
        <%foreach (var output in Model.Method.Response.First().Outputs) {%>
        <li>
            <a href="#resp<%=output.Key.GetHashCode()%>"><%=Html.Encode(output.Key)%></a>
         </li>
        <%}%>
        </ul>
        <div class="tab-content">
        <%foreach (var output in Model.Method.Response.First().Outputs) {%>
        <div id="resp<%=output.Key.GetHashCode()%>">
            <pre class="prettyprint"><code data-mediatype="<%=output.Key%>"><%=Html.Encode(output.Value)%></code></pre>
         </div>
        <%}%>
        </div>
    <% }
       else
       {%>
    <p class="alert-message warning">
        This method doesn't have an example. Come back later</p>
    <%} %>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptPlaceholder" runat="server">
</asp:Content>
