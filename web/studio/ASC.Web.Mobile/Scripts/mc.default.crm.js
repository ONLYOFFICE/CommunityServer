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

;window.DefaultMobile = (function(DefaultMobile) {
    if (!DefaultMobile) {
        console.log('Default.crm: has no DefaultMobile');
        return DefaultMobile;
    }
    var lastFilterValue = '', lastSearchValue = '';
    
    function getContactNoteData($page) {
        var data = {
            contactid: $page.find('input.note-contactid:first').removeClass('error-field').val(),
            title: $page.find('input.note-title:first').removeClass('error-field').val() || '',
            content: $page.find('textarea.note-description:first').removeClass('error-field').val() || ''
        };
        for (var fld in data) {
            if (data.hasOwnProperty(fld)) {
                data[fld] = TeamlabMobile.verificationValue(data[fld]);
            }
        }
        var errors = [];
        for (var fld in data) {
            if (data.hasOwnProperty(fld)) {
                if (data[fld] !== null) {
                    continue;
                }
                switch (fld) {
                    case 'contactid': errors.push($page.find('input.note-contactid:first').addClass('error-field')); break;
                    case 'content': errors.push($page.find('textarea.note-description:first').addClass('error-field')); break;
                    case 'title': errors.push($page.find('input.note-title:first').addClass('error-field')); break;
                }
            }
        }
        if (errors.length === 0) {
            return data;
        }
        ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
        return null;
    }
    
    DefaultMobile.reset_search_crm_contacts = function(evt, $page, $this) {
        lastSearchValue = '';
        TeamlabMobile.getCrmContactsBySearchValue(null);
    };
    DefaultMobile.search_crm_contacts = function(evt) {
        var $form = $(evt.target), searchvalue = '';
        searchvalue = $form.length > 0 ? $form.find('input.top-search-field:first').val() : searchvalue;
        if (typeof searchvalue !== 'string' || (searchvalue = (searchvalue.replace(/^\s+|\s+$/g, '')).toLowerCase()) === lastSearchValue) {
            return undefined;
        }
        lastSearchValue = searchvalue;
        TeamlabMobile.getCrmContactsBySearchValue(searchvalue);
    };
    DefaultMobile.filter_crm_contacts = function(evt) {
        var $page = null, item = null, $items = null, itemsInd = 0, value = '', filtervalue = evt.target.value;
        if (typeof filtervalue !== 'string' || (filtervalue = (filtervalue.replace(/^\s+|\s+$/g, '')).toLowerCase()) === lastFilterValue) {
            return undefined;
        }
        lastFilterValue = filtervalue;
        $page = $('div.ui-page-active:first');
        if (filtervalue) {
            $page.find('form.search-form:first').addClass('active');
        } else {
            $page.find('form.search-form:first').removeClass('active');
        }
        //todo        
        $items = $page.find('li.item.person');
        itemsInd = $items.length;
        while (itemsInd--) {
            item = $items[itemsInd];
            value = item.getAttribute('data-itemname');
            if (!value) {
                continue;
            }
            if (value.toLowerCase().indexOf(filtervalue) === -1) {
                $(item).addClass('uncorrect-item');
            } else {
                hasCorrect = true;
                $(item).removeClass('uncorrect-item');
            }
        }
    };
    DefaultMobile.search_crm_tasks = function(evt) {
        var $form = $(evt.target), searchvalue = '';
        searchvalue = $form.length > 0 ? $form.find('input.top-search-field:first').val() : searchvalue;
        if (searchvalue) {
            TeamlabMobile.getCrmTasksBySearchValue(searchvalue);
        }
        else {
            return;
        }
    };
    DefaultMobile.load_more_crm_items = function(evt, $page, $button) {
        var getMoreItems = null;
        var params = {};
        if ($button.attr('data-id')) {
            params.id = $button.attr('data-id');
        }
        if ($button.attr('page')) {
            params.page = $button.attr('page');
        }
        if ($page.hasClass('page-crm')) {
            getMoreItems = TeamlabMobile.getMoreCrmItems;
        }
        if ($page.hasClass('page-crm-tasks')) {
            getMoreItems = TeamlabMobile.getMoreCrmTasks;
        }
        if ($page.hasClass('page-crm-contacttasks')) {
            getMoreItems = TeamlabMobile.getMoreCrmContactTasks;
        }
        if ($page.hasClass('page-crm-history')) {
            getMoreItems = TeamlabMobile.getMoreCrmHistoryEvents;
        }
        if ($page.hasClass('page-crm-contactpersones')) {
            getMoreItems = TeamlabMobile.getMoreCrmContactPersones;
        }
        if ($page.hasClass('page-crm-contactfiles')) {
            getMoreItems = TeamlabMobile.getMoreCrmContactFiles;
        }
        if (getMoreItems) {
            $page.addClass('loading-items');
            if (params.id != null) {
                getMoreItems(null, params.id);
            }
            if (params.page != null) {
                getMoreItems(null, params.page);
            }
            if (params.id == null && params.page == null) {
                getMoreItems(null);
            }
        }
    }
    DefaultMobile.add_crm_task = function(evt, $page, $button) {
        if ($page.length === 0) {
            return undefined;
        }
        $button.removeClass("add-crm-task");
        var data = {
            //description   : null,            
            title: null,
            deadline: null,
            responsibleid: null,
            categoryid: -1,
            isnotify: true
        };
        var errors = [];
        if ($('div.ui-page-active').find('input.task-title').val().length <= 0) {
            errors.push($('div.ui-page-active').find('input.task-title').addClass('error-field'));
        }
        else {
            data.title = $('div.ui-page-active').find('input.task-title:first').removeClass('error-field').val();
        }
        if ($('div.ui-page-active').find('select.task-type').attr('value') == -1) {
            errors.push($('div.ui-page-active').find('select.task-type').parent().addClass('error-field'));
        }
        else {
            $('div.ui-page-active').find('select.task-type').parent().removeClass('error-field').attr('value');
            data.categoryid = $('div.ui-page-active').find('select.task-type  option:selected').val();
        }
        if ($('div.ui-page-active').find('textarea.task-description').val().length > 0) {
            data.description = $('div.ui-page-active').find('textarea.task-description').removeClass('error-field').val();
        }
        if ($('div.ui-page-active').find('input.task-duedate').val() == "" || $('div.ui-page-active').find('input.task-duedate').val() == null) {
            errors.push($('div.ui-page-active').find('input.task-duedate').addClass('error-field'));
        }
        else {
            data.deadline = new Date($('div.ui-page-active').find('input.task-duedate:first').removeClass('error-field').scroller('getDate'));
            data.deadline.setMinutes(00);
            data.deadline.setHours(00);
        }
        if ($('div.ui-page-active').find('select.group-responsible  option:selected').val() == -1 || $('div.ui-page-active').find('select.group-responsible  option:selected').val() == null) {
            errors.push($('div.ui-page-active').find('select.group-responsible').parent().addClass('error-field'));
        }
        else {
            $('div.ui-page-active').find('select.group-responsible').parent().removeClass('error-field');
        }
        if ($('div.ui-page-active').find('select.task-responsible  option:selected').val() == -1 || $('div.ui-page-active').find('select.task-responsible  option:selected').val() == null) {
            errors.push($('div.ui-page-active').find('select.task-responsible').addClass('error-field'));
        }
        else {
            data.responsibleid = $('div.ui-page-active').find('select.task-responsible  option:selected').removeClass('error-field').val();
        }
        if ($button.attr('data-id') != '' || $button.attr('data-id') != null || $button.attr('data-id') != undefined) {
            data.contactId = $button.attr('data-id');
        }
        if (errors.length == 0) {
            return Teamlab.addCrmTask(null, data, {
                success: function(params, items) {
                    if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.crmaddtask)) {
                        items = TeamlabMobile.preparationCrmTask(items);
                        if ($button.attr('data-id') != null) {
                            ASC.Controls.AnchorController.lazymove({ back: '#crm/contact/' + data.contactId + '/tasks' }, '#crm/contact/' + data.contactId + '/tasks');
                        }
                        else {
                            ASC.Controls.AnchorController.lazymove({ back: '#crm/tasks/' + items.timeType }, '#crm/tasks/' + items.timeType);
                        }
                    }
                }
            });
        }
        ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
        $button.addClass("add-crm-task");
        return false;
    }
    DefaultMobile.add_crm_company = function(evt, $page, $button) {
        if ($page.length === 0) {
            return undefined;
        }
        var data = {
            companyName: null
        };
        var contact_data = [];
        data.addresses = [];
        var errors = [];
        if ($('div.ui-page-active').find('input.persone-firstname').val().length <= 0) {
            errors.push($('div.ui-page-active').find('input.persone-firstname').addClass('error-field'));
        }
        else {
            data.companyName = $('div.ui-page-active').find('input.persone-firstname').removeClass('error-field').val();
        }
        var phones = $('div.ui-page-active div.fields-container[data = "phone"]').find('div.info');
        for (var i = 0; i < phones.length; i++) {
            $(phones[i]).find('input.contact-phone').removeClass('error-field');
            var phones_i = $(phones[i]).find('input.contact-phone').val();
            if ($(phones[i]).find('input.contact-phone').val().length > 0) {
                if (phones_i.search(/[0-9 -()]+/) == -1) {
                    errors.push($(phones[i]).find('input.contact-phone').addClass('error-field'));
                }
                else {
                    var phone = {};
                    phone.InfoType = 0;
                    phone.Category = $(phones[i]).find('select.phone-type option:selected"').val();
                    phone.Data = $(phones[i]).find('input.contact-phone').val();
                    if (i == 0) {
                        phone.IsPrimary = true;
                    }
                    else {
                        phone.IsPrimary = false;
                    }
                    contact_data.push(phone);
                }
            }
        }
        var emailes = $('div.ui-page-active div.fields-container[data = "email"]').find('div.info');
        for (var i = 0; i < emailes.length; i++) {
            $(emailes[i]).find('input.contact-email').removeClass('error-field');
            var emailes_i = $(emailes[i]).find('input.contact-email').val();
            if (emailes_i.length > 0) {
                if (emailes_i.search(/[\w]+[@][\w]+[.][a-zA-Z]+/) == -1) {
                    errors.push($(emailes[i]).find('input.contact-email').addClass('error-field'));
                }
                else {
                    var email = {};
                    email.InfoType = 1;
                    email.Category = $(emailes[i]).find('select.email-type option:selected"').val();
                    email.Data = $(emailes[i]).find('input.contact-email').val();
                    if (i == 0) {
                        email.IsPrimary = true;
                    }
                    else {
                        email.IsPrimary = false;
                    }
                    contact_data.push(email);
                }
            }
        }
        var addresses = $('div.ui-page-active').find('div.fields-container[data = "address"]').find("div.info");
        for (var i = 0; i < addresses.length; i++) {
            if ($(addresses[i]).find("input.detail").val().length > 0 || $(addresses[i]).find("input.city").val().length > 0 || $(addresses[i]).find("input.state").val().length > 0 || $(addresses[i]).find("input.zipcode").val().length > 0) {
                var address = {};
                address.InfoType = 7;
                address.Category = $(addresses[i]).find('select.address-type option:selected"').val();
                var country = $(addresses[i]).find("input.detail").val();
                var city = $(addresses[i]).find("input.city").val();
                var street = $(addresses[i]).find("input.street").val();
                var state = $(addresses[i]).find("input.state").val();
                var zipcode = $(addresses[i]).find("input.zipcode").val();
                address.data = '{\r\n  \"street\": \"' + street + '\",\r\n  \"city\": \"' + city + '\",\r\n  \"state\": \"' + state + '\",\r\n  \"zip\": \"' + zipcode + '\",\r\n  \"country\": \"' + country + '\"\r\n}';
                if (i == 0) {
                    address.IsPrimary = true;
                }
                else {
                    address.IsPrimary = false;
                }
                contact_data.push(address);
            }
        }
        var sites = $('div.ui-page-active').find('input.contact-site');
        for (var i = 0; i < sites.length; i++) {
            if ($(sites[i]).val().length > 0) {
                var site = {};
                site.InfoType = 2;
                site.Data = $(sites[i]).val();
                if (i == 0) {
                    site.IsPrimary = true;
                }
                else {
                    site.IsPrimary = false;
                }
                contact_data.push(site);
            }
        }
        if (errors.length == 0) {
            $button.removeClass("add-crm-company");
            if ($button.hasClass("edit")) {
                return Teamlab.updateCrmCompany(null, $button.attr("data-id"), data, {
                    success: function(params, items) {
                        var new_data = { items: contact_data };
                        Teamlab.updateCrmContactData(null, items.id, new_data);
                        if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.editcompany)) {
                            TeamlabMobile.onGetContact_(params, items);
                            ASC.Controls.AnchorController.lazymove({ back: '#crm/contact/' + items.id }, '#crm/contact/' + items.id);
                        }
                    }
                });
            }
            else {
                return Teamlab.addCrmCompany(null, data, {
                    success: function(params, items) {
                        var new_data = { items: contact_data };
                        if (new_data.items.length > 0) Teamlab.addCrmContactData(null, items.id, new_data);
                        if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.addcompany)) {
                            ASC.Controls.AnchorController.lazymove({ back: '#crm/contact/' + items.id }, '#crm/contact/' + items.id);
                        }
                    }
                });
            }
        }
        ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
        return false;
    }
    DefaultMobile.add_crm_persone = function(evt, $page, $button) {
        if ($page.length == 0) {
            return undefined;
        }
        var data = {
            firstName: null,
            lastName: null
        };
        var contact_data = [];
        data.addresses = [];
        var errors = [];
        if ($('div.ui-page-active').find('input.persone-firstname').val().length <= 0) {
            errors.push($('div.ui-page-active').find('input.persone-firstname').addClass('error-field'));
        }
        else {
            data.firstName = $('div.ui-page-active').find('input.persone-firstname').removeClass('error-field').val();
        }
        if ($('div.ui-page-active').find('input.persone-secondname:first').val().length <= 0) {
            errors.push($('div.ui-page-active').find('input.persone-secondname:first').addClass('error-field'));
        }
        else {
            data.lastName = $('div.ui-page-active').find('input.persone-secondname').removeClass('error-field').val();
        }
        if (($button.attr('data-contactid') != null && $button.hasClass("edit")) || ($button.attr('data-id') != null && !$button.hasClass("edit"))) {
            data.companyId = $button.attr('data-contactid') != null ? $button.attr('data-contactid') : $button.attr('data-id');
        }
        if ($('div.ui-page-active').find('input.persone-position').val().length > 0) {
            data.jobTitle = $('div.ui-page-active').find('input.persone-position').removeClass('error-field').val();
        }
        var phones = $('div.ui-page-active div.fields-container[data = "phone"]').find('div.info');
        for (var i = 0; i < phones.length; i++) {
            $(phones[i]).find('input.contact-phone').removeClass('error-field');
            var phones_i = $(phones[i]).find('input.contact-phone').val();
            if ($(phones[i]).find('input.contact-phone').val().length > 0) {
                if (phones_i.search(/[0-9 -()]+/) == -1) {
                    errors.push($(phones[i]).find('input.contact-phone').addClass('error-field'));
                }
                else {
                    var phone = {};
                    phone.InfoType = 0;
                    phone.Category = $(phones[i]).find('select.phone-type option:selected"').val();
                    phone.Data = $(phones[i]).find('input.contact-phone').val();
                    if (i == 0) {
                        phone.IsPrimary = true;
                    }
                    else {
                        phone.IsPrimary = false;
                    }
                    contact_data.push(phone);
                }
            }
        }
        var emailes = $('div.ui-page-active div.fields-container[data = "email"]').find('div.info');
        for (var i = 0; i < emailes.length; i++) {
            $(emailes[i]).find('input.contact-email').removeClass('error-field');
            var emailes_i = $(emailes[i]).find('input.contact-email').val();
            if (emailes_i.length > 0) {
                if (emailes_i.search(/[\w]+[@][\w]+[.][a-zA-Z]+/) == -1) {
                    errors.push($(emailes[i]).find('input.contact-email').addClass('error-field'));
                }
                else {
                    var email = {};
                    email.InfoType = 1;
                    email.Category = $(emailes[i]).find('select.email-type option:selected"').val();
                    email.Data = $(emailes[i]).find('input.contact-email').val();
                    if (i == 0) {
                        email.IsPrimary = true;
                    }
                    else {
                        email.IsPrimary = false;
                    }
                    contact_data.push(email);
                }
            }
        }
        var addresses = $('div.ui-page-active').find('div.fields-container[data = "address"]').find("div.info");
        for (var i = 0; i < addresses.length - 1; i++) {
            if ($(addresses[i]).find("input.detail").val().length > 0 || $(addresses[i]).find("input.city").val().length > 0 || $(addresses[i]).find("input.state").val().length > 0 || $(addresses[i]).find("input.zipcode").val().length > 0) {
                var address = {};
                address.InfoType = 7;
                address.Category = address.Category = $(addresses[i]).find('select.address-type option:selected"').val();
                var country = $(addresses[i]).find("input.detail").val();
                var city = $(addresses[i]).find("input.city").val();
                var street = $(addresses[i]).find("input.street").val();
                var state = $(addresses[i]).find("input.state").val();
                var zipcode = $(addresses[i]).find("input.zipcode").val();
                address.data = '{\r\n  \"street\": \"' + street + '\",\r\n  \"city\": \"' + city + '\",\r\n  \"state\": \"' + state + '\",\r\n  \"zip\": \"' + zipcode + '\",\r\n  \"country\": \"' + country + '\"\r\n}';
                if (i == 0) {
                    address.IsPrimary = true;
                }
                else {
                    address.IsPrimary = false;
                }
                contact_data.push(address);
            }
        }
        var sites = $('div.ui-page-active').find('input.contact-site');
        for (var i = 0; i < sites.length; i++) {
            if ($(sites[i]).val().length > 0) {
                var site = {};
                site.InfoType = 2;
                site.Data = $(sites[i]).val();
                if (i == 0) {
                    site.IsPrimary = true;
                }
                else {
                    site.IsPrimary = false;
                }
                contact_data.push(site);
            }
        }
        if (errors.length == 0) {
            $button.removeClass("add-crm-persone");
            if ($button.hasClass("edit")) {
                return Teamlab.updateCrmPerson({}, $button.attr("data-id"), data, {
                    success: function(params, items) {
                        if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.editperson)) {
                            var new_data = { items: contact_data };
                            Teamlab.updateCrmContactData(null, items.id, new_data);
                            TeamlabMobile.onGetContact_(params, items);
                            ASC.Controls.AnchorController.move({ back: '#crm/contact/' + items.id }, '#crm/contact/' + items.id);
                        }
                    }
                });
            }
            else {
                return Teamlab.addCrmPerson(null, data, {
                    success: function(params, items) {
                        var new_data = { items: contact_data };
                        if (new_data.items.length > 0) Teamlab.addCrmContactData(null, items.id, new_data);
                        if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.addpersone)) {
                            ASC.Controls.AnchorController.lazymove({ back: '#crm/contact/' + items.id }, '#crm/contact/' + items.id);
                        }
                    }
                });
            }
        }
        ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
        return false;
    }
    DefaultMobile.add_crm_history_event = function(evt, $page, $button) {
        if ($page.length === 0) {
            return undefined;
        }
        //$button.removeClass("add-crm-history-event");       
        var data = {
            contactId: $('div.ui-page-active').find('button.add-crm-history-event').attr('data-id')
        };
        var errors = [];
        if ($('div.ui-page-active').find('select.historyevent-type').attr('value') == -1) {
            errors.push($('div.ui-page-active').find('select.historyevent-type').parent().addClass('error-field'));
        }
        else {
            $('div.ui-page-active').find('select.historyevent-type').parent().removeClass('error-field');
            data.categoryId = $('div.ui-page-active').find('select.historyevent-type').attr('value');
        }
        if ($('div.ui-page-active').find('textarea.historyevent-description').val() == "" || $('div.ui-page-active').find('textarea.historyevent-description').val() == null) {
            errors.push($('div.ui-page-active').find('textarea.historyevent-description').addClass('error-field'));
        }
        else {
            data.content = $('div.ui-page-active').find('textarea.historyevent-description').removeClass('error-field').val();
        }
        if ($('div.ui-page-active').find('input.historyevent-date').val() == "" || $('div.ui-page-active').find('input.historyevent-date').val() == null) {
            errors.push($('div.ui-page-active').find('input.historyevent-date').addClass('error-field'));
        }
        else {
            data.created = new Date($('input.historyevent-date:first').removeClass('error-field').scroller('getDate'));
            var now = new Date();
            data.created.setHours(now.getHours(), now.getMinutes(), now.getSeconds());
        }
        if (errors.length == 0) {
            return Teamlab.addCrmHistoryEvent(null, data, { success: function() {
                if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.addhistoryevent)) {
                    ASC.Controls.AnchorController.lazymove({ back: '#crm/contact/' + data.contactId + '/history' }, '#crm/contact/' + data.contactId + '/history');
                }
            }
            });
        }
        ASC.Controls.messages.show(Base64.encode(ASC.Resources.ErrEmpyField), 'error', ASC.Resources.ErrEmpyField);
        return false;
    }
    DefaultMobile.cansel_crm_task = function(evt, $page, $button) {
        if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.crmaddtask)) {
            if ($page.attr('data-itemid') != undefined && $page.attr('data-itemid') != '' && $page.attr('data-itemid') != null) {
                ASC.Controls.AnchorController.lazymove({ back: '#crm/contact/' + $page.attr('data-itemid') + '/tasks' }, '#crm/contact/' + $page.attr('data-itemid') + '/tasks');
            }
            else {
                ASC.Controls.AnchorController.lazymove({ back: '#crm/tasks/today' }, '#crm/tasks/today');
            }
        }
    }
    DefaultMobile.cansel_crm_person = function(evt, $page, $button) {
        if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.crmaddtask)) {
            if ($page.attr('data-itemid') != undefined && $page.attr('data-itemid') != '' && $page.attr('data-itemid') != null) {
                ASC.Controls.AnchorController.lazymove({ back: '#crm/contact/' + $page.attr('data-itemid') }, '#crm/contact/' + $page.attr('data-itemid'));
            }
            else {
                ASC.Controls.AnchorController.lazymove({ back: '#crm/tasks/today' }, '#crm/tasks/today');
            }
        }
    }
    DefaultMobile.delete_crm_history_event = function(evt, $page, $button) {
        if (confirm('Delete this event?')) {
            Teamlab.removeCrmHistoryEvent(null, $button.attr('data-id'), {
                success: function() {
                    if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.contacthistory)) {
                        $button.parent().parent().remove();
                    }
                }
            })
        }
        else { }
    }
    DefaultMobile.add = function(evt, $page, $button) {
        var $item = $button.parent().parent();
        var fields = [];
        fields = $item.find("div");
        if (fields.length < 5) {
            $item.find("div:first").clone().prependTo($button.parent().parent());
        }
        else {
            $button.parent().hide;
        }
    }
    DefaultMobile.delete_field = function(evt, $page, $button) {
        var $item = $button.parent();
        $item.remove();
    }
    DefaultMobile.group_responsible = function(evt, $page, $button) {
        Teamlab.getProfiles(null, {
            success: function(params, items) {
                $('.task-responsible').parent().remove();
                $('.assign').remove();
                if ($page.find('select.group-responsible option:selected"').val() != -1) {
                    $('.group-responsible').parent().after('<label class = "assign">Assign to:</label><select class="task-responsible" type="text"></select>');
                    var group_id = $('.group-responsible option:selected').val();
                    if (group_id == 0) {
                        for (var i = 0; i < items.length; i++) {
                            if (items[i].group == null) {
                                $('.task-responsible').append('<option value="' + items[i].id + '">' + items[i].displayName + '</option>');
                            }
                        }
                    }
                    else {
                        for (var i = 0; i < items.length; i++) {
                            if (items[i].group != null && items[i].group.id == group_id) {
                                $('.task-responsible').append('<option value="' + items[i].id + '">' + items[i].displayName + '</option>');
                            }
                        }
                    }
                    jQuery(document).trigger('updatepage');
                }
            }
        });
    }
    DefaultMobile.task_delete = function(evt, $page, $button) {
        if (confirm('Delete this task?')) {
            Teamlab.removeCrmTask(null, $page.attr('data-itemid'));
            if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.crmtask) || ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.contacttask)) {
                if ($page.attr("data-contactid") != null || $page.attr("data-contactid") != undefined) {
                    ASC.Controls.AnchorController.lazymove({ back: '#crm/contact/' + $page.attr("data-contactid") + '/tasks' }, '#crm/contact/' + $page.attr("data-contactid") + '/tasks');
                }
                else {
                    ASC.Controls.AnchorController.lazymove({ back: '#crm/tasks/today' }, '#crm/tasks/today');
                }
            }
        }
        else { }
    }
    /*DefaultMobile.add_phone = function(evt, $page, $button){        if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.contact)) {                TeamlabMobile.editCrmContact($page.attr('data-itemid'));                //$(".contact-phone").focus();                  }          }        DefaultMobile.add_email = function(evt, $page, $button){        if (ASC.Controls.AnchorController.testAnchor(TeamlabMobile.regexps.contact)) {                TeamlabMobile.editCrmContact($page.attr('data-itemid'));                //$(".contact-email").focus();                        }          }*/
    DefaultMobile.edit_contact = function(evt, $page, $button) {
        TeamlabMobile.editCrmContact($button.attr('data-id'));
    }
    DefaultMobile.crm_add_note_to_contact = function(evt, $page, $button) {
        var data = getContactNoteData($page);
        if (data && TeamlabMobile.addCrmNoteToContact(data.contactid, data)) {
            jQuery(document).trigger('changepage');
            $button.addClass('disabled');
        }
    }
    return DefaultMobile;
})(DefaultMobile);;(function($) {        TeamlabMobile.bind(TeamlabMobile.events.crmPage, onCrmPage);    TeamlabMobile.bind(TeamlabMobile.events.crmTasksPage, onCrmTasksPage);            function onAddMoreCrmItemsToList (data, params) {              
        data = {items: data}
        $('.ui-page-active').find('.loading-indicator').hide();
        if(!params.nextIndex){
            $('div.ui-page-active').find('span.load-more-items').hide();
        }        
        $('.ui-page-active').find('ul.ui-timeline:first').append(DefaultMobile.processTemplate(params.tmpl, data));            jQuery(document).trigger('updatepage');     }    function onCrmPage(data, params) {        data = { pagetitle: ASC.Resources.LblCrmTitle, type: 'crm-page', items: data, nextIndex: params.nextIndex, filtervalue: params.filtervalue || null};        var $page = DefaultMobile.renderPage('crm-page', 'page-crm', 'crm', ASC.Resources.LblCrmTitle, data).addClass('filter-none');    }            function onCrmTasksPage(data, params) {                    data = { pagetitle: ASC.Resources.LblCrmTitle, type: 'crm-tasks-page', items: data, nextIndex: params.nextIndex, page: params.page};                var $page = DefaultMobile.renderPage('crm-tasks-page', 'page-crm-tasks', 'crm-tasks', ASC.Resources.LblCrmTitle, data).addClass('filter-none');    }        /*function onAddMoreCrmTasksToList (data, params) {           
        data = {items: data}
        $('.ui-page-active').find('.loading-indicator').hide();
        if(!params.nextIndex){
            $('div.ui-page-active').find('span.load-more-items').hide();
        }        
        $('.ui-page-active').find('ul.ui-timeline:first').append(DefaultMobile.processTemplate(TeamlabMobile.templates.lbcrmtaskstimeline, data));            jQuery(document).trigger('updatepage');     }*/    function onCrmContactTasksPage (contact, data, params) {        data = { pagetitle: ASC.Resources.LblCrmTitle, type: 'page-crm-contactasks', items: data, contact: contact, id: params.id, nextIndex: params.nextIndex};        var $page = DefaultMobile.renderPage('crm-contacttasks-page', 'page-crm-contactasks', 'crm-contacttasks', ASC.Resources.LblCrmTitle, data).addClass('filter-none');    }             function onCrmContactPersonesPage (contact, data, params) {        data = { pagetitle: ASC.Resources.LblCrmTitle, type: 'page-crm-contacpersones', items: data, contact: contact, id: params.id, nextIndex: params.nextIndex};        var $page = DefaultMobile.renderPage('crm-contactpersones-page', 'page-crm-contacpersones', 'crm-contactpersones', ASC.Resources.LblCrmTitle, data).addClass('filter-none');    }    function onCrmContactFilesPage (contact, data, params) {                data = { pagetitle: ASC.Resources.CrmDocsPage, type: 'page-crm-contactfiles', items: data, contact: contact, id: params.id, nextIndex: params.nextIndex };        var $page = DefaultMobile.renderPage('page-crm-contactfiles', 'page-crm-contactfiles', 'crm-contactfiles', ASC.Resources.LblCrmTitle, data).addClass('filter-none');    }        function onCrmContactHistoryPage(contact, data, params) {                data = { pagetitle: ASC.Resources.CrmHistoryPage, type: 'page-crm-contacthistory', items: data, contact: contact, id: params.id, nextIndex: params.nextIndex};                var $page = DefaultMobile.renderPage('page-crm-contacthistory', 'page-crm-contacthistory', 'crm-contacthistory', ASC.Resources.LblCrmTitle, data).addClass('filter-none');    }        function onPersonsPage(contact, data, params) {        data = { pagetitle: ASC.Resources.LblCrmTitle, type: 'crm-page-persons', items: data };        var $page = DefaultMobile.renderPage('crm-persons-page', 'page-crm-persons', 'crm-persons', ASC.Resources.LblCrmTitle, data).addClass('filter-persons');    }    function onCompaniesPage(data, params) {        data = { pagetitle: ASC.Resources.LblCrmTitle, type: 'crm-page-companies', items: data };        var $page = DefaultMobile.renderPage('crm-companies-page', 'page-crm-companies', 'crm-companies', ASC.Resources.LblCrmTitle, data).addClass('filter-companies');    }    function onPersonPage(data, params) {                var fromDashboard = TeamlabMobile.regexps.crm.test(ASC.Controls.AnchorController.getLastAnchor());        data = { pagetitle: ASC.Resources.CrmContactInfo, title: ASC.Resources.CrmContactInfo, id: data.id, parentid: null, fromDashboard: fromDashboard, back: null, item: data,  phone: params.phone, email: params.email, contactTypes: params.contactTypes};        if (params.hasOwnProperty('back')) {            data.back = params.back;        }        var $page = DefaultMobile.renderPage('crm-person-page', 'page-crm-person', 'crm-person-' + data.id, ASC.Resources.LblPerson, data);    }        function onCompanyPage(data, params) {                var fromDashboard = TeamlabMobile.regexps.crm.test(ASC.Controls.AnchorController.getLastAnchor());        data = { pagetitle: ASC.Resources.CrmContactInfo, title: ASC.Resources.CrmContactInfo, id: data.id, parentid: null, fromDashboard: fromDashboard, back: null, item: data, phone: params.phone, email: params.email, contactTypes: params.contactTypes};        if (params.hasOwnProperty('back')) {            data.back = params.back;        }        var $page = DefaultMobile.renderPage('crm-company-page', 'page-crm-company', 'crm-company-' + data.id, ASC.Resources.LblCompany, data);    }    function onCrmTaskPage(data, params) {                  var fromDashboard = TeamlabMobile.regexps.crm.test(ASC.Controls.AnchorController.getLastAnchor());        data = { pagetitle: ASC.Resources.LblCrmTitle, title: ASC.Resources.LblCrmTitle, id: data.id, parentid: null, fromDashboard: fromDashboard, back: null, item: data };               if(params.contactId != null){            data.contactId = params.contactId;        }        if (params.hasOwnProperty('back')) {            data.back = params.back;        }                var $page = DefaultMobile.renderPage('crm-task-page', 'page-crm-task', 'crm-task-' + data.id, ASC.Resources.LblCompany, data);    }    function onCrmAddTaskPage(params, groups, categories) {              data = { pagetitle: ASC.Resources.BtnCrmAddTask, title: ' ', type: 'crm-addtask', id: params, groups: groups, categories: categories};       var $page = DefaultMobile.renderPage('crm-addtask-page', 'page-crm-addtask', 'crm-addtask' + Math.floor(Math.random() * 1000000), ' ', data);    }    function onCrmAddCompanyPage(contact, params) {                   data = { pagetitle: ASC.Resources.CrmAddCompanyPage, title: ' ', type: 'crm-addcompany', contact: contact, contactTypes: params.contactTypes};        var $page = DefaultMobile.renderPage('crm-addcompany-page','page-crm-addcompany', 'crm-addcompany' + Math.floor(Math.random() * 1000000), ' ', data);    }    function onCrmAddPersonePage(contact, companies, params) {                                  data = { pagetitle: ASC.Resources.CrmAddPersonPage, title: ' ', type: 'crm-addpersone', companies: companies, contact: contact, contactTypes: params.contactTypes, id: params.id};        var $page = DefaultMobile.renderPage('crm-addpersone-page', 'page-crm-addpersone', 'crm-addpersone' + Math.floor(Math.random() * 1000000), ' ', data);    }    function onCrmAddHistoryEventPage(params, id, items) {                data = { pagetitle: ASC.Resources.CrmAddHistoryEventPage, title: ' ', type: 'crm-addhistoryevent', id: id, category: items};        var $page = DefaultMobile.renderPage('crm-addhistoryevent-page', 'page-crm-addhistoryevent', 'crm-addhistoryevent' + Math.floor(Math.random() * 1000000), ' ', data);    }    function onCrmAddNotePage(params, contact) {        data = { pagetitle: ASC.Resources.CrmAddDocumentPage, title: ' ', type: 'crm-addnote', item: contact };        var $page = DefaultMobile.renderPage('crm-addnote-page', 'page-crm-addnote', 'crm-addnote' + Math.floor(Math.random() * 1000000), ' ', data);    }        function onNavigateCrmDialog() {        var data = { dialogtotle: ASC.Resources.LblCrmTitle, type: 'crm-navigate' };        var $dialog = DefaultMobile.renderDialog('crm-navigate-dialog', 'dialog-crm-navigate', 'crm-navigate' + Math.floor(Math.random() * 1000000), ' ', data);    }    function onAddToContactCrmDialog(id) {        var data = { dialogtotle: ASC.Resources.LblCrmTitle, type: 'crm-addtocontact', id: id };        var $dialog = DefaultMobile.renderDialog('crm-addtocontact-dialog', 'dialog-crm-addtocontact', 'crm-addtocontact' + Math.floor(Math.random() * 1000000), ' ', data);    }    function onAddFileToContactCrmDialog(id) {        var data = { dialogtotle: ASC.Resources.LblDocumentsTitle, contactid: id, fileupload: $.support.fileupload, type: 'documents-additem' };      var $dialog = DefaultMobile.renderDialog('crm-additem-file-dialog', 'dialog-crm-additem-file', 'crm-additem-file' + Math.floor(Math.random() * 1000000), ' ', data);    }    function onAddCrmItemDialog() {        var data = { dialogtotle: ASC.Resources.LblCrmTitle, type: 'crm-additem' };        var $dialog = DefaultMobile.renderDialog('crm-additem-dialog', 'dialog-crm-additem', 'crm-additem' + Math.floor(Math.random() * 1000000), ' ', data);    }        function onUpdateCrmTaskCheckbox(task){                var $page = null, $checkboxes = $();                        if($('div.page-crm-contacttasks').hasClass('update-status')||$('div.page-crm-tasks').hasClass('update-status')||$('div.page-crm-task').hasClass('update-status')){                if($('div.page-crm-contacttasks').hasClass('update-status')){                    $page = $('div.page-crm-contacttasks').removeClass('update-status');                    $checkboxes = $checkboxes.add($page.find('li.item.task[data-itemid="' + task.taskid.id + '"] input.input-checkbox.item-status'));                              $checkboxes.parents('li.item').removeClass('update-status');                    $label = $('label[for="' + $checkboxes.attr('id') + '"]');                                            var $item = $page.find('li.item.task[data-itemid="' + task.taskid.id + '"]') || $page.find('.item.task[data-itemid="' + task.taskid.id + '"]');                                }                if($('div.page-crm-tasks').hasClass('update-status')){                    $page = $('div.page-crm-tasks').removeClass('update-status');                    $checkboxes = $checkboxes.add($page.find('li.item.task[data-itemid="' + task.taskid.id + '"] input.input-checkbox.item-status'));                              $checkboxes.parents('li.item').removeClass('update-status');                    $label = $('label[for="' + $checkboxes.attr('id') + '"]');                                            var $item = $page.find('li.item.task[data-itemid="' + task.taskid.id + '"]') || $page.find('.item.task[data-itemid="' + task.taskid.id + '"]');                               }                if($('div.page-crm-task').hasClass('update-status')){                    $page = $('div.page-crm-task').removeClass('update-status');                    $checkboxes = $checkboxes.add($page.find('input.input-checkbox.item-status'));                              $checkboxes.parents('li.item').removeClass('update-status');                    $label = null                     checkboxesInd = 0;                    checkboxesInd = $checkboxes.length;                    while (checkboxesInd--) {                        $checkbox = $($checkboxes[checkboxesInd]);                        $label = $('label[for="' + $checkboxes[checkboxesInd].id + '"]');                    }                                                                           var $item = $page.find('div.item.task[data-itemid="' + task.taskid.id + '"]');                                           }            }            else{                if($('div.page-crm-contacttasks').hasClass('ui-page-active')) $page = $('div.page-crm-contacttasks');                if($('div.page-crm-tasks').hasClass('ui-page-active')) $page = $('div.page-crm-tasks');                $checkboxes = $checkboxes.add($page.find('li.item.task[data-itemid="' + task.taskid.id + '"] input.input-checkbox.item-status'));                          $checkboxes.parents('li.item').removeClass('update-status');                $label = $('label[for="' + $checkboxes.attr('id') + '"]');                                        var $item = $page.find('li.item.task[data-itemid="' + task.taskid.id + '"]') || $page.find('.item.task[data-itemid="' + task.taskid.id + '"]');            }                                                               if($item.attr('data-isclosed') == '0'){                for (var i = 0; i<$checkboxes.length; i++) {                      $($checkboxes[i]).attr('checked', true);                                    $('label[for="' + $checkboxes[i].id + '"]').addClass('checked');                }                $item.find('span').addClass('closed');                $item.attr('data-isclosed', '1');                                            }             else{                for (var i = 0; i<$checkboxes.length; i++) {                      $($checkboxes[i]).attr('checked', false);                    $('label[for="' + $checkboxes[i].id + '"]').removeClass('checked');                }                $item.find('span').removeClass('closed');                $item.attr('data-isclosed', '0');                            }             }})(jQuery);