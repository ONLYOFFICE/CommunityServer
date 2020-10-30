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


ASC.ProductQuotes = (function () {
    var $recalculateButton,
        $tabsHeaderItems,
        $tabCorner,
        $tabsContent,
        $statisticsItemListTmpl,
        disableClass = "disable",
        activeClass = "active",
        timeOut = 10000,
        teamlab;

    function init() {
        $recalculateButton = jq('.storeUsage .button.blue');
        $tabsHeaderItems = jq(".tabs-header .tabs-header-item");
        $tabCorner = jq(".tabs-header .tabs-header-corner");
        $tabsContent = jq(".tabs-content");
        $statisticsItemListTmpl = jq("#statisticsItemListTmpl");
        teamlab = Teamlab;

        initClickEvents();

        if ($recalculateButton.length)
            checkRecalculate();

        if ($tabsHeaderItems.length)
            $tabsHeaderItems[0].click();
    }

    function initClickEvents() {

        jq.switcherAction(".toggle-button", ".toggle-content");
        
        jq(".tabs-header").on("click", ".tabs-header-item:not(.active)", setActiveTab);

        jq(".tabs-content").on("click", ".moreBox .link", showMore);

        $recalculateButton.click(function () {
            if ($recalculateButton.hasClass(disableClass)) return;

            teamlab.recalculateQuota({}, { success: onRecalculate });
        });
    }

    function onRecalculate() {
        $recalculateButton.addClass(disableClass);
        setTimeout(checkRecalculate, timeOut);
    }

    function checkRecalculate() {
        teamlab.checkRecalculateQuota({}, { success: onCheckRecalculate });
    }

    function onCheckRecalculate(params, result) {
        if (!result) {
            $recalculateButton.removeClass(disableClass);
            return;
        } 
        if (!$recalculateButton.hasClass(disableClass)) {
            $recalculateButton.addClass(disableClass);
        }

        setTimeout(checkRecalculate, timeOut);
    }

    function setActiveTab() {
        $tabsHeaderItems.removeClass(activeClass);
        var obj = jq(this).addClass(activeClass);

        if ($tabCorner.is(":visible"))
            $tabCorner.css("left", obj.position().left + (obj.width() / 2) - ($tabCorner.width() / 2));

        var itemId = obj.attr("data-id");
        var contentItem = jq("#tabsContent_" + itemId);
        var tabsContentItems = $tabsContent.find(".tabs-content-item");

        if (contentItem.length) {
            tabsContentItems.removeClass(activeClass);
            contentItem.addClass(activeClass);
            return;
        }

        teamlab.getSpaceUsageStatistics({}, itemId, {
            success: function (params, result) {
                tabsContentItems.removeClass(activeClass);
                $tabsContent.append($statisticsItemListTmpl.tmpl({ id: itemId, items: result }));
            },
            before: LoadingBanner.displayLoading,
            after: LoadingBanner.hideLoading,
            error: function (params, errors) {
                tabsContentItems.removeClass(activeClass);
                $tabsContent.append($statisticsItemListTmpl.tmpl({ id: itemId, items: [] }));

                if (errors && errors.length)
                    toastr.error(errors[0]);
            }
        });
    }

    function showMore() {
        var obj = jq(this);
        jq(obj.parent().prev()).find("tr").show();
        obj.parent().hide();
    }

    return { init: init };
})();

jq(document).ready(function () {
    ASC.ProductQuotes.init();
});