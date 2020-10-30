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


var AuditTrailView = function() {
    var $ = jq;

    var generateHash = '#generate';
    var auditEventTmpl = 'auditEventTmpl';

    var $generateText = $('#generate-text');
    var $eventsBox = $('#events-box');
    var $lifetimeInput = $('#lifetime-input');
    var $saveSettingsBtn = $('#save-settings-btn');
    var $eventsTable = $('#events-table');
    var $eventsTableCount = $('#events-table-dscr span');
    var $downloadReportBtn = $('#download-report-btn');
    var $emptyScreen = $('#empty-screen');

    var events;
    var auditSettings;

    function init() {
        if (window.location.hash == generateHash) {
            createReport();
            return;
        }

        getAuditSettings();
        getEvents();
    }

    function getEvents() {
        showLoader();
        return Teamlab.getAuditEvents({}, null, { success: getEventsCallback });
    }

    function getEventsCallback(params, data) {
        events = extendEvents(data);

        if (events.length) {
            var $events = $('#' + auditEventTmpl).tmpl(events);
            $events.appendTo($eventsTable.find('tbody'));
            $eventsBox.show();
            $eventsTableCount.text(events.length);
            $downloadReportBtn.click(createReport);
        } else {
            $emptyScreen.show();
        }
        
        hideLoader();
    }

    function extendEvents(evts) {
        for (var i = 0; i < evts.length; i++) {
            var event = evts[i];

            var dateStr = ServiceFactory.getDisplayDate(ServiceFactory.serializeDate(event.date));
            var timeStr = ServiceFactory.getDisplayTime(ServiceFactory.serializeDate(event.date));

            event.displayDate = dateStr + ' ' + timeStr;
        }

        return evts;
    }

    function getAuditSettings() {
        Teamlab.getAuditSettings(null, {
            success: function (params, data) {
                auditSettingsCallback(null, data);
            },
            error: function (err) {
                auditSettingsCallback(err);
            }
        });
    }
    
    function auditSettingsCallback(err, data) {
        if (err) {
            showErrorMessage();
        } else {
            renderAuditSettings(data);
        }
        
        hideLoader();
    }

    function renderAuditSettings(data) {
        if (!data) return;

        auditSettings = data;

        $lifetimeInput.val(auditSettings.auditTrailLifeTime).on('propertychange input', function () {
            var $input = $(this);
            $input.val($input.val().replace(/[^\d]+/g, ''));
        });
        
        $saveSettingsBtn.click(saveSettings);
    }

    function saveSettings() {
        var val = parseInt($lifetimeInput.val());

        if (isNaN(val) || val <= 0 || val > auditSettings.maxLifeTime) {
            $lifetimeInput.addClass("with-error");
            return;
        }

        $lifetimeInput.removeClass("with-error");

        auditSettings.auditTrailLifeTime = val;

        Teamlab.setAuditSettings(null, auditSettings.loginHistoryLifeTime, auditSettings.auditTrailLifeTime, {
            success: showSuccessMessage,
            error: showErrorMessage
        });
    }

    function createReport() {
        $generateText.show();
        showLoader();

        Teamlab.createAuditTrailReport({}, { success: createReportCallback, error: createReportErrorCallback });
        return false;
    }

    function createReportCallback (params, data) {
        location.href = data;
    }

    function createReportErrorCallback () {
        $generateText.hide();
        hideLoader();

        toastr.error(ASC.Resources.Master.Resource.CreateReportError);
    }

    function showLoader() {
        LoadingBanner.displayLoading();
    }

    function hideLoader() {
        LoadingBanner.hideLoading();
    }

    function showSuccessMessage() {
        toastr.success(ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage);
    }

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }

    init();
}();