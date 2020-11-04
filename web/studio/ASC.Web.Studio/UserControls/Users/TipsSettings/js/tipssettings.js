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


window.TipsSettings = new function() {
    var $ = jq;
    
    var $tipsSettingsBox = $('#tips-settings-box');

    var $toggleTipsSettingsContentBtn = $tipsSettingsBox.find('.toggle-button');
    var $tipsSettingsContent = $tipsSettingsBox.find('.tabs-content');
    var $updateTipsSettingsBtn = $tipsSettingsBox.find('.on_off_button');

    function init() {
        bindEvents();
    }
    
    function bindEvents() {
        $toggleTipsSettingsContentBtn.on('click', toogleTipsSettingsContent);
        $updateTipsSettingsBtn.on('click', updateTipsSettings);
    }

    function toogleTipsSettingsContent() {
        if ($tipsSettingsContent.is(':visible')) {
            $toggleTipsSettingsContentBtn.text($toggleTipsSettingsContentBtn.attr('data-showtext'));
            $tipsSettingsContent.hide();
        } else {
            $toggleTipsSettingsContentBtn.text($toggleTipsSettingsContentBtn.attr('data-hidetext'));
            $tipsSettingsContent.show();
        }
    }

    function updateTipsSettings() {
        var show = $updateTipsSettingsBtn.is('.off');
        Teamlab.updateTipsSettings({ show: show }, {
            success: function() {
                $updateTipsSettingsBtn.removeClass('on off').addClass(show ? 'on' : 'off');
                if (show && window.sessionStorage) {
                    window.sessionStorage.removeItem("tipsWasClosed");
                }
            },
            error: function() {
                toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
            }
        });
    }

    return {
        init: init
    };
};

jq(function() {
    window.TipsSettings.init();
});