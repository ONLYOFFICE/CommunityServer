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


window.IpSecurity = new function() {
    var $ = jq;

    var ipRegex = /^\s*(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\s*(\-\s*(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\s*)?$/;

    var $view = $('#iprestrictions-view');

    var restrictionTmpl = $view.find('#restriction-tmpl');

    var $settingsBlock = $view.find('.settings-block');
    var $restrictionsList = $view.find('#restrictions-list');
    var $addRestrictionBtn = $restrictionsList.find('#add-restriction-btn');

    var $ipsecurityOff = $view.find('#ipsecurityOff');
    var $ipsecurityOn = $view.find('#ipsecurityOn');
    var $saveRestrictionBtn = $view.find('#save-restriction-btn');

    var restrictions = [];

    function init() {
        bind$Events();
        getRestrictions(function(params, data) {
            restrictions = data;
            renderView();
        });
    }

    function getRestrictions(callback) {
        var ipsecurityDisable = $ipsecurityOn.is(':disabled');
        if (ipsecurityDisable) {
            return;
        }

        var ipsecurityOn = $ipsecurityOn.is(':checked');
        if (ipsecurityOn) {
            showLoader();
        }

        Teamlab.getIpRestrictions({
            success: function(params, data) {
                hideLoader();
                callback(params, data);
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    }

    function renderView() {
        var $restrictions = restrictionTmpl.tmpl(restrictions);
        $addRestrictionBtn.before($restrictions);
    }

    function bind$Events() {
        $ipsecurityOff.on('click', hideRestrictionsList);
        $ipsecurityOn.on('click', showRestrictionsList);

        $addRestrictionBtn.on('click', addRestriction);
        $saveRestrictionBtn.on('click', saveRestriction);

        $restrictionsList.on('click', '.restriction .delete-btn', deleteRestriction);
    }

    function hideRestrictionsList() {
        $restrictionsList.hide();
    }

    function showRestrictionsList() {
        $restrictionsList.show();
    }

    function addRestriction() {
        var $newRestriction = restrictionTmpl.tmpl();
        $addRestrictionBtn.before($newRestriction);
    }

    function saveRestriction() {
        if ($(this).is('.disable')) {
            return;
        }

        var ipsecurityOff = $ipsecurityOff.is(':checked');
        if (ipsecurityOff) {
            showLoader();
            Teamlab.updateIpRestrictionsSettings({ enable: false }, {
                success: function() {
                    hideLoader();
                    LoadingBanner.showMesInfoBtn($settingsBlock, ASC.Resources.Master.Resource.IPRestrictionsSettingsSuccessfullyUpdated, 'success');
                },
                error: function() {
                    hideLoader();
                    showErrorMessage();
                }
            });

            return;
        }

        var formRestrictions = [];
        $restrictionsList.find('.restriction .ip').each(function(idx, el) {
            formRestrictions.push($(el).val());
        });

        var restrictionsToSave = [];
        for (var i = 0; i < formRestrictions.length; i++) {
            var r = formRestrictions[i].replace(/\s/g, '');
            if (r == '') {
                continue;
            }

            if (!ipRegex.test(r)) {
                LoadingBanner.showMesInfoBtn($settingsBlock, ASC.Resources.Master.Resource.IncorrectIPAddressFormatError, 'error');
                return;
            }
            
            if (~restrictionsToSave.indexOf(r)) {
                LoadingBanner.showMesInfoBtn($settingsBlock, ASC.Resources.Master.Resource.SameIPRestrictionError, 'error');
                return;
            } else {
                restrictionsToSave.push(r);
            }
        }

        var enabled = restrictionsToSave.length > 0;

        showLoader();
        async.parallel([
                function(cb) {
                    Teamlab.saveIpRestrictions({ ips: restrictionsToSave }, {
                        success: function(params, data) {
                            cb(null, data);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                },
                function(cb) {
                    Teamlab.updateIpRestrictionsSettings({ enable: enabled }, {
                        success: function(params, data) {
                            cb(null, data);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                }], function(err) {
                    hideLoader();
                    if (err) {
                        showErrorMessage();
                    } else {
                        if (!enabled) {
                            $ipsecurityOff.click();
                        }
                        LoadingBanner.showMesInfoBtn($settingsBlock, ASC.Resources.Master.Resource.IPRestrictionsSettingsSuccessfullyUpdated, 'success');
                    }
                });
    }

    function deleteRestriction() {
        $(this).closest('.restriction').remove();
    }

    function showLoader() {
        LoadingBanner.showLoaderBtn($settingsBlock);
    }

    function hideLoader() {
        LoadingBanner.hideLoaderBtn($settingsBlock);
    }

    function showErrorMessage() {
        LoadingBanner.showMesInfoBtn($settingsBlock, ASC.Resources.Master.Resource.CommonJSErrorMsg, 'error');
    }

    return {
        init: init
    };
};

jq(function() {
    window.IpSecurity.init();
});