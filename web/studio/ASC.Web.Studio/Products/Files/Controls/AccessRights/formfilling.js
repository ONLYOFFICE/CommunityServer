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


window.ASC.Files.FormFilling = (function () {
    var isInit = false,
        isVisible = false,
        fileId = null,
        folderSelector = null;

    var $sharingSettingsDialog = jq("#studio_sharingSettingsDialog"),
        $sharingSettingsItems = jq("#sharingSettingsItems"),
        $formFillingPanel = jq("#formFillingPanel"),
        $formFillingLink = jq("#formFillingLink"),
        $formFillingDialogParent = jq("#formFillingDialogParent"),
        $formFillingCancelOwnerCrossBtn = jq("#formFillingOwnerDialog .popupCancel .cancelButton"),
        $formFillingSaveOwnerBtn = jq("#formFillingSaveOwnerBtn"),
        $formFillingCancelOwnerBtn = jq("#formFillingCancelOwnerBtn"),
        $formFillingDialogHeader = jq("#formFillingDialogHeader"),
        $formFillingDialogBody = jq("#formFillingDialogBody"),
        $formFillingEnableCbx = jq("#formFillingEnableCbx"),
        $formFillingFolderHeader = jq("#formFillingFolderContainer .header-base-small"),
        $formFillingFolder = jq("#formFillingFolder"),
        $formFillingCreateSubfolderCbx = jq("#formFillingCreateSubfolderCbx"),
        $formFillingSubfolderText = jq("#formFillingSubfolderText"),
        $formFillingSingleObjects = jq("#formFillingDialogBody .single-form"),
        $formFillingMultipleObjects = jq("#formFillingDialogBody .multiple-form"),
        $formFillingDefaultTitleRadio = jq("#formFillingDefaultTitleRadio"),
        $formFillingCustomTitleRadio = jq("#formFillingCustomTitleRadio"),
        $formFillingTitleText = jq("#formFillingTitleText"),
        $formFillingTitleMaskParameters = jq("#formFillingTitleMask .link.plus"),
        $formFillingSaveBtn = jq("#formFillingSaveBtn"),
        $formFillingCancelBtn = jq("#formFillingCancelBtn");

    var init = function () {
        if (isInit === false) {
            isInit = true;

            if (!ASC.Files.TreePrototype) return;

            jq(document).on("keyup", function (event) {
                if (!isVisible) {
                    return;
                }
                var code = event.keyCode || event.which;
                if (code == 27) {
                    closeDialog();
                }
            });

            folderSelector = new ASC.Files.TreePrototype("#formFillingFolderSelector");
            folderSelector.clickOnFolder = folderSelect;

            jq.dropdownToggle(
                {
                    dropdownID: "formFillingFolderSelectorContainer",
                    inPopup: true,
                    switcherSelector: "#formFillingFolder:not(.disabled)",
                });

            $formFillingLink.on("click", getPropertiesAndShowDialog);
            $formFillingCancelOwnerCrossBtn.on("click", closeDialog);
            $formFillingSaveOwnerBtn.on("click", closeOwnerDialog);
            $formFillingCancelOwnerBtn.on("click", closeDialog);
            $formFillingEnableCbx.on("click", enableFormFilling);
            $formFillingTitleMaskParameters.on("click", addMaskParameter);
            $formFillingSaveBtn.on("click", setProperties);
            $formFillingCancelBtn.on("click", closeDialog);
        }
    };

    var showOwnerDialog = function () {
        $formFillingDialogParent.addClass("form-filling-owner");
    }

    var closeOwnerDialog = function () {
        $formFillingDialogParent.removeClass("form-filling-owner");
    };

    var disableDialog = function (disableHeader, disableBody, disableSaveBtn) {
        $formFillingDialogHeader.toggleClass("disabled", disableHeader);
        $formFillingDialogHeader.find("input").toggleClass("disabled", disableHeader).prop("disabled", disableHeader);

        $formFillingDialogBody.toggleClass("disabled", disableBody);
        $formFillingDialogBody.find(".link").toggleClass("disabled", disableBody);
        $formFillingDialogBody.find("input").toggleClass("disabled", disableBody).prop("disabled", disableBody);

        $formFillingSaveBtn.toggleClass("disable", disableSaveBtn);

        clearErrors();
    };

    var clearErrors = function () {
        $formFillingFolderHeader.removeClass("red-text");
        $formFillingSubfolderText.removeClass("with-error");
        $formFillingTitleText.removeClass("with-error");
    };

    var enableFormFilling = function () {
        disableDialog(false, !jq(this).is(":checked"), false);
    };

    var addMaskParameter = function () {
        var $link = jq(this);
        if ($link.hasClass("disabled")) {
            return;
        }
        $formFillingTitleText.val($formFillingTitleText.val() + $link.attr("data-val")).trigger("focus");
    };

    var getFormFillingData = function () {
        var isValid = true;

        clearErrors();

        if (!$formFillingEnableCbx.is(":checked")) {
            return { collectFillForm: false };
        }

        var folderId = $formFillingFolder.attr("data-id");
        if (!folderId) {
            $formFillingFolderHeader.addClass("red-text");
            isValid = false;
        }

        var subfolderTitle = "";
        if ($formFillingCreateSubfolderCbx.is(":checked") && $formFillingSubfolderText.is(":visible")) {
            subfolderTitle = $formFillingSubfolderText.val().trim();
            if (!subfolderTitle) {
                $formFillingSubfolderText.addClass("with-error");
                isValid = false;
            }
        }

        var fileTitle = "";
        if ($formFillingCustomTitleRadio.is(":checked")) {
            fileTitle = $formFillingTitleText.val().trim();
            if (!fileTitle) {
                $formFillingTitleText.addClass("with-error");
                isValid = false;
            }
        }

        return isValid ? {
            collectFillForm: true,
            toFolderId: folderId,
            createFolderTitle: subfolderTitle,
            createFileMask: fileTitle
        } : null;
    };

    var showDialog = function (single, data) {
        var empty = false;

        $formFillingSingleObjects.toggleClass("display-none", !single);
        $formFillingMultipleObjects.toggleClass("display-none", single);

        if (!data) {
            data = {};
            empty = true;
        }

        if (!data.formFilling) {
            data.formFilling = {};
            empty = true;
        }

        var enabled = !!data.formFilling.collectFillForm;
        $formFillingEnableCbx.prop("checked", enabled);

        disableDialog(false, !enabled, empty);

        var folderId = data.formFilling.toFolderId || ASC.Files.Folders.currentFolder.id;
        var folderData = folderSelector.getFolderData(folderId);

        if (!folderData) {
            showOwnerDialog();
        }

        if (!folderData || !ASC.Files.UI.accessEdit(folderData)) {
            folderId = ASC.Files.Constants.FOLDER_ID_MY_FILES;
            folderData = folderSelector.getFolderData(folderId);
        }

        folderSelect(folderId, folderData);

        if (data.formFilling.createFolderTitle) {
            $formFillingCreateSubfolderCbx.prop("checked", true);
            $formFillingSubfolderText.val(data.formFilling.createFolderTitle);
        } else {
            $formFillingCreateSubfolderCbx.prop("checked", false);
            $formFillingSubfolderText.val("");
        }

        if (data.formFilling.createFileMask) {
            $formFillingCustomTitleRadio.prop("checked", true);
            $formFillingTitleText.val(data.formFilling.createFileMask);
        } else {
            $formFillingDefaultTitleRadio.prop("checked", true);
            $formFillingTitleText.val("");
        }

        if (!$sharingSettingsDialog.is(":visible")) {
            $sharingSettingsDialog.addClass("form-filling-only");
        }

        $formFillingDialogParent.show();
        isVisible = true;
    };

    var closeDialog = function () {
        closeOwnerDialog();
        $formFillingDialogParent.hide();
        $sharingSettingsDialog.removeClass("form-filling-only");
        isVisible = false;
    };

    var folderSelect = function (folderId, folderData) {
        folderSelector.rollUp();
        folderSelector.setCurrent(folderId);

        var folderData = folderData || folderSelector.getFolderData(folderId);

        if (ASC.Files.UI.accessEdit(folderData)) {
            var title = folderSelector.getFolderTitle(folderId);
            $formFillingFolder.attr("title", title).text(title).attr("data-id", folderId);
            ASC.Files.Actions.hideAllActionPanels();
        } else {
            var errorString = ASC.Files.FilesJSResource.ErrorMassage_SecurityException;
            if (folderId == ASC.Files.Constants.FOLDER_ID_PROJECT || folderId == ASC.Files.Constants.FOLDER_ID_SHARE) {
                errorString = ASC.Files.FilesJSResource.ErrorMassage_SecurityException_PrivateRoot;
            }
            ASC.Files.UI.displayInfoPanel(errorString, true);
            folderSelector.expandFolder(folderId);
        }
    };

    var getPropertiesAndShowDialog = function () {
        if (Array.isArray(fileId)) {
            showDialog(false, null);
            return;
        }

        Teamlab.getFileProperties(null, fileId, {
            success: function (_, data) {
                showDialog(true, data);
            },
            error: function (_, error) {
                ASC.Files.UI.displayInfoPanel(error[0], true);
            }
        });
    };

    var setProperties = function () {
        if ($formFillingSaveBtn.hasClass("disable")) {
            return;
        }

        var formFillingData = getFormFillingData();
        if (!formFillingData) {
            return;
        }

        var data = {
            fileProperties: {
                formFilling: formFillingData
            }
        }

        var options = {
            before: function () {
                disableDialog(true, true, true);
            },
            success: function () {
                closeDialog();
                ASC.Files.UI.displayInfoPanel(ASC.Resources.Master.ResourceJS.SuccessfullySaveSettingsMessage, false);
            },
            error: function (_, error) {
                ASC.Files.UI.displayInfoPanel(error[0], true);
            },
            after: function () {
                disableDialog(false, false, false);
            }
        };

        if (Array.isArray(fileId)) {
            data.filesId = fileId;
            data.createSubfolder = $formFillingCreateSubfolderCbx.is(":checked");
            Teamlab.setFilesProperties(null, data, options);
        } else {
            Teamlab.setFileProperties(null, fileId, data, options);
        }
    };

    var setFileId = function (id) {
        fileId = id;
    };

    var togglePanel = function (show) {
        $formFillingPanel.toggle(show);
        $sharingSettingsItems.toggleClass("with-formfilling-panel", show);
    };

    return {
        init: init,
        setFileId: setFileId,
        togglePanel: togglePanel,
        getPropertiesAndShowDialog: getPropertiesAndShowDialog
    };
})();

jq(document).ready(function () {
    window.ASC.Files.FormFilling.init();
});