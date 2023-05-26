/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
                obj = JSON.parse(evt.data);
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
        portalLicence = { name: '', cost: '', maxUsers: 0, currectUsers: 0, maxVisitors: 0, visitorsCount: 0, isStandalone: false },
        errorImport = '',
        flatUploader = null,
        msUploader = null,
        columns,
        clip = null,
        toWizard = true,
        manualImport = false,
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
        classIncorrectBox = 'incorrectBox',
        statuses = [{ title: jq("#createNewButton").find(".dropdown-item").eq(0).text(), id: 0 }, { title: jq("#createNewButton").find(".dropdown-item").eq(1).text(), id: 1 }, { title: ASC.People.Resources.PeopleResource.NotImport, id: 2 }];

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
        $addAsUserButton = jq("#addAsUser");
        $addAsGuestButton = jq("#addAsGuest");
        $notAddButton = jq("#notAdd");
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
        $errorContainer = jq(".error-container");

        $okImportUsers.on('click', function () { ASC.People.Import.hideInfoWindow('okcss'); });
        $importAreaBlock.find('.file').on('click', changeVisionFileSelector);
        $importAreaBlock.find('.fileSelector.studio-action-panel').on('click', changeVisionFileSelector);
        $checkAll.on('click', checkAll);
        $saveSettingsBtn.on('click', function () {
            addUser();
            checkEmptyUserItem();
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
        $addAsUserButton.on('click', userChecked);
        $addAsGuestButton.on('click', guestChecked);
        $notAddButton.on('click', notAddChecked);
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
            height: 30 * itemsImportFrom.length,
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
            });

        $delimiter.advancedSelector({
            height: 30 * itemsDelimiter.length,
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
            height: 30 * itemsSeparator.length,
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
            height: 30 * itemsEncoding.length,
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


        fName = resources.ImportContactsFirstName;
        emptyFName = resources.ImportContactsEmptyFirstName;
        lName = resources.ImportContactsLastName;
        emptyLName = resources.ImportContactsEmptyLastName;
        email = resources.ImportContactsEmail;
        emptyEmail = resources.ImportContactsEmptyEmail;
        errorImport = resources.ImportContactsFromFileError;
        errorImportFileTooLarge = resources.ImportContactsFromFileErrorTooLarge;
        errorEmail = resources.ImportContactsIncorrectFields;

        updateQuota();

        $addAsUserButton.val(statuses[0].title);
        $addAsGuestButton.val(statuses[1].title);
        $notAddButton.val(statuses[2].title);

        jq(document).on("click", function (event) {
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
        $checkAll.prop("disabled", true);
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
                        toastr.success(ASC.Resources.Master.ResourceJS.LinkCopySuccess);
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
        var firstName = $firstName.val().trim(),
            lastName = $lastName.val().trim(),
            address = $email.val().trim();

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
                $inputControl.trigger("focus");
            }
            return;
        } else {
            $inputControl.removeClass(classIncorrectBox);
            makeHidden($errorControl);
        }
    };

    function addUser() {
        var items = $userList.find('.userItem');

        var firstName = $firstName.val().trim(),
            lastName = $lastName.val().trim(),
            address = $email.val().trim();

        if (!isContainErrors(items, firstName, lastName, address)) {
            $userList.find('tr').not('.userItem').remove();
            $email.add($firstName).add($lastName).removeClass(classIncorrectBox);
            makeHidden($fnError.add($lnError).add($eaError));
            appendUser({ FirstName: $firstName.val(), LastName: $lastName.val(), Email: $email.val() });
            $firstName.val('').trigger("blur");
            $lastName.val('').trigger("blur");
            $email.val('').trigger("blur");
            $firstName.trigger("focus");
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
            $inputControl.trigger("focus");
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
            if (jq(items[index]).find('.email input').val().trim() == email) {
                if (select) {
                    jq(items[index]).find('.email').addClass('incorrectValue');
                }
                return true;
            }
            jq(items[index]).find('.email').removeClass('incorrectValue');
        }
        return false;
    };

    function removeChecked() {
        var $items = jq('.check:checkbox:checked').parent().parent();
        jQuery.each($items, function () {
            jq(this).remove();
        });
        checkEmptyUserItem();
    };

    function userChecked() {
        var $items = jq('.check:checkbox:checked').parent().parent();
        jQuery.each($items, function () {
            if (jq(this).find('.statusValue').text() == statuses[1].title) {
                portalLicence.visitorsCount--;
            }
            if (jq(this).find('.statusValue').text() != statuses[0].title) {
                portalLicence.currectUsers++;
                jq(this).find('.statusValue').text(statuses[0].title);
            } 
        });
        clearCheckBox();
    }

    function clearCheckBox() {
        jq(".check").prop('checked', false);
        $checkAll.prop({ 'indeterminate': false, 'checked': false });
        checkGroupButtonActive();
    }

    function guestChecked() {
        var $items = jq('.check:checkbox:checked').parent().parent();
        jQuery.each($items, function () {
            if (jq(this).find('.statusValue').text() == statuses[0].title) {
                portalLicence.currectUsers--;
            }
            if (jq(this).find('.statusValue').text() != statuses[1].title) {
                portalLicence.visitorsCount++;
                jq(this).find('.statusValue').text(statuses[1].title);
            } 
        });
        clearCheckBox();
    }

    function notAddChecked() {
        var $items = jq('.check:checkbox:checked').parent().parent();
        jQuery.each($items, function () {
            if (jq(this).find('.statusValue').text() == statuses[0].title) {
                portalLicence.currectUsers--;
            }
            if (jq(this).find('.statusValue').text() == statuses[1].title) {
                portalLicence.visitorsCount--;
            }
            if (jq(this).find('.statusValue').text() != statuses[2].title) {
                jq(this).find('.statusValue').text(statuses[2].title);
            }
        });
        clearCheckBox()
    }

    function checkEmptyUserItem() {
        checkButtonActive();
        checkGroupButtonActive();
        $checkAll.prop({ 'indeterminate': false, 'checked': false });
        if ($userList.find('.userItem').length == 0) {
            $checkAll.prop('disabled', true);
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
            if (jq(items[i]).find('.statusValue').text() != statuses[2].title) {
            arr.push({
                "FirstName": checkValue(jq(items[i]).find('.name .firstname .studioEditableInput').val().trim(), emptyFName),
                "LastName": checkValue(jq(items[i]).find('.name .lastname .studioEditableInput').val().trim(), emptyLName),
                "Email": jq(items[i]).find('.email input').val().trim(),
                "Status": jq(items[i]).find('.statusValue').text() == statuses[0].title ? 1 : 2
            });
        }
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
        createWindows();
    };

    function createWindows() {
        var usersCount = $userList.find('.userItem').length;
        $impBtn.add($cncBtn).removeClass("disabled");
        for (let i = 0; i < usersCount; i++) {
            if (jq('.statusValue').eq(i).text() == "") {
                var idStatus;
                if (portalLicence.maxUsers - portalLicence.currectUsers > 0) {
                    idStatus = 0;
                    jq('.statusValue').eq(i).text(statuses[idStatus].title);
                    portalLicence.currectUsers++;
                } else if (portalLicence.isStandalone || portalLicence.maxVisitors - portalLicence.visitorsCount > 0) {
                    idStatus = 1;
                    jq('.statusValue').eq(i).text(statuses[idStatus].title);
                    portalLicence.visitorsCount++;
                } else {
                    idStatus = 2;
                    jq('.statusValue').eq(i).text(statuses[idStatus].title);
                }
                jq(".status").eq(i).advancedSelector(
                    {
                        height: 30 * 3,
                        itemsChoose: statuses,
                        showSearch: false,
                        onechosen: true,
                        sortMethod: function () { return 0; }
                    }
                );
            }
        }
        jq(".status").on("click", function (event, item) {
            let value = jq(this).children().text();
            if (value != statuses[0].title && portalLicence.maxUsers - portalLicence.currectUsers <= 0) {
                jq(this).parent().find(".advanced-selector-list").children().eq(0).addClass("disableList");
            }
            if (value != statuses[1].title && portalLicence.maxVisitors - portalLicence.visitorsCount <= 0 && !portalLicence.isStandalone) {
                jq(this).parent().find(".advanced-selector-list").children().eq(1).addClass("disableList");
                console.log("sss");
            }
            jq(this).parent().find(".advanced-selector-list").children().removeClass("selected");
            jq(this).parent().find(".advanced-selector-list").children().eq(statuses.find(i => i.title == value).id).addClass("selected");
        });

        jq(".status").on("showList", function (event, item) {
            var status = statuses.find(function (i) { return i.id == item.id; });
            var lastValue = jq(this).children('.statusValue').text();
            if (status.title == statuses[0].title && lastValue != statuses[0].title) {
                if (lastValue == statuses[1].title) {
                    portalLicence.visitorsCount--;
                    portalLicence.currectUsers++;
                } else {
                    portalLicence.currectUsers++
                }
            } else if (status.title == statuses[1].title && lastValue != statuses[1].title) {
                if (lastValue == statuses[0].title) {
                    portalLicence.visitorsCount++;
                    portalLicence.currectUsers--;
                } else {
                    portalLicence.visitorsCount++
                }
            } else if (lastValue == statuses[0].title && status.title != statuses[0].title) {
                portalLicence.currectUsers--;
            } else if (lastValue == statuses[1].title && status.title != statuses[1].title) {
                portalLicence.visitorsCount--;
            }
            jq(this).children('.statusValue').text(status.title);
            checkGroupButtonActive();
        });
    }

    function cropTitle($text, $counter) {
        while ($text.text().length > 27 - $counter.text().length) {
            $text.text($text.text().split(' ').slice(0, $text.text().split(' ').length - 1).join(' ') + '...');
        }
    };

    function checkButtonActive() {
        var usersCount = $userList.find('.userItem').length;

        if (usersCount == 0) {
            $impBtn.prop("disabled", true).addClass('disable');
            jq('#last-step').removeClass('disable');
            $cncBtn.prop("disabled", true).addClass('disable');
        }
        else {
            $impBtn.prop("disabled", false).removeClass('disable');
            $cncBtn.prop("disabled", false).removeClass('disable');
        }
    };

    function checkGroupButtonActive() {
        var checkedCount = $userList.find('.userItem .check input:checkbox:checked').length;

        var countActiveUsers = $userList.find('.userItem .check input:checkbox:checked').parent().parent().find('.status .statusValue').text().split("user").length - 1;
        var countActiveGuest = $userList.find('.userItem .check input:checkbox:checked').parent().parent().find('.status .statusValue').text().split("guest").length - 1;
        if (checkedCount != 0) {
            if (portalLicence.maxUsers - portalLicence.currectUsers >= checkedCount - countActiveUsers) {
                jq("#addAsUser").addClass('activeGroupInput').prop('disabled', false);
            } else {
                jq("#addAsUser").removeClass('activeGroupInput').prop('disabled', true);
        }
            if (portalLicence.maxVisitors - portalLicence.visitorsCount >= checkedCount - countActiveGuest || portalLicence.isStandalone) {
                jq("#addAsGuest").addClass('activeGroupInput').prop('disabled', false);
            } else {
                jq("#addAsGuest").removeClass('activeGroupInput').prop('disabled', true);
            }
            jq("#notAdd").addClass('activeGroupInput').prop('disabled', false);
        }
        else {
            jq("#addAsUser").removeClass('activeGroupInput').prop('disabled', true);
            jq("#addAsGuest").removeClass('activeGroupInput').prop('disabled', true);
            jq("#notAdd").removeClass('activeGroupInput').prop('disabled', true);
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
            saveUsers(users);
            return;
        }
        hideImportUserLimitPanel();
        $errorContainer.addClass("display-none");
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

    function saveUsers(users) {
        showProgressPanel();
        $importUsers.css({ 'opacity': '0.5' }, { 'pointer-events': 'none' });

        teamlab.addImportUser({
            userList: JSON.stringify(users)
        }, {
            success: checkImportUsersStatus
        });
    };

    function checkImportUsersStatus() {
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
                        return;
                    }
                }
                setTimeout(checkImportUsersStatus, 1000);
            }
        });
    };

    function setProgressStep(step, userCount) {
        var $counter = jq('#3.wizard.active span.importCounter'),
            $text = jq('#3.wizard.active span.importStepName');

        $counter.html('');
        $counter.append(' (' + step + ' ' + ASC.Resources.Master.ResourceJS.ImportOf + ' ' + userCount + ')');
        cropTitle($text, $counter);
    };

    function showProgressPanel() {
        $wizardUsers.addClass("disable");
        $wizardUsersInput.prop("disabled", true);
        $importUsersButton.addClass("disable");
        $importUserLimitPanelButton.addClass("disable");
        $wizardAddToPortal.addClass("active");
    };

    function hideProgressPanel() {
        $wizardUsers.removeClass("disable");
        $wizardUsersInput.prop("disabled", false);
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
        toastr.success(ASC.Resources.Master.ResourceJS.FinishImportUserTitle);
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

            var valueMail = jq(this).find('.email input').val().trim();
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
                        console.log(data);
                        portalLicence.currectUsers = data.usersCount;
                    }
                });
                savedUsers.remove();
                toastr.success(ASC.Resources.Master.ResourceJS.SuccessfullyImportCountUsers.format(savedUsers.length))
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
                $checkAll.prop("disabled", false);
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
        jq(controlName).on("focus", function () {
            jq(controlName).removeClass('textEditDefault');
            jq(controlName).addClass('textEditMain');
            if (jq(controlName).val() == defaultText) {
                jq(controlName).val('');
            }
        });

        jq(controlName).on("blur", function () {
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

        $errorContainer.addClass("display-none");
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

        $errorContainer.removeClass("display-none");

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
        $checkAll.prop("disabled", false);

        manualImport = true;
        wizard.next();
        wizard.next();

        checkEmptyUserItem();
    };

    function updateQuota() {
        teamlab.getQuotas({}, {
            success: function (params, quota) {
                portalLicence.maxUsers = quota.maxUsersCount;
                portalLicence.currectUsers = quota.usersCount;
                portalLicence.maxVisitors = quota.maxVisitors;
                portalLicence.visitorsCount = quota.visitorsCount;
                portalLicence.isStandalone = quota.maxVisitors == -1;
            } 
        });
    }

    var wizard = (function () {
        var index = -1,
            PeopleResource = ASC.People.Resources.PeopleResource,
            steps = [
                { id: 0, name: PeopleResource.ImportWizardFirstStep, handler: function () { } },
                { id: 1, name: PeopleResource.ImportWizardSecondStep, handler: function () { if (!manualImport) { createComplTableHeader(); showWizardWindow(); updateQuota();} } },
                { id: 2, name: PeopleResource.ImportWizardThirdStep, handler: function () { if (!manualImport) { parseAllFile(); createSelectTableHeader(); showImportWindow(); } } },
                { id: 3, name: PeopleResource.ImportWizardFourthStep, handler: function () { } }
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

        if (users.length > 0) {
            saveUsers(users);
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