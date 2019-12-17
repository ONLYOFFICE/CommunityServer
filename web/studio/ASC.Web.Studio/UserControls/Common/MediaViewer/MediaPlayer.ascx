<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MediaPlayer.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.MediaPlayer" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.UserControls.Common" Assembly="ASC.Web.Studio" %>
<%@ Import Namespace="Resources" %>


<div id="mediaPlayer">
    <img id="imageViewerContainer" src="" alt="" height="10px" width="10px" />

    <div id="imageViewerInfo">
        <span>100%</span>
    </div>

    <div id="videoViewerOverlay"></div>
    <div id="jp_container_1" class="jp-audio" role="application" aria-label="media player">
        <div id="filesMediaPlayer" class="jp-jplayer">
        </div>
        <div class="jp-details">
            <div id="mediaPlayerClose" class="videoControlBtn" title="<%= UserControlsCommonResource.MediaViewerClose %>"></div>
            <div class="jp-title" aria-label="title">&nbsp;</div>
        </div>
        <div class="jp-no-solution infoblock">
            <span><%= UserControlsCommonResource.NotSupportedBrowser %></span>
            <span class="jp-error"></span>
        </div>
        <div class="jp-loading infoblock">
            <span><%= Resource.LoadingProcessing %></span>
        </div>
        <div class="jp-type-single">
            <div class="jp-gui jp-interface">
                <div class="jp-controls">
                    <div class="jp-play videoControl videoControlBtn" role="button" tabindex="0" title="<%= UserControlsCommonResource.MediaViewerPlay %>"></div>
                    <div class="jp-progress videoControl">
                        <div class="jp-seek">
                            <div class="jp-pos">
                                <span></span>
                            </div>
                        </div>
                    </div>
                    <div class="jp-duration videoControl videoControlBtn" role="timer" aria-label="duration">&nbsp;</div>
                    <div class="jp-volume-holder videoControl">
                        <div class="jp-mute videoControl videoControlBtn" role="button" tabindex="0" title="<%= UserControlsCommonResource.MediaViewerMute %>"></div>
                        <div class="jp-volume-wrapper">
                            <div class="jp-volume-bar">
                                <div class="jp-volume-bar-value">
                                    <span></span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="jp-full-screen videoControl videoControlBtn" role="button" tabindex="0" title="<%= UserControlsCommonResource.MediaViewerFullscreen %>"></div>
                </div>
            </div>
        </div>
        <div id="mediaScroll">
            <div id="mediaPrev" class="videoControlBtn" title="<%= UserControlsCommonResource.MediaViewerPrevious %>"></div>
            <div id="mediaNext" class="videoControlBtn" title="<%= UserControlsCommonResource.MediaViewerNext %>"></div>
        </div>
        <div id="mediaViewerToolbox">
            <span id="imageToolbox">
                <div id="imageZoomIn" class="videoControlBtn" title="<%= UserControlsCommonResource.MediaViewerZoomIn %>"></div>
                <div id="imageZoomOut" class="videoControlBtn" title="<%= UserControlsCommonResource.MediaViewerZoomOut %>"></div>
                <div id="imageFullScale" class="videoControlBtn" title="<%= UserControlsCommonResource.MediaViewerActualSize %>"></div>
                <div id="imageRotateLeft" class="videoControlBtn" title="<%= UserControlsCommonResource.MediaViewerRotateLeft %>"></div>
                <div id="imageRotateRight" class="videoControlBtn" title="<%= UserControlsCommonResource.MediaViewerRotateRight %>"></div>
            </span>
            <span>
                <div id="videoDownload" class="videoControlBtn" title="<%= UserControlsCommonResource.DownloadFile %>"></div>
                <div id="videoDelete" class="videoControlBtn" title="<%= UserControlsCommonResource.DeleteFile %>"></div>
            </span>
        </div>
    </div>
</div>
