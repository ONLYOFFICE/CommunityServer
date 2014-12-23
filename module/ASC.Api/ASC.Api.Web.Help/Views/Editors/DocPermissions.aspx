<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Document Permissions
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "document", null, new {@class = "up"}) %>
        <span class="hdr">Document Permissions</span>
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
                <td>edit</td>
                <td>defines if the document can be edited or only viewed. In case the editing permission is set to <b>'true'</b> the <b>File</b> menu will contain the <b>Edit Document</b> menu option</td>
                <td>boolean</td>
                <td>true</td>
            </tr>
            <tr class="tablerow">
                <td>download</td>
                <td>defines if the document can be downloaded or only viewed or edited online. In case the downloading permission is set to <b>'true'</b> the <b>File</b> menu will contain the <b>Download as...</b> menu option</td>
                <td>boolean</td>
                <td>false</td>
            </tr>
        </tbody>
    </table>

    <div class="header-gray">Example</div>
    <pre>
    var docEditor = new DocsAPI.DocEditor('placeholder', {
        document: {
            permissions: {
                edit: true,
                download: false
            }
        }
    });
    </pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>