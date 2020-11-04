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
    ASC.CRM = (function() { return {} })();
}

ASC.CRM.ListContactView = (function() {
    var _setCookie = function(page, countOnPage) {
        if (ASC.CRM.ListContactView.cookieKey && ASC.CRM.ListContactView.cookieKey != "") {
            var cookie = {
                    page: page,
                    countOnPage: countOnPage
                },
                path = '/',
                parts = location.pathname.split('/');
            parts.splice(parts.length - 1, 1);
            path = parts.join('/');

            jq.cookies.set(ASC.CRM.ListContactView.cookieKey, cookie, { path: path });
        }
    };

    var _lockMainActions = function() {
        jq("#contactsHeaderMenu .menuActionDelete").removeClass("unlockAction");
        jq("#contactsHeaderMenu .menuActionAddTag").removeClass("unlockAction");
        jq("#contactsHeaderMenu .menuActionAddTask").removeClass("unlockAction");
        jq("#contactsHeaderMenu .menuActionPermissions").removeClass("unlockAction");
        jq("#contactsHeaderMenu .menuActionSendEmail").removeClass("unlockAction");
    };

    var _checkForLockMainActions = function() {
        var count = ASC.CRM.ListContactView.selectedItems.length;
        if (count === 0) {
            _lockMainActions();
            return;
        }

        var unlockDelete = false,
            unlockAddTag = false,
            unlockSendEmail = false;

        for (var i = 0; i < count; i++) {
            if (ASC.CRM.ListContactView.selectedItems[i].canDelete) {
                unlockDelete = true;
            }
            if (ASC.CRM.ListContactView.selectedItems[i].canEdit) {
                unlockAddTag = true;
            }
            if (ASC.CRM.ListContactView.selectedItems[i].primaryEmail != null) {
                unlockSendEmail = true;
            }
        }

        if (unlockDelete) {
            jq("#contactsHeaderMenu .menuActionDelete:not('.unlockAction')").addClass("unlockAction");
        } else {
            jq("#contactsHeaderMenu .menuActionDelete.unlockAction").removeClass("unlockAction");
        }

        if (unlockAddTag) {
            jq("#contactsHeaderMenu .menuActionAddTag:not(.unlockAction)").addClass("unlockAction");
        } else {
            jq("#contactsHeaderMenu .menuActionAddTag.unlockAction").removeClass("unlockAction");
        }

        jq("#contactsHeaderMenu .menuActionPermissions:not(.unlockAction)").addClass("unlockAction");
        jq("#contactsHeaderMenu .menuActionAddTask:not(.unlockAction)").addClass("unlockAction");

        if (unlockSendEmail) {
            jq("#contactsHeaderMenu .menuActionSendEmail").addClass("unlockAction");
        } else {
            jq("#contactsHeaderMenu .menuActionSendEmail").removeClass("unlockAction");
        }
    };


    var _initPageNavigatorControl = function (countOfRows, currentPageNumber) {
        window.contactPageNavigator = new ASC.Controls.PageNavigator.init("contactPageNavigator", "#divForContactPager", countOfRows, ASC.CRM.Data.VisiblePageCount, currentPageNumber,
                                                                        ASC.CRM.Resources.CRMJSResource.Previous, ASC.CRM.Resources.CRMJSResource.Next);

        contactPageNavigator.changePageCallback = function(page) {
            _setCookie(page, contactPageNavigator.EntryCountOnPage);

            var startIndex = contactPageNavigator.EntryCountOnPage * (page - 1);
            ASC.CRM.ListContactView.renderContent(startIndex);
        };
    };

    var _initContactActionMenu = function() {

        jq.dropdownToggle({
            dropdownID: "contactActionMenu",
            switcherSelector: "#companyTable .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            beforeShowFunction: function (switcherObj, dropdownItem) {
                var contactId = switcherObj.attr("id").split('_')[1];
                if (!contactId) {
                    return;
                }
                _showActionMenu(parseInt(contactId));
            },
            showFunction: function(switcherObj, dropdownItem) {
                jq("#companyTable .entity-menu.active").removeClass("active");
                if (dropdownItem.is(":hidden")) {
                    switcherObj.addClass("active");
                }
            },
            hideFunction: function() {
                jq("#companyTable .entity-menu.active").removeClass("active");
            }
        });


        jq("body").unbind("contextmenu").bind("contextmenu", function(event) {
            var e = jq.fixEvent(event);

            if (typeof e == "undefined" || !e) {
                return true;
            }

            var target = jq(e.srcElement || e.target);

            if (!target.parents("#companyTable").length) {
                jq("#contactActionMenu").hide();
                return true;
            }

            var contactId = parseInt(target.closest("tr.with-entity-menu").attr("id").split('_')[1]);
            if (!contactId) {
                return true;
            }
            _showActionMenu(contactId);
            jq("#companyTable .entity-menu.active").removeClass("active");

            jq.showDropDownByContext(e, target, jq("#contactActionMenu"));

            return false;
        });

        if (ASC.VoipNavigationItem && ASC.VoipNavigationItem.isInit) {
            jq("#companyTable").on("click", ".primaryDataContainer .primaryPhone", function () {
                var contactId = jq(this).parents("tr:first").attr("id").split("_")[1];
                _openVoipClient(contactId)
            });
        }
    };

    var _openVoipClient = function (contactId) {
        try {
            if (contactId) {
                ASC.VoipNavigationItem.call(contactId);
            }
        } catch (e) {
            console.error(e);
        }
    };

    var _initEnableVoipSettingsPanel = function () {
        var helpLink = "";

        if (ASC.Resources.Master.HelpLink) {
            helpLink = [
                "<div class='headerPanelSmall-splitter'>",
                String.format(ASC.CRM.Resources.CRMJSResource.VoipSettingsLearnMore, "<a class='link underline' href='" + ASC.Resources.Master.HelpLink + "/guides/use-voip.aspx' target='_blank'>", "</a>"),
                "</div>",
            ].join('');
        }

        jq.tmpl("template-blockUIPanel", {
            id: "enableVoipSettingsPanel",
            headerTest: ASC.CRM.Resources.CRMJSResource.VoipSettingsPanelHeader,
            questionText: "",
            innerHtmlText:
                [
                    "<div class='headerPanelSmall-splitter'>",
                    ASC.CRM.Resources.CRMJSResource.VoipSettingsPanelText,
                    "</div>",
                    "<div class='headerPanelSmall-splitter usertext'>",
                    ASC.CRM.Resources.CRMJSResource.VoipSettingsPanelUserText,
                    "</div>",
                    "<div class='headerPanelSmall-splitter admintext display-none'>",
                    ASC.CRM.Resources.CRMJSResource.VoipSettingsPanelAdminText,
                    "&nbsp;<a class='link underline' href='/Management.aspx?type=9#Twilio'>",
                    ASC.CRM.Resources.CRMJSResource.GoToSettings,
                    "</a>",
                    "</div>",
                    helpLink
                ].join(''),
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.OK,
        }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");
    };

    var _showEnableVoipSettingsPanel = function () {
        PopupKeyUpActionProvider.EnableEsc = true;
        jq("#enableVoipSettingsPanel .usertext").toggleClass("display-none", Teamlab.profile.isAdmin);
        jq("#enableVoipSettingsPanel .admintext").toggleClass("display-none", !Teamlab.profile.isAdmin);
        StudioBlockUIManager.blockUI("#enableVoipSettingsPanel", 500);
    };

    var _initScrolledGroupMenu = function() {
        ScrolledGroupMenu.init({
            menuSelector: "#contactsHeaderMenu",
            menuAnchorSelector: "#mainSelectAll",
            menuSpacerSelector: "main .filter-content .header-menu-spacer",
            userFuncInTop: function() { jq("#contactsHeaderMenu .menu-action-on-top").hide(); },
            userFuncNotInTop: function() { jq("#contactsHeaderMenu .menu-action-on-top").show(); }
        });

        jq("#contactsHeaderMenu").on("click", ".menuActionDelete.unlockAction", function () {
            _showDeletePanel();
        });

        jq("#contactsHeaderMenu").on("click", ".menuActionSendEmail.unlockAction", function () {
            ASC.CRM.ListContactView.showSendEmailDialog();
        });

        jq("#contactsHeaderMenu").on("click", ".menuActionPermissions.unlockAction", function () {
            _showSetPermissionsPanel({ isBatch: true });
        });

        jq("#contactsHeaderMenu").on("click", ".menuActionAddTask.unlockAction", function () {
            ASC.CRM.ListContactView.showTaskPanel(ASC.CRM.ListContactView.selectedItems, false);
        });

    };

    var _renderContactPageNavigator = function(startIndex) {
        var tmpTotal;
        if (startIndex >= ASC.CRM.ListContactView.Total) {
            tmpTotal = startIndex + 1;
        } else {
            tmpTotal = ASC.CRM.ListContactView.Total;
        }
        contactPageNavigator.drawPageNavigator((startIndex / ASC.CRM.ListContactView.entryCountOnPage).toFixed(0) * 1 + 1, tmpTotal);
        //jq("#tableForContactNavigation").show();
    };

    var _renderSimpleContactPageNavigator = function() {
        jq("#contactsHeaderMenu .menu-action-simple-pagenav").html("");
        var $simplePN = jq("<div></div>"),
            lengthOfLinks = 0;
        if (jq("#divForContactPager .pagerPrevButtonCSSClass").length != 0) {
            lengthOfLinks++;
            jq("#divForContactPager .pagerPrevButtonCSSClass").clone().appendTo($simplePN);
        }
        if (jq("#divForContactPager .pagerNextButtonCSSClass").length != 0) {
            lengthOfLinks++;
            if (lengthOfLinks === 2) {
                jq("<span style='padding: 0 8px;'>&nbsp;</span>").clone().appendTo($simplePN);
            }
            jq("#divForContactPager .pagerNextButtonCSSClass").clone().appendTo($simplePN);
        }
        if ($simplePN.children().length != 0) {
            $simplePN.appendTo("#contactsHeaderMenu .menu-action-simple-pagenav");
            jq("#contactsHeaderMenu .menu-action-simple-pagenav").show();
        } else {
            jq("#contactsHeaderMenu .menu-action-simple-pagenav").hide();
        }
    };

    var _renderCheckedContactsCount = function(count) {
        if (count != 0) {
            jq("#contactsHeaderMenu .menu-action-checked-count > span").text(jq.format(ASC.CRM.Resources.CRMJSResource.ElementsSelectedCount, count));
            jq("#contactsHeaderMenu .menu-action-checked-count").show();
        } else {
            jq("#contactsHeaderMenu .menu-action-checked-count > span").text("");
            jq("#contactsHeaderMenu .menu-action-checked-count").hide();
        }
    };

    var _renderNoContactsEmptyScreen = function() {
        jq("#companyTable tbody tr").remove();
        jq("#contactsFilterContainer, #contactsHeaderMenu, #mainContactList, #tableForContactNavigation").hide();

        ASC.CRM.Common.hideExportButtons();
        jq("#emptyContentForContactsFilter").hide();
        jq("#contactsEmptyScreen").show();
    };

    var _renderNoContactsForQueryEmptyScreen = function() {
        jq("#companyTable tbody tr").remove();
        jq("#contactsHeaderMenu, #companyListBox, #tableForContactNavigation").hide();
        jq("#contactsFilterContainer").show();
        jq("#mainSelectAll").attr("disabled", true);

        ASC.CRM.Common.hideExportButtons();
        jq("#contactsEmptyScreen").hide();
        jq("#emptyContentForContactsFilter").show();
    };

    var _showActionMenu = function(contactID) {
        var contact = null;

        for (var i = 0, n = ASC.CRM.ListContactView.fullContactList.length; i < n; i++) {
            if (contactID == ASC.CRM.ListContactView.fullContactList[i].id) {
                contact = ASC.CRM.ListContactView.fullContactList[i];
                break;
            }
        }
        if (contact == null) return;

        jq("#contactActionMenu .addTaskLink").unbind("click").bind("click", function() {
            jq("#contactActionMenu").hide();
            jq("#taskActionMenu").hide();
            jq("#companyTable .entity-menu.active").removeClass("active");

            ASC.CRM.ListContactView.showTaskPanel(contact, false);
        });

        jq("#contactActionMenu .addDealLink").attr("href", jq.format("Deals.aspx?action=manage&contactID={0}", contactID));
        jq("#contactActionMenu .addCaseLink").attr("href", jq.format("Cases.aspx?action=manage&contactID={0}", contactID));

        if (contact.primaryEmail != null && contact.primaryEmail.emailHref != "") {
            jq("#contactActionMenu .sendEmailLink").attr("href", contact.primaryEmail.emailHref);
            jq("#contactActionMenu .sendEmailLink").removeClass("display-none");
        } else {
            jq("#contactActionMenu .sendEmailLink").addClass("display-none");
        }

        if (contact.primaryPhone != null && ASC.Resources.Master.Hub.VoipAllowed) {
            jq("#contactActionMenu .makeVoIPCallLink").removeClass("display-none");
            jq("#contactActionMenu .makeVoIPCallLink").unbind("click").bind("click", function () {
                jq("#contactActionMenu").hide();
                jq("#companyTable .entity-menu.active").removeClass("active");
                if (ASC.VoipNavigationItem && ASC.VoipNavigationItem.isInit) {
                    _openVoipClient(contact.id);
                } else {
                    _showEnableVoipSettingsPanel();
                }
            });
        } else {
            jq("#contactActionMenu .makeVoIPCallLink").addClass("display-none");
        }

        if (contact.canEdit == true) {
            jq("#contactActionMenu .dropdown-item-seporator").show();
            jq("#contactActionMenu .addPhoneLink").show();
            jq("#contactActionMenu .addEmailLink").show();
            jq("#contactActionMenu .editContactLink").show();
            if (contact.canDelete){
                jq("#contactActionMenu .deleteContactLink").show();
            } else {
                jq("#contactActionMenu .deleteContactLink").hide();
            }
            jq("#contactActionMenu .showProfileLink").show();
            jq("#contactActionMenu .showProfileLinkNewTab").show();


            jq("#contactActionMenu .addPhoneLink").text(contact.primaryPhone != null ? ASC.CRM.Resources.CRMJSResource.EditPhone : ASC.CRM.Resources.CRMJSResource.AddNewPhone);
            jq("#contactActionMenu .addPhoneLink").unbind("click").bind("click", function () {
                jq("#contactActionMenu").hide();
                jq("#companyTable .entity-menu.active").removeClass("active");
                _showAddPrimaryPhoneInput(contactID, contact.primaryPhone);
            });

            jq("#contactActionMenu .addEmailLink").text(contact.primaryEmail != null ? ASC.CRM.Resources.CRMJSResource.EditEmail : ASC.CRM.Resources.CRMJSResource.AddNewEmail);
            jq("#contactActionMenu .addEmailLink").unbind("click").bind("click", function () {
                jq("#contactActionMenu").hide();
                jq("#companyTable .entity-menu.active").removeClass("active");
                _showAddPrimaryEmailInput(contactID, contact.primaryEmail);
            });



            jq("#contactActionMenu .editContactLink").attr("href",
                        jq.format("Default.aspx?id={0}&action=manage{1}", contactID, !contact.isCompany ? "&type=people" : ""));

            jq("#contactActionMenu .deleteContactLink").unbind("click").bind("click", function () {
                jq("#contactActionMenu").hide();
                jq("#companyTable .entity-menu.active").removeClass("active");
                ASC.CRM.ListContactView.showConfirmationPanelForDelete(contact.displayName, contact.id, contact.isCompany, true);
            });

            jq("#contactActionMenu .showProfileLink").attr("href", jq.format("Default.aspx?id={0}{1}", contactID, !contact.isCompany ? "&type=people" : ""));

            jq("#contactActionMenu .showProfileLinkNewTab").unbind("click").bind("click", function () {
                jq("#contactActionMenu").hide();
                jq("#companyTable .entity-menu.active").removeClass("active");
                window.open(jq.format("Default.aspx?id={0}{1}", contactID, !contact.isCompany ? "&type=people" : ""), "_blank");
            });

        } else {
            jq("#contactActionMenu .dropdown-item-seporator").hide();
            jq("#contactActionMenu .addPhoneLink").hide();
            jq("#contactActionMenu .addEmailLink").hide();
            jq("#contactActionMenu .editContactLink").hide();
            jq("#contactActionMenu .deleteContactLink").hide();
            jq("#contactActionMenu .showProfileLink").hide();
            jq("#contactActionMenu .showProfileLinkNewTab").hide();
        }

        //if (ASC.CRM.Data.IsCRMAdmin === true || Teamlab.profile.id == contact.createdBy.id) {
        //    jq("#contactActionMenu .setPermissionsLink").show();
        //    jq("#contactActionMenu .setPermissionsLink").unbind("click").bind("click", function() {
        //        jq("#contactActionMenu").hide();
        //        jq("#companyTable .entity-menu.active").removeClass("active");

        //        ASC.CRM.ListContactView.deselectAll();

        //        ASC.CRM.ListContactView.selectedItems.push(createShortContact(contact));
        //        _showSetPermissionsPanel({ isBatch: false });
        //    });
        //} else {
        //    jq("#contactActionMenu .setPermissionsLink").hide();
        //}
    };

    var _showAddPrimaryPhoneInput = function(contactId, primaryPhone) {
        var $phoneInput = jq("#addPrimaryPhone_" + contactId);
        $phoneInput.css("borderColor", "");

        var $phoneElement = jq("#contactItem_" + contactId).find(".primaryPhone");
        if ($phoneElement.length != 0) {
            $phoneElement.remove();
            if (primaryPhone != null) {
                $phoneInput.val(primaryPhone.data);
            }
        } else {
            $phoneInput.val("");
        }
        $phoneInput.show().focus();

        $phoneInput.unbind("blur").bind("blur", function () {
            $phoneInput.unbind("blur");
            var text = jq.trim($phoneInput.val());
            if (text.length == 0) {
                if (primaryPhone == null) {
                    $phoneInput.val("").hide();
                    return;
                } else {
                    _deletePrimaryPhone(contactId, text, primaryPhone);
                }
            } else {
                _addPrimaryPhone(contactId, text, primaryPhone);
            }
        });

        var keyupfunc = function (event) {
            if (ASC.CRM.ListContactView.isEnterKeyPressed(event)) {
                $phoneInput.unbind("keyup");
                $phoneInput.unbind("blur");
                var text = jq.trim($phoneInput.val());
                if (text.length == 0) {
                    if (primaryPhone == null) {
                        $phoneInput.val("").hide();
                        return;
                    } else {
                        _deletePrimaryPhone(contactId, text, primaryPhone);
                    }
                } else {
                    _addPrimaryPhone(contactId, text, primaryPhone);
                }
            }
        };
        $phoneInput.unbind("keyup").bind("keyup", keyupfunc);
    };

    var _addPrimaryPhone = function(contactId, phoneNumber, oldPrimaryPhone) {
        //            var reg = new RegExp(/(^\+)?(\d+)/);
        //            if (val == "" || !reg.test(val)) {

        if (oldPrimaryPhone == null) {
            var params = { contactId: contactId },
                data = {
                    data: phoneNumber,
                    isPrimary: true,
                    infoType: "Phone",
                    category: "Work"
                };

            Teamlab.addCrmContactInfo(params, contactId, data,
            {
                success: ASC.CRM.ListContactView.CallbackMethods.add_primary_phone,
                before: function(params) { jq("#check_contact_" + params.contactId).hide(); jq("#loaderImg_" + params.contactId).show(); },
                after: function(params) { jq("#check_contact_" + params.contactId).show(); jq("#loaderImg_" + params.contactId).hide(); }
            });
        } else {
            var params = { contactId: contactId },
                data = {
                    id: oldPrimaryPhone.id,
                    data: phoneNumber,
                    isPrimary: true,
                    infoType: "Phone"
                };

            Teamlab.updateCrmContactInfo(params, contactId, data,
            {
                success: ASC.CRM.ListContactView.CallbackMethods.add_primary_phone,
                before: function(params) { jq("#check_contact_" + params.contactId).hide(); jq("#loaderImg_" + params.contactId).show(); },
                after: function(params) { jq("#check_contact_" + params.contactId).show(); jq("#loaderImg_" + params.contactId).hide(); }
            });
        }
    };

    var _deletePrimaryPhone = function(contactId, phoneNumber, oldPrimaryPhone) {
        var params = { contactId: contactId };
        Teamlab.deleteCrmContactInfo(params, contactId, oldPrimaryPhone.id,
            {
                success: ASC.CRM.ListContactView.CallbackMethods.delete_primary_phone,
                before: function(params) {
                    jq("#check_contact_" + params.contactId).hide();
                    jq("#loaderImg_" + params.contactId).show();
                },
                after: function(params) {
                    jq("#check_contact_" + params.contactId).show();
                    jq("#loaderImg_" + params.contactId).hide();
                }
            });
    };

    var _showAddPrimaryEmailInput = function(contactId, primaryEmail) {
        var $emailInput = jq("#addPrimaryEmail_" + contactId);
        $emailInput.css("borderColor", "");

        var $emailElement = jq("#contactItem_" + contactId).find(".primaryEmail");
        if ($emailElement.length != 0) {
            $emailElement.remove();
            if (primaryEmail != null) {
                $emailInput.val(primaryEmail.data);
            }
        } else {
            $emailInput.val("");
        }

        $emailInput.show().focus();

        $emailInput.unbind("blur").bind("blur", function() {
            var text = jq.trim($emailInput.val());

            if (text.length == 0) {
                if (primaryEmail == null) {
                    $emailInput.unbind("blur");
                    $emailInput.val("").hide();
                    return false;
                } else {
                    $emailInput.unbind("blur");
                    _deletePrimaryEmail(contactId, text, primaryEmail);
                }
            } else {
                if (jq.isValidEmail(text)) {
                    $emailInput.unbind("blur");
                    _addPrimaryEmail(contactId, text, primaryEmail);
                } else {
                    $emailInput.css("borderColor", "#CC0000");
                    return false;
                }
            }
        });

        var keyupfunc = function (event) {
            if (ASC.CRM.ListContactView.isEnterKeyPressed(event)) {
                var text = jq.trim($emailInput.val());
                if (text.length == 0) {
                    if (primaryEmail == null) {
                        $emailInput.val("").hide();
                        return false;
                    } else {
                        $emailInput.unbind("blur");
                        $emailInput.unbind("keyup");
                        _deletePrimaryEmail(contactId, text, primaryEmail);
                    }
                } else {
                    if (jq.isValidEmail(text)) {
                        $emailInput.unbind("blur");
                        $emailInput.unbind("keyup");
                        _addPrimaryEmail(contactId, text, primaryEmail);
                    } else {
                        $emailInput.css("borderColor", "#CC0000");
                        return false;
                    }
                }
            }
        }
        $emailInput.unbind("keyup").bind("keyup", keyupfunc);
    };

    var _addPrimaryEmail = function(contactId, email, oldPrimaryEmail) {
        if (oldPrimaryEmail == null) {
            var params = { contactId: contactId },
                data = {
                    data: email,
                    isPrimary: true,
                    infoType: "Email",
                    category: "Work"
                };

            Teamlab.addCrmContactInfo(params, contactId, data,
            {
                success: ASC.CRM.ListContactView.CallbackMethods.add_primary_email,
                before: function(params) { jq("#check_contact_" + params.contactId).hide(); jq("#loaderImg_" + params.contactId).show(); },
                after: function(params) { jq("#check_contact_" + params.contactId).show(); jq("#loaderImg_" + params.contactId).hide(); }
            });
        } else {
            var params = { contactId: contactId },
                data = {
                    id: oldPrimaryEmail.id,
                    data: email,
                    isPrimary: true,
                    infoType: "Email"
                };

            Teamlab.updateCrmContactInfo(params, contactId, data,
            {
                success: ASC.CRM.ListContactView.CallbackMethods.add_primary_email,
                before: function(params) { jq("#check_contact_" + params.contactId).hide(); jq("#loaderImg_" + params.contactId).show(); },
                after: function(params) { jq("#check_contact_" + params.contactId).show(); jq("#loaderImg_" + params.contactId).hide(); }
            });
        }
    };

    var _deletePrimaryEmail = function(contactId, email, oldPrimaryEmail) {
        var params = { contactId: contactId };
        Teamlab.deleteCrmContactInfo(params, contactId, oldPrimaryEmail.id,
            {
                success: ASC.CRM.ListContactView.CallbackMethods.delete_primary_email,
                before: function(params) {
                    jq("#check_contact_" + params.contactId).hide();
                    jq("#loaderImg_" + params.contactId).show();
                },
                after: function(params) {
                    jq("#check_contact_" + params.contactId).show();
                    jq("#loaderImg_" + params.contactId).hide();
                }
            });
    };

    var _getFilterSettings = function() {
        var settings = {
            sortBy: "displayName",
            sortOrder: "ascending",
            tags: []
        };

        if (ASC.CRM.ListContactView.advansedFilter.advansedFilter == null) return settings;

        var param = ASC.CRM.ListContactView.advansedFilter.advansedFilter();

        jq(param).each(function(i, item) {
            switch (item.id) {
                case "sorter":
                    settings.sortBy = item.params.id;
                    settings.sortOrder = item.params.dsc == true ? "descending" : "ascending";
                    break;
                case "text":
                    settings.filterValue = item.params.value;
                    break;
                case "fromToDate":
                    settings.fromDate = new Date(item.params.from);
                    settings.toDate = new Date(item.params.to);
                    break;
                default:
                    if (item.hasOwnProperty("apiparamname") && item.params.hasOwnProperty("value") && item.params.value != null) {
                        try {
                            var apiparamnames = jq.parseJSON(item.apiparamname),
                                apiparamvalues = jq.parseJSON(item.params.value);
                            if (apiparamnames.length != apiparamvalues.length) {
                                settings[item.apiparamname] = item.params.value;
                            }
                            for (var i = 0, len = apiparamnames.length; i < len; i++) {
                                settings[apiparamnames[i]] = apiparamvalues[i];
                            }
                        } catch (err) {
                            settings[item.apiparamname] = item.params.value;
                        }
                    }
                    break;
            }
        });
        if (!settings.hasOwnProperty("contactStage")) {
            settings.contactStage = "-1";
        }
        if (!settings.hasOwnProperty("contactType")) {
            settings.contactType = "-1";
        }
        return settings;
    };

    var _contactItemFactory = function(contact, selectedIDs) {
        var index = jq.inArray(contact.id, selectedIDs);
        contact.isChecked = index != -1;

        contact.primaryPhone = null;
        contact.primaryEmail = null;
        //contact.nearTask = null;

        for (var j = 0, n = contact.commonData.length; j < n; j++) {
            if (contact.commonData[j].isPrimary) {
                if (contact.commonData[j].infoType == 0) {
                    contact.primaryPhone = {
                        data: contact.commonData[j].data,
                        id: contact.commonData[j].id
                    };
                }
                if (contact.commonData[j].infoType == 1) {
                    contact.primaryEmail = {
                        data: contact.commonData[j].data,
                        id: contact.commonData[j].id,
                        emailHref: _getEmailHref(contact.id)
                    };
                }
            }
        }
    };

    var _getEmailHref = function (contactID) {
        if (typeof (ASC.CRM.ListContactView.basePathMail) == "undefined"){
            ASC.CRM.ListContactView.basePathMail = ASC.CRM.Common.getMailModuleBasePath();
        } 
        return [
                ASC.CRM.ListContactView.basePathMail,
                "#composeto/crm=",
                contactID
                ].join('');
    };

    var _renderTagElement = function(tag) {
        var $tagElem = jq("<a></a>").addClass("dropdown-item")
                        .text(ASC.CRM.Common.convertText(tag.title,false))
                        .bind("click", function() {
                            _addThisTag(this);
                        });
        jq("#addTagDialog ul.dropdown-content").append(jq("<li></li>").append($tagElem));
    };

    var _renderAndInitTagsDialog = function() {
        for (var i = 0, n = ASC.CRM.Data.contactTags.length; i < n; i++) {
            _renderTagElement(ASC.CRM.Data.contactTags[i]);
        }

        jq.dropdownToggle({
            dropdownID: "addTagDialog",
            switcherSelector: "#contactsHeaderMenu .menuActionAddTag.unlockAction",
            addTop: 5,
            addLeft: 0,
            showFunction: function(switcherObj, dropdownItem) {
                jq("#addTagDialog input.textEdit").val("");
            }
        });
    };

    var _initConfirmationPannels = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "deletePanel",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: ASC.CRM.Resources.CRMCommonResource.ConfirmationDeleteText,
            innerHtmlText:
            ["<div id=\"deleteList\" class=\"containerForListBatchDelete mobile-overflow\">",
                "<dl>",
                    "<dt class=\"listForBatchDelete confirmRemoveCompanies\">",
                        ASC.CRM.Resources.CRMContactResource.Companies,
                        ":",
                    "</dt>",
                    "<dd class=\"listForBatchDelete confirmRemoveCompanies\">",
                    "</dd>",
                    "<dt class=\"listForBatchDelete confirmRemovePersons\">",
                        ASC.CRM.Resources.CRMContactResource.Persons,
                        ":",
                    "</dt>",
                    "<dd class=\"listForBatchDelete confirmRemovePersons\">",
                    "</dd>",
                "</dl>",
            "</div>"].join(''),
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ASC.CRM.Resources.CRMContactResource.DeletingContacts
        }).insertAfter("#mainContactList");

        jq("#deletePanel").on("click", ".middle-button-container .button.blue", function () {
            ASC.CRM.ListContactView.deleteBatchContacts();
        });


        jq.tmpl("template-blockUIPanel", {
            id: "setPermissionsPanel",
            headerTest: ASC.CRM.Resources.CRMCommonResource.SetPermissions,
            innerHtmlText: "",
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            OKBtnClass: "setPermissionsLink",
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ASC.CRM.Resources.CRMCommonResource.SaveChangesProggress
        }).insertAfter("#mainContactList");

        jq("#permissionsContactsPanelInnerHtml").insertBefore("#setPermissionsPanel .containerBodyBlock .middle-button-container").removeClass("display-none");
    };

    var _initSendEmailDialogs = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "createLinkPanel",
            headerTest: ASC.CRM.Resources.CRMContactResource.GenerateLinks,
            questionText: "",
            innerHtmlText: [
                "<div class=\"headerPanel-splitter bold clearFix\">",
                    "<input type=\"checkbox\" id=\"cbxBlind\" style=\"float:left\" />",
                    "<label for=\"cbxBlind\" style=\"float:left;padding: 3px 0 0 4px;\">",
                        ASC.CRM.Resources.CRMContactResource.BlindLinkInfoText,
                    "</label>",
                "</div>",
                "<div class=\"describe-text headerPanel-splitter\">",
                    ASC.CRM.Resources.CRMContactResource.BatchSizeInfoText,
                "</div>",
                "<div class=\"headerPanel-splitter\">",
                    "<b style=\"padding-right:5px;\">",
                        ASC.CRM.Resources.CRMContactResource.BatchSize,
                    "</b>",
                    "<input maxlength=\"10\" class=\"textEdit\" id=\"tbxBatchSize\" style=\"width:100px;\" />",
                "</div>",
                "<div id=\"linkList\" style=\"display:none;\"></div>"
                ].join(''),
            OKBtn: ASC.CRM.Resources.CRMContactResource.Generate,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ASC.CRM.Resources.CRMContactResource.Generation
        }).insertAfter("#mainContactList");

    };

    var _addThisTag = function(obj) {
        var params = {
            tagName: jq(obj).text(),
            isNewTag: false
        };
        _addTag(params);
    };

    var _addTag = function(params) {
        var selectedIDs = new Array();
        for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
            if (ASC.CRM.ListContactView.selectedItems[i].canEdit)
                selectedIDs.push(ASC.CRM.ListContactView.selectedItems[i].id);
        }
        params.contactIDs = selectedIDs;

        Teamlab.addCrmTag(params, "contact", params.contactIDs, params.tagName,
        {
            success: ASC.CRM.ListContactView.CallbackMethods.add_tag,
            before: function(par) {
                for (var i = 0, n = par.contactIDs.length; i < n; i++) {
                    jq("#check_contact_" + par.contactIDs[i]).hide();
                    jq("#loaderImg_" + par.contactIDs[i]).show();
                }
            },
            after: function(par) {
                for (var i = 0, n = par.contactIDs.length; i < n; i++) {
                    jq("#check_contact_" + par.contactIDs[i]).show();
                    jq("#loaderImg_" + par.contactIDs[i]).hide();
                }
            }
        });
    };

    var _showSetPermissionsPanel = function(params) {
        if (jq("#setPermissionsPanel div.tintMedium").length > 0) {
            jq("#setPermissionsPanel div.tintMedium span.header-base").remove();
            jq("#setPermissionsPanel div.tintMedium").removeClass("tintMedium").css("padding", "0px");
        }
        jq("#isPrivate").prop("checked", false);
        ASC.CRM.PrivatePanel.changeIsPrivateCheckBox();
        jq("#selectedUsers div.selectedUser[id^=selectedUser_]").remove();
        SelectedUsers.IDs = new Array();
        LoadingBanner.hideLoaderBtn("#setPermissionsPanel");
        jq("#setPermissionsPanel .setPermissionsLink").unbind("click").bind("click", function() {
            _setPermissions(params);
        });
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#setPermissionsPanel", 600);
    };

    var _setPermissions = function(params) {
        var selectedUsers = SelectedUsers.IDs;
        selectedUsers.push(SelectedUsers.CurrentUserID);

        var selectedIDs = new Array();
        for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
            selectedIDs.push(ASC.CRM.ListContactView.selectedItems[i].id);
        }

        var data = {
            contactid: selectedIDs,
            isPrivate: jq("#isPrivate").is(":checked"),
            accessList: selectedUsers
        };

        Teamlab.updateCrmContactRights(params, data,
            {
                success: ASC.CRM.ListContactView.CallbackMethods.update_contact_rights,
                before: function () {
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.SaveChangesProggress;
                    LoadingBanner.showLoaderBtn("#setPermissionsPanel");
                },
                after: function () {
                    LoadingBanner.hidewLoaderBtn("#setPermissionsPanel");
                }
            });
    };

    var _showDeletePanel = function() {
        var showCompaniesPanel = false,
            showPersonsPanel = false;
        jq("#deleteList dd.confirmRemoveCompanies, #deleteList dd.confirmRemovePersons").html("");
        for (var i = 0, len = ASC.CRM.ListContactView.selectedItems.length; i < len; i++) {
            var item = ASC.CRM.ListContactView.selectedItems[i];
            if (item.canDelete != true) {
                continue;
            }
            if (item.isCompany) {
                showCompaniesPanel = true;
                var label = jq("<label></label>").attr("title", item.displayName).text(item.displayName);
                jq("#deleteList dd.confirmRemoveCompanies").append(
                            label.prepend(jq("<input>")
                            .attr("type", "checkbox")
                            .prop("checked", true)
                            .attr("id", "company_" + item.id))
                        );
            } else {
                showPersonsPanel = true;
                var label = jq("<label></label>")
                            .attr("title", item.displayName)
                            .text(item.displayName);
                jq("#deleteList dd.confirmRemovePersons").append(
                            label.prepend(jq("<input>")
                            .attr("type", "checkbox")
                            .prop("checked", true)
                            .attr("id", "person_" + item.id))
                        );
            }
        }

        if (showCompaniesPanel) {
            jq("#deleteList dt.confirmRemoveCompanies, #deleteList dd.confirmRemoveCompanies").show();
        } else {
            jq("#deleteList dt.confirmRemoveCompanies, #deleteList dd.confirmRemoveCompanies").hide();
        }
        if (showPersonsPanel) {
            jq("#deleteList dt.confirmRemovePersons, #deleteList dd.confirmRemovePersons").show();
        } else {
            jq("#deleteList dt.confirmRemovePersons, #deleteList dd.confirmRemovePersons").hide();
        }
        LoadingBanner.hideLoaderBtn("#deletePanel");

        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#deletePanel", 500);
    };

    var createShortContact = function(contact) {
        var shortContact = {
            id: contact.id,
            isCompany: contact.isCompany,
            primaryEmail: contact.primaryEmail,
            displayName: contact.displayName,
            smallFotoUrl: contact.smallFotoUrl,
            canEdit: contact.canEdit,
            canDelete: contact.canDelete
        };
        return shortContact;
    };

    var _initSimpleContactActionMenu = function () {
        jq.dropdownToggle({
            dropdownID: "simpleContactActionMenu",
            switcherSelector: "#contactTable .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            beforeShowFunction: function (switcherObj, dropdownItem) {
                var contactId = switcherObj.attr("id").split('_')[1];
                if (!contactId) {
                    return;
                }
                _showSimpleActionMenu(contactId, switcherObj.attr("data-displayName"), switcherObj.attr("data-email"));
            },
            showFunction: function (switcherObj, dropdownItem) {
                jq("#contactTable .entity-menu.active").removeClass("active");
                if (dropdownItem.is(":hidden")) {
                    switcherObj.addClass("active");
                }
            },
            hideFunction: function () {
                jq("#contactTable .entity-menu.active").removeClass("active");
            }
        });
    };

    var _showSimpleActionMenu = function (contactID, displayName, email) {

        jq("#simpleContactActionMenu .unlinkContact").unbind("click").bind("click", function () {
            ASC.CRM.ListContactView.removeMember(contactID);
        });

        if (typeof (ASC.CRM.ListContactView.basePathMail) == "undefined") {
            ASC.CRM.ListContactView.basePathMail = ASC.CRM.Common.getMailModuleBasePath();
        }

        if (email != "") {
            var pathCreateEmail = [
                ASC.CRM.ListContactView.basePathMail,
                "#composeto/crm=",
                contactID
             ].join('');

            var pathSortEmails = [
                ASC.CRM.ListContactView.basePathMail,
                "#inbox/",
                "from=",
                email,
                "/sortorder=descending/"
            ].join('');

            jq("#simpleContactActionMenu .writeEmail").attr("href", pathCreateEmail);
            jq("#simpleContactActionMenu .viewMailingHistory").attr("href", pathSortEmails).removeClass("display-none");
        } else {
            jq("#simpleContactActionMenu .writeEmail").addClass("display-none");
            jq("#simpleContactActionMenu .viewMailingHistory").addClass("display-none");
        }
    };

    var _add_new_task_render = function (task) {
        if (typeof (task.contact) === "object" && task.contact != null) {
            var currentContact = null;
            for (var i = 0, n = ASC.CRM.ListContactView.fullContactList.length; i < n; i++) {
                if (task.contact.id == ASC.CRM.ListContactView.fullContactList[i].id) {
                    currentContact = ASC.CRM.ListContactView.fullContactList[i];
                    break;
                }
            }
            if (currentContact != null) {
                currentContact.nearTask = task;

                for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
                    if (ASC.CRM.ListContactView.selectedItems[i].id == currentContact.id) {
                        currentContact.isChecked = true;
                        break;
                    }
                }

                jq("#contactItem_" + task.contact.id).replaceWith(jq.tmpl("contactTmpl", currentContact));

                ASC.CRM.Common.tooltip("#taskTitle_" + task.id, "tooltip", true);
            }
        }
    };

    var _preInitPage = function (entryCountOnPage) {
        jq("#mainSelectAll").prop("checked", false);//'cause checkboxes save their state between refreshing the page
        ASC.CRM.ListContactView.selectAll(jq("#mainSelectAll"));

        jq('#tableForContactNavigation select')
            .val(entryCountOnPage)
            .change(function () {
                ASC.CRM.ListContactView.changeCountOfRows(this.value);
            })
            .tlCombobox();
    };

    var _initEmptyScreen = function () {
        //init emptyScreen for all list

        var buttonHtml = ["<a class='link dotline plus' href='Default.aspx?action=manage'>",
                    ASC.CRM.Resources.CRMContactResource.CreateFirstCompany,
                    "</a><br/>",
                    "<a class='link dotline plus' href='Default.aspx?action=manage&type=people'>",
                    ASC.CRM.Resources.CRMContactResource.CreateFirstPerson,
                    "</a>"].join('');

        if (jq.browser.mobile !== true){
            buttonHtml += ["<br/><a class='crm-importLink link' href='Default.aspx?action=import'>",
                            ASC.CRM.Resources.CRMContactResource.ImportContacts,
                            "</a>"].join('');
        }

        jq.tmpl("template-emptyScreen",
            {
                ID: "contactsEmptyScreen",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_persons"],
                Header: ASC.CRM.Resources.CRMContactResource.EmptyContactListHeader,
                Describe: jq.format(ASC.CRM.Resources.CRMContactResource.EmptyContactListDescription,
                    //types
                    "<span class='hintTypes baseLinkAction' >", "</span>",
                    //csv
                    "<span class='hintCsv baseLinkAction' >", "</span>"),
                ButtonHTML: buttonHtml
            }).insertAfter("#mainContactList");

        //init emptyScreen for filter
        jq.tmpl("template-emptyScreen",
            {
                ID: "emptyContentForContactsFilter",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_filter"],
                Header: ASC.CRM.Resources.CRMContactResource.EmptyContactListFilterHeader,
                Describe: ASC.CRM.Resources.CRMContactResource.EmptyContactListFilterDescribe,
                ButtonHTML: ["<a class='clearFilterButton link dotline' href='javascript:void(0);' ",
                    "onclick='ASC.CRM.ListContactView.advansedFilter.advansedFilter(null);'>",
                    ASC.CRM.Resources.CRMCommonResource.ClearFilter,
                    "</a>"
                ].join('')
            }).insertAfter("#mainContactList");
    };

    var _initFilter = function () {
        if (!jq("#contactsAdvansedFilter").advansedFilter) return;

        var tmpDate = new Date(),
            today = new Date(tmpDate.getFullYear(), tmpDate.getMonth(), tmpDate.getDate(), 0, 0, 0, 0),
            yesterday = new Date(new Date(today).setDate(tmpDate.getDate() - 1)),
            beginningOfThisMonth = new Date(new Date(today).setDate(1)),

            endOfLastMonth = new Date(new Date(beginningOfThisMonth).setDate(beginningOfThisMonth.getDate() - 1)),
            beginningOfLastMonth = new Date(new Date(endOfLastMonth).setDate(1)),


            todayString = Teamlab.serializeTimestamp(today),
            yesterdayString = Teamlab.serializeTimestamp(yesterday),
            beginningOfThisMonthString = Teamlab.serializeTimestamp(beginningOfThisMonth),
            beginningOfLastMonthString = Teamlab.serializeTimestamp(beginningOfLastMonth),
            endOfLastMonthString = Teamlab.serializeTimestamp(endOfLastMonth);

        ASC.CRM.ListContactView.advansedFilter = jq("#contactsAdvansedFilter")
            .advansedFilter({
                anykey      : false,
                hintDefaultDisable: true,
                maxfilters  : -1,
                colcount    : 2,
                maxlength   : "100",
                store       : true,
                inhash      : true,
                filters     : [
                                {
                                    type        : "person",
                                    id          : "my",
                                    apiparamname: "responsibleid",
                                    title       : ASC.CRM.Resources.CRMCommonResource.My,
                                    filtertitle : ASC.CRM.Resources.CRMContactResource.FilterByContactManager,
                                    group       : ASC.CRM.Resources.CRMContactResource.ByContactManager,
                                    groupby     : "responsible",
                                    enable      : true,
                                    bydefault   : { id: Teamlab.profile.id, value: Teamlab.profile.id }
                                },
                                {
                                    type          : "flag",
                                    id            : "noresponsible",
                                    apiparamname  : "responsibleid",
                                    title         : ASC.CRM.Resources.CRMContactResource.WithoutContactManager,
                                    group         : ASC.CRM.Resources.CRMContactResource.ByContactManager,
                                    groupby       : "responsible",
                                    defaultparams : { value: '00000000-0000-0000-0000-000000000000' }
                                },
                                {
                                    type        : "person",
                                    id          : "responsibleID",
                                    apiparamname: "responsibleid",
                                    title       : ASC.CRM.Resources.CRMCommonResource.Custom,
                                    filtertitle : ASC.CRM.Resources.CRMContactResource.FilterByContactManager,
                                    group       : ASC.CRM.Resources.CRMContactResource.ByContactManager,
                                    groupby     : "responsible",
                                    enable      : true
                                },
                                {
                                    type        : "combobox",
                                    id          : "lastMonth",
                                    apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                    title       : ASC.CRM.Resources.CRMCommonResource.LastMonth,
                                    filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    group       : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    groupby     : "byDate",
                                    options     :
                                            [
                                            { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth, def: true },
                                            { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday },
                                            { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                                            { value: jq.toJSON([beginningOfThisMonthString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth }
                                            ]
                                },
                                {
                                    type        : "combobox",
                                    id          : "yesterday",
                                    apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                    title       : ASC.CRM.Resources.CRMCommonResource.Yesterday,
                                    filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    group       : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    groupby     : "byDate",
                                    options     :
                                            [
                                            { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth },
                                            { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday, def: true },
                                            { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                                            { value: jq.toJSON([beginningOfThisMonthString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth }
                                            ]
                                },
                                {
                                    type        : "combobox",
                                    id          : "today",
                                    apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                    title       : ASC.CRM.Resources.CRMCommonResource.Today,
                                    filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    group       : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    groupby     : "byDate",
                                    options     :
                                            [
                                            { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth },
                                            { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday },
                                            { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today, def: true },
                                            { value: jq.toJSON([beginningOfThisMonthString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth }
                                            ]
                                },
                                {
                                    type        : "combobox",
                                    id          : "thisMonth",
                                    apiparamname: jq.toJSON(["fromDate", "toDate"]),
                                    title       : ASC.CRM.Resources.CRMCommonResource.ThisMonth,
                                    filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    group       : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    groupby     : "byDate",
                                    options     :
                                            [
                                            { value: jq.toJSON([beginningOfLastMonthString, endOfLastMonthString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.LastMonth },
                                            { value: jq.toJSON([yesterdayString, yesterdayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Yesterday },
                                            { value: jq.toJSON([todayString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.Today },
                                            { value: jq.toJSON([beginningOfThisMonthString, todayString]), classname: '', title: ASC.CRM.Resources.CRMCommonResource.ThisMonth, def: true }
                                            ]
                                },
                                {
                                    type        : "daterange",
                                    id          : "fromToDate",
                                    title       : ASC.CRM.Resources.CRMCommonResource.Custom,
                                    filtertitle : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    group       : ASC.CRM.Resources.CRMCommonResource.FilterByCreationDate,
                                    groupby     : "byDate"
                                },
                                {
                                    type         : "combobox",
                                    id           : "restricted",
                                    apiparamname : "isShared",
                                    title        : ASC.CRM.Resources.CRMContactResource.PublicContacts,
                                    filtertitle  : ASC.CRM.Resources.CRMContactResource.FilterByAccessibility,
                                    group        : ASC.CRM.Resources.CRMContactResource.FilterByAccessibility,
                                    groupby      : "accessibility",
                                    options      :
                                            [
                                            { value: true, classname: '', title: ASC.CRM.Resources.CRMContactResource.PublicContacts, def: true },
                                            { value: false, classname: '', title: ASC.CRM.Resources.CRMContactResource.RestrictedContacts }
                                            ]
                                },
                                {
                                    type         : "combobox",
                                    id           : "shared",
                                    apiparamname : "isShared",
                                    title        : ASC.CRM.Resources.CRMContactResource.RestrictedContacts,
                                    filtertitle  : ASC.CRM.Resources.CRMContactResource.FilterByAccessibility,
                                    group        : ASC.CRM.Resources.CRMContactResource.FilterByAccessibility,
                                    groupby      : "accessibility",
                                    options      :
                                            [
                                            { value: true, classname: '', title: ASC.CRM.Resources.CRMContactResource.PublicContacts },
                                            { value: false, classname: '', title: ASC.CRM.Resources.CRMContactResource.RestrictedContacts, def: true }
                                            ]
                                },
                                {
                                    type        : "combobox",
                                    id          : "company",
                                    apiparamname: "contactListView",
                                    title       : ASC.CRM.Resources.CRMEnumResource.ContactListViewType_Company,
                                    filtertitle : ASC.CRM.Resources.CRMCommonResource.Show,
                                    group       : ASC.CRM.Resources.CRMCommonResource.Show,
                                    groupby     : "type",
                                    options     :
                                            [
                                            { value: "company", classname: '', title: ASC.CRM.Resources.CRMEnumResource.ContactListViewType_Company, def: true },
                                            { value: "person", classname: '', title: ASC.CRM.Resources.CRMEnumResource.ContactListViewType_Person },
                                            { value: "withopportunity", classname: '', title: ASC.CRM.Resources.CRMEnumResource.ContactListViewType_WithOpportunity }
                                            ]
                                },
                                {
                                    type        : "combobox",
                                    id          : "person",
                                    apiparamname: "contactListView",
                                    title       : ASC.CRM.Resources.CRMEnumResource.ContactListViewType_Person,
                                    filtertitle : ASC.CRM.Resources.CRMCommonResource.Show,
                                    group       : ASC.CRM.Resources.CRMCommonResource.Show,
                                    groupby     : "type",
                                    options     :
                                            [
                                            { value: "company", classname: '', title: ASC.CRM.Resources.CRMEnumResource.ContactListViewType_Company },
                                            { value: "person", classname: '', title: ASC.CRM.Resources.CRMEnumResource.ContactListViewType_Person, def: true },
                                            { value: "withopportunity", classname: '', title: ASC.CRM.Resources.CRMEnumResource.ContactListViewType_WithOpportunity }
                                            ]
                                },
                                {
                                    type        : "combobox",
                                    id          : "withopportunity",
                                    apiparamname: "contactListView",
                                    title       : ASC.CRM.Resources.CRMEnumResource.ContactListViewType_WithOpportunity,
                                    filtertitle : ASC.CRM.Resources.CRMCommonResource.Show,
                                    group       : ASC.CRM.Resources.CRMCommonResource.Show,
                                    groupby     : "type",
                                    options     :
                                            [
                                            { value: "company", classname: '', title: ASC.CRM.Resources.CRMEnumResource.ContactListViewType_Company },
                                            { value: "person", classname: '', title: ASC.CRM.Resources.CRMEnumResource.ContactListViewType_Person },
                                            { value: "withopportunity", classname: '', title: ASC.CRM.Resources.CRMEnumResource.ContactListViewType_WithOpportunity, def: true }
                                            ]
                                },
                                {
                                    type        : "combobox",
                                    id          : "contactStage",
                                    apiparamname: "contactStage",
                                    title       : ASC.CRM.Resources.CRMContactResource.AfterStage,
                                    group       : ASC.CRM.Resources.CRMCommonResource.Other,
                                    options     : ASC.CRM.Data.contactStages,
                                    defaulttitle: ASC.CRM.Resources.CRMCommonResource.Choose,
                                    enable      : ASC.CRM.Data.contactStages.length > 0
                                },
                                {
                                    type        : "combobox",
                                    id          : "contactType",
                                    apiparamname: "contactType",
                                    title       : ASC.CRM.Resources.CRMContactResource.ContactType,
                                    group       : ASC.CRM.Resources.CRMCommonResource.Other,
                                    options     : ASC.CRM.Data.contactTypes,
                                    defaulttitle: ASC.CRM.Resources.CRMCommonResource.Choose,
                                    enable      : ASC.CRM.Data.contactTypes.length > 0
                                },
                                {
                                    type        : "combobox",
                                    id          : "tags",
                                    apiparamname: "tags",
                                    title       : ASC.CRM.Resources.CRMCommonResource.FilterWithTag,
                                    group       : ASC.CRM.Resources.CRMCommonResource.Other,
                                    options     : ASC.CRM.Data.contactTags,
                                    defaulttitle: ASC.CRM.Resources.CRMCommonResource.Choose,
                                    multiple    : true,
                                    enable      : ASC.CRM.Data.contactTags.length > 0
                                }
                ],
                sorters: [
                            { id: "displayname", title: ASC.CRM.Resources.CRMCommonResource.Title, dsc: false, visible: true },
                            { id: "firstname", title: ASC.CRM.Resources.CRMContactResource.FirstName, dsc: false, visible: true },
                            { id: "lastname", title: ASC.CRM.Resources.CRMContactResource.LastName, dsc: false, visible: true },
                            { id: "contacttype", title: ASC.CRM.Resources.CRMContactResource.AfterStage, dsc: false },
                            { id: "created", title: ASC.CRM.Resources.CRMCommonResource.CreateDate, dsc: true, def: true },
                            { id: "history", title: ASC.CRM.Resources.CRMCommonResource.History, dsc: true, def: false }
                ]
            })
            .bind("setfilter", ASC.CRM.ListContactView.changeFilter)
            .bind("resetfilter", ASC.CRM.ListContactView.changeFilter);
    };

    return {
        CallbackMethods:
        {
            get_contacts_by_filter: function (params, contacts) {
                ASC.CRM.ListContactView.Total = params.__total || 0;
                var startIndex = params.__startIndex || 0;

                if (ASC.CRM.ListContactView.Total === 0 &&
                    typeof (ASC.CRM.ListContactView.advansedFilter) != "undefined" &&
                    ASC.CRM.ListContactView.advansedFilter.advansedFilter().length == 1) {
                    ASC.CRM.ListContactView.noContacts = true;
                    ASC.CRM.ListContactView.noContactsForQuery = true;
                } else {
                    ASC.CRM.ListContactView.noContacts = false;
                    if (ASC.CRM.ListContactView.Total === 0) {
                        ASC.CRM.ListContactView.noContactsForQuery = true;
                    } else {
                        ASC.CRM.ListContactView.noContactsForQuery = false;
                    }
                }

                if (ASC.CRM.ListContactView.noContacts) {
                    _renderNoContactsEmptyScreen();
                    ASC.CRM.ListContactView.isFirstLoad ? ASC.CRM.ListContactView.hideFirstLoader() : LoadingBanner.hideLoading();
                    return false;
                }

                if (ASC.CRM.ListContactView.noContactsForQuery) {
                    _renderNoContactsForQueryEmptyScreen();
                    ASC.CRM.ListContactView.isFirstLoad ? ASC.CRM.ListContactView.hideFirstLoader() : LoadingBanner.hideLoading();
                    return false;
                }

                if (contacts.length == 0) {//it can happen when select page without elements after deleting
                    jq("#contactsEmptyScreen").hide();
                    jq("#emptyContentForContactsFilter").hide();
                    jq("#contactsHeaderMenu").show();
                    jq("#companyListBox").show();
                    jq("#companyTable tbody tr").remove();
                    jq("#tableForContactNavigation").show();
                    jq("#mainSelectAll").attr("disabled", true);

                    ASC.CRM.ListContactView.Total = parseInt(jq("#totalContactsOnPage").text()) || 0;

                    var startIndex = ASC.CRM.ListContactView.entryCountOnPage * (contactPageNavigator.CurrentPageNumber - 1);

                    while (startIndex >= ASC.CRM.ListContactView.Total && startIndex >= ASC.CRM.ListContactView.entryCountOnPage) {
                        startIndex -= ASC.CRM.ListContactView.entryCountOnPage;
                    }

                    var page = (startIndex - startIndex % ASC.CRM.ListContactView.entryCountOnPage) / ASC.CRM.ListContactView.entryCountOnPage + 1;
                    _setCookie(page, ASC.CRM.ListContactView.entryCountOnPage);

                    ASC.CRM.ListContactView.renderContent(startIndex);
                    return false;
                }

                _renderContactPageNavigator(startIndex);
                _renderSimpleContactPageNavigator();

                jq("#totalContactsOnPage").text(ASC.CRM.ListContactView.Total);
                jq("#emptyContentForContactsFilter").hide();
                jq("#contactsHeaderMenu, #companyListBox, #tableForContactNavigation").show();
                jq("#contactsEmptyScreen").hide();
                jq("#companyListBox").show();
                jq("#mainSelectAll").removeAttr("disabled");
                ASC.CRM.Common.showExportButtons();

                var selectedIDs = new Array();
                for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
                    selectedIDs.push(ASC.CRM.ListContactView.selectedItems[i].id);
                }

                for (var i = 0, n = contacts.length; i < n; i++) {
                    _contactItemFactory(contacts[i], selectedIDs);
                    ASC.CRM.ListContactView.fullContactList.push(contacts[i]);
                }
                jq("#companyTable tbody").replaceWith(jq.tmpl("contactListTmpl", { contacts: contacts }));

                ASC.CRM.ListContactView.checkFullSelection();

                ASC.CRM.Common.RegisterContactInfoCard();
                ASC.CRM.Common.tooltip(".nearestTask", "tooltip", true);
                window.scrollTo(0, 0);
                ScrolledGroupMenu.fixContentHeaderWidth(jq('#contactsHeaderMenu'));
                
                ASC.CRM.ListContactView.isFirstLoad ? ASC.CRM.ListContactView.hideFirstLoader() : LoadingBanner.hideLoading();
            },

            add_primary_phone: function(params, data) {
                jq("#addPrimaryPhone_" + params.contactId).hide();

                var classAttr = "primaryPhone" + (ASC.VoipNavigationItem && ASC.VoipNavigationItem.isInit ? " link" : "");

                jq("<span></span>").attr("title", data.data).attr("class", classAttr).text(data.data).appendTo(jq("#addPrimaryPhone_" + params.contactId).parent());
                for (var i = 0, n = ASC.CRM.ListContactView.fullContactList.length; i < n; i++) {
                    if (ASC.CRM.ListContactView.fullContactList[i].id == params.contactId)
                        ASC.CRM.ListContactView.fullContactList[i].primaryPhone = {
                            data: data.data,
                            id: data.id
                        };
                }
            },

            delete_primary_phone: function(params) {
                jq("#addPrimaryPhone_" + params.contactId).val("").hide();
                for (var i = 0, n = ASC.CRM.ListContactView.fullContactList.length; i < n; i++) {
                    if (ASC.CRM.ListContactView.fullContactList[i].id == params.contactId) {
                        ASC.CRM.ListContactView.fullContactList[i].primaryPhone = null;
                    }
                }
            },

            add_primary_email: function(params, data) {
                jq("#addPrimaryEmail_" + params.contactId).hide();
                jq("<a></a>").attr("title", data.data)
                    .attr("class", "primaryEmail linkMedium").text(data.data)
                    .attr("href", "mailto:" + data.data).appendTo(jq("#addPrimaryEmail_" + params.contactId).parent());
                jq("#addPrimaryEmailMenu").hide();
                for (var i = 0, n = ASC.CRM.ListContactView.fullContactList.length; i < n; i++) {
                    if (ASC.CRM.ListContactView.fullContactList[i].id == params.contactId)
                        ASC.CRM.ListContactView.fullContactList[i].primaryEmail = {
                            data: data.data,
                            id: data.id,
                            emailHref: _getEmailHref(params.contactId)
                        };
                }

                if (jq("#contactItem_" + params.contactId).hasClass("selected")) {
                    for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
                        if (ASC.CRM.ListContactView.selectedItems[i].id == params.contactId)
                            ASC.CRM.ListContactView.selectedItems[i].primaryEmail = {
                                data: data.data,
                                id: data.id,
                                emailHref: _getEmailHref(params.contactId)
                            };
                    }
                }
                _checkForLockMainActions();
            },

            delete_primary_email: function(params) {
                jq("#addPrimaryEmail_" + params.contactId).val("").hide();
                for (var i = 0, n = ASC.CRM.ListContactView.fullContactList.length; i < n; i++) {
                    if (ASC.CRM.ListContactView.fullContactList[i].id == params.contactId) {
                        ASC.CRM.ListContactView.fullContactList[i].primaryEmail = null;
                    }
                }
                if (jq("#contactItem_" + params.contactId).hasClass("selected")) {
                    for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
                        if (ASC.CRM.ListContactView.selectedItems[i].id == params.contactId) {
                            ASC.CRM.ListContactView.selectedItems[i].primaryEmail = null;
                        }
                    }
                }
                _checkForLockMainActions();
            },

            delete_batch_contacts: function(params, data) {
                var newFullContactList = new Array();
                for (var i = 0, len_i = ASC.CRM.ListContactView.fullContactList.length; i < len_i; i++) {
                    var isDeleted = false;
                    for (var j = 0, len_j = params.contactIDsForDelete.length; j < len_j; j++)
                        if (params.contactIDsForDelete[j] == ASC.CRM.ListContactView.fullContactList[i].id) {
                            isDeleted = true;
                            break;
                        }
                    if (!isDeleted) {
                        newFullContactList.push(ASC.CRM.ListContactView.fullContactList[i]);
                    }

                }
                ASC.CRM.ListContactView.fullContactList = newFullContactList;

                ASC.CRM.ListContactView.Total -= params.contactIDsForDelete.length;
                jq("#totalContactsOnPage").text(ASC.CRM.ListContactView.Total);

                var selectedIDs = new Array();
                for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
                    selectedIDs.push(ASC.CRM.ListContactView.selectedItems[i].id);
                }

                for (var i = 0, len = params.contactIDsForDelete.length; i < len; i++) {
                    var $objForRemove = jq("#contactItem_" + params.contactIDsForDelete[i]);
                    if ($objForRemove.length != 0) {
                        $objForRemove.remove();
                    }

                    var index = jq.inArray(params.contactIDsForDelete[i], selectedIDs);
                    if (index != -1) {
                        selectedIDs.splice(index, 1);
                        ASC.CRM.ListContactView.selectedItems.splice(index, 1);
                    }
                }
                jq("#mainSelectAll").prop("checked", false);

                _checkForLockMainActions();
                _renderCheckedContactsCount(ASC.CRM.ListContactView.selectedItems.length);

                if (ASC.CRM.ListContactView.Total == 0
                    && (typeof (ASC.CRM.ListContactView.advansedFilter) == "undefined"
                    || ASC.CRM.ListContactView.advansedFilter.advansedFilter().length == 1)) {
                    ASC.CRM.ListContactView.noContacts = true;
                    ASC.CRM.ListContactView.noContactsForQuery = true;
                } else {
                    ASC.CRM.ListContactView.noContacts = false;
                    if (ASC.CRM.ListContactView.Total === 0) {
                        ASC.CRM.ListContactView.noContactsForQuery = true;
                    } else {
                        ASC.CRM.ListContactView.noContactsForQuery = false;
                    }
                }
                PopupKeyUpActionProvider.EnableEsc = true;

                if (ASC.CRM.ListContactView.noContacts) {
                    _renderNoContactsEmptyScreen();
                    jq.unblockUI();
                    return;
                }

                if (ASC.CRM.ListContactView.noContactsForQuery) {
                    _renderNoContactsForQueryEmptyScreen();
                    jq.unblockUI();
                    return;
                }

                if (jq("#companyTable tbody tr").length == 0) {
                    jq.unblockUI();

                    var startIndex = ASC.CRM.ListContactView.entryCountOnPage * (contactPageNavigator.CurrentPageNumber - 1);
                    while (startIndex >= ASC.CRM.ListContactView.Total && startIndex >= ASC.CRM.ListContactView.entryCountOnPage) {
                        startIndex -= ASC.CRM.ListContactView.entryCountOnPage;
                    }
                    ASC.CRM.ListContactView.renderContent(startIndex);
                } else {
                    jq.unblockUI();
                }
            },

            add_tag: function(params, data) {
                jq("#addTagDialog").hide();
                if (params.isNewTag) {
                    var tag = {
                        value: params.tagName,
                        title: params.tagName
                    };
                    ASC.CRM.Data.contactTags.push(tag);
                    _renderTagElement(tag);
                    ASC.CRM.ListContactView.advansedFilter.advansedfilter(
                    {
                        nonetrigger: true,
                        sorters: [],
                        filters: [
                            { id: "tags", type: 'combobox', options: ASC.CRM.Data.contactTags, enable: ASC.CRM.Data.contactTags.length > 0 }
                        ]
                    });
                }
            },

            add_new_task: function(params, task) {
                if (!ASC.CRM.Common.isArray(task)) {
                    ASC.CRM.UpdateCRMCaldavCalendar(task, 0);
                    _add_new_task_render(task);
                } else {
                    for (var i = 0, n = task.length; i < n; i++) {
                        ASC.CRM.UpdateCRMCaldavCalendar(task[i], 0);
                        _add_new_task_render(task[i]);
                    }
                }

                ASC.CRM.Common.RegisterContactInfoCard();
                PopupKeyUpActionProvider.EnableEsc = true;
                jq.unblockUI();
                //taskContactSelector.SelectedContacts = new Array();
            },

            render_simple_content: function(params, contacts) {
                for (var i = 0, n = contacts.length; i < n; i++) {
                    ASC.CRM.Common.contactItemFactory(contacts[i], params);
                }
                jq(window).trigger("getContactsFromApi", [contacts]);
                jq.tmpl("simpleContactTmpl", contacts).prependTo("#contactTable tbody");

                if (typeof (params) != "undefined" && params != null && params.hasOwnProperty("showActionMenu") && params.showActionMenu === true) {
                    jq.tmpl("simpleContactActionMenuTmpl", null).insertAfter("#contactTable");
                    _initSimpleContactActionMenu();
                }

                jq(window).trigger("renderSimpleContactListReady", params, contacts);
                ASC.CRM.Common.RegisterContactInfoCard();
                LoadingBanner.hideLoading();
            },

            removeMember: function(params, contact) {
                jq("#contactItem_" + params.contactID).remove();
            },

            addMember: function(params, contact) {
                ASC.CRM.Common.contactItemFactory(contact, params);
                jq.tmpl("simpleContactTmpl", contact).prependTo("#contactTable tbody");
                ASC.CRM.Common.RegisterContactInfoCard();
            },

            update_contact_rights: function(params, contacts) {
                for (var i = 0, n = contacts.length; i < n; i++) {
                    for (var j = 0, m = ASC.CRM.ListContactView.fullContactList.length; j < m; j++) {
                        if (contacts[i].id == ASC.CRM.ListContactView.fullContactList[j].id) {
                            var contact_id = contacts[i].id;
                            ASC.CRM.ListContactView.fullContactList[j].isPrivate = contacts[i].isPrivate;
                            jq("#contactItem_" + contact_id).replaceWith(jq.tmpl("contactTmpl", ASC.CRM.ListContactView.fullContactList[j]));
                            if (params.isBatch) {
                                jq("#check_contact_" + contact_id).prop("checked", true);
                            } else {
                                ASC.CRM.ListContactView.selectedItems = [];
                            }

                            if (ASC.CRM.ListContactView.fullContactList[j].nearTask && ASC.CRM.ListContactView.fullContactList[j].nearTask != null) {
                                ASC.CRM.Common.tooltip("#taskTitle_" + ASC.CRM.ListContactView.fullContactList[j].nearTask.id, "tooltip", true);
                            }
                            break;
                        }
                    }
                }
                ASC.CRM.Common.RegisterContactInfoCard();
                PopupKeyUpActionProvider.EnableEsc = true;
                jq.unblockUI();
            }
        },

        fullContactList           : [],
        selectedItems             : [],

        entryCountOnPage          : 0,
        defaultCurrentPageNumber  : 0,
        emailQuotas               : 0,

        noContacts         : false,
        noContactsForQuery : false,
        cookieKey: "",
        isFirstLoad: true,

        clear: function () {
            ASC.CRM.ListContactView.fullContactList = [];
            ASC.CRM.ListContactView.selectedItems = [];

            ASC.CRM.ListContactView.entryCountOnPage = 0;
            ASC.CRM.ListContactView.defaultCurrentPageNumber = 0;
            ASC.CRM.ListContactView.emailQuotas = 0;

            ASC.CRM.ListContactView.noContacts = false;
            ASC.CRM.ListContactView.noContactsForQuery = false;
            ASC.CRM.ListContactView.cookieKey = "";
        },

        init: function (parentSelector, filterSelector, pagingSelector) {
            if (jq(parentSelector).length == 0) return;
            ASC.CRM.Common.setDocumentTitle(ASC.CRM.Resources.CRMContactResource.AllContacts);
            ASC.CRM.ListContactView.clear();
            jq(parentSelector).removeClass("display-none");

            jq.tmpl("contactsListFilterTmpl", { IsCRMAdmin: ASC.CRM.Data.IsCRMAdmin }).appendTo(filterSelector);
            jq.tmpl("contactsListBaseTmpl", { IsCRMAdmin: ASC.CRM.Data.IsCRMAdmin }).appendTo(parentSelector);
            jq.tmpl("contactsListPagingTmpl").appendTo(pagingSelector);

            jq('#privatePanelWrapper').appendTo("#permissionsPanelInnerHtml");

            ASC.CRM.ListContactView.emailQuotas = ASC.CRM.Data.mailQuotas;
            ASC.CRM.ListContactView.cookieKey = ASC.CRM.Data.CookieKeyForPagination["contacts"];
            ASC.CRM.ListContactView.needToSendApi = true;
            ASC.CRM.ListContactView.advansedFilter = null;

            var settings = {
                    page: 1,
                    countOnPage: jq("#tableForContactNavigation select:first>option:first").val()
                },
                key = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + location.pathname + location.search,
                currentAnchor = location.hash,
                cookieKey = encodeURIComponent(key.charAt(key.length - 1) === '/' ? key + 'Default.aspx' : key);

            currentAnchor = currentAnchor && typeof currentAnchor === 'string' && currentAnchor.charAt(0) === '#'
                ? currentAnchor.substring(1)
                : currentAnchor;

            var cookieAnchor = jq.cookies.get(cookieKey);
            if (currentAnchor == "" || cookieAnchor == currentAnchor) {
                var tmp = ASC.CRM.Common.getPagingParamsFromCookie(ASC.CRM.ListContactView.cookieKey);
                if (tmp != null) {
                    settings = tmp;
                }
            } else {
                _setCookie(settings.page, settings.countOnPage);
            }

            ASC.CRM.ListContactView.entryCountOnPage = settings.countOnPage;
            ASC.CRM.ListContactView.defaultCurrentPageNumber = settings.page;

            _preInitPage(ASC.CRM.ListContactView.entryCountOnPage);
            ASC.CRM.ListContactView.isFirstLoad = true;
            jq(".containerBodyBlock").children(".loader-page").show();

            _initEmptyScreen();

            _initPageNavigatorControl(ASC.CRM.ListContactView.entryCountOnPage, ASC.CRM.ListContactView.defaultCurrentPageNumber);

            _initContactActionMenu();

            _renderAndInitTagsDialog();

            jq("#menuCreateNewTask").on("click", function () {
                ASC.CRM.ListContactView.showTaskPanel(null, true);
            });

            ASC.CRM.ListContactView.initConfirmationPanelForDelete();

            _initEnableVoipSettingsPanel();

            _initConfirmationPannels();

            if (ASC.CRM.Data.IsCRMAdmin === true) {
                _initSendEmailDialogs();
            }
            _initScrolledGroupMenu();

            jq(document).click(function(event) {
                jq.dropdownToggle().registerAutoHide(event, "#contactsHeaderMenu .menuActionAddTag", "#addTagDialog");
                jq.dropdownToggle().registerAutoHide(event, "#contactsHeaderMenu .menuActionSendEmail", "#sendEmailDialog");

                jq.dropdownToggle().registerAutoHide(event, "#companyTable .with-entity-menu", "#contactActionMenu", function() {
                    jq("#companyTable .entity-menu.active").removeClass("active");
                });
            });

            _initFilter();

            ///*tracking events*/
            ASC.CRM.ListContactView.advansedFilter.one("adv-ready", function () {
                var crmAdvansedFilterContainer = jq("#contactsAdvansedFilter .advansed-filter-list");
                crmAdvansedFilterContainer.find("li[data-id='my'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'me_manager');
                crmAdvansedFilterContainer.find("li[data-id='responsibleID'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'custom_manager');
                crmAdvansedFilterContainer.find("li[data-id='company'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'company');
                crmAdvansedFilterContainer.find("li[data-id='Persons'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'persons');
                crmAdvansedFilterContainer.find("li[data-id='withopportunity'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'with_opportunity');
                crmAdvansedFilterContainer.find("li[data-id='lastMonth'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'last_month');
                crmAdvansedFilterContainer.find("li[data-id='yesterday'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'yesterday');
                crmAdvansedFilterContainer.find("li[data-id='today'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'today');
                crmAdvansedFilterContainer.find("li[data-id='thisMonth'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'this_month');
                crmAdvansedFilterContainer.find("li[data-id='fromToDate'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'from_to_date');
                crmAdvansedFilterContainer.find("li[data-id='contactStage'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'contact_stage');
                crmAdvansedFilterContainer.find("li[data-id='contactType'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'contact_type');
                crmAdvansedFilterContainer.find("li[data-id='tags'] .inner-text").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, 'with_tags');

                jq("#contactsAdvansedFilter .btn-toggle-sorter").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, "sort");
                jq("#contactsAdvansedFilter .advansed-filter-input").trackEvent(ga_Categories.contacts, ga_Actions.filterClick, "search_text", "enter");
            });
            
            ASC.CRM.PartialExport.init(ASC.CRM.ListContactView.advansedFilter, "contact");
        },

        onContextMenu: function(event) {
            event.preventDefault();
            return false;
        },

        isEnterKeyPressed: function(event) {
            //Enter key was pressed
            return event.keyCode == 13;
        },

        filterSortersCorrection: function () {
            var settings = _getFilterSettings();

            if (settings.hasOwnProperty("contactListView") && settings.contactListView === "person") {
                var dsc_created = true,
                    dsc = settings.sortOrder == "descending";

                if (settings.sortBy == "created" ) {
                    dsc_created = dsc;
                }

                if (settings.sortBy == "displayname" ) {
                    settings.sortBy = "created";
                }

                ASC.CRM.ListContactView.advansedFilter.advansedfilter(
                    {
                        nonetrigger: true,
                        filters: [],
                        sorters: [
                                        { id: "displayname", visible: false, selected: false},
                                        { id: "firstname", visible: true, selected: settings.sortBy === "firstname", dsc: settings.sortBy === "firstname" ? dsc : false },
                                        { id: "lastname", visible: true, selected: settings.sortBy === "lastname", dsc: settings.sortBy === "lastname" ? dsc : false },
                                        { id: "contacttype", selected: settings.sortBy === "contacttype", dsc: settings.sortBy === "contacttype" ? dsc : false },
                                        { id: "created", selected: settings.sortBy === "created", dsc: dsc_created },
                                        { id: "history", selected: settings.sortBy === "history", dsc: settings.sortBy === "history" ? dsc : false }
                        ]
                    });
            } else {
                var dsc_created = true,
                    dsc = settings.sortOrder == "descending";

                if (settings.sortBy == "created") {
                    dsc_created = dsc;
                }

                if (settings.sortBy == "firstname" || settings.sortBy == "lastname") {
                    settings.sortBy = "created";
                }
                ASC.CRM.ListContactView.advansedFilter.advansedfilter(
                    {
                        nonetrigger: true,
                        filters: [],
                        sorters: [
                                        { id: "displayname", visible: true, selected: settings.sortBy === "displayname", dsc: settings.sortBy === "displayname" ? dsc : false },
                                        { id: "firstname", visible: false, selected: false },
                                        { id: "lastname", visible: false, selected: false },
                                        { id: "contacttype", selected: settings.sortBy === "contacttype", dsc: settings.sortBy === "contacttype" ? dsc : false },
                                        { id: "created", selected: settings.sortBy === "created", dsc: dsc_created },
                                        { id: "history", selected: settings.sortBy === "history", dsc: settings.sortBy === "history" ? dsc : false }
                        ]
                    });
            }
        },

        changeFilter: function () {
            ASC.CRM.ListContactView.needToSendApi = true;

            var defaultStartIndex = 0;
            if (ASC.CRM.ListContactView.defaultCurrentPageNumber != 0) {
                _setCookie(ASC.CRM.ListContactView.defaultCurrentPageNumber, window.contactPageNavigator.EntryCountOnPage);
                defaultStartIndex = (ASC.CRM.ListContactView.defaultCurrentPageNumber - 1) * window.contactPageNavigator.EntryCountOnPage;
                ASC.CRM.ListContactView.defaultCurrentPageNumber = 0;
            } else {
                _setCookie(0, window.contactPageNavigator.EntryCountOnPage);
            }

            ASC.CRM.ListContactView.advansedFilter.one("adv-ready", function () {
                if (ASC.CRM.ListContactView.needToSendApi === true) {
                    ASC.CRM.ListContactView.needToSendApi = false;
                    ASC.CRM.ListContactView.deselectAll();
                    ASC.CRM.ListContactView.renderContent(defaultStartIndex);
                }
            });

            var param = typeof (arguments[2]) == "object" ? arguments[2] : null;
            if (param != null && param.hasOwnProperty("id") && (param.id == "sorter" || param.id == "text")) {
                ASC.CRM.ListContactView.needToSendApi = false;
                ASC.CRM.ListContactView.deselectAll();
                ASC.CRM.ListContactView.renderContent(defaultStartIndex);
            }
            ASC.CRM.ListContactView.filterSortersCorrection();

        },

        renderContent: function(startIndex) {
            ASC.CRM.ListContactView.fullContactList = new Array();
            if (!ASC.CRM.ListContactView.isFirstLoad) {
                LoadingBanner.displayLoading();
                jq("#contactsFilterContainer").show();

                if (!jq("#emptyContentForContactsFilter").is(":visible")) {
                    jq("#contactsHeaderMenu, #tableForContactNavigation").show();
                }

                jq('#contactsAdvansedFilter').advansedFilter("resize");
            }
            jq("#mainSelectAll").prop("checked", false);

            ASC.CRM.ListContactView.getContacts(startIndex);
        },

        hideFirstLoader: function () {
            ASC.CRM.ListContactView.isFirstLoad = false;
            jq(".containerBodyBlock").children(".loader-page").hide();
            if (!jq("#contactsEmptyScreen").is(":visible")) {
                jq("#contactsFilterContainer").show();

                if (!jq("#emptyContentForContactsFilter").is(":visible")) {
                    jq("#contactsHeaderMenu, #tableForContactNavigation").show();
                }
                
                jq('#contactsAdvansedFilter').advansedFilter("resize");
            }
        },

        addRecordsToContent: function() {
            if (!ASC.CRM.ListContactView.showMore) { return false; }

            jq("#showMoreContactsButtons .crm-showMoreLink").hide();
            jq("#showMoreContactsButtons .loading-link").show();

            var startIndex = jq("#companyTable tbody tr").length;

            ASC.CRM.ListContactView.getContacts(startIndex);
        },

        getContacts: function(startIndex) {
            var filter = _getFilterSettings();

            if (typeof startIndex == 'undefined') {
                filter.StartIndex = 0;
            } else {
                filter.StartIndex = startIndex;
            }
            filter.Count = ASC.CRM.ListContactView.entryCountOnPage;

            trackingGoogleAnalytics(ga_Categories.contacts, 'crm_search_contacts_by_filter');

            Teamlab.getCrmSimpleContacts({}, { filter: filter, success: ASC.CRM.ListContactView.CallbackMethods.get_contacts_by_filter });
        },

        changeCountOfRows: function(newValue) {
            if (isNaN(newValue)) { return; }
            var newCountOfRows = newValue * 1;
            ASC.CRM.ListContactView.entryCountOnPage = newCountOfRows;
            contactPageNavigator.EntryCountOnPage = newCountOfRows;

            _setCookie(1, newCountOfRows);

            ASC.CRM.ListContactView.renderContent(0);
        },

        selectAll: function(obj) {
            var isChecked = jq(obj).is(":checked"),
                selectedIDs = [];

            for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
                selectedIDs.push(ASC.CRM.ListContactView.selectedItems[i].id);
            }

            for (var i = 0, n = ASC.CRM.ListContactView.fullContactList.length; i < n; i++) {
                var contact = ASC.CRM.ListContactView.fullContactList[i],
                    index = jq.inArray(contact.id, selectedIDs);
                if (isChecked && index == -1) {
                    ASC.CRM.ListContactView.selectedItems.push(createShortContact(contact));
                    selectedIDs.push(contact.id);
                    jq("#contactItem_" + contact.id).addClass("selected");
                    jq("#check_contact_" + contact.id).prop("checked", true);
                }
                if (!isChecked && index != -1) {
                    ASC.CRM.ListContactView.selectedItems.splice(index, 1);
                    selectedIDs.splice(index, 1);
                    jq("#contactItem_" + contact.id).removeClass("selected");
                    jq("#check_contact_" + contact.id).prop("checked", false);
                }
            }
            _renderCheckedContactsCount(ASC.CRM.ListContactView.selectedItems.length);
            _checkForLockMainActions();
        },

        selectItem: function(obj) {
            var id = parseInt(jq(obj).attr("id").split("_")[2]),
                selectedIDs = [],
                index = 0,
                selectedContact = null;
            for (var i = 0, n = ASC.CRM.ListContactView.fullContactList.length; i < n; i++) {
                if (id == ASC.CRM.ListContactView.fullContactList[i].id) {
                    selectedContact = createShortContact(ASC.CRM.ListContactView.fullContactList[i]);
                }
            }

            for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
                selectedIDs.push(ASC.CRM.ListContactView.selectedItems[i].id);
            }

            index = jq.inArray(id, selectedIDs);

            if (jq(obj).is(":checked")) {
                jq("#contactItem_" + id).addClass("selected");
                if (index == -1) {
                    ASC.CRM.ListContactView.selectedItems.push(selectedContact);
                }
                ASC.CRM.ListContactView.checkFullSelection();
            } else {
                jq("#mainSelectAll").prop("checked", false);
                jq("#contactItem_" + id).removeClass("selected");
                if (index != -1) {
                    ASC.CRM.ListContactView.selectedItems.splice(index, 1);
                }
            }
            _renderCheckedContactsCount(ASC.CRM.ListContactView.selectedItems.length);
            _checkForLockMainActions();
        },

        deselectAll: function() {
            ASC.CRM.ListContactView.selectedItems = new Array();
            jq("#companyTable input:checkbox").prop("checked", false);
            jq("#mainSelectAll").prop("checked", false);
            jq("#companyTable tr.selected").removeClass("selected");
            _renderCheckedContactsCount(0);
            _lockMainActions();
        },

        checkFullSelection: function() {
            var rowsCount = jq("#companyTable tbody tr").length,
                selectedRowsCount = jq("#companyTable input[id^=check_contact_]:checked").length;
            jq("#mainSelectAll").prop("checked", rowsCount == selectedRowsCount);
        },

        deleteBatchContacts: function() {
            var ids = [];
            jq("#deletePanel input:checked").each(function() {
                ids.push(parseInt(jq(this).attr("id").split("_")[1]));
            });
            var params = { contactIDsForDelete: ids };

            Teamlab.removeCrmContact(params, ids,
            {
                success: ASC.CRM.ListContactView.CallbackMethods.delete_batch_contacts,
                before: function () {
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMContactResource.DeletingContacts;
                    LoadingBanner.showLoaderBtn("#deletePanel");
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn("#deletePanel");
                }
            });
        },

        initConfirmationPanelForDelete: function () {
            jq.tmpl("template-blockUIPanel", {
                id: "confirmationDeleteOneContactPanel",
                headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
                questionText: "",
                innerHtmlText:
                ["<div class=\"confirmationAction\">",
                    "<b></b>",
                "</div>",
                "<div class=\"confirmationNote\">",
                    ASC.CRM.Resources.CRMJSResource.DeleteConfirmNote,
                "</div>"].join(''),
                OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
                CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
                progressText: ASC.CRM.Resources.CRMJSResource.DeleteContactInProgress
            }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");
        },

        showConfirmationPanelForDelete: function(contactName, contactID, isCompany, isListView) {
            if (isCompany == "true" || isCompany == true) {
                jq("#confirmationDeleteOneContactPanel .confirmationAction>b").text(jq.format(ASC.CRM.Resources.CRMJSResource.DeleteCompanyConfirmMessage, Encoder.htmlDecode(contactName)));
            } else {
                jq("#confirmationDeleteOneContactPanel .confirmationAction>b").text(jq.format(ASC.CRM.Resources.CRMJSResource.DeletePersonConfirmMessage, Encoder.htmlDecode(contactName)));
            }
            jq("#confirmationDeleteOneContactPanel .middle-button-container>.button.blue.middle").unbind("click").bind("click", function () {
                ASC.CRM.ListContactView.deleteContact(contactID, isListView);
            });
            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI("#confirmationDeleteOneContactPanel", 500);
        },

        deleteContact: function(contactID, isListView) {
            if (isListView === true) {
                var ids = new Array();
                ids.push(contactID);
                var params = { contactIDsForDelete: ids };

                Teamlab.removeCrmContact(params, ids,
                    {
                        success: ASC.CRM.ListContactView.CallbackMethods.delete_batch_contacts,
                        before: function() { jq("#contactActionMenu").hide(); }
                    });
            } else {
                Teamlab.removeCrmContact({}, contactID,
                    {
                        before: function () {
                            LoadingBanner.strLoading = ASC.CRM.Resources.CRMJSResource.DeleteContactInProgress;
                            LoadingBanner.showLoaderBtn("#confirmationDeleteOneContactPanel");

                            jq("#crm_contactMakerDialog input, #crm_contactMakerDialog select, #crm_contactMakerDialog textarea").attr("disabled", true);
                            LoadingBanner.strLoading = ASC.CRM.Resources.CRMJSResource.DeleteContactInProgress;
                            LoadingBanner.showLoaderBtn("#crm_contactMakerDialog");
                        },
                        success: function () {
                            ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                            location.href = "Default.aspx";
                        }
                    });
            }
        },


        showTaskPanel: function (contact, ShowChangeButton) {

            window.taskContactSelector.ShowChangeButton = ShowChangeButton;
            if (ShowChangeButton == true) {
                jq("#selector_taskContactSelector .crm-removeLink").removeClass("display-none");
            } else {
                jq("#selector_taskContactSelector .crm-removeLink").addClass("display-none");
            }
            ASC.CRM.TaskActionView.showTaskPanel(0, "contact", 0, contact, { success: ASC.CRM.ListContactView.CallbackMethods.add_new_task });
        },

        addNewTag: function() {
            var newTag = jq.trim(jq("#addTagDialog input").val());
            if (newTag == "") { return false; }

            var params = {
                tagName: newTag,
                isNewTag: true
            };
            _addTag(params);
        },

        showSendEmailDialog: function () {
            if (typeof (ASC.CRM.ListContactView.basePathMail) == "undefined") {
                ASC.CRM.ListContactView.basePathMail = ASC.CRM.Common.getMailModuleBasePath();
            }

            var sendMailByTlHref = [
                ASC.CRM.ListContactView.basePathMail,
                "#composeto/crm=",
            ].join('');


            for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
                if (ASC.CRM.ListContactView.selectedItems[i].primaryEmail != null) {
                    sendMailByTlHref += ASC.CRM.ListContactView.selectedItems[i].id + ",";
                }
            }
            sendMailByTlHref = sendMailByTlHref.substring(0, sendMailByTlHref.length - 1);
            jq("#sendEmailDialog .sendMailByTl").attr("href", sendMailByTlHref);

            jq.dropdownToggle().toggle("#contactsHeaderMenu .menuActionSendEmail", "sendEmailDialog", 5, 0);
        },

        showCreateLinkPanel: function() {
            var selectedEmails = [];
            jq("#sendEmailDialog").hide();
            jq("#createLinkPanel #linkList").html("");
            jq("#cbxBlind").prop("checked", false);
            jq("#tbxBatchSize").val("10");
            jq.forceNumber({
                parent: "#createLinkPanel",
                input: "#tbxBatchSize",
                integerOnly: true,
                positiveOnly: true
            });

            for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++) {
                if (ASC.CRM.ListContactView.selectedItems[i].primaryEmail != null) {
                    selectedEmails.push(ASC.CRM.ListContactView.selectedItems[i].primaryEmail.data);
                }
            }

            jq("#createLinkPanel .middle-button-container a:first").unbind("click").bind("click", function() {
                ASC.CRM.ListContactView.createLink(selectedEmails);
            });
            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI("#createLinkPanel", 500);
        },

        createLink: function(emails) {
            jq("#createLinkPanel #linkList").html("");
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMContactResource.Generation;
            LoadingBanner.showLoaderBtn("#createLinkPanel");
            var blindAttr = jq("#cbxBlind").is(":checked") ? "bcc=" : "cc=",
                batchSize = jq.trim(jq("#tbxBatchSize").val()) == "" ? 0 : parseInt(jq.trim(jq("#tbxBatchSize").val())),
                linkList = new Array(),
                link = "",
                counter = 0,
                info = "";

            if (ASC.CRM.ListContactView.selectedItems.length - emails.length > 0) {
                info = jq.format(ASC.CRM.Resources.CRMJSResource.GenerateLinkInfo,
                jq.format(ASC.CRM.Resources.CRMJSResource.RecipientsWithoutEmail, ASC.CRM.ListContactView.selectedItems.length - emails.length));
            } else {
                info = jq.format(ASC.CRM.Resources.CRMJSResource.GenerateLinkInfo, "");
            }

            jq("#createLinkPanel #linkList").append(jq("<div></div>").text(info).addClass("headerPanel-splitter"));

            for (var i = 0, n = emails.length; i < n; i++) {
                link += (counter == 0 ? ("mailto:" + emails[i] + "?" + blindAttr) : (emails[i] + ","));
                counter++;  
                if (counter != batchSize) continue;
                counter = 0;
                linkList.push(link);
                link = "";
            }

            if (link)
                linkList.push(link);

            for (var i = 0, n = linkList.length; i < n; i++) {
                counter = i + 1;
                jq("#linkList")
                .append(jq("<a></a>").text(ASC.CRM.Resources.CRMJSResource.Batch + " " + counter).attr("href", linkList[i]));
                if (i != linkList.length - 1)
                    jq("#linkList").append(jq("<span><span/>").addClass("splitter").text(","));
            }
            LoadingBanner.hideLoaderBtn("#createLinkPanel");
            jq("#linkList").show();
        },

        checkSMTPSettings: function () {
            var settings = ASC.CRM.Data.smtpSettings;

            if (!settings)
                return false;

            if (!settings.Host ||
                !settings.Port ||
                !settings.SenderDisplayName ||
                !settings.SenderEmailAddress)
                return false;

            if (settings.RequiredHostAuthentication && !settings.HostLogin)
                return false;

            return true;
        },

        renderSimpleContent: function (showUnlinkBtn, showActionMenu) {
            if (typeof window.entityData != "undefined" && window.entityData != null && window.entityData.id != 0) {
                LoadingBanner.displayLoading();
                Teamlab.getCrmEntityMembers({
                    showCompanyLink : window.entityData.type != "company",
                    showUnlinkBtn   : showUnlinkBtn,
                    showActionMenu  : showActionMenu
                },
                window.entityData.type, window.entityData.id, { success: ASC.CRM.ListContactView.CallbackMethods.render_simple_content });
            }
        },

        removeMember: function(contactID) {
            Teamlab.removeCrmEntityMember({ contactID: contactID }, window.entityData.type, window.entityData.id, contactID, {
                before: function (params) {
                    if (jq("#trashImg_" + params.contactID).length == 1) {
                        jq("#trashImg_" + params.contactID).hide();
                        jq("#loaderImg_" + params.contactID).show();
                    } else {
                        jq("#simpleContactActionMenu").hide();
                        jq("#contactTable .entity-menu.active").removeClass("active");
                    }
                },
                after: ASC.CRM.ListContactView.CallbackMethods.removeMember
            });
        },

        addMember: function(contactID) {
            var data =
                    {
                        contactid     : contactID,
                        personid      : contactID,
                        caseid        : window.entityData.id,
                        companyid     : window.entityData.id,
                        opportunityid : window.entityData.id

                    };
            Teamlab.addCrmEntityMember({
                showCompanyLink : window.entityData.type != "company",
                showUnlinkBtn   : false,
                showActionMenu  : true
            },
            window.entityData.type, window.entityData.id, contactID, data, { success: ASC.CRM.ListContactView.CallbackMethods.addMember });
        },
        
        showSenderPage: function () {
            var selectedTargets = new Array();

            for (var i = 0, n = ASC.CRM.ListContactView.selectedItems.length; i < n; i++)
                if (ASC.CRM.ListContactView.selectedItems[i].primaryEmail != null) {
                    var target = {};
                    target.primaryEmail = ASC.CRM.ListContactView.selectedItems[i].primaryEmail.data;
                    target.title = ASC.CRM.ListContactView.selectedItems[i].displayName;
                    target.id = ASC.CRM.ListContactView.selectedItems[i].id;
                    selectedTargets.push(target);
                }

            if (selectedTargets.length > ASC.CRM.ListContactView.emailQuotas) {
                toastr.error(jq.format(ASC.CRM.Resources.CRMJSResource.ErrorEmailRecipientsCount, ASC.CRM.ListContactView.emailQuotas));
                return false;
            }

            ASC.CRM.SmtpSender.setItems(selectedTargets);

            jq("#sendEmailDialog").hide();

            if (!ASC.CRM.ListContactView.checkSMTPSettings()) {
                window.location.href = "/Management.aspx?type=10";
                return false;
            } else {
                window.location.href = "Sender.aspx";
            }
        }
    };
})();


ASC.CRM.ContactPhotoUploader = (function() {
    return {
        initPhotoUploader: function(parentDialog, photoImg, data) {
            var a = new AjaxUpload('changeLogo', {
                action: 'ajaxupload.ashx?type=ASC.Web.CRM.Classes.ContactPhotoHandler,ASC.Web.CRM',
                autoSubmit: true,
                data: data,
                onSubmit: function(file, ext) {
                    var tmpDirName = "";
                    if (jq("#uploadPhotoPath").length == 1) {
                        tmpDirName = jq("#uploadPhotoPath").val();
                    }
                    this.setData(jQuery.extend({ tmpDirName: tmpDirName }, data));
                },
                onChange: function(file, extension) {
                    if (jQuery.inArray("." + extension, ASC.Files.Utility.Resource.ExtsImage) == -1) {
                        jq("#divLoadPhotoFromPC .fileUploadDscr").hide();
                        jq("#divLoadPhotoFromPC .fileUploadError").text(ASC.CRM.Resources.CRMJSResource.ErrorMessage_NotImageSupportFormat).show();
                        return false;
                    }
                    jq("#divLoadPhotoFromPC .fileUploadError").hide();
                    jq("#divLoadPhotoFromPC .fileUploadDscr").hide();

                    jq(".under_logo .linkChangePhoto").addClass("disable");
                    LoadingBanner.displayLoading();
                    return true;
                },
                onComplete: function(file, response) {
                    var responseObj = jq.evalJSON(response);
                    if (!responseObj.Success) {
                        jq("#divLoadPhotoFromPC .fileUploadDscr").hide();
                        jq("#divLoadPhotoFromPC .fileUploadError").text(responseObj.Message).show();
                        jq(".under_logo .linkChangePhoto").removeClass("disable");
                        LoadingBanner.hideLoading();
                        return;
                    }
                    jq("#divLoadPhotoFromPC .fileUploadError").hide();
                    jq("#divLoadPhotoFromPC .fileUploadDscr").show();
                    PopupKeyUpActionProvider.CloseDialog();
                    if (jq("#uploadPhotoPath").length == 1) {
                        jq("#uploadPhotoPath").val(responseObj.Data.Path);
                    }

                    var now = new Date();
                    photoImg.attr("src", responseObj.Data.Url + '?' + now.getTime());
                    jq(".under_logo .linkChangePhoto").removeClass("disable");
                    jq(window).trigger("contactPhotoUploadSuccessComplete", responseObj.Data.Url);
                    LoadingBanner.hideLoading();
                },
                parentDialog: parentDialog,
                isInPopup: true,
                name: "changeLogo"
            });
        }
    };
})();


ASC.CRM.ContactFullCardView = (function () {
    var _cookiePath = "/";
    var _cookieToggledBlocksKey = "contactFullCardToggledBlocks";

    var initSliderControl = function (canEdit) {
        if (typeof (window.sliderListItems) != "undefined" && window.sliderListItems != null) {
            var colors = [],
                values = [],
                status = 0;
            values[0] = "";

            for (var i = 0, n = window.sliderListItems.items.length; i < n; i++) {
                colors[i] = window.sliderListItems.items[i].color;
                values[i + 1] = window.sliderListItems.items[i].title;
                if (window.sliderListItems.items[i].id == window.sliderListItems.status) {
                    status = i + 1;
                }
            }

            if (jq('#loyaltySliderDetails').length != 0) {
                jq('#loyaltySliderDetails').sliderWithSections({
                    value: status,
                    values: values,
                    max: window.sliderListItems.positionsCount,
                    colors: colors,
                    marginWidth: 1,
                    sliderOptions: {
                        stop: function(event, ui) {
                            if (ui.value != 0) {
                                changeContactStatus(window.sliderListItems.items[ui.value - 1].id);
                            } else {
                                changeContactStatus(0);
                            }
                        }
                    },
                    disabled: !canEdit
                });
            }
        }
    };

    var initChangeContactStatusConfirmationPanel = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "changeContactStatusConfirmation",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: ASC.CRM.ContactFullCardView.isCompany ?
                ASC.CRM.Resources.CRMContactResource.ConfirmationChangePersonsStatus :
                ASC.CRM.Resources.CRMContactResource.ConfirmationChangeCompanyStatus,
            innerHtmlText:
            ["<div class=\"noAskingCCSAnymore\">",
                "<input type=\"checkbox\" style=\"float: left;\" id=\"noAskCCSAnymore\"/>",
                "<label style=\"float:left; padding: 2px 0 0 4px;\" for=\"noAskCCSAnymore\">",
                    ASC.CRM.Resources.CRMCommonResource.DontAskAnymore,
                "</label>",
                "<span style=\"height: 20px;margin: 0 0 0 4px;\">",
                    "<div class=\"HelpCenterSwitcher\" ",
                        "onclick=\"jq(this).helper({ BlockHelperID: 'changeContactStatusConfirmation_helpInfo', popup: true});\">",
                    "</div>",
                    "<div class=\"popup_helper\" id=\"changeContactStatusConfirmation_helpInfo\">",
                    ASC.CRM.Data.IsCRMAdmin === true
                    ? ASC.CRM.Resources.CRMContactResource.ContactStatusGroupChangeHelpForAdmin.format(
                            '<a href="Settings.aspx?type=contact_stage" target="_blank">',
                            '</a>')
                    : ASC.CRM.Resources.CRMContactResource.ContactStatusGroupChangeHelpForUser,
                    "</div>",
                "</span>",
            "</div>"].join(''),
            OKBtn: ASC.CRM.ContactFullCardView.isCompany ?
                ASC.CRM.Resources.CRMContactResource.OKCompanyStatusGroupChange :
                ASC.CRM.Resources.CRMContactResource.OKPersonStatusGroupChange,
            CancelBtn: ASC.CRM.Resources.CRMContactResource.CancelContactStatusGroupChange,
            progressText: ASC.CRM.Resources.CRMContactResource.LoadingWait
        }).insertAfter("#contactDetailsMenuPanel");

        jq("#changeContactStatusConfirmation").on("click", ".middle-button-container .button.blue", function () {
            if (jq("#noAskCCSAnymore").is(":checked")) {
                Teamlab.updateCRMContactStatusSettings({}, true,
                    function () {
                        ASC.CRM.ContactFullCardView.changeContactStatusGroupAuto = true;
                    });
            }
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.LoadingWait;
            LoadingBanner.showLoaderBtn("#changeContactStatusConfirmation");

            var statusValue = jq("#changeContactStatusConfirmation").attr("data-statusValue");

            if (ASC.CRM.ContactFullCardView.isCompany === true) {
                Teamlab.updateCrmCompanyContactStatus({}, window.sliderListItems.id,
                        { companyid: window.sliderListItems.id, contactStatusid: statusValue },
                        function () {
                            PopupKeyUpActionProvider.EnableEsc = true;
                            jq.unblockUI();
                        });

            } else {
                Teamlab.updateCrmPersonContactStatus({}, window.sliderListItems.id,
                       { personid: window.sliderListItems.id, contactStatusid: statusValue },
                       function () {
                           PopupKeyUpActionProvider.EnableEsc = true;
                           jq.unblockUI();
                       });
            }
        });

        jq("#changeContactStatusConfirmation").on("click", ".middle-button-container .button.gray", function () {
            if (jq("#noAskCCSAnymore").is(":checked")) {
                Teamlab.updateCRMContactStatusSettings({}, false,
                    function () {
                        ASC.CRM.ContactFullCardView.changeContactStatusGroupAuto = false;
                    });
            }
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.LoadingWait;
            LoadingBanner.showLoaderBtn("#changeContactStatusConfirmation");

            var statusValue = jq("#changeContactStatusConfirmation").attr("data-statusValue");

            Teamlab.updateCrmContactContactStatus({}, window.sliderListItems.id,
                    { contactid: window.sliderListItems.id, contactStatusid: statusValue },
                    function () {
                        PopupKeyUpActionProvider.EnableEsc = true;
                        jq.unblockUI();
                    });
        });

        jq("#changeContactStatusConfirmation").on("click", ".cancelButton", function () {
            var statusValue = jq("#changeContactStatusConfirmation").attr("data-statusValue");
            Teamlab.updateCrmContactContactStatus({}, window.sliderListItems.id,
                           { contactid: window.sliderListItems.id, contactStatusid: statusValue }, {});
        });

    };

    var initAddTagToContactGroupConfirmationPanel = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "addTagToContactGroupConfirmation",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: "",
            innerHtmlText:
            ["<div>",
                (ASC.CRM.ContactFullCardView.isCompany ?
                ASC.CRM.Resources.CRMContactResource.ConfirmationAddTagToCompanyGroup :
                ASC.CRM.Resources.CRMContactResource.ConfirmationAddTagToPersonGroup).replace(/\n/g, "<br/>"),
            "</div>",
            "<div class=\"noAskingATCAnymore clearFix\">",
                "<input type=\"checkbox\" style=\"float: left;\" id=\"noAskingATCAnymore\"/>",
                "<label style=\"float:left; padding: 2px 0 0 4px;\" for=\"noAskingATCAnymore\">",
                    ASC.CRM.Resources.CRMCommonResource.DontAskAnymore,
                "</label>",
                "<span style=\"height: 20px;margin: 0 0 0 4px;\">",
                    "<div class=\"HelpCenterSwitcher\" ",
                        "onclick=\"jq(this).helper({ BlockHelperID: 'addTagToContactGroupConfirmation_helpInfo', popup: true});\">",
                    "</div>",
                    "<div class=\"popup_helper\" id=\"addTagToContactGroupConfirmation_helpInfo\">",
                    ASC.CRM.Data.IsCRMAdmin === true
                    ? ASC.CRM.Resources.CRMContactResource.AddTagToContactGroupHelpForAdmin.format(
                            '<a href="Settings.aspx?type=tag" target="_blank">',
                            '</a>')
                    : ASC.CRM.Resources.CRMContactResource.AddTagToContactGroupHelpForUser,
                    "</div>",
                "</span>",
            "</div>"].join(''),
            OKBtn: ASC.CRM.Resources.CRMContactResource.OKAddContactTagToGroup,
            OKBtnClass: "OKAddTagToGroup",
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            OtherBtnHtml: ["<a class=\"button gray middle CancelAddTagToGroup\">",
                 ASC.CRM.ContactFullCardView.isCompany ?
                       ASC.CRM.Resources.CRMContactResource.CancelAddContactTagToGroupForCompany :
                       ASC.CRM.Resources.CRMContactResource.CancelAddContactTagToGroupForPerson,
                "</a>"].join(''),
            progressText: ASC.CRM.Resources.CRMContactResource.LoadingWait
        }).insertAfter("#contactDetailsMenuPanel");

        jq("#addTagToContactGroupConfirmation").on("click", ".OKAddTagToGroup", function () {
            if (jq("#noAskingATCAnymore").is(":checked")) {
                Teamlab.updateCRMContactTagSettings({}, true,
                    function () {
                        ASC.CRM.ContactFullCardView.addTagToContactGroupAuto = true;
                    });
            }
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.LoadingWait;
            LoadingBanner.showLoaderBtn("#addTagToContactGroupConfirmation");

            addTagToContactGroup(ASC.CRM.ContactFullCardView.tagParams, ASC.CRM.ContactFullCardView.tagText);
        });

        jq("#addTagToContactGroupConfirmation").on("click", ".CancelAddTagToGroup", function () {
            if (jq("#noAskingATCAnymore").is(":checked")) {
                Teamlab.updateCRMContactTagSettings({}, false,
                    function () {
                        ASC.CRM.ContactFullCardView.addTagToContactGroupAuto = false;
                    });
            }
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.LoadingWait;
            LoadingBanner.showLoaderBtn("#addTagToContactGroupConfirmation");
            addTagToContactOnly(ASC.CRM.ContactFullCardView.tagParams, ASC.CRM.ContactFullCardView.tagText);
        });
    };

    var initDeleteTagFromContactGroupConfirmationPanel = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "deleteTagFromContactGroupConfirmation",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: "",
            innerHtmlText:
            ["<div>",
                (ASC.CRM.ContactFullCardView.isCompany ?
                ASC.CRM.Resources.CRMContactResource.ConfirmationDeleteTagFromCompanyGroup :
                ASC.CRM.Resources.CRMContactResource.ConfirmationDeleteTagFromPersonGroup).replace(/\n/g, "<br/>"),
            "</div>",
            "<div class=\"noAskingATCAnymore clearFix\">",
                "<input type=\"checkbox\" style=\"float: left;\" id=\"noAskingDTCAnymore\"/>",
                "<label style=\"float:left; padding: 2px 0 0 4px;\" for=\"noAskingATCAnymore\">",
                    ASC.CRM.Resources.CRMCommonResource.DontAskAnymore,
                "</label>",
                "<span style=\"height: 20px;margin: 0 0 0 4px;\">",
                    "<div class=\"HelpCenterSwitcher\" ",
                        "onclick=\"jq(this).helper({ BlockHelperID: 'deleteTagFromContactGroupConfirmation_helpInfo', popup: true});\">",
                    "</div>",
                    "<div class=\"popup_helper\" id=\"deleteTagFromContactGroupConfirmation_helpInfo\">",
                    ASC.CRM.Data.IsCRMAdmin === true
                    ? ASC.CRM.Resources.CRMContactResource.DeleteTagFromContactGroupHelpForAdmin.format(
                            '<a href="Settings.aspx?type=tag" target="_blank">',
                            '</a>')
                    : ASC.CRM.Resources.CRMContactResource.DeleteTagFromContactGroupHelpForUser,
                    "</div>",
                "</span>",
            "</div>"].join(''),
            OKBtn: ASC.CRM.Resources.CRMContactResource.OKDeleteContactTagFromGroup,
            OKBtnClass: "OKDeleteTagFromGroup",
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            OtherBtnHtml: ["<a class=\"button gray middle CancelDeleteTagFromGroup\">",
                 ASC.CRM.ContactFullCardView.isCompany ?
                       ASC.CRM.Resources.CRMContactResource.CancelDeleteContactTagFromGroupForCompany :
                       ASC.CRM.Resources.CRMContactResource.CancelDeleteContactTagFromGroupForPerson,
                "</a>"].join(''),
            progressText: ASC.CRM.Resources.CRMContactResource.LoadingWait
        }).insertAfter("#contactDetailsMenuPanel");

        jq("#deleteTagFromContactGroupConfirmation").on("click", ".OKDeleteTagFromGroup", function () {
            if (jq("#noAskingDTCAnymore").is(":checked")) {
                Teamlab.updateCRMContactTagSettings({}, true,
                    function () {
                        ASC.CRM.ContactFullCardView.addTagToContactGroupAuto = true;
                    });
            }
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.LoadingWait;
            LoadingBanner.showLoaderBtn("#deleteTagFromContactGroupConfirmation");

            deleteTagFromContactGroup(ASC.CRM.ContactFullCardView.tagParams, ASC.CRM.ContactFullCardView.tagText);
        });

        jq("#deleteTagFromContactGroupConfirmation").on("click", ".CancelDeleteTagFromGroup", function () {
            if (jq("#noAskingDTCAnymore").is(":checked")) {
                Teamlab.updateCRMContactTagSettings({}, false,
                    function () {
                        ASC.CRM.ContactFullCardView.addTagToContactGroupAuto = false;
                    });
            }
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.LoadingWait;
            LoadingBanner.showLoaderBtn("#deleteTagFromContactGroupConfirmation");
            deleteTagFromContactOnly(ASC.CRM.ContactFullCardView.tagParams, ASC.CRM.ContactFullCardView.tagText);
        });
    };

    var initWriteMailConfirmationPanel = function (pathCreateEmail) {
        jq.tmpl("template-blockUIPanel", {
            id: "writeMailToHistoryConfirmation",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: '',
            innerHtmlText: [
                "<div>", ASC.CRM.Resources.CRMContactResource.WriteEmailByMailProductDscr, "</div>",

                "<div class=\"noAskingWMHAnymore clearFix\">",
                    "<input type=\"checkbox\" id=\"noAskMailHistoryAnymore\" style=\"float:left\" />",
                    "<label for=\"noAskMailHistoryAnymore\" style=\"float:left;padding: 2px 0 0 4px;\">",
                        ASC.CRM.Resources.CRMCommonResource.DontAskAnymore,
                    "</label>",
                "</div>",
                
                "<div id=\"linkList\" style=\"display:none;\"></div>"
            ].join(''),
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            OKBtnClass: "OKGoToWriteMail",
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel
        }).insertAfter("#contactDetailsMenuPanel");


        jq("#writeMailToHistoryConfirmation").on("click", ".OKGoToWriteMail", function () {
            if (jq("#noAskMailHistoryAnymore").is(":checked")) {
                LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.LoadingWait;
                LoadingBanner.showLoaderBtn("#writeMailToHistoryConfirmation");
                Teamlab.updateCRMContactMailToHistorySettings({}, true,
                    function () {
                        ASC.CRM.ContactFullCardView.writeMailToHistoryAuto = true;
                    });
            }
            window.open(pathCreateEmail, '_blank');
            jq.unblockUI();
        });
    };

    var initMergePanel = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "mergePanel",
            headerTest: ASC.CRM.Resources.CRMContactResource.MergePanelHeaderText,
            questionText: "",
            innerHtmlText:
            ["<div class=\"describe-text\">",
                ASC.CRM.Resources.CRMContactResource.MergePanelDescriptionText,
            "</div>",
            "<ul id=\"listContactsToMerge\">",
            "</ul>"].join(''),
            OKBtn: ASC.CRM.Resources.CRMContactResource.MergePanelButtonStartText,
            OKBtnClass: "OKMergeContacts",
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ASC.CRM.Resources.CRMContactResource.MergePanelProgress
        }).insertAfter("#contactProfile");

        jq("#mergePanel").on("click", ".OKMergeContacts", function () {
            ASC.CRM.ContactFullCardView.mergeContacts();
        });
    };

    var initUploadPhotoPanel = function (contactPhotoFileSizeNote, contactPhotoMedium) {
        jq.tmpl("template-blockUIPanel", {
            id: "divLoadPhotoWindow",
            headerTest: ASC.CRM.Resources.CRMContactResource.ChooseProfilePhoto,
            questionText: "",
            innerHtmlText:
            ["<div id=\"divLoadPhotoFromPC\" style=\"margin-top: -20px;\">",
                "<h4>",
                    ASC.CRM.Resources.CRMContactResource.LoadPhotoFromPC,
                "</h4>",
                "<div class=\"describe-text\" style=\"margin-bottom: 5px;\">",
                    contactPhotoFileSizeNote,
                "</div>",
                "<div>",
                    "<a id=\"changeLogo\" class=\"button middle gray\">",
                        ASC.CRM.Resources.CRMJSResource.Browse, "...",
                    "</a>",
                    "<span class=\"fileUploadDscr\">",
                        ASC.CRM.Resources.CRMJSResource.NoFileSelected,
                    "</span>",
                    "<span class=\"fileUploadError\"></span>",
                    "<br />",
                "</div>",
            "</div>",
            "<div id=\"divLoadPhotoDefault\">",
                "<h4>",
                    ASC.CRM.Resources.CRMContactResource.ChangePhotoToDefault,
                "</h4>",
                "<div id=\"divDefaultImagesHolder\" data-uploadOnly=\"false\">",
                    "<div class=\"ImageHolderOuter\" onclick=\"ASC.CRM.SocialMedia.DeleteContactAvatar();\">",
                        "<img class=\"AvatarImage\" src=",
                        contactPhotoMedium,
                        " alt=\"\" />",
                    "</div>",
                "</div>",
            "</div>",
            "<div id=\"divLoadPhotoFromSocialMedia\">",
                "<h4>",
                    ASC.CRM.Resources.CRMContactResource.LoadPhotoFromSocialMedia,
                "</h4>",
                "<div id=\"divImagesHolder\" data-uploadOnly=\"false\">",
                "</div>",
                "<div style=\"clear: both;\">",
                    "<div id=\"divAjaxImageContainerPhotoLoad\">",
                    "</div>",
                "</div>",
            "</div>"].join(''),
            OKBtn: "",
            CancelBtn: "",
            progressText: ""
        }).insertAfter("#contactProfile");
    };

    var initToggledBlocks = function () {
        jq.registerHeaderToggleClick("#contactProfile .crm-detailsTable", "tr.headerToggleBlock");

        jq("#contactProfile .crm-detailsTable").on("click", ".headerToggle, .openBlockLink, .closeBlockLink", function () {
            var $cur = jq(this).parents("tr.headerToggleBlock:first"),
                toggleid = $cur.attr("data-toggleid"),
                isopen = $cur.hasClass("open"),
                toggleObjStates = jq.cookies.get(_cookieToggledBlocksKey);

            if (toggleObjStates != null) {
                toggleObjStates[toggleid] = isopen;
            } else {
                toggleObjStates = {};

                var $list = jq("#contactProfile .crm-detailsTable tr.headerToggleBlock");
                for (var i = 0, n = $list.length; i < n; i++) {
                    toggleObjStates[jq($list[i]).attr("data-toggleid")] = jq($list[i]).hasClass("open");
                }
            }

            jq.cookies.set(_cookieToggledBlocksKey, toggleObjStates, { path: _cookiePath });
        });

        var toggleObjStates = jq.cookies.get(_cookieToggledBlocksKey);
        if (toggleObjStates != null) {
            var $list = jq("#contactProfile .crm-detailsTable tr.headerToggleBlock");
            for (var i = 0, n = $list.length; i < n; i++) {
                var toggleid = jq($list[i]).attr("data-toggleid");
                if (toggleObjStates.hasOwnProperty(toggleid) && toggleObjStates[toggleid] === true) {
                    jq($list[i]).addClass("open");
                }
            }
        } else {
            jq("#contactHistoryTable .headerToggleBlock").addClass("open");
        }

        jq("#contactProfile .headerToggle").not("#contactProfile .headerToggleBlock.open .headerToggle").each(
               function () {
                   jq(this).parents("tr.headerToggleBlock:first").nextUntil(".headerToggleBlock").hide();
               });

    };

    //+ adding to social media data
    var contactNetworksFactory = function(item) {
        if (item.infoType == 7) { //Address
            var addressObj = jq.parseJSON(Encoder.htmlDecode(item.data));
            item.data = ASC.CRM.Common.getAddressTextForDisplay(addressObj);
            var query = ASC.CRM.Common.getAddressQueryForMap(addressObj);
            item.href = "http://maps.google.com/maps?q=" + query;
        }
        if (item.infoType == 2) { //Website
            if (item.data.indexOf("://") == -1) {
                item.href = "http://" + item.data;
            } else {
                item.href = item.data;
            }
        }
        if (item.infoType == 4) { //Twitter
            if (item.data.indexOf("://") == -1) {
                item.href = "https://twitter.com/#!/" + item.data;
            } else {
                item.href = item.data;
            }
            ASC.CRM.SocialMedia.socialNetworks.push(item);
        }
        if (item.infoType == 5) { //LinkedIn
            if (item.data.indexOf("://") == -1) {
                item.href = "http://" + item.data;
            } else {
                item.href = item.data;
            }
            ASC.CRM.SocialMedia.socialNetworks.push(item);
        }
        if (item.infoType == 6) { //Facebook
            if (item.data.indexOf("://") == -1) {
                item.href = "http://facebook.com/" + item.data;
            }
            ASC.CRM.SocialMedia.socialNetworks.push(item);
        }
        if (item.infoType == 8) { //LiveJournal
            if (item.data.indexOf("://") == -1) {
                item.href = "http://" + item.data + ".livejournal.com/";
            } else {
                item.href = item.data;
            }
        }
        if (item.infoType == 9) { //MySpace
            if (item.data.indexOf("://") == -1) {
                item.href = "http://myspace.com/" + item.data;
            } else {
                item.href = item.data;
            }
        }
        if (item.infoType == 11) { //Blogger
            if (item.data.indexOf("://") == -1) {
                item.href = "http://" + item.data + ".blogspot.com/";
            } else {
                item.href = item.data;
            }
        }
        if (item.infoType == 14) { //ICQ
            item.href = "http://www.icq.com/people/" + item.data + "/";
        }
        if (item.infoType == 17) { //VK
            if (item.data.indexOf("://") == -1) {
                item.href = "https://vk.com/" + item.data;
            } else {
                item.href = item.data;
            }
        }
        return item;
        //      10  GMail, -mailto
        //      12 Yahoo, -mailto
        //      13  MSN, -mailto

    };

    var showChangeContactStatusConfirmationPanel = function (statusValue) {
        LoadingBanner.hideLoaderBtn("#changeContactStatusConfirmation");
        jq("#changeContactStatusConfirmation").attr("data-statusValue", statusValue);
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#changeContactStatusConfirmation", 520);
    };

    var showAddTagToContactGroupConfirmationPanel = function (params, text) {
        LoadingBanner.hideLoaderBtn("#addTagToContactGroupConfirmation");
        ASC.CRM.ContactFullCardView.tagText = text;
        ASC.CRM.ContactFullCardView.tagParams = params;
        jq("#addTagDialog").hide();
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#addTagToContactGroupConfirmation", 570);
    };

    var showDeleteTagFromContactGroupConfirmationPanel = function ($element, text) {
        LoadingBanner.hideLoaderBtn("#deleteTagFromContactGroupConfirmation");
        ASC.CRM.ContactFullCardView.tagText = text;
        ASC.CRM.ContactFullCardView.tagParams = $element;
        jq("#addTagDialog").hide();
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#deleteTagFromContactGroupConfirmation", 570);
    };

    var changeContactStatus = function (statusValue) {
        switch (ASC.CRM.ContactFullCardView.changeContactStatusGroupAuto) {
            case null:
                if (ASC.CRM.ContactFullCardView.additionalContactsCount == 0) {
                   Teamlab.updateCrmContactContactStatus({}, window.sliderListItems.id,
                       { contactid: window.sliderListItems.id, contactStatusid: statusValue },
                       function () {
                           PopupKeyUpActionProvider.EnableEsc = true;
                           jq.unblockUI();
                       });
                } else {
                    showChangeContactStatusConfirmationPanel(statusValue);
                }
                break;
            case false:
                Teamlab.updateCrmContactContactStatus({}, window.sliderListItems.id,
                       { contactid: window.sliderListItems.id, contactStatusid: statusValue },
                       function () {
                           PopupKeyUpActionProvider.EnableEsc = true;
                           jq.unblockUI();
                       });
                break;
            case true:
                if (ASC.CRM.ContactFullCardView.isCompany === true) {
                    Teamlab.updateCrmCompanyContactStatus({}, window.sliderListItems.id,
                        { companyid: window.sliderListItems.id, contactStatusid: statusValue }, 
                        function () {
                            PopupKeyUpActionProvider.EnableEsc = true;
                            jq.unblockUI();
                        });
                } else {
                    Teamlab.updateCrmPersonContactStatus({}, window.sliderListItems.id,
                        { personid: window.sliderListItems.id, contactStatusid: statusValue },
                        function () {
                            PopupKeyUpActionProvider.EnableEsc = true;
                            jq.unblockUI();
                        });
                }
                break;
        }
    };

    var addTagToContactOnly = function (params, text) {
        Teamlab.addCrmTag(params, ASC.CRM.TagView.EntityType, ASC.CRM.TagView.EntityID, text,
                {
                    success: function (params, tag) {
                        ASC.CRM.TagView.callback_add_tag(params, tag);
                        PopupKeyUpActionProvider.EnableEsc = true;
                        jq.unblockUI();
                    },
                    before: function () {
                        jq("#tagContainer .adding_tag_loading").show();
                        jq("#addTagDialog").hide();
                    },
                    after: function () {
                        jq("#tagContainer .adding_tag_loading").hide();
                        LoadingBanner.hideLoaderBtn("#addTagToContactGroupConfirmation");
                    }
                });
    };

    var addTagToContactGroup = function (params, text) {
        Teamlab.addCrmContactTagToGroup(params,
            ASC.CRM.ContactFullCardView.isCompany === true ? "company" : "person",
            ASC.CRM.ContactFullCardView.contactID,
            text,
            {
                success: function (params, tag) {
                    ASC.CRM.TagView.callback_add_tag(params, tag);
                    PopupKeyUpActionProvider.EnableEsc = true;
                    jq.unblockUI();
                },
                before: function () {
                    jq("#tagContainer .adding_tag_loading").show();
                    jq("#addTagDialog").hide();
                },
                after: function () {
                    jq("#tagContainer .adding_tag_loading").hide();
                    LoadingBanner.hideLoaderBtn("#addTagToContactGroupConfirmation");
                }
            });
    };

    var addTagToGroupOrNot = function (params, text) {
        switch (ASC.CRM.ContactFullCardView.addTagToContactGroupAuto) {
            case null:
                if (ASC.CRM.ContactFullCardView.additionalContactsCount == 0) {
                    addTagToContactOnly(params, text);
                } else {
                    showAddTagToContactGroupConfirmationPanel(params, text);
                }
                break;
            case false:
                addTagToContactOnly(params, text);
                break;
            case true:
                addTagToContactGroup(params, text);
                break;
        }
    };

    var deleteTagFromContactOnly = function ($element, text) {
      
        Teamlab.removeCrmTag({ element: $element }, "contact", ASC.CRM.ContactFullCardView.contactID, text,
            {
                success: function (params, tag) {
                    ASC.CRM.TagView.callback_delete_tag(params, tag);
                    PopupKeyUpActionProvider.EnableEsc = true;
                    jq.unblockUI();
                },
            });
    };

    var deleteTagFromContactGroup = function ($element, text) {
        Teamlab.deleteCrmContactTagFromGroup({ element: $element },
            ASC.CRM.ContactFullCardView.isCompany === true ? "company" : "person",
            ASC.CRM.ContactFullCardView.contactID,
            text,
            {
                success: function (params, tag) {
                    ASC.CRM.TagView.callback_delete_tag(params, tag);
                    PopupKeyUpActionProvider.EnableEsc = true;
                    jq.unblockUI();
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn("#deleteTagFromContactGroupConfirmation");
                }
            });
    };

    var deleteTagFromGroupOrNot = function ($element) {
        var text = jQuery.base64.decode($element.children(".tag_title:first").attr("data-value"));

        switch (ASC.CRM.ContactFullCardView.addTagToContactGroupAuto) {
            case null:
                if (ASC.CRM.ContactFullCardView.additionalContactsCount == 0) {
                    deleteTagFromContactOnly($element, text);
                } else {
                    showDeleteTagFromContactGroupConfirmationPanel($element, text);
                }
                break;
            case false:
                deleteTagFromContactOnly($element, text);
                break;
            case true:
                deleteTagFromContactGroup($element, text);
                break;
        }
    };

    var renderContactNetworks = function () {
        if (typeof (window.contactNetworks) != "undefined" && window.contactNetworks.length != 0) {
            var $currentContainer,
                $currentPrimaryContainer,
                isFirst = true;
            for (var i = 0, n = window.contactNetworks.length; i < n; i++) {
                contactNetworksFactory(window.contactNetworks[i]);
                var currentData = window.contactNetworks[i];
                if (currentData.isPrimary) {
                    $currentPrimaryContainer = jq.tmpl("collectionContainerTmpl",
                        { Type: currentData.infoTypeLocalName })
                        .insertBefore("#contactTagsTR").children(".collectionItemsTD");
                    jq.tmpl("collectionTmpl", currentData).appendTo($currentPrimaryContainer);
                    isFirst = true;
                } else {
                    if (isFirst || window.contactNetworks[i - 1].infoType != currentData.infoType) {
                        $currentContainer = jq.tmpl("collectionContainerTmpl",
                            { Type: currentData.infoTypeLocalName })
                            .appendTo("#contactAdditionalTable").children(".collectionItemsTD");
                        jq.tmpl("collectionTmpl", currentData).appendTo($currentContainer);
                        isFirst = false;
                    } else {
                        jq.tmpl("collectionTmpl", currentData).appendTo($currentContainer);
                    }
                }
            }
        }

        if (jq("#contactGeneralList .writeEmail").length == 1) {
                var basePathMail = ASC.CRM.Common.getMailModuleBasePath();

                pathCreateEmail = [
                    basePathMail,
                    "#composeto/crm=",
                    ASC.CRM.ContactFullCardView.contactID
                ].join(''),

                pathSortEmails = [
                    basePathMail,
                    "#inbox/",
                    "from=",
                    jq("#contactGeneralList .writeEmail").attr("data-email"),
                    "/sortorder=descending/"
                ].join('');

                initWriteMailConfirmationPanel(pathCreateEmail);
                jq("#contactGeneralList .writeEmail")
                    .on("click", function () {
                        if (ASC.CRM.ContactFullCardView.writeMailToHistoryAuto == true) {
                            window.open(pathCreateEmail, '_blank');
                            jq.unblockUI();
                        } else {
                            jq("#noAskMailHistoryAnymore").prop("checked", false);
                            StudioBlockUIManager.blockUI("#writeMailToHistoryConfirmation", 500);
                        }
                    });

            jq("#contactGeneralList .viewMailingHistory a.button").attr("href", pathSortEmails);
            jq("#contactGeneralList .viewMailingHistory").removeClass("display-none");
        } else {
            jq("#contactGeneralList .viewMailingHistory").remove();
        }
    };

    var renderCustomFields = function() {

        if (typeof (window.customFieldList) != "undefined" && window.customFieldList.length != 0) {
            var sortedList = [],
                subList = {
                    label: "",
                    labelid: 0,
                    list  : []
                };

            for (var i = 0, n = window.customFieldList.length; i < n; i++) {
                var field = window.customFieldList[i];
                if (jQuery.trim(field.mask) != "") {
                    field.mask = jq.parseJSON(field.mask);
                }

                if (field.fieldType == 4 || i == n - 1) {
                    if (field.fieldType != 4) {
                        subList.list.push(field);
                    }

                    if ((i != 0 || i == n - 1) && subList.label == "") {
                        if (jq("#contactAdditionalTable").length == 0) {
                            jq.tmpl("customFieldListWithoutLabelTmpl", { list: subList.list }).insertBefore("#contactHistoryTable");
                        } else {
                            jq.tmpl("customFieldListWithoutLabelWithGroupTmpl", { list: subList.list }).appendTo("#contactAdditionalTable");
                        }
                    } else {
                        sortedList.push(subList);
                    }

                    subList = {
                        label: field.label,
                        labelid: field.id,
                        list: []
                    };
                } else {
                    subList.list.push(field);
                }
            }

            for (var i = 0, n = sortedList.length; i < n; i++) {
                if (sortedList[i].list.length == 0) {
                    sortedList.splice(i, 1);
                    i--;
                    n--;
                }
            }
            jq.tmpl("customFieldListTmpl", sortedList).insertBefore("#contactHistoryTable");
        }
    };

    var showMergePanelComplete = function() {
        jq("#mergePanel .infoPanel").hide();
        jq("#contactDetailsMenuPanel").hide();
        jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#mergePanel", 400);
    };

    return {
        contactID: 0,
        isCompany: undefined,
        init: function (contactID,
                        isCompany,
                        companyID,
                        changeContactStatusGroupAuto,
                        addTagToContactGroupAuto,
                        writeMailToHistoryAuto,
                        contactPhotoFileSizeNote,
                        additionalContactsCount,
                        canEdit) {

            ASC.CRM.ContactFullCardView.contactID = contactID;
            ASC.CRM.ContactFullCardView.isCompany = isCompany;
            ASC.CRM.ContactFullCardView.companyID = companyID;
            ASC.CRM.ContactFullCardView.changeContactStatusGroupAuto = changeContactStatusGroupAuto;
            ASC.CRM.ContactFullCardView.addTagToContactGroupAuto = addTagToContactGroupAuto;
            ASC.CRM.ContactFullCardView.writeMailToHistoryAuto = writeMailToHistoryAuto;
            ASC.CRM.ContactFullCardView.additionalContactsCount = additionalContactsCount;
            ASC.CRM.ContactFullCardView.tagParams = {};
            ASC.CRM.ContactFullCardView.tagText = "";


            var parts = location.pathname.split('/');
            parts.splice(parts.length - 1, 1);
            _cookiePath = parts.join('/');


            var $avatar = jq("#contactProfile .contact_photo:first");
            ASC.CRM.Common.loadContactFoto($avatar, $avatar, $avatar.attr("data-avatarurl"));

            if (canEdit) {
                initUploadPhotoPanel(contactPhotoFileSizeNote, ASC.CRM.Data.DefaultContactPhoto[isCompany === true ? "CompanyMediumSizePhoto" : "PersonMediumSizePhoto"]);
                ASC.CRM.ContactPhotoUploader.initPhotoUploader(
                    jq("#divLoadPhotoWindow"),
                    jq("#contactProfile .contact_photo"),
                    { contactID: jq.getURLParam("id"), uploadOnly: false });
            }

            for (var i = 0, n = window.contactTags.length; i < n; i++) {
                window.contactTags[i] = Encoder.htmlDecode(window.contactTags[i]);
            }
            for (var i = 0, n = window.contactAvailableTags.length; i < n; i++) {
                window.contactAvailableTags[i] = Encoder.htmlDecode(window.contactAvailableTags[i]);
            }

            jq.tmpl("tagViewTmpl",
                    {
                      tags          : window.contactTags,
                      availableTags : window.contactAvailableTags,
                      readonly      : !canEdit
                    })
                    .appendTo("#contactTagsTR>td:last");

            ASC.CRM.TagView.init("contact", false, {
                addTag: function (params, text) {
                    addTagToGroupOrNot(params, text);
                },
                deleteTag: function ($element) {
                    deleteTagFromGroupOrNot($element);
                }
            });

            if (typeof (window.contactResponsibleIDs) != "undefined" && window.contactResponsibleIDs.length != 0) {
                jq("#contactManagerList").html(ASC.CRM.Common.getAccessListHtml(window.contactResponsibleIDs));
            }
            renderContactNetworks();
            renderCustomFields();

            if (jq("#contactAdditionalTable tbody").children().length <= 1) {
                jq("#contactAdditionalTable").remove();
            }

            ASC.CRM.Common.RegisterContactInfoCard();


            initToggledBlocks();
            initChangeContactStatusConfirmationPanel();
            initAddTagToContactGroupConfirmationPanel();
            initDeleteTagFromContactGroupConfirmationPanel();
            initMergePanel();

            initSliderControl(canEdit);
            if (jq.trim(jq("#contactManagerList").text()).length == 0) {
                jq("#contactManagerList").parents("tr:first").remove();
            }
        },

        showMergePanel: function() {
            if (typeof (window.contactToMergeSelector) == "undefined") {
                LoadingBanner.displayLoading();
                var _selectorType = ASC.CRM.ContactFullCardView.isCompany === true ? ASC.CRM.Data.ContactSelectorTypeEnum.Companies : ASC.CRM.Data.ContactSelectorTypeEnum.Persons,
                    _entityType = 0,
                    _entityID = 0;

                Teamlab.getCrmContactsByPrefix({},
                {
                    filter: {
                        prefix     : jq.trim(jq("#baseInfo_Title").val()),
                        searchType : _selectorType,
                        entityType : _entityType,
                        entityID   : _entityID
                    },
                    success: function (par, contacts) {
                        var _excludedArrayIDs = [];
                        for (var i = 0, n = contacts.length; i < n; i++) {
                            _excludedArrayIDs.push(contacts[i].id);
                            if (contacts[i].id == ASC.CRM.ContactFullCardView.contactID) {
                                contacts.splice(i, 1);
                                i--;
                                n--;
                            }
                        }

                        var contactsCount = contacts.length;

                        jq.tmpl("listContactsToMergeTmpl", { contacts: contacts, count: contactsCount }).appendTo("#listContactsToMerge");
                        window["contactToMergeSelector"] = new ASC.CRM.ContactSelector.ContactSelector("contactToMergeSelector",
                        {
                            SelectorType: _selectorType,
                            EntityType: _entityType,
                            EntityID: _entityID,
                            ShowOnlySelectorContent: false,
                            DescriptionText: (ASC.CRM.ContactFullCardView.isCompany === true ? ASC.CRM.Resources.CRMContactResource.FindCompanyByName : ASC.CRM.Resources.CRMContactResource.FindEmployeeByName),
                            DeleteContactText: "",
                            AddContactText: "",
                            IsInPopup: true,
                            ShowChangeButton: true,
                            ShowAddButton: false,
                            ShowDeleteButton: false,
                            ShowContactImg: true,
                            ShowNewCompanyContent: false,
                            ShowNewContactContent: false,
                            presetSelectedContactsJson: '',
                            ExcludedArrayIDs: _excludedArrayIDs,
                            HTMLParent: "#listContactsToMerge .contactToMergeSelectorContainer"
                        });


                        jq("#listContactsToMerge input[type='radio']:first").prop("checked", true);

                        LoadingBanner.hideLoading();
                        showMergePanelComplete();
                    }
                });
            } else {
                showMergePanelComplete();
            }
        },

        mergeContacts: function() {
            var fromID = ASC.CRM.ContactFullCardView.contactID,
                toID = jq("#listContactsToMerge input[name=contactToMerge]:checked").val() * 1;

            if (toID == 0) {
                if (typeof (window.contactToMergeSelector.SelectedContacts[0]) == "undefined") {
                    jq("#mergePanel .infoPanel > div").text(ASC.CRM.Resources.CRMJSResource.ErrorContactIsNotSelected);
                    jq("#mergePanel .infoPanel").show();
                    return;
                }
                toID = window.contactToMergeSelector.SelectedContacts[0] * 1;
            }

            Teamlab.mergeCrmContacts(
                {
                    isCompany: ASC.CRM.ContactFullCardView.isCompany
                },
                {
                    fromcontactid: fromID,
                    tocontactid: toID
                },
                {
                    before: function () {
                        LoadingBanner.strLoading = ASC.CRM.Resources.CRMContactResource.MergePanelProgress;
                        LoadingBanner.showLoaderBtn("#mergePanel");
                    },
                    success: function (params, contact) {
                        location.href = ["Default.aspx?id=", contact.id, params.isCompany === true ? "" : "&type=people"].join("");
                    },
                    error: function (params, error) {
                        toastr.error(error[0]);
                    },
                    after: function () {
                        LoadingBanner.hideLoaderBtn("#mergePanel");
                    }
                });
        }
    };
})();


ASC.CRM.ContactDetailsView = (function() {
    var _projectList = [];
    var _canCreateProjects = false;
    var _availableTabs = [];

    var _getCurrentTabAnch = function () {
        var anch = ASC.Controls.AnchorController.getAnchor();
        if (anch == null || anch == "" || jq.inArray(anch, _availableTabs) == -1) { anch = "profile"; }
        return anch;
    };

    var _initTabs = function (contactID, contactsTabVisible, currentTabAnch, projectsTabVisible, socialMediaTabVisible) {
        window.ASC.Controls.ClientTabsNavigator.init("ContactTabs", {
            tabs: [
            {
                title: ASC.CRM.Resources.CRMCommonResource.Profile,
                selected: currentTabAnch == "profile",
                anchor: "profile",
                divID: "profileTab",
                onclick: "ASC.CRM.ContactDetailsView.activateCurrentTab('" + contactID + "','profile');"
            },
            {
                title: ASC.CRM.Resources.CRMTaskResource.Tasks,
                selected: currentTabAnch == "tasks",
                anchor: "tasks",
                divID: "tasksTab",
                onclick: "ASC.CRM.ContactDetailsView.activateCurrentTab('" + contactID + "','tasks');"
            },
            {
                title: ASC.CRM.Resources.CRMContactResource.Persons,
                selected: currentTabAnch == "contacts",
                anchor: "contacts",
                divID: "contactsTab",
                visible: contactsTabVisible,
                onclick: "ASC.CRM.ContactDetailsView.activateCurrentTab('" + contactID + "','contacts');"
            },
            {
                title: ASC.CRM.Resources.CRMDealResource.Deals,
                selected: currentTabAnch == "deals",
                anchor: "deals",
                divID: "dealsTab",
                onclick: "ASC.CRM.ContactDetailsView.activateCurrentTab('" + contactID + "','deals');"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.InvoiceModuleName,
                selected: currentTabAnch == "invoices",
                anchor: "invoices",
                divID: "invoicesTab",
                onclick: "ASC.CRM.ContactDetailsView.activateCurrentTab('" + contactID + "','invoices');"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.Documents,
                selected: currentTabAnch == "files",
                anchor: "files",
                divID: "filesTab",
                onclick: "ASC.CRM.ContactDetailsView.activateCurrentTab('" + contactID + "','files');"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.Projects,
                selected: currentTabAnch == "projects",
                anchor: "projects",
                divID: "projectsTab",
                visible: projectsTabVisible,
                onclick: "ASC.CRM.ContactDetailsView.activateCurrentTab('" + contactID + "','projects');"
            },
            {
                title: "Twitter",
                selected: currentTabAnch == "twitter",
                anchor: "twitter",
                divID: "socialMediaTab",
                visible: socialMediaTabVisible,
                onclick: "ASC.CRM.ContactDetailsView.activateCurrentTab('" + contactID + "','twitter');"
            }
            ]
        });
    };

    var initProjectSelector = function(projectList) {
        if (jq.browser.mobile === true) {
            var chooseOption = {
                id: 0,
                title: ASC.CRM.Resources.CRMJSResource.LinkWithProject
            };
            projectList.splice(0, 0, chooseOption);

            jq.tmpl("projectSelectorOptionTmpl", projectList).appendTo("#projectsInContactPanel select");
            jq("#projectsInContactPanel select")
                .change(function(evt) {
                    chooseProject(jq(this).children("option:selected"), this.value);
                });
        } else {
            jq.tmpl("projectSelectorItemTmpl", projectList).appendTo("#projectSelectorContainer>.dropdown-content");

            jq.dropdownToggle({
                dropdownID       : "projectSelectorContainer",
                switcherSelector : "#projectsInContactPanel .selectProject>div",
                addTop           : 2
            });

            if (projectList.length > 0) {
                jq("#projectsInContactPanel .selectProject .menuAction").addClass("unlockAction");
                jq("#projectSelectorContainer").removeClass("display-none");
            }
            jq("#projectSelectorContainer").on("click", ".dropdown-content>li", function() {
                jq("#projectSelectorContainer").hide();
                var id = jq(this).attr("data-id");
                chooseProject(jq(this), id);
            });
        }
    };

    var chooseProject = function(element, id) {
        var data = {
            contactid: ASC.Projects.AllProject.contactID,
            projectid: id
        };
        Teamlab.addProjectForCrmContact({ element: element }, id, data, callback_add_project);
    };

    var removeProjectFromList = function(element) {
        element.remove();
        if (jq.browser.mobile === true) {
            jq("#projectsInContactPanel select").val(0).tlCombobox();
        } else {
            if (jq("#projectSelectorContainer .dropdown-item").length == 0) {
                jq("#projectsInContactPanel .selectProject .menuAction").removeClass("unlockAction");
                jq("#projectSelectorContainer").addClass("display-none");
            }
        }
    };

    var addProjectToList = function(project) {
        if (jq.browser.mobile === true) {
            jq.tmpl("projectSelectorOptionTmpl", project).appendTo("#projectsInContactPanel select");
            //jq("#projectsInContactPanel select").val(0).tlCombobox();
        } else {
            jq.tmpl("projectSelectorItemTmpl", project).appendTo("#projectSelectorContainer>.dropdown-content");
            jq("#projectsInContactPanel .selectProject .menuAction:not(.unlockAction)").addClass("unlockAction");
            jq("#projectSelectorContainer.display-none").hide();
            jq("#projectSelectorContainer.display-none").removeClass("display-none");
        }
    };

    var callback_add_project = function(params, project) {
        ASC.Projects.AllProject.addProjectsToSimpleList(project);
        removeProjectFromList(params.element);
    };

    var callback_remove_project = function(params, project) {
        jq(params.element).remove();
        if (jq("#tableListProjects>tbody>tr").length == 0) {
            jq("#projectsInContactPanel").hide();
            jq("#tableListProjectsContainer:not(.display-none)").addClass("display-none");
            jq("#projectsEmptyScreen.display-none").removeClass("display-none");
        }
        addProjectToList(project);
    };

    var callback_get_projects_data = function(params, allProjects, contactProjects) {
        _projectList = allProjects;
        if (typeof (params[0].__count) != "undefined" && params[0].__count != 0) {
            for (var i = 0; i < allProjects.length; i++) {
                if (allProjects[i].canLinkContact === false) {
                    allProjects.splice(i, 1);
                    i--;
                }
            }
            _projectList = allProjects;

            for (var i = 0, n = contactProjects.length; i < n; i++) {
                var idToExclude = contactProjects[i].id;
                for (var j = 0, m = allProjects.length; j < m; j++) {
                    if (allProjects[j].id == idToExclude) {
                        allProjects.splice(j, 1);
                        break;
                    }
                }
            }
            initProjectSelector(allProjects);
        }

        if (typeof (params[1].__count) != "undefined" && params[1].__count != 0) {
            jq("#projectsEmptyScreen:not(.display-none)").addClass("display-none");
            jq("#projectsEmptyScreenWithoutButton:not(.display-none)").addClass("display-none");

            jq("#tableListProjectsContainer.display-none").removeClass("display-none");
            ASC.Projects.AllProject.renderListProjects(contactProjects);
            jq("#projectsInContactPanel").show();
        } else {
            jq("#tableListProjectsContainer:not(.display-none)").addClass("display-none");
            if (_canCreateProjects === true || _projectList.length != 0) {
                jq("#projectsEmptyScreen.display-none").removeClass("display-none");
            } else {
                jq("#projectsEmptyScreenWithoutButton.display-none").removeClass("display-none");
            }
        }
        LoadingBanner.hideLoading();
    };

    var getProjectsData = function(contactID) {
        LoadingBanner.displayLoading();
        var filter = {
            sortBy    : "title",
            sortOrder : "ascending",
            fields    : "id,title,isPrivate,security,responsible"
        };

        Teamlab.joint()
            .getPrjProjects({}, { filter: filter })
            .getProjectsForCrmContact({}, contactID)
            .start({}, {
                success: callback_get_projects_data
            });
    };

    var activateProjectsTab = function (contactID) {
        jq("#projectsEmptyScreen .emptyScrBttnPnl>a").bind("click", function() {
            jq("#projectsEmptyScreen:not(.display-none)").addClass("display-none");
            jq("#projectsInContactPanel").show();
        });

        ASC.Projects.AllProject.init(true);
        ASC.Projects.AllProject.contactID = contactID;

        jq("#projectsInContactPanel .createNewProject>div").click(function() {
            location.href = [StudioManager.getLocationPathToModule("Projects"), "Projects.aspx?action=add&contactID=", ASC.Projects.AllProject.contactID].join("");
        });
        getProjectsData(ASC.Projects.AllProject.contactID);

        jq("#tableListProjects").on("click", ".trash>img.trash_delete", function(event) {
            var $trashObj = jq(this);
            $trashObj.hide();
            $trashObj.parent().children(".trash_progress").show();
            var $line = $trashObj.parents("tr:first"),
                id = $line.attr("id"),
                data = {
                    contactid: ASC.Projects.AllProject.contactID,
                    projectid: id
                };
            Teamlab.removeProjectFromCrmContact({ element: $line }, id, data, callback_remove_project);
        });
    };

    var initAttachments = function () {
        window.Attachments.init();
        window.Attachments.bind("addFile", function(ev, file) {
            var contactID = parseInt(jq.getURLParam("id"));
            if (!isNaN(contactID)) {

                var type = "contact",
                    fileids = [];
                fileids.push(file.id);

                Teamlab.addCrmEntityFiles({}, contactID, type, {
                    entityid   : contactID,
                    entityType : type,
                    fileids    : fileids
                }, function(params, data) {
                    window.Attachments.appendFilesToLayout(data.files);
                    params.fromAttachmentsControl = true;
                    ASC.CRM.HistoryView.isTabActive = false;
                });
            }
        });

        window.Attachments.bind("deleteFile", function(ev, fileId) {
            var $fileLinkInHistoryView = jq("#fileContent_" + fileId);
            if ($fileLinkInHistoryView.length != 0) {
                var messageID = $fileLinkInHistoryView.parents("[id^=eventAttach_]").attr("id").split("_")[1];
                ASC.CRM.HistoryView.deleteFile(fileId, messageID);
            } else {
                Teamlab.removeCrmEntityFiles({ fileId: fileId }, fileId, {
                    success: function(params) {
                        window.Attachments.deleteFileFromLayout(params.fileId);
                    }
                });
            }
        });
    };

    var initContactDetailsMenuPanel = function() {
        jq(document).ready(function() {
            jq.dropdownToggle({
                dropdownID: "contactDetailsMenuPanel",
                switcherSelector: ".mainContainerClass .containerHeaderBlock .menu-small",
                addTop: 0,
                addLeft: -10,
                showFunction: function(switcherObj, dropdownItem) {
                    if (dropdownItem.is(":hidden")) {
                        switcherObj.addClass('active');
                    } else {
                        switcherObj.removeClass('active');
                    }
                },
                hideFunction: function() {
                    jq(".mainContainerClass .containerHeaderBlock .menu-small.active").removeClass("active");
                }
            });
        });
    };

    var initOtherActionMenu = function (shareType) {

        var params = null;
        if (shareType == 0) {
            params = { taskResponsibleSelectorUserIDs: window.contactResponsibleIDs };
        }

        jq("#menuCreateNewTask").bind("click", function () {
            ASC.CRM.TaskActionView.showTaskPanel(0, "contact", 0, window.contactForInitTaskActionPanel, params);
        });

        ASC.CRM.ListTaskView.bindEmptyScrBtnEvent(params);

        var href = jq("#menuCreateNewDeal").attr("href") + "&contactID=" + jq.getURLParam("id");
        jq("#menuCreateNewDeal").attr("href", href);
        
        href = jq("#menuCreateNewInvoice").attr("href") + "&contactID=" + jq.getURLParam("id");
        jq("#menuCreateNewInvoice").attr("href", href);
    };

    var initEmptyScreens = function (isCompany) {

        if (isCompany === true) {
            jq.tmpl("template-emptyScreen",
                {ID: "emptyPeopleInCompanyPanel",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_company_participants"],
                Header: ASC.CRM.Resources.CRMContactResource.EmptyContentPeopleHeader,
                Describe: ASC.CRM.Resources.CRMContactResource.EmptyContentPeopleDescribe,
                ButtonHTML: ["<a class='link-with-entity link dotline' ",
                            "onclick='javascript:jq(\"#peopleInCompanyPanel\").show();jq(\"#emptyPeopleInCompanyPanel\").addClass(\"display-none\");'>",
                            ASC.CRM.Resources.CRMContactResource.AssignContact,
                            "</a>"
                            ].join(''),
                CssClass: "display-none"
            }).insertAfter("#contactListBox");
        }

        jq.tmpl("template-emptyScreen",
                {
                    ID: "projectsEmptyScreen",
                    ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_projects"],
                    Header: ASC.CRM.Resources.CRMContactResource.EmptyContactProjectListHeader,
                    Describe: ASC.CRM.Resources.CRMContactResource.EmptyContactProjectListDescription,
                    ButtonHTML: ["<a class='link-with-entity link dotline'>",
                                ASC.CRM.Resources.CRMContactResource.AssignProject,
                                "</a>"
                    ].join(''),
                    CssClass: "display-none"
                }).insertAfter("#tableListProjects");

        jq.tmpl("template-emptyScreen",
                {
                    ID: "projectsEmptyScreenWithoutButton",
                    ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_projects"],
                    Header: ASC.CRM.Resources.CRMContactResource.EmptyContactProjectListHeader,
                    Describe: ASC.CRM.Resources.CRMContactResource.EmptyContactProjectListDescriptionWithoutButton,
                    CssClass: "display-none"
                }).insertAfter("#tableListProjects");
        
        jq.tmpl("template-emptyScreen",
            {
                ID: "invoiceEmptyScreen",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_invoices"],
                Header: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoicesHeader,
                Describe: ASC.CRM.Resources.CRMInvoiceResource.EmptyContentInvoicesDescribe,
                ButtonHTML: ["<a class='link dotline plus' href='Invoices.aspx?action=create&contactID=" + jq.getURLParam("id") + "'>",
                    ASC.CRM.Resources.CRMInvoiceResource.CreateFirstInvoice,
                    "</a>"].join(''),
            }).insertAfter("#invoiceTable");
    };

    var initPeopleContactSelector = function() {
        window["contactSelector"] = new ASC.CRM.ContactSelector.ContactSelector("contactSelector",
                    {
                        SelectorType: 2,
                        EntityType: 0,
                        EntityID: 0,
                        ShowOnlySelectorContent: true,
                        DescriptionText: ASC.CRM.Resources.CRMContactResource.FindContactByName,
                        DeleteContactText: "",
                        AddContactText: "",
                        IsInPopup: false,
                        ShowChangeButton: false,
                        ShowAddButton: false,
                        ShowDeleteButton: false,
                        ShowContactImg: false,
                        ShowNewCompanyContent: false,
                        ShowNewContactContent: true,
                        presetSelectedContactsJson: '',
                        ExcludedArrayIDs: [],
                        HTMLParent: "#peopleInCompanyPanel"
                    });
        window.contactSelector.SelectItemEvent = ASC.CRM.ContactDetailsView.addPersonToCompany;
        ASC.CRM.ListContactView.removeMember = ASC.CRM.ContactDetailsView.removePersonFromCompany;

        jq(window).bind("getContactsFromApi", function(event, contacts) {
            var contactLength = contacts.length;

            ASC.CRM.ContactFullCardView.additionalContactsCount = contactLength;

            if (contactLength == 0) {
                jq("#emptyPeopleInCompanyPanel.display-none").removeClass("display-none");
            } else {
                jq("#peopleInCompanyPanel").show();
                var contactIDs = [];
                for (var i = 0; i < contactLength; i++) {
                    contactIDs.push(contacts[i].id);
                }
                window.contactSelector.SelectedContacts = contactIDs;
            }
        });
    };

    return {
        init: function (contactID,
                        isCompany,
                        projectsTabVisible,
                        socialMediaTabVisible,
                        shareType,
                        canEdit) {
            _canCreateProjects = ASC.CRM.Data.CanCreateProjects;

            _availableTabs = ["profile", "tasks", "deals", "invoices", "files"];
            if (isCompany === true) { _availableTabs.push("contacts"); }
            if (projectsTabVisible) { _availableTabs.push("projects"); }
            if (socialMediaTabVisible) { _availableTabs.push("twitter"); }

            var currentTabAnch = _getCurrentTabAnch();
            _initTabs(contactID, isCompany === true, currentTabAnch, projectsTabVisible, socialMediaTabVisible);
            
            ASC.CRM.HistoryView.init(contactID, "contact", 0);
            ASC.CRM.DealTabView.initTab(contactID);
            ASC.CRM.ListTaskView.initTab(contactID, "contact", 0);
            ASC.CRM.ListInvoiceView.initTab(contactID, "contact");
            ASC.CRM.SocialMedia.initTab(isCompany, canEdit);

            ASC.CRM.ListContactView.isContentRendered = false;
            ASC.CRM.ListInvoiceView.isContentRendered = false;
            ASC.CRM.ContactDetailsView.isProjectTabRendered = false;

            initEmptyScreens(isCompany);

            if (isCompany === true) {
                initPeopleContactSelector();
            }

            initAttachments();
            initContactDetailsMenuPanel();
            initOtherActionMenu(shareType);

            ASC.CRM.SocialMedia.init(ASC.CRM.Data.DefaultContactPhoto[isCompany === true ? "CompanyBigSizePhoto" : "PersonBigSizePhoto"]);
            ASC.CRM.ContactDetailsView.checkSocialMediaError();

            ASC.CRM.ContactDetailsView.activateCurrentTab(contactID, currentTabAnch);
        },

        activateCurrentTab : function (contactID, anchor) {
            if (anchor == "profile") { }
            if (anchor == "tasks") {
                ASC.CRM.ListTaskView.activate();
            }
            if (anchor == "contacts") {
                if (ASC.CRM.ListContactView.isContentRendered == false) {
                    ASC.CRM.ListContactView.isContentRendered = true;
                    ASC.CRM.ListContactView.renderSimpleContent(false, true);
                }
            }
            if (anchor == "twitter") {
                var hasTwitter = false
                if (typeof (window.contactNetworks) !== "undefined" && window.contactNetworks.length != 0) {
                    for (var i = 0, n = window.contactNetworks.length; i < n; i++) {
                        if (window.contactNetworks[i].infoType == 4) {
                            hasTwitter = true;
                            break;
                        }
                    }
                }
                ASC.CRM.SocialMedia.activate(hasTwitter);
            }
            if (anchor == "deals") {
                ASC.CRM.DealTabView.activate();
            }
            if (anchor == "invoices") {
                if (ASC.CRM.ListInvoiceView.isContentRendered == false) {
                    ASC.CRM.ListInvoiceView.isContentRendered = true;
                    ASC.CRM.ListInvoiceView.renderSimpleContent();
                }
            }
            if (anchor == "files") {
                window.Attachments.loadFiles();
            }
            if (anchor == "projects") {
                if (ASC.CRM.ContactDetailsView.isProjectTabRendered == false) {
                    ASC.CRM.ContactDetailsView.isProjectTabRendered = true;
                    activateProjectsTab(contactID);
                }
            }
        },

        removePersonFromCompany: function(id) {
            Teamlab.removeCrmEntityMember({ contactID: parseInt(id) }, window.entityData.type, window.entityData.id, id, {
                before: function (params) {
                    jq("#simpleContactActionMenu").hide();
                    jq("#contactTable .entity-menu.active").removeClass("active");
                },
                success: function (params, response) {
                    var index = jq.inArray(params.contactID, window.contactSelector.SelectedContacts);
                    if (index != -1) {
                        window.contactSelector.SelectedContacts.splice(index, 1);
                    } else {
                        console.log("Can't find such contact in list");
                    }
                    ASC.CRM.ContactSelector.Cache = {};
                    ASC.CRM.ContactFullCardView.additionalContactsCount = window.contactSelector.SelectedContacts.length;
                    jq("#contactItem_" + params.contactID).animate({ opacity: "hide" }, 500);

                    //ASC.CRM.Common.changeCountInTab("delete", "contacts");

                    setTimeout(function () {
                        jq("#contactItem_" + params.contactID).remove();
                        if (window.contactSelector.SelectedContacts.length == 0) {
                            jq("#peopleInCompanyPanel").hide();
                            jq("#emptyPeopleInCompanyPanel.display-none").removeClass("display-none");
                        }
                    }, 500);

                    ASC.CRM.HistoryView.isTabActive = false;
                }
            });
        },

        addPersonToCompany: function(obj) {
            if (jq("#contactItem_" + obj.id).length > 0) {
                return false;
            }
            var data =
                {
                    personid  : obj.id,
                    companyid : window.entityData.id
                };
            Teamlab.addCrmEntityMember({
                                            showCompanyLink : window.entityData.type != "company",
                                            showUnlinkBtn   : false,
                                            showActionMenu  : true
                                        },
                                        window.entityData.type, window.entityData.id, obj.id, data, {
                success: function(params, contact) {
                    ASC.CRM.ListContactView.CallbackMethods.addMember(params, contact);
                    //ASC.CRM.ContactSelector.Cache = {};

                    window.contactSelector.SelectedContacts.push(contact.id);
                    ASC.CRM.ContactFullCardView.additionalContactsCount = window.contactSelector.SelectedContacts.length;
                    jq("#emptyPeopleInCompanyPanel:not(.display-none)").addClass("display-none");

                    ASC.CRM.HistoryView.isTabActive = false;
                }
            });
        },

        checkSocialMediaError: function () {
            var smErrorMessage = jq("input[id$='_ctrlSMErrorMessage']").val();
            if (smErrorMessage != "" && smErrorMessage !== undefined) {
                ASC.CRM.SocialMedia.ShowErrorMessage(smErrorMessage);
            }
        }
    };
})();


ASC.CRM.ContactActionView = (function () {
    var isInit = false,
        confirmation = false,
        saveButtonId = "",
        cache = {};
    this.ContactData = null;

    var renderContactNetworks = function () {

        jq("#generalListEdit").on('click', ".not_primary_field", function() {
            ASC.CRM.ContactActionView.choosePrimaryElement(jq(this), jq(this).parent().parent().parent().attr("id") == "addressContainer");
        });

        if (typeof (window.contactNetworks) != "undefined" && window.contactNetworks.length != 0) {
            for (var i = 0, n = window.contactNetworks.length; i < n; i++) {
                var networkItem = window.contactNetworks[i];
                if (networkItem.hasOwnProperty("infoType") && networkItem.hasOwnProperty("data")) {
                    if (networkItem.infoType == 7) { //Address
                        var address = jq.parseJSON(Encoder.htmlDecode(networkItem.data));

                        var $addressJQ = createNewAddress(jq('#addressContainer').children('div:first').clone(), networkItem.isPrimary, networkItem.category, address.street, address.city, address.state, address.zip, address.country);
                        $addressJQ.insertAfter(jq('#addressContainer').children('div:last')).show();
                        continue;
                    }

                    var container_id,
                        $newContact;

                    if (networkItem.infoType == 0) { //Phone
                        container_id = 'phoneContainer';

                        $newContact = ASC.CRM.ContactActionView.createNewCommunication(container_id, Encoder.htmlDecode(networkItem.data), networkItem.isPrimary);

                        changePhoneCategory(
                                        $newContact.children('table').find('a'),
                                        jq("#phoneCategoriesPanel ul.dropdown-content li").children('a[category=' + networkItem.category + ']').text(),
                                        networkItem.category);
                    } else if (networkItem.infoType == 1) { //Email
                        container_id = 'emailContainer';

                        $newContact = ASC.CRM.ContactActionView.createNewCommunication(container_id, Encoder.htmlDecode(networkItem.data), networkItem.isPrimary);

                        changeBaseCategory(
                                        $newContact.children('table').find('a'),
                                        jq("#baseCategoriesPanel ul.dropdown-content li").children('a[category=' + networkItem.category + ']').text(),
                                        networkItem.category);
                    } else {
                        container_id = 'websiteAndSocialProfilesContainer';

                        $newContact = ASC.CRM.ContactActionView.createNewCommunication(container_id, Encoder.htmlDecode(networkItem.data));

                        changeBaseCategory(
                                        $newContact.find('a.social_profile_category'),
                                        jq("#baseCategoriesPanel ul.dropdown-content li").children('a[category=' + networkItem.category + ']').text(),
                                        networkItem.category);

                        ASC.CRM.ContactActionView.changeSocialProfileCategory(
                                        $newContact.find('a.social_profile_type'),
                                        networkItem.infoType,
                                        jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category=' + networkItem.infoType + ']').text(),
                                        jq("#socialProfileCategoriesPanel ul.dropdown-content li").children('a[category=' + networkItem.infoType + ']').attr('categoryName'));
                    }


                    $newContact.insertAfter(jq('#' + container_id).children('div:last')).show();
                    continue;
                }
            }

            var add_new_button_class = "crm-addNewLink";
            if (jq('#emailContainer').children('div').length > 1) {
                jq('#emailContainer').prev('dt').removeClass('crm-headerHiddenToggledBlock');
            }
            jq('#emailContainer').children('div:not(:first)').find("." + add_new_button_class).hide();
            jq('#emailContainer').children('div:last').find("." + add_new_button_class).show();

            if (jq('#phoneContainer').children('div').length > 1) {
                jq('#phoneContainer').prev('dt').removeClass('crm-headerHiddenToggledBlock');
            }
            jq('#phoneContainer').children('div:not(:first)').find("." + add_new_button_class).hide();
            jq('#phoneContainer').children('div:last').find("." + add_new_button_class).show();

            if (jq('#websiteAndSocialProfilesContainer').children('div').length > 1) {
                jq('#websiteAndSocialProfilesContainer').prev('dt').removeClass('crm-headerHiddenToggledBlock');
            }
            jq('#websiteAndSocialProfilesContainer').children('div:not(:first)').find("." + add_new_button_class).hide();
            jq('#websiteAndSocialProfilesContainer').children('div:last').find("." + add_new_button_class).show();

            if (jq('#addressContainer').children('div').length > 1) {
                jq('#addressContainer').prev('dt').removeClass('crm-headerHiddenToggledBlock');
            }
            jq('#addressContainer').children('div:not(:first)').find("." + add_new_button_class).hide();
            jq('#addressContainer').children('div:last').find("." + add_new_button_class).show();
        }
    };

    var renderContactTags = function () {

        for (var i = 0, n = window.contactActionTags.length; i < n; i++) {
            window.contactActionTags[i] = Encoder.htmlDecode(window.contactActionTags[i]);
        }
        for (var i = 0, n = window.contactActionAvailableTags.length; i < n; i++) {
            window.contactActionAvailableTags[i] = Encoder.htmlDecode(window.contactActionAvailableTags[i]);
        }

        jq.tmpl("tagViewTmpl",
                        {
                            tags: window.contactActionTags,
                            availableTags: window.contactActionAvailableTags
                        })
                        .appendTo("#tagsContainer>div:first");
        ASC.CRM.TagView.init("contact", true);
        if (window.contactActionTags.length > 0) {
            jq("#tagsContainer>div:first").removeClass("display-none");
            jq("#tagsContainer").prev().removeClass("crm-headerHiddenToggledBlock");
        }

    };

    var renderCustomFields = function() {
        if (typeof (window.customFieldList) != "undefined" && window.customFieldList.length != 0) {
            ASC.CRM.Common.renderCustomFields(customFieldList, "custom_field_", "customFieldRowTmpl", "#generalListEdit");
        }
        jq.registerHeaderToggleClick("#contactProfileEdit", "dt.headerToggleBlock");
        jq("#contactProfileEdit dt.headerToggleBlock").each(
                function() {
                    jq(this).nextUntil("dt.headerToggleBlock").hide();
                });
    };

    var initContactType = function (contactTypeID) {
        if (typeof (window.contactAvailableTypes) != "undefined" && window.contactAvailableTypes.length != 0) {
            var html = "";
            for (var i = 0, n = window.contactAvailableTypes.length; i < n; i++) {
                html += ["<option value='",
                    window.contactAvailableTypes[i].id,
                    "'",
                    window.contactAvailableTypes[i].id == contactTypeID ? " selected='selected'" : "",
                    ">",
                    jq.htmlEncodeLight(window.contactAvailableTypes[i].title),
                    "</option>"
                ].join('');
            }
            jq("#contactTypeContainer select").html(jq("#contactTypeContainer select").html() + html);
            if (contactTypeID != 0) {
                jq("#contactTypeContainer").prev().removeClass("crm-headerHiddenToggledBlock");
                jq("#contactTypeContainer > div:first").removeClass("display-none");
            }
        }
    };


    var initContactCurrency = function (contactCurrencyAbbr) {
        var tmp_cur = null,
            optionHtml = "",
            basicOptgroupHtml = ["<optgroup label=\"", ASC.CRM.Resources.CRMCommonResource.Currency_Basic, "\">"].join(""),
            otherOptgroupHtml = ["<optgroup label=\"", ASC.CRM.Resources.CRMCommonResource.Currency_Other, "\">"].join("");

        for (var i = 0, n = window.contactActionCurrencies.length; i < n; i++) {
            tmp_cur = window.contactActionCurrencies[i];

            optionHtml = [
                "<option value=\"",
                tmp_cur.abbreviation,
                "\"",
                tmp_cur.abbreviation == contactCurrencyAbbr ? " selected=\"selected\">" : ">",
                jq.format("{0} - {1}", tmp_cur.abbreviation, tmp_cur.title),
                "</option>"].join("");

            if (tmp_cur.isBasic) {
                basicOptgroupHtml += optionHtml;
            } else {
                otherOptgroupHtml += optionHtml;
            }
        }
        
        basicOptgroupHtml += "</optgroup>";
        otherOptgroupHtml += "</optgroup>";
        
        jq("#currencyContainer select").html(jq("#currencyContainer select").html() + basicOptgroupHtml + otherOptgroupHtml);

        if (contactCurrencyAbbr != "") {
            jq("#currencyContainer").prev().removeClass("crm-headerHiddenToggledBlock");
            jq("#currencyContainer > div:first").removeClass("display-none");
        }

    };

    var initOtherActionMenu = function() {
        jq("#menuCreateNewTask").bind("click", function() { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var initConfirmationAccessRightsPanel = function() {
        jq.tmpl("template-blockUIPanel", {
            id: "confirmationAccessRightsPanel",
            headerTest: ASC.CRM.Resources.CRMCommonResource.ConfirmationAccessRightsPanelHeader,
            innerHtmlText: [
                ASC.CRM.Resources.CRMCommonResource.ConfirmationAccessRightsPanelText1,
                "<p>",
                ASC.CRM.Resources.CRMCommonResource.ConfirmationAccessRightsPanelText2,
                "</p>"
            ].join(""),
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel
        }).insertAfter("#contactProfileEdit .contactManagerPanel");

        jq("#confirmationAccessRightsPanel").on("click", ".middle-button-container .button.blue", function () {
            confirmation = true;
            jq.unblockUI();
            ASC.CRM.ContactActionView.submitForm(saveButtonId);
            window.__doPostBack(saveButtonId, '');
        });
    };

    var initConfirmationGotoSettingsPanel = function (isCompany) {
        var view = isCompany === true ? "#company" : "#person";

        jq.tmpl("template-blockUIPanel", {
            id: "confirmationGotoSettingsPanel",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: "",
            innerHtmlText:
            ["<div class=\"confirmationNote\">",
                ASC.CRM.Resources.CRMJSResource.ConfirmGoToCustomFieldPage,
            "</div>"].join(''),
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            OKBtnHref: "Settings.aspx?type=custom_field" + view,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ""
        }).insertAfter("#otherContactCustomFieldPanel");

        jq("#confirmationGotoSettingsPanel .button.blue").on("click", function () {
            ASC.CRM.Common.unbindOnbeforeUnloadEvent();
        });
    };

    var initUploadPhotoPanel = function (contactPhotoFileSizeNote, contactPhotoMedium) {
        jq.tmpl("template-blockUIPanel", {
            id: "divLoadPhotoWindow",
            headerTest: ASC.CRM.Resources.CRMContactResource.ChooseProfilePhoto,
            questionText: "",
            innerHtmlText:
            ["<div id=\"divLoadPhotoFromPC\" style=\"margin-top: -20px;\">",
                "<h4>",
                    ASC.CRM.Resources.CRMContactResource.LoadPhotoFromPC,
                "</h4>",
                "<div class=\"describe-text\" style=\"margin-bottom: 5px;\">",
                    contactPhotoFileSizeNote,
                "</div>",
                "<div>",
                    "<a id=\"changeLogo\" class=\"button middle gray\">",
                        ASC.CRM.Resources.CRMJSResource.Browse, "...",
                    "</a>",
                    "<span class=\"fileUploadDscr\">",
                        ASC.CRM.Resources.CRMJSResource.NoFileSelected,
                    "</span>",
                    "<span class=\"fileUploadError\"></span>",
                    "<br />",
                "</div>",
            "</div>",
            "<div id=\"divLoadPhotoDefault\">",
                "<h4>",
                    ASC.CRM.Resources.CRMContactResource.ChangePhotoToDefault,
                "</h4>",
                "<div id=\"divDefaultImagesHolder\" data-uploadOnly=\"true\">",
                    "<div class=\"ImageHolderOuter\" onclick=\"ASC.CRM.SocialMedia.DeleteContactAvatar();\">",
                        "<img class=\"AvatarImage\" src=",
                        contactPhotoMedium,
                        " alt=\"\" />",
                    "</div>",
                "</div>",
            "</div>",
            "<div id=\"divLoadPhotoFromSocialMedia\">",
                "<h4>",
                    ASC.CRM.Resources.CRMContactResource.LoadPhotoFromSocialMedia,
                "</h4>",
                "<div id=\"divImagesHolder\" data-uploadOnly=\"true\">",
                "</div>",
                "<div style=\"clear: both;\">",
                    "<div id=\"divAjaxImageContainerPhotoLoad\">",
                    "</div>",
                "</div>",
            "</div>"].join(''),
            OKBtn: "",
            CancelBtn: "",
            progressText: ""
        }).insertAfter("#divSMProfilesWindow");
    };

    var createNewAddress = function($contact, is_primary, category, street, city, state, zip, country) {
        if (jq("#addressContainer").children("div").length != 1) {
            $contact.attr("style", "margin-top: 10px;");
        }

        if (typeof (is_primary) != "undefined") {
            changeAddressPrimaryCategory($contact, is_primary);
        }
        if (typeof (category) != "undefined") {
            var $select = $contact.find("select.address_category"),
                $categoryOption = $select.children('option[category="' + category + '"]');

            if ($categoryOption.length == 1) {
                $select.children("option").removeAttr("selected");
                $categoryOption.attr("selected", "selected");

                $select.val($categoryOption.val());
                changeAddressCategory($select, category);
            }
        }

        var parts = jq("#addressContainer").children("div:last").attr('selectname').split('_'),
            ind = parts[2] * 1 + 1;
        $contact.find('input, textarea, select').not('.address_category').each(function() {
            if (jq(this).attr('name') != "") {
                var parts = jq(this).attr('name').split('_');
                parts[2] = ind;
                jq(this).attr('name', parts.join('_'));
            }
        });

        parts = $contact.attr('selectname').split('_');
        parts[2] = ind;
        $contact.attr('selectname', parts.join('_'));

        $contact.find('textarea.contact_street').val(street || "");
        $contact.find('input.contact_city').val(city || "");
        $contact.find('input.contact_state').val(state || "");
        $contact.find('input.contact_zip').val(zip || "");
        $contact.find('input.contact_country').val(country || "");

        return $contact;
    };

    var changeAddressPrimaryCategory = function($divAddressObj, isPrimary) {
        var tmpNum = isPrimary ? 1 : 0,
            tmpClass = isPrimary ? "is_primary primary_field" : "is_primary not_primary_field",
            $switcerObj = $divAddressObj.find('.is_primary');

        $switcerObj.attr("class", tmpClass);
        $switcerObj.attr("alt", ASC.CRM.Resources.CRMJSResource.Primary);
        $switcerObj.attr("title", ASC.CRM.Resources.CRMJSResource.Primary);

        var parts = $divAddressObj.attr('selectname').split('_');
        parts[5] = tmpNum;
        $divAddressObj.attr('selectname', parts.join('_'));

        $divAddressObj.find('input, textarea, select').not('.address_category').each(function() {
            var parts = jq(this).attr('name').split('_');
            parts[5] = tmpNum;
            jq(this).attr('name', parts.join('_'));
        });
    };

    var _changeCommunicationPrimaryCategory = function($divObj, isPrimary) {
        var tmpNum = isPrimary ? 1 : 0,
            tmpClass = isPrimary ? "is_primary primary_field" : "is_primary not_primary_field",
            $switcerObj = $divObj.find('.is_primary');

        $switcerObj.attr("class", tmpClass);
        $switcerObj.attr("alt", ASC.CRM.Resources.CRMJSResource.Primary);
        $switcerObj.attr("title", ASC.CRM.Resources.CRMJSResource.Primary);

        var $inputObj = $divObj.find('input.textEdit'),
            parts = $inputObj.attr('name').split('_');

        parts[4] = tmpNum;
        $inputObj.attr('name', parts.join('_'));
    };

    var changeBaseCategory = function(Obj, text, category) {
        jq(Obj).text(text);
        var $inputObj = jq(Obj).parents('tr:first').find('input'),
            parts = $inputObj.attr('name').split('_');
        parts[3] = category;
        $inputObj.attr('name', parts.join('_'));
        jq("#baseCategoriesPanel").hide();
    };

    var changePhoneCategory = function(Obj, text, category) {
        jq(Obj).text(text);
        var $inputObj = jq(Obj).parents('tr:first').find('input'),
            parts = $inputObj.attr('name').split('_');
        parts[3] = category;
        $inputObj.attr('name', parts.join('_'));
        jq("#phoneCategoriesPanel").hide();
    };

    var changeAddressCategory = function(switcerObj, category) {
        jq(switcerObj).parents('table:first').find('input, textarea, select').not('.address_category').each(function() {
            var parts = jq(this).attr('name').split('_');
            parts[3] = category;
            jq(this).attr('name', parts.join('_'));
        });
    };

    var removeAssignedPersonFromCompany = function(id) {
        if (jq("#trashImg_" + id).length == 1) {
            jq("#trashImg_" + id).hide();
            jq("#loaderImg_" + id).show();
        }
        if (typeof (id) == "number") {
            var index = jq.inArray(id, window.assignedContactSelector.SelectedContacts);
            if (index != -1) {
                window.assignedContactSelector.SelectedContacts.splice(index, 1);
            } else {
                console.log("Can't find such contact in list");
            }
            ASC.CRM.ContactSelector.Cache = {};
        } else {
            for (var i = 0, n = ASC.CRM.SocialMedia.selectedPersons.length; i < n; i++) {
                if (ASC.CRM.SocialMedia.selectedPersons[i].id == id) {
                    ASC.CRM.SocialMedia.selectedPersons.splice(i, 1);
                    break;
                }
            }
        }

        jq("#contactItem_" + id).animate({ opacity: "hide" }, 500);

        setTimeout(function() {
            jq("#contactItem_" + id).remove();
            if (jq("#contactTable tr").length == 0) {
                jq("#contactListBox").parent().addClass('hiddenFields');
            }
        }, 500);
    };

    var addAssignedPersonToCompany = function(obj, params) {
        if (jq("#contactItem_" + obj.id).length > 0) return false;

        if (params.newContact == true) {
            var person = {
                Key: params.data.firstName,
                Value: params.data.lastName,
                canEdit: true,
                displayName: obj.displayName,
                id: "new_" + jq("#contactTable tr").length,
                isCompany: false,
                isPrivate: false,
                isShared: params.data.isShared,
                smallFotoUrl: obj.smallFotoUrl
            };

            ASC.CRM.Common.contactItemFactory(person, { showUnlinkBtn: true, showActionMenu: false });
            jq.tmpl("simpleContactTmpl", person).prependTo("#contactTable tbody");
            jq("#contactListBox").parent().removeClass('hiddenFields');
            ASC.CRM.Common.RegisterContactInfoCard();

            ASC.CRM.SocialMedia.selectedPersons.push(person);
        } else {

            Teamlab.getCrmContact({}, obj.id, {
                success: function (par, contact) {
                    ASC.CRM.Common.contactItemFactory(contact, { showUnlinkBtn: true, showActionMenu: false });
                    jq.tmpl("simpleContactTmpl", contact).prependTo("#contactTable tbody");
                    jq("#contactListBox").parent().removeClass('hiddenFields');
                    ASC.CRM.Common.RegisterContactInfoCard();

                    window.assignedContactSelector.SelectedContacts.push(contact.id);
                    //ASC.CRM.ContactSelector.Cache = {};
                }
            });
        }

    };

    var validateEmail = function($emailInputObj) {
        var email = jq.trim($emailInputObj.value),
            $tableObj = jq($emailInputObj).parents("table:first");

        if (email == "" || jq.isValidEmail(email)) {
            $tableObj.css("borderColor", "");
            $tableObj.parent().children(".requiredErrorText").hide();
            return true;
        } else {
            $tableObj.css("borderColor", "#CC0000");
            $tableObj.parent().children(".requiredErrorText").show();
            $emailInputObj.focus();
            return false;
        }

    };

    var validatePhone = function($phoneInputObj) {
        var phone = jq.trim($phoneInputObj.value),
            $tableObj = jq($phoneInputObj).parents("table:first"),
            reg = new RegExp(/(^\+)?(\d+)/);

        if (phone == "" || reg.test(phone)) {
            $tableObj.css("borderColor", "");
            $tableObj.parent().children(".requiredErrorText").hide();
            return true;
        } else {
            $tableObj.css("borderColor", "#CC0000");
            $tableObj.parent().children(".requiredErrorText").show();
            $phoneInputObj.focus();
            return false;
        }

    };

    var disableSubmitForm = function () {
        jq("#contactProfileEdit input, #contactProfileEdit select, #contactProfileEdit textarea").attr("readonly", "readonly").addClass('disabled');
        jq("#contactProfileEdit .input_with_type").addClass('disabled');
        jq(".under_logo .linkChangePhoto").addClass("disable");
    };

    var enableSubmitForm = function () {
        jq("#contactProfileEdit input.disabled, #contactProfileEdit select.disabled, #contactProfileEdit textarea.disabled").removeAttr("readonly").removeClass('disabled');
        jq("#contactProfileEdit .input_with_type.disabled").removeClass('disabled');
        jq(".under_logo .linkChangePhoto.disable").removeClass("disable");
    };

    var validateSubmitForm = function () {
        var isValid = true,
            isEmailValid = true;
        if (jq("#typeAddedContact").val() == "people") {
            if (jq.trim(jq("#contactProfileEdit input[name=baseInfo_firstName]").val()) == "") {
                ShowRequiredError(jq("#contactProfileEdit input[name=baseInfo_firstName]"));
                isValid = false;
            }

            if (typeof (window.companySelector) != "undefined") {
                jq("#companySelectorsContainer input[name=baseInfo_compID]").val(typeof (window.companySelector.SelectedContacts[0]) != 'undefined' ? window.companySelector.SelectedContacts[0] : "");
                jq("#companySelectorsContainer input[name=baseInfo_compName]").val(jq.trim(jq('#contactTitle_companySelector_0').val()));
            }
        } else {
            if (jq.trim(jq("#contactProfileEdit input[name=baseInfo_companyName]").val()) == "") {
                ShowRequiredError(jq("#contactProfileEdit input[name=baseInfo_companyName]"));
                isValid = false;
            }
        }

        if (!isValid) {
            enableSubmitForm();
            return false;
        }

        jq("#emailContainer > div:not(:first) > table input").each(function () {
            if (!validateEmail(this)) {
                isEmailValid = false;
            }
        });

        if (!isEmailValid) {
            enableSubmitForm();
            jq.scrollTo(jq("#emailContainer").position().top - 100, { speed: 400 });
            return false;
        }

        if (!ASC.CRM.Data.IsCRMAdmin &&
            !window.SelectedUsers_ContactManager.IDs.includes(Teamlab.profile.id) &&
            !jq("#isPublic").is(":checked") &&
            !confirmation) {
            StudioBlockUIManager.blockUI("#confirmationAccessRightsPanel", 400);
            return false;
        }

        return true;
    };

    var prepareAddressDataForSubmitForm = function () {
        jq('#addressContainer').children('div:first').find('input, textarea, select').attr('name', '');
        jq('#addressContainer').children('div:not(:first)').each(function () {
            var $curObj = jq(this);
            if (jq.trim($curObj.find('.contact_street').val()) +
            jq.trim($curObj.find('.contact_city').val()) +
            jq.trim($curObj.find('.contact_state').val()) +
            jq.trim($curObj.find('.contact_zip').val()) +
            jq.trim($curObj.find('.contact_country').val()) == "") {
                $curObj.find('input, textarea, select').attr('name', '');
            } else {
                $curObj.addClass("not_empty");
            }
        });

        if ((jq('#addressContainer').children('div:not(:first)').find('.primary_field').length == 0 ||
            !jq('#addressContainer').children('div:not(:first)').find('.primary_field').parent().parent().hasClass('not_empty'))
                && jq('#addressContainer').children('div.not_empty').length > 0) {
            ASC.CRM.ContactActionView.choosePrimaryElement(jq(jq('#addressContainer').children('div.not_empty')[0]).children(".actions_for_item").children(".is_primary"), true);
        }
    };

    var prepareEmailAndPhoneDataForSubmitForm = function () {
        jq('#emailContainer').children('div:first').find('input').attr('name', '');
        jq('#phoneContainer').children('div:first').find('input').attr('name', '');

        jq('#emailContainer').children('div:not(:first)').find('input').each(function () {
            if (jq.trim(jq(this).val()) != '') {
                jq(this).parents('table:first').parent().addClass("not_empty");
            }
        });
        jq('#phoneContainer').children('div:not(:first)').find('input').each(function () {
            if (jq.trim(jq(this).val()) != '') {
                jq(this).parents('table:first').parent().addClass("not_empty");
            }
        });

        if ((jq('#emailContainer').children('div:not(:first)').find('.primary_field').length == 0 ||
                       !jq('#emailContainer').children('div:not(:first)').find('.primary_field').parent().parent().hasClass('not_empty'))
                           && jq('#emailContainer').children('div.not_empty').length > 0) {
            ASC.CRM.ContactActionView.choosePrimaryElement(jq(jq('#emailContainer').children('div.not_empty')[0]).children(".actions_for_item").children(".is_primary"), false);
        }

        if ((jq('#phoneContainer').children('div:not(:first)').find('.primary_field').length == 0 ||
            !jq('#phoneContainer').children('div:not(:first)').find('.primary_field').parent().parent().hasClass('not_empty'))
                && jq('#phoneContainer').children('div.not_empty').length > 0) {
            ASC.CRM.ContactActionView.choosePrimaryElement(jq(jq('#phoneContainer').children('div.not_empty')[0]).children(".actions_for_item").children(".is_primary"), false);
        }
    };

    var _bindLeaveThePageEvent = function () {
        jq("#contactProfileEdit").on("keyup change paste", "input, select, textarea", function (e) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
        });
        jq("#contactProfileEdit").on("click", ".crm-deleteLink", function (e) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
        });
        jq(window).on("contactPhotoUploadSuccessComplete addTagComplete deleteTagComplete setContactInSelector editContactInSelector deleteContactFromSelector advUserSelectorDeleteComplete advUserSelectorPushUserComplete", function (e) {
            ASC.CRM.Common.bindOnbeforeUnloadEvent();
        });
    };

    return {
        init: function (contactID,
                        contactTypeID,
                        contactCurrency,
                        shareType,
                        _selectorType,
                        contactPhotoFileSizeNote,
                        errorCookieKey) {
            if (isInit === false) {
                isInit = true;

                var saveContactError = jq.cookies.get(errorCookieKey);
                if (saveContactError != null && saveContactError != "") {
                    jq.cookies.del(errorCookieKey);
                    jq.tmpl("template-blockUIPanel", {
                        id: "saveContactError",
                        headerTest: ASC.CRM.Resources.CRMCommonResource.Alert,
                        questionText: "",
                        innerHtmlText: ['<div>', saveContactError, '</div>'].join(''),
                        CancelBtn: ASC.CRM.Resources.CRMCommonResource.Close,
                        progressText: ""
                    }).insertAfter("#crm_contactMakerDialog");

                    PopupKeyUpActionProvider.EnableEsc = false;
                    StudioBlockUIManager.blockUI("#saveContactError", 500);
                }

                var isCompany = (jq("#typeAddedContact").val() === "company"),

                    $avatar = jq("#contactPhoto .contact_photo:first"),
                    handlerAvatarUrl = $avatar.attr("data-avatarurl");
                if (handlerAvatarUrl != "") {
                    ASC.CRM.Common.loadContactFoto($avatar, $avatar, handlerAvatarUrl);
                }

                ASC.CRM.UserSelectorListView.Init(
                    "_ContactManager",
                    "UserSelectorListView_ContactManager",
                    true,
                    ASC.CRM.Resources.CRMContactResource.NotifyContactManager,
                    [],
                    null,
                    [],
                    "#contactActionViewManager",
                    false);


                if (isCompany === false) {
                    if (window.presetCompanyForPersonJson != null && window.presetCompanyForPersonJson != "") {
                        window.presetCompanyForPersonJson = new Array(jq.parseJSON(window.presetCompanyForPersonJson));
                    }
                    window["companySelector"] = new ASC.CRM.ContactSelector.ContactSelector("companySelector",
                    {
                        SelectorType: _selectorType,
                        EntityType: 0,
                        EntityID: 0,
                        ShowOnlySelectorContent: false,
                        DescriptionText: ASC.CRM.Resources.CRMContactResource.FindCompanyByName,
                        DeleteContactText: "",
                        AddContactText: "",
                        IsInPopup: false,
                        ShowChangeButton: true,
                        ShowAddButton: false,
                        ShowDeleteButton: false,
                        ShowContactImg: true,
                        ShowNewCompanyContent: true,
                        ShowNewContactContent: false,
                        presetSelectedContactsJson: '',
                        ExcludedArrayIDs: [],
                        HTMLParent: "#companySelectorsContainer div:first",
                        presetSelectedContactsJson: window.presetCompanyForPersonJson
                    });

                } else {
                    window["assignedContactSelector"] = new ASC.CRM.ContactSelector.ContactSelector("assignedContactSelector",
                    {
                        SelectorType: _selectorType,
                        EntityType: 0,
                        EntityID: 0,
                        ShowOnlySelectorContent: true,
                        DescriptionText: ASC.CRM.Resources.CRMContactResource.FindContactByName,
                        DeleteContactText: "",
                        AddContactText: "",
                        IsInPopup: false,
                        ShowChangeButton: false,
                        ShowAddButton: false,
                        ShowDeleteButton: false,
                        ShowContactImg: false,
                        ShowNewCompanyContent: false,
                        ShowNewContactContent: true,
                        ShowOnlySelectorContent: true,
                        AcceptNewContactShowOnly: true,
                        HTMLParent: "#assignedContactsListEdit dd.assignedContacts",
                        ExcludedArrayIDs: [],
                        presetSelectedContactsJson: window.presetPersonsForCompanyJson,
                    });
                }
                if (ASC.CRM.Data.IsCRMAdmin === true) {
                    initConfirmationGotoSettingsPanel(isCompany);
                } else {
                    initConfirmationAccessRightsPanel();
                }
                ASC.CRM.ListContactView.renderSimpleContent(true, false);

                renderContactTags();
                renderContactNetworks();

                jq("#generalListEdit").on("click", ".crm-headerHiddenToggledBlock", function(event) {
                    var container_id = jq(this).next('dd').attr('id');
                    //jq(this).next('dd').find(".crm-addNewLink").show();

                    if (container_id == "addressContainer") {
                        ASC.CRM.ContactActionView.editAddress(jq("#addressContainer > div:first .crm-addNewLink"));
                    } else if (container_id == "tagsContainer" || container_id == "contactTypeContainer" || container_id == "currencyContainer") {
                        jq("#" + container_id + " > div:first").removeClass("display-none");
                    } else {
                        ASC.CRM.ContactActionView.editCommunications(jq("#" + container_id).children("div:first").find(".crm-addNewLink"), container_id);
                    }

                    jq(this).removeClass("crm-headerHiddenToggledBlock");
                });

                jq.dropdownToggle({ dropdownID: 'phoneCategoriesPanel', switcherSelector: '#phoneContainer .input_with_type a', addTop: 2, addLeft: 0 });
                jq.dropdownToggle({ dropdownID: 'baseCategoriesPanel', switcherSelector: '#emailContainer .input_with_type a', noActiveSwitcherSelector: '#websiteAndSocialProfilesContainer .input_with_type a.social_profile_category', addTop: 2, addLeft: 0 });
                jq.dropdownToggle({ dropdownID: 'baseCategoriesPanel', switcherSelector: '#websiteAndSocialProfilesContainer .input_with_type a.social_profile_category', noActiveSwitcherSelector: '#emailContainer .input_with_type a', addTop: 2, addLeft: 0 });
                jq.dropdownToggle({ dropdownID: 'socialProfileCategoriesPanel', switcherSelector: '#websiteAndSocialProfilesContainer .input_with_type a.social_profile_type', addTop: 2, addLeft: 0 });

                renderCustomFields();
                initContactType(contactTypeID);
                initContactCurrency(contactCurrency);

                var $html = jq.tmpl("makePublicPanelTemplate",
                    {
                        Title: isCompany ?
                                ASC.CRM.Resources.CRMContactResource.MakePublicPanelTitleForCompany :
                                ASC.CRM.Resources.CRMContactResource.MakePublicPanelTitleForPerson,
                        Description: ASC.CRM.Resources.CRMContactResource.MakePublicPanelDescrForContact,
                        IsPublicItem: shareType == 1 || shareType == 2,
                        CheckBoxLabel: isCompany ?
                                        ASC.CRM.Resources.CRMContactResource.MakePublicPanelCheckBoxLabelForCompany :
                                        ASC.CRM.Resources.CRMContactResource.MakePublicPanelCheckBoxLabelForPerson
                    });

                $html.find(".makePublicPanelSelector")
                    .append(jq("<option value='2'></option>").text(ASC.CRM.Resources.CRMCommonResource.AccessRightsForReading))
                    .append(jq("<option value='1'></option>").text(ASC.CRM.Resources.CRMCommonResource.AccessRightsForReadWriting))
                    .val(shareType == 1 || shareType == 2 ? shareType : "2")
                    .tlCombobox();

                $html.appendTo("#makePublicPanel");

                ASC.CRM.Common.initTextEditCalendars();

                initOtherActionMenu();

                ASC.CRM.ListContactView.initConfirmationPanelForDelete();


                initUploadPhotoPanel(contactPhotoFileSizeNote, ASC.CRM.Data.DefaultContactPhoto[isCompany === true ? "CompanyMediumSizePhoto" : "PersonMediumSizePhoto"]);

                ASC.CRM.ContactPhotoUploader.initPhotoUploader(
                    jq("#divLoadPhotoWindow"),
                    jq("#contactProfileEdit .contact_photo"),
                    {contactID: contactID, uploadOnly: true});


                if (typeof (window.assignedContactSelector) != "undefined") {
                    if (window.assignedContactSelector.SelectedContacts.length == 0) {
                        jq("#contactListBox").parent().addClass('hiddenFields');
                    }
                    window.assignedContactSelector.SelectItemEvent = addAssignedPersonToCompany;
                    ASC.CRM.ListContactView.removeMember = removeAssignedPersonFromCompany;
                }

                if (!isNaN(contactID) && jq("#deleteContactButton").length == 1) {
                    var contactName = jq("#deleteContactButton").attr("contactName");
                    jq("#deleteContactButton").unbind("click").bind("click", function() {
                        ASC.CRM.ListContactView.showConfirmationPanelForDelete(contactName, contactID, isCompany, false);
                    });
                }

                ASC.CRM.SocialMedia.init(ASC.CRM.Data.DefaultContactPhoto[isCompany === true ? "CompanyBigSizePhoto" : "PersonBigSizePhoto"]);

                jq(".cancelSbmtFormBtn:first").on("click", function () {
                    ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                    return true;
                });

                _bindLeaveThePageEvent();
            }
        },

        editCommunicationsEvent: function(evt, container_id) {
            evt = evt || window.event;
            var $target = jq(evt.target || evt.srcElement);
            ASC.CRM.ContactActionView.editCommunications($target, container_id);
        },

        editCommunications: function($target, container_id) {
            var add_new_button_class = "crm-addNewLink",
                delete_button_class = "crm-deleteLink",
                primary_class = "primary_field";

            if ($target.length == 0 && container_id == "overviewContainer" || $target.hasClass(add_new_button_class)) {
                var $lastVisibleDiv = jq('#' + container_id).children('div:visible:last');
                if ($lastVisibleDiv.length != 0) {
                    $lastVisibleDiv.find("." + add_new_button_class).hide();
                }

                var is_primary = jq('#' + container_id).find(".primary_field").length == 0 ? true : undefined,
                    $newContact = ASC.CRM.ContactActionView.createNewCommunication(container_id, "", is_primary);

                $newContact.insertAfter(jq('#' + container_id).children('div:last')).show()
                $newContact.find('input.textEdit').focus();

            } else if ($target.hasClass(delete_button_class)) {
                _bindLeaveThePageEvent(null, $target);
                var $divHTML = $target.parent().parent();
                if (jq('#' + container_id).children('div').length == 2) {
                    $divHTML.parent().prev('dt').addClass("crm-headerHiddenToggledBlock");
                }

                $divHTML.remove();
                if ($divHTML.find('.' + primary_class).length == 1 && jq('#' + container_id).children('div:not(:first)').length >= 1) {
                    ASC.CRM.ContactActionView.choosePrimaryElement(jq(jq('#' + container_id).children('div:not(:first)')[0]).find('.is_primary'), false);
                }
                jq('#' + container_id).children('div:not(:first)').find("." + add_new_button_class).hide();
                jq('#' + container_id).children('div:last').find("." + add_new_button_class).show();
            }
        },

        createNewCommunication: function(container_id, text, is_primary) {
            var $contact = jq('#' + container_id).children('div:first').clone();

            if (container_id != "overviewContainer") {
                var $lastInputElement = jq('#' + container_id).children('div:last').find('input.textEdit');
                if ($lastInputElement.length != 0) {
                    var parts = $lastInputElement.attr('name').split('_'),
                        ind = parts[2] * 1 + 1;

                    parts = $contact.find('input.textEdit').attr('name').split('_');
                    parts[2] = ind;
                    $contact.find('input.textEdit').attr('name', parts.join('_'));
                }

                if (text && text != "") {
                    $contact.children('table').find('input').val(text);
                }

                if (typeof (is_primary) != "undefined") {
                    var $isPrimaryElem = $contact.children(".actions_for_item").children(".is_primary");
                    if ($isPrimaryElem.length != 0) {
                        _changeCommunicationPrimaryCategory($contact, is_primary);
                    }
                }
            } else {
                if (text && text != "") {
                    $contact.children('textarea').val(text);
                }
            }
            return $contact;
        },

        editAddressEvent: function(evt) {
            evt = evt || window.event;
            var $target = jq(evt.target || evt.srcElement);
            ASC.CRM.ContactActionView.editAddress($target);
        },

        editAddress: function($target) {
            var add_new_button_class = "crm-addNewLink",
                delete_button_class = "crm-deleteLink",
                primary_class = "primary_field";
            if ($target.hasClass(add_new_button_class)) {
                var $lastVisibleDiv = jq('#addressContainer').children('div:visible:last');
                if ($lastVisibleDiv.length != 0) {
                    $lastVisibleDiv.find("." + add_new_button_class).hide();
                }
                var is_primary = jq("#addressContainer").find(".primary_field").length == 0 ? true : undefined,
                    $newContact = createNewAddress(jq('#addressContainer').children('div:first').clone(), is_primary).insertAfter(jq('#addressContainer').children('div:last')).show();
                $newContact.find('textarea').focus();
            } else if ($target.hasClass(delete_button_class)) {
                _bindLeaveThePageEvent(null, $target);
                var $divHTML = $target.parent().parent();

                if (jq('#addressContainer').children('div').length == 2) {
                    $divHTML.parent().prev('dt').addClass("crm-headerHiddenToggledBlock");
                }
                $divHTML.remove();

                if ($divHTML.find('.' + primary_class).length == 1 && jq('#addressContainer').children('div:not(:first)').length >= 1) {
                    ASC.CRM.ContactActionView.choosePrimaryElement(jq(jq('#addressContainer').children('div:not(:first)')[0]).find('.is_primary'), true);
                }
                jq('#addressContainer').children('div:not(:first)').find("." + add_new_button_class).hide();
                jq('#addressContainer').children('div:last').find("." + add_new_button_class).show();
            }
        },

        changeSocialProfileCategory: function(Obj, category, text, categoryName) {
            var $divObj = jq(Obj).parents('table:first').parent();
            jq(Obj).text(text);
            $inputObj = jq(Obj).parents('tr:first').find('input');
            var parts = $inputObj.attr('name').split('_');
            parts[1] = categoryName;
            $inputObj.attr('name', parts.join('_'));

            var $findProfileObj = $divObj.find('.find_profile'),
                isShown = false,
                func = "",
                title = "",
                description = " ";
            switch (category) { // Enum ContactInfoType
                case 4: // twitter
                    isShown = window.twitterSearchEnabled;
                    title = ASC.CRM.Resources.CRMJSResource.FindTwitter;
                    description = ASC.CRM.Resources.CRMJSResource.ContactTwitterDescription;
                    func = (function(p1, p2) { return function() { ASC.CRM.SocialMedia.FindTwitterProfiles(jq(this), jq("#typeAddedContact").val(), p1, p2); } })(-3, 5)
                    break;
            }

            if (isShown) {
                $findProfileObj.unbind('click').click(func);
                $findProfileObj.attr('title', title).show();
            } else {
                $findProfileObj.hide();
            }

            $divObj.children(".text-medium-describe").text(description);
            jq("#socialProfileCategoriesPanel").hide();
        },

        choosePrimaryElement: function(switcerObj, isAddress) {
            if (!isAddress) {
                var $divObj = jq(switcerObj).parent().parent();
                _changeCommunicationPrimaryCategory($divObj, true);

                jq(switcerObj).parents('dd:first').children('div:not(:first)').not($divObj).each(function() {
                    _changeCommunicationPrimaryCategory(jq(this), false);
                });
            } else {
                var $divAddressObj = jq(switcerObj).parent().parent();
                changeAddressPrimaryCategory($divAddressObj, true);

                jq(switcerObj).parents('dd:first').children('div:not(:first)').not($divAddressObj).each(function() {
                    changeAddressPrimaryCategory(jq(this), false);
                });
            }

            jq(switcerObj).parents('dd:first').find('.is_primary').not(switcerObj).attr("class", "is_primary not_primary_field");
            jq(switcerObj).parents('dd:first').find('.is_primary').not(switcerObj).attr("alt", ASC.CRM.Resources.CRMJSResource.CheckAsPrimary);
            jq(switcerObj).parents('dd:first').find('.is_primary').not(switcerObj).attr("title", ASC.CRM.Resources.CRMJSResource.CheckAsPrimary);
        },


        submitForm: function (buttonUnicId) {
            saveButtonId = buttonUnicId;

            if (jq("[id*=saveContactButton]:first").hasClass("postInProcess")) {
                return false;
            }
            jq("[id*=saveContactButton]:first").addClass("postInProcess");

            try {
                disableSubmitForm();
                HideRequiredError();

                if (validateSubmitForm() === true) {
                    LoadingBanner.showLoaderBtn("#crm_contactMakerDialog");

                    prepareAddressDataForSubmitForm();
                    prepareEmailAndPhoneDataForSubmitForm();

                    jq('#overviewContainer').children('div:first').find('textarea').attr('name', '');
                    jq('#websiteAndSocialProfilesContainer').children('div:first').find('input').attr('name', '');
                    jq('#websiteAndSocialProfilesContainer').children('div:not(:first)').find('input').each(function () {
                        if (jq.trim(jq(this).val()) != '') {
                            jq(this).parents('table:first').parent().addClass("not_empty");
                        }
                    });


                    jq("#isPublicContact").val(jq("#isPublic").is(":checked") ? jq(".makePublicPanel select.makePublicPanelSelector").val() : 0);

                    jq("#notifyContactManagers").val(jq("#cbxNotify_ContactManager").is(":checked"));
                    jq("#selectedContactManagers").val(window.SelectedUsers_ContactManager.IDs.join(","));
                    /********/

                    var $checkboxes = jq("#generalListEdit input[type='checkbox'][id^='custom_field_']");
                    if ($checkboxes) {
                        for (var i = 0, n = $checkboxes.length; i < n; i++) {
                            if (jq($checkboxes[i]).is(":checked")) {
                                var id = $checkboxes[i].id.replace('custom_field_', '');
                                jq("#generalListEdit input[name='customField_" + id + "']").val(jq($checkboxes[i]).is(":checked"));
                            }
                        }
                    }

                    ASC.CRM.TagView.prepareTagDataForSubmitForm(jq("input[name='baseInfo_assignedTags']"));

                    if (jq("#typeAddedContact").val() == "people") {
                        ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                        return true;
                    } else if (ASC.CRM.SocialMedia.selectedPersons.length == 0) {
                        jq("#assignedContactsListEdit input[name='baseInfo_assignedContactsIDs']").val(window.assignedContactSelector.SelectedContacts);
                        ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                        return true;
                    } else {
                        var data = [];
                        for (var i = 0, n = ASC.CRM.SocialMedia.selectedPersons.length; i < n; i++) {
                            data.push({
                                FirstName: ASC.CRM.SocialMedia.selectedPersons[i].Key,
                                LastName: ASC.CRM.SocialMedia.selectedPersons[i].Value
                            });
                        }

                        jq("#assignedContactsListEdit input[name='baseInfo_assignedNewContactsIDs']").val(jq.toJSON(data));
                        jq("#assignedContactsListEdit input[name='baseInfo_assignedContactsIDs']").val(window.assignedContactSelector.SelectedContacts);

                        ASC.CRM.Common.unbindOnbeforeUnloadEvent();
                        return true;
                    }
                }
                jq("[id*=saveContactButton]:first").removeClass("postInProcess");
                return false;

            } catch (e) {
                console.log(e);
                jq("[id*=saveContactButton]:first").removeClass("postInProcess");
                return false;
            }
        },

        showBaseCategoriesPanel: function(switcherUI) {
            jq("#baseCategoriesPanel a.dropdown-item").unbind('click').click(function() {
                changeBaseCategory(switcherUI, jq(this).text(), jq(this).attr("category"));

            });
        },

        showPhoneCategoriesPanel: function(switcherUI) {
            jq("#phoneCategoriesPanel a.dropdown-item").unbind('click').click(function() {
                changePhoneCategory(switcherUI, jq(this).text(), jq(this).attr("category"));
            });
        },

        showSocialProfileCategoriesPanel: function(switcherUI) {
            jq("#socialProfileCategoriesPanel a.dropdown-item").unbind('click').click(function() {
                ASC.CRM.ContactActionView.changeSocialProfileCategory(switcherUI, jq(this).attr("category") * 1, jq(this).text(), jq(this).attr("categoryName"));
            });
        },

        changeAddressCategory: function(obj) {
            changeAddressCategory(obj, jq(obj).find('option:selected').attr("category"));
        },

        showAssignedContactPanel: function() {
            jq('#assignedContactsListEdit .assignedContacts').removeClass('hiddenFields');
            jq('#assignedContactsListEdit .assignedContactsLink').addClass('hiddenFields');
        },

        showGotoAddSettingsPanel: function () {
            if (window.onbeforeunload == null) {//No need the confirmation
                var isCompany = (jq("#typeAddedContact").val() === "company"),
                    view = isCompany === true ? "#company" : "#person";
                location.href = "Settings.aspx?type=custom_field" + view;
            } else {
                PopupKeyUpActionProvider.EnableEsc = false;
                StudioBlockUIManager.blockUI("#confirmationGotoSettingsPanel", 500);
            }
        },

        prepareSocialNetworks: function () {

            jq("#divImagesHolder").html("");
            var $networks = jq("#websiteAndSocialProfilesContainer").find("input");

            ASC.CRM.SocialMedia.socialNetworks = [];
            for (var i = 0, n = $networks.length; i < n; i++) {
                var $el = jq($networks[i]),
                    text = jq.trim($el.val());

                if (text == "") continue;
                if ($el.attr("name").indexOf("contactInfo_Twitter_") == 0) {
                    ASC.CRM.SocialMedia.socialNetworks.push({
                        data: text,
                        infoType: 4
                    });
                } else if ($el.attr("name").indexOf("contactInfo_LinkedIn_") == 0) {
                    ASC.CRM.SocialMedia.socialNetworks.push({
                        data: text,
                        infoType: 5
                    });
                } else if ($el.attr("name").indexOf("contactInfo_Facebook_") == 0) {
                    ASC.CRM.SocialMedia.socialNetworks.push({
                        data: text,
                        infoType: 6
                    });
                }
            }
            ASC.CRM.SocialMedia.ContactImageListLoaded = false;
        }
    };
})();



/*
* --------------------------------------------------------------------
* jQuery-Plugin - sliderWithSections
* --------------------------------------------------------------------
*/

jQuery.fn.sliderWithSections = function (settings) {
    //accessible slider options
    var options = jQuery.extend({
        disabled: false,
        value: null,
        colors: null,
        values: null,
        defaultColor: '#E1E1E1',
        liBorderWidth: 1,
        sliderOptions: null,
        max: 0,
        marginWidth: 1,
        slide: function (e, ui) {
        }
    }, settings);


    //plugin-generated slider options (can be overridden)
    var sliderOptions = {
        step: 1,
        min: 0,
        orientation: 'horizontal',
        max: options.max,
        range: false, //multiple select elements = true
        slide: function (e, ui) { //slide function
            var thisHandle = jQuery(ui.handle);
            thisHandle.attr('aria-valuetext', options.values[ui.value]).attr('aria-valuenow', ui.value);

            if (ui.value != 0) {
                thisHandle.find('.ui-slider-tooltip .ttContent').html(options.values[ui.value]);
                thisHandle.removeClass("ui-slider-tooltip-hide");
            } else {
                thisHandle.addClass("ui-slider-tooltip-hide");
            }

            var liItems = jQuery(this).children('ol.ui-slider-scale').children('li');

            for (var i = 0; i < sliderOptions.max; i++) {
                if (i < ui.value) {
                    var color = options.colors != null && options.colors[i] ? options.colors[i] : 'transparent';
                    jQuery(liItems[i]).css('background-color', color);
                } else {
                    jQuery(liItems[i]).css('background-color', options.defaultColor);
                }
            }

            options.slide(e, ui);
        },

        value: options.value
    };

    //slider options from settings
    options.sliderOptions = (settings) ? jQuery.extend(sliderOptions, settings.sliderOptions) : sliderOptions;


    //create slider component div
    var sliderComponent = jQuery('<div></div>'),
        $tooltip = jQuery('<a href="#" tabindex="0" ' +
        'class="ui-slider-handle" ' +
        'role="slider" ' +
        'aria-valuenow="' + options.value + '" ' +
        'aria-valuetext="' + options.values[options.value] + '"' +
        '><span class="ui-slider-tooltip ui-widget-content ui-corner-all"><span class="ttContent"></span>' +
        '<span class="ui-tooltip-pointer-down ui-widget-content"><span class="ui-tooltip-pointer-down-inner"></span></span>' +
        '</span></a>')
        .data('handleNum', options.value)
        .appendTo(sliderComponent);
    sliderComponent.find('.ui-slider-tooltip .ttContent').html(options.values[options.value]);
    if (options.values[options.value] == "") {
        sliderComponent.children(".ui-slider-handle").addClass("ui-slider-tooltip-hide");
    }

    var scale = sliderComponent.append('<ol class="ui-slider-scale ui-helper-reset" role="presentation" style="width: 100%; height: 100%;"></ol>').find('.ui-slider-scale:eq(0)');

    //var widthVal = (1 / sliderOptions.max * 100).toFixed(2) + '%';
    var sliderWidth = jQuery(this).css('width').replace('px', '') * 1,
        widthVal = ((sliderWidth - options.marginWidth * (sliderOptions.max - 1) - 2 * options.liBorderWidth * sliderOptions.max) / sliderOptions.max).toFixed(4);
    for (var i = 0; i <= sliderOptions.max; i++) {
        var style = (i == sliderOptions.max || i == 0) ? 'display: none;' : '',
            liStyle = (i == sliderOptions.max) ? 'display: none;' : '',
            color = 'transparent';

        if (i < options.value) {
            color = options.colors != null && options.colors[i] ? options.colors[i] : 'transparent';
        } else {
            color = options.defaultColor;
        }

        scale.append('<li style="left:' + leftVal(i, sliderWidth) + '; background-color:' + color + '; height: 100%; width:' + widthVal + 'px;' + liStyle + '"></li>');
    };

    function leftVal (i, sliderWidth) {
        var widthVal = ((sliderWidth - options.marginWidth * (sliderOptions.max - 1) - 2 * options.liBorderWidth * sliderOptions.max) / sliderOptions.max);
        return ((widthVal + 2 * options.liBorderWidth + options.marginWidth) * i).toFixed(4) + 'px';
    }

    //inject and return
    sliderComponent.appendTo(jQuery(this)).slider(options.sliderOptions).attr('role', 'application');
    sliderComponent.find('.ui-tooltip-pointer-down-inner').each(function () {
        var bWidth = jQuery('.ui-tooltip-pointer-down-inner').css('borderTopWidth'),
            bColor = jQuery(this).parents('.ui-slider-tooltip').css('backgroundColor');
        jQuery(this).css('border-top', bWidth + ' solid ' + bColor);
    });

    if (options.disabled)
        sliderComponent.slider('disable');

    return this;
};

jq(document).ready(function() {
    jq.dropdownToggle({
        switcherSelector: ".noContentBlock .hintTypes",
        dropdownID: "files_hintTypesPanel",
        fixWinSize: false
    });

    jq.dropdownToggle({
        switcherSelector: ".noContentBlock .hintCsv",
        dropdownID: "files_hintCsvPanel",
        fixWinSize: false
    });

    ASC.CRM.Common.setPostionPageLoader();
});