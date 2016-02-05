<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<string>"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    How It Works
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">How It Works</span>
    </h1>

    <p class="dscr">
        The work with document files in ONLYOFFICE™ Document Server is quite a simple process but it requires some understanding of what is going on when you click your document link to <a href="<%= Url.Action("open") %>" class="underline">open</a> it in the browser, or <a href="<%= Url.Action("save") %>" class="underline">save</a> the document or <a href="<%= Url.Action("conversion") %>" class="underline">upload</a> it from your computer to the document server.
    </p>
    <p>The user-document interaction is done both at the client side and at the server side.</p>
    <p>Below the main notions used throughout the current documentation are explained.</p>
    <p class="list-header">The client side includes:</p>
    <ul>
        <li>
            <b>Document manager</b> - the list of the documents displayed in the user browser where the user can select the necessary document and perform some actions with it (depending on the provided rights, the user can open the document to view it or edit, share the document with other users).
        </li>
        <li>
            <b>Document editor</b> - the document viewing and editing interface with all the most known document editing features available, used as a medium between the user and the <b>document editing service</b>.
        </li>
    </ul>

    <p class="list-header">The server side includes:</p>
    <ul>
        <li>
            <b>Document storage service</b> - the server service which stores all the documents available to the users with the appropriate access rights. It provides the document IDs and links to these documents to the <b>document manager</b> which the user sees in the browser.
        </li>
        <li>
            <b>Document editing service</b> - the server service which allows to perform the document viewing and editing (in case the user has the appropriate rights to do that). The <b>document editor</b> interface is used to access all the <b>document editing service</b> features.
        </li>
        <li>
            <b>Document conversion service</b> - the server service which allows to convert the document file into the appropriate Office Open XML format (<em>docx</em> for text documents, <em>xlsx</em> for spreadsheets and <em>pptx</em> for presentations) for their editing or downloading.
        </li>
    </ul>

    <p class="list-header">
        Please note, that ONLYOFFICE™ Document Server includes the <b>document editor</b>, <b>document editing service</b> and <b>document conversion service</b>.
        The <b>document manager</b> and <b>document storage service</b> are either included to Community Server or must be implemented by the software integrators who use ONLYOFFICE™ Document Server on their own server.
    </p>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
