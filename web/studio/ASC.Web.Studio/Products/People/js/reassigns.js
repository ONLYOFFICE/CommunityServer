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


; if (typeof (ASC) === 'undefined')
    ASC = {};

if (typeof ASC.People === "undefined")
    ASC.People = (function () { return {}; })();

ASC.People.Reassigns = (function () {
    var isInit = false,
        removeData = false,
        aborted = false,
        fromUserId = null,
        toUserId = null,
        timeoutDelay = 3000,
        progressStatus = {
            queued: 0,
            started: 1,
            done: 2,
            failed: 3
        };

    function renderStatus(targetObj, statusClass, statusMessage, errorMessage) {
        targetObj.html(jq.tmpl("reassignStatus", { statusClass: statusClass, statusMessage: statusMessage, errorMessage: errorMessage }));
    }

    function renderItemStatus(item, data) {
        var step = item.attr("data-step");

        if (data.status == progressStatus.queued) {
            renderStatus(item, "queued", ASC.People.Resources.PeopleResource.ReassignStatusQueued, null);
            return;
        }

        if (step < data.percentage) {
            renderStatus(item, "finished", removeData ? ASC.People.Resources.PeopleResource.RemovingStatusFinished : ASC.People.Resources.PeopleResource.ReassignStatusFinished, null);
        } else if (step == data.percentage) {
            if (data.status == progressStatus.failed) {
                renderStatus(item, "error", removeData ? ASC.People.Resources.PeopleResource.RemovingStatusAborted : ASC.People.Resources.PeopleResource.ReassignStatusAborted, data.error);
            } else {
                renderStatus(item, "started", ASC.People.Resources.PeopleResource.ReassignStatusStarted, null);
            }
        } else {
            renderStatus(item, "notstarted", removeData ? ASC.People.Resources.PeopleResource.RemovingStatusNotStarted : ASC.People.Resources.PeopleResource.ReassignStatusNotStarted, null);
        }
    }

    function renderUserInfo(from, to) {
        if (from) {
            fromUserId = from;
            var fromUser = window.UserManager.getUser(fromUserId);
            jq(".from-user-link").attr("href", fromUser.profileUrl).html(fromUser.displayName);
        }

        if (to) {
            toUserId = to;
            var toUser = window.UserManager.getUser(toUserId);
            jq(".to-user-link").attr("href", toUser.profileUrl).html(toUser.displayName);
        }
    }

    function setBindings() {
        var $userSelector = jq("#userSelector");

        if ($userSelector.length) {
            $userSelector.useradvancedSelector({
                itemsChoose: [],
                itemsDisabledIds: [],
                canadd: false,
                isAdmin: false,
                showGroups: true,
                withGuests: false,
                onechosen: true
            });

            $userSelector.on("showList", function(event, item) {
                jq(this).html(item.title);
                toUserId = item.id;
                jq(".start-btn").removeClass("disable");
            });
        }

        jq(".start-btn").on("click", function () {
            if (jq(this).hasClass("disable")) return;

            aborted = false;

            var options = {
                before: LoadingBanner.displayLoading,
                after: LoadingBanner.hideLoading,
                success: function(params, data) {
                    renderUserInfo(data.fromUser, data.toUser);
                    showActionContainer(false);
                    renderProgress(data);
                },
                error: function(params, errors) {
                    toastr.error(errors[0]);
                }
            };

            if (removeData) {
                Teamlab.startRemove({}, fromUserId, options);
            } else {
                Teamlab.startReassign({}, fromUserId, toUserId, options);
            }
        });

        jq(".abort-btn").on("click", function () {
            aborted = true;

            var options = {
                before: LoadingBanner.displayLoading,
                after: LoadingBanner.hideLoading,
                success: function() {
                    var started = jq(".progress-container .progress-block .started");
                    var targetObj = started.length ? started.parent() : jq(".progress-container .progress-block .progress-desc:first");
                    renderStatus(targetObj, "aborted", removeData ? ASC.People.Resources.PeopleResource.RemovingStatusAborted : ASC.People.Resources.PeopleResource.ReassignStatusAborted, null);
                    toastr.warning(removeData ? ASC.People.Resources.PeopleResource.RemovingAbortToastrMsg : ASC.People.Resources.PeopleResource.ReassignAbortToastrMsg);
                    showButtons(false, true, false);
                },
                error: function(params, errors) {
                    toastr.error(errors[0]);
                }
            };

            if (removeData) {
                Teamlab.terminateRemove({}, fromUserId, options);
            } else {
                Teamlab.terminateReassign({}, fromUserId, options);
            }
        });
        
        jq(".restart-btn").on("click", function () {
            showActionContainer(true);
        });
    }

    function showActionContainer(show) {
        if (show) {
            jq(".action-container").removeClass("display-none");
            jq(".progress-container").addClass("display-none");
        } else {
            jq(".action-container").addClass("display-none");
            jq(".progress-container").removeClass("display-none");
        }
    }

    function showButtons(abort, restart, ok) {
        if (abort) {
            jq(".abort-btn").removeClass("display-none");
        } else {
            jq(".abort-btn").addClass("display-none");
        }
        
        if (restart) {
            jq(".restart-btn").removeClass("display-none");
        } else {
            jq(".restart-btn").addClass("display-none");
        }
        
        if (ok) {
            jq(".ok-btn").removeClass("display-none");
        } else {
            jq(".ok-btn").addClass("display-none");
        }
    }

    function renderProgress(data) {
        jq(".progress-block .progress-row .progress-desc").each(function (index, obj) {
            renderItemStatus(jq(obj), data);
        });

        if (data.status == progressStatus.failed)
            toastr.error(removeData ? ASC.People.Resources.PeopleResource.RemovingErrorToastrMsg : ASC.People.Resources.PeopleResource.ReassignErrorToastrMsg);

        if (data.isCompleted) {
            showButtons(false, data.status == progressStatus.failed, data.status == progressStatus.done);
        } else {
            showButtons(true, false, false);
            setTimeout(trackProgress, timeoutDelay);
        }  
    }

    function checkProgress() {
        var options = {
            before: LoadingBanner.displayLoading,
            after: LoadingBanner.hideLoading,
            success: function(params, data) {
                if (data && data.hasOwnProperty("isCompleted")) {
                    renderUserInfo(data.fromUser, data.toUser);
                    showActionContainer(false);
                    renderProgress(data);
                } else {
                    showActionContainer(true);
                }
            },
            error: function(params, errors) {
                showActionContainer(true);
                toastr.error(errors[0]);
            }
        };

        if (removeData) {
            Teamlab.getRemoveProgress({}, fromUserId, options);
        } else {
            Teamlab.getReassignProgress({}, fromUserId, options);
        }
    }
    
    function trackProgress() {
        if (aborted) return;

        var options = {
            success: function(params, data) {
                renderProgress(data);
            },
            error: function(params, errors) {
                toastr.error(errors[0]);
            }
        };

        if (removeData) {
            Teamlab.getRemoveProgress({}, fromUserId, options);
        } else {
            Teamlab.getReassignProgress({}, fromUserId, options);
        }
    }

    function init(userId, remove) {
        if (!userId || isInit) return;

        fromUserId = userId;
        removeData = remove;
        setBindings();
        checkProgress();
        isInit = true;
    };

    return {
        init: init
    };
})();
