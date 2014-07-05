<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ASC.Web.Mobile.Models.ManifestModel>" %>CACHE MANIFEST
# bit ts
# <%=Model.UniqueTs %>

# static file
CACHE:
<%foreach (var file in Model.Files){%>
  <%=file %>
<%} %>

NETWORK:
*


