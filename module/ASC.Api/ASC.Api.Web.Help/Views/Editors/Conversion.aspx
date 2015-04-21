<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Converting and Downloading File
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h1>
	    <span class="hdr">Converting and Downloading File</span>
	</h1>
	<p class="dscr">Document conversion service is a part of ONLYOFFICE™ Document Server. It lets the user convert files from one format into another to open them later in <b>document editors</b> or for their export.</p>
	<p>There are two main situations when document conversion is necessary:</p>
	<h2>Interim conversion needed for document editing</h2>
	<p>For the <b>document editors</b> correct work with the document files it is recommended to convert them  prior to editing into Office Open XML formats:</p>
	<ul>
		<li><em>docx</em> for text documents,</li>
		<li><em>xlsx</em> for spreadsheets,</li>
		<li><em>pptx</em> for presentations.</li>
	</ul>
	<p>The reference figure and the steps below explain the process of document conversion.</p>
	<img alt="Interim conversion needed for document editing" src="<%= Url.Content("~/content/img/conversion.jpg") %>" />
	<ol>
		<li>The users selects a file on the computer hard disk drive that is to be uploaded to the <b>document manager</b>.</li>
		<li>The <b>document manager</b> uploads the selected file to the <b>document storage service</b>.</li>
		<li>The <b>document storage service</b> sends the uploaded file to ONLYOFFICE™ Document Server <b>document conversion service</b> for conversion into the Office Open XML format using the <a class="underline" href="<%=Url.Action("conversionapi") %>">conversion API</a>.</li>
		<li>The <b>document conversion service</b> converts the selected file to the appropriate Office Open XML format.</li>
		<li>The <b>document storage service</b> downloads the converted document file.</li>
	</ol>
	<h2>Document export</h2>
	<p>When the user needs to download the file in some format different from the Office Open XML format, ONLYOFFICE™ Document Server converts the document file saved at the <b>document storage service</b> into the appropriate format prior to its export.</p>
	<p>The reference figure and the steps below explain the process of document export.</p>
	<img alt="Document export" src="<%= Url.Content("~/content/img/export.jpg") %>" />
	<ol>
		<li>The user selects the file in the <b>document manager</b> and the format the file must be downloaded in.</li>
		<li>The <b>document manager</b> transforms this user action into a request to the <b>document storage service</b>.</li>
		<li>The <b>document storage service</b> sends the uploaded file to ONLYOFFICE™ Document Server <b>document conversion service</b> for conversion into the appropriate format using the <a class="underline" href="<%=Url.Action("conversionapi") %>">conversion API</a>.</li>
		<li>When the conversion is finished the <b>document storage service</b> downloads the converted file.</li>
		<li>The <b>document storage service</b> notifies the <b>document manager</b> that the conversion is successfully performed.</li>
		<li>The <b>document manager</b> downloads the converted file.</li>
	</ol>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>