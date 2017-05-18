<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainContent.ascx.cs" Inherits="ASC.Web.Files.Controls.MainContent" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Files.Services.WCFService.FileOperations" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div class="files-content-panel" data-title="<%= TitlePage %>" data-rootid="<%= FolderIDCurrentRoot %>">
    <%-- Advansed Filter --%>
    <div class="files-filter">
        <div></div>
    </div>

    <%-- Main Content Header --%>
    <ul id="mainContentHeader" class="contentMenu">
        <li class="menuAction menuActionSelectAll">
            <div class="menuActionSelect">
                <input type="checkbox" id="filesSelectAllCheck" title="<%= FilesUCResource.MainHeaderSelectAll %>" />
            </div>
            <div class="down_arrow" title="<%= FilesUCResource.TitleSelectFile %>">
            </div>
        </li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="mainShare" class="menuAction" title="<%= FilesUCResource.ButtonShareAccess %>">
            <span><%= FilesUCResource.ButtonShareAccess %></span>
        </li>
        <% } %>
        <li id="mainDownload" class="menuAction" title="<%= FilesUCResource.ButtonDownload %>">
            <span><%= FilesUCResource.ButtonDownload %></span>
        </li>
        <% if (0 < FileUtility.ExtsConvertible.Count)
           { %>
        <li id="mainConvert" class="menuAction" title="<%= FilesUCResource.DownloadAs %>">
            <span><%= FilesUCResource.DownloadAs %></span>
        </li>
        <% } %>
        <% if (!Global.IsOutsider)
           { %>
        <li id="mainMove" class="menuAction" title="<%= FilesUCResource.ButtonMoveTo %>">
            <span><%= FilesUCResource.ButtonMoveTo %></span>
        </li>
        <li id="mainCopy" class="menuAction" title="<%= FilesUCResource.ButtonCopyTo %>">
            <span><%= FilesUCResource.ButtonCopyTo %></span>
        </li>
        <li id="mainMarkRead" class="menuAction" title="<%= FilesUCResource.RemoveIsNew %>">
            <span><%= FilesUCResource.RemoveIsNew %></span>
        </li>
        <li id="mainUnsubscribe" class="menuAction" title="<%= FilesUCResource.Unsubscribe %>">
            <span><%= FilesUCResource.Unsubscribe %></span>
        </li>
        <li id="mainRestore" class="menuAction" title="<%= FilesUCResource.ButtonRestore %>">
            <span><%= FilesUCResource.ButtonRestore %></span>
        </li>
        <li id="mainDelete" class="menuAction" title="<%= FilesUCResource.ButtonDelete %>">
            <span><%= FilesUCResource.ButtonDelete %></span>
        </li>
        <li id="mainEmptyTrash" class="menuAction" title="<%= FilesUCResource.ButtonEmptyTrash %>">
            <span><%= FilesUCResource.ButtonEmptyTrash %></span>
        </li>
        <% } %>
        <li id="switchViewFolder" class="menuSwitchViewFolder">
            <div id="switchToNormal" class="switchToNormal" title="<%= FilesUCResource.SwitchViewToNormal %>">
                &nbsp;
            </div>
            <div id="switchToCompact" class="switchToCompact" title="<%= FilesUCResource.SwitchViewToCompact %>">
                &nbsp;
            </div>
        </li>
        <li class="menu-action-on-top" title="<%= FilesUCResource.ButtonUp %>">
            <span class="on-top-link"><%= FilesUCResource.ButtonUp %></span>
        </li>
    </ul>

    <asp:PlaceHolder runat="server" ID="ListHolder"></asp:PlaceHolder>
</div>

<%--popup window's--%>
<div id="filesSelectorPanel" class="studio-action-panel">

    <ul class="dropdown-content">
        <li id="filesSelectAll"><a class="dropdown-item">
            <%= FilesUCResource.ButtonSelectAll %></a></li>
        <li id="filesSelectFile"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterFile %></a></li>
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
        <li id="filesSelectArchive"><a class="dropdown-item">
            <%= FilesUCResource.ButtonFilterArchive %></a></li>
    </ul>
</div>
<div id="filesActionsPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <% if (!Global.IsOutsider)
           { %>
        <li id="buttonShare"><a class="dropdown-item">
            <%= FilesUCResource.ButtonShareAccess %>
            (<span></span>)</a></li>
        <% } %>
        <li id="buttonDownload"><a class="dropdown-item">
            <%= FilesUCResource.ButtonDownload %>
            (<span></span>)</a></li>
        <% if (0 < FileUtility.ExtsConvertible.Count)
           { %>
        <li id="buttonConvert"><a class="dropdown-item">
            <%= FilesUCResource.DownloadAs %>
            (<span></span>)</a></li>
        <% } %>
        <% if (!Global.IsOutsider)
           { %>
        <li id="buttonMoveto"><a class="dropdown-item">
            <%= FilesUCResource.ButtonMoveTo %>
            (<span></span>)</a></li>
        <li id="buttonCopyto"><a class="dropdown-item">
            <%= FilesUCResource.ButtonCopyTo %>
            (<span></span>)</a></li>
        <li id="buttonRestore"><a class="dropdown-item">
            <%= FilesUCResource.ButtonRestore %>
            (<span></span>)</a></li>
        <li id="buttonUnsubscribe"><a class="dropdown-item">
            <%= FilesUCResource.Unsubscribe %>
            (<span></span>)</a></li>
        <li id="buttonDelete"><a class="dropdown-item">
            <%= FilesUCResource.ButtonDelete %>
            (<span></span>)</a></li>
        <li id="buttonEmptyTrash"><a class="dropdown-item">
            <%= FilesUCResource.ButtonEmptyTrash %></a></li>
        <% } %>
    </ul>
</div>
<div id="filesActionPanel" class="studio-action-panel">

    <ul id="actionPanelFiles" class="dropdown-content">
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesEdit"><a class="dropdown-item">
            <%= FilesUCResource.ButtonEdit %></a></li>
        <% } %>
        <li id="filesOpen"><a class="dropdown-item">
            <%= FilesUCResource.OpenFile %></a></li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesDocuSign"><a class="dropdown-item">
            <%= FilesUCResource.ButtonSendDocuSign %></a></li>
        <% } %>
        <li id="filesAccess"><a class="dropdown-item dropdown-with-item">
            <%= FilesUCResource.ButtonAccess %></a></li>
        <li id="filesVersion"><a class="dropdown-item dropdown-with-item">
            <%= FilesUCResource.ButtonVersion %></a></li>
        <li id="filesDownload"><a class="dropdown-item">
            <%= FilesUCResource.DownloadFile %></a></li>
        <% if (0 < FileUtility.ExtsConvertible.Count)
           { %>
        <li id="filesConvert"><a class="dropdown-item">
            <%= FilesUCResource.DownloadAs %></a></li>
        <% } %>
        <li id="filesMove"><a class="dropdown-item dropdown-with-item">
            <%= FilesUCResource.ButtonMoveCopy %></a></li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesRestore"><a class="dropdown-item">
            <%= FilesUCResource.ButtonRestore %></a></li>
        <li id="filesRename"><a class="dropdown-item">
            <%= FilesUCResource.ButtonRename %></a></li>
        <li id="filesUnsubscribe"><a class="dropdown-item">
            <%= FilesUCResource.Unsubscribe %></a></li>
        <li id="filesRemove"><a class="dropdown-item">
            <%= FilesUCResource.ButtonDelete %></a></li>
        <% } %>
    </ul>
    <ul id="actionPanelFolders" class="dropdown-content">
        <li id="foldersOpen"><a class="dropdown-item">
            <%= FilesUCResource.OpenFolder %></a></li>
        <li id="foldersAccess"><a class="dropdown-item dropdown-with-item">
            <%= FilesUCResource.ButtonAccess %></a></li>
        <li id="foldersDownload"><a class="dropdown-item">
            <%= FilesUCResource.DownloadFolder %></a></li>
        <li id="foldersMove"><a class="dropdown-item dropdown-with-item">
            <%= FilesUCResource.ButtonMoveCopy %></a></li>
        <% if (!Global.IsOutsider)
           { %>
        <li id="foldersRestore"><a class="dropdown-item">
            <%= FilesUCResource.ButtonRestore %></a></li>
        <li id="foldersRename"><a class="dropdown-item">
            <%= FilesUCResource.ButtonRename %></a></li>
        <li id="foldersChangeThirdparty"><a class="dropdown-item">
            <%= FilesUCResource.ButtonChangeThirdParty %></a></li>
        <li id="foldersRemoveThirdparty"><a class="dropdown-item">
            <%= FilesUCResource.ButtonDeleteThirdParty %></a></li>
        <li id="foldersUnsubscribe"><a class="dropdown-item">
            <%= FilesUCResource.Unsubscribe %></a></li>
        <li id="foldersRemove"><a class="dropdown-item">
            <%= FilesUCResource.ButtonDelete %></a></li>
        <% } %>
    </ul>
</div>
<div id="filesAccessPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesShareAccess"><a class="dropdown-item">
            <%= FilesUCResource.ButtonShareAccess %></a></li>
        <li id="filesChangeOwner"><a class="dropdown-item">
            <%= FilesUCResource.ButtonChangeOwner %></a></li>
        <% } %>
        <% if (!CoreContext.Configuration.Personal)
           { %>
        <li id="filesGetLink"><a class="dropdown-item">
            <%= UserControlsCommonResource.GetPortalLink %></a></li>
        <% } %>
        <% if (!Global.IsOutsider)
           { %>
        <li id="filesLock"><a class="dropdown-item">
            <%= FilesUCResource.ButtonLock %></a></li>
        <li id="filesUnlock"><a class="dropdown-item">
            <%= FilesUCResource.ButtonUnlock %></a></li>
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
<div id="foldersAccessPanel" class="studio-action-panel">
    <ul class="dropdown-content">
        <% if (!Global.IsOutsider)
           { %>
        <li id="foldersShareAccess"><a class="dropdown-item">
            <%= FilesUCResource.ButtonShareAccess %></a></li>
        <li id="foldersChangeOwner"><a class="dropdown-item">
            <%= FilesUCResource.ButtonChangeOwner %></a></li>
        <% } %>
        <% if (!CoreContext.Configuration.Personal)
           { %>
        <li id="foldersGetLink"><a class="dropdown-item">
            <%= UserControlsCommonResource.GetPortalLink %></a></li>
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
