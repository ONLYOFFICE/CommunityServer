<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<string>"
    ContentType="text/html" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Integration Plugins
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">Integration Plugins</span>
    </h1>

    <p>ONLYOFFICE™ Document Editors can be integrated into some of the existing document management systems using plugins. These plugins will add the document editing functions into the document management system you use quickly and easily without the need of writing the code by yourselves.</p>

    <p>Currently the following plugins are available:</p>
    <ul>
        <li><a href="<%= Url.Action("alfresco") %>" class="underline">Alfresco ONLYOFFICE™ integration plugin</a></li>
        <%--<li><a href="<%= Url.Action("confluence") %>" class="underline">Confluence ONLYOFFICE™ integration plugin</a></li>--%>
    </ul>
    
    <br />
    <p>
        If you have any further questions, please contact us at
        <a href="mailto:integration@onlyoffice.com" class="underline" onclick="if(window.ga){window.ga('send','event','mailto');}return true;">integration@onlyoffice.com</a>.
    </p>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceholder"></asp:Content>
