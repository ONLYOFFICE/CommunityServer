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

    var init = function () {
        if (isInit === false) {
            isInit = true;

            if (ASC.Files.Utility.CanWebView(jq("#saveAsTitle").data("title"))) {
                jq("#saveAsOpenTabPanel").insertBefore(".middle-button-container");
            } else {
                jq("#saveAsOpenTabPanel").remove();
            }

            var finishSubmit = function (data) {
                data.Referer = "onlyoffice";
                var message = JSON.stringify(data);
                window.parent.postMessage(message, "*");
            };

            ASC.Files.FileSelector.onSubmit = function (folderId) {
                var fileUri = jq("#saveAsTitle").data("url");
                var fileTitleOrigin = jq("#saveAsTitle").data("title");
                var fileExt = ASC.Files.Utility.GetFileExtension(fileTitleOrigin);
                var fileTitle = jq("#saveAsTitle").val();

                var curExt = ASC.Files.Utility.GetFileExtension(fileTitle);
                if (curExt != fileExt) {
                    if (!fileTitle.length) {
                        jq("#saveAsTitle").addClass("with-error");
                        return false;
                    } else {
                        fileTitle += fileExt;
                    }
                }

                var createFileUrl = ASC.Files.Utility.GetFileWebEditorExternalUrl(fileUri, fileTitle, folderId);

                if (jq("#saveAsOpenTab").is(":checked")) {
                    window.open(createFileUrl, "_blank");
                    finishSubmit({});
                } else {
                    jq("#submitFileSelector").addClass("disable");

                    createFileUrl += "&response=message";
                    jq.ajax({
                        url: createFileUrl,
                        success: function (response) {
                            if (response && response.indexOf("error: ") == 0) {
                                var data = {error: response.replace("error: ", "")};
                            } else {
                                data = {message: response.replace("ok: ", "")};
                            }
                            finishSubmit(data);
                        },
                        error: function (data) {
                            finishSubmit({error: data.statusText});
                        }
                    });
                }

                return true;
            };

            ASC.Files.FileSelector.onCancel = function () {
                finishSubmit({});
            };

            ASC.Files.FileSelector.openDialog(null, true);

            ASC.Files.UI.checkCharacter(jq("#saveAsTitle"));
        }
    };

    return {
        init: init,
    };
})();
