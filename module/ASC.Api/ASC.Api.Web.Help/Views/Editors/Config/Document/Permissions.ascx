<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

    <h1>
        <%= Html.ActionLink(" ", "config/document", null, new {@class = "up"}) %>
        <span class="hdr">Document Permissions</span>
    </h1>

    <div class="header-gray">Description</div>
    <p class="dscr">The document permission section allows to change the permission for the document to be edited and downloaded or not</p>

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
                <td id="download">download</td>
                <td>defines if the document can be downloaded or only viewed or edited online. In case the downloading permission is set to <b>"true"</b> the <b>File</b> menu will contain the <b>Download as...</b> menu option</td>
                <td>boolean</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td id="edit">edit</td>
                <td>defines if the document can be edited or only viewed. In case the editing permission is set to <b>"true"</b> the <b>File</b> menu will contain the <b>Edit Document</b> menu option; please note that if the editing permission is set to <b>"false"</b> the document will be opened in viewer and you will <b>not</b> be able to switch it to the editor even if the <a href="<%= Url.Action("config/editor") %>#mode" class="underline">mode</a> parameter is set to <b>edit</b></td>
                <td>boolean</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td id="print">print</td>
                <td>defines if the document can be printed or not. In case the printing permission is set to <b>"true"</b> the <b>File</b> menu will contain the <b>Print</b> menu option</td>
                <td>boolean</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td colspan="4">
                    <img src="/Content/img/Editor/permissions.png" alt=""/>
                </td>
            </tr>
            <tr class="tablerow">
                <td id="review">review</td>
                <td>defines if the document can be reviewed or not. In case the reviewing permission is set to <b>"true"</b> the document <b>status bar</b> will contain the <b>Review</b> menu option; the document review will only be available if the <a href="<%= Url.Action("config/editor") %>#mode" class="underline">mode</a> parameter is set to <b>edit</b></td>
                <td>boolean</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td colspan="4">
                    <img src="/Content/img/Editor/review.png" alt=""/>
                </td>
            </tr>
        </tbody>
    </table>
    <div class="note">
        In case 'edit' is set to <b>"true"</b> and 'review' is also set to <b>"true"</b>, the user will be able to edit the document, accept/reject the changes made and switch to the review mode him-/herself.
        In case 'edit' is set to <b>"true"</b> and 'review' is set to <b>"false"</b>, the user will be able to edit only.
        In case 'edit' is set to <b>"false"</b> and 'review' is set to <b>"true"</b>, the document will be available in review mode only.
    </div>

    <div class="header-gray">Example</div>
    <pre>
var docEditor = new DocsAPI.DocEditor("placeholder", {
    ...
    "document": {
        ...
        "permissions": {
            "download": true,
            "edit": true,
            "print": true,
            "review": true,
        },
    },
});
</pre>
