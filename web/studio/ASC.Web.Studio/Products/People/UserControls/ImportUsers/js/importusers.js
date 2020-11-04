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


window.master = {
    init: function () {
        if (window.addEventListener) {
            window.addEventListener('message', master.listener, false);
        } else {
            window.attachEvent('onmessage', master.listener);
        }
    },
    listener: function (evt) {
        var obj,
            peopleImport = ASC.People.Import;
        if (typeof evt.data == "string") {
            try {
                obj = jQuery.parseJSON(evt.data);
            } catch (err) {
                return;
            }
        } else {
            obj = evt.data;
        }

        if (obj == null || obj.Tpr == null || obj.Tpr == undefined || obj.Tpr != "Importer") {
            return;
        }

        var error = obj.error;

        if (error && error.length != 0) {
            peopleImport.showImportErrorWindow(error);
            return;
        }

        var data = obj.Data;

        if (data.length == 0) {
            peopleImport.showImportErrorWindow(ASC.People.Resources.Import.ImportContactsEmptyData);
            return;
        }
        peopleImport.wizard.last();
        jq("#file-option").hide();
        jq(".source-text").hide();
        peopleImport.showManualImportWindow();
        jq('#userList').html('');
        jq('#userList').find("tr").not(".userItem").remove();
        var parent = jq('#userList');
        var items = parent.find(".userItem");
        for (var i = 0, n = data.length; i < n; i++) {
            if (!(peopleImport.isExists(items, data[i].Email, false))) {
                peopleImport.appendUser(data[i]);
            }
        }
        peopleImport.bindHints();
        peopleImport.checkCountUsersForAdd();
        peopleImport.checkEmptyUserItem();
    }
};
master.init();

if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.People === "undefined")
    ASC.People = {};

ASC.People.Import = (function () {
    var teamlab,
        fName = '',
        emptyFName = '',
        lName = '',
        emptyLName = '',
        email = '',
        emptyEmail = '',
        info = true,
        portalLicence = { name: '', cost: '', maxUsers: 0, currectUsers: 0 },
        errorImport = '',
        flatUploader = null,
        msUploader = null,
        columns,
        clip = null,
        alreadyChecking = false,
        toWizard = true,
        manualImport = false,
        progressBarIntervalId = null,
        encoding = 0,
        separator = 0,
        delimiter = 0,
        resources,
        userNameRegExp,
        uploadFile,
        maxFileSize = 512000,
        saveResponse,
        uploadPosition = 0,
        isValidEmail,
        classError1 = 'error1',
        classError2 = 'error2',
        classError3 = 'error3',
        classLineError = 'lineError',
        classFirstName = '.firstName',
        classLastName = '.lastName',
        classEmail = '.studioEditableInput.email',
        classUserItem = '.userItem',
        classIncorrectBox = 'incorrectBox';

    var $blockProcess,
        $okwin,
        $checkAll,
        $impBtn,
        $cncBtn,
        $deleteUserButton,
        $impBtnAddClass,
        $cncBtnAddClass,
        $userList,
        $fnError,
        $lnError,
        $eaError,
        $email,
        $firstName,
        $lastName,
        $userListUserItem,
        $importUsersFileSelector,
        $importAreaBlock,
        $inviteUserLink,
        $okcss,
        $okcssInfoMessage,
        $wizardUsers,
        $wizardUsersInput,
        $importUsersProgress,
        $importUsersButton,
        $importUserLimitPanelButton,
        $showWizard,
        $filename,
        $errorBox,
        $errorBubble,
        $okImportUsers,
        $saveSettingsBtn,
        $importAsCollaborators,
        $importBtn,
        $importCancelBtn,
        $importLimitBtn,
        $importLimitCancelBtn,
        $infoMessage,
        $fromHand,
        $addDefaultHeader,
        $importUsers,
        $mainPageContent,
        $restr;

    function init() {
        wizard.init();
        teamlab = Teamlab;
        isValidEmail = ASC.Mail.Utility.IsValidEmail;
        resources = ASC.People.Resources.Import;
        userNameRegExp = new XRegExp(ASC.Resources.Master.UserNameRegExpr.Pattern);
        flatUploader = initUploader('import_flatUploader',
            'ajaxupload.ashx?type=ASC.Web.People.Core.Import.ContactsUploader,ASC.Web.People&obj=txt');
        msUploader = initUploader('import_msUploader',
            'ajaxupload.ashx?type=ASC.Web.People.Core.Import.ContactsUploader,ASC.Web.People&obj=ms');
        addHint('#firstName', fName);
        addHint('#lastName', lName);
        addHint('#email', email);

        $blockProcess = jq('#blockProcess');
        $okwin = jq('#okwin');
        $checkAll = jq('#checkAll');
        $checkOne = jq('.check');
        $impBtn = jq("#importUsers .middle-button-container .impBtn");
        $cncBtn = jq("#importUsers .middle-button-container .cncBtn");
        $deleteUserButton = jq("#deleteUserButton");
        $userList = jq('#userList');
        $fnError = jq('#fnError');
        $lnError = jq('#lnError');
        $eaError = jq('#eaError');
        $email = jq('#email');
        $firstName = jq('#firstName');
        $lastName = jq('#lastName');
        $userListUserItem = jq('#userList .userItem');
        $importUsersFileSelector = jq('.importUsers div.file .fileSelector');
        $importAreaBlock = jq("#importAreaBlock");
        $inviteUserLink = jq("#inviteUserLink");
        $okcss = jq('.okcss');
        $okcssInfoMessage = jq('.okcss #infoMessage');
        $wizardUsers = jq("#wizard_users");
        $wizardUsersInput = jq("#wizard_users input");
        $importUsersProgress = jq("#import-users-progress");
        $importUsersButton = jq("#importUsers .button");
        $importUserLimitPanelButton = jq("#importUserLimitPanel .button");
        $wizardAddToPortal = jq("#3.wizard");
        $errorBox = jq('.importErrorBox');
        $errorBubble = jq('.errorBubble');
        $okImportUsers = jq('.okImportUsers');
        $importAreaBlock = jq('#importAreaBlock');
        $saveSettingsBtn = jq("#saveSettingsBtn");
        $importAsCollaborators = jq("#importAsCollaborators");
        $importBtn = jq("#import-btn");
        $importCancelBtn = jq("#import-cancel-btn");
        $importLimitBtn = jq("#import-limit-btn");
        $importLimitCancelBtn = jq("#import-limit-cancel-btn");
        $infoMessage = jq('#infoMessage');
        $fromHand = jq("#fromHand");
        $addDefaultHeader = jq('#addDefaultHeader');
        $userList = jq("#userList");
        $deleteUserButton = jq("#deleteUserButton");
        $importUsers = jq('#importUsers');
        $mainPageContent = jq('.mainPageContent');
        $restr = jq('.restr');

        $okImportUsers.on('click', function () { ASC.People.Import.hideInfoWindow('okcss'); });
        $importAreaBlock.find('.file').on('click', changeVisionFileSelector);
        $importAreaBlock.find('.fileSelector.studio-action-panel').on('click', changeVisionFileSelector);
        $checkAll.on('click', checkAll);
        $saveSettingsBtn.on('click', function () {
            addUser();
            checkEmptyUserItem();
            checkCountUsersForAdd();
        });
        $importAsCollaborators.on('click', changeInviteLinkType);
        $importAreaBlock.find('.HelpCenterSwitcher').on('click', function () {
            jq(this).helper({ BlockHelperID: 'answerForHelpInviteGuests', position: 'fixed' });
        });
        $importBtn.on('click', importList);
        $importCancelBtn.on('click', hideImportWindow);
        $importAreaBlock.find('.HelpCenterSwitcher').on('click', function () {
            jq(this).helper({ BlockHelperID: 'answerForHelpInv', position: 'fixed' });
        });
        $importLimitBtn.on('click', confirmationLimit);
        $importLimitCancelBtn.on('click', hideImportUserLimitPanel);
        $infoMessage.find('.button').on('click', namename);
        $fromHand.on('click', showManualImportWindow);
        $addDefaultHeader.on('click', function () {
            (jq(this).is(':checked'))
                ? (uploadPosition = 1, dataPath.pos = 1, parseForCompliance())
                : (uploadPosition = 0, dataPath.pos = 0, parseForCompliance())
        });
        $wizardUsers.on('change', '.optionMenu', function (e) {
            getComplianceValues();
            selectNotEqualOptions(jq(e.target));
        });
        $userList.on('click', '.check', indetermCheck);
        $deleteUserButton.on('click', removeChecked);
        $importUsers.on('click', '.newRandomUser', updateExampleValues);
        $mainPageContent.on('click', function (e) { trackError(jq(e.target)); });
        $restr.on('scroll', function () { trackError(jq(document.activeElement)); });

        var $encoding = jq("#source span:last-child"),
            $separator = jq("#separator span:last-child"),
            $delimiter = jq("#delimiter span:last-child"),
            $importFrom = jq("#importFrom span:last-child"),
            $emailDomen = jq('#emailDomen span:last-child');

        var itemsEncoding = resources.Encoding,
            itemsSeparator = resources.Separator,
            itemsDelimiter = resources.Delimiter,
            itemsImportFrom = resources.ImportFromWhat,
            itemsEmailDomen = resources.EmailDomen;

        var dataPath = flatUploader._settings.data;

        $importFrom.advancedSelector({
            height: 26 * itemsImportFrom.length,
            onechosen: true,
            showSearch: false,
            itemsChoose: itemsImportFrom,
            sortMethod: function () { return 0; }
        })
        .on("showList",
            function (event, item) {
                var frame = jq('#ifr').contents()[0];
                if (item.id === 0) {
                    dataPath.head = null;
                    flatUploader._settings.data.head = '';
                    flatUploader._settings.data.raw = 'true';
                    flatUploader._settings.onComplete = uploadResultWizard;
                    jq(flatUploader._input).trigger("click");
                }
                if (item.id === 1) {
                    var button = frame.getElementsByClassName('google');
                    jq(button).trigger("click");
                }
                if (item.id === 2) {
                    var button = frame.getElementsByClassName('yahoo');
                    jq(button).trigger("click");
                }
            });

        $delimiter.advancedSelector({
            height: 26 * itemsDelimiter.length,
            onechosen: true,
            showSearch: false,
            itemsChoose: itemsDelimiter,
            itemsSelectedIds: [itemsDelimiter[0].id],
            sortMethod: function () { return 0; }
        })
        .on("showList",
            function (event, item) {
                $delimiter.html(item.title).attr("title", item.title);
                dataPath.del = item.id;
                parseForCompliance();
            });

        $delimiter.advancedSelector("selectBeforeShow", itemsDelimiter[0]);

        $separator.advancedSelector({
            height: 26 * itemsSeparator.length,
            onechosen: true,
            showSearch: false,
            itemsChoose: itemsSeparator,
            itemsSelectedIds: [itemsSeparator[0].id],
            sortMethod: function () { return 0; }
        })
        .on("showList",
            function (event, item) {
                $separator.html(item.title).attr("title", item.title);
                dataPath.sep = item.id;
                parseForCompliance();
            });

        $separator.advancedSelector("selectBeforeShow", itemsSeparator[0]);

        $encoding.advancedSelector({
            height: 26 * itemsEncoding.length,
            onechosen: true,
            showSearch: false,
            itemsChoose: itemsEncoding,
            itemsSelectedIds: [itemsEncoding[0].id],
            sortMethod: function () { return 0; }
        })
        .on("showList",
            function (event, item) {
                $encoding.html(item.title).attr("title", item.title);
                dataPath.enc = item.id;
                parseForCompliance();
            });

        $encoding.advancedSelector("selectBeforeShow", itemsEncoding[0]);

        var $userListDQ = $userList;
        $userListDQ.on("click", ".userItem [type='checkbox']", checkButtonActive);
        $userListDQ.on("change", ".userItem .email [type='text']", eraseError);
        $userListDQ.on("change", ".userItem .name .firstname [type='text']", eraseError);
        $userListDQ.on("change", ".userItem .name .lastname [type='text']", eraseError);
        $userListDQ.on("click", ".userItem .remove div", removeItem);

        fName = resources.ImportContactsFirstName;
        emptyFName = resources.ImportContactsEmptyFirstName;
        lName = resources.ImportContactsLastName;
        emptyLName = resources.ImportContactsEmptyLastName;
        email = resources.ImportContactsEmail;
        emptyEmail = resources.ImportContactsEmptyEmail;
        errorImport = resources.ImportContactsFromFileError;
        errorImportFileTooLarge = resources.ImportContactsFromFileErrorTooLarge;
        errorEmail = resources.ImportContactsIncorrectFields;

        teamlab.getQuotas({}, {
            success: function (params, quota) {
                portalLicence.maxUsers = quota.maxUsersCount;
                portalLicence.currectUsers = quota.usersCount;
            }
        });

        jq(document).click(function (event) {
            jq.dropdownToggle({ rightPos: true }).registerAutoHide(event, '.file', '.fileSelector');
            jq('#upload img').attr('src', StudioManager.GetImage('loader_16.gif'));
        });
        showImportControl('ASC.Controls.AnchorController.trigger()');
    }

    function showImportControl(callback) {
        $okwin.hide();
        $checkAll.prop('checked', false);
        $userList.find("tr").remove();
        makeHidden($fnError.add($lnError).add($eaError));
        jq('#donor tr').clone().appendTo('#userList');

        $impBtn.add($cncBtn).addClass("disable");
        $checkAll.attr("disabled", "disabled");

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterActionCallback = callback;
        PopupKeyUpActionProvider.EnterAction = 'ASC.People.Import.checkAndAdd();';
        PopupKeyUpActionProvider.CtrlEnterAction = "ASC.People.Import.importList();";

        updateClipboard();
    };

    function updateClipboard() {
        if (!ASC.Clipboard.enable) {
            jq("#inviteLinkCopy").remove();
        } else {
            clip = ASC.Clipboard.destroy(clip);

            var url = $inviteUserLink.val();

            clip = ASC.Clipboard.create(url, "inviteLinkCopy", {
                onComplete: function () {
                    if (typeof (window.toastr) !== "undefined") {
                        toastr.success(ASC.Resources.Master.Resource.LinkCopySuccess);
                    } else {
                        jq("#inviteUserLink, #inviteLinkCopy").yellowFade();
                    }
                }
            });
        }
    };

    function hideImportWindow() {
        if ($importCancelBtn.hasClass('disable')) {
            return;
        }

        defaultState();

        $okwin.hide();
        $userList.empty();
        jq.unblockUI();
    };

    function changeVisionFileSelector() {
        if ($importUsersFileSelector.css('display') == 'none') {
            $importUsersFileSelector.show();
        } else {
            $importUsersFileSelector.hide();
        }
    };

    function defaultState() {
        hideImportUserLimitPanel();

        addHint('#firstName', fName);
        addHint('#lastName', lName);
        addHint('#email', email);

        $email.add($firstName).add($lastName).removeClass(classIncorrectBox);
    };

    function checkAndAdd() {
        var firstName = jQuery.trim($firstName.val()),
            lastName = jQuery.trim($lastName.val()),
            address = jQuery.trim($email.val());

        checkInputValues(firstName, firstName == fName, $firstName, $fnError);
        checkInputValues(lastName, lastName == lName, $lastName, $lnError);
        checkInputValues(address, !isValidEmail(address), $email, $eaError);

        addUser();
    };

    function checkInputValues(currentVal, equalState ,$inputControl, $errorControl) {
        if (currentVal == '' || equalState) {
            if (wasFocused($inputControl)) {
                $inputControl.addClass(classIncorrectBox);
                makeVisible($errorControl);
            } else {
                $inputControl.focus();
            }
            return;
        } else {
            $inputControl.removeClass(classIncorrectBox);
            makeHidden($errorControl);
        }
    };

    function addUser() {
        var items = $userList.find('.userItem');

        var firstName = jQuery.trim($firstName.val()),
            lastName = jQuery.trim($lastName.val()),
            address = jQuery.trim($email.val());

        if (!isContainErrors(items, firstName, lastName, address)) {
            $userList.find('tr').not('.userItem').remove();
            $email.add($firstName).add($lastName).removeClass(classIncorrectBox);
            makeHidden($fnError.add($lnError).add($eaError));
            appendUser({ FirstName: $firstName.val(), LastName: $lastName.val(), Email: $email.val() });
            $firstName.val('').blur();
            $lastName.val('').blur();
            $email.val('').blur();
            $firstName.focus();
        } else {
            checkValuesBeforeAdd(address, !isValidEmail(address), $email, $eaError);
            checkValuesBeforeAdd(lastName, lastName == lName || !userNameRegExp.test(lastName), $lastName, $lnError);
            checkValuesBeforeAdd(firstName, firstName == fName || !userNameRegExp.test(firstName), $firstName, $fnError);
        }
    };

    function checkValuesBeforeAdd(currentVal, hasError, $inputControl, $errorControl) {
        if (currentVal == '' || hasError) {
            $inputControl.addClass(classIncorrectBox);
            makeVisible($errorControl);
            $inputControl.focus();
        } else {
            $inputControl.removeClass(classIncorrectBox);
            makeHidden($errorControl);
        }
    };

    function isContainErrors(items, firstName, lastName, address) {
        return isExists(items, address, true)
            || address == ''
            || address == email
            || !isValidEmail(address)
            || firstName == ''
            || firstName == fName
            || !userNameRegExp.test(firstName)
            || lastName == ''
            || lastName == lName
            || !userNameRegExp.test(lastName);
    };

    function wasFocused($elementName) {
        return $elementName.is(':focus');
    };

    function makeVisible($control) {
        $control.css('visibility', 'visible');
    };

    function makeHidden($control) {
        $control.css('visibility', 'hidden');
    };

    function isExists(items, email, select) {
        for (var index = 0, n = items.length; index < n; index++) {
            if (jQuery.trim(jq(items[index]).find('.email input').val()) == email) {
                if (select) {
                    jq(items[index]).find('.email').addClass('incorrectValue');
                }
                return true;
            }
            jq(items[index]).find('.email').removeClass('incorrectValue');
        }
        return false;
    };

    function removeItem() {
        var $item = jq(this).parent().parent();
        $item.addClass('remove');
        if ($userList.find(".userItem").not('.remove').length == 0) {
            jq('#addUserBlock').removeClass('bordered');
        }
        $item.remove();

        checkEmptyUserItem();
    };

    function removeChecked() {
        var $items = jq('.check:checkbox:checked').parent().parent();
        jQuery.each($items, function () {
            jq(this).remove();
        });
        checkEmptyUserItem();
    };

    function checkEmptyUserItem() {
        checkButtonActive();
        checkGroupButtonActive();
        $checkAll.prop({ 'indeterminate': false, 'checked': false });
        if ($userList.find('.userItem').length == 0) {
            $checkAll.prop('disabled', true);
            $importAsCollaborators.prop('disabled', true);
        } else {
            $checkAll.prop('disabled', false);
            $importAsCollaborators.prop('disabled', false);
        }
        checkCountUsersForAdd();
    };

    function getUsers() {
        var allItems = $userList.find('.userItem');
        var items = [];
        jQuery.each(allItems, function () {
            items.push(this);
        });
        var arr = new Array();

        for (var i = 0, n = items.length; i < n; i++) {
            arr.push({
                "FirstName": checkValue(jQuery.trim(jq(items[i]).find('.name .firstname .studioEditableInput').val()), emptyFName),
                "LastName": checkValue(jQuery.trim(jq(items[i]).find('.name .lastname .studioEditableInput').val()), emptyLName),
                "Email": jQuery.trim(jq(items[i]).find('.email input').val())
            });
        }
        return arr;
    };

    function checkValue(value, hint) {
        return (value == hint) ? "" : value;
    };

    function checkAll() {
        var checkBtn = jq('.check');
        ($checkAll.is(':checked'))
            ? checkBtn.prop('checked', true)
            : checkBtn.prop('checked', false);
        checkButtonActive();
        checkGroupButtonActive();
    };

    function indetermCheck() {
        var checkLength = jq('input.check').length,
            countCheck = jq('.check:checkbox:checked').length,
            checkBtn = jq('.check');

        (checkBtn.is(':checked'))
            ? ((countCheck == checkLength)
                ? $checkAll.prop({ 'indeterminate': false, 'checked': true })
                : $checkAll.prop('indeterminate', true))
            : ((countCheck == 0)
                ? $checkAll.prop({ 'indeterminate': false, 'checked': false })
                : $checkAll.prop('indeterminate', true));
        checkGroupButtonActive();
    };

    function checkCountUsersForAdd() {
        var usersCount = $userList.find('.userItem').length,
            $counter = jq('.wizard.active span.importCounter'),
            $text = jq('.wizard.active span.importStepName');

        $counter.html('');

        if (usersCount) {
            $counter.append(' (' + usersCount + ')');
            cropTitle($text, $counter);
        }
    };

    function cropTitle($text, $counter) {
        while ($text.text().length > 27 - $counter.text().length) {
            $text.text($text.text().split(' ').slice(0, $text.text().split(' ').length - 1).join(' ') + '...');
        }
    };

    function checkButtonActive() {
        var usersCount = $userList.find('.userItem').length;

        if (usersCount == 0) {
            $impBtn.attr('disabled', 'disabled').addClass('disable');
            jq('#last-step').removeClass('disable');
            $cncBtn.attr('disabled', 'disabled').addClass('disable');
        }
        else {
            $impBtn.removeAttr('disabled').removeClass('disable');
            $cncBtn.removeAttr('disabled').removeClass('disable');
        }
    };

    function checkGroupButtonActive() {
        var checkedCount = $userList.find('.userItem .check input:checkbox:checked').length;

        if (checkedCount == 0) {
            $deleteUserButton.removeClass('activeGroupInput').prop('disabled', true);
        }
        else {
            $deleteUserButton.addClass('activeGroupInput').prop('disabled', false);
        }
    };

    function eraseError() {
        var element = jq(this);
        element
            .removeClass(classError1)
            .removeClass(classError2)
            .removeClass(classError3)
            .parents(classUserItem)
            .removeClass(classLineError);
    };

    function importList() {
        if ($userList.find(classUserItem).not('fistable').length == 0 || $importBtn.hasClass('disable')) {
            return;
        }
        var users = getUsers();
        var importUsersAsCollaborators = $importAsCollaborators.is(':checked');
        var inputUsers = $userList.find(classUserItem);
        var error = 0;

        if (users.length > 0) {
            for (var i = 0; i < users.length; i++) {
                var $inputUser = jq(inputUsers[i]),
                    firstInput = $inputUser.find(classFirstName),
                    lastInput = $inputUser.find(classLastName),
                    emailInput = $inputUser.find(classEmail),
                    isEmptyInputFirst = users[i].FirstName.trim() === '',
                    isEmptyInputLast = users[i].LastName.trim() === '',
                    isEmptyInputEmail = users[i].Email.trim() === '',
                    isFirstValid = userNameRegExp.test(users[i].FirstName),
                    isLastValid = userNameRegExp.test(users[i].LastName),
                    isEmailValid = isValidEmail(users[i].Email);

                if (isEmptyInputFirst || isEmptyInputLast || isEmptyInputEmail || !isFirstValid || !isLastValid || !isEmailValid) {
                    if (isEmptyInputFirst) {
                        firstInput.addClass(classError1);
                        addHint(firstInput, emptyFName);
                    } else if (!isFirstValid) {
                        firstInput.addClass(classError1);
                    }

                    if (isEmptyInputLast) {
                        lastInput.addClass(classError2);
                        addHint(lastInput, emptyLName);
                    } else if (!isLastValid) {
                        lastInput.addClass(classError2);
                    }

                    if (isEmptyInputEmail) {
                        emailInput.addClass(classError3);
                        addHint(emailInput, emptyEmail);
                    } else if (!isEmailValid) {
                        emailInput.addClass(classError3);
                    }

                    $inputUser.addClass(classLineError);
                    error++;
                } else {
                    var attrs = $inputUser.attr('class');
                    $inputUser.attr('class', attrs.replace(/error\d/gi, ''));
                }
            }
            if (error > 0) {
                var $tableList = $wizardUsers.find('.restr'),
                    $errorElem = $tableList.find('tr').filter('.lineError').first(),
                    offset = $errorElem.length ? $errorElem.position().top : 0;
                $tableList.scrollTop(offset - $tableList.find('tr').first().position().top);
                return;
            }
            if (!importUsersAsCollaborators) {
                if (parseInt(portalLicence.maxUsers, 10) < (parseInt(portalLicence.currectUsers, 10) + users.length)) {
                    if (parseInt(portalLicence.maxUsers, 10) == (parseInt(portalLicence.currectUsers, 10))) {
                        jq("#importUserLimitHeader").html(ASC.Resources.Master.Resource.ImportUserOverlimitHeader);
                    }
                    showImportUserLimitPanel();
                    return;
                }
            }
            saveUsers(users, importUsersAsCollaborators);
            return;
        }
        hideImportUserLimitPanel();
        closeWindow(false);
    };

    function trackError($input) {
        var boxPosition = $input.offset(),
            positionOnTop = boxPosition.top + $input.height() + 16;
        if ($input.hasClass(classError1) || $input.hasClass(classError2) || $input.hasClass(classError3)) {
            if (250 < positionOnTop && positionOnTop < 490) {
                $errorBox.show().offset({ left: boxPosition.left, top: positionOnTop });
                checkErrorText($input);
            } else {
                $errorBox.hide();
            }
        } else {
            $errorBox.hide();
        }
    };

    function checkErrorText($input) {
        ($input.hasClass('firstName'))
         ? $errorBubble.text(($input.val() === '')
            ? resources.ImportContactsEmptyFirstName
            : resources.ImportContactsInvalidFirstName)
         : ($input.hasClass('lastName'))
                ? $errorBubble.text(($input.val() === '')
            ? resources.ImportContactsEmptyLastName
            : resources.ImportContactsInvalidLastName)
                : $errorBubble.text(($input.val() === '')
            ? resources.ImportContactsEmptyEmail
            : resources.ImportContactsInvalidEmail);
    };

    function saveUsers(users, importUsersAsCollaborators) {
        showProgressPanel();
        $importUsers.css({ 'opacity': '0.5' }, { 'pointer-events': 'none' });

        teamlab.addImportUser({
            userList: JSON.stringify(users),
            importUsersAsCollaborators: importUsersAsCollaborators
        }, {
            success: function() {
                progressBarIntervalId = setInterval(checkImportUsersStatus, 100);
            }
        });
    };

    function checkImportUsersStatus() {
        if (alreadyChecking) {
            return;
        }
        alreadyChecking = true;

        teamlab.getImportStatus({
            success: function(params, status) {
                if (status) {
                    setProgressStep(status.percents, status.userCounter);
                    if (status.completed) {
                        if (status.status == 0) {
                            toastr.error(status.error);
                        } else {
                            saveUsersCallback(status);
                            location.reload();
                        }
                    }
                }
                alreadyChecking = false;
            }
        });
    };

    function setProgressStep(step, userCount) {
        var $counter = jq('#3.wizard.active span.importCounter'),
            $text = jq('#3.wizard.active span.importStepName');

        $counter.html('');
        $counter.append(' (' + step + ' ' + ASC.Resources.Master.Resource.ImportOf + ' ' + userCount + ')');
        cropTitle($text, $counter);
    };

    function showProgressPanel() {
        $wizardUsers.addClass("disable");
        $wizardUsersInput.attr("disabled", true);
        $importUsersButton.addClass("disable");
        $importUserLimitPanelButton.addClass("disable");
        $wizardAddToPortal.addClass("active");
    };

    function hideProgressPanel() {
        $wizardUsers.removeClass("disable");
        $wizardUsersInput.attr("disabled", false);
        $importUsersButton.removeClass("disable");
        $importUserLimitPanelButton.removeClass("disable");
        $wizardAddToPortal.removeClass("active");
        jq('.wizard span').html('');
    };

    function closeWindow(checkMistakes, mistakes) {
        var users = getUsers();
        hideImportUserLimitPanel();
        if (!checkMistakes) {
            if ($userList.find('.saved').length == users.length && users.length != 0) {
                informImportedWindow();
            }
        } else {
            if (mistakes == 0 && ($userList.find('.saved').length == users.length && users.length != 0)) {
                informImportedWindow();
            } else {
                bindHints();
            }
        }
    };

    function informImportedWindow() {
        defaultState();
        jq.unblockUI();
        toastr.success(ASC.Resources.Master.Resource.FinishImportUserTitle);
        window.location.href = "./#";
    };

    function saveUsersCallback(result) {
        var parent = $userListUserItem;
        var mistakes = 0;

        hideImportUserLimitPanel();

        if (result.Status == 0) {
            showImportErrorWindow(result.Message);
            return;
        }

        jQuery.each(parent, function () {
            var attrs = jq(this).attr('class');
            jq(this).attr('class', attrs.replace(/error\d/gi, ''));
            jq(this).find('.remove').removeClass('removeError');

            var valueMail = jQuery.trim(jq(this).find('.email input').val());
            for (var i = 0, n = result.Data.length; i < n; i++) {
                if (result.Data[i].Email == valueMail) {
                    if (result.Data[i].Result != '') {
                        mistakes++;
                        jq(this).addClass(result.Data[i].Class);
                        jq(this).find('.errors').attr('title', result.Data[i].Result);
                        jq(this).find('.remove').addClass('removeError');
                    } else {
                        jq(this).addClass('saved');
                        var attrs = jq(this).attr('class');
                        jq(this).attr('class', attrs.replace(/error\d/gi, ''));
                        jq(this).find('.remove').removeClass('removeError');
                    }
                }
            }
        });

        hideProgressPanel();

        if (mistakes == 0) {
            jq.unblockUI();
        } else {
            var savedUsers = $userList.find('.saved');
            if (savedUsers.length) {
                teamlab.getQuotas({}, {
                    success: function (params, data) {
                        portalLicence.currectUsers = data.usersCount;
                    }
                });
                savedUsers.remove();
                toastr.success(ASC.Resources.Master.Resource.SuccessfullyImportCountUsers.format(savedUsers.length))
            }
            bindHints();
        }
        closeWindow(true, mistakes);
    };

    function hideInfoWindow(info) {
        $blockProcess.hide();
        jq('.' + info).hide();
        LoadingBanner.hideLoaderBtn("#importUsers");
    };

    function uploadResult(file, response) {
        jq('#upload').hide();
        $userList.html('');
        var result = JSON.parse(response);
        if (result.Success) {
            var extractedUsers = JSON.parse(result.Message);
            $userList.find("tr").not(".userItem").remove();
            var mistakes = 0;
            var copy = 0;
            var items = $userList.find(".userItem");
            for (var i = 1, n = extractedUsers.length; i < n; i++) {
                if (isExists(items, extractedUsers[i].Email, false)) {
                    copy++;
                    continue;
                }
                appendUser(extractedUsers[i]);
            }

            if (mistakes > 0 && copy != extractedUsers.length) {
                showImportErrorWindow(errorEmail);
            }

            bindHints();

            if ($userList.find(".userItem").length > 0) {
                $impBtn.add($cncBtn).removeClass("disable");
                $checkAll.removeAttr("disabled");
            }
            checkCountUsersForAdd();
            checkGroupButtonActive();

        } else {
            $blockProcess.show();
            $okcss.show();
            showImportErrorWindow(errorImport);
        }
    };

    function uploadResultWizard(file, response) {
        jq('#upload').hide();
        $userList.html('');
        saveResponse = response;
        try {
            var result = JSON.parse(response);
        } catch (e) {
            console.log(e);
            result = {Success: false};
        }

        if (result.Success) {
            var extractedUsers = JSON.parse(result.Message);
            columns = JSON.parse(result.Columns);
            $userList.find("tr").not(".userItem").remove();

            appendComplianceBlock(extractedUsers);

            bindHints();

            setOptionsLock();

            if ($userList.find(".userItem").length > 0) {
                $impBtn.add($cncBtn).removeClass("disable");
            }
        } else {
            $blockProcess.add($okcss).show();
            showImportErrorWindow(errorImport);
        }
    };

    function updateExampleValues() {
        $userList.html('');
        var result = JSON.parse(saveResponse);
        var extractedUsers = JSON.parse(result.Message);
        var optionPosition = flatUploader._settings.data.head;

        appendComplianceBlock(extractedUsers);

        setOptionsLock();

        if ($userList.find(".userItem").length > 0) {
            $impBtn.removeClass("disable").prop('disabled', false);
        }
    };

    function appendComplianceBlock(extractedUsers) {
        if (extractedUsers.length == 1) {
            appendWizard(extractedUsers[0]);
        } else {
            appendWizard(extractedUsers[Math.floor(Math.random() * (extractedUsers.length - 1)) + 1]);
        }
    }

    function bindHints() {
        var parent = $userList.find('.studioEditableInput');
        jQuery.each(parent, function () {
            if (jq(this).val() == '') {
                if (jq(this).is('[class*="firstName"]')) {
                    addHint(this, emptyFName);
                }
                if (jq(this).is('[class*="lastName"]')) {
                    addHint(this, emptyLName);
                }
                if (jq(this).is('[class*="email"]')) {
                    addHint(this, emptyEmail);
                }
            }
        });
    };

    function appendUser(item) {
        jq.tmpl("userRecord", item).appendTo('#userList');
        $impBtn.add($cncBtn).removeClass("disabled");
    };

    function appendWizard(userItem) {
        var headHolder = createColumnHolder(userItem);
        if (!uploadPosition) {
            jq.tmpl("userWizard", {
                headItem: headHolder,
                userItem: makeObjToArray(userItem)[1],
                columns: columns
            }).appendTo('#userList');
        } else {
            jq.tmpl("userWizard", {
                headItem: fillEmptyHolders(makeObjToArray(userItem)[0]),
                userItem: makeObjToArray(userItem)[1],
                columns: columns
            }).appendTo('#userList');
        }
        $impBtn.removeClass("disabled");
        $cncBtn.removeClass("disabled");
    };

    function setOptionsLock() {
        var selectors = jq('.optionMenu'),
            optionHead = flatUploader._settings.data.head;
        for (var i = 0; i < selectors.length; i++) {
            if (optionHead) {
                jq(selectors[i]).val(optionHead[i].split(' ').join(''));
            }
            selectNotEqualOptions(selectors.eq(i));
        }
    };

    function createColumnHolder(item) {
        var headerLength = makeObjToArray(item)[1].length,
            userHeader = [];
        for (var i = 0; i < headerLength; i++) {
            var value = i + 1;
            userHeader[i] = ASC.People.Resources.PeopleResource.ImportColumn + value;
        }
        return userHeader;
    }

    function fillEmptyHolders(array) {
        var i = array.length;
        while (--i) {
            if (array[i] == '') {
                var value = i + 1;
                array[i] = ASC.People.Resources.PeopleResource.ImportColumn + value;
            }
        }
        return array;
    };

    function makeObjToArray(obj) {
        var array = jq.map(obj, function (value, index) {
            return [value];
        })
        return array;
    };

    function addHint(control, text) {
        jq(control).val(text).addClass('textEditDefault');
        setControlHintSettings(control, text);
    };

    function setControlHintSettings(controlName, defaultText) {
        jq(controlName).focus(function () {
            jq(controlName).removeClass('textEditDefault');
            jq(controlName).addClass('textEditMain');
            if (jq(controlName).val() == defaultText) {
                jq(controlName).val('');
            }
        });

        jq(controlName).blur(function () {
            if (jq(controlName).val() == '') {
                jq(controlName).removeClass('textEditMain');
                jq(controlName).addClass('textEditDefault');
                jq(controlName).val(defaultText);
            }
        });
    };

    function initUploader(control, handler) {
        if (jq('#' + control).length > 0) {
            var cfg = {
                accept: '.csv',
                action: handler,
                onChange: function (file, ext) {
                    uploadFile = this._input.files;
                    if (ext.length == 0 || ext[0].toLowerCase() != "csv") {
                        showImportErrorWindow(errorImport);
                        return false;
                    }
                    if (uploadFile[0].size > maxFileSize) {
                        showImportErrorWindow(jq.format(errorImportFileTooLarge, (maxFileSize/1024)));
                        return false;
                    }
                    return true;
                },
                onSubmit: function (file, ext) {
                    jq('#upload').show();
                    $userList.html('');
                    showWizardWindow();
                    if (toWizard) {
                        wizard.next();
                        toWizard = false;
                        flatUploader._settings.data.compl = 1;
                    }
                    jq("#file-name").text(file);
                    jq("#file-option").show();
                },
                onComplete: uploadResultWizard,
                parentDialog: (jq.browser.msie && jq.browser.version == 9) ? jq("#" + control).parent() : false,
                isInPopup: (jq.browser.msie && jq.browser.version == 9)
            };
            return new AjaxUpload(control, cfg);
        }
    };

    function showImportErrorWindow(message) {
        $blockProcess.show();
        $okcssInfoMessage.html(message);
        $okcss.show();
    };

    function hideImportErrorWindow() {
        $blockProcess.hide();
        $okcssInfoMessage.html('');
        $okcss.hide();
        defaultState();
    };

    function changeInviteLinkType() {
        var importTypeCheckbox = $importAsCollaborators;
        var linkContainer = $inviteUserLink;

        if (importTypeCheckbox.is(":disabled")) return;

        if (importTypeCheckbox.is(":checked")) {
            linkContainer.val(linkContainer.attr("data-invite-visitor-link"));
        } else {
            linkContainer.val(linkContainer.attr("data-invite-user-link"));
        }
        updateClipboard();
    };

    function showImportUserLimitPanel() {
        $importUsers.block();
        jq("#importUserLimitPanel").show();
    };

    function hideImportUserLimitPanel() {
        if ($importLimitCancelBtn.hasClass("disable")) {
            return;
        }
        $importUsers.unblock();
        jq("#importUserLimitPanel").hide();
    };

    function getComplianceValues() {
        var inputs = jq('.optionMenu'),
            complianceValues = [];

        for (var i = 0; i < inputs.length; i++) {
            var currentValue = jq(inputs[i]).find('option:selected').val();

            if (currentValue == 'Not import') {
                currentValue = 'emptyOption';
            }
                complianceValues[i] = currentValue;
        }
        flatUploader._settings.data.head = complianceValues;
    };

    function selectNotEqualOptions($option) {
        var currVal = $option.val().split(' ').join(''),
            prevVal = $option.data("prev").split(' ').join(''),
            otherSelects = jq('.optionMenu').not($option);

        if (currVal !== 'emptyOption') {
            otherSelects.find("option[value=" + currVal + "]").prop('disabled', true);
        }

        if (prevVal) {
            otherSelects.find("option[value=" + prevVal + "]").prop('disabled', false);
        }

        $option.data("prev", currVal);
    };

    function createComplTableHeader() {
        jq(".tableHeader").html("");
        jq.tmpl("compliancetTableHeader").appendTo('.tableHeader');
    };

    function createSelectTableHeader() {
        jq(".tableHeader").html("");
        jq.tmpl("selectionTableHeader").appendTo('.tableHeader');
        $checkAll.prop('checked', false);
    };

    function parseForCompliance() {
        flatUploader._settings.data.raw = 'true';
        flatUploader._settings.onComplete = uploadResultWizard;
        updateFileParam();
    };

    function parseAllFile() {
        getComplianceValues();
        flatUploader._settings.data.raw = 'false';
        flatUploader._settings.onComplete = uploadResult;
        updateFileParam();
    };

    function updateFileParam() {
        flatUploader._input.files = uploadFile;
        flatUploader.submit();
    };

    function showWizardWindow() {
        $wizardUsers
            .add(jq("#file-option"))
            .add(jq(".source-text"))
            .add(jq("#next-step"))
            .add(jq(".userItem"))
            .add(jq(".middle-button-container"))
            .show();

        $importBtn
            .add(jq("#addUserBlock"))
            .add(jq("#last-step"))
            .add(jq("#panel"))
            .add(jq("#deleteUserBlock"))
            .add(jq(".desc"))
            .hide();
    };

    function showImportWindow() {
        $importBtn
            .add(jq("#deleteUserBlock"))
            .add(jq(".desc"))
            .show();
        
        jq("#file-option")
            .add(jq(".source-text"))
            .add(jq("#next-step"))
            .hide();

        jq("#last-step").show().removeClass("blue").addClass("gray");
    };

    function showManualImportWindow() {
        $wizardUsers
            .add(jq(".desc"))
            .add(jq(".middle-button-container"))
            .add(jq("#deleteUserBlock"))
            .show();

        jq("#panel")
            .add(jq("#next-step"))
            .add(jq("#last-step"))
            .add(jq('.tableHeader'))
            .hide();

        $importBtn.show().removeClass('disable');
        $checkAll.removeAttr('disabled');

        manualImport = true;
        wizard.next();
        wizard.next();

        checkEmptyUserItem();
    };

    var wizard = (function () {
        var index = -1,
            resource = ASC.People.Resources.PeopleResource,
            steps = [
                { id: 0, name: resource.ImportWizardFirstStep, handler: function () { } },
                { id: 1, name: resource.ImportWizardSecondStep, handler: function () { if (!manualImport) { createComplTableHeader(); showWizardWindow(); } } },
                { id: 2, name: resource.ImportWizardThirdStep, handler: function () { if (!manualImport) { parseAllFile(); createSelectTableHeader(); showImportWindow(); } } },
                { id: 3, name: resource.ImportWizardFourthStep, handler: function () { } }
            ],
            $steps = [],
            endStep = steps.length - 1,
            up = function () {
                if (index !== -1) {
                    $steps[index].removeClass("active").addClass("passive");
                }
                $steps[++index].addClass("active");
                steps[index].handler();
            },
            down = function () {
                $steps[index].removeClass("active")
                $steps[--index].removeClass("passive").addClass("active");
                steps[index].handler();
                updateExampleValues();
            };
        function next() {
            if (index < endStep) up();
        }
        function last() {
            if (index !== 0) down();
        }
        function init() {
            jq.tmpl("wizardStepsTmpl", steps).appendTo("#wizardSteps");
            $steps = steps.map(function (index) {
                return jq("#" + index.id);
            });
            next();
            jq("#next-step").on("click", next);
            jq("#last-step").on("click", last);
        }
        return {
            init: init,
            next: next,
            last: last
        };
    }());

    function namename() { hideInfoWindow('okcss'); };

    function confirmationLimit() {
        if ($userList.find('.userItem').not('fistable').length == 0 || $importLimitBtn.hasClass('disable')) {
            return;
        }

        var users = getUsers();
        var importUsersAsCollaborators = $importAsCollaborators.is(":checked");

        if (users.length > 0) {
            saveUsers(users, importUsersAsCollaborators);
            return;
        }

        hideImportUserLimitPanel();
        closeWindow(false);
    };

    function DisableImportDialog(disable) {
        if (disable) {
            LoadingBanner.showLoaderBtn("#importUsers");
            LoadingBanner.showLoaderBtn("#importUserLimitPanel");
        } else {
            LoadingBanner.hideLoaderBtn("#importUsers");
            LoadingBanner.hideLoaderBtn("#importUserLimitPanel");
        }
    };

    return {
        init: init,
        showImportControl: showImportControl,
        isExists: isExists,
        appendUser: appendUser,
        bindHints: bindHints,
        checkAndAdd: checkAndAdd,
        importList: importList,
        hideInfoWindow: hideInfoWindow,
        hideImportUserLimitPanel: hideImportUserLimitPanel,
        showManualImportWindow: showManualImportWindow,
        showImportErrorWindow: showImportErrorWindow,
        wizard: wizard,
        checkCountUsersForAdd: checkCountUsersForAdd,
        checkEmptyUserItem: checkEmptyUserItem
    };
})();

jq(document).ready(function () {
    ASC.People.Import.init();
});