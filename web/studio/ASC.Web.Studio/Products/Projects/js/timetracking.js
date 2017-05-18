/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


ASC.Projects.TimeSpendActionPage = (function() {
    var isTask = false,
        isInit = false,
        self,
        currentProjectId,
        currentUserId,
        currentTimesCount,
        filterTimesCount = 0,
        $totalTimeContainer,
        $timerList,
        $groupeMenu = "",
        listActionButtons = [],
        counterSelectedItems;

    var showCheckboxFlag = false,
        baseObject = ASC.Projects,
        resources = baseObject.Resources,
        projectsJsResource = resources.ProjectsJSResource,
        projectsFilterResource = resources.ProjectsFilterResource,
        filter = baseObject.ProjectsAdvansedFilter,
        common = baseObject.Common,
        teamlab,
        loadingBanner,
        $groupActionContainer,
        $selectAllTimers,
        $emptyListTimers,
        $studioActionPanel;

    var clickEventName = "click",
        checkedAttr = "checked",
        checkedRowClass = "checked-row";

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
                return prj.canCreateTimeSpend && prj.status === 0 && prj.taskCountTotal > 0;
            }

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
                                ASC.Projects.Master.Projects.some(canCreateTimeSpend);
                        }
                    }
                },
                filterEmptyScreen: {
                    header: resources.TimeTrackingResource.NoTimersFilter,
                    description: resources.TimeTrackingResource.DescrEmptyListTimersFilter
                },
                groupMenuEnabled: true
            });
            if (!self.$commonListContainer.find("#totalTimeText").length) {
                $totalTimeContainer = self.$commonListContainer.prepend("<div class='total-time-forFilter' id='totalTimeText'></div>").find("#totalTimeText");
                //var content = 
                $totalTimeContainer.append(jq.tmpl("projects_totalTimeText", {}));
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
                statuses: [
                    { cssClass: "not-chargeable", text: projectsFilterResource.PaymentStatusNotChargeable },
                    { cssClass: "not-billed", text: projectsFilterResource.PaymentStatusNotBilled },
                    { cssClass: "billed", text: projectsFilterResource.PaymentStatusBilled }
                ]
            },
            showEntityMenu,
            [
                eventConstructor(events.removePrjTime, onRemoveTime),
                eventConstructor(events.updatePrjTime, onUpdateTime),
                eventConstructor(events.getPrjProject,
                    function(params, project) {
                        currentProject = project;
                        initPanelsAndPopups();

                        if (isTask) {
                            teamlab.getPrjTaskTime({}, jq.getURLParam("id"), { success: onGetTaskTime });
                        }
                    })
            ]
        );

        
        $emptyListTimers = jq("#emptyListTimers");
        $studioActionPanel = jq(".studio-action-panel");

        var projectid = jq.getURLParam("prjid"),
            textSpan = $totalTimeContainer.find("span").first(),
            totalTimeText = "";

        if (isTask) {
            teamlab.getPrjProject({}, projectid);
            totalTimeText = textSpan.data("tasktext");
        } else {
            filter.createAdvansedFilterForTimeTracking(self);

            $timerList.addClass("forProject");
            totalTimeText = textSpan.data("listtext");
        }
        textSpan.text(totalTimeText);

        // discribe panel
        $timerList.on("mouseenter", ".pm-ts-noteColumn a", function (event) {
            var $targetObject = jq(event.target);
            var task = getFilteredTimeById($targetObject.parents("tr.timeSpendRecord").attr("timeid")).task;

            self.showDescPanel(task, $targetObject);
        });

        $timerList.on('mouseleave', '.pm-ts-noteColumn a', function () {
           self.hideDescrPanel(false);
        });

        function showEntityMenu(selectedActionCombobox) {
            var timeid = selectedActionCombobox.attr("timeid");

            return {
                menuItems: [
                    { id: "ta_edit", text: resources.TasksResource.Edit, handler: taEditHandler.bind(null, timeid) },
                    { id: "ta_remove", text: resources.CommonResource.Delete, handler: taRemoveHandler.bind(null, timeid) }
                ]
            };
        }

        $timerList.on(clickEventName, ".pm-ts-personColumn span", function () {
            var time = getFilteredTimeById(jq(this).parents("tr.timeSpendRecord").attr("timeid"));
            filter.addUser('tasks_responsible', time.person != null ? time.person.id : time.createdBy.id, ['noresponsible']);
        });

        $timerList.on("change", "input", function () {
            var input = jq(this);
            var timerId = input.attr("data-timeId");

            if (input.is(":" + checkedAttr)) {
                jq("tr[timeid=" + timerId + "]").addClass(checkedRowClass);
            } else {
                jq("tr[timeid=" + timerId + "]").removeClass(checkedRowClass);
                $selectAllTimers.removeAttr(checkedAttr);
            }

            var countCheckedTimers = getCountCheckedTimers();

            if (countCheckedTimers > 0) {
                if (countCheckedTimers == 1) {
                    unlockActionButtons();
                } else {
                    changeSelectedItemsCounter();
                }
            } else {
                lockActionButtons();
            }
        });

        jq("#deselectAllTimers").click(function () {
            var $checkboxes = $timerList.find("input");
            var $rows = $timerList.find(" tr");

            $selectAllTimers.removeAttr(checkedAttr);
            $checkboxes.removeAttr(checkedAttr);
            $rows.removeClass(checkedRowClass);
            lockActionButtons();
        });
    };

    function taEditHandler(id) {
        var time = getFilteredTimeById(id),
            task = time.task,
            taskId = task.id,
            taskTitle = task.title,
            recordNote = time.note,
            date = time.displayDateCreation,
            separateTime = jq.timeFormat(time.hours).split(':'),
            responsible = (time.person || time.createdBy).id,
            prjId = time.task.projectOwner.id;

        baseObject.TimeTrakingEdit.showPopup(prjId, taskId, taskTitle, id, { hours: parseInt(separateTime[0], 10), minutes: parseInt(separateTime[1], 10) }, date, recordNote, responsible);
    }
    function taRemoveHandler(id) {
        teamlab.removePrjTime({ timeid: id }, { timeids: [id] }, { error: onUpdatePrjTimeError });
    }

    function initPanelsAndPopups() {
        // group action panel
        if (teamlab.profile.isAdmin || (currentProjectId && currentProject.responsibleId === currentUserId)) {
            self.$commonListContainer.append(jq.tmpl("projects_timeTrakingGroupActionMenu", {}));
            $groupeMenu = jq("#timeTrakingGroupActionMenu");
        }

        $groupActionContainer = jq("#groupActionContainer");
        $groupActionContainer.append($groupeMenu);

        showCheckboxFlag = $groupeMenu.length ? true : false;

        listActionButtons = $groupeMenu ? $groupeMenu.find(".menuAction") : [];
        listActionButtons.splice(0, 1);

        counterSelectedItems = $groupeMenu ? $groupeMenu.find(".menu-action-checked-count") : [];

        if ($groupeMenu) {
            $groupeMenu.on(clickEventName, ".unlockAction", function () {
                var actionType = jq(this).attr("data-status");
                var status;
                switch (actionType) {
                    case "not-chargeable":
                        status = 0;
                        break;
                    case "not-billed":
                        status = 1;
                        break;
                    case "billed":
                        status = 2;
                        break;
                    default:
                        showQuestionWindow();
                        return;
                }
                changePaymentSatusByCheckedTimers(actionType, status);
            });

            var options = {
                menuSelector: "#timeTrakingGroupActionMenu",
                menuAnchorSelector: "#selectAllTimers",
                menuSpacerSelector: "#CommonListContainer .header-menu-spacer",
                userFuncInTop: function () { $groupeMenu.find(".menu-action-on-top").hide(); },
                userFuncNotInTop: function () { $groupeMenu.find(".menu-action-on-top").show(); }
            };
            ScrolledGroupMenu.init(options);

            $selectAllTimers = jq("#selectAllTimers");

            $selectAllTimers.change(function () {
                var $checkboxes = $timerList.find("input");
                var $rows = $timerList.find("tr");

                if ($selectAllTimers.is(":" + checkedAttr)) {
                    $checkboxes.each(function (id, item) { item.checked = true; });
                    $rows.addClass(checkedRowClass);
                    unlockActionButtons();
                } else {
                    $checkboxes.each(function (id, item) { item.checked = false; });
                    $rows.removeClass(checkedRowClass);
                    lockActionButtons();
                }
            });
        }
    };

    function getCountCheckedTimers() {
        return $timerList.find("input:" + checkedAttr).length;
    };

    function getTimersIds() {
        var timers = $timerList.find("input:" + checkedAttr);

        var timersIds = [];

        for (var i = 0; i < timers.length; i++) {
            timersIds.push(jq(timers[i]).attr("data-timeId"));
        }
        return timersIds;
    };

    function changeSelectedItemsCounter() {
        var checkedInputCount = getCountCheckedTimers();
        counterSelectedItems.find("span").text(checkedInputCount + " " + projectsJsResource.GroupMenuSelectedItems);
    };

    function unlockActionButtons() {
        jq(listActionButtons).addClass("unlockAction");
        changeSelectedItemsCounter();
        counterSelectedItems.show();
    };

    function lockActionButtons() {
        jq(listActionButtons).removeClass("unlockAction");
        counterSelectedItems.hide();
    };

    function showQuestionWindow() {
        self.showCommonPopup("trackingRemoveWarning", function () {
            removeChackedTimers();
            jq.unblockUI();
        });
    };

    function removeChackedTimers() {
        var timersIds = getTimersIds();

        loadingBanner.displayLoading();
        teamlab.removePrjTime({}, { timeids: timersIds });
    };

    function changePaymentSatusByCheckedTimers(textStatus, status) {
        var timersIds = getTimersIds();

        loadingBanner.displayLoading();
        teamlab.changePaymentStatus({ textStatus: textStatus }, { status: status, timeIds: timersIds }, onChangePaymentStatus);
    };

    function changePaymentStatus(timeId, textStatus) {
        var status = 0;
        switch (textStatus) {
            case "not-billed":
                status = 1;
                break;
            case "billed":
                status = 2;
                break;
        }
        loadingBanner.displayLoading();
        teamlab.changePaymentStatus({ textStatus: textStatus }, { status: status, timeIds: [timeId] }, onChangePaymentStatus);
    };

    function onChangePaymentStatus(params, times) {
        for (var i = 0, j = times.length; i < j; i++) {
            var time = times[i];
            jq('.timeSpendRecord[timeid=' + time.id + '] .changeStatusCombobox span:first').attr('class', params.textStatus);
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

        if ($groupeMenu) {
            if (!filteredTimes.length) {
                $groupeMenu.hide();
                $totalTimeContainer.css("visibility", "hidden");
            } else {
                $groupeMenu.show();
            }
        }
    };

    function getTotalTimeByFilter(params, filter) {
        if (params.mode != 'next') {
            currentTimesCount = 0;
        }
        filter.Count = 31;
        filter.StartIndex = currentTimesCount;

        if (!filter.status) {
            delete filter.status;
            teamlab.getTotalTimeByFilter(params, { filter: filter, success: onGetTotalTime });

            filter.status = 2;
            teamlab.getTotalTimeByFilter(params, { filter: filter, success: onGetTotalBilledTime });
        } else {
            teamlab.getTotalTimeByFilter(params, { filter: filter, success: onGetTotalTime });
            $totalTimeContainer.find(".billed-count").hide();
        }
    };

    function onGetTimes(params, data) {
        __filter = params.__filter;
        if (Object.keys(__filter).length > 4) {
            $totalTimeContainer = self.$commonListContainer.prepend("<div class='total-time-forFilter' id='totalTimeText'></div>").find("#totalTimeText");
            $totalTimeContainer.append(jq.tmpl("projects_totalTimeText", {}));
            
            getTotalTimeByFilter({}, params.__filter);

            var textSpan = $totalTimeContainer.find("span").first(), totalTimeText = isTask ? textSpan.data("tasktext") : textSpan.data("listtext");
            
            textSpan.text(totalTimeText);
        }

        if (currentProjectId === null) {
            initPanelsAndPopups();
        }

        filteredTimes = data;
        currentTimesCount += data.length;

        if (params.mode != 'next') {
            $timerList.find('tbody').html('');
        }

        jq.each(filteredTimes, function (i, time) {
            filteredTimes[i].showCheckbox = showCheckboxFlag;
        });


        filterTimesCount = params.__total != undefined ? params.__total : 0;

        if ($groupeMenu) {
            if (!filteredTimes.length) {
                $groupeMenu.hide();
                $totalTimeContainer.css("visibility", "hidden");
            } else {
                $groupeMenu.show();
            }
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
        time.showCheckbox = showCheckboxFlag;
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

        jq.each(times, function (i, time) {
            times[i].showCheckbox = showCheckboxFlag;
        });
        showTotalCountForTask(times);

        var tasksResource = ASC.Projects.Resources.TasksResource;
        var task = times[0].task;
        var taskData = {
            uplink: "tasks.aspx?prjID=" + task.projectId + "&id=" + task.id,
            icon: "tasks",
            title: task.title,
            subscribed: task.isSubscribed,
            subscribedTitle: task.isSubscribed ? tasksResource.UnfollowTask : tasksResource.FollowTask
        };

        if (!jq.browser.mobile && task.projectOwner.status === 0) {
            data.ganttchart = "ganttchart.aspx?prjID=" + task.projectOwner.id;
        }

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
        if ($groupActionContainer) {
            $groupActionContainer.hide();
        }
        if ($groupeMenu) {
            $groupeMenu.unbind();
        }
    };

    return jq.extend({
        init: init,
        unbindListEvents: unbindListEvents
    }, ASC.Projects.Base);
})(jQuery);
