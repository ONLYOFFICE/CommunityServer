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


ASC.StorageSettings = (function () {

    function init() {
        var $storageSettingsTemplateBlock = jq("#storageSettingsBlockTemplate");
        var storageBlock = { id: "storage", title: ASC.Resources.Master.ResourceJS.StorageStorageTitle };
        var cdnBlock = { id: "cdn", title: ASC.Resources.Master.ResourceJS.StorageCdnTitle };

        jq(".storageBlock")
            .append($storageSettingsTemplateBlock.tmpl(storageBlock))
            .append($storageSettingsTemplateBlock.tmpl(cdnBlock));

        async.parallel([
            function (cb) {
                Teamlab.getAmazonS3Regions(null, {
                    success: function (_, response) {
                        cb(null, response);
                    },
                    error: function (_, errors) {
                        cb(errors[0]);
                    }
                });
            },
            function (cb) {
                Teamlab.getAllStorages(null, {
                    success: function (_, response) {
                        cb(null, response);
                    },
                    error: function (_, errors) {
                        cb(errors[0]);
                    }
                });
            },
            function (cb) {
                Teamlab.getAllCdnStorages(null, {
                    success: function (_, response) {
                        cb(null, response);
                    },
                    error: function (_, errors) {
                        cb(errors[0]);
                    }
                });
            }
        ], function (error, results) {
            if (error) {
                toastr.error(error);
                return;
            }

            window.ConsumerStorageSettings.initS3Regions(results[0]);

            onGet(storageBlock.id, Teamlab.updateStorage, Teamlab.resetToDefaultStorage, results[1]);

            onGet(cdnBlock.id, Teamlab.updateCdnStorage, Teamlab.resetToDefaultCdn, results[2]);
        });
    }

    function onGet(storageid, updateFunc, resetFunc, data) {
        var current = data.find(function (item) { return item.current; }) ||
            data.find(function (item) { return item.isSet; }) ||
            data[0];

        var selected = current;
        var $storage = jq("#" + storageid);
        var $authService = $storage.find(".auth-service");
        var $link = $authService.find(".link");
        var $storageSettingsTemplate = jq("#storageSettingsTemplate");
        var $storageBlock = $storage.find(".auth-data");

        var disableClass = "disable";

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

            var tmplData = window.ConsumerStorageSettings.getTmplData(selected);

            var $storageSettings = $storageSettingsTemplate.tmpl(tmplData);
            $storageBlock.html($storageSettings);

            $storageSettings.find(".storage").removeClass("display-none");

            if (selected.properties && selected.properties.length) {
                window.ConsumerStorageSettings.bindEvents($storageSettings.find(".storage"), $storageSettings.find(".button"), selected);

                if (selected.current) {
                    window.ConsumerStorageSettings.setProps($storageSettings, selected);
                    $storageSettings.find("input, select").prop("disabled", true);
                }
            }
        });

        $link.advancedSelector("selectBeforeShow", current);

        $authService.removeClass("display-none");

        var clickEvent = "click";
        $storageBlock.on(clickEvent, "[id^='saveBtn']",function () {
            var $currentButton = jq(this);
            if ($currentButton.hasClass(disableClass)) return;
            $currentButton.addClass(disableClass);

            var $item = $currentButton.parents(".storageItem");

            var data = {
                module: selected.id,
                props: window.ConsumerStorageSettings.getProps($item)
            };

            if (!data.props) return;

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
            if ($currentButton.hasClass(disableClass)) return;
            $currentButton.addClass(disableClass);

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

    return { init: init };
})();

jq(document).ready(function () {
    ASC.StorageSettings.init();
});