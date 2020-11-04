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


window.ASC.Files.Marker = (function () {
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.MarkAsRead, ASC.Files.EventHandler.onGetTasksStatuses);

            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetNews, ASC.Files.Marker.onGetNews);

            jq(document).click(function (event) {
                jq.dropdownToggle().registerAutoHide(event, ".is-new", "#filesNewsPanel");
            });
        }
    };

    var markRootAsNew = function (xmlData) {
        if (typeof xmlData == "undefined" || xmlData == null) {
            return false;
        }

        jq("#studio_sidePanel .page-menu .is-new").hide();

        jq(xmlData.childNodes).each(function (i, childNode) {
            var xmlKey = childNode.childNodes[0];
            var xmlValue = childNode.childNodes[1];
            var folderId = xmlKey.textContent || xmlKey.text;
            var valueNew = xmlValue.textContent || xmlValue.text;
            if (valueNew > 0) {
                jq("#studio_sidePanel .page-menu .is-new" + ASC.Files.UI.getSelectorId(folderId)).text(valueNew).show();
            }
        });

        return true;
    };

    var removeNewIcon = function (entryType, entryId) {
        return ASC.Files.Marker.setNewCount(entryType, entryId, 0);
    };

    var setNewCount = function (entryType, entryId, countNew) {
        var result = false;

        var isUpdate = (countNew | 0) > 0;
        var newsObj = ASC.Files.UI.getEntryObject(entryType, entryId).find(".is-new");
        if (newsObj.is(":visible")) {
            var diffCount = (entryType == "file"
                ? -Math.pow(-1, countNew)
                : countNew - (newsObj.html() | 0));

            var rootId = ASC.Files.Tree.pathParts[0];
            var rootObj = jq("#studio_sidePanel .page-menu .is-new" + ASC.Files.UI.getSelectorId(rootId));
            var prevCount = rootObj.html() | 0;
            var rootCountNew = prevCount + diffCount;

            rootObj.html(rootCountNew > 0 ? rootCountNew : 0);
            if (rootCountNew > 0) {
                rootObj.show();
            } else {
                rootObj.hide();
            }

            if (isUpdate) {
                if (entryType == "folder") {
                    newsObj.html(countNew);
                }
            } else {
                newsObj.remove();
            }
            result = true;
        }

        if (entryType === "folder") {
            newsObj = jq("#studio_sidePanel .page-menu .is-new" + ASC.Files.UI.getSelectorId(entryId));
            if (newsObj.is(":visible")) {
                newsObj.html(countNew > 0 ? countNew : 0);
                if (isUpdate) {
                    newsObj.show();
                } else {
                    newsObj.hide();

                    if (jq.inArray(entryId + "", ASC.Files.Tree.pathParts) != -1) {
                        jq("#filesMainContent .is-new").remove();
                    }
                }
                result = true;
            }
        }

        return result;
    };

    //request

    var markAsRead = function (event) {
        var itemData;

        var data = {};
        data.entry = new Array();

        if (typeof event == "undefined") {
            jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):has(.checkbox input:checked)").each(function () {
                itemData = ASC.Files.UI.getObjectData(this);

                if (itemData.entryObject.find(".is-new").length > 0) {
                    data.entry.push(itemData.entryType + "_" + itemData.entryId);
                }
            });
        } else if (typeof event == "object") {
            itemData = event;

            data.entry.push(itemData.entryType + "_" + itemData.entryId);
        } else if (ASC.Files.Common.isCorrectId(event)) {
            data.entry.push("folder_" + event);
        }

        if (data.entry.length == 0) {
            return false;
        }

        ASC.Files.ServiceManager.markAsRead(
            ASC.Files.ServiceManager.events.MarkAsRead,
            { list: data.entry, doNow: true },
            { stringList: data });

        return false;
    };

    var getNews = function (target, folderId) {
        ASC.Files.Actions.hideAllActionPanels();

        if (!ASC.Files.Common.isCorrectId(folderId)) {
            var itemData = ASC.Files.UI.getObjectData(target);

            if (itemData.entryType == "file") {
                ASC.Files.Marker.markAsRead(itemData);
                return;
            }

            folderId = itemData.entryId;
        }

        //track event

        trackingGoogleAnalytics("documents", "shownews", "folder");

        var targetSize = {
            top: target.offset().top,
            left: target.offset().left,
            width: target.width(),
            height: target.height()
        };

        ASC.Files.ServiceManager.getNews(ASC.Files.ServiceManager.events.GetNews, { folderId: folderId, targetSize: targetSize, showLoading: true });
    };

    //event handler

    var onMarkAsRead = function (listData) {
        jq("#studio_sidePanel .page-menu .is-new").hide();

        for (var i = 0; i < listData.length; i++) {
            var curItem = ASC.Files.UI.parseItemId(listData[i]);
            if (curItem == null) {
                if (listData[i].indexOf("new_") == 0) {
                    var itemDataStr = listData[i].substring(("new_").length).replace(/\?/g, ":");
                    var itemData = JSON.parse(itemDataStr);
                    var folderId = itemData.key;
                    var valueNew = itemData.value;
                    if (valueNew > 0) {
                        jq("#studio_sidePanel .page-menu .is-new" + ASC.Files.UI.getSelectorId(folderId)).text(valueNew).show();
                    }
                }
                continue;
            }
            var entryType = curItem.entryType;
            var entryId = curItem.entryId;

            ASC.Files.UI.blockObjectById(curItem.entryType, curItem.entryId, false, null, true);
            ASC.Files.Marker.removeNewIcon(entryType, entryId);

            if (entryType == "folder" && jq.inArray(entryId + "", ASC.Files.Tree.pathParts) != -1) {
                jq("#filesMainContent .is-new").remove();
            }
        }

        ASC.Files.UI.updateMainContentHeader();
        ASC.Files.Actions.showActionsViewPanel();
    };

    var onGetNews = function (xmlData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var htmlData = ASC.Files.TemplateManager.translate(xmlData);

        if (htmlData.length) {
            jq("#filesNewsList").html(htmlData);

            jq("#filesNewsList .file-row").each(function () {
                var entryData = ASC.Files.UI.getObjectData(this);

                var entryId = entryData.entryId;
                var entryType = entryData.entryType;
                var entryObj = entryData.entryObject;
                var entryTitle = entryData.title;

                var ftClass = (entryType == "file" ? ASC.Files.Utility.getCssClassByFileTitle(entryTitle, true) : ASC.Files.Utility.getFolderCssClass(true));
                entryObj.find(".thumb-" + entryType).addClass(ftClass);

                var rowLink = entryObj.find(".entry-title .name a");

                if (entryType == "file" && rowLink.is(":not(:has(.file-extension))")) {
                    ASC.Files.UI.highlightExtension(rowLink, entryTitle);
                }

                var entryUrl = ASC.Files.UI.getEntryLink(entryType, entryId, entryTitle);
                rowLink.attr("href", entryUrl).attr("target", "_blank");
            });

            var targetSize = params.targetSize;
            var filesNewsPanel = jq("#filesNewsPanel");
            var margin = 8;
            var startY = targetSize.top + targetSize.height + margin;
            var correctionY = 0;
            var panelHeight = filesNewsPanel.outerHeight();
            if (document.body.clientHeight - (startY - pageYOffset + panelHeight) < 0) {
                startY = targetSize.top - margin;
                correctionY = panelHeight;
            }

            filesNewsPanel
                .css({
                    "top": startY - correctionY,
                    "left": targetSize.left
                })
                .toggle()
                .attr("data-id", params.folderId);

            var countNew = jq("#filesNewsList .file-row").length;
            ASC.Files.Marker.setNewCount("folder", params.folderId, countNew);
        } else {
            ASC.Files.Marker.removeNewIcon("folder", params.folderId);
        }
    };

    return {
        init: init,

        removeNewIcon: removeNewIcon,
        setNewCount: setNewCount,

        markAsRead: markAsRead,
        markRootAsNew: markRootAsNew,

        getNews: getNews,

        onMarkAsRead: onMarkAsRead,
        onGetNews: onGetNews
    };
})();

(function ($) {
    $(function () {
        ASC.Files.Marker.init();

        jq(document).on("click", ".is-new", function () {
            var target = jq(this);
            var folderId = target.attr("data-id");
            ASC.Files.Marker.getNews(target, folderId);
            return false;
        });

        jq("#studioPageContent").on("click", "#buttonMarkRead, #mainMarkRead.unlockAction", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Marker.markAsRead();
        });

        jq("#filesNewsMarkRead").click(function () {
            ASC.Files.Actions.hideAllActionPanels();
            var folderId = jq("#filesNewsPanel").attr("data-id");
            ASC.Files.Marker.markAsRead(folderId);
        });

        jq("#filesNewsList").on("click", ".folder-row:not(.error-entry) .entry-title .name a, .folder-row:not(.error-entry) .thumb-folder", function () {
            ASC.Files.Actions.hideAllActionPanels();
            var folderId = ASC.Files.UI.getObjectData(this).id;
            ASC.Files.Folders.clickOnFolder(folderId);
            return false;
        });

        jq("#filesNewsList").on("click", ".file-row:not(.folder-row):not(.error-entry) .entry-title .name a, .file-row:not(.folder-row):not(.error-entry) .thumb-file", function () {
            var fileData = ASC.Files.UI.getObjectData(this);
            var updated = true;

            if (ASC.Files.MediaPlayer && (ASC.Files.MediaPlayer.canPlay(fileData.title) || ASC.Files.Utility.CanImageView(fileData.title))) {
                ASC.Files.Actions.hideAllActionPanels();

                var newFiles = jq("#filesNewsList .file-row:not(.folder-row):not(.error-entry) .ft_Image, #filesNewsList .file-row:not(.folder-row):not(.error-entry) .ft_Video");

                var mediaFiles = [];
                var pos = 0;
                for (var i = 0; i < newFiles.length; i++) {
                    var fData = ASC.Files.UI.getObjectData(newFiles[i]);

                    if (ASC.Files.MediaPlayer.canPlay(fData.title) || ASC.Files.Utility.CanImageView(fData.title)) {
                        mediaFiles.push(fData);
                        if (fileData.id === fData.id) {
                            pos = mediaFiles.length - 1;
                        }
                    }
                }

                var newFolderId = jq("#filesNewsPanel").attr("data-id");
                var newObj = jq(".is-new" + ASC.Files.UI.getSelectorId(newFolderId));

                ASC.Files.MediaPlayer.init(-1, {
                    playlist: mediaFiles,
                    playlistPos: pos,
                    onCloseAction: function (folderId) {
                        if (!ASC.Files.Common.isCorrectId(ASC.Files.Folders.currentFolder.id)) {
                            ASC.Files.Anchor.navigationSet(folderId, false);
                            return;
                        }

                        ASC.Files.Anchor.navigationSet(ASC.Files.Folders.currentFolder.id, true);
                    },
                    onMediaChangedAction: function (fileId) {
                        var newsObj = ASC.Files.UI.getEntryObject("file", fileId).find(".is-new");
                        if (!newsObj.is(":visible")) {
                            if (!newObj.is(":visible")) {
                                newObj = ASC.Files.UI.getEntryObject("folder", newFolderId).find(".is-new");
                            }
                            prevCount = newObj.html() | 0;
                            ASC.Files.Marker.setNewCount("folder", newFolderId, prevCount - 1);
                        }

                        ASC.Files.Marker.removeNewIcon("file", fileId);

                        var hash = ASC.Files.MediaPlayer.getPlayHash(fileId);
                        ASC.Files.Anchor.move(hash, true);
                    },
                    downloadAction: ASC.Files.Utility.GetFileDownloadUrl
                });
            } else {
                updated = ASC.Files.Folders.clickOnFile(fileData);
            }

            if (!updated) {
                newFolderId = jq("#filesNewsPanel").attr("data-id");
                newObj = jq(".is-new" + ASC.Files.UI.getSelectorId(newFolderId));
                if (!newObj.is(":visible")) {
                    newObj = ASC.Files.UI.getEntryObject("folder", newFolderId).find(".is-new");
                }
                var prevCount = newObj.html() | 0;
                ASC.Files.Marker.setNewCount("folder", newFolderId, prevCount - 1);
            }

            return false;
        });

        jq("#studio_sidePanel .page-menu .is-new").trackEvent("files_tree", "action-click", "mark_as_read");
    });
})(jQuery);