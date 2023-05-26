<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ShareLinkPassword.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ShareLinkPassword" %>

<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Files.Utils" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>

<%
    string error = null;
    string cookieValue = null;
    bool isFolder = false;

    try
    {
        isFolder = bool.Parse(Request["folder"] ?? "false");
        var key = Request["key"];
        Tuple<ASC.Files.Core.File, ASC.Files.Core.Security.FileShareRecord> fileData = null;
        Tuple<ASC.Files.Core.Folder, ASC.Files.Core.Security.FileShareRecord> folderData = null;

        if (!isFolder)
        {
            fileData = Global.FileStorageService.ParseFileShareLinkKey(key);

            if (FileShareLink.CheckCookieOrPasswordKey(fileData.Item2, null, out cookieValue))
            {
                Response.Redirect(FileShareLink.GetLink(fileData.Item1, true, fileData.Item2.Subject));
            }
        }
        else
        {
            folderData = Global.FileStorageService.ParseFolderShareLinkKey(key);

            if (FileShareLink.CheckCookieOrPasswordKey(folderData.Item2, null, out cookieValue))
            {
                Response.Redirect(FileShareLink.GetLink(folderData.Item1, folderData.Item2.Subject));
            }
        }
    }
    catch (Exception ex)
    {
        error = ex.Message;
    }
%>

<div id="shareLinkPasswordBlock">
    <br />
    <% if(string.IsNullOrEmpty(error)) { %>
    <div class="header-base medium bold"><%= UserControlsCommonResource.ShareLinkPasswordHeader %></div>
    <div id="shareLinkPasswordSendBlock">
        <br />
        <div id="shareLinkPasswordField">
            <input id="shareLinkPasswordInput" class="pwdLoginTextbox" type="password" placeholder="<%= Resource.Password %>" value="" autocomplete="current-password" autofocus />
            <label id="shareLinkPasswordLabel" class="eye-label hide-label"></label>
            <div id="shareLinkPasswordInfo" class="gray-text"><%= !isFolder ? UserControlsCommonResource.ShareLinkPasswordLabel : UserControlsCommonResource.FolderShareLinkPasswordLabel %></div>
        </div>
        <div class="small-button-container">
            <a id="shareLinkPasswordSendBtn" class="button blue big"><%= Resource.OKButton %></a>
        </div>
    </div>
    <div id="shareLinkPasswordDownloadBlock" class="small-button-container display-none">
        <a id="shareLinkPasswordDownloadBtn" class="button blue big" href="#" target="_blank"><%= FilesUCResource.ButtonOpenFile %></a>
    </div>
    <% } else { %>
    <div class="header-base medium red-text"><%= error %></div>
    <% } %>
</div>
