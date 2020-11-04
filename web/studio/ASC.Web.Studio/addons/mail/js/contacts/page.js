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


window.contactsPage = (function($) {
    var isInit = false,
        isCrmFilterInit = false,
        isTlFilterInit = false,
        isCustomFilterInit = false,
        filter = new Object,
        page,
        pageActionContainer,
        keepSelectionOnReload = false,
        buttons = [],
        totalCount = 0;

    var selection = new TMContainers.IdMap();

    function init() {
        if (isInit === false) {
            isInit = true;

            setDefaultValueFilter();

            window.Teamlab.bind(window.Teamlab.events.updateMailContact, onUpdateContact);
            window.Teamlab.bind(window.Teamlab.events.createMailContact, onCreateContact);

            crmFilter.events.bind('set', onSetCrmFilter);
            crmFilter.events.bind('reset', onResetFilter);
            crmFilter.events.bind('resetall', onResetAllFilter);
            tlFilter.events.bind('set', onSetTlFilter);
            tlFilter.events.bind('reset', onResetFilter);
            tlFilter.events.bind('resetall', onResetAllFilter);
            customFilter.events.bind('set', onSetCustomFilter);
            customFilter.events.bind('reset', onResetFilter);
            customFilter.events.bind('resetall', onResetAllFilter);

            tlFilter.events.bind('ready', onFilterReady);
            crmFilter.events.bind('ready', onFilterReady);
            customFilter.events.bind('ready', onFilterReady);

            tlFilter.init();
            crmFilter.init();
            customFilter.init();

            page = $('#id_contacts_page');
            pageActionContainer = $('#pageActionContainer');

            buttons = [
                { selector: "#contactActionMenu .viewContact", handler: viewContact },
                { selector: "#contactActionMenu .editContact", handler: editContact },
                { selector: "#contactActionMenu .deleteContact", handler: deleteContacts },
                { selector: "#contactActionMenu .writeLetter", handler: massMailing }
            ];
        }
    }

    // Set checkbox states depending on ids from _Selection
    // Note: _updateSelectionComboCheckbox call don't needed
    function updateSelectionView() {
        var haveOneUnchecked = false;
        var emptyOrDisabled = true;
        $('#ContactsList .row').each(function() {
            var $rowDiv = $(this);
            var contactId = $rowDiv.attr('data_id');
            var $checkbox = $rowDiv.find('.checkbox input[type="checkbox"]');

            if ($checkbox.hasClass('disable')) {
                return true;
            }

            emptyOrDisabled = false;

            if (selection.HasId(contactId)) {
                $checkbox.prop('checked', true);
                $rowDiv.addClass('selected');
                $rowDiv.addClass('ui-selected');
            } else {
                $checkbox.prop('checked', false);
                $rowDiv.removeClass('selected');
                $rowDiv.removeClass('ui-selected');
                haveOneUnchecked = true;
            }
        });
        setSelectionComboCheckbox(!haveOneUnchecked && !emptyOrDisabled);
    }

    function updateSelectionComboCheckbox() {
        // Update checked state
        var uncheckedFound = false;
        $('#ContactsList .row .checkbox input[type="checkbox"]').each(function() {
            if (!selection.HasId($(this).attr('data_id'))) {
                uncheckedFound = true;
                return false;
            }
        });
        setSelectionComboCheckbox(!uncheckedFound);
    }

    function setSelectionComboCheckbox(checked) {
        var checkAll = $('#SelectAllContactsCB');

        if (selection.GetIds().length && !checked) {
            checkAll.prop({ 'indeterminate': true, 'checked': true });
        } else {
            checkAll.prop({ 'indeterminate': false, 'checked': checked });
        }
    }

    function clearCurrentPageSelection() {
        $('#ContactsList .row').each(function() {
            selection.RemoveId($(this).attr('data_id'));
        });
    }

    function onSetCrmFilter(e, params) {
        var isChange = false;
        if (isCrmFilterInit === false) {
            isCrmFilterInit = true;
        } else {
            switch (params.id) {
                case 'sorter':
                    // ToDo: refactore
                    // Filter is sensitive to uppercase N
                    if ('displayName' == params.params.id) {
                        return;
                    }
                    if (filter.CrmSortBy != params.params.id || filter.CrmSortOrder != params.params.sortOrder) {
                        filter.CrmSortBy = params.params.id;
                        filter.CrmSortOrder = params.params.sortOrder;
                        isChange = true;
                    }
                    break;
                case 'text':
                    if (filter.Search != params.params.value) {
                        filter.Search = params.params.value;
                        isChange = true;
                    }
                    break;
                case 'company':
                case 'person':
                case 'withopportunity':
                    if (filter.ContactListView != params.params.value) {
                        filter.ContactListView = params.params.value;
                        isChange = true;
                    }
                    break;
                case 'stages':
                    if (params.params.value == null) {
                        filter.ContactStage = undefined;
                        isChange = true;
                    }
                    if (filter.ContactStage != params.params.value) {
                        filter.ContactStage = params.params.value;
                        isChange = true;
                    }
                    break;
                case 'tags':
                    if (params.params.value == null) {
                        filter.Tags = [];
                        isChange = true;
                    } else if (filter.Tags.length != params.params.value.length) {
                        filter.Tags = [];
                        for (var i = 0; i < params.params.value.length; i++) {
                            filter.Tags.push(params.params.value[i]);
                        }
                        isChange = true;
                    }
                    break;
            }
            if (isChange) {
                ASC.Controls.AnchorController.move(getAnchorByType(filter.ContactsStore) + toAnchor());

                window.ASC.Mail.ga_track(ga_Categories.crmContacts, ga_Actions.filterClick, params.id);
            }
        }
    }

    function onSetTlFilter(e, params) {
        var isChange = false;
        if (isTlFilterInit === false) {
            isTlFilterInit = true;
        } else {
            switch (params.id) {
                case 'sorter':
                    if (filter.TlSortBy != params.params.id || filter.TlSortOrder != params.params.sortOrder) {
                        filter.TlSortBy = params.params.id;
                        filter.TlSortOrder = params.params.sortOrder;
                        isChange = true;
                    }
                    break;
                case 'text':
                    if (filter.Search != params.params.value) {
                        filter.Search = params.params.value;
                        isChange = true;
                    }
                    break;
                case 'group':
                    if (filter.FilterValue != params.params.value) {
                        filter.FilterValue = params.params.value;
                        isChange = true;
                    }
                    break;
            }
            if (isChange) {
                ASC.Controls.AnchorController.move(getAnchorByType(filter.ContactsStore) + toAnchor());

                //google analytics
                window.ASC.Mail.ga_track(ga_Categories.teamlabContacts, ga_Actions.filterClick, params.id);
            }
        }

    }

    function onSetCustomFilter(e, params) {
        var isChange = false;
        if (isCustomFilterInit === false) {
            isCustomFilterInit = true;
        } else {
            switch (params.id) {
                case 'sorter':
                    // ToDo: refactore
                    // filter bug workaround - lowercase n make sence
                    if ('displayname' == params.params.id) {
                        return;
                    }
                    if (filter.CustomSortBy != params.params.id || filter.CustomSortOrder != params.params.sortOrder) {
                        filter.CustomSortBy = params.params.id;
                        filter.CustomSortOrder = params.params.sortOrder;
                        isChange = true;
                    }
                    break;
                case 'text':
                    if (filter.Search != params.params.value) {
                        filter.Search = params.params.value;
                        isChange = true;
                    }
                    break;
                case 'personal':
                case 'auto':
                    if (filter.ContactType != params.params.value) {
                        filter.ContactType = params.params.value;
                        isChange = true;
                    }
                    break;
            }
            if (isChange) {
                ASC.Controls.AnchorController.move(getAnchorByType(filter.ContactsStore) + toAnchor());

                //google analytics
                window.ASC.Mail.ga_track(ga_Categories.teamlabContacts, ga_Actions.filterClick, params.id);
            }
        }

    }

    function onResetFilter(e, params) {
        switch (params.id) {
            case 'text':
                filter.Search = '';
                break;
            case 'company':
            case 'person':
            case 'withopportunity':
                filter.ContactListView = '';
                break;
            case 'stages':
                filter.ContactStage = undefined;
                break;
            case 'tags':
                filter.Tags = [];
                break;
            case 'group':
                filter.FilterValue = '';
                break;
            case 'personal':
            case 'auto':
                filter.ContactType = '';
                break;
        }

        var anchor = ASC.Controls.AnchorController.getAnchor();
        var newAnchor = getAnchorByType(filter.ContactsStore) + toAnchor();
        if (anchor != newAnchor)
            ASC.Controls.AnchorController.move(newAnchor);
    }

    function onResetAllFilter() {
        filter.ContactListView = '';
        filter.ContactStage = undefined;
        filter.Tags = [];
        filter.Search = '';
        filter.FilterValue = '';
        filter.ContactType = '';

        var anchor = ASC.Controls.AnchorController.getAnchor();
        var newAnchor = getAnchorByType(filter.ContactsStore) + toAnchor();
        if (anchor != newAnchor)
            ASC.Controls.AnchorController.move(newAnchor);

    }

    function onFilterReady() {
        doResetFilter();
        tlFilter.events.unbind('ready');
        crmFilter.events.unbind('ready');
        customFilter.events.unbind('ready');
    }

    function redrawPage() {
        mailBox.hidePages();
        mailBox.hideContentDivs();
        mailBox.unmarkAllPanels();
        blankPages.hide();
        contactsPanel.selectContact(filter.ContactsStore);
        pageActionContainer.empty();
        page.find('#ContactsList').remove();

        switch (filter.ContactsStore) {
            case 'crm':
                tlFilter.hide();
                customFilter.hide();
                crmFilter.show();
                break;
            case 'teamlab':
                crmFilter.hide();
                customFilter.hide();
                tlFilter.show();
                break;
            case 'custom':
                tlFilter.hide();
                crmFilter.hide();
                customFilter.show();
                break;
        }
    }

    function onGetTlContacts(params, contacts) {
        var isTitelEmpty = true;
        $.each(contacts, function (index, value) {
            value.name = value.displayName;
            value.emails = [];
            value.phones = [];
            if (value.email != undefined && value.email != '') {
                value.emails.push({ isPrimary: true, email: value.email });
            }

            if (value.tel != undefined && value.tel != '') {
                value.phones.push({ isPrimary: true, phone: value.tel });
            }

            if (value.contacts) {
                $.each(value.contacts.mailboxes, function(i, v) {
                    if (value.email != v.val) {
                        value.emails.push({ isPrimary: false, email: v.val });
                    }
                });

                $.each(value.contacts.telephones, function (i, v) {
                    if (value.tel != v.val) {
                        value.phones.push({ isPrimary: false, phone: v.val });
                    }
                });
            }
            if (value.title != '') {
                isTitelEmpty = false;
            }

            value.displayName = Encoder.htmlDecode(value.displayName);
        });

        onGetContacts(params, { contacts: contacts, emptyLabel: true, emptyTitel: isTitelEmpty });
    }

    function onGetCrmContacts(params, contacts) {
        var isLabelEmpty = true,
            isTitelEmpty = true;
        $.each(contacts, function (index, value) {
            value.name = value.displayName;
            value.emails = [];
            value.phones = [];
            $.each(value.commonData, function(i, v) {
                if (1 == v.infoType) {
                    value.emails.push({ isPrimary: v.isPrimary, email: v.data });
                }

                if (0 == v.infoType) {
                    value.phones.push({ isPrimary: v.isPrimary, phone: v.data });
                }
            });
            if (value.title != '') {
                isTitelEmpty = false;
            }
            if (value.tags && value.tags.length != 0) {
                isLabelEmpty = false;
            }
        });

        onGetContacts(params, { contacts: contacts, emptyLabel: isLabelEmpty, emptyTitel: isTitelEmpty });
    }

    function convertServerMailContact(serverContact) {
        var contact = {};
        contact.id = serverContact.id;
        contact.title = serverContact.description;
        contact.name = serverContact.name;
        contact.smallFotoUrl = serverContact.smallFotoUrl;
        contact.type = (serverContact.type == 0)? "auto" : "personal";
        contact.emails = [];
        contact.phones = [];

        $.each(serverContact.emails, function (i, v) {
            contact.emails.push({ isPrimary: v.isPrimary, email: v.value });
        });

        $.each(serverContact.phones, function (i, v) {
            contact.phones.push({ isPrimary: v.isPrimary, phone: v.value });
        });

        contact.displayName = contact.name == '' ? contact.emails[0].email.split('@')[0] : contact.name;

        return contact;
    }

    function onGetMailContacts(params, serverContacts) {
        var isTitelEmpty = true;
        var contacts = $.map(serverContacts, convertServerMailContact);
        for (var i = 0;  i < contacts.length; i++) {
            if (contacts[i].title != '') {
                isTitelEmpty = false;
                break;
            }
        }

        onGetContacts(params, { contacts: contacts, emptyLabel: true, emptyTitel: isTitelEmpty });
    }

    function onGetContacts(params, data) {
        redrawPage();

        if (data.contacts.length == 0) {
            showEmptyScreen();
        } else {
            showContacts(params, data);
        }
        page.show(0, onPageShow);

        LoadingBanner.hideLoading();
        mailBox.hideLoadingMask();
    }

    function showEmptyScreen() {
        page.find('.containerBodyBlock').toggleClass('hidden', true);
        pageActionContainer.empty();
        $('#bottomNavigationBar').hide();
        switch(filter.ContactsStore) {
            case 'crm':
                if (isFilterEmpty()) {
                    crmFilter.hide();
                    blankPages.showEmptyCrmContacts();
                } else {
                    blankPages.showNoCrmContacts();
                }
                break;
            case 'teamlab':
                blankPages.showNoTlContacts();
                break;
            case 'custom':
                if (isFilterEmpty()) {
                    customFilter.hide();
                    blankPages.showEmptyMailContacts();
                } else {
                    blankPages.showNoMailContacts();
                }
                break;
        }
    }

    function showContacts(params, data) {

        var contactsActionsHtml = $.tmpl("contactsActionsTmpl", {}, { htmlEncode: TMMail.htmlEncode });
        pageActionContainer.append(contactsActionsHtml);

        var contactListHtml = $.tmpl("contactsTmpl", { contacts: data.contacts }, { htmlEncode: TMMail.htmlEncode });
        page.find('.containerBodyBlock').append(contactListHtml);
        page.find('#ContactsList .contact_avatar').each(function () {
            loadContactFoto(jq(this), jq(this).attr("data-src"));
        });

        $('#id_contacts_page').actionMenu('contactActionMenu', buttons, pretreatment);

        createSelectActionPandel();

        pageActionContainer.find('#SelectAllContactsCB').bind('click', function (e) {
            if (e.target.checked) {
                actionPanelSelectAll();
            } else {
                actionPanelSelectNone();
            }
            e.stopPropagation();
            $('#SelectAllContactsDropdown').parent().actionPanel('hide');
        });

        pageActionContainer.find('.menuActionSendEmail').click(function () {
            if ($(this).hasClass('unlockAction')) {
                massMailing();

                var category = ga_Categories.crmContacts;
                if (TMMail.pageIs('tlContact')) {
                    category = ga_Categories.teamlabContacts;
                }
                else if (TMMail.pageIs('personalContact')) {
                    category = ga_Categories.personalContacts;
                }
                window.ASC.Mail.ga_track(category, ga_Actions.buttonClick, "write_letter");
            }
        });

        if (TMMail.pageIs('personalContact')) {
            pageActionContainer.find('.menuActionDelete').click(function () {
                if (!$(this).hasClass('unlockAction')) {
                    return false;
                }

                deleteContacts();
            });
            pageActionContainer.find('.menuActionCreate').click(function () {
                editContactModal.show(null, true);
            });
        } else {
            pageActionContainer.find('.menuActionDelete').hide();
            pageActionContainer.find('.menuActionCreate').hide();
        }

        // _Selection checkbox clicked
        page.find('#ContactsList .row > .checkbox').unbind('click').bind('click', onClickCheckbox);

        var $rows = page.find('#ContactsList .row');
        prepareContactcInfo($rows, data);

        totalCount = params.__total || data.contacts.length;
        redrawNavigation(params.Page, totalCount, params.ContactsStore);

        page.find('.containerBodyBlock').toggleClass('hidden', false);
    }

    function onClickCheckbox()
    {
        var $this = $(this);
        if ($this.hasClass('disable')) {
            return false;
        }

        var row = $this.parent();
        selectRow(row);
    }

    function selectRow(row)
    {
        var $input = row.find('input');

        if ($input.hasClass('disable')) {
            return true;
        }

        var contactId = $input.attr('data_id');
        if (row.is('.selected')) {
            selection.RemoveId(contactId);
            $input.prop('checked', false);
        } else {
            selection.AddId(contactId, getContactFromRow($(row)));
            $input.prop('checked', true);
        }
        row.toggleClass('selected');
        updateSelectionComboCheckbox();
        commonButtonsState();
    };

    function prepareContactcInfo($rows, data) {
        for (var j = 0, k = $rows.length; j < k; j++) {
            var $row = $($rows[j]),
                $emails = $row.find('.email[isprimary="true"]'),
                $phones = $row.find('.phone[isprimary="true"]');

            if ($emails.length > 0) {
                var primaryEmail = $($emails[0]);
                primaryEmail.show();

                primaryEmail.find('span').bind('click', function(event) {
                    writeLetter(event, { name: $(event.target).text(), contact_name: $(event.target).attr('contactName') });
                });
            }

            if ($row.find('.email').length == 1) {
                $row.find('.emails').addClass('oneEmail');
            }

            var $more = $row.find('.emails .more_lnk');
            // async action panel initialization - only after click on "more" element
            $more.find('.gray').unbind('.contactsPage').bind('click.contactsPage', function() {
                // action panel need to be initialized just once - so imidiatly unbind
                $(this).unbind('.contactsPage');
                // add action panel with more emails
                itemListActionPanel((this.parentElement).parentElement, "email");
            });


            if ($phones.length > 0) {
                $($phones[0]).show();
            }

            if ($row.find('.phone').length == 1) {
                $row.find('.phones').addClass('onePhone');
            }

            $more = $row.find('.phones .more_lnk');
            // async action panel initialization - only after click on "more" element
            $more.find('.gray').unbind('.contactsPage').bind('click.contactsPage', function() {
                // action panel need to be initialized just once - so imidiatly unbind
                $(this).unbind('.contactsPage');
                // add action panel with more emails
                itemListActionPanel((this.parentElement).parentElement, "phone");
            });

            if (data.emptyLabel) {
                $row.find('.labels').remove();
            }
            if (data.emptyTitel) {
                $row.find('.title').remove();
            }
        }
    }

    function onCreateContact(params, serverContact) {
        if (TMMail.pageIs('personalContact')) {
            getContacts('custom');
        }
        window.toastr.success(window.MailActionCompleteResource.AddContactSuccess);
    }

    function onUpdateContact(params, serverContact) {
        var contact = convertServerMailContact(serverContact);

        var $row = $('#ContactsList').find('.row[data_id="' + contact.id + '"]');
        var name = getNameFromRow($row);
        var displayName = getDisplayNameFromRow($row);

        if (contact.name == name && contact.displayName == displayName) {

            var contactListHtml = $.tmpl("contactItemTmpl", contact, { htmlEncode: TMMail.htmlEncode });

            contactListHtml.find('.contact_avatar').each(function() {
                loadContactFoto(jq(this), jq(this).attr("data-src"));
            });

            contactListHtml.actionMenu('contactActionMenu', buttons, pretreatment);

            $('#ContactsList').find('.row[data_id="' + contact.id + '"]').replaceWith(contactListHtml);
            $row = $('#ContactsList').find('.row[data_id="' + contact.id + '"]');
            $row.find('.checkbox').unbind('click').bind('click', onClickCheckbox);

            prepareContactcInfo($row, contact);
        } else {
            getContacts('custom');
        }

        window.toastr.success(window.MailActionCompleteResource.EditContactSuccess);
    }

    function onPageShow() {
        page.find('#ContactsList .row').each(function(index, value) {
            createTagsHidePanel($(value));
        });

        switch (filter.ContactsStore) {
            case 'crm':
                crmFilter.update();
                break;
            case 'teamlab':
                tlFilter.update();
                break;
            case 'custom':
                customFilter.update();
                break;
        }

        updateSelectionView();
        commonButtonsState();
    }

    function createSelectActionPandel() {
        if (filter.ContactsStore == 'crm') {
            $('#SelectAllContactsDropdown').parent().actionPanel({
                buttons: [
                    { text: window.MailScriptResource.AllLabel, handler: actionPanelSelectAll },
                    { text: window.MailScriptResource.WithTags, handler: actionPanelSelectWithTags },
                    { text: window.MailScriptResource.WithoutTags, handler: actionPanelSelectWithoutTags },
                    { text: window.MailScriptResource.NoneLabel, handler: actionPanelSelectNone }
                ],
                css: 'stick-over'
            });
        } else {
            $('#SelectAllContactsDropdown').parent().actionPanel({
                buttons: [
                    { text: window.MailScriptResource.AllLabel, handler: actionPanelSelectAll },
                    { text: window.MailScriptResource.NoneLabel, handler: actionPanelSelectNone }
                ],
                css: 'stick-over'
            });
        }

    }

    // Initializes action panel on more emails element
    function itemListActionPanel(items, item_class_name) {
        var btns = [];
        var itemArray = $(items).find('.' + item_class_name);
        for (var i = 0, n = itemArray.length; i < n; i++) {
            if (!$(itemArray[i]).is(':visible')) {
                var $itemSpan = $(itemArray[i]).find('span');
                if (item_class_name == 'email') {
                    btns.push({
                        'text': $itemSpan.text(),
                        handler: writeLetter,
                        name: $itemSpan.text(),
                        contact_name: $itemSpan.attr('contactName')
                    });
                } else {
                    btns.push({
                        'text': $itemSpan.text(),
                        'disabled': true
                    });
                }
            }
        }

        $(items).find('.gray').actionPanel({ 'buttons': btns }).click();
    }

    function setDefaultValueFilter() {
        setPageInfo(1, TMMail.option('ContactsPageSize'));
        filter.ContactsStore = '';
        filter.CrmSortBy = 'displayname';
        filter.CrmSortOrder = 'ascending';
        filter.TlSortBy = ASC.Mail.Master.userDisplayFormat == 1 ? 'firstname' : 'lastname';
        filter.TlSortOrder = 'ascending';
        filter.CustomSortBy = 'displayName';
        filter.CustomSortOrder = 'ascending';
        filter.ContactListView = '';
        filter.ContactStage = undefined;
        filter.Tags = [];
        filter.Search = '';
        filter.FilterValue = '';
        filter.ContactType = '';
    }

    function redrawNavigation(pageParam, totalItemsCount, contactsType) {
        var onChangePageSize = function(pageSize) {
            if (isNaN(pageSize) || pageSize < 1) {
                return;
            }
            TMMail.option('ContactsPageSize', pageSize);
            keepSelectionOnReload = true;
            updateAnchor(contactsType, pageParam);
        };
        
        var onChangePage = function(changedPage) {
            if (isNaN(changedPage) || changedPage < 1) {
                return;
            }
            keepSelectionOnReload = true;
            updateAnchor(contactsType, changedPage);
        };

        PagesNavigation.RedrawNavigationBar(window.mailPageNavigator,
            pageParam, TMMail.option('ContactsPageSize'), totalItemsCount, onChangePage, onChangePageSize,
            window.MailScriptResource.TotalContacts);
        PagesNavigation.FixAnchorPageNumberIfNecessary(pageParam);
        PagesNavigation.RedrawPrevNextControl();
        overallDeselectAll();
    }

    function show(type) {
        setDefaultValueFilter();

        var anchor = ASC.Controls.AnchorController.getAnchor();
        fromAnchor(anchor);
        filter.ContactsStore = type;

        if (isFilterEmpty() || filter.ContactsStore=='custom') {
            doResetFilter();
        }

        // checks weather page size value in anchor is correct - replace anchor if not
        if (PagesNavigation.FixAnchorPageSizeIfNecessary(filter.Count)) {
            return;
        }

        if (!keepSelectionOnReload) {
            selection.Clear();
        } else {
            keepSelectionOnReload = false;
        }

        getContacts(type);
    }

    function actionPanelSelectAll() {

        var category = ga_Categories.crmContacts;
        if (TMMail.pageIs('tlContact')) {
            category = ga_Categories.teamlabContacts;
        }
        else if (TMMail.pageIs('personalContact')) {
            category = ga_Categories.personalContacts;
        }
        window.ASC.Mail.ga_track(category, ga_Actions.actionClick, "all_select");

        $('#ContactsList .row').each(function() {
            if ($(this).find('input[type="checkbox"]').hasClass('disable')) {
                return true;
            }
            selection.AddId($(this).attr('data_id'), getContactFromRow($(this)));
        });

        updateSelectionView();
        commonButtonsState();
    }

    function actionPanelSelectNone() {

        var category = ga_Categories.crmContacts;
        if (TMMail.pageIs('tlContact')) {
            category = ga_Categories.teamlabContacts;
        }
        else if (TMMail.pageIs('personalContact')) {
            category = ga_Categories.personalContacts;
        }
        window.ASC.Mail.ga_track(category, ga_Actions.actionClick, "none_select");

        clearCurrentPageSelection();

        updateSelectionView();
        commonButtonsState();
    }

    function commonButtonsState() {
        if (isContainEmails()) {
            $('.contentMenuWrapper:visible .menuAction').addClass('unlockAction');
        } else {
            $('.contentMenuWrapper:visible .menuAction:not(.menuActionCreate)').removeClass('unlockAction');
        }
    }

    function isFilterEmpty() {
        var result = false;
        if (!filter.Tags.length && filter.ContactStage == undefined && filter.Search == ''
            && filter.FilterValue == '' && filter.ContactListView == '' && filter.ContactType == '') {
            result = true;
        }
        return result;
    }

    function doResetFilter() {

        switch (filter.ContactsStore) {
            case 'crm':
                crmFilter.setSort(filter.CrmSortBy, filter.CrmSortOrder);
                if (isFilterEmpty()) {
                    crmFilter.clear();
                }
                if (filter.Search != '') {
                    crmFilter.setSearch(filter.Search);
                }
                break;
            case 'teamlab':
                tlFilter.setSort(filter.TlSortBy, filter.TlSortOrder);
                if (isFilterEmpty()) {
                    tlFilter.clear();
                }
                if (filter.Search != '') {
                    tlFilter.setSearch(filter.Search);
                }
                break;
            case 'custom':
                customFilter.setSort(filter.CustomSortBy, filter.CustomSortOrder);
                if (isFilterEmpty()) {
                    customFilter.clear();
                }
                if (filter.Search != '') {
                    customFilter.setSearch(filter.Search);
                }
                break;
        }

        switch (filter.ContactListView) {
            case 'company':
                crmFilter.setCompany();
                break;
            case 'person':
                crmFilter.setPerson();
                break;
            case 'withopportunity':
                crmFilter.setOpportunity();
                break;
        }

        if (filter.Tags.length) {
            crmFilter.setTags(filter.Tags);
        }

        if (filter.ContactStage != undefined) {
            crmFilter.setStage(filter.ContactStage);
        }

        if (filter.FilterValue != '') {
            tlFilter.setGroup(filter.FilterValue);
        }
    }

    function actionPanelSelectWithTags() {

        window.ASC.Mail.ga_track(ga_Categories.crmContacts, ga_Actions.actionClick, "whith_tag_select");

        clearCurrentPageSelection();
        $('#ContactsList .row').each(function() {
            var $row = $(this);
            if ($row.find('.tag').length > 0) {
                selection.AddId($row.attr('data_id'), getContactFromRow($row));
            }
        });
        updateSelectionView();
        commonButtonsState();
    }

    function actionPanelSelectWithoutTags() {

        window.ASC.Mail.ga_track(ga_Categories.crmContacts, ga_Actions.actionClick, "without_select");

        clearCurrentPageSelection();
        $('#ContactsList .row').each(function() {
            var $row = $(this);
            if ($row.find('.tag').length == 0) {
                selection.AddId($row.attr('data_id'), getContactFromRow($row));
            }
        });
        updateSelectionView();
        commonButtonsState();
    }

    function pretreatment(id) {
        var $row = page.find('.row[data_id="' + id + '"]');

        if (!selection.HasId(id)) {
            overallDeselectAll();
            selectRow($row);
        }

        var contactIds = selection.GetIds();
        var emails = [];
        for (var i = 0; i < contactIds.length; i++) {
            emails = page.find('.row[data_id="' + contactIds[i] + '"] .email[isprimary="true"]');
            if (emails.length > 0)
                break;
        };

        var contactActionMenu = $("#contactActionMenu"),
            writeLetter = contactActionMenu.find(".writeLetter"),
            viewContact = contactActionMenu.find(".viewContact"),
            editContact = contactActionMenu.find(".editContact"),
            deleteContact = contactActionMenu.find(".deleteContact");

        if (emails.length > 0) {
            writeLetter.show();
        } else {
            writeLetter.hide();
        }

        switch (filter.ContactsStore) {
        case 'custom':
            viewContact.hide();
            deleteContact.show();
            if (contactIds.length > 1) {
                editContact.hide();
            } else {
                editContact.show();
            }
            break;
        case 'crm':
        case 'teamlab':
            if (contactIds.length > 1) {
                viewContact.hide();
            } else {
                viewContact.show();
            }
            editContact.hide();
            deleteContact.hide();
            break;
        }
    }

    function viewContact(id) {
        if (filter.ContactsStore == 'crm') {
            window.open('../../Products/CRM/Default.aspx?id=' + id, "_blank");
        } else if (filter.ContactsStore == 'teamlab') {
            window.open('../../Products/People/Profile.aspx?user=' + id, "_blank");
        }
    }

    function getContactInfo(id) {
        var contact = {};
        var $row = $('#ContactsList').find('.row[data_id="' + id + '"]');

        contact.id = id;
        contact.name = $row.find('.name').attr('contactName').trim();
        contact.type = $row.attr('type');

        var title = $row.find('.title').html();
        if (title != undefined)
            contact.description = title.trim();
        else
            contact.description = "";

        contact.emails = [];
        var emails = $row.find('.email');
        var i, len;
        for (i = 0, len = emails.length; i < len; i++) {
            var email = {};
            email.id = -1;
            email.isPrimary = $(emails[i]).attr('isprimary');
            email.value = $(emails[i]).find('.contactEmail').html().trim();
            contact.emails.push(email);
        }

        contact.phones = [];
        var phones = $row.find('.phone');
        for (i = 0, len = phones.length; i < len; i++) {
            var phone = {};
            phone.id = -1;
            phone.isPrimary = $(phones[i]).attr('isprimary');
            phone.value = $(phones[i]).find('.contactPhone').html().trim();
            contact.phones.push(phone);
        }

        return contact;
    }

    function editContact(id) {
        var contact = getContactInfo(id);
        editContactModal.show(contact, false);
    }

    function deleteContacts() {
        var ids = [];

        selection.Each(function (id) {
            ids.push(id);
        });

        if (!ids.length)
            return;

        var question = window.MailResource.DeleteContactsShure;
        var attention = window.MailResource.DeleteContactsAttention;

        if (ids.length === 1) {
            var contact = getContactInfo(ids[0]);
            question = window.MailResource.DeleteContactShure
                .format(contact.name || (!contact.emails[0] ? null : contact.emails[0].value) || (!contact.phones[0] ? null : contact.phones[0].value) || contact.description);
            attention = window.MailResource.DeleteContactAttention;
        }

        var body = $.tmpl('questionBoxTmpl', {
            attentionText: attention,
            questionText: question
        });

        body.find('.button.remove').unbind('click').bind('click', function () {
            serviceManager.deleteMailContacts(ids, {}, {
                success: function (e, contactIds) {
                    if (totalCount <= filter.Count) {
                        totalCount = totalCount - contactIds.length;
                        $('#totalItemsOnAllPages').html(totalCount);
                        if (totalCount === 0) {
                            showEmptyScreen();
                        } else {
                            for (var i = 0, len = contactIds.length; i < len; i++) {
                                $('#ContactsList').find('.row[data_id="' + contactIds[i] + '"]').remove();
                                selection.RemoveId(contactIds[i]);
                            }
                        }
                    } else {
                        getContacts('custom');
                        for (var i = 0, len = contactIds.length; i < len; i++) {
                            selection.RemoveId(contactIds[i]);
                        }
                    }

                    updateSelectionComboCheckbox();
                    commonButtonsState();

                    if (ids.length > 1)
                        window.toastr.success(window.MailActionCompleteResource.DeleteManyContacts.replace('%count%', ids.length));
                    else window.toastr.success(window.MailActionCompleteResource.DeleteOneContact);
                },
                error: function (e, error) {
                    window.toastr.error(window.MailApiErrorsResource.ErrorDeleteContact);
                    console.error(error);
                }
            }, ASC.Resources.Master.Resource.LoadingProcessing);
            popup.hide();
        });

        popup.addBig(window.MailResource.DeleteContacts, body);
    }

    function writeLetter(event, buttonContext) {
        messagePage.setToEmailAddresses([getContact(buttonContext.name, buttonContext.contact_name)]);
        messagePage.composeTo();
    }

    function massMailing() {

        var emails = new TMContainers.StringSet();

        selection.Each(function(id, email) {
            if (email) {
                emails.Add(email);
            }
        });

        if (emails.Count() > 0) {
            messagePage.setToEmailAddresses(emails.GetValues());
            messagePage.composeTo();
        }
    }

    function isContainEmails() {
        var haveEmails = false;
        selection.Each(function(id, email) {
            if (email) {
                haveEmails = true;
                return false;
            }
        });
        return haveEmails;
    }

    function createTagsHidePanel(row) {
        var $smalstags = row.find('.tag');
        var $labels = row.find('.labels');
        var labelNames = [];
        for (var i = 0; i < $smalstags.length; i++) {
            var $tag = $($smalstags[i]);
            labelNames.push(TMMail.htmlEncode($tag.text()));
            $tag.remove();
        }
        if (labelNames.length > 0) {
            $labels.hidePanel({ 'items': labelNames, 'item_to_html': labelsToHtml });
        }
    }

    function labelsToHtml(name) {
        var tag = tagsManager.getTagByName(TMMail.htmlDecode(name));
        var html = $.tmpl("contactTagTmpl", tag, { htmlEncode: TMMail.htmlEncode });
        return html;
    }

    function hide() {
        if (!(TMMail.pageIs('crmContact') && filter.ContactsStore == 'crm') &&
            !(TMMail.pageIs('tlContact') && filter.ContactsStore == 'teamlab') &&
            !(TMMail.pageIs('personalContact') && filter.ContactsStore == 'custom')) {
            page.hide();
        }
        pageActionContainer.empty();
    }

    function toAnchor(pageParam) {
        var res = '/';

        switch(filter.ContactsStore) {
            case 'crm':
                if (filter.CrmSortBy != '') {
                    res += 'sortBy=' + filter.CrmSortBy + '/';
                }
                if (filter.CrmSortOrder != '') {
                    res += 'sortOrder=' + filter.CrmSortOrder + '/';
                }
                break;
            case 'teamlab':
                if (filter.TlSortBy != '') {
                    res += 'sortBy=' + filter.TlSortBy + '/';
                }
                if (filter.TlSortOrder != '') {
                    res += 'sortOrder=' + filter.TlSortOrder + '/';
                }
                break;
            case 'custom':
                if (filter.CustomSortBy != '') {
                    res += 'sortBy=' + filter.CustomSortBy + '/';
                }
                if (filter.CustomSortOrder != '') {
                    res += 'sortOrder=' + filter.CustomSortOrder + '/';
                }
                break;
        }

        if (filter.ContactListView != '') {
            res += 'contactListView=' + filter.ContactListView + '/';
        }

        if (filter.ContactStage != undefined) {
            res += 'contactStage=' + filter.ContactStage + '/';
        }

        if (filter.Search != '') {
            res += 'search=' + encodeURIComponent(filter.Search) + '/';
        }

        if (filter.FilterValue != '') {
            res += 'filterValue=' + encodeURIComponent(filter.FilterValue) + '/';
        }

        for (var i = 0; i < filter.Tags.length; i++) {
            res += 'tags=' + encodeURIComponent(filter.Tags[i]) + '/';
        }

        if (filter.ContactType != '') {
            res += 'contactType=' + filter.ContactType + '/';
        }

        if (pageParam !== undefined) {
            var pageSize = TMMail.option('ContactsPageSize');
            if (filter.Count != pageSize) {
                pageParam = 1;
            }
            res += 'page=' + pageParam + '/';
            res += 'page_size=' + pageSize + '/';
        }
        return res;
    }

    function fromAnchor(params) {

        var sortBy, sortOrder, filterValue, contactStage, pageParam, pageSize, contactListView, tag, search, contactType;

        filter.Tags = [];

        if (typeof params !== 'undefined') {
            sortBy = TMMail.getParamsValue(params, /sortBy=([^\/]+)/);
            sortOrder = TMMail.getParamsValue(params, /sortOrder=([^\/]+)/);
            contactListView = TMMail.getParamsValue(params, /contactListView=([^\/]+)/);
            contactStage = TMMail.getParamsValue(params, /contactStage=([^\/]+)/);
            search = TMMail.getParamsValue(params, /search=([^\/]+)/);
            pageParam = TMMail.getParamsValue(params, /page=(\d+)/);
            pageSize = TMMail.getParamsValue(params, /page_size=(\d+)/);
            filterValue = TMMail.getParamsValue(params, /filterValue=([^\/]+)/);
            tag = TMMail.getParamsValue(params, /tags=([^\/]+)/);
            contactType = TMMail.getParamsValue(params, /contactType=([^\/]+)/);
            while (tag != undefined) {

                filter.Tags.push(decodeURIComponent(tag));
                var str = '/tags=' + tag;
                var startIndex = params.indexOf(str) + str.length;
                params = params.slice(startIndex, params.length);

                tag = TMMail.getParamsValue(params, /tags=([^\/]+)/);
            }

        }

        if (TMMail.pageIs('crmContact')) {
            if (sortBy) {
                filter.CrmSortBy = sortBy;
            }
            if (sortOrder) {
                filter.CrmSortOrder = sortOrder;
            }
        } else if (TMMail.pageIs('tlContact')) {
            if (sortBy) {
                filter.TlSortBy = sortBy;
            }
            if (sortOrder) {
                filter.TlSortOrder = sortOrder;
            }
        } else {
            if (sortBy) {
                filter.CustomSortBy = sortBy;
            }
            if (sortOrder) {
                filter.CustomSortOrder = sortOrder;
            }
        }

        if (contactListView) {
            filter.ContactListView = contactListView;
        } else {
            filter.ContactListView = '';
        }

        if (contactStage) {
            filter.ContactStage = contactStage;
        } else {
            filter.ContactStage = undefined;
        }
        
        if (contactType) {
            filter.ContactType = contactType;
        } else {
            filter.ContactType = '';
        }

        if (search) {
            filter.Search = decodeURIComponent(search);
        } else {
            filter.FilterValue = '';
        }

        if (filterValue) {
            filter.FilterValue = decodeURIComponent(filterValue);
        }

        if (pageParam && pageSize) {
            TMMail.option('ContactsPageSize', pageSize);
            setPageInfo(pageParam, pageSize);
        } else {
            setPageInfo(1, TMMail.option('ContactsPageSize'));
        }
    }

    function getContacts(type) {
        var filterData = {};

        if ('crm' == type) {
            filterData.StartIndex = filter.StartIndex;
            filterData.Count = filter.Count;
            filterData.sortBy = filter.CrmSortBy;
            filterData.sortOrder = filter.CrmSortOrder;
            filterData.contactStage = -1;
            if ('' != filter.ContactListView) {
                filterData.contactListView = filter.ContactListView;
            }
            if (filter.Search != '') {
                filterData.filterValue = filter.Search;
            }
            if (filter.ContactStage) {
                filterData.contactStage = filter.ContactStage;
            }
            filterData.tags = filter.Tags;

            serviceManager.getCrmContacts({ Page: getPageFromFilter(), ContactsStore: filter.ContactsStore },
                { filter: filterData, success: onGetCrmContacts },
                ASC.Resources.Master.Resource.LoadingProcessing);

        } else if ('teamlab' == type) {
            filterData.StartIndex = filter.StartIndex;
            filterData.Count = filter.Count;
            filterData.sortBy = filter.TlSortBy;
            filterData.sortorder = filter.TlSortOrder;

            if (filter.FilterValue) {
                filterData.groupId = encodeURIComponent(filter.FilterValue);
            }

            if (filter.Search != '') {
                filterData.filtervalue = filter.Search;
            }

            filterData.fields = 'id,status,isAdmin,isOwner,isVisitor,activationStatus,contacts,userName,email,displayName,avatarSmall,birthday,title,location,isLDAP,isSSO';

            var options = { filter: filterData, success: onGetTlContacts };

            serviceManager.getProfilesByFilter({ Page: getPageFromFilter(), ContactsStore: filter.ContactsStore },
                options, ASC.Resources.Master.Resource.LoadingProcessing);
        } else if ('custom' == type) {
            filterData.sortorder = filter.CustomSortOrder;
            filterData.fromIndex = filter.StartIndex;
            filterData.pageSize = filter.Count;
            filterData.contactType = filter.ContactType;
            if (filter.Search != '') {
                filterData.search = filter.Search;
            }

            serviceManager.getMailContacts(filterData, { Page: getPageFromFilter(), ContactsStore: filter.ContactsStore },
                {success: onGetMailContacts }, ASC.Resources.Master.Resource.LoadingProcessing);
        }
    }

    function updateAnchor(type, pageParam) {
        ASC.Controls.AnchorController.move(getAnchorByType(type) + toAnchor(pageParam));
    }

    function setPageInfo(pageParam, pageSize) {
        filter.Count = pageSize;
        filter.StartIndex = pageSize * (pageParam - 1);
    }

    function getPageFromFilter() {
        return filter.StartIndex / TMMail.option('ContactsPageSize') + 1;
    }

    function getAnchorByType(type) {
        var anchor = '';
        switch (type) {
            case 'crm':
                anchor = 'crmcontact';
                break;
            case 'teamlab':
                anchor = 'tlcontact';
                break;
            case 'custom':
                anchor = 'customcontact';
                break;
        }
        return anchor;
    }

    function getContactFromRow($row) {
        var email = getEmailFromRow($row);
        var name = getNameFromRow($row);
        return getContact(email, name);
    }

    function getContact(email, name) {
        return email ? (new ASC.Mail.Address(name, email, true).ToString()) : undefined;
    }

    function getEmailFromRow($row) {
        var $email = $row.find('.email[isprimary="true"]');
        return $email.length > 0 ? $($email[0]).text().trim() : undefined;
    }

    function getNameFromRow($row) {
        return $row.find('.name').attr('contactName').trim();
    }

    function getDisplayNameFromRow($row) {
        return TMMail.htmlEncode($row.find('.name').text().trim());
    }

    function resetFilter() {
        ASC.Controls.AnchorController.move(getAnchorByType(filter.ContactsStore));
        doResetFilter();
    }

    function loadContactFoto(imgObj, handlerSrc) {
        if (handlerSrc.indexOf("filehandler.ashx") !== -1 || handlerSrc.indexOf("contactphoto.ashx") !== -1) {
            jq.ajax({
                type: "GET",
                url: handlerSrc,
                success: function (response) {
                    try {
                        imgObj.one("load", function () {
                            imgObj.prev().addClass("display-none").hide();
                            imgObj.removeClass("display-none");
                        });

                        imgObj.attr("src", [response, "?", new Date().getTime()].join(''));
                    }
                    catch (e) { }
                }
            });
        } else {
            imgObj.attr("src", handlerSrc);
            imgObj.prev().addClass("display-none").hide();
            imgObj.removeClass("display-none");
        }
    }

    function overallDeselectAll() {
        selection.Clear();
        updateSelectionView();
    }

    return {
        init: init,
        show: show,
        hide: hide,

        deselectAll: overallDeselectAll,
        selectRow: selectRow,

        resetFilter: resetFilter
    };
})(jQuery);