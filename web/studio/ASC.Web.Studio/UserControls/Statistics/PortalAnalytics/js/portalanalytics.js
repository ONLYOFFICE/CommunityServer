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


jq(function () {
    jq("#portalAnalyticsView .button").on("click", function () {
        if (jq(this).hasClass("disable")) return;

        Teamlab.updatePortalAnalytics({}, jq("#portalAnalyticsOn").is(":checked"), {
            before: function () {
                LoadingBanner.showLoaderBtn("#portalAnalyticsView");
            },
            after: function () {
                LoadingBanner.hideLoaderBtn("#portalAnalyticsView");
            },
            success: function () {
                LoadingBanner.showMesInfoBtn("#portalAnalyticsView", ASC.Resources.Master.Resource.ChangesSuccessfullyAppliedMsg, "success");
            },
            error: function (params, errors) {
                LoadingBanner.showMesInfoBtn("#portalAnalyticsView", errors[0], "error");
            }
        });
    });
});