<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Saving File
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h1>
	    <span class="hdr">Saving File</span>
	</h1>
    <p class="dscr">The reference figure and the steps below explain the process of saving the document in Teamlab Editors.</p>
    <img alt="Opening File" src="<%= Url.Content("~/content/img/saving.jpg") %>" />
	<p>1. The user clicks the <b>Save</b> button in the <b>document editor</b>.</p>
    <p>2. The <b>document editor</b> uploads the document with the current changes to the <b>document editing service</b>.</p>
	<p>The <b>document editing service</b> stores the changes and returns the link to the <b>document editor</b>.</p>
	<p>The <b>document editor</b> generates the <a class="underline" href="<%=Url.Action("basic") %>">JavaScript API</a> <em>onSave</em> event and returns the link for the saved document as an argument to the <b>document manager</b>.</p>
	<p>The <b>document manager</b> sends this link to the <b>document storage service</b>.</p>
	<p>The <b>document storage service</b> downloads the document file with all the saved changes from the <b>document editing service</b> and stores it.</p>	
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>