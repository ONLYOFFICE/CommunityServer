<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<h1>
    <span class="hdr">Config</span>
</h1>

<div class="header-gray">Description</div>
<p class="dscr">The config base section allows to change the platform type used, document display size (width and height) and type of the document opened</p>

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
            <td id="documentType">documentType<span class="required">*</span></td>
            <td>defines the document type to be opened:
                <em>open a text document (.docx, .doc, .odt, .txt, .rtf, .html, .htm, .mht, .epub, .pdf, .djvu, .xps) for viewing or editing</em> - <b>text</b>,
                <em>open a spreadsheet (.xlsx, .xls, .ods, .csv) for viewing or editing</em> - <b>spreadsheet</b>,
                <em>open a presentation (.pptx, .ppt, .odp, .ppsx, .pps) for viewing  or editing</em> - <b>presentation</b>
            </td>
            <td>string</td>
            <td>"spreadsheet"</td>
        </tr>
        <tr class="tablerow">
            <td id="height">height</td>
            <td>defines the document height (<b>100%</b> by default) in the browser window</td>
            <td>string</td>
            <td>"100%"</td>
        </tr>
        <tr class="tablerow">
            <td id="type">type</td>
            <td>defines the platform type used to access the document. Can be:
                <em>optimized to access the document from a desktop or laptop computer</em> - <b>desktop</b>,
                <em>optimized to access the document from a tablet or a smartphone</em> - <b>mobile</b>,
                <em>specifically formed to be easily embedded into a web page</em> - <b>embedded</b>
            </td>
            <td>string</td>
            <td>"desktop"</td>
        </tr>
        <tr class="tablerow">
            <td id="width">width</td>
            <td>defines the document width (<b>100%</b> by default) in the browser window</td>
            <td>string</td>
            <td>"100%"</td>
        </tr>
    </tbody>
</table>

<span class="required-descr"><span class="required">*</span><em> - required field</em></span>

<div class="header-gray">Example</div>
<pre>
var docEditor = new DocsAPI.DocEditor("placeholder", {
    ...
    "documentType": "text",
    "height": "100%",
    "type": "desktop",
    "width": "100%",
});
</pre>

