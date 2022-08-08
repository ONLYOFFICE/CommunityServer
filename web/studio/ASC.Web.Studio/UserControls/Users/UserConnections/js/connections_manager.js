/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


jq(document).ready(function () {
    jq("#emptyDbipSwitcher").on("click", function () {
        jq(this).helper({
            BlockHelperID: "emptyDbipHelper"
        });
    });

    jq('#switcherConnectionsButton').one('click', function () {
        if (!jq('#connectionsBlockContainer').hasClass('connectionsLoaded') &&
            typeof (window.CommonConnectionsManager) != 'undefined' &&
            typeof (window.CommonConnectionsManager.LoadActiveConnections) === 'function') {
            window.CommonConnectionsManager.LoadActiveConnections();
            jq('#connectionsBlockContainer').addClass('connectionsLoaded');
        }
    });
});


CommonConnectionsManager = new function() {

    var loginEvent = [];

    this.LoadActiveConnections = function () {
        Teamlab.getLoginEventsForProfile({}, {
            success: function (_, data) {
                jq('#connections_list').html(jq("#contentConnectionsTemplate").tmpl(data));
            },
            error: function (params, error) {
                showToastrError();
            }
        });
    };

    this.OpenLogoutAllPopupDialog = function () {
        StudioBlockUIManager.blockUI("#confirmLogout", 600);

        PopupKeyUpActionProvider.ClearActions();
    };

    this.OpenLogoutActiveConnectionPopupDialog = function (id, ip, platform, browser) {
        loginEvent = [id, ip, platform, browser];

        if (!platform) platform = getUnknownText();
        if (!browser) browser = getUnknownText();
        jq('#confirmLogoutFromConnectionText').text(ASC.Resources.Master.ResourceJS.ConfirmLogoutFrom.format(platform, browser) + '?');
        StudioBlockUIManager.blockUI("#confirmLogoutConnection", 420);

        PopupKeyUpActionProvider.ClearActions();
    };

    this.CloseLogOutAllActiveConnectionsChangePassword = function () {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
        
        Teamlab.logoutAllActiveConnectionsWithChangePassword({}, {
            success: function (_, result) {
                if (result) {
                    window.location.href = result;
                }
                else {
                    showToastrError();
                }
            },
            error: function (params, error) {
                showToastrError();
            }
        });
    };

    this.CloseLogoutFromAllActiveConnectionsExceptThis = function () {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();

        Teamlab.logoutAllActiveConnectionsExceptThis({}, {
            success: function (_, result) {
                if (result) {
                    window.location.reload(true);
                }
                else {
                    showToastrError();
                }
            },
            error: function (params, error) {
                showToastrError();
            }
        });
    };

    this.CloseLogoutActiveConnection = function () {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();

        Teamlab.logoutActiveConnection({}, loginEvent[0], {
            success: function (_, result) {
                if (result) {
                    if (!loginEvent[2]) loginEvent[2] = getUnknownText();
                    if (!loginEvent[3]) loginEvent[3] = getUnknownText();
                    toastr.success(ASC.Resources.Master.ResourceJS.ActiveConnectionWasLoggedOut.format(loginEvent[2], loginEvent[3]));
                    jq('#connection_content_' + loginEvent[0]).hide();
                }
                else {
                    showToastrError();
                }
            },
            error: function (params, error) {
                showToastrError();
            }
        });
    };

    var getUnknownText = function () {
        return ASC.Resources.Master.ResourceJS.Unknown;
    };

    var showToastrError = function () {
        toastr.error(ASC.Resources.Master.ResourceJS.OperationFailedError);
    }

    this.GetDisplayDateTime = function (dateTimeFrom) {
        var dateTime = new Date(Date.parse(dateTimeFrom))
        var date = ServiceFactory.getDisplayDate(dateTime);
        var time = ServiceFactory.getDisplayTime(dateTime);
        var dateToday = new Date();
        var dateYesterday = new Date();
        dateYesterday.setDate(dateYesterday.getDate() - 1);

        dateToday = ServiceFactory.getDisplayDate(dateToday);
        dateYesterday = ServiceFactory.getDisplayDate(dateYesterday);
        var result;

        if (dateToday == date) {
            result = ASC.Resources.Master.ResourceJS.Today;
        }
        else if (dateYesterday == date) {
            result = ASC.Resources.Master.ResourceJS.Yesterday;
        }
        else {
            result = date;
        }
        return result + ', ' + time;
    };
};