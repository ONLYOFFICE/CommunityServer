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


window.ASC.Files.Unsubscribe = (function () {
    var isInit = false;

    var init = function () {
        if (isInit || jq("#filesConfirmUnsubscribe").length == 0) return;

        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.UnSubscribeMe, onUnSubscribeMe);

        jq("#studioPageContent").on("click", "#buttonUnsubscribe, #mainUnsubscribe.unlockAction", function () {
            ASC.Files.Actions.hideAllActionPanels();
            unSubscribeMe();
        });

        jq("#unsubscribeConfirmBtn").on("click", function () {
            confirmUnSubscribe();
        });

        isInit = true;
    };

    var unSubscribeMe = function (entryType, entryId) {
        if (ASC.Files.Folders.folderContainer != "forme"
            && ASC.Files.Folders.folderContainer != "privacy") {
            return;
        }
        var list = new Array();

        var textFolder = "";
        var textFile = "";
        var strHtml = "<label title=\"{0}\"><input type=\"checkbox\" class=\"checkbox\" entryType=\"{1}\" entryId=\"{2}\" checked=\"checked\">{0}</label>";

        if (entryType && entryId) {
            list.push({ entryType: entryType, entryId: entryId });

            var entryRowTitle = ASC.Files.UI.getEntryTitle(entryType, entryId);

            if (entryType == "file") {
                textFile += strHtml.format(entryRowTitle, entryType, entryId);
            } else {
                textFolder += strHtml.format(entryRowTitle, entryType, entryId);
            }
        } else {
            jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file):has(.checkbox input:checked)").each(function () {
                var entryRowData = ASC.Files.UI.getObjectData(this);
                var entryRowType = entryRowData.entryType;
                var entryRowId = entryRowData.entryId;

                list.push({ entryType: entryRowType, entryId: entryRowId });

                entryRowTitle = entryRowData.title;
                if (entryRowType == "file") {
                    textFile += strHtml.format(entryRowTitle, entryRowType, entryRowId);
                } else {
                    textFolder += strHtml.format(entryRowTitle, entryRowType, entryRowId);
                }
            });
        }

        if (list.length == 0) {
            return;
        }

        jq("#confirmUnsubscribeList dd.confirm-remove-files").html(textFile);
        jq("#confirmUnsubscribeList dd.confirm-remove-folders").html(textFolder);

        jq("#confirmUnsubscribeList .confirm-remove-folders, #confirmUnsubscribeList .confirm-remove-files").show();
        if (textFolder == "") {
            jq("#confirmUnsubscribeList .confirm-remove-folders").hide();
        }
        if (textFile == "") {
            jq("#confirmUnsubscribeList .confirm-remove-files").hide();
        }

        ASC.Files.UI.blockUI("#filesConfirmUnsubscribe", 420);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#unsubscribeConfirmBtn\").trigger('click');";
    };

    var confirmUnSubscribe = function () {
        if (jq("#filesConfirmUnsubscribe:visible").length == 0) {
            return;
        }

        PopupKeyUpActionProvider.CloseDialog();

        var listChecked = jq("#confirmUnsubscribeList input:checked");
        if (listChecked.length == 0) {
            return;
        }

        var data = {};
        data.entry = new Array();

        var list =
            listChecked.map(function (i, item) {
                var entryConfirmType = jq(item).attr("entryType");
                var entryConfirmId = jq(item).attr("entryId");
                var entryConfirmObj = ASC.Files.UI.getEntryObject(entryConfirmType, entryConfirmId);
                ASC.Files.UI.blockObject(entryConfirmObj, true, ASC.Files.FilesJSResource.DescriptRemove, true);
                data.entry.push(entryConfirmType + "_" + entryConfirmId);
                return { entryId: entryConfirmId, entryType: entryConfirmType };
            }).toArray();
        ASC.Files.UI.updateMainContentHeader();

        ASC.Files.ServiceManager.unSubscribeMe(ASC.Files.ServiceManager.events.UnSubscribeMe,
            {
                parentFolderID: ASC.Files.Folders.currentFolder.id,
                showLoading: true,
                list: list
            },
            { stringList: data });

    };

    var onUnSubscribeMe = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }

        var list = params.list;
        var foldersCountChange = false;
        var fromParent = false;

        jq(list).each(function (i, item) {
            if (!foldersCountChange && item.entryType == "folder") {
                foldersCountChange = true;
            }

            ASC.Files.Marker.removeNewIcon(item.entryType, item.entryId);
            var entryObject = ASC.Files.UI.getEntryObject(item.entryType, item.entryId);

            if (item.entryId == params.parentFolderID) {
                fromParent = true;
                ASC.Files.UI.blockObject(entryObject);
            } else {
                ASC.Files.UI.removeEntryObject(entryObject);
            }
        });

        if (foldersCountChange && ASC.Files.Tree) {
            if (fromParent) {
                ASC.Files.ServiceManager.removeFromCache(ASC.Files.ServiceManager.events.GetFolderItems);
                var parent = ASC.Files.Tree.getParentId(params.parentFolderID);
                ASC.Files.Tree.reloadFolder(parent);
                ASC.Files.Folders.clickOnFolder(parent);
            } else {
                ASC.Files.Tree.reloadFolder(params.parentFolderID);
            }
        }

        ASC.Files.UI.checkEmptyContent();
    };

    return {
        init: init,
        unSubscribeMe: unSubscribeMe
    };

})();

jq(document).ready(function () {
    (function ($) {
        ASC.Files.Unsubscribe.init();
    })(jQuery);
});