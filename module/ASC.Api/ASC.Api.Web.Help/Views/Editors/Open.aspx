<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Opening File
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h1>
	    <span class="hdr">Opening File</span>
	</h1>
    <p class="dscr">The reference figure and the steps below explain the process of opening the document in ONLYOFFICE™ Document Server.</p>
    <img alt="Opening File" src="<%= Url.Content("~/content/img/opening.jpg") %>" />
	<p>1. The user uses the <b>document manager</b> (found in his/her browser) to open the document for viewing or editing.</p>
    <div class="note">The browser <b>document manager</b> receives the list of all documents available to the user from the <b>document storage service</b>.</div>
	<p>2. The document identifier and the link to it at the <b>document storage service</b> are sent using the <a class="underline" href="<%=Url.Action("basic") %>">JavaScript API</a> to the <b>document editor</b>.</p>
	<p>3. The <b>document editor</b> forms a request to the <b>document editing service</b> for document opening. The <b>document editor</b> uses the document identifier and its link received from the <b>document manager</b> (at step 2).</p>
	<p>4. The <b>document editing service</b> downloads the document file from the <b>document storage service</b> using the ID and link provided. At this step the <a href="<%= Url.Action("conversion") %>" class="underline">conversion</a> of the file into Office Open XML format is also performed for the <b>document editor</b> better performance and formats compatibility.</p>
	<p>5. When ready the <b>document editing service</b> transfers the document file to the browser-based <b>document editor</b>.</p>
	<p>6. The <b>document editor</b> displays the document file and/or (in case the appropriate rights are provided) allows its editing.</p>
	<p>After the editing is finished, the <a href="<%=Url.Action("save") %>" class="underline">document saving</a> process takes place.</p>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>