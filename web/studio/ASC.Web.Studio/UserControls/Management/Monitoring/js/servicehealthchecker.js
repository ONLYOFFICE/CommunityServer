/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


ServiceHealthCheckerManager = new function () {
    var STATUS_NOT_FOUND = 0;
    var STATUS_RUNNING = 1;
    var STATUS_STOPPED = 2;
    var UPDATE_TABLE_TIMEOUT = 10000;
    var WAIT_STATUS_TIMEOUT = 500;

    this.Init = function () {
        updateServiceStatusTable();
        setTimeout(function() {
            updateServiceStatusTable();
            setTimeout(arguments.callee, UPDATE_TABLE_TIMEOUT);
        }, UPDATE_TABLE_TIMEOUT);

        jq('#clearCacheBtn').click(clearCacheBtnCallback);
    };

    function updateServiceStatusTable() {
        MonitoringAjax.GetServiceStatusList(function (response) {   
            if (response.value) {
                var statusList = response.value;
                for (var i = 0; i < statusList.length; i++) {
                    showServiceInfo(statusList[i]);
                }
            }
        });
    }
    
    function showServiceInfo(service) {
        var row = jq('#tr_' + service.id);
        var button = row.find('.button');

        row.removeClass('__open');
        row.removeClass('__close');
        button.removeClass('__play');
        button.removeClass('__update');
        button.unbind();
        button.removeClass('disable');

        if (service.status == STATUS_RUNNING) {
            button.addClass('__update');
            button.click(function () {
                restartService(service.name);
            });
            row.addClass('__open');
        } else if (service.status == STATUS_STOPPED) {
            button.addClass('__play');
            button.click(function() {
                startService(service.name);
            });
            row.addClass('__close');
        } else {
            button.addClass('disable');
            button.addClass('__play');
            row.addClass('__close');
        }
        
        row.children(' .monitoring-table_status').text(service.statusDescription);
    }
    
    function startService(serviceName, callback) {
        MonitoringAjax.StartService(serviceName, function (response) {
            if (response.error) {
                toastr.error(response.error.Message);
            } else {
                showServiceInfo(response.value);
                waitServiceStatus(serviceName, STATUS_RUNNING, callback);
            }
        });
    }
    
    function stopService(serviceName, callback) {
        MonitoringAjax.StopService(serviceName, function (response) {
            if (response.error) {
                toastr.error(response.error.Message);
            } else {
                showServiceInfo(response.value);
                waitServiceStatus(serviceName, STATUS_STOPPED, callback);
            }
        });
    }
    
    function restartService(serviceName) {
        stopService(serviceName, function() {
            startService(serviceName);
        });
    }
    
    function waitServiceStatus(serviceName, expectedStatus, callback) {
        setTimeout(function() {
            var response = MonitoringAjax.GetServiceStatus(serviceName);
            showServiceInfo(response.value);
            if (response.value.status == STATUS_NOT_FOUND || response.value.status == expectedStatus) {
                callback();
            } else {
                setTimeout(arguments.callee, WAIT_STATUS_TIMEOUT);
            }
        }, WAIT_STATUS_TIMEOUT);
    }
    
    function clearCacheBtnCallback() {
        LoadingBanner.showLoaderBtn('#serviceStatusContainer');
        MonitoringAjax.ClearCache(function (response) {
            LoadingBanner.hideLoaderBtn('#serviceStatusContainer');
        });
    }
};

jq(function () {
    ServiceHealthCheckerManager.Init();
});