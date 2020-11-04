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


(function () {
    ASC.Controls.AnchorController.bind('onupdate', function () {
        var anchor = ASC.Controls.AnchorController.getAnchor();
        var $tabObj = jq(".viewSwitcher>ul>.viewSwitcherTab_" + anchor).get(0);
        if ($tabObj) {
            viewSwitcherToggleTabs(jq($tabObj).attr("id"));
        }
    },
        {
            'once': true
        }
    );

})();

function viewSwitcherDropdownRegisterAutoHide (event, switcherID, dropdownID) {

    if (!jq((event.target) ? event.target : event.srcElement)
        .parents()
        .addBack()
        .is("'#" + switcherID + ", #" + dropdownID + "'"))
        jq("#" + dropdownID).hide();
}

function viewSwitcherToggleTabs (tabID) {

    var tab = jq('#' + tabID);

    tab.parent().children().each(
        function () {
            var child = jq(this);
            viewSwitcherToggleCurrentTab(child, tabID);
        }
    );
}

function viewSwitcherToggleCurrentTab (tab, tabID) {
    var hideFlag = true;
    var currentTabID = tab.attr('id');

    if (tab.hasClass('viewSwitcherTabSelected')) {
        tab.addClass('viewSwitcherTab');
        tab.removeClass('viewSwitcherTabSelected');

    }

    if (currentTabID == tabID) {
        tab.addClass('viewSwitcherTabSelected');
        tab.removeClass('viewSwitcherTab');
        hideFlag = false;
    }

    if (currentTabID == undefined || currentTabID == '' || currentTabID == null)
        currentTabID = '';

    var currentDivID = currentTabID.replace(/_ViewSwitcherTab$/gi, '');

    if (currentDivID != '') {
        if (hideFlag) {
            jq('#' + currentDivID).hide();
        } else {
            jq('#' + currentDivID).show();
        }
    }
}