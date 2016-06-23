<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Basic concepts
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">Basic concepts</span>
    </h1>

    <p class="dscr">ONLYOFFICE™ Document Server API is used to let the developers integrate the ONLYOFFICE™ Document/Spreadsheet/Presentation Editors into their own web sites and setup and manage the editors.</p>
    <p>The API JavaScript file can normally be found in the following editors folder:</p>
    <p><b>http://documentserver/OfficeWeb/apps/api/documents/api.js</b></p>
    <p>Where the <b>documentserver</b> is the name of the server with the ONLYOFFICE™ Document Server installed.</p>
    <p>The target HTML file where the editors are to be embedded need to have a placeholder <em>div</em> tag, where all the information about the editor parameters will be passed:</p>

    <pre>
&lt;div id=&quot;placeholder&quot;&gt;&lt;/div&gt;
&lt;script type=&quot;text/javascript&quot; src=&quot;http://documentserver/OfficeWeb/apps/api/documents/api.js&quot;&gt;&lt;/script&gt;
</pre>

    <p>The page code containing the changeable parameters looks the following way:</p>

    <pre>
var docEditor = new DocsAPI.DocEditor("placeholder", config);
</pre>

    <p>Where <em>config</em> is an object:</p>

    <pre>
config = {
    "documentType": "text",
    "document": {
        "fileType": "docx",
        "key": "Khirz6zTPdfd7",
        "title": "Example Document Title.docx",
        "url": "http://example.com/url-to-example-document.docx",
    },
    "editorConfig": {
        "callbackUrl": "http://example.com/url-to-callback.ashx",
    },
};
</pre>

    <p>From now the objetct <i>docEditor</i> can be used for calling <a class="underline" href="<%= Url.Action("methods") %>">Methods</a> <b>document editor</b>.</p>

    <p>The example above includes all the parameters necessary for Document Server correct startup. There are additional non-obligatory parameters though which can be changed to achieve different goals with your document (change access rights for the document, display different information about the document, etc.) See the <a href="<%= Url.Action("advanced") %>" class="underline">Advanced parameters</a> section to find out what these parameters are and how you can change them.</p>

    <h2>Support</h2>
    <p>
        You can ask our developers at <a href="http://dev.onlyoffice.org/viewforum.php?f=9" target="_blank" class="underline">dev.onlyoffice.org</a> (registration required).
    </p>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
