<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Confluence ONLYOFFICE™ integration plugin
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">Confluence ONLYOFFICE™ integration plugin</span>
    </h1>

    <p class="dscr">This <a href="https://github.com/ONLYOFFICE/onlyoffice-confluence" class="underline" target="_blank">plugin</a> enables users to edit office documents from <a href="https://www.atlassian.com/software/confluence/" class="underline" target="_blank">Confluence</a> using ONLYOFFICE™ Document Server. Currently the following document formats can be opened and edited with this plugin: DOCX, XLSX, PPTX.</p>
    <p>The plugin will create a new <b>Edit in ONLYOFFICE</b> action within the document library for Office documents. This allows multiple users to collaborate in real time and to save back those changes to Confluence.</p>


    <h2>Installing ONLYOFFICE™ Document Server</h2>
    <p>You will need an instance of ONLYOFFICE™ Document Server that is resolvable and connectable both from Confluence and any end clients (version 3.0 and later are supported for use with the plugin). If that is not the case, use the official ONLYOFFICE™ Document Server documentation page: <a href="http://helpcenter.onlyoffice.com/server/linux/document/linux-installation.aspx" class="underline">Document Server for Linux</a>. ONLYOFFICE™ Document Server must also be able to POST to Confluence directly.</p>
    <p>The easiest way to start an instance of ONLYOFFICE™ Document Server is to use <a href="https://github.com/ONLYOFFICE/Docker-DocumentServer" class="underline" target="_blank">Docker</a>.</p>


    <h2>Configuring Confluence CONLYOFFICE™ integration plugin</h2>
    <ol>
        <li>Change the <b>files.docservice.url.domain</b> properties in <em>src/main/resources/onlyoffice-config.properties</em> to the name of the server with the ONLYOFFICE™ Document Server installed:
            <span class="commandline">files.docservice.url.domain=http://documentserver/</span>
        </li>
    </ol>


    <h2>Installing Confluence ONLYOFFICE™ integration plugin</h2>
    <p>You will need:</p>
    <ul>
        <li>1.8.X of the Oracle Java SE Development Kit 8,</li>
        <li>Atlassian Plugin SDK (<a href="https://developer.atlassian.com/docs/getting-started/set-up-the-atlassian-plugin-sdk-and-build-a-project" class="underline" target="_blank">official instructions</a>),</li>
        <li>Compile package
            <span class="commandline">atlas-package</span>
        </li>
        <li>Upload <b>target/onlyoffice-confluence-plugin-*.jar</b> to Confluence on page <em>Manage add-ons</em>.</li>
    </ul>


    <h2>How it works</h2>
    <p>User navigates to a Confluence attachments and selects the <b>Edit in ONLYOFFICE</b> action.</p>
    <p>Confluence makes a request to OnlyOfficeEditorServlet (URL of the form: <em>/plugins/servlet/onlyoffice/doceditor?attachmentId=$attachment.id</em>).</p>
    <p>Confluence sends document to ONLYOFFICE™ Document storage service and receive a temporary link.</p>
    <p>Confluence prepares a JSON object with the following properties:</p>
    <ul>
        <li><b>fileUrl</b> - the temporary link that ONLYOFFICE™ Document Server uses to download the document;</li>
        <li><b>callbackUrl</b> - the URL that ONLYOFFICE™ Document Server informs about status of the document editing;</li>
        <li><b>docserviceApiUrl</b> - the URL that the client needs to reply to ONLYOFFICE™ Document Server (provided by the files.docservice.url.api property);</li>
        <li><b>key</b> - the UUID to instruct ONLYOFFICE™ Document Server whether to download the document again or not;</li>
        <li><b>fileName</b> - the document Title (name).</li>
    </ul>
    <p>Confluence takes this object and constructs a page from a freemarker template, filling in all of those values so that the client browser can load up the editor.</p>
    <p>The client browser makes a request for the javascript library from ONLYOFFICE™ Document Server and sends ONLYOFFICE™ Document Server the docEditor configuration with the above properties.</p>
    <p>Then ONLYOFFICE™ Document Server downloads the document from Confluence and the user begins editing.</p>
    <p>When all users and client browsers are done with editing, they close the editing window.</p>
    <p>After 10 seconds of inactivity, ONLYOFFICE™ Document Server sends a POST to the <em>callback</em> URL letting Confluence know that the clients have finished editing the document and closed it.</p>
    <p>Confluence downloads the new version of the document, replacing the old one.</p>

    <br />
    <p>Download the Confluence ONLYOFFICE™ integration plugin <a href="https://github.com/ONLYOFFICE/onlyoffice-confluence" class="underline" target="_blank">here</a>.</p>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
