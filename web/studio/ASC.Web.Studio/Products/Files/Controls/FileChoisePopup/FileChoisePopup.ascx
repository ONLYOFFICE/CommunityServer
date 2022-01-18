<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileChoisePopup.ascx.cs" Inherits="ASC.Web.Files.Controls.FileChoisePopup" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="fileChoisePopupDialod">
    <sc:Container ID="DocumentUploader" runat="server">
        <Header>
            <%= FilesUCResource.CreateFormTemplateFromFile %>
        </Header>
        <Body>
            <asp:PlaceHolder ID="LoaderHolder" runat="server"></asp:PlaceHolder>
            <div id="frameContainer" data-frame="<%= FrameUrl %>"></div>
        </Body>
    </sc:Container>
</div>
