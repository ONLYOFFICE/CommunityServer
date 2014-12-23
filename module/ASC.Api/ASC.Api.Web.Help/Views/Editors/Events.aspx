<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Events
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <span class="hdr">Events</span>
    </h1>

	<ul>
		<li>
		    <b>onReady</b> - the function called when the document is loaded into the document editor;
		</li>
		<li>
		    <b>onDocumentStateChange</b> - the function called when the document is modified;
		</li>
		<li>
		    <b>onRequestEditRights</b> - the function called when the user is trying to switch the document from the viewing into the editing mode;
		</li>
		<li>
		    <b>onBack</b> - the function called when the <b>Go to Documents</b> link is clicked.
		</li>
	</ul>
    
    <div class="note">
        <b>onRequestEditRights</b> parameter is obligatory when the <a href="<%=Url.Action("editor") %>" class="underline">editorConfig</a> <em>mode</em> parameter is set to <b>view</b> and the <em>permission</em> to <em>edit</em> the document (<a href="<%=Url.Action("docpermissions") %>" class="underline">document permissions</a>) is set to <b>true</b> so that the user to be able to switch to the editing mode.
    </div>
    
    <div class="header-gray">Example</div>
	<pre>
var docEditor = new DocsAPI.DocEditor('placeholder', {
events: {
'onReady': onDocEditorReady,
'onDocumentStateChange': onDocumentStateChange,
'onRequestEditRights': onRequestEditRights,
'onSave': onDocumentSave,
'onError': onError,
'onBack': onBack
}
});
</pre>

	<p>Two events have additional importance and should be viewed separately:</p>

	<ul>
		<li><b>onSave</b> - the function called on document save. Returns the url of the saved document that can be used for further purposes.</li>
	</ul>
    
    <div class="header-gray">Example</div>
	<pre>
var docEditor = new DocsAPI.DocEditor('placeholder', {
events: {
'onSave': onDocumentSave
}
});

function onDocumentSave(event) {
var url = event.data;
}
</pre>
    
	<ul>
		<li><b>onError</b> - the function called when an error or some other specific event occurs. Can help display error or message window styled in ONLYOFFICE™ document editor common internal style.</li>
	</ul>

    <div class="header-gray">Example</div>
	<pre>
var docEditor = new DocsAPI.DocEditor('placeholder', {
events: {
'onError': onError
}
});

function onError(event) {
var docEditorShowError = function (message, title) {
ASC.Files.Editor.docEditor.showError(title || "The Error Title", "The error text which is to be displayed in the error window.");
};

var docEditorShowMessage = function (message, title) {
ASC.Files.Editor.docEditor.showMessage(title || "The Message Title", "The message text which is to be displayed in the message window.");
};
}
</pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>