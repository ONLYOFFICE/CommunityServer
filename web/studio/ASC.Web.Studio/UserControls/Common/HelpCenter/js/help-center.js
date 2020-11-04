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


var showHelpPage = function (helpId) {

    jq("#studioPageContent .mainPageContent").scrollTo(0);

    var $icon = jq("link[rel*=icon][type^='image']:last");
    if ($icon.attr('href').indexOf('logo_favicon_general.ico') !== -1) {//not default
        $icon.attr('href', $icon.attr('href'));
    }

    var hideAll = !!jq("#contentHelp-" + helpId).length;

    jq(".help-center").addClass("currentCategory open").toggleClass("active", !hideAll);
    jq(".help-center .menu-sub-item").removeClass("active");

    jq("[id^='contentHelp-']").toggleClass("display-none", hideAll);
    if (hideAll) {
        jq("#contentHelp-" + helpId).removeClass("display-none");
        jq(jq(".help-center .menu-sub-item")[helpId]).addClass("active");
    }
};

jq(function () {
    ASC.Controls.AnchorController.bind(/^help(?:=(\d+))?/, showHelpPage);
});
