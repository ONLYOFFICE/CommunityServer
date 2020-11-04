<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectDocumentsPopup.ascx.cs" Inherits="ASC.Web.Projects.Controls.Common.ProjectDocumentsPopup" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<span id="portalDocUploader" class="linkAttachFile">
    <a class="baseLinkAction" ><%= UserControlsCommonResource.AttachOfProjectDocuments %></a>
</span>

<div id="popupDocumentUploader">
    <sc:Container ID="_documentUploader" runat="server">
        <Header>
            <%= UserControlsCommonResource.AttachOfProjectDocuments %>
        </Header>
        <Body>
            <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
            <div id="attachFrame" data-frame="<%= FrameUrl %>"></div>
        </Body>
    </sc:Container>
</div>
