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

window.VoipPhoneView = new function() {
    var $ = jq;

    //#region fields

    var initiated = false;
    var serviceUnavailable = false;

    var countries = [];

    var user = null;
    var operator = null;

    var missedCalls = [];

    var testCall = {
        to: '17205482333',
        contact: {
            displayName: 'John Snow',
            //mediumFotoUrl: 'https://pavelpavel.teamlab.info/products/crm/app_themes/default/images/empty_people_logo_82_82.png'
            mediumFotoUrl: '/products/crm/data/0/photos/01/47/37/contact_14737_200_200.jpeg'
        }
    };
    var currentCall = null;
    var currentCallStatus = null;


    var defaultPhoneCode = '+1';
    var defaultPhoneCountry = 'US';

    var selectedPhoneCountry = 'US';
    var selectedContact = null;

    var incomingCallPlayer = null;
    var defaultResetCallInterval = 5000;

    var callTimer = null;
    var callTimerOffset = null;

    var signalrConnection = null;
    var twilioConnection = null;

    //#endregion

    //#region fields helpers

    function operatorOnline() {
        return operator.status == 0;
    }

    function operatorBusy() {
        return operator.status == 1;
    }

    function setOperatorBusy() {
        operator.status = 1;
    }

    function operatorOffline() {
        return operator.status == 2;
    }

    function operatorAllowOutgoingCalls() {
        return operator.allowOutgoingCalls;
    }

    function operatorAnswersByPhone() {
        return operator.answer == 0;
    }

    function callRunning() {
        return currentCallStatus != null;
    }

    function outgoingCallStarted() {
        return currentCallStatus == 0;
    }

    function outgoingCallGoing() {
        return currentCallStatus == 1;
    }

    function outgoingCallCompleted() {
        return currentCallStatus == 2;
    }

    function incomingCallStarted() {
        return currentCallStatus == 3;
    }

    function incomingCallGoing() {
        return currentCallStatus == 4;
    }

    function incomingCallCompleted() {
        return currentCallStatus == 5;
    }

    function setOutgoingCallStartedStatus() {
        currentCallStatus = 0;
    }

    function setOutgoingCallGoingStatus() {
        currentCallStatus = 1;
    }

    function setOutgoingCallCompletedStatus() {
        currentCallStatus = 2;
    }

    function setIncomingCallStartedStatus() {
        currentCallStatus = 3;

        if (incomingCallPlayer) {
            incomingCallPlayer.play();
        }
    }

    function setIncomingCallGoingStatus() {
        currentCallStatus = 4;

        if (incomingCallPlayer && incomingCallPlayer.readyState != 0) {
            incomingCallPlayer.pause();
            incomingCallPlayer.currentTime = 0;
        }
    }

    function setIncomingCallCompletedStatus() {
        currentCallStatus = 5;
    }

    //#endregion

    //#region init

    function init() {
        signalrConnection = jq.connection.voip;

        cacheElements();

        bindEvents();
        bindDrops();
        bindPushEvents();

        setDefaultCountryData();

        if (!$.connection.onStart) {
            return;
        }

        $.connection.onStart
            .done(function() {
                getData(function(data) {
                    saveData(data);
                    renderView();

                    initiated = true;
                });
            })
            .fail(function() {
                serviceUnavailable = true;
                renderView();
            });
    }

    function initTwilio(token) {
        Twilio.Device.setup(token);
        Twilio.Device.sounds.incoming(false);
    }

    function cacheElements() {
        this.$dropViewBtn = $('.voipActiveBox');
        this.$dropView = $('#studio_dropVoipPopupPanel');

        this.$view = $dropView.find('#voip-phone-view');

        this.operatorStatusSwitcherOptionsTmpl = $view.find('#operator-status-switcher-options-tmpl');
        this.optionsBoxTmpl = $view.find('#options-box-tmpl');
        this.countriesPanelTmpl = $view.find('#countries-panel-tmpl');
        this.contactPhoneSwitcherItemTmpl = $view.find('#contact-phone-switcher-item-tmpl');
        this.missedCallTmpl = $view.find('#missed-call-tmpl');
        this.operatorsRedirectOptionTmpl = $view.find('#operators-redirect-option-tmpl');
        this.callTmpl = $view.find('#call-tmpl');

        incomingCallPlayer = $dropView.find('#incoming-player').get(0);
        if (supportsMp3()) {
            incomingCallPlayer.src = ASC.Resources.Master.VoipPhone.IncomingRingtoneMp3;
        } else if (supportsVAW()) {
            incomingCallPlayer.src = ASC.Resources.Master.VoipPhone.IncomingRingtoneWav;
        } else {
            incomingCallPlayer = null;
        }

        this.$serviceUnavailableBox = $view.find('#service-unavailable-box');
        this.$viewInitLoaderBox = $view.find('#view-init-loader-box');

        //operator box
        this.$operatorBox = $view.find('#operator-box');

        this.$operatorStatus = $operatorBox.find('#operator-status');
        this.$operatorName = $operatorBox.find('#operator-name');

        this.$operatorStatusSwitcher = $operatorBox.find('#operator-status-switcher');
        this.$operatorStatusSwitcherOptions = $operatorBox.find('#operator-status-switcher-options');

        //options box
        this.$optionsBox = $view.find('#options-box');

        //missed calls box
        this.$missedCallsBox = $view.find('#missed-calls-box');
        this.$missedCallsList = $view.find('#missed-calls-list');
        this.$missedCallsEmptyMsg = $missedCallsBox.find('#missed-calls-empty-msg');

        //phone box
        this.$phoneBox = $view.find('#phone-box');

        this.$selectedContact = $phoneBox.find('#selected-contact');
        this.$phoneSelectorBox = $phoneBox.find('#phone-selector-box');

        this.$countrySelector = $phoneSelectorBox.find('#country-selector');

        this.$phoneInput = $phoneSelectorBox.find('#phone-input');
        this.$phoneClearBtn = $phoneSelectorBox.find('#phone-clear-btn');

        this.$selectedContactPhoneSwitcherBtn = $phoneSelectorBox.find('#contact-phone-switcher-btn');
        this.$contactPhoneSwitcherPanel = $phoneSelectorBox.find('#contact-phone-switcher-panel');

        this.$selectContactBtn = $phoneBox.find('#select-contact-btn');

        this.$callBtn = $phoneBox.find('#call-btn');

        //call box
        this.$callBox = $view.find('#call-box');

        this.$callStatusBox = $callBox.find('#call-status-box');
        this.$callStatuses = $callStatusBox.find('.call-status');

        this.$callMainBox = $callBox.find('#call-main-box');

        this.$callDude = $callBox.find('#call-dude');
        this.$callTimer = $callBox.find('#call-timer');

        this.$callBtns = $callBox.find('.call-btn');

        this.$answerCallBtn = $callBox.find('.answer-btn');
        this.$rejectCallBtn = $callBox.find('.reject-btn');
        this.$completeCallBtn = $callBox.find('.complete-btn');
        this.$redirectCallBtn = $callBox.find('.redirect-btn');

        this.$operatorsRedirectBox = $callBox.find('#operators-redirect-box');
        this.$operatorsRedirectSelector = $operatorsRedirectBox.find('#operators-redirect-selector');
        this.$operatorsRedirectEmptyMsg = $operatorsRedirectBox.find('#operators-redirect-empty-msg');
    }

    function bindEvents() {
        //operator box
        $operatorBox.on('click', '#operator-online-btn', setOperatorStatus.bind(this, 0));
        $operatorBox.on('click', '#operator-offline-btn', setOperatorStatus.bind(this, 2));

        $operatorStatusSwitcher.on('click', switchOperatorStatus);

        $optionsBox.on('change', '#call-type-selector', setOperatorCallType);

        $phoneBox.on('click', '.panel-btn', phoneButtonClicked);

        $phoneInput.on('keyup', phoneInputPressed);
        $phoneClearBtn.on('click', clearPhoneBox);

        $phoneSelectorBox.on('click', '#countries-panel .dropdown-item', selectPhoneCountry);
        $phoneSelectorBox.on('click', '#contact-phone-switcher-panel .dropdown-item', selectContactPhone);

        $selectContactBtn.on('showList', selectContact);

        $missedCallsBox.on('click', '.recall-btn', makeRecall);

        $callBtn.on('click', makeCall);
        $answerCallBtn.on('click', answerCall);
        $rejectCallBtn.on('click', rejectCall);
        $completeCallBtn.on('click', completeCall);
        $redirectCallBtn.on('click', redirectCallHandler);

        $optionsBox.on('click', '#toggle-phone-box-btn', togglePhoneBox);

        Twilio.Device.incoming(incomingTwilioHandler);
        Twilio.Device.cancel(cancelTwilioHandler);

        Twilio.Device.error(function(error) {
            console.log('Device Error:' + error.message);
        });
    }

    function bindDrops() {
        $.dropdownToggle({
            switcherSelector: '.studio-top-panel .voipActiveBox',
            dropdownID: 'studio_dropVoipPopupPanel',
            addTop: 5,
            addLeft: -268
        });

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
            onechosen: true,
            withPhoneOnly: true
        });
    }

    function bindPushEvents() {
        signalrConnection.client.status = operatorStatusUpdated;
        signalrConnection.client.onlineAgents = onlineOperatorsUpdated;

        signalrConnection.client.dequeue = incomingCallInitiated;
        signalrConnection.client.start = callGoing;
        signalrConnection.client.end = callCompleted;

        signalrConnection.client.miss = callMissed;
    }

    //#endregion

    //#region data

    function getData(callback) {
        async.parallel([
                function(cb) {
                    Teamlab.getVoipToken(null, {
                        success: function(params, data) {
                            cb(null, data);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                },
                function(cb) {
                    Teamlab.getCrmCurrentVoipNumber(null, {
                        success: function(params, data) {
                            cb(null, data);
                        },
                        error: function(err) {
                            cb(err);
                        }
                    });
                }, function(cb) {
                    Teamlab.getVoipMissedCalls(null,
                        {
                            success: function(params, data) {
                                cb(null, data);
                            },
                            error: function(err) {
                                cb(err);
                            }
                        });
                }], function(err, data) {
                    if (err) {
                        showErrorMessage();
                    } else {
                        callback(data);
                    }
                });
    }

    function saveData(data) {
        countries = window.VoIPCountries;
        user = Teamlab.profile;

        var twilioToken = data[0];
        initTwilio(twilioToken);

        var number = data[1];
        operator = number.settings.caller;

        var calls = data[2];

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

        renderOperatorBox();
        renderOptionsBox();
        renderMissedCalls();
        renderPhoneBox();
        renderCallBox();

        if (incomingCallStarted()) {
            showDropView();
        }
    }

    function renderOperatorBox() {
        $operatorStatusSwitcherOptions.html(operatorStatusSwitcherOptionsTmpl.tmpl());

        $operatorStatusSwitcherOptions.hide();
        $operatorName.text(user.displayName);

        if (operatorOnline()) {
            $operatorStatus.removeClass('offline').addClass('online');
            $operatorStatusSwitcher.text(ASC.Resources.Master.Resource.OnlineStatus);
            $operatorStatusSwitcher.removeClass('lock');
        } else if (operatorOffline()) {
            $operatorStatus.removeClass('online').addClass('offline');
            $operatorStatusSwitcher.text(ASC.Resources.Master.Resource.OfflineStatus);
            $operatorStatusSwitcher.removeClass('lock');
        } else if (operatorBusy() && callRunning()) {
            $operatorStatus.removeClass('offline').addClass('online');
            $operatorStatusSwitcher.text(ASC.Resources.Master.Resource.BusyStatus);
            $operatorStatusSwitcher.addClass('lock');
        }

        $operatorBox.show();
    }

    function renderOptionsBox() {
        if (!operatorAllowOutgoingCalls()) {
            return;
        }

        if (callRunning()) {
            $optionsBox.hide();
            return;
        }

        var $box = optionsBoxTmpl.tmpl({
            answer: operator.answer,
            redirectToNumber: operator.redirectToNumber,
            phones: user.contacts.telephones
        });
        $optionsBox.html($box);

        var $toggleBtn = $optionsBox.find('#toggle-phone-box-btn');
        if (operatorAnswersByPhone()) {
            $toggleBtn.hide();
        } else {
            $toggleBtn.show();
        }

        if (operatorOffline()) {
            $optionsBox.hide();
        } else {
            $optionsBox.show();
        }
    }

    function renderMissedCalls() {
        if (callRunning()) {
            $missedCallsBox.hide();
            return;
        }

        if (operatorOffline() || operatorAnswersByPhone()) {
            $missedCallsBox.hide();
        } else {
            $missedCallsBox.show();
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

            var $calls = missedCallTmpl.tmpl(missedCalls);
            $missedCallsList.append($calls);
        }
    }

    function renderPhoneBox() {
        if (!initiated) {
            var $panel = countriesPanelTmpl.tmpl({ countries: countries });
            $phoneSelectorBox.append($panel);
        }

        if (operatorOffline() || callRunning() || operatorAnswersByPhone()) {
            $phoneBox.hide();
            return;
        }
    }

    function renderCallBox() {
        if (!callRunning()) {
            $callBox.hide();
            return;
        }

        $callTimer.empty();

        $callStatuses.hide().filter('[data-status=' + currentCallStatus + ']').show();
        $callBtns.hide().filter('[data-status=' + currentCallStatus + ']').show();

        if (incomingCallGoing()) {
            $operatorsRedirectBox.show();
        } else {
            $operatorsRedirectBox.hide();
        }

        var $call = callTmpl.tmpl(currentCall);
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

            signalrConnection.server.status(status)
                .done(function() {
                    hideLoader();
                }).fail(function() {
                    $operatorStatusSwitcherOptions.hide();
                    showErrorMessage();
                    hideLoader();
                });
        }
    }

    function switchOperatorStatus() {
        if (operatorBusy() && callRunning()) {
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

    function togglePhoneBox() {
        var visible = $phoneBox.is(':visible');

        if (visible) {
            $missedCallsBox.show();
            $phoneBox.hide();
        } else {
            $missedCallsBox.hide();
            $callBox.hide();
            $phoneBox.show();
        }
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

        var phone = contact.phone[0].data;
        renderSelectedPhone(phone);

        var $items = contactPhoneSwitcherItemTmpl.tmpl(contact.phone);
        $contactPhoneSwitcherPanel.find('.dropdown-content').html($items);

        $phoneClearBtn.show();
        $selectedContactPhoneSwitcherBtn.show();
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
        var phone = $phoneInput.val().replace(/\s/g, '');

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
            to: $phoneInput.val().replace(/\s/g, ''),
            contactId: selectedContact ? selectedContact.id : null
        };

        Teamlab.callVoipNumber(null, query, {
            success: function(params, call) {
                currentCall = call;

                setOperatorBusy();
                setOutgoingCallStartedStatus();

                $callBtn.removeClass('disable');
                renderView();
            },
            error: function() {
                $callBtn.removeClass('disable');
                showErrorMessage();
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
        $missedCallsList.addClass('lock');

        var number = $this.attr('data-number');
        Teamlab.callVoipNumber(null, { to: number }, {
            success: function(params, call) {
                currentCall = call;

                setOperatorBusy();
                setOutgoingCallStartedStatus();

                $this.removeClass('disable');
                $missedCallsList.removeClass('lock');
                renderView();

                deleteMissedCalls(number);
                $missedCallsList.find('.missed-call[data-number="' + number + '"]').remove();
            },
            error: function() {
                $this.removeClass('disable');
                $this.closest('.missed-call').removeClass('selected');
                $missedCallsList.removeClass('lock');
                showErrorMessage();
            }
        });
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
                setIncomingCallCompletedStatus();
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
                    setIncomingCallCompletedStatus();
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
            setIncomingCallCompletedStatus();

            renderView();
            resetCall();
        }
    }

    //#endregion

    //#region push handlers

    function operatorStatusUpdated(status) {
        operator.status = status;

        if (operatorBusy()) {
            renderOperatorBox();
            return;
        }

        renderView();
    }

    function onlineOperatorsUpdated(ids) {
        var operators = getUsers(ids);

        if (operators && operators.length) {
            var $operators = operatorsRedirectOptionTmpl.tmpl(operators);
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

    function incomingCallInitiated(call) {
        currentCall = Teamlab.create('crm-voipCall', null, $.parseJSON(call));

        setIncomingCallStartedStatus();
        renderView();
    }

    function callGoing() {
        if (incomingCallStarted()) {
            setIncomingCallGoingStatus();
        } else if (outgoingCallStarted()) {
            setOutgoingCallGoingStatus();
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

        if (incomingCallGoing()) {
            setIncomingCallCompletedStatus();
        } else if (outgoingCallGoing()) {
            setOutgoingCallCompletedStatus();
        } else {
            return;
        }

        renderView();
        resetCall();
    }

    function callMissed(call) {
        call = Teamlab.create('crm-voipCall', null, $.parseJSON(call));

        var res = addMissedCall(call);

        var $call = missedCallTmpl.tmpl(res.call);
        if (res.repeatedCall) {
            $missedCallsList.find('.missed-call:first').replaceWith($call);
        } else {
            $missedCallsList.prepend($call);
        }

        resetCall(0);
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

        if (incomingCallPlayer && incomingCallPlayer.readyState != 0) {
            incomingCallPlayer.pause();
            incomingCallPlayer.currentTime = 0;
        }

        $operatorStatusSwitcher.removeClass('lock');
        clearPhoneBox();
    }

    //#endregion

    //#region twilio handlers

    function incomingTwilioHandler(connection) {
        twilioConnection = connection;
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
        return !!Modernizr.audio.mp3;
    }

    function supportsVAW() {
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
        init: init
    };
};

jq(function() {
    window.VoipPhoneView.init();
});