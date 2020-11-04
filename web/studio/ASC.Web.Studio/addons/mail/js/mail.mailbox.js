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
        actionButtons = [],
        softRefrash = false;

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
        if (isInit === true) return;

        isInit = true;

        ASC.Controls.AnchorController.bind(TMMail.anchors.sysfolders, onSysFolderPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.compose, onCompose);
        ASC.Controls.AnchorController.bind(TMMail.anchors.composeto, onComposeTo);
        ASC.Controls.AnchorController.bind(TMMail.anchors.reply, onReplyPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.replyAll, onReplyAllPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.forward, onForwardPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.draftitem, onDraftItemPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.templateitem, onTemplateItemPage);

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
        ASC.Controls.AnchorController.bind(TMMail.anchors.foldersettings, onUserFoldersPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.filtersettings, onFiltersPage);

        ASC.Controls.AnchorController.bind(TMMail.anchors.administration, onAdministrationPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.commonSettings, onCommonSettingsPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.helpcenter, onHelpPage);

        ASC.Controls.AnchorController.bind(TMMail.anchors.messagePrint, onMessagePrintPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.conversationPrint, onConversationPrintPage);

        ASC.Controls.AnchorController.bind(TMMail.anchors.contacts, onContactsPage);
        ASC.Controls.AnchorController.bind(TMMail.anchors.settings, onSettingsPage);

        messagePage.init();

        tagsManager.bind(tagsManager.events.OnDelete, onDeleteTag);
        tagsManager.bind(tagsManager.events.OnUpdate, onUpdateTag);

        window.Teamlab.bind(window.Teamlab.events.getMailFilteredMessages, onGetMailConversations);
        window.Teamlab.bind(window.Teamlab.events.getMailFilteredConversations, onGetMailConversations);
        window.Teamlab.bind(window.Teamlab.events.removeMailFolderMessages, onRemoveMailFolderMessages);
        window.Teamlab.bind(window.Teamlab.events.restoreMailMessages, onRestoreMailMessages);
        window.Teamlab.bind(window.Teamlab.events.restoreMailConversations, onRestoreMailMessages);
        window.Teamlab.bind(window.Teamlab.events.moveMailMessages, onMoveMailMessages);
        window.Teamlab.bind(window.Teamlab.events.moveMailConversations, onMoveMailConversations);
        window.Teamlab.bind(window.Teamlab.events.removeMailMessages, onRemoveMailMessages);
        window.Teamlab.bind(window.Teamlab.events.removeMailConversations, onRemoveMailMessages);
        window.Teamlab.bind(window.Teamlab.events.markMailMessages, onMarkMailMessages);
        window.Teamlab.bind(window.Teamlab.events.markMailConversations, onMarkMailMessages);

        window.Teamlab.bind(window.Teamlab.events.getNextMailConversationId, onGetNextPrevConversationId);
        window.Teamlab.bind(window.Teamlab.events.getPrevMailConversationId, onGetNextPrevConversationId);

        window.Teamlab.bind(window.Teamlab.events.getNextMailMessageId, onGetNextPrevMessageId);
        window.Teamlab.bind(window.Teamlab.events.getPrevMailMessageId, onGetNextPrevMessageId);

        $('#createNewMailBtn').trackEvent(ga_Categories.leftPanel, ga_Actions.buttonClick, "create-new-Email");
        $('#check_email_btn').trackEvent(ga_Categories.leftPanel, ga_Actions.buttonClick, "check_email");

        $(document).on("click", '.menu-list a.menu-item-label', messagePage.onLeaveMessage);
        $('#createNewMailBtn').click(messagePage.onLeaveMessage);
        $('#check_email_btn').click(messagePage.onLeaveMessage);

        $(window).scroll(stickActionMenuToTheTop);
        $(window).on('resize', function () { setTimeout(groupGroupButtons, 200); });

        actionButtons = [
            { selector: "#messagesActionMenu .openMail .dropdown-item", handler: openConversation },
            { selector: "#messagesActionMenu .openNewTabMail .dropdown-item", handler: openNewTabConversation },
            { selector: "#messagesActionMenu .replyMail .dropdown-item", handler: TMMail.moveToReply },
            { selector: "#messagesActionMenu .replyAllMail .dropdown-item", handler: TMMail.moveToReplyAll },
            { selector: "#messagesActionMenu .createEmail .dropdown-item", handler: createEmailToSender },
            { selector: "#messagesActionMenu .forwardMail .dropdown-item", handler: TMMail.moveToForward },
            { selector: "#messagesActionMenu .setReadMail .dropdown-item", handler: readUnreadGroupOperation },
            { selector: "#messagesActionMenu .markImportant .dropdown-item", handler: impotantGroupOperation },
            { selector: "#messagesActionMenu .printMail .dropdown-item", handler: moveToPrint },
            { selector: "#messagesActionMenu .moveToFolder .dropdown-item", handler: moveToFolder },
            { selector: "#messagesActionMenu .deleteMail .dropdown-item", handler: deleteGroupOperation }
        ];
    }

    function getScrolledGroupOptions() {
        var options = undefined;
        if (TMMail.pageIs('sysfolders')) {
            options = {
                menuSelector: "#MessagesListGroupButtons",
                menuAnchorSelector: "#SelectAllMessagesCB",
                menuSpacerSelector: "#actionContainer .contentMenuWrapper.messagesList .header-menu-spacer",
                userFuncInTop: function () { $("#MessagesListGroupButtons .menu-action-on-top").hide(); },
                userFuncNotInTop: function () { $("#MessagesListGroupButtons .menu-action-on-top").show(); }
            };
        } else if (TMMail.pageIs('tlContact') || TMMail.pageIs('crmContact') || TMMail.pageIs('personalContact')) {
            options = {
                menuSelector: "#ContactsListGroupButtons",
                menuAnchorSelector: "#SelectAllContactsCB",
                menuSpacerSelector: "#pageActionContainer .contentMenuWrapper .header-menu-spacer",
                userFuncInTop: function () { $("#ContactsListGroupButtons .menu-action-on-top").hide(); },
                userFuncNotInTop: function () { $("#ContactsListGroupButtons .menu-action-on-top").show(); }
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
                menuSpacerSelector: "#pageActionContainer .messageHeader .contentMenuWrapper .header-menu-spacer",
                userFuncInTop: function () { $("#MessageGroupButtons .menu-action-on-top").hide(); },
                userFuncNotInTop: function () { $("#MessageGroupButtons .menu-action-on-top").show(); }
            };
        } else if (TMMail.pageIs('writemessage')) {
            options = {
                menuSelector: "#WriteMessageGroupButtons",
                menuAnchorSelector: "#WriteMessageGroupButtons .btnSend",
                menuSpacerSelector: "#editMessagePageHeader .header-menu-spacer",
                userFuncInTop: function () { $("#WriteMessageGroupButtons .menu-action-on-top").hide(); },
                userFuncNotInTop: function () { $("#WriteMessageGroupButtons .menu-action-on-top").show(); }
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
        messagePage.slideTemplateSelector();
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
        groupGroupButtons();
    }

    function groupGroupButtons() {
        var $listGroupButtons = $('#MessagesListGroupButtons'),
            $importantButton = $listGroupButtons.find('.menuActionImportant'),
            $readButton = $listGroupButtons.find('.menuActionRead'),
            $moreButton = $listGroupButtons.find('.menuActionMore');

        if ($('body').hasClass('media-width-0-1400')) {
            $importantButton.hide();
            $moreButton.show();
            if ($('body').hasClass('media-width-0-1250')) {
                $readButton.hide();
            } else {
                $readButton.show();
            }
        } else {
            $moreButton.hide();
            $importantButton.show();
            $readButton.show();
        }
        minimizeNavButtons($listGroupButtons);
    }

    function minimizeNavButtons($groupButtons) {
        var $navButtons = $groupButtons.find('.menu-action-simple-pagenav'),
            $selectionButtons = $groupButtons.find('.menu-action-checked-count'),
            currentCulture = ASC.Resources.Master.CurrentCulture,
            cultures = [
                { name: 'sk-SK', widthClass: 'media-width-0-1210' },
                { name: 'fi-FI', widthClass: 'media-width-0-1150' },
                { name: 'nl-NL', widthClass: 'media-width-0-1120' },
                { name: 'de-DE', widthClass: 'media-width-0-1048' }
            ];

        for (var i = 0; i < cultures.length; i++) {
            if (currentCulture === cultures[i].name && $('body').hasClass(cultures[i].widthClass)) {
                $selectionButtons.addClass('minimize');
                $navButtons.find('a').css('font-size', '0');
                $navButtons.find('span').hide();
            } else {
                $selectionButtons.removeClass('minimize');
                $navButtons.find('a').css('font-size', '11px');
                $navButtons.find('span').show();
            }
        }
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

        userFoldersPanel.unmarkAll();
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
        var messages = $('.messages:visible .row');
        messages.each(function () {
            var row = $(this);
            var messageId = row.attr('data_id');
            var $checkbox = row.find('input[type="checkbox"]');
            if (selection.HasId(messageId)) {
                currentSelection.AddId(messageId);
                $checkbox.prop('checked', true);
                row.addClass('selected');
                row.addClass('ui-selected');
            } else {
                $checkbox.prop('checked', false);
                row.removeClass('selected');
                row.removeClass('ui-selected');
                haveOneUnchecked = true;
            }
        });


        if (!(jq.browser.mobile || 'ontouchstart' in window))
        {
            $("#itemContainer .messages:visible .row")
            .draggable({
                containment: "document",
                helper: function () {
                    var textInfo = getDragHelperText();

                    var tooltip = jq("<div id=\"mailMovingTooltip\" class=\"studio-action-panel\"></div>");
                    tooltip.html(textInfo);

                    return tooltip;
                },
                cursor: "alias",
                cursorAt: { left: 0, top: 0 },
                delay: 300,
                start: function (e, ui) {
                    $('body').addClass('mailMouseMove');
                    var row = $(this),
                        id = row.attr("data_id");

                        if (!currentSelection.HasId(id)) {
                            overallDeselectAll();
                            selectRow(row);

                            var textInfo = getDragHelperText();
                            $(ui.helper).html(textInfo);
                        }

                    var $folderContainer = $('#userFolderContainer');

                    $folderContainer.find('.menu-item').addClass('mailMoveTo');
                    $folderContainer.find('.jstree-node').addClass('mailMoveToUserFolder');

                    var $moveFolderContainer = $('.mailMoveToUserFolder');

                    $moveFolderContainer.on('mouseover', function (e) {
                        var $target = $(e.target),
                            $parentTargets = $moveFolderContainer.find('.jstree-wholerow'),
                            $row = $target.parent().find('.jstree-wholerow').first(),
                            $selectedFolder = $target.parent().find('.jstree-wholerow').first().hasClass('jstree-wholerow-clicked'),
                            $selectedFolderCounters = $target.parent().parent().find('.jstree-wholerow').first().hasClass('jstree-wholerow-clicked');

                        if (!$selectedFolder
                            && ($target.hasClass('new-label-menu')
                            || $target.hasClass('jstree-anchor')
                            || $target.hasClass('jstree-icon')
                            || $target.hasClass('lattersCount')
                            || $target.hasClass('jstree-wholerow')
                            || $target.hasClass('new-label-menu'))
                            ) {
                            if (!$row.length
                                && !$selectedFolderCounters
                                && ($target.hasClass('jstree-icon')
                                || $target.hasClass('new-label-menu'))) {
                                $row = $target.parent().parent().find('.jstree-wholerow').first();
                            }
                            $parentTargets.removeClass('mailMoveToUserFolderHover');
                            $row.addClass('mailMoveToUserFolderHover');
                        } else {
                            $parentTargets.removeClass('mailMoveToUserFolderHover');
                        }
                    });
                    $moveFolderContainer.on('mouseout', function () {
                        var $parentTargets = $moveFolderContainer.find('.jstree-wholerow');

                        $parentTargets.removeClass('mailMoveToUserFolderHover');
                    });
                },
                stop: function (e, ui) {
                    $('body').removeClass('mailMouseMove');
                    $('.mailMoveTo').removeClass('mailMoveTo');
                    $('.mailMoveToUserFolder').off().removeClass('mailMoveToUserFolder');
                    $('.mailMoveToUserFolderHover').removeClass('mailMoveToUserFolderHover');
                }
              });
        }

        setSelectionComboCheckbox(!haveOneUnchecked);
        updateOverallSelectionLine();
        groupGroupButtons();
    }

    function getDragHelperText() {
        var textInfo;

        if (currentSelection.Count() > 1) {
            textInfo = (commonSettingsPage.isConversationsEnabled())
                ? window.MailScriptResource.InfoSelectConversations.format('<b>' + currentSelection.Count() + '</b>')
                : window.MailScriptResource.InfoSelectMessages.format('<b>' + currentSelection.Count() + '</b>')
        } else {
            textInfo = (commonSettingsPage.isConversationsEnabled())
                ? window.MailScriptResource.InfoSelectConversation
                : window.MailScriptResource.InfoSelectMessage;
        }

        textInfo = window.MailScriptResource.InfoSelectMove.format(textInfo);

        return textInfo;
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
        var checkAll = $('#SelectAllMessagesCB');

        if (currentSelection.Count() && !checked) {
            checkAll.prop({'indeterminate': true, 'checked': true});
        } else {
            checkAll.prop({ 'indeterminate': false, 'checked': checked });
        }
    }

    function updateOverallSelectionLine() {
        var selectedNum = currentSelection.Count();
        $('#OverallSelectionNumber').toggle(selectedNum != 0);
        $('#OverallSelectionNumberText').text(selectedNum);

        $('#OverallDeselectAll').toggle(selectedNum != 0);
    }

    function overallDeselectAll() {
        selection.Clear();
        lastSelectedConcept = null;
        updateSelectionView();
        commonLettersButtonsState();
        groupGroupButtons();
    }

    function hideContentDivs(skipFolderFilter) {
        var itemContainer = $('#itemContainer');
        var actionContainer = $('#actionContainer');

        itemContainer.find('.messages').remove();

        actionContainer.find('.contentMenuWrapper').hide();
        itemContainer.find('.itemWrapper').remove();
        itemContainer.find('.mailContentWrapper').remove();
        itemContainer.find('.simpleWrapper').remove();

        $('#bottomNavigationBar').hide();

        $('.filter-content .filterPanel').hide();

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

        if (!messagePage.isMessageDirty() || !TMMail.isTemplate()) {
            serviceManager.setTag([messageId], tagId, { tag_id: tagId, message_id: messageId });
        }
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
            if (commonSettingsPage.isConversationsEnabled() && !fromMessage) {
                serviceManager.setConverstationsTag(ids, tagId, { ids: ids, tag_id: tagId });
            } else {
                serviceManager.setTag(ids, tagId, { tag_id: tagId, ids: ids });
            }
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
            }
            ids.push(messageIds[i]);
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
            if (!TMMail.pageIs('sysfolders'))
                softRefrash = true;
            markFolderAsChanged(currentFolderId);
            updateAnchor(false, false, true);
        }
    }

    function unsetCurrentMessageTag(tagId) {
        messagePage.unsetTag(mailBox.currentMessageId, tagId);
        unsetTagFromCachedMessageLine(tagId, mailBox.currentMessageId);

        if (mailBox.currentMessageId < 1) {
            return;
        }

        tagsManager.decrement(tagId);

        if (!messagePage.isMessageDirty() || !TMMail.isTemplate()) {
            serviceManager.unsetTag([mailBox.currentMessageId], tagId, { ids: [mailBox.currentMessageId], tag_id: tagId });
        }
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
            if (!tag) {
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
            if (tagsPanel.length) {
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
        var usedTags = [];
        var selectedIds = currentSelection.GetIds();
        for (var i = 0; i < selectedIds.length; i++) {
            var tagsIds = messageTagsIds(selectedIds[i]);

            if(!tagsIds || !tagsIds.length)
                continue;

            for (var j = 0; j < tagsIds.length; j++) {
                var tag = +tagsIds[j];
                if (usedTags.indexOf(tag) === -1) {
                    usedTags.push(tag);
                    }
                }
            }

        return usedTags;
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
        skipRefresh(skipRefreshAll);
        mailCache.setRead(messageIds, asRead);
    }

    function setConversationReadUnread(messageIds, asRead, skipRefreshAll) {
        var status = asRead === true ? 'read' : 'unread';
        serviceManager.markConversations(messageIds, status, { status: status, messageIds: messageIds, folderId: MailFilter.getFolder() });
        skipRefresh(skipRefreshAll);
        mailCache.setRead(messageIds, asRead);
    }

    function updateMessageImportance(id, importance, skipRefreshAll) {
        var status = importance === true ? 'important' : 'normal';
        serviceManager.markMessages(id, status, { status: status, messageIds: id, folderId: MailFilter.getFolder() });
        skipRefresh(!skipRefreshAll);
        mailCache.setImportant(id, importance);
    }

    function updateConversationImportance(id, importance, skipRefreshAll) {
        var status = importance === true ? 'important' : 'normal';
        serviceManager.markConversations(id, status, { status: status, messageIds: id, folderId: MailFilter.getFolder() });
        skipRefresh(!skipRefreshAll);
        mailCache.setImportant(id, importance);
    }

    function skipRefresh(needSkip) {
        if (needSkip)
            serviceManager.getMailFolders();
        else
            serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
    }

    // Removes messages from the specified folder to trash. And removes them completely if we delete them from trash.

    function deleteMessages(ids, fromFolder, fromConversation) {
        if (fromFolder == TMMail.sysfolders.trash.id) {
            serviceManager.deleteMessages(ids, { fromFolder: fromFolder, fromConversation: fromConversation }, {}, window.MailScriptResource.DeletionMessage);
            mailCache.remove(ids);
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
        var row = $('#itemContainer .messages .row[data_id="' + id + '"]'),
            isImportant = row.find('.icon-important').length ? true : false,
            icon = row.find('.icon-important, .icon-unimportant'),
            isFiltered = MailFilter.getImportance();

        if (commonSettingsPage.isConversationsEnabled()) {
            updateConversationImportance(id, !isImportant, isFiltered);
        } else {
            updateMessageImportance(id, !isImportant, isFiltered);
        }

        icon.toggleClass('icon-unimportant').toggleClass('icon-important');
        changeImportanceTitle(icon, !isImportant);
    }

    function openConversation(id) {
        if (TMMail.pageIs('drafts')) {
            TMMail.moveToDraftItem(id);
        } else if (TMMail.pageIs('templates')) {
            TMMail.moveToTemplateItem(id);
        } else {
            TMMail.moveToConversation(id);
        }
    }

    function openNewTabConversation(id) {
        if (TMMail.pageIs('drafts')) {
            TMMail.openDraftItem(id);
        } else if (TMMail.pageIs('templates')) {
            TMMail.openTemplateItem(id);
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

    function moveToPrint(id) {

        if (commonSettingsPage.isConversationsEnabled())
            TMMail.moveToConversationPrint(id, [id], [id], true, true);
        else
            TMMail.moveToMessagePrint(id, true, true);
    }

    function deleteMessage(id, fromFolder, fromConversation) {
        deleteMessages([id], fromFolder, fromConversation);
    }

    function moveMessages(ids, fromFolder, toFolder, fromConversation, userFolderId, onAfterMoveCallback) {
        serviceManager.moveMailMessages(ids,
            toFolder,
            userFolderId,
            { fromFolder: fromFolder, toFolder: toFolder, fromConversation: fromConversation, userFolderId: userFolderId, onAfterMoveCallback: onAfterMoveCallback },
            {
                error: function (p, errors) {
                    console.error("moveMessages()", errors);

                    if (errors[0] === "Folder not found") {
                        window.toastr.error(MailScriptResource.UserFolderNotFoundError);
                    }
                }
            },
            window.MailScriptResource.MovingMessages);

        if (toFolder == TMMail.sysfolders.trash.id ||
            toFolder == TMMail.sysfolders.spam.id) {
            mailCache.remove(ids);
        } else {
            mailCache.setFolder(ids, toFolder, userFolderId);
        }
        serviceManager.updateFolders({ forced: fromFolder == TMMail.sysfolders.userfolder.id });
    }

    function moveMessage(id, fromFolder, toFolder, fromConversation, userFolderId, onAfterMoveCallback) {
        return moveMessages([id], fromFolder, toFolder, fromConversation, userFolderId, onAfterMoveCallback);
    }

    function moveConversations(ids, fromFolder, toFolder, userFolderId) {
        serviceManager.moveMailConversations(ids, toFolder, userFolderId, { fromFolder: fromFolder, toFolder: toFolder, userFolderId: userFolderId },
        {
            error: function (p, errors) {
                console.error("moveConversations()", errors);

                if (errors[0] === "Folder not found") {
                    window.toastr.error(MailScriptResource.UserFolderNotFoundError);
                }
            }
        }, window.MailScriptResource.MovingMessages);

        if (toFolder == TMMail.sysfolders.trash.id || 
            toFolder == TMMail.sysfolders.spam.id) {
            mailCache.remove(ids);
        } else {
            mailCache.setFolder(ids, toFolder, userFolderId);
        }
    }

    function moveConversation(id, fromFolder, folderTypeId, userFolderId) {
        return moveConversations([id], fromFolder, folderTypeId, userFolderId);
    }

    function restoreMessages(ids, removeCahe) {
        var folderIdsForUpdate = new TMContainers.IdMap();

        for (var i = 0; i < ids.length; i++) {
            var prevfolderid = $('div[data_id="' + ids[i].messageId + '"]').attr('prevfolderid');
            folderIdsForUpdate.AddId(prevfolderid);
        }
        serviceManager.restoreMailMessages(ids, { folders: folderIdsForUpdate }, {}, window.MailScriptResource.RestoringMessages);

        if (removeCahe) {
            mailCache.remove(ids);
        } else {
            mailCache.setFolder(ids, TMMail.sysfolders.inbox.id);
        }
    }

    function restoreConversations(ids, removeCahe) {
        serviceManager.restoreMailConversations(ids, {
            folders: [
                TMMail.sysfolders.inbox.id,
                TMMail.sysfolders.sent.id,
                TMMail.sysfolders.drafts.id,
                TMMail.sysfolders.templates.id,
                TMMail.sysfolders.trash.id,
                TMMail.sysfolders.spam.id
            ]
        }, {}, window.MailScriptResource.RestoringMessages);

        if (removeCahe) {
            mailCache.remove(ids);
        } else {
            mailCache.setFolder(ids, TMMail.sysfolders.inbox.id);
        }
    }

    function hidePages() {
        messagePage.hide();
        tagsPage.hide();
        accountsPage.hide();
        administrationPage.hide();
        contactsPage.hide();
        blankPages.hide();
        helpPage.hide();
        userFoldersPage.hide();
        commonSettingsPage.hide();
        filtersPage.hide();
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

        if (!accountsManager.any()) {
            folderFilter.hide();
            blankPages.showEmptyAccounts();
            return;
        }

        $('#itemContainer').height('auto');

        var prevPageLink = $('#itemContainer .pagerPrevButtonCSSClass:visible');

        if ($html.find('[data_id]').length) {
            $html.show();
            blankPages.hide();
            folderFilter.show();
            changeTagStyle();
            groupGroupButtons();
            $('#actionContainer .contentMenuWrapper').show();
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

    function convertMessagesToHtml(messages, options) {
        var tagsIds = $.map(tagsManager.getAllTags(), function (value) { return value.id; });
        var refreshTags = false;

        $.each(messages, function (i, m) {
            var address;
            if (m.folder == TMMail.sysfolders.sent.id || m.folder == TMMail.sysfolders.drafts.id) {
                address = ASC.Mail.Utility.ParseAddress(m.to);
            } else {
                address = ASC.Mail.Utility.ParseAddress(m.from);
            }

            m.sender = address.email;
            m.author = address.name || "";

            m.displayDate = window.ServiceFactory.getDisplayDate(window.ServiceFactory.serializeDate(m.receivedDate));
            m.displayTime = window.ServiceFactory.getDisplayTime(window.ServiceFactory.serializeDate(m.receivedDate));

            // remove tags wich doesn't exist
            m.tagIds = $.grep(m.tagIds, function (tagId) {
                if (0 > $.inArray(tagId, tagsIds)) {
                    return false;
                }
                return true;
            });

            // add tags objects array
            m.tags = [];
            $.each(m.tagIds, function (index, id) {
                var tag = tagsManager.getTag(id);
                if (tag) {
                    if (tag.lettersCount == 0) {
                        refreshTags = true;
                    }
                    m.tags.push(tag);
                }
            });

            m.subject = m.subject || '';

            if (m.folder == TMMail.sysfolders.drafts.id) {
                m.anchor = '#draftitem/' + m.id;
            } else if (m.folder == TMMail.sysfolders.templates.id) {
                m.anchor = '#templateitem/' + m.id;
            } else {
                m.anchor = (commonSettingsPage.isConversationsEnabled() ? '#conversation/' : '#message/') + m.id;
            }

            if (options.showFolders) {
                m.folderName = TMMail.getSysFolderNameById(m.folder);
            }

            m.index = i+1;
        });

        var html = $.tmpl(
            'messagesTmpl',
            {
                messages: messages,
                has_next: options.hasNext || false,
                has_prev: options.hasPrev || false,
                targetBlank: options.targetBlank || false,
                hideCheckboxes: options.hideCheckboxes || false,
                showFolders: options.showFolders || false
            },
            {
                htmlEncode: TMMail.htmlEncode,
                getTagsToDisplay: getTagsToDisplay,
                getCountTagsInMore: getCountTagsInMore
            });

        if (refreshTags)
            serviceManager.getTags();

        var $html = $(html);
        processTagsMore($html);

        return html;
    }

    function onGetMailConversations(params, messages) {
        var filter = MailFilter.toData();
        var prevFlag = filter.hasOwnProperty("prev_flag") && filter.prev_flag;
        var hasFromDate = filter.hasOwnProperty("from_date");

        //console.log(`onGetMailConversations(prevFlag=${prevFlag}, hasFromDate=${hasFromDate}), total=${params.__total}`, filter);

        if (!messages) {
            return;
        }

        hideLoadingMask();

        var hasNext, hasPrev;

        if (commonSettingsPage.isConversationsEnabled()) {
            hasNext = prevFlag || params.__total > filter.page_size;
            hasPrev = (!prevFlag && hasFromDate) || (prevFlag && params.__total > filter.page_size);

            if (prevFlag && !hasPrev) {
                softRefrash = true;
                var newUrl = window.location.href
                    .replace(/from_message=(\d+)\//, "")
                    .replace(/prev=true\//, "")
                    .replace(/from_date=([\w\d%:+(,)]+)\//g, "")
                    .replace(/page_size=25\//, "");

                //console.log(`orig-url=${window.location.href}, new-url=${newUrl}`);

                history.pushState({}, null, newUrl);
            }
        } else {
            hasNext = messages.length >= MailFilter.getPageSize();
            hasPrev = MailFilter.getPage() > 1;

            var hasPage = window.location.href.indexOf(/page=1/) > -1;

            if (hasPage) {
                softRefrash = true;
                var newUrl = window.location.href
                    .replace(/page=1\//, "")
                    .replace(/page_size=25\//, "");

                //console.log(`orig-url=${window.location.href}, new-url=${newUrl}`);

                history.pushState({}, null, newUrl);
            }
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

        var itemContainer = $('#itemContainer');

        var pageContent = $(".page-content");

        var windowWidth = $(window).width();

        var scrollX = windowWidth > 1200
            ? pageContent.scrollLeft() || 0
            : window.pageXOffset || document.documentElement.scrollLeft;

        var scrollY = windowWidth > 1200
            ? pageContent.scrollTop() || 0
            : window.pageYOffset || document.documentElement.scrollTop;

        if (TMMail.pageIs('sysfolders')) {
            hidePages();
            hideContentDivs();
        }

        var options = {
            hasNext: hasNext,
            hasPrev: hasPrev
        };

        var html = convertMessagesToHtml(messages, options);

        var $html = $(html);
        $html.hide();

        if ($html.find('[data_id]').length) {
            itemContainer.append($html);
            itemContainer.find('> .messages').actionMenu('messagesActionMenu', actionButtons, pretreatment);
        }

        if (TMMail.pageIs('sysfolders')) {
            showFolder(params.folder_id, $html, 0);
        }

        if (windowWidth > 1200) {
            pageContent = $(".page-content");
            pageContent.scrollLeft(scrollX);
            pageContent.scrollTop(scrollY);
        }
        else {
            window.scrollTo(scrollX, scrollY);
        }

        stickActionMenuToTheTop();
        updateGroupButtonsComposition();
        groupGroupButtons();
        updateView();
        updateSelectionView();
        commonLettersButtonsState();
    }

    function pretreatment(id) {
        var row = $('#itemContainer .messages .row[data_id="' + id + '"]');

        var messagesActionMenu = $('#messagesActionMenu');

        var openMail = messagesActionMenu.find('.openMail'),
            openNewTabMail = messagesActionMenu.find('.openNewTabMail'),
            replyMail = messagesActionMenu.find('.replyMail'),
            replyAllMail = messagesActionMenu.find('.replyAllMail'),
            createEmail = messagesActionMenu.find('.createEmail'),
            forwardMail = messagesActionMenu.find('.forwardMail'),
            setReadMail = messagesActionMenu.find('.setReadMail'),
            markImportant = messagesActionMenu.find('.markImportant'),
            printMail = messagesActionMenu.find('.printMail'),
            moveToFolder = messagesActionMenu.find('.moveToFolder'),

            openSeparator = messagesActionMenu.find('.openSeparator'),
            composeSeparator = messagesActionMenu.find('.composeSeparator'),
            markSeparator = messagesActionMenu.find('.markSeparator'),
            printSeparator = messagesActionMenu.find('.printSeparator');

        if (!currentSelection.HasId(id)) {
            overallDeselectAll();
            selectRow(row);
        }

        var messageIds = currentSelection.GetIds();
        var showSingleMenu = messageIds.length === 1,
            isDraftPage = TMMail.pageIs('drafts');

        openMail.toggle(showSingleMenu);
        openNewTabMail.toggle(showSingleMenu);

        openSeparator.toggle(showSingleMenu);
        composeSeparator.toggle(showSingleMenu && !isDraftPage);

        markSeparator.show();

        printSeparator.toggle(showSingleMenu && !isDraftPage);

        replyMail.toggle(showSingleMenu && !isDraftPage);
        replyAllMail.toggle(showSingleMenu && !isDraftPage);
        createEmail.toggle(showSingleMenu && !isDraftPage);
        forwardMail.toggle(showSingleMenu && !isDraftPage);
        printMail.toggle(showSingleMenu && !isDraftPage);

        var hasNew = containNewMessage(messageIds);

        setReadMail.find('.dropdown-item')
            .text(hasNew ? window.MailResource.MarkAsRead : window.MailResource.MarkAsUnread);

        var hasImportant = containImportantMessage(messageIds);

        markImportant.find('.dropdown-item')
            .text(hasImportant ? window.MailResource.MarkAsNotImportant : window.MailResource.MarkAsImportant);

        var moveToFolderTitle = null;

        switch (currentFolderId) {
            case TMMail.sysfolders.inbox.id:
            case TMMail.sysfolders.userfolder.id:
                moveToFolderTitle = window.MailScriptResource.SpamLabel;
                break;
            case TMMail.sysfolders.spam.id:
                moveToFolderTitle = window.MailScriptResource.NotSpamLabel;
                break;
            case TMMail.sysfolders.trash.id:
                moveToFolderTitle = window.MailScriptResource.RestoreBtnLabel;
                break;
            default:
                break;
        }

        if (moveToFolderTitle) {
            moveToFolder.find('.dropdown-item').text(moveToFolderTitle);
            moveToFolder.show();
        } else {
            moveToFolder.hide();
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

    function containImportantMessage(messageIds) {
        var contains = false;
        for (var i = 0; i < messageIds.length; i++) {
            var row = $('#itemContainer .messages .row[data_id="' + messageIds[i] + '"]');
            contains = row.find('.icon-important').length ? true : false;
            if (contains) break;
        }
        return contains;
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

        MailFilter.setFromDate(undefined);
        MailFilter.setFromMessage(undefined);
        MailFilter.setPrevFlag(false);

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
        // google analytics track
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
        // google analytics track
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
        // google analytics track
        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.actionClick, 'unread_select');

        unselectAll();
        lastSelectedConcept = selectionConcept.unread;
        actionPanelSelect('.new');
    }

    function actionPanelSelectRead() {
        // google analytics track
        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.actionClick, 'read_select');

        unselectAll();
        lastSelectedConcept = selectionConcept.read;
        actionPanelSelect(':not(.new)');
    }

    function actionPanelSelectImportant() {
        // google analytics track
        window.ASC.Mail.ga_track(ga_Categories.folder, ga_Actions.actionClick, 'important_select');

        unselectAll();
        lastSelectedConcept = selectionConcept.important;
        actionPanelSelect(':has(.icon-important)');
    }

    function actionPanelSelectWithAttachments() {
        // google analytics track
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
            var $this = $(this),
                icon = $this.find('.icon-important, .icon-unimportant'),
                newimportance = icon.is('.icon-unimportant'),
                msgId = $this.parent().attr('data_id'),
                isFiltered = MailFilter.getImportance();

            if (commonSettingsPage.isConversationsEnabled()) {
                mailBox.updateConversationImportance(msgId, newimportance, isFiltered);
            } else {
                mailBox.updateMessageImportance(msgId, newimportance, isFiltered);
            }

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
        groupGroupButtons();
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

        // Important/NotImportant (group button)
        $('#MessagesListGroupButtons .menuActionImportant').click(function () {
            if ($(this).hasClass('unlockAction')) {
                impotantGroupOperation();
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
            if (!$(this).hasClass('unlockAction')) return;

            var options = {
                hideMarkRecipients: true,
                onSelect: function (tag) {
                    mailBox.setTag(tag.id);
                },
                onDeselect: function (tag) {
                    mailBox.unsetConversationsTag(tag.id);
                },
                getUsedTagsIds: function () {
                    return mailBox.getSelectedMessagesUsedTags();
                }
            }

            tagsDropdown.show($(this), options);
        });

        // Move to (group button)
        $('#MessagesListGroupButtons .menuActionMoveTo').click(function () {
            if ($(this).hasClass('unlockAction')) {
                var options = {
                    btnCaption: window.MailResource.MoveHere,
                    hideDefaults: false,
                    hideRoot: true,
                    callback: function (folder) {
                        console.log("#MessagesListGroupButtons .menuActionMoveTo -> callback", folder);
                        moveGroupOperation(folder.folderType, folder.userFolderId);
                    }
                };

                var folderType = MailFilter.getFolder();
                if (folderType !== TMMail.sysfolders.userfolder.id) {
                    options.disableDefaultId = folderType;
                } else {
                    options.disableUFolderId = MailFilter.getUserFolder();
                }

                userFoldersDropdown.show($(this), options);
            }
        });

        // More (group button)
        $('#MessagesListGroupButtons .menuActionMore').on('click', function () {
            if ($(this).hasClass('unlockAction')) {
                updateMoreAction();
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

    function updateMoreAction() {
        var $listGroupButtons = $('#MessagesListGroupButtons'),
            $moreButton = $listGroupButtons.find('.menuActionMore'),
            $readButton = $listGroupButtons.find('.menuActionRead'),
            $importantButton = $listGroupButtons.find('.menuActionImportant');

        if ($moreButton != undefined) {
            var folderId = parseInt(MailFilter.getFolder()),
                buttons = [];
            if ($readButton.is(':hidden')) {
                buttons.push({ text: getButtonTitle($readButton), handler: readUnreadGroupOperation });
            }
            if ($importantButton.is(':hidden')) {
                buttons.push({ text: getButtonTitle($importantButton), handler: impotantGroupOperation });
            }
            $moreButton.actionPanel({ buttons: buttons, css: 'stick-over' });
        }
    }

    function getButtonTitle($control) {
        return $control.find('span').attr('title');
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

    function moveGroupOperation(folderTypeId, userFolderId) {
        var messages = currentSelection.GetIds();
        if (messages.length > 0) {
            enableGroupOperations(false);
            if (commonSettingsPage.isConversationsEnabled())
                moveConversations(messages, currentFolderId, folderTypeId, userFolderId);
            else
                moveMessages(messages, currentFolderId, folderTypeId, null, userFolderId);
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
            var read = containNewMessage(messageIds),
                isFiltered = MailFilter.getUnread() === undefined;

            if (commonSettingsPage.isConversationsEnabled())
                setConversationReadUnread(messageIds, read, isFiltered);
            else
                setMessagesReadUnread(messageIds, read, isFiltered);
        }
        overallDeselectAll();
    }

    function impotantGroupOperation() {
        var messageIds = currentSelection.GetIds();
        if (messageIds.length > 0) {
            var important = containImportantMessage(messageIds),
                isFiltered = MailFilter.getImportance();

            if (commonSettingsPage.isConversationsEnabled())
                updateConversationImportance(messageIds, !important, isFiltered);
            else
                updateMessageImportance(messageIds, !important, isFiltered);

            if (isFiltered)
                overallDeselectAll();
        }
    }

    function updateGroupButtonsComposition(folderId) {
        var groupButtons = $('#MessagesListGroupButtons');

        groupButtons.show();

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
        groupButtons.find('.menuActionNotSpam').toggle(folderId == TMMail.sysfolders.spam.id);
        groupButtons.find('.menuActionSpam').toggle(folderId == TMMail.sysfolders.inbox.id || folderId == TMMail.sysfolders.userfolder.id);

        // Read / unread
        groupButtons.find('.menuActionRead').toggle(folderId != TMMail.sysfolders.drafts.id && folderId != TMMail.sysfolders.templates.id);

        // Important / NotImportant
        groupButtons.find('.menuActionImportant').toggle(folderId != TMMail.sysfolders.drafts.id);

        // Restore
        groupButtons.find('.menuActionRestore').toggle(folderId == TMMail.sysfolders.trash.id);

        var deleteBtnLabel = groupButtons.find('.menuActionDelete span');

        if (folderId === TMMail.sysfolders.trash.id || folderId === TMMail.sysfolders.spam.id) {
            deleteBtnLabel.text(MailScriptResource.FilterActionDeleteLabel);
            deleteBtnLabel.attr("title", MailScriptResource.FilterActionDeleteLabel);
        } else {
            deleteBtnLabel.text(MailResource.DeleteBtnLabel);
            deleteBtnLabel.attr("title", MailResource.DeleteBtnLabel);
        }

        updateOverallSelectionLine();
    }

    function enableGroupOperations(enable) {
        $('#MessagesListGroupButtons .menuAction').toggleClass('unlockAction', enable);
        updateMoreAction();
    }

    function commonLettersButtonsState() {
        enableGroupOperations(currentSelection.Count() != 0);

        /* read/unread */

        var readUnread = false,
            importantNotImportant = true;

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

        /* important/unimportant */

        $('.messages .row').each(function () {
            var $this = $(this);
            var isImportant = $this.find('.icon-important').length ? true : false;
            if (isImportant) {
                var messageId = $this.attr('data_id');
                if (currentSelection.HasId(messageId)) {
                    importantNotImportant = false;
                    return false;
                }
            }
            return true;
        });

        var $importantBtn = $('#MessagesListGroupButtons .menuActionImportant');
        if (importantNotImportant) {
            $importantBtn.attr('do', 'important');
            $importantBtn.html('<span title="' + $importantBtn.attr('important') + '">' + $importantBtn.attr('important') + '</span>');
        } else {
            $importantBtn.attr('do', 'notImportant');
            $importantBtn.html('<span title="' + $importantBtn.attr('notImportant') + '">' + $importantBtn.attr('notImportant') + '</span>');
        }
    }

    // ReSharper disable UnusedParameter

    function onMarkMailMessages(params, ids) {
        if (TMMail.pageIs('conversation') || TMMail.pageIs('message'))
            return;

        // ReSharper restore UnusedParameter
        hidePages();

        var $allMainTables = $('#itemContainer .messages');
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

        if (params.status == 'important' || params.status == 'normal') {
            mailCache.setImportant(params.messageIds, params.status == 'important');
        }
    }

    // prev next ids responces handlers
    // if zero returned - move to folder anchor

    function onGetNextPrevId(method, id) {
        if (0 != id) {
            method.apply(TMMail, [id]);
        } else {
            var folderId = MailFilter.getFolder();
            var filter = MailFilter.toAnchor(false),
                folder = folderId !== TMMail.sysfolders.userfolder.id
                    ? TMMail.getSysFolderNameById(folderId)
                    : TMMail.getSysFolderNameById(folderId) + "=" + MailFilter.getUserFolder(),
                anchor = ASC.Controls.AnchorController.getAnchor(),
                newAnchor = (folder + filter).replace(/'/g, "%27");
            if (anchor != newAnchor) {
                ASC.Controls.AnchorController.move(newAnchor);
            }
        }
    }

    function onGetNextPrevConversationId(params, id) {
        onGetNextPrevId(TMMail.moveToConversation, id);
    }

    function onGetNextPrevMessageId(params, id) {
        onGetNextPrevId(TMMail.moveToMessage, id);
    }

    function markMessageDivsInTable($table, status, messageIds) {
        if (!(messageIds instanceof Array) || !messageIds.length) return;

        var selector = "", i, n = messageIds.length;

        for (i = 0; i < n; i++) {
            selector += ".row[data_id=\"" + messageIds[i] + "\"]";

            if (i + 1 < n) {
                selector += ", ";
            }
        }

        var $messages = $table.find(selector);
        if (!$messages.length) return;

        n = $messages.length;

        for (i = 0; i < n; i++) {
            var $message = $($messages[i]);
            setStatusToMessage($message, status);
        }
    }

    function setStatusToMessage(el, status) {
        var icon;
        switch (status) {
            case 'read':
                el.removeClass('new');
                break;
            case 'unread':
                el.addClass('new');
                break;
            case 'important':
                icon = el.find('.icon-important, .icon-unimportant');
                icon.attr('class', 'icon-important');
                changeImportanceTitle(icon, true);
                break;
            case 'normal':
                icon = el.find('.icon-important, .icon-unimportant');
                icon.attr('class', 'icon-unimportant');
                changeImportanceTitle(icon, false);
                break;
            default:
                break;
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
        if (TMMail.pageIs('viewmessage')) {
            if (moveNextIfPossible(params.toFolder, params.userFolderId)) {
                return;
            }
            updateAnchor(true);
        }
        else {
            serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
        }
        // Clear checkboxes
        selection.RemoveIds(ids);
        lastSelectedConcept = null;
    }

    function onRemoveMailMessages(params, ids) {
        // Mark the source folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.fromFolder);
        if (!params.hasOwnProperty("fromConversation") || !params.fromConversation) {
            TMMail.showCompleteActionHint(TMMail.action_types.delete_messages, window.commonSettingsPage.isConversationsEnabled(), ids.length);

            updateView();

            if (TMMail.pageIs('viewmessage')) {
                updateAnchor(true);
            }
            else {
                serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
            }

            selection.RemoveIds(ids); // Clear checkboxes
            lastSelectedConcept = null;
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
        var callOnAfterMoveCallback = function () {
            if (typeof (params.onAfterMoveCallback) == "function") {
                params.onAfterMoveCallback();
            }
        }

        // Mark the source folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.fromFolder);
        // Mark the destination folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.toFolder);

        // Clear checkboxes
        selection.RemoveIds(ids);
        lastSelectedConcept = null;

        // if not conversation or single message in conversation
        if (!params.fromConversation || $('#itemContainer div.message-wrap').length <= 1) {

            TMMail.showCompleteActionHint(TMMail.action_types.move, false, ids.length, params.toFolder, params.userFolderId);

            if (moveNextIfPossible(params.toFolder, params.userFolderId)) {
                callOnAfterMoveCallback();
                return;
            }
            if (!(params.fromFolder === TMMail.sysfolders.drafts.id &&
                params.toFolder === TMMail.sysfolders.templates.id)) {
                updateAnchor(true);
            }
            callOnAfterMoveCallback();
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
        }

        callOnAfterMoveCallback();
    }

    function onMoveMailConversations(params, ids) {
        // Mark the source folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.fromFolder);
        // Mark the destination folder to be updated (unnecessary to update right now)
        markFolderAsChanged(params.toFolder);

        // Clear checkboxes
        selection.RemoveIds(ids);
        lastSelectedConcept = null;

        if (moveNextIfPossible(params.toFolder, params.userFolderId)) {
            return;
        } else {
            TMMail.showCompleteActionHint(TMMail.action_types.move, true, ids.length, params.toFolder, params.userFolderId);
        }

        if (TMMail.pageIs('viewmessage')) {
            updateAnchor(true);
        }
        else {
            serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
        }
    }

    function moveNextIfPossible(folder, userFolderId) {
        if (TMMail.pageIs('conversation') || TMMail.pageIs('message')) {
            if (window.commonSettingsPage.GoNextAfterMoveEnabled()) {
                var nextMessageLink = $('.itemWrapper .btnNext:visible');
                if (nextMessageLink.length && nextMessageLink.hasClass("unlockAction")) {
                    nextMessageLink.trigger("click");

                    messagePage.conversation_moved = true;
                    messagePage.dstFolderType = folder;
                    messagePage.dstUserFolderId = userFolderId;

                    return true;
                }
            }
        }

        return false;
    }

    function onSysFolderPage(folderName, params) {
        if (softRefrash) {
            softRefrash = false;
            return;
        }


        TMMail.scrollTop();

        currentFolderId = TMMail.getSysFolderIdByName(folderName, TMMail.sysfolders.inbox.id);

        if (currentFolderId != folderPanel.getMarkedFolder()) {
            unmarkAllPanels(currentFolderId == MailFilter.getFolder());
        }

        // check al least one account exists
        // if not - shows stub page
        if (!accountsManager.any()) {
            hidePages();
            folderFilter.hide();
            hideContentDivs();
            blankPages.showEmptyAccounts();
            folderPanel.markFolder(currentFolderId);
            TMMail.setPageHeaderFolderName(currentFolderId);
            hideLoadingMask();
            return;
        }

        if (currentFolderId !== TMMail.sysfolders.userfolder.id) {
            folderPanel.markFolder(currentFolderId);
        } else {
            userFoldersPanel.markFolder(TMMail.extractUserFolderIdFromAnchor());
        }

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

        serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
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
        TMMail.scrollTop();
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

    function onTemplateItemPage(id) {
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
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.Tags);
        hideContentDivs();
        tagsPage.show();
        settingsPanel.selectItem('tagsSettings');
        hideLoadingMask();
        TMMail.scrollTop();
    }

    function onUserFoldersPage() {
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.UserFolders);
        hideContentDivs();
        userFoldersPage.show();
        settingsPanel.selectItem('userFoldersSettings');
        hideLoadingMask();
        TMMail.scrollTop();
    }

    function onFiltersPage() {
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.MailFilters);
        hideContentDivs();
        filtersPage.show();
        settingsPanel.selectItem('filterSettings');
        hideLoadingMask();
        TMMail.scrollTop();
    }

    function onAccountsPage(params) {
        unmarkAllPanels();
        hidePages();
        TMMail.setPageHeaderTitle(window.MailScriptResource.AccountsLabel);
        hideContentDivs();
        accountsPage.show();
        settingsPanel.selectItem('accountsSettings');
        hideLoadingMask();
        TMMail.scrollTop();

        if (params) {
            var email = TMMail.getParamsValue(params, /changepwd=([^&\/]+)/);

            if (!email) return;

            var account = accountsManager.getAccountByAddress(email);

            if (account && account.is_teamlab === true && account.is_shared_domain === false) {
                accountsModal.changePassword(account.email, account.mailbox_id);
            }
        }
    }

    function onAdministrationPage() {
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.AdministrationLabel);
        hideContentDivs();
        contactsManager.init();
        administrationManager.loadData();
        settingsPanel.selectItem('adminSettings');
        TMMail.scrollTop();
    }

    function onCommonSettingsPage() {
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.CommonSettingsLabel);
        hideContentDivs();
        commonSettingsPage.show();
        settingsPanel.selectItem('commonSettings');
        hideLoadingMask();
        TMMail.scrollTop();
    }

    function onTeamLabContactsPage() {
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.TeamLabContactsLabel);
        hideContentDivs();
        if (ASC.Mail.Constants.PEOPLE_AVAILABLE) {
            contactsPage.show('teamlab', 1);
            TMMail.scrollTop();
        } else {
            TMMail.moveToInbox();
        }
    }

    function onCrmContactsPage() {
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.CRMContactsLabel);
        hideContentDivs();
        if (ASC.Mail.Constants.CRM_AVAILABLE) {
            contactsPage.show('crm', 1);
            TMMail.scrollTop();
        } else {
            TMMail.moveToInbox();
        }
    }

    function onPersonalContactsPage() {
        hidePages();
        unmarkAllPanels();
        TMMail.setPageHeaderTitle(window.MailScriptResource.PersonalContactsLabel);
        hideContentDivs();
        contactsPage.show('custom', 1);
        TMMail.scrollTop();
    }

    function onHelpPage(helpId) {
        TMMail.setPageHeaderTitle(window.MailScriptResource.HelpCenterLabel);
        unmarkAllPanels();
        hidePages();
        hideContentDivs();
        helpPanel.selectItem(helpId);
        helpPage.show(helpId);
        TMMail.scrollTop();
    }

    function onContactsPage() {
        console.log("onContactsPage click");
    }

    function onSettingsPage() {
        console.log("onSettingsPage click");
    }

    function printParamParser(params) {
        var simIdsStr = TMMail.getParamsValue(params, /sim=([^&\/]+)/);
        var squIdsStr = TMMail.getParamsValue(params, /squ=([^&\/]+)/);
        var sortAscStr = TMMail.getParamsValue(params, /sortAsc=([^&\/]+)/);
        var showAllStr = TMMail.getParamsValue(params, /showAll=([^&\/]+)/);

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

        var showAll = false;
        if (showAllStr) {
            showAll = showAllStr == '1';
        }


        return {
            simIds: simIds,
            squIds: squIds,
            sortAsc: sortAsc,
            showAll: showAll
        };
    }

    function onMessagePrintPage(messageId, params) {
        var parser = printParamParser(params);
        printPage.init(false, messageId, parser.simIds, parser.squIds);
    }

    function onConversationPrintPage(conversationId, params) {
        var parser = printParamParser(params);
        printPage.init(true, conversationId, parser.simIds, parser.squIds, parser.sortAsc, parser.showAll);
    }

    function updateAnchor(includePagingInfo, keepSel, forced) {
        if (keepSel) {
            keepSelectionOnReload = true;
        }

        if (forced === undefined) {
            forced = false;
        }

        var folderId = MailFilter.getFolder();

        var filter = MailFilter.toAnchor(includePagingInfo),
            folder = folderId !== TMMail.sysfolders.userfolder.id
                ? TMMail.getSysFolderNameById(folderId)
                : TMMail.getSysFolderNameById(folderId) + "=" +MailFilter.getUserFolder(),
            anchor = ASC.Controls.AnchorController.getAnchor(),
            newAnchor = (folder + filter).replace(/'/g, "%27");
        if (anchor != newAnchor || forced) {
            ASC.Controls.AnchorController.move(newAnchor);
        }

        TMMail.hideAllActionPanels();
    }

    function setUnreadInCache(messageId) {
        $('#itemContainer .messages .row[data_id="' + messageId + '"]').removeClass('new');
    }

    function setImportanceInCache(messageId) {
        var importance = $('#itemContainer .messages .row[data_id="' + messageId + '"] .importance');
        if (importance.length > 0) {
            var flag = $(importance).find('.icon-important, .icon-unimportant');
            var newimportance = flag.is('.icon-unimportant');
            flag.toggleClass('icon-unimportant').toggleClass('icon-important');
            changeImportanceTitle(flag, newimportance);
            mailCache.setImportant(messageId, newimportance);
        }
    }

    function needCacheCorrect(cache) {
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
            $('#firstLoader').remove();
            //$('body').css('overflow', 'auto');
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
        stickActionMenuToTheTop: stickActionMenuToTheTop,
        moveTo: moveGroupOperation,
        changeTagStyle: changeTagStyle,
        resizeActionMenuWidth: resizeActionMenuWidth,

        deselectAll: overallDeselectAll,
        selectRow: selectRow,
        convertMessagesToHtml: convertMessagesToHtml
    };

})(jQuery);