/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

    var restrictions = [];

    function init() {
        bind$Events();
        getRestrictions(function(params, data) {
            restrictions = data;
            renderView();
        });
    }

    function getRestrictions(callback) {
        var ipsecurityDisable = $view.find('#ipsecurityOn').is(':disabled');
        if (ipsecurityDisable) {
            return;
        }

        var ipsecurityOn = $view.find('#ipsecurityOn').is(':checked');
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
        $('#ipsecurityOff').on('click', hideRestrictionsList);
        $('#ipsecurityOn').on('click', showRestrictionsList);

        $('#add-restriction-btn').on('click', addRestriction);
        $('#save-restriction-btn').on('click', saveRestriction);

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

        var ipsecurityOff = $view.find('#ipsecurityOff').is(':checked');
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
                    Teamlab.updateIpRestrictionsSettings({ enable: true }, {
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