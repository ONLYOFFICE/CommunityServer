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


if (typeof ASC === "undefined") {
    ASC = {};
}

if (typeof ASC.CRM === "undefined") {
    ASC.CRM = function () { return {} };
}

if (typeof ASC.CRM.Voip === "undefined") {
    ASC.CRM.Voip = function () { return {} };
}

ASC.CRM.Voip.PhoneView = (function($) {
    //#region fields

    var initiated = false,
        serviceUnavailable = false,
        serviceAlreadyRunning = false,
        countries = [],
        user = null,
        operator = null,
        pause = false,
        allowOutgoingCalls = false,
        missedCalls = [],
        currentCall = null,
        currentCallStatus = null,
        defaultPhoneCode = '+1',
        defaultPhoneCountry = 'US',
        selectedPhoneCountry = 'US',
        selectedContact = null,
        incomingCallPlayer = null,
        defaultResetCallInterval = 5000,
        callTimer = null,
        callTimerOffset = null,
        socket = null,
        twilioConnection = null;

    var callToContactId;
    var disableClass = "disable",
        activeClass = "active";

    var $dropViewBtn,
        $dropView,

        $view,
        operatorStatusSwitcherOptionsTmpl,
        optionsBoxTmpl,
        countriesPanelTmpl,
        contactPhoneSwitcherItemTmpl,
        missedCallTmpl,
        operatorsRedirectOptionTmpl,
        callTmpl,
        $serviceUnavailableBox,
        $serviceAlreadyRunning,
        $viewInitLoaderBox,

        //operator box
        $operatorBox,
        $operatorStatus,
        $operatorName,
        $operatorStatusSwitcher,
        $operatorStatusSwitcherOptions,

        //options box
        $optionsBox,

        //missed calls box
        $missedCallsBox,
        $missedCallsList,
        $missedCallsEmptyMsg,

        //phone box
        $phoneBox,
        $selectedContact,
        $phoneSelectorBox,
        $countrySelector,
        $phoneInput,
        $phoneClearBtn,
        $selectedContactPhoneSwitcherBtn,
        $contactPhoneSwitcherPanel,
        $selectContactBtn,
        $callBtn,

        //call box
        $callBox,
        $callStatusBox,
        $callStatuses,
        $callMainBox,
        $callDude,
        $callTimer,
        $callBtns,
        $answerCallBtn,
        $rejectCallBtn,
        $completeCallBtn,
        $redirectCallBtn,
        $operatorsRedirectBox,
        $operatorsRedirectSelector,
        $operatorsRedirectEmptyMsg;

    var loadingBanner = LoadingBanner,
        master = ASC.Resources.Master,
        resource = master.Resource;
    var operatorStatus = {
        online: 0,
        busy: 1,
        offline: 2
    }
    var device;

    var callStatus = {
        outgoingStarted: 0,
        outgoingGoing: 1,
        outgoingCompleted: 2,
        incomingStarted: 3,
        incomingGoing: 4,
        incomingCompleted: 5
    }

    var documentHidden = true,
        defaultDocTitle;
    //#endregion

    //#region init

    function init() {
        if (ASC.SocketIO && !ASC.SocketIO.disabled()) {
            socket = ASC.SocketIO.Factory.voip;
        }

        if (typeof socket === "undefined") {
            $(".studio-top-panel .voip").hide();
            return;
        }

        document.addEventListener("visibilitychange", function() { documentHidden = document.hidden; }, false);

        cacheElements();

        bindEvents();
        bindDrops();

        setDefaultCountryData();

        socket
            .on('reconnecting',
                function() {
                    operatorStatusUpdated(operatorStatus.offline);
                    device.destroy();
                })
            .on('disconnected',
                function() {
                    if (initiated) {
                        operatorStatusUpdated(operatorStatus.offline);
                        device.destroy();
                    }
                })
            .connect(function() {
                getData(onGetData);
            })
            .reconnect_failed(function() {
                serviceUnavailable = true;
                renderView();
            })
            .on('status', operatorStatusUpdated)
            .on('miss', callMissed)
            .on('onlineAgents', onlineOperatorsUpdated)
            .on('dequeue', incomingCallInitiated)
            .on('reload', function() {
                 location.reload();
            });
    }

    function onGetData(data) {
        saveData(data);

        socket.emit('getStatus', function (status) {
            operator.status = status;
            if (status != operatorStatus.offline) {
                serviceAlreadyRunning = true;
            }

            renderView();

            if (status === operatorStatus.offline) {
                setOperatorStatus(operatorStatus.online);
            }
        });
    }

    function initTwilio(status) {
        Teamlab.getVoipToken(null,
        {
            success: function (params, data) {
                device = new Twilio.Device(data, { disableAudioContextSounds: true });

                device.on("ready", function () {
                    initiated = true;

                    pushOperatorStatus(status);
                });

                device.on("incoming", function (connection) { incomingTwilioHandler(connection) });
                device.on("cancel", cancelTwilioHandler);

                device.on("error", function (error) {
                    if (error && (error.code === 31201 || error.code === 31208)) {
                        rejectCall();
                    }
                });

                device.on("offline", function () {
                    initiated = false;
                    twilioConnection = null;
                });
            }
        });
    }

    function cacheElements() {
        $dropViewBtn = $('.voipActiveBox');
        $dropView = $('#studio_dropVoipPopupPanel');

        $view = $dropView.find('#voip-phone-view');

        operatorStatusSwitcherOptionsTmpl = 'operator-status-switcher-options-tmpl';
        optionsBoxTmpl = 'options-box-tmpl';
        countriesPanelTmpl = 'countries-panel-tmpl';
        contactPhoneSwitcherItemTmpl = 'contact-phone-switcher-item-tmpl';
        missedCallTmpl = 'missed-call-tmpl';
        operatorsRedirectOptionTmpl = 'operators-redirect-option-tmpl';
        callTmpl = 'call-tmpl';

        incomingCallPlayer = $dropView.find('#incoming-player').get(0);
        if (supportsMp3()) {
            incomingCallPlayer.src = "https://media.twiliocdn.com/sdk/js/client/sounds/releases/1.0.0/incoming.mp3"; // master.VoipPhone.IncomingRingtoneMp3;
        } else if (supportsVAW()) {
            incomingCallPlayer.src = master.VoipPhone.IncomingRingtoneWav;
        } else {
            incomingCallPlayer = null;
        }
        $serviceUnavailableBox = $view.find('#service-unavailable-box');
        $serviceAlreadyRunning = $view.find('#service-already-running-box');
        $viewInitLoaderBox = $view.find('#view-init-loader-box');

        //operator box
        $operatorBox = $view.find('#operator-box');

        $operatorStatus = $operatorBox.find('#operator-status');
        $operatorName = $operatorBox.find('#operator-name');

        $operatorStatusSwitcher = $operatorBox.find('#operator-status-switcher');
        $operatorStatusSwitcherOptions = $operatorBox.find('#operator-status-switcher-options');

        //options box
        $optionsBox = $view.find('#options-box');

        //missed calls box
        $missedCallsBox = $view.find('#missed-calls-box');
        $missedCallsList = $view.find('#missed-calls-list');
        $missedCallsEmptyMsg = $missedCallsBox.find('#missed-calls-empty-msg');

        //phone box
        $phoneBox = $view.find('#phone-box');

        $selectedContact = $phoneBox.find('#selected-contact');
        $phoneSelectorBox = $phoneBox.find('#phone-selector-box');

        $countrySelector = $phoneSelectorBox.find('#country-selector');

        $phoneInput = $phoneSelectorBox.find('#phone-input');
        $phoneClearBtn = $phoneSelectorBox.find('#phone-clear-btn');

        $selectedContactPhoneSwitcherBtn = $phoneSelectorBox.find('#contact-phone-switcher-btn');
        $contactPhoneSwitcherPanel = $phoneSelectorBox.find('#contact-phone-switcher-panel');

        $selectContactBtn = $phoneBox.find('#select-contact-btn');

        $callBtn = $phoneBox.find('#call-btn');

        //call box
        $callBox = $view.find('#call-box');

        $callStatusBox = $callBox.find('#call-status-box');
        $callStatuses = $callStatusBox.find('.call-status');

        $callMainBox = $callBox.find('#call-main-box');

        $callDude = $callBox.find('#call-dude');
        $callTimer = $callBox.find('#call-timer');

        $callBtns = $callBox.find('.call-btn');

        $answerCallBtn = $callBox.find('.answer-btn');
        $rejectCallBtn = $callBox.find('.reject-btn');
        $completeCallBtn = $callBox.find('.complete-btn');
        $redirectCallBtn = $callBox.find('.redirect-btn');

        $operatorsRedirectBox = $callBox.find('#operators-redirect-box');
        $operatorsRedirectSelector = $operatorsRedirectBox.find('#operators-redirect-selector');
        $operatorsRedirectEmptyMsg = $operatorsRedirectBox.find('#operators-redirect-empty-msg');
    }

    function bindEvents() {
        var clickEventName = 'click';
        //operator box
        $operatorBox.on(clickEventName, '#operator-online-btn', setOperatorStatus.bind(this, 0));
        $operatorBox.on(clickEventName, '#operator-offline-btn', setOperatorStatus.bind(this, 2));

        $operatorStatusSwitcher.on(clickEventName, switchOperatorStatus);

        $optionsBox.on('change', '#call-type-selector', setOperatorCallType);

        $phoneBox.on(clickEventName, '.panel-btn', phoneButtonClicked);

        $phoneInput.on('keyup', phoneInputPressed);
        $phoneClearBtn.on(clickEventName, clearPhoneBox);

        $phoneSelectorBox.on(clickEventName, '#countries-panel .dropdown-item', selectPhoneCountry);
        $phoneSelectorBox.on(clickEventName, '#contact-phone-switcher-panel .dropdown-item', selectContactPhone);

        $selectContactBtn.on('showList', selectContact);

        $missedCallsBox.on(clickEventName, '.recall-btn', makeRecall);

        $callBtn.on(clickEventName, makeCall);
        $answerCallBtn.on(clickEventName, answerCall);
        $rejectCallBtn.on(clickEventName, rejectCall);
        $completeCallBtn.on(clickEventName, completeCall);
        $redirectCallBtn.on(clickEventName, redirectCallHandler);

        $optionsBox.on(clickEventName, '#toggle-phone-box-btn', showPhoneBox);
        $optionsBox.on(clickEventName, '#toggle-missed-box-btn', showMissedBox);
    }

    function bindDrops() {
        $.dropdownToggle({
            switcherSelector: '#operator-status-switcher',
            dropdownID: 'operator-status-switcher-options',
            addTop: -7,
            addLeft: 9,
            inPopup: true
        });

        $.dropdownToggle({
            switcherSelector: '#country-selector',
            dropdownID: 'countries-panel',
            addTop: 8,
            addLeft: 10,
            inPopup: true
        });

        $.dropdownToggle({
            switcherSelector: '#contact-phone-switcher-btn',
            dropdownID: 'contact-phone-switcher-panel',
            addTop: 18,
            addLeft: -207,
            inPopup: true
        });

        $selectContactBtn.contactadvancedSelector({
            inPopup: true,
            isTempLoad: true,
            onechosen: true
        });
    }

    //#endregion

    //#region data

    function getData(callback) {
        var teamlabParams = function(cb) {
            return {
                success: function(params, data) {
                    cb(null, data);
                },
                error: function(err) {
                    cb(err);
                }
            }
        };

        async.parallel([
                function (cb) { Teamlab.getCrmCurrentVoipNumber(null, teamlabParams(cb)); },
                function (cb) { Teamlab.getVoipMissedCalls(null, teamlabParams(cb)); }],
                function (err, data) {
                    if (err) {
                        showErrorMessage();
                    } else {
                        callback(data);
                    }
                });
    }

    function spawnNotification(body, title) {
        var n;

        if (!("Notification" in window)) {
            return;
        }

        if (Notification.permission === "granted") {
            n = new Notification(title, { body: body });
        }
        else if (Notification.permission !== 'denied') {
            Notification.requestPermission(function (permission) {
                if (permission === "granted") {
                    n = new Notification(title, { body: body });
                }
            });
        }

        if (n) {
            n.onclick = function (event) {
                event.preventDefault();
                window.focus();
            };
        }
    }

    function saveData(data) {
        if ("Notification" in window) {
            Notification.requestPermission();
        }
        countries = ASC.Voip.Countries;
        user = Teamlab.profile;

        var number = data[0];
        operator = number.settings.caller;
        pause = number.settings.pause;
        allowOutgoingCalls = operator.allowOutgoingCalls && number.settings.allowOutgoingCalls;

        if (!allowOutgoingCalls) {
            lockMissedCallList();
        }

        defaultDocTitle = "+" + number.number;
        document.title = defaultDocTitle;

        var calls = data[1];

        if (!calls.length) {
            return;
        }

        missedCalls.push({
            lastCall: calls[0],
            callsCount: 1
        });

        for (var i = 1; i < calls.length; i++) {
            var call = calls[i];

            var dublicate = false;
            for (var j = 0; j < missedCalls.length; j++) {
                var missedCall = missedCalls[j];
                if (missedCall.lastCall.from == call.from) {
                    missedCall.callsCount++;
                    dublicate = true;
                    break;
                }
            }

            if (!dublicate) {
                missedCalls.push({
                    lastCall: calls[i],
                    callsCount: 1
                });
            }
        }
    }

    function addMissedCall(call) {
        var repeatedCall = false;
        if (!missedCalls.length || missedCalls[0].lastCall.from != call.from) {
            missedCalls.unshift({
                lastCall: call,
                callsCount: 1
            });
        } else {
            repeatedCall = true;
            missedCalls[0].lastCall = call;
            missedCalls[0].callsCount++;
        }

        return {
            repeatedCall: repeatedCall,
            call: missedCalls[0]
        };
    }

    function deleteMissedCalls(number) {
        var missed = [];
        for (var i = 0; i < missedCalls.length; i++) {
            if (missedCalls[i].lastCall.from != number) {
                missed.push(missedCalls[i]);
            }
        }
        missedCalls = missed;
    }

    function getUsers(ids) {
        var users = UserManager.getUsers(ids);

        for (var i = 0; i < users.length; i++) {
            if (users[i].id == user.id) {
                users.splice(i, 1);
                break;
            }
        }

        return users;
    }

    function getPhoneCountry(phone) {
        var cies = [];
        for (var i = 0; i < countries.length; i++) {
            var country = countries[i];

            if (phone.indexOf('+' + country.code) == 0) {
                cies.push(country);
            }
        }

        if (cies.length == 0) {
            return null;
        } else if (cies.length == 1) {
            return cies[0];
        } else {
            for (i = 0; i < cies.length; i++) {
                if (cies[i].iso == selectedPhoneCountry) {
                    return cies[i];
                }
            }

            return cies[0];
        }
    }

    function saveOperator(settings) {
        operator.answer = settings.answerType;
        operator.redirectToNumber = settings.redirectToNumber;
    }

    //#endregion 

    //#region rendering

    function renderView() {
        $viewInitLoaderBox.hide();

        if (serviceUnavailable) {
            $serviceUnavailableBox.show();
            return;
        }

        if (serviceAlreadyRunning) {
            $serviceAlreadyRunning.show();
            return;
        } else {
            $serviceAlreadyRunning.hide();
        }

        renderOperatorBox();
        renderOptionsBox();
        renderMissedCalls();
        renderPhoneBox();
        renderCallBox();

        if (currentCallStatus == callStatus.incomingStarted) {
            showDropView();
        }
    }

    function renderOperatorBox() {
        $operatorStatusSwitcherOptions.html(jq.tmpl(operatorStatusSwitcherOptionsTmpl));

        $operatorStatusSwitcherOptions.hide();
        $operatorName.text(user.displayName);

        if (operator.status == operatorStatus.online) {
            $operatorStatus.removeClass('offline').addClass('online');
            $operatorStatusSwitcher.text(resource.OnlineStatus);
            $operatorStatusSwitcher.removeClass('lock');
        } else if (operator.status == operatorStatus.offline) {
            $operatorStatus.removeClass('online').addClass('offline');
            $operatorStatusSwitcher.text(resource.OfflineStatus);
            $operatorStatusSwitcher.removeClass('lock');
        } else if (operator.status == operatorStatus.busy) {
            $operatorStatus.removeClass('offline').addClass('online');
            $operatorStatusSwitcher.text(resource.BusyStatus);
            $operatorStatusSwitcher.removeClass('lock');
            if (currentCallStatus != null) {
                $operatorStatusSwitcher.addClass('lock');
            }
        }

        $operatorBox.show();
    }

    function renderOptionsBox() {
        if (!allowOutgoingCalls) {
            return;
        }

        if (currentCallStatus != null) {
            $optionsBox.hide();
            return;
        }

        var $box = jq.tmpl(optionsBoxTmpl, {
            answer: operator.answer,
            redirectToNumber: operator.redirectToNumber,
            phones: user.contacts.telephones
        });
        $optionsBox.html($box);

        var $toggleBtn = $optionsBox.find('#toggle-phone-box-btn');
        var $toggleMissedBtn = $optionsBox.find('#toggle-missed-box-btn');
        if (operator.answer == 0) {
            $toggleBtn.hide();
            $toggleMissedBtn.hide();
        } else {
            $toggleBtn.show();
            $toggleMissedBtn.show();
            if (operator.status === operatorStatus.offline) {
                $toggleBtn.addClass(disableClass);
                $toggleMissedBtn.addClass(disableClass);
            } else {
                $toggleBtn.removeClass(disableClass);
                $toggleMissedBtn.removeClass(disableClass);
            }
        }

        $optionsBox.show();
    }

    function renderMissedCalls() {
        if (currentCallStatus != null || $phoneBox.is(':visible')) {
            $missedCallsBox.hide();
            return;
        }

        if (operator.status == operatorStatus.offline || operator.answer == 0) {
            $missedCallsBox.hide();
        } else {
            showMissedBox();
        }

        if (missedCalls.length) {
            $missedCallsEmptyMsg.hide();
        } else {
            $missedCallsEmptyMsg.show();
        }

        if (initiated) {
            return;
        }

        if (missedCalls.length) {
            $missedCallsEmptyMsg.hide();

            var $calls = $.tmpl(missedCallTmpl, missedCalls);
            $missedCallsList.html($calls);
        }
    }

    function renderPhoneBox() {
        if (!initiated) {
            var $panel = $.tmpl(countriesPanelTmpl, { countries: countries });
            $phoneSelectorBox.append($panel);
        }

        if (operator.status == operatorStatus.offline || currentCallStatus != null || operator.answer == 0) {
            $phoneBox.hide();
            return;
        }
    }

    function renderCallBox() {
        if (currentCallStatus == null) {
            $callBox.hide();
            return;
        }

        $callTimer.empty();

        $callStatuses.hide().filter('[data-status=' + currentCallStatus + ']').show();
        $callBtns.hide().filter('[data-status=' + currentCallStatus + ']').show();

        if (currentCallStatus == callStatus.incomingGoing) {
            $operatorsRedirectBox.show();
        } else {
            $operatorsRedirectBox.hide();
        }

        var $call = $.tmpl(callTmpl, currentCall);
        $callMainBox.html($call);

        $callBox.show();
    }

    function showDropView() {
        if (!$dropView.is(':visible')) {
            $dropViewBtn.click();
        }
    }

    function setDefaultCountryData() {
        setPhoneCountry();
        $phoneInput.val(defaultPhoneCode);
    }

    ;

    function setPhoneCountry(isoCode) {
        var iso = arguments.length == 0 ? defaultPhoneCountry : isoCode;

        selectedPhoneCountry = iso;
        $countrySelector.attr('class', 'voip-flag link arrow-down ' + iso);
    }

    //#endregion

    //#region handlers

    function setOperatorStatus(status) {
        if (operator.status == status) {
            $operatorStatusSwitcherOptions.hide();
        } else {
            showLoader();
            if (operator.status === operatorStatus.offline && status === operatorStatus.online) {
                initTwilio(status);
            } else {
                if (operator.status === operatorStatus.online && status === operatorStatus.offline) {
                    device.destroy();
                }
                pushOperatorStatus(status);
            }
        }
    }

    function pushOperatorStatus(status) {
        socket.emit('status', status,
            function() {
                hideLoader();
            });
    }

    function switchOperatorStatus() {
        if (operator.status == operatorStatus.busy && currentCallStatus != null) {
            return false;
        }

        return true;
    }

    function setOperatorCallType(e) {
        var $this = $(e.target);
        var type = $(e.target).val();

        var settings = type == "0" ? {
            answerType: type,
            redirectToNumber: $this.find(':selected').attr('data-number')
        } : {
            answerType: type
        };

        showLoader();

        Teamlab.updateCrmVoipOperator(null, user.id,
            settings, {
                success: function() {
                    hideLoader();
                    saveOperator(settings);

                    renderView();
                },
                error: function() {
                    hideLoader();
                    showErrorMessage();
                }
            });
    }

    function showPhoneBox() {
        if (jq(this).hasClass(disableClass)) return;
        $missedCallsBox.hide();
        $callBox.hide();
        $phoneBox.show();
        $optionsBox.find('#toggle-missed-box-btn').removeClass(activeClass);
        $optionsBox.find('#toggle-phone-box-btn').addClass(activeClass);
    }

    function showMissedBox() {
        if (jq(this).hasClass(disableClass)) return;
        $callBox.hide();
        $phoneBox.hide();
        $missedCallsBox.show();
        $optionsBox.find('#toggle-phone-box-btn').removeClass(activeClass);
        $optionsBox.find('#toggle-missed-box-btn').addClass(activeClass);
    }

    function selectPhoneCountry(e) {
        var $this = $(e.currentTarget).find('.voip-flag');
        var iso = $this.attr('data-iso');
        var code = $this.attr('data-code');

        clearPhoneBox();

        if (iso != defaultPhoneCountry) {
            $phoneClearBtn.show();
        } else {
            $phoneClearBtn.hide();
        }

        setPhoneCountry(iso);
        $phoneInput.val('+' + code);

        $this.closest('.studio-action-panel').hide();
    }

    function selectContactPhone(e) {
        var $this = $(e.currentTarget).find('.data');
        var phone = $this.text();

        renderSelectedPhone(phone);

        $this.closest('.studio-action-panel').hide();
    }

    function selectContact(e, contact) {
        selectedContact = contact;
        $selectedContact.text(contact.title).show();

        if (contact.phone && contact.phone.length) {
            var phone = contact.phone[0].data;
            renderSelectedPhone(phone);

            var $items = jq.tmpl(contactPhoneSwitcherItemTmpl, contact.phone);
            $contactPhoneSwitcherPanel.find('.dropdown-content').html($items);
            $selectedContactPhoneSwitcherBtn.show();
        } else {
            setDefaultCountryData();
        }

        $phoneClearBtn.show();
    }

    function renderSelectedPhone(phone) {
        var country = getPhoneCountry(phone);
        if (country) {
            setPhoneCountry(country.iso);
        } else {
            setPhoneCountry();
        }

        $phoneInput.val(phone);
    }

    function phoneButtonClicked(e) {
        var btnValue = $(e.currentTarget).attr('data-value');
        var newNumberValue = $phoneInput.val() + btnValue;

        $phoneInput.val(newNumberValue);
        phoneInputPressed();
    }

    function phoneInputPressed() {
        var phone = $phoneInput.val().replace(/\D/g, '');

        if (phone != defaultPhoneCode) {
            $phoneClearBtn.show();
        } else {
            $phoneClearBtn.hide();
        }

        var country = getPhoneCountry(phone);
        if (country) {
            setPhoneCountry(country.iso);
        } else {
            setPhoneCountry();
        }

        clearSelectedContact();
    }

    function clearPhoneBox() {
        setDefaultCountryData();
        $phoneClearBtn.hide();
        clearSelectedContact();
    }

    function clearSelectedContact() {
        $selectedContact.hide();
        $selectedContactPhoneSwitcherBtn.hide();

        if (selectedContact) {
            $selectContactBtn.contactadvancedSelector('unselect', [selectedContact.id]);
            selectedContact = null;
        }
    }

    //#endregion

    //#region call handlers

    function makeCall() {
        if ($callBtn.is('.disable')) {
            return;
        }
        $callBtn.addClass('disable');

        var query = {
            to: $phoneInput.val().replace(/\D/g, ''),
            contactId: selectedContact ? selectedContact.id : null
        };

        Teamlab.callVoipNumber(null, query, {
            success: function(params, call) {
                currentCall = call;

                operator.status = operatorStatus.busy;
                currentCallStatus = callStatus.outgoingStarted;

                $callBtn.removeClass('disable');
                renderView();
            },
            error: function (params, errors) {
                showErrorMessage();
                $callBtn.removeClass('disable');
            }
        });
    }

    function makeRecall(e) {
        var $this = $(e.currentTarget);

        if ($this.is('.disable')) {
            return;
        }

        $this.addClass('disable');
        $this.closest('.missed-call').addClass('selected');
        lockMissedCallList();

        var number = $this.attr('data-number');
        Teamlab.callVoipNumber(null, { to: number }, {
            success: function(params, call) {
                currentCall = call;

                operator.status = operatorStatus.busy;
                currentCallStatus = callStatus.outgoingStarted;

                $this.removeClass('disable');
                unlockMissedCallList();
                renderView();

                deleteMissedCalls(number);
                $missedCallsList.find('.missed-call[data-number="' + number + '"]').remove();
            },
            error: function() {
                $this.removeClass('disable');
                $this.closest('.missed-call').removeClass('selected');
                unlockMissedCallList();
                showErrorMessage();
            }
        });
    }

    function lockMissedCallList() {
        $missedCallsList.addClass('lock');
    }

    function unlockMissedCallList() {
        if (!allowOutgoingCalls) return;
        $missedCallsList.removeClass('lock');
    }

    function answerCall() {
        Teamlab.answerVoipCall({}, currentCall.id, {
            error: function() {
                showErrorMessage();
            }
        });
    }

    function rejectCall() {
        Teamlab.rejectVoipCall({}, currentCall.id, {
            success: function() {
                currentCallStatus = callStatus.incomingCompleted;
                renderView();
                resetCall();
            },
            error: function() {
                showErrorMessage();
                resetCall();
            }
        });
    }

    function redirectCallHandler() {
        if ($redirectCallBtn.is('.disable')) {
            return;
        }

        var to = $operatorsRedirectSelector.val();
        Teamlab.redirectVoipCall({}, currentCall.id, { to: to },
            {
                success: function() {
                    currentCallStatus = callStatus.incomingCompleted;
                    renderView();
                    resetCall();
                },
                error: function() {
                    showErrorMessage();
                    resetCall();
                }
            });
    }

    function completeCall() {
        if (twilioConnection) {
            twilioConnection.disconnect();
        } else {
            stopCallTimer();
            currentCallStatus = callStatus.incomingCompleted;

            renderView();
            resetCall();
        }
    }

    function makeCallToContact(contactId) {
        if (!operator || operator.status !== operatorStatus.online || currentCallStatus !== null || serviceUnavailable || serviceAlreadyRunning) {
            callToContactId = contactId;
            return;
        }
        callToContactId = null;
        showPhoneBox();
        Teamlab.getCrmContact({},
            contactId,
            {
                success: function(params, contact) {
                    var newObj = {
                        title: contact.displayName || contact.title || contact.name || contact.Name,
                        id: contact.id && contact.id.toString(),
                        phone: contact.phone || (contact.commonData && contact.commonData.filter(function(el) {
                                return el.infoType == 0;
                            }))
                    }

                    if (contact.hasOwnProperty("contactclass")) {
                        newObj.type = contact.contactclass;
                    }

                    selectContact(undefined, newObj);
                }
            });
    }

    //#endregion

    //#region push handlers

    function operatorStatusUpdated(status) {
        if (!initiated) {
            if (operator.status == operatorStatus.offline && status == operatorStatus.online) {
                serviceAlreadyRunning = true;
            }
            if (status === operatorStatus.offline) {
                serviceAlreadyRunning = false;
            }
        }

        operator.status = status;

        if (operator.status != operatorStatus.online) {
            $callBtn.addClass(disableClass);
        } else {
            $callBtn.removeClass(disableClass);
        }

        if (operator.status == operatorStatus.busy) {
            renderOperatorBox();
            return;
        }

        renderView();
        if (callToContactId) {
            makeCallToContact(callToContactId);
        }
    }

    function onlineOperatorsUpdated(ids) {
        if (!initiated) return;

        var operators = getUsers(ids);

        if (operators && operators.length) {
            var $operators = jq.tmpl(operatorsRedirectOptionTmpl, operators);
            $operatorsRedirectSelector.html($operators);

            $redirectCallBtn.removeClass('disable');
            $operatorsRedirectEmptyMsg.hide();
            $operatorsRedirectSelector.show();
        } else {
            $operatorsRedirectSelector.hide();
            $redirectCallBtn.addClass('disable');
            $operatorsRedirectEmptyMsg.show();
        }
    }

    function incomingCallInitiated(callId) {
        if (!initiated) return;

        socket.emit('status', 1, function () {
            Teamlab.getVoipCall({},
                callId,
                {
                    success: function (params, call) {
                        currentCall = call;
                        currentCallStatus = callStatus.incomingStarted;
                        if (incomingCallPlayer) {
                            incomingCallPlayer.play();
                        }
                        if (documentHidden) {
                            spawnNotification(call.from, resource.CallIncoming);
                            var focusTimer = setInterval(function () {
                                if (window.closed) {
                                    clearInterval(focusTimer);
                                    return;
                                }
                                document.title = resource.CallIncoming + ":" + call.from;
                                window.focus();
                            }, 1000);

                            window.onmousemove = function () {
                                clearInterval(focusTimer);
                                document.title = defaultDocTitle;
                                document.onmousemove = null;
                            }
                        }
                        renderView();
                    }
                });
        });
    }

    function callGoing() {
        if (currentCallStatus == callStatus.incomingStarted) {
            currentCallStatus = callStatus.incomingGoing;
            stopIncomingCallPlayer();
        } else if (currentCallStatus == callStatus.outgoingStarted) {
            currentCallStatus = callStatus.outgoingGoing;
        } else {
            return;
        }

        renderView();
        startCallTimer();
    }

    function callCompleted() {
        stopCallTimer();

        if (twilioConnection) {
            twilioConnection.disconnect();
        }

        if (currentCallStatus == callStatus.incomingGoing) {
            currentCallStatus = callStatus.incomingCompleted;
        } else if (currentCallStatus == callStatus.outgoingGoing) {
            currentCallStatus = callStatus.outgoingCompleted;
        } else {
            return;
        }

        renderView();
        resetCall();
    }

    function callMissed(callId) {
        if (!initiated) return;


        Teamlab.getVoipCall({},
            callId,
            {
                success: function(params, call) {
                    var res = addMissedCall(call);

                    var $call = jq.tmpl(missedCallTmpl, res.call);
                    if (res.repeatedCall) {
                        $missedCallsList.find('.missed-call:first').replaceWith($call);
                    } else {
                        $missedCallsList.prepend($call);
                    }

                    resetCall(0);

                    if (!pause) {
                        socket.emit('status', 0);
                    }

                    if (documentHidden) {
                        spawnNotification(call.from, resource.CallMissed);
                    }
                }
            });
    }

    function resetCall(interval) {
        clearCall();

        var i = arguments.length == 1 ? interval : defaultResetCallInterval;
        setTimeout(function() {
            renderView();
        }, i);
    }

    function clearCall() {
        currentCall = null;
        currentCallStatus = null;
        
        stopIncomingCallPlayer();

        $operatorStatusSwitcher.removeClass('lock');
        clearPhoneBox();
    }

    function stopIncomingCallPlayer() {
        if (incomingCallPlayer && incomingCallPlayer.readyState != 0) {
            incomingCallPlayer.pause();
            incomingCallPlayer.currentTime = 0;
        }
    }

    //#endregion

    //#region twilio handlers

    function incomingTwilioHandler(connection) {
        twilioConnection = connection;
        connection.on("accept", function () {
            socket.emit('status', 1);
            callGoing();
            Teamlab.saveVoipCall({}, currentCall.id,
            {
                 answeredBy: Teamlab.profile.id
            },
            {
                async: true,
                success: function (params, call) {
                    currentCall = call;
                    renderView();
                }
            });
        });
        connection.on("disconnect", function () {
            if (!pause) {
                socket.emit('status', 0);
            }
            callCompleted();
        });

        connection.accept();
    }

    function cancelTwilioHandler() {
        stopCallTimer();
    }

    //#endregion

    //#region call timer

    function startCallTimer() {
        callTimerOffset = Date.now();
        callTimer = setInterval(function() { updateCallTimer(); }, 1000);
    }

    function updateCallTimer() {
        var now = Date.now();
        var delta = now - callTimerOffset;

        var minutes = ~~(delta / (1000 * 60));
        var seconds = ~~((delta - minutes * 1000 * 60) / 1000);

        minutes = minutes / 10 < 1 ? '0' + minutes : minutes;
        seconds = seconds / 10 < 1 ? '0' + seconds : seconds;

        $callTimer.text(minutes + ':' + seconds);
    }

    function stopCallTimer() {
        if (callTimer) {
            clearInterval(callTimer);
            callTimer = null;
        }
    }


    //#endregion

    //#region utils

    function supportsMp3() {
        return supportsAudioType("mp3");
    }

    function supportsVAW() {
        return supportsAudioType("wav");
    }

    function supportsAudioType(audioType) {
        try {
            var elem = document.createElement('audio');
            if (!!elem.canPlayType) {
                switch (audioType) {
                case "ogg":
                    return elem.canPlayType('audio/ogg; codecs="vorbis"').replace(/^no$/, '');
                case "mp3":
                    return elem.canPlayType('audio/mpeg;').replace(/^no$/, '');
                case "wav":
                    return elem.canPlayType('audio/wav; codecs="1"').replace(/^no$/, '');
                case "m4a":
                    return (elem.canPlayType('audio/x-m4a;') || elem.canPlayType('audio/aac;')).replace(/^no$/, '');
                }
            }
        } catch (e) {
        }

        return false;
    }

    function showLoader() {
        loadingBanner.displayLoading();
    }

    function hideLoader() {
        loadingBanner.hideLoading();
    }

    function showErrorMessage() {
        toastr.error(resource.CommonJSErrorMsg);
    }

    //#endregion

    return {
        init: init,
        makeCallToContact: makeCallToContact
    };
})(jq);

jq(function() {
    ASC.CRM.Voip.PhoneView.init();
});