<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Attachments.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Attachments.Attachments" %>

<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<%if(CanAddFile) {%>  
<div class="infoPanelAttachFile" >
    <div id="fileMaxSize"><%=ASC.Web.Studio.Core.FileSizeComment.GetFileSizeNote()%></div>
    <div class="warn" id="errorFileUpload"></div>
    <div class="warn" id="wrongSign"><%=UserControlsCommonResource.ErrorMassage_SpecCharacter %></div>
</div>
<div id="files_newDocumentPanel" class="studio-action-panel" >
        <div class="corner-top left"></div>
        <ul class="dropdown-content">
            <li id="files_create_text" class="dropdown-item">
                <a onclick="Attachments.createNewDocument('<%= FileUtility.InternalExtension[FileType.Document] %>');">
                    <%= UserControlsCommonResource.ButtonCreateText%>
                </a>
            </li>
            <li id="files_create_spreadsheet" class="dropdown-item">
                <a onclick="Attachments.createNewDocument('<%= FileUtility.InternalExtension[FileType.Spreadsheet] %>');">
                    <%= UserControlsCommonResource.ButtonCreateSpreadsheet%>
                </a>
            </li>
            <li id="files_create_presentation" class="dropdown-item">
                <a onclick="Attachments.createNewDocument('<%= FileUtility.InternalExtension[FileType.Presentation] %>');">
                    <%= UserControlsCommonResource.ButtonCreatePresentation%>
                </a>
            </li>
        </ul>
</div>
         
    <div id="actionPanel" runat="server" class="containerAction">
        <span id="showDocumentPanel" >
            <a class="baseLinkAction"><%=MenuNewDocument %></a>
            <span class="sort-down-black newDocComb"></span>
        </span>
        <span id="linkNewDocumentUpload"><a class="baseLinkAction"><%=MenuUploadFile %></a></span>
        
        <%if (PortalDocUploaderVisible)
          {%>
        <span id="portalDocUploader" class="linkAttachFile" onclick="ProjectDocumentsPopup.showPortalDocUploader()" href="javascript: false;"><a class="baseLinkAction"><%=MenuProjectDocuments %></a></span>
        <% } %>
        
    </div>

<div class="information-upload-panel clearFix">
    <div class="gray-text"><%= UserControlsCommonResource.ConfirmStoreOriginalUploadTitleAC %></div>
    <div class="checkbox-container">
        <input id="storeOriginalFileFlag" type="checkbox" checked="checked"/>
        <label for="storeOriginalFileFlag" class="gray-text"><%=UserControlsCommonResource.ConfirmStoreOriginalUploadCbxLabelTextAC %></label>
    </div>
</div>
<%} %>
<div id="questionWindowAttachments" style="display: none;">
    <sc:Container ID="_hintPopup" runat="server">
    <Header>
    <%=UserControlsCommonResource.DeleteFile %>
    </Header>
    <Body>        
        <p><%=UserControlsCommonResource.QuestionDeleteFile%></p>
        <p><%=UserControlsCommonResource.NotBeUndone%></p>
        <p><a class="button blue marginLikeButton" id="okButton"><%=UserControlsCommonResource.DeleteFile%></a><a id="noButton" class="button gray"><%=UserControlsCommonResource.CancelButton %></a></p>    
    </Body>
    </sc:Container>
</div>

<asp:PlaceHolder runat="server" ID="TariffDocsEditionPlaceHolder"></asp:PlaceHolder>

<%if (EmptyScreenVisible){ %>
<%-- popup window --%>
<div id="files_hintCreatePanel" class="hintDescriptionPanel">
    <div class="popup-corner"></div>
    <%= string.Format(UserControlsCommonResource.TooltipCreate,
                        FileUtility.InternalExtension[FileType.Document],
                        FileUtility.InternalExtension[FileType.Spreadsheet],
                        FileUtility.InternalExtension[FileType.Presentation]) %>
    <a href="<%= CommonLinkUtility.GetHelpLink(true) + "gettingstarted/documents.aspx" %>" target="_blank"><%=UserControlsCommonResource.ButtonLearnMore%></a>
</div>
<div id="files_hintUploadPanel" class="hintDescriptionPanel">
    <div class="popup-corner"></div>
    <%=UserControlsCommonResource.TooltipUpload%>
</div>
<div id="files_hintOpenPanel" class="hintDescriptionPanel">
    <div class="popup-corner"></div>
    <%=string.Format(UserControlsCommonResource.TooltipOpen, ExtsWebPreviewed)%>
</div>
<div id="files_hintEditPanel" class="hintDescriptionPanel">
    <div class="popup-corner"></div>
    <%=string.Format(UserControlsCommonResource.TooltipEdit, ExtsWebEdited)%>
</div>
<%} %>
<div class="wrapperFilesContainer" moduleName="<%=ModuleName%>" projectId=<%=ProjectId %> entityType=<%=EntityType %>>
    <%if (EmptyScreenVisible)
      { %>
    <div id="emptyDocumentPanel" class="display-none">
        <asp:PlaceHolder runat="server" ID="_phEmptyDocView"></asp:PlaceHolder>
    </div>
    <%} %>
    <table id="attachmentsContainer">
        <tbody>
        </tbody>
    </table>
</div>
<div id="popupDocumentUploader">
    <asp:PlaceHolder id="_phDocUploader" runat="server"></asp:PlaceHolder>
</div>

<script id="newFileTmpl" type="text/x-jquery-tmpl">
    <tr class="newDoc">
        <td class="${tdclass}" colspan="2">
            <input id="newDocTitle" type="text" class="textEdit" data="<%= UserControlsCommonResource.NewDocument%>" maxlength="165" value="<%= UserControlsCommonResource.NewDocument%>"/>
            <span id="${type}" onclick="Attachments.createFile();" class="button gray btn-action __apply createFile" title="<%= Resource.AddButton%>"></span>
            <span onclick="Attachments.removeNewDocument();" title="<%= UserControlsCommonResource.QuickLinksDeleteLink%>" class="button gray btn-action __reset remove"></span>
        </td>        
    </tr>
</script>

<script id="fileAttachTmpl" type="text/x-jquery-tmpl">

<tr>
    <td id="af_${id}">
        {{if type=="image"}}
        
            <a href="${viewUrl}" rel="imageGalery" class="screenzoom ${exttype}" title="${title}">
                <div class="attachmentsTitle">${title}</div>
                {{if versionGroup > 1}}
                        <span class="version"><%= UserControlsCommonResource.Version%>${versionGroup}</span>
                {{/if}}
            </a>
            
        {{else}}
            {{if type == "editedFile" || type == "viewedFile"}}
                <a href="${docViewUrl}" class="${exttype}" title="${title}" target="_blank">
                    <div class="attachmentsTitle">${title}</div>
                    {{if versionGroup > 1}}
                        <span class="version"><%= UserControlsCommonResource.Version%>${versionGroup}</span>
                    {{/if}}
                </a>
            {{else}}
                <a href="${downloadUrl}" class="${exttype} noEdit" title="${title}" target="_blank">
                    <div class="attachmentsTitle">${title}</div>
                    {{if versionGroup > 1}}
                        <span class="version"><%= UserControlsCommonResource.Version%>${versionGroup}</span>
                    {{/if}}
                </a>
            {{/if}}
            
        {{/if}}
    </td>
    
    <td class="editFile">
        {{if (access==0 || access==1)}}
            <a class="{{if trashAction == "delete"}}deleteDoc{{else}}unlinkDoc{{/if}}" title="{{if trashAction == "delete"}}<%= UserControlsCommonResource.DeleteFile%>{{else}}<%= UserControlsCommonResource.RemoveFromList%>{{/if}}" data-fileId="${id}"></a>
        {{/if}}
        {{if (!jq.browser.mobile)}}
        <a class="downloadLink" title="<%= UserControlsCommonResource.DownloadFile%>" href="${downloadUrl}"></a>
        {{/if}}
        {{if (type == "editedFile")&&(access==0 || access==1)}}
            <a id="editDoc_${id}" title="<%= UserControlsCommonResource.EditFile%>" target="_blank" href="${editUrl}"></a>
        {{/if}}
    </td>
</tr>

</script>