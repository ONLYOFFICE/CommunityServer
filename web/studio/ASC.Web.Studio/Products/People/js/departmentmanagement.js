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
                jq("#membersDepartmentList").append($o);

                var itemIds = [];
                items.forEach(function (i) { itemIds.push(i.id); });
                $membersAdvancedSelector.useradvancedSelector("disable", itemIds);
            });

            jq("#membersDepartmentList").on("click", "li .reset-icon:not(.disabled)", function () {
                var $this = jq(this),
                    $elem = $this.parents("li");
                $membersAdvancedSelector.useradvancedSelector("undisable", [$elem.attr("data-id")]);
                $elem.remove();
            })
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
              })

            jq("#departmentManager").find(".reset-icon").on("click", function () {
                if (jq(this).hasClass("disabled")) return;
                $membersAdvancedSelector.useradvancedSelector("undisable", [jq("#departmentManager .result-name").attr("data-id")]);
                jq("#departmentManager").addClass("display-none");
                jq("#departmentManager .result-name").attr("data-id", "").text('');
                $headAdvancedSelector.show();
            })
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
        jq("#grouptitle").html(ASC.Resources.Master.AddDepartmentHeader);
        jq("#studio_departmentAddDialog .middle-button-container a.blue").html(ASC.Resources.Master.Resource.AddButton);

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = 'DepartmentManagement.AddDepartmentCallback()';
    };

    this.EditDepartmentOpenDialog = function (id, gName, gOwner) {

        HideRequiredError();
        DepartmentManagement.initDepartmentDialog();
        StudioBlockUIManager.blockUI("#studio_departmentAddDialog", 400, 300, 0);
        jq("#studio_departmentAddDialog .infoPanel").hide();

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.EnterAction = 'DepartmentManagement.AddDepartmentCallback()';

        this._groupId = id;
        jq("#grouptitle").html(ASC.Resources.Master.EditDepartmentHeader);
        jq("#studio_departmentAddDialog .middle-button-container .blue").html(ASC.Resources.Master.Resource.EditButton);
        jq("#studio_newDepName").val(gName);

        var $headAdvancedSelector = jq("#headAdvancedSelector"),
            $membersAdvancedSelector = jq("#membersAdvancedSelector");

        if (gOwner.id && gOwner.id != "4a515a15-d4d6-4b8e-828e-e0586f18f3a3") {// profile removed
            var $headCnt = jq("#departmentManager");
            $headCnt.find(".result-name").attr("data-id", gOwner.id).html(gOwner.displayName);
            $headCnt.removeClass("display-none");
            $headAdvancedSelector.hide();
            $headAdvancedSelector.useradvancedSelector("select", gOwner.id);
            $membersAdvancedSelector.useradvancedSelector("disable", [gOwner.id]);
        }
        var $memberList = jq(".members-dep-list");

        var filter = {
            groupId: id
        };

        Teamlab.getProfilesByFilter({}, {
            filter: filter,
            before: function () {
                jq("#membersDepartmentList").html("");
                $memberList.append("<div class=\"loading-link\">" + ASC.Resources.Master.Resource.LoadingPleaseWait + "</div>")
            },
            after: function(){
                $memberList.find(".loading-link").remove();
            },
            success: function (params, gMembers) {
                console.log(gMembers);
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

                    $membersAdvancedSelector.useradvancedSelector("disable", memberIds);
                    var $o = jq.tmpl("template-selector-selected-items", { Items: members.sort(SortData) });
                    jq("#membersDepartmentList").html($o);
                }
            }
        });
    };

    this.AddDepartmentCallback = function () {

        if (jq("#headAdvancedSelector + .advanced-selector-container").is(":visible") ||
            jq("#membersAdvancedSelector + .advanced-selector-container").is(":visible"))
            return;

        var departmentName = jq("#studio_newDepName");
        if (jq.trim(jq(departmentName).val()) == "") {
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
                success: function (params) {
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
                success: function (params) {
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