/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/


/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/

window.CrmLinkPopup = ( function($) {
    // CONSTANTS
    var CONTACT = 1;
    var CASE = 2;
    var OPPORTUNITY = 3;
    //variables
    var selected_contact_ids = [];
    var new_added_contact_ids = [];
    var on_delete_contact_ids = [];
    var linked_count = 0;
    var added_rows = 0;
    var export_message_id = -1;


    function init(){
        serviceManager.bind(window.Teamlab.events.getLinkedCrmEntitiesInfo, onGetLinkedCrmEntitiesInfo);
        serviceManager.bind(window.Teamlab.events.exportMessageToCrm, onExportMessageToCrm);
        serviceManager.bind(window.Teamlab.events.linkChainToCrm, onLinkChainToCrm);
        serviceManager.bind(window.Teamlab.events.unmarkChainAsCrmLinked, onUnmarkChainAsCrmLinked);
    }

    function getCrmLinkControl(hasLinked) {
        var html = $.tmpl('crmLinkPopupTmpl');

        selected_contact_ids = [];
        new_added_contact_ids = [];
        on_delete_contact_ids = [];
        export_message_id = -1;

        _addAutocomplete(html, false);

        var message_id = getMessageId();
        serviceManager.getLinkedCrmEntitiesInfo(message_id);

        html.find('.buttons .link_btn').unbind('click').bind('click', function(){
            if(new_added_contact_ids.length > 0)
            {
                window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "link_chain_with_crm");

                var message_id = getMessageId();
                serviceManager.linkChainToCrm(message_id, new_added_contact_ids, {}, ASC.Resources.Master.Resource.LoadingProcessing);

                selected_contact_ids = [];
                new_added_contact_ids = [];
                $('.header-crm-link').show();
                messagePage.setHasLinked(true);
                window.LoadingBanner.displayLoading(true, true);
            }
            if(on_delete_contact_ids.length > 0)
            {
                var message_id = getMessageId();
                serviceManager.unmarkChainAsCrmLinked(message_id, on_delete_contact_ids, {},  ASC.Resources.Master.Resource.LoadingProcessing);
                if(new_added_contact_ids.length == 0 && selected_contact_ids.length == on_delete_contact_ids.length){
                    $('.header-crm-link').hide();
                    messagePage.setHasLinked(false);
                }
                on_delete_contact_ids = [];
                window.LoadingBanner.displayLoading(true, true);
            }

            popup.hide();
            return false;
        });

        html.find('.buttons .link_btn').prop('disabled', true).addClass('disable');
        html.find('.buttons .unlink_all').prop('disabled', true).addClass('disable');

        html.find('.buttons .unlink_all').unbind('click').bind('click', function () {
            var html = $.tmpl('crmUnlinkAllPopupTmpl');

            html.find('.buttons .unlink').bind('click', function () {
                if(selected_contact_ids.length > 0)
                {
                    window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "unlink_chain_with_crm");

                    var message_id = getMessageId();
                    serviceManager.unmarkChainAsCrmLinked(message_id, selected_contact_ids, {}, ASC.Resources.Master.Resource.LoadingProcessing);
                    $('.header-crm-link').hide();
                    messagePage.setHasLinked(false);
                }
                popup.hide();
                return false;
            });

            popup.addBig(window.MailScriptResource.UnlinkFromCRMPopupHeader, html);
            popup.hide();
        });

        if(hasLinked == undefined || hasLinked == false){
            hideLoader(html);
        }

        $('.crm_popup .linked_table_parent').hide().find('.linked_contacts_table .linked_entity_row').remove();

        return html;
    }


    function getCrmExportControl(message_id) {
        var html = $.tmpl('crmExportPopupTmpl');
        //Export popup initialization
        selected_contact_ids = [];
        new_added_contact_ids = [];
        on_delete_contact_ids = [];

        if(message_id != undefined){
            export_message_id = message_id;
        }

        _addAutocomplete(html, true);

        html.find('.buttons .link_btn').unbind('click').bind('click', function () {
            window.ASC.Mail.ga_track(ga_Categories.message, ga_Actions.buttonClick, "export_mail_to_crm");
            var message_id = getMessageId();
            serviceManager.exportMessageToCrm(message_id, new_added_contact_ids, {}, ASC.Resources.Master.Resource.LoadingProcessing);
            new_added_contact_ids = [];
            export_message_id = -1;
            window.LoadingBanner.displayLoading(true, true);
            popup.hide();
            return false;
        });

        html.find('.buttons .link_btn').prop('disabled', true).addClass('disable');
        return html;
    }

    function getMessageId() {
        var message_id = mailBox.currentMessageId;
        if (TMMail.pageIs('conversation'))
            message_id = messagePage.getActualConversationLastMessageId();
        if(export_message_id != undefined && export_message_id > 0)
            message_id = export_message_id;
        return message_id;
    }

    function onGetLinkedCrmEntitiesInfo(params, linked_entities){
        linked_count = linked_entities.length;
        if(linked_count == 0)
            hideLoader();
        added_rows = 0;
        $('.buttons .link_btn').prop('disabled', false).removeClass('disable');
        $('.buttons .unlink_all').prop('disabled', false).removeClass('disable');
        for(var i = 0; i < linked_count; ++i){
            selected_contact_ids.push({"Id": linked_entities[i].id, "Type": linked_entities[i].type});
            switch(linked_entities[i].type){
                case CONTACT:
                    window.Teamlab.getCrmContact({id: linked_entities[i].id}, linked_entities[i].id,
                     {success: onGetCrmContact,
                        error: onGetCrmContactError,
                        is_single: true});
                break;
                case CASE:
                    window.Teamlab.getCrmCase({id: linked_entities[i].id}, linked_entities[i].id,
                     {success: onGetCrmCase,
                        error: onGetCrmCaseError,
                        is_single: true});
                break;
                case OPPORTUNITY:
                    window.Teamlab.getCrmOpportunity({id: linked_entities[i].id}, linked_entities[i].id,
                     {success: onGetCrmOpportunity,
                        error: onGetCrmOpportunityError,
                        is_single: true});
                break;
            }
        }
    }

    function addLinkedTableRow(linked_entity_info, is_export){
        var linked_table_row = $.tmpl('crmContactItemTmpl', linked_entity_info);
        //Download Img url for crm entity
        var img = linked_table_row.find('.linked_entity_row_avatar_column img');
        var link = img.attr('src');
        $.ajax({
          url: link,
          context: document.body
        }).done(function(resp) {
            img.attr('src', resp);
        }).always(function(){
            //Add unlink handler
            if(is_export){
                linked_table_row.find('.unlink_entity').unbind('click').bind('click', function(){
                    exportAndLinkUnlinkWorkflow($(this));
                });
            } else{
                linked_table_row.find('.unlink_entity').unbind('click').bind('click', function(){
                    var data = exportAndLinkUnlinkWorkflow($(this));
                    if(data.delete_from_new_status == false || data.delete_from_new_status == undefined){
                        on_delete_contact_ids.push(data.data);
                    }
                });
            }
            added_rows = added_rows + 1;
            if(linked_count == added_rows)
                hideLoader();
            
            if ($('.crm_popup .linked_table_parent:visible').length == 0)
                $('.crm_popup .linked_table_parent').show();

            $('.crm_popup .linked_table_parent .linked_contacts_table').append(linked_table_row);
            if(is_export)
                $('.buttons .link_btn').prop('disabled', false).removeClass('disable');
        });
    }

    function exportAndLinkUnlinkWorkflow(element){
        var row = element.closest('tr.linked_entity_row');
        var data = {"Id": row.data('entity_id'),
                    "Type": row.attr('entity_type')};
        var status = deleteElementFromNewAdded(data);
        deleteRowFromLinkedTable(data);
        return {data:data, delete_from_new_status: status};
    }

    function deleteRowFromLinkedTable(data) {
        $('.crm_popup .linked_contacts_table .linked_entity_row[data-entity_id=' + data.Id + '][entity_type=' + data.Type + ']').remove();
        if ($('.crm_popup .linked_contacts_table .linked_entity_row').length == 0)
            $('.crm_popup .linked_table_parent').hide();
    }

    function deleteElementFromNewAdded(data){
        var index = findEntityIn(new_added_contact_ids, data);
        var status = false;
        if(index > -1){
            new_added_contact_ids.splice(index, 1);
            status = true;
        }

        if(new_added_contact_ids.length == 0)
            $('.buttons .link_btn').prop('disabled', true).addClass('disable');

        return status;
    }

    function findEntityIn(array, value) {
        for(var i = 0; i < array.length; i += 1) {
            if(array[i].Id == value.Id && array[i].type == value.type) {
                return i;
            }
        }
        return -1;
    }

    function onGetCrmContact(params, contact){
        var contact_info = {};
        contact_info.entity_id = contact.id;
        contact_info.entity_type = CONTACT;
        contact_info.title = contact.displayName;
        contact_info.avatarLink = contact.smallFotoUrl;
        addLinkedTableRow(contact_info);
    }

    function onGetCrmContactError(params, response){
        if(response[0] == 'Not found' 
            && response[1] == 'Not found'
            && response[2] == 'Not found'){
            var message_id = getMessageId();
            var data = [{"Id": params.id,
                         "Type": CONTACT}];
            serviceManager.unmarkChainAsCrmLinked(message_id, data);
        }
        added_rows = added_rows + 1;
        if(linked_count == added_rows)
            hideLoader();
    }

    function onGetCrmOpportunity(params, opportunity){
        var contact_info = {};
        contact_info.entity_id = opportunity.id;
        contact_info.entity_type = OPPORTUNITY;
        contact_info.title = opportunity.title;
        addLinkedTableRow(contact_info);
    }

    function onGetCrmOpportunityError(params, response){
        if(response[0] == 'Not found' 
            && response[1] == 'Not found'
            && response[2] == 'Not found'){
            var message_id = getMessageId();
            var data = [{"Id": params.id,
                         "Type": OPPORTUNITY}];
            serviceManager.unmarkChainAsCrmLinked(message_id, data);
        }

        added_rows = added_rows + 1;
        if(linked_count == added_rows)
            hideLoader();
    }

    function onGetCrmCase(params, case_info){
        var contact_info = {};
        contact_info.entity_id = case_info.id;
        contact_info.entity_type = CASE;
        contact_info.title = case_info.title;
        addLinkedTableRow(contact_info);
    }

    function onGetCrmCaseError(params, response){
        if(response[0] == 'Not found' 
            && response[1] == 'Not found'
            && response[2] == 'Not found'){
            var message_id = getMessageId();
            var data = [{"Id": params.id,
                         "Type": CASE}];
            serviceManager.unmarkChainAsCrmLinked(message_id, data);
        }

        added_rows = added_rows + 1;
        if(linked_count == added_rows)
            hideLoader();
    }

    function hideLoader(html){
        if(html == undefined){
            $('.crm_popup .loader').hide();
        } else{
            html.find('.loader').hide();
        }
    }

    function getSelectedEntityType(){
        return $('.crm_popup #entity-type').val();
    };

    function _getAlreadyLinkedContacts(){
        var ids = {};
        $('.crm_popup .linked_entity_row').each(function(i,val){
                ids[$(val).attr('entity_type').concat(':').concat($(val).data('entity_id'))] = true;
        });
        return ids;
    }

    function _addAutocomplete(html, is_export){
        var input = '#link_search_panel';
        var search_icon = '.crm_search_contact_icon';
        var loading_icon = '.crm_search_loading_icon';

        var onGetCrmContactsByPrefix = function(params, contacts) {
            var already_linked_contacts = _getAlreadyLinkedContacts();
            var names = contacts.map(function(val, i){
                if(!already_linked_contacts["1:".concat(val.id)])
                    return {
                        label: val.displayName,
                        value: val.id,
                        entity_type: CONTACT,
                        entity_id: val.id,
                        title: val.displayName,
                        avatarLink: val.smallFotoUrl
                    };
            }).filter(function(val){return val != undefined; });

            params.responseFunction(names);
            $(search_icon).show();
            $(loading_icon).hide();
        };

        var onGetCrmCaseByPrefix = function(params, cases) {
            var already_linked_contacts = _getAlreadyLinkedContacts();
            var names = cases.map(function(val, i){
                if(!already_linked_contacts["2:".concat(val.id)])
                    return {
                        label: val.title,
                        value: val.id,
                        entity_type: CASE,
                        entity_id: val.id,
                        title: val.title,
                    };
            }).filter(function(val){return val != undefined; });


            params.responseFunction(names);
            $(search_icon).show();
            $(loading_icon).hide();
        };

        var onGetCrmOpportunityByPrefix = function(params, cases) {
            var already_linked_contacts = _getAlreadyLinkedContacts();
            var names = cases.map(function(val, i){
                if(!already_linked_contacts["3:".concat(val.id)])
                    return {
                        label: val.title,
                        value: val.id,
                        entity_type: OPPORTUNITY,
                        entity_id: val.id,
                        title: val.title,
                    };
            }).filter(function(val){return val != undefined; });


            params.responseFunction(names);
            $(search_icon).show();
            $(loading_icon).hide();
        };

        html.find(input).autocomplete({
            minLength: 1,
            delay: 500,
            autoFocus: true,
            appendTo: html.find(input).parent(),
            select: function(event, ui) {
                addLinkedTableRow(ui.item, is_export);
                new_added_contact_ids.push({"Id": ui.item.entity_id, "Type": getSelectedEntityType()});
                $(input).val('');
                return false;
            },
            create: function(event, ui) {
                $(window).resize(function () {
                    if ($(input).data("uiAutocomplete") != undefined) $(input).data("uiAutocomplete").close();
                });
            },
            focus: function(event, ui) {
                return false;
            },
            search: function(event, ui) {
                return true;
            },
            source: function(request, response) {
                var term = request.term;
                $(search_icon).hide();
                $(loading_icon).show();
                var data;
                if(getSelectedEntityType() == CONTACT){
                    data = {filter:{prefix: term, searchType: -1},
                        success: onGetCrmContactsByPrefix};
                    window.Teamlab.getCrmContactsByPrefix({ searchText: term, responseFunction: response, input: input }, data);
                }
                if(getSelectedEntityType() == CASE){
                    data = {filter:{prefix: term, searchType: -1},
                        success: onGetCrmCaseByPrefix};
                    window.Teamlab.getCrmCasesByPrefix({ searchText: term, responseFunction: response, input: input }, data);
                }
                if(getSelectedEntityType() == OPPORTUNITY){
                    data = {filter:{prefix: term},
                        success: onGetCrmOpportunityByPrefix};
                    window.Teamlab.getCrmOpportunitiesByPrefix({ searchText: term, responseFunction: response, input: input }, data);
                }
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

    return {
        init: init,
        getCrmLinkControl: getCrmLinkControl,
        getCrmExportControl: getCrmExportControl
    };
})(jQuery)