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