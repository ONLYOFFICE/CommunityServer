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


var $ = jq;

var VoIPQuickView = {
    buyPhonePopupInit: false,

    numbers: [],
    operators: [],

    init: function() {
        this.cacheElements();
        this.bindEvents();

        this.showLoader();

        var self = this;
        this.getData(function(numbers) {
            self.saveData(numbers);
            self.renderView();

            self.hideLoader();
        });
    },

    cacheElements: function() {
        this.countriesListTmpl = $('#countries-list-tmpl');

        this.operatorsTmpl = $('#operators-tmpl');
        this.existingNumberTmpl = $('#existing-number-tmpl');

        this.availableNumberTmpl = $('#available-number-tmpl');

        this.$view = $('#voip-quick-view');

        this.$showBuyPhonePopupBtn = this.$view.find('#show-buy-phone-popup-btn');
        this.$buyPhonePopup = this.$view.find('#buy-phone-popup');

        this.$countrySelectorBox = this.$buyPhonePopup.find('#country-selector-box');

        this.$countrySelector = this.$countrySelectorBox.find('#country-selector');
        this.$countryCode = this.$countrySelectorBox.find('#country-code');
        this.$countryInput = this.$countrySelectorBox.find('#country-input');
        this.$countryInputClearBtn = this.$countrySelectorBox.find('#country-input-clear-btn');

        this.$countrySelectorSearchBtn = this.$buyPhonePopup.find('#country-selector-search-btn');

        this.$availableNumbers = this.$view.find('#available-numbers');
        this.$availableNumbersLoader = this.$view.find('#available-numbers-loader');
        this.$availableNumbersEmptyMsg = this.$view.find('#available-numbers-empty-msg');
        this.$availableNumbersEmptySearchMsg = this.$view.find('#available-numbers-empty-search-msg');

        this.$buyPhoneBtn = this.$view.find('#buy-phone-btn');
        this.$cancelBuyPhoneBtn = this.$view.find('#cancel-buy-phone-btn');
        this.$buyPhoneLoader = this.$view.find('#buy-phone-loader');

        this.$existingNumbersEmptyBox = this.$view.find('#existing-numbers-empty-box');
        this.$existingNumbersList = this.$view.find('#existing-numbers-list');
    },

    bindEvents: function() {
        $('body').on('click', this.clickHandler.bind(this));

        this.$showBuyPhonePopupBtn.on('click', this.showBuyPhonePopupHandler.bind(this));

        this.$countrySelectorBox.on('click', '.studio-action-panel .dropdown-item', this.countryChangedHandler.bind(this));
        this.$countrySelector.on('click', this.countrySelectorToggleHandler.bind(this));

        this.$countryInput.on('keyup', this.countryInputKeyupHandler.bind(this));
        this.$countryInputClearBtn.on('click', this.countryInputClearHandler.bind(this));
        this.$countrySelectorSearchBtn.on('click', this.countrySelectorSearchHandler.bind(this));

        this.$existingNumbersList.on('click', '.number-box .actions', this.toggleNumberActionsHandler.bind(this));
        this.$existingNumbersList.on('click', '.operators-box .actions', this.toggleOperatorActionsHandler.bind(this));

        this.$existingNumbersList.on('click', '.number .number-box .outgoing-calls .on_off_button', this.numberOutgoingCallsUpdatedHandler.bind(this));
        this.$existingNumbersList.on('click', '.number .number-box .voicemail .on_off_button', this.numberVoicemailUpdatedHandler.bind(this));
        this.$existingNumbersList.on('click', '.number .number-box .recording-calls .on_off_button', this.numberRecordingCallsUpdatedHandler.bind(this));

        this.$existingNumbersList.on('click', '.number .switcher', this.toggleOperatorsBoxHandler.bind(this));

        this.$existingNumbersList.on('click', '.number .operator .outgoing-calls .on_off_button', this.operatorOutgoingCallsUpdatedHandler.bind(this));
        this.$existingNumbersList.on('click', '.number .operator .recording-calls .on_off_button', this.operatorRecordingCallsUpdatedHandler.bind(this));

        this.$existingNumbersList.on('showList', '.number .operators-box .add-operators-btn', this.addOperatorsHandler.bind(this));
        this.$existingNumbersList.on('click', '.number .operators-box .delete-operator-btn', this.deleteOperatorHandler.bind(this));


        this.$availableNumbers.on('click', '.number input', this.numberSelectedHandler.bind(this));

        this.$buyPhoneBtn.on('click', this.buyNumberHandler.bind(this));
        this.$cancelBuyPhoneBtn.on('click', this.cancelBuyNumberHandler.bind(this));
    },

    //#region data

    getData: function(cb) {
        this.showLoader();

        var self = this;
        Teamlab.getCrmVoipExistingNumbers(null, {
            success: function(params, numbers) {
                self.hideLoader();
                cb(numbers);
            },
            error: function() {
                self.hideLoader();
                self.showErrorMessage();
            }
        });
    },

    saveData: function(numbers) {
        if (!numbers || !numbers.length) {
            return;
        }

        this.numbers = numbers;

        for (var i = 0; i < numbers.length; i++) {
            var number = numbers[i];
            for (var j = 0; j < number.settings.operators.length; j++) {
                this.operators.push(number.settings.operators[j].id);
            }
        }
    },
    
    saveNumber: function(number) {
        for (var i = 0; i < this.numbers.length; i++) {
            if (this.numbers[i].id == number.id) {
                this.numbers[i] = number;
                return;
            }
        }
    },

    addOperators: function(numberId, operators) {
        if (!operators || !operators.length) {
            return;
        }

        var result = null;
        for (var i = 0; i < this.numbers.length; i++) {
            if (this.numbers[i].id == numberId) {
                result = this.numbers[i];
                break;
            }
        }

        if (!result) return;

        this.setOperatorsUserInfo(operators);
        result.settings.operators = operators;

        for (i = 0; i < operators.length; i++) {
            this.operators.push(operators[i].id);
        }

        return result;
    },

    deleteOperator: function(operatorId) {
        for (var i = 0; i < this.operators.length; i++) {
            if (this.operators[i] == operatorId) {
                this.operators.splice(i, 1);
                return;
            }
        }
    },
    
    //#endregion

    //#region rendering

    renderView: function() {
        this.renderExistingNumbers();
        this.$view.show();
    },

    renderExistingNumbers: function() {
        if (!this.numbers.length) {
            this.$existingNumbersEmptyBox.show();
            return;
        }

        var $numbers = [];
        for (var i = 0; i < this.numbers.length; i++) {
            this.setOperatorsUserInfo(this.numbers[i].settings.operators);

            var $number = this.existingNumberTmpl.tmpl(this.numbers[i]);
            var $addOperatorsBtn = $number.find('.add-operators-btn');

            $addOperatorsBtn.useradvancedSelector({ showGroups: true });
            $addOperatorsBtn.useradvancedSelector('disable', this.operators);

            $numbers.push($number);
        }

        this.$existingNumbersList.append($numbers);
        this.$existingNumbersList.show();
    },

    renderAddedOperators: function(number) {
        var $operators = this.operatorsTmpl.tmpl(number);
        this.$existingNumbersList.find('#enumber' + number.id + ' .operators-box').append($operators);

        var $addOperatorBtn = this.$view.find('.add-operators-btn');
        $addOperatorBtn.useradvancedSelector('disable', this.operators);
    },
    
    //#endregion
    
    //#region handlers

    clickHandler: function(e) {
        var $this = $(e.target);

        if (!$this.is('.actions')) {
            this.clearActionPanel();
        }

        if (!$this.is('#country-selector')) {
            this.$buyPhonePopup.find('#country-selector-box .studio-action-panel').hide();
        }
    },

    clearActionPanel: function() {
        this.$view.find('.number-box').removeClass('selected').removeClass('operator-selected');
        this.$view.find('.operator').removeClass('selected');
        this.$view.find('.studio-action-panel').hide();
    },

    showBuyPhonePopupHandler: function() {
        if (!this.buyPhonePopupInit) {
            var $countriesList = this.countriesListTmpl.tmpl({ countries: window.VoIPCountries });
            this.$countrySelector.after($countriesList);
            this.renderAvailableNumbers('US', 1);

            this.buyPhonePopupInit = true;
        }

        StudioBlockUIManager.blockUI(this.$buyPhonePopup, 550, 550, 0, 'absolute');
    },

    countrySelectorToggleHandler: function() {
        this.$countrySelector.siblings('.studio-action-panel').toggle();
    },

    countryInputKeyupHandler: function() {
        var text = this.$countryInput.val();
        if (text) {
            this.$countryInputClearBtn.show();
        } else {
            this.$countryInputClearBtn.hide();
        }

        this.countrySelectorSearchHandler();
    },

    countryInputClearHandler: function() {
        this.$countryInput.val('');
        this.$countryInputClearBtn.hide();

        this.countrySelectorSearchHandler();
    },

    countrySelectorSearchHandler: function() {
        var text = this.$countryInput.val();
        var seachText = this.$countryCode.text() + text;

        var exist = false;
        this.$availableNumbers.find('.number').each(function() {
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
            this.$availableNumbersEmptySearchMsg.show();
        } else {
            this.$availableNumbersEmptySearchMsg.hide();
        }
    },

    toggleNumberActionsHandler: function(e) {
        var $this = $(e.target);
        var $panel = $this.find('.studio-action-panel');
        var $number = $this.closest('.number-box');

        var visible = $panel.is(':visible');
        this.clearActionPanel();

        if (visible) {
            $panel.hide();
            $number.removeClass('selected');
        } else {
            var offset = $this.offset();
            $panel.css({
                top: offset.top + 20,
                left: offset.left - $panel.width() + 26
            });
            $panel.show();
            $number.addClass('selected');
        }
    },

    toggleOperatorActionsHandler: function(e) {
        var $this = $(e.target);
        var $panel = $this.find('.studio-action-panel');
        var $operator = $this.closest('.operator');
        var $number = $this.closest('.number').find('.number-box');

        var visible = $panel.is(':visible');
        this.clearActionPanel();

        if (visible) {
            $panel.hide();
            $number.removeClass('operator-selected');
            $operator.removeClass('selected');
        } else {
            var offset = $this.offset();
            $panel.css({
                top: offset.top + 20,
                left: offset.left - $panel.width() + 26
            });
            $panel.show();
            $number.addClass('operator-selected');
            $operator.addClass('selected');
        }
    },

    numberOutgoingCallsUpdatedHandler: function(e) {
        var on = $(e.target).is('.off');
        var setting = { allowOutgoingCalls: on };

        this.numberSettinglUpdatedHandler(e, setting);
    },

    numberVoicemailUpdatedHandler: function(e) {
        var on = $(e.target).is('.off');
        var setting = { voiceMail: { enabled: on } };

        this.numberSettinglUpdatedHandler(e, setting);
    },

    numberRecordingCallsUpdatedHandler: function(e) {
        var on = $(e.target).is('.off');
        var setting = { record: on };

        this.numberSettinglUpdatedHandler(e, setting);
    },

    numberSettinglUpdatedHandler: function(e, setting) {
        var $this = $(e.target);

        var numberId = $this.closest('.number').attr('data-numberId');
        var on = $this.is('.off');

        var operatorBtnSelector = $this.attr('data-operator-btn-selector');

        this.showLoader();

        var self = this;
        Teamlab.updateCrmVoipNumberSettings(null, numberId, setting, {
            success: function(params, number) {
                self.saveNumber(number);
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

                self.hideLoader();
                self.showSuccessOpearationMessage();
            },
            error: function() {
                self.hideLoader();
                self.showErrorMessage();
            }
        });
    },

    operatorOutgoingCallsUpdatedHandler: function(e) {
        var on = $(e.target).is('.off');
        var setting = { allowOutgoingCalls: on };

        this.operatorSettingUpdatedHandler(e, setting);
    },
    
    operatorRecordingCallsUpdatedHandler: function(e) {
        var on = $(e.target).is('.off');
        var setting = { record: on };

        this.operatorSettingUpdatedHandler(e, setting);
    },

    operatorSettingUpdatedHandler: function(e, setting) {
        var $this = $(e.target);
        if ($this.is('.disable')) {
            return;
        }

        var operatorId = $this.closest('.operator').attr('data-operatorId');
        var on = $this.is('.off');

        this.showLoader();

        var self = this;
        Teamlab.updateCrmVoipOperator(null, operatorId, setting, {
            success: function() {
                if (on) {
                    $this.removeClass('off').addClass('on');
                } else {
                    $this.removeClass('on').addClass('off');
                }

                self.hideLoader();
                self.showSuccessOpearationMessage();
            },
            error: function() {
                self.hideLoader();
                self.showErrorMessage();
            }
        });
    },

    addOperatorsHandler: function(e, addedOperators) {
        var ids = addedOperators.map(function(o) {
            return o.id;
        });

        if (!ids.length) {
            return;
        }

        var self = this;
        var numberId = $(e.target).closest('.number').attr('data-numberid');

        this.showLoader();
        Teamlab.addCrmVoipNumberOperators(null, numberId, { operators: ids }, {
            success: function(params, operators) {
                var number = self.addOperators(numberId, operators);
                self.renderAddedOperators(number);

                self.hideLoader();
            },
            error: function() {
                self.showErrorMessage();
                self.hideLoader();
            }
        });
    },

    deleteOperatorHandler: function(e) {
        var numberId = $(e.target).closest('.number').attr('data-numberid');
        var operatorId = $(e.target).closest('.operator').attr('data-operatorid');

        this.showLoader();

        var self = this;
        Teamlab.removeCrmVoipNumberOperators(null, numberId, { oper: operatorId }, {
            success: function() {
                self.deleteOperator(operatorId);
                self.$existingNumbersList.find('#enumber' + numberId + ' .operator[data-operatorid=' + operatorId + ']').remove();

                self.hideLoader();

                var $addOperatorBtns = self.$view.find('.add-operators-btn');
                $addOperatorBtns.useradvancedSelector('undisable', [operatorId]);
            },
            error: function() {
                self.hideLoader();
                self.showErrorMessage();
            }
        });
    },

    countryChangedHandler: function(e) {
        var $this = $(e.currentTarget).find('.voip-flag');

        var iso = $this.attr('data-iso');
        var code = $this.attr('data-code');

        this.renderAvailableNumbers(iso, code);
    },

    numberSelectedHandler: function() {
        this.$buyPhoneBtn.removeClass('disable');
    },

    toggleOperatorsBoxHandler: function(e) {
        var $this = $(e.currentTarget);
        $this.closest('.number').find('.operators-box').toggle();
        $this.find('.expander-icon').toggleClass('open');
    },

    renderAvailableNumbers: function(iso, code) {
        this.$buyPhoneBtn.addClass('disable');

        this.$countrySelector.attr('class', 'voip-flag link arrow-down ' + iso);
        this.$countryCode.text('+' + code);
        this.$countryInput.val('');
        this.$countryInput.hide();
        this.$countryInputClearBtn.hide();

        this.$availableNumbers.empty();
        this.$availableNumbersEmptyMsg.hide();
        this.$availableNumbersEmptySearchMsg.hide();
        this.$availableNumbersLoader.show();

        var self = this;
        Teamlab.getCrmVoipAvailableNumbers(null, {
            filter: { numberType: 0, isoCountryCode: iso },
            success: function(params, numbers) {
                self.$availableNumbersLoader.hide();

                if (numbers.length) {
                    var $numbers = self.availableNumberTmpl.tmpl(numbers);

                    self.$availableNumbers.empty();
                    self.$availableNumbers.append($numbers);
                } else {
                    self.$availableNumbersEmptyMsg.show();
                }

                self.$countryInput.show();
            },
            error: function() {
                self.$availableNumbersLoader.hide();
                self.showErrorMessage();
            }
        });
    },

    buyNumberHandler: function() {
        if (this.$buyPhoneBtn.is('.disable')) {
            return;
        }

        this.$buyPhoneBtn.addClass('disable');
        this.$cancelBuyPhoneBtn.addClass('disable');
        this.$buyPhoneLoader.css('display', 'inline-block');

        var $selectedNumber = this.$availableNumbers.find('input:checked');
        if (!$selectedNumber.length) {
            return;
        }

        var number = $selectedNumber.attr('data-number');

        var self = this;
        Teamlab.createCrmVoipNumber(null,
            { number: number },
            {
                success: function(params, addedNumber) {
                    var $number = self.existingNumberTmpl.tmpl(addedNumber);

                    self.$availableNumbers.find('#anumber\\' + number).remove();
                    self.$existingNumbersList.append($number);

                    self.$cancelBuyPhoneBtn.removeClass('disable');
                    self.$buyPhoneLoader.hide();

                    toastr.success(ASC.Resources.Master.Resource.BuyNumberSuccessMsg);
                },
                error: function() {
                    self.$cancelBuyPhoneBtn.removeClass('disable');
                    self.$buyPhoneLoader.hide();
                    self.showErrorMessage();
                }
            });
    },

    cancelBuyNumberHandler: function() {
        if (this.$cancelBuyPhoneBtn.is('.disable')) {
            return;
        }
        $.unblockUI();
    },

    setOperatorsUserInfo: function(operators) {
        if (!operators || !operators.length) {
            return;
        }

        for (var i = 0; i < operators.length; i++) {
            operators[i].userInfo = this.getUserInfo(operators[i].id);
        }
    },

    getUserInfo: function(id) {
        var users = ASC.Resources.Master.ApiResponses_Profiles.response;
        if (!users) {
            return null;
        }

        for (var i = 0; i < users.length; i++) {
            if (users[i].id == id) {
                return users[i];
            }
        }

        return null;
    },
    
    //#endregion
    
    //#region utils

    showLoader: function() {
        LoadingBanner.displayLoading();
    },

    hideLoader: function() {
        LoadingBanner.hideLoading();
    },

    showSuccessOpearationMessage: function() {
        toastr.success(ASC.Resources.Master.Resource.ChangesSuccessfullyAppliedMsg);
    },

    showErrorMessage: function() {
        toastr.error(ASC.Resources.Master.Resource.CommonJSErrorMsg);
    }
    
    //#endregion
};