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

ASC.CRM.Voip.QuickView = (function ($) {
    var buyPhonePopupInit = false,
        linkPhonePopupInit = false,
        numbers = [],
        operators = [];

    var $view,

        $showBuyPhonePopupBtn,
        $showLinkPhonePopupBtn,
        $linkPhonePopup,
        $buyPhonePopup,
        $removeNumberPopup,

        $countrySelectorBox,

        $countrySelector,
        $countryCode,
        $countryInput,
        $countryInputClearBtn,

        $countrySelectorSearchBtn,

        $availableNumbers,
        $availableNumbersLoader,
        $availableNumbersEmptyMsg,
        $availableNumbersEmptySearchMsg,

        $buyPhoneBtn,
        $cancelBuyPhoneBtn,

        $existingNumbers,
        $existingNumbersLoader,
        $existingNumbersEmptyMsg,

        $linkPhoneBtn,
        $cancelLinkPhoneBtn,

        $existingNumbersEmptyBox,
        $existingNumbersList,

        $emptyBuyPhoneBtn,
        $emptyLinkPhoneBtn;

    var loadingBanner = LoadingBanner,
        clickEventName = "click";

    function init () {
        cacheElements();
        bindEvents();

        showLoader();

        getData(function(numbersResp) {
            saveData(numbersResp);
            renderView();

            hideLoader();
        });
    };

    function cacheElements() {
        $view = $('#voip-quick-view');

        $showBuyPhonePopupBtn = $view.find('#show-buy-phone-popup-btn');
        $showLinkPhonePopupBtn = $view.find('#show-link-phone-popup-btn');
        $buyPhonePopup = $view.find('#buy-phone-popup');
        $linkPhonePopup = $view.find('#link-phone-popup');
        $removeNumberPopup = $view.find('#remove-number-popup');

        $countrySelectorBox = $buyPhonePopup.find('#country-selector-box');

        $countrySelector = $countrySelectorBox.find('#country-selector');
        $countryCode = $countrySelectorBox.find('#country-code');
        $countryInput = $countrySelectorBox.find('#country-input');
        $countryInputClearBtn = $countrySelectorBox.find('#country-input-clear-btn');

        $countrySelectorSearchBtn = $buyPhonePopup.find('#country-selector-search-btn');

        $availableNumbers = $buyPhonePopup.find('.numbers');
        $availableNumbersLoader = $buyPhonePopup.find('.numbers-loader');
        $availableNumbersEmptyMsg = $buyPhonePopup.find('.numbers-empty-msg');
        $availableNumbersEmptySearchMsg = $buyPhonePopup.find('#available-numbers-empty-search-msg');

        $buyPhoneBtn = $buyPhonePopup.find('.phone-btn');
        $cancelBuyPhoneBtn = $buyPhonePopup.find('.cancel-btn');

        $existingNumbers = $linkPhonePopup.find('.numbers');
        $existingNumbersLoader = $linkPhonePopup.find('.numbers-loader');
        $existingNumbersEmptyMsg = $linkPhonePopup.find('.numbers-empty-msg');

        $linkPhoneBtn = $linkPhonePopup.find('.phone-btn');
        $cancelLinkPhoneBtn = $linkPhonePopup.find('.cancel-btn');

        $existingNumbersEmptyBox = $view.find('#existing-numbers-empty-box');
        $existingNumbersList = $view.find('#existing-numbers-list');

        var resource = ASC.CRM.Resources.CRMVoipResource;
        var describe = [
            Encoder.htmlEncode(resource.EmptyScreenNumberDescription1),
            "<br/><br/>",
            Encoder.htmlEncode(resource.EmptyScreenNumberDescription2)
        ].join('');

        $existingNumbersEmptyBox.append(jq.tmpl("template-emptyScreen",
        {
            ID: "phones-empty-screen",
            ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_phones"],
            Header: resource.EmptyScreenNumberHeader,
            Describe: describe,
            ButtonHTML: jq.format("<a class='link dotline plus' id=\"empty-buy-phone-btn\">{0}</a><br/><a class='link dotline plus' id=\"empty-link-phone-btn\">{1}</a>", resource.BuyNumberBtn, resource.LinkNumberBtn)
        }));

        $emptyBuyPhoneBtn = $view.find('#empty-buy-phone-btn');
        $emptyLinkPhoneBtn = $view.find('#empty-link-phone-btn');
    };

    function bindEvents() {
        $('body').on(clickEventName, clickHandler);

        $showBuyPhonePopupBtn.add($emptyBuyPhoneBtn).on(clickEventName, showBuyPhonePopupHandler);
        $showLinkPhonePopupBtn.add($emptyLinkPhoneBtn).on(clickEventName, showLinkPhonePopupHandler);

        $countrySelectorBox.on(clickEventName, '.studio-action-panel .dropdown-item', countryChangedHandler);
        $countrySelector.on(clickEventName, countrySelectorToggleHandler);

        $countryInput.on('keyup', countryInputKeyupHandler);
        $countryInputClearBtn.on(clickEventName, countryInputClearHandler);
        $countrySelectorSearchBtn.on(clickEventName, countrySelectorSearchHandler);

        $existingNumbersList.on(clickEventName, '.number-box .actions', toggleNumberActionsHandler);
        $existingNumbersList.on(clickEventName, '.operators-box .actions', toggleOperatorActionsHandler);

        $existingNumbersList.on(clickEventName, '.number .number-box .outgoing-calls .on_off_button', numberOutgoingCallsUpdatedHandler);
        $existingNumbersList.on(clickEventName, '.number .number-box .voicemail .on_off_button', numberVoicemailUpdatedHandler);
        $existingNumbersList.on(clickEventName, '.number .number-box .recording-calls .on_off_button', numberRecordingCallsUpdatedHandler);
        $existingNumbersList.on(clickEventName, '.number .number-box .show-remove-number-btn', showNumberRemovePopup);
        $existingNumbersList.on(clickEventName, '.number .number-box .edit-number-btn', editNumber);

        $existingNumbersList.on(clickEventName, '.number .switcher', toggleOperatorsBoxHandler);

        $existingNumbersList.on(clickEventName, '.number .operator .outgoing-calls .on_off_button', operatorOutgoingCallsUpdatedHandler);
        $existingNumbersList.on(clickEventName, '.number .operator .recording-calls .on_off_button', operatorRecordingCallsUpdatedHandler);

        $existingNumbersList.on('showList', '.number .operators-box .add-operators-btn', addOperatorsHandler);
        $existingNumbersList.on(clickEventName, '.number .operators-box .delete-operator-btn', deleteOperatorHandler);


        $availableNumbers.on(clickEventName, '.number input', numberSelectedHandler);

        $buyPhoneBtn.on(clickEventName, buyNumberHandler);
        $cancelBuyPhoneBtn.on(clickEventName, cancelBuyNumberHandler);

        $existingNumbers.on(clickEventName, '.number input', existingNumberSelectedHandler);

        $linkPhoneBtn.on(clickEventName, linkNumberHandler);
        $cancelLinkPhoneBtn.on(clickEventName, cancelLinkPhoneHandler);
    };

    //#region data

    function getData(cb) {
        showLoader();

        Teamlab.getCrmVoipExistingNumbers(null, {
            success: function(params, numbersResp) {
                hideLoader();
                cb(numbersResp);
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    };

    function saveData(numbersResp) {
        if (!numbersResp || !numbersResp.length) {
            $existingNumbersEmptyBox.removeClass("display-none");
            return;
        }

        numbers = numbersResp;

        for (var i = 0; i < numbersResp.length; i++) {
            var number = numbersResp[i];
            for (var j = 0; j < number.settings.operators.length; j++) {
                operators.push(number.settings.operators[j].id);
            }
        }
    };

    function saveNumber(number) {
        for (var i = 0; i < numbers.length; i++) {
            if (numbers[i].id == number.id) {
                numbers[i] = number;
                return;
            }
        }
    };

    function deleteNumber(numberId) {
        numbers = numbers.filter(function (item) { return item.id !== numberId });
    };

    function addOperators(numberId, operatorsResp) {
        if (!operatorsResp || !operatorsResp.length) {
            return;
        }

        var result = null;
        for (var i = 0; i < numbers.length; i++) {
            if (numbers[i].id == numberId) {
                result = numbers[i];
                break;
            }
        }

        if (!result) return;

        operatorsResp = setOperatorsUserInfo(operatorsResp);
        result.settings.operators = operatorsResp;

        for (i = 0; i < operatorsResp.length; i++) {
            operators.push(operatorsResp[i].id);
        }

        return result;
    };

    function deleteOperator(operatorId) {
        for (var i = 0; i < operators.length; i++) {
            if (operators[i] == operatorId) {
                operators.splice(i, 1);
                return;
            }
        }
    };

    //#endregion

    //#region rendering

    function renderView() {
        renderExistingNumbers();
        $view.show();
    };

    function renderExistingNumbers() {
        if (!numbers.length) {
            $existingNumbersEmptyBox.show();
            return;
        }

        $showBuyPhonePopupBtn.removeClass("display-none");
        $showLinkPhonePopupBtn.removeClass("display-none");

        var $numbers = [];
        for (var i = 0; i < numbers.length; i++) {
            numbers[i].settings.operators = setOperatorsUserInfo(numbers[i].settings.operators);
            $numbers.push(renderExistingNumber(numbers[i]));
        }

        $existingNumbersList.append($numbers);
        $existingNumbersList.show();
    };

    function renderExistingNumber(number) {
        var $number = jq.tmpl("voip-existing-number-tmpl", number);
        var $addOperatorsBtn = $number.find('.add-operators-btn');

        $addOperatorsBtn.useradvancedSelector({ showGroups: true });
        $addOperatorsBtn.useradvancedSelector('disable', operators);
        return $number;
    };

    function renderAddedOperators(number) {
        var $operators = jq.tmpl("voip-operators-tmpl", number);
        $existingNumbersList.find('#enumber' + number.id + ' .operators-box').append($operators);

        var $addOperatorBtn = $view.find('.add-operators-btn');
        $addOperatorBtn.useradvancedSelector('disable', operators);
    };

    //#endregion

    //#region handlers

    function clickHandler(e) {
        var $this = $(e.target);

        if (!$this.is('.actions')) {
            clearActionPanel();
        }

        if (!$this.is('#country-selector')) {
            $buyPhonePopup.find('#country-selector-box .studio-action-panel').hide();
        }
    };

    function clearActionPanel() {
        $view.find('.number-box').removeClass('selected').removeClass('operator-selected');
        $view.find('.operator').removeClass('selected');
        $view.find('.studio-action-panel').hide();
    };

    function showBuyPhonePopupHandler() {
        if (!buyPhonePopupInit) {
            var $countriesList = jq.tmpl("voip-countries-list-tmpl", { countries: ASC.Voip.Countries });
            $countrySelector.after($countriesList);
            renderAvailableNumbers('US', 1);

            buyPhonePopupInit = true;
        }

        StudioBlockUIManager.blockUI($buyPhonePopup, 550);
    };

    function showLinkPhonePopupHandler() {
        if (!linkPhonePopupInit) {
            $linkPhoneBtn.addClass('disable');

            $existingNumbers.empty();
            $existingNumbersEmptyMsg.hide();
            $existingNumbersLoader.show();

            Teamlab.getCrmVoipUnlinkedNumbers({}, {
                success: function (params, numbersResp) {
                    $existingNumbersLoader.hide();

                    if (numbersResp.length) {
                        var $numbers = jq.tmpl("voip-available-number-tmpl", numbersResp);

                        $existingNumbers.html($numbers);
                    } else {
                        $existingNumbersEmptyMsg.show();
                    }
                },
                error: function () {
                    $existingNumbersLoader.hide();
                    showErrorMessage();
                }
            });

            linkPhonePopupInit = true;
        }

        StudioBlockUIManager.blockUI($linkPhonePopup, 550);
    };

    function countrySelectorToggleHandler() {
        $countrySelector.siblings('.studio-action-panel').toggle();
    };

    function countryInputKeyupHandler() {
        var text = $countryInput.val();
        if (text) {
            $countryInputClearBtn.show();
        } else {
            $countryInputClearBtn.hide();
        }

        countrySelectorSearchHandler();
    };

    function countryInputClearHandler() {
        $countryInput.val('');
        $countryInputClearBtn.hide();

        countrySelectorSearchHandler();
    };

    function countrySelectorSearchHandler() {
        var text = $countryInput.val();
        var seachText = $countryCode.text() + text;

        var exist = false;
        $availableNumbers.find('.number').each(function() {
            var $el = $(this);
            var number = $el.find('.number-value').text();

            if (number.indexOf(seachText) != 0) {
                $el.hide();
            } else {
                exist = true;
                $el.show();
            }
        });

        if (!exist) {
            $availableNumbersEmptySearchMsg.show();
        } else {
            $availableNumbersEmptySearchMsg.hide();
        }
    };

    function toggleNumberActionsHandler(e) {
        var $this = $(e.target);
        var $panel = $this.find('.studio-action-panel');
        var $number = $this.closest('.number-box');

        var visible = $panel.is(':visible');
        clearActionPanel();

        if (visible) {
            $panel.hide();
            $number.removeClass('selected');
        } else {
            $panel.css({
                top: $this.outerHeight(),
                left: $this.width() - $panel.width()
            });
            $panel.show();
            $number.addClass('selected');
        }
    };

    function toggleOperatorActionsHandler(e) {
        var $this = $(e.target);
        var $panel = $this.find('.studio-action-panel');
        var $operator = $this.closest('.operator');
        var $number = $this.closest('.number').find('.number-box');

        var visible = $panel.is(':visible');
        clearActionPanel();

        if (visible) {
            $panel.hide();
            $number.removeClass('operator-selected');
            $operator.removeClass('selected');
        } else {
            $panel.css({
                top: $this.outerHeight(),
                left: $this.width() - $panel.width()
            });
            $panel.show();
            $number.addClass('operator-selected');
            $operator.addClass('selected');
        }
    };

    function numberOutgoingCallsUpdatedHandler(e) {
        var on = $(e.target).is('.off');
        var setting = { allowOutgoingCalls: on };

        numberSettinglUpdatedHandler(e, setting);
    };

    function numberVoicemailUpdatedHandler(e) {
        var on = $(e.target).is('.off');
        var setting = { voiceMail: { enabled: on } };

        numberSettinglUpdatedHandler(e, setting);
    };

    function numberRecordingCallsUpdatedHandler(e) {
        var on = $(e.target).is('.off');
        var setting = { record: on };

        numberSettinglUpdatedHandler(e, setting);
    };

    function editNumber(e) {
        var $this = $(e.target).closest('.number');
        var numberId = $this.attr('data-numberId');
        window.open("Settings.aspx?type=voip.numbers#" + numberId);
    }

    function showNumberRemovePopup(e) {
        $removeNumberPopup.off(clickEventName)
            .on(clickEventName, "#remove-number-btn", numberRemoveHandler.bind(null, e.target))
            .on(clickEventName, "#cancel-remove-phone-btn", cancelNumberRemoveHandler);
        StudioBlockUIManager.blockUI($removeNumberPopup, 550);
    }

    function cancelNumberRemoveHandler() {
        jq.unblockUI();
    }

    function numberRemoveHandler(target) {
        var $this = $(target).closest('.number');
        var numberId = $this.attr('data-numberId');

        Teamlab.removeCrmVoipNumber({}, numberId, {
            after: jq.unblockUI,
            success: function (params, number) {
                deleteNumber(numberId);

                if (numbers.length) {
                    $this.remove();

                    var $addOperatorBtns = $view.find('.add-operators-btn');
                    number.settings.operators.forEach(function (item) {
                        deleteOperator(item.id);
                        $addOperatorBtns.useradvancedSelector('undisable', [item.id]);
                    });

                } else {
                    location.reload();
                }
            },
            error: showErrorMessage
        });
    };

    function numberSettinglUpdatedHandler(e, setting) {
        var $this = $(e.target);

        var numberId = $this.closest('.number').attr('data-numberId');
        var on = $this.is('.off');

        var operatorBtnSelector = $this.attr('data-operator-btn-selector');

        showLoader();

        Teamlab.updateCrmVoipNumberSettings(null, numberId, setting, {
            success: function(params, number) {
                saveNumber(number);
                if (on) {
                    $this.removeClass('off').addClass('on');
                    if (operatorBtnSelector) {
                        $this.closest('.number').find('.operators-box ' + operatorBtnSelector + ' .on_off_button')
                            .removeClass('disable');
                    }
                } else {
                    $this.removeClass('on').addClass('off');
                    if (operatorBtnSelector) {
                        $this.closest('.number').find('.operators-box ' + operatorBtnSelector + ' .on_off_button')
                            .removeClass('on').addClass('off').addClass('disable');
                    }
                }

                hideLoader();
                showSuccessOpearationMessage();
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    };

    function operatorOutgoingCallsUpdatedHandler(e) {
        var on = $(e.target).is('.off');
        var setting = { allowOutgoingCalls: on };

        operatorSettingUpdatedHandler(e, setting);
    };

    function operatorRecordingCallsUpdatedHandler(e) {
        var on = $(e.target).is('.off');
        var setting = { record: on };

        operatorSettingUpdatedHandler(e, setting);
    };

    function operatorSettingUpdatedHandler(e, setting) {
        var $this = $(e.target);
        if ($this.is('.disable')) {
            return;
        }

        var operatorId = $this.closest('.operator').attr('data-operatorId');
        var on = $this.is('.off');

        showLoader();

        Teamlab.updateCrmVoipOperator(null, operatorId, setting, {
            success: function() {
                if (on) {
                    $this.removeClass('off').addClass('on');
                } else {
                    $this.removeClass('on').addClass('off');
                }

                hideLoader();
                showSuccessOpearationMessage();
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    };

    function addOperatorsHandler(e, addedOperators) {
        var ids = addedOperators.map(function(o) {
            return o.id;
        });

        if (!ids.length) {
            return;
        }

        var numberId = $(e.target).closest('.number').attr('data-numberid');

        showLoader();
        Teamlab.addCrmVoipNumberOperators(null, numberId, { operators: ids }, {
            success: function(params, operatorsResp) {
                var number = addOperators(numberId, operatorsResp);
                renderAddedOperators(number);

                hideLoader();
            },
            error: function() {
                showErrorMessage();
                hideLoader();
            }
        });
    };

    function deleteOperatorHandler(e) {
        var numberId = $(e.target).closest('.number').attr('data-numberid');
        var operatorId = $(e.target).closest('.operator').attr('data-operatorid');

        showLoader();

        Teamlab.removeCrmVoipNumberOperators(null, numberId, { oper: operatorId }, {
            success: function() {
                deleteOperator(operatorId);
                $existingNumbersList.find('#enumber' + numberId + ' .operator[data-operatorid=' + operatorId + ']').remove();

                hideLoader();

                var $addOperatorBtns = $view.find('.add-operators-btn');
                $addOperatorBtns.useradvancedSelector('undisable', [operatorId]);
            },
            error: function() {
                hideLoader();
                showErrorMessage();
            }
        });
    };

    function countryChangedHandler(e) {
        var $this = $(e.currentTarget).find('.voip-flag');

        var iso = $this.attr('data-iso');
        var code = $this.attr('data-code');

        renderAvailableNumbers(iso, code);
    };

    function numberSelectedHandler() {
        $buyPhoneBtn.removeClass('disable');
    };

    function existingNumberSelectedHandler() {
        $linkPhoneBtn.removeClass('disable');
    };

    function toggleOperatorsBoxHandler(e) {
        var $this = $(e.currentTarget);
        $this.closest('.number').find('.operators-box').toggle();
        $this.find('.expander-icon').toggleClass('open');
    };

    function renderAvailableNumbers(iso, code) {
        $buyPhoneBtn.addClass('disable');

        $countrySelector.attr('class', 'voip-flag link arrow-down ' + iso);
        $countryCode.text('+' + code);
        $countryInput.val('');
        $countryInput.hide();
        $countryInputClearBtn.hide();

        $availableNumbers.empty();
        $availableNumbersEmptyMsg.hide();
        $availableNumbersEmptySearchMsg.hide();
        $availableNumbersLoader.show();

        Teamlab.getCrmVoipAvailableNumbers(null, {
            filter: { numberType: 0, isoCountryCode: iso },
            success: function(params, numbersResp) {
                $availableNumbersLoader.hide();

                if (numbersResp.length) {
                    var $numbers = jq.tmpl("voip-available-number-tmpl", numbersResp);

                    $availableNumbers.empty();
                    $availableNumbers.append($numbers);
                } else {
                    $availableNumbersEmptyMsg.show();
                }

                $countryInput.show();
            },
            error: function() {
                $availableNumbersLoader.hide();
                showErrorMessage();
            }
        });
    };

    function buyNumberHandler() {
        if ($buyPhoneBtn.is('.disable')) {
            return;
        }

        $buyPhoneBtn.addClass('disable');
        $cancelBuyPhoneBtn.addClass('disable');

        var $selectedNumber = $availableNumbers.find('input:checked');
        if (!$selectedNumber.length) {
            return;
        }

        var number = $selectedNumber.attr('data-number');

        Teamlab.createCrmVoipNumber(null,
        { number: number },
        {
            before: function () {
                loadingBanner.showLoaderBtn("#buy-phone-popup");
            },
            after: function () {
                loadingBanner.hideLoaderBtn("#buy-phone-popup");
            },
            success: function(params, addedNumber) {
                $existingNumbersEmptyBox.hide();
                var $number = renderExistingNumber(addedNumber);
                numbers.push(addedNumber);

                $availableNumbers.find('#anumber\\' + number).remove();
                $existingNumbersList.append($number);
                $existingNumbersList.show();
                $showBuyPhonePopupBtn.removeClass("display-none");
                $showLinkPhonePopupBtn.removeClass("display-none");

                $cancelBuyPhoneBtn.removeClass('disable');
                $.unblockUI();
                toastr.success(ASC.Resources.Master.Resource.BuyNumberSuccessMsg);
            },
            error: function() {
                $cancelBuyPhoneBtn.removeClass('disable');
                showErrorMessage();
            }
        });
    };

    function linkNumberHandler() {
        if ($linkPhoneBtn.is('.disable')) {
            return;
        }

        $linkPhoneBtn.addClass('disable');
        $cancelLinkPhoneBtn.addClass('disable');

        var $selectedNumber = $existingNumbers.find('input:checked');
        if (!$selectedNumber.length) {
            return;
        }

        var number = $selectedNumber.attr('data-number');
        var numberId = $selectedNumber.attr('data-numberId');

        Teamlab.linkCrmVoipNumber(null,
        { number: number, id: numberId },
        {
            before: function () {
                loadingBanner.showLoaderBtn("#link-phone-popup");
            },
            after: function () {
                loadingBanner.hideLoaderBtn("#link-phone-popup");
            },
            success: function(params, addedNumber) {
                $existingNumbersEmptyBox.hide();
                var $number = renderExistingNumber(addedNumber);
                numbers.push(addedNumber);

                $existingNumbers.find('#anumber\\' + number).remove();
                $existingNumbersList.append($number);
                $existingNumbersList.show();
                $showBuyPhonePopupBtn.removeClass("display-none");
                $showLinkPhonePopupBtn.removeClass("display-none");

                $cancelLinkPhoneBtn.removeClass('disable');
                $.unblockUI();
                toastr.success(ASC.CRM.Resources.CRMVoipResource.LinkNumberSuccessMsg);
            },
            error: function() {
                $cancelLinkPhoneBtn.removeClass('disable');
                showErrorMessage();
            }
        });
    };

    function cancelBuyNumberHandler() {
        if ($cancelBuyPhoneBtn.is('.disable')) {
            return;
        }
        $.unblockUI();
    };

    function cancelLinkPhoneHandler() {
        if ($cancelLinkPhoneBtn.is('.disable')) {
            return;
        }
        $.unblockUI();
    };

    function setOperatorsUserInfo(operatorsResp) {
        if (!operatorsResp || !operatorsResp.length) {
            return operatorsResp;
        }

        for (var i = 0; i < operatorsResp.length; i++) {
            operatorsResp[i].userInfo = window.UserManager.getUser(operatorsResp[i].id);
        }

        return operatorsResp.filter(function (item) { return item.userInfo !== null; });
    };

    //#endregion

    //#region utils
    function showLoader() {
        loadingBanner.displayLoading();
    };

    function hideLoader() {
        loadingBanner.hideLoading();
    };

    function showSuccessOpearationMessage() {
        toastr.success(ASC.Resources.Master.Resource.ChangesSuccessfullyAppliedMsg);
    };

    function showErrorMessage() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }

    //#endregion


    return {
        init: init
    };
})(jq);