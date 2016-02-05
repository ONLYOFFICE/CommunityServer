<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Editor Config
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "config", null, new {@class = "up"}) %>
        <span class="hdr">Editor Config</span>
    </h1>

    <div class="header-gray">Description</div>
    <p class="dscr">The editorConfig section allows to change the parameters pertaining to the editor interface: opening mode (viewer or editor), interface language, additional buttons, etc.)</p>

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
                <td>callbackUrl<span class="required">*</span></td>
                <td>specifies URL to the <b>document storage service</b> (which <a href="<%= Url.Action("callback") %>" class="underline">must be implemented</a> by the software integrators who use ONLYOFFICE™ Document Server on their own server)</td>
                <td>string</td>
                <td>"http://example.com/url-to-callback.ashx"</td>
            </tr>
            <tr class="tablerow">
                <td>createUrl</td>
                <td>defines the URL of the document where it will be created and available after creation. If not specified, there will be no creation button
                </td>
                <td>string</td>
                <td>"http://example.com/url-to-create-document/"</td>
            </tr>
            <tr class="tablerow">
                <td colspan="4">
                    <img src="/Content/img/Editor/create.png" alt=""/>
                </td>
            </tr>
            <tr class="tablerow">
                <td>customerId<span class="required">**</span></td>
                <td>defines your <a href="<%= Url.Action("license") %>" class="underline">company ID</a> which is issued upon registration and unique for each company</td>
                <td>string</td>
                <td>"66D4DD68-4366-403C-8906-4BA36D44913B"</td>
            </tr>
            <tr class="tablerow">
                <td>lang</td>
                <td>defines the editor interface language (if some other languages other than English are present). Is set using the two letter (<b>de</b>, <b>ru</b>, <b>it</b>, etc.) or four letter (<b>en-US</b>, <b>fr-FR</b>, etc.) language codes
                </td>
                <td>string</td>
                <td>"en-US"</td>
            </tr>
            <tr class="tablerow">
                <td>licenseUrl<span class="required">**</span></td>
                <td>defines the URL to the license key file with the .lic extension where the license information about your server edition is stored</td>
                <td>string</td>
                <td>"http://example.com/license.lic"</td>
            </tr>
            <tr class="tablerow">
                <td>mode</td>
                <td>defines the editor opening mode. Can be either <b>view</b> to open the document for viewing, or <b>edit</b> to open the document in the editing mode allowing to apply changes to the document data
                </td>
                <td>string</td>
                <td>"edit"</td>
            </tr>
            <tr>
                <td>recent</td>
                <td>defines the presence or absence of the documents in the <b>Open Recent...</b> menu option where the following document parameters can be set:
                    <ul>
                        <li>
                            <b>folder</b> - the folder where the document is stored (can be empty in case the document is stored in the root folder)
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "Example Files"
                        </li>
                        <li>
                            <b>title</b> - the document title that will be displayed in the <b>Open Recent...</b> menu option
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "exampledocument1.docx"
                        </li>
                        <li>
                            <b>url</b> - the URL to the document where it is stored
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "http://example.com/exampledocument1.docx"
                        </li>
                    </ul>
                </td>
                <td>Collection of object
                    <div class="infotext">Collection</div>
                </td>
                <td></td>
            </tr>
            <tr class="tablerow">
                <td colspan="4">
                    <img src="/Content/img/Editor/recent.png" alt=""/>
                </td>
            </tr>
            <tr class="tablerow">
                <td id="user">user<span class="required">**</span></td>
                <td>the user currently viewing or editing the document:
                    <ul>
                        <li>
                            <b>id</b> - the identification of the user
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "78e1e841-8314-48465-8fc0-e7d6451b6475"
                        </li>
                        <li>
                            <b>firstname</b> - the first name of the user
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "John"
                        </li>
                        <li>
                            <b>lastname</b> - the last name of the user
                            <br />
                            <b>type</b>: string
                            <br />
                            <b>example</b>: "Smith"
                        </li>
                    </ul>
                </td>
                <td>object</td>
                <td></td>
            </tr>
        </tbody>
    </table>

    <span class="required-descr"><span class="required">*</span><em> - required field</em></span>
    <span class="required-descr"><span class="required">**</span><em> - required field for ONLYOFFICE™ Document Server <a href="<%= Url.Action("license") %>" class="underline">Enterprise Edition</a></em></span>

    <div class="header-gray">Example</div>
    <pre>
var docEditor = new DocsAPI.DocEditor('placeholder', {
    ...
    editorConfig: {
        callbackUrl: 'http://example.com/url-to-callback.ashx',
        createUrl: 'http://example.com/url-to-create-document/',
        customerId: "66D4DD68-4366-403C-8906-4BA36D44913B",
        lang: 'en-US',
        licenseUrl: 'http://example.com/license.lic',
        mode: 'edit',
        recent: [
            {
                folder: 'Example Files',
                title: 'exampledocument1.docx',
                url: 'http://example.com/exampledocument1.docx',
            },
            {
                folder: 'Example Files',
                title: 'exampledocument2.docx',
                url: 'http://example.com/exampledocument2.docx',
            },
            ...
        ],
        user: {
            id: '78e1e841-8314-48465-8fc0-e7d6451b6475',
            firstname: 'John',
            lastname: 'Smith',
        },
    },
});
</pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
