/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
    if (jq(".confirm-block-page").hasClass("confirm-personal")) {
        jq("body").addClass("body-personal-confirm");

        var $formOperationBlock = jq("#ctl00_PageContent_ctl00_operationBlock");
        AddPaddingWithoutScrollTo($formOperationBlock, $formOperationBlock);
        var $formResultBlock = jq("#ctl00_PageContent_ctl00_result");
        AddPaddingWithoutScrollTo($formResultBlock, $formResultBlock);

        jq(window).on("resize", function () {
            AddPaddingWithoutScrollTo($formOperationBlock, $formOperationBlock);
            AddPaddingWithoutScrollTo($formResultBlock, $formResultBlock);
        });
    }
});