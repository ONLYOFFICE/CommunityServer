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


window.mailBox = (function($) {
    var isInit = false,
        currentMessageId = -1,
        currentFolderId = -1,
        keepSelectionOnReload = false,
        pageIsLoaded = false,
        lastSelectedConcept = null,
        maxWidthOfSwitching = 1520,
        minWidthOfSwitching = 1024,
        maxDisplayedTagsCount = 3,
        actionButtons = [];

    var selection = new TMContainers.IdMap();
    var currentSelection = new TMContainers.IdMap();

    var selectionConcept = {
        unread: { displayAcc: ASC.Mail.Resources.MailResource.OverallConceptUnread_Acc, displayGen: ASC.Mail.Resources.MailResource.OverallConceptUnread_Gen },
        read: { displayAcc: ASC.Mail.Resources.MailResource.OverallConceptRead_Acc, displayGen: ASC.Mail.Resources.MailResource.OverallConceptRead_Gen },
        important: { displayAcc: ASC.Mail.Resources.MailResource.OverallConceptImportant_Acc, displayGen: ASC.Mail.Resources.MailResource.OverallConceptImportant_Gen },
        with_attachments: { displayAcc: ASC.Mail.Resources.MailResource.OverallConceptWithAttachments_Acc, displayGen: ASC.Mail.Resources.MailResource.OverallConceptWithAttachments_Gen },
        all: { displayAcc: ASC.Mail.Resources.MailResource.OverallConceptAll, displayGen: ASC.Mail.Resources.MailResource.OverallConceptAll }
    };

    function init() {
        if (isInit === false) {
            isInit = true;

            ASC.Controls.AnchorController.bind(TMMail.anchors.sysfolders, onSysFolderPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.compose, onCompose);
            ASC.Controls.AnchorController.bind(TMMail.anchors.composeto, onComposeTo);
            ASC.Controls.AnchorController.bind(TMMail.anchors.reply, onReplyPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.replyAll, onReplyAllPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.forward, onForwardPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.draftitem, onDraftItemPage);

            ASC.Controls.AnchorController.bind(TMMail.anchors.message, onMessagePage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.next_message, onNextMessagePage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.prev_message, onPrevMessagePage);

            ASC.Controls.AnchorController.bind(TMMail.anchors.conversation, onConversationPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.next_conversation, onNextConversationPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.prev_conversation, onPrevConversationPage);

            ASC.Controls.AnchorController.bind(TMMail.anchors.accounts, onAccountsPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.teamlab, onTeamLabContactsPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.crm, onCrmContactsPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.personal_contact, onPersonalContactsPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.tags, onTagsPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.administration, onAdministrationPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.commonSettings, onCommonSettingsPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.helpcenter, onHelpPage);

            ASC.Controls.AnchorController.bind(TMMail.anchors.messagePrint, onMessagePrintPage);
            ASC.Controls.AnchorController.bind(TMMail.anchors.conversationPrint, onConversationPrintPage);

            messagePage.init();

            tagsManager.events.bind('delete', onDeleteTag);
            tagsManager.events.bind('update', onUpdateTag);

            serviceManager.bind(window.Teamlab.events.getMailFilteredMessages, onGetMailConversations);
            serviceManager.bind(window.Teamlab.events.getMailFilteredConversations, onGetMailConversations);
            serviceManager.bind(window.Teamlab.events.removeMailFolderMessages, onRemoveMailFolderMessages);
            serviceManager.bind(window.Teamlab.events.restoreMailMessages, onRestoreMailMessages);
            serviceManager.bind(window.Teamlab.events.restoreMailConversations, onRestoreMailMessages);
            serviceManager.bind(window.Teamlab.events.moveMailMessages, onMoveMailMessages);
            serviceManager.bind(window.Teamlab.events.moveMailConversations, onMoveMailConversations);
            serviceManager.bind(window.Teamlab.events.removeMailMessages, onRemoveMailMessages);
            serviceManager.bind(window.Teamlab.events.removeMailConversations, onRemoveMailMessages);
            serviceManager.bind(window.Teamlab.events.markMailMessages, onMarkMailMessages);

            serviceManager.bind(window.Teamlab.events.getNextMailConversationId, onGetNextPrevConversationId);
            serviceManager.bind(window.Teamlab.events.getPrevMailConversationId, onGetNextPrevConversationId);

            serviceManager.bind(window.Teamlab.events.getNextMailMessageId, onGetNextPrevMessageId);
            serviceManager.bind(window.Teamlab.events.getPrevMailMessageId, onGetNextPrevMessageId);

            $('#createNewMailBtn').trackEvent(ga_Categories.leftPanel, ga_Actions.buttonClick, "create-new-Email");
            $('#check_email_btn').trackEvent(ga_Categories.leftPanel, ga_Actions.buttonClick, "check_email");

            $(document).on("click", '.menu-list a.menu-item-label', messagePage.onLeaveMessage);
            $('#createNewMailBtn').click(messagePage.onLeaveMessage);
            $('#check_email_btn').click(messagePage.onLeaveMessage);

            $(window).resize(function() {
                if (TMMail.pageIs('sysfolders') || TMMail.pageIs('crmContact')) {
                    changeTagStyle();
                }
                resizeActionMenuWidth();
                TMMail.resizeContent();
            });

            TMMail.resizeContent();

            $(window).scroll(stickActionMenuToTheTop);

            actionButtons = [
                { selector: "#messagesActionMenu .openMail", handler: openConversation },
                { selector: "#messagesActionMenu .openNewTabMail", handler: openNewTabConversation },
                { selector: "#messagesActionMenu .replyMail", handler: TMMail.moveToReply },
                { selector: "#messagesActionMenu .replyAllMail", handler: TMMail.moveToReplyAll },
                { selector: "#messagesActionMenu .createEmail", handler: createEmailToSender },
                { selector: "#messagesActionMenu .forwardMail", handler: TMMail.moveToForward },
                { selector: "#messagesActionMenu .setReadMail", handler: readUnreadGroupOperation },
                { selector: "#messagesActionMenu .markImportant", handler: markImportant },
                { selector: "#messagesActionMenu .moveToFolder", handler: moveToFolder },
                { selector: "#messagesActionMenu .printMail", handler: moveToConversationPrint },
                { selector: "#messagesActionMenu .deleteMail", handler: deleteGroupOperation }];
        }
    }

    function getScrolledGroupOptions() {
        var options = undefined;
        if (TMMail.pageIs('sysfolders')) {
            options = {
                menuSelector: "#MessagesListGroupButtons",
                menuAnchorSelector: "#SelectAllMessagesCB",
                menuSpacerSelector: "#itemContainer .contentMenuWrapper.messagesList .header-menu-spacer",
                userFuncInTop: function() { $("#MessagesListGroupButtons .menu-action-on-top").hide(); },
                userFuncNotInTop: function() { $("#MessagesListGroupButtons .menu-action-on-top").show(); }
            };
        } else if (TMMail.pageIs('tlContact') || TMMail.pageIs('crmContact') || TMMail.pageIs('personalContact')) {
            options = {
                menuSelector: "#ContactsListGroupButtons",
                menuAnchorSelector: "#SelectAllContactsCB",
                menuSpacerSelector: "#id_contacts_page .contentMenuWrapper .header-menu-spacer",
                userFuncInTop: function() { $("#ContactsListGroupButtons .menu-action-on-top").hide(); },
                userFuncNotInTop: function() { $("#ContactsListGroupButtons .menu-action-on-top").show(); }
            };
        } else if (TMMail.pageIs('message') || TMMail.pageIs('conversation')) {
            var menuAnchorSelector = "#MessageGroupButtons .btnReply";

            var current = MailFilter.getFolder();
            if (current == TMMail.sysfolders.spam.id || current == TMMail.sysfolders.trash.id) {
                menuAnchorSelector = "#MessageGroupButtons .btnDelete";
            }

            options = {
                menuSelector: "#MessageGroupButtons",
                menuAnchorSelector: menuAnchorSelector,
                menuSpacerSelector: "#itemContainer .messageHeader .contentMenuWrapper .header-menu-spacer",
                userFuncInTop: function() { $("#MessageGroupButtons .menu-action-on-top").hide(); },
                userFuncNotInTop: function() { $("#MessageGroupButtons .menu-action-on-top").show(); }
            };
        } else if (TMMail.pageIs('writemessage')) {
            options = {
                menuSelector: "#WriteMessageGroupButtons",
                menuAnchorSelector: "#WriteMessageGroupButtons .btnSend",
                menuSpacerSelector: "#editMessagePageHeader .header-menu-spacer",
                userFuncInTop: function() { $("#WriteMessageGroupButtons .menu-action-on-top").hide(); },
                userFuncNotInTop: function() { $("#WriteMessageGroupButtons .menu-action-on-top").show(); }
            };
        }

        return options;
    }

    function resizeActionMenuWidth() {
        var options = getScrolledGroupOptions();
        if (options != undefined && $(options.menuSelector).is(":visible") && $(options.menuSelector).css("position") != "static") {
            window.ScrolledGroupMenu.resizeContentHeaderWidth(options.menuSelector);
        }
    }

    function stickActionMenuToTheTop() {
        var options = getScrolledGroupOptions();
        if (options != undefined && $(options.menuSelector).is(":visible")) {
            window.ScrolledGroupMenu.stickMenuToTheTop(options);
        }
    }

    function getTagWidth() {
        var width;
        var currentWidowWidth = $(window).width();
        if (currentWidowWidth >= maxWidthOfSwitching) {
            width = 160;
        } else if (currentWidowWidth <= minWidthOfSwitching) {
            width = 30;
        } else {
            width = 30 + (currentWidowWidth - minWidthOfSwitching) * 0.2;
        }
        return width;
    }

    function changeTagStyle() {
        var maxWidth = "max-width: " + getTagWidth() + "px";
        $("#mailBoxContainer").find(".tag.tagArrow").attr("style", maxWidth);
    }

    function unmarkAllPanels(skipFilterPanels) {
        if (true !== skipFilterPanels) {
            tagsPanel.unmarkAllTags();
            if (!TMMail.pageIs('writemessage')) {
                accountsPanel.unmark();
            }
        }

        accountsPanel.updateAnchors();

        contactsPanel.unmarkContacts();
        folderPanel.unmarkFolders();
        settingsPanel.unmarkSettings();
        helpPanel.unmarkSettings();
    }

    function getSelectedAddresses() {
        var addresses = [];
        selection.Each(function(messageId) {
            var mailAuthor = $('tr[data_id="{0}"] .author'.format(messageId)), email;
            if (!mailAuthor || !(email = mailAuthor.attr("email")))
                return true;

            var address = ASC.Mail.Utility.ParseAddress(email);
            if (address.isValid && !TMMail.inArray(address, addresses)) {
                if (!address.name)
                    address.name = mailAuthor.text().trim();

                addresses.push(address.ToString());
            }

            return true;
        });
        return addresses;
    }

    // Makes checkboxes checked or not depending on ids from _Selection
    // Note: No need to call updateSelectionComboCheckbox after this

    function updateSelectionView() {
        var haveOneUnchecked = false;
        currentSelection.Clear();
        $('.messages:visible .row').each(function() {
            var row = $(this);
            var messageId = row.attr('data_id');
            var $checkbox = row.find('input[type="checkbox"]');
            if (selection.HasId(messageId)) {
                currentSelection.AddId(messageId);
                $checkbox.prop('checked', true);
                row.addClass('selected');
            } else {
                $checkbox.prop('checked', false);
                row.removeClass('selected');
                haveOneUnchecked = true;
            }
        });
        setSelectionComboCheckbox(!haveOneUnchecked);
        updateOverallSelectionLine();
    }

    function updateSelectionComboCheckbox(messagesTable) {
        // Update checked state
        var uncheckedFound = false;
        messagesTable = messagesTable || $('.messages:visible');
        messagesTable.find('.row input[type="checkbox"]').each(function() {
            if (!currentSelection.HasId($(this).attr('data_id'))) {
                uncheckedFound = true;
                return false;
            }
            return true;
        });
        setSelectionComboCheckbox(!uncheckedFound);
    }

    function setSelectionComboCheckbox(checked) {
        $('#SelectAllMessagesCB').prop('checked', checked);
    }

    function updateOverallSelectionLine() {
        var selectedNum = selection.Count();
        $('#OverallSelectionNumber').toggle(selectedNum != 0);
        $('#OverallSelectionNumberText').text(selectedNum);
        $('#OverallSelectedNumberCategory').html(lastSelectedConcept && lastSelectedConcept != selectionConcept.all ? (lastSelectedConcept.displayGen + '&nbsp;') : '');

        $('#OverallDeselectAll').toggle(selectedNum != 0);
    }

    function overallDeselectAll() {
        selection.Clear();
        lastSelectedConcept = null;
        updateSelectionView();
        commonLettersButtonsState();
    }

    function hideContentDivs(skipFolderFilter) {
        $('#itemContainer .messages').hide();
        $('#bottomNavigationBar').hide();
        $('#itemContainer .contentMenuWrapper').hide();
        $('#itemContainer .itemWrapper').remove();
        $('#itemContainer .mailContentWrapper').remove();
        $('#itemContainer .simpleWrapper').remove();
        if (true !== skipFolderFilter) {
            folderFilter.hide();
        }
    }

    function markFolderAsChanged(folderId) {
        var folderName = TMMail.getSysFolderNameById(folderId, null);
        var $visibleTableAnchor = $('.messages:visible').attr('anchor');
        if (folderName != null) {
            $('.messages[anchor^="' + folderName + '"]').attr('changed', 'true');
            $('.messages[anchor^="' + folderName + '"][anchor!="' + $visibleTableAnchor + '"]').remove();
        }
        filterCache.drop(folderId);
    }

    function getConversationMessages() {
        return $('.itemWrapper:visible .message-wrap');
    }

    function setTag(tagId, messageIds) {
        if (!TMMail.pageIs('sysfolders') && !messageIds) {

            if (TMMail.pageIs('conversation')) {
                var chainMessageId = mailBox.currentMessageId;
                setMessagesTag(tagId, [chainMessageId]);

                var messages = getConversationMessages();
                var i, len = messages.length;
                for (i = 0; i < len; i++) {
                    var messageId = $(messages[i]).attr('message_id');
                    messagePage.setTag(messageId, tagId);
                }
            } else {
                setCurrentMessageTag(tagId);
            }

            // Google Analytics
            window.ASC.Mail.ga_track(
                TMMail.pageIs('message') ? ga_Categories.message : ga_Categories.createMail, ga_Actions.buttonClick, "set_tag");
        } else {
            // Google Analytics
            window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.buttonClick, "set_tag");

            setMessagesTag(tagId, messageIds);
        }

        folderFilter.update();
    }

    function setCurrentMessageTag(tagId) {
        var messageId = mailBox.currentMessageId;
        if (TMMail.pageIs('conversation')) {
            messageId = messagePage.getActualConversationLastMessageId();
        }

        messagePage.setTag(messageId, tagId);

        if (mailBox.currentMessageId < 1) {
            return;
        }

        tagsManager.increment(tagId);
        setTagToCachedMessageLine(tagId, messageId);
        serviceManager.setTag([messageId], tagId, { tag_id: tagId, message_id: messageId });
    }

    function setMessagesTag(tagId, messageIds, fromMessage) {
        if (typeof(messageIds) === 'undefined') {
            messageIds = mailBox._Selection.GetIds();
        }

        var ids = [], i, len;
        for (i = 0, len = messageIds.length; i < len; i++) {
            if (!messageHasTag(tagId, messageIds[i])) {
                setTagToCachedMessageLine(tagId, messageIds[i]);
                tagsManager.increment(tagId);
                ids.push(messageIds[i]);
                mailCache.setTag(messageIds[i], tagId);
            }
        }

        if (ids.length > 0) {
            if (commonSettingsPage.isConversationsEnabled() &&
            (fromMessage == undefined || fromMessage == false)) {
                serviceManager.setConverstationsTag(ids, tagId, { ids: ids, tag_id: tagId });
            }

            serviceManager.setTag(ids, tagId, { tag_id: tagId, ids: ids });
        }
    }

    function unsetTag(tagId, messageIds) {
        tagsDropdown.hide();

        if (!TMMail.pageIs('sysfolders') && !messageIds[0]) {
            unsetCurrentMessageTag(tagId);
        } else {
            unsetMessagesTag(tagId, messageIds);
        }

        folderFilter.update();
    }

    function unsetConversationsTag(tagId) {
        tagsDropdown.hide();
        var messageIds = [];
        if (TMMail.pageIs('sysfolders')) {
            messageIds = mailBox._Selection.GetIds();
        } else if (TMMail.pageIs('conversation')) {
            messageIds = messagePage.getCurrentConversationIds();
        } else {
            messageIds.push(mailBox.currentMessageId);
        }
        unsetMessagesTag(tagId, messageIds, true);
    }

    function unsetMessagesTag(tagId, messageIds, fromConversation) {
        if (typeof(messageIds) === 'undefined') {
            messageIds = mailBox._Selection.GetIds();
        }

        var ids = [];
        for (var i = 0; i < messageIds.length; i++) {
            if (messageHasTag(tagId, messageIds[i])) {
                var messages;
                if (TMMail.pageIs('conversation')) {
                    messages = getConversationMessages();
                    var foundTags = messages.find('.tagDelete[tagid="' + tagId + '"]');
                    if (foundTags.length == 1) {
                        var k, len = messages.length;
                        for (k = 0; k < len; k++) {
                            var messageId = $(messages[k]).attr('message_id');
                            unsetTagFromCachedMessageLine(tagId, messageId);
                        }
                        tagsManager.decrement(tagId);
                    }
                } else {
                    unsetTagFromCachedMessageLine(tagId, messageIds[i]);
                    tagsManager.decrement(tagId);
                }
                ids.push(messageIds[i]);
            }
            messagePage.unsetTag(messageIds[i], tagId);
        }
        if (ids.length > 0) {
            if (!commonSettingsPage.isConversationsEnabled() || fromConversation == undefined || fromConversation == false) {
                serviceManager.unsetTag(ids, tagId, { ids: ids, tag_id: tagId });
            } else {
                serviceManager.unsetConverstationsTag(ids, tagId, { ids: ids, tag_id: tagId });
            }
        }

        if ($.inArray(tagId.toString(), MailFilter.getTags()) >= 0) {
            markFolderAsChanged(currentFolderId);
            updateAnchor();
        }
    }

    function unsetCurrentMessageTag(tagId) {
        messagePage.unsetTag(mailBox.currentMessageId, tagId);
        unsetTagFromCachedMessageLine(tagId, mailBox.currentMessageId);

        if (mailBox.currentMessageId < 1) {
            return;
        }

        tagsManager.decrement(tagId);
        serviceManager.unsetTag([mailBox.currentMessageId], tagId, { ids: [mailBox.currentMessageId], tag_id: tagId });
    }

    // __________________________________________ cached messages tags panels _________________________________________

    function getMessageTagInfo(messageId) {
        var $el = $('.messages .row[data_id="' + messageId + '"] .subject a');
        if (0 == $el.length) {
            return undefined;
        }

        var tagsStr = $el.attr('_tags');
        return { '$el': $el, 'tags_str': tagsStr };
    }

    function updateTagsLine($el) {
        $el.find('.tag').add($el.find('.more-tags')).remove();
        var tagsStr = $el.attr('_tags');

        if ('' == tagsStr) {
            return;
        }
        var tags = [];
        var tagsIds = tagsStr.split(','), tag, i;

        // Search and remove old deleted crm tags
        for (i = 0; i < tagsIds.length; i++) {
            tag = tagsManager.getTag(tagsIds[i]);
            if (tag == undefined) {
                mailBox.unsetTag(tags[i], [message_id]);
            } else {
                tags.push(tag);
            }
        }

        var $tagsMarkup = $.tmpl('messageItemTagsTmpl', getTagsToDisplay(tags), { htmlEncode: TMMail.htmlEncode });
        var $moreMarkup = $.tmpl('messageItemTagsMoreTmpl', getCountTagsInMore(tags));

        if ($moreMarkup.length > 0) {
            $tagsMarkup.push($moreMarkup[0]);
        }

        $el.prepend($tagsMarkup);
        processTagsMore($el);
    }

    function setTagToCachedMessageLine(tagId, messageId) {
        var info = getMessageTagInfo(messageId);
        if (!info) {
            return;
        }
        if ('' === info.tags_str) {
            info.tags_str = '' + tagId;
        } else {
            info.tags_str += ',' + tagId;
        }
        info.$el.attr('_tags', info.tags_str);
        updateTagsLine(info.$el, messageId);
    }

    function unsetTagFromCachedMessageLine(tagId, messageId) {
        var info = getMessageTagInfo(messageId);
        if (!info) {
            return;
        }
        var tags = info.tags_str.split(',');
        var newTagsStr = '';
        $.each(tags, function(index, value) {
            if (value != tagId) {
                if ('' != newTagsStr) {
                    newTagsStr += ',';
                }
                newTagsStr += value;
            }
        });
        info.$el.attr('_tags', newTagsStr);
        updateTagsLine(info.$el, messageId);
    }

    function messageHasTag(tagId, messageId) {
        if (TMMail.pageIs('message') || TMMail.pageIs('conversation')) {

            var tagsPanel = $('#itemContainer .head[message_id=' + messageId + '] .tags');
            if (tagsPanel) {
                var delEl = tagsPanel.find('.value .itemTags .tagDelete[tagid="' + tagId + '"]');
                if (delEl.length) {
                    return true;
                }
            }
        } else {
            var info = getMessageTagInfo(messageId);
            if (!info) {
                return false;
            }
            var tags = info.tags_str.split(',');
            for (var i = 0; i < tags.length; i++) {
                if (tags[i] == tagId) {
                    return true;
                }
            }
        }
        return false;
    }

    function messageTagsIds(messageId) {
        var info = getMessageTagInfo(messageId);
        if (!info) {
            return [];
        }
        return info.tags_str.split(',');
    }

    function getSelectedMessagesUsedTags() {
        var res = [];
        var selectedIds = currentSelection.GetIds();
        for (var i = 0; i < selectedIds.length; i++) {
            var tagsIds = messageTagsIds(selectedIds[i]);
            if (0 == i) {
                res = tagsIds;
            } else {
                var temp = [];
                $.each(res, function(index, value) {
                    if (-1 != $.inArray(value, tagsIds)) {
                        temp.push(value);
                    }
                });
                res = temp;
                if (0 == res.length) {
                    return [];
                }
            }
        }
        res = $.map(res, function(v) { return parseInt(v); });
        return res;
    }

    function onDeleteTag(e, tagId) {
        var els = $('.messages .row .subject a[_tags*="' + tagId + '"]');
        $.each(els, function(index, el) {
            var messageId = $(el).closest('.row[data_id]').attr('data_id');
            if (messageHasTag(tagId, messageId)) {
                unsetTagFromCachedMessageLine(tagId, messageId);
            }
        });
    }

    function onUpdateTag(e, tag) {
        var els = $('.messages .row .subject a[_tags*="' + tag.id + '"]');
        $.each(els, function(index, el) {
            var $el = $(el);
            var messageId = $el.closest('.messages .row[data_id]').attr('data_id');
            if (messageHasTag(tag.id, messageId)) {
                updateTagsLine($el, messageId);
            }
        });
    }

    function setMessagesReadUnread(messageIds, asRead, skipRefreshAll) {
        var status = asRead === true ? 'read' : 'unread';
        serviceManager.markMessages(messageIds, status, { status: status, messageIds: messageIds, folderId: MailFilter.getFolder() });
        if (!skipRefreshAll)
            serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
        else
            serviceManager.getMailFolders();
        mailCache.setRead(messageIds, asRead);
    }

    function setConversationReadUnread(messageIds, asRead, skipRefreshAll) {
        var status = asRead === true ? 'read' : 'unread';
        serviceManager.markConversations(messageIds, status, { status: status, messageIds: messageIds, folderId: MailFilter.getFolder() });
        if (!skipRefreshAll)
            serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
        else
            serviceManager.getMailFolders();
        mailCache.setRead(messageIds, asRead);
    }

    function updateMessageImportance(id, importance) {
        var status = importance === true ? 'important' : 'normal';
        serviceManager.markMessages([id], status, { status: status, messageIds: [id], folderId: MailFilter.getFolder() });
    }

    function updateConversationImportance(id, importance) {
        var status = importance === true ? 'important' : 'normal';
        serviceManager.markConversations([id], status);
    }

    // Removes messages from the specified folder to trash. And removes them completely if we delete them from trash.

    function deleteMessages(ids, fromFolder, fromConversation) {
        if (fromFolder == TMMail.sysfolders.trash.id) {
            serviceManager.deleteMessages(ids, { fromFolder: fromFolder, fromConversation: fromConversation }, {}, window.MailScriptResource.DeletionMessage);
            serviceManager.updateFolders();
        } else {
            moveMessages(ids, fromFolder, TMMail.sysfolders.trash.id, fromConversation);
        }
    }

    // Removes conversations from the specified folder to trash, or removes them completely if we delete them from trash.

    function deleteConversations(ids, fromFolder) {
        if (fromFolder == TMMail.sysfolders.trash.id ||
            fromFolder == TMMail.sysfolders.spam.id) {
            serviceManager.deleteConversations(ids, { fromFolder: fromFolder }, {}, window.MailScriptResource.DeletionMessage);
            mailCache.remove(ids);
            serviceManager.updateFolders();
        } else {
            moveConversations(ids, fromFolder, TMMail.sysfolders.trash.id);
        }
    }

    function deleteConversation(id) {
        return deleteConversations([id], currentFolderId);
    }

    function createEmailToSender(id) {
        var autor = $('#itemContainer .messages .row[data_id="' + id + '"] .author');
        var email = autor.attr('email');
        var name = autor.attr('title');

        var address = new ASC.Mail.Address(name, email);

        messagePage.setToEmailAddresses([address.ToString()]);
        messagePage.composeTo();
    }

    function markImportant(id) {
        var row = $('#itemContainer .messages .row[data_id="' + id + '"]');
        var isImportant = row.find('.icon-important').length ? true : false;
        var icon = row.find('[class^="icon-"], [class*=" icon-"]');
        updateConversationImportance(id, !isImportant);
        icon.toggleClass('icon-unimportant').toggleClass('icon-important');
        changeImportanceTitle(icon, !isImportant);
    }

    function openConversation(id) {
        if (TMMail.pageIs('drafts')) {
            TMMail.moveToDraftItem(id);
        } else {
            TMMail.moveToConversation(id);
        }
    }

    function openNewTabConversation(id) {
        if (TMMail.pageIs('drafts')) {
            TMMail.openDraftItem(id);
        } else {
            TMMail.openConversation(id);
        }
    }

    function moveToFolder() {
        switch (currentFolderId) {
            case TMMail.sysfolders.inbox.id:
                spamGroupOperation();
                break;
            case TMMail.sysfolders.spam.id:
            case TMMail.sysfolders.trash.id:
                restoreGroupOperation();
                break;
        }
    }

    function moveToConversationPrint(conversationId) {
        TMMail.moveToConversationPrint(conversationId);
    }

    function deleteMessage(id, fromFolder, fromConversation) {
        deleteMessages([id], fromFolder, fromConversation);
    }

    function moveMessages(ids, fromFolder, toFolder, fromConversation) {
        serviceManager.moveMailMessages(ids, toFolder, { fromFolder: fromFolder, toFolder: toFolder, fromConversation: fromConversation }, {}, window.MailScriptResource.MovingMessages);
        if (toFolder == TMMail.sysfolders.trash.id ||
            toFolder == TMMail.sysfolders.spam.id) {
            mailCache.remove(ids);
        } else {
            mailCache.setFolder(ids, toFolder);
        }
        serviceManager.updateFolders();
    }

    function moveMessage(id, fromFolder, toFolder, fromConversation) {
        return moveMessages([id], fromFolder, toFolder, fromConversation);
    }

    function moveConversations(ids, fromFolder, toFolder) {
        serviceManager.moveMailConversations(ids, toFolder, { fromFolder: fromFolder, toFolder: toFolder }, {}, window.MailScriptResource.MovingMessages);
        if (toFolder == TMMail.sysfolders.trash.id || 
            toFolder == TMMail.sysfolders.spam.id) {
            mailCache.remove(ids);
        } else {
            mailCache.setFolder(ids, toFolder);
        }
        serviceManager.updateFolders();
    }

    function moveConversation(id, fromFolder, toFolder) {
        return moveConversations([id], fromFolder, toFolder);
    }

    function restoreMessages(ids) {
        var folderIdsForUpdate = new TMContainers.IdMap();

        for (var i = 0; i < ids.length; i++) {
            var prevfolderid = $('div[data_id="' + ids[i].messageId + '"]').attr('prevfolderid');
            folderIdsForUpdate.AddId(prevfolderid);
        }
        serviceManager.restoreMailMessages(ids, { folders: folderIdsForUpdate }, {}, window.MailScriptResource.RestoringMessages);

        mailCache.setFolder(ids, TMMail.sysfolders.inbox.id);

        serviceManager.updateFolders();
    }

    function restoreConversations(ids) {
        serviceManager.restoreMailConversations(ids, {
            folders: [
                TMMail.sysfolders.inbox.id,
                TMMail.sysfolders.sent.id,
                TMMail.sysfolders.drafts.id,
                TMMail.sysfolders.trash.id,
                TMMail.sysfolders.spam.id
            ]
        }, {}, window.MailScriptResource.RestoringMessages);

        mailCache.setFolder(ids, TMMail.sysfolders.inbox.id);

        serviceManager.updateFolders();
    }

    function hidePages() {
        tagsPage.hide();
        accountsPage.hide();
        administrationPage.hide();
        contactsPage.hide();
        blankPages.hide();
        helpPage.hide();
        commonSettingsPage.hide();
    }

    function onRemoveMailFolderMessages(params, folder) {
        if ((folder == TMMail.sysfolders.trash.id && TMMail.pageIs('trash'))
            || (folder == TMMail.sysfolders.spam.id && TMMail.pageIs('spam'))
            || (MailFilter.getFolder() == folder && TMMail.pageIs('message'))) {
            serviceManager.updateFolders();
        } else {
            markFolderAsChanged(folder);
        }
        TMMail.showCompleteActionHint(TMMail.action_types.clear_folder, null, null, folder);
    }

    function showFolder(id, $html, scrollTo) {
        messagePage.hide();
        hidePages();

        if (!accountsManager.any()) {
            folderFilter.hide();
            blankPages.showEmptyAccounts();
            return;
        }

        $('#itemContainer').height('auto');

        var prevPageLink = $('#itemContainer .pagerPrevButtonCSSClass:visible');

        hideContentDivs(true);

        if ($html.find('[data_id]').length) {
            $html.show();
            blankPages.hide();
            folderFilter.show();
            changeTagStyle();
            $('#itemContainer .contentMenuWrapper').show();
            itemsHandler();
            folderFilter.update();
            redrawNavigation($html.attr('has_next') ? true : false, $html.attr('has_prev') ? true : false);
        } else {

            if (prevPageLink.length) {
                prevPageLink.trigger("click");
                return;
            }

            $('#MessagesListGroupButtons').hide();
            if (MailFilter.isBlank()) {
                folderFilter.hide();
                blankPages.showEmptyFolder();
            } else {
                folderFilter.show();
                blankPages.showNoLettersFilter();
            }
        }

        $(window).scrollTop(scrollTo);

        TMMail.setPageHeaderFolderName(id);
    }

    function loadCache(messages) {
        if (!window.commonSettingsPage.AutocacheMessagesEnabled())
            return;

        var unreadIds = [],
            notCachedIds = [];
        for (var i = 0, n = messages.length; i < n; i++) {
            var message = messages[i];
            if (message.isNew) {
                unreadIds.push(message.id);
            }
        }

        notCachedIds = mailCache.findMissingIds(unreadIds);

        if (notCachedIds.length > 0)
            mailCache.loadToCache(notCachedIds);
    }

    function onGetMailConversations(params, messages) {
        if (undefined == messages.length) {
            return;
        }

        var curScroll = $(window).scrollTop();

        hideLoadingMask();

        var hasNext, hasPrev;

        if (commonSettingsPage.isConversationsEnabled()) {
            hasNext = (true === MailFilter.getPrevFlag()) || params.__total > MailFilter.getPageSize();
            hasPrev = (false === MailFilter.getPrevFlag() &&
                null != MailFilter.getFromDate() &&
                undefined != MailFilter.getFromDate()) ||
            (true === MailFilter.getPrevFlag() && params.__total > MailFilter.getPageSize());
        } else {
            hasNext = params.__total > MailFilter.getPageSize() * MailFilter.getPage();
            hasPrev = MailFilter.getPage() > 1;
        }

        if ((0 == messages.length && MailFilter.getFromDate()) || (false == hasPrev && true == MailFilter.getPrevFlag())) {
            MailFilter.setFromDate(null);
            MailFilter.setPrevFlag(false);
            MailFilter.setFromMessage(0);
            keepSelectionOnReload = true;
            var newAnchor = "#" + TMMail.getSysFolderNameById(MailFilter.getFolder()) + MailFilter.toAnchor(true);
            ASC.Controls.AnchorController.move(newAnchor);
        }

        var folderId = MailFilter.getFolder();
        if (params.folder_id != folderId) {
            return;
        }

        if (params.folder_id == TMMail.sysfolders.inbox.id) {
            var data = MailFilter.toData();
            if (data.sortorder === "descending" && !data.hasOwnProperty("from_date"))
                loadCache(messages);
        }

        var filter = MailFilter.toAnchor(true);
        var folder = TMMail.getSysFolderNameById(folderId);
        var anchor = folder + filter;

        anchor = anchor.replace(/'/g, "%27");
        if ($('#itemContainer .messages[anchor="' + anchor + '"]').length) {
            $('#itemContainer .messages[anchor="' + anchor + '"]').remove();
        }

        var tagsIds = $.map(tagsManager.getAllTags(), function (value) { return value.id; });
        var isNeedRefreshTags = false;

        $.each(messages, function (i, m) {
            var address
            if (params.folder_id == TMMail.sysfolders.sent.id || params.folder_id == TMMail.sysfolders.drafts.id) {
                address = ASC.Mail.Utility.ParseAddress(m.to);
            } else {
                address = ASC.Mail.Utility.ParseAddress(m.from);
            }

            m.sender = address.email;
            m.author = address.name || "";

            m.displayDate = window.ServiceFactory.getDisplayDate(window.ServiceFactory.serializeDate(m.receivedDate));
            m.displayTime = window.ServiceFactory.getDisplayTime(window.ServiceFactory.serializeDate(m.receivedDate));

            // remove tags wich doesn't exist
            m.tagIds = $.grep(m.tagIds, function(v) {
                if (0 > $.inArray(v, tagsIds)) {
                    mailBox.unsetTag(v, m.id);
                    return false;
                }
                return true;
            });

            // add tags objects array
            m.tags = [];
            $.each(m.tagIds, function(index, id) {
                var tag = tagsManager.getTag(id);
                if (tag != undefined) {
                    if (tag.lettersCount == 0) {
                        isNeedRefreshTags = true;
                    }
                    m.tags.push(tag);
                }
            });

            m.subject = m.subject || '';

            if (m.folder == TMMail.sysfolders.drafts.id) {
                m.anchor = '#draftitem/' + m.id;
            } else {
                m.anchor = (commonSettingsPage.isConversationsEnabled() ? '#conversation/' : '#message/') + m.id;
            }
        });

        var html = $.tmpl(
            'messagesTmpl',
            {
                messages: messages,
                has_next: hasNext,
                has_prev: hasPrev
            },
            {
                htmlEncode: TMMail.htmlEncode,
                getTagsToDisplay: getTagsToDisplay,
                getCountTagsInMore: getCountTagsInMore
            });
        // HTML is set here
        var $html = $(html);
        $html.hide();
        $html.attr('anchor', anchor);

        processTagsMore($html);

        if ($html.find('[data_id]').length) {
            $('#itemContainer .contentMenuWrapper.messagesList').after($html);
            $('#itemContainer .messages').actionMenu('messagesActionMenu', actionButtons, pretreatment);
        }

        if (TMMail.pageIs('sysfolders')) {
            showFolder(params.folder_id, $html, curScroll);
        }

        if (isNeedRefreshTags)
            serviceManager.getTags();

        stickActionMenuToTheTop();
        updateGroupButtonsComposition();
        updateView();
        updateSelectionView();
        commonLettersButtonsState();
    }

    function pretreatment(id) {

        var row = $('#itemContainer .messages .row[data_id="' + id + '"]');

        if (!currentSelection.HasId(id)) {
            overallDeselectAll();
            selectRow(row);
        }

        var messageIds = currentSelection.GetIds();

        if (messageIds.length > 1) {
            $('#messagesActionMenu .openMail').parent().hide();
            $('#messagesActionMenu .openNewTabMail').parent().hide();
            $('#messagesActionMenu .replyMail').parent().hide();
            $('#messagesActionMenu .replyAllMail').parent().hide();
            $('#messagesActionMenu .createEmail').parent().hide();
            $('#messagesActionMenu .forwardMail').parent().hide();
            $('#messagesActionMenu .markImportant').parent().hide();
            $('#messagesActionMenu .setReadMail').parent().show();
            $('#messagesActionMenu .printMail').parent().hide();
        } else if (TMMail.pageIs('drafts')) {
            $('#messagesActionMenu .openMail').parent().show();
            $('#messagesActionMenu .openNewTabMail').parent().show();
            $('#messagesActionMenu .replyMail').parent().hide();
            $('#messagesActionMenu .replyAllMail').parent().hide();
            $('#messagesActionMenu .createEmail').parent().hide();
            $('#messagesActionMenu .forwardMail').parent().hide();
            $('#messagesActionMenu .setReadMail').parent().hide();
            $('#messagesActionMenu .markImportant').parent().show();
            $('#messagesActionMenu .printMail').parent().hide();
        } else {
            $('#messagesActionMenu .openMail').parent().show();
            $('#messagesActionMenu .openNewTabMail').parent().show();
            $('#messagesActionMenu .replyMail').parent().show();
            $('#messagesActionMenu .replyAllMail').parent().show();
            $('#messagesActionMenu .createEmail').parent().show();
            $('#messagesActionMenu .forwardMail').parent().show();
            $('#messagesActionMenu .setReadMail').parent().show();
            $('#messagesActionMenu .markImportant').parent().show();
            $('#messagesActionMenu .printMail').parent().show();
        }

        var readUnread = containNewMessage(messageIds);
        var readItem = $('#messagesActionMenu .setReadMail');
        if (readUnread) {
            readItem.html(readItem.attr('read'));
        } else {
            readItem.html(readItem.attr('unread'));
        }

        if (messageIds.length == 1) {
            var isImportant = row.find('.icon-important').length ? true : false;
            var importantItem = $('#messagesActionMenu .markImportant');
            if (isImportant) {
                importantItem.html(importantItem.attr('not_important'));
            } else {
                importantItem.html(importantItem.attr('important'));
            }
        }

        var moveItem = $('#messagesActionMenu .moveToFolder');
        moveItem.show();
        switch (currentFolderId) {
            case TMMail.sysfolders.inbox.id:
                moveItem.html(moveItem.attr('spam'));
                break;
            case TMMail.sysfolders.spam.id:
                moveItem.html(moveItem.attr('not_spam'));
                break;
            case TMMail.sysfolders.trash.id:
                moveItem.html(moveItem.attr('restore'));
                break;
            default:
                moveItem.hide();
        }
    }

    function containNewMessage(messageIds) {
        var isContain = false;
        for (var i = 0; i < messageIds.length; i++) {
            var row = $('#itemContainer .messages .row[data_id="' + messageIds[i] + '"]');
            if (row.is('.new')) {
                isContain = true;
                break;
            }
        }
        return isContain;
    }

    function getTagsToDisplay(tags) {
        var displayTags = [];

        if (tags != undefined) {
            var i, len = tags.length;
            for (i = 0; i < len && i < maxDisplayedTagsCount; i++) {
                displayTags.push(tags[i]);
            }
        }
        return displayTags;
    }

    function getCountTagsInMore(tags) {
        if (tags != undefined) {
            return tags.length - maxDisplayedTagsCount;
        }
        return 0;
    }

    function processTagsMore($html) {
        $html.find('.more-tags').unbind('.processTagsMore').bind('click.processTagsMore', function(event) {
            var $this = $(this);
            $this.unbind('.processTagsMore');
            var tagsIds = $this.parent().attr('_tags').split(',');
            var buttons = [];
            for (var i = maxDisplayedTagsCount; i < tagsIds.length; i++) {
                var tag = tagsManager.getTag(tagsIds[i]);
                if (tag) {
                    buttons.push({ 'text': TMMail.htmlEncode(tag.name), 'disabled': true });
                }
            }
            $this.actionPanel({ 'buttons': buttons, 'show': true });
            event.preventDefault();
        });
    }

    function redrawNavigation(hasNext, hasPrev) {
        PagesNavigation.RedrawFolderNavigationBar(window.mailPageNavigator,
            TMMail.option('MessagesPageSize'),
            onChangePageSize,
            hasNext,
            hasPrev);
        PagesNavigation.RedrawPrevNextControl();
    }

    function onChangePageSize(pageSize) {
        if (isNaN(pageSize) || pageSize < 1) {
            return;
        }
        TMMail.option('MessagesPageSize', pageSize);
        MailFilter.setPageSize(pageSize);

        keepSelectionOnReload = true;
        updateAnchor(true);
    }

    function actionPanelSelect(selectorSuffix) {
        $('.messages:visible .row' + selectorSuffix).each(function() {
            selection.AddId($(this).attr('data_id'));
        });

        updateSelectionView();
        commonLettersButtonsState();
        hidePages();
    }

    function actionPanelSelectAll() {
        // google analitics track
        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.actionClick, 'all_select');

        $('.messages:visible .row').each(function() {
            selection.AddId($(this).attr('data_id'));
        });

        lastSelectedConcept = selectionConcept.all;
        updateSelectionView();
        commonLettersButtonsState();
        hidePages();
    }

    function actionPanelSelectNone() {
        // google analitics track
        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.actionClick, 'none_select');
        unselectAll();
    }

    function unselectAll() {
        $('.messages:visible .row').each(function() {
            selection.RemoveId($(this).attr('data_id'));
        });

        lastSelectedConcept = null;
        updateSelectionView();
        commonLettersButtonsState();
        hidePages();
    }

    function actionPanelSelectUnread() {
        // google analitics track
        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.actionClick, 'unread_select');

        unselectAll();
        lastSelectedConcept = selectionConcept.unread;
        actionPanelSelect('.new');
    }

    function actionPanelSelectRead() {
        // google analitics track
        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.actionClick, 'read_select');

        unselectAll();
        lastSelectedConcept = selectionConcept.read;
        actionPanelSelect(':not(.new)');
    }

    function actionPanelSelectImportant() {
        // google analitics track
        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.actionClick, 'important_select');

        unselectAll();
        lastSelectedConcept = selectionConcept.important;
        actionPanelSelect(':has(.icon-important)');
    }

    function actionPanelSelectWithAttachments() {
        // google analitics track
        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.actionClick, 'whith_atachments_select');

        unselectAll();
        lastSelectedConcept = selectionConcept.with_attachments;
        actionPanelSelect(':has(.icon-attachment)');
    }

    function changeImportanceTitle(element, importance) {
        var title;
        if (importance) {
            title = MailScriptResource.ImportantLabel;
        } else {
            title = MailScriptResource.NotImportantLabel;
        }
        element.attr('title', title);
    }

    function itemsHandler() {
        var $messages = $('.messages:visible .row');

        // set message importance flag click handler
        $messages.find('.importance').unbind('click').bind('click', function() {
            var $this = $(this);
            var icon = $this.find('[class^="icon-"], [class*=" icon-"]');
            var newimportance = icon.is('.icon-unimportant');
            var msgId = $this.parent().attr('data_id');
            mailBox.updateConversationImportance(msgId, newimportance);
            icon.toggleClass('icon-unimportant').toggleClass('icon-important');
            changeImportanceTitle(icon, newimportance);
            mailCache.setImportant(msgId, newimportance);
        });

        // _Selection checkbox clicked
        $messages.find('.checkbox').unbind('click').bind('click', function() {
            selectRow($(this).parent());
        });
    }

    function selectRow(row) {
        var messageId = row.attr('data_id');
        var checkbox = row.find('input[type="checkbox"]');
        if (row.is('.selected')) {
            selection.RemoveId(messageId);
            currentSelection.RemoveId(messageId);
            checkbox.prop('checked', false);
        } else {
            selection.AddId(messageId);
            currentSelection.AddId(messageId);
            checkbox.prop('checked', true);
        }
        row.toggleClass('selected');
        var messagesTable = row.parent();
        updateSelectionComboCheckbox(messagesTable);
        lastSelectedConcept = null;
        updateOverallSelectionLine();
        commonLettersButtonsState();
    }

    function groupButtonsMenuHandlers() {
        // Delete (group button)
        $('#MessagesListGroupButtons .menuActionDelete').click(function() {
            if ($(this).hasClass('unlockAction')) {
                deleteGroupOperation();
            }
        });

        // Spam (group button)
        $('#MessagesListGroupButtons .menuActionSpam').click(function() {
            if ($(this).hasClass('unlockAction')) {
                spamGroupOperation();
            }
        });

        // NotSpam (group button)
        $('#MessagesListGroupButtons .menuActionNotSpam').click(function() {
            if ($(this).hasClass('unlockAction')) {
                restoreGroupOperation();
            }
        });

        // Read/Unread (group button)
        $('#MessagesListGroupButtons .menuActionRead').click(function() {
            if ($(this).hasClass('unlockAction')) {
                readUnreadGroupOperation();
            }
        });

        // Restore (group button)
        $('#MessagesListGroupButtons .menuActionRestore').click(function() {
            if ($(this).hasClass('unlockAction')) {
                restoreGroupOperation();
            }
        });

        // Add tag (group button)
        $('#MessagesListGroupButtons .menuActionAddTag').click(function() {
            if ($(this).hasClass('unlockAction')) {
                tagsDropdown.show($(this));
            }
        });

        // Select all
        $('#SelectAllMessagesCB').unbind('click');
        $('#SelectAllMessagesCB').bind('click', function(e) {
            if (e.target.checked) {
                actionPanelSelectAll();
            } else {
                actionPanelSelectNone();
            }
            e.stopPropagation();
            $('#SelectAllMessagesDropdown').parent().actionPanel('hide');
        });

        // _Selection combo-checkbox dropdown
        $('#SelectAllMessagesDropdown').parent().actionPanel({
            buttons: [
                { text: window.MailScriptResource.AllLabel, handler: actionPanelSelectAll },
                { text: window.MailScriptResource.FilterUnread, handler: actionPanelSelectUnread },
                { text: window.MailScriptResource.FilterRead, handler: actionPanelSelectRead },
                { text: window.MailScriptResource.ImportantLabel, handler: actionPanelSelectImportant },
                { text: window.MailScriptResource.WithAttachments, handler: actionPanelSelectWithAttachments },
                { text: window.MailScriptResource.NoneLabel, handler: actionPanelSelectNone }
            ],
            css: 'stick-over'
        });

        $('#OverallDeselectAll').click(function() {
            overallDeselectAll();
        });
    }

    function deleteGroupOperation() {
        var messages = currentSelection.GetIds();
        if (messages.length > 0) {
            enableGroupOperations(false);
            if (commonSettingsPage.isConversationsEnabled())
                deleteConversations(messages, currentFolderId);
            else
                deleteMessages(messages, currentFolderId);
        }
    }

    function spamGroupOperation() {
        var messages = currentSelection.GetIds();
        if (messages.length > 0) {
            enableGroupOperations(false);
            if (commonSettingsPage.isConversationsEnabled())
                moveConversations(messages, currentFolderId, TMMail.sysfolders.spam.id);
            else
                moveMessages(messages, currentFolderId, TMMail.sysfolders.spam.id);
        }
    }

    function restoreGroupOperation() {
        var messagesIds = currentSelection.GetIds();
        if (messagesIds.length > 0) {
            enableGroupOperations(false);
            if (commonSettingsPage.isConversationsEnabled())
                restoreConversations(messagesIds);
            else
                restoreMessages(messagesIds);
        }
    }

    function readUnreadGroupOperation() {
        var messageIds = currentSelection.GetIds();
        if (messageIds.length > 0) {
            enableGroupOperations(false);
            var read = containNewMessage(messageIds);
            if (commonSettingsPage.isConversationsEnabled())
                setConversationReadUnread(messageIds, read);
            else
                setMessagesReadUnread(messageIds, read);
        }
    }

    function updateGroupButtonsComposition(folderId) {
        $('#MessagesListGroupButtons').show();

        if (folderId === undefined) {
            folderId = MailFilter.getFolder();
        }

        // Update selection combo-checkbox disabled state
        var $comboCheckbox = $('#SelectAllMessagesCB');
        if ($('.messages:visible .row').length == 0) {
            $comboCheckbox.attr('disabled', 'true');
            $('#SelectAllMessagesDropdown').attr('disabled', 'true');
        } else {
            $comboCheckbox.removeAttr('disabled');
            $('#SelectAllMessagesDropdown').removeAttr('disabled');
        }
        // Spam / Not spam
        $('#MessagesListGroupButtons .menuActionNotSpam').toggle(folderId == TMMail.sysfolders.spam.id);
        $('#MessagesListGroupButtons .menuActionSpam').toggle(folderId == TMMail.sysfolders.inbox.id);

        // Read / unread
        $('#MessagesListGroupButtons .menuActionRead').toggle(folderId != TMMail.sysfolders.drafts.id);

        // Restore
        $('#MessagesListGroupButtons .menuActionRestore').toggle(folderId == TMMail.sysfolders.trash.id);

        updateOverallSelectionLine();
    }

    function enableGroupOperations(enable) {
        $('#MessagesListGroupButtons .menuAction').toggleClass('unlockAction', enable);
    }

    function commonLettersButtonsState() {
        enableGroupOperations(currentSelection.Count() != 0);

        /* read/unread */

        var readUnread = false;

        $('.messages .row.new').each(function() {
            var $this = $(this);
            var messageid = $this.attr('data_id');
            if (currentSelection.HasId(messageid)) {
                readUnread = true;
                return false;
            }
            return true;
        });

        var $readBtn = $('#MessagesListGroupButtons .menuActionRead');
        if (readUnread) {
            $readBtn.attr('do', 'read');
            $readBtn.html('<span title="' + $readBtn.attr('read') + '">' + $readBtn.attr('read') + '</span>');
        } else {
            $readBtn.attr('do', 'unread');
            $readBtn.html('<span title="' + $readBtn.attr('unread') + '">' + $readBtn.attr('unread') + '</span>');
        }
    }

    // ReSharper disable UnusedParameter

    function onMarkMailMessages(params, ids) {
        // ReSharper restore UnusedParameter
        hidePages();

        var $allMainTables = $('.messages');
        var tablesIdx = $allMainTables.length;
        while (tablesIdx--) {
            var $mainTable = $($allMainTables[tablesIdx]);
            var tableAnchor = $mainTable.attr('anchor');
            if (MailFilter.anchorHasMarkStatus(params.status, tableAnchor)) {
                $mainTable.attr('changed', 'true');
                // Visible table must be updated right now
                if ($mainTable.is(':visible')) {
                    updateAnchor();
                }
            }
                // No need to update a table that has no marking status in filter (anchor)
            else {
                markMessageDivsInTable($mainTable, params.status, params.messageIds);
            }
        }

        if (!TMMail.pageIs('message')) {
            commonLettersButtonsState();
        }

        if (params.status == 'read' || params.status == 'unread') {
            mailCache.setRead(params.messageIds, params.status == 'read');
            serviceManager.getMailFolders();
        }
    }

    // prev next ids responces handlers
    // if zero returned - move to folder anchor

    function onGetNextPrevId(method, id) {
        if (0 != id) {
            method.apply(TMMail, [id]);
        } else {
            var filter = MailFilter.toAnchor(false);
            var folder = TMMail.getSysFolderNameById(MailFilter.getFolder());
            ASC.Controls.AnchorController.move(folder + filter);
        }
    }

    function onGetNextPrevConversationId(params, id) {
        onGetNextPrevId(TMMail.moveToConversation, id);
    }

    function onGetNextPrevMessageId(params, id) {
        onGetNextPrevId(TMMail.moveToMessage, id);
    }

    function markMessageDivsInTable($table, status, messageIds) {
        var $messages = $table.find('.row'),
            divIndex = $messages.length;
        while (divIndex--) {
            var $message = $($messages[divIndex]);
            var messageId = $message.attr('data_id');
            var messageIndex = messageIds.length;
            while (messageIndex--) {
                if (messageId === messageIds[messageIndex]) {
                    setStatusToMessage($message, status);
                    break;
                }
            }
        }
    }

    function setStatusToMessage(el, status) {
        if (status == 'read') {
            el.removeClass('new');
        } else if (status == 'unread') {
            el.addClass('new');
        } else {
            var icon;
            if (status == 'important') {
                icon = el.find('[class^="icon-"], [class*=" icon-"]');
                icon.attr('class', 'icon-important');
                changeImportanceTitle(icon, true);
            } else if (status == 'normal') {
                icon = el.find('[class^="icon-"], [class*=" icon-"]');
                icon.attr('class', 'icon-unimportant');
                changeImportanceTitle(icon, false);
            }
        }
    }

    function onRestoreMailMessages(params, ids) {
        // TODO: add a check on the inclusion of chains (is_conversation param)
        if (ids.length == 1) {
            var restoreFolderId = undefined;
            if (TMMail.pageIs('trash') || TMMail.pageIs('spam')) {
                restoreFolderId = parseInt($('#itemContainer .messages .row[data_id="' + ids[0] + '"]').attr('prevfolderid'));
            } else if (TMMail.pageIs('conversation') || TMMail.pageIs('message')) {
                restoreFolderId = parseInt($('#itemContainer .message-wrap[message_id="' + ids[0] + '"]').attr('restore_folder_id'));
            }

            if ($.isNumeric(restoreFolderId)) {
                TMMail.showCompleteActionHint(TMMail.action_types.restore, window.commonSettingsPage.isConversationsEnabled(), ids.length, restoreFolderId);
            }
        } else {
            TMMail.showCompleteActionHint(TMMail.action_types.restore, window.commonSettingsPage.isConversationsEnabled(), ids.length);
        }

        $.each(params.folders, function(index, value) {
            markFolderAsChanged(value);
        });

        updateView();
        updateAnchor(true);
        // Clear checkboxes
        selection.RemoveIds(ids);
        lastSelectedConcept = null;
    }

    function onRemoveMailMessages(params, ids) {
        // Mark the source folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.fromFolder);
        if (params.fromConversation == undefined || !params.fromConversation) {
            updateView();
            updateAnchor(true);
            selection.RemoveIds(ids); // Clear checkboxes
            lastSelectedConcept = null;

            TMMail.showCompleteActionHint(TMMail.action_types.delete_messages, window.commonSettingsPage.isConversationsEnabled(), ids.length);
        } else {
            for (var i = 0; i < ids.length; i++) {
                var deletedMessage = $('#itemContainer div.message-wrap[message_id=' + ids[i] + ']');
                if (deletedMessage != undefined) {
                    var conversationMessages = $('#itemContainer div.message-wrap');

                    if (conversationMessages.length > 1) {
                        deletedMessage.remove();
                    } else {
                        params.fromConversation = false;
                        onRemoveMailMessages(params, ids);
                    }
                }
            }
        }
    }

    function onMoveMailMessages(params, ids) {
        // Mark the source folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.fromFolder);
        // Mark the destination folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.toFolder);

        // Clear checkboxes
        selection.RemoveIds(ids);
        lastSelectedConcept = null;

        // if not conversation or single message in conversation
        if (!params.fromConversation || $('#itemContainer div.message-wrap').length <= 1) {

            TMMail.showCompleteActionHint(TMMail.action_types.move, false, ids.length, params.toFolder);

            if (moveNextIfPossible(params.toFolder)) {
                return;
            }

            updateAnchor(true);
            return;
        }

        $.each(ids, function(i, v) {
            var deletedMessage = $('#itemContainer div.message-wrap[message_id=' + v + ']');
            deletedMessage && deletedMessage.remove();
        });

        // if main conversation message was removed - fix anchor
        if (-1 != $.inArray(+mailBox.currentMessageId, ids)) {
            mailBox.currentMessageId = messagePage.getActualConversationLastMessageId();
            TMMail.moveToConversation(mailBox.currentMessageId, true);
        }

        TMMail.showCompleteActionHint(TMMail.action_types.move, false, ids.length, params.toFolder);

        if (params.fromConversation && $('#itemContainer div.message-wrap').length === 1) {
            $("#itemContainer #sort-conversation").hide();
            $("#itemContainer #collapse-conversation").hide();
            $('#itemContainer .itemWrapper .short-view').trigger("click");
            return;
        }
    }

    function onMoveMailConversations(params, ids) {
        // Mark the source folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.fromFolder);
        // Mark the destination folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.toFolder);

        // Clear checkboxes
        selection.RemoveIds(ids);
        lastSelectedConcept = null;

        TMMail.showCompleteActionHint(TMMail.action_types.move, true, ids.length, params.toFolder);

        if (moveNextIfPossible(params.toFolder)) {
            return;
        }

        updateAnchor(true);
    }

    function moveNextIfPossible(folder) {
        if (TMMail.pageIs('conversation') || TMMail.pageIs('message')) {
            if (window.commonSettingsPage.GoNextAfterMoveEnabled()) {
                var nextMessageLink = $('.itemWrapper .btnNext:visible');
                if (nextMessageLink.length && nextMessageLink.hasClass("unlockAction")) {
                    nextMessageLink.trigger("click");

                    messagePage.conversation_moved = true;
                    messagePage.dst_folder_id = folder;

                    return true;
                }
            }
        }

        return false;
    }

    function onSysFolderPage(folderName, params) {
        currentFolderId = TMMail.getSysFolderIdByName(folderName, TMMail.sysfolders.inbox.id);

        if (currentFolderId != folderPanel.getMarkedFolder()) {
            unmarkAllPanels(currentFolderId == MailFilter.getFolder());
        }

        // check al least one account exists
        // if not - shows stub page
        if (!accountsManager.any()) {
            hidePages();
            folderFilter.hide();
            messagePage.hide();
            hideContentDivs();
            blankPages.showEmptyAccounts();
            folderPanel.markFolder(currentFolderId);
            TMMail.setPageHeaderFolderName(currentFolderId);
            hideLoadingMask();
            return;
        }

        folderPanel.markFolder(currentFolderId);

        $(document).off('click', 'a[href]');

        // checks weather page size value in anchor is correct - replace anchor if not
        if (PagesNavigation.FixAnchorPageSizeIfNecessary(MailFilter.getPageSize())) {
            return;
        }

        // store page size setting for next session
        TMMail.option('MessagesPageSize', MailFilter.getPageSize());

        if (!keepSelectionOnReload) {
            overallDeselectAll();
        } else {
            keepSelectionOnReload = false;
        }

        MailFilter.fromAnchor(folderName, params);

        var filter = MailFilter.toAnchor(true);
        var folder = TMMail.getSysFolderNameById(MailFilter.getFolder());
        var anchor = folder + filter;
        var cache = $('#itemContainer .messages[anchor="' + anchor + '"][changed!="true"]');

        // if no cash exists - get filtered conversations
        if (0 == cache.length || isNeedCacheCorrect(cache)) {
            serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
            return;
        }

        hidePages();

        if (currentFolderId != folderPanel.getMarkedFolder()) {
            unmarkAllPanels();
        }

        messagePage.hide();
        hideContentDivs();
        $('#itemContainer .contentMenuWrapper').show();
        folderFilter.show();
        cache.show();
        stickActionMenuToTheTop();
        updateGroupButtonsComposition();
        updateSelectionView();
        commonLettersButtonsState();

        if (anchor.indexOf("page_size") > -1) {
            anchor = anchor.substring(0, anchor.indexOf("page_size"));
        } else if (anchor.indexOf("from_date") > -1) {
            anchor = anchor.substring(0, anchor.indexOf("from_date"));
        }

        if (anchor != folder + filter) {
            updateView();
        }

        redrawNavigation(cache.attr('has_next') ? true : false, cache.attr('has_prev') ? true : false);
        folderPanel.markFolder(currentFolderId);
        TMMail.setPageHeaderFolderName(currentFolderId);
        itemsHandler();
    }

    function updateView() {
        markTags();
        accountsPanel.mark();
        accountsPanel.updateAnchors();
        folderFilter.update();

        folderFilter.setUnread(MailFilter.getUnread());
        folderFilter.setImportance(MailFilter.getImportance());
        folderFilter.setWithCalendar(MailFilter.getWithCalendar());
        folderFilter.setAttachments(MailFilter.getAttachments());
        folderFilter.setFrom(MailFilter.getFrom());
        folderFilter.setTo(MailFilter.getTo());
        folderFilter.setPeriod({ period: MailFilter.getPeriod(), period_within: MailFilter.getPeriodWithin() });
        folderFilter.setSearch(MailFilter.getSearch());
        folderFilter.setTags(MailFilter.getTags());
        folderFilter.setSort(MailFilter.getSort(), MailFilter.getSortOrder());
    }

    function markTags() {
        tagsPanel.unmarkAllTags();
        $.each(MailFilter.getTags(), function(index, value) {
            tagsPanel.markTag(value);
        });
    }

    function onCompose() {
        unmarkAllPanels(false);
        mailBox.currentMessageId = -1;
        messagePage.onCompose();
    }

    function onComposeTo(params) {
        mailBox.currentMessageId = -1;
        unmarkAllPanels(false);
        if (params) {
            var emails = TMMail.getParamsValue(params, /email=([^\/]+)/);
            if (emails) {
                emails = decodeURIComponent(emails);
                messagePage.onComposeTo(emails.split(','));
            } else {
                var ids = TMMail.getParamsValue(params, /crm=([^\/]+)/);
                if (ids) {
                    serviceManager.getCrmContactsById({}, { contactids: ids.split(',') }, { success: onGetCrmContacts },
                        ASC.Resources.Master.Resource.LoadingProcessing);
                } else {
                    messagePage.onComposeTo();
                }
            }
        } else {
            messagePage.onComposeTo();
        }
    }

    function onGetCrmContacts(params, resp) {
        var addresses = [];
        var contactsInfo = [];
        for (var i = 0; i < resp.length; i++) {
            var contact = resp[i];
            if (contact.email.data) {
                var parsed = ASC.Mail.Utility.ParseAddresses(contact.email.data);
                if (parsed) {
                    addresses.push(parsed.addresses.map(function(a) {
                        if (!a.name)
                            a.name = contact.displayName;

                        return a.ToString();
                    }));
                } 
                contactsInfo.push({ Id: contact.id, Type: contact.type == 'contact' ? "1" : undefined });
            }
        }
        messagePage.onComposeFromCrm({ addresses: addresses, contacts_info: contactsInfo });
    }

    function onReplyPage(id) {
        mailBox.currentMessageId = -1;
        unmarkAllPanels(false);
        messagePage.reply(id);
    }

    function onReplyAllPage(id) {
        mailBox.currentMessageId = -1;
        unmarkAllPanels(false);
        messagePage.replyAll(id);
    }

    function onForwardPage(id) {
        mailBox.currentMessageId = -1;
        unmarkAllPanels(false);
        messagePage.forward(id);
    }

    function onDraftItemPage(id) {
        messagePage.edit(id);
    }

    function onMessagePage(id) {
        mailBox.currentMessageId = id;
        setUnreadInCache(id);
        messagePage.view(id, false);
    }

    function onNextMessagePage(id) {
        var cachedId = filterCache.getNextId(MailFilter, id);
        if (0 == cachedId) {
            serviceManager.getNextMessageId(id);
        } else {
            onGetNextPrevMessageId({}, cachedId);
        }
    }

    function onPrevMessagePage(id) {
        var cachedId = filterCache.getPrevId(MailFilter, id);
        if (0 == cachedId) {
            serviceManager.getPrevMessageId(id);
        } else {
            onGetNextPrevMessageId({}, cachedId);
        }
    }

    function onConversationPage(id) {
        mailBox.currentMessageId = id;
        setUnreadInCache(id);
        messagePage.conversation(id, false);
        updateSelectionView();
    }

    function onNextConversationPage(id) {
        var cachedId = filterCache.getNextId(MailFilter, id);
        if (0 == cachedId) {
            serviceManager.getNextConversationId(id);
        } else {
            onGetNextPrevConversationId({}, cachedId);
        }
    }

    function onPrevConversationPage(id) {
        var cachedId = filterCache.getPrevId(MailFilter, id);
        if (0 == cachedId) {
            serviceManager.getPrevConversationId(id);
        } else {
            onGetNextPrevConversationId({}, cachedId);
        }
    }

    function onTagsPage() {
        messagePage.hide();
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.Tags);
        hideContentDivs();
        tagsPage.show();
        settingsPanel.selectItem('tagsSettings');
        hideLoadingMask();
    }

    function onAccountsPage() {
        messagePage.hide();
        unmarkAllPanels();
        hidePages();
        TMMail.setPageHeaderTitle(window.MailScriptResource.AccountsLabel);
        hideContentDivs();
        accountsPage.show();
        settingsPanel.selectItem('accountsSettings');
        hideLoadingMask();
    }

    function onAdministrationPage() {
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.AdministrationLabel);
        contactsManager.init();
        administrationManager.loadData();
        settingsPanel.selectItem('adminSettings');
    }

    function onCommonSettingsPage() {
        messagePage.hide();
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.CommonSettingsLabel);
        hideContentDivs();
        commonSettingsPage.show();
        settingsPanel.selectItem('common');
        hideLoadingMask();
    }

    function onTeamLabContactsPage() {
        TMMail.setPageHeaderTitle(window.MailScriptResource.TeamLabContactsLabel);
        if (ASC.Mail.Constants.PEOPLE_AVAILABLE) {
            contactsPage.show('teamlab', 1);
        } else {
            TMMail.moveToInbox();
        }
    }

    function onCrmContactsPage() {
        TMMail.setPageHeaderTitle(window.MailScriptResource.CRMContactsLabel);
        if (ASC.Mail.Constants.CRM_AVAILABLE) {
            contactsPage.show('crm', 1);
        } else {
            TMMail.moveToInbox();
        }
    }

    function onPersonalContactsPage() {
        TMMail.setPageHeaderTitle(window.MailScriptResource.PersonalContactsLabel);
        contactsPage.show('custom', 1);
    }

    function onHelpPage(helpId) {
        TMMail.setPageHeaderTitle(window.MailScriptResource.HelpCenterLabel);
        messagePage.hide();
        unmarkAllPanels();
        hidePages();
        hideContentDivs();
        helpPanel.selectItem(helpId);
        helpPage.show(helpId);
    }

    function printParamParser(params) {
        var simIdsStr = TMMail.getParamsValue(params, /sim=([^&\/]+)/);
        var squIdsStr = TMMail.getParamsValue(params, /squ=([^&\/]+)/);
        var sortAscStr = TMMail.getParamsValue(params, /sortAsc=([^&\/]+)/);

        var simIds = [];
        if (simIdsStr) {
            simIds = simIdsStr.split(',');
        }
        
        var squIds = [];
        if (squIdsStr) {
            squIds = squIdsStr.split(',');
        }

        var sortAsc = true;
        if (sortAscStr) {
            sortAsc = sortAscStr == '1';
        }

        return {
            simIds: simIds,
            squIds: squIds,
            sortAsc: sortAsc
        };
    }

    function onMessagePrintPage(messageId, params) {
        var parser = printParamParser(params);
        printPage.init(false, messageId, parser.simIds, parser.squIds);
    }

    function onConversationPrintPage(conversationId, params) {
        var parser = printParamParser(params);
        printPage.init(true, conversationId, parser.simIds, parser.squIds, parser.sortAsc);
    }

    function updateAnchor(includePagingInfo, keepSel) {
        if (keepSel) {
            keepSelectionOnReload = true;
        }
        var filter = MailFilter.toAnchor(includePagingInfo),
            folder = TMMail.getSysFolderNameById(MailFilter.getFolder()),
            anchor = ASC.Controls.AnchorController.getAnchor(),
            newAnchor = (folder + filter).replace(/'/g, "%27");
        if (anchor != newAnchor) {
            ASC.Controls.AnchorController.move(newAnchor);
        }
    }

    function setUnreadInCache(messageId) {
        $('#itemContainer .messages .row[data_id="' + messageId + '"]').removeClass('new');
    }

    function setImportanceInCache(messageId) {
        var importance = $('#itemContainer .messages .row[data_id="' + messageId + '"] .importance');
        if (importance.length > 0) {
            var flag = $(importance).find('[class^="icon-"], [class*=" icon-"]');
            var newimportance = flag.is('.icon-unimportant');
            flag.toggleClass('icon-unimportant').toggleClass('icon-important');
            changeImportanceTitle(flag, newimportance);
            mailCache.setImportant(messageId, newimportance);
        }
    }

    function isNeedCacheCorrect(cache) {
        var incorrectRows = [];
        if (MailFilter.anchorHasMarkStatus('unread')) {
            if (MailFilter.getUnread()) {
                incorrectRows = cache.find('.row:not(.new)');
            } else {
                incorrectRows = cache.find('.row.new');
            }
        }

        if (incorrectRows.length == 0 && MailFilter.anchorHasMarkStatus('important')) {
            if (MailFilter.getImportance()) {
                incorrectRows = cache.find('.row .importance .icon-unimportant');
            } else {
                incorrectRows = cache.find('.row .importance .icon-important');
            }
        }

        return incorrectRows.length ? true : false;
    }

    function hideLoadingMask() {
        if (!pageIsLoaded) {
            //remove loading mask element
            $('.loader-page').remove();
            $('body').css('overflow', 'auto');
            pageIsLoaded = true;
        }
    }

    function keepSelection(value) {
        keepSelectionOnReload = value;
    }

    return {
        currentMessageId: currentMessageId,

        init: init,
        setMessagesReadUnread: setMessagesReadUnread,
        setConversationReadUnread: setConversationReadUnread,
        moveMessage: moveMessage,
        moveConversation: moveConversation,
        deleteMessage: deleteMessage,
        deleteConversation: deleteConversation,
        updateMessageImportance: updateMessageImportance,
        updateConversationImportance: updateConversationImportance,
        setTag: setTag,
        unsetTag: unsetTag,
        unsetConversationsTag: unsetConversationsTag,
        updateAnchor: updateAnchor,
        hideContentDivs: hideContentDivs,
        hidePages: hidePages,
        unmarkAllPanels: unmarkAllPanels,
        markFolderAsChanged: markFolderAsChanged,
        getSelectedAddresses: getSelectedAddresses,
        onChangePageSize: onChangePageSize,
        restoreConversations: restoreConversations,
        restoreMessages: restoreMessages,
        getSelectedMessagesUsedTags: getSelectedMessagesUsedTags,
        _Selection: currentSelection,
        groupButtonsMenuHandlers: groupButtonsMenuHandlers,
        hideLoadingMask: hideLoadingMask,
        keepSelection: keepSelection,
        setImportanceInCache: setImportanceInCache,
        getConversationMessages: getConversationMessages,
        stickActionMenuToTheTop: stickActionMenuToTheTop
    };

})(jQuery);