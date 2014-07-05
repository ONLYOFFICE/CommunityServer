<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Collections.Generic.IEnumerable<ASC.Api.Web.Help.DocumentGenerator.MsDocEntryPoint>>" ContentType="text/html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Toc
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Toc</h2>
    <% foreach (var entryPoint in Model)
       {
           Html.RenderPartial("SectionSummary",entryPoint);
       } %>
</asp:Content>
