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
            /*{
                audioType: 1,
                name: ASC.Resources.Master.Resource.WaitingRingtones,
                ringtones: []
            },*/
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

    function bindUploader(browseButtonId, audioType, ringtoneSelectorId) {
        var uploader = new plupload.Uploader({
            runtimes: 'html5',
            browse_button: browseButtonId,
            url: 'ajaxupload.ashx?type=ASC.Web.CRM.Controls.Settings.VoipUploadHandler,ASC.Web.CRM',
            filters: {
                max_file_size: '20mb',
                mime_types: [
                    { title: 'MP3 Files', extensions: "mp3" }
                ]
            }
        });

        uploader.settings.multipart_params = {
            'audioType': audioType
        };

        uploader.ringtoneSelectorId = ringtoneSelectorId;
        uploader.init();

        uploader.bind('FilesAdded', ringtoneAddedHandler);
        uploader.bind('FileUploaded', ringtoneUploadedHandler);
        uploader.bind('Error', ringtoneUploadedErrorHandler);
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

    function ringtoneAddedHandler(uploader) {
        showLoader();
        uploader.start();
    }

    function ringtoneUploadedHandler(uploader, files, res) {
        var response = $.parseJSON(res.response);
        if (!response.Success || !response.Data) {
            toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileErrorMsg);
            return;
        }

        var newRingtone = {
            name: response.Data.Name,
            path: response.Data.Path,
            audioType: response.Data.AudioType
        };

        var $ringtoneGroup = $('#' + uploader.ringtoneSelectorId);

        var $newRingtone = ringtoneTmpl.tmpl(newRingtone);
        $ringtoneGroup.find('.ringtones-box').append($newRingtone);

        $ringtoneGroup.find('.ringtone-group-box .switcher .expander-icon').addClass('open');
        $ringtoneGroup.find('.ringtones-box').show();

        $.each(files, function(i, file) {
            uploader.removeFile(file);
        });

        hideLoader();
    }

    function ringtoneUploadedErrorHandler() {
        hideLoader();

        if (error.code == -600) {
            toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileSizeErrorMsg);
        } else if (error.code == -601) {
            toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileFormatErrorMsg);
        } else {
            toastr.error(ASC.Resources.Master.Resource.UploadVoipRingtoneFileErrorMsg);
        }
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