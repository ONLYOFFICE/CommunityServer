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


if (!window.userProfileControl) {
  window.userProfileControl = {
    openContact : function (name) {
      var tcExist = false;
      try {
        tcExist = !!ASC.Controls.JabberClient;
      } catch (err) {
        tcExist = false;
      }
      if (tcExist === true) {
        try {ASC.Controls.JabberClient.open(name)} catch (err) {}
      }
    }
  };
}

jq(function () {
    var tcExist = false;
    try {
        tcExist = !!ASC.Controls.JabberClient;
    } catch (err) {
        tcExist = false;
    }
    if (tcExist === true) {
        jq('div.userProfileCard:first ul.info:first li.contact span')
          .addClass('link')
          .click(function () {
              var username = jq(this).parents('li.contact:first').attr('data-username');
              if (!username) {
                  var
                    search = location.search,
                    arr = null,
                    ind = 0,
                    b = null;
                  if (search.charAt(0) === '?') {
                      search = search.substring(1);
                  }
                  arr = search.split('&');
                  ind = arr.length;
                  while (ind--) {
                      b = arr[ind].split('=');
                      if (b[0].toLowerCase() !== 'user') {
                          continue;
                      }
                      username = b[1];

                      break;
                  }
              }
              if (typeof username === 'string') {
                  userProfileControl.openContact(username);
              }
          });
    }
    setStatusPosition();
    initActionMenu();
    initTenantQuota();

    if (!jq.browser.mobile) {
        initPhotoUploader();
    }
    else {
        jq("#loadPhotoImage").hide();
        jq(".profile-role").css("bottom", "6px");
    }

    jq("#userProfilePhoto img").on("load", function () {
        initBorderPhoto();
        LoadingBanner.hideLoading();
    });

    jq("#loadPhotoImage").on("click", function () {
        if (!jq.browser.mobile) {
            ASC.Controls.LoadPhotoImage.showPhotoDialog();
        }
    });

    jq("#divLoadPhotoWindow .default-image").on("click", function () {
        var userId = jq("#studio_userProfileCardInfo").attr("data-id");
        ASC.Controls.LoadPhotoImage.setDefaultPhoto(userId);
    });


    jq("#joinToAffilliate:not(.disable)").on("click", function () {
        JoinToAffilliate();
    });

    if (jq("#studio_emailChangeDialog").length == 0) {
        jq(".profile-status.pending div").css("cursor", "default");
    }

    jq("#imageNotActivatedActivation, #linkNotActivatedActivation").on("click", function () {
        var userEmail = jq("#studio_userProfileCardInfo").attr("data-email");
        var userId = jq("#studio_userProfileCardInfo").attr("data-id");
        EmailOperationManager.SendEmailActivationInstructions(userEmail, userId, onActivateEmail.bind(null, userEmail));
        return false;
    });

    jq("#imagePendingActivation, #linkPendingActivation").on("click", function () {
        var userEmail = jq("#studio_userProfileCardInfo").attr("data-email");
        var userId = jq("#studio_userProfileCardInfo").attr("data-id");
        EmailOperationManager.ShowResendInviteWindow(userEmail, userId, Teamlab.profile.isAdmin, onActivateEmail.bind(null, userEmail));
        return false;
    });

    jq.switcherAction("#switcherAccountLinks", ".account-links");
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
            userName = $menuItem.attr("data-name"),
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
            EmailOperationManager.ShowEmailChangeWindow(userEmail, userId);
        }
        if (jq(parent).hasClass("email-activate")) {
            EmailOperationManager.SendEmailActivationInstructions(userEmail, userId, onActivateEmail.bind(null, userEmail));
        }
        if (jq(parent).hasClass("edit-photo")) {
            UserPhotoThumbnail.ShowDialog();
        }
        if (jq(parent).hasClass("delete-user")) {
            jq("#actionMenu").hide();
            StudioBlockUIManager.blockUI("#studio_deleteProfileDialog", 400, 300, 0);
            PopupKeyUpActionProvider.ClearActions();
            PopupKeyUpActionProvider.EnterAction = 'SendInstrunctionsToRemoveProfile();';
        }
        if (jq(parent).hasClass("delete-self")) {
            ProfileManager.RemoveUser(userId, userName);
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

var tenantQuota = {};

var initTenantQuota = function () {
    Teamlab.getQuotas({}, {
        success: function (params, data) {
            tenantQuota = data;
        },
        error: function (params, errors) { }
    });
};

function initPhotoUploader() {
    var userId = jq("#studio_userProfileCardInfo").attr("data-id");
    new AjaxUpload('changeLogo', {
        action: "ajaxupload.ashx?type=ASC.Web.People.UserPhotoUploader,ASC.Web.People&autosave=true&userId=" + userId,
        autoSubmit: true,
        onChange: function(file, extension) {
            return true;
        },
        onComplete: ChangeUserPhoto,
        parentDialog: jq("#divLoadPhotoWindow"),
        isInPopup: true,
        name: "changeLogo"
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

function ChangeUserPhoto (file, response) {
    jq('.profile-action-usericon').unblock();
    var result = JSON.parse(response);
    if (result.Success) {

        jq("#userProfilePhotoDscr").show();
        jq('#userProfilePhotoError').html('');
        jq('#userProfilePhoto').find("img").attr("src", result.Data.main + "?_=" + new Date().getTime());
        PopupKeyUpActionProvider.CloseDialog();
        UserPhotoThumbnail.ShowDialog();
        UserPhotoThumbnail.updateMainPhoto(result.Data.main,
            [
                result.Data.big,
                result.Data.medium,
                result.Data.small
            ]);

        jq(".edit-photo").removeClass("display-none");
    } else {
        jq("#userProfilePhotoDscr").hide();
        jq('#userProfilePhotoError').html(result.Message);
    }
}

function JoinToAffilliate() {
    var $button = jq("#joinToAffilliate");
    AjaxPro.onLoading = function(b) {
        if (b) {
            $button.addClass("disable");
            LoadingBanner.displayLoading();
        } else {
            $button.removeClass("disable");
            LoadingBanner.hideLoading();
        }
    };
    AjaxPro.timeoutPeriod = 1800000;
    AjaxPro.UserProfileControl.JoinToAffiliateProgram(function(result) {
        var res = result.value;
        if (res.rs1 == "1" && res.rs2) {
            location.href = res.rs2;
        } else if (res.rs1 == "0") {
            jq("#errorAffilliate").text(res.rs2);
        }
    });
}

var SendInstrunctionsToRemoveProfile = function () {
    AjaxPro.UserProfileControl.SendInstructionsToDelete(function (result) {
        var value = result.value;
        jq.unblockUI();
        toastr.success(value.Message);
    });
};