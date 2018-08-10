/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


window.crmLinkPopup = (function($) {
    var contactType = 1;
        caseType = 2,
        opportunityType = 3,
        selectedContactIds = [],
        needAddContactIds = [],
        needDeleteContactIds = [],
        linkedCount = 0,
        addedRows = 0,
        exportMessageId = -1;

    function getMessageId() {
        var messageId = mailBox.currentMessageId;
        if (TMMail.pageIs('conversation')) {
            messageId = messagePage.getActualConversationLastMessageId();
        }
        if (exportMessageId != undefined && exportMessageId > 0) {
            messageId = exportMessageId;
        }
        return messageId;
    }

    function onGetLinkedCrmEntitiesInfo(params, linkedEntities) {
        linkedCount = linkedEntities.length;
        addedRows = 0;
        $('.buttons .link_btn').prop('disabled', false).removeClass('disable');

        if (linkedCount == 0) {
            hideLoader();
        } else {
            $('.buttons .unlink_all').prop('disabled', false).removeClass('disable');

            for (var i = 0; i < linkedCount; ++i) {
                selectedContactIds.push({ "Id": linkedEntities[i].id, "Type": linkedEntities[i].type });
                switch (linkedEntities[i].type) {
                    case contactType:
                        window.Teamlab.getCrmContact({ id: linkedEntities[i].id }, linkedEntities[i].id,
                            {
                                success: onGetCrmContact,
                                error: onGetCrmContactError,
                                is_single: true
                            });
                        break;
                    case caseType:
                        window.Teamlab.getCrmCase({ id: linkedEntities[i].id }, linkedEntities[i].id,
                            {
                                success: onGetCrmCase,
                                error: onGetCrmCaseError,
                                is_single: true
                            });
                        break;
                    case opportunityType:
                        window.Teamlab.getCrmOpportunity({ id: linkedEntities[i].id }, linkedEntities[i].id,
                            {
                                success: onGetCrmOpportunity,
                                error: onGetCrmOpportunityError,
                                is_single: true
                            });
                        break;
                }
            }
        }
    }

    function addLinkedTableRow(linkedEntityInfo, isExport) {
        var linkedTableRow = $.tmpl('crmContactItemTmpl', linkedEntityInfo);
        //Download Img url for crm entity
        var img = linkedTableRow.find('.linked_entity_row_avatar_column img');
        var link = img.attr('src');
        $.ajax({
            url: link,
            context: document.body
        }).done(function(resp) {
            img.attr('src', resp);
        }).always(function() {
            //Add unlink handler
            if (isExport) {
                linkedTableRow.find('.unlink_entity').unbind('click').bind('click', function() {
                    exportAndLinkUnlinkWorkflow($(this));
                });
            } else {
                linkedTableRow.find('.unlink_entity').unbind('click').bind('click', function() {
                    var data = exportAndLinkUnlinkWorkflow($(this));
                    if (data.delete_from_new_status == false || data.delete_from_new_status == undefined) {
                        needDeleteContactIds.push(data.data);
                    }
                });
            }
            addedRows = addedRows + 1;
            if (linkedCount == addedRows) {
                hideLoader();
            }

            if ($('.crm_popup .linked_table_parent:visible').length == 0) {
                $('.crm_popup .linked_table_parent').show();
            }

            $('.crm_popup .linked_table_parent .linked_contacts_table').append(linkedTableRow);
            if (isExport) {
                $('.buttons .link_btn').prop('disabled', false).removeClass('disable');
            }
        });
    }

    function exportAndLinkUnlinkWorkflow(element) {
        var row = element.closest('tr.linked_entity_row');
        var data = {
            "Id": row.data('entity_id'),
            "Type": row.attr('entity_type')
        };
        var status = deleteElementFromNewAdded(data);
        deleteRowFromLinkedTable(data);
        return { data: data, delete_from_new_status: status };
    }

    function deleteRowFromLinkedTable(data) {
        $('.crm_popup .linked_contacts_table .linked_entity_row[data-entity_id=' + data.Id + '][entity_type=' + data.Type + ']').remove();
        if ($('.crm_popup .linked_contacts_table .linked_entity_row').length == 0) {
            $('.crm_popup .linked_table_parent').hide();
        }
    }

    function deleteElementFromNewAdded(data) {
        var index = findEntityIn(needAddContactIds, data);
        var status = false;
        if (index > -1) {
            needAddContactIds.splice(index, 1);
            status = true;
        }

        return status;
    }

    function findEntityIn(array, value) {
        for (var i = 0; i < array.length; i += 1) {
            if (array[i].Id == value.Id && array[i].type == value.type) {
                return i;
            }
        }
        return -1;
    }

    function onGetCrmContact(params, contact) {
        var contactInfo = {};
        contactInfo.entity_id = contact.id;
        contactInfo.entity_type = contactType;
        contactInfo.title = contact.displayName;
        contactInfo.avatarLink = contact.smallFotoUrl;
        addLinkedTableRow(contactInfo);
    }

    function onGetCrmContactError(params, response) {
        if (response[0] == 'Not found'
            && response[1] == 'Not found'
            && response[2] == 'Not found') {
            var messageId = getMessageId();
            var data = [{
                "Id": params.id,
                "Type": contactType
            }];
            serviceManager.unmarkChainAsCrmLinked(messageId, data);
        }
        addedRows = addedRows + 1;
        if (linkedCount == addedRows) {
            hideLoader();
        }
    }

    function onGetCrmOpportunity(params, opportunity) {
        var contactInfo = {};
        contactInfo.entity_id = opportunity.id;
        contactInfo.entity_type = opportunityType;
        contactInfo.title = opportunity.title;
        addLinkedTableRow(contactInfo);
    }

    function onGetCrmOpportunityError(params, response) {
        if (response[0] == 'Not found'
            && response[1] == 'Not found'
            && response[2] == 'Not found') {
            var messageId = getMessageId();
            var data = [{
                "Id": params.id,
                "Type": opportunityType
            }];
            serviceManager.unmarkChainAsCrmLinked(messageId, data);
        }

        addedRows = addedRows + 1;
        if (linkedCount == addedRows) {
            hideLoader();
        }
    }

    function onGetCrmCase(params, caseInfo) {
        var contactInfo = {};
        contactInfo.entity_id = caseInfo.id;
        contactInfo.entity_type = caseType;
        contactInfo.title = caseInfo.title;
        addLinkedTableRow(contactInfo);
    }

    function onGetCrmCaseError(params, response) {
        if (response[0] == 'Not found'
            && response[1] == 'Not found'
            && response[2] == 'Not found') {
            var messageId = getMessageId();
            var data = [{
                "Id": params.id,
                "Type": caseType
            }];
            serviceManager.unmarkChainAsCrmLinked(messageId, data);
        }

        addedRows = addedRows + 1;
        if (linkedCount == addedRows) {
            hideLoader();
        }
    }

    function hideLoader(html) {
        if (html == undefined) {
            $('.crm_popup .loader').hide();
        } else {
            html.find('.loader').hide();
        }
    }

    function getSelectedEntityType() {
        return $('.crm_popup #entity-type').val();
    }

    function getAlreadyLinkedContacts() {
        var ids = {};
        $('.crm_popup .linked_entity_row').each(function(i, val) {
            ids[$(val).attr('entity_type').concat(':').concat($(val).data('entity_id'))] = true;
        });
        return ids;
    }

    function addAutocomplete(html, isExport) {
        var input = '#link_search_panel';
        var searchIcon = '.crm_search_contact_icon';
        var loadingIcon = '.crm_search_loading_icon';

        var onGetCrmContactsByPrefix = function(params, contacts) {

            var cache = $(input).data("cache");
            cache.contactCache[params.searchText] = contacts;
            $(input).data("cache", cache);

            var alreadyLinkedContacts = getAlreadyLinkedContacts();
            var names = contacts.map(function(val) {
                if (!alreadyLinkedContacts["1:".concat(val.id)]) {
                    return {
                        label: val.displayName,
                        value: val.id,
                        entity_type: contactType,
                        entity_id: val.id,
                        title: val.displayName,
                        avatarLink: val.smallFotoUrl
                    };
                }
            }).filter(function(val) { return val != undefined; });

            params.responseFunction(names);
            $(searchIcon).show();
            $(loadingIcon).hide();
        };

        var onGetCrmCaseByPrefix = function(params, cases) {

            var cache = $(input).data("cache");
            cache.caseCache[params.searchText] = cases;
            $(input).data("cache", cache);

            var alreadyLinkedContacts = getAlreadyLinkedContacts();
            var names = cases.map(function(val) {
                if (!alreadyLinkedContacts["2:".concat(val.id)]) {
                    return {
                        label: val.title,
                        value: val.id,
                        entity_type: caseType,
                        entity_id: val.id,
                        title: val.title,
                    };
                }
            }).filter(function(val) { return val != undefined; });

            params.responseFunction(names);
            $(searchIcon).show();
            $(loadingIcon).hide();
        };

        var onGetCrmOpportunityByPrefix = function(params, opportunities) {

            var cache = $(input).data("cache");
            cache.opportunityCache[params.searchText] = opportunities;
            $(input).data("cache", cache);

            var alreadyLinkedContacts = getAlreadyLinkedContacts();
            var names = opportunities.map(function (val) {
                if (!alreadyLinkedContacts["3:".concat(val.id)]) {
                    return {
                        label: val.title,
                        value: val.id,
                        entity_type: opportunityType,
                        entity_id: val.id,
                        title: val.title,
                    };
                }
            }).filter(function(val) { return val != undefined; });

            params.responseFunction(names);
            $(searchIcon).show();
            $(loadingIcon).hide();
        };

        var fromCache = function(term, cache, params, callback) {
            if (term in cache) {
                callback(params, cache[term]);
                return true;
            } else {
                for (var cacheterm in cache) {
                    if (cache[cacheterm].length == 0 && term.indexOf(cacheterm) == 0) {
                        callback(params, []);
                        return true;
                    }
                }
            }

            return false;
        };

        html.find(input).autocomplete({
            minLength: 1,
            delay: 500,
            autoFocus: true,
            appendTo: html.find(input).parent(),
            select: function(event, ui) {
                addLinkedTableRow(ui.item, isExport);
                needAddContactIds.push({ "Id": ui.item.entity_id, "Type": getSelectedEntityType() });
                $(input).val('');
                return false;
            },
            create: function() {
                $(window).resize(function() {
                    if ($(input).data("ui-autocomplete") != undefined) {
                        $(input).data("ui-autocomplete").close();
                    }
                });
            },
            focus: function() {
                return false;
            },
            search: function() {
                return true;
            },
            source: function(request, response) {
                var term = request.term;
                
                $(searchIcon).hide();
                $(loadingIcon).show();

                var params = { searchText: term, responseFunction: response, input: input };
                var cache = $(input).data("cache");

                if (getSelectedEntityType() == contactType) {
                    if (fromCache(term, cache.contactCache, params, onGetCrmContactsByPrefix))
                        return;

                    window.Teamlab.getCrmContactsByPrefix(params, {
                        filter: { prefix: term, searchType: -1 },
                        success: onGetCrmContactsByPrefix
                    });
                }
                if (getSelectedEntityType() == caseType) {
                    if (fromCache(term, cache.caseCache, params, onGetCrmCaseByPrefix))
                        return;

                    window.Teamlab.getCrmCasesByPrefix(params, {
                        filter: { prefix: term, searchType: -1 },
                        success: onGetCrmCaseByPrefix
                    });
                }
                if (getSelectedEntityType() == opportunityType) {
                    if (fromCache(term, cache.opportunityCache, params, onGetCrmOpportunityByPrefix))
                        return;

                    window.Teamlab.getCrmOpportunitiesByPrefix(params, {
                        filter: { prefix: term },
                        success: onGetCrmOpportunityByPrefix
                    });
                }
            }
        });

        html.find(input).data("cache", {
            contactCache: {},
            caseCache: {},
            opportunityCache: {},
        });
    }

    function onLinkChainToCrm() {
        window.LoadingBanner.hideLoading();
        window.toastr.success(window.MailScriptResource.LinkConversationText);
    }

    function onUnmarkChainAsCrmLinked() {
        window.LoadingBanner.hideLoading();
    }

    function onExportMessageToCrm() {
        window.LoadingBanner.hideLoading();
        window.toastr.success(window.MailScriptResource.ExportMessageText);
    }

    function showCrmLinkConversationPopup(hasLinked) {
        var html = $.tmpl('crmLinkPopupTmpl');

        selectedContactIds = [];
        needAddContactIds = [];
        needDeleteContactIds = [];
        exportMessageId = -1;

        addAutocomplete(html, false);

        serviceManager.getLinkedCrmEntitiesInfo(getMessageId(), {}, {
            success: onGetLinkedCrmEntitiesInfo,
            error: function(params, error) {
                window.LoadingBanner.hideLoading();
            }
        });

        html.find('.buttons .link_btn').unbind('click').bind('click', function() {
            var messageId;
            if (needAddContactIds.length > 0) {
                window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "link_chain_with_crm");
                messageId = getMessageId();

                serviceManager.linkChainToCrm(messageId, needAddContactIds, {},
                {
                    success: onLinkChainToCrm,
                    error: function(params, error) {
                        window.LoadingBanner.hideLoading();
                        window.toastr.error(window.MailScriptResource.LinkFailurePopupText);
                    }
                }, ASC.Resources.Master.Resource.LoadingProcessing);

                selectedContactIds = [];
                needAddContactIds = [];
                $('.header-crm-link').show();
                messagePage.setHasLinked(true);
                window.LoadingBanner.displayMailLoading();
            }
            if (needDeleteContactIds.length > 0) {
                messageId = getMessageId();

                serviceManager.unmarkChainAsCrmLinked(messageId, needDeleteContactIds, {},
                {
                    success: onUnmarkChainAsCrmLinked,
                    error: function(params, error) {
                        window.LoadingBanner.hideLoading();
                    }
                }, ASC.Resources.Master.Resource.LoadingProcessing);

                if (needAddContactIds.length == 0 && selectedContactIds.length == needDeleteContactIds.length) {
                    $('.header-crm-link').hide();
                    messagePage.setHasLinked(false);
                }
                needDeleteContactIds = [];
                window.LoadingBanner.displayMailLoading();
            }

            popup.hide();
            return false;
        });

        html.find('.buttons .link_btn').prop('disabled', true).addClass('disable');
        html.find('.buttons .unlink_all').prop('disabled', true).addClass('disable');

        html.find('.buttons .unlink_all').unbind('click').bind('click', function() {
            var htmlTmpl = $.tmpl('crmUnlinkAllPopupTmpl');
            htmlTmpl.find('.buttons .unlink').bind('click', function() {
                if (selectedContactIds.length > 0) {
                    window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "unlink_chain_with_crm");

                    var messageId = getMessageId();
                    serviceManager.unmarkChainAsCrmLinked(messageId, selectedContactIds, {}, ASC.Resources.Master.Resource.LoadingProcessing);
                    $('.header-crm-link').hide();
                    messagePage.setHasLinked(false);
                }
                popup.hide();
                return false;
            });

            popup.addBig(window.MailScriptResource.UnlinkFromCRMPopupHeader, htmlTmpl);
            popup.hide();
        });

        if (hasLinked == undefined || hasLinked == false) {
            hideLoader(html);
        }

        $('.crm_popup .linked_table_parent').hide().find('.linked_contacts_table .linked_entity_row').remove();

        window.popup.init();
        window.popup.addBig(window.MailScriptResource.LinkConversationPopupHeader, html);
    }

    function showCrmExportMessagePopup(messageId) {
        var html = $.tmpl('crmExportPopupTmpl');
        //Export popup initialization
        selectedContactIds = [];
        needAddContactIds = [];
        needDeleteContactIds = [];

        if (messageId != undefined) {
            exportMessageId = messageId;
        }

        addAutocomplete(html, true);

        html.find('.buttons .link_btn').unbind('click').bind('click', function () {
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "export_mail_to_crm");
            serviceManager.exportMessageToCrm(getMessageId(), needAddContactIds, {},
            {
                success: onExportMessageToCrm,
                error: function () {
                    window.LoadingBanner.hideLoading();
                    window.toastr.error(window.MailScriptResource.ExportFailurePopupText);
                }
            }, ASC.Resources.Master.Resource.LoadingProcessing);
            needAddContactIds = [];
            exportMessageId = -1;
            window.LoadingBanner.displayMailLoading();
            popup.hide();
            return false;
        });

        html.find('.buttons .link_btn').prop('disabled', true).addClass('disable');

        window.popup.init();
        window.popup.addBig(window.MailScriptResource.ExportConversationPopupHeader, html);
    }

    return {
        showCrmLinkConversationPopup: showCrmLinkConversationPopup,
        showCrmExportMessagePopup: showCrmExportMessagePopup

    };
})(jQuery)