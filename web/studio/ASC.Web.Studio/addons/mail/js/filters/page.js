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


window.filtersPage = (function($) {
    var isInit = false,
        page,
        header,
        actionButtons = [];

    function init() {
        if (isInit === true) return;
        isInit = true;

        page = $('#filters_page');
        
        header = $('#pageActionContainer');

        header.on('click', '#createFilter', function() {
            if (!$(this).hasClass('disable')) {
                createFilter();
            }
            return false;
        });

        header.on('click', '#filtersHelpCenterSwitcher', function (e) {
            jq('#filtersHelpCenterSwitcher').helper({ BlockHelperID: 'FiltersHelperBlock' });
            e.preventDefault();
            e.stopPropagation();
        });

        actionButtons = [
            {
                selector: "#filterActionMenu .enableFilter",
                handler: function(id) {
                    page.find("#filtersContainer .row[data_id=\"" + id + "\"] .on-off-checkbox").trigger("click");
                }
            },
            {
                selector: "#filterActionMenu .applyFilter",
                handler: applyFilter
            },
            { selector: "#filterActionMenu .editFilter", handler: editFilter },
            { selector: "#filterActionMenu .deleteFilter", handler: deleteFilter }
        ];

        window.filtersManager.bind(window.filtersManager.events.OnDelete, show);
    }

    function applyFilter(id) {
        id = parseInt(id);
        window.filtersManager.applyFilter(id);
    }

    function enableFilter(id, enabled) {
        id = parseInt(id);
        window.filtersManager.enable(id, enabled);
    }

    function createFilter() {
        ASC.Controls.AnchorController.move('createfilter');
    }

    function editFilter(id) {
        id = parseInt(id);
        ASC.Controls.AnchorController.move('editfilter/' + id);
    }

    function deleteFilter(id) {
        id = parseInt(id);
        window.filtersManager.remove(id);
    }

    function translateConditionKey(key) {
        switch (key) {
        case ASC.Mail.Filter.Constants.Condition.KeyType.From:
            return MailScriptResource.FromLabel;
        case ASC.Mail.Filter.Constants.Condition.KeyType.ToOrCc:
            return MailScriptResource.ToOrCcLabel;
        case ASC.Mail.Filter.Constants.Condition.KeyType.To:
            return MailScriptResource.ToLabel;
        case ASC.Mail.Filter.Constants.Condition.KeyType.Cc:
            return MailScriptResource.CcLabel;
        case ASC.Mail.Filter.Constants.Condition.KeyType.Subject:
            return MailScriptResource.SubjectLabel;
        default:
            return null;
        }
    }

    function translateConditionOperation(operation) {
        switch (operation) {
        case ASC.Mail.Filter.Constants.Condition.OperationType.Matches:
            return MailScriptResource.MatchesLabel;
        case ASC.Mail.Filter.Constants.Condition.OperationType.NotMatches:
            return MailScriptResource.NotMatchesLabel;
        case ASC.Mail.Filter.Constants.Condition.OperationType.Contains:
            return MailScriptResource.ContainsLabel;
        case ASC.Mail.Filter.Constants.Condition.OperationType.NotContains:
            return MailScriptResource.NotContainsLabel;
        default:
            return null;
        }
    }

    function translateAction(action) {
        switch (action) {
        case ASC.Mail.Filter.Constants.ActionType.DeleteForever:
            return MailScriptResource.FilterActionDeleteLabel;
        case ASC.Mail.Filter.Constants.ActionType.MoveTo:
            return MailScriptResource.FilterActionMoveToLabel;
        case ASC.Mail.Filter.Constants.ActionType.MarkTag:
            return MailScriptResource.FilterActionMarkWithTagLabel;
        case ASC.Mail.Filter.Constants.ActionType.MarkAsImportant:
            return MailScriptResource.FilterActionMarkAsImportantLabel;
        case ASC.Mail.Filter.Constants.ActionType.MarkAsRead:
            return MailScriptResource.FilterActionMarkAsReadLabel;
        default:
            return null;
        }
    }

    function translateActionData(action, data) {
        try {
            switch (action) {
            case ASC.Mail.Filter.Constants.ActionType.MoveTo:
                var dataObj = JSON.parse(data);
                var folderName;
                if (dataObj.type === TMMail.sysfolders.userfolder.id) {
                    var userFolder = userFoldersManager.get(dataObj.userFolderId);
                    folderName = userFolder ? userFolder.name : null;
                } else {
                    folderName = TMMail.getSysFolderDisplayNameById(dataObj.type, '');
                }
                return folderName;
            case ASC.Mail.Filter.Constants.ActionType.MarkTag:
                var id = parseInt(data);
                var tag = tagsManager.getTag(id);
                return tag ? tag.name : null;
            default:
                return null;
            }
        } catch (e) {
            console.error(e);
        }
        return null;
    }

    function translateMatchMultiConditions(match) {
        switch (match) {
            case ASC.Mail.Filter.Constants.Condition.MatchMultiType.MatchAll:
            return MailScriptResource.AndLabel;
            case ASC.Mail.Filter.Constants.Condition.MatchMultiType.MatchAtLeastOne:
            return MailScriptResource.OrLabel;
        default:
            return MailScriptResource.IfLabel;
        }
    }

    function translateApplyToMailboxes(mailboxes) {
        if (!mailboxes || mailboxes.length === 0) return null;

        var i, n = mailboxes.length, arr = [];

        for (i = 0; i < n; i++) {
            var id = mailboxes[i];
            var account = accountsManager.getAccountById(id);
            if(!account) continue;

            arr.push(account.email);
        }

        if (arr.length === 0)
            return null;

        var str = ASC.Mail.Resources.MailScriptResource.ApplyToLabel + " " + arr.join(", ");

        return str;
    }

    function translateApplyToFolder(folders) {

        if (!folders || folders.length === 0) return null;

        var i, n = folders.length, arr = [];

        for (i = 0; i < n; i++) {
            var id = folders[i];
            var folderName = TMMail.getSysFolderDisplayNameById(id, '');

            arr.push(folderName);
        }

        if (arr.length === 0)
            return null;

        var str = ASC.Mail.Resources.MailScriptResource.ApplyToMessagesFromLabel + " " + arr.join(", ");

        return str;
    }

    function translateApplyToAttachments(type) {
        if (type === ASC.Mail.Filter.Constants.ApplyTo.AttachmentsType.WithAndWithoutAttachments)
            return "";
        else if (type === ASC.Mail.Filter.Constants.ApplyTo.AttachmentsType.WithAttachments) {
            return ASC.Mail.Resources.MailScriptResource.FilterApplyToAttachemnts +
                " " +
                ASC.Mail.Resources.MailScriptResource.FilterApplyToAttachemntsPresent;
        }
        else if (type === ASC.Mail.Filter.Constants.ApplyTo.AttachmentsType.WithoutAttachments) {
            return ASC.Mail.Resources.MailScriptResource.FilterApplyToAttachemnts +
                " " +
                ASC.Mail.Resources.MailScriptResource.FilterApplyToAttachemntsNotPresent;
        }
        return "";
    }

    function pretreatments(id) {
        var $enableFilter = page.find("#filterActionMenu .enableFilter");

        var filter = filtersManager.get(id);
        $enableFilter.text(filter.enabled ? MailResource.DisableBtnLabel : MailResource.EnableBtnLabel);
        $enableFilter.toggleClass("disable-item", filter.enabled).toggleClass("enable-item", !filter.enabled);
    }

    function show() {
        if (!TMMail.pageIs('filtersettings')) {
            hide();
        };

        window.LoadingBanner.displayMailLoading();

        filtersManager.refresh()
            .then(function() {
                    window.LoadingBanner.hideLoading();

                    var filters = filtersManager.getList();

                    if (!filters.length) {
                        hide();
                        blankPages.showEmptyFilters();
                    } else {
                        blankPages.hide();

                        filters.forEach(function(f) {
                            if (!f.enabled)
                                return;

                            var brokenActions = f.actions.filter(function(a) {
                                if (a.action === ASC.Mail.Filter.Constants.ActionType.MoveTo) {
                                    var data = JSON.parse(a.data);

                                    if (data.type === TMMail.sysfolders.userfolder.id) {
                                        var userFolder = userFoldersManager.get(data.userFolderId);
                                        return !userFolder;

                                    } else {
                                        return false;
                                    }

                                } else if (a.action === ASC.Mail.Filter.Constants.ActionType.MarkTag) {
                                    var tag = tagsManager.getTag(a.data);
                                    return !tag;
                                } else {
                                    return false;
                                }
                            });

                            var brokenMailboxes = [], existsMailboxes = [];

                            if (f.options.applyTo.mailboxes.length !== 0) {
                                brokenMailboxes = f.options.applyTo.mailboxes.filter(function(mbId) {
                                    var mailbox = accountsManager.getAccountById(mbId);

                                    if (mailbox) {
                                        existsMailboxes.push(mbId);
                                    }

                                    return !mailbox;
                                });
                            }

                            var needDisableFilter = brokenActions.length > 0 ||
                            (brokenMailboxes
                                .length >
                                0 &&
                                brokenMailboxes.length === f.options.applyTo.mailboxes.length);

                            if (brokenMailboxes
                                .length >
                                0 &&
                                brokenMailboxes.length !== f.options.applyTo.mailboxes.length) {
                                f.options.applyTo.mailboxes = existsMailboxes;
                                filtersManager.update(f);
                            }

                            if (needDisableFilter) {
                                f.enabled = false;
                                filtersManager.enable(f.id, false);
                            }
                        });

                        filters = filters.sort(function(f1, f2) {
                            return !f1.enabled && f2.enabled ? 1 : (f1.enabled && !f2.enabled ? -1 : 0);
                        });

                        var html = $.tmpl('filtersTmpl',
                        {
                            filters: filters,
                            translateConditionKey: translateConditionKey,
                            translateConditionOperation: translateConditionOperation,
                            translateAction: translateAction,
                            translateActionData: translateActionData,
                            translateMatchMultiConditions: translateMatchMultiConditions,
                            translateApplyToMailboxes: translateApplyToMailboxes,
                            translateApplyToFolder: translateApplyToFolder,
                            translateApplyToAttachments: translateApplyToAttachments
                        });

                        var $html = $(html);

                        var filterList = $html.find("tbody");

                        filterList.sortable({
                                helper: 'clone',
                                update: function() {
                                    var self = $(this);
                                    self.children()
                                        .each(function(index) {
                                            var id = $(this).attr("data_id");

                                            var filter = filtersManager.get(id);

                                            if (!filter || filter.position === index + 1) return;

                                            filter.position = index + 1;

                                            filtersManager.update(filter);
                                        });
                                }
                            })
                            .disableSelection();

                        $html.actionMenu('filterActionMenu', actionButtons, pretreatments);

                        $html.find(".on-off-checkbox")
                            .on("change",
                                function(e) {
                                    e.preventDefault();

                                    var self = jq(this);
                                    var enabled = self.prop("checked");
                                    var row = self.closest(".row");

                                    var id = row.attr("data_id");
                                    enableFilter(id, enabled);

                                    row.toggleClass("disable");

                                    show();

                                    return false;
                                });

                        $html.find(".row")
                            .on("click",
                                function(e) {
                                    var target = $(e.target);

                                    if (target.hasClass("checkbox") ||
                                        target.hasClass("on-off-checkbox") ||
                                        target.hasClass("entity-menu")) {
                                        return true;
                                    }

                                    var row = jq(this);
                                    var id = row.attr("data_id");
                                    editFilter(id);

                                    return false;
                                });

                        header.html($.tmpl('filtersPageHeaderTmpl'));

                        page.find("#filtersContainer").html(html);

                        page.show();
                        
                    }
                },
                function() {
                    window.LoadingBanner.hideLoading();
                });
    }

    function hide() {
        if (!page) return;

        header.empty();
        page.hide();
    }

    return {
        init: init,

        show: show,
        hide: hide,

        createFilter: createFilter,
        editFilter: editFilter,

        translateAction: translateAction,
        translateActionData: translateActionData
    };
})
(jQuery);