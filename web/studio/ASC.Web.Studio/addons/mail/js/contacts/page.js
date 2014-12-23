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

window.contactsPage = (function($) {
    var isInit = false,
        isCRMFilterInit = false,
        isTLFilterInit = false,
        filter = new Object,
        _page,
        _keep_selection_on_reload = false,
        buttons = [];


    var _Selection = new TMContainers.IdMap();

    var init = function() {
        if (isInit === false) {
            isInit = true;

            _setDefaultValueFilter();

            crmFilter.events.bind('set', _onSetCRMFilter);
            crmFilter.events.bind('reset', _onResetFilter);
            crmFilter.events.bind('resetall', _onResetAllFilter);
            tlFilter.events.bind('set', _onSetTLFilter);
            tlFilter.events.bind('reset', _onResetFilter);
            tlFilter.events.bind('resetall', _onResetAllFilter);

            tlFilter.init();
            crmFilter.init();

            _page = $('#id_contacts_page');

            tlFilter.events.bind('ready', _onFilterReady);
            crmFilter.events.bind('ready', _onFilterReady);

            buttons = [
                { selector: "#contactActionMenu .viewContact", handler: _viewContact },
                { selector: "#contactActionMenu .writeLetter", handler: _writeLetterById}];
        }
    };

    // Set checkbox states depending on ids from _Selection
    // Note: _updateSelectionComboCheckbox call don't needed
    var _updateSelectionView = function() {
        var have_one_unchecked = false;
        var empty_or_disabled = true;
        $('#ContactsList .row').each(function() {
            var $row_div = $(this);
            var contact_id = $row_div.attr('data_id');
            var $checkbox = $row_div.find('.checkbox input[type="checkbox"]');

            if ($checkbox.hasClass('disable'))
                return true;

            empty_or_disabled = false;

            if (_Selection.HasId(contact_id)) {
                $checkbox.prop('checked', true);
                $row_div.addClass('selected');
            }
            else {
                $checkbox.prop('checked', false);
                $row_div.removeClass('selected');
                have_one_unchecked = true;
            }
        });
        _setSelectionComboCheckbox(!have_one_unchecked && !empty_or_disabled);
    };

    var _updateSelectionComboCheckbox = function() {
        // Update checked state
        var unchecked_found = false;
        $('#ContactsList .row .checkbox input[type="checkbox"]').each(function() {
            if (!_Selection.HasId($(this).attr('data_id'))) {
                unchecked_found = true;
                return false;
            }
        });
        _setSelectionComboCheckbox(!unchecked_found);
    };

    var _setSelectionComboCheckbox = function(checked) {
        $('#SelectAllContactsCB').prop('checked', checked);
    };

    var _clearCurrentPageSelection = function() {
        $('#ContactsList .row').each(function() {
            _Selection.RemoveId($(this).attr('data_id'));
        });
    };

    var _onSetCRMFilter = function(e, params) {
        var isChange = false;
        if (isCRMFilterInit === false) {
            isCRMFilterInit = true;
        }
        else {
            switch (params.id) {
                case 'sorter':
                    // ToDo: refactore
                    // Filter is sensitive to uppercase N
                    if ('displayName' == params.params.id)
                        return;
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
                case 'types':
                    if (params.params.value == null) {
                        filter.ContactType = undefined;
                        isChange = true;
                    }
                    if (filter.ContactType != params.params.value) {
                        filter.ContactType = params.params.value;
                        isChange = true;
                    }
                    break;
                case 'tags':
                    if (params.params.value == null) {
                        filter.Tags = [];
                        isChange = true;
                    }
                    else if (filter.Tags.length != params.params.value.length) {
                        filter.Tags = [];
                        for (var i = 0; i < params.params.value.length; i++) {
                            filter.Tags.push(params.params.value[i]);
                        }
                        isChange = true;
                    }
                    break;
            }
            if (isChange) {
                ASC.Controls.AnchorController.move(_getAnchorByType(filter.ContactsStore) + _toAnchor());

                window.ASC.Mail.ga_track(ga_Categories.crmContacts, ga_Actions.filterClick, params.id);
            }
        }
    };

    var _onSetTLFilter = function(e, params) {
        var isChange = false;
        if (isTLFilterInit === false) {
            isTLFilterInit = true;
        }
        else {
            switch (params.id) {
                case 'sorter':
                    // ToDo: refactore
                    // filter bug workaround - lowercase n make sence
                    if ('displayname' == params.params.id)
                        return;
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
                ASC.Controls.AnchorController.move(_getAnchorByType(filter.ContactsStore) + _toAnchor());

                //google analytics
                window.ASC.Mail.ga_track(ga_Categories.teamlabContacts, ga_Actions.filterClick, params.id);
            }
        }

    };


    var _onResetFilter = function(e, params) {
        switch (params.id) {
            case 'text':
                filter.Search = '';
                break;
            case 'company':
            case 'person':
            case 'withopportunity':
                filter.ContactListView = '';
                break;
            case 'types':
                filter.ContactType = undefined;
                break;
            case 'tags':
                filter.Tags = [];
                break;
            case 'group':
                filter.FilterValue = '';
                break;
        }

        ASC.Controls.AnchorController.move(_getAnchorByType(filter.ContactsStore) + _toAnchor());
    };

    var _onResetAllFilter = function(e, params) {
        filter.ContactListView = '';
        filter.ContactType = undefined;
        filter.Tags = [];
        filter.Search = '';
        filter.FilterValue = '';

        ASC.Controls.AnchorController.move(_getAnchorByType(filter.ContactsStore) + _toAnchor());

    };

    var _onFilterReady = function(e, params) {
        _resetFilter();
        tlFilter.events.unbind('ready');
        crmFilter.events.unbind('ready');
    };


    var _redrawPage = function() {
        mailBox.hidePages();
        messagePage.hide();
        mailBox.hideContentDivs();
        mailBox.unmarkAllPanels();
        blankPages.hide();
        contactsPanel.selectContact(filter.ContactsStore);
        _page.find('.contentMenuWrapper').remove();
        _page.find('#ContactsList').remove();
        if (filter.ContactsStore == 'crm') {
            tlFilter.hide();
            crmFilter.show();
        }
        else {
            crmFilter.hide();
            tlFilter.show();
        }
    };

    var _onGetTlContacts = function (params, contacts) {
        var isLabelEmpty = true,
            isTitelEmpty = true;
        $.each(contacts, function(index, value) {
            value.emails = [];
            if (value.email != undefined && value.email != '')
                value.emails.push({ isPrimary: true, email: value.email });
            if (value.contacts) {
                $.each(value.contacts.mailboxes, function(i, v) {
                    if (value.email != v.val)
                        value.emails.push({ isPrimary: false, email: v.val });
                });
            }
            if (value.title != '') isTitelEmpty = false;

            value.displayName = Encoder.htmlDecode(value.displayName);
        });

        _onGetContacts(params, { contacts: contacts, emptyLabel: isLabelEmpty, emptyTitel: isTitelEmpty });
    };

    var _onGetCrmContacts = function (params, contacts) {
        var isLabelEmpty = true,
            isTitelEmpty = true;
        $.each(contacts, function(index, value) {
            value.emails = [];
            $.each(value.commonData, function(i, v) {
                if (1 == v.infoType)
                    value.emails.push({ isPrimary: v.isPrimary, email: v.data });
            });
            if (value.title != '') isTitelEmpty = false;
            if (value.tags && value.tags.length != 0) isLabelEmpty = false;
        });

        _onGetContacts(params, { contacts: contacts, emptyLabel: isLabelEmpty, emptyTitel: isTitelEmpty });
    };

    var _onGetContacts = function(params, data) {
        _redrawPage();

        if (data.contacts.length == 0) {
            _showEmptyScreen();
        } else {
            _showContacts(params, data);
        }
        _page.show(0, _onPageShow);

        LoadingBanner.hideLoading();
        mailBox.hideLoadingMask();
    };

    var _showEmptyScreen = function() {
        _page.find('.containerBodyBlock').hide();
        if (filter.ContactsStore == 'crm') {
            if (_isFilterEmpty()) {
                crmFilter.hide();
                blankPages.showEmptyCrmContacts();
            } else {
                blankPages.showNoCrmContacts();
            }
        } else {
            blankPages.showNoTlContacts();
        }
    };

    var _showContacts = function(params, data) {
        var contactListHTML = $.tmpl("contactsTmpl", { contacts: data.contacts }, { htmlEncode: TMMail.htmlEncode });
        _page.find('.containerBodyBlock').append(contactListHTML);

        $('#id_contacts_page').actionMenu('contactActionMenu', buttons, _pretreatment);

        _crateSelectActionPandel();

        _page.find('#SelectAllContactsCB').bind('click', function(e) {
            if (e.target.checked)
                _actionPanelSelectAll();
            else
                _actionPanelSelectNone();
            e.stopPropagation();
            $('#SelectAllContactsDropdown').parent().actionPanel('hide');
        });

        _page.find('.menuActionSendEmail').click(function() {
            if ($(this).hasClass('unlockAction')) {
                _massMailing();

                var category = ga_Categories.crmContacts;
                if (TMMail.pageIs('tlcontact')) category = teamlabContacts;
                window.ASC.Mail.ga_track(category, ga_Actions.buttonClick, "write_letter");
            }
        });

        // _Selection checkbox clicked
        _page.find('#ContactsList .row > .checkbox').unbind('click').bind('click', function () {
            var $this = $(this);
            if ($this.hasClass('disable'))
                return false;

            var row = $this.parent();
            var $input = $this.find('input');
            var contact_id = $input.attr('data_id');
            if (row.is('.selected')) {
                _Selection.RemoveId(contact_id);
                $input.prop('checked', false);
            } else {
                _Selection.AddId(contact_id, _getContactFromRow($(row)));
                $input.prop('checked', true);
            }
            row.toggleClass('selected');
            _updateSelectionComboCheckbox();
            _commonButtonsState();
        });


        var $rows = _page.find('#ContactsList .row');
        for (var j = 0, k = $rows.length; j < k; j++) {
            var $row = $($rows[j]);

            var $emails = $row.find('.email[isprimary="true"]');
            if ($emails.length > 0) {
                var primaryEmail = $($emails[0]);
                primaryEmail.show();

                primaryEmail.find('span').bind('click', function (event) {
                    _writeLetter(event, { name: $(event.target).text(), contact_name: $(event.target).attr('contact_name') });
                });
            }

            if ($row.find('.email').length == 1) $row.find('.emails').addClass('oneEmail');

            var $more = $row.find('.emails .more_lnk');
            // async action panel initialization - only after click on "more" element
            $more.find('.gray').unbind('.contactsPage').bind('click.contactsPage', function () {
                // action panel need to be initialized just once - so imidiatly unbind
                $(this).unbind('.contactsPage');
                // add action panel with more emails
                _emailListActionPanel((this.parentElement).parentElement);
            });

            if (data.emptyLabel) $row.find('.labels').remove();
            if (data.emptyTitel) $row.find('.title').remove();
        }

        RedrawNavigation(params.Page, params.__total || data.contacts.length, params.ContactsStore);

        _page.find('.containerBodyBlock').show();
    };

    var _onPageShow = function() {
        _page.find('#ContactsList .row').each(function(index, value) {
            _createTagsHidePanel($(value));
        });
        if (filter.ContactsStore == 'crm') {
            crmFilter.update();
        }
        else {
            tlFilter.update();
        }
        _updateSelectionView();
        _commonButtonsState();
    };

    var _crateSelectActionPandel = function() {
        if (filter.ContactsStore == 'crm') {
            _page.find('#SelectAllContactsDropdown').parent().actionPanel({ buttons: [
                { text: window.MailScriptResource.AllLabel, handler: _actionPanelSelectAll },
                { text: window.MailScriptResource.WithTags, handler: _actionPanelSelectWithTags },
                { text: window.MailScriptResource.WithoutTags, handler: _actionPanelSelectWithoutTags },
                { text: window.MailScriptResource.NoneLabel, handler: _actionPanelSelectNone }
            ], css: 'stick-over'});
        }
        else {
            _page.find('#SelectAllContactsDropdown').parent().actionPanel({ buttons: [
                { text: window.MailScriptResource.AllLabel, handler: _actionPanelSelectAll },
                { text: window.MailScriptResource.NoneLabel, handler: _actionPanelSelectNone }
            ], css: 'stick-over'});
        }

    };

    // Initializes action panel on more emails element
    var _emailListActionPanel = function (emails) {
        var buttons = [];
        var email_array = $(emails).find('.email');
        for (var i = 0, n = email_array.length; i < n; i++) {
            if (!$(email_array[i]).is(':visible')) {
                var $email_span = $(email_array[i]).find('span');
                buttons.push({
                    'text': $email_span.text(),
                    handler: _writeLetter,
                    name: $email_span.text(),
                    contact_name: $email_span.attr('contact_name')
                });
            }
        }
        $(emails).find('.gray').actionPanel({ 'buttons': buttons }).click();
    };

    var _setDefaultValueFilter = function() {
        _setPageInfo(1, TMMail.option('ContactsPageSize'));
        filter.ContactsStore = '';
        filter.CrmSortBy = 'displayname';
        filter.CrmSortOrder = 'ascending';
        filter.TlSortBy = 'displayName';
        filter.TlSortOrder = 'ascending';
        filter.ContactListView = '';
        filter.ContactType = undefined;
        filter.Tags = [];
        filter.Search = '';
        filter.FilterValue = '';
    };

    var RedrawNavigation = function(page, total_items_count, contacts_type) {
        var onChangePageSize = function(page_size) {
            if (isNaN(page_size) || page_size < 1) return;
            TMMail.option('ContactsPageSize', page_size);
            _keep_selection_on_reload = true;
            _updateAnchor(contacts_type, page);
        };
        var onChangePage = function(page) {
            if (isNaN(page) || page < 1) return;
            _keep_selection_on_reload = true;
            _updateAnchor(contacts_type, page);
        };

        PagesNavigation.RedrawNavigationBar(mailPageNavigator,
            page, TMMail.option('ContactsPageSize'), total_items_count, onChangePage, onChangePageSize,
            window.MailScriptResource.TotalContacts);
        PagesNavigation.FixAnchorPageNumberIfNecessary(page);
        PagesNavigation.RedrawPrevNextControl();
    };

    var HideNavigation = function() {
        $('#ContactsBottomNavigationBar').hide();
    };

    var show = function(type) {
        _setDefaultValueFilter();

        var anchor = ASC.Controls.AnchorController.getAnchor();
        _fromAnchor(anchor);
        filter.ContactsStore = type;

        if (_isFilterEmpty()) {
            _resetFilter();
        }

        // checks weather page size value in anchor is correct - replace anchor if not
        if (PagesNavigation.FixAnchorPageSizeIfNecessary(filter.Count))
            return;

        if (!_keep_selection_on_reload) {
            _Selection.Clear();
        }
        else _keep_selection_on_reload = false;

        _getContacts(type);
    };

    var _actionPanelSelectAll = function() {

        var category = ga_Categories.crmContacts;
        if (TMMail.pageIs('tlcontact')) category = teamlabContacts;
        window.ASC.Mail.ga_track(category, ga_Actions.actionClick, "all_select");

        $('#ContactsList .row').each(function () {
            if ($(this).find('input[type="checkbox"]').hasClass('disable'))
                return true;
            _Selection.AddId($(this).attr('data_id'), _getContactFromRow($(this)));
        });

        _updateSelectionView();
        _commonButtonsState();
    };


    var _actionPanelSelectNone = function() {

        var category = ga_Categories.crmContacts;
        if (TMMail.pageIs('tlcontact')) category = teamlabContacts;
        window.ASC.Mail.ga_track(category, ga_Actions.actionClick, "none_select");

        _clearCurrentPageSelection();

        _updateSelectionView();
        _commonButtonsState();
    };

    var _commonButtonsState = function() {
        if (_isContainEmails()) {
            $('.contentMenuWrapper:visible .menuAction').addClass('unlockAction');
        } else {
            $('.contentMenuWrapper:visible .menuAction').removeClass('unlockAction');
        }
    };

    var _isFilterEmpty = function() {
        var result = false;
        if (!filter.Tags.length && filter.ContactType == undefined && filter.Search == ''
            && filter.FilterValue == '' && filter.ContactListView == '')
            result = true;
        return result;
    };

    var _resetFilter = function() {

        if (filter.ContactsStore == 'crm')
            crmFilter.setSort(filter.CrmSortBy, filter.CrmSortOrder);
        else
            tlFilter.setSort(filter.TlSortBy, filter.TlSortOrder);

        if (_isFilterEmpty()) {
            if (filter.ContactsStore == 'crm')
                crmFilter.clear();
            else tlFilter.clear();
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

        if (filter.Tags.length)
            crmFilter.setTags(filter.Tags);

        if (filter.ContactType != undefined)
            crmFilter.setType(filter.ContactType);

        if (filter.Search != '') {
            if (filter.ContactsStore == 'crm')
                crmFilter.setSearch(filter.Search);
            else tlFilter.setSearch(filter.Search);
        }

        if (filter.FilterValue != '')
            tlFilter.setGroup(filter.FilterValue);

    };

    var _actionPanelSelectWithTags = function() {

        window.ASC.Mail.ga_track(ga_Categories.crmContacts, ga_Actions.actionClick, "whith_tag_select");

        _clearCurrentPageSelection();
        $('#ContactsList .row').each(function() {
            var $row = $(this);
            if ($row.find('.tag').length > 0) {
                _Selection.AddId($row.attr('data_id'), _getContactFromRow($row));
            }
        });
        _updateSelectionView();
        _commonButtonsState();
    };

    var _actionPanelSelectWithoutTags = function() {

        window.ASC.Mail.ga_track(ga_Categories.crmContacts, ga_Actions.actionClick, "without_select");

        _clearCurrentPageSelection();
        $('#ContactsList .row').each(function() {
            var $row = $(this);
            if ($row.find('.tag').length == 0) {
                _Selection.AddId($row.attr('data_id'), _getContactFromRow($row));
            }
        });
        _updateSelectionView();
        _commonButtonsState();
    };

    var _pretreatment = function(id) {
        var $emails = _page.find('.row[data_id="' + id + '"] .email[isprimary="true"]');

        if ($emails.length > 0) {
            $("#contactActionMenu .writeLetter").show();
        }
        else $("#contactActionMenu .writeLetter").hide();
    };

    var _viewContact = function(id) {
        if (filter.ContactsStore == 'crm')
            window.open('../../products/crm/default.aspx?id=' + id, "_blank");
        else if (filter.ContactsStore == 'teamlab')
            window.open('../../products/people/profile.aspx?user=' + id, "_blank");
    };


    var _writeLetter = function(event, buttonContext) {
        messagePage.setToEmailAddresses([_getContact(buttonContext.name, buttonContext.contact_name)]);
        messagePage.composeTo();
    };

    var _writeLetterById = function(id) {
        var $row = _page.find('#ContactsList .row[data_id="' + id + '"]');
        messagePage.setToEmailAddresses([_getContactFromRow($row)]);
        messagePage.composeTo();
    };

    var _massMailing = function() {

        var emails = new TMContainers.StringSet();

        _Selection.Each(function(id, email) {
            if (email) emails.Add(email);
        });

        if (emails.Count() > 0) {
            messagePage.setToEmailAddresses(emails.GetValues());
            messagePage.composeTo();
        }
    };

    var _isContainEmails = function() {
        var have_emails = false;
        _Selection.Each(function(id, email) {
            if (email) {
                have_emails = true;
                return false;
            }
        });
        return have_emails;
    };

    var _createTagsHidePanel = function(row) {
        var $smalstags = row.find('.tag');
        var $labels = row.find('.labels');
        var labelNames = [];
        for (var i = 0; i < $smalstags.length; i++) {
            var $tag = $($smalstags[i]);
            labelNames.push(TMMail.htmlEncode($tag.text()));
            $tag.remove();
        }
        if (labelNames.length > 0) {
            $labels.hidePanel({ 'items': labelNames, 'item_to_html': _labels_to_html });
        }
    };

    var _labels_to_html = function(name) {
        var tag = tagsManager.getTagByName(TMMail.htmlDecode(name));
        var html = $.tmpl("contactTagTmpl", tag, { htmlEncode: TMMail.htmlEncode });
        return html;
    };

    var hide = function() {
        if (!(TMMail.pageIs('crm') && filter.ContactsStore == 'crm') &&
            !(TMMail.pageIs('teamlab') && filter.ContactsStore == 'teamlab')) {
            _page.hide();
        }
    };

    var _toAnchor = function(page) {
        var res = '/';

        if (filter.ContactsStore == 'crm') {
            if (filter.CrmSortBy != '')
                res += 'sortBy=' + filter.CrmSortBy + '/';
            if (filter.CrmSortOrder != '')
                res += 'sortOrder=' + filter.CrmSortOrder + '/';
        } else {
            if (filter.TlSortBy != '')
                res += 'sortBy=' + filter.TlSortBy + '/';
            if (filter.TlSortOrder != '')
                res += 'sortOrder=' + filter.TlSortOrder + '/';
        }

        if (filter.ContactListView != '')
            res += 'contactListView=' + filter.ContactListView + '/';

        if (filter.ContactType != undefined)
            res += 'contactType=' + filter.ContactType + '/';

        if (filter.Search != '')
            res += 'search=' + encodeURIComponent(filter.Search) + '/';

        if (filter.FilterValue != '')
            res += 'filterValue=' + encodeURIComponent(filter.FilterValue) + '/';

        for (var i = 0; i < filter.Tags.length; i++)
            res += 'tags=' + encodeURIComponent(filter.Tags[i]) + '/';

        if (page !== undefined) {
            var page_size = TMMail.option('ContactsPageSize');
            if (filter.Count != page_size) page = 1;
            res += 'page=' + page + '/';
            res += 'page_size=' + page_size + '/';
        }
        return res;
    };

    var _fromAnchor = function(params) {

        var sortBy, sortOrder, filterValue, contactType, page, page_size, contactListView, tag, search;

        filter.Tags = [];

        if (typeof params !== 'undefined') {
            sortBy = TMMail.getParamsValue(params, /sortBy=([^\/]+)/);
            sortOrder = TMMail.getParamsValue(params, /sortOrder=([^\/]+)/);
            contactListView = TMMail.getParamsValue(params, /contactListView=([^\/]+)/);
            contactType = TMMail.getParamsValue(params, /contactType=([^\/]+)/);
            search = TMMail.getParamsValue(params, /search=([^\/]+)/);
            page = TMMail.getParamsValue(params, /page=(\d+)/);
            page_size = TMMail.getParamsValue(params, /page_size=(\d+)/);
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
            if (sortBy) filter.CrmSortBy = sortBy;
            if (sortOrder) filter.CrmSortOrder = sortOrder;
        } else {
            if (sortBy) filter.TlSortBy = sortBy;
            if (sortOrder) filter.TlSortOrder = sortOrder;
        }

        if (contactListView)
            filter.ContactListView = contactListView;
        else
            filter.ContactListView = '';

        if (contactType)
            filter.ContactType = contactType;
        else
            filter.ContactType = undefined;

        if (search)
            filter.Search = decodeURIComponent(search);
        else
            filter.FilterValue = '';

        if (filterValue)
            filter.FilterValue = decodeURIComponent(filterValue);

        if (page && page_size) {
            TMMail.option('ContactsPageSize', page_size);
            _setPageInfo(page, page_size);
        }
        else {
            _setPageInfo(1, TMMail.option('ContactsPageSize'));
        }
    };

    var _getContacts = function(type) {
        var filter_data = {};
        filter_data.StartIndex = filter.StartIndex;
        filter_data.Count = filter.Count;

        if ('crm' == type) {

            filter_data.sortBy = filter.CrmSortBy;
            filter_data.sortOrder = filter.CrmSortOrder;
            filter_data.contactStage = -1;
            if ('' != filter.ContactListView)
                filter_data.contactListView = filter.ContactListView;
            if (filter.Search != '')
                filter_data.filterValue = filter.Search;
            if (filter.ContactType)
                filter_data.contactType = filter.ContactType;
            filter_data.tags = filter.Tags;

            serviceManager.getCrmContacts({ Page: _getPageFromFilter(), ContactsStore: filter.ContactsStore },
                { filter: filter_data, success: _onGetCrmContacts },
                ASC.Resources.Master.Resource.LoadingProcessing);

        } else {
            //('teamlab' == type)
            filter_data.sortBy = filter.TlSortBy;
            filter_data.sortorder = filter.TlSortOrder;

            if (filter.FilterValue) {
                filter_data.groupId = encodeURIComponent(filter.FilterValue);
            }

            if (filter.Search != '')
                filter_data.filtervalue = filter.Search;

            var options = { filter: filter_data, success: _onGetTlContacts };

            serviceManager.getProfilesByFilter({ Page: _getPageFromFilter(), ContactsStore: filter.ContactsStore },
                options, ASC.Resources.Master.Resource.LoadingProcessing);
        }
    };

    var _updateAnchor = function(type, page) {
        ASC.Controls.AnchorController.move(_getAnchorByType(type) + _toAnchor(page));
    };

    var _setPageInfo = function(page, page_size) {
        filter.Count = page_size;
        filter.StartIndex = page_size * (page - 1);
    };

    var _getPageFromFilter = function() {
        return filter.StartIndex / TMMail.option('ContactsPageSize') + 1;
    };

    var _getAnchorByType = function(type) {
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

    var _getContactFromRow = function($row) {
        var email = _getEmailFromRow($row);
        var name = _getNameFromRow($row);
        return _getContact(email, name);
    };

    var _getContact = function(email, name) {
        var contact = undefined;
        if (email) {
            if (email) contact = '"' + name + '"';
            if (contact) contact += ' <' + email + '>';
            else contact = email;
        }
        return contact;
    };

    var _getEmailFromRow = function($row) {
        var $email = $row.find('.email[isprimary="true"]');
        return $email.length > 0 ? $($email[0]).text().trim() : undefined;
    };

    var _getNameFromRow = function($row) {
        var $name = $row.find('.name');
        return $name.length > 0 ? $($name[0]).text().trim() : undefined;
    };

    var resetFilter = function() {
        ASC.Controls.AnchorController.move(_getAnchorByType(filter.ContactsStore));
        _resetFilter();
    };


    return {
        init: init,
        show: show,
        hide: hide,
        resetFilter: resetFilter
    };

})(jQuery);