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


ASC.Projects.TimeTraking = (function() {
    var timer,
        seconds = 0,
        timerHours = 0,
        timerMin = 0,
        timerSec = 0,
        startTime = null,
        focusTime = null,
        pausedTime = null,
        clickPauseFlag = false,
        isPlay = true,
        inputMinutes,
        inputHours,
        inputDate,
        errorPanel;

    var init = function () {
        inputMinutes = jq("#inputTimeMinutes");
        inputHours = jq("#inputTimeHours");
        inputDate = jq('#inputDate');
        errorPanel = jq("#timeTrakingErrorPanel");
        
        window.onfocus = function () {
            var diffTime = {};
            if (startTime && seconds > 0 && !isPlay) {
                var focusTime = new Date();
                if (focusTime.getHours() > startTime.getHours()) {
                    diffTime.h = focusTime.getHours() - startTime.getHours();
                } else {
                    diffTime.h = 0;
                }
                if (focusTime.getMinutes() >= startTime.getMinutes()) {
                    diffTime.m = focusTime.getMinutes() - startTime.getMinutes(); 
                } else if (diffTime.h > 0) {
                    diffTime.m = (focusTime.getMinutes() + 60) - startTime.getMinutes();
                    diffTime.h--;
                    if (diffTime.m == 60) {
                        diffTime.m = 0;
                        diffTime.h++;
                    }
                } else {
                    diffTime.m = 0;
                }
                if (focusTime.getSeconds() >= startTime.getSeconds()) {
                    diffTime.s = focusTime.getSeconds() - startTime.getSeconds();
                } else if (diffTime.m > 0) {
                    diffTime.s = (focusTime.getSeconds() + 60) - startTime.getSeconds();
                    diffTime.m--;
                    if (diffTime.s == 60) {
                        diffTime.s = 0;
                        diffTime.m++;
                    }
                } else {
                    diffTime.s = 0;
                }
                var time = {};
                if (clickPauseFlag) {
                    time = timeSum(pausedTime, diffTime);
                } else {
                    time = diffTime;
                }
                timerHours = time.h;
                timerMin = time.m;
                timerSec = time.s;
                updateTimer(time);
            }
        };

        if (jq("#addLog").length) {
            window.onbeforeunload = function(evt) {
                if (jq("#timerTime .start").hasClass("stop")) {
                    window.ASC.Projects.TimeTraking.playPauseTimer();
                    return '';
                }

                if (window.ASC.Projects.TimeTraking.ifNotAdded()) {
                    return '';
                }
                return;
            };

            unlockElements();
            inputHours.focus();
            
            if (jq("#teamList option").length == 0 || jq("#selectUserTasks option").length == 0) {
                lockStartAndAddButtons();
            }

            if (!inputDate.hasClass('hasDatepicker')) {
                inputDate.datepicker({ selectDefaultDate: false, onSelect: function () { inputDate.blur(); } });
            }

            inputDate.mask(ASC.Resources.Master.DatePatternJQ);
            inputDate.datepicker('setDate', Teamlab.getDisplayDate(new Date()));

            jq('#timerTime #selectUserProjects').bind('change', function(event) {
                var prjid = parseInt(jq("#selectUserProjects option:selected").val());

                Teamlab.getPrjTeam({}, prjid, {
                    before: function() {
                        jq("#teamList").attr("disabled", "disabled");
                        jq("#selectUserTasks").attr("disabled", "disabled");
                    },
                    success: onGetTeam,
                    after: function() {
                        jq("#teamList").removeAttr("disabled");
                        jq("#selectUserTasks").removeAttr("disabled"); ;
                    }
                });

                Teamlab.getPrjTasks({}, null, null, null, { success: onGetTasks, filter: { sortBy: 'title', sortOrder: 'ascending', projectId: prjid} });
            });

            jq('#timerTime .start').bind('click', function(event) {
                if (jq(this).hasClass("disable")) return;
                playPauseTimer();
            });

            jq('#timerTime .reset').bind('click', function (event) {
                if (jq(this).hasClass("disable")) return;
                resetTimer();
            });

            jq('#timerTime #addLog').bind('click', function (event) {
                if (jq(this).hasClass("disable")) return;
                lockStartAndAddButtons();
                var h, m, s;
                var prjid = parseInt(jq("#selectUserProjects option:selected").attr("value"));
                var personid = jq("#teamList option:selected").attr("value");
                var taskid = parseInt(jq("#selectUserTasks option:selected").attr("value"));
                var description = jq.trim(jq("#textareaTimeDesc").val());
                inputHours.removeClass('error');
                inputMinutes.removeClass('error');
                errorPanel.empty();
                var invalidTime = false;

                if (seconds > 0) {
                    h = parseInt(jq("#firstViewStyle .h").text(), 10);
                    m = parseInt(jq("#firstViewStyle .m").text(), 10);
                    s = parseInt(jq("#firstViewStyle .s").text(), 10);
                    if (!(h > 0)) h = 0;
                    if (!(m > 0)) m = 0;
                    if (!(s > 0)) s = 0;
                    var hours = h + m / 60 + s / 3600;

                    resetTimer();
                } else {
                    if (inputHours.val() == "" && inputMinutes.val() == "") {
                        errorPanel.addClass('error').removeClass("success");
                        errorPanel.text(ASC.Projects.Resources.ProjectsJSResource.TimerNoData);
                        unlockStartAndAddButtons();
                        return;
                    }
                    h = parseInt(inputHours.val(), 10);
                    m = parseInt(inputMinutes.val(), 10);

                    if (h < 0 || !isInt(h)) {
                        inputHours.addClass('error');
                        invalidTime = true;
                    }

                    if (m > 59 || m < 0 || !isInt(m)) {
                        inputMinutes.addClass('error');
                        invalidTime = true;
                    }

                    if (invalidTime) {
                        errorPanel.addClass('error').removeClass("success");
                        errorPanel.text(ASC.Projects.Resources.ProjectsJSResource.InvalidTime).show();
                        unlockStartAndAddButtons();
                        return;
                    }

                    if (!(h > 0)) h = 0;
                    if (!(m > 0)) m = 0;

                    var hours = h + m / 60;
                }

                var data = { hours: hours, note: description, personId: personid, projectId: prjid };
                data.date = inputDate.datepicker('getDate');
                data.date.setHours(0);
                data.date.setMinutes(0);
                data.date = Teamlab.serializeTimestamp(data.date);

                Teamlab.addPrjTime({}, taskid, data, { success: onAddTaskTime });
            });

            inputMinutes.on("blur", function (e) {
                var min = jq(this).val();
                if (min.length == 1) {
                    jq(this).val("0" + min);
                }
            });
        }
    };

    var isInt = function(input) {
        return parseInt(input, 10) == input;
    };

    var playTimer = function() {
        lockElements(true);

        inputHours.val('').removeClass('error');
        inputMinutes.val('').removeClass('error');
        errorPanel.empty();

        timer = setInterval(timerTick, 1000);
    };

    var timerTick = function () {
        timerSec++;
        if (timerSec == 60) {
            timerSec = 0;
            timerMin++;
            if (timerMin == 60) {
                timerMin = 0;
                timerHours++;
            }
        }
        var time = { h: timerHours, m: timerMin, s: timerSec };
        updateTimer(time);
    };

    var pauseTimer = function () {
        pausedTime = getCurrentTime();
        window.clearTimeout(timer);
    };

    var updateTimer = function (time) {
        seconds = time.h * 60 * 60 + time.m * 60 + time.s;      // refactor?
        if (time.h < 10) showHours = "0" + time.h; else showHours = time.h;
        if (time.m < 10) showMin = "0" + time.m; else showMin = time.m;
        if (time.s < 10) showSec = "0" + time.s; else showSec = time.s;

        jq("#firstViewStyle .h").text(showHours);
        jq("#firstViewStyle .m").text(showMin);
        jq("#firstViewStyle .s").text(showSec);
    };

    var getCurrentTime = function () {
        var time = {};
        time.h = parseInt(jq("#firstViewStyle .h").text(), 10);
        time.m = parseInt(jq("#firstViewStyle .m").text(), 10);
        time.s = parseInt(jq("#firstViewStyle .s").text(), 10);
        return time;
    };

    var timeSum = function (firstTime, secondTime) {
        var resultTime = {};
        resultTime.h = firstTime.h + secondTime.h;
        resultTime.m = firstTime.m + secondTime.m;
        if (resultTime.m >= 60) {
            resultTime.h++;
            resultTime.m -= 60;
        }
        resultTime.s = firstTime.s + secondTime.s;
        if (resultTime.s >= 60) {
            resultTime.m++;
            resultTime.s -= 60;
        }
        return resultTime;
    };

    var resetTimer = function() {
        unlockElements();
        pauseTimer();

        jq("#firstViewStyle .h").text('00');
        jq("#firstViewStyle .m").text('00');
        jq("#firstViewStyle .s").text('00');

        var startButton = jq("#timerTime .start");
        startButton.removeClass("stop").attr("title", startButton.attr("data-title-start"));
        isPlay = true;
        seconds = 0;
        timerSec = 0;
        timerMin = 0;
        timerHours = 0;
        startTime = null;
        clickPauseFlag = false;
    };

    var lockElements = function(onlyManualInput) {
        inputHours.attr("disabled", "true");
        inputMinutes.attr("disabled", "true");
        if (!onlyManualInput) {
            inputDate.attr("disabled", "true");
            jq("#textareaTimeDesc").attr("disabled", "true");
        }
    };

    var unlockElements = function() {
        inputHours.removeAttr("disabled");
        inputMinutes.removeAttr("disabled");
        inputDate.removeAttr("disabled");
        jq("#textareaTimeDesc").removeAttr("disabled");
    };

    var lockStartAndAddButtons = function() {
        jq("#firstViewStyle .start, #firstViewStyle .reset, #addLog").addClass("disable");
        lockElements();
    };

    var unlockStartAndAddButtons = function() {
        jq("#firstViewStyle .start, #firstViewStyle .reset, #addLog").removeClass("disable");
        unlockElements();
    };

    var playPauseTimer = function() {
        var startButton = jq("#timerTime .start");
        if (isPlay) {
            startButton.addClass("stop").attr("title", startButton.attr("data-title-pause"));
            isPlay = false;
            startTime = new Date();
            playTimer();
        }
        else {
            startButton.removeClass("stop").attr("title", startButton.attr("data-title-start"));
            isPlay = true;
            clickPauseFlag = true;
            startTime = null;
            pauseTimer();
        }
    };

    var onAddTaskTime = function(data) {
        errorPanel.removeClass("error").addClass("success");
        errorPanel.text(ASC.Projects.Resources.ProjectsJSResource.SuccessfullyAdded);
        jq("#textareaTimeDesc").val('');
        inputHours.val('');
        inputMinutes.val('');
        jq("#firstViewStyle .h,#firstViewStyle .m,#firstViewStyle .s").val('00');
        unlockStartAndAddButtons();
    };

    var onGetTeam = function(params, team) {
        var teamList = jq('#teamList');
        teamList.find('option').remove();
        
        team = ASC.Projects.Common.excludeVisitors(team);
        
        team.forEach(function (item) {
            if (item.displayName != "profile removed") {
                teamList.append('<option value="' + item.id + '" id="optionUser_' + item.id + '">' + item.displayName + '</option>');
            }
        });
        
        if (teamList.find('option').length == 0) {
            lockStartAndAddButtons();
        } else {
            unlockStartAndAddButtons();
        }
    };

    var onGetTasks = function(params, tasks) {
        var taskInd = tasks ? tasks.length : 0;
        jq('#selectUserTasks option').remove();

        var openTasks = jq('#openTasks');
        var closedTasks = jq('#closedTasks');

        tasks.forEach(function(item) {
            if (item.status == 1) {
                openTasks.append('<option value="' + item.id + '" id="optionUser_' + item.id + '">' + jq.htmlEncodeLight(item.title) + '</option>');
            }
            if (item.status == 2) {
                closedTasks.append('<option value="' + item.id + '" id="optionUser_' + item.id + '">' + jq.htmlEncodeLight(item.title) + '</option>');
            }
        });
        
        if (jq("#selectUserTasks option").length == 0) {
            lockStartAndAddButtons();
        } else {
            unlockStartAndAddButtons();
        }
    };

    var ifNotAdded = function() {
        return seconds > 0 || inputHours.val() != '' || inputMinutes.val() != '';
    };

    return {
        init: init,
        playPauseTimer: playPauseTimer,
        ifNotAdded: ifNotAdded
    };
})(jQuery);

ASC.Projects.TimeTrakingEdit = (function () {
    var timeCreator,
        isInit,
        oldTime = {},
        loadListTeamFlag = false,
        commonPopupContainer = jq("#commonPopupContainer"),
        $popupContainer,
        inputMinutes,
        inputHours,
        errorPanel;

    var initPopup = function () {
        if (isInit) return;
        var clonedPopup = commonPopupContainer.clone();
        clonedPopup.attr("id", "timeTrakingPopup");
        jq("#CommonListContainer").append(clonedPopup);
        $popupContainer = clonedPopup;
        $popupContainer.find(".commonPopupContent").append(jq.tmpl("projects_editTimerPopup", {}));
        $popupContainer.find(".commonPopupHeaderTitle").empty().text($popupContainer.find(".hidden-title-text").text());
    };

    var init = function () {
        initPopup();
        
        if (!isInit) {
            isInit = true;
        }
        
        inputMinutes = jq("#inputTimeMinutes");
        inputHours = jq("#inputTimeHours");
        errorPanel = jq("#timeTrakingErrorPanel");
        
        inputMinutes.on("blur", function (e) {
            var min = jq(this).val();
            if (min.length == 1) {
                jq(this).val("0" + min);
            }
        });
        
        if (jq.getURLParam("prjID"))
                loadListTeamFlag = true;

        jq('#timeTrakingPopup .middle-button-container a.button.blue.middle').bind('click', function (event) {
            var data = {};
            var h = inputHours.val();
            var m = inputMinutes.val();

            if (checkError(h, m, jq('#timeTrakingPopup #timeTrakingDate').val())) {
                return;
            }
           
            m = parseInt(m, 10);
            h = parseInt(h, 10);

            if (!(h > 0)) h = 0;
            if (!(m > 0)) m = 0;

            data.hours = h + m / 60;
            
            var timeid = jq("#timeTrakingPopup").attr('timeid');
            
            data.date = Teamlab.serializeTimestamp(jq('#timeTrakingPopup #timeTrakingDate').datepicker('getDate'));
            data.note = jq('#timeTrakingPopup #timeDescription').val();
            data.personId = jq('#teamList option:selected').attr('value');

            Teamlab.updatePrjTime({ oldTime: oldTime, timeid: timeid }, timeid, data, { error: onUpdatePrjTimeError });

            jq.unblockUI();

        });
    };

    var checkError = function (h, m, d) {
        var error = false;
        
        if (parseInt(m, 10) > 59 || parseInt(m, 10) < 0 || !isInt(m)) {
            errorPanel.text(ASC.Projects.Resources.ProjectsJSResource.InvalidTime);
            inputMinutes.focus();
            error = true;
        }
        if (parseInt(h, 10) < 0 || !isInt(h)) {
            errorPanel.text(ASC.Projects.Resources.ProjectsJSResource.InvalidTime);
            inputHours.focus();
            error = true;
        }
        
        if (jq.trim(d) == "" || d == null || !jq.isDateFormat(d)) {
            errorPanel.text(ASC.Projects.Resources.ProjectsJSResource.IncorrectDate);
            jq('#timeTrakingPopup #timeTrakingDate').focus();
            error = true;
        }

        if (error) {
            errorPanel.show();
            setInterval('jq("#timeTrakingErrorPanel").hide();', 5000);
        }

        return error;
    };
    
    var isInt = function (input) {
        return parseInt(input, 10) == input;
    };
    
    var showPopup = function (prjid, taskid, taskName, timeId, time, date, description, responsible) {
        $timerDate = jq("#timeTrakingDate");
        timeCreator = responsible;
        $popupContainer.attr('timeid', timeId);
        jq("#timeDescription").val(description);
        jq("#TimeLogTaskTitle").text(taskName);
        jq("#TimeLogTaskTitle").attr('taskid', taskid);

        oldTime = time;
        inputHours.val(time.hours);
        if (time.minutes > 9) inputMinutes.val(time.minutes);
        else inputMinutes.val("0" + time.minutes);

        date = Teamlab.getDisplayDate(new Date(date));
        $timerDate.mask(ASC.Resources.Master.DatePatternJQ);
        $timerDate.datepicker({ popupContainer: "#timeTrakingPopup", selectDefaultDate: true });
        $timerDate.datepicker('setDate', date);
        jq('select#teamList option').remove();

        Teamlab.getPrjTime({}, taskid, { success: onGetTimeSpend });
        if (loadListTeamFlag) {
            jq('select#teamList option').remove();
            if (ASC.Projects.Master.Team.length) {
                appendListOptions(ASC.Projects.Common.excludeVisitors(ASC.Projects.Master.Team));
            }
        } else {
            Teamlab.getPrjProjectTeamPersons({}, prjid, { success: onGetTeamByProject });
        }

        StudioBlockUIManager.blockUI($popupContainer, 550, 400, 0, "absolute");
        inputHours.focus();
    };

    var onGetTimeSpend = function (params, data) {
        var hours = data.reduce(function (a, b) { return a + b.hours; }, 0);
        
        var time = jq.timeFormat(hours);
        jq(".addLogPanel-infoPanelBody #TotalHoursCount").text(time);
    };

    var appendListOptions = function (team) {
        var teamListSelect = jq('select#teamList');
        team.forEach(function (item) {
            if (timeCreator == item.id) {
                teamListSelect.append(jq('<option value="' + item.id + '" selected="selected"></option>').html(item.displayName));
            } else {
                teamListSelect.append(jq('<option value="' + item.id + '"></option>').html(item.displayName));
            }
        });
    };
    var onUpdatePrjTimeError = function (params, data) {
        jq("div.entity-menu[timeid=" + params.timeid + "]").hide();
    };

    var onGetTeamByProject = function (params, data) {
        var team = data;
        if (team.length) {
            appendListOptions(ASC.Projects.Common.excludeVisitors(team));
        }
    };
    return {
        init: init,
        showPopup: showPopup
    };
})(jQuery);