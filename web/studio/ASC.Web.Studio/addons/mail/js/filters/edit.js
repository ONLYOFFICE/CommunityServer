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


window.editFilterPage = (function($) {
    var isInit = false,
        container,
        header,
        page = 0,
        pageSize = 10,
        total = 0,
        needApply = false;

    function init() {
        if (isInit === true) return;

        container = $('#itemContainer').find('#editFilterPage');
        header = $('#pageActionContainer');

        ASC.Controls.AnchorController.bind(TMMail.anchors.createfilter, onEdit);
        ASC.Controls.AnchorController.bind(TMMail.anchors.editfilter, onEdit);

        window.filtersManager.bind(window.filtersManager.events.OnCreate, onSave);
        window.filtersManager.bind(window.filtersManager.events.OnUpdate, onSave);
    }

    function getNewPosition() {
        var array = filtersManager.getList().map(function (x) { return x.position; });
        if (array.length === 0) return 0;

        var max = Math.max.apply(Math, array);
        return max + 1;
    }

    function getFilter(id) {
        function isChecked(id) {
            var i, n = filter.options.applyTo.mailboxes.length;
            for (i = 0; i < n; i++) {
                var item = filter.options.applyTo.mailboxes[i];
                if (item === id)
                    return true;
            }

            return false;
        }

        var filter = id > 0
            ? TMMail.copy(filtersManager.get(id))
            : new ASC.Mail.Filter(0,
                "",
                getNewPosition(),
                true,
                [
                    new ASC.Mail.Filter.Condition(ASC.Mail.Filter.Constants.Condition.KeyType.From,
                        ASC.Mail.Filter.Constants.Condition.OperationType.Matches,
                        "")
                ],
                [],
                new ASC.Mail.Filter.Options([TMMail.sysfolders.inbox.id]));

        if (!filter) {
            return null;
        }

        var actions = {
            deleteAction: {
                checked: false
            },
            markAsReadAction: {
                checked: false
            },
            moveToAction: {
                checked: false,
                type: 1,
                id: null,
                name: null
            },
            tagAsAction: {
                checked: false,
                id: null,
                name: null
            },
            markAsImportantAction: {
                checked: false
            }
        };

        var i, n = filter.actions.length;
        var userFolder;
        for (i = 0; i < n; i++) {
            try {
                var item = filter.actions[i];

                switch (item.action) {
                case ASC.Mail.Filter.Constants.ActionType.DeleteForever:
                    actions.deleteAction.checked = true;
                    break;
                case ASC.Mail.Filter.Constants.ActionType.MoveTo:
                    actions.moveToAction.checked = true;

                    var data = JSON.parse(item.data);

                    if (data.type === TMMail.sysfolders.userfolder.id) {
                        userFolder = userFoldersManager.get(data.userFolderId);

                        actions.moveToAction.id = userFolder ? userFolder.id : null;
                        actions.moveToAction.name = userFolder ? userFolder.name : null;
                        actions.moveToAction.type = userFolder ? TMMail.sysfolders.userfolder.id : null;

                    } else {
                        actions.moveToAction.type = data.type;
                        actions.moveToAction.id = null;
                        actions.moveToAction.name = TMMail.getSysFolderDisplayNameById(data.type, '');
                    }

                    break;
                case ASC.Mail.Filter.Constants.ActionType.MarkTag:
                    actions.tagAsAction.checked = true;

                    var tag = tagsManager.getTag(item.data);

                    actions.tagAsAction.name = tag ? tag.name : null;
                    actions.tagAsAction.id = tag ? tag.id : null;

                    break;
                case ASC.Mail.Filter.Constants.ActionType.MarkAsImportant:
                    actions.markAsImportantAction.checked = true;
                    break;
                case ASC.Mail.Filter.Constants.ActionType.MarkAsRead:
                    actions.markAsReadAction.checked = true;
                    break;
                default:
                    continue;
                };
            } catch (e) {
                console.error(e);
            }
        }

        filter.acts = actions;

        filter.options.applyTo.mbxs = filter.options.applyTo.mailboxes || [];
 
        if (filter.options.applyTo.mailboxes.length === 0) {
            filter.options.applyTo.allMailboxes = true;

            filter.options.applyTo.mbxs = $.map(accountsManager.getAccountList(),
                function (a) {
                    if (a.is_alias || a.is_group)
                        return null;

                    return {
                        id: a.mailbox_id,
                        address: a.email,
                        checked: false
                    }
                });
        } else {
            filter.options.applyTo.allMailboxes = false;
            filter.options.applyTo.mbxs = $.map(accountsManager.getAccountList(),
                function (a) {
                    if (a.is_alias || a.is_group)
                        return null;

                    var id = parseInt(a.mailbox_id);
                    return {
                        id: id,
                        address: a.email,
                        checked: isChecked(id)
                    }
                });
        }

        filter.options.applyTo.flds = {
            inbox: filter.options.applyTo.folders.indexOf(TMMail.sysfolders.inbox.id) !== -1,
            sent: filter.options.applyTo.folders.indexOf(TMMail.sysfolders.sent.id) !== -1,
            spam: filter.options.applyTo.folders.indexOf(TMMail.sysfolders.spam.id) !== -1
        };

        return filter;
    }

    function onEdit(filterId) {
        mailBox.hidePages();
        var itemContainer = $('#itemContainer');

        itemContainer.height('auto');

        mailBox.hideContentDivs();
        mailBox.hideLoadingMask();

        needApply = false;

        filterId = filterId || 0;

        var filter = getFilter(filterId);

        if (!filter) {
            filtersManager.refresh()
                .then(function(params, filters) {
                        if (!filters.length || !getFilter(filterId)) {
                            window.toastr.error(window.MailScriptResource.UserFilterNotFound);
                            ASC.Controls.AnchorController.move('#filtersettings');
                            return;
                        }

                        onEdit(filterId);
                    },
                    function() {
                        window.toastr.error(window.MailScriptResource.UserFilterNotFound);
                        ASC.Controls.AnchorController.move('#filtersettings');
                    });

            return;
        }

        var editFilterActionHtml = $.tmpl('editFilterActionTmpl', filter);
        header.html(editFilterActionHtml);

        var writeMessageHtml = $.tmpl('editFilterTmpl', filter);
        container = itemContainer.find('#editFilterPage');

        if (container.length > 0) {
            container.replaceWith(writeMessageHtml);
        } else {
            itemContainer.append(writeMessageHtml);
        }

        container = itemContainer.find('#editFilterPage');

        mailBox.stickActionMenuToTheTop();

        container.find("select").tlcombobox({ align: 'left' });

        container.on("click",
            ".delete-field",
            function() {
                $(this).closest(".field-condition").remove();
                toggleMultiConditions();
                setFocus();
            });

        container.find("#conditionsContainer .add-new-field")
            .bind("click",
                function() {
                    addNewField(this);
                    toggleMultiConditions();
                    setFocus();
                });

        container.find(".applyToAttachments")
            .on("change",
                function() {
                    var self = $(this);

                    if (self.hasClass("disable")) return;

                    self.parent().find("input[type='checkbox']").prop("checked", true);
                });

        container.find("#selectMoveToFolderFilter")
            .on("click",
                function() {
                    var self = $(this);

                    if (self.hasClass("disable")) return;

                    var options = {
                        btnCaption: window.MailResource.Select,
                        hideDefaults: false,
                        hideRoot: true,
                        callback: function(folder) {
                            console.log("#selectMoveToFolderFilter .menuActionMoveTo -> callback", folder);

                            self.text(folder.name);
                            self.attr("selectedType", folder.folderType);
                            self.attr("selectedId", folder.userFolderId);
                            self.toggleClass("error", false);

                            self.parent().find("input[type='checkbox']").prop("checked", true);
                        }
                    };

                    userFoldersDropdown.show($(this), options);
                });

        container.find("#selectAddTagFilter")
            .on("click",
                function() {
                    var self = $(this);

                    if (self.hasClass("disable")) return;

                    var options = {
                        hideMarkRecipients: true,
                        onSelect: function(tag) {
                            console.log(tag);

                            self.text(tag.name);
                            self.attr("selectedId", tag.id);
                            self.toggleClass("error", false);

                            self.parent().find("input[type='checkbox']").prop("checked", true);
                        },
                        onDeselect: function(tag) {
                            console.log(tag);
                        },
                        getUsedTagsIds: function() {
                            var id = self.attr("selectedId");
                            return !id ? [] : [parseInt(id)];
                        }
                    }

                    tagsDropdown.show($(this), options);
                });

        container.find("#filterActionDelete")
            .on("click",
                function(e) {
                    var siblings = $(this)
                        .closest("li")
                        .siblings();

                    siblings
                        .find("label, input, a")
                        .toggleClass("disable", e.target.checked)
                        .attr("disabled", e.target.checked);

                    if (e.target.checked) {
                        siblings
                            .find("input")
                            .prop("checked", false);
                    }
                });

        header.find(".btnSaveFilter")
            .on("click", function() { saveFilter(filterId); });

        header.find(".btnCheckFilter")
            .on("click", checkFilter);

        header.find(".btnApplyFilter")
            .on("click",
                function() {
                    saveFilter(filterId);
                    needApply = true;
                });

        header.find(".btnDeleteFilter")
            .on("click",
                function() {
                    window.filtersManager.remove(filterId);
                    closeEdit();
                });

        container.find("#filterApplyToMailiboxes")
            .on("click",
                function(e) {
                    var otherMailboxesEl = $(this)
                        .closest(".field-with-actions")
                        .find(".applyToMailboxes.menu-list");

                    if (!e.target.checked) {
                        otherMailboxesEl.show();
                    } else {
                        otherMailboxesEl.hide();
                    }
                });

        container.find("#selectApplyToFolderFilter")
            .on("click",
                function() {
                    var self = $(this);

                    if (self.hasClass("disable")) return;

                    var options = {
                        hideDefaults: false,
                        hideRoot: true,
                        callback: function(folder) {
                            console.log("#selectApplyToFolderFilter .menuActionMoveTo -> callback", folder);

                            self.text(folder.name);
                            self.attr("selectedType", folder.folderType);
                            self.attr("selectedId", folder.userFolderId);
                            self.toggleClass("error", false);

                            self.parent().find("input[type='checkbox']").prop("checked", true);
                        }
                    };

                    userFoldersDropdown.show($(this), options);
                });

        var enterlinkText = container.find("#filterNameContainer .enterlinkText"),
            linkDeleteFilterName = container.find("#filterNameContainer .clear-field"),
            filterNameEnterContainer = container.find("#filterNameContainer .field-condition"),
            filterNameInput = filterNameEnterContainer.find(".filterNameInput");

        container.find("#filterNameContainer .filterName")
            .on("click",
                function() {
                    enterlinkText.hide();
                    filterNameEnterContainer.show();
                    filterNameInput.focus();
                });

        linkDeleteFilterName
            .on("click",
                function() {
                    filterNameInput.val("");
                    filterNameEnterContainer.hide();
                    enterlinkText.show();
                });

        container.find("#checkFilterResultsMoreBtn").on("click", checkFilterNext);

        container.find("select.key").on("change", onConditionKeyChange);

        var i, n = filter.conditions.length;
        for (i = 0; i < n; i++) {
            var id = filter.conditions[i].id;

            container.find('#' + id)
                .EmailsSelector("init",
                {
                    isInPopup: false,
                    items: []
                });
        }

        var foldersCbxs = container.find("#advancedContainer .applyToFolders input");
        foldersCbxs.change(function() {
            if (this.checked) {
                var labels = container.find("#advancedContainer .applyToFolders label");
                if (labels.hasClass("error")) {
                    labels.toggleClass("error", false);
                }
            }
        });

        var mailboxesCbxs = container.find("#advancedContainer #filterApplyToMailiboxesContainer input");
        mailboxesCbxs.change(function () {
            if (this.checked) {
                var labels = container.find("#advancedContainer #filterApplyToMailiboxesContainer label");
                if (labels.hasClass("error")) {
                    labels.toggleClass("error", false);
                }
            }
        });

        var actionsCbxs = container.find("#actionsContainer input");
        actionsCbxs.change(function () {
            if (this.checked) {
                var labels = container.find("#actionsContainer label");
                if (labels.hasClass("error")) {
                    labels.toggleClass("error", false);
                }
            }
        });

        if (!filter.options.applyTo.allMailboxes) {
            var allCheckedMbs = filter.options.applyTo.mbxs.filter(function (f) { return f.checked });
            if (!allCheckedMbs.length) {
                getFilterMailboxes(); // display no mailboxes selected error
            }
        }

        if (filter.id === 0) {
            setFocus();
        }

        TMMail.scrollTop();
    }

    function onConditionKeyChange() {
        var self = $(this);
        var key = +self.parent().attr("data-value");

        self.closest('.field-condition')
            .find('input.conditionInput')
            .EmailsSelector(key === ASC.Mail.Filter.Constants.Condition.KeyType.Subject
                ? "turnOff"
                : "turnOn");
    }

    function getApplyToAttachments() {
        var checked = container.find("#filterApplyToAttachments").prop('checked');
        if (!checked)
            return ASC.Mail.Filter.Constants.ApplyTo.AttachmentsType.WithAndWithoutAttachments;

        var value = container.find("#advancedContainer .filterApplyToAttachmentsCnt .tl-combobox").attr("data-value");
        return value ? parseInt(value) : ASC.Mail.Filter.Constants.ApplyTo.AttachmentsType.WithAndWithoutAttachments;
    }

    function getMatchConditions() {
        var value = container.find("#applyMultiConditionsContainer:visible .tl-combobox").attr("data-value");
        return value ? parseInt(value) : 0;
    }

    function getConditions() {
        var conditions = [];

        var list = container.find("#conditionsContainer .field-condition");

        var i, n = list.length;
        for (i = 0; i < n; i++) {
            var el = $(list[i]);

            var key = el.find("select.key").parent().attr("data-value"),
                operation = el.find("select.operation").parent().attr("data-value"),
                conditionInput = el.find(".conditionInput"),
                value = conditionInput.val().trim();

            if (!value) {
                conditionInput.toggleClass("invalidField", true);
                throw MailScriptResource.FilterErrorEmptyCondition;
            }

            conditionInput.toggleClass("invalidField", false);

            var condition = new ASC.Mail.Filter.Condition(key, operation, value);

            conditions.push(condition);
        }

        return conditions;
    }

    function getActions() {
        var actions = [];

        var deleteChecked = container.find("#actionsContainer #filterActionDelete").prop('checked');
        if (deleteChecked) {
            actions.push(new ASC.Mail.Filter.Action(ASC.Mail.Filter.Constants.ActionType.DeleteForever));
            // Skip other checkboxes
            return actions;
        }

        var data, elm;

        elm = container.find("#actionsContainer #selectMoveToFolderFilter");
        var moveToChecked = container.find("#actionsContainer #filterActionMoveTo").prop('checked');
        if (moveToChecked) {
            data = elm.attr('selectedid') || elm.attr('selectedtype');

            if (!data) {
                elm.toggleClass("error", true);
                throw MailScriptResource.FilterErrorEmptyMoveToFolderValue;
            }

            var selectedtype = +elm.attr('selectedtype');
            var selectedid = elm.attr('selectedid');

            if (selectedtype === TMMail.sysfolders.userfolder.id) {
                data = { type: TMMail.sysfolders.userfolder.id, userFolderId: selectedid };
            } else {
                data = { type: selectedtype };
            }

            data = JSON.stringify(data);

            elm.toggleClass("error", false);

            actions.push(new ASC.Mail.Filter.Action(ASC.Mail.Filter.Constants.ActionType.MoveTo, data));
        } else {
            elm.toggleClass("error", false);
        }

        elm = container.find("#actionsContainer #selectAddTagFilter");
        var tagAsChecked = container.find("#actionsContainer #filterActionMarkWithTag").prop('checked');
        if (tagAsChecked) {

            data = elm.attr('selectedid');

            if (!data) {
                elm.toggleClass("error", true);
                throw MailScriptResource.FilterErrorEmptyTagValue;
            }
            elm.toggleClass("error", false);

            actions.push(new ASC.Mail.Filter.Action(ASC.Mail.Filter.Constants.ActionType.MarkTag, data));
        } else {
            elm.toggleClass("error", false);
        }

        var markAsImportantChecked = container.find("#actionsContainer #filterActionMarkAsImportant").prop('checked');
        if (markAsImportantChecked) {
            actions.push(new ASC.Mail.Filter.Action(ASC.Mail.Filter.Constants.ActionType.MarkAsImportant));
        }

        var markAsReadChecked = container.find("#actionsContainer #filterActionMarkAsRead").prop('checked');
        if (markAsReadChecked) {
            actions.push(new ASC.Mail.Filter.Action(ASC.Mail.Filter.Constants.ActionType.MarkAsRead));
        }

        var labels = container.find("#actionsContainer label");

        if (!actions.length) {
            labels.toggleClass("error", true);
            throw MailScriptResource.FilterErrorNoActionSelected;
        }

        labels.toggleClass("error", false);

        return actions;
    }

    function getFilterName() {
        var elm = container.find("#filterNameContainer .field-condition:visible .filterNameInput");
        return elm ? elm.val() : null;
    }

    function getIgnoreOther() {
        var checked = container.find("#filterIgnoreOther").prop('checked');
        return checked;
    }

    function getFilterMailboxes() {
        var checkboxes = container.find("#advancedContainer .applyToMailboxes.menu-list:visible input:checked");
        var mailboxes = $.map(checkboxes, function(cbx) {
            return +$(cbx).attr("data_id");
        });

        var label = container.find("#advancedContainer #filterApplyToMailiboxesContainer label");

        if (!mailboxes.length && !container.find("#advancedContainer #filterApplyToMailiboxes").is(":checked")) {
            label.toggleClass("error", true);
            throw MailScriptResource.FilterErrorNoApplyToMailboxSelected;
        }

        label.toggleClass("error", false);

        return mailboxes;
    }

    function getApplyToFolders() {
        var checkboxes = container.find("#advancedContainer .applyToFolders.menu-list:visible input:checked");
        var folders = $.map(checkboxes, function (cbx) {
            return +$(cbx).attr("data_id");
        });

        var labels = container.find("#advancedContainer .applyToFolders label");

        if (!folders.length) {
            labels.toggleClass("error", true);
            throw MailScriptResource.FilterErrorNoApplyToFolderSelected;
        }

        labels.toggleClass("error", false);

        return folders;
    }

    function getFilterData(id, skipActions) {
        try {
            var matchMultiConditions = getMatchConditions(),
                conditions = getConditions(),
                actions = skipActions ? [] : getActions(),
                name = getFilterName(),
                position = id > 0 ? filtersManager.get(id).position : getNewPosition(),
                folders = getApplyToFolders(),
                mailboxes = getFilterMailboxes(),
                applyToAttachments = getApplyToAttachments(),
                ignoreOther = getIgnoreOther();

            var options = new ASC.Mail.Filter.Options();
            options.matchMultiConditions = matchMultiConditions;
            options.applyTo.folders = folders ? folders : [TMMail.sysfolders.inbox.id];
            options.applyTo.mailboxes = mailboxes;
            options.applyTo.withAttachments = applyToAttachments;
            options.ignoreOther = ignoreOther;

            var filter = new ASC.Mail.Filter(id, name, position, true, conditions, actions, options);

            return filter.ToData();

        } catch (e) {
            window.toastr.error(e);
        }

        return null;
    }

    function setFocus() {
        var conditions = container.find("#conditionsContainer .conditionInput");
        for (var i = 0; i < conditions.length; i++) {
            var condition = $(conditions[i]);

            if (condition.val() === '') {
                condition.focus();
                return;
            }
        }
    }

    function toggleMultiConditions() {
        var applyMultiConditionsContainer = container.find("#applyMultiConditionsContainer");
        var conditions = container.find("#conditionsContainer .field-condition");

        if (conditions.length > 1) {
            applyMultiConditionsContainer.show();
        } else {
            applyMultiConditionsContainer.hide();
        }
    }

    function addNewField(item) {
        var elm = $(item);
        if (elm.hasClass("disabled")) return;

        var filterCondition = new ASC.Mail.Filter.Condition(ASC.Mail.Filter.Constants.Condition.KeyType.From,
            ASC.Mail.Filter.Constants.Condition.OperationType.Matches,
            "");

        var newCondition = $($.tmpl('filterConditionTmpl', filterCondition));

        var conditionsContainer = $('#itemContainer').find('#editFilterPage #conditionsContainer');
        var list = conditionsContainer.find(".field-condition:last");

        if (list.length > 0) {
            list.after(newCondition);
        } else {
            elm.before(newCondition);
        }

        var selects = conditionsContainer.find("select");
        $(selects).tlcombobox({ align: 'left' });

        newCondition.find("select.key").on("change", onConditionKeyChange);

        newCondition.find('#' + filterCondition.id).EmailsSelector("init", {
            isInPopup: false,
            items: []
        });
    }

    function saveFilter(filterId) {
        var newFilter = getFilterData(filterId);
        if (!newFilter) return;

        var needConfirm = newFilter.actions.filter(function(a) {
            return a.action === ASC.Mail.Filter.Constants.ActionType.DeleteForever;
        }).length > 0;

        function save(data) {
            if (data.id === 0) {
                window.filtersManager.create(data);
            } else {
                window.filtersManager.update(data);
            }
        }

        if (needConfirm) {
            filterModal.showDeleteActionConfirm(newFilter,
            {
                onConfirm: function(filter) {
                    save(filter);
                },
                onChange: function (filter) {
                    var data = { type: TMMail.sysfolders.trash.id };
                    data = JSON.stringify(data);

                    filter.actions = [new ASC.Mail.Filter.Action(ASC.Mail.Filter.Constants.ActionType.MoveTo, data)];
                    save(filter);
                }
            });
        } else {
            save(newFilter);
        }
    }

    function checkFilter() {
        var filter = getFilterData(0, true);

        if (!filter) return;

        LoadingBanner.displayMailLoading();

        page = 0;
        pageSize = 10;

        window.Teamlab.checkMailFilter({}, filter, page, pageSize,
        {
            success: function (params, messages) {
                console.log("Check filter", filter, messages);
                LoadingBanner.hideLoading();

                container.find("#checkFilterResultsTitle").show();

                if (!messages || messages.length === 0) {
                    container.find("#checkFilterResults").empty().show();
                    var el = $("<p style='height: 20px; width=100%' />").text(MailScriptResource.NoLettersFilterHeader);
                    container.find("#checkFilterResults").append(el);
                    container.find("#checkFilterResultsMoreBtn").hide();
                    return;
                }

                page = 1;
                total = params.__total;

                var options = {
                    hasNext: false, 
                    hasPrev: false,
                    targetBlank: true,
                    hideCheckboxes: true,
                    showFolders: true
                };

                var html = mailBox.convertMessagesToHtml(messages, options);
                container.find("#checkFilterResults").html(html).show();

                var loadedCount = container.find("#checkFilterResults .row").length;

                if (loadedCount < total) {
                    container.find("#checkFilterResultsMoreBtn").show();
                } else {
                    container.find("#checkFilterResultsMoreBtn").hide();
                }
            },
            error: function (params, errors) {
                window.toastr.error(errors[0]);
                LoadingBanner.hideLoading();
            }
        });
    }

    function checkFilterNext() {
        var filter = getFilterData(0, true);

        if (!filter) return;

        LoadingBanner.displayMailLoading();

        window.Teamlab.checkMailFilter({}, filter, page, pageSize,
        {
            success: function (params, messages) {
                console.log("Check filter next", filter, messages);
                LoadingBanner.hideLoading();

                container.find("#checkFilterResultsTitle").show();

                if (!messages || messages.length === 0) {
                    container.find("#checkFilterResultsMoreBtn").hide();
                    return;
                }

                page +=1;
                total = params.__total;

                var options = {
                    hasNext: false,
                    hasPrev: false,
                    targetBlank: true,
                    hideCheckboxes: true,
                    showFolders: true
                };

                var html = mailBox.convertMessagesToHtml(messages, options);
                container.find("#checkFilterResults").append(html);

                var loadedCount = container.find("#checkFilterResults .row").length;

                if (loadedCount < total) {
                    container.find("#checkFilterResultsMoreBtn").show();
                } else {
                    container.find("#checkFilterResultsMoreBtn").hide();
                }
            },
            error: function (params, errors) {
                window.toastr.error(errors[0]);
                LoadingBanner.hideLoading();
            }
        });
    }

    function onSave(e, filter) {
        if (needApply) {
            window.filtersManager.applyFilter(filter.id);
            needApply = false;
        }

        closeEdit();
    }

    function closeEdit() {
        if (!TMMail.pageIs('editfilter') && !TMMail.pageIs('createfilter')) return;

        ASC.Controls.AnchorController.move('#filtersettings');

        container.remove();

        header.empty();
    }

    return {
        init: init,

        getFilterData: getFilterData,

        close: closeEdit
    };
})(jQuery);