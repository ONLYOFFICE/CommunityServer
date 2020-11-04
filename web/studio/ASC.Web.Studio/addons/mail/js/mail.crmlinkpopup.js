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


window.crmLinkPopup = (function($) {
    var contactType = 1,
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

        if (linkedCount === 0) {
            hideLoader();
        } else {
            $('.buttons .unlink_all').prop('disabled', false).removeClass('disable');

            for (var i = 0; i < linkedCount; ++i) {
                selectedContactIds.push({ "Id": linkedEntities[i].id, "Type": linkedEntities[i].type });

                var newParams = { id: linkedEntities[i].id, entityType: linkedEntities[i].type };
                switch (linkedEntities[i].type) {
                    case contactType:
                        window.Teamlab.getCrmContact(newParams, linkedEntities[i].id,
                            {
                                success: onGetCrmEntity,
                                error: onGetCrmEntityError,
                                is_single: true
                            });
                        break;
                    case caseType:
                        window.Teamlab.getCrmCase(newParams, linkedEntities[i].id,
                            {
                                success: onGetCrmEntity,
                                error: onGetCrmEntityError,
                                is_single: true
                            });
                        break;
                    case opportunityType:
                        window.Teamlab.getCrmOpportunity(newParams, linkedEntities[i].id,
                            {
                                success: onGetCrmEntity,
                                error: onGetCrmEntityError,
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

        function addRow() {
            //Add unlink handler
            if (isExport) {
                linkedTableRow.find('.unlink_entity').unbind('click').bind('click', function() {
                    exportAndLinkUnlinkWorkflow($(this));
                });
            } else {
                linkedTableRow.find('.unlink_entity').unbind('click').bind('click', function() {
                    var data = exportAndLinkUnlinkWorkflow($(this));
                    if (!data.delete_from_new_status) {
                        needDeleteContactIds.push(data.data);
                    }
                });
            }
            addedRows = addedRows + 1;
            if (linkedCount === addedRows) {
                hideLoader();
            }

            if ($('.crm_popup .linked_table_parent:visible').length === 0) {
                $('.crm_popup .linked_table_parent').show();
            }

            $('.crm_popup .linked_table_parent .linked_contacts_table').append(linkedTableRow);
            if (isExport) {
                $('.buttons .link_btn').prop('disabled', false).removeClass('disable');
            }
        }

        if (!link) {
            addRow();
            return;
        }

        // Pre-load image
        $.ajax({
            url: link,
            context: document.body
        }).done(function(resp) {
            img.attr('src', resp);
        }).always(addRow);
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
        if ($('.crm_popup .linked_contacts_table .linked_entity_row').length === 0) {
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
            if (array[i].Id === value.Id && array[i].type === value.type) {
                return i;
            }
        }
        return -1;
    }

    function onGetCrmEntity(params, contact) {
        var contactInfo = {
            id: contact.id,
            entityType: params.entityType,
            displayName: contact.displayName || contact.title
        };

        if (contact.smallFotoUrl) {
            contactInfo.smallFotoUrl = contact.smallFotoUrl;
        }

        addLinkedTableRow(contactInfo);
    }

    function onGetCrmEntityError(params, response) {
        if (response[0] === 'Not found' //TODO: Simplify
            && response[1] === 'Not found'
            && response[2] === 'Not found') {
            var messageId = getMessageId();
            var data = [{
                "Id": params.id,
                "Type": params.entityType
            }];
            serviceManager.unmarkChainAsCrmLinked(messageId, data);
        }
        addedRows = addedRows + 1;
        if (linkedCount === addedRows) {
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
        return +$('.crm_popup #entity-type').val();
    }

    function getAlreadyLinkedContacts() {
        var ids = {};
        $('.crm_popup .linked_entity_row').each(function(i, val) {
            ids[$(val).attr('entity_type').concat(':').concat($(val).data('entity_id'))] = true;
        });
        return ids;
    }

    function addAutocomplete(html, isExport) {
        html.find('#link_search_panel').CrmSelector("init", {
            isInPopup: true,
            getEntityType: getSelectedEntityType,
            onSelectItem: function (item) {
                addLinkedTableRow(item, isExport);
                needAddContactIds.push({ "Id": item.id, "Type": item.entityType });
            },
            isExists: function(entityType, id) {
                var search = "{0}:{1}".format(entityType, id);
                var alreadyLinkedContacts = getAlreadyLinkedContacts();
                return alreadyLinkedContacts[search];
            }
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
            error: function (params, error) {
                console.error(error);
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
                    error: function (params, error) {
                        console.error(error);
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
                    error: function (params, error) {
                        console.error(error);
                        window.LoadingBanner.hideLoading();
                    }
                }, ASC.Resources.Master.Resource.LoadingProcessing);

                if (needAddContactIds.length === 0 && selectedContactIds.length === needDeleteContactIds.length) {
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

        if (!hasLinked) {
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