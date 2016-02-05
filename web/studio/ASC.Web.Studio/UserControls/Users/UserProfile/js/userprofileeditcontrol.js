/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


window.EditProfileManager = (function () {

    var init = function () {
        var edit = jq.getURLParam("action") == "edit";

        if (!jq.browser.mobile) {
            initPhotoUploader();
        }
        else {
            jq("#loadPhotoImage").hide();
            jq(".profile-role").css("bottom", "6px");
        }

        initBorderPhoto();

        jq("#userProfilePhoto img").on("load", function () {
            initBorderPhoto();
            LoadingBanner.hideLoading();
        });

        jq("#loadPhotoImage").on("click", function () {
            if (jq(this).hasClass("disabled")) return;

            if (!jq.browser.mobile) {
                var curPhotoSrc = jq("#userProfilePhoto").find("img").attr("src");
                ASC.Controls.LoadPhotoImage.showPhotoDialog(curPhotoSrc);
            }
        });

        jq("#divLoadPhotoWindow .default-image").on("click", function () {
            if (typeof (window.userId) != "undefined") {
                var userId = window.userId;
                ASC.Controls.LoadPhotoImage.setDefaultPhoto(userId);
            } else {
                var src = jq(this).attr("data-src");
                jq("#userProfilePhoto").find("img").attr("src", src);
                PopupKeyUpActionProvider.CloseDialog();
                jq(".profile-action-usericon").unblock();
                jq("#userProfilePhotoDscr").show();
                jq("#userProfilePhotoError").html("");
            }
        });
        jq("#userType").change(function () {
            changeDescriptionUserType(this);
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
        var sex = jq("#userdataSex").attr("data-value");
        jq('#userdataSex option[value=' + sex + ']').attr('selected', true);

        jq("#userType, .group-field").tlcombobox();
        jq("#userdataSex").tlcombobox();
        jq("#profileFirstName").focus();

        jq(".tabs-content select").each(function () {
            var className = jq(this).find("option:selected").val();
            jq(this).siblings(".combobox-title").addClass(className);
        });

        // Departments
        var userManagerGroups = [];
        var $chooseGroupsSelector = jq("#chooseGroupsSelector");
        $chooseGroupsSelector.groupadvancedSelector({
            canadd: Teamlab.profile.isAdmin 
        });
        
        $chooseGroupsSelector.on("showList", function (event, items) {
            var $this = jq(this),
                itemIds = [];
            var $o = jq.tmpl("template-selector-selected-items", { Items: items });
            jq(".departments-list").append($o);
            items.forEach(function (u) {
                itemIds.push(u.id);
            });
            $chooseGroupsSelector.groupadvancedSelector("disable", itemIds);
        });
        
        $chooseGroupsSelector.on("afterCreate", function (event, item) {
            var groupList = jq("#groupList");
            if (groupList.length) {
                groupList.prepend("<li class=\"menu-sub-item\" data-id=\"" + item.id
                    + "\"><a class=\"menu-item-label outer-text text-overflow\" title=\""
                    + Encoder.htmlEncode(item.title) + "\"" + " href=\"/products/people/#group=" + item.id + "\">"
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
                $elem = $this.parents("li");
            $chooseGroupsSelector.groupadvancedSelector("undisable", [$elem.attr("data-id")]);
            $elem.remove();
        })

        if (edit) {
            var userName = jq.getURLParam("user");

            var $o = jq.tmpl("template-selector-selected-items", { Items: window.departmentsList.sort(SortData) }),
                depIds = [];
            jq("#departmentsField .departments-list").html($o);
            window.departmentsList.forEach(function (el) { depIds.push(el.id) });
            $chooseGroupsSelector.groupadvancedSelector("disable", depIds);

            Teamlab.getUserGroups( window.userId, {
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
        })

        //////
        InitDatePicker();

        jq("#profileActionButton").on("click", function () {

            HideRequiredError();
            var isVisitor = false;

            if (jq("#userType").length) {
                isVisitor = (jq("#userType").val() == "user") ? false : true;
            } else {
                isVisitor = (jq("#userTypeField").attr("data-type") == "user") ? false : true;
            }
            var firstName = jq("#profileFirstName").val() ? jq("#profileFirstName").val().trim() : "",
                lastName = jq("#profileSecondName").val() ? jq("#profileSecondName").val().trim() : "",
                position = jq("#profilePosition").val() ? jq("#profilePosition").val().trim() : "",
                location = jq("#profilePlace").val() ? jq("#profilePlace").val().trim() : "",
                email = jq("#profileEmail").val() ? jq("#profileEmail").val().trim() : "",
                workFromDate = jq("#profileRegistrationDate").val(),
                birthDate = jq("#profileBirthDate").val(),
                pathname = jq("#userProfilePhoto").find("img").attr("src"),
                sex,
                departments = [],
                contacts = [];

            var comment = jq("#profileComment").val();
            if (comment == null || comment == "null") {
                comment = "";
            }

            var isError = false;
            if (firstName == "") {
                ShowRequiredError(jq("#profileFirstName"));
                isError = true;
            }
            if (lastName == "") {
                ShowRequiredError(jq("#profileSecondName"));
                isError = true;
            }
            if (!jq.isValidEmail(email)) {
                ShowRequiredError(jq("#profileEmail"));
                isError = true;
            }
            if (firstName.length > 64) {
                jq("#profileFirstName").siblings(".requiredErrorText").text(ASC.Resources.Master.Resource.ErrorMessageLongField64);
                ShowRequiredError(jq("#profileFirstName"));
                isError = true;
            }
            if (lastName.length > 64) {
                jq("#profileSecondName").siblings(".requiredErrorText").text(ASC.Resources.Master.Resource.ErrorMessageLongField64);
                ShowRequiredError(jq("#profileSecondName"));
                isError = true;
            }
            if (position.length > 64) {
                ShowRequiredError(jq("#profilePosition"));
                isError = true;
            }
            if (location.length > 255) {
                ShowRequiredError(jq("#profilePlace"));
                isError = true;
            }
            if (workFromDate && !jq.isDateFormat(workFromDate)) {
                ShowRequiredError(jq("#profileRegistrationDate"));
                isError = true;
            }
            if (birthDate && !jq.isDateFormat(birthDate)) {
                ShowRequiredError(jq("#profileBirthDate"));
                isError = true;
            }
            if (birthDate && workFromDate && jq("#profileRegistrationDate").datepicker('getDate').getTime() < jq("#profileBirthDate").datepicker('getDate').getTime()) {
                jq("#profileRegistrationDate").siblings(".requiredErrorText").text(ASC.Resources.Master.Resource.ErrorMessage_InvalidDate);
                ShowRequiredError(jq("#profileRegistrationDate"));
                isError = true;
            }

            if (isError) {
                return;
            }

            var
            type = "",
            value = "",
            $contact = null,
            $contacts = jq(".contacts-group div.field-with-actions:not(.default)");

            for (var i = 0, n = $contacts.length; i < n; i++) {
                $contact = $contacts.slice(i, i + 1);
                type = $contact.find("select").val();
                value = $contact.find("input.textEdit").val();
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

            switch (jq("#userdataSex").val()) {
                case "1": sex = "male"; break;
                case "0": sex = "female"; break;
                case "-1": sex = ""; break;
            }

            var resetDate = new Date(1900, 00, 01);

            if (birthDate && birthDate.length) {
                birthDate = jq("#profileBirthDate").datepicker('getDate');
                birthDate.setHours(0);
                birthDate.setMinutes(0);
                birthDate = Teamlab.serializeTimestamp(birthDate);
            } else if (edit) {
                birthDate = Teamlab.serializeTimestamp(resetDate);
            }


            if (workFromDate && workFromDate.length) {
                workFromDate = jq("#profileRegistrationDate").datepicker('getDate');
                workFromDate.setHours(0);
                workFromDate.setMinutes(0);
                workFromDate = Teamlab.serializeTimestamp(workFromDate);
            } else if (edit) {
                workFromDate = Teamlab.serializeTimestamp(resetDate);
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
                email: email,
                birthday: birthDate,
                worksfrom: workFromDate,
                contacts: contacts,
                files: pathname,
                department: departments
            };


            lockProfileActionPageElements();
            if (edit && typeof (window.userId) != "undefined") {
                updateProfile(window.userId, profile);
            } else {
                addProfile(profile);
            }

        });

        jq.confirmBeforeUnload(confirmBeforeUnloadCheck);
    };

    var confirmBeforeUnloadCheck = function () {
        return jq("#profileFirstName").val().length ||
                jq("#profileSecondName").val().length ||
                jq("#profilePosition").val().length ||
                jq("#profilePlace").val().length ||
                jq("#profileEmail").val().length ||
                jq("#profileBirthDate").val().length ||
                jq("#profileComment").val().length ||
                jq(".departments-list.advanced-selector-list-results li").length;
    };

    function addProfile(profile) {
        var params = {};
        Teamlab.addProfile(params, profile, { success: onAddProfile, error: onProfileError });
    };

    function onAddProfile(params, profile) {
        profile = this.__responses[0];
        window.onbeforeunload = null;
        document.location.replace('profile.aspx?user=' + encodeURIComponent(profile.userName));
    };


    function updateProfile(profileId, profile) {
        var params = {};
        Teamlab.updateProfile(params, profileId, profile, { success: onUpdateProfile, error: onProfileError });
    }

    var onUpdateProfile = function (params, profile) {
        window.onbeforeunload = null;
        if (document.location.href.indexOf("my.aspx") > 0) {
            document.location.replace("/my.aspx");
        } else {
            document.location.replace("profile.aspx?user=" + encodeURIComponent(profile.userName));
        }
    };


    var IsInitDatePicker = false;

    var InitDatePicker = function () {

        var fromDateInp = jq("#profileRegistrationDate"),
            birthDateInp = jq("#profileBirthDate"),
            maxBirthDate = null;

        if (!IsInitDatePicker) {
            jq(fromDateInp).mask(ASC.Resources.Master.DatePatternJQ);
            jq(birthDateInp).mask(ASC.Resources.Master.DatePatternJQ);

            if (fromDateInp) {
                jq(fromDateInp)
                         .datepicker({
                             onSelect: function () {
                                 var date = jq(this).datepicker("getDate");
                                 jq(birthDateInp).datepicker("option", "maxDate", date || null);
                             }
                         }).val(jq(fromDateInp).attr("data-value"));
            }
            if (birthDateInp) {
                jq(birthDateInp)
                    .datepicker({
                        onSelect: function () {
                            var date = jq(this).datepicker("getDate");
                            jq(fromDateInp).datepicker("option", "minDate", date || null);
                        }
                    }).val(jq(birthDateInp).attr("data-value"));
            }
            IsInitDatePicker = true;
        };
        jq(fromDateInp).datepicker("option", "minDate", jq(birthDateInp).datepicker("getDate") || null);
        maxBirthDate = jq(fromDateInp).datepicker("getDate") || "";
        jq(birthDateInp).datepicker("option", "maxDate", maxBirthDate.length ? maxBirthDate : null);
    };

    var isValidBithday = function () {
        var fromDateInp = jq("#profileRegistrationDate"),
            birthDateInp = jq("#profileBirthDate");
        return (fromDateInp.datepicker("getDate").getTime() > birthDateInp.datepicker("getDate").getTime()) ? true : false;
    };

    var ChangeUserPhoto = function (file, response) {
        jq('.profile-action-usericon').unblock();
        var result = eval("(" + response + ")");
        if (result.Success) {
            jq("#userProfilePhotoDscr").show();
            jq('#userProfilePhotoError').html('');
            jq('#userProfilePhoto').find("img").attr("src", result.Data);
            PopupKeyUpActionProvider.CloseDialog();
        } else {
            jq("#userProfilePhotoDscr").hide();
            jq('#userProfilePhotoError').html(result.Message);
        }
    };

    var initPhotoUploader = function () {
        new AjaxUpload('changeLogo', {
            action: "ajaxupload.ashx?type=ASC.Web.People.UserPhotoUploader,ASC.Web.People",
            autoSubmit: true,
            onChange: function (file, extension) {
                
                return true;
            },
            onComplete: EditProfileManager.ChangeUserPhoto,
            parentDialog: jq("#divLoadPhotoWindow"),
            isInPopup: true,
            name: "changeLogo"
        });
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
                    addNewBlock(contact.classname, contact.text, "#contactInfoContainer");
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
        jq(selects).tlcombobox();
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
    };

    function onProfileError() {
        unlockProfileActionPageElements();
        toastr.error(this.__errors[0]);
    };
    function lockProfileActionPageElements() {
        jq(".profile-action-userdata .userdata-value input, #profileComment, .contacts-group input").attr("disabled", "disabled");
        jq("#userType, #userdataSex , .group-field").tlcombobox(false);
        jq(".add-new-field, #loadPhotoImage, #chooseGroupsSelector, #departmentsField .departments-list .reset-icon").addClass("disabled");
        LoadingBanner.showLoaderBtn("#userProfileEditPage");
    }

    function unlockProfileActionPageElements() {
        jq(".profile-action-userdata .userdata-value input, #profileComment, .contacts-group input").removeAttr("disabled");
        jq("#userType, #userdataSex , .group-field").tlcombobox(true);
        jq(".add-new-field, #loadPhotoImage, #chooseGroupsSelector, #departmentsField .departments-list .reset-icon").removeClass("disabled");

        LoadingBanner.hideLoaderBtn("#userProfileEditPage");
    }

    function changeDescriptionUserType(typeSelector) {
        var isEdit = jq.getURLParam("action") == "edit" ? true : false;
        if (jq(typeSelector).val() == "user") {
            jq("#collaboratorCanBlock").hide();
            jq("#userCanBlock").show();
        } else {
            jq("#userCanBlock").hide();
            jq("#collaboratorCanBlock").show();
        }
    };

    return {
        init: init,
        ChangeUserPhoto: ChangeUserPhoto
    };
})();

jq(function () {
    EditProfileManager.init();
})
