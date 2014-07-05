<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.IEnumerable<ASC.Api.Web.Help.DocumentGenerator.MsDocEntryPoint>>" %>
<ul class="treeview" id="method_nav" style="display: none">
    <% foreach (var entryPoint in Model)
       {%>
    <li><a href="<%=Url.DocUrl(entryPoint) %>">
        <%=entryPoint.Name%></a>
        <ul class="treeview">
            <% foreach (var methodGroup in entryPoint.Methods.GroupBy(x => x.Category))
               {
                   if (!string.IsNullOrEmpty(methodGroup.Key))
                   {%>
            <li><span class="category">
                <%=methodGroup.Key%></span>
                <ul class="treeview">
                    <%}
               foreach (var method in methodGroup.OrderBy(x => x.HttpMethod, new HttpMethodOrderComarer()).ThenBy(x=>x.Path.Length))
               { %>
                    <li><a href="<%=Url.DocUrl(entryPoint, method)%>">
                        <%=string.IsNullOrEmpty(method.ShortName)?(string.IsNullOrEmpty(method.Summary)?method.FunctionName:method.Summary):method.ShortName%></a></li>
                    <% }
               if (!string.IsNullOrEmpty(methodGroup.Key))
               {%>
                </ul>
            </li>
            <%}
}%>
        </ul>
    </li>
    <%} %>
</ul>
