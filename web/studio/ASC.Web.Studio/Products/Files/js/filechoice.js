/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

    var init = function (folderId, onlyFolder, thirdParty, mailMerge) {
        if (isInit === false) {
            isInit = true;

            jq("#fileSelectorTree").css("visibility", "visible");

            if (thirdParty) {
                ASC.Files.FileSelector.toggleThirdParty(true);

                ASC.Files.FileSelector.createThirdPartyTree();
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
                ASC.Files.FileSelector.onSubmit = function (files) {
                    var file = files[0];
                    var fileExt = ASC.Files.Utility.GetFileExtension(file.title);
                    fileExt = fileExt.substring(fileExt.indexOf('.') + 1);

                    if (!mailMerge) {
                        var message = JSON.stringify({
                            fileId: file.id,
                            fileTitle: file.title,
                        });

                        window.parent.postMessage(message, "*");
                    } else {
                        Teamlab.getPresignedUri(file.id, {
                            success: function (params, url) {
                                var data = {
                                    file: {
                                        fileType: fileExt,
                                        url: url
                                    }
                                };

                                message = JSON.stringify(data);

                                window.parent.postMessage(message, "*");
                            },
                            error: function (params, errors) {
                                toastr.error(errors);
                            }
                        });
                    }
                };
            }

            ASC.Files.FileSelector.onCancel = function () {
                var message = JSON.stringify({file: null});
                window.parent.postMessage(message, "*");
            };

            ASC.Files.FileSelector.openDialog(folderId, onlyFolder, thirdParty);
        }
    };

    return {
        init: init,
    };
})();

//(function ($) {
//    $(function () {
//        ASC.Files.FileChoice.init();
//    });
//})(jQuery);