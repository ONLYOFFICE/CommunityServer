<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EmptyFolder.ascx.cs" Inherits="ASC.Web.Files.Controls.EmptyFolder" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<asp:PlaceHolder runat="server" ID="EmptyScreenFolder" />

<%-- popup window --%>
<div id="hintCreatePanel" class="hintDescriptionPanel">
    <div class="popup-corner"></div>
    <%= string.Format(FilesUCResource.TooltipCreate,
                        FileUtility.InternalExtension[FileType.Document],
                        FileUtility.InternalExtension[FileType.Spreadsheet],
                        FileUtility.InternalExtension[FileType.Presentation]) %>
    <% if (!string.IsNullOrEmpty(CommonLinkUtility.GetHelpLink()))
             { %>
    <a href="<%= CommonLinkUtility.GetHelpLink(true) + "/gettingstarted/documents.aspx" %>" target="_blank"><%= FilesUCResource.ButtonLearnMore %></a>
    <% } %>
</div>
<div id="hintUploadPanel" class="hintDescriptionPanel">
    <div class="popup-corner"></div>
    <%= FilesUCResource.TooltipUpload %>
</div>
<div id="hintOpenPanel" class="hintDescriptionPanel">
    <div class="popup-corner"></div>
    <%= string.Format(FilesUCResource.TooltipOpen, ExtsWebPreviewed) %>
</div>
<div id="hintEditPanel" class="hintDescriptionPanel">
    <div class="popup-corner"></div>
    <%= string.Format(FilesUCResource.TooltipEdit, ExtsWebEdited) %>
</div>
