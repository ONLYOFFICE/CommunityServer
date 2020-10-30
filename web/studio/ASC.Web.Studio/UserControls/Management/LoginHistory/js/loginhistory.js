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


var LoginHistory = function() {
    var $ = jq;

    var generateHash = '#generate';

    var $loginEventTmpl = $('#login-event-tmpl');
    var $onlineUserTmpl = $('#online-user-tmpl');

    var $generateText = $('#generate-text');
    var $eventsBox = $('#events-box');
    var $lifetimeInput = $('#lifetime-input');
    var $saveSettingsBtn = $('#save-settings-btn');
    var $eventsList = $('#events-list');
    var $eventsListCount = $('#events-list-dscr span');
    var $onlineUsersBox = $('#online-users-box');
    var $onlineUsersList = $('#online-users-list');
    var $downloadReportBtn = $('#download-report-btn');
    var $emptyScreen = $('#empty-screen');

    var lastEvents = [];
    var auditSettings = null;

    var renderUserTimeout = 5000;

    var socket;

    var userManager;
    var isDataRequested = false;

    function init() {
        userManager = window.UserManager;
        if (window.location.hash == generateHash) {
            createReport();
            return;
        }

        if (!ASC.SocketIO || ASC.SocketIO.disabled()) {
            getData(getDataCallback);
            return;
        }
        showLoader();

        socket = ASC.SocketIO.Factory.counters
            .on('connect', function() {
                isDataRequested || getData(getAllDataCallback);
            })
            .on('connect_error', function() {
                isDataRequested || getData(getDataCallback);
            })
            .on('renderOnlineUsers', renderOnlineUsers)
            .on('renderOnlineUser', renderOnlineUser)
            .on('renderOfflineUser', renderOfflineUser);
            
    }

    function getDataCallback(err, data) {
        if (err) {
            hideLoader();
            showErrorMessage();
        } else {
            saveData(data);
            renderLastEvents();
            renderAuditSettings();
            hideLoader();
        }
    }

    function getAllDataCallback(err, data) {
        if (err) {
            hideLoader();
            showErrorMessage();
        } else {
            saveData(data);
            renderLastEvents();
            renderAuditSettings();
            socket.emit('renderOnlineUsers');
            hideLoader();
        }
    }

    function getData(callback) {
        async.parallel([
                function(cb) {
                    Teamlab.getLoginEvents({
                        success: function(params, data) {
                            cb(null, data);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                },
                function (cb) {
                    Teamlab.getAuditSettings(null, {
                        success: function (params, data) {
                            cb(null, data);
                        },
                        error: function (err) {
                            cb(err);
                        }
                    });
                }],
            function(err, data) {
                callback(err, data);
            });
            isDataRequested = true;
    }

    function saveData(data) {
        lastEvents = getExtendedEvents(data[0]);
        auditSettings = data[1] || null;
    }

    function getExtendedEvents(evts) {
        for (var i = 0; i < evts.length; i++) {
            var event = evts[i];

            var dateStr = ServiceFactory.getDisplayDate(ServiceFactory.serializeDate(event.date));
            var timeStr = ServiceFactory.getDisplayTime(ServiceFactory.serializeDate(event.date));

            event.displayDate = dateStr + ' ' + timeStr;
        }

        return evts;
    }

    function renderLastEvents() {
        if (lastEvents.length) {
            var $events = $loginEventTmpl.tmpl(lastEvents);
            $events.appendTo($eventsList.find('tbody'));
            $eventsBox.show();
            $eventsListCount.text(lastEvents.length);
            $downloadReportBtn.show().click(createReport);
        } else {
            $emptyScreen.show();
        }
    }

    function renderAuditSettings() {
        if (!auditSettings) return;
        
        $lifetimeInput.val(auditSettings.loginHistoryLifeTime).on('propertychange input', function () {
            var $input = $(this);
            $input.val($input.val().replace(/[^\d]+/g, ''));
        });
        $saveSettingsBtn.click(saveSettings);
    }

    function renderOnlineUsers(usersDictionary) {
        var users = getUsers(usersDictionary);

        if (users.length) {
            var $users = $onlineUserTmpl.tmpl(users);
            $onlineUsersList.html($users);

            $onlineUsersBox.show();
        } else {
            $onlineUsersBox.hide();
        }
    }

    var renderOfflineUserTimeouts = {};

    function renderOnlineUser(userId) {
        if (typeof renderOfflineUserTimeouts[userId] !== "undefined") {
            clearTimeout(renderOfflineUserTimeouts[userId]);
            delete renderOfflineUserTimeouts[userId];
        } else {
            var user = userManager.getUser(userId);
            if (user == null) return;
            var $user = $onlineUserTmpl.tmpl(user);

            $onlineUsersList.append($user);
            $user.colorFade('#83e281', renderUserTimeout);
        }
    }

    function renderOfflineUser(userId) {
        renderOfflineUserTimeouts[userId] = setTimeout(function () {
            var user = userManager.getUser(userId);
            var $user = $onlineUsersList.find('.online-user[data-userid="' + userId + '"]');

            user.presenceDuration = "";
            $user.colorFade('#fe4042', renderUserTimeout, function () {
                $user.remove();
            });
            delete renderOfflineUserTimeouts[userId];
        }, 2000);
    }

    function createReport() {
        $generateText.show();
        showLoader();

        Teamlab.createLoginHistoryReport({}, { success: createReportCallback, error: createReportErrorCallback });
        return false;
    }

    function saveSettings() {
        var val = parseInt($lifetimeInput.val());

        if (isNaN(val) || val <= 0 || val > auditSettings.maxLifeTime) {
            $lifetimeInput.addClass("with-error");
            return;
        }
        
        $lifetimeInput.removeClass("with-error");

        auditSettings.loginHistoryLifeTime = val;

        Teamlab.setAuditSettings(null, auditSettings.loginHistoryLifeTime, auditSettings.auditTrailLifeTime, {
            success: showSuccessMessage,
            error: showErrorMessage
        });
    }

    function createReportCallback (params, data) {
        location.href = data;
    }

    function createReportErrorCallback() {
        $generateText.hide();
        hideLoader();

        toastr.error(ASC.Resources.Master.Resource.CreateReportError);
    }

    function getUsers(usersDictionary) {
        var users = [];

        var info = userManager.getAllUsers();

        for (var userId in info) {
            if (!info.hasOwnProperty(userId)) continue;
            var user = info[userId];
            if (usersDictionary[userId]) {
                user.firstConnection = usersDictionary[userId].FirstConnection;
                user.lastConnection = usersDictionary[userId].LastConnection;
                user.presenceDuration = getPresenceDuration(usersDictionary[userId].FirstConnection);

                users.push(user);
            }
        }

        return users;
    }

    function getPresenceDuration(firstConnectionTime) {
        var now = new Date();
        var firstConnection = new Date(firstConnectionTime);

        var diff = toUtcDate(new Date(now - firstConnection));

        var hours = diff.getHours();
        var minutes = diff.getMinutes();

        if (hours == 0 && minutes == 0) return '';

        if (hours < 10) hours = '0' + hours;
        if (minutes < 10) minutes = '0' + minutes;

        return hours + ':' + minutes;
    }

    function toUtcDate(date) {
        return new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate(), date.getUTCHours(), date.getUTCMinutes(), date.getUTCSeconds());
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

    return {
        init: init
    };
}();

jQuery(function() {
    LoginHistory.init();
});