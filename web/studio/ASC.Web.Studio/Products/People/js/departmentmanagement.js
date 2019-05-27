/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


;
var DepartmentManagement = new function () {

    //--------------departments management---------------------------------------

    this._groupId = null;
    this._deleteGroup = '';
    this.isInitHeadSelector = false;
    this.isInitMembersSelector = false;

    this.initDepartmentDialog = function () {
        var $headAdvancedSelector = jq("#headAdvancedSelector"),
            $membersAdvancedSelector = jq("#membersAdvancedSelector");

        if (!DepartmentManagement.isInitMembersSelector) {
            var $headDepartment = jq("#departmentManager .result-name").attr("data-id");
            DepartmentManagement.isInitMembersSelector = true;
            $membersAdvancedSelector.useradvancedSelector(
              {
                  itemsDisabledIds: $headDepartment.length ? [$headDepartment] : [],
                  canadd: true,       // enable to create the new item
                  showGroups: true, // show the group list
                  inPopup: true
              }
            ).on("showList", function (e, items) {
                var $o = jq.tmpl("template-selector-selected-items", { Items: items });
                jq("#membersDepartmentList").html($o);

                var itemIds = [];
                items.forEach(function (i) { itemIds.push(i.id); });
            });

            jq("#membersDepartmentList").on("click", "li .reset-icon:not(.disabled)", function () {
                var $this = jq(this),
                    $elem = $this.parents("li");
                $membersAdvancedSelector.useradvancedSelector("unselect", [$elem.attr("data-id")]);
                $elem.remove();
            });
        } else {
            $membersAdvancedSelector.useradvancedSelector("reset");
        }

        if (!DepartmentManagement.isInitHeadSelector) {

            DepartmentManagement.isInitHeadSelector = true;

            $headAdvancedSelector.useradvancedSelector(
              {
                  canadd: true,       // enable to create the new item
                  showGroups: true, // show the group list
                  onechosen: true,   // list without checkbox, you can choose only one item 
                  inPopup: true,
                  withGuests: false
              }
              ).on("showList", function (event, item) {
                  var $headCnt = jq("#departmentManager"),
                      $oldHeadId = $headCnt.find(".result-name").attr("data-id");

                  $membersAdvancedSelector.useradvancedSelector("undisable", $oldHeadId.length ? [$oldHeadId] : []);
                  $membersAdvancedSelector.useradvancedSelector("disable", [item.id]);
                  jq("#membersDepartmentList li[data-id=" + item.id + "]").remove();

                  $headCnt.find(".result-name").attr("data-id", item.id).html(item.title);
                  $headCnt.removeClass("display-none");
                  jq(this).hide();
              });

            jq("#departmentManager").find(".reset-icon").on("click", function () {
                if (jq(this).hasClass("disabled")) return;
                $membersAdvancedSelector.useradvancedSelector("undisable", [jq("#departmentManager .result-name").attr("data-id")]);
                jq("#departmentManager").addClass("display-none");
                jq("#departmentManager .result-name").attr("data-id", "").text('');
                $headAdvancedSelector.show();
            });
        } else {
            $headAdvancedSelector.useradvancedSelector("reset");
        }
    }

    this.AddDepartmentOpenDialog = function () {
        HideRequiredError();
        DepartmentManagement.initDepartmentDialog();
        StudioBlockUIManager.blockUI("#studio_departmentAddDialog", 400, 300, 0);
        jq("#studio_departmentAddDialog .infoPanel").hide();
        jq("#studio_newDepName").val('');
        jq("#headAdvancedSelector").show();
        jq("#departmentManager").addClass("display-none");
        jq("#departmentManager .result-name").text("");
        jq("#membersDepartmentList li").remove();
        jq("#membersAdvancedSelector").useradvancedSelector("reset");
        this._groupId = null;
        jq("#studio_departmentAddDialog .grouptitle").html(ASC.Resources.Master.AddDepartmentHeader);
        jq("#studio_departmentAddDialog .middle-button-container a.blue").html(ASC.Resources.Master.Resource.AddButton);

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = 'DepartmentManagement.AddDepartmentCallback()';
    };

    this.EditDepartmentOpenDialog = function (groupData) {
        var id = groupData.id, gName = groupData.name, gOwner = groupData.manager, gMembers = groupData.members;

        HideRequiredError();
        DepartmentManagement.initDepartmentDialog();
        StudioBlockUIManager.blockUI("#studio_departmentAddDialog", 400, 300, 0);
        jq("#studio_departmentAddDialog .infoPanel").hide();

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = 'DepartmentManagement.AddDepartmentCallback()';

        this._groupId = id;
        jq("#studio_departmentAddDialog .grouptitle").html(ASC.Resources.Master.EditDepartmentHeader);
        jq("#studio_departmentAddDialog .middle-button-container .blue").html(ASC.Resources.Master.Resource.EditButton);
        jq("#studio_newDepName").val(gName);

        var $headAdvancedSelector = jq("#headAdvancedSelector"),
            $membersAdvancedSelector = jq("#membersAdvancedSelector");

        if (gOwner && gOwner.id && gOwner.id !== "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {// profile removed
            var $headCnt = jq("#departmentManager");
            $headCnt.find(".result-name").attr("data-id", gOwner.id).html(gOwner.displayName);
            $headCnt.removeClass("display-none");
            $headAdvancedSelector.hide();
            $headAdvancedSelector.useradvancedSelector("select", [gOwner.id]);
            $membersAdvancedSelector.useradvancedSelector("disable", [gOwner.id]);
        }
        var $memberList = jq(".members-dep-list");

        jq("#membersDepartmentList").html("");
        $memberList.append("<div class=\"loading-link\">" + ASC.Resources.Master.Resource.LoadingPleaseWait + "</div>");

        if (gMembers.length) {

            var memberIds = [],
                members = [];

            gMembers.forEach(function (m) {
                memberIds.push(m.id);
                if (m.id !== gOwner.id) {
                    members.push({
                        id: m.id,
                        title: m.displayName
                    });
                }
            });

            $membersAdvancedSelector.useradvancedSelector("select", memberIds);
            var $o = jq.tmpl("template-selector-selected-items", { Items: members.sort(SortData) });
            jq("#membersDepartmentList").html($o);
        }

        $memberList.find(".loading-link").remove();
    };

    this.AddDepartmentCallback = function () {

        if (jq("#headAdvancedSelector + .advanced-selector-container").is(":visible") ||
            jq("#membersAdvancedSelector + .advanced-selector-container").is(":visible"))
            return;

        var departmentName = jq("#studio_newDepName");
        if (jq.trim(jq(departmentName).val()) === "") {
            ShowRequiredError(departmentName);
            return;
        }

        var managerId = jq("#departmentManager .result-name").attr("data-id"),
            membersIds = [];

        jq("#membersDepartmentList li").each(function (i, el) {
            membersIds.push(jq(el).attr("data-id"));
        });

        var data = {
            groupManager: managerId.length ? managerId : "00000000-0000-0000-0000-000000000000",
            groupName: departmentName.val(),
            groupId: DepartmentManagement._groupId,
            members: membersIds
        };

        DepartmentManagement.LockDepartmentDialog();

        if (DepartmentManagement._groupId == null) {
            Teamlab.addGroup(null, data, {
                success: function () {
                    DepartmentManagement.CloseAddDepartmentDialog();
                    window.onbeforeunload = null;
                    window.location.reload(true);
                },
                error: function (params, errors) {
                    DepartmentManagement.UnlockDepartmentDialog();
                    jq("#studio_departmentAddDialog .infoPanel").html("<div>" + errors + "</div>");
                    jq("#studio_departmentAddDialog .infoPanel").show();
                }
            });
        } else {
            Teamlab.updateGroup(null, DepartmentManagement._groupId, data, {
                success: function () {
                    DepartmentManagement.CloseAddDepartmentDialog();
                    window.location.reload(true);
                },
                error: function (params, errors) {
                    DepartmentManagement.UnlockDepartmentDialog();
                    jq("#studio_departmentAddDialog .infoPanel").html("<div>" + errors + "</div>");
                    jq("#studio_departmentAddDialog .infoPanel").show();
                }
            });
        }
    };

    this.CloseAddDepartmentDialog = function () {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
    };

    this.LockDepartmentDialog = function () {
        jq("#headAdvancedSelector, #membersAdvancedSelector, #departmentManager .reset-icon, #membersDepartmentList .reset-icon").addClass("disabled");
        LoadingBanner.showLoaderBtn("#studio_departmentAddDialog");
    };

    this.UnlockDepartmentDialog = function () {
        jq("#headAdvancedSelector, #membersAdvancedSelector, #departmentManager .reset-icon , #membersDepartmentList .reset-icon").removeClass("disabled");
        LoadingBanner.hideLoaderBtn("#studio_departmentAddDialog");
    };
};



jq(document).ready(function () {
    jq.tmpl("template-blockUIPanel", {
        id: "studio_departmentAddDialog",
        headerTest: ASC.People.Resources.AddDepartmentDlgTitle,
        headerClass: "grouptitle"
    }).insertAfter("#peopleSidepanel");
    jq.tmpl("depAddPopupBodyTemplate", null).appendTo("#studio_departmentAddDialog .containerBodyBlock");

    jq("#depActionBtn").on("click", ".button.blue", function () { DepartmentManagement.AddDepartmentCallback(); });
    jq("#depActionBtn").on("click", ".button.gray", function () { DepartmentManagement.CloseAddDepartmentDialog(); });

});