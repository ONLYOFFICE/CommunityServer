<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ASC.Api.Web.Help.Models.SectionMethodViewModel>" %>
<h3><%=Html.DocMethodLink(Model.Section,Model.Method) %></h3>
<p class="summary"><%=Model.Method.Summary%></p>
<p class="returns"><%=Model.Method.Returns%></p>
