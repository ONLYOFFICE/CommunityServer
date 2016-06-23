/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


var $ = jq;

var VoIPNumbersView = {
    numbers: [],
    operators: [],
    ringtones: [],

    currentNumber: null,
    hash: null,

    timeRegex: /^([01]\d|2[0123]):[0-5]\d$/,

    init: function() {
        this.hash = window.location.hash.slice(1);
        
        if (!this.hash) {
            VoIPQuickView.init();
            return;
        }

        this.cacheElements();
        this.bindEvents();

        this.showLoader();

        var self = this;
        this.getData(function(numbers, ringtones) {
            self.saveData(numbers, ringtones);
            self.renderView();

            self.hideLoader();
        });
    },

    cacheElements: function() {
        this.numberSelectorOptionTmpl = $('#number-selector-option-tmpl');
        this.settingsTmpl = $('#settings-tmpl');
        this.ringtoneSelectorOptionTmpl = $('#ringtone-selector-option-tmpl');
        this.operatorTmpl = $('#operator-tmpl');

        this.$view = $('#voip-numbers-view');

        this.$emptyNumbersListMsg = this.$view.find('#empty-numbers-list-msg');
        this.$numberSelector = this.$view.find('#number-selector');
        this.$numberSettingsBox = this.$view.find('#number-settings-box');

        this.$ringtonePlayer = this.$view.find('#ringtone-player');
        this.ringtonePlayer = this.$ringtonePlayer.get(0);

        this.$saveSettingsBtn = this.$view.find('#save-settings-btn');
        this.$deleteNumberBtn = this.$view.find('#delete-number-btn');

        this.$operatorsList = this.$view.find('#operators-list-body');
        this.$addOperatorsBtn = this.$view.find('#add-operators-btn');
    },

    bindEvents: function() {
        $('body').on('click', this.clickHandler.bind(this));

        this.$numberSelector.on('change', this.numberChangedHandler.bind(this));

        this.$numberSettingsBox.on('click', '#outgoing-calls-setting-btn', this.quickSettingChangedHandler.bind(this));
        this.$numberSettingsBox.on('click', '#voicemail-setting-btn', this.quickSettingChangedHandler.bind(this));
        this.$numberSettingsBox.on('click', '#record-incoming-setting-btn', this.quickSettingChangedHandler.bind(this));
        this.$numberSettingsBox.on('click', '#working-hours-setting-btn', this.quickSettingChangedHandler.bind(this));

        this.$numberSettingsBox.on('change', '.ringtone-selector', this.ringtoneSettingChangedHandler.bind(this));
        this.$numberSettingsBox.on('click', '.ringtone-play-btn', this.ringtonePlayHandler.bind(this));

        this.$saveSettingsBtn.on('click', this.saveSettingsHandler.bind(this));

        this.$addOperatorsBtn.on('showList', this.addOperatorsHandler.bind(this));
        this.$operatorsList.on('click', '.operator .actions', this.toggleActionsHandler.bind(this));
        this.$operatorsList.on('click', '.operator .actions .delete-operator-btn', this.deleteOperatorHandler.bind(this));
        
        this.$operatorsList.on('click', '.operator .outgoing-calls .on_off_button', this.operatorOutgoingCallsUpdatedHandler.bind(this));
        this.$operatorsList.on('click', '.operator .incoming-recording .on_off_button', this.operatorIncomingRecordingUpdatedHandler.bind(this));
    },

    //#region data

    getData: function(callback) {
        async.parallel([
                function(cb) {
                    Teamlab.getCrmVoipExistingNumbers(null, {
                        success: function(params, numbers) {
                            cb(null, numbers);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                }, function(cb) {
                    Teamlab.getVoipUploads({}, {
                        success: function(params, ringtones) {
                            cb(null, ringtones);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                }], function(err, data) {
                    if (err) {
                        self.showErrorMessage();
                    } else {
                        callback(data[0], data[1]);
                    }
                });
    },

    saveData: function(numbers, ringtones) {
        this.ringtones = this.getTypedRingtones(ringtones);

        for (var i = 0; i < numbers.length; i++) {
            var number = numbers[i];

            number.settings.ringtones = this.ringtones;

            for (var j = 0; j < number.settings.operators.length; j++) {
                this.operators.push(number.settings.operators[j].id);
            }

            this.setOperatorsUserInfo(number.settings.operators);
            this.numbers.push(number);
            
            if (number.id == this.hash) {
                this.currentNumber = number;
            }
        }

        if (this.numbers.length && !this.currentNumber) {
            this.currentNumber = this.numbers[0];
        }
    },

    setOperatorsUserInfo: function(operators) {
        if (!operators || !operators.length) {
            return;
        }

        for (var i = 0; i < operators.length; i++) {
            operators[i].userInfo = this.getUserInfo(operators[i].id);
        }
    },

    getUserInfo: function(id) {
        var users = ASC.Resources.Master.ApiResponses_Profiles.response;
        if (!users) {
            return null;
        }

        for (var i = 0; i < users.length; i++) {
            if (users[i].id == id) {
                return users[i];
            }
        }

        return null;
    },

    saveNumber: function(number) {
        for (var i = 0; i < this.numbers.length; i++) {
            if (this.numbers[i].id == number.id) {
                this.numbers[i] = number;
                this.numbers[i].settings.ringtones = this.ringtones;
                return;
            }
        }
    },

    setCurrentNumber: function(numberId) {
        for (var i = 0; i < this.numbers.length; i++) {
            var number = this.numbers[i];
            if (number.id == numberId) {
                this.currentNumber = number;
                return true;
            }
        }

        return false;
    },

    addOperators: function(operators) {
        this.setOperatorsUserInfo(operators);

        for (var i = 0; i < operators.length; i++) {
            this.operators.push(operators[i].id);
            this.currentNumber.settings.operators.push(operators[i]);
        }

        var result = $.extend(true, {}, this.currentNumber);
        result.settings.operators = operators;

        return result;
    },

    deleteOperator: function(operatorId) {
        for (var i = 0; i < this.currentNumber.settings.operators.length; i++) {
            if (this.currentNumber.settings.operators[i].id == operatorId) {
                this.currentNumber.settings.operators.splice(i, 1);
                break;
            }
        }

        for (var j = 0; j < this.operators.length; j++) {
            if (this.operators[j] == operatorId) {
                this.operators.splice(j, 1);
                break;
            }
        }
    },

    getTypedRingtones: function(ringtones) {
        var result = {
            greeting: [],
            hold: [],
            voicemail: [],
            queue: []
        };

        for (var i = 0; i < ringtones.length; i++) {
            var ringtone = ringtones[i];
            var targetRingtones;

            if (ringtone.audioType == 0) {
                targetRingtones = result.greeting;
            } else if (ringtone.audioType == 1) {
                targetRingtones = result.hold;
            } else if (ringtone.audioType == 2) {
                targetRingtones = result.voicemail;
            } else {
                targetRingtones = result.queue;
            }

            targetRingtones.push(ringtone);
        }

        return result;
    },

    addRingtone: function(ringtone) {
        var targetRingtones;

        if (ringtone.audioType == 0) {
            targetRingtones = this.ringtones.greeting;
        } else if (ringtone.audioType == 1) {
            targetRingtones = this.ringtones.hold;
        } else if (ringtone.audioType == 2) {
            targetRingtones = this.ringtones.voicemail;
        } else {
            targetRingtones = this.ringtones.queue;
        }

        targetRingtones.push(ringtone);
    },
    
    //#endregion

    createFileuploadInput: function (browseButtonId, audioType) {
        var buttonObj = jq("#" + browseButtonId);

        var inputObj = jq("<input/>")
            .attr("id", "fileupload_" + audioType)
            .attr("type", "file")
            .attr("multiple", "multiple")
            .css("width", "0")
            .css("height", "0");

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on("click", function (e) {
            e.preventDefault();
            jq("#fileupload_" + audioType).click();
        });

        return inputObj;
    },

    getFileExtension: function (fileTitle) {
        if (typeof fileTitle == "undefined" || fileTitle == null) {
            return "";
        }
        fileTitle = fileTitle.trim();
        var posExt = fileTitle.lastIndexOf(".");
        return 0 <= posExt ? fileTitle.substring(posExt).trim().toLowerCase() : "";
    },

    correctFile: function (file) {
        if (this.getFileExtension(file.name) != ".mp3") {
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
    },

    bindUploader: function(browseButtonId, audioType, selectorId) {

        var self = this;

        var uploader = self.createFileuploadInput(browseButtonId, audioType);

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
            ]
        });

        uploader
            .bind("fileuploadadd", function (e, data) {
                if (self.correctFile(data.files[0])) {
                    data.submit();
                }
            })
            .bind("fileuploaddone", function (e, data) {
                var response = $.parseJSON(data.result);
                var newRingtone = {
                    name: response.Data.Name,
                    path: response.Data.Path,
                    audioType: response.Data.AudioType
                };
                self.addRingtone(newRingtone);

                var $selector = $('#' + selectorId);
                var $newOption = self.ringtoneSelectorOptionTmpl.tmpl(newRingtone);

                var $player = $selector.siblings('.ringtone-player');
                var $playBtn = $selector.siblings('.ringtone-play-btn');

                $selector.append($newOption);

                $selector.val(newRingtone.path);
                $player.attr('src', newRingtone.path);
                $playBtn.removeClass('disable');
            })
            .bind("fileuploadfail", function () {
                self.hideLoader();
                toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileErrorMsg);
            })
            .bind("fileuploadstart", self.showLoader)
            .bind("fileuploadstop", self.hideLoader);
    },

    //#region rendering

    renderView: function() {
        if (this.numbers.length) {
            var $numberSelectorOptions = this.numberSelectorOptionTmpl.tmpl(this.numbers);
            this.$numberSelector.append($numberSelectorOptions);
            this.$numberSelector.val(this.currentNumber.id);
            this.renderSettings();
        } else {
            this.$emptyNumbersListMsg.show();
        }

        this.$view.show();
    },

    renderSettings: function() {
        this.renderGeneralSettings();
        this.renderOperators();
    },

    renderGeneralSettings: function() {
        if (!this.currentNumber) {
            return;
        }

        var $settings = this.settingsTmpl.tmpl(this.currentNumber);
        this.$numberSettingsBox.html($settings);

        this.bindUploader('greeting-ringtone-load-btn', 0, 'greeting-ringtone-selector');
        this.bindUploader('queue-wait-ringtone-load-btn', 3, 'queue-wait-ringtone-selector');
        this.bindUploader('hold-ringtone-load-btn', 1, 'hold-ringtone-selector');
        this.bindUploader('voicemail-ringtone-load-btn', 2, 'voicemail-ringtone-selector');

        this.$numberSettingsBox.find('#greeting-ringtone-player').bind('ended', this.recorPlayerEndedHandler.bind(this));
        this.$numberSettingsBox.find('#hold-ringtone-player').bind('ended', this.recorPlayerEndedHandler.bind(this));
        this.$numberSettingsBox.find('#voicemail-ringtone-player').bind('ended', this.recorPlayerEndedHandler.bind(this));
    },

    renderOperators: function() {
        if (!this.currentNumber) {
            return;
        }

        this.$addOperatorsBtn.useradvancedSelector({ showGroups: true });
        this.$addOperatorsBtn.useradvancedSelector('disable', this.operators);

        var $operators = this.operatorTmpl.tmpl(this.currentNumber);
        this.$operatorsList.html($operators);
    },
    
    renderOperatorSettingsChanges: function(number) {
        if (number.settings.allowOutgoingCalls) {
            this.$operatorsList.find('.outgoing-calls .on_off_button').removeClass('disable');
        } else {
            this.$operatorsList.find('.outgoing-calls .on_off_button').removeClass('on').addClass('off').addClass('disable');
        }
        
        if (number.settings.record) {
            this.$operatorsList.find('.incoming-recording .on_off_button').removeClass('disable');
        } else {
            this.$operatorsList.find('.incoming-recording .on_off_button').removeClass('on').addClass('off').addClass('disable');
        }
    },

    renderAddedOperators: function(operators) {
        this.$addOperatorsBtn.useradvancedSelector('disable', this.operators);

        this.setOperatorsUserInfo(operators);
        var $operators = this.operatorTmpl.tmpl(operators);
        this.$operatorsList.append($operators);
    },

    renderDeletedOperator: function(operatorId) {
        this.$operatorsList.find('.operator[data-operatorid=' + operatorId + ']').remove();
        this.$addOperatorsBtn.useradvancedSelector('undisable', [operatorId]);
    },
    
    //#endregion

    //#region handlers

    clickHandler: function(e) {
        var $this = $(e.target);

        if (!$this.is('.actions')) {
            this.$view.find('.operator').removeClass('selected');
            this.$view.find('.studio-action-panel').hide();
        }
    },

    toggleActionsHandler: function(e) {
        var $this = $(e.target);
        var $panel = $this.find('.studio-action-panel');
        var $operator = $this.closest('.operator');

        var visible = $panel.is(':visible');
        if (visible) {
            $panel.hide();
            $operator.removeClass('selected');
        } else {
            var offset = $this.offset();
            $panel.css({
                top: offset.top + 20,
                left: offset.left - $panel.width() + 26
            });
            $panel.show();
            $operator.addClass('selected');
        }
    },

    numberChangedHandler: function() {
        var changedNumberId = this.$numberSelector.val();
        this.setCurrentNumber(changedNumberId);

        window.location.hash = changedNumberId;

        this.renderSettings();
    },

    quickSettingChangedHandler: function(e) {
        var $this = $(e.target);
        if ($this.is('.on')) {
            $this.removeClass('on').addClass('off');
        } else {
            $this.removeClass('off').addClass('on');
        }
    },

    saveSettingsHandler: function() {
        var $workingHoursInvalidFormatError = this.$numberSettingsBox.find('#working-hours-invalid-format-error');
        var $workingHoursInvalidIntervalError = this.$numberSettingsBox.find('#working-hours-invalid-interval-error');

        $workingHoursInvalidFormatError.hide();
        $workingHoursInvalidIntervalError.hide();

        var whFrom = this.$numberSettingsBox.find('#working-hours-from-input').val();
        var whTo = this.$numberSettingsBox.find('#working-hours-to-input').val();
        if (!this.timeRegex.test(whFrom) || !this.timeRegex.test(whTo)) {
            $workingHoursInvalidFormatError.show();
            return;
        }

        var fromNumber = +whFrom.replace(':', '');
        var toNumber = +whTo.replace(':', '');
        if (fromNumber >= toNumber) {
            $workingHoursInvalidIntervalError.show();
            return;
        }

        var obj = {
            alias: this.$numberSettingsBox.find('#number-alias-input').val(),
            allowOutgoingCalls: this.$numberSettingsBox.find('#outgoing-calls-setting-btn').is('.on'),
            voiceMail: {
                enabled: this.$numberSettingsBox.find('#voicemail-setting-btn').is('.on'),
                url: this.$numberSettingsBox.find('#voicemail-ringtone-selector').val()
            },
            record: this.$numberSettingsBox.find('#record-incoming-setting-btn').is('.on'),
            workingHours: {
                enabled: this.$numberSettingsBox.find('#working-hours-setting-btn').is('.on'),
                from: whFrom,
                to: whTo
            },
            greeting: this.$numberSettingsBox.find('#greeting-ringtone-selector').val(),
            holdUp: this.$numberSettingsBox.find('#hold-ringtone-selector').val(),
            wait: this.$numberSettingsBox.find('#queue-wait-ringtone-selector').val()
        };

        this.showLoader();

        var self = this;
        Teamlab.updateCrmVoipNumberSettings(null, this.currentNumber.id, obj, {
            success: function(params, number) {
                self.saveNumber(number);
                self.renderOperatorSettingsChanges(number);
                self.hideLoader();
                self.showSuccessOpearationMessage();
            },
            error: function() {
                self.hideLoader();
                self.showErrorMessage();
            }
        });
    },

    ringtoneSettingChangedHandler: function(e) {
        var $this = $(e.target);
        var ringtone = $this.val();
        
        if ($this.siblings('.ringtone-play-btn').attr('data-type') == this.currentRingtoneType) {
            this.stopRingtonePlayer();
        }

        var $playBtn = $this.closest('.ringtone-setting-item').find('.ringtone-play-btn');
        if (ringtone) {
            $playBtn.removeClass('disable').removeClass('__stop').addClass('__play');
        } else {
            $playBtn.addClass('disable');
        }
    },

    ringtonePlayHandler: function(e) {
        var $this = $(e.target);
        if ($this.is('.disable')) return false;

        this.ringtonePlayer.setAttribute('src', $this.siblings('.ringtone-selector').val());

        var playOn = $this.is('.__stop');

        this.stopRingtonePlayer();

        if (!playOn) {
            this.currentRingtoneType = $this.attr('data-type');
            this.ringtonePlayer.play();
            $this.removeClass('__play').addClass('__stop');
        }

        return false;
    },
    
    stopRingtonePlayer: function() {
        if (this.ringtonePlayer.readyState != 0) {
            this.ringtonePlayer.pause();
            this.ringtonePlayer.currentTime = 0;
        }
        
        this.$numberSettingsBox.find('.ringtone-play-btn.__stop').removeClass('__stop').addClass('__play');
    },
    
    recorPlayerEndedHandler: function(e) {
        var $player = $(e.target);
        var player = $player.get(0);

        player.currentTime = 0;
        player.pause();

        $player.closest('.ringtone-setting-item').find('.ringtone-play-btn').removeClass('__stop').addClass('__play');
    },

    addOperatorsHandler: function(e, addedOperators) {
        var ids = addedOperators.map(function(o) {
            return o.id;
        });

        if (!ids.length) {
            return;
        }

        this.showLoader();

        var self = this;
        Teamlab.addCrmVoipNumberOperators(null, this.currentNumber.id, { operators: ids }, {
            success: function(params, operators) {
                var number = self.addOperators(operators);
                self.renderAddedOperators(number);
                self.hideLoader();
            },
            error: function() {
                self.showErrorMessage();
                self.hideLoader();
            }
        });
    },

    deleteOperatorHandler: function(e) {
        var operatorId = $(e.target).closest('.operator').attr('data-operatorid');

        this.showLoader();

        var self = this;
        Teamlab.removeCrmVoipNumberOperators(null, this.currentNumber.id, { oper: operatorId }, {
            success: function() {
                self.deleteOperator(operatorId);
                self.renderDeletedOperator(operatorId);
                self.hideLoader();
            },
            error: function() {
                self.hideLoader();
                self.showErrorMessage();
            }
        });
    },

    operatorOutgoingCallsUpdatedHandler: function(e) {
        var on = $(e.target).is('.off');
        var setting = { allowOutgoingCalls: on };

        this.operatorSettingUpdatedHandler(e, setting);
    },

    operatorIncomingRecordingUpdatedHandler: function(e) {
        var on = $(e.target).is('.off');
        var setting = { record: on };

        this.operatorSettingUpdatedHandler(e, setting);
    },

    operatorSettingUpdatedHandler: function(e, setting) {
        var $this = $(e.target);
        if ($this.is('.disable')) {
            return;
        }

        var operatorId = $this.closest('.operator').attr('data-operatorId');
        var on = $this.is('.off');

        this.showLoader();

        var self = this;
        Teamlab.updateCrmVoipOperator(null, operatorId, setting, {
            success: function() {
                if (on) {
                    $this.removeClass('off').addClass('on');
                } else {
                    $this.removeClass('on').addClass('off');
                }

                self.hideLoader();
                self.showSuccessOpearationMessage();
            },
            error: function() {
                self.hideLoader();
                self.showErrorMessage();
            }
        });
    },

    //#endregion

    //#region utils

    showLoader: function() {
        LoadingBanner.displayLoading();
    },

    hideLoader: function() {
        LoadingBanner.hideLoading();
    },

    showSuccessOpearationMessage: function() {
        toastr.success(ASC.Resources.Master.Resource.ChangesSuccessfullyAppliedMsg);
    },

    showErrorMessage: function() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }
    
    //#endregion
};

$(function() {
    VoIPNumbersView.init();
});