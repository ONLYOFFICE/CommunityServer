<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainContent.ascx.cs" Inherits="ASC.Web.Files.Controls.MainContent" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Files.Services.WCFService.FileOperations" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<div class="files-content-panel" data-title="<%= TitlePage %>" data-rootid="<%= FolderIDCurrentRoot %>" data-deleteConfirm="<%= FilesSettings.ConfirmDelete ? "true" : null %>">
    <asp:PlaceHolder runat="server" ID="ListHolder"></asp:PlaceHolder>
</div>

<%--popup window's--%>
<div id="filesSelectorPanel" class="studio-action-panel">

    <ul class="dropdown-content">
        <li id="filesSelectAll"><a class="dropdown-item">
            <%= FilesUCResource.ButtonSelectAll %></a></li>
        <li id="filesSelectFolder"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterFolder %></a></li>
        <li id="filesSelectDocument"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterDocument %></a></li>
        <li id="filesSelectPresentation"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterPresentation %></a></li>
        <li id="filesSelectSpreadsheet"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterSpreadsheet %></a></li>
        <li id="filesSelectImage"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterImage %></a></li>
        <li id="filesSelectMedia"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterMedia %></a></li>
        <li id="filesSelectArchive"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterArchive %></a></li>
        <li id="filesSelectFile"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterFiles %></a></li>
    </ul>
</div>
<div id="filesActionsPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <% if (!Global.IsOutsider)
           { %>
        <li id="buttonShare"><a class="dropdown-item with-icon access first-section">
            <%= FilesUCResource.ButtonShareAccess %>
            (<span></span>)</a></li>
        <% if (!CoreContext.Configuration.Personal && ProductMailAvailable && !Request.DesktopApp())
           { %>
        <li id="buttonSendInEmail"><a class="dropdown-item with-icon email first-section">
            <%= FilesUCResource.ButtonSendInEmail %>
            (<span></span>)</a></li>
        <% } %>
        <li class="dropdown-item-seporator first-section"></li>
        <% } %>
        <li id="buttonMarkRead"><a class="dropdown-item with-icon mark-as-read second-section">
            <%= FilesUCResource.RemoveIsNew %>
            (<span></span>)</a></li>
        <li id="buttonAddFavorite"><a class="dropdown-item with-icon favorites second-section">
            <%= FilesUCResource.ButtonAddFavorite %>
            (<span></span>)</a></li>
        <li id="buttonDownload"><a class="dropdown-item with-icon download second-section">
            <%= FilesUCResource.ButtonDownload %>
            (<span></span>)</a></li>
        <% if (0 < FileUtility.ExtsConvertible.Count)
           { %>
        <li id="buttonConvert"><a class="dropdown-item with-icon download-as second-section">
            <%= FilesUCResource.DownloadAs %>
            (<span></span>)</a></li>
        <% } %>
        <% if (!Global.IsOutsider)
           { %>
        <li id="buttonMoveto"><a class="dropdown-item with-icon move second-section">
            <%= FilesUCResource.ButtonMoveTo %>
            (<span></span>)</a></li>
        <li id="buttonCopyto"><a class="dropdown-item with-icon move-or-copy second-section">
            <%= FilesUCResource.ButtonCopyTo %>
            (<span></span>)</a></li>
        <li id="buttonRestore"><a class="dropdown-item with-icon restore second-section">
            <%= FilesUCResource.ButtonRestore %>
            (<span></span>)</a></li>
        <li class="dropdown-item-seporator second-section"></li>
        <li id="buttonRemoveFavorite"><a class="dropdown-item with-icon favorites third-section">
            <%= FilesUCResource.ButtonRemoveFavorite %>
            (<span></span>)</a></li>
        <li id="buttonRemoveTemplate"><a class="dropdown-item with-icon templates third-section">
            <%= FilesUCResource.ButtonRemoveTemplate %>
            (<span></span>)</a></li>
        <li id="buttonUnsubscribe"><a class="dropdown-item with-icon unlink third-section">
            <%= FilesUCResource.Unsubscribe %>
            (<span></span>)</a></li>
        <li id="buttonDelete"><a class="dropdown-item with-icon delete third-section">
            <%= FilesUCResource.ButtonDelete %>
            (<span></span>)</a></li>
        <li id="buttonEmptyTrash"><a class="dropdown-item with-icon empty-recycle-bin third-section">
            <%= FilesUCResource.ButtonEmptyTrash %></a></li>
        <% } %>
    </ul>
</div>
<div id="filesActionPanel" class="studio-action-panel">
    <ul id="actionPanelFiles" class="dropdown-content">
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesByTemplate"><a class="dropdown-item with-icon templates first-section">
            <%= FilesUCResource.ButtonCreateByTemplate %></a></li>
        <li id="filesEdit"><a class="dropdown-item with-icon edit first-section">
            <%= FilesUCResource.ButtonEdit %></a></li>
        <% } %>
        <li id="filesOpen"><a class="dropdown-item with-icon preview first-section">
            <%= FilesUCResource.OpenFile %></a></li>
        <li class="dropdown-item-seporator first-section"></li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesShareAccess"><a class="dropdown-item with-icon access second-section">
            <%= FilesUCResource.ButtonShareAccess %></a></li>
        <li id="filesGetExternalLink" data-trial="<%= !CoreContext.Configuration.Standalone && TenantExtra.GetTenantQuota().Trial ? "true" : "" %>">
            <a class="dropdown-item with-icon with-toggle extrn-link-v2 second-section">
                <%= FilesUCResource.ButtonCopyExternalLink %>
                <span class="toggle off">
                    <span class="switcher"></span>
                </span>
            </a>
        </li>
        <% if (!CoreContext.Configuration.Personal)
           { %>
        <li id="filesChangeOwner"><a class="dropdown-item with-icon user second-section">
            <%= FilesUCResource.ButtonChangeOwner %></a></li>
        <% } %>
        <% } %>
        <% if (!CoreContext.Configuration.Personal)
           { %>
        <li id="filesGetLink"><a class="dropdown-item with-icon link second-section">
            <%= UserControlsCommonResource.GetPortalLink %></a></li>
        <% } %>
        <% if (!Global.IsOutsider)
           { %>
        <% if (!CoreContext.Configuration.Personal && ProductMailAvailable && !Request.DesktopApp())
           { %>
        <li id="filesSendInEmail"><a class="dropdown-item with-icon email second-section">
            <%= FilesUCResource.ButtonSendInEmail %></a></li>
        <% } %>
        <li id="filesDocuSign"><a class="dropdown-item with-icon sign second-section">
            <%= FilesUCResource.ButtonSendDocuSign %></a></li>
        <% } %>
        <li id="filesVersion"><a class="dropdown-item dropdown-with-item with-icon history second-section">
            <%= FilesUCResource.ButtonVersion %></a></li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesLock">
            <a class="dropdown-item with-icon with-toggle block second-section">
                <%= FilesUCResource.ButtonLock %>
                <span class="toggle off">
                    <span class="switcher"></span>
                </span>
            </a>
        </li>
        <li id="filesUnlock">
            <a class="dropdown-item with-icon with-toggle block second-section">
                <%= FilesUCResource.ButtonUnlock %>
                <span class="toggle">
                    <span class="switcher"></span>
                </span>
            </a>
        </li>
        <% } %>
        <li class="dropdown-item-seporator second-section"></li>
        <li id="filesGotoParent"><a class="dropdown-item with-icon open-location third-section">
            <%= FilesUCResource.OpenParent %></a></li>
        <li id="filesMarkRead"><a class="dropdown-item with-icon mark-as-read third-section">
            <%= FilesUCResource.RemoveIsNew %></a></li>
         <% if (!Global.IsOutsider)
           { %>
        <li id="filesAddFavorite"><a class="dropdown-item with-icon favorites third-section">
            <%= FilesUCResource.ButtonAddFavorite %></a></li>
        <li id="filesAddTemplate"><a class="dropdown-item with-icon templates third-section">
            <%= FilesUCResource.ButtonAddTemplate %></a></li>
        <% } %>
        <li id="filesDownload"><a class="dropdown-item with-icon download third-section">
            <%= FilesUCResource.DownloadFile %></a></li>
        <% if (0 < FileUtility.ExtsConvertible.Count)
           { %>
        <li id="filesConvert"><a class="dropdown-item with-icon download-as third-section">
            <%= FilesUCResource.DownloadAs %></a></li>
        <% } %>
        <li id="filesMove"><a class="dropdown-item dropdown-with-item with-icon move-or-copy third-section">
            <%= FilesUCResource.ButtonMoveCopy %></a></li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesRestore"><a class="dropdown-item with-icon restore third-section">
            <%= FilesUCResource.ButtonRestore %></a></li>
        <li id="filesRename"><a class="dropdown-item with-icon rename third-section">
            <%= FilesUCResource.ButtonRename %></a></li>
        <li class="dropdown-item-seporator third-section"></li>
        <li id="filesRemoveFavorite"><a class="dropdown-item with-icon favorites fourth-section">
            <%= FilesUCResource.ButtonRemoveFavorite %></a></li>
        <li id="filesRemoveTemplate"><a class="dropdown-item with-icon templates fourth-section">
            <%= FilesUCResource.ButtonRemoveTemplate %></a></li>
        <li id="filesUnsubscribe"><a class="dropdown-item with-icon unlink fourth-section">
            <%= FilesUCResource.Unsubscribe %></a></li>
        <li id="filesRemove"><a class="dropdown-item with-icon delete fourth-section">
            <%= FilesUCResource.ButtonDelete %></a></li>
        <% } %>
    </ul>
    <ul id="actionPanelFolders" class="dropdown-content">
        <li id="foldersOpen"><a class="dropdown-item with-icon open-folder first-section">
            <%= FilesUCResource.OpenFolder %></a></li>
        <li class="dropdown-item-seporator first-section"></li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="foldersShareAccess"><a class="dropdown-item with-icon access second-section">
            <%= FilesUCResource.ButtonShareAccess %></a></li>
        <li id="foldersChangeOwner"><a class="dropdown-item with-icon user second-section">
            <%= FilesUCResource.ButtonChangeOwner %></a></li>
        <% } %>
        <% if (!CoreContext.Configuration.Personal)
           { %>
        <li id="foldersGetLink"><a class="dropdown-item second-section with-icon link second-section">
            <%= UserControlsCommonResource.GetPortalLink %></a></li>
        <% } %>
        <li class="dropdown-item-seporator second-section"></li>
        <li id="foldersGotoParent"><a class="dropdown-item with-icon open-location third-section">
            <%= FilesUCResource.OpenParent %></a></li>
        <li id="foldersDownload"><a class="dropdown-item with-icon download third-section">
            <%= FilesUCResource.DownloadFolder %></a></li>
        <li id="foldersMove"><a class="dropdown-item dropdown-with-item with-icon move-or-copy third-section">
            <%= FilesUCResource.ButtonMoveCopy %></a></li>
        <li id="foldersMarkRead"><a class="dropdown-item with-icon mark-as-read third-section">
            <%= FilesUCResource.RemoveIsNew %></a></li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="foldersRestore"><a class="dropdown-item with-icon restore third-section">
            <%= FilesUCResource.ButtonRestore %></a></li>
        <li id="foldersRename"><a class="dropdown-item with-icon rename third-section">
            <%= FilesUCResource.ButtonRename %></a></li>
        <li id="foldersChangeThirdparty"><a class="dropdown-item with-icon edit third-section">
            <%= FilesUCResource.ButtonChangeThirdParty %></a></li>
        <li class="dropdown-item-seporator third-section"></li>
        <li id="foldersRemoveThirdparty"><a class="dropdown-item with-icon delete fourth-section">
            <%= FilesUCResource.ButtonDeleteThirdParty %></a></li>
        <li id="foldersUnsubscribe"><a class="dropdown-item with-icon unlink fourth-section">
            <%= FilesUCResource.Unsubscribe %></a></li>
        <li id="foldersRemove"><a class="dropdown-item with-icon delete fourth-section">
            <%= FilesUCResource.ButtonDelete %></a></li>
        <% } %>
    </ul>
</div>
<div id="filesVersionPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <li id="filesVersions"><a class="dropdown-item">
            <%= FilesUCResource.ButtonShowVersions %></a></li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesCompleteVersion"><a class="dropdown-item">
            <%= FilesUCResource.ButtonVersionComplete %></a></li>
        <% } %>
    </ul>
</div>
<div id="filesMovePanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesMoveto"><a class="dropdown-item">
            <%= FilesUCResource.ButtonMoveTo %></a></li>
        <li id="filesCopyto"><a class="dropdown-item">
            <%= FilesUCResource.ButtonCopyTo %></a></li>
        <li id="filesCopy"><a class="dropdown-item">
            <%= FilesUCResource.ButtonCopy %></a></li>
        <% } %>
    </ul>
</div>
<div id="foldersMovePanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <% if (!Global.IsOutsider)
           { %>
        <li id="foldersMoveto"><a class="dropdown-item">
            <%= FilesUCResource.ButtonMoveTo %></a></li>
        <li id="foldersCopyto"><a class="dropdown-item">
            <%= FilesUCResource.ButtonCopyTo %></a></li>
        <% } %>
    </ul>
</div>
<div id="filesNewsPanel" class="files-news-panel studio-action-panel freeze-display">
    <ul id="filesNewsList" class="dropdown-content webkit-scrollbar"></ul>
    <span id="filesNewsMarkRead" class="baseLinkAction"><%= FilesUCResource.RemoveIsNewAll %></span>
</div>
<div id="filesTemplatesPanel" class="files-templates-panel studio-action-panel freeze-display">
    <span id="filesTemplateLoader"><%= FilesUCResource.TemplatesLoader %></span>
    <ul id="filesTemplateList" class="dropdown-content webkit-scrollbar"></ul>
    <span id="filesTemplateEmpty"><%= FilesUCResource.TemplatesEmpty %></span>
</div>

<%--dialog window--%>
<div id="confirmRemove" class="popup-modal">
    <sc:Container ID="confirmRemoveDialog" runat="server">
        <Header><%= FilesUCResource.ConfirmRemove %></Header>
        <Body>
            <div id="confirmRemoveText">
            </div>
            <div id="confirmRemoveList" class="files-remove-list webkit-scrollbar">
                <dl>
                    <dt class="confirm-remove-folders">
                        <%= FilesUCResource.Folders %> (<span class="confirm-remove-folders-count"></span>):</dt>
                    <dd class="confirm-remove-folders"></dd>
                    <dt class="confirm-remove-files">
                        <%= FilesUCResource.Documents %> (<span class="confirm-remove-files-count"></span>):</dt>
                    <dd class="confirm-remove-files"></dd>
                </dl>
            </div>
            <span id="confirmRemoveTextDescription" class="text-medium-describe">
                <%= FilesUCResource.ConfirmRemoveDescription %>
            </span>
            <span id="confirmRemoveSharpBoxTextDescription" class="text-medium-describe">
                <%= FilesUCResource.ConfirmRemoveSharpBoxDescription %>
            </span>
            <div class="middle-button-container">
                <a id="removeConfirmBtn" class="button blue middle">
                    <%= FilesUCResource.ButtonOk %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= FilesUCResource.ButtonCancel %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>
<div id="confirmOverwriteFiles" class="popup-modal">
    <sc:Container ID="confirmOverwriteDialog" runat="server">
        <Header><%= FilesUCResource.ConfirmOverwrite %></Header>
        <Body>
            <div id="overwriteMessage"></div>
            <span id="overwriteListShow" class="baseLinkAction"><%= FilesUCResource.OverwriteListShow %></span>
            <span id="overwriteListHide" class="baseLinkAction"><%= FilesUCResource.OverwriteListHide %></span>
            <ul id="overwriteList" class="webkit-scrollbar"></ul>

            <div class="overwrite-capt"><%= FilesUCResource.SelectOverwrite %></div>

            <label class="overwrite-resolve selected">
                <input type="radio" name="resolveType" value="<%= (int) FileConflictResolveType.Overwrite %>" checked="checked" />
                <span class="overwrite-resolve-descr">
                    <span class="overwrite-resolve-head"><%= FilesUCResource.ButtonOverwrite %></span>
                    <br />
                    <%= FilesUCResource.ConfirmOverwriteDescr %>
                </span>
            </label>
            <label class="overwrite-resolve">
                <input type="radio" name="resolveType" value="<%= (int) FileConflictResolveType.Duplicate %>" />
                <span class="overwrite-resolve-descr">
                    <span class="overwrite-resolve-head"><%= FilesUCResource.ButtonDuplicate %></span>
                    <br />
                    <%= FilesUCResource.ConfirmDuplicateDescr %>
                </span>
            </label>
            <label class="overwrite-resolve">
                <input type="radio" name="resolveType" value="<%= (int) FileConflictResolveType.Skip %>" />
                <span class="overwrite-resolve-descr">
                    <span class="overwrite-resolve-head"><%= FilesUCResource.ButtonSkip %></span>
                    <br />
                    <%= FilesUCResource.ConfirmSkipDescr %>
                </span>
            </label>

            <div class="middle-button-container">
                <a id="buttonConfirmResolve" class="button blue middle">
                    <%= FilesUCResource.ButtonOk %>
                </a>
                <span class="splitter-buttons"></span>
                <a id="buttonCancelOverwrite" class="button gray middle">
                    <%= FilesUCResource.ButtonCancel %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>
<div id="changeOwner" class="popup-modal">
    <sc:Container ID="changeOwnerDialog" runat="server">
        <Header>
            <div class="change-owner-header"><%= FilesUCResource.ChangeOwnerHeader %> (<span id="changeOwnerTitle"></span>)</div>
        </Header>
        <Body>
            <div id="ownerSelector" class="advanced-selector-select" data-id="">
                <div class="change-owner-selector"></div>
            </div>
            <br />
            <br />
            <span class="text-medium-describe">
                <%= FilesUCResource.ChangeOwnerDescription %>
            </span>
            <div class="middle-button-container">
                <a id="buttonSaveChangeOwner" class="button blue middle disable">
                    <%= FilesUCResource.ButtonSave %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= FilesUCResource.ButtonCancel %>
                </a>
            </div>
        </Body>
    </sc:Container>
</div>

<%-- progress --%>
<div id="bottomLoaderPanel" class="progress-dialog-container">
    <div id="progressTemplate" class="progress-dialog">
        <div class="progress-dialog-header">
        </div>
        <div class="progress-dialog-body files-progress-box">
            <progress max="100" value="0">
                <div class="asc-progress-wrapper">
                    <div class="asc-progress-value"></div>
                </div>
            </progress>
            <div class="asc-progress-percent">0</div>
        </div>
    </div>
    <asp:PlaceHolder runat="server" ID="UploaderPlaceHolder"></asp:PlaceHolder>
</div>

<asp:PlaceHolder runat="server" ID="ControlPlaceHolder"></asp:PlaceHolder>
