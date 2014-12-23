<%@ Assembly Name="ASC.Web.Files" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileSelector.ascx.cs" Inherits="ASC.Web.Files.Controls.FileSelector" %>

<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="fileSelectorDialog" class="popup-modal">
    <sc:Container runat="server" ID="FileSelectorTemp">
        <header><span id="fileSelectorTitle"><%= DialogTitle %></span></header>
        <body>
            <table cellspacing="0" cellpadding="0">
                <tr>
                    <td class="file-selector-tree">
                        <asp:PlaceHolder runat="server" ID="TreeHolder"></asp:PlaceHolder>
                    </td>
                    <td class="file-selector-files borderBase">
                        <asp:PlaceHolder runat="server" ID="ContentHolder"></asp:PlaceHolder>
                    </td>
                </tr>
            </table>
            <div class="middle-button-container">
                <a id="submitFileSelector" class="button blue middle disable">
                    <%= FilesUCResource.ButtonOk %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();return false;">
                    <%= FilesUCResource.ButtonCancel %>
                </a>
                <span class="splitter-buttons"></span>
                <span id="fileSelectorAdditionalButton"></span>
            </div>
        </body>
    </sc:Container>
</div>
