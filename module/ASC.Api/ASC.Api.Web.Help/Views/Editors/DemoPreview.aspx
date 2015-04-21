<%@ 
    Page
    Title=""
    Language="C#"
    MasterPageFile="~/Views/Shared/Site.Master"
    Inherits="System.Web.Mvc.ViewPage<List<string>>"
    ContentType="text/html"
%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    DocumentEditor
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <% var examples = Model as List<string>; %>

    <h1>
        <span class="hdr">Demo Preview</span>
    </h1>

    <div id="embeddedEditor"></div>

    <% if(examples.Count > 0) { %>
    <h2>Want to learn how it works?</h2>

    <div class="demo-info-container">1. Download Document Server installation and set it up on your local server:</div>
    <div>
        <a class="button" href="http://sourceforge.net/projects/teamlab/files/ONLYOFFICE%207.7/ONLYOFFICE-OnlineEditors-v2.5.7.exe/download" target="_blank">
            Download Editors
        </a>
    </div>

    <div class="demo-info-container">2. Select the programming language and download the code for the sample of online editors integration into your web site:</div>
    <div>
        <% foreach (var example in examples) { %>
        <a class="button" href="<%= Url.Content("~/app_data/" + example.Replace("#","%23")) %>" target="_blank">
            <%= example.TrimEnd(".zip".ToCharArray()) %>
        </a>
        <% } %>
    </div>
    <% } %>

    <div class="demo-info-container">3. Edit the configuration files in the sample changing the default path for the one to the editors installed at step 1.</div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptPlaceholder" runat="server">
    
    <script id="scriptApi" type="text/javascript" src="<%= ConfigurationManager.AppSettings["editor_api_url"] ?? "" %>"></script>
    <script type="text/javascript" src="<%=Url.Content("~/scripts/init.js") %>"></script>

</asp:Content>
