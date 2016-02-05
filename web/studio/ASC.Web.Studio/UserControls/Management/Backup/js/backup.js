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


window.BackupManager = new function() {
    var $ = jq;

    var backupStorageFolderId = null;
    var autoBackupSettingsStorageFolderId = null;

    var autoBackupInitOff = null;

    var $view = $('#backup-manager-view');

    var $maxStoredCopiesCount = $view.find('#maxStoredCopiesCount');
    var thirdPartyStorageSelectorHelpers = $view.find('.thirdPartyStorageSelectorHelper');

    var $backupBox = $view.find('#backupBox');

    var $backupStoragesBox = $backupBox.find('#backupStoragesBox');
    var $backupTeamlabStorageFolderSelector = $backupBox.find('#backupTeamlabStorageFolderSelector');
    var $backupTeamlabStorageFolderSelectorBtn = $backupBox.find('#backupTeamlabStorageFolderSelectorBtn');

    var $backupThirdPartyStorageSelectorBox = $backupBox.find('#backupThirdPartyStorageSelectorBox');
    var $backupThirdPartyStorage = $backupBox.find('#backupThirdPartyStorage');
    var $backupAmazonStorageSettingsBox = $backupBox.find('#backupAmazonStorageSettingsBox');
    var $backupWithMailCheck = $backupBox.find('#backupWithMailCheck');

    var $startBackupBtn = $backupBox.find('#startBackupBtn');

    var $backupProgressBox = $backupBox.find('#backupProgressBox');
    var $backupProgressValue = $backupBox.find('#backupProgressValue');
    var $backupProgressText = $backupBox.find('#backupProgressText');

    var $backupResultLinkBox = $backupBox.find('#backupResultLinkBox');
    var $backupResultLink = $backupBox.find('#backupResultLink');

    var $autoBackupSettingsBox = $view.find('#autoBackupSettingsBox');

    var $autoBackupOn = $autoBackupSettingsBox.find('#autoBackupOn');
    var $autoBackupOff = $autoBackupSettingsBox.find('#autoBackupOff');

    var $autoBackuSettingsBlock = $autoBackupSettingsBox.find('#autoBackuSettingsBlock');
    var $autoBackupSettingsStoragesBox = $autoBackuSettingsBlock.find('#autoBackupSettingsStoragesBox');

    var $autoBackupSettingsTeamlabStorageFolderSelectorBox = $autoBackuSettingsBlock.find('#autoBackupSettingsTeamlabStorageFolderSelectorBox');
    var $autoBackupSettingsTeamlabStorageFolderSelector = $autoBackuSettingsBlock.find('#autoBackupSettingsTeamlabStorageFolderSelector');
    var $autoBackupSettingsTeamlabStorageFolderSelectorBtn = $autoBackuSettingsBlock.find('#autoBackupSettingsTeamlabStorageFolderSelectorBtn');

    var $autoBackupSettingsThirdPartyStorage = $autoBackuSettingsBlock.find('#autoBackupSettingsThirdPartyStorage');
    var $autoBackupSettingsTeamlabStorage = $autoBackuSettingsBlock.find('#autoBackupSettingsTeamlabStorage');

    var $autoBackupSettingsAmazonStorageSettingsBox = $autoBackuSettingsBlock.find('#autoBackupSettingsAmazonStorageSettingsBox');
    var $autoBackupSettingsWithMailCheck = $autoBackuSettingsBlock.find('#autoBackupSettingsWithMailCheck');

    var $saveSettingsBtn = $view.find('#saveSettingsBtn');

    function init() {
        checkSizeAvailable();

        showLoader();

        initFolderSelector();
        initMaxStoredCopiesCount();

        getData(function(response) {
            initAutoBackupSchedule(response);
            bind$Events();

            hideLoader();
            AjaxPro.Backup.GetBackupProgress(processBackupResponseContinue);
        });
    }

    function checkSizeAvailable() {
        var disable = !!$startBackupBtn.data('locked');
        if (jq("#spaceExceedMessage").length) {
            var status = jq("#spaceExceedMessage").data("status");
            if (status == "NotAvailable" || $backupWithMailCheck.is(":checked")) {
                jq("#spaceExceedMessage").show();
                disable = true;
            } else {
                jq("#spaceExceedMessage").hide();
            }
        }
        $startBackupBtn.toggleClass("disable",  disable);
    }

    function getData(callback) {
        AjaxPro.Backup.GetSchedule(callback);
    }

    function initFolderSelector() {
        $('#fileSelectorTree > ul > li.tree-node:not([data-id=\'' + ASC.Files.Constants.FOLDER_ID_COMMON_FILES + '\'])').remove();

        ASC.Files.FileSelector.onInit = function() {
            ASC.Files.FileSelector.toggleThirdParty(true);

            ASC.Files.FileSelector.onThirdPartyTreeCreated = function() {
                $('*:not(.disabled) > .settings-block .thirdPartyStorageSelectorBox')
                    .removeAttr('title')
                    .removeClass('disabled')
                    .find(':radio')
                    .attr('disabled', false);
            };
            ASC.Files.FileSelector.createThirdPartyTree();
        };
    }

    function initMaxStoredCopiesCount() {
        for (var i = 1; i < 31; i++) {
            if (i == 10) {
                $maxStoredCopiesCount.append('<option selected="selected" val=' + i + '>' + i + '</option>');
            } else {
                $maxStoredCopiesCount.append('<option val=' + i + '>' + i + '</option>');
            }
        }
    }

    function initAutoBackupSchedule(response) {
        if (response && response.error) {
            toastr.error(response.error.Message);
        } else if (response && response.value) {
            var value = response.value;
            var storage = {
                name: value.StorageType,
                params: value.StorageParams
            };
            var withMail = value.BackupMail;
            var scheduleTime = value.CronParams;
            var maxStoredCopiesCount = value.BackupsStored;

            switch (storage.name) {
                case 0:
                case 1:
                    $autoBackupSettingsTeamlabStorageFolderSelector.attr('data-folderid', storage.params.FolderId);
                    autoBackupSettingsStorageFolderId = storage.params.FolderId;
                    displayFolderPath($autoBackupSettingsBox, autoBackupSettingsStorageFolderId);
                    getAutoBackupTeamlabFolderStorage(autoBackupSettingsStorageFolderId);
                    break;
                case 2:
                    $autoBackupSettingsAmazonStorageSettingsBox.find('.accessKeyId').val(storage.params.AccessKeyId);
                    $autoBackupSettingsAmazonStorageSettingsBox.find('.secretAccessKey').val(storage.params.SecretAccessKey);
                    $autoBackupSettingsAmazonStorageSettingsBox.find('.bucket').val(storage.params.Bucket);
                    $autoBackupSettingsAmazonStorageSettingsBox.find('.region').val(storage.params.Region);
                    break;
            }

            var $time = $('.select-time-period');
            var $period = $time.find('#generatedTimePeriod');
            var $week = $time.find('#week');
            var $month = $time.find('#month');
            var $hours = $time.find('#hours');

            switch (scheduleTime.Period) {
                case 0:
                    $period.val('day');
                    break;
                case 1:
                    $period.val('week');
                    $week.val(scheduleTime.Day).removeClass('display-none');
                    break;
                case 2:
                    $period.val('month');
                    $month.val(scheduleTime.Day).removeClass('display-none');
                    break;
            }
            $hours.val(scheduleTime.Hour);

            $autoBackupOn.prop('checked', true);
            $autoBackupSettingsStoragesBox.find('input[value="' + storage.name + '"]').prop('checked', true);

            $autoBackupSettingsWithMailCheck.prop('checked', withMail);
            $maxStoredCopiesCount.val(maxStoredCopiesCount);

            $autoBackuSettingsBlock.show();
            $view.show();
        } else if (response && !response.value) {
            autoBackupInitOff = true;
            $autoBackupOff.prop('checked', true);
            $view.show();
        }
    }

    function getAutoBackupTeamlabFolderStorage(folderId) {
        Teamlab.getFolderPath(folderId, {
            success: function(e, data) {
                var path = $(data).map(function(i, folder) {
                    return folder.title;
                });
                var pathTitle = path.toArray().join(' > ');
                $autoBackupSettingsTeamlabStorageFolderSelector.val(pathTitle).attr('data-folderId', folderId);
                $autoBackupSettingsTeamlabStorageFolderSelectorBox.show();
            },
            error: function(params, errors) {
                toastr.error(errors);
            }
        });
    }

    function bind$Events() {
        $backupTeamlabStorageFolderSelectorBtn.on('click', showTeamlabStorageFolderPop);
        $autoBackupSettingsTeamlabStorageFolderSelectorBtn.on('click', showTeamlabStorageFolderPop);

        thirdPartyStorageSelectorHelpers.on('click', showThirdPartyStorageSelectorHelpBox);

        $autoBackupOn.on('change', toggleAutoBackupSettingsBlock);
        $autoBackupOff.on('change', toggleAutoBackupSettingsBlock);

        $backupStoragesBox.find('input[name=backupStorageSelector]').on('change', selectStorage);
        $autoBackupSettingsStoragesBox.find('input[name=autoBackupSettingsStorageSelector]').on('change', selectStorage);

        $startBackupBtn.on('click', startBackup);
        $saveSettingsBtn.on('click', saveSettings);

        $backupWithMailCheck.click(checkSizeAvailable);
    }

    function showTeamlabStorageFolderPop() {
        var $this = $(this);
        if ($this.is('.disable')) {
            return;
        }

        var $box = $(this).closest('.backupBox');
        ASC.Files.FileSelector.onSubmit = function(folderId) {
            displayFolderPath($box, folderId);
        };

        if ($(this).is($backupTeamlabStorageFolderSelectorBtn)) {
            ASC.Files.FileSelector.openDialog(backupStorageFolderId, true, $backupThirdPartyStorage.is(':checked'));
            ASC.Files.FileSelector.setTitle(ASC.Resources.Master.Resource.SelectFolder);
        } else {
            ASC.Files.FileSelector.openDialog(autoBackupSettingsStorageFolderId, true, $autoBackupSettingsThirdPartyStorage.is(':checked'));
            ASC.Files.FileSelector.setTitle(ASC.Resources.Master.Resource.SelectFolder);
        }
    }

    function toggleAutoBackupSettingsBlock() {
        var on = $autoBackupOn.is(':checked');
        if (on) {
            $autoBackuSettingsBlock.show();
            
            if (autoBackupInitOff) {
                autoBackupInitOff = false;
                $autoBackupSettingsTeamlabStorage.click();
            }
        } else {
            $autoBackuSettingsBlock.hide();
        }
    }

    function showThirdPartyStorageSelectorHelpBox() {
        $(this).helper({
            BlockHelperID: $(this).attr('data-helpboxselector')
        });
    }

    function selectStorage() {
        var $box = $(this).closest('.backupBox');

        var $storagesBox = $box.find('.backupStoragesBox');
        var storage = $storagesBox.find(':checked').val();

        var $teamlabStorageFolderSelectorBox = $box.find('.teamlabStorageFolderSelectorBox');
        var $teamlabStorageFolderSelector = $teamlabStorageFolderSelectorBox.find('.teamlabStorageFolderSelector');

        var $amazonStorageSettingsBox = $box.find('.amazonStorageSettingsBox');

        switch (storage) {
            case '0':
                $amazonStorageSettingsBox.hide();

                if ($box.is('#backupBox')) {
                    backupStorageFolderId = ASC.Files.Constants.FOLDER_ID_COMMON_FILES;
                    $teamlabStorageFolderSelector.attr('data-folderid', backupStorageFolderId);
                    displayFolderPath($box, backupStorageFolderId);
                } else {
                    autoBackupSettingsStorageFolderId = ASC.Files.Constants.FOLDER_ID_COMMON_FILES;
                    $teamlabStorageFolderSelector.attr('data-folderid', autoBackupSettingsStorageFolderId);
                    displayFolderPath($box, autoBackupSettingsStorageFolderId);
                }

                $teamlabStorageFolderSelectorBox.show();
                break;
            case '1':
                $amazonStorageSettingsBox.hide();

                $teamlabStorageFolderSelector.val('').attr('data-folderId', '');
                $teamlabStorageFolderSelectorBox.show();
                break;
            case '2':
                $teamlabStorageFolderSelectorBox.hide();
                $amazonStorageSettingsBox.show();
                break;
            case '4':
                $teamlabStorageFolderSelectorBox.hide();
                $amazonStorageSettingsBox.hide();
                break;
        }
    }

    function displayFolderPath($box, folderId) {
        var folderTitle = ASC.Files.FileSelector.fileSelectorTree.getFolderTitle(folderId);

        var path = ASC.Files.FileSelector.fileSelectorTree.getPath(folderId);
        var pathId = $(path).map(function(i, fId) {
            return ASC.Files.FileSelector.fileSelectorTree.getFolderTitle(fId);
        });

        pathId.push(folderTitle);
        var pathTitle = pathId.toArray().join(' > ');

        var $teamlabStorageFolderSelector = $box.find('.teamlabStorageFolderSelector');
        $teamlabStorageFolderSelector.val(pathTitle).attr('data-folderId', folderId);
    }

    function getStorage($box) {
        var $storage = $box.find('.backupStoragesBox :checked');
        var storage = {
            name: $storage.val(),
            params: {}
        };

        $box.find('.teamlabStorageFolderSelector, .amazonStorageSettingsParam').removeClass('with-error');

        switch (storage.name) {
            case '0':
            case '1':
                var $path = $box.find('.teamlabStorageFolderSelector');
                storage.params.FolderId = $path.attr('data-folderid');
                if (!storage.params.FolderId) {
                    $path.addClass('with-error');
                    return false;
                }

                break;
            case '2':
                var $settings = $box.find('.amazonStorageSettingsBox');
                var isError;

                storage.params = {
                    AccessKeyId: $settings.find('.accessKeyId').val(),
                    SecretAccessKey: $settings.find('.secretAccessKey').val(),
                    Bucket: $settings.find('.bucket').val(),
                    Region: $settings.find('.region').val()
                };

                for (var param in storage.params) {
                    if (!storage.params[param]) {
                        var className = param.charAt(0).toLowerCase() + param.slice(1);
                        $settings.find('.' + className).addClass('with-error');
                        isError = true;
                    }
                }
                if (isError) {
                    return;
                }
                break;
        }

        return storage;
    }

    function startBackup() {
        if ($startBackupBtn.is('.disable')) {
            return;
        }

        $backupProgressBox.hide();
        $backupResultLinkBox.hide();

        var storage = getStorage($backupBox);
        var withMail = $backupWithMailCheck.is(':checked');

        if (!storage || $startBackupBtn.hasClass('disable')) {
            return;
        }

        LoadingBanner.showLoaderBtn($backupBox);
        lockBackupBox(true);
        showBackupProgress(0);

        AjaxPro.Backup.StartBackup(storage.name, storage.params, withMail, processBackupResponse);
    }

    function processBackupResponse(response) {
        $backupAmazonStorageSettingsBox.removeClass('with-error');

        if (response.error) {
            showAmazonServerError($backupBox, response.error.Message);
            $backupProgressBox.hide();
            LoadingBanner.hideLoaderBtn($backupBox);
            lockBackupBox(false);
            return;
        } else if (response.value && response.value.IsCompleted) {
            $backupProgressBox.hide();
            LoadingBanner.hideLoaderBtn($backupBox);
            lockBackupBox(false);

            if (response.value.Link) {
                $backupResultLink.attr('href', response.value.Link);
                $backupResultLinkBox.show();
            }
            toastr.success(ASC.Resources.Master.Resource.BackupMakeCopySuccess);
            return;
        }

        processBackupResponseContinue(response);
    }

    function processBackupResponseContinue(response) {
        if (response.error || !response.value || response.value.IsCompleted) {
            return;
        }

        showBackupProgress(response.value.Progress);
        setTimeout(function () {
            AjaxPro.Backup.GetBackupProgress(processBackupResponse);
        }, 1000);
    }

    function lockBackupBox(locked) {
        $backupBox.find('input[name="backupStorageSelector"]:not(#backupThirdPartyStorage)').prop('disabled', locked);
        $backupTeamlabStorageFolderSelector.prop('disabled', locked);
        $backupWithMailCheck.prop('disabled', locked);

        $backupTeamlabStorageFolderSelectorBtn.toggleClass('disable', locked);
        $startBackupBtn.data('locked', locked);
        checkSizeAvailable();

        if (!$backupThirdPartyStorageSelectorBox.is('.disabled')) {
            $backupThirdPartyStorage.prop('disabled', locked);
        }
    }

    function showBackupProgress(progress) {
        $backupProgressValue.animate({ 'width': progress + '%' });
        $backupProgressText.text(progress + '% ');
        $backupProgressBox.show();
    }

    function saveSettings() {
        if ($saveSettingsBtn.hasClass('disable')) {
            return;
        }

        var autoOn = $autoBackupOn.is(':checked');
        if (autoOn) {
            var storage = getStorage($autoBackupSettingsBox);
            var $period = $('.select-time-period');
            var periodTime = {};
            var countCopy = $maxStoredCopiesCount.val();
            var withMail = $autoBackupSettingsWithMailCheck.is(':checked');

            if (!storage) {
                return;
            }

            switch ($period.find('#generatedTimePeriod').val()) {
                case 'day':
                    periodTime.Period = 0;
                    break;
                case 'week':
                    periodTime.Period = 1;
                    break;
                case 'month':
                    periodTime.Period = 2;
                    break;
            }
            periodTime.Hour = $period.find('#hours').val();

            if ($period.find('#week').is(':visible')) {
                periodTime.Day = $period.find('#week').val();
            }
            if ($period.find('#month').is(':visible')) {
                periodTime.Day = $period.find('#month').val();
            }
            LoadingBanner.showLoaderBtn($autoBackupSettingsBox);
            AjaxPro.Backup.CreateSchedule(storage.name, storage.params, countCopy, periodTime, withMail, onCompleteCreateShedule);
        } else {
            LoadingBanner.showLoaderBtn($autoBackupSettingsBox);
            AjaxPro.Backup.DeleteSchedule(onCompleteCreateShedule);
        }
    }

    function onCompleteCreateShedule(response) {
        $autoBackupSettingsBox.find('.with-error').removeClass('with-error');
        if (response.error) {
            showAmazonServerError($autoBackupSettingsBox, response.error.Message);
        } else {
            toastr.success(ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage);
        }
        LoadingBanner.hideLoaderBtn($autoBackupSettingsBox);
    }

    function showAmazonServerError($box, error) {
        var $amazonStorageSettingsBox = $box.find('.amazonStorageSettingsBox');

        switch (error) {
            case 'InvalidAccessKeyId':
                $amazonStorageSettingsBox.find('.accessKeyId').addClass('with-error');
                break;
            case 'SignatureDoesNotMatch':
                $amazonStorageSettingsBox.find('.secretAccessKey').addClass('with-error');
                break;
            case 'NoSuchBucket':
                $amazonStorageSettingsBox.find('.bucket').addClass('with-error');
                break;
            default:
                toastr.error(error);
                break;
        }
    }

    function showLoader() {
        LoadingBanner.displayLoading();
    }

    function hideLoader() {
        LoadingBanner.hideLoading();
    }

    return {
        init: init
    };
};

jq(function() {
    window.BackupManager.init();
});