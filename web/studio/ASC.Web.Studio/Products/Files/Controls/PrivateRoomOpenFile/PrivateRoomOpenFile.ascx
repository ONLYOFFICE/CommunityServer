<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PrivateRoomOpenFile.ascx.cs" Inherits="ASC.Web.Files.Controls.PrivateRoomOpenFile" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<div class="private-room-open-document-wrapper">
    <div class="private-room-open-document-logo">
        <a class="" href="/" title="<%: LogoText %>">
            <img src="<%= LogoPath %>" alt="" />
        </a>
    </div>
    <div class="private-room-open-document-content">
        <div class="private-room-open-document-content-header">
            <h1>
                <%= FilesCommonResource.PvOpenDocumentTitle %>
            </h1>
            <div class="private-room-open-document-content-header-text">
                <%= String.Format(FilesCommonResource.PvOpenDocumentTextBlock.HtmlEncode(), "<strong>", "</strong>", "<br />") %>
            </div>
        </div>
        <div class="private-room-open-document-content-body">
            <div class="private-room-open-document-content-body-text1">
                <%= FilesCommonResource.PvOpenAppTextBlock %>
            </div>
            <div class="private-room-open-document-content-body-button1">
                <a id="dialogBoxCustomProtocol" class="button blue big" href="<%= EditorUrl %>">
                    <%: FilesCommonResource.PvOpenAppButton %>
                </a>
            </div>
            <div class="private-room-open-document-content-body-hr"></div>
            <div class="private-room-open-document-content-body-text2">
                <%: FilesCommonResource.PvDesktopEditorsNotInstalledTextBlock %>
                    <a href="<%= ASC.Web.Studio.Core.SetupInfo.DownloadForDesktopUrl %>" target="_blank">
                        <%= FilesCommonResource.PvDownloadNowButton %>
                    </a>
            </div>
            <div class="private-room-open-document-content-body-text3">
                <%= String.Format(FilesCommonResource.PvFileBlockingFileOpeningTextBlock.HtmlEncode(), "<br />") %>
            </div>
        </div>
    </div>
</div>
