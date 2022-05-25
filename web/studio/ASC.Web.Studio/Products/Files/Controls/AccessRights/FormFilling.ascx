<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FormFilling.ascx.cs" Inherits="ASC.Web.Files.Controls.FormFilling" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<div id="formFillingPanel">
    <a id="formFillingLink">
        <span class="baseLinkAction"><%= FilesUCResource.FormFillingCollect %></span>
    </a>

    <div id="formFillingDialogParent">
        <div id="formFillingOwnerDialog" class="popupContainerClass">
            <div class="containerHeaderBlock">
                <div><%= FilesUCResource.FormFillingOwnerDialogHeader %></div>
                <div class="popupCancel">
                    <div class="cancelButton">×</div>
                </div>
            </div>
            <div class="containerBodyBlock">
                <p><%= FilesUCResource.FormFillingOwnerDialogBody %></p>
                <div class="small-button-container">
                    <a id="formFillingSaveOwnerBtn" class="button middle blue"><%= FilesUCResource.FormFillingOwnerDialogResetBtn %></a>
                    <span class="splitter-buttons"></span>
                    <a id="formFillingCancelOwnerBtn" class="button middle gray"><%= Resource.CancelButton %></a>
                </div>
            </div>
        </div>
        <div id="formFillingDialog">
            <div id="formFillingDialogHeader">
                <div class="bottom-indent-16">
                    <input type="checkbox" id="formFillingEnableCbx" class="on-off-checkbox" />
                    <label for="formFillingEnableCbx" class="header-base-small"><%= FilesUCResource.FormFillingDialogEnableCollection %></label>
                </div>
            </div>
            <div id="formFillingDialogBody">
                <div id="formFillingFolderContainer" class="bottom-indent-8">
                    <span class="header-base-small"><%= FilesUCResource.FormFillingDialogCollectIn %>:</span>&nbsp;<a id="formFillingFolder" class="link dotline"></a>
                    <div id="formFillingFolderSelectorContainer" class="studio-action-panel webkit-scrollbar">
                        <asp:PlaceHolder ID="FolderSelectorHolder" runat="server"></asp:PlaceHolder>
                    </div>
                </div>
                <div class="bottom-indent-24">
                    <div class="bottom-indent-4">
                        <input type="checkbox" id="formFillingCreateSubfolderCbx" />
                        <label for="formFillingCreateSubfolderCbx" class="single-form"><%= FilesUCResource.FormFillingDialogCreateSubfolder %>:</label>
                        <label for="formFillingCreateSubfolderCbx" class="multiple-form"><%= FilesUCResource.FormFillingDialogCreateUniqueSubfolder %></label>
                    </div>
                    <div>
                        <input id="formFillingSubfolderText" type="text" class="textEdit single-form" placeholder="<%= FilesUCResource.FormFillingDialogCreateSubfolderExample %>" maxlength="100"/>
                        <i class="gray-text multiple-form"><%= FilesUCResource.FormFillingDialogCreateUniqueSubfolderDscr %></i>
                    </div>
                </div>
                <div class="bottom-indent-8 header-base-small"><%= FilesUCResource.FormFillingDialogTitleSetting %>:</div>
                <div class="bottom-indent-4">
                    <input type="radio" id="formFillingDefaultTitleRadio" name="formFillingTitle" checked="checked"/>
                    <label for="formFillingDefaultTitleRadio" class="header-base-small"><%= FilesUCResource.FormFillingDialogByDefault %>:</label>
                </div>
                <div class="bottom-indent-8">
                    <i class="gray-text"><%= FilesUCResource.FormFillingDialogDefaultTitleFormat %></i>
                </div>
                <div class="bottom-indent-4">
                    <input type="radio" id="formFillingCustomTitleRadio" name="formFillingTitle"/>
                    <label for="formFillingCustomTitleRadio" class="header-base-small"><%= FilesUCResource.FormFillingDialogCustomize %>:</label>
                </div>
                <div class="bottom-indent-8">
                    <% string text = ASC.Files.Core.EntryProperties.FormFillingProperties.DefaultTitleMask; %>
                    <input id="formFillingTitleText" type="text" class="textEdit" value="<%= text %>" placeholder="<%= text %>" maxlength="100"/>
                </div>
                <div id="formFillingTitleMask">
                    <a class="link dotline plus" data-val="{0}"><%= FilesUCResource.FormFillingDialogFileName %></a>&nbsp;
                    <a class="link dotline plus" data-val="{1}"><%= FilesUCResource.FormFillingDialogUserName %></a>&nbsp;
                    <a class="link dotline plus" data-val="{2}"><%= FilesUCResource.FormFillingDialogDate %></a>
                </div>
            </div>

            <div class="small-button-container">
                <a id="formFillingSaveBtn" class="button middle blue"><%= Resource.SaveButton %></a>
                <span class="splitter-buttons"></span>
                <a id="formFillingCancelBtn" class="button middle gray"><%= Resource.CancelButton %></a>
            </div>
        </div>
    </div>
</div>