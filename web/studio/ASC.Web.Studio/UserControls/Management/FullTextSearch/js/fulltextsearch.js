/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


if (typeof ASC.Settings === "undefined")
    ASC.Settings = {};

ASC.Settings.FullTextSearch = (function () {
    var settingsBlock = jq(".fts-settings-block"),
        hostInput = settingsBlock.find(".host input"),
        portInput = settingsBlock.find(".port input"),
        inputs = [portInput, hostInput];
    
    var init = function () {
        jq('#ftsButtonSave').on("click", save);
        jq('#ftsButtonTest').on("click", test);
    };

    var save = function () {
        if (checkError()) return;

        blockFields();
        
        FullTextSearch.Save(getSettings(), function (result) {
            if (result.error != null) {
                toastr.error(result.error.Message);
            }
            unblockFields();
        });
    };

    var checkError = function() {
        HideRequiredError();

        var result = false;
        inputs.forEach(function (item) {
            if (!item.val()) {
                ShowRequiredError(item);
                result = true;
            }
        });
        
        return result;
    };

    var blockFields = function () {
        inputs.forEach(function(item) { item.attr("disabled", true); });
        LoadingBanner.showLoaderBtn("#settingsContainer");
    };
    
    var getSettings = function () {
        return {
            Host: hostInput.val(),
            Port: portInput.val()
        };
    };
    
    var unblockFields = function () {
        inputs.forEach(function (item) { item.attr("disabled", false); });
        LoadingBanner.hideLoaderBtn("#settingsContainer");
    };
    
    var test = function () {
        FullTextSearch.Test(function (result) {
            var toasterSuccessOrError = result.value.success ? toastr.success : toastr.error;
            toasterSuccessOrError(result.value.message);
        });
    };

    return {
        init: init
    };

})(jQuery);


jq(function() {
    ASC.Settings.FullTextSearch.init();
});