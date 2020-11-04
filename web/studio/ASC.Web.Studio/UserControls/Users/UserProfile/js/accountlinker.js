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
    jq("#accountLinks, .account-links").delegate(".popup", "click", function () {
        var obj = jq(this);
        if (obj.hasClass('linked')) {
            //unlink
            Teamlab.thirdPartyUnLinkAccount({ provider: obj.attr('id') }, { provider: obj.attr('id') }, {
                success: function (params, response) {
                    for (var i = 0, n = window.AccountLinkControl_Providers.length; i < n; i++) {
                        if (window.AccountLinkControl_Providers[i].Provider == params.provider) {
                            window.AccountLinkControl_Providers[i].Linked = false;
                            break;
                        }
                    }
                    jq("#accountLinks").html(jq.tmpl("template-accountLinkCtrl", { infos: window.AccountLinkControl_Providers, lang: ASC.Resources.Master.TwoLetterISOLanguageName }));
                },
                error: function (params, errors) {
                    toastr.error(errors[0]);
                }
            });
        }
        else {
            var link = obj.attr('href');
            window.open(link, 'login', 'width=800,height=500,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no');
        }
        return false;
    });

    if (window.AccountLinkControl_SettingsView === true) {
        jq("#accountLinks").html(jq.tmpl("template-accountLinkCtrl", { infos: window.AccountLinkControl_Providers, lang: ASC.Resources.Master.TwoLetterISOLanguageName }));
    }
});

function loginCallback(profile) {
    Teamlab.thirdPartyLinkAccount({provider: profile.Provider}, { serializedProfile: profile.Serialized }, {
        success: function (params, response) {
            for (var i = 0, n = window.AccountLinkControl_Providers.length; i < n; i++) {
                if (window.AccountLinkControl_Providers[i].Provider == params.provider) {
                    window.AccountLinkControl_Providers[i].Linked = true;
                    break;
                }
            }
            jq("#accountLinks").html(jq.tmpl("template-accountLinkCtrl", { infos: window.AccountLinkControl_Providers, lang: ASC.Resources.Master.TwoLetterISOLanguageName }));
        },
        error: function (params, errors) {
            toastr.error(errors[0]);
        }
    });
}

function authCallback(profile) {
    if (profile.AuthorizationError && profile.AuthorizationError.length) {
        if (profile.AuthorizationError != "Canceled at provider")
        {
            jq("#authMessage").html("<div class='errorBox'>" + Encoder.htmlEncode(profile.AuthorizationError) + "</div>")
        }
    } else {
        window.submitForm("signInLogin", profile.Hash);
    }
}

function loginJoinCallback(profile) {
    window.submitForm("thirdPartyLogin", profile.Serialized);
}
