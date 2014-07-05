<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileViewer.ascx.cs" Inherits="ASC.Web.Files.Controls.FileViewer" %>
<%@ Import Namespace="ASC.Web.Core.Mobile" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<div id="fileViewerDialog">
    <img id="imageViewerContainer" src="" alt="" height="10px" width="10px" />
    <img id="imageViewerPreload" src="" alt="" />

    <div id="imageViewerInfo">
        <span>100%</span>
    </div>

    <div id="imageViewerToolbox">
        <div class="image-info describe-text">
            &nbsp;
        </div>
        <div class="toolbox-wrapper">
            <ul>
                <li>
                    <a id="imageZoomIn" title="<%= FilesUCResource.ButtonZoomIn %>"></a>
                </li>
                <li>
                    <a id="imageZoomOut" title="<%= FilesUCResource.ButtonZoomOut %>"></a>
                </li>
                <li>
                    <a id="imageFullScale" title="<%= FilesUCResource.ButtonFullScale %>"></a>
                </li>
                <li class="action-block-wrapper"></li>
                <li>
                    <a id="imagePrev" title="<%= FilesUCResource.ButtonPrevImg %>"></a>
                </li>
                <li>
                    <a id="imageNext" title="<%= FilesUCResource.ButtonNextImg %>"></a>
                </li>
                <li class="action-block-wrapper"></li>
                <li>
                    <a id="imageRotateLeft" title="<%= FilesUCResource.ButtonRotateLeft %>"></a>
                </li>
                <li>
                    <a id="imageRotateRight" title="<%= FilesUCResource.ButtonRotateRight %>"></a>
                </li>
                <li>
                    <a id="viewerOtherActionsSwitch" title="<%= FilesUCResource.ButtonOtherAction %>"></a>
                </li>
            </ul>
        </div>
    </div>
    <div id="viewerOtherActions">
        <ul>
            <% if (!MobileDetector.IsMobile)
               { %>
            <li>
                <a id="imageDownload" class="action-download" title="<%= FilesUCResource.ButtonDownload %>">
                    <%= FilesUCResource.ButtonDownload %>
                </a>
            </li>
            <% } %>
            <li>
                <a id="imageDelete" class="action-delete" title="<%= FilesUCResource.ButtonDelete %>">
                    <%= FilesUCResource.ButtonDelete %>
                </a>
            </li>
        </ul>
    </div>

    <div id="imageViewerClose" title="<%= FilesUCResource.ButtonClose %>"></div>
</div>
