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


if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Settings === "undefined")
    ASC.Settings = (function() { return {}; })();

ASC.Settings.ProductsAndInstruments = new function() {


    return {

        disableElements: function (disable) {
            if (disable) {
                LoadingBanner.displayLoading();
                jq(".web-item-list input[type=checkbox]").attr("disabled", true);
                jq("#btnSaveSettings").addClass("disable");
            } else {
                LoadingBanner.hideLoading();
                jq(".web-item-list input[type=checkbox]").removeAttr("disabled");
                jq("#btnSaveSettings").removeClass("disable");
            }
        },

        showInfoPanel: function (success, message) {
            LoadingBanner.showMesInfoBtn("#studio_productSettings", message, success ? "success" : "error");
        },

        changeSubItems: function(cbx) {
            var webItem = jq(cbx).closest(".web-item");
            var subItemList = jq(webItem).find(".web-item-subitem-list");
            if(subItemList.length > 0) {
                var checked = jq(cbx).is(":checked");
                if(checked) {
                    jq(subItemList).find("input[type=checkbox]").each(function() {
                        jq(this).prop("checked", true);
                    });
                    jq(subItemList).show();
                } else {
                    jq(subItemList).find("input[type=checkbox]").each(function() {
                        jq(this).prop("checked", false);
                    });
                    jq(subItemList).hide();
                }
            }
        },

        saveSettings: function () {

            var data = {};
            data.items = new Array();

            jq(".web-item").each(function() {
                var cbx = jq(this).find(".web-item-header input[type=checkbox]");
                var itemId = jq(cbx).attr("data-id");
                var itemEnabled = jq(cbx).is(":checked");

                var subItemList = jq(this).find(".web-item-subitem-list");
                if(subItemList.length > 0 && itemEnabled) {
                    var hasEnabledSubitems = false;

                    jq(subItemList).find("input[type=checkbox]").each(function () {
                        var subItemId = jq(this).attr("data-id");
                        var subItemEnabled = itemEnabled && jq(this).is(":checked");

                        if (subItemEnabled) hasEnabledSubitems = true;

                        data.items.push({
                            Key: subItemId,
                            Value: subItemEnabled
                        });
                    });
                    
                    if (!hasEnabledSubitems) itemEnabled = false;
                }

                data.items.push({
                    Key: itemId,
                    Value: itemEnabled
                });
            });

            Teamlab.setAccessToWebItems({}, data, {
                before: function() {
                    ASC.Settings.ProductsAndInstruments.disableElements(true);
                },
                error: function (params, error) {
                    ASC.Settings.ProductsAndInstruments.disableElements(false);
                    ASC.Settings.ProductsAndInstruments.showInfoPanel(false, error[0]);
                },
                success: function() {
                    ASC.Settings.ProductsAndInstruments.disableElements(false);
                    ASC.Settings.ProductsAndInstruments.showInfoPanel(true, ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage);
                    window.location.reload();
                }
            });

        }

    };
};

(function ($) {
    $(function () {

        jq(".web-item-header input[type=checkbox]").change(function () {
            ASC.Settings.ProductsAndInstruments.changeSubItems(this);
        });
        
        jq("#btnSaveSettings").click(function () {
            if (jq(this).hasClass("disable")) return;
            ASC.Settings.ProductsAndInstruments.saveSettings();
        });

    });
})(jQuery);

