<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocumentsPopup.ascx.cs" Inherits="ASC.Web.Mail.Controls.DocumentsPopup" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<sc:Container id="_documentUploader" runat="server">
        <header>
        <%= PopupName %>
        </header>
        <body>
            <table>
                <tbody>
                    <tr>
                        <td style="vertical-align: top; max-width: 220px;">
                            <asp:PlaceHolder runat="server" ID="TreeHolder"></asp:PlaceHolder>
                        </td>
                        <td style="width:10px; border-left: solid 1px #D1D1D1;" />
                        <td style="max-width: 380px; width: 416px;">
                            <div class="fileContainer">
                                <img class="loader" src="<%= LoaderImgSrc %>"/>
                                <div id="emptyFileList" class="display-none">
                                    <asp:PlaceHolder runat="server" ID="_phEmptyDocView"></asp:PlaceHolder>
                                </div>
                                <div id="filesViewContainer">
                                    <ul class='fileList'>
                                    </ul>
                                </div>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
            <div class="buttonContainer" style="padding-top: 20px;">
                <button id="attach_btn" class="button middle blue disable" type="button"><%= AttachFilesButtonText %></button>
                <span class="splitter-buttons"></span>
                <button id="cancel_btn" class="button middle gray" type="button"><%= CancelButtonText %></button>
            </div>
        </body>
</sc:Container>