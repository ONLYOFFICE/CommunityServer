<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Conversion API
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <span class="hdr">Conversion API</span>
    </h1>
    <p class="dscr">For the interaction with the <b>document conversion service</b> the GET requests with the parameter set are used. The requests are sent to the <span class="fakelink">http://documentserver/ConvertService.ashx</span> address where the <b>documentserver</b> is the name of the server with the ONLYOFFICE™ Document Server installed.</p>
    <h2>Request parameters and their description:</h2>
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
                <td>key</td>
                <td>string</td>
                <td>required</td>
                <td>Document identifier used to unambiguously identify the document file.</td>
            </tr>
            <tr class="tablerow">
                <td>url</td>
                <td>string</td>
                <td>required</td>
                <td>Link to the document to be converted.</td>
            </tr>
            <tr class="tablerow">
                <td>title</td>
                <td>string</td>
                <td>not required</td>
                <td>Converted file name.</td>
            </tr>
            <tr class="tablerow">
                <td>filetype</td>
                <td>string</td>
                <td>required</td>
                <td>Type of the document file to be converted.<br />
                    Supported values:
                    <ul>
                        <li>docx, doc, odt, txt, rtf, html
                            <br />
                            (for text document files)</li>
                        <li>xlsx, xls, csv, ods, html
                            <br />
                            (for spreadsheet files)</li>
                        <li>pptx, ppt, pps, ppsx
                            <br />
                            (for presentation files)</li>
                    </ul>
                </td>
            </tr>
            <tr class="tablerow">
                <td>outputtype</td>
                <td>string</td>
                <td>required</td>
                <td>Resulting converted document type.<br />
                    Supported values:
                    <ul>
                        <li>docx, doc, odt, txt, rtf, html
                            <br />
                            (for text document files)</li>
                        <li>xlsx, xls, csv, ods, html
                            <br />
                            (for spreadsheet files)</li>
                        <li>pptx
                            <br />
                            (for presentation files)</li>
                    </ul>
                </td>
            </tr>
            <tr class="tablerow">
                <td>embeddedfonts</td>
                <td>boolean</td>
                <td>not required</td>
                <td>Whether or not embed the fonts into the document file.<br />
                    Supported values:
                    <ul>
                        <li>true</li>
                        <li>false</li>
                    </ul>
                    This parameter is applied not to all formats.
                </td>
            </tr>
            <tr class="tablerow">
                <td>async</td>
                <td>boolean</td>
                <td>not required</td>
                <td>Conversion request type: asynchronous or not.<br />
                    Supported values:
                    <ul>
                        <li>true</li>
                        <li>false</li>
                    </ul>
                    When the asynchronous request type is used, the reply is formed instantly. In this case to get the result it is necessary to send requests without parameter change until the conversion is finished.
                </td>
            </tr>
        </tbody>
    </table>
    <p>The request result is returned in XML form.</p>
    <div class="header-gray">Reply format</div>
    <pre>
&lt;?xml version="1.0" encoding="utf-8"?&gt;
&lt;FileResult&gt;
    &lt;FileUrl&gt;[Link to the converted file at the server]&lt;/FileUrl&gt;
    &lt;Percent&gt;[Conversion progress percentage]&lt;/Percent&gt;
    &lt;EndConvert&gt;[Conversion is finished - True|False]&lt;/EndConvert&gt;
&lt;/FileResult&gt;
</pre>
    <p>When forming the link to the resulting file, the same server name is used which was made the conversion request to.</p>
    <div class="header-gray">Reply example</div>
    <pre>
&lt;?xml version="1.0" encoding="utf-8"?&gt;
&lt;FileResult&gt;
    &lt;FileUrl&gt;http://documentserver/ResourceService.ashx?filename=output.doc&lt;/FileUrl&gt;
    &lt;Percent&gt;100&lt;/Percent&gt;
    &lt;EndConvert&gt;True&lt;/EndConvert&gt;
&lt;/FileResult&gt;
</pre>
    <div class="header-gray">Example of the intermediate reply to the asynchronous request (with the parameter <em>async=true</em>)</div>
    <pre>
&lt;?xml version="1.0" encoding="utf-8"?&gt;
&lt;FileResult&gt;
    &lt;FileUrl&gt;&lt;/FileUrl&gt;
    &lt;Percent&gt;95&lt;/Percent&gt;
    &lt;EndConvert&gt;False&lt;/EndConvert&gt;
&lt;/FileResult&gt;
</pre>
    <div class="header-gray">Reply format when an error occurred</div>
    <pre>
&lt;?xml version="1.0" encoding="utf-8"?&gt;
&lt;FileResult&gt;
    &lt;Error&gt;Error code&lt;/Error&gt;
&lt;/FileResult&gt;
</pre>
    <div class="header-gray">Example of the reply when an error occurred</div>
    <pre>
&lt;?xml version="1.0" encoding="utf-8"?&gt;
&lt;FileResult&gt;
    &lt;Error&gt;-3&lt;/Error&gt;
&lt;/FileResult&gt;
</pre>
    <div class="header-gray">Possible error codes and their description</div>
    <table class="table">
        <colgroup>
            <col style="width: 100px;" />
            <col />
        </colgroup>
        <thead>
            <tr class="tablerow">
                <td>Error code</td>
                <td>Description</td>
            </tr>
        </thead>
        <tbody>
            <tr class="tablerow">
                <td>-1</td>
                <td>Unknown error</td>
            </tr>
            <tr class="tablerow">
                <td>-2</td>
                <td>Timeout conversion error</td>
            </tr>
            <tr class="tablerow">
                <td>-3</td>
                <td>Conversion error</td>
            </tr>
            <tr class="tablerow">
                <td>-4</td>
                <td>Error while downloading the document file to be converted</td>
            </tr>
            <tr class="tablerow">
                <td>-6</td>
                <td>Error while accessing the conversion result database</td>
            </tr>
        </tbody>
    </table>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
