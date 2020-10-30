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

    var init = function (originForPost) {
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
                window.parent.postMessage(message, originForPost);
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
