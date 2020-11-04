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


jq(function () {
    
    initActionMenu();
    initTenantQuota();
    initBorderPhoto();

    jq("#userProfilePhoto img").on("load", function () {
        initBorderPhoto();
        LoadingBanner.hideLoading();
    });

    jq("#loadPhotoImage").on("click", function () {
        ASC.Controls.LoadPhotoImage.showDialog();
    });

    


    jq("#joinToAffilliate:not(.disable)").on("click", function () {
        JoinToAffilliate();
    });

    if (jq("#studio_emailChangeDialog").length == 0) {
        jq(".profile-status.pending div").css("cursor", "default");
    } else {
        jq("#linkNotActivatedActivation").on("click", function() {
            var userEmail = jq("#studio_userProfileCardInfo").attr("data-email");
            var userId = jq("#studio_userProfileCardInfo").attr("data-id");
            ASC.EmailOperationManager.sendEmailActivationInstructions(userEmail, userId, onActivateEmail.bind(null, userEmail));
            return false;
        });

        jq("#imagePendingActivation, #linkPendingActivation, #linkPendingEmailChange").on("click", function () {
            var userEmail = jq("#studio_userProfileCardInfo").attr("data-email");
            var userId = jq("#studio_userProfileCardInfo").attr("data-id");
            ASC.EmailOperationManager.showResendInviteWindow(userEmail, userId, Teamlab.profile.isAdmin, onActivateEmail.bind(null, userEmail));
            return false;
        });
    }

    jq.switcherAction("#switcherAccountLinks", ".account-links");
    jq.switcherAction("#switcherLoginSettings", "#loginSettingsContainer")
    jq.switcherAction("#switcherCommentButton", "#commentContainer");
    jq.switcherAction("#switcherContactsPhoneButton", "#contactsPhoneContainer");
    jq.switcherAction("#switcherContactsSocialButton", "#contactsSocialContainer");
    jq.switcherAction("#switcherSubscriptionButton", "#subscriptionContainer");

    //track event

    jq("#joinToAffilliate").trackEvent("affilliate-button", "affilliate-button-click", "");
});

function initActionMenu() {
    var _top = jq(".profile-header").offset() == null ? 0 : -jq(".profile-header").offset().top,
        _left = jq(".profile-header").offset() == null ? 0 : -jq(".profile-header").offset().left,
        menuID = "actionMenu";

    jq.dropdownToggle({
        dropdownID: menuID,
        switcherSelector: ".header-with-menu .menu-small",
        addTop: _top,
        addLeft: _left - 11,
        showFunction: function(switcherObj, dropdownItem) {
            if (dropdownItem.is(":hidden")) {
                switcherObj.addClass("active");
            } else {
                switcherObj.removeClass("active");
            }
        },
        hideFunction: function() {
            jq(".header-with-menu .menu-small.active").removeClass("active");
        }
    });

    jq(jq("#" + menuID).find(".dropdown-item")).on("click", function() {
        var $menuItem = jq("#" + menuID);
        var userId = $menuItem.attr("data-id"),
            userEmail = $menuItem.attr("data-email"),
            userAdmin = $menuItem.attr("data-admin") == "true",
            displayName = $menuItem.attr("data-displayname"),
            userName = $menuItem.attr("data-username"),
            isVisitor = $menuItem.attr("data-visitor").toLowerCase(),
            parent = jq(this).parent();

        jq("#actionMenu").hide();
        jq("#userMenu").removeClass("active");

        if (jq(parent).hasClass("enable-user")) {
            onChangeUserStatus(userId, 1, isVisitor);
        }
        if (jq(parent).hasClass("disable-user")) {
            onChangeUserStatus(userId, 2, isVisitor);
        }
        if (jq(parent).hasClass("psw-change")) {
            PasswordTool.ShowPwdReminderDialog("1", userEmail);
        }
        if (jq(parent).hasClass("email-change")) {
            ASC.EmailOperationManager.showEmailChangeWindow(userEmail, userId);
        }
        if (jq(parent).hasClass("email-activate")) {
            ASC.EmailOperationManager.sendEmailActivationInstructions(userEmail, userId, onActivateEmail.bind(null, userEmail));
        }
        if (jq(parent).hasClass("edit-photo")) {
            window.ASC.Controls.LoadPhotoImage.showDialog();
        }
        if (jq(parent).hasClass("delete-user")) {
            jq("#actionMenu").hide();
            StudioBlockUIManager.blockUI("#studio_deleteProfileDialog", 400);
            PopupKeyUpActionProvider.ClearActions();
            PopupKeyUpActionProvider.EnterAction = 'SendInstrunctionsToRemoveProfile();';
        }
        if (jq(parent).hasClass("delete-self")) {
            ProfileManager.RemoveUser(userId, displayName, userName, function () { window.location.replace("/Products/People/"); });
        }
        if (jq(parent).hasClass("subscribe-tips")) {
            onChangeTipsSubscription(jq(this));
        }
    });
}

function onActivateEmail(userEmail, response) {
    var newEmail = response.request.args.email;
    var $oldEmail = jq("#emailUserProfile .mail");
    var $menuItem = jq("#email-activate");
    $menuItem.attr("data-email", newEmail);
    $oldEmail.attr("title", newEmail);
    $oldEmail.attr("href", $oldEmail.attr("href").replace(userEmail, newEmail));
    $oldEmail.text(newEmail);
    jq("#studio_userProfileCardInfo").attr("data-email", newEmail);
}

function onChangeUserStatus(userID, status, isVisitor) { 

    if (status == 1 && tenantQuota.availableUsersCount == 0 && isVisitor =="false") {
        if (jq("#tariffLimitExceedUsersPanel").length) {
            TariffLimitExceed.showLimitExceedUsers();
        }
        return;
    }

    var user = new Array();
    user.push(userID);

    var data = { userIds: user };

    Teamlab.updateUserStatus({}, status, data, {
        success: function (params, data) {
            window.location.reload(true);
            //switch (status) {
            //    case 1:
            //        jq(".profile-status").show();
            //        jq(".profile-status.blocked").hide();
            //        jq(".enable-user, .delete-self").addClass("display-none");
            //        jq(".edit-user, .psw-change, .email-change, .disable-user, .email-activate").removeClass("display-none");
            //        break;
            //    case 2:
            //        jq(".profile-status").hide();
            //        jq(".profile-status.blocked").show();
            //        jq(".enable-user, .delete-self").removeClass("display-none");
            //        jq(".edit-user, .psw-change, .email-change, .disable-user, .email-activate").addClass("display-none");
            //        break;
            //}
            //toastr.success(ASC.People.Resources.PeopleJSResource.SuccessChangeUserStatus);
            //initTenantQuota();
        },
        before: LoadingBanner.displayLoading,
        after: LoadingBanner.hideLoading,
        error: function (params, errors) {
            toastr.error(errors);
        }
    });
}

function onChangeTipsSubscription(obj) {
    Teamlab.updateTipsSubscription({
        success: function (params, data) {
            var text = data ? ASC.Resources.Master.Resource.TipsAndTricksUnsubscribeBtn : ASC.Resources.Master.Resource.TipsAndTricksSubscribeBtn;
            obj.attr("title", text).html(text);
            toastr.success(ASC.Resources.Master.Resource.ChangesSuccessfullyAppliedMsg);
        },
        before: LoadingBanner.displayLoading,
        after: LoadingBanner.hideLoading,
        error: function (params, errors) {
            toastr.error(errors);
        }
    });
}

var tenantQuota = {};

var initTenantQuota = function () {
    Teamlab.getQuotas({}, {
        success: function (params, data) {
            tenantQuota = data;
        },
        error: function (params, errors) { }
    });
};

function initBorderPhoto() {
    var $block = jq("#userProfilePhoto"),
        $img = $block.children("img"),
        indent = ($img.width() < $block.width() && $img.height() < 200 ) ? ($block.width() - $img.width()) / 2 : 0;
    $img.css("padding", indent + "px 0");
    setStatusPosition();
};

function setStatusPosition() {
    var $block = jq("#userProfilePhoto"),
    $img = $block.children("img"),
    status = $block.siblings(".profile-status");

    for (var i = 0, n = status.length; i < n; i++) {
        jq(status[i]).css({
            top: ($img.outerHeight() - jq(status[i]).outerHeight()) / 2,
            left: ($block.width() - jq(status[i]).outerWidth()) / 2
        });
        if (jq(status[i]).attr("data-visible") !== "hidden") {
            jq(status[i]).show();
        }
    }
}

function JoinToAffilliate() {
    Teamlab.joinAffiliate({},
        {
            before: function (params) {
                jq("#joinToAffilliate").addClass("disable");
                LoadingBanner.displayLoading();
            },
            after: function (params) {
                jq("#joinToAffilliate").removeClass("disable");
                LoadingBanner.hideLoading();
            },
            success: function (params, response) {
                location.href = response;
            },
            error: function (params, errors) {
                var err = errors[0];
                jq("#errorAffilliate").text(err);
            }
        });
}

var SendInstrunctionsToRemoveProfile = function () {

    Teamlab.removeSelf({},
        {
            success: function (params, response) {
                jq.unblockUI();
                toastr.success(response);
            }
        });
};