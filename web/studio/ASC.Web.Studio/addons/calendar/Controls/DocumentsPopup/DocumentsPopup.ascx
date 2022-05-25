<%@ Assembly Name="ASC.Web.Calendar" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocumentsPopup.ascx.cs" Inherits="ASC.Web.Calendar.Controls.DocumentsPopup" %>

<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<sc:Container ID="_documentUploader" runat="server">
    <Header>
        <%= UserControlsCommonResource.AttachFromDocuments %>
    </Header>
    <Body>
        <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
        <div id="attachFrame" data-frame="<%= FrameUrl %>"></div>
    </Body>
</sc:Container>
