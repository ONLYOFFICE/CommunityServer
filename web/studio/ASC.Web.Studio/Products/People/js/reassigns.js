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
        fromUserId = null,
        toUserId = null;

    function setBindings() {
        var $userSelector = jq("#userSelector");

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
            jq("#reassignBtn").removeClass("disable");
        });

        jq("#reassignBtn").on("click", function () {
            if (jq(this).hasClass("disable")) return;

            Teamlab.startReassign({}, fromUserId, toUserId, {
                success: function() {
                    showContainer(false);
                },
                before: LoadingBanner.displayLoading,
                after: LoadingBanner.hideLoading,
                error: function(params, errors) {
                    toastr.error(errors[0]);
                }
            });
        });

        jq("#terminateBtn").on("click", function () {
            Teamlab.terminateReassign({}, fromUserId, {
                success: function() {
                    window.location.replace(jq("#сancelBtn").attr("href"));
                },
                before: LoadingBanner.displayLoading,
                after: LoadingBanner.hideLoading,
                error: function(params, errors) {
                    toastr.error(errors[0]);
                }
            });
        });
    }

    function showContainer(action) {
        if (action) {
            jq("#reassignActionContainer").removeClass("display-none");
            jq("#reassignProgressContainer").addClass("display-none");
        } else {
            jq("#reassignActionContainer").addClass("display-none");
            jq("#reassignProgressContainer").removeClass("display-none");
        }
    }

    function checkProgress() {
        Teamlab.getReassignProgress({}, fromUserId, {
            success: function (params, data) {
                if (data && data.hasOwnProperty("isCompleted")) {
                    showContainer(data.isCompleted);
                } else {
                    showContainer(true);
                }
            },
            before: LoadingBanner.displayLoading,
            after: LoadingBanner.hideLoading,
            error: function (params, errors) {
                showContainer(true);
                toastr.error(errors[0]);
            }
        });
    }

    function init(userId) {
        if (!userId || isInit) return;
        
        fromUserId = userId;
        setBindings();
        checkProgress();
        isInit = true;
    };

    return {
        init: init
    };
})();
