/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


window.ASC.Files.FileChoice = (function () {
    var isInit = false;

    var init = function (folderId, onlyFolder, thirdParty, fromEditor) {
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

                    window.parent.postMessage(message, "*");
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

                        window.parent.postMessage(message, "*");
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

                            window.parent.postMessage(message, "*");
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
                window.parent.postMessage(message, "*");
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