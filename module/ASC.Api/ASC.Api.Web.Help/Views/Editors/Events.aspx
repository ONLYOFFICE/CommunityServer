<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Events
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        <%= Html.ActionLink(" ", "config", null, new {@class = "up"}) %>
        <span class="hdr">Events</span>
    </h1>

    <ul>
        <li>
            <b>onDocumentStateChange</b> - the function called when the document is modified;
        </li>
        <li>
            <b>onError</b> - the function called when an error or some other specific event occurs;
        </li>
        <li>
            <b>onReady</b> - the function called when the document is loaded into the document editor;
        </li>
        <li>
            <b>onRequestEditRights</b> - the function called when the user is trying to switch the document from the viewing into the editing mode.
        </li>
    </ul>

    <div class="note">
        <b>onRequestEditRights</b> parameter is obligatory when the <a href="<%= Url.Action("editor") %>" class="underline">editorConfig</a> <em>mode</em> parameter is set to <b>view</b> and the <em>permission</em> to <em>edit</em> the document (<a href="<%= Url.Action("docpermissions") %>" class="underline">document permissions</a>) is set to <b>true</b> so that the user could switch to the editing mode.
    </div>

    <div class="header-gray">Example</div>
    <pre>
var onDocumentStateChange = function (event) {
    if (event.data) {
        console.log("The document changed");
    } else {
        console.log("Changes are collected on document editing service");
    }
};

var onError = function (event) {
    console.log("ONLYOFFICE™ Document Editor reports an error: " + event.data);
};

var onReady = function() {
    console.log("ONLYOFFICE™ Document Editor is ready");
};

var onRequestEditRights = function () {
    console.log("ONLYOFFICE™ Document Editor requests editing rights");
};

var docEditor = new DocsAPI.DocEditor('placeholder', {
    ...
    events: {
        'onDocumentStateChange': onDocumentStateChange,
        'onError': onError,
        'onReady': onReady,
        'onRequestEditRights': onRequestEditRights,
    },
});
</pre>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
