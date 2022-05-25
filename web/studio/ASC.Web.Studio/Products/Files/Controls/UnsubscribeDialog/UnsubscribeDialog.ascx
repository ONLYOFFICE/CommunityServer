<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UnsubscribeDialog.ascx.cs" Inherits="ASC.Web.Files.Controls.UnsubscribeDialog" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>


<div id="filesConfirmUnsubscribe" class="popup-modal">
    <sc:Container ID="UnsubscribeDialogContainer" runat="server">
        <header><%= FilesUCResource.ConfirmRemove %></header>
        <body>
            <div id="confirmUnsubscribeText">
                <%= FilesUCResource.ConfirmUnsubscribe %>
            </div>
            <div id="confirmUnsubscribeList" class="files-remove-list webkit-scrollbar">
                <dl>
                    <dt class="confirm-remove-folders">
                        <%= FilesUCResource.Folders %>:</dt>
                    <dd class="confirm-remove-folders"></dd>
                    <dt class="confirm-remove-files">
                        <%= FilesUCResource.Documents %>:</dt>
                    <dd class="confirm-remove-files"></dd>
                </dl>
            </div>
            <div class="middle-button-container">
                <a id="unsubscribeConfirmBtn" class="button blue middle">
                    <%= FilesUCResource.ButtonOk %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= FilesUCResource.ButtonCancel %>
                </a>
            </div>
        </body>
    </sc:Container>
</div>