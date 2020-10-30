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


if (typeof ASC === "undefined")
    ASC = {};

ASC.RestoreManager = (function () {
    var init = function () {

        if (jq("#restoreBlock").hasClass("disable")) {
            jq("#restoreBlock").find("input").prop("disabled", true);
            jq("#startRestoreBtn").addClass("disable");
            return;
        }

        initUploader();
        initChooseStorage();
        initFileSelector();
        Teamlab.getBackupStorages({},
        {
            success: function(params, response) {
                initConsumerStorages(response);
            },
            error: function(params, errors) {
                toastr.error(errors[0]);
            }
        });

        jq(".restore-settings_places input:radio").on("change", initChooseStorage);
        jq(".restore-settings_show-list").on("click", function () {
            initBackupList();
            StudioBlockUIManager.blockUI("#restoreChooseBackupDialog", 525);
        });
        jq(".restore-backup-list").on("click", "span.restore-backup-list_del", deleteBackup);
        jq(".restore-backup-list").on("click", "span.restore-backup-list_action", onClickRestoreAction);

        jq("#restoreChosenFile").on('change', function () {
            jq("#restoreChosenFileField").val(jq(this).val());
        });

        jq('#startRestoreBtn').on('click', onClickRestoreBtn);

        jq("#helpRestoreThirdStorageDisable").on("click", function () {
            jq(this).helper({
                BlockHelperID: 'restoreThirdStorageDisable'
            });
        });

        jq("#clearBackupList").on("click", function () {
            Teamlab.deleteBackupHistory({},
            {
                success: function() {
                    jq("#restoreChooseBackupDialog").find(".restore-backup-list").empty();
                    jq("#emptyListRestore").show();
                },
                error: function(params, errors) {
                    toastr.error(errors[0]);
                }
            });
        });
    };

    function initFileSelector() {
        jq("#fileSelectorTree>ul>li.tree-node:not([data-id=\"" + ASC.Files.Constants.FOLDER_ID_COMMON_FILES + "\"])").remove();
        ASC.Files.FileSelector.filesFilter = ASC.Files.Constants.FilterType.ArchiveOnly;

        jq("#restoreChooseTeamlabFile").click(function () {
            ASC.Files.FileSelector.onSubmit = function (files) {
                var file = files[0];
                jq("#restoreChosenTeamlabFile").attr("data-fileId", file.id).val(file.title);
            };

            var onlyThirdParty = jq("#restoreThirdStorage").is(":checked");
            ASC.Files.FileSelector.openDialog(null, false, onlyThirdParty);
            ASC.Files.FileSelector.setTitle(ASC.Resources.Master.Resource.SelectFile);
        });
    };

    var uploadData = null;

    var storageTypes = {
        Docs: "0",
        ThirdPartyDocs: "1",
        Local: "3",
        Consumers: "5"
    }

    var displayNoneClass = "display-none", withErrorClass = "with-error";

    function initConsumerStorages(storages) {
        storages.forEach(function (item) {
            item.properties.push({ name: "filePath", title: ASC.Resources.Master.Resource.RestoreConsumerPath });
        });
        var $backupConsumerStorageSettingsBox = jq("#restoreConsumerStorageSettingsBox");
        var selectedConsumer = storages.find(function (item) { return item.isSet }) || storages[0];
        initConsumerStorage($backupConsumerStorageSettingsBox, selectedConsumer, storages);
    }

    function initConsumerStorage($box, selectedConsumer, storages) {
        $box.html(jq.tmpl("consumerSettingsTmpl", { storages: storages, selectedConsumer: selectedConsumer }));
        $box.on("change", ".textEdit", function () {
            var newVal = jq(this).find(":selected").val();
            $box.find(".textBox").removeClass(withErrorClass);
            $box.find("div[data-id]").addClass(displayNoneClass);
            $box.find("[data-id='" + newVal + "']").removeClass(displayNoneClass);
        });
        $box.find(".textEdit").val(selectedConsumer.title).trigger("change");
        $box.off("input.textbox").on("input.textbox", ".textBox", function () {
            $(this).removeClass(withErrorClass);
        });
    }

    var createFileuploadInput = function (buttonObj) {
        var inputObj = jq("<input/>")
            .attr("id", "fileupload")
            .attr("type", "file")
            .css("width", "0")
            .css("height", "0")
            .hide();

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on("click", function (e) {
            e.preventDefault();
            jq("#fileupload").click();
        });
    };

    function initUploader() {

        createFileuploadInput(jq("#restoreChosenFileBtn"));

        var uploader = jq("#fileupload").fileupload({
            url: "ajaxupload.ashx?type=ASC.Web.Studio.Core.Backup.BackupFileUploadHandler,ASC.Web.Studio",
            autoUpload: false,
            singleFileUploads: true,
            sequentialUploads: true,
            progressInterval: 1000,
        });

        uploader
            .bind("fileuploadadd", function (e, data) {
                uploadData = data;
                jq("#restoreChosenFileField").val(data.files[0].name);
            })
            .bind("fileuploaddone", function (e, data) {
                var source = getSourceRestore();
                source.params.push({ key: "filePath", value: jq.parseJSON(data.result).Data });
                startRestore(source);
            })
            .bind("fileuploadfail", function () {
                uploadData = null;
                jq("#restoreChosenFileField").val("");
                unlockRestoreBlock();
            });
    };

    function initChooseStorage () {
        var storage = jq(".restore-settings_places input:radio:checked").val(),
            $pathFile = jq(".restore-setting_computer-file"),
            $pathTeamlabFile = jq(".restore-setting_teamlab-file"),
            $consumerSettingBox = jq("#restoreConsumerStorageSettingsBox");

        function hideAll() {
            $pathTeamlabFile.addClass(displayNoneClass);
            $pathFile.addClass(displayNoneClass);
            $consumerSettingBox.addClass(displayNoneClass);
        }

        switch (storage) {
            case storageTypes.Docs:
            case storageTypes.ThirdPartyDocs:
                hideAll();
                $pathTeamlabFile.removeClass(displayNoneClass).attr("data-fileId", "").find("input:text").val("");
                break;
            case storageTypes.Local:
                hideAll();
                $pathFile.removeClass(displayNoneClass);
                break;
            case storageTypes.Consumers:
                hideAll();
                $consumerSettingBox.removeClass(displayNoneClass);
                break;
        }

    };

    function initBackupList() {
        Teamlab.getbackuphistory({},
        {
            success: function(params, data) {
                var $restoreListBlock = jq("#restoreChooseBackupDialog");
                $restoreListBlock.find(".loader-text-block").hide();
                if (data.length) {
                    $restoreListBlock.find(".restore-backup-list").html(jq('#backupList').tmpl({ items: data }));
                    jq("#emptyListRestore").hide();
                } else {
                    $restoreListBlock.find(".restore-backup-list").empty();
                    jq("#emptyListRestore").show();
                }
            },
            error: function (params, errors) {
                toastr.error(errors[0]);
            }
        });
    };

    function deleteBackup() {
        var $item = jq(this).parents("tr"),
            id = $item.attr("data-id");
        Teamlab.deleteBackup({}, id,
            {
                success: function(data, response) {
                    $item.remove();
                    var $backupItems = jq("#restoreChooseBackupDialog").find(".restore-backup-list tr");
                    if (!$backupItems.length) {
                        jq("#emptyListRestore").show();
                    }
                },
                error: function(data, errors) {
                    toastr.error(errors[0]);
                }
            });
    };

    function startRestore(source) {
        var isNotify = jq("#restoreSendNotification").is(":checked"),
            fileId = "";
        var fileIdParam = source.params.find(function (item) { return item.key === "fileId" });
        if (fileIdParam) {
            fileId = fileIdParam.value;
        }

        var data = {
            backupId: fileId,
            storageType: source.name,
            storageParams: source.params,
            notify: isNotify
        };

        Teamlab.startRestore({},
            data,
            {
                success: function() {
                    unlockRestoreBlock();
                    window.location.href = "./PreparationPortal.aspx?type=1";
                },
                error: function (params, errors) {
                    jq(".restore-setting_block input").attr("disabled", false);
                    toastr.error(errors[0]);
                }
            });
    };

    var getSourceRestore = function () {
        var checkItem = jq(".restore-settings_places input:radio:checked"),
            source = {
                name: checkItem.val(),
                params: []
            },
            isError;

        if (!checkItem.length) {
            return false;
        }
        jq("#restoreChosenTeamlabFile, #restoreConsumerStorageSettingsBox, #restoreChosenFileField").removeClass(withErrorClass);

        switch (source.name) {
            case storageTypes.Docs:
            case storageTypes.ThirdPartyDocs:
                var $path = jq("#restoreChosenTeamlabFile");
                var val = $path.attr("data-fileid");
                storage.params.push({ key: "filePath", value: val });
                if (!val) {
                    $path.addClass(withErrorClass);
                    return false;
                }

                break;
            case storageTypes.Local:
                var val = jq("#restoreChosenFileField").val();
                storage.params.push({ key: "filePath", value: val });
                if (!val) {
                    jq("#restoreChosenFileField").addClass(withErrorClass);
                    return false;
                }
                break;
            case storageTypes.Consumers:
                var $consumerStorageSettingsBox = jq("#restoreConsumerStorageSettingsBox");
                var selectedConsumer = $consumerStorageSettingsBox.find('.textEdit :selected').val();
                var $settings = $consumerStorageSettingsBox.find('div[data-id="' + selectedConsumer + '"] .textBox');
                source.params.push({ key: "module", value: selectedConsumer });
                var settingsLength = $settings.length;
                for (var i = 0; i < settingsLength; i++) {
                    var $item = $($settings[i]);
                    if (!$item.val()) {
                        $item.addClass(withErrorClass);
                        isError = true;
                    }
                    else {
                        source.params.push({ key: $item.attr("data-id"), value: $item.val() });
                    }
                }
                
                if (isError) {
                    return false;
                }
                break;
        }
        return source;
    };

    function onClickRestoreBtn() {
        if (uploadData) uploadData.submit();
        var source = getSourceRestore();

        if (!source || jq(this).hasClass("disable")) {
            return false;
        }
        lockRestoreBlock();

        if (source.name == 3) return false; // 3 - the computer file 
        startRestore(source);
    };

    function onClickRestoreAction() {
        var source = {
            name: "0",
            params: [
                { key: "fileId", value: jq(this).closest("tr").attr("data-id") }
            ]
        };
        PopupKeyUpActionProvider.CloseDialog();
        startRestore(source);
    };

    function lockRestoreBlock () {
        LoadingBanner.showLoaderBtn("#restoreBlock");
        jq(".restore-setting_block input").attr("disabled", true);
    }

    function unlockRestoreBlock () {
        LoadingBanner.hideLoaderBtn("#restoreBlock");
        jq(".restore-setting_block input").attr("disabled", false);
    }

    return{
        init: init
    }
})();


jq(function () {
    ASC.RestoreManager.init();
});


