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
    jq.dropdownToggle({
        dropdownID: "languageMenu",
        switcherSelector: ".usrLang",
        addTop: 4,
        addLeft: -17,
        showFunction: function (switcherObj, dropdownItem) {
            if (dropdownItem.is(":hidden")) {
                switcherObj.addClass('active');
            } else {
                switcherObj.removeClass("active");
            }
        },
        hideFunction: function () {
            jq(".languageMenu.active").removeClass("active");
        }
    });

    jq(".languageMenu ul.options li.option").on("click", function () {
        var lng = jq(this).attr('data');

        if (!lng) {
            jq("#languageMenu").hide();
            var switcher = jq(".usrLang").next(".HelpCenterSwitcher");
            if (switcher.length)
                setTimeout(function() {
                    switcher.helper({ BlockHelperID: 'NotFoundLanguage' });
                }, 0);

            return;
        }

        AjaxPro.UserLangController.SaveUserLanguageSettings(lng, function (res) {
            jq("#languageMenu").hide();
            
            var langOption = jq("#languageMenu").find("li." + lng);
            if (langOption.length == 1) {
                jq(".usrLang").attr("class", "field-value usrLang active " + lng);
                jq(".usrLang>.val").text(langOption.children("a").html());
            }
            
            var result = res.value;
            if (result.Status == 1) {
                LoadingBanner.displayLoading();
                window.location.reload(true);
            } else if (result.Status == 0) {
                toastr.error(result.Message);
            }
        });
    });
});