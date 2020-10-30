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


window.filtersManager = (function($) {
    var isInit = false,
        filters = [],
        eventsHandler = $({}),
        supportedCustomEvents = {
            OnCreate: "mf.create",
            OnUpdate: "mf.update",
            OnDelete: "mf.delete",
            OnError: "mf.error"
        },
        onLoading = false,
        progressBarIntervalId,
        getStatusTimeout = 1000;

    function init() {
        if (isInit === true) return;

        if (ASC.Mail.Presets.Filters) {
            onLoading = true;
            loadFilters(ASC.Mail.Presets.Filters);
            onLoading = false;
        }

        filterModal.init();
        filtersPage.init();
        editFilterPage.init();
    }

    function loadFilters(filterList) {
        filters = [];
        filterList.forEach(function (filter) {
            filter.actions.forEach(function(actionItem) {
                if (actionItem.action === ASC.Mail.Filter.Constants.ActionType.MoveTo) {
                    actionItem.data = actionItem.data.replace(/&quot;/g, '"');
                }
            });

            filter.conditions = $.map(filter.conditions, function (conditionItem) {
                return new ASC.Mail.Filter.Condition(conditionItem.key,
                    conditionItem.operation,
                    conditionItem.value);
            });

            filters.push(filter);
        });
    }

    function compare(f1, f2) {
        if (f1.position < f2.position)
            return -1;
        if (f1.position > f2.position)
            return 1;
        return 0;
    }

    function refresh() {
        var d = jq.Deferred();

        if (onLoading) {
            return d.resolve({}, getList());
        }

        onLoading = true;

        window.Teamlab.getMailFilters({},
        {
            success: function (params, filterList) {
                onLoading = false;

                loadFilters(filterList);

                return d.resolve(params, getList());
            },
            error: function (params, errors) {
                onLoading = false;

                window.toastr.error(errors[0]);

                eventsHandler.trigger(supportedCustomEvents.OnError, errors[0]);

                return d.reject(params, errors);
            }
        });

        return d.promise();
    }

    function getList() {
        return filters.sort(compare);
    }

    function get(id) {
        id = parseInt(id);

        var index = getFilterIndex(id);

        return index > -1 ? filters[index] : null;
    }

    function enable(id, enabled) {
        var filter = get(id);

        if (!filter) return;

        if (filter.enabled === enabled)
            return;

        filter.enabled = enabled;

        update(filter);
    }

    function create(filter) {
        window.Teamlab.createMailFilter({}, filter,
        {
            success: function (params, newFilter) {
                filters.push(newFilter);
                eventsHandler.trigger(supportedCustomEvents.OnCreate, newFilter);
            },
            error: function (params, errors) {
                window.toastr.error(errors[0]);
                eventsHandler.trigger(supportedCustomEvents.OnError, errors[0]);
            }
        });
    }

    function update(filter) {
        window.Teamlab.updateMailFilter({}, filter,
        {
            success: function (params, newFilter) {
                var index = getFilterIndex(newFilter.id);

                if (index > -1)
                    filters[index] = newFilter;

                eventsHandler.trigger(supportedCustomEvents.OnUpdate, newFilter);
            },
            error: function (params, errors) {
                window.toastr.error(errors[0]);
                eventsHandler.trigger(supportedCustomEvents.OnError, errors[0]);
            }
        });
    }

    function remove(id) {
        filterModal.showDelete(get(id),
        {
            onSuccess: function (filter) {
                window.Teamlab.deleteMailFilter({},
                    filter.id,
                    {
                        success: function () {
                            var index = getFilterIndex(filter.id);

                            if (index === -1)
                                return;

                            filters.splice(index, 1);
                            eventsHandler.trigger(supportedCustomEvents.OnDelete, filter.id);
                        },
                        error: function(params, errors) {
                            window.toastr.error(errors[0]);
                            eventsHandler.trigger(supportedCustomEvents.OnError, errors[0]);
                        }
                    });
            }
        });
    }

    function applyFilter(id) {
        filterModal.showApply(get(id),
        {
            onSuccess: function(filter) {

                LoadingBanner.displayMailLoading();

                window.Teamlab.applyMailFilter({},
                    filter.id,
                    {
                        success: function(p, operation) {
                            //console.log("Applying filter started", p, operation);

                            progressBarIntervalId = setInterval(function() {
                                    Teamlab.getMailOperationStatus({},
                                        operation.id,
                                        {
                                            success: function(params, data) {
                                                if (data.completed) {
                                                    clearInterval(progressBarIntervalId);
                                                    progressBarIntervalId = null;

                                                    if (data.error !== "") {
                                                        toastr.error(ASC.Resources.Master.Resource.OperationFailedMsg);
                                                        console.log(data.error);
                                                    } else {
                                                        toastr
                                                            .success(ASC.Resources.Master.Resource
                                                                .OperationSuccededMsg);
                                                    }

                                                    window.serviceManager.updateFolders();
                                                    window.LoadingBanner.hideLoading();
                                                }
                                            },
                                            error: function(params, errors) {
                                                console.error("checkMailOperationStatus failed", e, errors);
                                                clearInterval(progressBarIntervalId);
                                                progressBarIntervalId = null;

                                                window.LoadingBanner.hideLoading();
                                            }
                                        });
                                },
                                getStatusTimeout);

                        },
                        error: function(params, errors) {
                            window.toastr.error(errors[0]);
                            eventsHandler.trigger(supportedCustomEvents.OnError, errors[0]);
                            LoadingBanner.hideLoading();
                        }
                    });
            }
        });
    }

    function getFilterIndex(id) {
        var i, n = filters.length;
        for (i = 0; i < n; i++) {
            var filter = filters[i];

            if (filter.id === id) {
                return i;
            }
        }

        return -1;
    }

    function bind(eventName, fn) {
        eventsHandler.bind(eventName, fn);
    }

    function unbind(eventName) {
        eventsHandler.unbind(eventName);
    }

    function isLoading() {
        return onLoading;
    }

    return {
        init: init,
        bind: bind,
        unbind: unbind,
        events: supportedCustomEvents,

        getList: getList,
        get: get,
        refresh: refresh,

        create: create,
        update: update,
        remove: remove,
        enable: enable,
        applyFilter: applyFilter,

        isLoading: isLoading
    };
})(jQuery);