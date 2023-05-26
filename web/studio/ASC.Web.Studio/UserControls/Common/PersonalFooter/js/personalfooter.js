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


jq(document).ready(function () {

    jq.dropdownToggle({
        switcherSelector: ".personal-languages_select",
        dropdownID: "AuthFormLanguagesPanel",
        addTop: 2,
        addLeft: -2,
        rightPos: true,
        inPopup: true,
        alwaysUp: false,
        beforeShowFunction: function () {
            $authForm = jq(".auth-form-with_form_w");
            if (!$authForm.parents(".first-screen-content").length && !$authForm.hasClass("separate-window")) {
                $authForm.hide();
            }
        }
    });
});