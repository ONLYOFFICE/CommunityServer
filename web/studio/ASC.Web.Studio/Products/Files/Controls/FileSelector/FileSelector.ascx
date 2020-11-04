<%@ Assembly Name="ASC.Web.Files" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileSelector.ascx.cs" Inherits="ASC.Web.Files.Controls.FileSelector" %>

<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="fileSelectorDialog" <%= IsFlat ? "" : "class=\"popup-modal\"" %>>
    <sc:Container runat="server" ID="FileSelectorTemp">
        <Header>
            <% if (!IsFlat)
               { %>
            <span id="fileSelectorTitle"><%= DialogTitle %></span>
            <% } %>
        </Header>

        <Body>
            <table cellspacing="0" cellpadding="0">
                <tr>
                    <td class="file-selector-tree">
                        <asp:PlaceHolder runat="server" ID="TreeHolder"></asp:PlaceHolder>
                    </td>
                    <td class="file-selector-files borderBase <%= Multiple ? "" : "file-selector-single" %>">
                        <asp:PlaceHolder runat="server" ID="ContentHolder"></asp:PlaceHolder>
                    </td>
                </tr>
            </table>
            <div class="middle-button-container">
                <a id="submitFileSelector" class="button blue middle disable">
                    <%= SuccessButton %>
                </a>
                <span class="splitter-buttons"></span>
                <a id="closeFileSelector" class="button gray middle">
                    <%= FilesUCResource.ButtonCancel %>
                </a>
                <span class="splitter-buttons"></span>
                <span id="fileSelectorAdditionalButton"></span>
            </div>
        </Body>
    </sc:Container>
</div>
