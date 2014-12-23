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