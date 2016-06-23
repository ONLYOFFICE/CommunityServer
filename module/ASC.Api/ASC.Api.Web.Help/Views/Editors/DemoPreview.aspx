<%@ Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<List<string>>"
    ContentType="text/html" %>

<%@ Import Namespace="ASC.Web.Core.Files" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Integration Examples
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h1>
        <span class="hdr">Integration Examples</span>
    </h1>

    <h2>To start integrating document editors into your own website you need to do the following:</h2>

    <ol>
        <li>Download Document Server installation and set it up on your local server:
            <div class="demo-info-buttons">
                <a class="button" href="http://sourceforge.net/projects/teamlab/files/ONLYOFFICE_DocumentServer/v3.0/binaries/" target="_blank">Download Editors
                </a>
            </div>
        </li>

        <% if (Model.Count > 0)
           { %>
        <li>Select the programming language and download the code for the sample of online editors integration into your web site:
            <div class="demo-info-buttons">
                <% foreach (var example in Model)
                   { %>
                <a class="button" href="<%= Url.Content("~/app_data/" + example.Replace("#", "%23")) %>" target="_blank">
                    <%= example.TrimEnd(".zip".ToCharArray()) %>
                </a>
                <% } %>
            </div>
        </li>
        <% } %>

        <li><a href="<%= Url.Action("advanced") %>" class="underline">Edit the configuration files</a> in the sample changing the default path for the one to the editors installed at step 1 and other advanced parameters available for editor configuration.</li>
    </ol>

    <p>The result should look like the demo preview below.</p>

    <h2 id="DemoPreview">Demo Preview</h2>
    <table class="demo-tab-panel">
        <tr>
            <td>
                <%= Html.ActionLink("Demo Document editor", "demopreview", "editors", null, null, "text", null, new { id = "textDemo", @class = "active" }) %>
            </td>
            <td>
                <%= Html.ActionLink("Demo Spreadsheet editor", "demopreview", "editors", null, null, "spreadsheet", null, new { id = "spreadsheetDemo", @class = "demo-tab-center" }) %>
            </td>
            <td>
                <%= Html.ActionLink("Demo Presentation editor", "demopreview", "editors", null, null, "presentation", null, new { id = "presentationDemo" }) %>
            </td>
        </tr>
    </table>

    <div class="demo-block">
        <div id="embeddedEditor"></div>
    </div>

    <p>
        If you have any further questions, please contact us at
        <a href="mailto:integration@onlyoffice.com" class="underline" onclick="if(window.ga){window.ga('send','event','mailto');}return true;">integration@onlyoffice.com</a>.
    </p>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptPlaceholder" runat="server">
    <script type="text/javascript">
        <%
        var docService = new DocumentService(ConfigurationManager.AppSettings["editor_key_id"] ?? "", ConfigurationManager.AppSettings["editor_key_secret"] ?? "", 0);
        var docKey = DocumentService.GenerateRevisionId(Guid.NewGuid().ToString());
        %>
        Config.EditorKey = "<%= docKey %>";
        Config.EditorVKey = "<%= docService.GenerateValidateKey(docKey, string.Empty) %>";
        Config.DocumentDemoUrl = "<%= ConfigurationManager.AppSettings["document_demo_url"] ?? "" %>";
        Config.SpreadsheetDemoUrl = "<%= ConfigurationManager.AppSettings["spreadsheet_demo_url"] ?? "" %>";
        Config.PresentationDemoUrl = "<%= ConfigurationManager.AppSettings["presentation_demo_url"] ?? "" %>";
    </script>

    <script id="scriptApi" type="text/javascript" src="<%= ConfigurationManager.AppSettings["editor_api_url"] ?? "" %>"></script>
    <script type="text/javascript" src="<%= Url.Content("~/scripts/init.js") %>"></script>

</asp:Content>
