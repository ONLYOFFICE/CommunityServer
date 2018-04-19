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


if (typeof ASC === "undefined") {
    ASC = {};
}
if (typeof ASC.CRM === "undefined") {
    ASC.CRM = function() { return {} };
}


ASC.CRM.SocialMedia = (function() {
    var CallbackMethods = {
        addAndSaveTwitter: function(params, twitter) {
            ASC.CRM.SocialMedia.LoadContactActivity();
        },

        getContactTweets: function (params, response) {
            LoadingBanner.hideLoading();
            if (response.length != 0) {
                jq("#tweetsEmptyScreen:not(.display-none)").addClass("display-none");
                jq.tmpl("twitterMessageListTmpl", response).appendTo("#divSocialMediaContent");
            } else {
                ASC.CRM.SocialMedia.ShowErrorMessage(ASC.CRM.Resources.CRMCommonResource.NoLoadedMessages);
            }
        }
    };

    _GetContactSMImagesResponse = function (response) {
        var imageCount = response.length;
        if (imageCount > 0) {
            for (var i = 0; i < imageCount; i++) {
                jq.tmpl("socialMediaAvatarTmpl", response[i]).appendTo("#divImagesHolder");
                jq("#linkSaveAvatar").css("display", "inline");
            }
            _FinishGettingContactImages(true);
        } else {
            jq("#divImagesHolder").html(
                ["<div class=\"describe-text\">",
                ASC.CRM.Resources.CRMContactResource.NoPhotoFromSocialMedia,
                "</div>"].join(''));
            _FinishGettingContactImages(false);
        }
    };

    _FinishGettingContactImages = function(hasPhotos) {
        ASC.CRM.SocialMedia.ContactImageListLoaded = true;
        jq("#divAjaxImageContainerPhotoLoad [id$='_ctrlImgAjaxLoader']").remove();
        if (hasPhotos) {
            jq("#divLoadPhotoFromSocialMedia").css("height", "100px");
        }
        jq("#divImagesHolder").css("display", "block");

        LoadingBanner.hideLoading();
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#divLoadPhotoWindow", 520, 550, 0);
    };

    _FindTwitterProfilesResponse = function (target, addTop, addLeft, response) {
        var tmplID = "TwitterProfileTmpl";
        if (target.parent().is(".emptyScrBttnPnl")) {
            tmplID = "TwitterProfileTabTmpl";
        }

        _RenderSMProfiles(response, tmplID);

        jq("#divSMProfilesWindow .divWait").hide();
        _CalculateProfilesWindowHeight();

        ASC.CRM.SocialMedia.TwitterTargetTextbox = jq(target).parent().parent().children('table').find('input');
        _ShowProfilesWindow();
    };

    _FindFacebookProfilesResponse = function(target, addTop, addLeft, response) {
        _RenderSMProfiles(response, "FacebookProfileTmpl");
        jq("#divSMProfilesWindow .divWait").hide();
        _CalculateProfilesWindowHeight();

        ASC.CRM.SocialMedia.FacebookTargetTextbox = jq(target).parent().parent().children('table').find('input');
        _ShowProfilesWindow();
    };

    _CalculateProfilesWindowHeight = function() {
        var height = jq("#sm_tbl_UserList").outerHeight();
        if (height > 200) {
            return 270;
        }
        if (height == 0) {
            return 100;
        }
        return height + 65;
    };

    _CalculateProfilesWindowPosition = function(targetElement, addTop, addLeft) {
        var top = addTop == null ? 0 : addTop,
            left = addLeft == null ? 0 : addLeft,
            dropdownItem = jq("#divSMProfilesWindow"),
            targetPos = targetElement.offset();
        elemPosTop = targetPos.top - 23 + top;
        elemPosLeft = targetPos.left + targetElement.outerWidth() + 3 + left;

        dropdownItem.css(
            {
                'position': 'absolute',
                'top': elemPosTop,
                'left': elemPosLeft
            });
    };

    _RenderSMProfiles = function(profiles, templateID) {
        var profileCount = profiles.length;
        if (profileCount > 0) {
            for (var i = 0; i < profileCount; i++) {
                jq.tmpl(templateID, profiles[i]).appendTo("#sm_tbl_UserList");
            }
        } else {
            jq("#divSMProfilesWindow .divNoProfiles").css("display", "block");
        }
    };

    _AddTwitterProfileToContactResponse = function(response) {
        if (response.error != null) toastr.error("Error");
        _HideProfilesWindow();
    };

    _ShowProfilesWindow = function() {
        jq("#divSMProfilesWindow").show();
        var windowHeight = _CalculateProfilesWindowHeight();
        jq("#divSMProfilesWindow").css("height", windowHeight);

        jq("#divSMProfilesWindow").unbind("mouseenter").mouseenter(function() {
            ASC.CRM.SocialMedia.MouseInProfilesWindow = true;
        });
        jq("#divSMProfilesWindow").unbind("mouseleave").mouseleave(function() {
            ASC.CRM.SocialMedia.MouseInProfilesWindow = false;
        });
        jq(document).bind("click", _CheckProfilesWindow);
    };

    _ShowWaitProfilesWindow = function(name) {
        jq("#divSMProfilesWindow .divWait").show();
        jq("#divSMProfilesWindow .divHeader span").text(jq.format(ASC.CRM.Resources.CRMJSResource.PossibleSocialMediaAccounts, name));

        var windowHeight = _CalculateProfilesWindowHeight();
        jq("#divSMProfilesWindow").css("height", windowHeight);
        jq("#divSMProfilesWindow").show();
    };

    _CheckProfilesWindow = function() {
        if (ASC.CRM.SocialMedia.MouseInProfilesWindow == false) {
            _HideProfilesWindow();
        }
    };

    _HideProfilesWindow = function() {
        jq("#divSMProfilesWindow").hide();
        jq(document).unbind("click", _CheckProfilesWindow);
    };


    return {
        init: function (defaultAvatarSrc) {
            ASC.CRM.SocialMedia.SocialMediaLoaded = false;
            ASC.CRM.SocialMedia.ContactImageListLoaded = false;
            ASC.CRM.SocialMedia.MouseInProfilesWindow = false;
            ASC.CRM.SocialMedia.selectedPersons = new Array();
            ASC.CRM.SocialMedia.defaultAvatarSrc = defaultAvatarSrc;

            ASC.CRM.SocialMedia.socialNetworks = new Array();
        },

        initTab: function (isCompany) {

            jq.tmpl("twitterMessageListPanelTmpl").appendTo("#divSocialMediaContent");

            jq.tmpl("template-emptyScreen",
            {
                ID: "tweetsEmptyScreen",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_twitter"],
                Header: ASC.CRM.Resources.CRMSocialMediaResource.EmptyContentTwitterAccountsHeader,
                Describe: ASC.CRM.Resources.CRMSocialMediaResource.EmptyContentTwitterAccountsDescribe,
                ButtonHTML: ["<a class='link dotline plus' href='javascript:void(0);'",
                              "onclick='ASC.CRM.SocialMedia.FindTwitterProfiles(jq(this),\"",
                              isCompany ? "company" : "people",
                              "\", 1, 9);'>",
                              ASC.CRM.Resources.CRMSocialMediaResource.LinkTwitterAccount,
                              "</a>"]
                        .join(''),
                CssClass: "display-none"
            }).appendTo("#divSocialMediaContent");
        },

        activate: function (hasTwitter) {
            if (ASC.CRM.SocialMedia.SocialMediaLoaded == false) {
                ASC.CRM.SocialMedia.SocialMediaLoaded = true;
                if (hasTwitter) {
                    ASC.CRM.SocialMedia.LoadContactActivity();
                } else {
                    jq("#tweetsEmptyScreen.display-none").removeClass("display-none");
                }
            }
        },

        switchCheckedPersonsInCompany: function(checked) {
            jq("#chbPersonsRelationship input[type='checkbox']").prop("checked", checked);
        },

        ShowErrorMessage:  function(text) {
            jq("#smErrorDescriptionContainer").css("display", "block");
            jq("#smErrorDescription").text(text);
        },

        LoadContactActivity: function() {
            jq("#smErrorDescriptionContainer").css("display", "none");
            jq("#smErrorDescription").text("");

            LoadingBanner.displayLoading();
            var contactID = jq("[id$='_ctrlContactID']").val(),
                number = jq.cookies.get("sm_msg_count");
            if (number == null || number === undefined || isNaN(number)) {
                number = 10;
            }

            Teamlab.getCrmContactTweets({}, contactID, number,
                {
                    max_request_attempts: 1,
                    success: CallbackMethods.getContactTweets,
                    error: function (params, errors) {
                        var err = errors[0];
                        LoadingBanner.hideLoading();
                        try {
                            var json = jq.parseJSON(err);
                            err = json.description;
                        } catch (e) { }

                        if (err === ASC.CRM.Resources.SocialMediaAccountNotFoundTwitter) {
                            jq("#tweetsEmptyScreen.display-none").removeClass("display-none");
                        } else {
                            ASC.CRM.SocialMedia.ShowErrorMessage(err);
                        }
                    }
                });
        },

        GetContactImageList: function() {
            if (ASC.CRM.SocialMedia.socialNetworks != null && ASC.CRM.SocialMedia.socialNetworks.length != 0) {
                Teamlab.getCrmContactSocialMediaAvatar({}, ASC.CRM.SocialMedia.socialNetworks,
                        {
                            max_request_attempts: 1,
                            success: function (params, response) {
                                _GetContactSMImagesResponse(response);
                            },
                            error: function (params, errors) {
                                _GetContactSMImagesResponse([]);
                            }
                        });

            } else {
                _GetContactSMImagesResponse([]);
            }
        },

        OpenLoadPhotoWindow: function () {
            var $link = jq(".linkChangePhoto");
            if (!$link || ($link && !$link.hasClass("disable"))) {

                jq("[name='chbSocialNetwork']").removeAttr("checked");

                var curAvatarSrc = jq("img.contact_photo").attr("src");
                if (curAvatarSrc.indexOf(ASC.CRM.SocialMedia.defaultAvatarSrc + '?') == 0) {
                    jq("#divLoadPhotoDefault").addClass("display-none");
                    jq("#divLoadPhotoFromSocialMedia").css("margin-top", "10px");
                } else {
                    jq("#divLoadPhotoDefault").removeClass("display-none");
                    jq("#divLoadPhotoFromSocialMedia").css("margin-top", "-16px");
                }

                if (ASC.CRM.SocialMedia.ContactImageListLoaded == false) {
                    LoadingBanner.displayLoading();
                    ASC.CRM.SocialMedia.GetContactImageList();
                } else {
                    PopupKeyUpActionProvider.EnableEsc = false;
                    StudioBlockUIManager.blockUI("#divLoadPhotoWindow", 520, 550, 0);
                }
            }
        },

        SelectUserAvatar: function(event, socialNetwork, identity) {
            if (!event.target) {
                event.target = event.srcElement;
            }
            var imageChecked = jq(event.target).is(":checked");
            if (imageChecked == false) { return; }
            jq("[name='chbSocialNetwork']").not(jq(event.target)).removeAttr("checked");
        },

        UploadUserAvatar: function (event, socialNetwork, userIdentity) {
            LoadingBanner.displayLoading();
            jq(".under_logo .linkChangePhoto").addClass("disable");
            var contactId = jq("[id$='_ctrlContactID']").val();
            if (contactId == "") {
                contactId = 0;
            }
            var uploadOnly = jq("#divImagesHolder").attr("data-uploadOnly") == "true",
                data = { contactId: contactId, socialNetwork: socialNetwork, userIdentity: userIdentity, uploadOnly: uploadOnly };

            if (jq("#uploadPhotoPath").length == 1) {
                data.tmpDirName = jq("#uploadPhotoPath").val();
            }

            Teamlab.updateCrmContactAvatar({}, contactId, data, { 
                success: function (params, response) {
                        PopupKeyUpActionProvider.CloseDialog();
                        jq(".under_logo .linkChangePhoto").removeClass("disable");
                        LoadingBanner.hideLoading();

                        var now = new Date();
                        jq("img.contact_photo").attr("src", response.url + '?' + now.getTime());
                        if (jq("#uploadPhotoPath").length == 1) {
                            jq("#uploadPhotoPath").val(response.path);
                        }
                    },
                error: function (params, errors) {
                        PopupKeyUpActionProvider.CloseDialog();
                        jq(".under_logo .linkChangePhoto").removeClass("disable");
                        LoadingBanner.hideLoading();

                        toastr.error(ASC.CRM.Resources.CRMJSResource.ErrorMessage_SaveImageError);
                    }
                });
        },

        DeleteContactAvatar: function () {
            LoadingBanner.displayLoading();
            jq(".under_logo .linkChangePhoto").addClass("disable");
            var contactId = jq("[id$='_ctrlContactID']").val();
            if (contactId == "") {
                contactId = 0;
            }
            var contactType = jq.getURLParam("type"),
                uploadOnly = jq("#divImagesHolder").attr("data-uploadOnly") == "true",
                data = { contactId: contactId, uploadOnly: uploadOnly, contactType: contactType };

            Teamlab.removeCrmContactAvatar({}, contactId, data, {
                success: function (params, response) {
                    PopupKeyUpActionProvider.CloseDialog();
                    jq(".under_logo .linkChangePhoto").removeClass("disable");
                    LoadingBanner.hideLoading();

                    var now = new Date();
                    jq("img.contact_photo").attr("src",
                        [
                            response != "" ? response : ASC.CRM.SocialMedia.defaultAvatarSrc,
                            '?',
                            now.getTime()]
                        .join(''));
                },
                error: function (params, errors) {
                    PopupKeyUpActionProvider.CloseDialog();
                    jq(".under_logo .linkChangePhoto").removeClass("disable");
                    LoadingBanner.hideLoading();

                    toastr.error(ASC.CRM.Resources.CRMJSResource.ErrorMessage_SaveImageError);
                }
            });
        },

        FindTwitterProfiles: function(target, contactType, addTop, addLeft) {
            _HideProfilesWindow();
            jq("#divSMProfilesWindow .divNoProfiles").css("display", "none");
            jq("#sm_tbl_UserList").html("");
            jq("#divSMProfilesWindow .divSMProfilesWindowBody .errorBox").remove();

            var searchText;

            //contact type can be "company" or "people"
            if (jq("#baseInfo_Title").length == 1) {
                searchText = jq.trim(jq("#baseInfo_Title").val());
            } else {
                if (contactType == "company") {
                    searchText = jq("[name='baseInfo_companyName']").val();
                }
                if (contactType == "people") {
                    searchText = jq("[name='baseInfo_firstName']").val() + " " + jq("[name='baseInfo_lastName']").val();
                }
            }

            if (searchText === undefined || jq.trim(searchText).length == 0) {
                return;
            }

            _CalculateProfilesWindowPosition(jq(target), addTop, addLeft);
            _ShowWaitProfilesWindow(searchText);

            Teamlab.getCrmContactTwitterProfiles({}, searchText, {
                max_request_attempts: 1,
                success: function (params, response) {
                    _FindTwitterProfilesResponse(target, addTop, addLeft, response);
                },
                error: function (params, errors) {
                    var err = errors[0];
                    jq("#divSMProfilesWindow .divWait").hide();
                    jq("#divSMProfilesWindow .divSMProfilesWindowBody").prepend(jq("<div></div>").addClass("errorBox").text(err));
                }
            });
        },

        FindFacebookProfiles: function(target, contactType, addTop, addLeft) {
            _HideProfilesWindow();
            jq("#divSMProfilesWindow .divNoProfiles").css("display", "none");
            jq("#sm_tbl_UserList").html("");
            jq("#divSMProfilesWindow .divSMProfilesWindowBody .errorBox").remove();

            var searchText,
                isUser = true;

            //contact type can be "company" or "people"
            if (contactType == "company") {
                isUser = false;
                searchText = jq("[name='baseInfo_companyName']").val();
            }
            if (contactType == "people") {
                searchText = jq("[name='baseInfo_firstName']").val() + " " + jq("[name='baseInfo_lastName']").val();
            }

            if (searchText === undefined || jq.trim(searchText).length == 0) {
                return;
            }

            _CalculateProfilesWindowPosition(jq(target), addTop, addLeft);
            _ShowWaitProfilesWindow(searchText);

            Teamlab.getCrmContactFacebookProfiles({}, searchText, isUser, {
                max_request_attempts: 1,
                success: function (params, response) {
                    _FindFacebookProfilesResponse(target, addTop, addLeft, response);
                },
                error: function (params, errors) {
                    var err = errors[0];
                    jq("#divSMProfilesWindow .divWait").hide();
                    jq("#divSMProfilesWindow .divSMProfilesWindowBody").prepend(jq("<div></div>").addClass("errorBox").text(err));
                }
            });
        },

        AddAndSaveTwitterProfileToContact: function(twitterScreenName, contactID) {
            var params = {},
                data = {
                    data: twitterScreenName,
                    isPrimary: true,
                    infoType: "Twitter",
                    category: "Work"
                };
            Teamlab.addCrmContactTwitter(params, contactID, data,
            {
                success: CallbackMethods.addAndSaveTwitter,
                before: function(params) { },
                after: function(params) { _HideProfilesWindow(); }
            });
        },

        AddTwitterProfileToContact: function(twitterScreenName) {
            jq(ASC.CRM.SocialMedia.TwitterTargetTextbox).val(twitterScreenName);
            _HideProfilesWindow();
        },

        AddFacebookProfileToContact: function(profileId) {
            jq(ASC.CRM.SocialMedia.FacebookTargetTextbox).val(profileId);
            _HideProfilesWindow();
        }
    };
})();