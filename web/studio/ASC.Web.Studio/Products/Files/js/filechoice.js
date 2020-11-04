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


window.ASC.Files.FileChoice = (function () {
    var isInit = false;

    var init = function (folderId, onlyFolder, thirdParty, fromEditor, originForPost) {
        if (fromEditor) {
            thirdParty = undefined;
        }
        if (isInit === false) {
            isInit = true;

            if (typeof thirdParty == "string") {
                if (thirdParty == "") {
                    thirdParty = undefined;
                } else {
                    thirdParty = (thirdParty == "true");
                }
            }

            jq("#fileSelectorTree").css("visibility", "visible");

            var callback = function () {
                ASC.Files.FileSelector.openDialog(folderId, onlyFolder, thirdParty);
                if (onlyFolder) {
                    ASC.Files.FileChoice.eventAfter();
                }
            };

            if (thirdParty) {
                jq("#fileSelectorTree>ul>li:not(.third-party-entry)").toggle(false);
                ASC.Files.FileSelector.createThirdPartyTree(callback);
            }

            if (onlyFolder) {
                jq("body").addClass("only-folder");

                ASC.Files.FileSelector.onSubmit = function (selectedFolderId) {
                    var folderTitle = ASC.Files.FileSelector.fileSelectorTree.getFolderTitle(selectedFolderId);

                    var path = ASC.Files.FileSelector.fileSelectorTree.getPath(selectedFolderId);
                    var pathId = jq(path).map(function (i, fId) {
                        return ASC.Files.FileSelector.fileSelectorTree.getFolderTitle(fId);
                    });

                    pathId.push(folderTitle);
                    var pathTitle = pathId.toArray().join(' > ');

                    var message = JSON.stringify({
                        folderId: selectedFolderId,
                        pathTitle: pathTitle,
                    });

                    window.parent.postMessage(message, originForPost);
                };
            } else {
                ASC.Files.Folders.eventAfter = function () {
                    ASC.Files.FileChoice.eventAfter();
                };

                ASC.Files.FileSelector.onSubmit = function (files) {

                    if (!fromEditor) {
                        if (jq(".file-selector-files").hasClass("file-selector-single")) {
                            var file = files[0];
                            var message = JSON.stringify({
                                fileId: file.id,
                                fileTitle: file.title,
                            });
                        } else {
                            message = JSON.stringify({
                                files: files,
                                folderShareable: ASC.Files.Folders.currentFolder.shareable
                            });
                        }

                        window.parent.postMessage(message, originForPost);
                    } else {
                        file = files[0];

                        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetPresignedUri, function (jsonData, params, errorMessage) {
                            if (typeof errorMessage != "undefined") {
                                toastr.error(errorMessage);
                                return;
                            }
                            var data = {
                                file: jsonData,
                                Referer: "onlyoffice"
                            };

                            message = JSON.stringify(data);

                            window.parent.postMessage(message, originForPost);
                        });

                        ASC.Files.ServiceManager.getPresignedUri(ASC.Files.ServiceManager.events.GetPresignedUri, {fileId: file.id});
                    }
                };
            }

            ASC.Files.FileSelector.onCancel = function () {
                var data = {
                    file: null,
                    Referer: "onlyoffice"
                };
                var message = JSON.stringify(data);
                window.parent.postMessage(message, originForPost);
            };

            if (!thirdParty) {
                callback();
            }
        }
    };

    var eventAfter = function () {
    };

    return {
        init: init,
        eventAfter:eventAfter
    };
})();

//(function ($) {
//    $(function () {
//        ASC.Files.FileChoice.init();
//    });
//})(jQuery);