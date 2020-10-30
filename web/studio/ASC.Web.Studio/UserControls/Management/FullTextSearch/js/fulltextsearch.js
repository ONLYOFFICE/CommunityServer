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