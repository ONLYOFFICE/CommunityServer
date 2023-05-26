/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


window.FileChoisePopup = (function () {
    var isInit = false;
    var $dialod, $loader, $frameContainer, $frame, $button;

    var init = function() {
        if (!isInit) {
            isInit = true;
            initElements();
            bindEvents();
        }
    }

    var initElements = function() {
        $dialod = jq("#fileChoisePopupDialod");
        $loader = $dialod.find(".loader-page");
        $frameContainer = jq("#frameContainer");
        $button = jq("#createMasterFormFromFile")

        $frame = jq("<iframe/>",
            {
                "frameborder": 0,
                "height": "566px",
                "id": "fileChoiceFrame",
                "scrolling": "no",
                "onload": "javascript:FileChoisePopup.frameOnLoad();return false;"
            });

        $frame.appendTo($frameContainer);
    }

    var bindEvents = function() {
        $button.on("click", showFileChoisePopup);

        window.addEventListener("message",
            function (message) {
                if (!message || typeof message.data != "string") {
                    return;
                }

                var file = null;

                try {
                    file = JSON.parse(message.data);
                } catch (e) {
                    console.error(e);
                    return;
                }

                if (!file || !file.fileId) {
                    PopupKeyUpActionProvider.EnableEsc = true;
                    jq.unblockUI();
                    return;
                }

                createMasterFormFromFile(file);
            },
            false);
    }

    var showFileChoisePopup = function (e) {
        if ($button.hasClass("disable")) {
            e.stopPropagation();
            return;
        }

        $frame.attr("src", $frameContainer.data("frame") + "&folderid=" + ASC.Files.Folders.currentFolder.id);

        ASC.Files.Actions.hideAllActionPanels();

        StudioBlockUIManager.blockUI($dialod, 1000, { bindEvents: false });

        $loader.show();
    };

    var createMasterFormFromFile = function (file) {
        title = file.fileTitle;
        lenExt = ASC.Files.Utility.GetFileExtension(title).length;
        title = title.substring(0, title.length - lenExt);

        PopupKeyUpActionProvider.EnableEsc = true;
        jq.unblockUI();

        ASC.Files.Folders.createNewDoc({
            title: title + ASC.Files.Utility.Resource.MasterFormExtension,
            entryId: file.fileId
        }, false, copyFileAs);
    };

    var copyFileAs = function (params) {
        Teamlab.copyDocFileAs(null, params.templateId,
            {
                destFolderId: params.folderID,
                destTitle: params.fileTitle
            },
            {
                success: function (_, data) {
                    ASC.Files.ServiceManager.getFile(ASC.Files.ServiceManager.events.CreateNewFile,
                        {
                            fileId: data.id,
                            show: true,
                            isStringXml: false,
                            folderID: params.folderID,
                            winEditor: params.winEditor
                        });
                },
                error: function (_, error) {
                    var fileNewObj = ASC.Files.UI.getEntryObject("file", "0");
                    ASC.Files.UI.blockObject(fileNewObj);
                    ASC.Files.UI.removeEntryObject(fileNewObj);
                    if (jq("#filesMainContent .file-row").length == 0) {
                        ASC.Files.EmptyScreen.displayEmptyScreen();
                    }

                    if (params.winEditor) {
                        params.winEditor.close();
                    }

                    ASC.Files.UI.displayInfoPanel(error[0], true);
                },
                processUrl: function (url) {
                    return ASC.Files.Utility.AddExternalShareKey(url);
                }
            });
    };

    var frameOnLoad = function() {
        $loader.hide();
    }

    return {
        init: init,
        frameOnLoad: frameOnLoad
    };
})(jQuery);

(function ($) {
    window.FileChoisePopup.init();
}) (jQuery);