/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

BackupManager = new function () {
    this.defaultFolderId = null;
    this.Init = function () {
        initFolderSelector();
        BackupManager.OnCheckAutoBackup();
        setTimeout(BackupManager.InitChooseStorage, 0);
        BackupManager.InitBackupCountCopy();
        BackupManager.InitSchedule();
        

        jq(".backup-settings_places input:radio").on("change", BackupManager.InitChooseStorage);
        jq('#backupAutoSaving').on("change", BackupManager.OnCheckAutoBackup);
        jq("#startBackupBtn").on("click", BackupManager.StartBackup);
        jq("#saveBackupBtn").on("click", BackupManager.SaveBackup);

        jq(".backup-settings_auto-params select, #backupWithMailCheck").on("change", BackupManager.UnlockSaveBtn);

        jq("#helpBackupThirdStorageDisable").on("click", function () {
            jq(this).helper({
                BlockHelperID: 'backupThirdStorageDisable'
            });
        });
    };

    var displayFolderPath = function (folderId) {
        var folderTitle = ASC.Files.FileSelector.fileSelectorTree.getFolderTitle(folderId),
                   path = ASC.Files.FileSelector.fileSelectorTree.getPath(folderId),
                   pathId = jq(path).map(function (i, fId) {
                       return ASC.Files.FileSelector.fileSelectorTree.getFolderTitle(fId);
                   });
        pathId.push(folderTitle);
        var pathTitle = pathId.toArray().join(" > ");
        jq("#backupFolderPath").val(pathTitle).attr("data-folderId", folderId);
        BackupManager.UnlockSaveBtn();
    };

    var initFolderSelector = function () {
        jq("#fileSelectorTree>ul>li.tree-node:not([data-id=\"" + ASC.Files.Constants.FOLDER_ID_COMMON_FILES + "\"])").remove();

        ASC.Files.FileSelector.onInit = function () {
            ASC.Files.FileSelector.toggleThirdParty(true);

            ASC.Files.FileSelector.onThirdPartyTreeCreated = function () {
                jq("*:not(.disabled)>.settings-block .third-party-storage").removeAttr("title").removeClass("disabled")
                    .find("input:radio").attr("disabled", false);
            };
            ASC.Files.FileSelector.createThirdPartyTree();
        };

        jq("#backupFolderPathBtn").click(function () {
            ASC.Files.FileSelector.onSubmit = function (folderId) {
                displayFolderPath(folderId);
            };

            var onlyThirdParty = jq("#backupThirdStorage").is(":checked");
            ASC.Files.FileSelector.openDialog(BackupManager.defaultFolderId, true, onlyThirdParty);
            ASC.Files.FileSelector.setTitle(ASC.Resources.Master.Resource.SelectFolder);
        });
    };

    function showBackupProgress(progress) {
        var $progressContainer = jq("#progressbar_container"),
            $progressValue = jq(".asc-progress-value"),
            $progressText = jq("#backup_percent");

        $progressValue.animate({ "width": progress + "%" });
        $progressText.text(progress + "% ");

        if ($progressContainer.is(":hidden")) {
            $progressContainer.show();
        }
    }

    function processBackupResponse(response) {
        jq("#backupAmazonSettings").removeClass("with-error");

        if (response.error) {
            var $progressContainer = jq("#progressbar_container");
            showErrorAmazonServer(response.error.Message);
            $progressContainer.hide();            
            LoadingBanner.hideLoaderBtn("#backupSettings");
            lockBackupBlock(false);
            return;
        } else if (response.value && response.value.IsCompleted) {
            jq('#progressbar_container').hide();            
            LoadingBanner.hideLoaderBtn("#backupSettings");
            lockBackupBlock(false);

            if (response.value.Link) {
                jq("#backupLinkTemp").attr("href", response.value.Link);
                jq("#backupLinkCnt").show();
            }
            toastr.success(ASC.Resources.Master.Resource.BackupMakeCopySuccess);
            return;
        }

        showBackupProgress(response.value.Progress);
        setTimeout(function () {
            AjaxPro.Backup.GetBackupProgress(processBackupResponse);
        }, 1000);
    };

    this.InitBackupCountCopy = function(){
        var $select = jq("#backupCountCopy");
        for(var i = 1, ln = 31; i < ln; i++){
            i == 10 ? $select.append("<option selected='selected' val="+ i +">" + i +"</option>")
            : $select.append("<option val="+ i +">" + i +"</option>");
        }
    };

    this.InitChooseStorage = function () {
        if (jq("#backupAutoSaving").is(":checked")) {
            BackupManager.UnlockSaveBtn();
        }

        var storage = jq(".backup-settings_places input:radio:checked").val(),
        $pathFolder = jq(".backup-settings_folder"),
        $cloudSettings = jq("#backupAmazonSettings");
        switch (storage) {
            case "0":
                $pathFolder.show();
                jq("#backupFolderPath").attr("data-folderid", ASC.Files.Constants.FOLDER_ID_COMMON_FILES);
                BackupManager.defaultFolderId = ASC.Files.Constants.FOLDER_ID_COMMON_FILES;
                displayFolderPath(BackupManager.defaultFolderId);
                $cloudSettings.hide();
                break;
            case "1":
                $pathFolder.show();
                jq("#backupFolderPath").val("").attr("data-folderId", "");
                $cloudSettings.hide();
                break;
            case "2":
                $pathFolder.hide();
                $cloudSettings.show();
                break;
            case "4":
                $cloudSettings.hide();
                $pathFolder.hide();
                jq("#saveBackupBtn").addClass("disable");
                break;
        }
    };

    this.InitSchedule = function () {
        var isAutoBackup,
            storage = {},
            count,
            paramsTime = {},
            $storagePlaces = jq(".backup-settings_places"),
            $storageChoose = $storagePlaces.find("input").first();
        AjaxPro.Backup.GetSchedule(function (response) {
            if (response) {
                if (response.error) {
                    toastr.error(response.error.Message);
                } else if (response.value) {
                    var value = response.value;
                    isAutoBackup = true;
                    count = value.BackupsStored;
                    storage = {
                        name: value.StorageType,
                        params: value.StorageParams
                    };
                    paramsTime = value.CronParams;
                    jq(".backup-settings_auto-params").show();

                    jq("#backupWithMailCheck").prop("checked", value.BackupMail);

                    $storageBackupAuto = $storagePlaces.find("input[value='" + storage.name + "']");
                    if (!$storageBackupAuto.parent("li").hasClass("disabled")) {
                        $storageChoose = $storageBackupAuto;
                    }
                    if (!$storagePlaces.find("li").hasClass("disabled")) {
                        $storageChoose = $storagePlaces.find("input[value=" + storage.name + "]");
                    }
                        $storageChoose.trigger("click");
                    switch (storage.name) {
                    case 0:
                    case 1:
                        jq("#backupFolderPath").attr("data-folderid", storage.params.FolderId);
                        BackupManager.defaultFolderId = storage.params.FolderId;
                        displayFolderPath(BackupManager.defaultFolderId);
                        getFolderPath(BackupManager.defaultFolderId);
                        break;
                    case 2:
                        var $amazonSettings = jq("#backupAmazonSettings");
                        $amazonSettings.find(".access-key-id").val(storage.params.AccessKeyId);
                        $amazonSettings.find(".secret-access-key").val(storage.params.SecretAccessKey);
                        $amazonSettings.find(".bucket").val(storage.params.Bucket);
                        $amazonSettings.find(".region").val(storage.params.Region);
                        break;
                    }
                    var $time = jq(".select-time-period"),
                        $period = $time.find("#generatedTimePeriod"),
                        $week = $time.find("#week"),
                        $month = $time.find("#month"),
                        $hours = $time.find("#hours");
                    switch (paramsTime.Period) {
                    case 0:
                        $period.val("day");
                        break;
                    case 1:
                        $period.val("week");
                        $week.val(paramsTime.Day).removeClass("display-none");
                        break;
                    case 2:
                        $period.val("month");
                        $month.val(paramsTime.Day).removeClass("display-none");
                        break;
                    }

                    $hours.val(paramsTime.Hour);
                    jq("#backupCountCopy").val(count);
                } else {
                    $storageChoose.trigger("click");
                }
            }
            jq("#backupAutoSaving").prop("checked", isAutoBackup);
            jq("#saveBackupBtn").addClass("disable");

        });
    };

    this.UnlockSaveBtn = function () {
        if (!jq("#backupTempTeamlab").prop("checked")) {
            jq("#saveBackupBtn").removeClass("disable");
        }
    };

    this.OnCheckAutoBackup = function () {
        var isCheck = jq("#backupAutoSaving").is(":checked"),
            $timePeriodBlock = jq(".backup-settings_auto-params");

        BackupManager.UnlockSaveBtn();
        isCheck ? $timePeriodBlock.show() : $timePeriodBlock.hide();
    };

    this.GetStorage = function () {
        var checkItem = jq(".backup-settings_places input:radio:checked");

        if (!checkItem.length) {
            return false;
        }
        var storage = {
            name: checkItem.val(),
            params: {}
        };

        jq("#backupFolderPath , #backupAmazonSettings .backup-settings_amazon_params").removeClass("with-error");

        switch (storage.name) {
            case "0":
            case "1":
                var $path = jq("#backupFolderPath");
                storage.params.FolderId = $path.attr("data-folderid");
                if (!storage.params.FolderId) {
                    $path.addClass("with-error");
                    return false;
                }

                break;
            case "2":
                var $settings = jq("#backupAmazonSettings"),
                    isError;
                storage.params = {
                    AccessKeyId: $settings.find(".access-key-id").val(),
                    SecretAccessKey: $settings.find(".secret-access-key").val(),
                    Bucket: $settings.find(".bucket").val(),
                    Region: $settings.find(".region").val()
                };
                    
                for (var param in storage.params) {
                    if (!storage.params[param]) {
                        var className = param.replace(/[A-Z]/g, function (match, i) {
                            return i == 0 ? match.toLowerCase() : '-' + match.toLowerCase();
                        });
                        $settings.find("." + className).addClass("with-error");
                        isError = true;
                    }
                }
                if (isError) return;
                break;
        }
        return storage;
    };

    this.StartBackup = function () {
        var storage = BackupManager.GetStorage(),
            $button = jq("#startBackupBtn"),
            withMail = jq("#backupWithMailCheck").is(":checked");
        if (!storage || $button.hasClass("disable")) {
            return;
        }
        LoadingBanner.showLoaderBtn("#backupSettings");
        lockBackupBlock(true);
        showBackupProgress(0);

        AjaxPro.Backup.StartBackup(storage.name, storage.params, withMail, processBackupResponse);
    }

    this.SaveBackup = function () {
        if (jq(this).hasClass("disable")) return;

        var isAuto = jq("#backupAutoSaving").is(":checked");

        if (isAuto) {
            var storage = BackupManager.GetStorage(),
                $period = jq(".select-time-period"),
                periodTime = {},
                countCopy = jq("#backupCountCopy").val(),
                $button = jq("#saveBackupBtn"),
                withMail = jq("#backupWithMailCheck").is(":checked");

            if (!storage || $button.hasClass("disable")) {
                return;
            }

            switch ($period.find("#generatedTimePeriod").val()) {
                case "day":
                    periodTime.Period = 0;
                    break;
                case "week":
                    periodTime.Period = 1;
                    break;
                case "month":
                    periodTime.Period = 2;
                    break;
            }
            periodTime.Hour = $period.find("#hours").val();

            if ($period.find("#week").is(":visible")) {
                periodTime.Day = $period.find("#week").val();
            };
            if ($period.find("#month").is(":visible")) {
                periodTime.Day = $period.find("#month").val();
            };
            LoadingBanner.showLoaderBtn("#backupSettings");
            AjaxPro.Backup.CreateSchedule(storage.name, storage.params, countCopy, periodTime, withMail, BackupManager.onCompleteCreateShedule);
        }
        else {
            LoadingBanner.showLoaderBtn("#backupSettings");
            AjaxPro.Backup.DeleteSchedule(BackupManager.onCompleteCreateShedule);
        }
    };

    this.onCompleteCreateShedule = function (response) {
        jq(".backup-settings_block").find(".with-error").removeClass("with-error");

        if (response.error) {
            showErrorAmazonServer(response.error.Message);
        } else {
            toastr.success(ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage);
        }
        LoadingBanner.hideLoaderBtn("#backupSettings");
    };

    function lockBackupBlock(isLock) {
        var $thirdParty = jq("#backupSettings").find(".third-party-storage");
        jq("#backupSettings").find("input, select").attr("disabled", isLock);
        if (!isLock && $thirdParty.hasClass("disabled")) {
            jq("#backupThirdStorage").attr("disabled", !isLock);
        }
        if (jq("#backupTempTeamlab").prop("checked")) {
            jq("#saveBackupBtn").addClass("disable");
        }
    };

    function showErrorAmazonServer (errorMes) {
        var $amazonSettings = jq("#backupAmazonSettings");

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

    function getFolderPath (folderId) {
        Teamlab.getFolderPath(folderId, {
            success: function (e, data) {
                var path = jq(data).map(function (i, folder) {
                    return folder.title;
                });
                var pathTitle = path.toArray().join(" > ");
                jq("#backupFolderPath").val(pathTitle).attr("data-folderId", folderId);
            },
            error: function (params, errors) {
                toastr.error(errors);
            }
        });
    };
};

(function ($) {
    $(function () {
        BackupManager.Init();
    });
})(jQuery);
