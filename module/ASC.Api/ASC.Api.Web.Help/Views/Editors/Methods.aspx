<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Methods
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">Methods</span>
    </h1>

    <p>After initialising <b>document editor</b> you will get the object that can be used for calling methods.</p>
    <pre>
var docEditor = new DocsAPI.DocEditor("placeholder", config);
</pre>

    <p id="denyEditingRights">Deny editing. This method can be called when you want to make the document editing unavailable.</p>
    <pre>
docEditor.denyEditingRights(message);
</pre>
    <table class="table">
        <colgroup>
            <col style="width: 100px;" />
            <col style="width: 100px;" />
            <col style="width: 150px;" />
            <col />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Parameter</td>
                <td>Type</td>
                <td>Presence</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>message</td>
                <td>string</td>
                <td>not required</td>
                <td>Text messages for dialog</td>
            </tr>
        </tbody>
    </table>

    <br />

    <p id="downloadAs">Download the edited file. This method can be called only when the existence of the <a href="<%= Url.Action("config/events") %>#onDownloadAs" class="underline">onDownloadAs</a> events. <b>Document editing service</b> asynchronously creates a document and triggers the <b>onDownloadAs</b> event with a link in parameter:</p>
    <pre>
docEditor.downloadAs();
</pre>

    <br />

    <p id="showMessage">Display dialog with the message. This method can be called only after the <a href="<%= Url.Action("config/events") %>#onReady" class="underline">onReady</a> events:</p>
    <pre>
docEditor.showMessage(title, message, type);
</pre>
    <table class="table">
        <colgroup>
            <col style="width: 100px;" />
            <col style="width: 100px;" />
            <col style="width: 150px;" />
            <col />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Parameter</td>
                <td>Type</td>
                <td>Presence</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>title</td>
                <td>string</td>
                <td>required</td>
                <td>Dialog title</td>
            </tr>
            <tr class="tablerow">
                <td>message</td>
                <td>string</td>
                <td>required</td>
                <td>Message text</td>
            </tr>
            <tr class="tablerow">
                <td>type</td>
                <td>string</td>
                <td>not required</td>
                <td>Defines dialog type. Can be: <b>info</b>, <b>warning</b>, <b>error</b></td>
            </tr>
        </tbody>
    </table>

    <br />

    <p id="refreshHistory">Show the document version history or the error message explaining why the version history can not be displayed. This method must be called after the <a href="<%= Url.Action("config/events") %>#onRequestHistory" class="underline">onRequestHistory</a> events:</p>
    <pre>
docEditor.refreshHistory({
    "currentVersion": 1,
    "history": [
        {
            "key": "Khirz6zTPdfd7",
            "version": 1,
            "created": "2010-07-07 3:46 PM",
            "user": {
                "id": "78e1e841",
                "name": "John Smith",
            },
            "changes": changeshistory,
        },
        ...
    ],
    "error": null,
});

</pre>
    <table class="table">
        <colgroup>
            <col style="width: 100px;" />
            <col style="width: 100px;" />
            <col style="width: 150px;" />
            <col />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Parameter</td>
                <td>Type</td>
                <td>Presence</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>currentVersion</td>
                <td>int</td>
                <td>required</td>
                <td>defines the current document version number</td>
            </tr>
            <tr class="tablerow">
                <td>history</td>
                <td>array</td>
                <td>required</td>
                <td>defines the array with the document versions</td>
            </tr>
            <tr class="tablerow">
                <td>history.key</td>
                <td>string</td>
                <td>required</td>
                <td>defines the unique document identifier used for document recognition by the service</td>
            </tr>
            <tr class="tablerow">
                <td>history.version</td>
                <td>int</td>
                <td>required</td>
                <td>defines the document version number</td>
            </tr>
            <tr class="tablerow">
                <td>history.created</td>
                <td>string</td>
                <td>required</td>
                <td>defines the document version creation date</td>
            </tr>
            <tr class="tablerow">
                <td>history.user.id</td>
                <td>string</td>
                <td>not required</td>
                <td>defines the identifier of the user who is the author of the document version </td>
            </tr>
            <tr class="tablerow">
                <td>history.user.name</td>
                <td>string</td>
                <td>not required</td>
                <td>defines the name of the user who is the author of the document version</td>
            </tr>
            <tr class="tablerow">
                <td>history.changes</td>
                <td>Object</td>
                <td>not required</td>
                <td>defines the <em>changeshistory</em> from <a href="<%= Url.Action("callback") %>#changeshistory" class="underline">the JSON object</a> returned after saving the document</td>
            </tr>
            <tr class="tablerow">
                <td>error</td>
                <td>string</td>
                <td>not required</td>
                <td>defines the error message text</td>
            </tr>
        </tbody>
    </table>

    <br />

    <p id="setHistoryData">Send the link to the document for viewing the version history or the error message explaining why the document version can not be displayed. This method must be called after the  <a href="<%= Url.Action("config/events") %>#onRequestHistoryData" class="underline">onRequestHistoryData</a> events.</p>
    <pre>
docEditor.setHistoryData({
    "version": 1,
    "url": "http://example.com/url-to-example-document.docx",
    "error": null,
});
</pre>
    <p>If after editing and saving the document the link <em>changesurl</em> to the file with changes data is returned, download the file by this link and send the file url in <em>changesUrl</em> parameter. The url address of the document previous version must be sent in <em>url</em> parameter.</p>
    <pre>
docEditor.setHistoryData({
    "version": 1,
    "url": "http://example.com/url-to-the-previous-version-of-the-document.docx",
    "changesUrl": "http://example.com/url-to-changes.zip",
    "error": null,
});
</pre>
    <table class="table">
        <colgroup>
            <col style="width: 100px;" />
            <col style="width: 100px;" />
            <col style="width: 150px;" />
            <col />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Parameter</td>
                <td>Type</td>
                <td>Presence</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>version</td>
                <td>int</td>
                <td>required</td>
                <td>defines the document version number</td>
            </tr>
            <tr class="tablerow">
                <td>url</td>
                <td>string</td>
                <td>required</td>
                <td>defines the url address of the current version of the document if <em>changesUrl</em> address is absent or the url address of the previous version of the document if <em>changesUrl</em> address was returned after saving the document. Can be downloaded by the <em>url</em> link from <a href="<%= Url.Action("callback") %>#url" class="underline">the JSON object</a> returned after saving the document</td>
            </tr>
            <tr class="tablerow">
                <td>changesUrl</td>
                <td>string</td>
                <td>not required</td>
                <td>defines the url address of the file with the document changes data, which can be downloaded by the <em>changesurl</em> link from <a href="<%= Url.Action("callback") %>#changesurl" class="underline">the JSON object</a>
                returned after saving the document
            </tr>
            <tr class="tablerow">
                <td>error</td>
                <td>string</td>
                <td>not required</td>
                <td>defines the error message text</td>
            </tr>
        </tbody>
    </table>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
