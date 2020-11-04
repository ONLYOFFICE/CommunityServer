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


ASC.Projects.TimeSpendActionPage = (function() {
    var isTask = false,
        isInit = false,
        self,
        currentProjectId,
        currentUserId,
        currentTimesCount,
        filterTimesCount = 0,
        $totalTimeContainer,
        $timerList;

    var baseObject = ASC.Projects,
        resources = baseObject.Resources,
        projectsJsResource = resources.ProjectsJSResource,
        projectsFilterResource = resources.ProjectsFilterResource,
        ttResource = resources.TimeTrackingResource,
        filter = baseObject.ProjectsAdvansedFilter,
        common = baseObject.Common,
        teamlab,
        loadingBanner,
        $emptyListTimers,
        $studioActionPanel;

    var clickEventName = "click",
        checkedAttr = "checked";

    var filteredTimes = [], __filter, currentProject;

    var init = function () {
        $timerList = jq("#timeSpendsList");
        teamlab = Teamlab;
        currentProjectId = jq.getURLParam("prjID");
        currentUserId = teamlab.profile.id;
        isTask = jq.getURLParam("id") != null;

        if (isInit === false) {
            isInit = true;
            self = this;

            function canCreateTimeSpend(prj) {
                return prj.canCreateTimeSpend && prj.taskCountTotal > 0;
            }

            var actions = [
                {
                    id: "gaBilled",
                    title: ttResource.PaymentStatusBilled,
                    handler: changePaymentSatusByCheckedTimers.bind(null, "billed", 2),
                    checker: function (item) {
                        return item.canEditPaymentStatus;
                    }
                },
                {
                    id: "gaNotBilled",
                    title: ttResource.PaymentStatusNotBilled,
                    handler: changePaymentSatusByCheckedTimers.bind(null, "not-billed", 1),
                    checker: function (item) {
                        return item.canEditPaymentStatus;
                    }
                },
                {
                    id: "gaNotChargeable",
                    title: ttResource.PaymentStatusNotChargeable,
                    handler: changePaymentSatusByCheckedTimers.bind(null, "not-chargeable", 0),
                    checker: function (item) {
                        return item.canEditPaymentStatus;
                    }
                },
                {
                    id: "gaDelete",
                    title: resources.CommonResource.Delete,
                    handler: showQuestionWindow,
                    checker: function (item) {
                        return item.canEdit;
                    }
                }
            ];

            self.showOrHideData = self.showOrHideData.bind(self, {
                $container: $timerList.find("tbody"),
                tmplName: "projects_timeTrackingTmpl",
                baseEmptyScreen: {
                    img: "time_tracking",
                    header: resources.TimeTrackingResource.NoTtimers,
                    description: resources.TimeTrackingResource.NoTimersNote,
                    button: {
                        title: resources.TimeTrackingResource.StartTimer,
                        onclick: function () {
                            if (isTask) {
                                var taskId = jq.getURLParam("ID");
                                common.showTimer(currentProjectId, taskId);
                            } else {
                                if (currentProjectId != null) {
                                    common.showTimer(currentProjectId);
                                } else {
                                    common.showTimer();
                                }
                            }
                        },
                        canCreate: function () {
                            return currentProjectId ?
                                canCreateTimeSpend(currentProject) :
                                baseObject.Master.Projects.some(canCreateTimeSpend);
                        }
                    }
                },
                filterEmptyScreen: {
                    header: resources.TimeTrackingResource.NoTimersFilter,
                    description: resources.TimeTrackingResource.DescrEmptyListTimersFilter
                },
                groupMenu: {
                    actions: actions,
                    getItemByCheckbox: getFilteredTimeByTarget,
                    getLineByCondition: function(condition) {
                        return filteredTimes
                            .filter(condition)
                            .map(function(item) {
                                return $timerList.find("[timeid=" + item.id + "]");
                            });
                    },
                    multiSelector: [
                    {
                        id: "gasBilled",
                        title: projectsFilterResource.PaymentStatusBilled,
                        condition: function (item) {
                            return item.paymentStatus === 2;
                        }
                    },
                    {
                        id: "gasNotBilled",
                        title: projectsFilterResource.PaymentStatusNotBilled,
                        condition: function (item) {
                            return item.paymentStatus === 1;
                        }
                    },
                    {
                        id: "gasNotChargeable",
                        title: projectsFilterResource.PaymentStatusNotChargeable,
                        condition: function (item) {
                            return item.paymentStatus === 0;
                        }
                    }]
                }
            });
            if (!self.$commonListContainer.find("#totalTimeText").length) {
                $totalTimeContainer = jq.tmpl("projects_totalTimeText", {}).insertBefore(self.$groupeMenu);
            }
            self.getData = self.getData.bind(self, teamlab.getPrjTime, onGetTimes);
        }
        
        loadingBanner = LoadingBanner;
        var eventConstructor = ASC.Projects.Event,
            events = teamlab.events;

        self.baseInit({
                moduleTitle: resources.CommonResource.TimeTracking
            },
            isTask ? undefined : { pagination: "timeKeyForPagination" },
            {
                handler: changePaymentStatus,
                getItem: getFilteredTimeByTarget,
                statuses: [
                    { cssClass: "not-chargeable", text: projectsFilterResource.PaymentStatusNotChargeable, id: 0 },
                    { cssClass: "not-billed", text: projectsFilterResource.PaymentStatusNotBilled, id: 1 },
                    { cssClass: "billed", text: projectsFilterResource.PaymentStatusBilled, id: 2 }
                ]
            },
            showEntityMenu,
            [
                eventConstructor(events.updatePrjTime, onUpdateTime),
                eventConstructor(events.getPrjProject,
                    function(params, project) {
                        currentProject = project;

                        if (isTask) {
                            teamlab.getPrjTaskTime({}, jq.getURLParam("id"), { success: onGetTaskTime });
                        }
                    })
            ],
            {
                getItem: function (target) { return getFilteredTimeByTarget(target).task; },
                selector: '.pm-ts-noteColumn a'
            }
        );

        
        $emptyListTimers = jq("#emptyListTimers");
        $studioActionPanel = jq(".studio-action-panel");

        var projectid = jq.getURLParam("prjid"),
            textSpan = $totalTimeContainer.find("span").first(),
            totalTimeText = "";

        if (isTask) {
            teamlab.getPrjProject({}, projectid);
            totalTimeText = ttResource.TimeSpentForTask;
        } else {
            filter.createAdvansedFilterForTimeTracking(self);

            $timerList.addClass("forProject");
            totalTimeText = ttResource.TotalTimeNote;
        }
        textSpan.text(totalTimeText);

        function showEntityMenu(selectedActionCombobox) {
            var timeid = selectedActionCombobox.attr("timeid");

            return {
                menuItems: [
                    { id: "ta_edit", text: resources.TasksResource.Edit, handler: taEditHandler.bind(null, timeid), classname: "edit" },
                    { id: "ta_remove", text: resources.CommonResource.Delete, handler: taRemoveHandler.bind(null, timeid), classname: "delete" }
                ]
            };
        }

        $timerList.on(clickEventName, ".pm-ts-personColumn span", function (event) {
            var time = getFilteredTimeByTarget(event.target);
            filter.addUser('tasks_responsible', time.person != null ? time.person.id : time.createdBy.id, ['noresponsible']);
        });
    };

    function taEditHandler(id) {
        var time = getFilteredTimeById(id),
            task = time.task,
            recordNote = time.note,
            date = time.date,
            separateTime = jq.timeFormat(time.hours).split(':'),
            responsible = (time.person || time.createdBy).id,
            prjId = time.task.projectOwner.id;

        baseObject.TimeTrakingEdit.showPopup(prjId, task, id, { hours: parseInt(separateTime[0], 10), minutes: parseInt(separateTime[1], 10), seconds: parseInt(separateTime[2], 10) }, date, recordNote, responsible);
    }
    function taRemoveHandler(id) {
        teamlab.removePrjTime({ timeid: id }, { timeids: [id] }, { success: onRemoveTime, error: onUpdatePrjTimeError });
    }

    function showQuestionWindow(timerIds) {
        self.showCommonPopup("trackingRemoveWarning", function () {
            loadingBanner.displayLoading();
            teamlab.removePrjTime({}, { timeids: timerIds },
            {
                success: function () {
                    jq.unblockUI();
                    common.displayInfoPanel(projectsJsResource.TimesRemoved);
                    self.getData();
                }
            });
            jq.unblockUI();
        });
    };

    function changePaymentSatusByCheckedTimers(textStatus, status, timersIds) {
        loadingBanner.displayLoading();
        teamlab.changePaymentStatus({ textStatus: textStatus }, { status: status, timeIds: timersIds }, onChangePaymentStatus);
    };

    function changePaymentStatus(timeId, status) {
        loadingBanner.displayLoading();
        teamlab.changePaymentStatus({}, { status: status, timeIds: [timeId] }, onChangePaymentStatus);
    };

    function onChangePaymentStatus(params, times) {
        var newClass = ASC.Projects.StatusList.getById(times[0].paymentStatus).cssClass;
        for (var i = 0, j = times.length; i < j; i++) {
            var time = times[i];
            jq('.timeSpendRecord[timeid=' + time.id + '] .changeStatusCombobox span:first').attr('class', newClass);
            setFilteredTime(time);
        }

        updateTextFilter();

        loadingBanner.hideLoading();
    };

    function showTotalCountForTask(times) {
        var timesCount = times.length;
        if (!timesCount) return;
        var totalTime = 0;
        var billedTime = 0;
        for (var i = 0; i < timesCount; i++) {
            totalTime += times[i].hours;

            if (times[i].paymentStatus == 2) {
                billedTime += times[i].hours;
            }
        }

        var taskTotalTime = jq.timeFormat(totalTime).split(":");
        $totalTimeContainer.find(".total-count .hours").text(taskTotalTime[0]);
        $totalTimeContainer.find(".total-count .minutes").text(taskTotalTime[1]);

        var taskBilledTime = jq.timeFormat(billedTime).split(":");
        $totalTimeContainer.find(".billed-count .hours").text(taskBilledTime[0]);
        $totalTimeContainer.find(".billed-count .minutes").text(taskBilledTime[1]);

        $totalTimeContainer.addClass("float-none");
        $totalTimeContainer.css("visibility", "visible");
    };

    function showEmptyScreen() {
        self.showOrHideData(filteredTimes, filterTimesCount);

        if (!filteredTimes.length) {
            $totalTimeContainer.css("visibility", "hidden");
        }
    };

    function getTotalTimeByFilter() {
        __filter.Count = 31;
        __filter.StartIndex = currentTimesCount;

        if (!filter.status) {
            delete __filter.status;
            teamlab.getTotalTimeByFilter({}, { filter: __filter, success: onGetTotalTime });

            __filter.status = 2;
            teamlab.getTotalTimeByFilter({}, { filter: __filter, success: onGetTotalBilledTime });
        } else {
            teamlab.getTotalTimeByFilter({}, { filter: __filter, success: onGetTotalTime });
            $totalTimeContainer.find(".billed-count").hide();
        }
    };

    function onGetTimes(params, data) {
        __filter = params.__filter;

        if (Object.keys(__filter).length > 4) {
            $totalTimeContainer = jq.tmpl("projects_totalTimeText", {}).insertBefore(self.$groupeMenu);
            
            getTotalTimeByFilter();

            var textSpan = $totalTimeContainer.find("span").first(),
                totalTimeText = isTask ? ttResource.TimeSpentForTask : ttResource.TotalTimeNote;
            
            textSpan.text(totalTimeText);
        }


        filteredTimes = data;
        currentTimesCount += data.length;

        if (params.mode != 'next') {
            $timerList.find('tbody').html('');
        }


        filterTimesCount = params.__total != undefined ? params.__total : 0;

        if (!filteredTimes.length) {
            $totalTimeContainer.css("visibility", "hidden");
        }

        self.showOrHideData(filteredTimes, filterTimesCount);
    };

    function onGetTotalTime(params, time) {
        if (time != 0 && filteredTimes.length) {
            var fotmatedTime = jq.timeFormat(time).split(":");
            $totalTimeContainer.find(".total-count .hours").text(fotmatedTime[0]);
            $totalTimeContainer.find(".total-count .minutes").text(fotmatedTime[1]);

            $totalTimeContainer.find(".billed-count .hours").text("0");
            $totalTimeContainer.find(".billed-count .minutes").text("00");

            $totalTimeContainer.css("visibility", "visible");
        } else {
            $totalTimeContainer.css("visibility", "hidden");
        }
    };

    function onGetTotalBilledTime(params, time) {
        var $billedTimeContainer = $totalTimeContainer.find(".billed-count");
        if (time != 0) {
            var fotmatedTime = jq.timeFormat(time).split(":");
            $billedTimeContainer.find(".hours").text(fotmatedTime[0]);
            $billedTimeContainer.find(".minutes").text(fotmatedTime[1]);
        } else {
            $billedTimeContainer.find(".hours").text("0");
            $billedTimeContainer.find(".minutes").text("00");
        }
    };

    function onUpdateTime(params, time) {
        common.displayInfoPanel(projectsJsResource.TimeUpdated);
        $timerList.find('.timeSpendRecord[timeid=' + time.id + ']').replaceWith(jq.tmpl("projects_timeTrackingTmpl", time));
        setFilteredTime(time);

        updateTextFilter();
    };

    function onRemoveTime(params, data) {
        var totalTime = 0;
        for (var i = 0; i < data.length; i++) {
            var timer = jq("#timeSpendRecord" + data[i].id);
            totalTime += data[i].hours;
            timer.animate({ opacity: "hide" }, 500);
            filterTimesCount--;
            timer.remove();
            filteredTimes = filteredTimes.filter(function (t) { return t.id !== data[i].id });
            showEmptyScreen();
        }

        loadingBanner.hideLoading();
        common.displayInfoPanel(projectsJsResource.TimeRemoved);
    };

    function onGetTaskTime(data, times) {
        if (!times.length) {
            showEmptyScreen();
            return;
        }

        showTotalCountForTask(times);

        var task = times[0].task;
        var taskData = {
            icon: "tasks",
            title: task.title
        };

        ASC.Projects.InfoContainer.init(taskData);
        filteredTimes = times;
        filterTimesCount = data.__total != undefined ? data.__total : 0;
        showEmptyScreen();
    }

    function setFilteredTime(time) {
        for (var i = 0, max = filteredTimes.length; i < max; i++) {
            if (filteredTimes[i].id == time.id) {
                filteredTimes[i] = time;
                break;
            }
        }
    };
    
    function getFilteredTimeById(timeId) {
        return filteredTimes.find(function(item) { return item.id == timeId});
    };
    
    function getFilteredTimeByTarget($targetObject) {
        return getFilteredTimeById(jq($targetObject).parents("tr.timeSpendRecord").attr("timeid"));
    };

    function updateTextFilter() {
        if (!isTask) {
            getTotalTimeByFilter({}, __filter);
        } else {
            showTotalCountForTask(filteredTimes);
        }
    }

    function onUpdatePrjTimeError(params, data) {
        jq("div.entity-menu[timeid=" + params.timeid + "]").hide();
    };

    function unbindListEvents() {
        if (!isInit) return;
        $timerList.unbind();
        self.unbindEvents();
        ASC.Projects.GroupActionPanel.hide();
    };

    return jq.extend({
        init: init,
        unbindListEvents: unbindListEvents
    }, ASC.Projects.Base);
})(jQuery);
