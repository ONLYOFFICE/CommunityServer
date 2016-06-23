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


window.VoipCallsView = new function() {
    var $ = jq;

    var filterHashStorageKey = 'voip.calls.filterhash';
    var pageCountStorageKey = 'voip.calls.pagecount';
    var currentPageStorageKey = 'voip.calls.currentpage';

    var callTmpl;
    var calls = [];
    
    var pagingCtrl;
    var pagingVisibleInterval = 3;

    var pageCount;
    var currentPage;
    var filterItemsCount;

    var filterInit;
    var currentFilter;

    function init() {
        var hashCallId = getHashParam(window.location.hash, 'id');
        if (hashCallId) {
            renderCallView(hashCallId);
            return;
        }

        cacheElements();
        bindEvents();

        initPaging();
        initFilter();

        if (!supportsAudioVAW()) {
            $playRecordNotSupportBox.show();
        }
    }

    function renderCallView(callId) {
        this.$view = $('#voip-calls-view');

        showLoader();
        Teamlab.getVoipCall(null, callId, {
            success: function(params, call) {
                var callJS = getJSCall(call);
                var $callView = $('#call-view-tmpl').tmpl(callJS);
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
        this.$view = $('#voip-calls-view');
        this.$playRecordNotSupportBox = $view.find('#play-record-not-support-box');
        
        callTmpl = $view.find('#call-tmpl');

        this.$filter = $view.find('#calls-filter');
        this.$callList = $view.find('#calls-list');

        this.$paging = $view.find('#calls-paging');
        this.$pagingPageCount = $paging.find('#calls-paging-page-count');
        this.$pagingItemsCount = $paging.find('#calls-paging-items-count');

        this.$emptyListBox = $view.find('#voip-calls-empty-list-box');
        this.$emptyFilterBox = $view.find('#voip-calls-empty-filter-box');

        this.$callRecordPlayPanel = $view.find('#call-record-play-panel');
        this.$callRecordPlayPanelLoader = $callRecordPlayPanel.find('#call-record-play-panel-loader');
        this.$callRecordPlayPanelProgrees = $callRecordPlayPanel.find('#call-record-play-panel-progress');
        this.$callRecordPlayPanelProgreesPersentage = $view.find('#call-record-play-panel-progress-percentage');
        this.$callRecordPlayPanelTimer = $callRecordPlayPanel.find('#call-record-play-panel-timer');

        this.$callRecordPlayer = $callRecordPlayPanel.find('#call-record-player');
        this.callRecordPlayer = $callRecordPlayer.get(0);
    }

    function bindEvents() {
        ASC.Controls.AnchorController.bind(/^(.+)*$/, function() {
            var filterHash = getFilterObj($filter.advansedFilter()).hash;
            if (window.location.hash != filterHash) {
                initFilterByHash(window.location.hash);
            }
        });

        $pagingPageCount.on('change', function(e) {
            currentPage = 0;
            pageCount = $(e.target).val();
            saveStoragePaging();

            pagingCtrl.EntryCountOnPage = pageCount;

            getCalls();
        });

        $emptyFilterBox.on('click', '.clearFilterButton', clearFilter);

        if (supportsAudioVAW()) {
            $callList.on('click', '.call-type .call-type-icon.play', startPlayCallRecord);

            $callRecordPlayPanel.on('click', '.pause', pauseCallRecord);
            $callRecordPlayPanel.on('click', '.play', resumeCallRecord);
            $callRecordPlayPanel.on('click', '.stop', stopCallRecord);

            $callRecordPlayPanelProgrees.on('click', changeCallRecordTime);

            $callRecordPlayPanel.on('click', function() {
                return false;
            });

            callRecordPlayer.addEventListener('canplaythrough', playCallRecord);
            callRecordPlayer.addEventListener('timeupdate', updateCallRecordTimer);
            callRecordPlayer.addEventListener('ended', completeCallRecord);
        }

        $callList.on('click', '.toggle-redirections-btn', toogleCallRedirections);
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

        pagingCtrl = new ASC.Controls.PageNavigator.init(
            'VoipCallsView.pagingCtrl', '#calls-paging-box',
            pageCount, pagingVisibleInterval, currentPage + 1,
            ASC.CRM.Resources.CRMJSResource.Previous, ASC.CRM.Resources.CRMJSResource.Next
        );

        pagingCtrl.changePageCallback = function(page) {
            currentPage = page - 1;
            saveStoragePaging();

            getCalls();
        };
    }

    function getStoragePaging() {
        if (!localStorageManager.isAvailable) {
            return null;
        }

        var pc = parseInt(localStorageManager.getItem("pageCountStorageKey"));
        var cp = parseInt(localStorageManager.getItem("currentPageStorageKey"));

        if (isNaN(pc) || isNaN(cp)) {
            return null;
        }

        return {
            pageCount: pc,
            currentPage: cp,
        };
    }

    function saveStoragePaging() {
        localStorageManager.setItem("pageCountStorageKey", pageCount);
        localStorageManager.setItem("currentPageStorageKey", currentPage);
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
        var sortByParam = getHashParam(hash, 'sortBy');
        var sortOrderParam = getHashParam(hash, 'sortOrder');

        var typeParam = getHashParam(hash, 'callType');
        var agentParam = getHashParam(hash, 'agent');

        var fromParam = getHashParam(hash, 'from');
        var toParam = getHashParam(hash, 'to');

        var textParam = getHashParam(hash, 'text');

        var days = getFilterDays();
        $filter.advansedFilter(
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
                        title: ASC.CRM.Resources.CRMJSResource.VoipCallAnsweredType,
                        group: ASC.CRM.Resources.CRMJSResource.VoipCallType,
                        groupby: 'type',
                        hash: 'callType',
                        options:
                        [
                            { value: 'answered', title: ASC.CRM.Resources.CRMJSResource.VoipCallAnsweredType, def: true },
                            { value: 'missed', title: ASC.CRM.Resources.CRMJSResource.VoipCallMissedType },
                            { value: 'outgoing', title: ASC.CRM.Resources.CRMJSResource.VoipCallOutgoingType }
                        ],
                        isset: !!typeParam && typeParam == 'answered',
                        params: !!typeParam && typeParam == 'answered' ? { value: 'answered' } : null
                    },
                    {
                        type: 'combobox',
                        id: 'missed',
                        title: ASC.CRM.Resources.CRMJSResource.VoipCallMissedType,
                        group: ASC.CRM.Resources.CRMJSResource.VoipCallType,
                        groupby: 'type',
                        hash: 'callType',
                        options:
                        [
                            { value: 'answered', title: ASC.CRM.Resources.CRMJSResource.VoipCallAnsweredType },
                            { value: 'missed', title: ASC.CRM.Resources.CRMJSResource.VoipCallMissedType, def: true },
                            { value: 'outgoing', title: ASC.CRM.Resources.CRMJSResource.VoipCallOutgoingType }
                        ],
                        isset: !!typeParam && typeParam == 'missed',
                        params: !!typeParam && typeParam == 'missed' ? { value: 'missed' } : null
                    },
                    {
                        type: 'combobox',
                        id: 'outgoing',
                        title: ASC.CRM.Resources.CRMJSResource.VoipCallOutgoingType,
                        group: ASC.CRM.Resources.CRMJSResource.VoipCallType,
                        groupby: 'type',
                        hash: 'callType',
                        options:
                        [
                            { value: 'answered', title: ASC.CRM.Resources.CRMJSResource.VoipCallAnsweredType },
                            { value: 'missed', title: ASC.CRM.Resources.CRMJSResource.VoipCallMissedType },
                            { value: 'outgoing', title: ASC.CRM.Resources.CRMJSResource.VoipCallOutgoingType, def: true }
                        ],
                        isset: !!typeParam && typeParam == 'outgoing',
                        params: !!typeParam && typeParam == 'outgoing' ? { value: 'outgoing' } : null
                    },
                    // Date
                    {
                        type: 'daterange',
                        id: 'today',
                        title: ASC.CRM.Resources.CRMJSResource.Today,
                        filtertitle: ' ',
                        group: ASC.CRM.Resources.CRMJSResource.VoipCallDate,
                        groupby: 'date',
                        bydefault: { from: days.today, to: days.today }
                    },
                    {
                        type: 'daterange',
                        id: 'currentweek',
                        title: ASC.CRM.Resources.CRMJSResource.CurrentWeek,
                        filtertitle: ' ',
                        group: ASC.CRM.Resources.CRMJSResource.VoipCallDate,
                        groupby: 'date',
                        bydefault: { from: days.startWeek, to: days.endWeek }
                    },
                    {
                        type: 'daterange',
                        id: 'currentmonth',
                        title: ASC.CRM.Resources.CRMJSResource.CurrentMonth,
                        filtertitle: ' ',
                        group: ASC.CRM.Resources.CRMJSResource.VoipCallDate,
                        groupby: 'date',
                        bydefault: { from: days.startMonth, to: days.endMonth }
                    },
                    {
                        type: 'daterange',
                        id: 'date',
                        title: ASC.CRM.Resources.CRMJSResource.CustomPeriod,
                        filtertitle: ' ',
                        group: ASC.CRM.Resources.CRMJSResource.VoipCallDate,
                        groupby: 'date',
                        isset: !!fromParam && !!toParam,
                        params: !!fromParam && !!toParam ? { from: fromParam, to: toParam } : null
                    },
                    // Caller
                    {
                        type: 'person',
                        id: 'agent',
                        title: ASC.CRM.Resources.CRMJSResource.VoipCallAgent,
                        group: ASC.CRM.Resources.CRMJSResource.VoipCallCaller,
                        isset: !!agentParam,
                        params: !!agentParam ? { id: agentParam } : null
                    },
                    // Text
                    {
                        type: 'text',
                        id: 'text',
                        isset: !!textParam,
                        reset: !textParam,
                        params: !!textParam ? { value: textParam } : null
                    }
                ],
                sorters: [
                    { id: 'date', title: ASC.CRM.Resources.CRMJSResource.VoipCallDate, selected: sortByParam == 'date', sortOrder: sortByParam == 'date' ? sortOrderParam : 'descending', def: sortByParam == 'date' || !sortByParam },
                    { id: 'duration', title: ASC.CRM.Resources.CRMJSResource.VoipCallDuration, selected: sortByParam == 'duration', sortOrder: sortByParam == 'duration' ? sortOrderParam : 'descending', def: sortByParam == 'duration' },
                    { id: 'cost', title: ASC.CRM.Resources.CRMJSResource.VoipCallCost, selected: sortByParam == 'cost', sortOrder: sortByParam == 'cost' ? sortOrderParam : 'descending', def: sortByParam == 'cost' }
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
        return localStorageManager.getItem("filterHashStorageKey");
    }

    function saveStorageFilterHash(filterHash) {
        localStorageManager.setItem("filterHashStorageKey", '#' + filterHash);
    }

    function clearFilter() {
        $filter.advansedFilter(null);
    }

    function filterChangedHandler(e, $fltr) {
        $filter = $fltr;

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
                data.FilterValue = filter.params.value;
                hash += filter.id + '=' + filter.params.value;
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
                calls = getJSCalls(data);

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

    function getJSCalls(serverCalls) {
        for (var i = 0; i < serverCalls.length; i++) {
            serverCalls[i] = getJSCall(serverCalls[i]);
        }

        return serverCalls;
    }

    function getJSCall(call) {
        var supportsPlaying = supportsAudioVAW();
        call.supportsPlaying = supportsPlaying;

        call.typeClass = getCallTypeClass(call);
        call.typeString = getCallTypeString(call);

        call.contactTitle = call.contact.id != -1
            ? call.contact.displayName ? call.contact.displayName : ASC.CRM.Resources.CRMJSResource.VoipCallContactRemoved
            : '';
        call.durationString = getTimeString(call.dialDuration);

        for (var i = 0; i < call.history.length; i++) {
            var h = call.history[i];
            h.supportsPlaying = supportsPlaying;
            h.agent = window.UserManager.getUser(h.answeredBy);
            h.durationString = getTimeString(h.wrapUpTime);
            h.waitingTimeString = getTimeString(h.waitTime);
        }

        return call;
    }

    function getCallTypeClass(call) {
        if (call.status == 2) {
            return 'answered';
        } else if (call.status == 3) {
            return 'missed';
        } else if (call.status == 1) {
            return 'outgoing';
        } else {
            return '';
        }
    }

    function getCallTypeString(call) {
        if (call.status == 2) {
            return ASC.CRM.Resources.CRMJSResource.VoipCallAnsweredType;
        } else if (call.status == 3) {
            return ASC.CRM.Resources.CRMJSResource.VoipCallMissedType;
        } else if (call.status == 1) {
            return ASC.CRM.Resources.CRMJSResource.VoipCallOutgoingType;
        } else {
            return '';
        }
    }

    function setPaging() {
        $pagingItemsCount.text(filterItemsCount);
        pagingCtrl.drawPageNavigator(currentPage + 1, filterItemsCount);
    }

    function renderCalls() {
        stopCallRecord();

        if (calls.length) {
            var $calls = callTmpl.tmpl(calls);
            $callList.find('tbody').html($calls);

            $filter.show();
            $filter.advansedFilter('resize');
            $callList.show();
            $paging.show();

            $emptyListBox.hide();
            $emptyFilterBox.hide();

        } else {
            $callList.hide();
            $paging.hide();

            if (filterApplied()) {
                $filter.show();
                $filter.advansedFilter('resize');
                $emptyFilterBox.show();
            } else {
                $filter.hide();
                $emptyListBox.show();
            }
        }
    }

    function filterApplied() {
        var currentHash = window.location.hash;
        if (getHashParam(currentHash, 'callType')
            || getHashParam(currentHash, 'agent')
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

        $callRecordPlayPanel.appendTo($el);
        $callRecordPlayPanel.show();
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

    function supportsAudioVAW() {
        return !!Modernizr.audio.wav;
    }

    function showLoader() {
        LoadingBanner.displayLoading();
    }

    function hideLoader() {
        LoadingBanner.hideLoading();
    }

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }

    //#endregion

    return {
        init: init,
        pagingCtrl: pagingCtrl
    };
};

jq(function() {
    window.VoipCallsView.init();
});