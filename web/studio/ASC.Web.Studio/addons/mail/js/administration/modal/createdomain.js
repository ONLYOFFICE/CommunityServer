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


window.createDomainModal = (function($) {
    var requireDnsInfo;
    var firstStep = 1;
    var lastStep = ASC.Resources.Master.Standalone ? 4 : 5;
    var domainName = '';
    var domainOwnershipIsProved = false;

    function show(requireDnsInfoParam) {
        requireDnsInfo = requireDnsInfoParam;
        showStepPopup(firstStep);
        domainName = "";
        domainOwnershipIsProved = false;
    }

    function showStepPopup(step) {
        var html = $.tmpl('createDomainWizardTmpl',
        {
            step: step,
            total: lastStep,
            require_dns_info: requireDnsInfo,
            domain_name: domainName,
            tplName: 'domainWizardStep' + (ASC.Resources.Master.Standalone && step !== firstStep ? step + 1 : step)
        });

        $(html).find('.next').unbind('click').bind('click', function() {
            if ($(this).hasClass('disable')) {
                return false;
            }

            if (step == firstStep) {
                checkDomainExistance();
            } else if (step == firstStep + 1 && !ASC.Resources.Master.Standalone) {
                checkDomainOwnership();
            } else if (step == lastStep) {
                addDomain();
            } else {
                var newStep = step == lastStep ? lastStep : step + 1;
                showStepPopup(newStep);
            }
            return false;
        });

        if (step != firstStep) {
            $(html).find('.prev').unbind('click').bind('click', function() {
                if ($(this).hasClass('disable')) {
                    return false;
                }

                var newStep = step == firstStep ? firstStep : step - 1;
                showStepPopup(newStep);

                if (newStep == firstStep && domainName != '') {
                    $('#wizard_add_domain').find('.web_domain').val(domainName);
                }

                return false;
            });
        }

        $(html).find('.cancel').unbind('click').bind('click', function() {
            if ($(this).hasClass('disable')) {
                return false;
            }
            popup.hide();
            return false;
        });

        popup.hide();
        popup.addPopup(window.MailAdministrationResource.DomainWizardPopupHeader, html, step == firstStep ? 392 : 470, null, null, {
            onBlock: function () {
                if (step == firstStep) {
                    var domainInput = $('#mail_server_create_domain_wizard .web_domain');
                    domainInput.unbind('textchange').bind('textchange', function () {
                        TMMail.setRequiredError('wizard_add_domain', false);
                    });
                } else {
                    setTimeout(function () {
                        $('#mail_server_create_domain_wizard:visible .next').focus();
                    }, 100);
                }
            }
        });

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = "jq('#mail_server_create_domain_wizard:visible .next').click();";
    }

    function checkDomainExistance() {
        window.LoadingBanner.hideLoading();

        var newDomainName = $('#wizard_add_domain').find('.web_domain').val();
        if (newDomainName.length === 0) {
            TMMail.setRequiredHint('wizard_add_domain', window.MailScriptResource.ErrorEmptyField);
            TMMail.setRequiredError('wizard_add_domain', true);
            return;
        }

        if (!ASC.Mail.Utility.IsValidDomainName(newDomainName)) {
            TMMail.setRequiredHint('wizard_add_domain', window.MailApiErrorsResource.ErrorInvalidDomain);
            TMMail.setRequiredError('wizard_add_domain', true);
            return;
        }

        domainName = newDomainName;
        domainOwnershipIsProved = false;

        TMMail.setRequiredError('wizard_add_domain', false);

        displayLoading('#mail_server_create_domain_wizard', true);

        disableButtons(true);

        serviceManager.isDomainExists(domainName, {}, {
            success: function(e, isExists) {
                disableButtons(false);
                if (!isExists) {
                    if ($('#mail_server_create_domain_wizard').is(':visible')) {
                        $('#mail_server_create_domain_wizard .cancel').trigger('click');
                        showStepPopup(2);
                    }
                } else {
                    popup.error(MailAdministrationResource.DomainWizardDomainAlreadyExists.format(domainName));
                    displayLoading('#mail_server_create_domain_wizard', false);
                    domainOwnershipIsProved = false;
                    TMMail.setRequiredHint('wizard_add_domain', '');
                    TMMail.setRequiredError('wizard_add_domain', true);
                }
            },
            error: function(e, error) {
                disableButtons(false);
                popup.error(administrationError.getErrorText("checkDomainExistance", error));
                TMMail.setRequiredHint('wizard_add_domain', '');
                TMMail.setRequiredError('wizard_add_domain', true);
                displayLoading('#mail_server_create_domain_wizard', false);
            }
        });
    }

    function checkDomainOwnership() {
        window.LoadingBanner.hideLoading();

        displayLoading('#mail_server_create_domain_wizard', true);

        disableButtons(true);

        if (domainOwnershipIsProved) {
            if ($('#mail_server_create_domain_wizard').is(':visible')) {
                $('#mail_server_create_domain_wizard .cancel').trigger('click');
                showStepPopup(3);
                return;
            }
        }

        serviceManager.checkDomainOwnership(domainName, {}, {
            success: function(e, ownershipIsProved) {
                disableButtons(false);
                domainOwnershipIsProved = ownershipIsProved;
                if (domainOwnershipIsProved) {
                    if ($('#mail_server_create_domain_wizard').is(':visible')) {
                        $('#mail_server_create_domain_wizard .cancel').trigger('click');
                        showStepPopup(3);
                    }
                } else {
                    popup.error(MailAdministrationResource.DomainWizardDomainNotVerified.format(domainName));
                    displayLoading('#mail_server_create_domain_wizard', false);
                }
            },
            error: function(e, error) {
                disableButtons(false);
                popup.error(administrationError.getErrorText("checkDomainOwnership", error));
                displayLoading('#mail_server_create_domain_wizard', false);
            }
        });
    }

    function addDomain() {
        window.LoadingBanner.hideLoading();

        displayLoading('#mail_server_create_domain_wizard', true);

        disableButtons(true);

        serviceManager.addMailDomain(domainName, requireDnsInfo.dns.id, {}, {
            success: function(e, domain) {
                disableButtons(false);
                displayLoading('#mail_server_create_domain_wizard', false);
                if ($('#mail_server_create_domain_wizard').is(':visible')) {
                    $('#mail_server_create_domain_wizard .cancel').trigger('click');
                }

                showDomainAddedBox(domain);

                TMMail.disableButton($("#mail-server-add-domain"), true);
                serviceManager.getMailServerFreeDns({}, {
                    success: function() {
                        TMMail.disableButton($("#mail-server-add-domain"), false);
                    },
                    error: function(ev, error) {
                        TMMail.disableButton($("#mail-server-add-domain"), false);
                        administrationError.showErrorToastr("getMailServerFreeDkim", error);
                    }
                });
            },
            error: function(e, error) {
                disableButtons(false);
                popup.error(administrationError.getErrorText("addMailDomain", error));
                displayLoading('#mail_server_create_domain_wizard', false);
            }
        });

        return false;
    }

    function displayLoading(selector, isVisible) {
        var loader = $(selector).find('.progressContainer .loader');
        
        if (loader) {
            if (isVisible) {
                $(selector).find('.progressContainer .toast-popup-container').remove();
                loader.show();
            } else {
                loader.hide();
            }
        }
    }

    function disableButtons(disable) {
        TMMail.disableButton($('#mail_server_create_domain_wizard .cancel'), disable);
        TMMail.disableButton($('#mail_server_create_domain_wizard .next'), disable);
        TMMail.disableButton($('#mail_server_create_domain_wizard .prev'), disable);
        TMMail.disableButton($('#commonPopup .cancelButton'), disable);
        popup.disableCancel(disable);
        TMMail.disableInput($('#mail_server_create_domain_wizard .web_domain'), disable);
        TMMail.disableButton($('#mail_server_domain_dns_settings .cancel'), disable);
        TMMail.disableButton($('#mail_server_domain_dns_settings .verify'), disable);
    }

    function showDomainAddedBox(domain) {
        var footer = "";
        //TODO: Uncommit when email send will be work from DNSChecker Service 
        //var footer = MailAdministrationResource.VerifiedDomainAddedPopupFooter.format('<span class="bold">' + Teamlab.profile.email + '</span>');

        var body;

        if (domain.dns.isVerified) {
            body = $($.tmpl("domainAddedTmpl", {
                errorBodyHeader: MailAdministrationResource.UnverifiedDomainAddedPopupHeader,
                errorBody: MailAdministrationResource.UnverifiedDomainAddedPopupBody.format('admin@' + domain.name)
            }));
        } else {
            body = $($.tmpl("domainAddedTmpl", {
                errorBodyHeader: MailAdministrationResource.VerifiedDomainAddedPopupHeader,
                errorBody: MailAdministrationResource.VerifiedDomainAddedPopupBody,
                errorBodyFooter: footer
            }));
        }

        body.find('.errorImg').removeClass('errorImg').addClass('domainAddedImg');

        popup.addBig(MailAdministrationResource.DomainAddedPopupHeader, body);
    }

    function showDnsSettings(domainId, dnsInfo) {
        var body = getDnsSettingsBody(domainId, dnsInfo);
        var domain = administrationManager.getDomain(domainId);
        popup.addBig(MailAdministrationResource.DomainDnsSettingsPopupHeader.replace('%1', domain.name), body);
    }

    function getDnsSettingsBody(domainId, dnsInfo) {
        var body = $($.tmpl("domainDnsSettingsTmpl", { require_dns_info: dnsInfo }));

        body.find(".buttons .verify").unbind('click').bind('click', function() {
            if ($(this).hasClass('disable')) {
                return false;
            }
            verifyDomainDns(domainId);
            return false;
        });

        body.find(".buttons .cancel").unbind('click').bind('click', function() {
            if ($(this).hasClass('disable')) {
                return false;
            }
            popup.hide();
            return false;
        });
        return body;
    }

    function verifyDomainDns(domainId) {
        var domain = administrationManager.getDomain(domainId);
        var domainEl = $('.domain_table_container[domain_id="' + domain.id + '"] .domain');
        if (domainEl.length == 1) {

            displayLoading("#mail_server_domain_dns_settings", true);
            disableButtons(true);

            serviceManager.getDomainDnsSettings(domainId, { domainId: domainId }, {
                success: function(e, dns) {
                    displayLoading("#mail_server_domain_dns_settings", false);
                    disableButtons(false);

                    var body = getDnsSettingsBody(domainId, { dns: dns });
                    $("#mail_server_domain_dns_settings").replaceWith(body);

                    domain.dns = dns;
                    if (dns.isVerified == true) {
                        domainEl.find('.verify_dns').hide();
                    }
                },
                error: function(e, error) {
                    disableButtons(false);
                    popup.error(administrationError.getErrorText("verifyDomainDns", error));
                    displayLoading("#mail_server_domain_dns_settings", false);
                }
            });
        }
    }

    function getCurrentDomainName() {
        return domainName;
    }

    return {
        show: show,
        showDnsSettings: showDnsSettings,
        getCurrentDomainName: getCurrentDomainName
    };

})(jQuery);