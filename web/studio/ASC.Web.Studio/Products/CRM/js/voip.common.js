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


window.VoipCommonView = new function() {
    var $ = jq;

    var numbers = [];
    var settings = null;
    var ringtones = [];

    var headerTmpl;
    var settingsTmpl;
    var ringtonesTmpl;
    var ringtoneTmpl;

    var $view;
    var ringtonePlayer;

    var $headerMsg;
    var $settingsList;
    var $ringtonesList;

    var $currentPlayBtn;
    
    function init() {
        cacheElements();
        bindEvents();

        showLoader();
        getData(function(data) {
            saveData(data);
            renderView();

            hideLoader();
        });
    }

    function cacheElements() {
        headerTmpl = $('#header-tmpl');
        settingsTmpl = $('#settings-tmpl');
        ringtonesTmpl = $('#ringtones-tmpl');
        ringtoneTmpl = $('#ringtone-tmpl');

        $view = $('#voip-common-view');

        ringtonePlayer = $view.find('#ringtone-player').get(0);

        $headerMsg = $view.find('#header-message');
        $settingsList = $view.find('#settings-list');
        $ringtonesList = $view.find('#ringtones-list');
    }

    function bindEvents() {
        $('body').on('click', clickHandler);

        ringtonePlayer.addEventListener('loadeddata', playRingtone);
        ringtonePlayer.addEventListener('ended', completePlayRingtone);

        $settingsList.on('change', '#queue-size-selector', settingsUpdatedHandler);
        $settingsList.on('change', '#queue-wait-time-selector', settingsUpdatedHandler);
        $settingsList.on('change', '#operator-pause-selector', settingsUpdatedHandler);

        $ringtonesList.on('click', '.ringtone-group .ringtone-group-box .switcher', toggleRingtoneGroupHandler);
        $ringtonesList.on('click', '.ringtone-group .ringtones-box .ringtone-play-btn', startPlayRingtone);

        $ringtonesList.on('click', '.ringtone-group-box .actions', toggleRingtoneGroupActionsHandler);

        $ringtonesList.on('click', '.ringtone .actions', toggleRingtoneActionsHandler);
        $ringtonesList.on('click', '.ringtone .actions .delete-ringtone-btn', ringtoneDeletedHandler);
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
                }, function(cb) {
                    Teamlab.getVoipUploads({}, {
                        success: function(params, data) {
                            cb(null, data);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                }], function(err, data) {
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

    function getTypedRingtones(rawRingtones) {
        var result = [
            {
                audioType: 0,
                name: ASC.Resources.Master.Resource.GreetingRingtones,
                ringtones: []
            },
            {
                audioType: 3,
                name: ASC.Resources.Master.Resource.QueueRingtones,
                ringtones: []
            },
            {
                audioType: 1,
                name: ASC.Resources.Master.Resource.WaitingRingtones,
                ringtones: []
            },
            {
                audioType: 2,
                name: ASC.Resources.Master.Resource.VoicemailRingtones,
                ringtones: []
            }
        ];

        for (var i = 0; i < rawRingtones.length; i++) {
            var ringtone = rawRingtones[i];
            var targetRingtones;

            if (ringtone.audioType == 0) {
                targetRingtones = result[0].ringtones;
            } else if (ringtone.audioType == 3) {
                targetRingtones = result[1].ringtones;
            } else if (ringtone.audioType == 1) {
                targetRingtones = result[2].ringtones;
            } else {
                targetRingtones = result[3].ringtones;
            }

            targetRingtones.push(ringtone);
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
            .css("display", "none");

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
            ]
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
        var $header = headerTmpl.tmpl(numbers.length);
        $headerMsg.append($header);
    }

    function renderSettings() {
        var $settings = settingsTmpl.tmpl(settings);
        $settingsList.append($settings);
    }

    function renderRingtones() {
        var $ringtones = ringtonesTmpl.tmpl(ringtones);
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
            var offset = $this.offset();
            $panel.css({
                top: offset.top + 20,
                left: offset.left - $panel.width() + 26
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
            var offset = $this.offset();
            $panel.css({
                top: offset.top + 20,
                left: offset.left - $panel.width() + 26
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
            toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileErrorMsg);
            return;
        }

        var newRingtone = {
            name: response.Data.Name,
            path: response.Data.Path,
            audioType: response.Data.AudioType
        };

        var $ringtoneGroup = $("#" + data.paramName);

        var $newRingtone = ringtoneTmpl.tmpl(newRingtone);
        $ringtoneGroup.find(".ringtones-box").append($newRingtone);

        $ringtoneGroup.find(".ringtone-group-box .switcher .expander-icon").addClass("open");
        $ringtoneGroup.find(".ringtones-box").show();
    }

    function ringtoneUploadedErrorHandler() {
        hideLoader();
        toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileErrorMsg);
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

        showLoader();

        Teamlab.deleteVoipUploads(null, { audioType: audioType, fileName: fileName }, {
            success: function() {
                $ringtone.remove();
                hideLoader();
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    }

    //#endregion
    
    //#region utils

    function showLoader() {
        LoadingBanner.displayLoading();
    }

    function hideLoader() {
        LoadingBanner.hideLoading();
    }

    function showSuccessOpearationMessage() {
        toastr.success(ASC.Resources.Master.Resource.ChangesSuccessfullyAppliedMsg);
    }

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }
    
    //#endregion

    return {
        init: init
    };
};

jq(function() {
    VoipCommonView.init();
});