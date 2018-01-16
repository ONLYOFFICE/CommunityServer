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

if (typeof ASC === "undefined") {
    ASC = {};
}

if (typeof ASC.CRM === "undefined") {
    ASC.CRM = function () { return {} };
}

if (typeof ASC.CRM.Voip === "undefined") {
    ASC.CRM.Voip = function () { return {} };
}

ASC.CRM.Voip.CallsView = (function ($) {

    var myCallsContactFilter = (function () {
        var showList = "showList",
            filterId = 'calls-filter',
            idFilterByContact = 'contactID',

            type = 'custom-contact',
            hiddenContainerId = 'hiddenBlockForCallsContactSelector',
            headerContainerId = 'callsContactSelectorForFilter';

        function init() {
            jq("#" + headerContainerId)
                .contactadvancedSelector(
                {
                    showme: true,
                    addtext: ASC.CRM.Resources.CRMContactResource.AddNewCompany,
                    noresults: ASC.CRM.Resources.CRMCommonResource.NoMatches,
                    noitems: ASC.CRM.Resources.CRMCommonResource.NoMatches,
                    inPopup: true,
                    onechosen: true,
                    isTempLoad: true
                });

            jq(window).bind("afterResetSelectedContact", function (event, obj, objName) {
                if (objName === "callsContactSelectorForFilter" && filterId) {
                    jq('#' + filterId).advansedFilter('resize');
                }
            });
        }

        function onSelectContact(event, item) {
            jq("#" + headerContainerId).find(".inner-text .value").text(item.title);

            var $filter = jq('#' + filterId);
            $filter.advansedFilter(idFilterByContact, { id: item.id, displayName: item.title, value: item.id });
            $filter.advansedFilter('resize');
        }

        function createFilterByContact(filter) {
            var o = document.createElement('div');
            o.innerHTML = [
              '<div class="default-value">',
                '<span class="title">',
                  filter.title,
                '</span>',
                '<span class="selector-wrapper">',
                  '<span class="contact-selector"></span>',
                '</span>',
                '<span class="btn-delete">&times;</span>',
              '</div>'
            ].join('');
            return o;
        }

        function customizeFilterByContact($container, $filteritem, filter) {
            var $headerContainer = jq('#' + headerContainerId);
            if ($headerContainer.parent().is("#" + hiddenContainerId)) {
                $headerContainer.off(showList).on(showList, onSelectContact);
                $headerContainer.next().andSelf().appendTo($filteritem.find('span.contact-selector:first'));
            }
        }

        function destroyFilterByContact($container, $filteritem, filter) {
            var $headerContainer = jq('#' + headerContainerId);
            if (!$headerContainer.parent().is("#" + hiddenContainerId)) {
                $headerContainer.off(showList);
                $headerContainer.find(".inner-text .value").text(ASC.CRM.Resources.CRMCommonResource.Select);
                $headerContainer.next().andSelf().appendTo(jq('#' + hiddenContainerId));
                jq('#' + headerContainerId).contactadvancedSelector("reset");
            }
        }

        function processFilter($container, $filteritem, filtervalue, params) {
            if (params && params.id && isFinite(params.id)) {
                var $headerContainer = jq('#' + headerContainerId);
                $headerContainer.find(".inner-text .value").text(params.displayName);
                $headerContainer.contactadvancedSelector("select", [params.id]);
            }
        }

        return{
            type: type,
            id: idFilterByContact,
            apiparamname: jq.toJSON(["entityid", "entityType"]),
            title: ASC.CRM.Resources.CRMContactResource.Contact,
            group: ASC.CRM.Resources.CRMCommonResource.Other,
            hashmask: '',
            create: createFilterByContact,
            customize: customizeFilterByContact,
            destroy: destroyFilterByContact,
            process: processFilter,
            init: init
        };

    })();

    var callTmpl,
        calls = [],
        pagingCtrl,
        pagingVisibleInterval = 3,
        pageCount,
        currentPage,
        filterItemsCount,
        filterInit,
        currentFilter,
        $view,
        $playRecordNotSupportBox,
        $callList,
        $paging,
        $pagingPageCount,
        $pagingItemsCount,
        $emptyListBox,
        $emptyFilterBox,
        $callRecordPlayPanel,
        $callRecordPlayPanelLoader,
        $callRecordPlayPanelProgrees,
        $callRecordPlayPanelProgreesPersentage,
        $callRecordPlayPanelTimer,
        $callRecordPlayer,
        callRecordPlayer;

    var loadingBanner = LoadingBanner,
        resources = ASC.CRM.Resources,
        jsresource = resources.CRMJSResource,
        localStorageManagerLocal = localStorageManager,
        self;

    var pageCountStorageKey = "pageCountStorageKey",
        currentPageStorageKey = "currentPageStorageKey",
        filterHashStorageKey = "filterHashStorageKey";

    function init() {
        var hashCallId = getHashParam(window.location.hash, 'id');
        if (hashCallId) {
            renderCallView(hashCallId);
            return;
        }

        self = this;

        cacheElements();
        bindEvents();

        initPaging();
        initFilter();

        if (!supportsAudioVAW()) {
            $playRecordNotSupportBox.show();
        }

        myCallsContactFilter.init();
    }

    function renderCallView(callId) {
        $view = $('#voip-calls-view');

        showLoader();
        Teamlab.getVoipCall(null, callId, {
            success: function(params, call) {
                var callJS = getJSCall(call);
                var $callView = jq.tmpl('voip-call-view-tmpl', callJS);
                $view.append($callView);

                hideLoader();
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    }

    function cacheElements() {
        $view = $('#voip-calls-view');
        $playRecordNotSupportBox = $view.find('#play-record-not-support-box');
        
        $callList = $view.find('#calls-list');

        $paging = $view.find('#calls-paging');
        $pagingPageCount = $paging.find('#calls-paging-page-count');
        $pagingItemsCount = $paging.find('#calls-paging-items-count');

        $emptyListBox = $view.find('#voip-calls-empty-list-box');
        $emptyFilterBox = $view.find('#voip-calls-empty-filter-box');

        $callRecordPlayPanel = $view.find('#call-record-play-panel');
        $callRecordPlayPanelLoader = $callRecordPlayPanel.find('#call-record-play-panel-loader');
        $callRecordPlayPanelProgrees = $callRecordPlayPanel.find('#call-record-play-panel-progress');
        $callRecordPlayPanelProgreesPersentage = $view.find('#call-record-play-panel-progress-percentage');
        $callRecordPlayPanelTimer = $callRecordPlayPanel.find('#call-record-play-panel-timer');

        $callRecordPlayer = $callRecordPlayPanel.find('#call-record-player');
        callRecordPlayer = $callRecordPlayer.get(0);
    }

    function bindEvents() {
        $pagingPageCount.on('change', function(e) {
            currentPage = 0;
            pageCount = $(e.target).val();
            saveStoragePaging();

            self.pagingCtrl.EntryCountOnPage = pageCount;

            getCalls();
        });

        var clickEventName = 'click';
        $emptyFilterBox.on(clickEventName, '.clearFilterButton', clearFilter);

        if (supportsAudioVAW()) {
            $callList.on(clickEventName, '.call-type .call-type-icon.play', startPlayCallRecord);

            $callRecordPlayPanel.on(clickEventName, '.pause', pauseCallRecord);
            $callRecordPlayPanel.on(clickEventName, '.play', resumeCallRecord);
            $callRecordPlayPanel.on(clickEventName, '.stop', stopCallRecord);

            $callRecordPlayPanelProgrees.on(clickEventName, changeCallRecordTime);

            $callRecordPlayPanel.on(clickEventName, function () {
                return false;
            });

            callRecordPlayer.addEventListener('canplaythrough', playCallRecord);
            callRecordPlayer.addEventListener('timeupdate', updateCallRecordTimer);
            callRecordPlayer.addEventListener('ended', completeCallRecord);
        }

        $callList.on(clickEventName, '.toggle-redirections-btn', toogleCallRedirections);
    }

    function initPaging() {
        var storagePaging = getStoragePaging();
        if (storagePaging) {
            pageCount = storagePaging.pageCount;
            currentPage = storagePaging.currentPage;
        } else {
            pageCount = 25;
            currentPage = 0;
        }

        $pagingPageCount.val(pageCount).tlCombobox();

        self.pagingCtrl = new ASC.Controls.PageNavigator.init(
            'ASC.CRM.Voip.CallsView.pagingCtrl', '#calls-paging-box',
            pageCount, pagingVisibleInterval, currentPage + 1,
            jsresource.Previous, jsresource.Next
        );

        self.pagingCtrl.changePageCallback = function (page) {
            currentPage = page - 1;
            saveStoragePaging();

            getCalls();
        };
    }

    function getStoragePaging() {
        if (!localStorageManagerLocal.isAvailable) {
            return null;
        }

        var pc = parseInt(localStorageManagerLocal.getItem(pageCountStorageKey));
        var cp = parseInt(localStorageManagerLocal.getItem(currentPageStorageKey));

        if (isNaN(pc) || isNaN(cp)) {
            return null;
        }

        return {
            pageCount: pc,
            currentPage: cp
        };
    }

    function saveStoragePaging() {
        localStorageManagerLocal.setItem(pageCountStorageKey, pageCount);
        localStorageManagerLocal.setItem(currentPageStorageKey, currentPage);
    }

    function initFilter() {
        var storageHash = getStorageFilterHash();

        var hash;
        if (window.location.hash) {
            hash = window.location.hash;
            if (hash !== storageHash) {
                currentPage = 0;
                saveStoragePaging();
            }
        } else {
            hash = storageHash;
        }

        initFilterByHash(hash);
    }

    function initFilterByHash(hash) {
        var sortByParam = getHashParam(hash, 'sortBy'),
            sortOrderParam = getHashParam(hash, 'sortOrder'),
            typeParam = getHashParam(hash, 'callType'),
            agentParam = getHashParam(hash, 'agent'),
            fromParam = getHashParam(hash, 'from'),
            toParam = getHashParam(hash, 'to'),
            textParam = decodeURIComponent(getHashParam(hash, 'text'));

        var days = getFilterDays();
        $view.find('#calls-filter').advansedFilter(
            {
                store: false,
                colcount: 2,
                anykeytimeout: 1000,
                filters:
                [
                    //Type
                    {
                        type: 'combobox',
                        id: 'answered',
                        title: jsresource.VoipCallAnsweredType,
                        group: jsresource.VoipCallType,
                        groupby: 'type',
                        hash: 'callType',
                        options:
                        [
                            { value: 'answered', title: jsresource.VoipCallAnsweredType, def: true },
                            { value: 'missed', title: jsresource.VoipCallMissedType },
                            { value: 'outgoing', title: jsresource.VoipCallOutgoingType }
                        ],
                        isset: !!typeParam && typeParam == 'answered',
                        params: !!typeParam && typeParam == 'answered' ? { value: 'answered' } : null
                    },
                    {
                        type: 'combobox',
                        id: 'missed',
                        title: jsresource.VoipCallMissedType,
                        group: jsresource.VoipCallType,
                        groupby: 'type',
                        hash: 'callType',
                        options:
                        [
                            { value: 'answered', title: jsresource.VoipCallAnsweredType },
                            { value: 'missed', title: jsresource.VoipCallMissedType, def: true },
                            { value: 'outgoing', title: jsresource.VoipCallOutgoingType }
                        ],
                        isset: !!typeParam && typeParam == 'missed',
                        params: !!typeParam && typeParam == 'missed' ? { value: 'missed' } : null
                    },
                    {
                        type: 'combobox',
                        id: 'outgoing',
                        title: jsresource.VoipCallOutgoingType,
                        group: jsresource.VoipCallType,
                        groupby: 'type',
                        hash: 'callType',
                        options:
                        [
                            { value: 'answered', title: jsresource.VoipCallAnsweredType },
                            { value: 'missed', title: jsresource.VoipCallMissedType },
                            { value: 'outgoing', title: jsresource.VoipCallOutgoingType, def: true }
                        ],
                        isset: !!typeParam && typeParam == 'outgoing',
                        params: !!typeParam && typeParam == 'outgoing' ? { value: 'outgoing' } : null
                    },
                    // Date
                    {
                        type: 'daterange',
                        id: 'today',
                        title: jsresource.Today,
                        filtertitle: ' ',
                        group: jsresource.VoipCallDate,
                        groupby: 'date',
                        bydefault: { from: days.today, to: days.today }
                    },
                    {
                        type: 'daterange',
                        id: 'currentweek',
                        title: jsresource.CurrentWeek,
                        filtertitle: ' ',
                        group: jsresource.VoipCallDate,
                        groupby: 'date',
                        bydefault: { from: days.startWeek, to: days.endWeek }
                    },
                    {
                        type: 'daterange',
                        id: 'currentmonth',
                        title: jsresource.CurrentMonth,
                        filtertitle: ' ',
                        group: jsresource.VoipCallDate,
                        groupby: 'date',
                        bydefault: { from: days.startMonth, to: days.endMonth }
                    },
                    {
                        type: 'daterange',
                        id: 'date',
                        title: jsresource.CustomPeriod,
                        filtertitle: ' ',
                        group: jsresource.VoipCallDate,
                        groupby: 'date',
                        isset: !!fromParam && !!toParam,
                        params: !!fromParam && !!toParam ? { from: fromParam, to: toParam } : null
                    },
                    // Caller
                    {
                        type: 'person',
                        id: 'agent',
                        title: jsresource.VoipCallAgent,
                        group: jsresource.VoipCallCaller,
                        isset: !!agentParam,
                        params: !!agentParam ? { id: agentParam } : null
                    },
                    myCallsContactFilter,
                    // Text
                    {
                        type: 'text',
                        id: 'text',
                        isset: !!textParam,
                        reset: !textParam,
                        params: !!textParam ? { value: textParam } : ""
                    }
                ],
                sorters: [
                    { id: 'date', title: jsresource.VoipCallDate, selected: sortByParam == 'date', sortOrder: sortByParam == 'date' ? sortOrderParam : 'descending', def: sortByParam == 'date' || !sortByParam },
                    { id: 'duration', title: jsresource.VoipCallDuration, selected: sortByParam == 'duration', sortOrder: sortByParam == 'duration' ? sortOrderParam : 'descending', def: sortByParam == 'duration' },
                    { id: 'cost', title: jsresource.VoipCallCost, selected: sortByParam == 'cost', sortOrder: sortByParam == 'cost' ? sortOrderParam : 'descending', def: sortByParam == 'cost' }
                ]
            })
            .bind('setfilter', filterChangedHandler)
            .bind('resetfilter', filterChangedHandler);
    }

    function getFilterDays() {
        var now = new Date();
        now = new Date(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate(), now.getUTCHours(), 0, 0, 0);
        now.setHours(now.getHours() + ASC.Resources.Master.CurrentTenantUtcHoursOffset);
        now.setHours(0);

        var startWeek = new Date(now);
        startWeek.setDate(now.getDate() - now.getDay() + 1);

        var endWeek = new Date(now);
        endWeek.setDate(now.getDate() - now.getDay() + 7);

        var startMonth = new Date(now);
        startMonth.setDate(1);

        var endMonth = new Date(startMonth);
        endMonth.setMonth(startMonth.getMonth() + 1);
        endMonth.setDate(endMonth.getDate() - 1);

        return {
            today: now.getTime(),
            startWeek: startWeek.getTime(),
            endWeek: endWeek.getTime(),
            startMonth: startMonth.getTime(),
            endMonth: endMonth.getTime(),
        };
    }

    function getStorageFilterHash() {
        return localStorageManagerLocal.getItem(filterHashStorageKey);
    }

    function saveStorageFilterHash(filterHash) {
        localStorageManagerLocal.setItem(filterHashStorageKey, '#' + filterHash);
    }

    function clearFilter() {
        $view.find('#calls-filter').advansedFilter(null);
    }

    function filterChangedHandler(e, $fltr) {
        if (!filterInit) {
            filterInit = true;
        } else {
            currentPage = 0;
            saveStoragePaging();
        }

        var filters = $fltr.advansedFilter();
        syncHashAndFilter(filters);

        var filter = getFilterObj(filters).data;
        getCalls(filter);
    }

    function syncHashAndFilter(filters) {
        var filterHash = getFilterObj(filters).hash;
        if (window.location.hash != filterHash) {
            window.location.hash = filterHash;
        }

        saveStorageFilterHash(filterHash);
    }

    function getFilterObj(filters) {
        var data = {};
        var hash = '';

        for (var i = 0; i < filters.length; i++) {
            var filter = filters[i];
            if (filter.id == 'sorter') {
                data.SortBy = filter.params.id;
                data.SortOrder = filter.params.sortOrder;
                hash += 'sortBy=' + filter.params.id + '&sortOrder=' + filter.params.sortOrder;
            } else if (filter.id == 'text') {
                if (filter.params.value !== "null") {
                    data.FilterValue = filter.params.value;
                    hash += filter.id + '=' + filter.params.value;
                } else {
                    hash = hash.substr(0, hash.length - 1);
                }
            } else if (filter.type == 'daterange') {
                data.from = filter.params.from ? ServiceFactory.serializeTimestamp(new Date(filter.params.from)) : null;
                data.to = filter.params.to ? ServiceFactory.serializeTimestamp(new Date(filter.params.to)) : null;
                hash += filter.params.from ? 'from=' + filter.params.from : '';
                hash += filter.params.to ? '&to=' + filter.params.to : '';
            } else {
                if (filter.params && filter.params.value) {
                    var key = filter.hash || filter.id;
                    data[key] = filter.params.value;
                    hash += key + '=' + filter.params.value;
                } else if (filter.options && filter.options.length) {
                    for (var j = 0; j < filter.options.length; j++) {
                        if (filter.options[j].def) {
                            data[filter.hash] = filter.options[j].value;
                            hash += filter.hash + '=' + filter.options[j].value;
                            break;
                        }
                    }
                }
            }

            if (i != filters.length - 1) {
                hash += '&';
            }
        }

        return {
            data: $.isEmptyObject(data) ? null : data,
            hash: '#' + hash
        };
    }

    function getCalls(filter, cb) {
        if (!filter) {
            filter = currentFilter;
        }

        filter.StartIndex = currentPage * pageCount;
        filter.Count = pageCount;

        currentFilter = filter;

        showLoader();
        Teamlab.getVoipCalls(null, filter, {
            success: function(params, data) {
                filterItemsCount = params.__total;
                calls = data.map(getJSCall);

                setPaging();
                renderCalls();

                hideLoader();

                if (cb) {
                    cb();
                }
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    }

    function getJSCall(call) {
        var supportsPlaying = supportsAudioVAW();
        call.supportsPlaying = supportsPlaying;

        call.typeClass = getCallTypeClass(call);
        call.typeString = getCallTypeString(call);

        call.contactTitle = call.contact.id != -1
            ? call.contact.displayName ? call.contact.displayName : jsresource.VoipCallContactRemoved
            : '';
        call.durationString = getTimeString(call.dialDuration);
        if (call.recordUrl && call.recordUrl.length) {
            call.recordUrl = "https://api.twilio.com" + call.recordUrl.replace("json", "wav");
        }
        return call;
    }

    function getCallTypeClass(call) {
        switch (call.status) {
            case 1:
                return 'outgoing';
            case 2:
                return 'answered';
            case 3:
                return 'missed';
            default:
                return '';
        }
    }

    function getCallTypeString(call) {
        switch (call.status) {
            case 1: return jsresource.VoipCallOutgoingType;
            case 2: return jsresource.VoipCallAnsweredType;
            case 3: return jsresource.VoipCallMissedType;
            default: return '';
        }
    }

    function setPaging() {
        $pagingItemsCount.text(filterItemsCount);
        self.pagingCtrl.drawPageNavigator(currentPage + 1, filterItemsCount);
    }

    function renderCalls() {
        stopCallRecord();
        
        if (calls.length) {
            var $calls = jq.tmpl("voip-view-call-tmpl", calls);
            $callList.find('tbody').html($calls);

            $view.find('#calls-filter').show();
            $view.find('#calls-filter').advansedFilter('resize');
            $callList.show();
            $paging.show();

            $emptyListBox.hide();
            $emptyFilterBox.hide();

        } else {
            $callList.hide();
            $paging.hide();

            if (filterApplied()) {
                $view.find('#calls-filter').show();
                $view.find('#calls-filter').advansedFilter('resize');
                $emptyFilterBox.show();
            } else {
                $view.find('#calls-filter').hide();
                $emptyListBox.show();
            }
        }
    }

    function filterApplied() {
        var currentHash = window.location.hash;
        if (getHashParam(currentHash, 'callType')
            || getHashParam(currentHash, 'agent')
            || getHashParam(currentHash, 'contactID')
            || getHashParam(currentHash, 'text')
            || getHashParam(currentHash, 'from')
            || getHashParam(currentHash, 'to')) {
            return true;
        }
        return false;
    }

    function toogleCallRedirections(e) {
        var $el = $(e.target);
        $el.closest('.call-row').nextUntil('.call-row').toggle();
    }

    //#region call record timer

    function startPlayCallRecord(e) {
        var $el = $(e.target);
        $callRecordPlayer.attr('src', $el.attr('data-recordUrl'));

        $callRecordPlayPanelLoader.show();
        $callRecordPlayPanel.find('.pause').hide();
        $callRecordPlayPanel.find('.play').hide();

        updateCallRecordTimer();

        $callRecordPlayPanel.css("top", "");
        $callRecordPlayPanel.css("left", "");
        $callRecordPlayPanel.offset($el.offset());
        $callRecordPlayPanel.show();
        playCallRecord();
    }

    function playCallRecord() {
        $callRecordPlayPanelLoader.hide();
        $callRecordPlayPanel.find('.pause').show();
        $callRecordPlayPanel.find('.play').hide();
        
        callRecordPlayer.play();
    }

    function pauseCallRecord() {
        callRecordPlayer.pause();

        $callRecordPlayPanel.find('.pause').hide();
        $callRecordPlayPanel.find('.play').show();

        return false;
    }

    function resumeCallRecord() {
        callRecordPlayer.play();

        $callRecordPlayPanel.find('.pause').show();
        $callRecordPlayPanel.find('.play').hide();

        return false;
    }

    function stopCallRecord() {
        if (!supportsAudioVAW()) {
            return;
        }

        callRecordPlayer.pause();
        $callRecordPlayPanel.hide();

        return false;
    }

    function completeCallRecord() {
        $callRecordPlayPanel.find('.pause').hide();
        $callRecordPlayPanel.find('.play').show();

        updateCallRecordTimer();
    }

    function changeCallRecordTime(e) {
        if (callRecordPlayer.readyState != 4) {
            return;
        }

        var $el = $(e.target);

        // jquery bug fix
        var offsetX = (e.offsetX || e.clientX - $(e.target).offset().left);
        var newTime = ~~(offsetX / $el.width() * callRecordPlayer.duration);

        callRecordPlayer.currentTime = newTime;
        updateCallRecordTimer();
    }

    function updateCallRecordTimer() {
        var percentage = !callRecordPlayer.duration ? 0 :
            callRecordPlayer.ended ? 0 : callRecordPlayer.currentTime / callRecordPlayer.duration * 100;

        $callRecordPlayPanelProgreesPersentage.css('width', percentage + '%');
        $callRecordPlayPanelTimer.text(getTimeString(callRecordPlayer.currentTime));
    }

    //#endregion

    //#region helpers

    function getTimeString(seconds) {
        var mm = ~~(seconds / 60);
        var ss = ~~(seconds - mm * 60);

        mm = mm / 10 < 1 ? '0' + mm : mm;
        ss = ss / 10 < 1 ? '0' + ss : ss;

        return mm + ':' + ss;
    }

    function getHashParam(hash, param) {
        if (!hash) {
            return null;
        }

        var regex = new RegExp('[#&]' + param + '=([^&]*)');

        var results = regex.exec(hash);
        return results == null ? '' : results[1];
    }

    var supportsAudioWAV, supportsAudioMP3;

    function supportsAudioVAW() {
        if (typeof supportsAudioWAV == "undefined") {
            supportsAudioWAV = supportsAudioType("wav");
        }
        return supportsAudioWAV;
    }

    function checkSupportsAudioMP3() {
        if (typeof supportsAudioMP3 == "undefined") {
            supportsAudioMP3 = supportsAudioType("mp3");
        }
        return supportsAudioMP3;
    }

    function supportsAudioType(audioType) {
        try {
            var elem = document.createElement('audio');
            if (!!elem.canPlayType) {
                switch (audioType) {
                    case "ogg": return elem.canPlayType('audio/ogg; codecs="vorbis"').replace(/^no$/, '');
                    case "mp3": return elem.canPlayType('audio/mpeg;').replace(/^no$/, '');
                    case "wav": return elem.canPlayType('audio/wav; codecs="1"').replace(/^no$/, '');
                    case "m4a": return (elem.canPlayType('audio/x-m4a;') || elem.canPlayType('audio/aac;')).replace(/^no$/, '');
                }
            }
        } catch (e) { }

        return false;
    }

    function showLoader() {
        loadingBanner.displayLoading();
    }

    function hideLoader() {
        loadingBanner.hideLoading();
    }

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }

    //#endregion

    return {
        init: init,
        pagingCtrl: pagingCtrl
    };
})(jq);

jq(function() {
    ASC.CRM.Voip.CallsView.init();
});