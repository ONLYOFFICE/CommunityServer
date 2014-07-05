<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ASC.Api.Web.Help.DocumentGenerator.MsDocEntryPoint>" %>
<h2><%=Html.DocSectionLink(Model) %></h2>
<p><%=Model.Summary%></p>
