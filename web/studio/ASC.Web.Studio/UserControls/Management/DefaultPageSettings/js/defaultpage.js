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


var DefaultPage = new function() {
    this.SaveSettings = function() {
        var selectedProductId = jq("input[name='defaultPage']:checked").val();

        Teamlab.setDefaultpage({}, selectedProductId, {
            bafore: function() { LoadingBanner.showLoaderBtn("#studio_defaultPageSettings"); },
            after: function() { LoadingBanner.hideLoaderBtn("#studio_defaultPageSettings"); },
            success: function() {
                LoadingBanner.showMesInfoBtn("#studio_defaultPageSettings", ASC.Resources.Master.Resource.SuccessfullySaveSettingsMessage, "success");
            },
            error: function(params, errors) {
                LoadingBanner.showMesInfoBtn("#studio_defaultPageSettings", errors[0], "error");
            }
        });
    };
};

jq(function() {
    jq("input[name=defaultPage]").each(function() {
        var obj = jq(this);
        if (obj.attr("checked") == "checked")
            obj.prop("checked", true);
    });
});