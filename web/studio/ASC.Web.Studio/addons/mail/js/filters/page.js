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


window.filtersPage = (function($) {
    var isInit = false,
        page,
        btnCreate,
        actionButtons = [];

    function init() {
        if (isInit === true) return;
        isInit = true;

        page = $('#filters_page');

        btnCreate = page.find('#createFilter');

        btnCreate.click(function() {
            if (!$(this).hasClass('disable')) {
                createFilter();
            }
            return false;
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