<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmConvert.ascx.cs" Inherits="ASC.Web.Files.Controls.ConfirmConvert" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="confirmConvert" class="popup-modal"
    data-save="<%= FilesSettings.HideConfirmConvertSave.ToString().ToLower() %>"
    data-open="<%= FilesSettings.HideConfirmConvertOpen.ToString().ToLower() %>">
    <sc:Container runat="server" ID="confirmConvertDialog">
        <header>
            <span id="confirmConvertSave"><%= FilesUCResource.CaptionCopyConvertSave %></span>
            <span id="confirmConvertEdit"><%= FilesUCResource.CaptionCopyConvertOpen2 %></span>
        </header>
        <body>
            <div class="confirm-convert-panel">
                <span id="confirmConvertSaveDescript"><%= FilesUCResource.ConfirmStoreOriginalSaveTitle %></span>
                <span id="confirmConvertEditDescript"><%= FilesUCResource.ConfirmStoreOriginalOpenTitle.HtmlEncode() %></span>
                <br/>
                <br/>
                <label>
                    <input type="checkbox" class="store-original checkbox" <%= FilesSettings.StoreOriginalFiles ? "checked=\"checked\"" : string.Empty %> />
                    <%= FilesUCResource.ConfirmStoreOriginalOpenCbxLabelText %>
                </label>
                <br/>
                <br/>
                <label>
                    <input type="checkbox" id="confirmConvertHide" class="checkbox"  />
                    <%= FilesUCResource.ConfirmDontShow %>
                </label>
            </div>

            <div class="middle-button-container">
                <a id="confirmConvertContinue" class="button blue middle">
                    <%= Resource.ContinueButton %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= FilesUCResource.ButtonClose %>
                </a>
            </div>

        </body>
    </sc:Container>
</div>
