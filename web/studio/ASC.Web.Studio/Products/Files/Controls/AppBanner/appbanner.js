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
    jq(".mobile-app-win").trackEvent("mobileApp-banner", "action-click", "app-win");
    jq(".mobile-app-max").trackEvent("mobileApp-banner", "action-click", "app-max");
    jq(".mobile-app-linux").trackEvent("mobileApp-banner", "action-click", "app-linux");
    jq(".mobile-app-android").trackEvent("mobileApp-banner", "action-click", "app-android");
    jq(".mobile-app-ios").trackEvent("mobileApp-banner", "action-click", "app-store");
});
