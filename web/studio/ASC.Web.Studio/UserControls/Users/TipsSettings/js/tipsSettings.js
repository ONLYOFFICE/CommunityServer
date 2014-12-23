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