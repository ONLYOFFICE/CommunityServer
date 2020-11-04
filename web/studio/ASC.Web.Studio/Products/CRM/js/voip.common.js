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

ASC.CRM.Voip.CommonView = (function ($) {
    var numbers = [],
        settings = null,
        ringtones = [],
        $view,
        ringtonePlayer,
        $headerMsg,
        $settingsList,
        $ringtonesList,
        $currentPlayBtn;

    var loadingBanner = LoadingBanner,
        master = ASC.Resources.Master,
        resource = master.Resource,
        toastrLocal = toastr;

    function init() {
        cacheElements();
        bindEvents();

        if (!master.Voip.enabled) {
            return;
        }

        showLoader();
        getData(function(data) {
            saveData(data);
            renderView();

            hideLoader();
        });
    }

    function cacheElements() {
        $view = $('#voip-common-view');

        ringtonePlayer = $view.find('#ringtone-player').get(0);

        $headerMsg = $view.find('#header-message');
        $settingsList = $view.find('#settings-list');
        $ringtonesList = $view.find('#ringtones-list');
    }

    function bindEvents() {
        var clickEventName = 'click',
            changeEventName = 'change';

        $('body').on(clickEventName, clickHandler);

        ringtonePlayer.addEventListener('loadeddata', playRingtone);
        ringtonePlayer.addEventListener('ended', completePlayRingtone);

        $settingsList.on(changeEventName, '#queue-size-selector', settingsUpdatedHandler);
        $settingsList.on(changeEventName, '#queue-wait-time-selector', settingsUpdatedHandler);
        $settingsList.on(changeEventName, '#operator-pause-selector', settingsUpdatedHandler);

        $ringtonesList.on(clickEventName, '.ringtone-group .ringtone-group-box .switcher', toggleRingtoneGroupHandler);
        $ringtonesList.on(clickEventName, '.ringtone-group .ringtones-box .ringtone-play-btn', startPlayRingtone);

        $ringtonesList.on(clickEventName, '.ringtone-group-box .actions', toggleRingtoneGroupActionsHandler);

        $ringtonesList.on(clickEventName, '.ringtone .actions', toggleRingtoneActionsHandler);
        $ringtonesList.on(clickEventName, '.ringtone .actions .delete-ringtone-btn', ringtoneDeletedHandler);
    }

    //#region data

    function getData(callback) {
        async.parallel([
                function(cb) {
                    Teamlab.getCrmVoipExistingNumbers(null, {
                        success: function(params, data) {
                            cb(null, data);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                },
                function(cb) {
                    Teamlab.getCrmVoipSettings(null, {
                        success: function(params, data) {
                            cb(null, data);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                },
                function(cb) {
                    Teamlab.getVoipUploads({}, {
                        success: function(params, data) {
                            cb(null, data);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                }
            ],
            function(err, data) {
                if (err) {
                    showErrorMessage();
                } else {
                    callback(data);
                }
            });
    }

    function saveData(data) {
        numbers = data[0];
        settings = data[1];
        ringtones = getTypedRingtones(data[2]);
    }

    function ringtoneFactory(audioType, name) {
        return { audioType: audioType, name: name, ringtones: [] };
    }

    function getTypedRingtones(rawRingtones) {
        var result = [
            ringtoneFactory(0, resource.GreetingRingtones),
            ringtoneFactory(1, resource.WaitingRingtones),
            ringtoneFactory(2, resource.VoicemailRingtones),
            ringtoneFactory(3, resource.QueueRingtones)
        ];

        for (var i = 0; i < rawRingtones.length; i++) {
            result[rawRingtones[i].audioType].ringtones.push(rawRingtones[i]);
        }

        return result;
    }

    //#endregion

    function createFileuploadInput(browseButtonId, audioType) {
        var buttonObj = jq("#" + browseButtonId);

        var inputObj = jq("<input/>")
            .attr("id", "fileupload_" + audioType)
            .attr("type", "file")
            .attr("multiple", "multiple")
            .css("width", "0")
            .css("height", "0")
            .addClass("display-none");

        inputObj.appendTo(buttonObj.parent());

        buttonObj.on("click", function (e) {
            e.preventDefault();
            jq("#fileupload_" + audioType).click();
        });

        return inputObj;
    }

    function getFileExtension(fileTitle) {
        if (typeof fileTitle == "undefined" || fileTitle == null) {
            return "";
        }
        fileTitle = fileTitle.trim();
        var posExt = fileTitle.lastIndexOf(".");
        return 0 <= posExt ? fileTitle.substring(posExt).trim().toLowerCase() : "";
    }

    function correctFile (file) {
        if (getFileExtension(file.name) != ".mp3") {
            toastrLocal.error(resource.UploadVoipRingtoneFileFormatErrorMsg);
            return false;
        }

        if (file.size <= 0) {
            toastrLocal.error(resource.UploadVoipRingtoneEmptyFileErrorMsg);
            return false;
        }

        if (file.size > 20 * 1024 * 1024) {
            toastrLocal.error(resource.UploadVoipRingtoneFileSizeErrorMsg);
            return false;
        }

        return true;
    }

    function bindUploader(browseButtonId, audioType, ringtoneSelectorId) {

        var uploader = createFileuploadInput(browseButtonId, audioType);

        uploader.fileupload({
            url: "ajaxupload.ashx?type=ASC.Web.CRM.Controls.Settings.VoipUploadHandler,ASC.Web.CRM",
            autoUpload: false,
            singleFileUploads: true,
            sequentialUploads: true,
            progressInterval: 1000,
            paramName: ringtoneSelectorId,
            formData: [
                {
                    name: "audioType",
                    value: audioType
                }
            ],
            dropZone: jq("#" + browseButtonId).parents(".ringtone-group-box")
        });

        uploader
            .bind("fileuploadadd", ringtoneAddedHandler)
            .bind("fileuploaddone", ringtoneUploadedHandler)
            .bind("fileuploadfail", ringtoneUploadedErrorHandler)
            .bind("fileuploadstart", showLoader)
            .bind("fileuploadstop", hideLoader);
    }

    //#region rendering

    function renderView() {
        renderHeader();

        renderSettings();
        renderRingtones();

        $view.show();
    }

    function renderHeader() {
        var $header = jq.tmpl("voip-header-tmpl", numbers.length);
        $headerMsg.append($header);
    }

    function renderSettings() {
        var $settings = jq.tmpl("voip-common-settings-tmpl", settings);
        $settingsList.append($settings);
    }

    function renderRingtones() {
        var $ringtones = jq.tmpl("voip-ringtones-tmpl", ringtones);
        $ringtonesList.append($ringtones);

        for (var i = 0; i < ringtones.length; i++) {
            var ringtone = ringtones[i];
            bindUploader('add-ringtone-' + ringtone.audioType + '-btn', ringtone.audioType, 'ringtone-group-' + ringtone.audioType);
        }
    }

    //#endregion
    
    //#region handlers

    function settingsUpdatedHandler() {
        var settingsToSave = {
            queue:
            {
                size: $settingsList.find('#queue-size-selector').val(),
                waitTime: $settingsList.find('#queue-wait-time-selector').val()
            },

            pause: $settingsList.find('#operator-pause-selector').val() == '1'
        };

        showLoader();
        Teamlab.updateCrmVoipSettings(null, settingsToSave, {
            success: function() {
                hideLoader();
                showSuccessOpearationMessage();
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    }

    function toggleRingtoneGroupHandler(e) {
        var $this = $(e.currentTarget);
        $this.closest('.ringtone-group').find('.ringtones-box').toggle();
        $this.find('.expander-icon').toggleClass('open');
    }

    function clickHandler(e) {
        var $this = $(e.target);

        if (!$this.is('.actions')) {
            clearActionPanel();
        }
    }

    function clearActionPanel() {
        $view.find('.ringtone-group-box').removeClass('selected').removeClass('ringtone-selected');
        $view.find('.ringtone').removeClass('selected');

        $view.find('.studio-action-panel').hide();
    }

    function toggleRingtoneGroupActionsHandler(e) {
        var $this = $(e.target);

        var $panel = $this.find('.studio-action-panel');
        var $ringtoneGroup = $this.closest('.ringtone-group-box');

        var visible = $panel.is(':visible');
        clearActionPanel();

        if (visible) {
            $panel.hide();
            $ringtoneGroup.removeClass('selected');
        } else {
            $panel.css({
                top: ($this.outerHeight() + $this.height()) / 2,
                left: $this.outerWidth() - $panel.width()
            });
            $panel.show();
            $ringtoneGroup.addClass('selected');
        }
    }

    function toggleRingtoneActionsHandler(e) {
        var $this = $(e.target);
        var $panel = $this.find('.studio-action-panel');

        var $ringtone = $this.closest('.ringtone');
        var $ringtoneGroup = $this.closest('.ringtone-group').find('.ringtone-group-box');

        var visible = $panel.is(':visible');
        clearActionPanel();

        if (visible) {
            $panel.hide();
            $ringtoneGroup.removeClass('ringtone-selected');
            $ringtone.removeClass('selected');
        } else {
            $panel.css({
                top: ($this.outerHeight() + $this.height()) / 2,
                left: $this.outerWidth() - $panel.width()
            });
            $panel.show();
            $ringtoneGroup.addClass('ringtone-selected');
            $ringtone.addClass('selected');
        }
    }

    //#endregion
    
    //#region ringtones handlers

    function startPlayRingtone(e) {
        var $this = $(e.target);
        if ($this.is('.disable')) return false;

        if (!$this.is($currentPlayBtn)) {
            ringtonePlayer.setAttribute('src', $this.attr('data-path'));
            
            if ($currentPlayBtn) {
                $currentPlayBtn.siblings('.loader16').hide();
                $currentPlayBtn.removeClass('__stop').addClass('__play');
                $currentPlayBtn.show();
            }
        }

        $currentPlayBtn = $this;

        var playOn = $this.is('.__stop');
        if (playOn) {
            ringtonePlayer.pause();
            ringtonePlayer.currentTime = 0;
            
            $currentPlayBtn.removeClass('__stop').addClass('__play');
        } else if (ringtonePlayer.readyState == 4) {
            ringtonePlayer.play();
            $currentPlayBtn.removeClass('__play').addClass('__stop');
        } else {
            /*$currentPlayBtn.siblings('.loader16').show();*/
            $currentPlayBtn.removeClass('__play').addClass('__stop').addClass('disable');
        }
        
        return false;
    }

    function playRingtone() {
        /*$currentPlayBtn.siblings('.loader16').hide();*/
        $currentPlayBtn.removeClass('disable');
        ringtonePlayer.play();
    }

    function completePlayRingtone() {
        $currentPlayBtn.removeClass('__stop').addClass('__play');
    }

    function ringtoneAddedHandler(e, data) {
        if (correctFile(data.files[0])) {
            data.submit();
        }
    }

    function ringtoneUploadedHandler(e, data) {
        var response = $.parseJSON(data.result);
        if (!response.Success || !response.Data) {
            if (response.Message) {
                toastrLocal.error(response.Message);
            } else {
                toastrLocal.error(resource.UploadVoipRingtoneFileErrorMsg);
            }
            return;
        }

        var newRingtone = {
            name: response.Data.Name,
            path: response.Data.Path,
            audioType: response.Data.AudioType,
            isDefault: response.Data.isDefault
        };

        ringtones[newRingtone.audioType].ringtones.push(newRingtone);

        var $ringtoneGroup = $("#" + data.paramName);

        var $newRingtone = jq.tmpl("voip-ringtone-tmpl", newRingtone);
        $ringtoneGroup.find(".ringtones-box").append($newRingtone);

        $ringtoneGroup.find(".ringtone-group-box .switcher .expander-icon").removeClass("display-none").addClass("open");
        $ringtoneGroup.find(".ringtones-box").show();
    }

    function ringtoneUploadedErrorHandler() {
        hideLoader();
        toastrLocal.error(resource.UploadVoipRingtoneFileErrorMsg);
    }

    function ringtoneDeletedHandler(e) {
        var audioType = $(e.target).closest('.ringtone-group').attr('data-audiotype');

        var $ringtone = $(e.target).closest('.ringtone');

        var fileName = $ringtone.attr('data-filename');
        var fileSrc = $ringtone.find('.ringtone-play-btn').attr('data-path');
        
        if (fileSrc == ringtonePlayer.getAttribute('src')) {
            ringtonePlayer.pause();
            ringtonePlayer.setAttribute('src', '');
        }

        Teamlab.deleteVoipUploads(null, { audioType: audioType, fileName: fileName }, {
            before: showLoader,
            after: hideLoader,
            success: function() {
                ringtones[audioType].ringtones = ringtones[audioType].ringtones.filter(function(r) {
                    return !(r.audioType == audioType && r.name === fileName);
                });
                if (!ringtones[audioType].ringtones.length) {
                    $(e.target).closest('.ringtone-group').find('.expander-icon').addClass("display-none");
                }
                $ringtone.remove();
            },
            error: showErrorMessage
        });
    }

    //#endregion
    
    //#region utils

    function showLoader() {
        loadingBanner.displayLoading();
    }

    function hideLoader() {
        loadingBanner.hideLoading();
    }

    function showSuccessOpearationMessage() {
        toastrLocal.success(resource.ChangesSuccessfullyAppliedMsg);
    }

    function showErrorMessage() {
        toastrLocal.error(resource.CommonJSErrorMsg);
    }
    
    //#endregion

    return {
        init: init
    };
})(jq);

jq(function() {
    ASC.CRM.Voip.CommonView.init();
});