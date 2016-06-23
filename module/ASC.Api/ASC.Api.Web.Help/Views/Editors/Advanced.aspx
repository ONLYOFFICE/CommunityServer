<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Advanced parameters
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">Advanced parameters</span>
    </h1>

    <p>The parameters, which can be changed for ONLYOFFICE™ Document Server, can be subdivided into the following main sections:</p>

    <ul>
        <li>
            <a href="<%= Url.Action("config/") %>" class="underline"><b>config</b></a> - allows to change the platform type used, document display size (width and height) and type of the document opened;
        </li>
        <li>
            <a href="<%= Url.Action("config/document") %>" class="underline"><b>document</b></a> - contains all the parameters pertaining to the document (title, url, file type, etc.);
        </li>
        <li>
            <a href="<%= Url.Action("config/document/info") %>" class="underline"><b>info</b></a> - contains additional parameters for the document (document author, folder where the document is stored, creation date, sharing settings);
        </li>
        <li>
            <a href="<%= Url.Action("config/document/permissions") %>" class="underline"><b>permissions</b></a> - defines whether the document can be edited and downloaded or not;
        </li>
        <li>
            <a href="<%= Url.Action("config/editor") %>" class="underline"><b>editor</b></a> - defines parameters pertaining to the editor interface: opening mode (viewer or editor), interface language, additional buttons, etc.);
        </li>
        <li>
            <a href="<%= Url.Action("config/editor/customization") %>" class="underline"><b>customization</b></a> - allows to customize the editor interface so that it looked like your other products (if there are any) and change the presence or absence of the additional buttons, links, change logos and editor owner details;
        </li>
        <li>
            <a href="<%= Url.Action("config/editor/embedded") %>" class="underline"><b>embedded</b></a> - is used for the embedded document type only and allows to change the behavior of the buttons used to control the embedded mode;
        </li>
        <li>
            <a href="<%= Url.Action("config/events") %>" class="underline"><b>events</b></a> - is the list of special events called when some action is applied to the document (when it is loaded, modified, etc.).
        </li>
    </ul>

    <p>The complete <em>config</em> with all the additional parameters looks the following way:</p>

    <pre>
config = {
    "documentType": "text",
    "height": "100%",
    "type": "desktop",
    "width": "100%",
    "document": {
        "fileType": "docx",
        "key": "Khirz6zTPdfd7",
        "title": "Example Document Title.docx",
        "url": "http://example.com/url-to-example-document.docx",
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
        "permissions": {
            "download": true,
            "edit": true,
            "print": true,
            "review": true,
        },
    },
    "editorConfig": {
        "callbackUrl": "http://example.com/url-to-callback.ashx",
        "createUrl": "http://example.com/url-to-create-document/",
        "lang": "en-US",
        "mode": "edit",
        "customization": {
            "about": true,
            "chat": true,
            "comments": true
            "customer": {
                "address": "My City, 123a-45",
                "info": "Some additional information",
                "logo": "http://example.com/logo-big.png",
                "mail": "john@example.com",
                "name": "John Smith and Co.",
                "www": "example.com",
            },
            "feedback": {
                "url": "http://example.com",
                "visible": true,
            },
            "goback": {
                "text": "Go to Documents",
                "url": "http://example.com",
            },
            "logo": {
                "image": "http://example.com/logo.png",
                "imageEmbedded": "http://example.com/logo_em.png",
                "url": "http://example.com",
            },
        },
        "embedded": {
            "embedUrl": "http://example.com/embedded?doc=exampledocument1.docx",
            "fullscreenUrl": "http://example.com/embedded?doc=exampledocument1.docx#fullscreen",
            "saveUrl": "http://example.com/download?doc=exampledocument1.docx",
            "shareUrl": "http://example.com/view?doc=exampledocument1.docx",
            "toolbarDocked": "top",
        },
        "recent": [
            {
                "folder": "Example Files",
                "title": "exampledocument1.docx",
                "url": "http://example.com/exampledocument1.docx",
            },
            {
                "folder": "Example Files",
                "title": "exampledocument2.docx",
                "url": "http://example.com/exampledocument2.docx",
            },
            ...
        ],
        "user": {
            "firstname": "John",
            "id": "78e1e841",
            "lastname": "Smith",
        },
    },
    "events": {
        "onCollaborativeChanges": onCollaborativeChanges,
        "onDocumentStateChange": onDocumentStateChange,
        "onDownloadAs": onDownloadAs,
        "onError": onError,
        "onReady": onReady,
        "onRequestEditRights": onRequestEditRights,
        "onRequestHistory": onRequestHistory,
        "onRequestHistoryData": onRequestHistoryData,
        "onRequestHistoryClose": onRequestHistoryClose,
    },
};
</pre>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
