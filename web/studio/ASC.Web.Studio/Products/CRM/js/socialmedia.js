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

    _GetContactSMImagesResponse = function(response) {
        var result = jq.parseJSON(response),
            imageCount = result.length;
        if (imageCount > 0) {
            for (var i = 0; i < imageCount; i++) {
                jq.tmpl("socialMediaAvatarTmpl", result[i]).appendTo("#divImagesHolder");
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
    };

    _FindTwitterProfilesResponse = function(target, addTop, addLeft, response) {
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

    _FindLinkedInProfilesResponse = function (target, addTop, addLeft, response) {
        _RenderSMProfiles(response, "LinkedInProfileTmpl");
        jq("#divSMProfilesWindow .divWait").hide();
        _CalculateProfilesWindowHeight();

        ASC.CRM.SocialMedia.LinkedInTargetTextbox = jq(target).parent().parent().children('table').find('input');
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

    _ShowCrunchbaseContact = function (contactNamespace, result) {
        var resObj = jq.parseJSON(result);
        resObj.namespace = contactNamespace;
        jq("#divSMContactsSearchContainer .divWaitForSearching").hide();
        jq("#divSMContactsSearchContainer .divNoProfiles").css("display", "none");

        jq("#divContactDescription").css("display", "block");
        jq("#divCrbsContactConfirm").css("display", "block");
        jq("#divContactDescription").html("");

        var uniqueRelationships = [],
            permalinks = [],
            item = {};
        if (contactNamespace == "organization" && resObj.relationships && resObj.relationships.length != 0) {
            for (var i = 0, n = resObj.relationships.length; i < n; i++) {
                item = resObj.relationships[i];
                if (jQuery.inArray(item.person.permalink, permalinks) == -1) {
                    permalinks.push(item.person.permalink);
                    uniqueRelationships.push(item);
                }
            }
            resObj.relationships = uniqueRelationships;
        }

        jq.tmpl("crunchbaseContactFullTmpl", resObj).appendTo("#divContactDescription");
        if (jq("#tblCompanyFields > tbody > tr").length == 0) {
            jq("#divContactDescription").html("");
            jq("#divCrbsContactConfirm").css("display", "none");
            jq("#divSMContactsSearchContainer .divNoProfiles").css("display", "block");
        }
    };

    return {
        init: function (defaultAvatarSrc) {
            ASC.CRM.SocialMedia.SocialMediaLoaded = false;
            ASC.CRM.SocialMedia.ContactImageListLoaded = false;
            ASC.CRM.SocialMedia.MouseInProfilesWindow = false;
            ASC.CRM.SocialMedia.selectedPersons = new Array();
            ASC.CRM.SocialMedia.defaultAvatarSrc = defaultAvatarSrc;
        },

        initTab: function (isCompany) {

            jq.tmpl("twitterMessageListPanelTmpl").appendTo("#divSocialMediaContent");

            jq.tmpl("emptyScrTmpl",
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

        initFindInCrunchbasePanel: function (blockSelector) {
            jq.tmpl("blockUIPanelTemplate", {
                id: "divSMContactsSearchContainer",
                headerTest: ASC.CRM.Resources.CRMSocialMediaResource.ProfilesInSocialMedia,
                questionText: "",
                innerHtmlText: jq.tmpl("findInCrunchbasePanelBodyTmpl").html(),
                OKBtn: "",
                CancelBtn: "",
                progressText: ""
            }).insertAfter(blockSelector);
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
            var contactID = jq("input[id$='_ctrlContactID']").val();

            if (contactID != "") {
                jq("#divAjaxImageContainerPhotoLoad").append(jq("[id$='_ctrlImgAjaxLoader']").clone().css("display", "block"));
                jq("#divAjaxImageContainerPhotoLoad").css("display", "block");

                Teamlab.getCrmContactSocialMediaAvatar({}, contactID,
                    {
                        max_request_attempts: 1,
                        success: function (params, response) {
                            _GetContactSMImagesResponse(response)
                        },
                        error: function (params, errors) {
                            _GetContactSMImagesResponse("[]");
                        }
                    });
            } else {
                _GetContactSMImagesResponse("[]");
            }
        },

        OpenLoadPhotoWindow: function () {
            var $link = jq(".linkChangePhoto");
            if (!$link || ($link && !$link.hasClass("disable"))) {
                PopupKeyUpActionProvider.EnableEsc = false;
                StudioBlockUIManager.blockUI("#divLoadPhotoWindow", 520, 550, 0);
                jq("[name='chbSocialNetwork']").removeAttr("checked");
                if (ASC.CRM.SocialMedia.ContactImageListLoaded == false) {
                    ASC.CRM.SocialMedia.GetContactImageList();
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


            Teamlab.updateCrmContactAvatar({}, contactId, data, { 
                success: function (params, response) {
                        PopupKeyUpActionProvider.CloseDialog();
                        jq(".under_logo .linkChangePhoto").removeClass("disable");
                        LoadingBanner.hideLoading();

                        var now = new Date();
                        jq("img.contact_photo").attr("src", response + '?' + now.getTime());
                        if (jq("#uploadPhotoPath").length == 1) {
                            jq("#uploadPhotoPath").val(response);
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
                    if (jq("#uploadPhotoPath").length == 1) {
                        jq("#uploadPhotoPath").val(response);
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

            var searchText;

            //contact type can be "company" or "people"
            if (contactType == "company") {
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

            Teamlab.getCrmContactFacebookProfiles({}, searchText, {
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

        FindLinkedInProfiles: function(target, contactType, addTop, addLeft) {
            _HideProfilesWindow();
            jq("#divSMProfilesWindow .divNoProfiles").css("display", "none");
            jq("#sm_tbl_UserList").html("");

            var searchText;
            //contact type can be only "person"
            if (contactType == "company")
                return;
            if (contactType == "people") {
                jq("#divSMProfilesWindow .divSMProfilesWindowBody .errorBox").remove();

                var firstName = jq("[name='baseInfo_firstName']").val(),
                    lastName = jq("[name='baseInfo_lastName']").val(),
                    searchText = firstName + " " + lastName;

                if (searchText === undefined || jq.trim(searchText).length == 0) {
                    return;
                }

                _CalculateProfilesWindowPosition(jq(target), addTop, addLeft);
                _ShowWaitProfilesWindow(searchText);

                Teamlab.getCrmContactLinkedinProfiles({}, firstName, lastName,
                    {
                        max_request_attempts: 1,
                        success: function (params, response) {
                            _FindLinkedInProfilesResponse(target, addTop, addLeft, response);
                        },
                        error: function (params, errors) {
                            var err = errors[0];
                            jq("#divSMProfilesWindow .divWait").hide();
                            jq("#divSMProfilesWindow .divSMProfilesWindowBody").prepend(jq("<div></div>").addClass("errorBox").text(err));
                        }
                });
            }
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
        },

        AddLinkedInProfileToContact: function(profileId, position, publicProfileUrl) {
            if (jq.trim(jq("[name='baseInfo_personPosition']").val()).length == 0) {
                jq("[name='baseInfo_personPosition']").val(jQuery.base64.decode(position));
            }

            jq(ASC.CRM.SocialMedia.LinkedInTargetTextbox).val(jQuery.base64.decode(publicProfileUrl));
            _HideProfilesWindow();
        },

        FindContacts: function(isCompany) {
            jq("#divSMContactsSearchContainer .divNoProfiles").hide();
            jq("#divSMContactsSearchContainer .divWaitForAdding").hide();
            jq("#divSMContactsSearchContainer #divModalContent .errorBox").remove();

            var searchUrl = "";
            if (isCompany) {
                var name = jq("[name='baseInfo_companyName']").val().trim();
                if (name == "") { return; }
                searchUrl = "http://api.crunchbase.com/v/2/organizations?name=" + name;
            } else {
                var first_name = jq("[name='baseInfo_firstName']").val().trim(),
                    last_name = jq("[name='baseInfo_lastName']").val().trim();
                if (first_name == "" || last_name == "") { return; }
                searchUrl = "http://api.crunchbase.com/v/2/people?first_name=" + first_name + "&last_name=" + last_name;
            }

            var contactNamespace = isCompany ? "organization" : "person";
            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI("#divSMContactsSearchContainer", 550, 500, 0);

            jq("#divContactDescription").css("display", "none");
            jq("#divCrbsContactConfirm").css("display", "none");
            jq("#divSMContactsSearchContainer .divWaitForSearching").show();

            Teamlab.getCrmContactInCruchBase({},
                { searchUrl: searchUrl, contactNamespace: contactNamespace },
                {
                    max_request_attempts: 1,
                    success: function (params, response) {
                        _ShowCrunchbaseContact(contactNamespace, response);
                    },
                    error: function (params, errors) {
                        jq("#divSMContactsSearchContainer .divWaitForSearching").hide();

                        var err = errors[0];
                        jq("#divSMProfilesWindow .divWait").hide();
                        jq("#divSMContactsSearchContainer #divModalContent").prepend(jq("<div></div>").addClass("errorBox").text(err));
                    }
            });
        },

        ConfirmCrunchbaseContact: function() {
            jq("#divCrbsContactConfirm").hide();
            jq("#divSMContactsSearchContainer .divWaitForAdding").show();

            if (jq("#chbWebsite").is(":checked")) {
                var newValue = jq("#crbsWebSite").val(),
                    $newContact = ASC.CRM.ContactActionView.createNewCommunication("websiteAndSocialProfilesContainer", newValue);
                ASC.CRM.ContactActionView.changeSocialProfileCategory(
                    $newContact.find('a.social_profile_type'),
                    2,
                    jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category="2"]').text(),
                    jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category="2"]').attr('categoryName')
                );
                $newContact.insertAfter(jq("#websiteAndSocialProfilesContainer").children('div:last')).show();
            }

            if (jq("#chbEmail").is(":checked")) {
                var newValue = jq("#crbsEmail").val(),
                    $newContact = ASC.CRM.ContactActionView.createNewCommunication("emailContainer", newValue, jq("#emailContainer").find(".primary_field").length == 0);
                $newContact.insertAfter(jq("#emailContainer").children('div:last')).show();
                jq('#emailContainer').prev('dt').removeClass('crm-withGrayPlus');
            }

            if (jq("#chbPhoneNumber").is(":checked")) {
                var newValue = jq("#crbsPhoneNumber").val(),
                    $newContact = ASC.CRM.ContactActionView.createNewCommunication("phoneContainer", newValue, jq("#phoneContainer").find(".primary_field").length == 0);
                $newContact.insertAfter(jq("#phoneContainer").children('div:last')).show();
                jq('#phoneContainer').prev('dt').removeClass('crm-withGrayPlus');
            }

            if (jq("#chbTwitter").is(":checked")) {
                var newValue = jq("#crbsTwitterUserName").val(),
                    $newContact = ASC.CRM.ContactActionView.createNewCommunication("websiteAndSocialProfilesContainer", newValue);
                $newContact.insertAfter(jq("#websiteAndSocialProfilesContainer").children('div:last')).show();
                ASC.CRM.ContactActionView.changeSocialProfileCategory(
                    $newContact.find('a.social_profile_type'),
                    4,
                    jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category="4"]').text(),
                    jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category="4"]').attr('categoryName')
                );
            }

            if (jq("#chbBlog").is(":checked")) {
                var newValue = jq("#crbsBlogUrl").val(),
                    $newContact = ASC.CRM.ContactActionView.createNewCommunication("websiteAndSocialProfilesContainer", newValue);
                ASC.CRM.ContactActionView.changeSocialProfileCategory(
                    $newContact.find('a.social_profile_type'),
                    11,
                    jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category="11"]').text(),
                    jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category="11"]').attr('categoryName')
                );
                $newContact.insertAfter(jq("#websiteAndSocialProfilesContainer").children('div:last')).show();
            }
            if (jq("#chbBlogFeed").is(":checked")) {
                var newValue = jq("#crbsBlogFeedUrl").val(),
                    $newContact = ASC.CRM.ContactActionView.createNewCommunication("websiteAndSocialProfilesContainer", newValue);
                ASC.CRM.ContactActionView.changeSocialProfileCategory(
                    $newContact.find('a.social_profile_type'),
                    11,
                    jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category="11"]').text(),
                    jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category="11"]').attr('categoryName')
                );
                $newContact.insertAfter(jq("#websiteAndSocialProfilesContainer").children('div:last')).show();
            }

            if (jq("#chbDescription").is(":checked")) {
                var newValue = jq("#crbsOverview").val();
                if (jq("#overviewContainer").children("div").length == 1) {
                    var $newContact = ASC.CRM.ContactActionView.createNewCommunication("overviewContainer", newValue);
                    $newContact.insertAfter(jq("#overviewContainer").children('div:last')).show();
                } else {
                    jq("#overviewContainer").children("div:last").find("[name='baseInfo_contactOverview']").val(newValue);
                }
                jq('#overviewContainer').prev('dt').removeClass('crm-withGrayPlus');
            }


            var add_new_button_class = "crm-addNewLink",
                delete_button_class = "crm-deleteLink";
            jq('#emailContainer').children('div:not(:first)').find("." + add_new_button_class).hide();
            jq('#emailContainer').children('div:last').find("." + add_new_button_class).show();

            jq('#phoneContainer').children('div:not(:first)').find("." + add_new_button_class).hide();
            jq('#phoneContainer').children('div:last').find("." + add_new_button_class).show();

            if (jq('#websiteAndSocialProfilesContainer').children('div').length > 1) {
                jq('#websiteAndSocialProfilesContainer').prev('dt').removeClass('crm-withGrayPlus');
            }
            jq('#websiteAndSocialProfilesContainer').children('div:not(:first)').find("." + add_new_button_class).hide();
            jq('#websiteAndSocialProfilesContainer').children('div:last').find("." + add_new_button_class).show();


            if (jq("#chbImage").is(":checked")) {
                var imageObj = jq.parseJSON(jq("#crbsImageJSON").val()).available_sizes,
                    imgSrc = imageObj[imageObj.length - 1][1];

                jq("#uploadPhotoPath").val(imgSrc);
                jq("#contactPhoto img").attr("src", imgSrc);
            }

            var relationshipObj = jq.parseJSON(jq("#crbsPeopleJSON").val()),
                data = [];
            for (var i = 0, n = relationshipObj.length; i < n; i++) {
                if (relationshipObj[i].person) {
                    if (jq("#chbPersonsRelationship input[id=" + relationshipObj[i].person.permalink + "]").is(":checked")) {
                        var person = {
                                        Key: relationshipObj[i].person.first_name,
                                        Value: relationshipObj[i].person.last_name,
                                        canEdit: true,
                                        showUnlinkBtn: true,
                                        displayName: [relationshipObj[i].person.first_name, " ", relationshipObj[i].person.last_name].join(''),
                                        id: relationshipObj[i].person.permalink,
                                        isCompany: false,
                                        isPrivate: false,
                                        smallFotoUrl: ASC.CRM.Data.EmptyScrImgs["empty_people_logo_40_40"]
                                    };
                        ASC.CRM.SocialMedia.selectedPersons.push(person);
                        data.push(person);
                    }
                }
            }

            if (data.length > 0) {
                //jq.tmpl("simpleContactTmpl", data).prependTo("#contactTable tbody");
                for (var i = 0, n = data.length; i < n; i++) {
                    data[i].isShared = false;
                }
                ASC.CRM.ListContactView.CallbackMethods.addMember({}, data);
                if (jq("#contactTable tr").length > 0) {
                    jq("#contactListBox").parent().removeClass('hiddenFields');
                }
            }
            jq(window).trigger("confirmCrunchBaseContactSuccess", null);
            PopupKeyUpActionProvider.CloseDialog();
        }
    };
})();