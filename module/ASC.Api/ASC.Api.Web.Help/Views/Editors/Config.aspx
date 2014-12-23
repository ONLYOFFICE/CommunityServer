<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Config
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <h1>
        <span class="hdr">Config</span>
    </h1>
    
    <div class="header-gray">Description</div>
    <p class="dscr">The config base section allows to change the following editor settings</p>

    <div class="header-gray">Parameters</div>
    <table class="table">
        <colgroup>
            <col style="width: 20%"/>
            <col/>
            <col style="width: 110px"/>
            <col style="width: 20%"/>
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
                <td>type</td>
                <td>defines the platform type used to access the document. Can be:
                    <em>optimized to access the document from a desktop or laptop computer</em> - <b>desktop</b>,
                    <em>optimized to access the document from a tablet or a smartphone</em> - <b>mobile</b>,
                    <em>specifically formed to be easily embedded into a web page</em> - <b>embedded</b>
                </td>
                <td>string</td>
                <td>"desktop"</td>
            </tr>
            <tr class="tablerow">
                <td>width</td>
                <td>defines the document width (<b>100%</b> by default) in the browser window</td>
                <td>string</td>
                <td>"100%"</td>
            </tr>
            <tr class="tablerow">
                <td>height</td>
                <td>defines the document height (<b>100%</b> by default) in the browser window</td>
                <td>string</td>
                <td>"100%"</td>
            </tr>
            <tr class="tablerow">
                <td>documentType</td>
                <td>defines the document type to be opened:
                    <em>open a text document (.doc, .docx, .odt, .txt) for viewing or editing</em> - <b>text</b>,
				    <em>open a PDF-document (.pdf) for viewing or editing</em> - <b>text-pdf</b>,
				    <em>open a spreadsheet (.xls, .xlsx, .ods) for viewing or editing</em> - <b>spreadsheet</b>,
				    <em>open a presentation (.ppt, .pptx, .odp) for viewing  or editing</em>- <b>presentation</b>
                </td>
                <td>string</td>
                <td>"spreadsheet"</td>
            </tr>
        </tbody>
    </table>

    <div class="header-gray">Example</div>
    <pre>
    var docEditor = new DocsAPI.DocEditor('placeholder', {
       type: 'desktop',
       width: '100%',
       height: '100%',
       documentType: 'spreadsheet'
    });
    </pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>