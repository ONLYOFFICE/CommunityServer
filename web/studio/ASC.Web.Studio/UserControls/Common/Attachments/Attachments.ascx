<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Attachments.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Attachments.Attachments" %>

<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="Resources" %>

<% if (CanAddFile)
   { %>  
<div class="infoPanelAttachFile" >
    <div id="fileMaxSize"><%= ASC.Web.Studio.Core.FileSizeComment.GetFileSizeNote() %></div>
    <div class="warn" id="errorFileUpload"></div>
    <div class="warn" id="wrongSign"><%= UserControlsCommonResource.ErrorMassage_SpecCharacter %></div>
</div>
<div id="files_newDocumentPanel" class="studio-action-panel" >
    <ul class="dropdown-content">
        <li id="files_create_text" >
            <a onclick="Attachments.createNewDocument('<%= FileUtility.InternalExtension[FileType.Document] %>');" class="dropdown-item">
                <%= UserControlsCommonResource.ButtonCreateText %>
            </a>
        </li>
        <li id="files_create_spreadsheet">
            <a onclick="Attachments.createNewDocument('<%= FileUtility.InternalExtension[FileType.Spreadsheet] %>');" class="dropdown-item">
                <%= UserControlsCommonResource.ButtonCreateSpreadsheet %>
            </a>
        </li>
        <li id="files_create_presentation">
            <a onclick="Attachments.createNewDocument('<%= FileUtility.InternalExtension[FileType.Presentation] %>');" class="dropdown-item">
                <%= UserControlsCommonResource.ButtonCreatePresentation %>
            </a>
        </li>
    </ul>
</div>

<div id="actionPanel" runat="server" class="containerAction">
    <span id="showDocumentPanel" >
        <a class="baseLinkAction"><%= MenuNewDocument %></a>
        <span class="sort-down-black newDocComb"></span>
    </span>
    <span id="linkNewDocumentUpload"><a class="baseLinkAction"><%= MenuUploadFile %></a></span>

    <asp:PlaceHolder id="DocUploaderHolder" runat="server"></asp:PlaceHolder>
</div>

<% if (EnableAsUploaded)
   { %>
<div class="information-upload-panel clearFix">
    <div class="gray-text"><%= UserControlsCommonResource.ConfirmStoreOriginalUploadTitleAC %></div>
    <div class="checkbox-container">
        <input id="storeOriginalFileFlag" type="checkbox" checked="checked"/>
        <label for="storeOriginalFileFlag" class="gray-text"><%= UserControlsCommonResource.ConfirmStoreOriginalUploadCbxLabelTextAC %></label>
    </div>
</div>
<% } %>
<% } %>

<asp:PlaceHolder runat="server" ID="TariffDocsEditionPlaceHolder"></asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="MediaViewersPlaceHolder"></asp:PlaceHolder>

<% if (EmptyScreenVisible)
   { %>
<%-- popup window --%>
<div id="files_hintCreatePanel" class="hintDescriptionPanel">
    <%= string.Format(UserControlsCommonResource.TooltipCreate,
                      FileUtility.InternalExtension[FileType.Document],
                      FileUtility.InternalExtension[FileType.Spreadsheet],
                      FileUtility.InternalExtension[FileType.Presentation]) %>
    <% if (!string.IsNullOrEmpty(HelpLink))
       { %>
    <a href="<%= HelpLink + "/gettingstarted/documents.aspx" %>" target="_blank"><%= UserControlsCommonResource.ButtonLearnMore %></a>
    <% } %>
</div>
<div id="files_hintUploadPanel" class="hintDescriptionPanel">
    <%= UserControlsCommonResource.TooltipUpload %>
</div>
<div id="files_hintOpenPanel" class="hintDescriptionPanel">
    <%= string.Format(UserControlsCommonResource.TooltipOpen, ExtsWebPreviewed) %>
</div>
<div id="files_hintEditPanel" class="hintDescriptionPanel">
    <%= string.Format(UserControlsCommonResource.TooltipEdit, ExtsWebEdited) %>
</div>
<% } %>
<div class="wrapperFilesContainer" moduleName="<%= ModuleName %>" entityType="<%= EntityType %>" >
    <% if (EmptyScreenVisible)
       { %>
    <div id="emptyDocumentPanel" class="display-none">
        <asp:PlaceHolder runat="server" ID="_phEmptyDocView"></asp:PlaceHolder>
    </div>
    <% } %>
    <table id="attachmentsContainer">
        <tbody>
        </tbody>
    </table>
</div>
