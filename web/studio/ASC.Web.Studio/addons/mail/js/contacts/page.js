/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


window.contactsPage = (function($) {
    var isInit = false,
        isCrmFilterInit = false,
        isTlFilterInit = false,
        filter = new Object,
        page,
        keepSelectionOnReload = false,
        buttons = [];

    var selection = new TMContainers.IdMap();

    var init = function() {
        if (isInit === false) {
            isInit = true;

            setDefaultValueFilter();

            crmFilter.events.bind('set', onSetCrmFilter);
            crmFilter.events.bind('reset', onResetFilter);
            crmFilter.events.bind('resetall', onResetAllFilter);
            tlFilter.events.bind('set', onSetTlFilter);
            tlFilter.events.bind('reset', onResetFilter);
            tlFilter.events.bind('resetall', onResetAllFilter);

            tlFilter.init();
            crmFilter.init();

            page = $('#id_contacts_page');

            tlFilter.events.bind('ready', onFilterReady);
            crmFilter.events.bind('ready', onFilterReady);

            buttons = [
                { selector: "#contactActionMenu .viewContact", handler: viewContact },
                { selector: "#contactActionMenu .writeLetter", handler: writeLetterById }];
        }
    };

    // Set checkbox states depending on ids from _Selection
    // Note: _updateSelectionComboCheckbox call don't needed
    var updateSelectionView = function() {
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
            } else {
                $checkbox.prop('checked', false);
                $rowDiv.removeClass('selected');
                haveOneUnchecked = true;
            }
        });
        setSelectionComboCheckbox(!haveOneUnchecked && !emptyOrDisabled);
    };

    var updateSelectionComboCheckbox = function() {
        // Update checked state
        var uncheckedFound = false;
        $('#ContactsList .row .checkbox input[type="checkbox"]').each(function() {
            if (!selection.HasId($(this).attr('data_id'))) {
                uncheckedFound = true;
                return false;
            }
        });
        setSelectionComboCheckbox(!uncheckedFound);
    };

    var setSelectionComboCheckbox = function(checked) {
        $('#SelectAllContactsCB').prop('checked', checked);
    };

    var clearCurrentPageSelection = function() {
        $('#ContactsList .row').each(function() {
            selection.RemoveId($(this).attr('data_id'));
        });
    };

    var onSetCrmFilter = function(e, params) {
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
    };

    var onSetTlFilter = function(e, params) {
        var isChange = false;
        if (isTlFilterInit === false) {
            isTlFilterInit = true;
        } else {
            switch (params.id) {
                case 'sorter':
                    // ToDo: refactore
                    // filter bug workaround - lowercase n make sence
                    if ('displayname' == params.params.id) {
                        return;
                    }
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

    };


    var onResetFilter = function(e, params) {
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
        }

        ASC.Controls.AnchorController.move(getAnchorByType(filter.ContactsStore) + toAnchor());
    };

    var onResetAllFilter = function() {
        filter.ContactListView = '';
        filter.ContactStage = undefined;
        filter.Tags = [];
        filter.Search = '';
        filter.FilterValue = '';

        ASC.Controls.AnchorController.move(getAnchorByType(filter.ContactsStore) + toAnchor());

    };

    var onFilterReady = function() {
        doResetFilter();
        tlFilter.events.unbind('ready');
        crmFilter.events.unbind('ready');
    };


    var redrawPage = function() {
        mailBox.hidePages();
        messagePage.hide();
        mailBox.hideContentDivs();
        mailBox.unmarkAllPanels();
        blankPages.hide();
        contactsPanel.selectContact(filter.ContactsStore);
        page.find('.contentMenuWrapper').remove();
        page.find('#ContactsList').remove();
        if (filter.ContactsStore == 'crm') {
            tlFilter.hide();
            crmFilter.show();
        } else {
            crmFilter.hide();
            tlFilter.show();
        }
    };

    var onGetTlContacts = function(params, contacts) {
        var isTitelEmpty = true;
        $.each(contacts, function(index, value) {
            value.emails = [];
            if (value.email != undefined && value.email != '') {
                value.emails.push({ isPrimary: true, email: value.email });
            }
            if (value.contacts) {
                $.each(value.contacts.mailboxes, function(i, v) {
                    if (value.email != v.val) {
                        value.emails.push({ isPrimary: false, email: v.val });
                    }
                });
            }
            if (value.title != '') {
                isTitelEmpty = false;
            }

            value.displayName = Encoder.htmlDecode(value.displayName);
        });

        onGetContacts(params, { contacts: contacts, emptyLabel: true, emptyTitel: isTitelEmpty });
    };

    var onGetCrmContacts = function(params, contacts) {
        var isLabelEmpty = true,
            isTitelEmpty = true;
        $.each(contacts, function(index, value) {
            value.emails = [];
            $.each(value.commonData, function(i, v) {
                if (1 == v.infoType) {
                    value.emails.push({ isPrimary: v.isPrimary, email: v.data });
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
    };

    var onGetContacts = function(params, data) {
        redrawPage();

        if (data.contacts.length == 0) {
            showEmptyScreen();
        } else {
            showContacts(params, data);
        }
        page.show(0, onPageShow);

        LoadingBanner.hideLoading();
        mailBox.hideLoadingMask();
    };

    var showEmptyScreen = function() {
        page.find('.containerBodyBlock').hide();
        if (filter.ContactsStore == 'crm') {
            if (isFilterEmpty()) {
                crmFilter.hide();
                blankPages.showEmptyCrmContacts();
            } else {
                blankPages.showNoCrmContacts();
            }
        } else {
            blankPages.showNoTlContacts();
        }
    };

    var showContacts = function(params, data) {
        var contactListHtml = $.tmpl("contactsTmpl", { contacts: data.contacts }, { htmlEncode: TMMail.htmlEncode });
        page.find('.containerBodyBlock').append(contactListHtml);

        $('#id_contacts_page').actionMenu('contactActionMenu', buttons, pretreatment);

        crateSelectActionPandel();

        page.find('#SelectAllContactsCB').bind('click', function(e) {
            if (e.target.checked) {
                actionPanelSelectAll();
            } else {
                actionPanelSelectNone();
            }
            e.stopPropagation();
            $('#SelectAllContactsDropdown').parent().actionPanel('hide');
        });

        page.find('.menuActionSendEmail').click(function() {
            if ($(this).hasClass('unlockAction')) {
                massMailing();

                var category = ga_Categories.crmContacts;
                if (TMMail.pageIs('tlcontact')) {
                    category = teamlabContacts;
                }
                window.ASC.Mail.ga_track(category, ga_Actions.buttonClick, "write_letter");
            }
        });

        // _Selection checkbox clicked
        page.find('#ContactsList .row > .checkbox').unbind('click').bind('click', function() {
            var $this = $(this);
            if ($this.hasClass('disable')) {
                return false;
            }

            var row = $this.parent();
            var $input = $this.find('input');
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
        });

        var $rows = page.find('#ContactsList .row');
        for (var j = 0, k = $rows.length; j < k; j++) {
            var $row = $($rows[j]);

            var $emails = $row.find('.email[isprimary="true"]');
            if ($emails.length > 0) {
                var primaryEmail = $($emails[0]);
                primaryEmail.show();

                primaryEmail.find('span').bind('click', function(event) {
                    writeLetter(event, { name: $(event.target).text(), contact_name: $(event.target).attr('contact_name') });
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
                emailListActionPanel((this.parentElement).parentElement);
            });

            if (data.emptyLabel) {
                $row.find('.labels').remove();
            }
            if (data.emptyTitel) {
                $row.find('.title').remove();
            }
        }

        redrawNavigation(params.Page, params.__total || data.contacts.length, params.ContactsStore);

        page.find('.containerBodyBlock').show();
    };

    var onPageShow = function() {
        page.find('#ContactsList .row').each(function(index, value) {
            createTagsHidePanel($(value));
        });
        if (filter.ContactsStore == 'crm') {
            crmFilter.update();
        } else {
            tlFilter.update();
        }
        updateSelectionView();
        commonButtonsState();
    };

    var crateSelectActionPandel = function() {
        if (filter.ContactsStore == 'crm') {
            page.find('#SelectAllContactsDropdown').parent().actionPanel({
                buttons: [
                    { text: window.MailScriptResource.AllLabel, handler: actionPanelSelectAll },
                    { text: window.MailScriptResource.WithTags, handler: actionPanelSelectWithTags },
                    { text: window.MailScriptResource.WithoutTags, handler: actionPanelSelectWithoutTags },
                    { text: window.MailScriptResource.NoneLabel, handler: actionPanelSelectNone }
                ],
                css: 'stick-over'
            });
        } else {
            page.find('#SelectAllContactsDropdown').parent().actionPanel({
                buttons: [
                    { text: window.MailScriptResource.AllLabel, handler: actionPanelSelectAll },
                    { text: window.MailScriptResource.NoneLabel, handler: actionPanelSelectNone }
                ],
                css: 'stick-over'
            });
        }

    };

    // Initializes action panel on more emails element
    var emailListActionPanel = function(emails) {
        var btns = [];
        var emailArray = $(emails).find('.email');
        for (var i = 0, n = emailArray.length; i < n; i++) {
            if (!$(emailArray[i]).is(':visible')) {
                var $emailSpan = $(emailArray[i]).find('span');
                btns.push({
                    'text': $emailSpan.text(),
                    handler: writeLetter,
                    name: $emailSpan.text(),
                    contact_name: $emailSpan.attr('contact_name')
                });
            }
        }
        $(emails).find('.gray').actionPanel({ 'buttons': btns }).click();
    };

    var setDefaultValueFilter = function() {
        setPageInfo(1, TMMail.option('ContactsPageSize'));
        filter.ContactsStore = '';
        filter.CrmSortBy = 'displayname';
        filter.CrmSortOrder = 'ascending';
        filter.TlSortBy = 'displayName';
        filter.TlSortOrder = 'ascending';
        filter.ContactListView = '';
        filter.ContactStage = undefined;
        filter.Tags = [];
        filter.Search = '';
        filter.FilterValue = '';
    };

    var redrawNavigation = function(pageParam, totalItemsCount, contactsType) {
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
    };

    var show = function(type) {
        setDefaultValueFilter();

        var anchor = ASC.Controls.AnchorController.getAnchor();
        fromAnchor(anchor);
        filter.ContactsStore = type;

        if (isFilterEmpty()) {
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
    };

    var actionPanelSelectAll = function() {

        var category = ga_Categories.crmContacts;
        if (TMMail.pageIs('tlcontact')) {
            category = teamlabContacts;
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
    };

    var actionPanelSelectNone = function() {

        var category = ga_Categories.crmContacts;
        if (TMMail.pageIs('tlcontact')) {
            category = teamlabContacts;
        }
        window.ASC.Mail.ga_track(category, ga_Actions.actionClick, "none_select");

        clearCurrentPageSelection();

        updateSelectionView();
        commonButtonsState();
    };

    var commonButtonsState = function() {
        if (isContainEmails()) {
            $('.contentMenuWrapper:visible .menuAction').addClass('unlockAction');
        } else {
            $('.contentMenuWrapper:visible .menuAction').removeClass('unlockAction');
        }
    };

    var isFilterEmpty = function() {
        var result = false;
        if (!filter.Tags.length && filter.ContactStage == undefined && filter.Search == ''
            && filter.FilterValue == '' && filter.ContactListView == '') {
            result = true;
        }
        return result;
    };

    var doResetFilter = function() {

        if (filter.ContactsStore == 'crm') {
            crmFilter.setSort(filter.CrmSortBy, filter.CrmSortOrder);
        } else {
            tlFilter.setSort(filter.TlSortBy, filter.TlSortOrder);
        }

        if (isFilterEmpty()) {
            if (filter.ContactsStore == 'crm') {
                crmFilter.clear();
            } else {
                tlFilter.clear();
            }
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

        if (filter.Search != '') {
            if (filter.ContactsStore == 'crm') {
                crmFilter.setSearch(filter.Search);
            } else {
                tlFilter.setSearch(filter.Search);
            }
        }

        if (filter.FilterValue != '') {
            tlFilter.setGroup(filter.FilterValue);
        }

    };

    var actionPanelSelectWithTags = function() {

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
    };

    var actionPanelSelectWithoutTags = function() {

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
    };

    var pretreatment = function(id) {
        var $emails = page.find('.row[data_id="' + id + '"] .email[isprimary="true"]');

        if ($emails.length > 0) {
            $("#contactActionMenu .writeLetter").show();
        } else {
            $("#contactActionMenu .writeLetter").hide();
        }
    };

    var viewContact = function(id) {
        if (filter.ContactsStore == 'crm') {
            window.open('../../products/crm/default.aspx?id=' + id, "_blank");
        } else if (filter.ContactsStore == 'teamlab') {
            window.open('../../products/people/profile.aspx?user=' + id, "_blank");
        }
    };


    var writeLetter = function(event, buttonContext) {
        messagePage.setToEmailAddresses([getContact(buttonContext.name, buttonContext.contact_name)]);
        messagePage.composeTo();
    };

    var writeLetterById = function(id) {
        var $row = page.find('#ContactsList .row[data_id="' + id + '"]');
        messagePage.setToEmailAddresses([getContactFromRow($row)]);
        messagePage.composeTo();
    };

    var massMailing = function() {

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
    };

    var isContainEmails = function() {
        var haveEmails = false;
        selection.Each(function(id, email) {
            if (email) {
                haveEmails = true;
                return false;
            }
        });
        return haveEmails;
    };

    var createTagsHidePanel = function(row) {
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
    };

    var labelsToHtml = function(name) {
        var tag = tagsManager.getTagByName(TMMail.htmlDecode(name));
        var html = $.tmpl("contactTagTmpl", tag, { htmlEncode: TMMail.htmlEncode });
        return html;
    };

    var hide = function() {
        if (!(TMMail.pageIs('crm') && filter.ContactsStore == 'crm') &&
            !(TMMail.pageIs('teamlab') && filter.ContactsStore == 'teamlab')) {
            page.hide();
        }
    };

    var toAnchor = function(pageParam) {
        var res = '/';

        if (filter.ContactsStore == 'crm') {
            if (filter.CrmSortBy != '') {
                res += 'sortBy=' + filter.CrmSortBy + '/';
            }
            if (filter.CrmSortOrder != '') {
                res += 'sortOrder=' + filter.CrmSortOrder + '/';
            }
        } else {
            if (filter.TlSortBy != '') {
                res += 'sortBy=' + filter.TlSortBy + '/';
            }
            if (filter.TlSortOrder != '') {
                res += 'sortOrder=' + filter.TlSortOrder + '/';
            }
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

        if (pageParam !== undefined) {
            var pageSize = TMMail.option('ContactsPageSize');
            if (filter.Count != pageSize) {
                pageParam = 1;
            }
            res += 'page=' + pageParam + '/';
            res += 'page_size=' + pageSize + '/';
        }
        return res;
    };

    var fromAnchor = function(params) {

        var sortBy, sortOrder, filterValue, contactType, pageParam, pageSize, contactListView, tag, search;

        filter.Tags = [];

        if (typeof params !== 'undefined') {
            sortBy = TMMail.getParamsValue(params, /sortBy=([^\/]+)/);
            sortOrder = TMMail.getParamsValue(params, /sortOrder=([^\/]+)/);
            contactListView = TMMail.getParamsValue(params, /contactListView=([^\/]+)/);
            contactType = TMMail.getParamsValue(params, /contactStage=([^\/]+)/);
            search = TMMail.getParamsValue(params, /search=([^\/]+)/);
            pageParam = TMMail.getParamsValue(params, /page=(\d+)/);
            pageSize = TMMail.getParamsValue(params, /page_size=(\d+)/);
            filterValue = TMMail.getParamsValue(params, /filterValue=([^\/]+)/);
            tag = TMMail.getParamsValue(params, /tags=([^\/]+)/);
            while (tag != undefined) {

                filter.Tags.push(decodeURIComponent(tag));
                var str = '/tags=' + tag;
                var startIndex = params.indexOf(str) + str.length;
                params = params.slice(startIndex, params.length);

                tag = TMMail.getParamsValue(params, /tags=([^\/]+)/);
            }

        }

        if (TMMail.pageIs('crm')) {
            if (sortBy) {
                filter.CrmSortBy = sortBy;
            }
            if (sortOrder) {
                filter.CrmSortOrder = sortOrder;
            }
        } else {
            if (sortBy) {
                filter.TlSortBy = sortBy;
            }
            if (sortOrder) {
                filter.TlSortOrder = sortOrder;
            }
        }

        if (contactListView) {
            filter.ContactListView = contactListView;
        } else {
            filter.ContactListView = '';
        }

        if (contactType) {
            filter.ContactStage = contactType;
        } else {
            filter.ContactStage = undefined;
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
    };

    var getContacts = function(type) {
        var filterData = {};
        filterData.StartIndex = filter.StartIndex;
        filterData.Count = filter.Count;

        if ('crm' == type) {

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

        } else {
            //('teamlab' == type)
            filterData.sortBy = filter.TlSortBy;
            filterData.sortorder = filter.TlSortOrder;

            if (filter.FilterValue) {
                filterData.groupId = encodeURIComponent(filter.FilterValue);
            }

            if (filter.Search != '') {
                filterData.filtervalue = filter.Search;
            }

            var options = { filter: filterData, success: onGetTlContacts };

            serviceManager.getProfilesByFilter({ Page: getPageFromFilter(), ContactsStore: filter.ContactsStore },
                options, ASC.Resources.Master.Resource.LoadingProcessing);
        }
    };

    var updateAnchor = function(type, pageParam) {
        ASC.Controls.AnchorController.move(getAnchorByType(type) + toAnchor(pageParam));
    };

    var setPageInfo = function(pageParam, pageSize) {
        filter.Count = pageSize;
        filter.StartIndex = pageSize * (pageParam - 1);
    };

    var getPageFromFilter = function() {
        return filter.StartIndex / TMMail.option('ContactsPageSize') + 1;
    };

    var getAnchorByType = function(type) {
        var anchor = '';
        switch (type) {
            case 'crm':
                anchor = 'crmcontact';
                break;
            case 'teamlab':
                anchor = 'tlcontact';
                break;
        }
        return anchor;
    };

    var getContactFromRow = function($row) {
        var email = getEmailFromRow($row);
        var name = getNameFromRow($row);
        return getContact(email, name);
    };

    var getContact = function(email, name) {
        var contact = undefined;
        if (email) {
            contact = '"' + name + '"';
            contact += ' <' + email + '>';
        }
        return contact;
    };

    var getEmailFromRow = function($row) {
        var $email = $row.find('.email[isprimary="true"]');
        return $email.length > 0 ? $($email[0]).text().trim() : undefined;
    };

    var getNameFromRow = function($row) {
        var $name = $row.find('.name');
        return $name.length > 0 ? $($name[0]).text().trim() : undefined;
    };

    var resetFilter = function() {
        ASC.Controls.AnchorController.move(getAnchorByType(filter.ContactsStore));
        doResetFilter();
    };


    return {
        init: init,
        show: show,
        hide: hide,
        resetFilter: resetFilter
    };

})(jQuery);