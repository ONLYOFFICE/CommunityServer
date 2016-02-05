<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Embedded
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "editor", null, new {@class = "up"}) %>
        <span class="hdr">Embedded</span>
    </h1>

    <div class="header-gray">Description</div>
    <p class="dscr">The embedded section is for the <b>embedded</b> document type only (see the <a href="<%= Url.Action("config") %>" class="underline">config</a> section to find out how to define the <b>embedded</b> document type). It allows to change the settings which define the behavior of the buttons in the embedded mode
    </p>

    <div class="header-gray">Parameters</div>
    <table class="table">
        <colgroup>
            <col class="table-name" />
            <col />
            <col class="table-type" />
            <col class="table-example" />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Name</td>
                <td>Description</td>
                <td>Type</td>
                <td>Example</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>embedUrl</td>
                <td>defines the URL to the document serving as a source file for the document embedded into the web page</td>
                <td>string</td>
                <td>"http://example.com/embedded?doc=exampledocument1.docx"</td>
            </tr>
            <tr class="tablerow">
                <td>fullscreenUrl</td>
                <td>defines the URL to the document which will open in full screen mode</td>
                <td>string</td>
                <td>"http://example.com/embedded?doc=exampledocument1.docx#fullscreen"</td>
            </tr>
            <tr class="tablerow">
                <td>saveUrl</td>
                <td>defines the URL that will allow the document to be saved onto the user personal computer</td>
                <td>string</td>
                <td>"http://example.com/download?doc=exampledocument1.docx"</td>
            </tr>
            <tr class="tablerow">
                <td>shareUrl</td>
                <td>defines the URL that will allow other users to share this document</td>
                <td>string</td>
                <td>"http://example.com/view?doc=exampledocument1.docx"</td>
            </tr>
            <tr class="tablerow">
                <td>toolbarDocked</td>
                <td>defines the place for the embedded viewer toolbar, can be either <b>top</b> or <b>bottom</b></td>
                <td>string</td>
                <td>"top"</td>
            </tr>
            <tr class="tablerow">
                <td colspan="4">
                    <img src="/Content/img/Editor/embedded.png" alt="" />
                </td>
            </tr>
        </tbody>
    </table>

    <div class="header-gray">Example</div>
    <pre>
var docEditor = new DocsAPI.DocEditor('placeholder', {
    ...
    editorConfig: {
        ...
        embedded: {
            embedUrl: 'http://example.com/embedded?doc=exampledocument1.docx',
            fullscreenUrl: 'http://example.com/embedded?doc=exampledocument1.docx#fullscreen',
            saveUrl: 'http://example.com/download?doc=exampledocument1.docx',
            shareUrl: 'http://example.com/view?doc=exampledocument1.docx',
            toolbarDocked: 'top',
        },
    },
});
</pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
