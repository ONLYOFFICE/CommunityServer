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


if (typeof window.ASC === 'undefined') {
    window.ASC = {};
}
if (typeof window.ASC.Controls == 'undefined') {
    ASC.Controls = {};
}

window.ASC.Controls.ClientTabsNavigator = new function () {
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

    this.init = function (blockID, options) {
        var tab = {},
            tabCssName = "",
            tabHref = "",
            javascriptText = "",

            splitterHtml = "<span class=\"splitter\"></span>";
            html = ["<div class=\"clearFix\">",
                    "  <div id=\"",
                    blockID,
                    "\" class=\"tabsNavigationLinkBox\">"].join('');

        if (typeof (options) != "undefined" && options.hasOwnProperty("tabs"))
        {
            for (var i = 0, n = options.tabs.length; i < n; i++) {
                tab = options.tabs[i];
                if ((!tab.hasOwnProperty("divID") || jq("#" + tab.divID).length != 1) && !tab.hasOwnProperty("href")) {
                    continue;
                }
                if (!tab.hasOwnProperty("selected") || tab.selected == false) {
                    jq("#" + tab.divID + ":not(.display-none)").addClass("display-none");
                } else {
                    jq("#" + tab.divID + ".display-none").removeClass("display-none");
                }

                if (!tab.hasOwnProperty("visible") || tab.visible == true) {
                    tabCssName =
                        tab.hasOwnProperty("anchor")
                        ? ["tabsNavigationLink",
                            tab.hasOwnProperty("selected") && tab.selected == true ? " selectedTab" : "",
                            " tabsNavigationLink_",
                            tab.anchor
                        ].join('')
                        : tab.hasOwnProperty("selected") && tab.selected == true ? "tabsNavigationLink selectedTab" : "tabsNavigationLink";

                    if (!(!tab.hasOwnProperty("href") || tab.hasOwnProperty("selected") && tab.selected == true)) {
                        tabHref = [" href=\"", tab.href, "\""].join('');
                    } else {
                        tabHref = "";
                    }

                    if (!tab.hasOwnProperty("href")) {
                        javascriptText = [" onclick=\"",
                                            tab.hasOwnProperty("onclick") ? tab.onclick + ";" : "",
                                            " ASC.Controls.ClientTabsNavigator.toggleTabs(this.id, '",
                                            tab.anchor,
                                            "');\""
                                        ].join('');
                    }

                    html += ["<a id='",
                        tab.divID,
                        "_tab' class='",
                        tabCssName,
                        "'",
                        tabHref,
                        " ",
                        javascriptText,
                        ">",
                        tab.title,
                        "</a>",
                        splitterHtml].join('');
                }
            }
            if (html.lastIndexOf(splitterHtml) != -1) {
                html = html.substr(0, html.lastIndexOf(splitterHtml));
            }
 
        }
        html+= ["  </div>",
            "</div>"].join('');

        jq("#" + blockID).replaceWith(html);

        ASC.Controls.AnchorController.bind('onupdate', function () {
            var anchor = ASC.Controls.AnchorController.getAnchor();
            try {
                var $tabObj = jq(".tabsNavigationLinkBox>.tabsNavigationLink_" + anchor + ":first");
                if ($tabObj) {
                    ASC.Controls.ClientTabsNavigator.toggleTabs(jq($tabObj).attr("id"), "");
                }
            } catch (e) {
            }
        },
            { 'once': true }
        );

        
    }
};