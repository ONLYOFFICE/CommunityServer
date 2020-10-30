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
    var $autobackupConsumerStorageSettingsBox = $autoBackuSettingsBlock.find('#backupConsumerStorageScheduleSettingsBox');
    var $autoBackupSettingsTeamlabStorage = $autoBackuSettingsBlock.find('#autoBackupSettingsTeamlabStorage');

    var $autoBackupSettingsWithMailCheck = $autoBackuSettingsBlock.find('#autoBackupSettingsWithMailCheck');

    var $saveSettingsBtn = $view.find('#saveSettingsBtn');

    var $spaceExceedMessage = jq("#spaceExceedMessage");

    var storageTypes = {
        Temp: "4",
        Docs: "0",
        ThirdPartyDocs: "1",
        Consumers: "5"
    }

    var displayNoneClass = "display-none", withErrorClass = "withError", disable = "disable";
    var resource = ASC.Resources.Master.Resource;

    function init() {
        checkSizeAvailable();

        showLoader();

        initFolderSelector();
        initMaxStoredCopiesCount();

        Teamlab.getBackupSchedule({},
        {
            success: function (params, response) {
                initAutoBackupSchedule(response);
                bind$Events();

                hideLoader();
                Teamlab.getBackupProgress({},
                {
                    success: function (params, data) {
                        processBackupResponseContinue(data);
                    }
                });
            },
            error: function (params, errors) {
                toastr.error(errors[0]);
            }
        });

        Teamlab.getBackupStorages({},
        {
            success: function (params, response) {
                initConsumerStorages(response);
            },
            error: function (params, errors) {
                toastr.error(errors[0]);
            }
        });
    }

    function checkSizeAvailable() {
        var disable = !!$startBackupBtn.data('locked');
        if ($spaceExceedMessage.length) {
            var status = $spaceExceedMessage.data("status");
            if (status == "NotAvailable" || $backupWithMailCheck.is(":checked")) {
                $spaceExceedMessage.show();
                disable = true;
            } else {
                $spaceExceedMessage.hide();
            }
        }
        $startBackupBtn.toggleClass("disable", disable);

        jq("#autoBackupOn").attr("disabled", disable);
        if (disable && !jq("#autoBackupOn").is(":checked")) {
            jq("#saveSettingsBtn, #autoBackuSettingsBlock").remove();
        }
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
        if (typeof (response.storageType) !== "undefined") {
            var value = response;
            var storage = {
                name: value.storageType,
                params: value.storageParams
            };
            var withMail = value.backupMail;
            var scheduleTime = value.cronParams;
            var maxStoredCopiesCount = value.backupsStored;
            var storageType = storage.name.toString();

            switch (storageType) {
                case storageTypes.Docs:
                case storageTypes.ThirdPartyDocs:
                    $autoBackupSettingsTeamlabStorageFolderSelector.attr('data-folderid', storage.params["folderId"]);
                    autoBackupSettingsStorageFolderId = storage.params["folderId"];
                    displayFolderPath($autoBackupSettingsBox, autoBackupSettingsStorageFolderId);
                    getAutoBackupTeamlabFolderStorage(autoBackupSettingsStorageFolderId);
                    break;
                case storageTypes.Consumers:
                    $autobackupConsumerStorageSettingsBox.removeClass(displayNoneClass);
                    break;
            }

            var $time = $('.select-time-period');
            var $period = $time.find('#generatedTimePeriod');
            var $week = $time.find('#week');
            var $month = $time.find('#month');
            var $hours = $time.find('#hours');

            switch (scheduleTime.period) {
                case 0:
                    $period.val('day');
                    break;
                case 1:
                    $period.val('week');
                    $week.val(scheduleTime.day).removeClass('display-none');
                    break;
                case 2:
                    $period.val('month');
                    $month.val(scheduleTime.day).removeClass('display-none');
                    break;
            }
            $hours.val(scheduleTime.hour);

            $autoBackupOn.prop('checked', true);
            $autoBackupSettingsStoragesBox.find('input[value="' + storage.name + '"]').prop('checked', true);

            $autoBackupSettingsWithMailCheck.prop('checked', withMail);
            $maxStoredCopiesCount.val(maxStoredCopiesCount);

            $autoBackuSettingsBlock.show();
            $view.show();
        } else {
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
            ASC.Files.FileSelector.setTitle(resource.SelectFolder);
        } else {
            ASC.Files.FileSelector.openDialog(autoBackupSettingsStorageFolderId, true, $autoBackupSettingsThirdPartyStorage.is(':checked'));
            ASC.Files.FileSelector.setTitle(resource.SelectFolder);
        }
    }

    function initConsumerStorages(storages) {
        if (!storages.length) return;

        var $input = $view.find("#backupConsumerStorage,#scheduleConsumerStorage");
        $input.removeAttr("disabled");
        $input.parent("li").removeClass("disabled");

        var $backupConsumerStorageSettingsBox = $view.find("#backupConsumerStorageSettingsBox");
        var selectedConsumer = storages.find(function (item) { return item.isSet }) || storages[1];
        initConsumerStorage($backupConsumerStorageSettingsBox, selectedConsumer, storages);

        var $backupConsumerStorageScheduleSettingsBox = $view.find("#backupConsumerStorageScheduleSettingsBox");
        selectedConsumer = storages.find(function (item) { return item.current }) || selectedConsumer;
        initConsumerStorage($backupConsumerStorageScheduleSettingsBox, selectedConsumer, storages);
        for (var i = 0; i < selectedConsumer.properties.length; i++) {
            var prop = selectedConsumer.properties[i];
            $backupConsumerStorageScheduleSettingsBox.find("[data-id='" + prop.title + "']").val(prop.value);
        }
    }

    function initConsumerStorage($box, selectedConsumer, storages) {
        var textBoxClass = ".textBox";
        var textEditClass = ".textEdit";

        $box.html(jq.tmpl("consumerSettingsTmpl", { storages: storages, selectedConsumer: selectedConsumer }));
        $box.off("change" + textEditClass).on("change" + textEditClass, textEditClass, function () {
            $box.find(textBoxClass).val('');

            var newVal = jq(this).find(":selected").val();
            $box.find(textBoxClass).removeClass(withErrorClass);
            $box.find("div[data-id]").addClass(displayNoneClass);
            $box.find("[data-id='" + newVal + "']").removeClass(displayNoneClass);
            $startBackupBtn.addClass(disable);
        });
        $box.find("select option[value='" + selectedConsumer.id + "']").attr("selected", "selected");
        $box.find("select");
        $box.off("input" + textBoxClass).on("input" + textBoxClass, textBoxClass, function () {
            var $self = $(this);
            var $siblings = Array.from($self.siblings());
            $siblings.push(this);

            $self.removeClass(withErrorClass);

            function notEmpty(item) {
                return item.value.length > 0;
            }

            if ($siblings.every(notEmpty)) {
                $startBackupBtn.removeClass(disable);
            } else {
                $startBackupBtn.addClass(disable);
            }
        });
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

        var $backupConsumerStorageSettingsBox = $box.find('.consumerStorageSettingsBox');
        $teamlabStorageFolderSelectorBox.addClass(displayNoneClass);
        $backupConsumerStorageSettingsBox.addClass(displayNoneClass);

        if ($spaceExceedMessage.length) {
            var status = $spaceExceedMessage.data("status");
            if (status == "NotAvailable") {
                $startBackupBtn.addClass(disable);
            } else {
                $startBackupBtn.removeClass(disable);
            }
        } else {
            $startBackupBtn.removeClass(disable);
        }

        switch (storage) {
            case storageTypes.Docs:
                if ($box.is('#backupBox')) {
                    backupStorageFolderId = ASC.Files.Constants.FOLDER_ID_COMMON_FILES;
                    $teamlabStorageFolderSelector.attr('data-folderid', backupStorageFolderId);
                    displayFolderPath($box, backupStorageFolderId);
                } else {
                    autoBackupSettingsStorageFolderId = ASC.Files.Constants.FOLDER_ID_COMMON_FILES;
                    $teamlabStorageFolderSelector.attr('data-folderid', autoBackupSettingsStorageFolderId);
                    displayFolderPath($box, autoBackupSettingsStorageFolderId);
                }

                $teamlabStorageFolderSelectorBox.removeClass(displayNoneClass);
                break;
            case storageTypes.ThirdPartyDocs:
                $teamlabStorageFolderSelector.val('').attr('data-folderId', '');
                $teamlabStorageFolderSelectorBox.removeClass(displayNoneClass);
                break;
            case storageTypes.Consumers:
                if (!$view.find("#backupConsumerStorage").parent("li").hasClass("disabled")) {
                    $backupConsumerStorageSettingsBox.find("select").trigger("change");
                    $backupConsumerStorageSettingsBox.removeClass(displayNoneClass);
                }
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
            storageType: $storage.val(),
            storageParams: []
        };

        $box.find('.teamlabStorageFolderSelector').removeClass('with-error');

        switch (storage.storageType) {
            case storageTypes.Docs:
            case storageTypes.ThirdPartyDocs:
                var $path = $box.find('.teamlabStorageFolderSelector');
                var val = $path.attr('data-folderid');
                storage.storageParams.push({ key: "folderId", value: val });
                if (!val) {
                    $path.addClass('with-error');
                    return false;
                }

                break;
            case storageTypes.Consumers:
                var isError;
                var $selectedConsumer = $box.find('.consumerStorageSettingsBox');
                var selectedConsumer = $selectedConsumer.find('.textEdit :selected').val();
                var $settings = $selectedConsumer.find('div[data-id="' + selectedConsumer + '"] .textBox');
                storage.storageParams.push({ key: "module", value: selectedConsumer });
                var settingsLength = $settings.length;
                for (var i = 0; i < settingsLength; i++) {
                    var $item = $($settings[i]);
                    if (!$item.val()) {
                        $item.addClass(withErrorClass);
                        isError = true;
                    }
                    else {
                        storage.storageParams.push({ key: $item.attr("data-id"), value: $item.val() });
                    }
                }

                if (isError) {
                    return false;
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
        storage.backupMail = $backupWithMailCheck.is(':checked');

        if (!storage) {
            return;
        }

        LoadingBanner.showLoaderBtn($backupBox);
        lockBackupBox(true);
        showBackupProgress(0);

        Teamlab.startbackup({}, storage,
        {
            success: function(params, data) {
                processBackupResponse(data);
            },
            error: function(params, errors) {
                showAmazonServerError($backupBox, errors[0]);
                $backupProgressBox.hide();
                LoadingBanner.hideLoaderBtn($backupBox);
                lockBackupBox(false);
            }
        });
    }

    function processBackupResponse(response) {
        if (response && response.isCompleted) {
            $backupProgressBox.hide();
            LoadingBanner.hideLoaderBtn($backupBox);
            lockBackupBox(false);

            if (response.link) {
                $backupResultLink.attr('href', response.link);
                $backupResultLinkBox.show();
            }
            toastr.success(resource.BackupMakeCopySuccess);
            return;
        }

        processBackupResponseContinue(response);
    }

    function processBackupResponseContinue(response) {
        if (typeof(response.isCompleted) === "undefined" || response.isCompleted) {
            return;
        }

        showBackupProgress(response.progress);
        setTimeout(function () {
            Teamlab.getBackupProgress({},
            {
                success: function (params, data) {
                    processBackupResponse(data);
                },
                error: function (params, errors) {
                    $backupProgressBox.hide();
                    LoadingBanner.hideLoaderBtn($backupBox);
                    lockBackupBox(false);
                    toastr.error(resource.CommonJSErrorMsg);
                },
                max_request_attempts: 1
            });
        }, 1000);
    }

    function lockBackupBox(locked) {
        $backupBox.find('input[name="backupStorageSelector"]:not(#backupThirdPartyStorage)').prop('disabled', locked);
        $backupTeamlabStorageFolderSelector.prop('disabled', locked);
        $backupWithMailCheck.prop('disabled', locked);

        $backupTeamlabStorageFolderSelectorBtn.toggleClass(disable, locked);
        $startBackupBtn.data('locked', locked);
        $startBackupBtn.toggleClass(disable, locked);
        jq("#saveSettingsBtn").toggleClass(disable, locked);

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
        if ($saveSettingsBtn.hasClass(disable)) {
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

            storage.backupsStored = countCopy;
            storage.cronParams = periodTime;
            storage.backupMail = withMail;

            Teamlab.createBackupSchedule({}, storage,
            {
                success: function(params, data) {
                    onCompleteCreateShedule(data);
                },
                error: function(params, data) {
                    oErrorCreateShedule(data);
                }
            });
        } else {
            LoadingBanner.showLoaderBtn($autoBackupSettingsBox);

            Teamlab.deleteBackupSchedule({},
            {
                success: function (params, data) {
                    onCompleteCreateShedule(data);
                },
                error: function (params, data) {
                    oErrorCreateShedule(data);
                }
            });
        }
    }

    function onCompleteCreateShedule() {
        $autoBackupSettingsBox.find('.with-error').removeClass('with-error');
        toastr.success(resource.SuccessfullySaveSettingsMessage);
        LoadingBanner.hideLoaderBtn($autoBackupSettingsBox);
    }

    function oErrorCreateShedule(errors) {
        showAmazonServerError($autoBackupSettingsBox, errors[0]);
    }


    function showAmazonServerError($box, error) {
        switch (error) {
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