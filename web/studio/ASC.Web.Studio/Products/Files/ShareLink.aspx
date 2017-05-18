<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="true" CodeBehind="ShareLink.aspx.cs" Inherits="ASC.Web.Files.ShareLink" %>

<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<%@ MasterType TypeName="ASC.Web.Files.Masters.BasicTemplate" %>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <img alt="" src="<%= GetAbsoluteCompanyTopLogoPath() %>" />

    <br />
    <input type="text" id="shareLink" class="textEdit" readonly="readonly" value="<%= Link %>" />

    <p class="share-mail-title requiredTitle"><%= FilesUCResource.ShareLinkMailTo %></p>
    <div id="shareLinkEmailSelector" class="emailselector">
        <input type="text" class="emailselector-input" autocomplete="off">
        <pre class="emailSelector-input-buffer"></pre>
    </div>

    <p class="share-mail-title"><%= FilesUCResource.ShareLinkMailMessageTitle %></p>
    <textarea id="shareMailText" class="textEdit" cols="10" rows="2" placeholder="<%= FilesUCResource.ShareLinkMailMessage %>"></textarea>
    <div class="middle-button-container">
        <a id="shareSendLinkToEmail" class="button middle gray" data-id="<%= HttpUtility.HtmlEncode(Request[FilesLinkUtility.FileId]) %>"><%= FilesUCResource.LinkViaMailSend %></a>
    </div>
</asp:Content>
