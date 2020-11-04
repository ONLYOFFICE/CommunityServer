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


window.ASC.Files.EmptyScreen = (function () {
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }
        jq.dropdownToggle({
            switcherSelector: "#emptyContainer .hintCreate",
            dropdownID: "hintCreatePanel",
            fixWinSize: false
        });

        jq.dropdownToggle({
            switcherSelector: "#emptyContainer .hintUpload",
            dropdownID: "hintUploadPanel",
            fixWinSize: false
        });

        jq.dropdownToggle({
            switcherSelector: "#emptyContainer .hintOpen",
            dropdownID: "hintOpenPanel",
            fixWinSize: false
        });

        jq.dropdownToggle({
            switcherSelector: "#emptyContainer .hintEdit",
            dropdownID: "hintEditPanel",
            fixWinSize: false
        });
    };

    var displayEmptyScreen = function () {
        ASC.Files.UI.hideAllContent(true);
        if (ASC.Files.Mouse) {
            ASC.Files.Mouse.finishMoveTo();
            ASC.Files.Mouse.finishSelecting();
        }

        jq("#filesMainContent, #switchViewFolder, #mainContentHeader, #pageNavigatorHolder, #filesBreadCrumbs").hide();
        jq("#emptyContainer > div").hide();

        if (!ASC.Files.Filter || !ASC.Files.Filter.getFilterSettings().isSet) {
            jq(".files-filter").hide();

            jq("#emptyContainer .empty-folder-create").toggle(ASC.Files.UI.accessEdit());

            if (!ASC.Files.Tree || ASC.Files.Tree.pathParts.length > 1) {
                jq("#emptyContainer_subfolder").show();

                ASC.Files.UI.checkButtonBack(".empty-folder-toparent");
            } else {
                jq("#emptyContainer_" + ASC.Files.Folders.folderContainer).show();
            }
        } else {
            jq("#emptyContainer_filter").show();
        }

        jq("#emptyContainer").show();
        ASC.Files.UI.stickContentHeader();
    };

    var hideEmptyScreen = function () {
        if (jq("#filesMainContent").is(":visible") && jq("#mainContentHeader").is(":visible")) {
            return;
        }
        ASC.Files.UI.hideAllContent(true);
        if (ASC.Files.Mouse) {
            ASC.Files.Mouse.finishMoveTo();
            ASC.Files.Mouse.finishSelecting();
        }

        jq("#emptyContainer").hide();

        ASC.Files.UI.checkButtonBack(".to-parent-folder", "#filesBreadCrumbs");

        jq(".files-filter").show();
        if (ASC.Files.Filter) {
            ASC.Files.Filter.resize();
        }

        jq("#filesMainContent, #switchViewFolder, #mainContentHeader").show();
        ASC.Files.UI.stickContentHeader();
    };

    return {
        init: init,

        hideEmptyScreen: hideEmptyScreen,
        displayEmptyScreen: displayEmptyScreen
    };
})();

(function ($) {

    if (jq("#hintCreatePanel").length == 0)
        return;

    ASC.Files.EmptyScreen.init();
    $(function () {
    });
})(jQuery);