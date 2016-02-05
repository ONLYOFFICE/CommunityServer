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


RestoreManager = new function () {
    this.Init = function () {

        if (jq("#restoreBlock").hasClass("disable")) {
            jq("#restoreBlock").find("input").prop("disabled", true);
            jq("#startRestoreBtn").addClass("disable");
            return;
        }

        RestoreManager.InitUploader();
        initChooseStorage();
        initFileSelector();

        jq(".restore-settings_places input:radio").on("change", initChooseStorage);
        jq(".restore-settings_show-list").on("click", function () {
            RestoreManager.InitBackupList();
            StudioBlockUIManager.blockUI("#restoreChooseBackupDialog", 525, 300, 0);
        });
        jq(".restore-backup-list").on("click", "span.restore-backup-list_del", RestoreManager.DeleteBackup);
        jq(".restore-backup-list").on("click", "span.restore-backup-list_action", RestoreManager.onClickRestoreAction);

        jq("#restoreChosenFile").on('change', function () {
            jq("#restoreChosenFileField").val(jq(this).val());
        });

        jq('#startRestoreBtn').on('click', RestoreManager.onClickRestoreBtn);

        jq("#helpRestoreThirdStorageDisable").on("click", function () {
            jq(this).helper({
                BlockHelperID: 'restoreThirdStorageDisable'
            });
        });

        jq("#clearBackupList").on("click", function () {
            AjaxPro.Backup.DeleteAllBackups(function (response) {
                if (response.error) {
                    toastr.error(response.error.Message);
                } else {
                    jq("#restoreChooseBackupDialog").find(".restore-backup-list").empty();
                    jq("#emptyListRestore").show();
                }
            });
        });
    };

    var initFileSelector = function () {
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

    var createFileuploadInput = function (buttonObj) {
        var inputObj = jq("<input/>")
            .attr("id", "fileupload")
            .attr("type", "file")
            .css("width", "0")
            .css("height", "0");

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on("click", function (e) {
            e.preventDefault();
            jq("#fileupload").click();
        });
    };

    this.InitUploader = function () {

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
                var source = RestoreManager.getSourceRestore();
                source.params.FilePath = jq.parseJSON(data.result).Data;
                RestoreManager.StartRestore(source);
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
            $cloudSettings = jq("#restoreAmazonSettings");
        switch (storage) {
            case "0":
            case "1":
                $pathTeamlabFile.show().attr("data-fileId", "").find("input:text").val("");
                $pathFile.hide();
                $cloudSettings.hide();
                break;
            case "2":
                $pathTeamlabFile.hide();
                $pathFile.hide();
                $cloudSettings.show();
                break;
            case "3":
                $pathTeamlabFile.hide();
                $pathFile.show();
                $cloudSettings.hide();
                break;
        }

    };

    this.InitBackupList = function () {
        AjaxPro.Backup.GetBackupHistory(function (response) {
            var $restoreListBlock = jq("#restoreChooseBackupDialog");
            $restoreListBlock.find(".loader-text-block").hide();
            if (response.error) {
                toastr.error(response.error.Message);
            } else {
                if (response.value.length) {
                    $restoreListBlock.find(".restore-backup-list").html(jq('#backupList').tmpl({items: response.value}));
                    jq("#emptyListRestore").hide();
                } else {
                    $restoreListBlock.find(".restore-backup-list").empty();
                    jq("#emptyListRestore").show();
                }
            }

        });

    };

    this.DeleteBackup = function () {
        var $item = jq(this).parents("tr"),
            id = $item.attr("data-id");
        AjaxPro.Backup.DeleteBackup(id, function (response) {
            if (response.error) {
                toastr.error(response.error.Message);
            } else {
                $item.remove();
                var $backupItems = jq("#restoreChooseBackupDialog").find(".restore-backup-list tr");
                if (!$backupItems.length) {
                    jq("#emptyListRestore").show();
                }
            }
        });
    };

    this.StartRestore = function (source) {
        var isNotify = jq("#restoreSendNotification").is(":checked"),
            fileId = source.params.FileId || "";
        AjaxPro.timeoutPeriod = 1800000;
        AjaxPro.Backup.StartRestore(fileId, source.name, source.params, isNotify, RestoreManager.onCompleteRestore);
    };

    this.onCompleteRestore = function (response) {
        unlockRestoreBlock();
        if (response.error) {
            jq(".restore-setting_block input").attr("disabled", false);
            showErrorAmazonServer(response.error.Message);
        } else {
            window.location.href = "./PreparationPortal.aspx?type=1";
        }
    };

    function showErrorAmazonServer (errorMes) {
        var $amazonSettings = jq("#restoreAmazonSettings");

        switch (errorMes) {
            case "InvalidAccessKeyId":
                $amazonSettings.find(".access-key-id").addClass("with-error");
                break;
            case "SignatureDoesNotMatch":
                $amazonSettings.find(".secret-access-key").addClass("with-error");
                break;
            case "NoSuchBucket":
                $amazonSettings.find(".bucket").addClass("with-error");
                break;
            default:
                toastr.error(errorMes);
                break;
        }
    };

    this.getSourceRestore = function () {
        var checkItem = jq(".restore-settings_places input:radio:checked"),
            source = {
                name: checkItem.val(),
                params: {}
            },
            isError;

        if (!checkItem.length) {
            return false;
        }
        jq("#restoreChosenTeamlabFile, #restoreAmazonSettings .restore-settings_amazon_params, #restoreChosenFileField").removeClass("with-error");

        switch (source.name) {
            case "0":
            case "1":
                var $path = jq("#restoreChosenTeamlabFile");
                source.params.FilePath = $path.attr("data-fileid");
                if (!source.params.FilePath) {
                    $path.addClass("with-error");
                    return false;
                }
                break;
            case "2":
                var $settings = jq("#restoreAmazonSettings");
                source.params = {
                    AccessKeyId: $settings.find(".access-key-id").val(),
                    SecretAccessKey: $settings.find(".secret-access-key").val(),
                    Bucket: $settings.find(".bucket").val(),
                    Region: $settings.find(".region").val(),
                    FilePath: $settings.find(".file-path").val()
                };

                for (var param in source.params) {
                    if (!source.params[param]) {
                        var className = param.replace(/[A-Z]/g, function (match, i) {
                            return i == 0 ? match.toLowerCase() : '-' + match.toLowerCase();
                        });
                        $settings.find("." + className).addClass("with-error");
                        isError = true;
                    }
                }
                if (isError) return false;
                break;
            case "3":
                source.params.FilePath = jq("#restoreChosenFileField").val();
                if (!source.params.FilePath) {
                    jq("#restoreChosenFileField").addClass("with-error");
                    return false;
                }
                break;
        }
        return source;
    };

    this.onClickRestoreBtn = function () {
        if (uploadData) uploadData.submit();
        var source = RestoreManager.getSourceRestore();

        if (!source || jq(this).hasClass("disable")) {
            return false;
        }
        lockRestoreBlock();

        if (source.name == 3) return false; // 3 - the computer file 
        RestoreManager.StartRestore(source);
    };

    this.onClickRestoreAction = function () {
        var source = {
            name: "0",
            params: {
                FileId: jq(this).closest("tr").attr("data-id")
            }
        };
        PopupKeyUpActionProvider.CloseDialog();
        RestoreManager.StartRestore(source);
    };

    function lockRestoreBlock () {
        LoadingBanner.showLoaderBtn("#restoreBlock");
        jq(".restore-setting_block input").attr("disabled", true);
    }

    function unlockRestoreBlock () {
        LoadingBanner.hideLoaderBtn("#restoreBlock");
        jq(".restore-setting_block input").attr("disabled", false);
    }
};


jq(function () {
        RestoreManager.Init();
});


