/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


window.ASC.Files.MediaPlayer = (function () {
    var isInit = false;
    var audio = 1;
    var video = 2;

    var imageRef;
    var isImage;

    var isView = false;
    var lockMediaChange = false;
    
    var headerRef;

    var imageToolbox;

    var playerRef;
    var playerControls;
    var playerProgress;
    var playerVolume;
    var playerProgressSeek;
    var playerControlsBtns;
    var playerHeaderHeight;
    var playerControlsHeight;
    var playerLoading;
    var playerNoSolution;

    var isSeeking = false;
    var isChangingVolume = false;
    var barPos;
    var barLength;
    var pipMode = false;
    var pipModeExit;

    var scrollVolumeStep = 0.05;

    var createOverlay;
    var overlayRef;

    var currentFileId;
    var playlist = [];
    var playlistPos = -1;

    var mapSupplied = {
        ".aac": { supply: "m4a", type: audio },
        ".flac": { supply: "mp3", type: audio },
        ".m4a": { supply: "m4a", type: audio },
        ".mp3": { supply: "mp3", type: audio },
        ".oga": { supply: "oga", type: audio },
        ".ogg": { supply: "oga", type: audio },
        ".wav": { supply: "wav", type: audio },

        ".f4v": { supply: "m4v", type: video },
        ".m4v": { supply: "m4v", type: video },
        ".mov": { supply: "m4v", type: video },
        ".mp4": { supply: "m4v", type: video },
        ".ogv": { supply: "ogv", type: video },
        ".webm": { supply: "webmv", type: video },
        ".wmv": { supply: "m4v", type: video, convertable: true },
        ".avi": { supply: "m4v", type: video, convertable: true },
        ".mpeg": { supply: "m4v", type: video, convertable: true },
        ".mpg": { supply: "m4v", type: video, convertable: true }
    };

    var currentFolderId;
    var options;

    var init = function (fileId, optionsParam) {
        if (isInit === false) {
            isInit = true;

            jq("[id=mediaPlayer]").slice(1).remove();

            if (ASC.Files.ServiceManager) {
                ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetMediaFile, onGetMediaFile);
                ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetSiblingsImage, onGetSiblingsImage);
            }

            headerRef = jq(".jp-title");

            imageToolbox = jq("#imageToolbox");

            playerRef = jq("#filesMediaPlayer");
            playerControls = jq(".jp-type-single");
            playerProgress = jq(".jp-progress");
            playerVolume = jq(".jp-volume-bar")
            playerProgressSeek = playerProgress.children();
            playerControlsBtns = jq(".jp-controls .videoControlBtn");
            playerHeaderHeight = jq(".jp-details").height() + parseInt(jq(".jp-details").css("padding-top")) * 2; 
            playerControlsHeight = jq(".jp-controls").height() +
                jq("#mediaViewerToolbox").height() + parseInt(jq("#mediaViewerToolbox").css("padding-top")) * 2;
            playerLoading = jq(".jp-loading");
            playerNoSolution = jq(".jp-no-solution");

            jq("#mediaPlayerClose").click(closePlayer);

            jq("#videoViewerOverlay").click(closePlayer);

            playerRef.click(function () { jq(".jp-play").click(); });

            playerProgressSeek.bind("mousedown.mediaPlayer", function (e) {
                isSeeking = true;
                barPos = e.clientX - e.offsetX;
                barLength = jq(this).width();
            });
            playerVolume.bind("mousedown.mediaPlayer", function (e) {
                isChangingVolume = true;
                barPos = e.clientY - e.offsetY;
                barLength = jq(this).height();
                playerVolume.parent().addClass("isChanging");
            });

            jq(window).bind("mouseup.mediaPlayer", function (e) {
                if (isSeeking) {
                    isSeeking = false;
                    var perc = calcBarPerc(e.clientX);
                    playerRef.jPlayer("playHead", perc * 100);
                }
                if (isChangingVolume) {
                    isChangingVolume = false;
                    var perc = calcBarPerc(e.clientY);
                    playerRef.jPlayer("volume", 1 - perc);
                    playerVolume.parent().removeClass("isChanging");
                }
            });

            jq(window).bind("mousemove.mediaPlayer", function (e) {
                if (isSeeking) {
                    var perc = calcBarPerc(e.clientX);
                    playerProgressSeek.children().width(perc * 100 + "%");
                }
                if (isChangingVolume) {
                    var perc = calcBarPerc(e.clientY);
                    playerRef.jPlayer("volume", 1 - perc);
                }
            });

            jq("#videoDelete").click(deleteFile);

            jq("#videoDownload").click(function () {
                location.href = options.downloadAction(currentFileId);
            });

            jq("#mediaPrev").click(function () {
                if (playlist.length > 1)
                    prevMedia();
            });
            jq("#mediaNext").click(function () {
                if (playlist.length > 1)
                    nextMedia();
            });
        }

        if (!optionsParam) {
            return;
        }

        options = optionsParam;

        if (!imageRef) {
            imageRef = ASC.Files.ImageViewer.init(options, headerRef, playerHeaderHeight, playerControlsHeight - jq(".jp-controls").height(),
                function () { playerLoading.hide(); },
                function (next) {
                    if (next) {
                        nextMedia();
                    } else {
                        prevMedia();
                    }
                }, showError,
                function (lock) {
                    lockMediaChange = lock;
                });
        } else {
            imageRef.setOptions(options);
        }        

        if (options.playlist) {
            playlist = options.playlist;
            playlistPos = options.playlistPos || 0;
            openPlayer(playlist[playlistPos].id, playlist[playlistPos].title);
            return;
        }

        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        if (fileObj == null || !fileObj.length) {
            ASC.Files.ServiceManager.getFile(ASC.Files.ServiceManager.events.GetMediaFile,
                {
                    dataType: "json",
                    fileId: fileId,
                });
        } else {
            currentFolderId = ASC.Files.Folders.currentFolder.id;
            var fileData = ASC.Files.UI.getObjectData(fileObj);
            openPlayer(fileId, fileData.title);
            getSiblings(fileId);
        }
    };

    var getSiblings = function (fileId) {
        var filterSettings = ASC.Files.Filter.getFilterSettings();

        if (ASC.Files.Common.isCorrectId(currentFolderId)) {
            ASC.Files.ServiceManager.getSiblingsImage(ASC.Files.ServiceManager.events.GetSiblingsImage,
            {
                fileId: fileId,
                folderId: currentFolderId,
                filter: filterSettings.filter,
                subjectGroup: filterSettings.subjectGroup,
                subjectId: filterSettings.subject,
                search: filterSettings.text,
                orderBy: filterSettings.sorter,
                withSubfolders: filterSettings.withSubfolders,
                searchTitleOnly: filterSettings.searchTitleOnly
            },
            { orderBy: filterSettings.sorter });
        }
    };

    var getPlayHash = function (fileId) {
        return "preview/" + fileId;
    };

    var canPlay = function (fileTitle, allowConvert) {
        var ext = fileTitle[0] === "." ? fileTitle : ASC.Files.Utility.GetFileExtension(fileTitle);

        var supply = mapSupplied[ext];

        var canConv = allowConvert || (options && options.allowConvert);

        return !!supply && jq.inArray(ext, ASC.Files.Utility.Resource.ExtsMediaPreviewed) != -1
            && (!supply.convertable || canConv);
    };

    var canViewImage = function (fileTitle) {
        return ASC.Files.Utility.CanImageView(fileTitle);
    };

    var calcBarPerc = function(pos) {
        var perc = (pos - barPos) / barLength;
        if (perc < 0) perc = 0;
        if (perc > 1) perc = 1;
        return perc;
    }

    var keyCodes = { enter: 13, esc: 27, spaceBar: 32, left: 37, up: 38, right: 39, down: 40, deleteKey: 46, S: 83 };
    var keyDownEvent = function (e) {
        if (jq(".blockUI:visible").length) return true;

        var key = e.keyCode || e.which;

        switch (key) {
            case keyCodes.left:
                if (playlist.length > 1)
                    prevMedia();
                return false;

            case keyCodes.right:
                if (playlist.length > 1)
                    nextMedia();
                return false;

            case keyCodes.up:
                if (!isImage) {
                    changeVolume(scrollVolumeStep);
                } else {
                    imageRef.zoomImage(true);
                }
                return false;

            case keyCodes.down:
                if (!isImage) {
                    changeVolume(-scrollVolumeStep);
                } else {
                    imageRef.zoomImage(false);
                }
                return false;

            case keyCodes.spaceBar:
            case keyCodes.enter:
                if (!isImage) {
                    jq(".jp-play").click();
                }
                return false;

            case keyCodes.esc:
                closePlayer();
                return false;

            case keyCodes.deleteKey:
                deleteFile();
                return false;

            case keyCodes.S:
                if (e.ctrlKey) {
                    jq("#videoDownload").click();
                    return false;
                }
        }
        return true;
    };

    var changeVolume = function(value) {
        var currentVolume = playerRef.jPlayer("option", "volume")
        currentVolume += value;

        if (currentVolume > 1) currentVolume = 1;
        if (currentVolume < 0) currentVolume = 0;

        playerRef.jPlayer("volume", currentVolume);
    }

    var formMedia = function (url, supply, title) {
        var media = {};
        media[supply] = url;
        media.title = title;
        return media;
    }

    var openPlayer = function (fileId, fileTitle) {
        playerRef.hide();
        jq("#mediaPlayer").show();
        imageToolbox.hide();
        imageRef.closeImageViewer();
        playerControls.hide();
        ASC.Files.MediaPlayer.isView = true;

        hideError();

        jq("html").addClass("scroll-fix");
        jq("#mediaPrev,#mediaNext").toggleClass("inactive", playlist.length < 2);

        var url = options.downloadAction(fileId);

        var ext = ASC.Files.Utility.GetFileExtension(fileTitle) ? ASC.Files.Utility.GetFileExtension(fileTitle) : ASC.Files.Utility.GetFileExtension(url);

        if (!canPlay(ext) && !canViewImage(ext)) {
            location.href = url;
            return;
        }

        currentFileId = fileId;
       
        if (canViewImage(ext)) {
            isImage = true;
        } else {
            isImage = false;
            var supply = mapSupplied[ext].supply;
        }

        if (options.onMediaChangedAction) {
            options.onMediaChangedAction(fileId);
        }

        if (options.deleteAction && options.canDelete && options.canDelete(currentFileId)) {
            jq("#videoDelete").show();
        } else {
            jq("#videoDelete").hide();
        }

        if (!isImage) {
            playerRef.show();
            jq(document).unbind("mousewheel.mediaPlayer");
            jq(document).bind("mousewheel.mediaPlayer", function (event, delta) {
                changeVolume(delta > 0 ? scrollVolumeStep : -scrollVolumeStep)
            });
        }
        
        jq(document).unbind("keydown.mediaPlayer");
        jq(document).bind("keydown.mediaPlayer", keyDownEvent);

        if (isImage
            || (mapSupplied[ext].type == audio && playerRef.children("video").length)
            || (mapSupplied[ext].type == video && playerRef.children("audio").length)) {
            playerRef.jPlayer("destroy");
        }

        if (isImage || mapSupplied[ext].type == audio) {
            exitPipMode();
        }

        if (!isImage && mapSupplied[ext].convertable && !url.includes("#")) {
            url += (url.includes("?") ? "&" : "?") + "convpreview=true";
        }

        if (isImage) {
            playerLoading.show();
            imageRef.showImage(ext, fileId, fileTitle, playerLoading.hide);
            headerRef.text(fileTitle);
            imageToolbox.show();
        } else {
            if (playerRef.children("video, audio").length) {
                var media = formMedia(url, supply, fileTitle);
                if (mapSupplied[ext].type == audio) media.poster = "/UserControls/Common/MediaViewer/Images/volume.svg";
                playerRef.jPlayer('setMedia', media);
            } else {
                playerRef.jPlayer({
                    autoBlur: false,
                    keyEnabled: true,
                    remainingDuration: true,
                    smoothPlayBar: true,
                    solution: "html, flash",
                    supplied: mapSupplied[ext].type == audio ? "m4a, mp3, m4a, oga, wav" : "m4v, ogv, webmv",
                    toggleDuration: true,
                    useStateClassSkin: true,
                    wmode: "window",
                    swfPath: "./jquery.jplayer.swf",
                    verticalVolume: true,
                    size: {
                        width: 0,
                        height: 0
                    },
                    sizeFull: {
                        width: "100%",
                        height: "100%"
                    },

                    timeupdate: function (e) {
                        if (isSeeking) return;

                        playerProgressSeek.children().width(e.jPlayer.status.currentPercentAbsolute + "%");
                    },
                    loadeddata: function () {
                        positionControls();
                        playerLoading.hide();
                        jq(window).unbind("resize.mediaPlayer");
                        jq(window).bind("resize.mediaPlayer", positionControls);
                        jq(document).unbind("webkitfullscreenchange.mediaPlayer");
                        jq(document).unbind("fullscreenchange.mediaPlayer");
                        jq(document).bind("webkitfullscreenchange.mediaPlayer", positionControls);
                        jq(document).bind("fullscreenchange.mediaPlayer", positionControls);

                        var vid = playerRef.children("video");

                        vid.bind('enterpictureinpicture.vjp', function (event) {
                            pipMode = true;
                        });
                        vid.bind('leavepictureinpicture.vjp', function (event) {
                            pipMode = false;
                            pipModeExit = new Date();
                        });
                    },
                    ready: function () {
                        var media = formMedia(url, supply, fileTitle);

                        if (mapSupplied[ext].type == audio) {
                            media.poster = "/UserControls/Common/MediaViewer/Images/volume.svg";
                        } else {
                            playerRef.dblclick(function () {
                                playerRef.jPlayer("option", "fullScreen", !playerRef.jPlayer("option", "fullScreen"));
                            });

                            createOverlay = true;
                        }

                        jq(this).jPlayer("setMedia", media);
                        playerLoading.show();
                    },
                    error: function (e) {
                        showError(e.jPlayer.error.message);
                    },
                    pause: function (e) {
                        // video stops playing when user exits PiP mode
                        // ToDo: better way to detect exit from PiP
                        if (!pipModeExit || new Date() - pipModeExit > 150) return;

                        playerRef.jPlayer("play");
                    }
                });
            }
        }
    };

    var hideError = function () {
        playerNoSolution.hide().children().first().show();
        playerNoSolution.children().last().hide();
    };

    var showError = function (message) {
        playerNoSolution.show().children().first().hide();
        playerNoSolution.children().last().text(message).show();
        playerLoading.hide();
    };

    var resizePlayer = function (videoSize, screenSize) {
        var ratio = videoSize.h / videoSize.w;

        if (videoSize.h > screenSize.h) {
            videoSize.h = screenSize.h;
            videoSize.w = videoSize.h / ratio;
        }
        if (videoSize.w > screenSize.w) {
            videoSize.w = screenSize.w;
            videoSize.h = videoSize.w * ratio;
        }

        var newSize = {
            width: videoSize.w,
            height: videoSize.h
        };

        playerRef.jPlayer("option", "size", newSize);
        if (playerRef.children("video, object").length)
            playerRef.children("video, object").css(newSize);
    };

    var positionControls = function () {
        var screenSize = {
            w: jq(window).width(),
            h: jq(window).height()
        };

        if (!playerRef.parent().hasClass("jp-video-full")) {
            screenSize.h -= playerHeaderHeight + playerControlsHeight;

            var centerAreaOx = screenSize.w / 2 + jq(window).scrollLeft();
            var centerAreaOy = screenSize.h / 2 + jq(window).scrollTop();

            var videoSize = {};
            if (playerRef.children("video").length) {
                videoSize = {
                    h: playerRef.children("video")[0].videoHeight || 270,
                    w: playerRef.children("video")[0].videoWidth || 480
                };
            } else {
                if (playerRef.children("audio").length > 0)
                    videoSize = {
                        h: 0,
                        w: screenSize.w * 0.8
                    };
                else
                    videoSize = {
                        h: 270,
                        w: 480
                    };
            }

            resizePlayer(videoSize, screenSize);

            playerRef.css({
                left: centerAreaOx - videoSize.w / 2,
                top: centerAreaOy - videoSize.h / 2 + playerHeaderHeight
            });

            playerControls.css({
                left: centerAreaOx - videoSize.w / 2,
                top: centerAreaOy + videoSize.h / 2 + playerHeaderHeight
            });
        } else {
            var videoSize = screenSize;

            resizePlayer(videoSize, screenSize);

            playerRef.css({
                left: 0,
                top: 0
            });

            playerControls.css({
                left: 0,
                top: screenSize.h - 40
            });
        }

        if (createOverlay) {
            createOverlay = false;
            playerRef.append("<div id=\"videoOverlay\" title=\"" + ASC.Resources.Master.UserControlsCommonResource.MediaViewerPlay + "\"></div>");
            overlayRef = jq("#videoOverlay").click(function () { playerRef.jPlayer("play"); });
        }
        if (overlayRef)
            overlayRef.css({
                top: (videoSize.h - 60) / 2,
                left: (videoSize.w - 80) / 2
            });

        playerControls.show();

        var availableW = parseInt(playerControlsBtns.first().css("margin-left")) * 2 * (playerControlsBtns.length + 1);
        playerControlsBtns.each(function () { availableW += jq(this).width(); });
        playerProgress.width(videoSize.w - availableW);
    };

    var closePlayer = function () {
        playerRef.hide();
        imageRef.closeImageViewer();
        playerRef.jPlayer("destroy");
        ASC.Files.MediaPlayer.isView = false;

        playlist = [];
        playlistPos = -1;
        createOverlay = false;

        hideError();

        jq(window).unbind("resize.mediaPlayer");
        jq(document).unbind("webkitfullscreenchange.mediaPlayer");
        jq(document).unbind("fullscreenchange.mediaPlayer");
        jq(document).unbind("mousewheel.mediaPlayer");
        jq(document).unbind("keydown.mediaPlayer");
        jq("html").removeClass("scroll-fix");

        exitPipMode();

        playerControls.hide();
        jq("#mediaPlayer").hide();

        if (options.onCloseAction) {
            options.onCloseAction(currentFolderId);
        }
    };

    var exitPipMode = function () {
        if (pipMode) {
            pipMode = false;
            document.exitPictureInPicture();
        }
    };

    var nextMedia = function (openCurrent) {
        if (lockMediaChange) return;

        if (!openCurrent) {
            playlistPos = (playlistPos + 1) % playlist.length;
        }

        openPlayer(playlist[playlistPos].id, playlist[playlistPos].title);
    };

    var prevMedia = function () {
        if (lockMediaChange) return;

        playlistPos--;
        if (playlistPos < 0)
            playlistPos = playlist.length - 1;

        openPlayer(playlist[playlistPos].id, playlist[playlistPos].title);
    };

    var deleteFile = function () {
        if (!options.canDelete || !options.canDelete(currentFileId) || !options.deleteAction) {
            return;
        }

        var successfulDeletion = function () {
            playlist.splice(playlistPos, 1);

            if (playlist.length > 0) {
                if (playlist.length == playlistPos) {
                    playlistPos = 0;
                }
                nextMedia(true);
            } else {
                closePlayer();
            }
        };

        options.deleteAction(currentFileId, successfulDeletion);
    };

    var hideLoading = function () {
        LoadingBanner.hideLoading();
        jq(".mainPageContent").children(".loader-page").remove();
    };

    //event handler

    var onGetMediaFile = function (jsonData, params, errorMessage) {
        hideLoading();

        if (errorMessage || !jsonData) {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            //var url = ASC.Files.Utility.GetFileDownloadUrl(params.fileId);
            //location.href = url;
            ASC.Files.Anchor.defaultFolderSet();
            return;
        }

        currentFolderId = jsonData.folder_id;

        openPlayer(jsonData.id, jsonData.title);
        getSiblings(jsonData.id);
    };

    var onGetSiblingsImage = function (jsonData, params, errorMessage) {
        hideLoading();

        playlist = [];

        if (errorMessage || !jsonData || !jsonData.length) {
            jq("#mediaPrev,#mediaNext").toggleClass("inactive", !playlist.length);
            return;
        }

        for (var i = 0; i < jsonData.length; i++) {
            if (canPlay(jsonData[i].title) || canViewImage(jsonData[i].title)) {
                playlist.push({ id: jsonData[i].id, title: jsonData[i].title });

                if (jsonData[i].id == currentFileId) {
                    playlistPos = playlist.length - 1;
                    currentFolderId = jsonData[i].folder_id;
                }
            }
        }

        if (playlist.length == 1) {
            playlist = [];
            playlistPos = -1;
        }

        jq("#mediaPrev,#mediaNext").toggleClass("inactive", !playlist.length);
        return;
    };

    return {
        init: init,
        getPlayHash: getPlayHash,

        isView: isView,

        canPlay: canPlay,
        canViewImage: canViewImage,
        closePlayer: closePlayer,
    };
})();

(function ($) {
    if (jq("#filesMediaPlayer").length == 0)
        return;

    $(function () {
    });
})(jQuery);