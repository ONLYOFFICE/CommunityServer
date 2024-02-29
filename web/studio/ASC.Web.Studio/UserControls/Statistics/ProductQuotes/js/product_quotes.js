/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
        $recalculateUserQuotaButton,
        $tabsHeaderItems,
        $tabCorner,
        $tabsContent,
        $statisticsItemListTmpl,
        $editQuotaVal,
        disableClass = "disable",
        activeClass = "active",
        timeOut = 10000,
        teamlab;

    function init() {
        $recalculateButton = jq('.storeUsage .button.blue');
        $recalculateUserQuotaButton = jq("#recalculateQuota");
        $initRecalculateUserQuotaButton = jq("#initRecalculateQuota");
        $tabsHeaderItems = jq(".tabs-header .tabs-header-item");
        $tabCorner = jq(".tabs-header .tabs-header-corner");
        $tabsContent = jq(".tabs-content");
        $statisticsItemListTmpl = jq("#statisticsItemListTmpl");
        $editQuotaVal = jq("#editQuotaVal");
        teamlab = Teamlab;

        initClickEvents();

        if ($recalculateUserQuotaButton.length || $initRecalculateUserQuotaButton.length)
            checkUserQuotaRecalculate();

        if ($recalculateButton.length)
            checkRecalculate();

        if ($tabsHeaderItems.length)
            $tabsHeaderItems[0].click();


        var size = jq("#editQuotaVal").attr('data');
      
        var sizeNames = ASC.Resources.Master.FileSizePostfix ? ASC.Resources.Master.FileSizePostfix.split(',') : ["bytes", "KB", "MB", "GB", "TB"];
        var sizeNameIndex = (sizeNames.indexOf(size)); 

        $editQuotaVal.prop('title', sizeNames[0]);
        $editQuotaVal.text(sizeNames[0]);

        $editQuotaVal.advancedSelector({
            height: 30 * sizeNames.length,
            itemsSelectedIds: [sizeNameIndex],
            onechosen: true,
            showSearch: false,
            itemsChoose: sizeNames.map(function (item, index) { return { id: index, title: item } }),
            sortMethod: function () { return 0; }
        })
        .on("showList",
            function (event, item) {
                $editQuotaVal.html(item.title).attr("title", item.title).attr("data", item.title).attr("data-id", item.id);
                jq("#saveQuota").removeClass(disableClass);
            });

        jq("#editQuotaVal").html(size).attr("title", size).attr('data-id', sizeNameIndex);
        jq("input[name=\"quota\"]").on("change", function () {
            jq("#saveQuota").removeClass(disableClass);

            if (jq(this).attr('id') == "quota-enabled") {
                jq("#setQuotaForm").removeClass("display-none");
            } else {
                jq("#setQuotaForm").addClass("display-none");
            }
        });
        jq('#setQuotaForm input').on('input', function () {
            this.value = this.value.replace(/[^0-9\.\,]/g, '');
            jq("#saveQuota").removeClass(disableClass);
        });
        jq("#saveQuota").on('click', saveQuota);
        $recalculateUserQuotaButton.on('click', recalculateUsedSpace);
        $initRecalculateUserQuotaButton.on('click', initRecalculateUsedSpace);

    }
    function initRecalculateUsedSpace() {
        teamlab.recalculateUserQuota({}, {
            success: function (params, data) {
                toastr.success(ASC.Resources.Master.ResourceJS.OperationStartedMsg);

                onUserQuotaInitRecalculate();
            }
        });
    }
    function recalculateUsedSpace() {
        teamlab.recalculateUserQuota({}, {
            success: function (params, data) {
                toastr.success(ASC.Resources.Master.ResourceJS.OperationStartedMsg);
                LoadingBanner.showLoaderBtn(".quotaSettingBlock");
                onUserQuotaRecalculate();
            }
        });
        teamlab.recalculateQuota({}, {
            success: function (params, data) {},
            error: function (params, errors) {
                console.error(errors);
            }
        });
    }
    function saveQuota() {
        if (jq(this).hasClass('disable')) {
            return;
        }
        var enabledQuota = jq("#quota-enabled").is(":checked");
        var quota = -1;

        if (enabledQuota) {
            var quotaLimit = parseInt(jq("#setQuotaForm input").val());
            var quotaVal = jq("#editQuotaVal").attr("data-id");

            switch (quotaVal) {
                case '0':                                               //bytes
                    quota = quotaLimit;
                    break;
                case '1':                                               //KB
                    quota = quotaLimit * 1024;
                    break;
                case '2':                                               //GB
                    quota = parseInt(quotaLimit * Math.pow(1024, 2))
                    break;
                case '3':
                    quota = parseInt(quotaLimit * Math.pow(1024, 3))
                    break;
                case '4':
                    quota = parseInt(quotaLimit * Math.pow(1024, 4))
                    break;
            }
        }

        var data = { EnableUserQuota: enabledQuota, DefaultUserQuota: quota };

        Teamlab.updateDefaultUsersQuota({}, data,
            {
                success: function (params, data) {
                    if (enabledQuota) {
                        jq("#peopleHeaderMenu").removeClass("no-quota");
                        toastr.success(ASC.Resources.Master.ResourceJS.QuotaEnabled);
                    } else {
                        jq("#peopleHeaderMenu").addClass("no-quota");
                        toastr.success(ASC.Resources.Master.ResourceJS.QuotaDisabled);
                    }
                    ASC.UserQuotaController.searchQuery();
                },
                before: LoadingBanner.displayLoading,
                after: LoadingBanner.hideLoading,
                error: function (params, errors) {
                    toastr.error(errors);
                }
            });
    }
    function initClickEvents() {

        jq.switcherAction(".toggle-button", ".toggle-content");
        
        jq(".tabs-header").on("click", ".tabs-header-item:not(.active)", setActiveTab);

        jq(".tabs-content").on("click", ".moreBox .link", showMore);

        $recalculateButton.on("click", function () {
            if ($recalculateButton.hasClass(disableClass)) return;

            teamlab.recalculateQuota({}, { success: onRecalculate });
        });
    }

    function onRecalculate() {
        $recalculateButton.addClass(disableClass);
        setTimeout(checkRecalculate, timeOut);
    }
    
    function onUserQuotaInitRecalculate() {
        $initRecalculateUserQuotaButton.addClass(disableClass);
        setTimeout(checkUserQuotaRecalculate, timeOut);
    }
    function onUserQuotaRecalculate() {
        $recalculateUserQuotaButton.addClass(disableClass);
        setTimeout(checkUserQuotaRecalculate, timeOut);
    }

    function checkRecalculate() {
        teamlab.checkRecalculateQuota({}, { success: onCheckRecalculate });
    }

    function checkUserQuotaRecalculate() {
        Teamlab.checkUserRecalculateQuota({}, {
            success: onCheckUserQuotaRecalculate});
    }

    function onCheckUserQuotaRecalculate(params, result) {
        if (!result) {
            if ($recalculateUserQuotaButton.length == 0) {
                window.location.reload();
            } else {
                $recalculateUserQuotaButton.removeClass(disableClass);
                LoadingBanner.hideLoaderBtn(".quotaSettingBlock");
                toastr.success(ASC.Resources.Master.ResourceJS.OperationCompletedMsg);
                return;
            }

        }
        if (!$recalculateUserQuotaButton.hasClass(disableClass)) {
            $recalculateUserQuotaButton.addClass(disableClass);
        }

        setTimeout(checkUserQuotaRecalculate, timeOut);
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