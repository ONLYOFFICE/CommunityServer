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


if (typeof(ASC) == "undefined") {
    ASC = {};
}
if (typeof(ASC.Controls) == "undefined") {
    ASC.Controls = {};
}

ASC.Controls.TabsNavigator = new function () {
    (function () {
        ASC.Controls.AnchorController.bind('onupdate', function () {
            var anchor = ASC.Controls.AnchorController.getAnchor();
            try {
                var $tabObj = jq(".tabsNavigationLinkBox>.tabsNavigationLink_" + anchor + ":first");
                if ($tabObj) {
                    ASC.Controls.TabsNavigator.toggleTabs(jq($tabObj).attr("id"), "");
                }
            } catch (e) {
            }
        },
            { 'once': true }
        );
    })();

    var toggleCurrentTab = function (tab, tabID) {
        var hideFlag = true;
        var currentTabID = tab.attr("id");

        if (tab.hasClass("selectedTab")) {
            tab.removeClass("selectedTab");
        }
        if (currentTabID == tabID) {
            tab.addClass("selectedTab");
            hideFlag = false;
        }

        if (currentTabID == undefined || currentTabID == '' || currentTabID == null) {
            currentTabID = '';
        }
        var currentDivID = currentTabID.replace(/_tab$/gi, '');

        if (currentDivID != '' && jq("#" + currentDivID).length == 1) {
            if (hideFlag) {
                jq("#" + currentDivID + ":not(.display-none)").addClass("display-none");
            } else {
                jq("#" + currentDivID + ".display-none").removeClass("display-none");
            }
        }
    };

    this.toggleTabs = function (tabID, anchor) {
        var tab = jq("#" + tabID);
        tab.parent().children('a').each(
            function () {
                var child = jq(this);
                toggleCurrentTab(child, tabID);
            }
        );
        if (typeof(anchor) == "string" && anchor != "") {
            ASC.Controls.AnchorController.move(anchor);
        }
    };
};