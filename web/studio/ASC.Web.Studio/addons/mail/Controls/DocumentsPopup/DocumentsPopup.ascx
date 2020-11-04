<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocumentsPopup.ascx.cs" Inherits="ASC.Web.Mail.Controls.DocumentsPopup" %>

<%@ Import Namespace="ASC.Web.Mail.Resources" %>
<%@ Import Namespace="Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<sc:Container ID="_documentUploader" runat="server">
    <Header>
        <%= UserControlsCommonResource.AttachFromDocuments %>
    </Header>
    <Body>
        <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
        <label id="attachFilesAsLinks" for="attachFilesAsLinksSelector" class="checkbox">
            <input id="attachFilesAsLinksSelector" type="checkbox" checked="checked" />
            <%= MailResource.AttachFilesAsLinksLabel %>
        </label>
        <div id="attachFrame" data-frame="<%= FrameUrl %>"></div>
    </Body>
</sc:Container>
