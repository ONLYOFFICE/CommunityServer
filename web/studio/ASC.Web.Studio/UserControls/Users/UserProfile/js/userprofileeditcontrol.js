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


window.EditProfileManager = (function () {

    var teamlab,
        showRequiredError,
        removeRequiredErrorClass,
        isValidEmail,
        isUserEmail = true,
        isUserPassword = false,
        isPasswordApprove = false,
        isEmailApprove = false,
        passwordSettings,
        enabledModulesList,
        domainsList,
        localPart,
        selectedDomain,
        isError,
        isEmailExist,
        edit,
        savedUserDomain = '',
        classActive = 'active',
        tmplCheckEmail = "templateCheckingEmail",
        classEmailInfo = '.emailInfo',
        redTextColor = '#B40404',
        greenTextColor = '#44BB00',
        requiredErrorText = '.requiredErrorText',
        profileURL = 'Profile.aspx?user=',
        dateFormatErrorMsg = '';

    var $setPassword,
        $createEmailOnDomain,
        $inputUserEmail,
        $inputPortalEmail,
        $tablePassword,
        $titlePassword,
        $setExistingEmail,
        $password,
        $copyValues,
        $passwordGen,
        $passwordShow,
        $generatedPassword,
        $portalEmail,
        $profileEmail,
        $passUpper,
        $passDigits,
        $passSpecial,
        $passMinLength,
        $passwordInfo,
        $emailInfo,
        $domainSelector,
        $profileFirstName,
        $profileSecondName,
        $profilePosition,
        $profilePlace,
        $profileRegistrationDate,
        $profileBirthDate,
        $userProfilePhoto,
        $profileComment,
        $profilePhotoBlock,
        $passwordShowLabel,
        $advancedUserType,
        $popupHelperCreateEmailString;

    var init = function () {

        teamlab = Teamlab;
        showRequiredError = ShowRequiredError;
        removeRequiredErrorClass = RemoveRequiredErrorClass;
        isValidEmail = ASC.Mail.Utility.IsValidEmail;

        $setPassword = jq('#setPassword');
        $createEmailOnDomain = jq('#createEmailOnDomain');
        $inputUserEmail = jq('#inputUserEmail');
        $inputPortalEmail = jq('#inputPortalEmail');
        $tablePassword = jq('#tablePassword');
        $titlePassword = jq('#titlePassword');
        $setExistingEmail = jq('#setExistingEmail');
        $password = jq('#password');
        $copyValues = jq('#copyValues');
        $passwordGen = jq('#passwordGen');
        $passwordShow = jq('#passwordShow');
        $generatedPassword = jq('#generatedPassword');
        $portalEmail = jq('.portalEmail');
        $profileEmail = jq('#profileEmail');
        $passUpper = jq('#passUpper');
        $passDigits = jq('#passDigits');
        $passSpecial = jq('#passSpecial');
        $passMinLength = jq('#passMinLength');
        $passwordInfo = jq('#passwordInfo');
        $emailInfo = jq(classEmailInfo);
        $domainSelector = jq('#domainSelector');
        $profileFirstName = jq("#profileFirstName");
        $profileSecondName = jq("#profileSecondName");
        $profilePosition = jq("#profilePosition");
        $profilePlace = jq("#profilePlace");
        $profileRegistrationDate = jq("#profileRegistrationDate");
        $profileBirthDate = jq("#profileBirthDate");
        $userProfilePhoto = jq("#userProfilePhoto");
        $profileComment = jq("#profileComment");
        $profilePhotoBlock = jq('.profile-photo-block');
        $passwordShowLabel = jq('#passwordShowLabel');
        $advancedUserType = jq('#advancedUserType');
        $popupHelperCreateEmailString = jq('#AnswerForEmail p:eq(1)');

        $setPassword.on('click', showSettingPassword);
        $createEmailOnDomain.on('click', showCreateEmailOnDomain);
        $setExistingEmail.on('click', showUseExistingEmail);
        $password.on('input', checkPassword);
        $password.on('focus', function () { $passwordInfo.show(); });
        $password.on('blur', function () { $passwordInfo.hide(); });
        $copyValues.on('click', copyToClipboard);
        $passwordGen.on('click', passwordGenerator);
        $passwordShow.on('click', showOrHidePassword);
        jq('#advancedUserType span').on('click', toggleSwitch);
        jq('#advancedSexType span').on('click', toggleSwitch);
        $profileFirstName.on('input', function () { validateFirstName(false, true, true); });
        $profileSecondName.on('input', function () { validateLastName(false, true, true); });
        $portalEmail.on('input', $emailInfo.empty);
        $profileEmail.on('input', function () { $emailInfo.empty; savedUserDomain = ''; });
        $portalEmail.on('keyup paste input', function () { delayedCheck($portalEmail); });
        $profileEmail.on('keyup paste input', function () { delayedCheck($profileEmail); });
        $inputPortalEmail.on('click', '.optionItem', function () { delayedCheck($portalEmail); cutDomainInSelector(); });

        edit = jq.getURLParam("action") == "edit";

        teamlab.getPortalPasswordSettings({},
            {
                success: function (params, data) {
                    passwordSettings = data;
                }
            });

        teamlab.getEnabledModules({},
            {
                success: function (params, data) {
                    enabledModulesList = data;
                    fillModulesName();
                }
            });

        initBorderPhoto();
        hideEmailOnDomainForGuest();

        jq("#userProfilePhoto img").on("load", function () {
            initBorderPhoto();
            LoadingBanner.hideLoading();
        });

        jq("#loadPhotoImage").on("click", function () {
            if (jq(this).hasClass("disabled")) return;

            ASC.Controls.LoadPhotoImage.showDialog();
        });

        jq.switcherAction("#switcherCommentButton", "#commentContainer");
        jq.switcherAction("#switcherContactInfoButton", "#contactInfoContainer");
        jq.switcherAction("#switcherSocialButton", "#socialContainer");

        jq(".containerBodyBlock").on("click", ".delete-field", function () {
            jq(this).parent(".field-with-actions").remove();
        });

        jq(".add-new-field").on("click", function () { addNewField(this) });

        jq(".tabs-content").on("change", "select", function () {
            var className = jq(jq(this).find("option:selected")).val();
            jq(this).siblings(".combobox-title").removeClass().addClass("combobox-title " + className);
        });

        renderDepsContacts();

        if (edit) {
            if (userSex) {
                jq("#advancedSexType span:first").addClass(classActive);
            } else if (userSex == 0) {
                jq("#advancedSexType span:last").addClass(classActive)
            }
            $popupHelperCreateEmailString.hide();
        }

        jq(".group-field").tlcombobox({ align: 'left' });
        jq(".group-field.external").tlcombobox(false);

        $profileFirstName.focus();

        jq(".tabs-content select").each(function () {
            var className = jq(this).find("option:selected").val();
            jq(this).siblings(".combobox-title").addClass(className);
        });

        // Departments
        var userManagerGroups = [];
        var selectedGroups = {};
        var $chooseGroupsSelector = jq("#chooseGroupsSelector");
        $chooseGroupsSelector.groupadvancedSelector({
            canadd: teamlab.profile.isAdmin
        });

        $chooseGroupsSelector.on("showList", function (event, items) {
            var itemsForAdd = [];
            var itemIds = [];

            for (var i = 0, j = items.length; i < j; i++) {
                var itemsI = items[i];
                if (!selectedGroups.hasOwnProperty(itemsI.id)) {
                    selectedGroups[itemsI.id] = undefined;
                    itemIds.push(itemsI.id);
                    itemsForAdd.push(itemsI);
                }
            }

            var $o = jq.tmpl("template-selector-selected-items", { Items: itemsForAdd });
            jq(".departments-list").append($o);
            $chooseGroupsSelector.groupadvancedSelector("reset", true);
            $chooseGroupsSelector.groupadvancedSelector("disable", itemIds);
        });

        $chooseGroupsSelector.on("afterCreate", function (event, item) {
            var groupList = jq("#groupList");
            if (groupList.length) {
                groupList.prepend("<li class=\"menu-sub-item\" data-id=\"" + item.id
                    + "\"><a class=\"menu-item-label outer-text text-overflow\" title=\""
                    + Encoder.htmlEncode(item.title) + "\"" + " href=\"Products/People/#group=" + item.id + "\">"
                    + Encoder.htmlEncode(item.title)
                    + "</a></li>");
                if (groupList.parents("li").hasClass("none-sub-list")) {
                    groupList.parents("li").removeClass("none-sub-list").addClass("sub-list");
                    jq("#defaultLinkPeople").before("<span class=\"expander\"></span>");
                }
            }
            if (item.head.id == window.userId) {
                userManagerGroups.push(item);
                var $o = jq.tmpl("template-selector-selected-items", { Items: [item] }),
                    depList = jq("#departmentsField .departments-list");
                depList.append($o);
                $chooseGroupsSelector.groupadvancedSelector("disable", [item.id]);
            }
        });

        jq("#departmentsField .departments-list").on("click", "li .reset-icon:not(.disabled)", function () {
            var $this = jq(this),
                $elem = $this.parents("li"),
                id = $elem.attr("data-id");
            $chooseGroupsSelector.groupadvancedSelector("undisable", [id]);
            delete selectedGroups[id];
            $elem.remove();
        });

        if (edit) {
            var userName = jq.getURLParam("user");

            var $o = jq.tmpl("template-selector-selected-items", { Items: window.departmentsList.sort(SortData) }),
                depIds = [];
            jq("#departmentsField .departments-list").html($o);
            window.departmentsList.forEach(function (el) {
                depIds.push(el.id);
                selectedGroups[el.id] = undefined;
            });

            $chooseGroupsSelector.groupadvancedSelector("disable", depIds);

            teamlab.getUserGroups(window.userId, {
                success: function (params, groups) {
                    for (var i = 0, ln = groups.length; i < ln; i++) {
                        if (groups[i].manager && userName && (groups[i].manager.toLowerCase() == userName.toLowerCase())) {
                            userManagerGroups.push(
                                {
                                    id: groups[i].id,
                                    title: groups[i].name
                                });
                            $chooseGroupsSelector.groupadvancedSelector("disable", [groups[i].id]);
                        }
                    }
                }
            });
        }

        jq("#cancelProfileAction").on("click", function () {
            $this = jq(this);
            window.onbeforeunload = null;
            document.location.replace($this.attr("data-url"));
        });

        //////
        InitDatePicker();

        jq("#profileActionButton").on("click", function () {

            HideRequiredError();
            var isVisitor = (jq("#advancedUserType span:first").hasClass(classActive)) ? true : false;

            var firstName = $profileFirstName.val() ? $profileFirstName.val().trim() : '',
                lastName = $profileSecondName.val() ? $profileSecondName.val().trim() : '',
                position = $profilePosition.val() ? $profilePosition.val().trim() : '',
                location = $profilePlace.val() ? $profilePlace.val().trim() : '',
                email = getEmail(),
                password = $password.val(),
                workFromDate = $profileRegistrationDate.val(),
                birthDate = $profileBirthDate.val(),
                pathname = $userProfilePhoto.find('img').attr('src'),
                sex,
                departments = [],
                contacts = [],
                comment = $profileComment.val();

            isError = false;

            if (comment === null || comment === 'null') {
                comment = '';
            }

            if (password == "") {
                isUserPassword = false;
            }
            if (isUserPassword && !passwordValidation(password)) {
                showRequiredError($password);
                isError = true;
            }

            if (!edit) {
                (isUserEmail)
                    ? checkEmptyField(email, $profileEmail)
                    : checkEmptyField(localPart, $portalEmail);

                (isUserEmail)
                    ? ((checkUserEmailExist(email))
                        ? showRequiredError($profileEmail)
                        : null)
                    : ((checkEmailExistOnDomain())
                        ? showRequiredError($portalEmail)
                        : null);
            }

            if (!ASC.Mail.Utility.IsValidEmail(email)) {
                if (isUserEmail) {
                    showRequiredError($profileEmail);
                } else {
                    showRequiredError($portalEmail);
                }
                isError = true;
            }

            if (!validateFirstName(true, false, false)) {
                isError = true;
            }

            if (!validateLastName(true, false, false)) {
                isError = true;
            }

            checkFieldLength(lastName, 64, $profileSecondName, true);
            checkFieldLength(firstName, 64, $profileFirstName, true);
            checkFieldLength(position, 64, $profilePosition, false);
            checkFieldLength(location, 255, $profilePlace, false);

            if (!validateDateFields()) {
                isError = true;
            }

            if (isError || isEmailExist) {
                return;
            }

            var type = "",
                value = "",
                isExternalContact = false,
                $contact = null,
                $contacts = jq(".contacts-group div.field-with-actions:not(.default)");

            for (var i = 0, n = $contacts.length; i < n; i++) {
                $contact = $contacts.slice(i, i + 1);
                isExternalContact = $contact.attr("isExternalContact");
                type = $contact.find("select").val().trim();
                value = $contact.find("input.textEdit").val().trim();

                if (isExternalContact) {
                    switch (type) {
                        case "mail":
                        case "phone":
                        case "mobphone":
                        case "skype":
                            type = "ext" + type;
                            break;
                    }
                }

                if (type && value) {
                    for (var j = 0, k = contacts.length; j < k; j++) {
                        if (type == contacts[j].Type && value == contacts[j].Value) {
                            toastr.error(ASC.Resources.Master.Resource.ErrorMessageContactsDuplicated);
                            return;
                        }
                    }
                    contacts.push({ Type: type, Value: value });
                }
            }

            (jq('#advancedSexType span:first').hasClass(classActive))
                ? sex = 'male'
                : (jq('#advancedSexType span:last').hasClass(classActive))
                    ? sex = 'female'
                    : sex = '';

            var resetDate = new Date(1900, 00, 01);

            if (birthDate && birthDate.length) {
                birthDate = $profileBirthDate.datepicker('getDate');
                birthDate.setHours(0);
                birthDate.setMinutes(0);
                birthDate = teamlab.serializeTimestamp(birthDate);
            } else if (edit) {
                birthDate = teamlab.serializeTimestamp(resetDate);
            }

            if (workFromDate && workFromDate.length) {
                workFromDate = $profileRegistrationDate.datepicker('getDate');
                workFromDate.setHours(0);
                workFromDate.setMinutes(0);
                workFromDate = teamlab.serializeTimestamp(workFromDate);
            } else if (edit) {
                workFromDate = teamlab.serializeTimestamp(resetDate);
            }

            var $depSelectors = jq("#departmentsField .departments-list li");
            if ($depSelectors) {
                for (var i = 0, n = $depSelectors.length; i < n; i++) {
                    $depSelector = jq($depSelectors[i]);
                    departments.push($depSelector.attr("data-id"));
                }
            }
            var profile =
                {
                    isVisitor: isVisitor,
                    firstname: firstName,
                    lastname: lastName,
                    comment: comment,
                    sex: sex,
                    title: position,
                    location: location,
                    birthday: birthDate,
                    worksfrom: workFromDate,
                    contacts: contacts,
                    files: "", //pathname,
                    department: departments
                };


            if (!edit) {
                profile.email = email;
            }

            lockProfileActionPageElements();
            if (edit && typeof (window.userId) != "undefined") {
                updateProfile(window.userId, profile);
            } else {
                if (isUserPassword) {
                    window.hashPassword(password, function (passwordHash) {
                        profile.passwordHash = passwordHash;

                        addProfile(profile);
                    });
                } else {
                    addProfile(profile);
                }
            }
        });

        jq.confirmBeforeUnload(confirmBeforeUnloadCheck);
    };

    var confirmBeforeUnloadCheck = function () {
        return $profileFirstName.val().length ||
                $profileSecondName.val().length ||
                $profilePosition.val().length ||
                $profilePlace.val().length ||
                $profileEmail.val().length ||
                $profileBirthDate.val().length ||
                $profileComment.val().length ||
                jq(".departments-list.advanced-selector-list-results li").length;
    };

    function addProfile(profile) {
        var params = {};
        teamlab.addProfile(params, profile, {
            success: onAddProfile,
            error: onProfileError
        });
    };

    function onAddProfile(params, profile) {
        profile = this.__responses[0];
        window.onbeforeunload = null;
        if (!isUserEmail) {
            teamlab.addMailbox(params, profile.displayName, localPart, getSelectedDomainId(), profile.id, true, false, {
                success: function (params, res) {
                    window.ASC.Controls.LoadPhotoImage.save(profile.id, function () {
                        document.location.replace(profileURL + encodeURIComponent(profile.userName));
                    });
                },
                error: function () {
                    toastr.error(ASC.Resources.Master.EmailOnDomainIsNotCreated);
                    setTimeout(window.ASC.Controls.LoadPhotoImage.save(profile.id, function () {
                        document.location.replace(profileURL + encodeURIComponent(profile.userName));
                    }), 3000);
                }
            });
        } else {
            window.ASC.Controls.LoadPhotoImage.save(profile.id, function() {
                document.location.replace(profileURL + encodeURIComponent(profile.userName));
            });
        }
    };

    function updateProfile(profileId, profile) {
        var params = {};
        teamlab.updateProfile(params, profileId, profile, {
            success: onUpdateProfile,
            error: onProfileError
        });
    };

    var onUpdateProfile = function (params, profile) {
        window.onbeforeunload = null;
        if (document.location.href.toLowerCase().indexOf("my.aspx") > 0) {
            document.location.replace("/My.aspx");
        } else {
            document.location.replace(profileURL + encodeURIComponent(profile.userName));
        }
    };

    var IsInitDatePicker = false;

    var InitDatePicker = function () {
        var fromDateInp = jq("#profileRegistrationDate"),
            birthDateInp = jq("#profileBirthDate"),
            now = new Date();

        if (!IsInitDatePicker) {
            fromDateInp.mask(ASC.Resources.Master.DatePatternJQ);
            birthDateInp.mask(ASC.Resources.Master.DatePatternJQ);

            fromDateInp.datepicker({
                onSelect: function() {
                    var date = jq(this).blur().datepicker("getDate");
                    birthDateInp.datepicker("option", "maxDate", date);
                    jQuery.datepicker._hideDatepicker();
                }
            }).val(fromDateInp.attr("data-value"));

            birthDateInp.datepicker({
                onSelect: function() {
                    var date = jq(this).blur().datepicker("getDate");
                    fromDateInp.datepicker("option", "minDate", date);
                    jQuery.datepicker._hideDatepicker();
                }
            }).val(birthDateInp.attr("data-value"));

            IsInitDatePicker = true;
        };

        var maxBirthDate = now;
        var birthDate = birthDateInp.datepicker("getDate");
        var fromDate = fromDateInp.datepicker("getDate");

        fromDateInp.datepicker("option", "maxDate", now);
        fromDateInp.datepicker("option", "minDate", birthDate);

        if (fromDate && fromDate < maxBirthDate) {
            maxBirthDate = fromDate;
        }

        birthDateInp.datepicker("option", "maxDate", maxBirthDate);
    };

    var validateDateFields = function () {
        var fromDateInp = $profileRegistrationDate,
            fromDateStr = fromDateInp.length ? fromDateInp.val().trim() : null,
            fromDate = fromDateInp.length ? fromDateInp.datepicker("getDate") : null,

            birthDateInp = $profileBirthDate,
            birthDateStr = birthDateInp.length ? birthDateInp.val().trim() : null,
            birthDate = birthDateInp.length ? birthDateInp.datepicker("getDate") : null,

            maxDate = new Date(),
            isValid = true;

        if (!dateFormatErrorMsg) {
            dateFormatErrorMsg = fromDateInp.siblings(requiredErrorText).text();
        }

        if (!fromDateStr && !birthDateStr) {
            return isValid;
        }

        if (fromDateStr && !jq.isDateFormat(fromDateStr)) {
            fromDateInp.siblings(requiredErrorText).text(dateFormatErrorMsg);
            showRequiredError(fromDateInp);
            isValid = false;
        }

        if (fromDate && fromDate > maxDate) {
            fromDateInp.siblings(requiredErrorText).text(ASC.Resources.Master.Resource.Error);
            showRequiredError(fromDateInp);
            isValid = false;
        }

        if (birthDateStr && !jq.isDateFormat(birthDateStr)) {
            birthDateInp.siblings(requiredErrorText).text(dateFormatErrorMsg);
            showRequiredError(birthDateInp);
            isValid = false;
        }

        if (birthDate && birthDate > maxDate) {
            birthDateInp.siblings(requiredErrorText).text(ASC.Resources.Master.Resource.Error);
            showRequiredError(birthDateInp);
            isValid = false;
        }

        if (birthDate && fromDate && fromDate < birthDate) {
            fromDateInp.siblings(requiredErrorText).text(ASC.Resources.Master.Resource.ErrorMessage_InvalidDate);
            showRequiredError(fromDateInp);
            isValid = false;
        }

        return isValid;
    };

    var initBorderPhoto = function () {
        var $block = jq("#userProfilePhoto"),
            $img = $block.children("img"),
            indent = ($img.width() < $block.width() && $img.height() < 200) ? ($block.width() - $img.width()) / 2 : 0;

        $img.css("padding", indent + "px 0");
    };

    var renderDepsContacts = function () {
        if (typeof (window.socContacts) != "undefined" && window.socContacts.length != 0) {
            for (var i = 0, n = window.socContacts.length; i < n; i++) {
                var soc = window.socContacts[i];
                if (soc.hasOwnProperty("classname")) {
                    addNewBlock(soc.classname, soc.text, "#socialContainer");
                }
            }
        }
        if (typeof (window.otherContacts) != "undefined" && window.otherContacts.length != 0) {
            for (var i = 0, n = window.otherContacts.length; i < n; i++) {
                var contact = window.otherContacts[i];
                if (contact.hasOwnProperty("classname")) {
                    var classname = contact.classname;

                    var isExternalContact = false;

                    switch (classname) {
                        case "extmail":
                        case "extmobphone":
                        case "extphone":
                        case "extskype":
                            classname = classname.slice(3);
                            isExternalContact = true;
                            break;
                    }

                    var $newEl = addNewBlock(classname, contact.text, "#contactInfoContainer");

                    if (isExternalContact) {
                        $newEl.attr("isExternalContact", true);
                        $newEl.find(".delete-field").css("visibility", "hidden");
                        $newEl.find(".group-field").addClass("external");

                        var title = jq("#profileFirstName").attr("title");
                        $newEl.find(".textEdit").addClass("disable").attr("disabled", true).attr("title", title);

                    }
                }
            }
        }
    };

    var addNewField = function (item) {
        if (jq(item).hasClass("disabled")) return;

        var clone = jq(item).siblings(".field-with-actions.default").clone().removeClass("default");
        jq(clone).children("input").val("");
        jq(item).before(jq(clone));
        var selects = jq(item).siblings().find("select");

        jq(selects).tlcombobox({ align: 'left' });
        jq(selects).filter(".external").tlcombobox(false);
    };

    var addNewBlock = function (selectedValue, value, fieldID) {
        var $linkAdd = jq(fieldID).children(".add-new-field"),
            $newSelect = $linkAdd.siblings(".field-with-actions.default").clone().removeClass("default");
        if (selectedValue != "") {
            $newSelect.find("select").val(selectedValue);
            if (value != null) {
                $newSelect.find("select").siblings("input").val(value);
            }
        } else {
            $newSelect.find("select").val($newSelect.find("select").children("option:first").val());
        }
        $linkAdd.before($newSelect);

        return $newSelect;
    };

    function onProfileError() {
        unlockProfileActionPageElements();
        toastr.error(this.__errors[0]);
    };

    function lockProfileActionPageElements() {
        jq(".profile-action-userdata .userdata-value input, #profileComment, .contacts-group input").attr("disabled", "disabled");
        jq(".group-field").tlcombobox(false);
        jq(".add-new-field, #loadPhotoImage, #chooseGroupsSelector, #departmentsField .departments-list .reset-icon").addClass("disabled");

        LoadingBanner.showLoaderBtn("#userProfileEditPage");
    };

    function unlockProfileActionPageElements() {
        jq(".profile-action-userdata .userdata-value input, #profileComment, .contacts-group input").removeAttr("disabled");
        jq(".group-field").tlcombobox(true);
        jq(".add-new-field, #loadPhotoImage, #chooseGroupsSelector, #departmentsField .departments-list .reset-icon").removeClass("disabled");

        LoadingBanner.hideLoaderBtn("#userProfileEditPage");
    };

    function hideEmailOnDomainForGuest() {
        if (jq('#advancedUserType span:first').hasClass(classActive)) {
            $createEmailOnDomain.hide();
            if (!isUserEmail) {
                $emailInfo.empty();
                removeRequiredErrorClass($profileEmail);
            }
            showCreateGuest();
            $popupHelperCreateEmailString.hide();
        } else {
            if (!edit) {
                $popupHelperCreateEmailString.show();
            }

            if (!$inputPortalEmail.length || domainsList === null) {
                return;
            }

            if (domainsList) {
                $createEmailOnDomain.show();
                return;
            }

            teamlab.getMailServer(null, {
                success: function (params, res) {
                    fillDomainSelector();
                },
                error: function () {
                    $createEmailOnDomain.hide();
                    domainsList = null;
                },
                async: true
            });
        }
    };

    function toggleSwitch() {
        var toggle = jq(this),
            toggleBlock = toggle.parent();
        if (!toggle.hasClass('disabled') && !toggleBlock.hasClass('disabled')) {
            toggleBlock.find('.active').not(this).removeClass(classActive);
            toggle.addClass(classActive);
            hideEmailOnDomainForGuest();
        }
    };

    function fillDomainSelector() {
        teamlab.getMailDomains(null, {
            success: function (params, domains) {
                if (!domains.length) return;

                domainsList = domains;
                $domainSelector.empty();
                for (var i = 0; i < domains.length; i++) {
                    $domainSelector.append('<option class="optionItem" value="' + domains[i].name + '">' + domains[i].name + '</option>');
                }
                $domainSelector.tlcombobox();

                jq('.tl-combobox').css({ 'top': '3px', 'width': '120px' });
                jq('.combobox-title-inner-text').css('width', 'max-content');
                jq('.combobox-title').css('width', '128px');

                if (jq.browser.mobile) {
                    $domainSelector.css('max-width', '116px');
                }

                $createEmailOnDomain.show();
            }
        });
    };

    function getSelectedDomainId() {
        var selectedDomainId;
        for (var i = 0; i < domainsList.length; i++) {
            var domain = jq('span.tl-combobox:first').attr('data-value') || jq('#domainSelector').val();
            if (domainsList[i].name === domain) {
                selectedDomainId = domainsList[i].id;
            }
        }
        return selectedDomainId;
    };

    function validateFirstName(checkIsEmpty, withouthScroll, withouthFocus) {
        return validateUserName($profileFirstName, checkIsEmpty, withouthScroll, withouthFocus, ASC.Resources.Master.Resource.ErrorInvalidUserFirstName);
    }

    function validateLastName(checkIsEmpty, withouthScroll, withouthFocus) {
        return validateUserName($profileSecondName, checkIsEmpty, withouthScroll, withouthFocus, ASC.Resources.Master.Resource.ErrorInvalidUserLastName);
    }

    function validateUserName(obj, checkIsEmpty, withouthScroll, withouthFocus, errorMsg) {
        var regexp = new XRegExp(ASC.Resources.Master.UserNameRegExpr.Pattern);
        var val = obj.val().trim();

        if (checkIsEmpty && !val) {
            showRequiredError(obj, withouthScroll, withouthFocus);
            obj.next().text("").show();
            return false;
        }

        if (val && !regexp.test(val)) {
            showRequiredError(obj, withouthScroll, withouthFocus);
            obj.next().text(errorMsg).show();
            return false;
        }

        removeRequiredErrorClass(obj);
        obj.next().hide();
        return true;
    }

    function checkUserEmailValidation() {
        var userEmail = $profileEmail.val().trim();
        if (userEmail.length > 0) {
            validationEmail(userEmail, isValidEmail(userEmail), $profileEmail);
        } else {
            isEmailApprove = false;
            undisableCopyToClipboard();
        }
    };

    function checkUserEmailExist(userEmail) {
        var allUsers = UserManager.getAllUsers(),
            userEmail = userEmail.toLowerCase(),
            exep;

        if (Object.keys(allUsers).length > 0) {
            for (var userId in allUsers) {
                if (!allUsers.hasOwnProperty(userId)) continue;
                var user = allUsers[userId];
                exep = userEmail === user.email;
                existingEmail(exep, $profileEmail);
                if (exep) {
                    isError = true;
                    return;
                };
            }
        } else {
            existingEmail(false, $profileEmail);
        }
    };

    function checkEmailValidation() {
        if ($portalEmail.val() !== '') {
            var localpart = $portalEmail.val().trim(),
                domain = getSelectedDomainId();

            if (domain === undefined)
                return;

            teamlab.isMailServerAddressValid(null, localpart, domain, {
                success: function (params, isValid) {
                    validationEmail(null, isValid, $portalEmail);
                },
                error: function () {
                    validationEmail('error', false, $portalEmail);
                }
            });
        } else {
            isEmailApprove = false;
            undisableCopyToClipboard();
        }
    };
    
    function checkEmailExistOnDomain() {
        var localPart = $portalEmail.val().trim().toLowerCase(),
            domain = getSelectedDomainId();

        if (domain === undefined)
            return;

        teamlab.isMailServerAddressExists(null, localPart, domain, {
            success: function (params, exist) {
                existingEmail(exist, $portalEmail);
            }
        });
    };

    function validationEmail(param, expression, control) {
        if (expression) {
            (isUserEmail)
                ? checkUserEmailExist(param)
                : checkEmailExistOnDomain();
        } else {
            var infoText = ASC.Resources.Master.EmailAndPasswordIncorrectEmail;

            if (param === 'error') {
                infoText = ASC.Resources.Master.Resource.Error;
            }

            showRequiredError(control, true, true);
            isError = true;
            $emailInfo.empty();
            jq.tmpl(tmplCheckEmail, { text: infoText, color: redTextColor }).appendTo(classEmailInfo);
            isEmailApprove = false;
            undisableCopyToClipboard();
        }
    };

    function existingEmail(expression, control) {
        if (expression) {
            showRequiredError(control, true, true);
            isEmailExist = true;
            $emailInfo.empty();
            jq.tmpl(tmplCheckEmail, { text: ASC.Resources.Master.ErrorEmailAlreadyExists, color: redTextColor }).appendTo(classEmailInfo);
            isEmailApprove = false;
            isError = true;
        } else {
            $emailInfo.empty();
            jq.tmpl(tmplCheckEmail, { text: ASC.Resources.Master.EmailIsAvailable, color: greenTextColor }).appendTo(classEmailInfo);
            removeRequiredErrorClass(control);
            isEmailApprove = true;
            isEmailExist = false;
        }

        undisableCopyToClipboard();
    };

    function checkEmptyField(field, $control) {
        if (field === '') {
            showRequiredError($control);
            isError = true;
        }
    };

    function checkFieldLength(field, length, $control, showErrMesage) {
        if (field.length > length) {
            if (showErrMesage) {
                $control.siblings(requiredErrorText).text(ASC.Resources.Master.Resource.ErrorMessageLongField64);
            }
            showRequiredError($control);
            isError = true;
        }
    };

    function delayedCheck(control) {
        if (control.val().length) {
            $emailInfo.empty().fadeIn();
            setDelay(function () {
                (isUserEmail)
                    ? checkUserEmailValidation()
                    : checkEmailValidation();
            }, 1000);
        } else {
            $emailInfo.hide();
            removeRequiredErrorClass(control);
        }
    };

    var setDelay = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();

    function showCreateEmailOnDomain() {
        isUserEmail = false;
        $inputUserEmail.hide();
        $inputPortalEmail.show();
        showSettingPassword();
        copyLocalPart();
        cutDomainInSelector();

        if (domainsList.length < 1) {
            toastr.error(jq.format(ASC.Resources.Master.NoMailServerDomainsMsg, '<a href=\"/addons/mail/#administration\" class=\"link\" target=\"_blank\">', '</a>'), null, {
                "closeButton": true,
                "timeOut": "0",
                "extendedTimeOut": "0"
            });
        }
    };

    function cutDomainInSelector() {
        var domainText = $inputPortalEmail.find('.combobox-title-inner-text'),
            cutDomainText = domainText.text().slice(0, 14);

        if (cutDomainText.length < domainText.text().length) {
            cutDomainText += '...';
        }
        domainText.text(cutDomainText);
    }

    function showUseExistingEmail() {
        isUserEmail = true;
        $inputUserEmail.show();
        $inputPortalEmail.hide();
        hideSettingPassword();
        copyLocalPart();
    };

    function showCreateGuest() {
        isUserEmail = true;
        $inputUserEmail.show();
        $inputPortalEmail.hide();
        clearEmailInfo();
        hideSettingPassword();
    };

    function copyLocalPart() {
        if (isUserEmail) {
            $profileEmail.val(splitEmail($portalEmail)).focus();
            delayedCheck($profileEmail);
        } else {
            $portalEmail.val(splitEmail($profileEmail)).focus();
            delayedCheck($portalEmail);
        }
        clearEmailInfo();
    };

    function splitEmail($control) {
        var savedUserLocalPart;

        if ($control.val().indexOf('@') != -1) {
            savedUserDomain = $control.val().substring($control.val().lastIndexOf("@"));
            savedUserLocalPart = $control.val().substring(0, $control.val().lastIndexOf("@"));
        } else {
            savedUserLocalPart = $control.val();
        }

        if (isUserEmail) {
            return savedUserLocalPart + savedUserDomain;
        } else {
            return savedUserLocalPart;
        }
    };

    function clearEmailInfo() {
        if (!edit) {
            removeRequiredErrorClass($profileEmail);
            removeRequiredErrorClass($portalEmail);
            $emailInfo.empty();
        }
    };

    function showSettingPassword() {
        isUserPassword = true;
        $generatedPassword.hide();
        jq('.validationBlock').show();
        if (!isUserEmail) {
            $tablePassword.addClass('requiredField');
            $titlePassword.addClass('requiredTitle');
        }
    };

    function hideSettingPassword() {
        isUserPassword = false;
        $generatedPassword.show();
        jq('.validationBlock').hide();
        $tablePassword.removeClass('requiredField');
        $titlePassword.removeClass('requiredTitle');
    };

    function checkPassword() {
        var inputValues = $password.val().trim(),
            inputLength = inputValues.length,
            progress = jq('.validationProgress'),
            progressStep = ($password.width() + 41) / passwordSettings.minLength;

        progress.width(inputLength * progressStep);
        
        (passwordValidation(inputValues))
               ? (progress.css('background', greenTextColor),
                    removeRequiredErrorClass($password),
                    isPasswordApprove = true)
               : (progress.css('background', redTextColor),
                    isPasswordApprove = false);

        undisableCopyToClipboard();
    };

    function passwordGenerator() {
        teamlab.getRandomPassword(null, {
            success: function (params, password) {
                $password.val(password);
                $password.prop('type', 'text');
                $passwordShowLabel.removeClass('hide').addClass('show');
                $passwordShow.prop('checked', true);
                checkPassword();
            }
        });
    };

    function showOrHidePassword() {
        ($passwordShow.prop('checked'))
            ? ($password.prop('type', 'text'),
                $passwordShowLabel.removeClass('hide').addClass('show'))
            : ($password.prop('type', 'password'),
                $passwordShowLabel.removeClass('show').addClass('hide'));
    };

    function undisableCopyToClipboard() {
        if ($copyValues.length && $password.length && $profileEmail.length && isPasswordApprove && isEmailApprove) {
            $copyValues.removeClass('disabled');
        } else {
            $copyValues.addClass('disabled');
        }
    }

    function copyToClipboard() {
        var email = getEmail(),
            password = $password.val(),
            clip = null,
            clipAera = jq('#clip').val('email: ' + email + '\npassword: ' + password);

        clip = ASC.Clipboard.destroy(clip);

        clip = ASC.Clipboard.create(clipAera.val(), "copyValues", {});

        toastr.success(ASC.Resources.Master.EmailAndPasswordCopiedToClipboard);
    };

    function getEmail() {
        if (isUserEmail) {
            return $profileEmail.length ? $profileEmail.val().trim() : "";
        } else {
            localPart = $portalEmail.length ? $portalEmail.val().trim() : "";
            var domain = jq('span.tl-combobox:first').attr('data-value') || jq('#domainSelector').val();
            return localPart + '@' + domain;
        }
    };

    function passwordValidation(inputValues) {
        var upper,
            digits,
            special;

        (passwordSettings.upperCase)
            ? upper = /[A-Z]/.test(inputValues)
            : upper = true;

        (passwordSettings.digits)
            ? digits = /\d/.test(inputValues)
            : digits = true;

        (passwordSettings.specSymbols)
            ? special = /[!@#$%^&*_\-()=]/.test(inputValues)
            : special = true;

        checkPasswordInfoColor(upper, digits, special, inputValues);

        return digits && upper && special && inputValues.length >= passwordSettings.minLength;
    };

    function checkPasswordInfoColor(upper, digits, special, inputValues) {
        (upper)
            ? greenText($passUpper)
            : redText($passUpper);

        (digits)
            ? greenText($passDigits)
            : redText($passDigits);

        (special)
            ? greenText($passSpecial)
            : redText($passSpecial);

        (inputValues.length >= passwordSettings.minLength)
            ? greenText($passMinLength)
            : redText($passMinLength);
    };

    function greenText(control) {
        control.removeClass('red').addClass('green');
    };

    function redText(control) {
        control.removeClass('green').addClass('red');
    };

    function fillModulesName() {
        var read = '<td><div class="access read">&nbsp;</div></td>',
            check = '<td><div class="access check">&nbsp;</div></td>';

        var listModules = [
            {id: "documents", value: check + read},
            {id: "projects", value: check + read},
            {id: "crm", value: check},
            {id: "mail", value: check},
            {id: "people", value: check},
            {id: "community", value: check + read},
            {id: "talk", value: check + check},
            {id: "calendar", value: check + check}
        ];
        
        var enabledModulesId = enabledModulesList.map(function (m) {
             return m.id;
        });
        var moduleInfo = jq(".moduleInfo");

        jq.each(listModules, function (_, module) {
            var enabledModuleIndex = enabledModulesId.indexOf(module.id);
            if (enabledModuleIndex >= 0) {
                moduleInfo.append("<tr>"
                    + "<td>" + jq.trim(enabledModulesList[enabledModuleIndex].title) + "</td>"
                    + module.value
                    + "</tr>");
            }
        });
    };

    return {
        init: init
    };
})();

jq(function() {
    EditProfileManager.init();
});
