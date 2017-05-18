/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


window.createDomainModal = (function($) {
    var requireDnsInfo;
    var firstStep = 1;
    var lastStep = 5;
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
            require_dns_info: requireDnsInfo,
            domain_name: domainName
        });

        $(html).find('.next').unbind('click').bind('click', function() {
            if ($(this).hasClass('disable')) {
                return false;
            }

            if (step == firstStep) {
                checkDomainExistance();
            } else if (step == firstStep + 1) {
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

        PopupKeyUpActionProvider.EnterAction = "jq('#mail_server_create_domain_wizard:visible .next').trigger('click');";
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