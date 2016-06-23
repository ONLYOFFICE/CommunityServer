<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<h1>
    <%= Html.ActionLink(" ", "config/document", null, new {@class = "up"}) %>
    <span class="hdr">Document Info</span>
</h1>

<div class="header-gray">Description</div>
<p class="dscr">The document info section allows to change additional parameters for the document (document author, folder where the document is stored, creation date, sharing settings)</p>

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
            <td id="author">author</td>
            <td>defines the name of the document author/creator</td>
            <td>string</td>
            <td>"John Smith"</td>
        </tr>
        <tr class="tablerow">
            <td id="created">created</td>
            <td>defines the document creation date</td>
            <td>string</td>
            <td>"2010-07-07 3:46 PM"</td>
        </tr>
        <tr class="tablerow">
            <td id="folder">folder</td>
            <td>defines the folder where the document is stored (can be empty in case the document is stored in the root folder)</td>
            <td>string</td>
            <td>"Example Files"</td>
        </tr>
        <tr class="tablerow">
            <td id="sharingSettings">sharingSettings</td>
            <td>defines the settings which will allow to share the document with other users:
                <ul>
                    <li>
                        <b>permissions</b> - the access rights for the user with the name above. Can be <b>Full Access</b>, <b>Read Only</b> or <b>Deny Access</b>
                        <br />
                        <b>type</b>: string
                        <br />
                        <b>example</b>: "Full Access"
                    </li>
                    <li>
                        <b>user</b> - the name of the user the document will be shared with
                        <br />
                        <b>type</b>: string
                        <br />
                        <b>example</b>: "John Smith"
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
                <img src="/Content/img/Editor/info.png" alt="" />
            </td>
        </tr>
    </tbody>
</table>

<div class="header-gray">Example</div>
<pre>
var docEditor = new DocsAPI.DocEditor("placeholder", {
    ...
    "document": {
        ...
        "info": {
            "author": "John Smith",
            "created": "2010-07-07 3:46 PM",
            "folder": "Example Files",
            "sharingSettings": [
                {
                    "permissions": "Full Access",
                    "user": "John Smith",
                },
                {
                    "permissions": "Read Only",
                    "user": "Kate Cage",
                },
                ...
            ],
        },
    },
});
</pre>
