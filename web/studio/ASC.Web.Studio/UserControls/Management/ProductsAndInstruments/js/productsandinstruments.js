/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

