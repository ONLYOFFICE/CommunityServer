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


var ForumMakerProvider = new function() {
    this.All = "All";
    this.ConfirmMessage = "Are you sure?";
    this.NameEmptyString = "Enter title";
    this.SaveButton = "Save";
    this.CancelButton = "Cancel"

    this.ModuleID = '853B6EB9-73EE-438d-9B09-8FFEEDF36234';
    this.Callback = '';
    this.IsRenderCategory = false;

    //-----------Forum Maker-----------------------------------------
    this.ShowForumMakerDialog = function(isRenderCategory, callback) {
        HideRequiredError();
        if (isRenderCategory != null && isRenderCategory != undefined)
            this.IsRenderCategory = isRenderCategory;
        else
            this.IsRenderCategory = false;

        if (callback != null && callback != undefined)
            this.Callback = callback;
        else
            this.Callback = '';

        AjaxPro.onLoading = function(b) { };
        ForumMaker.GetCategoryList(function(result) {

            jq('#forum_fmCategoryList').html(result.value);

            jq('#forum_fmMessage').html('');
            jq('#forum_fmMessage').hide();

            jq('#forum_fmCategoryName').val('');
            jq('#forum_fmForumName').val('');
            jq('#forum_fmForumDescription').val('');

            StudioBlockUIManager.blockUI("#forum_fmDialog", 400);

            PopupKeyUpActionProvider.ClearActions();
            PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveThreadCategory();';

            if (jq('#forum_fmCategoryID').val() == '-1')
                jq('#forum_fmCaregoryNameBox').show();

            jq("#forum_fmDialog").show();
            jq('#forum_fmContent').show();
            jq('#forum_fmInfo').hide();

        });
    };

    this.SelectCategory = function() {
        HideRequiredError();
        if (jq('#forum_fmCategoryID').val() == '-1') {
            jq('#forum_fmCaregoryNameBox').show();
            jq('#forum_fmCaregoryNameBox').removeClass("hideField");
            jq('#forum_fmCaregoryDescriptionBox').show();
            jq('#forum_fmCaregoryDescriptionBox').removeClass("hideField");
        }
        else {
            jq('#forum_fmCaregoryNameBox').hide();
            jq('#forum_fmCaregoryNameBox').addClass("hideField");
            jq('#forum_fmCaregoryDescriptionBox').hide();
            jq('#forum_fmCaregoryDescriptionBox').addClass("hideField");
        }

    };

    this.SaveThreadCategory = function() {
        if (jq("#createThreadCategoryBth").hasClass("disable"))
            return;

        HideRequiredError();
        var newCategoryName = jq("#forum_fmCategoryName");
        var forumName = jq("#forum_fmThreadName");

        if ((jq(newCategoryName).val() == "") && !jq("#forum_fmCaregoryNameBox").hasClass("hideField")) {
            AddRequiredErrorText(newCategoryName, this.NameEmptyString);
            ShowRequiredError(newCategoryName);
            return;
        }
        if (jq(forumName).val() == "") {
            AddRequiredErrorText(forumName, this.NameEmptyString);
            ShowRequiredError(forumName);
            return;
        }
        AjaxPro.onLoading = function(b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#forum_fmContent");
            }
            else {
                LoadingBanner.hideLoaderBtn("#forum_fmContent");
            }
        };

        ForumMaker.SaveThreadCategory(jq('#forum_fmCategoryID').val(),
            jq('#forum_fmCategoryName').val(),
            jq('#forum_fmCategoryDescription').val(),
            jq('#forum_fmThreadName').val(),
            jq('#forum_fmThreadDescription').val(),
            this.IsRenderCategory,
            function (result) {
                    var res = result.value;
                    if (res.rs1 == '1') {
                        jq('#forum_fmContent').hide();
                        jq('#forum_fmInfo').show();
                        location.replace("topics.aspx?f=" + res.rs5);
                    }
                    else {

                        jq('#forum_fmMessage').html(res.rs2);
                        jq('#forum_fmMessage').show();
                    }
                });
    };

    this.CloseFMDialog = function() {
        PopupKeyUpActionProvider.ClearActions();
        jq.unblockUI();
    };


    this.ToggleThreadCategory = function(categoryID) {
        if (jq('#forum_threadListBox_' + categoryID).is(':visible')) {
            jq('#forum_threadListBox_' + categoryID).hide();
            jq('#forum_categoryState_' + categoryID).attr('src', StudioManager.GetImage('plus.png'));

        }
        else {
            jq('#forum_threadListBox_' + categoryID).show();
            jq('#forum_categoryState_' + categoryID).attr('src', StudioManager.GetImage('minus.png'));
        }
    };

    this.InitSortCategory = function() {
        //category sort
        jq('#forum_threadCategories').sortable({
            cursor: 'move',
            items: '>div[id^="forum_categoryBox_"]',
            handle: 'td[id^="forum_categoryBoxHandle_"]',
            update: function(ev, ui) {
                ForumMakerProvider.UpdateCategorySortOrder();
            },
            helper: function(e, el) {
                return jq(el).clone().width(jq(el).width());
            },

            dropOnEmpty: false
        });

        //property sort
        jq('div[id^="forum_threadListBox_"]').sortable({
            dropOnEmpty: false,
            cursor: 'move',
            items: '>div[id^="forum_threadBox_"]',
            connectWith: 'div[id^="forum_threadListBox_"]',
            handle: 'td[id^="forum_threadBoxHandle_"]',
            update: function(ev, ui) {
            },

            stop: function(event, ui) {
                ForumMakerProvider.UpdateThreadSortOrder();
            },

            helper: function(e, el) {
                return jq(el).clone().width(jq(el).width());
            }
        });
    };

    this.UpdateCategorySortOrder = function() {
        var sortOrders = new String();
        jq('div[id^="forum_categoryBox_"]').each(function(i, el) {
            if (i > 0)
                sortOrders += ',';

            sortOrders += jq(this).attr('name') + ":" + i;
        });

        AjaxPro.onLoading = function(b) { };
        ForumEditor.UpdateCategorySortOrder(sortOrders, function(result) { });
    };

    this.UpdateThreadSortOrder = function() {
        var sortOrders = new String();
        jq('div[id^="forum_categoryBox_"]').each(function(i, el) {
            if (i > 0)
                sortOrders += ';';

            var cid = jq(this).attr('name');
            sortOrders += cid + "@";
            jq('#forum_threadListBox_' + cid + ' > div[id^="forum_threadBox_"][name!="empty"]').each(function(j, obj) {
                if (j > 0)
                    sortOrders += ',';

                sortOrders += jq(this).attr('name') + ':' + j;
            });

        });

        AjaxPro.onLoading = function(b) { };
        ForumEditor.UpdateThreadSortOrder(sortOrders, function(result) { });
    };

    this.ShowEditCategoryDialog = function(categoryID, name, description) {
        jq('#forum_editCategoryMessage').html('');
        jq('#forum_editCategoryMessage').hide();

        jq('#forum_editCategoryID').val(categoryID);
        jq('#forum_editCategoryName').val(name);
        jq('#forum_editCategoryDescription').val(description);

        StudioBlockUIManager.blockUI("#forum_edit_categoryDialog", 400);

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveCategory("edit");'
    };

    this.ShowNewCategoryDialog = function() {
        jq('#forum_newCategoryMessage').html('');
        jq('#forum_newCategoryMessage').hide('');

        jq('#forum_newCategoryName').val('');
        jq('#forum_newCategoryDescription').val('');

        StudioBlockUIManager.blockUI("#forum_new_categoryDialog", 400);

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveCategory("new");'
    };

    this.SaveCategory = function(prefix) {
        var id = '';
        if (prefix == 'edit')
            id = jq('#forum_editCategoryID').val();

        var name = jq('#forum_' + prefix + 'CategoryName').val();
        var description = jq('#forum_' + prefix + 'CategoryDescription').val();

        AjaxPro.onLoading = function(b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#forum_" + prefix + "_categoryDialog");
            }
            else {
                LoadingBanner.hideLoaderBtn("#forum_" + prefix + "_categoryDialog");
            }
        };

        if (prefix == 'new') {
            ForumEditor.CreateCategory(name, description, function(result) {
                var res = result.value;
                if (res.rs1 == '1') {
                    jq.unblockUI();
                    jq('#forum_threadCategories').append(res.rs2);
                    ForumMakerProvider.InitSortCategory();
                }
                else {
                    jq('#forum_newCategoryMessage').html(res.rs2);
                    jq('#forum_newCategoryMessage').show();
                }
            });
        }
        else {
            ForumEditor.SaveCategory(id, name, description, function(result) {
                var res = result.value;
                if (res.rs1 == '1') {
                    jq.unblockUI();
                    jq('#forum_categoryBox_' + res.rs2).replaceWith(res.rs3);
                    ForumMakerProvider.InitSortCategory();
                }
                else {
                    jq('#forum_editCategoryMessage').html(res.rs3);
                    jq('#forum_editCategoryMessage').show();
                }
            });
        }

    };

    this.ShowNewThreadDialog = function(categoryID) {
        jq('#forum_newThreadMessage').html('');
        jq('#forum_newThreadMessage').hide('');

        jq('#forum_newThreadName').val('');
        jq('#forum_newThreadDescription').val('');
        jq('#forum_newThreadCategoryID').val(categoryID);

        StudioBlockUIManager.blockUI("#forum_new_threadDialog", 350);

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveThread("new");'
    };

    this.ShowEditThreadDialog = function(threadID, categoryID, name, description) {
        jq('#forum_editThreadMessage').html('');
        jq('#forum_editThreadMessage').hide('');

        jq('#forum_editThreadCategoryID').val(categoryID);
        jq('#forum_editThreadID').val(threadID);
        jq('#forum_editThreadName').val(name);
        jq('#forum_editThreadDescription').val(description);

        StudioBlockUIManager.blockUI("#forum_edit_threadDialog", 350);

        PopupKeyUpActionProvider.ClearActions();
        PopupKeyUpActionProvider.CtrlEnterAction = 'ForumMakerProvider.SaveThread("edit");'

    };

    this.SaveThread = function(prefix) {
        var threadID = '';
        if (prefix == 'edit')
            id = jq('#forum_editThreadID').val();

        var categoryID = jq('#forum_' + prefix + 'ThreadCategoryID').val();
        var name = jq('#forum_' + prefix + 'ThreadName').val();
        var description = jq('#forum_' + prefix + 'ThreadDescription').val();

        AjaxPro.onLoading = function(b) {
            if (b) {
                LoadingBanner.showLoaderBtn("#forum_" + prefix + "_threadDialog");
            }
            else {
                LoadingBanner.hideLoaderBtn("#forum_" + prefix + "_threadDialog");
            }
        };

        if (prefix == 'new') {
            ForumEditor.DoCreateThread(categoryID, name, description, function(result) {
                var res = result.value;
                if (res.rs1 == '1') {
                    ForumMakerProvider.CloseDialogByID('forum_new_threadDialog');
                    jq('#forum_categoryBox_' + res.rs2).replaceWith(res.rs3);
                    ForumMakerProvider.InitSortCategory();
                }
                else {
                    jq('#forum_newThreadMessage').html(res.rs2);
                    jq('#forum_newThreadMessage').show();
                }
            });
        }
        else {
            ForumEditor.SaveThread(id, categoryID, name, description, function(result) {
                var res = result.value;
                if (res.rs1 == '1') {
                    ForumMakerProvider.CloseDialogByID('forum_edit_threadDialog');
                    jq('#forum_categoryBox_' + res.rs2).replaceWith(res.rs3);
                    ForumMakerProvider.InitSortCategory();
                }
                else {
                    jq('#forum_editThreadMessage').html(res.rs3);
                    jq('#forum_editThreadMessage').show();
                }
            });
        }
    };

    this.CloseDialogByID = function(dialogID) {
        PopupKeyUpActionProvider.ClearActions();
        jq('#' + dialogID).hide();
        jq.unblockUI();

    };

    this.DeleteThread = function(threadID, categoryID) {
        if (!confirm(this.ConfirmMessage))
            return;

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_threadCategories').block();
            else
                jq('#forum_threadCategories').unblock();
        }

        ForumEditor.DoDeleteThread(threadID, categoryID, function(result) {
            var res = result.value;
            if (res.rs1 == '1')
                jq('#forum_threadBox_' + res.rs2).remove();

            else
                jq('#forum_threadBox_' + res.rs2).html('<div class="errorBox">' + res.rs4 + '</div>');
        });
    };
    this.DeleteThreadTopic = function(threadID, categoryID) {
        if (!confirm(this.ConfirmMessage))
            return;


        ForumEditor.DoDeleteThread(threadID, categoryID, function(result) {
            var res = result.value;
            if (res.rs1 == '1')
                window.location.href = "Default.aspx";
            else {
                var itemDiv = "errorMessageTopic";
                if (itemDiv != null) {
                    var divDel = document.getElementById(itemDiv);
                    divDel.className = 'errorBox errorText';
                    divDel.innerHTML = res.rs4;
                }
                /* jq('#errorMessageTopic').html('<div class="errorBox">' + res.rs4 + '</div>');*/
            }
        });
    };


    this.RemoveCategory = function(id) {
        if (!confirm(this.ConfirmMessage))
            return;

        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_threadCategories').block();
            else
                jq('#forum_threadCategories').unblock();
        };

        ForumEditor.DoDeleteThreadCategory(id, function(result) {
            var res = result.value;
            if (res.rs1 == '1')
                jq('#forum_categoryBox_' + res.rs2).remove();

            else
                jq('#forum_categoryBox_' + res.rs2).html('<div class="errorBox">' + res.rs3 + '</div>');
        });
    };

    this.SelectModerators = function(id, isCategory) {
        jq('#forum_securityObjID').val(id);

        var userListElement = 'forum_moderators' + (isCategory ? 'Category' : 'Thread') + '_' + id;
        var userIDs = jq('#' + userListElement).val().split(',');

        subjectSelector.ClearChecked();
        subjectSelector.OpenSearchTab();

        for (var i = 0; i < userIDs.length; i++) {
            if (userIDs[i] != '') {
                subjectSelector.UserSelect(userIDs[i]);
            }
        }

        if (isCategory) {
            subjectSelector.OnSelectButtonClick = this.SelectCategoryModerators;
        }
        else {
            subjectSelector.OnSelectButtonClick = this.SelectThreadModerators;
        }

        subjectSelector.ShowDialog();
    };

    this.SaveModerators = function(isCategory) {
        var userIDs = '';
        var userNames = ''
        var isFirst = true;
        jq(subjectSelector.Checked).each(function(i, el) {

            if (el.Type == 'Group')
                return true;

            if (isFirst)
                isFirst = false;
            else {
                userIDs += ',';
                userNames += ', ';
            }

            userIDs += el.ID;
            userNames += el.Name;

        });

        jq('#forum_modNames' + (isCategory ? 'Category' : 'Thread') + '_' + jq('#forum_securityObjID').val()).html(userNames);
        jq('#forum_moderators' + (isCategory ? 'Category' : 'Thread') + '_' + jq('#forum_securityObjID').val()).val(userIDs);

        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        };

        ForumEditor.SaveModerators(jq('#forum_securityObjID').val(), isCategory, userIDs, function(result) {
            var res = result.value;
            if (res.rs1 != '1')
                jq('#forum_modNames' + ((res.rs4 == '1') ? 'Category' : 'Thread') + '_' + jq('#forum_securityObjID').val()).html(res.rs3);
        });
    };

    this.SelectCategoryModerators = function() {
        ForumMakerProvider.SaveModerators(true);
    };

    this.SelectThreadModerators = function() {
        ForumMakerProvider.SaveModerators(false);
    };

    this.SelectVisibleList = function(id, isCategory) {
        jq('#forum_securityObjID').val(id);

        var userListElement = 'forum_vl' + (isCategory ? 'Category' : 'Thread') + '_' + id;
        var userIDs = jq('#' + userListElement).val().split(',');

        subjectSelector.ClearChecked();
        subjectSelector.OpenSearchTab();

        for (var i = 0; i < userIDs.length; i++) {
            if (userIDs[i] != '') {
                subjectSelector.UserSelect(userIDs[i]);
            }
        }

        if (isCategory) {
            subjectSelector.OnSelectButtonClick = this.SelectCategoryVisibleList;
        }
        else {
            subjectSelector.OnSelectButtonClick = this.SelectThreadVisibleList;
        }

        subjectSelector.ShowDialog();
    };

    this.SaveVisibleList = function(isCategory) {
        var userIDs = '';
        var userNames = '';
        var isFirst = true;
        jq(subjectSelector.Checked).each(function(i, el) {

            if (el.Type == 'Group')
                return true;

            if (isFirst)
                isFirst = false;
            else {
                userIDs += ',';
                userNames += ', ';
            }

            userIDs += el.ID;
            userNames += el.Name;

        });

        if (userNames == '')
            userNames = this.All;

        jq('#forum_vlNames' + (isCategory ? 'Category' : 'Thread') + '_' + jq('#forum_securityObjID').val()).html(userNames);
        jq('#forum_vl' + (isCategory ? 'Category' : 'Thread') + '_' + jq('#forum_securityObjID').val()).val(userIDs);

        AjaxPro.onLoading = function(b) {
            if (b)
                jq.blockUI();
            else
                jq.unblockUI();
        };

        ForumEditor.SaveMembers(jq('#forum_securityObjID').val(), isCategory, userIDs, function(result) {
            var res = result.value;
            if (res.rs1 != '1')
                jq('#forum_vlNames' + ((res.rs4 == '1') ? 'Category' : 'Thread') + '_' + jq('#forum_securityObjID').val()).html(res.rs3);
        });
    };

    this.SelectCategoryVisibleList = function() {
        ForumMakerProvider.SaveVisibleList(true);
    };

    this.SelectThreadVisibleList = function() {
        ForumMakerProvider.SaveVisibleList(false);
    };

    jq(document).ready(function() {
        ForumMakerProvider.InitSortCategory();
        var textInput = jq("#forum_fmCategoryName");
        if (textInput.length)
            textInput.focus();
    });
};

//-------------------------------------------
ForumSubscriber = new function() {
    this.SubscribeOnTopic = function(topicID, state) {
        AjaxPro.onLoading = function (b) {
            if (b)
                LoadingBanner.displayLoading();
            else
                LoadingBanner.hideLoading();
        }
        Subscriber.SubscribeOnTopic(topicID, state, function(result) {
            var res = result.value;
            jq('#statusSubscribe').replaceWith(res.rs2);
            if (res.rs3 == "subscribed") {
                jq('#statusSubscribe').removeClass("unsubscribed").addClass("subscribed");
            }
            if (res.rs3 == "unsubscribed") {
                jq('#statusSubscribe').removeClass("subscribed").addClass("unsubscribed");
            }
        });
        jq("#forumsActionsMenuPanel").hide();

    };

    this.SubscribeOnThread = function(threadID, state) {

        Subscriber.SubscribeOnThread(threadID, state, function(result) {
            var res = result.value;
            jq('#statusSubscribe').replaceWith(res.rs2);
            if (res.rs3 == "subscribed") {
                jq('#statusSubscribe').removeClass("unsubscribed").addClass("subscribed");
            }
            if (res.rs3 == "unsubscribed") {
                jq('#statusSubscribe').removeClass("subscribed").addClass("unsubscribed");
            }
        });
        jq("#forumsActionsMenuPanel").hide();

    };    

    this.SubscribeOnNewTopics = function(state) {
        AjaxPro.onLoading = function(b) {
            if (b)
                jq('#forum_subcribeOnNewTopicBox').block();
            else
                jq('#forum_subcribeOnNewTopicBox').unblock();
        }

        Subscriber.SubscribeOnNewTopic(state, function(result) {
            var res = result.value;
            jq('#forum_subcribeOnNewTopicBox').replaceWith(res.rs2);
        });
    };
}