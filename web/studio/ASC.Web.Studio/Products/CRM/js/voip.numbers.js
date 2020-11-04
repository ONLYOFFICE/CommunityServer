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


if (typeof ASC === "undefined") {
    ASC = {};
}

if (typeof ASC.CRM === "undefined") {
    ASC.CRM = function () { return {} };
}

if (typeof ASC.CRM.Voip === "undefined") {
    ASC.CRM.Voip = function () { return {} };
}

ASC.CRM.Voip.NumbersView = (function ($) {
    var numbers = [],
        operators = [],
        ringtones = [],

        currentNumber = null,
        hash = null,

        timeRegex = /^([01]\d|2[0123]):[0-5]\d$/,
        currentRingtoneType;

    var $view,

        $emptyNumbersListMsg,
        $numberSelector,
        $numberSettingsBox,
        $workingHoursFromInput,
        $workingHoursToInput,
        $ringtonePlayer,
        ringtonePlayer,
        $saveSettingsBtn,
        $showRemoveNumberBtn,
        $removeNumberPopup,
        $operatorsList,
        $addOperatorsBtn;

    function init() {
        hash = window.location.hash.slice(1);

        if (!hash) {
            ASC.CRM.Voip.QuickView.init();
            return;
        }

        cacheElements();
        bindEvents();

        showLoader();

        getData(function(numbersResp, ringtonesResp) {
            saveData(numbersResp, ringtonesResp);
            renderView();

            hideLoader();
        });
    };

    function cacheElements() {
        $view = $('#voip-numbers-view');

        $emptyNumbersListMsg = $view.find('#empty-numbers-list-msg');
        $numberSelector = $view.find('#number-selector');
        $numberSettingsBox = $view.find('#number-settings-box');

        $ringtonePlayer = $view.find('#ringtone-player');
        ringtonePlayer = $ringtonePlayer.get(0);

        $saveSettingsBtn = $view.find('#save-settings-btn');
        $removeNumberPopup = $('#remove-number-popup');
        $showRemoveNumberBtn = $view.find('#show-remove-number-btn');

        $operatorsList = $view.find('#operators-list-body');
        $addOperatorsBtn = $view.find('#add-operators-btn');
    };

    function bindEvents() {
        var clickEventName = "click",
            changeEventName = "change";

        $('body').on(clickEventName, clickHandler);

        $showRemoveNumberBtn.on(clickEventName, showNumberRemovePopup);
        $removeNumberPopup.on(clickEventName, "#remove-number-btn", numberRemoveHandler);
        $removeNumberPopup.on(clickEventName, "#cancel-remove-phone-btn", cancelNumberRemoveHandler);

        $numberSelector.on(changeEventName, numberChangedHandler);

        $numberSettingsBox.on("keydown", function (e) {
            if (e.which == 13) {
                return false;
            }
            
        });
        $numberSettingsBox.on(clickEventName, '#outgoing-calls-setting-btn', quickSettingChangedHandler);
        $numberSettingsBox.on(clickEventName, '#voicemail-setting-btn', quickSettingChangedHandler);
        $numberSettingsBox.on(clickEventName, '#record-incoming-setting-btn', quickSettingChangedHandler);
        $numberSettingsBox.on(clickEventName, '#working-hours-setting-btn', workHoursChangedHandler);

        $numberSettingsBox.on(changeEventName, '.ringtone-selector', ringtoneSettingChangedHandler);
        $numberSettingsBox.on(clickEventName, '.ringtone-play-btn', ringtonePlayHandler);

        $saveSettingsBtn.on(clickEventName, saveSettingsHandler);

        $addOperatorsBtn.on('showList',addOperatorsHandler);
        $operatorsList.on(clickEventName, '.operator .actions', toggleActionsHandler);
        $operatorsList.on(clickEventName, '.operator .actions .delete-operator-btn', deleteOperatorHandler);

        $operatorsList.on(clickEventName, '.operator .outgoing-calls .on_off_button', operatorOutgoingCallsUpdatedHandler);
        $operatorsList.on(clickEventName, '.operator .incoming-recording .on_off_button', operatorIncomingRecordingUpdatedHandler);
    };

    //#region data

    function getData(callback) {
        async.parallel([
            function(cb) {
                Teamlab.getCrmVoipExistingNumbers(null, {
                    success: function(params, numbersResp) {
                        cb(null, numbersResp);
                    },
                    error: function(err) {
                        cb(err);
                    }
                });
            }, function(cb) {
                Teamlab.getVoipUploads({}, {
                    success: function(params, ringtonesResp) {
                        cb(null, ringtonesResp);
                    },
                    error: function(err) {
                        cb(err);
                    }
                });
            }
        ], function(err, data) {
            if (err) {
                showErrorMessage();
            } else {
                callback(data[0], data[1]);
            }
        });
    };

    function saveData(numbersResp, ringtonesResp) {
        ringtones = getTypedRingtones(ringtonesResp);

        for (var i = 0; i < numbersResp.length; i++) {
            var number = numbersResp[i];

            number.settings.ringtones = ringtones;

            for (var j = 0; j < number.settings.operators.length; j++) {
                operators.push(number.settings.operators[j].id);
            }

            number.settings.operators = setOperatorsUserInfo(number.settings.operators);
            numbers.push(number);

            if (number.id == hash) {
                currentNumber = number;
            }
        }

        if (numbers.length && !currentNumber) {
            currentNumber = numbers[0];
        }
    };

    function setOperatorsUserInfo(operatorsResp) {
        if (!operatorsResp || !operatorsResp.length) {
            return operatorsResp;
        }

        for (var i = 0; i < operatorsResp.length; i++) {
            operatorsResp[i].userInfo = window.UserManager.getUser(operatorsResp[i].id);
        }

        return operatorsResp.filter(function (item) { return item.userInfo !== null; });
    };

    function saveNumber(number) {
        number.settings.operators = setOperatorsUserInfo(number.settings.operators);
        for (var i = 0; i < numbers.length; i++) {
            if (numbers[i].id == number.id) {
                numbers[i] = number;
                numbers[i].settings.ringtones = ringtones;
                return;
            }
        }
    };

    function setCurrentNumber(numberId) {
        for (var i = 0; i < numbers.length; i++) {
            var number = numbers[i];
            if (number.id == numberId) {
                currentNumber = number;
                return true;
            }
        }

        return false;
    };

    function addOperators(operatorsResp) {
        operatorsResp = setOperatorsUserInfo(operatorsResp);

        for (var i = 0; i < operatorsResp.length; i++) {
            operators.push(operatorsResp[i].id);
            currentNumber.settings.operators.push(operatorsResp[i]);
        }

        var result = $.extend(true, {}, currentNumber);
        result.settings.operators = operatorsResp;

        return result;
    };

    function deleteOperator(operatorId) {
        for (var i = 0; i < currentNumber.settings.operators.length; i++) {
            if (currentNumber.settings.operators[i].id == operatorId) {
                currentNumber.settings.operators.splice(i, 1);
                break;
            }
        }

        for (var j = 0; j < operators.length; j++) {
            if (operators[j] == operatorId) {
                operators.splice(j, 1);
                break;
            }
        }
    };

    function getTypedRingtones(ringtonesResp) {
        var result = {
            greeting: [],
            hold: [],
            voicemail: [],
            queue: []
        };

        for (var i = 0; i < ringtonesResp.length; i++) {
            var ringtone = ringtonesResp[i];
            var targetRingtones;
            switch (ringtone.audioType) {
                case 0:
                    targetRingtones = result.greeting;
                    break;
                case 1:
                    targetRingtones = result.hold;
                    break;
                case 2:
                    targetRingtones = result.voicemail;
                    break;
                default:
                    targetRingtones = result.queue;
                    break;
            }

            targetRingtones.push(ringtone);
        }

        return result;
    };

    function addRingtone(ringtone) {
        var targetRingtones;

        switch (ringtone.audioType) {
            case 0:
                targetRingtones = ringtones.greeting;
                break;
            case 1:
                targetRingtones = ringtones.hold;
                break;
            case 2:
                targetRingtones = ringtones.voicemail;
                break;
            default:
                targetRingtones = ringtones.queue;
                break;
        }

        targetRingtones.push(ringtone);
    };

    //#endregion

    function createFileuploadInput(browseButtonId, audioType) {
        var buttonObj = jq("#" + browseButtonId);

        var inputObj = jq("<input/>")
            .attr("id", "fileupload_" + audioType)
            .attr("type", "file")
            .attr("multiple", "multiple")
            .css("width", "0")
            .css("height", "0");

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on("click", function(e) {
            e.preventDefault();
            jq("#fileupload_" + audioType).click();
        });

        return inputObj;
    };

    function getFileExtension(fileTitle) {
        if (typeof fileTitle == "undefined" || fileTitle == null) {
            return "";
        }
        fileTitle = fileTitle.trim();
        var posExt = fileTitle.lastIndexOf(".");
        return 0 <= posExt ? fileTitle.substring(posExt).trim().toLowerCase() : "";
    };

    function correctFile(file) {
        if (getFileExtension(file.name) != ".mp3") {
            toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileFormatErrorMsg);
            return false;
        }

        if (file.size <= 0) {
            toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneEmptyFileErrorMsg);
            return false;
        }

        if (file.size > 20 * 1024 * 1024) {
            toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileSizeErrorMsg);
            return false;
        }

        return true;
    };

    function bindUploader(browseButtonId, audioType, selectorId) {
        var uploader = createFileuploadInput(browseButtonId, audioType);

        uploader.fileupload({
            url: "ajaxupload.ashx?type=ASC.Web.CRM.Controls.Settings.VoipUploadHandler,ASC.Web.CRM",
            autoUpload: false,
            singleFileUploads: true,
            sequentialUploads: true,
            progressInterval: 1000,
            formData: [
                {
                    name: "audioType",
                    value: audioType
                }
            ],
            dropZone: jq("#" + browseButtonId).parents(".ringtone-setting-item")
        });

        uploader
            .bind("fileuploadadd", function (e, data) {
                if (correctFile(data.files[0]) ) {
                    data.submit();
                }
            })
            .bind("fileuploaddone", function(e, data) {
                var response = $.parseJSON(data.result);
                if (!response.Success || !response.Data) {
                    if (response.Message) {
                        toastr.error(response.Message);
                    } else {
                        toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileErrorMsg);
                    }
                    hideLoader();
                    return;
                }
                var newRingtone = {
                    name: response.Data.Name,
                    path: response.Data.Path,
                    audioType: response.Data.AudioType,
                    isDefault: response.Data.isDefault
                };
                addRingtone(newRingtone);

                var $selector = $('#' + selectorId);
                var $newOption = jq.tmpl("voip-ringtone-selector-option-tmpl", newRingtone);

                var $player = $selector.siblings('.ringtone-player');
                var $playBtn = $selector.siblings('.ringtone-play-btn');

                $selector.append($newOption);

                $selector.val(newRingtone.path);
                $player.attr('src', newRingtone.path);
                $playBtn.removeClass('disable');
            })
            .bind("fileuploadfail", function() {
                hideLoader();
                toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileErrorMsg);
            })
            .bind("fileuploadstart", showLoader)
            .bind("fileuploadstop", hideLoader);
    };

    //#region rendering

    function renderView() {
        if (numbers.length) {
            var $numberSelectorOptions = jq.tmpl("voip-number-selector-option-tmpl", numbers);
            $numberSelector.append($numberSelectorOptions);
            $numberSelector.val(currentNumber.id);
            renderSettings();
        } else {
            $emptyNumbersListMsg.show();
        }

        $view.show();
    };

    function renderSettings() {
        renderGeneralSettings();
        renderOperators();
    };

    function renderGeneralSettings() {
        if (!currentNumber) {
            return;
        }

        var $settings = jq.tmpl("voip-settings-tmpl", currentNumber);
        $numberSettingsBox.html($settings);

        $workingHoursFromInput = $numberSettingsBox.find("#working-hours-from-input");
        $workingHoursToInput = $numberSettingsBox.find("#working-hours-to-input");

        bindUploader('greeting-ringtone-load-btn', 0, 'greeting-ringtone-selector');
        bindUploader('queue-wait-ringtone-load-btn', 3, 'queue-wait-ringtone-selector');
        bindUploader('hold-ringtone-load-btn', 1, 'hold-ringtone-selector');
        bindUploader('voicemail-ringtone-load-btn', 2, 'voicemail-ringtone-selector');

        $numberSettingsBox.find('#greeting-ringtone-player').bind('ended', recorPlayerEndedHandler);
        $numberSettingsBox.find('#hold-ringtone-player').bind('ended', recorPlayerEndedHandler);
        $numberSettingsBox.find('#voicemail-ringtone-player').bind('ended', recorPlayerEndedHandler);
    };

    function renderOperators() {
        if (!currentNumber) {
            return;
        }

        $addOperatorsBtn.useradvancedSelector({ showGroups: true });
        $addOperatorsBtn.useradvancedSelector('disable', operators);

        var $operators = jq.tmpl("voip-operator-tmpl", currentNumber);
        $operatorsList.html($operators);
    };

    function renderOperatorSettingsChanges(number) {
        if (number.settings.allowOutgoingCalls) {
            $operatorsList.find('.outgoing-calls .on_off_button').removeClass('disable');
        } else {
            $operatorsList.find('.outgoing-calls .on_off_button').removeClass('on').addClass('off').addClass('disable');
        }

        if (number.settings.record) {
            $operatorsList.find('.incoming-recording .on_off_button').removeClass('disable');
        } else {
            $operatorsList.find('.incoming-recording .on_off_button').removeClass('on').addClass('off').addClass('disable');
        }
    };

    function renderAddedOperators(addedOperators) {
        $addOperatorsBtn.useradvancedSelector('disable', operators);

        addedOperators = setOperatorsUserInfo(addedOperators);
        var $operators = jq.tmpl("voip-operator-tmpl", addedOperators);
        $operatorsList.append($operators);
    };

    function renderDeletedOperator(operatorId) {
        $operatorsList.find('.operator[data-operatorid=' + operatorId + ']').remove();
        $addOperatorsBtn.useradvancedSelector('undisable', [operatorId]);
    };

    //#endregion

    //#region handlers

    function clickHandler(e) {
        var $this = $(e.target);

        if (!$this.is('.actions')) {
            $view.find('.operator').removeClass('selected');
            $view.find('.studio-action-panel').hide();
        }
    };

    function toggleActionsHandler(e) {
        var $this = $(e.target);
        var $panel = $this.find('.studio-action-panel');
        var $operator = $this.closest('.operator');

        var visible = $panel.is(':visible');
        if (visible) {
            $panel.hide();
            $operator.removeClass('selected');
        } else {
            $panel.css({
                top: $this.outerHeight(),
                left: $this.width() - $panel.width()
            });
            $panel.show();
            $operator.addClass('selected');
        }
    };

    function showNumberRemovePopup() {
        StudioBlockUIManager.blockUI($removeNumberPopup, 550);
    }

    function numberRemoveHandler() {
        Teamlab.removeCrmVoipNumber({}, $numberSelector.val(), {
            before: showLoader,
            after: hideLoader,
            success: function () {
                location.hash = "";
                location.reload();
            },
            error: showErrorMessage
        });
    };

    function cancelNumberRemoveHandler() {
        jq.unblockUI();
    };

    function numberChangedHandler() {
        var changedNumberId = $numberSelector.val();
        setCurrentNumber(changedNumberId);

        window.location.hash = changedNumberId;

        renderSettings();
    };

    function quickSettingChangedHandler(e) {
        var $this = $(e.target);
        var on = $this.is('.on');
        if (on) {
            $this.removeClass('on').addClass('off');
        } else {
            $this.removeClass('off').addClass('on');
        }
        return on;
    };

    function workHoursChangedHandler(e) {
        var wasOn = quickSettingChangedHandler(e);
        if (wasOn) {
            $workingHoursFromInput.add($workingHoursToInput).val("").attr("disabled", "disabled");
        } else {
            $workingHoursFromInput.add($workingHoursToInput).removeAttr("disabled");
            $workingHoursFromInput.val("6:00");
            $workingHoursToInput.val("23:00");
            $workingHoursFromInput.focus();
        }

    };

    function saveSettingsHandler() {
        var $workingHoursInvalidFormatError = $numberSettingsBox.find('#working-hours-invalid-format-error');
        var $workingHoursInvalidIntervalError = $numberSettingsBox.find('#working-hours-invalid-interval-error');

        $workingHoursInvalidFormatError.hide();
        $workingHoursInvalidIntervalError.hide();

        var whEnabled = $numberSettingsBox.find('#working-hours-setting-btn').is('.on');
        var whFrom = $workingHoursFromInput.val().trim();
        var whTo = $workingHoursToInput.val().trim();

        if (whFrom.length === 4 && whFrom.indexOf("0") !== 0) {
            whFrom = "0" + whFrom;
            $workingHoursFromInput.val(whFrom);
        }
        if (whTo.length === 4 && whTo.indexOf("0") !== 0) {
            whTo = "0" + whTo;
            $workingHoursToInput.val(whTo);
        }

        if (whEnabled && (!timeRegex.test(whFrom) || !timeRegex.test(whTo))) {
            $workingHoursInvalidFormatError.show();
            return;
        }

        var fromNumber = +whFrom.replace(':', '');
        var toNumber = +whTo.replace(':', '');
        if (whEnabled && fromNumber >= toNumber) {
            $workingHoursInvalidIntervalError.show();
            return;
        }

        var obj = {
            alias: $numberSettingsBox.find('#number-alias-input').val(),
            allowOutgoingCalls: $numberSettingsBox.find('#outgoing-calls-setting-btn').is('.on'),
            voiceMail: $numberSettingsBox.find('#voicemail-ringtone-selector').val(),
            record: $numberSettingsBox.find('#record-incoming-setting-btn').is('.on'),
            workingHours: {
                enabled: whEnabled,
                from: whFrom,
                to: whTo
            },
            greeting: $numberSettingsBox.find('#greeting-ringtone-selector').val(),
            holdUp: $numberSettingsBox.find('#hold-ringtone-selector').val(),
            wait: $numberSettingsBox.find('#queue-wait-ringtone-selector').val()
        };

        showLoader();

        Teamlab.updateCrmVoipNumberSettings(null, currentNumber.id, obj, {
            success: function(params, number) {
                saveNumber(number);
                renderOperatorSettingsChanges(number);
                hideLoader();
                showSuccessOpearationMessage();
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    };

    function ringtoneSettingChangedHandler(e) {
        var $this = $(e.target);
        var ringtone = $this.val();

        if ($this.siblings('.ringtone-play-btn').attr('data-type') == currentRingtoneType) {
            stopRingtonePlayer();
        }

        var $playBtn = $this.closest('.ringtone-setting-item').find('.ringtone-play-btn');
        if (ringtone) {
            $playBtn.removeClass('disable').removeClass('__stop').addClass('__play');
        } else {
            $playBtn.addClass('disable');
        }
    };

    function ringtonePlayHandler(e) {
        var $this = $(e.target);
        if ($this.is('.disable')) return false;

        ringtonePlayer.setAttribute('src', $this.siblings('.ringtone-selector').val());

        var playOn = $this.is('.__stop');

        stopRingtonePlayer();

        if (!playOn) {
            currentRingtoneType = $this.attr('data-type');
            ringtonePlayer.play();
            $this.removeClass('__play').addClass('__stop');
        }

        return false;
    };

    function stopRingtonePlayer() {
        if (ringtonePlayer.readyState != 0) {
            ringtonePlayer.pause();
            ringtonePlayer.currentTime = 0;
        }

        $numberSettingsBox.find('.ringtone-play-btn.__stop').removeClass('__stop').addClass('__play');
    };

    function recorPlayerEndedHandler(e) {
        var $player = $(e.target);
        var player = $player.get(0);

        player.currentTime = 0;
        player.pause();

        $player.closest('.ringtone-setting-item').find('.ringtone-play-btn').removeClass('__stop').addClass('__play');
    };

    function addOperatorsHandler(e, addedOperators) {
        var ids = addedOperators.map(function(o) {
            return o.id;
        });

        if (!ids.length) {
            return;
        }

        showLoader();

        Teamlab.addCrmVoipNumberOperators(null, currentNumber.id, { operators: ids }, {
            success: function(params, operatorsResp) {
                var number = addOperators(operatorsResp);
                renderAddedOperators(number);
                hideLoader();
            },
            error: function() {
                showErrorMessage();
                hideLoader();
            }
        });
    };

    function deleteOperatorHandler(e) {
        var operatorId = $(e.target).closest('.operator').attr('data-operatorid');

        showLoader();

        Teamlab.removeCrmVoipNumberOperators(null, currentNumber.id, { oper: operatorId }, {
            success: function() {
                deleteOperator(operatorId);
                renderDeletedOperator(operatorId);
                hideLoader();
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    };

    function operatorOutgoingCallsUpdatedHandler(e) {
        var on = $(e.target).is('.off');
        var setting = { allowOutgoingCalls: on };

        operatorSettingUpdatedHandler(e, setting);
    };

    function operatorIncomingRecordingUpdatedHandler(e) {
        var on = $(e.target).is('.off');
        var setting = { record: on };

        operatorSettingUpdatedHandler(e, setting);
    };

    function operatorSettingUpdatedHandler(e, setting) {
        var $this = $(e.target);
        if ($this.is('.disable')) {
            return;
        }

        var operatorId = $this.closest('.operator').attr('data-operatorId');
        var on = $this.is('.off');

        showLoader();

        Teamlab.updateCrmVoipOperator(null, operatorId, setting, {
            success: function(params, data) {
                if (on) {
                    $this.removeClass('off').addClass('on');
                } else {
                    $this.removeClass('on').addClass('off');
                }

                saveNumber(currentNumber);

                hideLoader();
                showSuccessOpearationMessage();
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    };

    //#endregion

    //#region utils

    function showLoader() {
        LoadingBanner.displayLoading();
    };

    function hideLoader() {
        LoadingBanner.hideLoading();
    };

    function showSuccessOpearationMessage() {
        toastr.success(ASC.Resources.Master.Resource.ChangesSuccessfullyAppliedMsg);
    };

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    };

    //#endregion

    return {
        init : init
    };
})(jq);

jq(function() {
    ASC.CRM.Voip.NumbersView.init();
});