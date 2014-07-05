/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

var LoginHistory = function() {
    var $ = jq;

    var generateHash = '#generate';
    var loginEventTmpl = 'loginEventTmpl';

    var $generateText = $('#generate-text');
    var $lastEventsText = $('#last-events-text');
    var $eventsTable = $('#events-table');

    var $eventsTableDscr = $('#events-table-dscr');
    var $eventsTableCount = $eventsTableDscr.find('span');

    var $downloadReportBtn = $('#download-report-btn');

    var $emptyScreen = $('#empty-screen');

    var events;

    function init() {
        if (window.location.hash == generateHash) {
            $generateText.show();
            createReport();
            return;
        }

        getEvents();
    }

    function getEvents() {
        showLoader();
        return Teamlab.getLoginEvents({}, null, { success: getEventsCallback });
    }

    function getEventsCallback(params, data) {
        events = extendEvents(data);

        if (events.length) {
            var $events = $('#' + loginEventTmpl).tmpl(events);
            $events.appendTo($eventsTable.find('tbody'));

            $lastEventsText.show();
            $eventsTable.show();

            $eventsTableCount.text(events.length);
            $eventsTableDscr.show();

            $downloadReportBtn.show();
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

    function createReport() {
        showLoader();

        Teamlab.createLoginHistoryReport({}, null, { success: createReportCallback, error: createReportErrorCallback });
    }

    function createReportCallback(params, fileUrl) {
        hideLoader();
        location.href = fileUrl;
    }
    
    function createReportErrorCallback() {
        hideLoader();

        toastr.error(ASC.Resources.Master.Resource.CreateReportError);
    }

    function showLoader() {
        LoadingBanner.displayLoading();
    }

    function hideLoader() {
        LoadingBanner.hideLoading();
    }

    init();
}();