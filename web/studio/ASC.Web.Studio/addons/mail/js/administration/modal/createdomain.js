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

window.createDomainModal = (function ($) {
    var require_dns_info,
        first_step = 1,
        last_step = 5,
        domain_name = '',
        domain_ownership_is_proved = false;

    function show(requireDnsInfo) {
        require_dns_info = requireDnsInfo;
        showStepPopup(first_step);
        domain_name = "";
        domain_ownership_is_proved = false;
    }

    function showStepPopup(step) {
        var html = $.tmpl('createDomainWizardTmpl', { step: step, require_dns_info: require_dns_info, faq_url: TMMail.getFaqLink(), domain_name: domain_name });

        $(html).find('.next').unbind('click').bind('click', function () {
            if ($(this).hasClass('disable'))
                return false;

            if (step == first_step) {
                checkDomainExistance();
            } else if (step == first_step + 1) {
                checkDomainOwnership();
            }
            else if (step == last_step)
                addDomain();
            else {
                var new_step = step == last_step ? last_step : step + 1;
                showStepPopup(new_step);
            }
            return false;
        });

        if (step != first_step) {
            $(html).find('.prev').unbind('click').bind('click', function() {
                if ($(this).hasClass('disable'))
                    return false;

                var new_step = step == first_step ? first_step : step - 1;
                showStepPopup(new_step);

                if (new_step == first_step && domain_name != '') {
                    $('#wizard_add_domain').find('.web_domain').val(domain_name);
                }

                return false;
            });
        }

        $(html).find('.cancel').unbind('click').bind('click', function () {
            if ($(this).hasClass('disable'))
                return false;
            popup.hide();
            return false;
        });

        popup.hide();
        popup.addPopup(window.MailAdministrationResource.DomainWizardPopupHeader, html, step == first_step ? '392px' : '470px');

        if (step == first_step) {
            var domain_input = $('#mail_server_create_domain_wizard .web_domain');
            domain_input.focus();
            domain_input.unbind('textchange').bind('textchange', function () {
                TMMail.setRequiredError('wizard_add_domain', false);
            });
        }

        $(document).unbind('keyup').bind('keyup', function (e) {
            if (e.which == 13) {
                if ($('#mail_server_create_domain_wizard').is(':visible')) {
                    $('#mail_server_create_domain_wizard .next').trigger('click');
                }
                else if ($('#domain_added_container').is(':visible')) {
                    $('#domain_added_container .cancel').trigger('click');
                }
            }
            else if (e.which == 39 && step != first_step) {
                if ($('#mail_server_create_domain_wizard').is(':visible')) {
                    $('#mail_server_create_domain_wizard .next').trigger('click');
                }
            }
            else if (e.which == 37 && step != first_step) {
                if ($('#mail_server_create_domain_wizard').is(':visible')) {
                    $('#mail_server_create_domain_wizard .prev').trigger('click');
                }
            }
        });
    }

    function checkDomainExistance() {
        window.LoadingBanner.hideLoading();

        var new_domain_name = $('#wizard_add_domain').find('.web_domain').val();
        if (new_domain_name.length === 0) {
            TMMail.setRequiredHint('wizard_add_domain', window.MailScriptResource.ErrorEmptyField);
            TMMail.setRequiredError('wizard_add_domain', true);
            return;
        }

        if (!TMMail.reDomainStrict.test(new_domain_name)) {
            TMMail.setRequiredHint('wizard_add_domain', window.MailApiErrorsResource.ErrorInvalidDomain);
            TMMail.setRequiredError('wizard_add_domain', true);
            return;
        }

        domain_name = new_domain_name;
        domain_ownership_is_proved = false;

        TMMail.setRequiredError('wizard_add_domain', false);

        displayLoading('#mail_server_create_domain_wizard', true);

        disableButtons(true);
        
        serviceManager.isDomainExists(domain_name, {}, {
            success: function(e, isExists) {
                disableButtons(false);
                if (!isExists) {
                    if ($('#mail_server_create_domain_wizard').is(':visible')) {
                        $('#mail_server_create_domain_wizard .cancel').trigger('click');
                        showStepPopup(2);
                    }
                } else {
                    administrationError.showErrorToastr("checkDomainExistance", [MailAdministrationResource.DomainWizardDomainAlreadyExists.format(domain_name)]);
                    displayLoading('#mail_server_create_domain_wizard', false);
                    domain_ownership_is_proved = false;
                    TMMail.setRequiredHint('wizard_add_domain', '');
                    TMMail.setRequiredError('wizard_add_domain', true);
                }
            },
            error: function (e, error) {
                disableButtons(false);
                administrationError.showErrorToastr("checkDomainExistance", error);
                TMMail.setRequiredHint('wizard_add_domain', '');
                TMMail.setRequiredError('wizard_add_domain', true);
                displayLoading('#mail_server_create_domain_wizard', false);
            }
        }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function checkDomainOwnership() {
        window.LoadingBanner.hideLoading();

        displayLoading('#mail_server_create_domain_wizard', true);

        disableButtons(true);

        if (domain_ownership_is_proved) {
            if ($('#mail_server_create_domain_wizard').is(':visible')) {
                $('#mail_server_create_domain_wizard .cancel').trigger('click');
                showStepPopup(3);
                return;
            }
        }

        serviceManager.checkDomainOwnership(domain_name, {}, {
            success: function (e, ownershipIsProved) {
                disableButtons(false);
                domain_ownership_is_proved = ownershipIsProved;
                if (domain_ownership_is_proved) {
                    if ($('#mail_server_create_domain_wizard').is(':visible')) {
                        $('#mail_server_create_domain_wizard .cancel').trigger('click');
                        showStepPopup(3);
                    }
                } else {
                    administrationError.showErrorToastr("checkDomainOwnership", [MailAdministrationResource.DomainWizardDomainNotVerified.format(domain_name)]);
                    displayLoading('#mail_server_create_domain_wizard', false);
                }
            },
            error: function (e, error) {
                disableButtons(false);
                administrationError.showErrorToastr("checkDomainOwnership", error);
                displayLoading('#mail_server_create_domain_wizard', false);
            }
        }, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    function addDomain() {
        window.LoadingBanner.hideLoading();

        displayLoading('#mail_server_create_domain_wizard', true);

        disableButtons(true);

        serviceManager.addMailDomain(domain_name, require_dns_info.dns.id, {}, {
            success: function (e, domain) {
                disableButtons(false);
                if ($('#mail_server_create_domain_wizard').is(':visible')) {
                    $('#mail_server_create_domain_wizard .cancel').trigger('click');
                }

                showDomainAddedBox(domain);

                TMMail.disableButton($("#mail-server-add-domain"), true);
                serviceManager.getMailServerFreeDns({}, {
                    success: function () {
                        TMMail.disableButton($("#mail-server-add-domain"), false);
                    },
                    error: function (ev, error) {
                        TMMail.disableButton($("#mail-server-add-domain"), false);
                        administrationError.showErrorToastr("getMailServerFreeDkim", error);
                    }
                }, ASC.Resources.Master.Resource.LoadingProcessing);
            },
            error: function (e, error) {
                disableButtons(false);
                administrationError.showErrorToastr("addMailDomain", error);
                displayLoading('#mail_server_create_domain_wizard', false);
            }
        });

        return false;
    }

    function displayLoading(selector, isVisible) {
        var loader = $(selector).find('.progressContainer .loader');
        if (loader) {
            if (isVisible) 
                loader.show();
            else
                loader.hide();
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

        body.find(".buttons .verify").unbind('click').bind('click', function () {
            if ($(this).hasClass('disable'))
                return false;
            verifyDomainDns(domainId);
            return false;
        });

        body.find(".buttons .cancel").unbind('click').bind('click', function () {
            if ($(this).hasClass('disable'))
                return false;
            popup.hide();
            return false;
        });
        return body;
    }

    function verifyDomainDns(domainId) {
        var domain = administrationManager.getDomain(domainId);
        var domain_el = $('.domain_table_container[domain_id="' + domain.id + '"] .domain');
        if (domain_el.length == 1) {

            displayLoading("#mail_server_domain_dns_settings", true);
            disableButtons(true);

            serviceManager.getDomainDnsSettings(domainId, { domainId: domainId }, {
                success: function (e, dns) {
                    displayLoading("#mail_server_domain_dns_settings", false);
                    disableButtons(false);

                    var body = getDnsSettingsBody(domainId,{ dns: dns });
                    $("#mail_server_domain_dns_settings").replaceWith(body);

                    domain.dns = dns;
                    if (dns.isVerified == true)
                        domain_el.find('.verify_dns').hide();
                },
                error: function (e, error) {
                    displayLoading("#mail_server_domain_dns_settings", false);
                    disableButtons(false);
                    administrationError.showErrorToastr("verifyDomainDns", error);
                }
            }, ASC.Resources.Master.Resource.LoadingProcessing);
        }
    }

    return {
        show: show,
        showDnsSettings: showDnsSettings
    };

})(jQuery);