<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Document
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "config", null, new {@class = "up"}) %>
        <span class="hdr">Document</span>
    </h1>
    
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
                <td>title</td>
                <td>defines the desired title for the viewed or edited document</td>
                <td>string</td>
                <td>"Example Document Title.doc"</td>
            </tr>
            <tr class="tablerow">
                <td>url</td>
                <td>defines the url where the source viewed or edited document is stored</td>
                <td>string</td>
                <td>"http://url-to-example-document/"</td>
            </tr>
            <tr class="tablerow">
                <td>fileType</td>
                <td>defines the type of the file for the source viewed or edited document</td>
                <td>string</td>
                <td>"doc"</td>
            </tr>
            <tr class="tablerow">
                <td>key</td>
                <td>the unique document identifier used for document recognition by the service. In case the known key is sent the document will be taken from the cache. The document url can be used as the <b>key</b> but without the special characters and the size is limited</td>
                <td>string</td>
                <td>"Khirz6zTPdfd7riF8lgCc56Rya"</td>
            </tr>
            <tr class="tablerow">
                <td>vkey</td>
                <td>the additional validation key generated based on the <b>key</b> - the encoded combination in which the additional data is stored: permission rights check, user IP, current time, license key</td>
                <td>string</td>
                <td>"d0hOVHBKdVVQaTBma1dHa1dBbFJ"</td>
            </tr>
        </tbody>
    </table>
    
    <div class="header-gray">Example</div>
    <pre>
    var docEditor = new DocsAPI.DocEditor('placeholder', {
       document: {
          title: 'Example Document Title.doc',
          url: 'http://url-to-example-document/',
          fileType: 'doc',
          key: 'Khirz6zTPdfd7riF8lgCc56Rya',
          vkey: 'd0hOVHBKdVVQaTBma1dHa1dBbFJ'
       }
    });
    </pre>
	
    <div class="note">The key characters can be used: <b>0-9</b>, <b>a-z</b>, <b>A-Z</b>, <b>-._=</b>. The maximal key length is <b>50</b> characters.</div>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>