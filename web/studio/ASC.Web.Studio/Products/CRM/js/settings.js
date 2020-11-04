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
    ASC.CRM = function() { return {} };
}

ASC.CRM.SettingsPage = (function() {

    var _initSortableFields = function (entityType) {

        jq("#customFieldList").sortable({
            cursor: "move",
            handle: '.sort_drag_handle',
            items: 'li',
            start: function(event, ui) {
                jq("#customFieldActionMenu").hide();
                jq("#customFieldList .entity-menu.active").removeClass("active");
            },
            beforeStop: function(event, ui) {
                jq(ui.item[0]).attr("style", "");
            },
            update: function(event, ui) {
                var fieldID = [];

                jq("#customFieldList li").find("[id^=custom_field_]").each(function() {
                    fieldID.push(jq(this).attr("id").split('_')[2]);
                });

                Teamlab.reorderCrmCustomFields({}, entityType, fieldID, {});
            }
        });
    };

    var _initFieldsActionMenu = function() {
        if (jq("#customFieldActionMenu").length != 1) return;

        jq.dropdownToggle({
            dropdownID: "customFieldActionMenu",
            switcherSelector: "#customFieldList .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            beforeShowFunction: function (switcherObj, dropdownItem) {
                var fieldId = switcherObj.attr("fieldid");
                if (!fieldId) return;
            },
            showFunction: function(switcherObj, dropdownItem) {
                jq("#customFieldList .entity-menu.active").removeClass("active");
                if (dropdownItem.is(":hidden")) {
                    switcherObj.addClass("active");
                    if (switcherObj.attr("fieldid") != dropdownItem.attr("fieldid")) {
                        dropdownItem.attr("fieldid", switcherObj.attr("fieldid"));
                    }
                }
            },
            hideFunction: function() {
                jq("#customFieldList .entity-menu.active").removeClass("active");
            }
        });
    };

    var _showAddFieldPanel = function() {
        jq('#manageField div.containerHeaderBlock td:first').text(ASC.CRM.Resources.CRMSettingResource.CreateNewField);
        jq('#manageField .middle-button-container a.button.blue.middle').text(ASC.CRM.Resources.CRMSettingResource.AddThisField);

        LoadingBanner.strLoading = ASC.CRM.Resources.CRMSettingResource.CreateFieldInProgressing;

        _resetManageFieldPanel();
        RemoveRequiredErrorClass(jq("#manageField dl input:first"));
        jq('#manageField .middle-button-container a.button.blue.middle').unbind('click').click(function () {
            if (jq(this).hasClass("disable"))
                return;

            _createField();
        });
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#manageField", 400);
    };

    var _initOtherActionMenu = function() {
        jq("#menuCreateNewTask").bind("click", function() { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var _initNumericFieldsLimitation = function() {
        var maxCountRows = ASC.CRM.Data.MaxCustomFieldRows,
            maxCountCols = ASC.CRM.Data.MaxCustomFieldCols,
            maxCountSize = ASC.CRM.Data.MaxCustomFieldSize;

        jq.forceNumber({
            parent: "#manageField",
            input: "#text_field_size, #textarea_field_rows, #textarea_field_cols",
            integerOnly: true,
            positiveOnly: true
        });

        jq("#text_field_size").focusout(function (e) {
            var fieldSize = jq.trim(jq("#text_field_size").val());
            if (fieldSize != "" && fieldSize * 1 > maxCountSize) {
                jq("#text_field_size").val(maxCountSize);
            }
        });

        jq("#textarea_field_rows").focusout(function(e) {
            var fieldSize = jq.trim(jq("#textarea_field_rows").val());
            if (fieldSize != "" && fieldSize * 1 > maxCountRows) {
                jq("#textarea_field_rows").val(maxCountRows);
            }
        });

        jq("#textarea_field_cols").focusout(function(e) {
            var fieldSize = jq.trim(jq("#textarea_field_cols").val());
            if (fieldSize != "" && fieldSize * 1 > maxCountCols) {
                jq("#textarea_field_cols").val(maxCountCols);
            }
        });
    };

    var _findIndexOfFieldByID = function(fieldID) {
        for (var i = 0, n = ASC.CRM.SettingsPage.customFieldList.length; i < n; i++) {
            if (ASC.CRM.SettingsPage.customFieldList[i].id == fieldID) {
                return i;
            }
        }
        return -1;
    };

    var _resetManageFieldPanel = function() {
        var $p = jq("#manageField");
        $p.find("dl input:first").val("");
        $p.find("dl dd:last ul li:not(:first)").remove();
        $p.find("select").prop('value', '0');
        $p.find("dl .field_mask").hide();
        $p.find("dl .text_field").show();
        jq("#manageField input, #manageField select, #manageField textarea").removeAttr("readonly").removeAttr("disabled").removeClass('disabled');
        jq("#addOptionButton.display-none").removeClass("display-none");
        $p.find(".select_options .deleteBtn.display-none").removeClass("display-none");

        jq("#text_field_size").val(ASC.CRM.Data.DefaultCustomFieldSize);
        jq("#textarea_field_rows").val(ASC.CRM.Data.DefaultCustomFieldRows);
        jq("#textarea_field_cols").val(ASC.CRM.Data.DefaultCustomFieldCols);
    };


    var _fieldFactory = function(field) {
        if (jQuery.trim(field.mask) == "") {
            field.maskObj = "";
        } else {
            field.maskObj = jq.evalJSON(field.mask);
        }

        field.relativeItemsString = ASC.CRM.Common.getRelativeItemsLinkString(field.relativeItemsCount, jq.getURLParam("type"), _getCurEntityType());
    };

    var _validateDataField = function() {
        if (jq("#manageField dl input:first").val().trim() == "") {
            ShowRequiredError(jq("#manageField dl input:first"), true);
            return false;
        } else {
            RemoveRequiredErrorClass(jq("#manageField dl input:first"));
            return true;
        }
    };

    var _readFieldData = function(sortOrder) {
        var view = _getCurEntityType(),
            field = {
                label: jq("#manageField dl input:first").val().trim(),
                fieldType: parseInt(jq("#manageField dl select:first").val()),
                entityType: view != null && view != "" ? view : "contact",
                position: sortOrder
            };

        switch (field.fieldType) {
            case 0:
                field.mask = jq.toJSON({ "size": jq("#text_field_size").val() });
                break;
            case 1:
                field.mask = jq.toJSON({
                    "rows": jq("#textarea_field_rows").val(),
                    "cols": jq("#textarea_field_cols").val()
                });
                break;
            case 2:
                var selectedItem = new Array();
                jq("#manageField dd.select_options :input").each(function() {
                    var value = jq(this).val().trim();
                    if (value == "") { return; }
                    selectedItem.push(jq(this).val());
                });

                if (selectedItem.length == 0) {
                    toastr.error(ASC.CRM.Resources.CRMJSResource.EmptyItemList);
                    return;
                }
                field.mask = jq.toJSON(selectedItem);
                break;
            default:
                field.mask = "";
                break;
        }
        return field;
    };

    var _createField = function() {
        if (!_validateDataField()) { return; }

        var $listCustomFields = jq("#customFieldList li"),
            field = _readFieldData($listCustomFields.length),
            index = 0;

        for (var i = 0, len = $listCustomFields.length; i < len; i++) {
            if (jq($listCustomFields[i]).find(".customFieldTitle").text().trim() == field.label) {
                index = i + 1;
                break;
            }
        }
        if (index == 0) {
            Teamlab.addCrmCustomField({}, field.entityType, field, {
                before: function (params) {
                    LoadingBanner.showLoaderBtn("#manageField");
                },
                after: function (params) {
                    LoadingBanner.hideLoaderBtn("#manageField");
                },
                success: ASC.CRM.SettingsPage.CallbackMethods.add_customField
            });
        } else {
            ASC.CRM.Common.animateElement({
                element: jq($listCustomFields[index - 1]),
                afterScrollFunc: jq.unblockUI()
            });
        }
    };

    var _editField = function (liObj, oldField, indexOfField) {
        var $listCustomFields = jq("#customFieldList li"),
            index = 0,
            field = null;
        if (!_validateDataField()) { return; }

        if (oldField.relativeItemsCount != 0) {
            var tmpField = _readFieldData(indexOfField);
            field = oldField;
            field.label = tmpField.label;
            field.entityType = tmpField.entityType;
            field.mask = tmpField.mask;
        } else {
            field = _readFieldData(indexOfField);
            field.id = oldField.id;
        }

   
        for (var i = 0, n = $listCustomFields.length; i < n; i++) {
            if (jq($listCustomFields[i]).find(".customFieldTitle").text().trim() == field.label && jq($listCustomFields[i]).not(jq(liObj)).length != 0) {
                index = i + 1;
                break;
            }
        }
        if (index == 0) {
            Teamlab.updateCrmCustomField({ liObj: liObj, indexOfField: indexOfField }, field.entityType, field.id, field, {
                before: function() {
                    LoadingBanner.showLoaderBtn("#manageField");
                },
                after: function() {
                    LoadingBanner.hideLoaderBtn("#manageField");
                },
                success: ASC.CRM.SettingsPage.CallbackMethods.edit_customField
            });
        } else {
            ASC.CRM.Common.animateElement({
                element: jq($listCustomFields[index - 1]),
                afterScrollFunc: jq.unblockUI()
            });
        }
    };

    var _initEmptyScreen = function () {
        jq.tmpl("template-emptyScreen",
            {
                ID: "emptyCustomFieldContent",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_userfields"],
                Header: ASC.CRM.Resources.CRMSettingResource.EmptyContentCustomFields,
                Describe: ASC.CRM.Resources.CRMSettingResource.EmptyContentCustomFieldsDescript,
                ButtonHTML: ['<a class="link dotline plus">', ASC.CRM.Resources.CRMSettingResource.CreateCustomField, '</a>'].join(''),
                CssClass: "display-none"
            }).insertAfter("#customFieldList");

        jq("#emptyCustomFieldContent .emptyScrBttnPnl > a").on("click", function () { _showAddFieldPanel(); });
    };

    var _initTabs = function (type) {

        jq("<div class='fakeTabContainer' id='contFake'></div>\
            <div class='fakeTabContainer' id='persFake'></div>\
            <div class='fakeTabContainer' id='compFake'></div>\
            <div class='fakeTabContainer' id='dealFake'></div>\
            <div class='fakeTabContainer' id='caseFake'></div>").insertAfter("#CustomFieldsTabs");
        window.ASC.Controls.ClientTabsNavigator.init("CustomFieldsTabs", {
            tabs: [
            {
                title: ASC.CRM.Resources.CRMSettingResource.BothPersonAndCompany,
                selected: type === "contact",
                anchor: "contact",
                divID: "contFake",
                onclick: "ASC.CRM.SettingsPage.initData('contact');"
            },
            {
                title: ASC.CRM.Resources.CRMSettingResource.JustForPerson,
                selected: type === "person",
                anchor: "person",
                divID: "persFake",
                onclick: "ASC.CRM.SettingsPage.initData('person');"
            },
            {
                title: ASC.CRM.Resources.CRMSettingResource.JustForCompany,
                selected: type === "company",
                anchor: "company",
                divID: "compFake",
                onclick: "ASC.CRM.SettingsPage.initData('company');"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.DealModuleName,
                selected: type === "opportunity",
                anchor: "opportunity",
                divID: "dealFake",
                onclick: "ASC.CRM.SettingsPage.initData('opportunity');"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.CasesModuleName,
                selected: type === "case",
                anchor: "case",
                divID: "caseFake",
                onclick: "ASC.CRM.SettingsPage.initData('case');"
            }]
        });
    };

    var _getCurEntityType = function () {
        var available = ["contact", "person", "company", "opportunity", "case"],
            type = ASC.Controls.AnchorController.getAnchor();
        if (type == null || type == "" || jq.inArray(type, available) == -1) { type = "contact"; }
        return type;
    };

    var _initManagePanel = function () {

        jq.tmpl("template-blockUIPanel", {
            id: "manageField",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: "",
            innerHtmlText: jq.tmpl("customFieldActionPanelBodyTmpl", {}).html(),
            OKBtn: ASC.CRM.Resources.CRMSettingResource.AddThisField,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ""
        }).insertAfter("#customFieldList");

        jq("#manageField .button.gray").on("click", function(){PopupKeyUpActionProvider.EnableEsc = true;jq.unblockUI();});
    };

    var _iniDeleteFieldConfirmationPanel = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "deleteFieldConfirmation",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: '',
            innerHtmlText: ["<div>", ASC.CRM.Resources.CRMSettingResource.DeleteCustomFieldConfirmationText, "</div>"].join(''),
            OKBtn: ASC.CRM.Resources.CRMCommonResource.OK,
            OKBtnClass: "OKDeleteField",
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel
        }).insertAfter("#customFieldList");

        jq("#deleteFieldConfirmation").on("click", ".OKDeleteField", function () {
            var fieldid = jq("#customFieldActionMenu").attr('fieldid'),
                $menu = null,
                liObj = null;

            if (typeof (fieldid) === "undefined" || fieldid == "" || jq("#fieldMenu_" + fieldid).length == 0)
            { return; }

            $menu = jq("#fieldMenu_" + fieldid);
            if ($menu.length != 1) { return; }
            liObj = jq($menu.parents('li').get(0));

            _deleteFieldComplete(fieldid, liObj);
        });
    };

    var _deleteFieldComplete = function (fieldid, liObj) {
        var entityType = _getCurEntityType();

        Teamlab.removeCrmCustomField({ liObj: liObj }, entityType, fieldid, {
            before: function (params) {
                params.liObj.find(".entity-menu").hide();
                params.liObj.find("div.ajax_loader").show();
            },
            success: ASC.CRM.SettingsPage.CallbackMethods.delete_customField
        });
    };

    var timeoutId = null;

    return {
        CallbackMethods: {
            add_customField: function(params, field) {
                _fieldFactory(field);
                ASC.CRM.SettingsPage.customFieldList.push(field);

                var $itemHTML = jq.tmpl("customFieldSettingsRowTmpl", field);
                if (jq("#customFieldList li").length == 0) {
                    jq("#emptyCustomFieldContent").addClass("display-none");
                    jq("#customFieldList").show();
                    jq("#createNewField.display-none").removeClass("display-none");
                }
                $itemHTML.appendTo("#customFieldList");

                ASC.CRM.Common.animateElement({
                    element: $itemHTML,
                    afterScrollFunc: jq.unblockUI()
                });
            },

            edit_customField: function(params, field) {
                _fieldFactory(field);
                ASC.CRM.SettingsPage.customFieldList[params.indexOfField] = field;

                var $itemHTML = jq.tmpl("customFieldSettingsRowTmpl", field);

                params.liObj.hide();
                $itemHTML.insertAfter(params.liObj);
                params.liObj.remove();

                ASC.CRM.Common.animateElement({
                    element: $itemHTML,
                    afterScrollFunc: jq.unblockUI()
                });
            },

            delete_customField: function (params, field) {
                jq.unblockUI();
                params.liObj.remove();

                var index = _findIndexOfFieldByID(field.id);
                if (index != -1) { ASC.CRM.SettingsPage.customFieldList.splice(index, 1); }

                if (jq("#customFieldList li").length == 0) {
                    jq("#customFieldList").hide();
                    jq("#createNewField:not(.display-none)").addClass("display-none");
                    jq("#emptyCustomFieldContent").removeClass("display-none");
                }
            }
        },

        initData: function (type) {
            ASC.CRM.SettingsPage.customFieldList = [];
            jq("#customFieldList").html("").hide();
            jq("#emptyCustomFieldContent:not(.display-none)").addClass("display-none");
            LoadingBanner.displayLoading();

            Teamlab.getCrmCustomFields({}, type, function (params, fields) {
                for (var i = 0, len = fields.length; i < len; i++) {
                    _fieldFactory(fields[i]);
                    ASC.CRM.SettingsPage.customFieldList.push(fields[i]);
                }

                if (ASC.CRM.SettingsPage.customFieldList.length != 0) {
                    jq("#customFieldList").show();
                    jq("#createNewField.display-none").removeClass("display-none");
                    jq.tmpl("customFieldSettingsRowTmpl", ASC.CRM.SettingsPage.customFieldList).appendTo("#customFieldList");
                } else {
                    jq("#createNewField:not(.display-none)").addClass("display-none");
                    jq("#emptyCustomFieldContent").removeClass("display-none");
                }
                _initSortableFields(type);
                LoadingBanner.hideLoading();
            });

        },

        init: function () {
            var type = _getCurEntityType();
            _initTabs(type);
            _initEmptyScreen();
            _initManagePanel();
            _iniDeleteFieldConfirmationPanel();
            //jq("#customFieldList").disableSelection();

            jq("#createNewField").on("click", function () { _showAddFieldPanel(); });

            _initFieldsActionMenu();
            _initOtherActionMenu();
            _initNumericFieldsLimitation();
            ASC.CRM.SettingsPage.initData(type);
        },

        initExportView: function() {
            _initOtherActionMenu();
            ASC.CRM.SettingsPage.checkExportStatus(true);
        },

        startExportData: function () {
            jq("#exportDataContent a.button.blue.middle").hide();
            jq("#exportDataContent p.header-base-small").show();

            Teamlab.startCrmExportToCSV({},
                {
                    success: function (params, response) {
                        ASC.CRM.SettingsPage.checkExportStatus(true);
                    },
                    error: function (params, errors) {
                        var err = errors[0];
                        if (err != null) {
                            toastr.error(err);
                        }
                    }
                });
        },

        deleteField: function() {
            jq("#customFieldActionMenu").hide();
            jq("#customFieldList .entity-menu.active").removeClass("active");

            var index = -1,
                fieldid = jq("#customFieldActionMenu").attr('fieldid'),
                field = null,
                liObj = null,
                $menu = null;


            if (typeof (fieldid) === "undefined" || fieldid == "" || jq("#fieldMenu_" + fieldid).length == 0)
            { return; }

            $menu = jq("#fieldMenu_" + fieldid);
            if ($menu.length != 1) { return; }
            liObj = jq($menu.parents('li').get(0));

            index = _findIndexOfFieldByID(fieldid);
            if (index === -1) { return; }

            field = ASC.CRM.SettingsPage.customFieldList[index];

            if (field.relativeItemsCount == 0) {
                _deleteFieldComplete(fieldid, liObj);
            } else {
                StudioBlockUIManager.blockUI("#deleteFieldConfirmation", 500);
            }

        },

        showEditFieldPanel: function() {
            jq("#customFieldActionMenu").hide();
            jq("#customFieldList .entity-menu.active").removeClass("active");
            var fieldid = jq("#customFieldActionMenu").attr("fieldid"),
                $menu = null,
                liObj = null,
                index = -1,
                field = null;

            if (typeof (fieldid) === "undefined" || fieldid == "") { return; }

            $menu = jq("#fieldMenu_" + fieldid);
            if ($menu.length != 1) { return; }

            liObj = jq($menu.parents('li:first'));

            index = _findIndexOfFieldByID(fieldid);
            if (index === -1) { return; }

            field = ASC.CRM.SettingsPage.customFieldList[index];
            _resetManageFieldPanel();
            if (field.relativeItemsCount != 0) {
                jq("#manageField input, #manageField select, #manageField textarea")
                    .not("#manageField dl input:first")
                    .not("#text_field_size")
                    .not("#textarea_field_rows")
                    .not("#textarea_field_cols")
                    .prop("readonly", "readonly").prop("disabled", "disabled").addClass('disabled');
                if (field.fieldType == 2) {//select box
                    jq("#manageField .select_options .deleteBtn:not(.display-none)").addClass("display-none");
                }
            }

            jq('#manageField div.containerHeaderBlock td:first').text(ASC.CRM.Resources.CRMJSResource.EditSelectedCustomField.replace('{0}', field.label));
            jq('#manageField .middle-button-container a.button.blue.middle').text(ASC.CRM.Resources.CRMJSResource.SaveChanges);
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.SaveChangesProggress;


            jq("#manageField dl input:first").val(field.label);
            jq("#manageField dl select:first").val(field.fieldType);
            jq("#manageField dl .field_mask").hide();

            switch (field.fieldType) {
                case 0:
                    jq("#text_field_size").val(field.maskObj.size);
                    jq("#manageField dl .text_field").show();
                    break;
                case 1:
                    jq("#textarea_field_rows").val(field.maskObj.rows);
                    jq("#textarea_field_cols").val(field.maskObj.cols);
                    jq("#manageField dl .textarea_field").show();
                    break;
                case 2:
                    for (var i = 0, n = field.maskObj.length; i < n; i++) {
                        ASC.CRM.SettingsPage.toSelectBox(jq("#addOptionButton"));
                        jq("#manageField dd.select_options ul li:last input").val(field.maskObj[i])
                    }
                    jq("#manageField .select_options .deleteBtn:first").removeClass("display-none");
                    jq("#manageField .select_options input:first").removeAttr("readonly").removeAttr("disabled", "disabled").removeClass('disabled');
                    jq("#manageField dl .select_options").show();
                    break;
                default:
                    break;
            }

            RemoveRequiredErrorClass(jq("#manageField dl input:first"));
            jq('#manageField .middle-button-container a.button.blue.middle').unbind('click').click(function() {
                _editField(liObj, field, index);
            });
            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI("#manageField", 400);
        },

        selectTypeEvent: function(selectObj) {
            var idx = selectObj.selectedIndex,
                which = parseInt(selectObj.options[idx].value);
            jq("#manageField dl .field_mask").hide();

            switch (which) {
                case 0:
                    jq("#manageField dl .text_field").show();
                    break;
                case 1:
                    jq("#manageField dl .textarea_field").show();
                    break;
                case 2:
                    jq("#manageField dl .select_options").show();
                    break;
            }
        },

        toSelectBox: function(buttonObj) {
            var ulObj = jq(buttonObj).prev();
            ulObj.children(":first").clone().show().appendTo(ulObj).focus();
        },

        toggleCollapceExpand: function(elem) {
            $Obj = jq(elem);
            var isCollapse = $Obj.hasClass("headerCollapse");
            if (isCollapse) {
                $Obj.parents('li').nextUntil('#customFieldList li.expand_collapce_element').show();
            } else {
                $Obj.parents('li').nextUntil('#customFieldList li.expand_collapce_element').hide();
            }
            $Obj.toggleClass('headerExpand');
            $Obj.toggleClass('headerCollapse');
        },

        checkExportStatus: function(isFirstVisit) {
            if (isFirstVisit) {
                ASC.CRM.SettingsPage.closeExportProgressPanel();
            }

            Teamlab.getStatusExportToCSV({},
                {
                    success: function (params, response) {
                        if (response == null || response == "" || jQuery.isEmptyObject(response)) {
                            ASC.CRM.SettingsPage.closeExportProgressPanel();
                            return false;
                        }

                        var $edt = jq("#exportDataContent");
                        $edt.find("div.progress").css("width", parseInt(response.percentage) + "%");
                        $edt.find("div.percent").text(parseInt(response.percentage) + "%");
                        $edt.find("a.button.blue.middle").hide();
                        $edt.find("div.progress-container,div.middle-button-container,p.header-base-small,#abortButton").show();
                        $edt.find("#okButton").hide();

                        if (response.error != null && response.error != "") {
                            ASC.CRM.SettingsPage.buildErrorList(response);
                        } else {
                            if (response.isCompleted) {
                                $edt.find("#exportLinkBox span").html(
                                    jq("<a></a>").attr("href", response.fileUrl).text(response.fileName)
                                );
                                $edt.find("p.header-base-small").hide();
                                $edt.find("#exportLinkBox").show();
                                $edt.find("#abortButton").hide();
                                $edt.find("#okButton").show();
                            } else {
                                timeoutId = setTimeout(ASC.CRM.SettingsPage.checkExportStatus, 3000);
                            }
                        }
                    },
                    error: function (params, errors) {
                        var err = errors[0];
                        if (err != null) {
                            toastr.error(err);
                        }
                    }
                });
        },

        abortExport: function () {
            Teamlab.cancelExportToCSV({},
                {
                    success: function (params, response) {
                        clearTimeout(timeoutId);
                        ASC.CRM.SettingsPage.closeExportProgressPanel();
                    },
                    error: function (params, errors) {
                        var err = errors[0];
                        if (err != null) {
                            toastr.error(err);
                        }
                    }
                });
        },

        closeExportProgressPanel: function () {
            var $edt = jq("#exportDataContent");
            $edt.find("div.progress").css("width", "0%");
            $edt.find("div.percent").text("0%");
            $edt.find("#abortButton").show();

            $edt.find("#okButton,div.progress-container,div.middle-button-container").hide();
            $edt.find("a.button.blue.middle").show();
            $edt.find("#exportErrorBox,#exportLinkBox,p.header-base-small").hide();

            $edt.find("div.progressErrorBox,#exportLinkBox span").html("");
        },

        buildErrorList: function(res) {
            var mess = "error";
            switch (typeof res.error) {
                case "object":
                    mess = res.error.Message + "<br/>";
                    break;
                case "string":
                    mess = res.error;
                    break;
            }

            jq("#exportDataContent div.progressErrorBox").html(
                jq("<div></div>").addClass("red-text").html(mess)
            );
            jq("#exportDataContent #exportErrorBox").show();
        }
    };
})();


ASC.CRM.ListItemView = (function() {

    var _initSortableElements = function() {
        jq("#listView").sortable({
            cursor: "move",
            handle: '.sort_drag_handle',
            items: 'li',
            start: function(event, ui) {
                if (ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3) {
                    jq(".iconsPanelSettings").hide();
                }
                if (ASC.CRM.ListItemView.CurrentType === 1) {
                    jq("#colorsPanel").hide();
                    jq("#popup_colorsPanel").hide();
                }
                jq("#listItemActionMenu").hide();
                jq("#listView .entity-menu.active").removeClass("active");
            },
            stop: function(event, ui) {
                if (ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3) {
                    var $activeImg = jq(ui.item).find("label.currentIcon");
                    if ($activeImg.length != 0) {
                        $activeImg.removeClass('active').removeClass('hover');
                    }
                }
            },

            beforeStop: function(event, ui) {
                jq(ui.item[0]).attr("style", "");
            },
            update: function(event, ui) {
                var itemsName = [];

                jq("#listView li[id*='list_item_id_']").each(function() {
                    var index = _findIndexOfItemByID(parseInt(jq(this).attr("id").replace("list_item_id_", "")));
                    if (index === -1) { return; }
                    itemsName.push(ASC.CRM.ListItemView.itemList[index].title);
                });
                Teamlab.reorderCrmListItems({}, ASC.CRM.ListItemView.CurrentType, itemsName, {});
            }
        });
    };

    var _initElementsActionMenu = function() {
        if (jq("#listItemActionMenu").length != 1) return;

        jq.dropdownToggle({
            dropdownID: "listItemActionMenu",
            switcherSelector: "#listView .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            showFunction: function(switcherObj, dropdownItem) {
                jq("#listView .entity-menu.active").removeClass("active");
                if (dropdownItem.is(":hidden")) {
                    switcherObj.addClass("active");
                    var listitemid = switcherObj.attr("data-listitemid"),
                        relativeCount = switcherObj.attr("data-relativecount");

                    if (relativeCount == "0") {
                        dropdownItem.find(".editItem.display-none").removeClass("display-none");
                    } else {
                        dropdownItem.find(".editItem").addClass("display-none");
                    }
                    
                    dropdownItem.attr("data-listitemid", listitemid);
                    dropdownItem.attr("data-relativecount", relativeCount);
                }
            },
            hideFunction: function() {
                jq("#listView .entity-menu.active").removeClass("active");
            }
        });
    };

    var _initOtherActionMenu = function() {
        jq("#menuCreateNewTask").bind("click", function() { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var _findIndexOfItemByID = function(id) {
        for (var i = 0, n = ASC.CRM.ListItemView.itemList.length; i < n; i++) {
            if (ASC.CRM.ListItemView.itemList[i].id == id) {
                return i;
            }
        }
        return -1;
    };

    var _resetManageItemPanel = function() {
        jq("#manageItem input").val("");
        jq("#manageItem textarea").val("");

        if (ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3) {
            var $iconsPanel = jq("#iconsPanel_" + ASC.CRM.ListItemView.CurrentType);
            if ($iconsPanel.length == 1) {
                var $first_icon = $iconsPanel.children('label:first');
                if ($first_icon.length == 1) {
                    var $selectedIcinObj = jq("#manageItem label.selectedIcon"),
                        cssClass = $first_icon.attr('data-imgName').split('.')[0];

                    $selectedIcinObj.attr('data-imgName', $first_icon.attr('data-imgName'));
                    $selectedIcinObj.attr('title', $first_icon.attr('title'));
                    $selectedIcinObj.attr("class", ["selectedIcon ", ASC.CRM.ListItemView.CurrentType == 2 ? "task_category" : "event_category", " ", cssClass].join(''));
                }
            }
        }
        if (ASC.CRM.ListItemView.CurrentType === 1) {
            var $divColors = jq("#colorsPanel > span"),
                ind = Math.floor(Math.random() * $divColors.length);
            jq("#manageItem .selectedColor").css("background-color", ASC.CRM.Common.getHexRGBColor(jq($divColors.get(ind)).css("background-color")));
        }
    };

    var _showAddItemPanel = function() {
        jq('#manageItem div.containerHeaderBlock td:first').text(ASC.CRM.ListItemView.AddItemHeaderText);
        jq('#manageItem .middle-button-container a.button.blue.middle').text(ASC.CRM.ListItemView.AddItemButtonText);

        _resetManageItemPanel();

        jq('#manageItem .middle-button-container a.button.blue.middle').unbind('click').click(function () {
            if (jq(this).hasClass("disable"))
                return;
            
            _createItem();
        });
        RemoveRequiredErrorClass(jq("#manageItem input:first"));
        PopupKeyUpActionProvider.CloseDialogAction = "javascript:jq('.iconsPanelSettings').hide();jq('#colorsPanel').hide();jq('#popup_colorsPanel').hide();";
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#manageItem", 400);

        if (!ASC.CRM.ListItemView.IsDropdownToggleRegistered) {
            ASC.CRM.ListItemView.IsDropdownToggleRegistered = true;
            if (ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3) {
                jq.dropdownToggle({
                    dropdownID: "popup_iconsPanel_" + ASC.CRM.ListItemView.CurrentType,
                    switcherSelector: "#manageItem .change_icon",
                    noActiveSwitcherSelector: "#listView label.currentIcon",
                    addTop: 1,
                    addLeft: jq("#manageItem .change_icon").width() - 12,
                    position: "fixed"
                });
            }
            if (ASC.CRM.ListItemView.CurrentType === 1) {
                jq.dropdownToggle({
                    dropdownID: "popup_colorsPanel",
                    switcherSelector: "#manageItem .change_color",
                    noActiveSwitcherSelector: "#listView .currentColor",
                    addTop: 2,
                    addLeft: jq("#manageItem .change_color").width() - 12,
                    position: "fixed"
                });
            }
        }
    };

    var _changeColor = function(Obj, listItemId, color) {
        var data = {
            id: listItemId,
            color: color
        };
        Teamlab.updateCrmContactStatusColor({ Obj: Obj }, listItemId, data,
            function(params, contactStatus) {
                var index = _findIndexOfItemByID(contactStatus.id);
                if (index === -1) { return; }
                ASC.CRM.ListItemView.itemList[index] = contactStatus;

                jq(params.Obj).css("background-color", contactStatus.color);
                jq("#colorsPanel").hide();
            }
        );
    };

    var _changeIcon = function(Obj, listItemId, $imgObj) {
        var imgName = $imgObj.attr("data-imgName"),
            data = {
                id: listItemId,
                imageName: imgName
            };

        Teamlab.updateCrmListItemIcon({ Obj: jq(Obj) }, ASC.CRM.ListItemView.CurrentType, listItemId, data, {
            before: function(params) {
                params.Obj.hide();
                params.Obj.parent().find("div.ajax_change_icon").show();
            },
            success: function(params, listItem) {
                var index = _findIndexOfItemByID(listItem.id);
                if (index === -1) { return; }

                _itemFactory(listItem);
                ASC.CRM.ListItemView.itemList[index] = listItem;

                params.Obj
                    .attr("class", ["currentIcon ", ASC.CRM.ListItemView.CurrentType == 2 ? "task_category" : "event_category", " ", listItem.cssClass].join(''))
                    .attr("data-imgName", listItem.imageName)
                    .attr("title", listItem.imageTitle);

                params.Obj.parent().find("div.ajax_change_icon").hide();

                jq(params.Obj).show();
                jq(".iconsPanelSettings").hide();
            }
        });
    };

    var _getIconByCssClass = function (cssClass) {
        if (typeof (cssClass) === "undefined" || cssClass == "") return null;

        var $icon = jq('#iconsPanel_' + ASC.CRM.ListItemView.CurrentType + ' label.' + cssClass);
        if ($icon.length != 1)  return null;

        return $icon;
    };

    var _itemFactory = function(item) {
        if (typeof (item.relativeItemsCount) === "undefined") {
            item.relativeItemsCount = 0;
        }
        item.relativeItemsString = ASC.CRM.Common.getRelativeItemsLinkString(item.relativeItemsCount, jq.getURLParam("type"), null);
        if (ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3) {
            if (item.hasOwnProperty("imagePath") && item.imagePath != "") {
                item.cssClass = jq.trim(item.imagePath.split('/')[item.imagePath.split('/').length - 1].split('.')[0]);
                var $icon = _getIconByCssClass(item.cssClass);

                if ($icon != null) {
                    item.imageTitle = $icon.attr('title');
                    item.imageName = $icon.attr('data-imgName');
                } else {
                    item.imageTitle = "";
                    item.imageName = "";
                }
                
            } else {
                item.imageTitle = "";
                item.imageName = "";
                item.cssClass= "";
            }
        }

        var hash = [],
            filtervaluehash = {},
            anchor = "";

        if (ASC.CRM.ListItemView.CurrentType == 1) { //Contact types
            //hash.push('{"id":"sorter","type":"sorter","params":"eyJpZCI6ImRpc3BsYXluYW1lIiwiZGVmIjp0cnVlLCJkc2MiOmZhbHNlLCJzb3J0T3JkZXIiOiJhc2NlbmRpbmcifQ=="}');

            filtervaluehash = {
                id: "contactStage",
                type: "combobox",
                params: jq.base64.encode(jq.toJSON({
                    value: item.id,
                    title: item.title
                }))
            };

            hash.push(jq.toJSON(filtervaluehash));
            anchor = jq.base64.encode(hash.join(';'));
            item.relativeItemsUrl = "Default.aspx#" + anchor;
        }
        if (ASC.CRM.ListItemView.CurrentType == 2) { //Task category
            //hash.push('{"id":"sorter","type":"sorter","params":"eyJpZCI6ImNhdGVnb3J5IiwiZGVmIjpmYWxzZSwiZHNjIjpmYWxzZSwic29ydE9yZGVyIjoiYXNjZW5kaW5nIn0="}');

            filtervaluehash = {
                id: "categoryID",
                type: "combobox",
                params: jq.base64.encode(jq.toJSON({
                    value: item.id,
                    title: item.title
                }))
            };

            hash.push(jq.toJSON(filtervaluehash));
            anchor = jq.base64.encode(hash.join(';'));
            item.relativeItemsUrl = "Tasks.aspx#" + anchor;
        }

        if (ASC.CRM.ListItemView.CurrentType == 4) { //Contact type
            //hash.push('{"id":"sorter","type":"sorter","params":"eyJpZCI6ImNhdGVnb3J5IiwiZGVmIjpmYWxzZSwiZHNjIjpmYWxzZSwic29ydE9yZGVyIjoiYXNjZW5kaW5nIn0="}');

            filtervaluehash = {
                id: "contactType",
                type: "combobox",
                params: jq.base64.encode(jq.toJSON({
                    value: item.id,
                    title: item.title
                }))
            };

            hash.push(jq.toJSON(filtervaluehash));
            anchor = jq.base64.encode(hash.join(';'));
            item.relativeItemsUrl = "Default.aspx#" + anchor;
        }
    };

    var _validateDataItem = function() {
        if (jq("#manageItem input:first").val().trim() == "") {
            ShowRequiredError(jq("#manageItem input:first"), true);
            return false;
        } else {
            RemoveRequiredErrorClass(jq("#manageItem input:first"));
            return true;
        }
    };

    var _readItemData = function(sortOrder) {
        var item = {
            title: jq("#manageItem input:first").val().trim(),
            sortOrder: sortOrder
        };
        if (ASC.CRM.ListItemView.CurrentType !== 4) {
            item.description = jq("#manageItem textarea").val().trim();
        }

        if (ASC.CRM.ListItemView.CurrentType === 1) {
            item.color = ASC.CRM.Common.getHexRGBColor(jq("#manageItem .selectedColor").css("background-color"));
        }
        if (ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3) {
            item.imageName = jq("#manageItem label.selectedIcon").attr('data-imgName');
        }

        return item;
    };

    var _createItem = function() {
        if (!_validateDataItem()) { return; }

        var $listItems = jq("#listView li"),
            item = _readItemData($listItems.length),
            index = 0;

        for (var i = 0, len = $listItems.length; i < len; i++) {
            if (jq($listItems[i]).find("td.item_title").text().trim() == item.title) {
                index = i + 1;
                break;
            }
        }
        if (index == 0) {
            Teamlab.addCrmListItem({}, ASC.CRM.ListItemView.CurrentType, item, {
                before: function (params) {
                    LoadingBanner.strLoading = ASC.CRM.ListItemView.AddItemProcessText;
                    LoadingBanner.showLoaderBtn("#manageItem");
                },
                after: function(params) {
                    LoadingBanner.hideLoaderBtn("#manageItem");
                },
                success: ASC.CRM.ListItemView.CallbackMethods.add_item
            });
        } else {
            ASC.CRM.Common.animateElement({
                element: jq($listItems[index - 1]),
                afterScrollFunc: jq.unblockUI()
            });
        }
    };

    var _editItem = function(liObj, id, indexOfItem) {
        if (!_validateDataItem()) return;

        var $listItems = jq("#listView li"),
            item = _readItemData(indexOfItem),
            index = 0;

        item.id = id;
        for (var i = 0, len = $listItems.length; i < len; i++) {
            if (jq($listItems[i]).find("td.item_title").text().trim() == item.title && jq($listItems[i]).not(jq(liObj)).length != 0) {
                index = i + 1;
                break;
            }
        }
        if (index == 0) {
            Teamlab.updateCrmListItem({ liObj: liObj, indexOfItem: indexOfItem }, ASC.CRM.ListItemView.CurrentType, item.id, item, {
                before: function (params) {
                    LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.SaveChangesProggress;
                    LoadingBanner.showLoaderBtn("#manageItem");
                },
                after: function(params) {
                    LoadingBanner.hideLoaderBtn("#manageItem");
                },
                success: ASC.CRM.ListItemView.CallbackMethods.edit_item
            });
        } else {
            ASC.CRM.Common.animateElement({
                element: jq($listItems[index - 1]),
                afterScrollFunc: jq.unblockUI()
            });
        }
    };

    var _initManagePanel = function () {
        jq.tmpl("template-blockUIPanel", {
            id: "manageItem",
            headerTest: ASC.CRM.ListItemView.AddItemHeaderText,
            questionText: "",
            innerHtmlText: jq.tmpl("listViewActionPanelBodyTmpl", { currentType: ASC.CRM.ListItemView.CurrentType }).html(),
            OKBtn: ASC.CRM.ListItemView.AddItemButtonText,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ""
        }).insertAfter("#listView");

        jq("#manageItem .button.gray").on("click", function () { PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI(); });
    };

    var _getAndRenderPageData = function (firstTime) {
        LoadingBanner.displayLoading();

        ASC.CRM.ListItemView.itemList = [];

        Teamlab.getCrmListItem({}, ASC.CRM.ListItemView.CurrentType, function (params, items) {
            for (var i = 0, len = items.length; i < len; i++) {
                _itemFactory(items[i]);
                ASC.CRM.ListItemView.itemList.push(items[i]);
            }

            jq.tmpl("listItemsTmpl", ASC.CRM.ListItemView.itemList).appendTo(jq("#listView").html(""));
            if (ASC.CRM.ListItemView.itemList.length == 1) {
                var $menuSwitcher = jq("#listView li [id^=list_item_menu_]:first");
                if ($menuSwitcher.attr("data-relativecount") != "0") {
                    $menuSwitcher.hide();
                } else {
                    jq("#listItemActionMenu .deleteItem").hide();
                }
            }

            if (firstTime === true) {
                _initSortableElements();
            }

            LoadingBanner.hideLoading();
        });

    };

    return {
        CallbackMethods: {
            delete_item: function(params, item) {
                params.liObj.remove();

                var index = _findIndexOfItemByID(item.id);
                if (index != -1) {
                    ASC.CRM.ListItemView.itemList.splice(index, 1);
                }

                if (jq("#listView li").length == 1) {
                    var $menuSwitcher = jq("#listView li [id^=list_item_menu_]:first");
                    if ($menuSwitcher.attr("data-relativecount") != "0") {
                        $menuSwitcher.hide();
                    } else {
                        jq("#listItemActionMenu .deleteItem").hide();
                    }
                }
            },

            add_item: function(params, item) {
                _itemFactory(item);
                ASC.CRM.ListItemView.itemList.push(item);

                var $itemHTML = jq.tmpl("listItemsTmpl", item);

                if (jq("#listView li").length == 1) {
                    jq("#listView li [id^=list_item_menu_]:first").show();
                    jq("#listItemActionMenu .deleteItem").show();
                }

                $itemHTML.appendTo("#listView");

                ASC.CRM.Common.animateElement({
                    element: $itemHTML,
                    afterScrollFunc: jq.unblockUI()
                });
            },

            edit_item: function(params, item) {
                _itemFactory(item);
                ASC.CRM.ListItemView.itemList[params.indexOfItem] = item;

                var $itemHTML = jq.tmpl("listItemsTmpl", item);
                jq(params.liObj).hide();
                $itemHTML.insertAfter(params.liObj);
                jq(params.liObj).remove();

                ASC.CRM.Common.animateElement({
                    element: $itemHTML,
                    afterScrollFunc: jq.unblockUI()
                });
            }
        },

        init: function (currentType, addItemHeaderText, addItemButtonText) {
            ASC.CRM.ListItemView.CurrentType = currentType;
            ASC.CRM.ListItemView.AddItemHeaderText = addItemHeaderText;
            ASC.CRM.ListItemView.AddItemButtonText = addItemButtonText;

            _getAndRenderPageData(true);

            _initManagePanel();
            _initElementsActionMenu();
            _initOtherActionMenu();

            if (jq("#cbx_ChangeContactStatusWithoutAsking").length == 1) {
                jq("#cbx_ChangeContactStatusWithoutAsking").bind("change", function () {
                    var changeContactStatusWithoutAsking = jq(this).is(":checked") ? true : null;
                    Teamlab.updateCRMContactStatusSettings({}, changeContactStatusWithoutAsking,
                        function () {
                        });
                });
            }

            jq("#createNewItem").bind("click", function() { _showAddItemPanel(); });

            ASC.CRM.ListItemView.IsDropdownToggleRegistered = false;
            if (ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3) {
                jq.dropdownToggle({
                    dropdownID: "iconsPanel_" + ASC.CRM.ListItemView.CurrentType,
                    switcherSelector: "#listView label.currentIcon",
                    noActiveSwitcherSelector: "#manageItem .change_icon",
                    addTop: 1,
                    addLeft: -4,
                    showFunction: function(switcherObj, dropdownItem) {
                        if (dropdownItem.is(":hidden")) {
                            switcherObj.addClass("active");
                        } else {
                            switcherObj.removeClass("active");
                        }
                    },
                    hideFunction: function() {
                        var $activeImg = jq("#listView label.currentIcon.active");
                        if ($activeImg.length != 0) {
                            $activeImg.removeClass("active");
                        }
                    }
                });
            }
            if (ASC.CRM.ListItemView.CurrentType === 1) {
                jq.dropdownToggle({
                    dropdownID: "colorsPanel",
                    switcherSelector: "#listView .currentColor",
                    noActiveSwitcherSelector: "#manageItem .change_color",
                    addTop: 2,
                    addLeft: -12
                });
            }

            if (ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3) {
                ASC.CRM.Common.registerChangeHoverStateByParent("label.currentIcon", "#listView .with-entity-menu");
                ASC.CRM.Common.registerChangeHoverStateByParent(".iconsPanelSettings>label", "");
            }
        },

        showEditItemPanel: function() {
            jq("#listItemActionMenu").hide();
            jq("#listView .entity-menu.active").removeClass("active");

            var index = 0,
                item = null,
                liObj = null,
                currIcon = null,
                listitemid = jq("#listItemActionMenu").attr("data-listitemid");
            if (typeof (listitemid) === "undefined" || listitemid == "") { return; }

            index = _findIndexOfItemByID(listitemid);
            if (index === -1) { return; }
            item = ASC.CRM.ListItemView.itemList[index];

            liObj = jq("#list_item_id_" + listitemid);
            if (liObj.length != 1) { return; }

            jq('#manageItem div.containerHeaderBlock td:first').text(ASC.CRM.ListItemView.EditItemHeaderText.replace('{0}', item.title));
            jq('#manageItem .middle-button-container a.button.blue.middle').text(ASC.CRM.Resources.CRMJSResource.SaveChanges);

            LoadingBanner.strLoading = ASC.CRM.ListItemView.AddItemProcessText;

            jq('#manageItem input:first').val(item.title);
            jq('#manageItem textarea').val(item.description);
            jq('#manageItem div.add_params input').val(item.additionalParams);

            currIcon = jq(liObj).find("label.currentIcon");
            if (currIcon.length == 1) {
                var $selectedIcinObj = jq("#manageItem label.selectedIcon");

                $selectedIcinObj.attr('data-imgName', item.imageName);
                $selectedIcinObj.attr('title', item.imageTitle);

                $selectedIcinObj.attr("class", ["selectedIcon ", ASC.CRM.ListItemView.CurrentType == 2 ? "task_category" : "event_category", " ", item.cssClass].join(''));
            } else {
                var currentColor = jq(liObj).find("div.currentColor").css("background-color");
                jq("#manageItem .selectedColor").css("background-color", currentColor);
            }

            jq('#manageItem .middle-button-container a.button.blue.middle').unbind('click').click(function () {
                _editItem(liObj, item.id, index);
            });
            RemoveRequiredErrorClass(jq("#manageItem input:first"));
            PopupKeyUpActionProvider.CloseDialogAction = "javascript:jq('.iconsPanelSettings').hide();jq('#colorsPanel').hide();jq('#popup_colorsPanel').hide();";
            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI("#manageItem", 400);

            if (!ASC.CRM.ListItemView.IsDropdownToggleRegistered) {
                ASC.CRM.ListItemView.IsDropdownToggleRegistered = true;
                if (ASC.CRM.ListItemView.CurrentType === 2 || ASC.CRM.ListItemView.CurrentType === 3) {
                    jq.dropdownToggle({
                        dropdownID: "popup_iconsPanel_" + ASC.CRM.ListItemView.CurrentType,
                        switcherSelector: "#manageItem .change_icon",
                        noActiveSwitcherSelector: "#listView label.currentIcon",
                        addTop: 1,
                        addLeft: jq("#manageItem .change_icon").width() - 12,
                        position: "fixed"
                    });
                }
                if (ASC.CRM.ListItemView.CurrentType === 1) {
                    jq.dropdownToggle({
                        dropdownID: "popup_colorsPanel",
                        switcherSelector: "#manageItem .change_color",
                        noActiveSwitcherSelector: "#listView .currentColor",
                        addTop: 2,
                        addLeft: jq("#manageItem .change_color").width() - 12,
                        position: "fixed"
                    });
                }
            }
        },

        deleteItem: function() {
            jq("#listItemActionMenu").hide();
            jq("#listView .entity-menu.active").removeClass("active");

            var listitemid = jq("#listItemActionMenu").attr("data-listitemid"),
                relativecount = jq("#listItemActionMenu").attr("data-relativecount");
            if (typeof (listitemid) === "undefined" || listitemid == "") { return; }

            var liObj = jq("#list_item_id_" + listitemid);
            if (liObj.length != 1) { return; }

            if (jq("#listView li").length == 1) {
                if (ASC.CRM.ListItemView.CurrentType == 1) {
                    toastr.error(jq.format(ASC.CRM.Resources.CRMJSResource.ErrorTheLastContactStage,
                        jq.trim(jq(liObj).find(".item_title").text())) + "\n" + ASC.CRM.Resources.CRMJSResource.PleaseRefreshThePage);
                    jq("#listView").find(".entity-menu").hide();
                    return;
                }
                if (ASC.CRM.ListItemView.CurrentType == 2) {
                    toastr.error(jq.format(ASC.CRM.Resources.CRMJSResource.ErrorTheLastTaskCategory,
                        jq.trim(jq(liObj).find(".item_title").text())) + "\n" + ASC.CRM.Resources.CRMJSResource.PleaseRefreshThePage);
                    jq("#listView").find(".entity-menu").hide();
                    return;
                }
                if (ASC.CRM.ListItemView.CurrentType == 3) {//HistoryCategory
                    toastr.error(jq.format(ASC.CRM.Resources.CRMJSResource.ErrorTheLastHistoryCategory,
                        jq.trim(jq(liObj).find(".item_title").text())) + "\n" + ASC.CRM.Resources.CRMJSResource.PleaseRefreshThePage);
                    jq("#listView").find(".entity-menu").hide();
                    return;
                }
                if (ASC.CRM.ListItemView.CurrentType == 4) {
                    toastr.error(jq.format(ASC.CRM.Resources.CRMJSResource.ErrorTheLastContactType,
                        jq.trim(jq(liObj).find(".item_title").text())) + "\n" + ASC.CRM.Resources.CRMJSResource.PleaseRefreshThePage);
                    jq("#listView").find(".entity-menu").hide();
                    return;
                }
                return;
            }

            if (ASC.CRM.ListItemView.CurrentType === 2 && relativecount != "0") {//For task categories with linked task

                if (jq("#selectItemForReplacePopUp").length == 0) {
                    jq.tmpl("template-blockUIPanel", {
                        id: "selectItemForReplacePopUp",
                        headerTest: ASC.CRM.Resources.CRMSettingResource.DeleteTaskCategoryConfirmation,
                        questionText: "",
                        innerHtmlText: "<div class=\"selectItemForReplaceContainer\"></div>",
                        progressText: ""
                    }).appendTo("#studioPageContent .mainPageContent .containerBodyBlock:first");

                    jq("#selectItemForReplacePopUp .selectItemForReplaceContainer").replaceWith(jq("#selectItemForReplacePopUpBody").removeClass("display-none"));
                }


                var selectedItem = { id: 0, title: "", cssClass: "" },
                    itemsForSelector = [];

                jq("#itemForReplaceSelector").remove();

                if (ASC.CRM.ListItemView.itemList.length > 0) {
                    for (var i = 0, n = ASC.CRM.ListItemView.itemList.length; i < n; i++) {
                        if (ASC.CRM.ListItemView.itemList[i].id != listitemid) {
                            itemsForSelector.push({
                                id: ASC.CRM.ListItemView.itemList[i].id,
                                title: ASC.CRM.ListItemView.itemList[i].title,
                                cssClass: "task_category " + ASC.CRM.ListItemView.itemList[i].cssClass
                            });
                        }
                    }
                    selectedItem = itemsForSelector[0];
                } else {
                    itemsForSelector.push(selectedItem);
                }

                window.itemForReplaceSelector = new ASC.CRM.CategorySelector("itemForReplaceSelector", selectedItem);
                window.itemForReplaceSelector.renderControl(itemsForSelector, selectedItem, "#itemForReplaceSelectorContainer", 0, "");

                jq("#deleteItemPopupOK").off("click").on("click", function () {
                    Teamlab.removeCrmListItem({ liObj: liObj }, ASC.CRM.ListItemView.CurrentType, listitemid, window.itemForReplaceSelector.CategoryID, {
                        before: function (params) {
                            params.liObj.find(".entity-menu").hide();
                            params.liObj.find("div.ajax_loader").show();

                            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.LoadingWait;
                            LoadingBanner.showLoaderBtn("#selectItemForReplacePopUp");
                        },
                        success: function (params, response) {
                            LoadingBanner.hideLoaderBtn("#selectItemForReplacePopUp");
                            jq.unblockUI();
                            _getAndRenderPageData(false);
                        }
                    });
                });

                StudioBlockUIManager.blockUI("#selectItemForReplacePopUp", 500);

            } else {
                Teamlab.removeCrmListItem({ liObj: liObj }, ASC.CRM.ListItemView.CurrentType, listitemid, 0, {
                    before: function (params) {
                        params.liObj.find(".entity-menu").hide();
                        params.liObj.find("div.ajax_loader").show();
                    },
                    success: ASC.CRM.ListItemView.CallbackMethods.delete_item
                });
            }
        },

        showColorsPanel: function(switcherUI) {
            jq("#colorsPanel > span").unbind("click").click(function() {
                _changeColor(switcherUI, jq(switcherUI).parents("li").get(0).id.replace("list_item_id_", "") * 1, ASC.CRM.Common.getHexRGBColor(jq(this).css("background-color")));
            });
        },

        showColorsPanelToSelect: function() {
            jq("#popup_colorsPanel > span").unbind("click").click(function() {
                jq("#manageItem .selectedColor").css("background", ASC.CRM.Common.getHexRGBColor(jq(this).css("background-color")));
                jq("#popup_colorsPanel").hide();
            });
        },

        showIconsPanel: function(switcherUI) {
            var $iconsPanel = jq("#iconsPanel_" + ASC.CRM.ListItemView.CurrentType);
            if ($iconsPanel.length != 1) return;
            $iconsPanel.children("label").unbind("click").click(function() {
                _changeIcon(switcherUI, jq(switcherUI).parents("li").get(0).id.replace("list_item_id_", "") * 1, jq(this));
            });
        },

        showIconsPanelToSelect: function() {
            var $popup_iconsPanel = jq("#popup_iconsPanel_" + ASC.CRM.ListItemView.CurrentType);
            if ($popup_iconsPanel.length != 1) return;
            $popup_iconsPanel.children("label").unbind("click").click(function() {
                var $selectedIcinObj = jq("#manageItem label.selectedIcon"),
                    cssClass = jq(this).attr('data-imgName').split('.')[0];

                $selectedIcinObj.attr('data-imgName', jq(this).attr('data-imgName'));
                $selectedIcinObj.attr('title', jq(this).attr('title'));
                $selectedIcinObj.attr('alt', jq(this).attr('alt'));
                $selectedIcinObj.attr("class", ["selectedIcon ", ASC.CRM.ListItemView.CurrentType == 2 ? "task_category" : "event_category", " ", cssClass].join(''));

                $popup_iconsPanel.hide();
            });
        }
    };
})();


ASC.CRM.DealMilestoneView = (function() {

    var _initSortableDealMilestones = function() {
        jq("#dealMilestoneList").sortable({
            cursor: "move",
            handle: ".sort_drag_handle",
            items: "li",
            start: function(event, ui) {
                jq("#colorsPanel").hide();
                jq("#popup_colorsPanel").hide();
                jq("#dealMilestoneActionMenu").hide();
                jq("#dealMilestoneList .entity-menu.active").removeClass("active");
            },
            beforeStop: function(event, ui) {
                jq(ui.item[0]).attr("style", "");
            },
            update: function(event, ui) {
                var itemsIDs = [];

                jq("#dealMilestoneList li[id^='deal_milestone_id_']").each(function() {
                    itemsIDs.push(jq(this).attr("id").replace("deal_milestone_id_", "") * 1);
                });

                Teamlab.reorderCrmDealMilestones({}, itemsIDs, {});
            }
        });
    };

    var _initDealMilestonesActionMenu = function() {

        jq.dropdownToggle({
            dropdownID: "dealMilestoneActionMenu",
            switcherSelector: "#dealMilestoneList .entity-menu",
            addTop: 0,
            addLeft: 10,
            rightPos: true,
            showFunction: function(switcherObj, dropdownItem) {
                jq("#dealMilestoneList .entity-menu.active").removeClass("active");
                if (dropdownItem.is(":hidden")) {
                    switcherObj.addClass("active");
                    if (switcherObj.attr("dealmilestoneid") != dropdownItem.attr("dealmilestoneid")) {
                        dropdownItem.attr('dealmilestoneid', switcherObj.attr("dealmilestoneid"));
                    }
                }
            },
            hideFunction: function() {
                jq("#dealMilestoneList .entity-menu.active").removeClass("active");
            }
        });
    };

    var _initOtherActionMenu = function() {
        jq("#menuCreateNewTask").bind("click", function() { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var _resetManageItemPanel = function() {
        jq("#manageDealMilestone .title").val("");
        jq("#manageDealMilestone .probability").val("0");
        jq("#manageDealMilestone textarea").val("");
        jq("#manageDealMilestone [name=deal_milestone_status]:first").prop("checked", true);

        var $divColors = jq("#colorsPanel > span"),
            ind = Math.floor(Math.random() * $divColors.length);
        jq("#manageDealMilestone .selectedColor").css("background-color", ASC.CRM.Common.getHexRGBColor(jq($divColors.get(ind)).css("background-color")));
    };

    var _showAddDealMilestonePanel = function() {
        jq('#manageDealMilestone div.containerHeaderBlock td:first').text(ASC.CRM.Resources.CRMSettingResource.CreateNewDealMilestone);
        jq('#manageDealMilestone .middle-button-container a.button.blue.middle').text(ASC.CRM.Resources.CRMSettingResource.AddThisDealMilestone);
        LoadingBanner.strLoading = ASC.CRM.Resources.CRMSettingResource.CreateDealMilestoneInProgressing;
        _resetManageItemPanel();

        jq('#manageDealMilestone .middle-button-container a.button.blue.middle').unbind('click').click(function () {
            if (jq(this).hasClass("disable"))
                return;

            _createDealMilestone();
        });
        RemoveRequiredErrorClass(jq("#manageDealMilestone .title"));

        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#manageDealMilestone", 400);

        if (!ASC.CRM.DealMilestoneView.IsDropdownToggleRegistered) {
            ASC.CRM.DealMilestoneView.IsDropdownToggleRegistered = true;
            jq.dropdownToggle({
                dropdownID: "popup_colorsPanel",
                switcherSelector: "#manageDealMilestone .change_color",
                noActiveSwitcherSelector: "#dealMilestoneList .currentColor",
                addTop: 2,
                addLeft: jq("#manageDealMilestone .change_color").width() - 12,
                position: "fixed"
            });
        }
    };

    var _findIndexOfDealMilestoneByID = function(id) {
        for (var i = 0, n = ASC.CRM.DealMilestoneView.dealMilestoneList.length; i < n; i++) {
            if (ASC.CRM.DealMilestoneView.dealMilestoneList[i].id == id) {
                return i;
            }
        }
        return -1;
    };

    var _changeColor = function(Obj, listItemId, color) {
        var data = {
            id: listItemId,
            color: color
        };
        Teamlab.updateCrmDealMilestoneColor({ Obj: Obj }, listItemId, data,
            function(params, dealMilestone) {
                _dealMilestoneItemFactory(dealMilestone);

                var index = _findIndexOfDealMilestoneByID(dealMilestone.id);
                if (index != -1) {
                    ASC.CRM.DealMilestoneView.dealMilestoneList[index] = dealMilestone;
                }

                jq(params.Obj).css("background", dealMilestone.color);
                jq("#colorsPanel").hide();
            }
        );
    };

    var _dealMilestoneItemFactory = function(dealMilestone) {
        if (typeof (dealMilestone.relativeItemsCount) === "undefined") {
            dealMilestone.relativeItemsCount = 0;
        }
        dealMilestone.relativeItemsString = ASC.CRM.Common.getRelativeItemsLinkString(dealMilestone.relativeItemsCount, jq.getURLParam("type"), null);
        var hash = [];
        hash.push('{"id":"sorter","type":"sorter","params":"eyJpZCI6InN0YWdlIiwiZGVmIjp0cnVlLCJkc2MiOmZhbHNlLCJzb3J0T3JkZXIiOiJhc2NlbmRpbmcifQ=="}');

        var filtervaluehash = {
            id: "opportunityStagesID",
            type: "combobox",
            params: jq.base64.encode(jq.toJSON({
                value: dealMilestone.id,
                title: dealMilestone.title
            }))
        };

        hash.push(jq.toJSON(filtervaluehash));
        var anchor = jq.base64.encode(hash.join(';'));
        dealMilestone.relativeItemsUrl = "Deals.aspx#" + anchor;
    };

    var _readDealMilestoneData = function(sortOrder) {
        var dealMilestone =
        {
            title: jq("#manageDealMilestone .title").val().trim(),
            sortOrder: sortOrder
        };

        dealMilestone.description = jq("#manageDealMilestone dl textarea").val().trim();
        dealMilestone.color = ASC.CRM.Common.getHexRGBColor(jq("#manageDealMilestone .selectedColor").css("background-color"));

        var percent = jq("#manageDealMilestone .probability").val();
        if (percent * 1 > 100) { percent = "100"; }

        dealMilestone.successProbability = percent;
        dealMilestone.stageType = jq("#manageDealMilestone [name=deal_milestone_status]:checked").val() * 1;

        return dealMilestone;
    };

    var _editDealMilestone = function(liObj, id) {
        if (jq("#manageDealMilestone .title").val().trim() == "") {
            ShowRequiredError(jq("#manageDealMilestone .title"), true);
            return;
        } else {
            RemoveRequiredErrorClass(jq("#manageDealMilestone .title"));
        }

        var $listItems = jq("#dealMilestoneList li"),
            dealMilestone = _readDealMilestoneData($listItems.length),
            index = 0;

        dealMilestone.id = id;

        for (var i = 0, len = $listItems.length; i < len; i++) {
            if (jq($listItems[i]).find("td.deal_milestone_title").text().trim() == dealMilestone.title && jq($listItems[i]).not(jq(liObj)).length != 0) {
                index = i + 1;
                break;
            }
        }
        if (index == 0) {
            Teamlab.updateCrmDealMilestone({ liObj: liObj }, dealMilestone.id, dealMilestone, {
                before: function () {
                    LoadingBanner.showLoaderBtn("#manageDealMilestone");
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn("#manageDealMilestone");
                },
                success: ASC.CRM.DealMilestoneView.CallbackMethods.edit_dealMilestone
            });
        } else {
            ASC.CRM.Common.animateElement({
                element: jq($listItems[index - 1]),
                afterScrollFunc: jq.unblockUI()
            });
        }
    };

    var _createDealMilestone = function() {
        if (jq("#manageDealMilestone .title").val().trim() == "") {
            ShowRequiredError(jq("#manageDealMilestone .title"), true);
            return;
        } else {
            RemoveRequiredErrorClass(jq("#manageDealMilestone .title"));
        }

        var $listItems = jq("#dealMilestoneList li"),
            dealMilestone = _readDealMilestoneData($listItems.length),
            index = 0;

        for (var i = 0, len = $listItems.length; i < len; i++) {
            if (jq($listItems[i]).find("td.deal_milestone_title").text().trim() == dealMilestone.title) {
                index = i + 1;
                break;
            }
        }

        if (index == 0) {
            Teamlab.addCrmDealMilestone({}, dealMilestone, {
                before: function () {
                    LoadingBanner.showLoaderBtn("#manageDealMilestone");
                },
                after: function () {
                    LoadingBanner.hideLoaderBtn("#manageDealMilestone");
                },
                success: ASC.CRM.DealMilestoneView.CallbackMethods.add_dealMilestone
            });
        } else {
            ASC.CRM.Common.animateElement({
                element: jq($listItems[index - 1]),
                afterScrollFunc: jq.unblockUI()
            });
        }
    };


    var _initManagePanel = function () {

        jq.tmpl("template-blockUIPanel", {
            id: "manageDealMilestone",
            headerTest: ASC.CRM.Resources.CRMCommonResource.Confirmation,
            questionText: "",
            innerHtmlText: jq.tmpl("dealMilestoneActionPanelBodyTmpl", {}).html(),
            OKBtn: ASC.CRM.Resources.CRMSettingResource.AddThisDealMilestone,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ""
        }).insertAfter("#dealMilestoneList");

        jq("#manageDealMilestone .button.gray").on("click", function () { PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI(); });
    };

    return {
        CallbackMethods: {
            add_dealMilestone: function(params, dealMilestone) {
                _dealMilestoneItemFactory(dealMilestone);
                ASC.CRM.DealMilestoneView.dealMilestoneList.push(dealMilestone);

                if (jq("#dealMilestoneList li").length == 1) {
                    jq(jq("#dealMilestoneActionMenu>.dropdown-content>li")[1]).show();
                }

                var $itemHTML = jq.tmpl("dealMilestoneTmpl", dealMilestone);
                $itemHTML.appendTo("#dealMilestoneList");

                ASC.CRM.Common.animateElement({
                    element: $itemHTML,
                    afterScrollFunc: jq.unblockUI()
                });
            },

            edit_dealMilestone: function(params, dealMilestone) {
                _dealMilestoneItemFactory(dealMilestone);

                var index = _findIndexOfDealMilestoneByID(dealMilestone.id);
                if (index != -1) {
                    ASC.CRM.DealMilestoneView.dealMilestoneList[index] = dealMilestone;
                }

                var $itemHTML = jq.tmpl("dealMilestoneTmpl", dealMilestone);

                params.liObj.hide();
                $itemHTML.insertAfter(params.liObj);
                params.liObj.remove();

                ASC.CRM.Common.animateElement({
                    element: $itemHTML,
                    afterScrollFunc: jq.unblockUI()
                });
            },

            delete_dealMilestone: function(params, dealMilestone) {
                params.liObj.remove();
                var index = _findIndexOfDealMilestoneByID(dealMilestone.id);
                if (index != -1) {
                    ASC.CRM.DealMilestoneView.dealMilestoneList.splice(index, 1);
                }
                if (jq("#dealMilestoneList li").length == 1) {
                    jq(jq("#dealMilestoneActionMenu>.dropdown-content>li")[1]).hide();
                }
            }
        },

        initData: function () {
            ASC.CRM.DealMilestoneView.dealMilestoneList = [];
            LoadingBanner.displayLoading();

            Teamlab.getCrmDealMilestones({}, function (params, dealMilestones) {
                for (var i = 0, n = dealMilestones.length; i < n; i++) {
                    _dealMilestoneItemFactory(dealMilestones[i]);
                    ASC.CRM.DealMilestoneView.dealMilestoneList.push(dealMilestones[i]);
                }
                jq.tmpl("dealMilestoneTmpl", ASC.CRM.DealMilestoneView.dealMilestoneList).appendTo("#dealMilestoneList");

                if (ASC.CRM.DealMilestoneView.dealMilestoneList.length == 1) {
                    jq(jq("#dealMilestoneActionMenu>.dropdown-content>li")[1]).hide();
                }

                _initSortableDealMilestones();
                LoadingBanner.hideLoading();
            });

        },

        dealMilestoneList: [],
        init: function () {
            ASC.CRM.DealMilestoneView.initData();

            _initManagePanel();
            _initDealMilestonesActionMenu();
            _initOtherActionMenu();
            jq("#createNewDealMilestone").bind("click", function() { _showAddDealMilestonePanel(); });

            ASC.CRM.DealMilestoneView.IsDropdownToggleRegistered = false;
            jq.dropdownToggle({
                dropdownID: "colorsPanel",
                switcherSelector: "#dealMilestoneList .currentColor",
                noActiveSwitcherSelector: "#manageDealMilestone .change_color",
                addTop: 2,
                addLeft: -12
            });

            jq.forceNumber({
                parent: "#manageDealMilestone",
                input: ".probability",
                integerOnly: true,
                positiveOnly: true
            });
        },

        showEditDealMilestonePanel: function() {
            jq("#dealMilestoneActionMenu").hide();
            jq("#dealMilestoneList .entity-menu.active").removeClass("active");

            var dealmilestoneid = jq("#dealMilestoneActionMenu").attr("dealmilestoneid");
            if (typeof (dealmilestoneid) === "undefined" || dealmilestoneid == "") { return; }

            var liObj = jq("#deal_milestone_id_" + dealmilestoneid);
            if (liObj.length != 1) { return; }

            var index = _findIndexOfDealMilestoneByID(dealmilestoneid);
            if (index === -1) { return; }
            var dealMilestone = ASC.CRM.DealMilestoneView.dealMilestoneList[index];


            jq('#manageDealMilestone div.containerHeaderBlock td:first').text(ASC.CRM.Resources.CRMSettingResource.EditSelectedDealMilestone.replace('{0}', dealMilestone.title));
            jq('#manageDealMilestone .middle-button-container a.button.blue.middle').text(ASC.CRM.Resources.CRMJSResource.SaveChanges);
            LoadingBanner.strLoading = ASC.CRM.Resources.CRMCommonResource.SaveChangesProggress;

            jq('#manageDealMilestone .title').val(dealMilestone.title);
            jq('#manageDealMilestone textarea').val(dealMilestone.description);
            jq('#manageDealMilestone .probability').val(dealMilestone.successProbability);
            jq("#manageDealMilestone [name=deal_milestone_status][value=" + dealMilestone.stageType + "]").prop("checked", true);

            jq('#manageDealMilestone .middle-button-container a.button.blue.middle').unbind('click').click(function () {
                _editDealMilestone(liObj, dealMilestone.id);
            });
            jq("#manageDealMilestone .selectedColor").css("background-color", dealMilestone.color);

            RemoveRequiredErrorClass(jq("#manageDealMilestone .title"));
            PopupKeyUpActionProvider.EnableEsc = false;
            StudioBlockUIManager.blockUI("#manageDealMilestone", 400);

            if (!ASC.CRM.DealMilestoneView.IsDropdownToggleRegistered) {
                ASC.CRM.DealMilestoneView.IsDropdownToggleRegistered = true;
                jq.dropdownToggle({
                    dropdownID: "popup_colorsPanel",
                    switcherSelector: "#manageDealMilestone .change_color",
                    noActiveSwitcherSelector: "#dealMilestoneList .currentColor",
                    addTop: 2,
                    addLeft: jq("#manageDealMilestone .change_color").width() - 12,
                    position: "fixed"
                });
            }
        },

        deleteDealMilestone: function() {
            jq("#dealMilestoneActionMenu").hide();
            jq("#dealMilestoneList .entity-menu.active").removeClass("active");

            var dealmilestoneid = jq("#dealMilestoneActionMenu").attr("dealmilestoneid");
            if (typeof (dealmilestoneid) === "undefined" || dealmilestoneid == "") { return; }

            var liObj = jq("#deal_milestone_id_" + dealmilestoneid);
            if (liObj.length != 1) { return; }

            if (jq("#dealMilestoneList li").length == 1) {
                toastr.error(ASC.CRM.Resources.CRMJSResource.ErrorTheLastDealMilestone);
                return;
            }

            Teamlab.removeCrmDealMilestone({ liObj: liObj }, dealmilestoneid, {
                before: function(params) {
                    params.liObj.find(".entity-menu").hide();
                    params.liObj.find("div.ajax_loader").show();
                },
                success: ASC.CRM.DealMilestoneView.CallbackMethods.delete_dealMilestone
            });
        },

        showColorsPanel: function(switcherUI) {
            jq("#colorsPanel > span").unbind("click").click(function() {
                _changeColor(switcherUI, jq(switcherUI).parents('li').get(0).id.replace("deal_milestone_id_", "") * 1, ASC.CRM.Common.getHexRGBColor(jq(this).css("background-color")));
            });
        },

        showColorsPanelToSelect: function() {
            jq("#popup_colorsPanel > span").unbind("click").click(function() {
                jq("#manageDealMilestone .selectedColor").css("background", ASC.CRM.Common.getHexRGBColor(jq(this).css("background-color")));
                jq("#popup_colorsPanel").hide();
            });
        }
    };
})();


ASC.CRM.TagSettingsView = (function() {

    var _initOtherActionMenu = function() {
        if (jq("#otherActions").length == 1) {
            jq.tmpl("deleteUnusedTagsButtonTmpl").appendTo("#otherActions ul.dropdown-content");
            jq("#deleteUnusedTagsButton").bind("click", function() { _deleteUnusedTags(); });
        }

        jq("#menuCreateNewTask").bind("click", function() { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var _showAddTagPanel = function() {
        jq("#tagTitle").val("");
        RemoveRequiredErrorClass(jq("#tagTitle"));
        PopupKeyUpActionProvider.EnableEsc = false;
        StudioBlockUIManager.blockUI("#manageTag", 400);
    };

    var _deleteUnusedTags = function() {
        jq("#otherActions").hide();
        jq("#deleteUnusedTagsButton").parent().addClass("display-none");

        Teamlab.removeCrmUnusedTag({}, _getCurEntityType(), {
            success: ASC.CRM.TagSettingsView.CallbackMethods.delete_unused_tags
        });
    };

    var _tagItemFactory = function(tag) {
        var type = _getCurEntityType();

        tag.relativeItemsString = ASC.CRM.Common.getRelativeItemsLinkString(tag.relativeItemsCount, jq.getURLParam("type"), type);

        var hash = [],
            filtervaluehash = {},
            anchor = "",
            baseUrl = "Default.aspx";
        //hash.push('{"id":"sorter","type":"sorter","params":"eyJpZCI6InRpdGxlIiwiZGVmIjp0cnVlLCJkc2MiOmZhbHNlLCJzb3J0T3JkZXIiOiJhc2NlbmRpbmcifQ=="}');

        filtervaluehash = {
            id: "tags",
            type: "combobox",
            params: jq.base64.encode(jq.toJSON({
                value: [ASC.CRM.Common.convertText(tag.title,false)],
                title: ASC.CRM.Common.convertText(tag.title,false)
            }))
        };

        hash.push(jq.toJSON(filtervaluehash));
        anchor = jq.base64.encode(hash.join(';'));

        if (type == "contacts") {
            baseUrl = "Default.aspx";
        } else if (type == "opportunity") {
            baseUrl = "Deals.aspx";
        } else if (type == "case") {
            baseUrl = "Cases.aspx";
        }
        tag.relativeItemsUrl = baseUrl + "#" + anchor;
    };

    var _findIndexOfTagByName = function(name) {
        for (var i = 0, n = ASC.CRM.TagSettingsView.tagList.length; i < n; i++) {
            if (ASC.CRM.TagSettingsView.tagList[i].title == name) {
                return i;
            }
        }
        return -1;
    };

    var _doUnusedTagsExist = function() {
        for (var i = 0, n = ASC.CRM.TagSettingsView.tagList.length; i < n; i++) {
            if (ASC.CRM.TagSettingsView.tagList[i].relativeItemsCount == 0) {
                return true;
            }
        }
        return false;
    };
  
    var _initEmptyScreen = function () {
        jq.tmpl("template-emptyScreen",
            {
                ID: "emptyTagContent",
                ImgSrc: ASC.CRM.Data.EmptyScrImgs["empty_screen_tags"],
                Header: ASC.CRM.Resources.CRMSettingResource.EmptyContentTags,
                Describe: ASC.CRM.Resources.CRMSettingResource.EmptyContentTagsDescript,
                ButtonHTML: ['<a id="addTag" class="link dotline plus">', ASC.CRM.Resources.CRMSettingResource.CreateNewTag, '</a>'].join(''),
                CssClass: "display-none"
            }).insertAfter("#tagList");
    };

    var _initTabs = function (type) {

        jq("<div class='fakeTabContainer' id='contTagsFake'></div>\
            <div class='fakeTabContainer' id='dealTagsFake'></div>\
            <div class='fakeTabContainer' id='caseTagsFake'></div>").insertAfter("#TagSettingsTabs");
        window.ASC.Controls.ClientTabsNavigator.init("TagSettingsTabs", {
            tabs: [
            {
                title: ASC.CRM.Resources.CRMSettingResource.BothPersonAndCompany,
                selected: type === "contact",
                anchor: "contact",
                divID: "contTagsFake",
                onclick: "ASC.CRM.TagSettingsView.initData('contact');"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.DealModuleName,
                selected: type === "opportunity",
                anchor: "opportunity",
                divID: "dealTagsFake",
                onclick: "ASC.CRM.TagSettingsView.initData('opportunity');"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.CasesModuleName,
                selected: type === "case",
                anchor: "case",
                divID: "caseTagsFake",
                onclick: "ASC.CRM.TagSettingsView.initData('case');"
            }]
        });
    };

    var _getCurEntityType = function () {
        var available = ["contact", "opportunity", "case"],
            type = ASC.Controls.AnchorController.getAnchor();
        if (type == null || type == "" || available.indexOf(type) == -1) { type = "contact"; }
        return type;
    };

    var _initManagePanel = function () {

        jq.tmpl("template-blockUIPanel", {
            id: "manageTag",
            headerTest: ASC.CRM.Resources.CRMSettingResource.CreateNewTag,
            questionText: "",
            innerHtmlText: jq.tmpl("tagSettingsActionPanelBodyTmpl", {}).html(),
            OKBtn: ASC.CRM.Resources.CRMSettingResource.AddThisTag,
            CancelBtn: ASC.CRM.Resources.CRMCommonResource.Cancel,
            progressText: ""
        }).insertAfter("#tagList");

        jq("#manageTag .button.gray").on("click", function () { PopupKeyUpActionProvider.EnableEsc = true; jq.unblockUI(); });
        jq("#manageTag .button.blue").on("click", function () {
            if (jq(this).hasClass("disable"))
                return;

            ASC.CRM.TagSettingsView.createTag();
        });
    };

    return {
        CallbackMethods: {
            add_tag: function(params, tag) {
                tag = { title: tag, relativeItemsCount : 0 };
                _tagItemFactory(tag);

                ASC.CRM.TagSettingsView.tagList.push(tag);

                if (jq("#deleteUnusedTagsButton").length == 1) {
                    jq("#deleteUnusedTagsButton").parent().removeClass("display-none");
                }

                var $itemHTML = jq.tmpl("tagRowTemplate", tag);
                if (params.tagsCount == 0) {
                    jq("#emptyTagContent").addClass("display-none");
                    jq("#tagList").show();
                    jq("#createNewTagSettings.display-none").removeClass("display-none");
                    jq(".cbx_AddTagWithoutAskingContainer.display-none").removeClass("display-none");
                }
                $itemHTML.appendTo("#tagList");

                ASC.CRM.Common.animateElement({
                    element: $itemHTML,
                    afterScrollFunc: jq.unblockUI()
                });
            },

            delete_tag: function(params, tag) {
                params.liObj.remove();

                var index = _findIndexOfTagByName(tag);
                if (index != -1) {
                    ASC.CRM.TagSettingsView.tagList.splice(index, 1);
                }
                if (jq("#deleteUnusedTagsButton").length == 1 && !_doUnusedTagsExist()) {
                    jq("#deleteUnusedTagsButton").parent().addClass("display-none");
                }

                if (jq("#tagList li").length == 0) {
                    jq("#tagList").hide();
                    jq("#createNewTagSettings:not(.display-none)").addClass("display-none");
                    jq(".cbx_AddTagWithoutAskingContainer:not(.display-none)").addClass("display-none");
                    if (jq("#deleteUnusedTagsButton").length == 1) {
                        jq("#deleteUnusedTagsButton").parent().addClass("display-none");
                    }
                    jq("#emptyTagContent").removeClass("display-none");
                }
            },

            delete_unused_tags: function(params, tags) {
                var $listTags = jq("#tagList li");

                for (var j = 0, n = tags.length; j < n; j++) {
                    for (var i = 0, m = $listTags.length; i < m; i++) {
                        if (jq($listTags[i]).find(".title").text().trim() == tags[j]) {
                            jq($listTags[i]).remove();
                        }
                    }
                    var index = _findIndexOfTagByName(tags[j]);
                    if (index != -1) {
                        ASC.CRM.TagSettingsView.tagList.splice(index, 1);
                    }
                }

                if (jq("#deleteUnusedTagsButton").length == 1 && !_doUnusedTagsExist()) {
                    jq("#deleteUnusedTagsButton").parent().addClass("display-none");
                }

                if (jq("#tagList li").length == 0) {
                    jq("#tagList").hide();
                    jq("#createNewTagSettings:not(.display-none)").addClass("display-none");
                    jq(".cbx_AddTagWithoutAskingContainer:not(.display-none)").addClass("display-none");
                    if (jq("#deleteUnusedTagsButton").length == 1) {
                        jq("#deleteUnusedTagsButton").parent().addClass("display-none");
                    }
                    jq("#emptyTagContent").removeClass("display-none");
                }
            }
        },


        initData: function (type) {
            ASC.CRM.TagSettingsView.tagList = [];
            jq("#tagList").html("").hide();
            jq("#emptyTagContent:not(.display-none)").addClass("display-none");
            jq(".cbx_AddTagWithoutAskingContainer:not(.display-none)").addClass("display-none");

            LoadingBanner.displayLoading();

            Teamlab.getCrmEntityTags({}, type, function (params, tags) {
                for (var i = 0, len = tags.length; i < len; i++) {
                    _tagItemFactory(tags[i]);
                    ASC.CRM.TagSettingsView.tagList.push(tags[i]);
                }

                if (ASC.CRM.TagSettingsView.tagList.length != 0) {
                    jq.tmpl("tagRowTemplate", ASC.CRM.TagSettingsView.tagList).appendTo("#tagList");
                    jq("#tagList").show();
                    jq("#createNewTagSettings.display-none").removeClass("display-none");
                    if (type === "contact") {
                        jq(".cbx_AddTagWithoutAskingContainer.display-none").removeClass("display-none");
                    }
                } else {
                    jq("#createNewTagSettings:not(.display-none)").addClass("display-none");
                    jq(".cbx_AddTagWithoutAskingContainer:not(.display-none)").addClass("display-none");
                    jq("#emptyTagContent").removeClass("display-none");
                }
                if (jq("#deleteUnusedTagsButton").length == 1) {
                    if (!_doUnusedTagsExist()) {
                        jq("#deleteUnusedTagsButton").parent().addClass("display-none");
                    } else {
                        jq("#deleteUnusedTagsButton").parent().removeClass("display-none");
                    }
                }
                LoadingBanner.hideLoading();
            });

        },

        tagList: [],
        init: function (alreadyExistsErrorText) {
            //jq("#tagList").disableSelection();

            var type = _getCurEntityType();
            _initTabs(type);

            _initOtherActionMenu();
            _initEmptyScreen();
            _initManagePanel();

            jq("#createNewTagSettings").on("click", function() { _showAddTagPanel(); });
            jq("#emptyTagContent .emptyScrBttnPnl > a").on("click", function() { _showAddTagPanel(); });

            ASC.CRM.TagSettingsView.initData(type);

            if (jq("#cbx_AddTagWithoutAsking").length == 1) {
                jq("#cbx_AddTagWithoutAsking").bind("change", function () {
                    var addTagWithoutAsking = jq(this).is(":checked") ? true : null;
                    Teamlab.updateCRMContactTagSettings({}, addTagWithoutAsking,
                        function () {
                        });
                });
            }
        },

        createTag: function () {
            var tagTitle = jq.trim(jq("#tagTitle").val());
            RemoveRequiredErrorClass(jq("#tagTitle"));

            if (tagTitle == "") {
                AddRequiredErrorText(jq("#tagTitle"), ASC.CRM.Resources.CRMSettingResource.EmptyLabelError);
                ShowRequiredError(jq("#tagTitle"), true);
                return;
            }

            var $listTags = jq("#tagList li"),
                tagsCount = $listTags.length;

            for (var i = 0; i < tagsCount; i++) {
                if (jq($listTags[i]).find(".title").text().trim() == tagTitle) {
                    AddRequiredErrorText(jq("#tagTitle"), ASC.CRM.Resources.CRMSettingResource.TagAlreadyExistsError);
                    ShowRequiredError(jq("#tagTitle"), true);
                    return;
                }
            }

            Teamlab.addCrmEntityTag({ tagsCount: tagsCount }, _getCurEntityType(), tagTitle, {
                before: function(params) {
                    LoadingBanner.showLoaderBtn("#manageTag");
                },
                success: ASC.CRM.TagSettingsView.CallbackMethods.add_tag,
                after: function() {
                    LoadingBanner.hideLoaderBtn("#manageTag");
                }
            });

        },

        deleteTag: function(deleteLink) {
            var liObj = jq(jq(deleteLink).parents('li').get(0)),
                tagTitle = liObj.find(".title").text().trim();

            Teamlab.removeCrmEntityTag({ liObj: liObj }, _getCurEntityType(), tagTitle, {
                before: function(params) {
                    params.liObj.find(".crm-deleteLink").hide();
                    params.liObj.find(".ajax_loader").show();
                },
                success: ASC.CRM.TagSettingsView.CallbackMethods.delete_tag
            });
        }
    };
})();


ASC.CRM.SettingsPage.WebToLeadFormView = (function() {
    var _initOtherActionMenu = function() {

        jq("#menuCreateNewTask").bind("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    function _renderTable(container) {
        var counter = 0,
            row,
            cell,
            input,
            label;
        var indexRow = 0,
            isCompany = jq("input[name=radio]:checked").val() == "company",
            serverData = isCompany ?
                jQuery.extend(true, [], ASC.CRM.Data.columnSelectorDataCompany) :
                jQuery.extend(true, [], ASC.CRM.Data.columnSelectorDataPerson),
            residue = serverData.length % 3 > 0 ? 3 - serverData.length % 3 : 0;

        jq(serverData).each(function() {
            if (!isCompany && (this.name == "firstName" || this.name == "lastName")) {
                this.disabled = true;
            } else if (isCompany && this.name == "companyName") {
                this.disabled = true;
            } else {
                this.disabled = false;
            }
        });

        for (var i = 0; i < serverData.length + residue; i++) {
            if (counter == 0) {
                row = jq("<tr></tr>");
            }
            cell = jq("<td></td>").attr("width", "33%");

            if (serverData[i] != null) {
                input = jq("<input>")
                                .attr("type", "checkbox")
                                .attr("id", serverData[i].name)
                                .attr("name", serverData[i].name);

                if (serverData[i].disabled) {
                    jq(input).prop("checked", true).prop("disabled", true);
                }

                label = jq("<label>").attr("for", serverData[i].name).text(serverData[i].title);
                input.data("fieldInfo", serverData[i]);
                cell.append(input).append(label);
            }

            row.append(cell);
            counter++;

            if (counter == 3 || i == serverData.length + residue - 1) {
                container.append(row);
                counter = 0;
                indexRow++;
            }
        }
    };

    function _validateInputData() {

        var regexp = /(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;

        var returnURL = jq("#returnURL").val().trim(),
            webFormKey = jq("#properties_webFormKey input").val().trim(),
            isValid = true;

        if (returnURL == "" || !regexp.test(returnURL)) {
            ShowRequiredError(jq("#returnURL"));
            isValid = false;
        } else {
            RemoveRequiredErrorClass(jq("#returnURL"));
        }

        if (webFormKey == "") {
            ShowRequiredError(jq("#properties_webFormKey input"));
            isValid = false;
        } else {
            RemoveRequiredErrorClass(jq("#properties_webFormKey input"));
        }

        var firstName = jq("#tblFieldList input[name='firstName']:checked"),
            lastName = jq("#tblFieldList input[name='lastName']:checked"),
            companyName = jq("#tblFieldList input[name='companyName']:checked");

        if ((firstName.length == 0 || lastName.length == 0) && companyName.length == 0) {
            toastr.error(ASC.CRM.Resources.CRMJSResource.ErrorNotMappingBasicColumn);
            isValid = false;
        }

        return isValid;
    };

    return {
        init: function (webtoleadfromhandlerPath) {
            webtoleadfromhandlerPath = webtoleadfromhandlerPath.replace("http:", "").replace("https:", "");
            ASC.CRM.SettingsPage.WebToLeadFormView.webtoleadfromhandlerPath = webtoleadfromhandlerPath;

            jq.tmpl("tagViewTmpl",
                    {
                        tags          : [],
                        availableTags : ASC.CRM.Data.tagList
                    })
                    .appendTo("#wtlfTags");
            ASC.CRM.TagView.init("contact", true);


            ASC.CRM.UserSelectorListView.Init(
                    "_ContactManager",
                    "UserSelectorListView_ContactManager",
                    false,
                    "",
                    new Array(ASC.CRM.Resources.CurrentUser),
                    null,
                    new Array(Teamlab.profile.id),
                    "#wtlfAccessRights",
                    false);


            var $html = jq.tmpl("makePublicPanelTemplate",
                {
                    Title: ASC.CRM.Resources.CRMContactResource.MakePublicPanelTitleForImportContacts,
                    Description: ASC.CRM.Resources.CRMContactResource.MakePublicPanelDescrForContact,
                    IsPublicItem: false,
                    CheckBoxLabel: ASC.CRM.Resources.CRMContactResource.MakePublicPanelCheckBoxLabelForImportContacts
                });

            $html.find(".makePublicPanelSelector")
                .append(jq("<option value='2'></option>").text(ASC.CRM.Resources.CRMCommonResource.AccessRightsForReading))
                .append(jq("<option value='1'></option>").text(ASC.CRM.Resources.CRMCommonResource.AccessRightsForReadWriting))
                .val("2")
                .tlCombobox();

            $html.appendTo("#wtlfMakePublicPanel");
            //isPublic


            ASC.CRM.UserSelectorListView.Init(
                                "_Notify",
                                "UserSelectorListView_Notify",
                                false,
                                "",
                                [],
                                null,
                                [],
                                "#userSelectorListViewContainer",
                                false);

            for (var i = 0, n = ASC.CRM.Data.columnSelectorDataCompany.length; i < n; i++) {
                var field = ASC.CRM.Data.columnSelectorDataCompany[i];
                if (jq.trim(field.mask) != "") {
                    field.mask = jq.parseJSON(field.mask);
                }
            }
            for (var i = 0, n = ASC.CRM.Data.columnSelectorDataPerson.length; i < n; i++) {
                var field = ASC.CRM.Data.columnSelectorDataPerson[i];
                if (jq.trim(field.mask) != "") {
                    field.mask = jq.parseJSON(field.mask);
                }
            }
            _renderTable(jq("#tblFieldList tbody"));

            jq("#privateSettingsBlock").parent().css("padding", "0px");
            var text = jq("#privateSettingsBlock").parent().find("span:first").text() + ":";
            jq("#privateSettingsBlock").parent().find("span:first").replaceWith(
                jq("<div><div>").text(text).css("font-weight", "bold")
            );

            _initOtherActionMenu();
        },

        changeContactType: function() {
            jq("#tblFieldList tbody").html("");

            _renderTable(jq("#tblFieldList tbody"));
        },

        changeWebFormKey: function() {

            if (!confirm(ASC.CRM.Resources.CRMJSResource.ConfirmChangeKey + "\n" + ASC.CRM.Resources.CRMJSResource.ConfirmChangeKeyNote))
            { return false; }

            Teamlab.updateWebToLeadFormKey({}, {
                success: function (params, response) {
                    jq("#properties_webFormKey input").val(response);
                    jq("#webFormKeyContainer").html(response);
                },
                error: function (params, errors) {
                    var err = errors[0];
                    if (err != null) {
                        toastr.error(err);
                    }
                }
            });
        },

        generateSampleForm: function() {
            if (!_validateInputData()) { return; }

            var fieldListInfo = jQuery.map(jq("#tblFieldList input:checked"),
                   function(a) {
                       return jq(a).data("fieldInfo");
                   });
            var tagListInfo = jq.map(jq("#tagContainer .tag_title"),
                   function(item) {
                       return {
                           name: "tag_" + jq(item).text().trim(),
                           title: jq(item).text().trim()
                       };
                   });

            if (fieldListInfo.length == 0) {
                jq("#resultContainer, #previewHeader").hide();
                jq("#previewHeader div.content").html("");
                return;
            }

            var notifyList = [],
                managersList = new Array(window.SelectedUsers_ContactManager.CurrentUserID);

            for (var i = 0, n = window.SelectedUsers_ContactManager.IDs.length; i < n; i++) {
                managersList.push(window.SelectedUsers_ContactManager.IDs[i]);
            }
            managersList = managersList.join(",");

            if (window.SelectedUsers_Notify.IDs.length != 0) {
                for (var i = 0, n = window.SelectedUsers_Notify.IDs.length; i < n; i++) {
                    notifyList.push(window.SelectedUsers_Notify.IDs[i]);
                }
                notifyList = notifyList.join(",");
            } else {
                notifyList = "";
            }

            var formContainer = jq("<div>");

            jq.tmpl("sampleFormTmpl", {
                webtoleadfromhandlerPath: ASC.CRM.SettingsPage.WebToLeadFormView.webtoleadfromhandlerPath,
                fieldListInfo: fieldListInfo,
                tagListInfo: tagListInfo,
                returnURL: jq("#returnURL").val(),
                webFormKey: jq("#properties_webFormKey input").val(),
                notifyList: notifyList,
                managersList: managersList,
                shareType: jq("#isPublic").is(":checked") ? jq(".makePublicPanel select.makePublicPanelSelector").val() : 0,
                isCompany: jq("input[name=radio]:checked").val() == "company"
            }).appendTo(formContainer);

            var sampleFrom = formContainer.html().replace(/\s{1,}/g, " ");

            jq("#resultContainer textarea").val(sampleFrom);
            jq("#resultContainer").show();
            jq("#previewHeader div.content").html(sampleFrom);
            jq("#previewHeader").show();

        }
    };
})();


ASC.CRM.TaskTemplateView = (function() {

    var _initOtherActionMenu = function() {
        jq("#menuCreateNewTask").bind("click", function() { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    };

    var _getCurEntityType = function () {
        var available = ["contact", "person", "company", "opportunity", "case"],
            type = jq.getURLParam("view");
        if (type == null || type == "" || available.indexOf(type) == -1) { type = "contact"; }
        return type;
    };

    var _initTabs = function (type) {

        jq("<div class='fakeTabContainer' id='contTagsFake'></div>\
            <div class='fakeTabContainer' id='compTagsFake'></div>\
            <div class='fakeTabContainer' id='persTagsFake'></div>\
            <div class='fakeTabContainer' id='dealTagsFake'></div>\
            <div class='fakeTabContainer' id='caseTagsFake'></div>").insertAfter("#TaskTemplateViewTabs");

        window.ASC.Controls.ClientTabsNavigator.init("TaskTemplateViewTabs", {
            tabs: [
            {
                title: ASC.CRM.Resources.CRMSettingResource.BothPersonAndCompany,
                selected: type === "contact",
                divID: "contTagsFake",
                href: "Settings.aspx?type=task_template&view=contact"
            },
            {
                title: ASC.CRM.Resources.CRMSettingResource.JustForCompany,
                selected: type === "company",
                divID: "contTagsFake",
                href: "Settings.aspx?type=task_template&view=company"
            },
            {
                title: ASC.CRM.Resources.CRMSettingResource.JustForPerson,
                selected: type === "person",
                divID: "contTagsFake",
                href: "Settings.aspx?type=task_template&view=person"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.DealModuleName,
                selected: type === "opportunity",
                divID: "dealTagsFake",
                href: "Settings.aspx?type=task_template&view=opportunity"
            },
            {
                title: ASC.CRM.Resources.CRMCommonResource.CasesModuleName,
                selected: type === "case",
                divID: "caseTagsFake",
                href: "Settings.aspx?type=task_template&view=case"
            }]
        });
    };


    function checkTemplateConatainers() {
        if (jq("#templateConatainerContent li").length != 0) {
            jq("#emptyContent").hide();
            jq("#templateConatainerContent").show();
        } else {
            jq("#templateConatainerContent").hide();
            jq("#emptyContent").show();
        }
    };

    return {
        init: function () {

            var type = _getCurEntityType();
            _initTabs(type);


            if (typeof (window.templateConatainerList) != "undefined") {
                window.templateConatainerList = jq.parseJSON(jQuery.base64.decode(window.templateConatainerList)).response;
            } else {
                window.templateConatainerList = [];
            }

            jq.forceNumber({
                parent: "#templatePanel",
                input: "#tbxTemplateDisplacement",
                integerOnly: true,
                positiveOnly: true
            });

            jq.tmpl("templateContainerRow", window.templateConatainerList).appendTo("#templateConatainerContent");
            checkTemplateConatainers();

            _initOtherActionMenu();

            for (var i = 0, n = window.templateConatainerList.length; i < n; i++) {
                if (window.templateConatainerList[i].items) {
                    for (var j = 0, m = window.templateConatainerList[i].items.length; j < m; j++) {
                        ASC.CRM.Common.tooltip("#templateTitle_" + window.templateConatainerList[i].items[j].id, "tooltip");
                    }
                }
            }
        },

        showTemplateConatainerPanel: function(id) {
            if (!id) {
                ASC.CRM.TaskTemplateView.initTemplateConatainerPanel();
                PopupKeyUpActionProvider.EnableEsc = false;
                StudioBlockUIManager.blockUI("#templateConatainerPanel", 500);
            } else {
                Teamlab.getCrmEntityTaskTemplateContainer({}, id, {
                    success: function(params, templateContainer) {
                        ASC.CRM.TaskTemplateView.initTemplateConatainerPanel(templateContainer);
                        PopupKeyUpActionProvider.EnableEsc = false;
                        StudioBlockUIManager.blockUI("#templateConatainerPanel", 500);
                    }
                });
            }
        },

        createTemplateConatainer: function() {
            var title = jq("#templateConatainerTitle").val().trim(),
                view = jq.getURLParam("view"),
                entityType = view != null && view != ""
                            ? view
                            : "contact";

            if (title == "") {
                ShowRequiredError(jq("#templateConatainerTitle"), true);
                return false;
            } else {
                RemoveRequiredErrorClass(jq("#templateConatainerTitle"));
            }

            Teamlab.addCrmEntityTaskTemplateContainer({}, { entityType: entityType, title: title }, {
                success: function(params, templateContainer) {
                    jq.tmpl("templateContainerRow", templateContainer).appendTo("#templateConatainerContent");
                    checkTemplateConatainers();
                },
                before: function(params) {
                    jq("#templateConatainerPanel div.action_block").hide();
                    jq("#templateConatainerPanel div.ajax_info_block").show();
                },
                after: function(params) {
                    jq("#templateConatainerPanel div.ajax_info_block").hide();
                    jq("#templateConatainerPanel div.action_block").show();
                    PopupKeyUpActionProvider.EnableEsc = true;
                    jq.unblockUI();
                }
            });
        },

        editTemplateConatainer: function(containerid) {
            var title = jq("#templateConatainerTitle").val().trim(),
                view = jq.getURLParam("view"),
                entityType = view != null && view != "" ? view : "contact";

            if (title == "") {
                ShowRequiredError(jq("#templateConatainerTitle"), true);
                return false;
            }
            else RemoveRequiredErrorClass(jq("#templateConatainerTitle"));

            Teamlab.updateCrmEntityTaskTemplateContainer({}, containerid, { entityType: entityType, title: title }, {
                success: function(params, templateContainer) {
                    jq("#templateContainerBody_" + containerid).remove();
                    var newTemlateContainer = jq.tmpl("templateContainerRow", templateContainer);
                    jq("#templateContainerHeader_" + containerid).replaceWith(newTemlateContainer);
                },
                before: function(params) {
                    jq("#templateConatainerPanel div.action_block").hide();
                    jq("#templateConatainerPanel div.ajax_info_block").show();
                },
                after: function(params) {
                    jq("#templateConatainerPanel div.ajax_info_block").hide();
                    jq("#templateConatainerPanel div.action_block").show();
                    PopupKeyUpActionProvider.EnableEsc = true;
                    jq.unblockUI();
                }
            });
        },

        deleteTemplateConatainer: function(containerid) {
            Teamlab.removeCrmEntityTaskTemplateContainer({}, containerid, {
                success: function(params, templateContainer) {
                    jq("#templateContainerHeader_" + containerid).remove();
                    jq("#templateContainerBody_" + containerid).remove();
                    checkTemplateConatainers();
                },
                before: function(params) {
                    jq("#templateContainerHeader_" + containerid + " .crm-addNewLink").hide();
                    jq("#templateContainerHeader_" + containerid + " .crm-editLink").hide();
                    jq("#templateContainerHeader_" + containerid + " .crm-deleteLink").hide();
                    jq("#templateContainerHeader_" + containerid + " .loaderImg").show();
                }
            });
        },

        initTemplateConatainerPanel: function(container) {
            if (!container) {
                jq("#templateConatainerTitle").val("");
                RemoveRequiredErrorClass(jq("#templateConatainerTitle"));
                jq("#templateConatainerPanel div.containerHeaderBlock td:first")
                        .text(ASC.CRM.TaskTemplateView.ConatainerPanel_AddHeaderText);
                jq("#templateConatainerPanel div.action_block a.button.blue")
                        .text(ASC.CRM.TaskTemplateView.ConatainerPanel_AddButtonText)
                        .unbind("click").click(function() {
                            ASC.CRM.TaskTemplateView.createTemplateConatainer();
                        });
            } else {
                jq("#templateConatainerTitle").val(container.title);
                RemoveRequiredErrorClass(jq("#templateConatainerTitle"));
                jq("#templateConatainerPanel div.containerHeaderBlock td:first")
                        .text(jq.format(ASC.CRM.TaskTemplateView.ConatainerPanel_EditHeaderText, container.title));
                jq("#templateConatainerPanel div.action_block a.button.blue")
                        .text(ASC.CRM.Resources.CRMJSResource.SaveChanges)
                        .unbind("click").click(function() {
                            ASC.CRM.TaskTemplateView.editTemplateConatainer(container.id);
                        });
            }
        },

        toggleCollapceExpand: function(elem) {
            $Obj = jq(elem);

            if ($Obj.hasClass("headerCollapse")) {
                var body = $Obj.parents("li").next();
                if (body.find("table").length > 0) {
                    body.show();
                }
            } else {
                $Obj.parents("li").next().hide();
            }
            $Obj.toggleClass("headerExpand");
            $Obj.toggleClass("headerCollapse");
        },

        showTemplatePanel: function(containerid, id) {
            if (!id) {
                ASC.CRM.TaskTemplateView.initTemplatePanel(containerid);
                PopupKeyUpActionProvider.EnableEsc = false;
                StudioBlockUIManager.blockUI("#templatePanel", 500);
            } else {
                Teamlab.getCrmEntityTaskTemplate({}, id, {
                    success: function(params, template) {
                        ASC.CRM.TaskTemplateView.initTemplatePanel(containerid, template);
                        PopupKeyUpActionProvider.EnableEsc = false;
                        StudioBlockUIManager.blockUI("#templatePanel", 500);
                    }
                });
            }
        },

        createTemplate: function(containerid) {
            var data = ASC.CRM.TaskTemplateView.getCurrentTemplateData();
            data.containerid = containerid;

            if (data.title == "") {
                ShowRequiredError(jq("#tbxTemplateTitle"), true);
                return false;
            } else {
                RemoveRequiredErrorClass(jq("#tbxTemplateTitle"));
            }

            if (data.responsibleid == null || data.responsibleid == "") {
                if (jq.browser.mobile === true) {
                    RemoveRequiredErrorClass(jq("#taskTemplateViewAdvUsrSrContainer select:first"), true);
                } else {
                    RemoveRequiredErrorClass(jq("#templatePanel .inputUserName:first"), true);
                }
                return false;
            } else {
                if (jq.browser.mobile === true) {
                    RemoveRequiredErrorClass(jq("#taskTemplateViewAdvUsrSrContainer select:first"));
                } else {
                    RemoveRequiredErrorClass(jq("#templatePanel .inputUserName:first"));
                }
            }

            Teamlab.addCrmEntityTaskTemplate({}, data, {
                success: function(params, template) {
                    jq.tmpl("templateRow", template).appendTo("#templateContainerBody_" + containerid);
                    if (jq("#templateContainerBody_" + containerid + " table").length > 0) {
                        jq("#templateContainerBody_" + containerid).show();
                    }
                    ASC.CRM.Common.tooltip("#templateTitle_" + template.id, "tooltip");
                },
                before: function(params) {
                    jq("#templatePanel div.action_block").hide();
                    jq("#templatePanel div.ajax_info_block").show();
                },
                after: function(params) {
                    jq("#templatePanel div.ajax_info_block").hide();
                    jq("#templatePanel div.action_block").show();
                    PopupKeyUpActionProvider.EnableEsc = true;
                    jq.unblockUI();
                }
            });
        },

        editTemplate: function(containerid, templateid) {
            var data = ASC.CRM.TaskTemplateView.getCurrentTemplateData();
            data.containerid = containerid,
            data.id = templateid;

            if (data.title == "") {
                ShowRequiredError(jq("#tbxTemplateTitle"), true);
                return false;
            } else {
                RemoveRequiredErrorClass(jq("#tbxTemplateTitle"));
            }

            if (data.responsibleid == null) {
                if (jq.browser.mobile === true) {
                    ShowRequiredError(jq("#taskTemplateViewAdvUsrSrContainer select:first"), true);
                } else {
                    ShowRequiredError(jq("#templatePanel .inputUserName:first"), true);
                }
                return false;
            } else {
                if (jq.browser.mobile === true) {
                    RemoveRequiredErrorClass(jq("#taskTemplateViewAdvUsrSrContainer select:first"));
                } else {
                    RemoveRequiredErrorClass(jq("#templatePanel .inputUserName:first"));
                }
            }

            Teamlab.updateCrmEntityTaskTemplate({}, data, {
                success: function(params, template) {
                    var newTemlate = jq.tmpl("templateRow", template);
                    jq("#templateRow_" + templateid).replaceWith(newTemlate);
                    ASC.CRM.Common.tooltip("#templateTitle_" + template.id, "tooltip");
                },
                before: function(params) {
                    jq("#templatePanel div.action_block").hide();
                    jq("#templatePanel div.ajax_info_block").show();
                },
                after: function(params) {
                    jq("#templatePanel div.ajax_info_block").hide();
                    jq("#templatePanel div.action_block").show();
                    PopupKeyUpActionProvider.EnableEsc = true;
                    jq.unblockUI();
                }
            });
        },

        deleteTemplate: function(templateid) {
            Teamlab.removeCrmEntityTaskTemplate({}, templateid, {
                success: function(params, template) {
                    jq("#templateRow_" + templateid).remove();
                    if (jq("#templateContainerBody_" + template.containerID + " table").length == 0)
                        jq("#templateContainerBody_" + template.containerID).hide();
                },
                before: function(params) {
                    jq("#templateRow_" + templateid + " .crm-addNewLink").hide();
                    jq("#templateRow_" + templateid + " .crm-editLink").hide();
                    jq("#templateRow_" + templateid + " .crm-deleteLink").hide();
                    jq("#templateRow_" + templateid + " .loaderImg").show();
                }
            });
        },

        initTemplatePanel: function(containerid, template) {
            if (!template) {
                jq("#templatePanel div.containerHeaderBlock td:first")
                        .text(ASC.CRM.TaskTemplateView.TemplatePanel_AddHeaderText);
                jq("#templatePanel div.action_block a.button.blue")
                        .text(ASC.CRM.TaskTemplateView.TemplatePanel_AddButtonText)
                        .unbind("click").click(function() {
                            ASC.CRM.TaskTemplateView.createTemplate(containerid);
                        });
                jq("#tbxTemplateTitle").val("");
                jq("#tbxTemplateDescribe").val("");
                ASC.CRM.TaskTemplateView.setTemplateDeadlineFromTicks();
                jq("#notifyResponsible").removeAttr("checked");
                window.taskTemplateResponsibleSelector.ClearFilter();
                window.taskTemplateResponsibleSelector.ChangeDepartment(window.taskTemplateResponsibleSelector.Groups[0].ID);
                var obj = window.taskTemplateCategorySelector.getRowByContactID(0);
                window.taskTemplateCategorySelector.changeContact(obj);

            } else {
                jq("#templatePanel div.containerHeaderBlock td:first")
                        .text(jq.format(ASC.CRM.TaskTemplateView.TemplatePanel_EditHeaderText, template.title));
                jq("#templatePanel div.action_block a.button.blue")
                        .text(ASC.CRM.Resources.CRMJSResource.SaveChanges).unbind("click").click(function() {
                            ASC.CRM.TaskTemplateView.editTemplate(containerid, template.id);
                        });
                jq("#tbxTemplateTitle").val(template.title);
                jq("#tbxTemplateDescribe").val(template.description);
                ASC.CRM.TaskTemplateView.setTemplateDeadlineFromTicks(template.offsetTicks, template.deadLineIsFixed);
                jq("#notifyResponsible").prop("checked", template.isNotify);
                window.taskTemplateResponsibleSelector.ClearFilter();
                window.taskTemplateResponsibleSelector.ChangeDepartment(window.taskTemplateResponsibleSelector.Groups[0].ID);

                var obj;
                if (!jq.browser.mobile) {
                    obj = document.getElementById("User_" + template.responsible.id);
                    if (obj != null)
                        window.taskTemplateResponsibleSelector.SelectUser(obj);
                } else {
                    obj = jq("#taskResponsibleSelector select option[value=" + template.responsible.id + "]");
                    if (obj.length > 0) {
                        window.taskTemplateResponsibleSelector.SelectUser(obj);
                        jq(obj).prop("selected", true);
                    }
                }

                obj = window.taskTemplateCategorySelector.getRowByContactID(template.category.id);
                window.taskTemplateCategorySelector.changeContact(obj);
            }
        },

        getTemplateDeadlineTicks: function() {
            var displacement = jq("#tbxTemplateDisplacement").val().trim() != ""
                    ? parseInt(jq("#tbxTemplateDisplacement").val())
                    : 0,
                hour = displacement * 24 + parseInt(jq("#templateDeadlineHours option:selected").val()),
                minute = parseInt(jq("#templateDeadlineMinutes option:selected").val());

            return ((hour * 3600) + minute * 60) * 10000000;
        },

        setTemplateDeadlineFromTicks: function(ticks, isFixed) {
            if (!ticks && !isFixed) {
                jq("#deadLine_fixed").prop("checked", true);
                jq("#tbxTemplateDisplacement").val("0");
                jq("#templateDeadlineHours option:first").prop("selected", true);
                jq("#templateDeadlineMinutes option:first").prop("selected", true);
            } else {
                if (isFixed) {
                    jq("#deadLine_fixed").prop("checked", true);
                } else {
                    jq("#deadLine_not_fixed").prop("checked", true);
                }

                var minute = ((ticks / 10000000) % 3600) / 60,
                    hour = (parseInt((ticks / 10000000) / 3600)) % 24,
                    displacement = parseInt((parseInt((ticks / 10000000) / 3600)) / 24);

                jq("#tbxTemplateDisplacement").val(displacement);
                jq("#optDeadlineHours_" + hour).prop("selected", true);
                jq("#optDeadlineMinutes_" + minute).prop("selected", true);
            }
        },

        getCurrentTemplateData: function() {
            var data = {
                title: jq("#tbxTemplateTitle").val().trim(),
                description: jq("#tbxTemplateDescribe").val().trim(),
                responsibleid: window.taskTemplateResponsibleSelector.SelectedUserId,
                categoryid: window.taskTemplateCategorySelector.CategoryID,
                isNotify: jq("#notifyResponsible").is(":checked"),
                offsetTicks: ASC.CRM.TaskTemplateView.getTemplateDeadlineTicks(),
                deadLineIsFixed: jq('#templatePanel #deadLine_fixed').is(":checked")
            };
            return data;
        },

        getDeadlineDisplacement: function(ticks, isFixed) {
            var minute = ((ticks / 10000000) % 3600) / 60,
                hour = (parseInt((ticks / 10000000) / 3600)) % 24,
                displacement = parseInt((parseInt((ticks / 10000000) / 3600)) / 24);

            if (isFixed) {
                return jq.format(ASC.CRM.Resources.CRMJSResource.TemplateFixedDeadline, displacement, hour, minute);
            } else {
                return jq.format(ASC.CRM.Resources.CRMJSResource.TemplateNotFixedDeadline, displacement, hour, minute);
            }
        }
    };
})();

ASC.CRM.CurrencySettingsView = (function () {
    
    function initOtherActionMenu () {
        jq("#menuCreateNewTask").bind("click", function () { ASC.CRM.TaskActionView.showTaskPanel(0, "", 0, null, {}); });
    }

    function setBindings() {
        jq("#defaultCurrency").bind("change", changeDefaultCurrency);
        jq("#currencySelector").bind("change", changeCurrency);
        jq("#addCurrencyRate").bind("click", addCurrencyRate);
        jq("#currencyRateList").on("click", ".crm-deleteLink", deleteCurrencyRate);
        jq("#currencyRateList").on("change", ".textEdit", changeCurrencyRate);
        jq("#saveCurrencySettings").bind("click", saveCurrencySettings);
        jq("#cancelCurrencySettings").bind("click", cancelCurrencySettings);
    }

    function renderCurrencyRateList(items, clear) {
        if (clear) {
            jq("#currencyRateList").empty();
        }

        if (items) {
            render(items);
            return;
        }

        Teamlab.getCrmCurrencyRates({},
            {
                before: function () {
                    LoadingBanner.displayLoading();
                },
                after: function () {
                    LoadingBanner.hideLoading();
                },
                success: function (params, response) {
                    render(response);
                },
                error: function (params, errors) {
                    console.log(errors);
                    toastr.error(errors[0]);
                }
            });

        function render(data) {
            jq.tmpl("currencyRateItemTmpl", data).appendTo("#currencyRateList");
            
            jq(data).each(function (index, item) {
                jq.forceNumber({
                    parent: "#currencyRateList",
                    input: "#currencyRate_" + item.fromCurrency,
                    integerOnly: false,
                    positiveOnly: true
                });
            });
            
            jq("#currencySelector").change();
        }
    }

    function changeDefaultCurrency() {
        renderCurrencyRateList(jq(this).val() == window.defaultCurrency ? window.currencyRates : [], true);
        jq("#currencySelector").change();
    }

    function changeCurrency() {
        var value = jq(this).val();
        var exist = value == jq("#defaultCurrency").val();

        if (!exist) {
            jq("#currencyRateList .currency-rate-item span:first-child").each(function(index, item) {
                if (jq(item).text().trim() == value)
                    exist = true;
            });
        }

        if (exist) {
            jq("#addCurrencyRate").addClass("disable");
        } else {
            jq("#addCurrencyRate").removeClass("disable");
        }
    }

    function addCurrencyRate() {
        if (jq(this).hasClass("disable"))
            return;

        var data = [{
            fromCurrency: jq("#currencySelector").val(),
            rate: "1.00",
            toCurrency: jq("#defaultCurrency").val()
        }];

        renderCurrencyRateList(data, false);
        jq("#currencySelector").change();
    }

    function deleteCurrencyRate() {
        jq(this).parent().remove();
        jq("#currencySelector").change();
    }

    function changeCurrencyRate() {
        var obj = jq(this);
        var rate = Number(obj.val());

        if (rate <= 0) {
            obj.val("1.00");
        } else {
            obj.val(rate.toFixed(2));
        }
    }

    function saveCurrencySettings() {
        var newDefaultCurrency = jq("#defaultCurrency").val();
        var newCurrencyRates = [];

        jq("#currencyRateList .currency-rate-item").each(function (index, item) {
            var obj = jq(item);

            newCurrencyRates.push({
                fromCurrency: obj.find("span:first-child").text().trim(),
                rate: obj.find("input").val().trim(),
                toCurrency: newDefaultCurrency
            });
        });

        Teamlab.setCrmCurrencyRates({ newDefaultCurrency: newDefaultCurrency }, newDefaultCurrency, newCurrencyRates,
            {
                before: function() {
                    LoadingBanner.displayLoading();
                },
                after: function() {
                    LoadingBanner.hideLoading();
                },
                success: function(params, response) {
                    window.defaultCurrency = params.newDefaultCurrency;
                    window.currencyRates = response;
                    toastr.success(ASC.CRM.Resources.CRMJSResource.SettingsUpdated);
                },
                error: function(params, errors) {
                    console.log(errors);
                    toastr.error(errors[0]);
                }
            });
    }

    function cancelCurrencySettings() {
        jq("#defaultCurrency").val(window.defaultCurrency);
        renderCurrencyRateList(window.currencyRates, true);
        jq("#currencySelector").change();
    }

    return {
        init: function () {
            initOtherActionMenu();
            setBindings();
            renderCurrencyRateList(window.currencyRates, true);
        }
    };
})();