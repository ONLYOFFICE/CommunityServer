<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ASC.Web.Mobile.Models.ResourcesModel>" %>
window.ASC=window.ASC||{};
window.ASC.Resources=window.ASC.Resources||{};
<%foreach (var item in Model.Items) {%>
  <%="window.ASC.Resources['" + item.Key + "']" + "=" + "'" + item.Value + "';"%>
<%}%>