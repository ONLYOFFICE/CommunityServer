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


ASC.StorageSettings = (function () {
    function init() {
        var $storageSettingsTemplateBlock = jq("#storageSettingsBlockTemplate");
        var storageBlock = { id: "storage", title: ASC.Resources.Master.Resource.StorageStorageTitle };
        var cdnBlock = { id: "cdn", title: ASC.Resources.Master.Resource.StorageCdnTitle };

        jq(".storageBlock")
            .append($storageSettingsTemplateBlock.tmpl(storageBlock))
            .append($storageSettingsTemplateBlock.tmpl(cdnBlock));

        Teamlab.getAllStorages({},
        {
            success: onGet.bind(null, storageBlock.id, Teamlab.updateStorage, Teamlab.resetToDefaultStorage)
        });

        Teamlab.getAllCdnStorages({},
        {
            success: onGet.bind(null, cdnBlock.id, Teamlab.updateCdnStorage, Teamlab.resetToDefaultCdn)
        });
    }

    function onGet(storageid, updateFunc, resetFunc, params, data) {
        var current = data.find(function (item) { return item.current; }) ||
            data.find(function (item) { return item.isSet; }) ||
            data[0];

        var selected = current;
        var $storage = jq("#" + storageid);
        var $authService = $storage.find(".auth-service");
        var $link = $authService.find(".link");
        var $storageSettingsTemplate = jq("#storageSettingsTemplate");
        var $storageBlock = $storage.find(".auth-data");

        $link.advancedSelector(
                {
                    itemsChoose: data,
                    showSearch: false,
                    onechosen: true,
                    sortMethod: function() { return 0; }
                }
            );

        $link.on("showList", function (event, item) {
            selected = data.find(function (dataItem) { return dataItem.id === item.id; });
            $link.text(selected.title);
            $storageBlock.html($storageSettingsTemplate.tmpl(selected));
        });

        $link.advancedSelector("selectBeforeShow", current);

        $authService.removeClass("display-none");

        var clickEvent = "click";
        $storageBlock.on(clickEvent, "[id^='saveBtn']",function () {
            var $currentButton = jq(this);
            if ($currentButton.hasClass("disable")) return;
            $currentButton.addClass("disable");

            var $item = $currentButton.parents(".storageItem");

            var data = {
                module: selected.id,
                props: initProps($item.find("input"))
            };

            updateFunc({},
            data,
            {
                success: function () {
                    location.reload();
                },
                error: function (params, data) {
                    toastr.error(data);
                }
            });
        });

        $storageBlock.on(clickEvent, "[id^='setDefault']", function () {
            var $currentButton = jq(this);
            if ($currentButton.hasClass("disable")) return;
            $currentButton.addClass("disable");

            resetFunc({},
            {
                success: function () {
                    location.reload();
                },
                error: function (params, data) {
                    toastr.error(data);
                }
            });
        });
    }

    function initProps($inputs) {
        var result = [];
        for (var i = 0; i < $inputs.length; i++) {
            var $inputItem = jq($inputs[i]);
            result.push({
                key: $inputItem.attr("id"),
                value: $inputItem.val()
            });
        }
        return result;
    }

    return { init: init };
})();

jq(document).ready(function () {
    ASC.StorageSettings.init();
});