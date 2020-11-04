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


var SmallChat = (function() {
    var socket,
        currentAccount = null,
        already = false,
        bufferStates = {},
        offlineBuffer = [],
        ONLINE = "online",
        AWAY = "away",
        NOT_AVAILABLE = "not_available",
        OFFLINE = "offline",
        NUMBER_ONLINE = 1,
        NUMBER_AWAY = 2,
        NUMBER_NOT_AVAILABLE = 3,
        NUMBER_OFFLINE = 4,
        IMAGE_ONLINE = "image_online",
        IMAGE_AWAY = "image_away",
        IMAGE_NOT_AVAILABLE = "image_not_available",
        IMAGE_OFFLINE = "image_offline",
        PX = "px",
        HEIGHT_OFFSET = 100,
        OPEN_USER_DETAIL_LIST_INTERVAL = 650,
        PING_TIMEOUT_INTERVAL = 100000,
        INT_MAX_VALUE = 2147483647,
        SEARCH_CONTACT_INTERVAL = 750,
        TITLE_INTERVAL = 400,
        ENTER_KEY_CODE = 13,
        CONVERSATION_BLOCK_HEIGHT = 300,
        CONVERSATION_BLOCK_WIDTH = 282,
        MESSAGE_WINDOW_ITEM_HEIGHT = 21,
        currentStatus = ONLINE,
        currentImage = IMAGE_ONLINE,
        NUMBER_OF_RECENT_MSGS = 10,
        oldWindowHeight = jq(window).height(),
        originalTitle = null,
        isActive = false,
        titleTimerId = null,
        starsNumber = 1,
        soundPath = jq(".small_chat_en_dis_sounds").attr("data-path"),
        sendTypingSignalTimeout = null,
        typingSignalTimeout = null,
        shouldOpenUserDialogs = [],
        disableUser = false,
        timersForMessages = {},
        pingTimerId = null,
        flashBlocks = {},
        shouldOpenContacts = false,
        jabberTalkWindow = null,
        initEmoticons = false,
        maxDialogNumber = null,
        isMobile = jq.browser.mobile,
        connectionStartTimer = null,
        reloadPage = false,
        disconnectCompleted = true,
        messageContract = {
            i: "Id",
            u: "UserName",
            t: "Text",
            d: "DateTimeText"
        },
        userStrContract = {
            u: "UserName",
            d: "DisplayUserName",
            s: "State"
        },
        userInformations = {},
        isResizeChat;
    if (Teamlab.profile.id && sessionStorageManager.getItem("currentUserId") != Teamlab.profile.id) {
        sessionStorageManager.clear();
        sessionStorageManager.setItem("currentUserId", Teamlab.profile.id);
    }

    function initSocket() {
        if (!(socket && ASC.Resources.Master.Hub.WebChat)) {
            jq(".small_chat_main_window").addClass("display-none");
            return;
        }

        socket
            .on('disconnect', function () {
                closeChat();
            })
            .on('disconnected', function () {
                connectionStop();
                hideChat();
            })
            .on('initDataRetrieved',
            function(userName, showUserName, users, tenantId, tenantName) {
                currentAccount = {
                    TenantName: tenantName,
                    TenantId: tenantId,
                    UserName: userName,
                    ShowUserName: showUserName
                };
                var $userList = jq(".user_list"),
                    usersOnline = {},
                    usersAway = {},
                    usersNotAvailable = {},
                    usersOffline = {},
                    stateNumber;
                if ($userList.length) {
                    jq(".user_list").remove();
                }
                for (var i = 0; i < users.length; i++) {
                    users[i] = reMap(users[i], userStrContract);
                    stateNumber = users[i].State;
                    users[i].State = getImageByNumber(stateNumber);
                    switch (stateNumber) {
                    case NUMBER_ONLINE:
                        usersOnline[i] = users[i];
                        break;
                    case NUMBER_AWAY:
                        usersAway[i] = users[i];
                        break;
                    case NUMBER_NOT_AVAILABLE:
                        usersNotAvailable[i] = users[i];
                        break;
                    default:
                        usersOffline[i] = users[i];
                    }
                }
                var tenantGuid = sessionStorageManager.getItem("TenantGuid");
                if (!tenantGuid) {
                    tenantGuid = guid();
                    sessionStorageManager.setItem("TenantGuid", tenantGuid);
                }
                var html = jq("#contactListTmpl")
                        .tmpl({
                            UsersOnline: usersOnline,
                            UsersAway: usersAway,
                            UsersNotAvailable: usersNotAvailable,
                            UsersOffline: usersOffline
                        }),
                    htmlTenant = jq("#tenantBlockTmpl")
                        .tmpl({
                            TenantGuid: tenantGuid,
                            TenantName: currentAccount.TenantName
                        }),
                    smallChatHeight = sessionStorageManager.getItem("SmallChatHeight");
                if (smallChatHeight) {
                    jq(".small_chat_main_window").css("height", smallChatHeight);
                }
                jq(".chat_contact_loader").addClass("display-none");
                jq(".conversation_block").removeClass("display-none");
                jq(html).appendTo(".contact_container");
                jq(".contact_container").find(".tenant_user_list").append(htmlTenant);
                sessionStorageManager.setItem("WasConnected", true);
                searchContact();
                var status = sessionStorageManager.getItem("CurrentStatus");
                if (status && status != OFFLINE) {
                    changeStatus(getUserNumberByState(status));
                }
                for (var un in bufferStates) {
                    setState(un, bufferStates[un]);
                }
                bufferStates = {};

                for (var i = 0; i < offlineBuffer.length; i++) {
                    if (offlineBuffer[i] == "") {
                        offlineBuffer[i] = sessionStorageManager.getItem("TenantGuid");
                    }
                    if (isDialogOpen(offlineBuffer[i])) {
                        closeConversationBlock(offlineBuffer[i]);
                    }
                    removeUserInSessionIfExists(offlineBuffer[i]);
                    openMessageDialog(offlineBuffer[i]);
                    var result = flashConversationBlock(offlineBuffer[i], true);
                    if (result) {
                        flashBlocks[offlineBuffer[i]] = result;
                    }
                }
                loadMessageDialogs(offlineBuffer);
                offlineBuffer = [];
                jq(".extend_chat_icon").off("click").on("click", extendChat);
                ASC.Controls.JabberClient.extendChat = extendChat;
                setPingSending();
                SendMessagesCount();
                already = false;
            })
        .on('sendInvite',
            function (message) {
                if (currentStatus === OFFLINE) return;
                message = reMap(message, messageContract);
                if (!isDialogOpen(sessionStorageManager.getItem("TenantGuid"))) {
                    openMessageDialog(sessionStorageManager.getItem("TenantGuid"));
                }
                putMessage({
                        IsMessageOfCurrentUser: false,
                        Name: message.UserName,
                        DateTime: Teamlab.getDisplayTime(new Date()),
                        Message: addBr(ASC.Resources.Master.ChatResource.ChatRoomInvite +
                            " " +
                            message.Text +
                            ". " +
                            ASC.Resources.Master.ChatResource.GoTalk),
                        NotRead: true
                    },
                    sessionStorageManager.getItem("TenantGuid"));
            })
        .on('send',
            function (message, calleeUserName, isTenantUser) {
                if (currentStatus === OFFLINE) return;
                message = reMap(message, messageContract);
                if (currentAccount != null) {
                    var userName = undefined,
                        showUserName = undefined,
                        $document = jq(document),
                        isMessageOfCurrentUser = undefined;
                    if (!isTenantUser) {
                        userName = message.UserName;
                        isMessageOfCurrentUser = (userName == currentAccount.UserName);
                        showUserName = isMessageOfCurrentUser
                            ? currentAccount.ShowUserName
                            : jq(".contact_block[data-username='" + message.UserName + "']")
                            .find(".contact_record")
                            .text();
                    } else {
                        userName = sessionStorageManager.getItem("TenantGuid");
                        showUserName = currentAccount.TenantName;
                        isMessageOfCurrentUser = (userName == currentAccount.UserName);
                        openMessageDialog(userName);
                    }
                    var realUserName = isMessageOfCurrentUser ? calleeUserName : userName;
                    if (isDialogOpen(userName) || isDialogOpen(calleeUserName)) {
                        if (!isMessageOfCurrentUser) {
                            var $conversationBlock = jq(".conversation_block[data-username='" + userName + "']");
                            hideTypingMessageNotification($conversationBlock,
                                $conversationBlock.find(".message_bus_container"));
                        }
                        putMessage({
                                IsMessageOfCurrentUser: isMessageOfCurrentUser,
                                Name: showUserName,
                                DateTime: Teamlab.getDisplayTime(new Date()),
                                Message: addBr(message.Text),
                                NotRead: !isMessageOfCurrentUser
                            },
                            realUserName);
                    } else if (isActive && !isMessageOfCurrentUser) {
                        showMessageNotification({
                            UserName: userName,
                            Message: addBr(message.Text),
                            ShowUserName: showUserName,
                        });
                    }
                    if (!isMessageOfCurrentUser) {
                        if (!isMobile && soundPath && localStorageManager.getItem("EnableSound")) {
                            playSound(soundPath);
                        }
                        if (!isActive) {
                            if (!isTenantUser) {
                                shouldOpenUserDialogs[shouldOpenUserDialogs.length] = userName;
                            }
                            if (!titleTimerId) {
                                originalTitle = $document.find("title").text();
                                $document.find("title")
                                    .text("*" +
                                        ASC.Resources.Master.ChatResource.NewMessageLabel +
                                        " " +
                                        originalTitle);
                                titleTimerId = setInterval(function() {
                                        starsNumber++;
                                        $document.find("title")
                                            .text(getStars(starsNumber) +
                                                ASC.Resources.Master.ChatResource.NewMessageLabel +
                                                " " +
                                                originalTitle);
                                        if (starsNumber == 3) {
                                            starsNumber = 0;
                                        }
                                    },
                                    TITLE_INTERVAL);
                            }
                        }
                    } else {
                        openMessageDialog(calleeUserName);
                    }
                }
            })
        .on('sendOfflineMessages',
            function(userNames) {
                offlineBuffer = userNames;
                if (currentAccount && sessionStorageManager.getItem("WasConnected")) {
                    for (var i = 0; i < offlineBuffer.length; i++) {
                        if (offlineBuffer[i] == "") {
                            offlineBuffer[i] = sessionStorageManager.getItem("TenantGuid");
                        }
                        if (isDialogOpen(offlineBuffer[i])) {
                            closeConversationBlock(offlineBuffer[i]);
                        }
                        removeUserInSessionIfExists(offlineBuffer[i]);
                        openMessageDialog(offlineBuffer[i]);
                        var result = flashConversationBlock(offlineBuffer[i], true);
                        if (result) {
                            flashBlocks[offlineBuffer[i]] = result;
                        }
                    }
                    offlineBuffer = [];
                }
            })
        .on('sendTypingSignal',
            function (userName) {
                if (currentStatus === OFFLINE) return;
                if (isDialogOpen(userName)) {
                    var $conversationBlock = jq(".conversation_block[data-username='" + userName + "']"),
                        $typingMessageNotification = $conversationBlock.find(".typing_message_notification");

                    if ($typingMessageNotification.hasClass("display-none")) {
                        $messageBusContainer = $conversationBlock.find(".message_bus_container");

                        $typingMessageNotification.css("bottom",
                            $conversationBlock.find(".message_input_area").outerHeight() + PX);
                        $typingMessageNotification.removeClass("display-none");
                    }
                    if (typingSignalTimeout) {
                        clearTimeout(typingSignalTimeout);
                        typingSignalTimeout = null;
                    }
                    typingSignalTimeout = setTimeout(function() {
                            hideTypingMessageNotification($conversationBlock,
                                $conversationBlock.find(".message_bus_container"));
                        },
                        5000);
                }
            })
        .on('setState',
            function(userName, stateNumber, isJabberClient) {
                if (currentAccount != null) {
                    setState(userName, stateNumber, isJabberClient);
                } else {
                    bufferStates[userName] = stateNumber;
                }
            })
        .on('statesRetrieved',
            function(states) {
                var keys = Object.keys(states),
                    status = sessionStorageManager.getItem("CurrentStatus"),
                    $conversationBlocks = jq(".conversation_block"),
                    $chatMessagesLoading,
                    $conversationBlock;
                $conversationBlocks.removeClass("display-none");
                for (var i = 0; i < keys.length; i++) {
                    setState(keys[i], states[keys[i]]);
                }
                sessionStorageManager.setItem("WasConnected", true);
                for (var i = 0; i < $conversationBlocks.length; i++) {
                    $conversationBlock = jq($conversationBlocks[i]);
                    if (!$conversationBlock.find(".chat_messages_loading").hasClass("display-none")) {
                        getRecentMessagesOnStart($conversationBlock.attr("data-username"));
                    }
                }
                if (status && status != OFFLINE) {
                    changeStatus(getUserNumberByState(status));
                }
                for (var userName in bufferStates) {
                    setState(userName, bufferStates[userName]);
                }
                searchContact();
                bufferStates = {};

                for (var i = 0; i < offlineBuffer.length; i++) {
                    if (offlineBuffer[i] == "") {
                        offlineBuffer[i] = sessionStorageManager.getItem("TenantGuid");
                    }
                    if (isDialogOpen(offlineBuffer[i])) {
                        closeConversationBlock(offlineBuffer[i]);
                    }
                    openMessageDialog(offlineBuffer[i]);
                    var result = flashConversationBlock(offlineBuffer[i], true);
                    if (result) {
                        flashBlocks[offlineBuffer[i]] = result;
                    }
                }
                offlineBuffer = [];
                jq(".extend_chat_icon").off("click").on("click", extendChat);
                ASC.Controls.JabberClient.extendChat = extendChat;
                setPingSending();
                SendMessagesCount();
                already = false;
            })
        .on('setStatus',
            function(number) {
                changeStatus(number);
            })
        .on('disconnectUser', function() {
            already = false;
        })
        .on('connectUser', function (error) {
            var $smallChatMainWindow = jq(".small_chat_main_window");
            if (error) {
                $smallChatMainWindow.off("click", ".show_small_chat_icon");
                $smallChatMainWindow.on("click", ".show_small_chat_icon", showOrHideSmallChat);
                showErrorNotification();
                closeChat();
                already = false;
                return;
            }

            $smallChatMainWindow.off("click", ".show_small_chat_icon");
            $smallChatMainWindow.on("click", ".show_small_chat_icon", showOrHideSmallChat);
            if (connectionStartTimer) {
                clearTimeout(connectionStartTimer);
                connectionStartTimer = null;
            }
            if (!currentAccount) {
                socket.emit('getInitData', function (error) { if (error) already = false; });
            } else {
                socket.emit('getStates', function (error) { if (error) already = false; });
            }
        })
        .on('e',
            function() {
                showErrorNotification();
                connectionStop();
                closeChat();
            });
    }

    function SendMessagesCount() {
        try {
            ASC.Controls.TalkNavigationItem.updateValue(0);
            ASC.SocketIO.Factory.counters.emit('sendMessagesCount', 0);
        } catch (e) {
            console.error(e.message);
        }
    }

    function getRecentMessagesOnStart(u) {
        var userName = u;
        socket.emit('getRecentMessages', userName == sessionStorageManager.getItem("TenantGuid") ? "" : userName, INT_MAX_VALUE, function (recentMessages) {
            var $cb = jq(".conversation_block[data-username='" + userName + "']"),
                $chatLoading = $cb.find(".chat_messages_loading");
            receiveRecentMessages(userName, recentMessages, $chatLoading, $cb, 0);
        });
    }

    function removeUserInSessionIfExists(currentUserName) {
        var dialogsNumber = sessionStorageManager.getItem("dialogsNumber");
        for (var i = 0; i < dialogsNumber; i++) {
            if (currentUserName == sessionStorageManager.getItem("userName" + i)) {
                for (var j = i; j < dialogsNumber; j++) {
                    sessionStorageManager.setItem("userName" + j, sessionStorageManager.getItem("userName" + (j + 1)));
                }
                sessionStorageManager.removeItem("userName" + dialogsNumber - 1);
                sessionStorageManager.setItem("dialogsNumber", dialogsNumber - 1);
                break;
            }
        }
    }

    function s4() {
        return (0|((1 + Math.random()) * 0x10000)).toString(16).substring(1);
    }

    function guid() {
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
    }

    function setPingSending() {
        if (pingTimerId) {
            clearInterval(pingTimerId);
        }
        pingTimerId = setInterval(function () {
            if (sessionStorageManager.getItem("WasConnected")) {
                socket.emit('chatPing', getUserNumberByState(sessionStorageManager.getItem("CurrentStatus")));
            } else {
                clearInterval(pingTimerId);
                pingTimerId = null;
            }
        }, PING_TIMEOUT_INTERVAL);
    }

    function hideTypingMessageNotification(conversationBlock, messageBusContainer) {
        jq(conversationBlock).find(".typing_message_notification").addClass("display-none");
        jq(messageBusContainer).css({ "height": "100%", "padding-bottom": "0" });
        if (typingSignalTimeout) {
            clearTimeout(typingSignalTimeout);
            typingSignalTimeout = null;
        }
    }

    function getStars(starsNumber) {
        switch (starsNumber) {
            case 1: return "*";
            case 2: return "**";
            case 3: return "***";
        }
    }

    function loadMessageDialogs(offlineBuffer) {
        var userName,
            dialogsNumber = sessionStorageManager.getItem("dialogsNumber"),
            dialogsNumberInMenu = sessionStorageManager.getItem("dialogsNumberInMenu");
        if (dialogsNumber) {
            for (var i = 0; i < dialogsNumber; i++) {
                userName = sessionStorageManager.getItem("userName" + i);
                if (jq.inArray(userName, offlineBuffer) == -1) {
                    openMessageDialog(userName, true, i);
                    // if user was disabled
                    if (disableUser) {
                        i--;
                        dialogsNumber--;
                        disableUser = false;
                    }
                    if (sessionStorageManager.getItem("MiniCB" + userName)) {
                        minimize(jq(".conversation_block[data-username='" + userName + "']"));
                    }
                }
            }
        }

        if (dialogsNumberInMenu) {
            for (var i = 0; i < dialogsNumberInMenu; i++) {
                userName = sessionStorageManager.getItem("dn_userName" + i);
                if (jq.inArray(userName, offlineBuffer) == -1) {
                    var $contactRecord = jq(".contact_block[data-username='" + userName + "']").find(".contact_record");
                    // if user was disabled
                    if (!$contactRecord.length) {
                        sessionStorageManager.setItem("dialogsNumberInMenu", dialogsNumberInMenu - 1);
                        for (var j = i; j < dialogsNumberInMenu - 1; j++) {
                            sessionStorageManager.setItem("dn_userName" + j, sessionStorageManager.getItem("dn_userName" + (j + 1)));
                        }
                        sessionStorageManager.removeItem("dn_userName" + (dialogsNumberInMenu - 1));
                        i--;
                        dialogsNumberInMenu--;
                        continue;
                    }
                    addToMenu({
                        UserName: userName,
                        ShowUserName: $contactRecord.text(),
                    }, true, sessionStorageManager.getItem("messageInMenu" + userName));
                }
            }
            // if user was disabled
            if (getMaxDialogNumber() > sessionStorageManager.getItem("dialogsNumber")) {
                userName = jq("#messageDialogPopupID").find(".message_dialog_item").first().attr("data-username"),
                messageInMenu = sessionStorageManager.getItem("messageInMenu" + userName);
                closeMessageDialogItem(userName);
                openMessageDialog(userName, null, null, null, messageInMenu);
            }
        }
        jq(".message_input_area").blur();
    }

    function isDialogOpen(userName) {
        if (jq(".conversation_block[data-username='" + userName + "']").length) {
            return true;
        }
        return false;
    }

    function isDialogInMenu(userName) {
        if (jq(".message_dialog_item[data-username='" + userName + "']").length) {
            return true;
        }
        return false;
    }

    function removeParametersOfDialogInMenu(userName) {
        var $block = jq(".message_dialog_item[data-username='" + userName + "']"),
            $items = jq(".message_dialog_item"),
            dialogsNumberInMenu = sessionStorageManager.getItem("dialogsNumberInMenu") - 1,
            $messageDialogItem = jq(".message_dialog_item");
        for (var i = 0; i < $messageDialogItem.length; i++) {
            var nextIndex = $items.index($messageDialogItem[i]);
            if ($items.index($block) < nextIndex) {
                sessionStorageManager.setItem("dn_userName" + (nextIndex - 1),
                    sessionStorageManager.getItem("dn_userName" + nextIndex));
            }
        }
        $block.remove();
        sessionStorageManager.removeItem("dn_userName" + dialogsNumberInMenu);
        sessionStorageManager.setItem("dialogsNumberInMenu", dialogsNumberInMenu);
    }

    function setParametersOfDialog(userName) {
        var dialogsNumber = sessionStorageManager.getItem("dialogsNumber");
        if (dialogsNumber) {
            sessionStorageManager.setItem("userName" + dialogsNumber, userName);
            jq(".conversation_block[data-username='" + userName + "']").attr("data-dialog-number", dialogsNumber);
            sessionStorageManager.setItem("dialogsNumber", ++dialogsNumber);
        } else {
            sessionStorageManager.setItem("userName0", userName);
            jq(".conversation_block[data-username='" + userName + "']").attr("data-dialog-number", "0");
            sessionStorageManager.setItem("dialogsNumber", 1);
        }
    }

    function setParametersOfDialogInMenu(userName) {
        var dialogsNumberInMenu = sessionStorageManager.getItem("dialogsNumberInMenu");
        if (dialogsNumberInMenu) {
            sessionStorageManager.setItem("dn_userName" + dialogsNumberInMenu, userName);
            sessionStorageManager.setItem("dialogsNumberInMenu", ++dialogsNumberInMenu);
        } else {
            sessionStorageManager.setItem("dn_userName0", userName);
            sessionStorageManager.setItem("dialogsNumberInMenu", 1);
        }
    }

    function getContactNumberBeforeCurrent(name, ulClass) {
        var $lists = jq("." + ulClass).children();
        for (var i = 0; i < $lists.length; i++) {
            if (name <= jq($lists[i]).find(".contact_record").text().toLowerCase()) {
                break;
            }
        }
        return i;
    }

    function setState(userName, stateNumber, isJabberClient) {
        if (userName != currentAccount.UserName) {
            var newState = getImageByNumber(stateNumber),
                $contactBlock = jq(".contact_block[data-username='" + userName + "']"),
                ulClass;
            if ($contactBlock.length) {
                var $elem = $contactBlock.find(".chat_user_state");
                if ($elem.attr("class") != undefined) {
                    var oldState = $elem.attr("class").split(" ")[1];
                    $elem.removeClass(oldState);
                    $elem.addClass(newState);
                    $contactBlock.detach();
                    switch (stateNumber) {
                        case NUMBER_ONLINE:
                            ulClass = "online_user_list";
                            break;
                        case NUMBER_AWAY:
                            ulClass = "away_user_list";
                            break;
                        case NUMBER_NOT_AVAILABLE:
                            ulClass = "not_available_user_list";
                            break;
                        default:
                            ulClass = "offline_user_list";
                    }
                    var contactNumber = getContactNumberBeforeCurrent($contactBlock.find(".contact_record").text().toLowerCase(), ulClass);
                    if (contactNumber) {
                        $contactBlock.insertAfter("." + ulClass + " li:nth-child(" + contactNumber + ")");
                    } else {
                        $contactBlock.prependTo("." + ulClass);
                    }
                }
            } else {
                if (sessionStorageManager.getItem("WasConnected")) {
                    socket.emit('getContactInfo', userName, function (calleeUserName, calleeUserState, error) {
                        if (error) {
                            showErrorNotification();
                            return;
                        }
                        switch (calleeUserState) {
                            case NUMBER_ONLINE:
                                ulClass = "online_user_list";
                                break;
                            case NUMBER_AWAY:
                                ulClass = "away_user_list";
                                break;
                            case NUMBER_NOT_AVAILABLE:
                                ulClass = "not_available_user_list";
                                break;
                            default:
                                ulClass = "offline_user_list";
                        }

                        createContactInfo({
                            UserName: userName,
                            ShowUserName: calleeUserName,
                            StateClass: ulClass
                        }, ulClass);
                    });
                }
            }
            if (isDialogOpen(userName)) {
                var $stateBlock = jq(".conversation_block[data-username='" + userName + "']").find(".conversation_block_user_state");
                if ($stateBlock.attr("class") != undefined) {
                    var oldState = $stateBlock.attr("class").split(" ")[1];
                    $stateBlock.removeClass(oldState);
                    $stateBlock.addClass(newState);
                    $stateBlock.attr("title", getRealStateByNumber(stateNumber));
                }
            }
        } else {
            changeStatus(stateNumber);
        }
    }

    function createContactInfo(contact, ulClass) {
        var contactNumber = getContactNumberBeforeCurrent(contact.UserName, ulClass),
            html = jq("#contactTmpl").tmpl(contact);
        if (contactNumber) {
            html.insertAfter("." + ulClass + " li:nth-child(" + contactNumber + ")");
        } else {
            html.prependTo("." + ulClass);
        }
    }


    function changeStatus(stateNumber, NoIconGreen) {
        var state = getUserStateByNumber(stateNumber);
        if (state != currentStatus) {
            var prevStatus = currentStatus,
                $showSmallChatIcon = jq(".show_small_chat_icon"),
                $smallChatPopupID = jq("#smallChatPopupID"),
                $smallChatTextStatus = jq(".small_chat_text_status"),
                $currentStatus = jq("." + currentStatus),
                realStatus;
            $smallChatPopupID.removeClass("display_block");
            $smallChatPopupID.addClass("display-none");
            $currentStatus.removeClass("disable");
            $smallChatTextStatus.removeClass(currentImage);
            $currentStatus.click(chooseStatus);
            if (state == OFFLINE) {
                $showSmallChatIcon.addClass("small_chat_icon_white");
                $showSmallChatIcon.removeClass("small_chat_icon_green");
                connectionStop();
            } else if (currentStatus == OFFLINE) {
                connectionStart();
            } else if (!NoIconGreen) {
                $showSmallChatIcon.addClass("small_chat_icon_green");
                $showSmallChatIcon.removeClass("small_chat_icon_white");
            }
            currentImage = getImageByNumber(stateNumber);
            currentStatus = state;
            sessionStorageManager.setItem("CurrentStatus", currentStatus);
            $currentStatus = jq("." + currentStatus);
            $currentStatus.off("click");
            $currentStatus.addClass("disable");
            $smallChatTextStatus.addClass(currentImage);
            realStatus = getRealState(currentStatus);
            $smallChatTextStatus.text(realStatus);
            jq("small_chat_status_menu").attr("title", realStatus);
        }
    }

    function showMessageNotification(object) {
        noty({
            text: jq("#messageNotificationTmpl").tmpl(object),
            layout: "bottomRight",
            theme: "defaultTheme",
            type: "alert",
            animation: {
                easing: "swing",
                open: { "height": "toggle" },
                close: { "height": "toggle" },
                speed: "400",
            },
            timeout: "7000",
            maxVisible: 7,
            force: true
        });
    }

    function putMessage(object, userName) {
        var $conversationBlock = jq(".conversation_block[data-username='" + userName + "']"),
            $messageBusContainer = $conversationBlock.find(".message_bus_container"),
            $last = $messageBusContainer.find(".message_of_user:last");
        if ((($last.hasClass("message_of_other_user") && object.IsMessageOfCurrentUser) ||
            ($last.hasClass("message_of_current_user") && !object.IsMessageOfCurrentUser)) &&
            timersForMessages[userName]) {
            clearTimeout(timersForMessages[userName]);
            delete timersForMessages[userName];
        }
        if (!object.IsMessageOfCurrentUser && !(userName in flashBlocks)) {
            var result = flashConversationBlock(userName, true);
            if (result) {
                flashBlocks[userName] = result;
            }
        }
        if (!timersForMessages[userName]) {
            $messageBusContainer.append(jq("#messageTmpl").tmpl(object));
            $last = $messageBusContainer.find(".message_of_user:last");
        } else {
            $last.next().text(object.DateTime);
            $last.after("<div class='message_of_user " +
                ($last.hasClass("message_of_current_user") ? "message_of_current_user" : "message_of_other_user not_read_message") +
                "'>" + object.Message + "</div>");
            clearTimeout(timersForMessages[userName]);
            delete timersForMessages[userName];
        }
        timersForMessages[userName] = setTimeout(function () {
            clearTimeout(timersForMessages[userName]);
            delete timersForMessages[userName];
        }, 30000);
        $last = $messageBusContainer.find(".message_of_user:last");
        if (userName != sessionStorageManager.getItem("TenantGuid")) {
            emoticonize($last);
        }
        $last.linkify();
        scrollTopMessageContainer($messageBusContainer);
    }

    function scrollTopMessageContainer(messageBusContainer) {
        var $messageBusContainer = jq(messageBusContainer);
        scrollHeight = $messageBusContainer.prop("scrollHeight");
        if (scrollHeight > $messageBusContainer.scrollTop() && scrollHeight > $messageBusContainer.height()) {
            $messageBusContainer.scrollTop(scrollHeight);
        }
    }

    function putSmileInMessage(e) {
        var $smile = jq(e.currentTarget),
            $smileMenu = jq(".smile_menu"),
            dn = $smileMenu.attr("id").split("smilesPopupID")[1],
            $conversationBlock = jq(".conversation_block[data-dialog-number='" + dn + "']"),
            $messageInputArea = $conversationBlock.find(".message_input_area"),
            cursorPos = $messageInputArea.prop("selectionStart"),
            v = $messageInputArea.val(),
            userName = $conversationBlock.attr("data-username")
            smileString = " " + $smile.text();

        $messageInputArea.val(v.substring(0, cursorPos) + smileString + v.substring(cursorPos, v.length));
        $messageInputArea.val($messageInputArea.val() + " ");
        setCaretPosition($messageInputArea, cursorPos + smileString.length);
        autosize.update($messageInputArea);
        sessionStorageManager.setItem("message" + userName, $messageInputArea.val());
        $messageInputArea.scrollTop($messageInputArea.prop("scrollHeight"));
        sendTypingSignal(userName);
        $smileMenu.css("display", "none");
    }

    function getRealState(state) {
        switch (state) {
            case ONLINE:
                return ASC.Resources.Master.ChatResource.StatusOnline;
            case AWAY:
                return ASC.Resources.Master.ChatResource.StatusAway;
            case NOT_AVAILABLE:
                return ASC.Resources.Master.ChatResource.StatusNA;
            default:
                return ASC.Resources.Master.ChatResource.StatusOffline;
        }
    }

    function getRealStateByNumber(stateNumber) {
        switch (stateNumber) {
            case NUMBER_ONLINE:
                return ASC.Resources.Master.ChatResource.StatusOnline;
            case NUMBER_AWAY:
                return ASC.Resources.Master.ChatResource.StatusAway;
            case NUMBER_NOT_AVAILABLE:
                return ASC.Resources.Master.ChatResource.StatusNA;
            default:
                return ASC.Resources.Master.ChatResource.StatusOffline;
        }
    }

    function getRealStateByImageState(stateNumber) {
        switch (stateNumber) {
            case IMAGE_ONLINE:
                return ASC.Resources.Master.ChatResource.StatusOnline;
            case IMAGE_AWAY:
                return ASC.Resources.Master.ChatResource.StatusAway;
            case IMAGE_NOT_AVAILABLE:
                return ASC.Resources.Master.ChatResource.StatusNA;
            default:
                return ASC.Resources.Master.ChatResource.StatusOffline;
        }
    }

    function getUserStateByImageState(imageState) {
        switch (imageState) {
            case IMAGE_ONLINE:
                return ONLINE;
            case IMAGE_AWAY:
                return AWAY;
            case IMAGE_NOT_AVAILABLE:
                return NOT_AVAILABLE;
            default:
                return OFFLINE;
        }
    }

    function getImageByNumber(stateNumber) {
        switch (stateNumber) {
            case NUMBER_ONLINE:
                return IMAGE_ONLINE;
            case NUMBER_AWAY:
                return IMAGE_AWAY;
            case NUMBER_NOT_AVAILABLE:
                return IMAGE_NOT_AVAILABLE;
            default:
                return IMAGE_OFFLINE;
        }
    }

    function getUserStateByNumber(stateNumber) {
        switch (stateNumber) {
            case NUMBER_ONLINE:
                return ONLINE;
            case NUMBER_AWAY:
                return AWAY;
            case NUMBER_NOT_AVAILABLE:
                return NOT_AVAILABLE;
            default:
                return OFFLINE;
        }
    }

    function getUserNumberByState(state) {
        switch (state) {
            case ONLINE:
                return NUMBER_ONLINE;
            case AWAY:
                return NUMBER_AWAY;
            case NOT_AVAILABLE:
                return NUMBER_NOT_AVAILABLE;
            default:
                return NUMBER_OFFLINE;
        }
    }

    function getUserNumberByStateForConnection() {
        var state = getUserNumberByState(sessionStorageManager.getItem("CurrentStatus"));
        if (!state || state == NUMBER_OFFLINE) {
            state = NUMBER_ONLINE;
        }
        return state;
    }
    
    function openUserDetailList(e) {
        if (isMobile) {
            return;
        }
        closeUserDetailList();
        if (sessionStorageManager.getItem("WasConnected")) {
            var $contactBlock = jq(e.currentTarget),
                userName = $contactBlock.attr("data-username");
            if (!$contactBlock.is(":hover") || userName == sessionStorageManager.getItem("TenantGuid")) {
                return;
            }
            var timeoutId = setTimeout(function () {
                if (!$contactBlock.is(":hover") || !sessionStorageManager.getItem("WasConnected")) {
                    return;
                }
                var $detailUserList = jq(".detail_user_list");
                if ($detailUserList.length) {
                    $detailUserList.remove();
                }
                if (userInformations[userName]) {
                    openDetail(userName, userInformations[userName], $contactBlock);
                } else {
                    Teamlab.getProfile({}, userName, {
                        success: function (params, data) {
                            var departments = {};
                            if (data.groups && data.groups.length) {
                                for (var i = 0; i < data.groups.length; i++) {
                                    departments[data.groups[i].id] = data.groups[i].name;
                                }
                            } else {
                                departments = undefined;
                            }
                            userInformation = {
                                UserName: userName,
                                ShowUserName: data.displayName,
                                Email: data.email,
                                UserType: data.isVisitor ? ASC.Resources.Master.Guest : ASC.Resources.Master.User,
                                Title: data.title,
                                PhotoURL: data.avatar,
                                Departments: departments
                            };
                            userInformations[userName] = userInformation;
                            openDetail(userName, userInformation, $contactBlock);
                        }
                    });
                }
            }, OPEN_USER_DETAIL_LIST_INTERVAL);
            $contactBlock.data("timeoutId", timeoutId);
        }
    }
    
    function openDetail(userName, data, $contactBlock) {
        if (!jq($contactBlock).is(":hover")) {
            return;
        }
        if (data) {
            var html = jq("#detailUserListTmpl").tmpl(data),
            $detailUserList = jq(html).appendTo(".mainPageTableSidePanel"),
            positionOfContact = jq(".contact_block[data-username='" + userName + "']").offset(),
            top = positionOfContact.top - 30,
            $window = jq(window),
            scrollTop = $window.scrollTop();

            $detailUserList.css("left", positionOfContact.left + 220 + PX);
            if (top + $detailUserList.outerHeight(true) < $window.height() + scrollTop) {
                $detailUserList.css("top", top - scrollTop + PX);
            } else {
                $detailUserList.css("bottom", 15 + PX);
            }
            $detailUserList.fadeIn(170);
            $detailUserList.removeClass("display-none");
        }
    }

    function closeUserDetailList(e) {
        if (isMobile) {
            return;
        }
        if (sessionStorageManager.getItem("WasConnected")) {
            setTimeout(function () {
                var $detailUserList = jq(".detail_user_list");
                if (e) {
                    clearTimeout(jq(e.currentTarget).data("timeoutId"));
                }
                if ($detailUserList.length && !$detailUserList.hasClass("hover")) {
                    $detailUserList.fadeOut(100);
                }
            }, 50);
        }
    }

    function showChat() {
        var smallChatHeight = sessionStorageManager.getItem("SmallChatHeight"),
            $showSmallChatIcon = jq(".show_small_chat_icon"),
            $smallChatMainWindow = jq(".small_chat_main_window");
        jq(".extend_chat_icon").off("click");
        ASC.Controls.JabberClient.extendChat = function () { ASC.Controls.JabberClient.open(); }
        sessionStorageManager.setItem("WasLoad", true);
        $smallChatMainWindow.addClass("small_chat_main_window_full");
        $showSmallChatIcon.addClass("small_chat_icon_green");
        $showSmallChatIcon.removeClass("small_chat_icon_white");
        jq(".contact_container").removeClass("display-none");
        jq(".small_chat_top_panel").removeClass("display-none");
        jq(".icon_ch_size").removeClass("display-none");
        resizeChat();
        jq(".conversation_block").removeClass("display-none");
        jq(".message_dialog_btn").removeClass("display-none");
        if (smallChatHeight) {
            $smallChatMainWindow.css("height", smallChatHeight);
        }
        if (!currentAccount) {
            jq(".chat_contact_loader").removeClass("display-none");
        }
        if (currentStatus == OFFLINE) {
            chooseUserStatus(ONLINE);
        }
    }

    function closeChat() {
        var smallChatHeight = sessionStorageManager.getItem("SmallChatHeight"),
            $showSmallChatIcon = jq(".show_small_chat_icon"),
            $smallChatMainWindow = jq(".small_chat_main_window"),
            $notifications = jq(".notification_username"),
            $noRemove = jq(".chat_contact_loader"),
            $contactContainer = jq(".contact_container");
        sessionStorageManager.setItem("WasConnected", false);
        sessionStorageManager.setItem("WasLoad", false);
        var dialogsNumber = sessionStorageManager.getItem("dialogsNumber"),
                dialogsNumberInMenu = sessionStorageManager.getItem("dialogsNumberInMenu") || 0;
        for (var i = 0; i < dialogsNumber; i++) {
            sessionStorageManager.removeItem("userName" + i);
        }
        for (var i = 0; i < dialogsNumberInMenu; i++) {
            sessionStorageManager.removeItem("dn_userName");
        }
        sessionStorageManager.setItem("dialogsNumber", 0);
        sessionStorageManager.setItem("dialogsNumberInMenu", 0);
        jq(".conversation_block").remove();
        jq(".message_dialog_btn").remove();
        jq(".detail_user_list").remove();
        $contactContainer.empty();
        $contactContainer.html($noRemove);
        currentAccount = null;
        if ($notifications.length) {
            for (var i = 0; i < $notifications.length; i++) {
                jq($notifications[i]).closest("li").remove();
            }
        }

        if (smallChatHeight) {
            $smallChatMainWindow.css("height", "initial");
        }
        $smallChatMainWindow.removeClass("small_chat_main_window_full");
        $showSmallChatIcon.addClass("small_chat_icon_white");
        $showSmallChatIcon.removeClass("small_chat_icon_green");
        jq(".contact_container").addClass("display-none");
        jq(".small_chat_top_panel").addClass("display-none");
        jq(".icon_ch_size").addClass("display-none");
        if (isResizeChat) {
            $smallChatMainWindow.resizable("disable").removeClass("ui-state-disabled");
        }
        //restore default state
        changeStatus(NUMBER_ONLINE, true);
    }

    function hideChat() {
        var smallChatHeight = sessionStorageManager.getItem("SmallChatHeight"),
            $showSmallChatIcon = jq(".show_small_chat_icon"),
            $smallChatMainWindow = jq(".small_chat_main_window");
        if (smallChatHeight) {
            $smallChatMainWindow.css("height", "auto");
        }
        $smallChatMainWindow.removeClass("small_chat_main_window_full");
        $showSmallChatIcon.addClass("small_chat_icon_white");
        $showSmallChatIcon.removeClass("small_chat_icon_green");
        jq(".contact_container").addClass("display-none");
        jq(".small_chat_top_panel").addClass("display-none");
        jq(".icon_ch_size").addClass("display-none");
        if (isResizeChat) {
            $smallChatMainWindow.resizable("disable").removeClass("ui-state-disabled");
        }

        jq(".conversation_block").addClass("display-none");
        jq(".message_dialog_btn").addClass("display-none");
        jq(".detail_user_list").remove();
        //restore default state
        changeStatus(NUMBER_ONLINE, true);
    }

    function showOrHideSmallChat() {
        if (!sessionStorageManager.getItem("WasConnected") && currentStatus != OFFLINE) {
            socket.connect(
                function() {
                    connectionStart();
                    showChat();
                });
        } else {
            connectionStop();
            hideChat();
        }
        setSmallChatPosition();
    }

    function openContacts() {
        if (shouldOpenContacts) {
            shouldOpenContacts = false;
            var dialogsNumber = sessionStorageManager.getItem("dialogsNumber"),
                dialogsNumberInMenu = sessionStorageManager.getItem("dialogsNumberInMenu") || 0,
                allDialogsNumber = dialogsNumber + dialogsNumberInMenu,
                userName;
            for (var i = 0; i < dialogsNumber; i++) {
                userName = sessionStorageManager.getItem("userName" + i);
                if (userName == sessionStorageManager.getItem("TenantGuid")) {
                    ASC.Controls.JabberClient.openTenant(currentAccount.TenantName);
                } else {
                    ASC.Controls.JabberClient.openContact(userName);
                }
            }
            for (var i = dialogsNumber; i < allDialogsNumber; i++) {
                userName = sessionStorageManager.getItem("dn_userName" + (i - dialogsNumber));
                if (userName == sessionStorageManager.getItem("TenantGuid")) {
                    ASC.Controls.JabberClient.openTenant(currentAccount.TenantName);
                } else {
                    ASC.Controls.JabberClient.openContact(userName);
                }
            }
        }
    }

    function extendChat() {
        if (!jabberTalkWindow || jabberTalkWindow.closed) {
            jabberTalkWindow = ASC.Controls.JabberClient.open();
            if (!sessionStorageManager.getItem("WasConnected") && currentStatus != OFFLINE) {
                shouldOpenContacts = false;
            } else {
                shouldOpenContacts = true;
            }
            setTimeout(function () {
                if (sessionStorageManager.getItem("WasConnected")) {
                    connectionStop();
                }
                if (sessionStorageManager.getItem("WasLoad")) {
                    hideChat();
                }
            }, 100);
        } else {
            jabberTalkWindow.focus();
        }
    }

    function putMessages(recentMessages, userName) {
        var $conversationBlock = jq(".conversation_block[data-username='" + userName + "']"),
            $messageBusContainer = $conversationBlock.find(".message_bus_container"),
            messages = [],
            $allMessageBlocks = undefined,
            isCurrentAccount;
        
        if (recentMessages.length != NUMBER_OF_RECENT_MSGS) {
            $conversationBlock.attr("data-internal-id", "stop");
        } else if (recentMessages[0]) {
            $conversationBlock.attr("data-internal-id", recentMessages[0].Id);
        }
        var todayDate = new Date();
        for (var i = 0; i < recentMessages.length; i++) {
            isCurrentAccount = recentMessages[i].UserName == currentAccount.UserName;
            var date = Teamlab.serializeDate(recentMessages[i].DateTimeText);
            messages[i] = {
                IsMessageOfCurrentUser: isCurrentAccount,
                Name: isCurrentAccount ? currentAccount.ShowUserName :
                    $conversationBlock.find(".header_of_conversation_block").text(),
                DateTime: new Date(date.valueOf()).setHours(0, 0, 0, 0) == todayDate.setHours(0, 0, 0, 0) ?
                    Teamlab.getDisplayTime(date) : Teamlab.getDisplayDatetime(date),
                Message: addBr(recentMessages[i].Text),
                Emoticon: true
            }
        }
        var html = jq("#messagesTmpl").tmpl({
            Messages: messages
        });
        $messageBusContainer.find(".chat_messages_loading").after(html);

        $allMessageBlocks = $messageBusContainer.find(".message_of_user");
        for (var i = 0; i < recentMessages.length; i++) {
            jq($allMessageBlocks[i]).linkify();
        }
        if (userName != sessionStorageManager.getItem("TenantGuid")) {
            emoticonize(null, $conversationBlock);
        }
    }

    function emoticonize($lastMessage, $conversationBlock) {
        if ($lastMessage) {
            jq($lastMessage).emoticonize({ delay: 400 });
        } else {
            var $messages = jq($conversationBlock).find(".emoticon_message");
            $messages.emoticonize({ delay: 400 });
            $messages.removeClass("emoticon_message");
        }
    }

    function showMessageDialogFromMenu(newUserName, dialogsNumber, maxDialogNumber) {
        var userName = undefined,
            message,
            messageInMenu,
            $contactRecord;
        if (maxDialogNumber) {
            if (dialogsNumber == maxDialogNumber) {
                userName = sessionStorageManager.getItem("userName" + (dialogsNumber - 1));
                message = sessionStorageManager.getItem("message" + userName);
                closeConversationBlock(userName);
            }
            messageInMenu = sessionStorageManager.getItem("messageInMenu" + newUserName);
            closeMessageDialogItem(newUserName);
            openMessageDialog(newUserName, null, null, null, messageInMenu);
            if (!sessionStorageManager.getItem("WasConnected")) {
                jq(".conversation_block[data-username='" + newUserName + "']").addClass("display-none");
            }
            if (dialogsNumber == maxDialogNumber) {
                $contactRecord = jq(".contact_block[data-username='" + userName + "']").find(".contact_record");
                if ($contactRecord.length) {
                    addToMenu({
                        UserName: userName,
                        ShowUserName: $contactRecord.text()
                    }, null, message);
                }
            }
        }
    }

    function getMaxDialogNumber() {
        var max = 0 | ((jq(window).width() - (jq(".mainPageTableSidePanel:first").width() + 130)) / CONVERSATION_BLOCK_WIDTH);
        return max < 0 ? 0 : max;
    }

    function preventDefault(e) {
        e = e || window.event;
        if (e.preventDefault) {
            e.preventDefault();
        }
        e.returnValue = false;
    }


    function enable_scroll() {
        if (window.removeEventListener) {
            window.removeEventListener("DOMMouseScroll", preventDefault, false);
        }
        window.onmousewheel = document.onmousewheel = null;
    }

    function disable_scroll() {
        if (window.addEventListener) {
            window.addEventListener("DOMMouseScroll", preventDefault, false);
        }
        window.onmousewheel = document.onmousewheel = preventDefault;
    }

    function openMessageDialog(userName, reload, index, inMenu, messageInMenu) {
        var dialogsNumber = sessionStorageManager.getItem("dialogsNumber") || 0,
            $conversationBlock  = undefined,
            closeUserName = undefined;
        if (!sessionStorageManager.getItem("dialogsNumber")) {
            var $allWindows = jq(".small_chat_minimize_all_windows");
            if (!$allWindows.length) {
                $allWindows = jq(".small_chat_restore_all_windows");
            }
            $allWindows.removeClass("disable");
            jq(".small_chat_close_all_windows").removeClass("disable");
            jq("body").on("click", ".small_chat_minimize_all_windows", function () {
                minimizeConversationsBlocks(true);
            })
            .on("click", ".small_chat_restore_all_windows", restoreConversationsBlocks)
            .on("click", ".small_chat_close_all_windows", closeAllWindowsEvent);
        }
        if (!isDialogOpen(userName)) {
            maxDialogNumber = getMaxDialogNumber();
            if (dialogsNumber < maxDialogNumber || reload) {
                var $contactBlock = jq(".contact_block[data-username='" + userName + "']"),
                    dn,
                    imageStatus;
                // if user was disabled
                if (!$contactBlock.length && dialogsNumber) {
                    sessionStorageManager.setItem("dialogsNumber", dialogsNumber - 1);
                    sessionStorageManager.removeItem("MiniCB" + userName);
                    for (var i = index; i < dialogsNumber - 1; i++) {
                        sessionStorageManager.setItem("userName" + i, sessionStorageManager.getItem("userName" + (i + 1)));
                    }
                    sessionStorageManager.removeItem("userName" + (dialogsNumber - 1));
                    disableUser = true;
                    return;
                }
                imageStatus = userName != sessionStorageManager.getItem("TenantGuid") ?
                    $contactBlock.find(".chat_user_state").attr("class").split(" ")[1] : false;
                var $conversationBlock = jq("#messageDialogTmpl").tmpl({
                    UserName: userName,
                    HeaderText: $contactBlock.find(".contact_record").text(),
                    ImageStatus: imageStatus,
                    ImageTitle: getRealStateByImageState(imageStatus)
                }),
                $messageInputArea = $conversationBlock.find(".message_input_area");
                jq($conversationBlock).appendTo(".mainPageContent");

                flashConversationBlock(userName);
                if (!isMobile) {
                    $messageInputArea.focus();
                }
                autosize($messageInputArea);
                $messageInputArea.on("autosize:resized", resizeMessageInputArea);

                var message = messageInMenu;
                if (!message || message == "") {
                    message = sessionStorageManager.getItem("message" + userName);
                }
                if (message && message != "") {
                    $messageInputArea.val(message);
                    autosize.update($messageInputArea);
                    sessionStorageManager.setItem("message" + userName, $messageInputArea.val());
                }
                var $chatMessagesLoading = $conversationBlock.find(".chat_messages_loading");
                $conversationBlock.find(".message_bus_container").scroll(function () {
                    if ($conversationBlock.attr("data-internal-id") != "stop" && !$conversationBlock.find(".message_bus_container").scrollTop() &&
                        sessionStorageManager.getItem("WasConnected")) {
                        $chatMessagesLoading.removeClass("display-none");
                        var arg1 = userName == sessionStorageManager.getItem("TenantGuid") ? "" : userName,
                            arg2 = $conversationBlock.attr("data-internal-id");
                        disable_scroll();
                        socket.emit('getRecentMessages', arg1, arg2, function (recentMessages, error) {
                            enable_scroll();
                            if (error) return;
                            receiveRecentMessages(userName, recentMessages, $chatMessagesLoading, $conversationBlock, $conversationBlock.attr("data-scroll-height"));
                        });
                    }
                });
                if (sessionStorageManager.getItem("WasConnected")) {
                    $chatMessagesLoading.removeClass("display-none");
                    // getRecentMessages
                    socket.emit('getRecentMessages', userName == sessionStorageManager.getItem("TenantGuid") ? "" : userName,
                        INT_MAX_VALUE, function (recentMessages) {
                            receiveRecentMessages(userName, recentMessages, $chatMessagesLoading, $conversationBlock, 0);
                    });
                }
                if (reload) {
                    jq(".conversation_block[data-username='" + userName + "']").css("right", index * CONVERSATION_BLOCK_WIDTH + 10 + PX);
                    $conversationBlock.attr("data-dialog-number", index);
                    dn = index;
                } else {
                    jq(".conversation_block[data-username='" + userName + "']").css("right", dialogsNumber * CONVERSATION_BLOCK_WIDTH + 10 + PX);
                    setParametersOfDialog(userName);
                    dn = dialogsNumber;
                }
                if (!initEmoticons) {
                    initSmilesPopupMenu();
                    initEmoticons = true;
                }
                setSmilesPopupMenu(userName, dn);
            } else if (maxDialogNumber) {
                var messageInMenu,
                    message;
                if (isDialogInMenu(userName)) {
                    messageInMenu = sessionStorageManager.getItem("messageInMenu" + userName);
                    closeMessageDialogItem(userName);
                }
                closeUserName = sessionStorageManager.getItem("userName" + (dialogsNumber - 1));
                message = sessionStorageManager.getItem("message" + closeUserName);
                closeConversationBlock(closeUserName);
                openMessageDialog(userName, null, null, null, messageInMenu);
                addToMenu({
                    UserName: closeUserName,
                    ShowUserName: jq(".contact_block[data-username='" + closeUserName + "']").find(".contact_record").text()
                }, inMenu, message);
            }
            else {
                addToMenu({
                    UserName: userName,
                    ShowUserName: jq(".contact_block[data-username='" + userName + "']").find(".contact_record").text()
                }, inMenu, sessionStorageManager.getItem("message" + userName));
            }
        } else {
            flashConversationBlock(userName);
        }
    }

    function receiveRecentMessages(userName, recentMessages, $chatMessagesLoading, conversationBlock, dataScrollHeight) {
        var $conversationBlock = jq(conversationBlock);
        if (recentMessages) {
            for (var count = 0; count < recentMessages.length; count++) {
                recentMessages[count] = reMap(recentMessages[count], messageContract);
            }
            jq($chatMessagesLoading).addClass("display-none");
            putMessages(recentMessages, userName);
            var $messageBusContainer = $conversationBlock.find(".message_bus_container"),
                scrollHeight = $conversationBlock.find(".message_bus_container").prop("scrollHeight");
            $messageBusContainer.scrollTop(scrollHeight - dataScrollHeight);
            $conversationBlock.attr("data-scroll-height", scrollHeight);
        }
    }

    function resizeMessageInputArea() {
        var $this = jq(this),
            $conversationBlock = $this.closest(".conversation_block"),
            scrollHeight = 0,
            $messageBusContainer = $conversationBlock.find(".message_bus_container"),
            height = parseInt($this.css("height")),
            previousHeight = +$this.attr("data-height"),
            conversationBlockHeight = parseInt($conversationBlock.css("height")) - height + previousHeight;
        // for false trippings in IE
        if (height == previousHeight || conversationBlockHeight > 300) {
            return;
        }
        $conversationBlock.css("height", conversationBlockHeight + PX);
        $conversationBlock.css("padding-bottom", parseInt($conversationBlock.css("padding-bottom")) + height - previousHeight + PX);
        scrollHeight = $messageBusContainer.prop("scrollHeight");
        if ($messageBusContainer.scrollTop() < scrollHeight) {
            $messageBusContainer.scrollTop(scrollHeight);
        }
        scrollHeight = $this.prop("scrollHeight");
        if ($this.scrollTop() < scrollHeight) {
            $this.scrollTop(scrollHeight);
        }
        $this.attr("data-height", height);
    }

    function flashConversationBlock(userName, always) {
        var $headerOfConversationBlock =
            jq(".conversation_block[data-username='" + userName + "']").find(".header_of_conversation_block");
        if ($headerOfConversationBlock.length) {
            if (always) {
                return setInterval(function (conversationBlock) {
                    if ($headerOfConversationBlock.hasClass("main-header-color-hover")) {
                        $headerOfConversationBlock.addClass("main-header-color");
                        $headerOfConversationBlock.removeClass("main-header-color-hover");
                    } else {
                        $headerOfConversationBlock.addClass("main-header-color-hover");
                        $headerOfConversationBlock.removeClass("main-header-color");
                    }
                }, 300);
            } else {
                $headerOfConversationBlock.addClass("main-header-color-hover");
                $headerOfConversationBlock.removeClass("main-header-color");
                setTimeout(function (conversationBlock) {
                    $headerOfConversationBlock.addClass("main-header-color");
                    $headerOfConversationBlock.removeClass("main-header-color-hover");
                }, 100);
            }
        }
    }

    function addToMenu(object, inMenu, message) {
        if (!jq(".message_dialog_btn").length) {
            html = jq("#messageDialogMenuTmpl").tmpl(object);
            jq(".mainPageContent").append(html);
            if (!sessionStorageManager.getItem("WasConnected")) {
                jq(".message_dialog_btn").addClass("display-none");
            }
            jq.dropdownToggle({
                switcherSelector: ".message_dialog_btn",
                dropdownID: "messageDialogPopupID",
                addTop: -4,
                addLeft: 0,
                alwaysUp: true
            });
        } else {
            html = jq("#messageDialogItemTmpl").tmpl(object);
            jq("#messageDialogPopupID").find(".dropdown-content").append(html);
            var $messageDialogText = jq(".message_dialog_text");
            $messageDialogText.text(+$messageDialogText.text() + 1);
        }
        if (!inMenu) {
            setParametersOfDialogInMenu(object.UserName);
        }
        if (message && message != "") {
            sessionStorageManager.setItem("messageInMenu" + object.UserName, message);
        }
    }

    function setSmilesPopupMenu(userName, dn) {
        jq(".conversation_block[data-username='" + userName + "']").find(".smile_icon").attr("id", "smileIcon" + dn);
    }

    function initSmilesPopupMenu() {
        jq("#smileMenuTmpl").tmpl().appendTo(".mainPageContent");
        jq(".smile").emoticonize();
    }

    function moveSmileMenu(e) {
        var $smileMenu = jq(".smile_menu"),
            dn = jq(e.currentTarget).closest(".conversation_block").attr("data-dialog-number");
        $smileMenu.attr("id", "smilesPopupID" + dn);
        jq.dropdownToggle({
            switcherSelector: "#smileIcon" + dn,
            dropdownID: "smilesPopupID" + dn,
            addLeft: 176,
            addTop: 6,
            rightPos: true,
            alwaysUp: true
        });
        jq("#smilesPopupID" + dn).addClass("smiles_menu_padding");
    }

    function closeAllConversationBlocks() {
        var dialogsNumber = sessionStorageManager.getItem("dialogsNumber");
        if (dialogsNumber) {
            dialogsNumber--;
            for (var i = dialogsNumber; i >= 0; i--) {
                closeConversationBlock(sessionStorageManager.getItem("userName" + i));
            }
        }
    }

    function closeAllMessageDialogItems() {
        var dialogsNumber = sessionStorageManager.getItem("dialogsNumberInMenu");
        if (dialogsNumber) {
            dialogsNumber--;
            for (var i = dialogsNumber; i >= 0; i--) {
                closeMessageDialogItem(sessionStorageManager.getItem("dn_userName" + i));
            }
        }
    }

    function closeConversationBlock(userName) {
        var $conversationBlock = jq(".conversation_block[data-username='" + userName + "']");
        sessionStorageManager.removeItem("userName" + $conversationBlock.attr("data-dialog-number"));
        autosize.destroy($conversationBlock.find(".message_input_area"));
        $conversationBlock.remove();
        moveOtherConversationBlocks(+$conversationBlock.attr("data-dialog-number"));
        sessionStorageManager.setItem("dialogsNumber",+sessionStorageManager.getItem("dialogsNumber") - 1);
        sessionStorageManager.removeItem("MiniCB" + userName);
        sessionStorageManager.removeItem("message" + userName);

        if (!sessionStorageManager.getItem("dialogsNumber")) {
            var $this = jq(".small_chat_restore_all_windows");
            if ($this && $this.hasClass("small_chat_restore_all_windows")) {
                $this.removeClass("small_chat_restore_all_windows");
                $this.addClass("small_chat_minimize_all_windows");
                $this.text(ASC.Resources.Master.ChatResource.MinimizeAllWindows);
                $this.closest("#smallChatOptionsPopupID").css("display", "none");
                sessionStorageManager.removeItem("minimizeWindows");
            } else {
                $this = jq(".small_chat_minimize_all_windows");
            }
            $this.addClass("disable");
            jq(".small_chat_close_all_windows").addClass("disable");
            jq("body")
                .off("click", ".small_chat_minimize_all_windows")
                .off("click", ".small_chat_restore_all_windows")
                .off("click", ".small_chat_close_all_windows");
        }
    }

    function closeMessageDialogItem(userName) {
        var $messageDialogText = jq(".message_dialog_text");
        removeParametersOfDialogInMenu(userName);
        $messageDialogText.text(+$messageDialogText.text() - 1);
        if (!+$messageDialogText.text()) {
            jq(".message_dialog_btn").remove();
            jq("#messageDialogPopupID").remove();
        }
        sessionStorageManager.removeItem("messageInMenu" + userName);
    }

    function moveOtherConversationBlocks(dialogNumber) {
        var $conversationBlocks = jq(".conversation_block");
        for (var i = 0; i < $conversationBlocks.length; i++) {
            var $conversationBlock = jq($conversationBlocks[i]);
            if ($conversationBlock.attr("data-dialog-number") > dialogNumber) {
                var userName = $conversationBlock.attr("data-username"),
                    dn = +$conversationBlock.attr("data-dialog-number") - 1;
                $conversationBlock.css("right", parseInt($conversationBlock.css("right")) - CONVERSATION_BLOCK_WIDTH + PX);
                $conversationBlock.attr("data-dialog-number", dn);
                sessionStorageManager.setItem("userName" + dn, userName);
                $conversationBlock.find(".smile_icon").attr("id", "smileIcon" + dn);
            }
        }
        sessionStorageManager.removeItem("userName" + (sessionStorageManager.getItem("dialogsNumber") - 1));
    }

    function restOrMinConversationBlock(e) {
        var $conversationBlock = jq(e.currentTarget).closest(".conversation_block");
        if ($conversationBlock.find(".smile_icon").hasClass("display-none")) {
            restore($conversationBlock);
        } else {
            minimize($conversationBlock);
        }
    }

    function minimize(conversationBlock) {
        var $conversationBlock = jq(conversationBlock),
            userName = $conversationBlock.attr("data-username"),
            $minimizeRestoreConversationBlock = $conversationBlock.find(".minimize_restore_conversation_block");
        $conversationBlock.find(".smile_icon").addClass("display-none");
        $conversationBlock.find(".message_bus_container").addClass("display-none");
        $conversationBlock.find(".message_input_area").addClass("display-none");
        $minimizeRestoreConversationBlock.addClass("restore_conversation_block");
        $minimizeRestoreConversationBlock.removeClass("minimize_conversation_block");
        $minimizeRestoreConversationBlock.attr("title", ASC.Resources.Master.ChatResource.RestoreMessageWindowAltTitle);

        $conversationBlock.attr({
            "data-height": $conversationBlock.css("height"),
            "data-padding-bottom": $conversationBlock.css("padding-bottom")
        });
        $conversationBlock.css({
            "height": "15px",
            "padding-bottom": "0",
            "bottom": "26px"
        });
        jq(".smile_menu").css("display", "none");
        sessionStorageManager.setItem("MiniCB" + userName, true);
        if (!jq(".minimize_conversation_block").length) {
            var $this = jq(".small_chat_minimize_all_windows");
            if ($this && $this.hasClass("small_chat_minimize_all_windows")) {
                $this.removeClass("small_chat_minimize_all_windows");
                $this.addClass("small_chat_restore_all_windows");
                $this.text(ASC.Resources.Master.ChatResource.RestoreAllWindows);
                $this.closest("#smallChatOptionsPopupID").css("display", "none");
                sessionStorageManager.setItem("minimizeWindows", true);
            }
        }
    }

    function restore(conversationBlock) {
        var $conversationBlock = jq(conversationBlock),
            userName = $conversationBlock.attr("data-username"),
            $messageBusContainer = $conversationBlock.find(".message_bus_container"),
            $minimizeRestoreConversationBlock = $conversationBlock.find(".minimize_restore_conversation_block");
            $messageInputArea = $conversationBlock.find(".message_input_area");

        $conversationBlock.find(".smile_icon").removeClass("display-none");
        $messageBusContainer.removeClass("display-none");
        $messageInputArea.removeClass("display-none");
        $minimizeRestoreConversationBlock.addClass("minimize_conversation_block");
        $minimizeRestoreConversationBlock.removeClass("restore_conversation_block");
        $minimizeRestoreConversationBlock.attr("title", ASC.Resources.Master.ChatResource.MinimizeMessageWindowAltTitle);
        $conversationBlock.css({
            "height": $conversationBlock.attr("data-height"),
            "padding-bottom": $conversationBlock.attr("data-padding-bottom"),
            "bottom": "10px"
        });
        sessionStorageManager.removeItem("MiniCB" + userName);
        scrollTopMessageContainer($messageBusContainer);

        if (jq(".minimize_conversation_block").length) {
            var $this = jq(".small_chat_restore_all_windows");
            if ($this && $this.hasClass("small_chat_restore_all_windows")) {
                $this.removeClass("small_chat_restore_all_windows");
                $this.addClass("small_chat_minimize_all_windows");
                $this.text(ASC.Resources.Master.ChatResource.MinimizeAllWindows);
                $this.closest("#smallChatOptionsPopupID").css("display", "none");
                sessionStorageManager.removeItem("minimizeWindows");
            }
        }
        $messageInputArea.focus();
    }

    function restoreConversationsBlocks() {
        var $blocks = jq(".conversation_block"),
            $this = jq(".small_chat_restore_all_windows");
        if ($blocks.length) {
            for (var i = 0; i < $blocks.length; i++) {
                if (jq($blocks[i]).find(".smile_icon").hasClass("display-none")) {
                    restore($blocks[i]);
                }
            }
        }
        if ($this && $this.hasClass("small_chat_restore_all_windows")) {
            $this.removeClass("small_chat_restore_all_windows");
            $this.addClass("small_chat_minimize_all_windows");
            $this.text(ASC.Resources.Master.ChatResource.MinimizeAllWindows);
            $this.closest("#smallChatOptionsPopupID").css("display", "none");
            sessionStorageManager.removeItem("minimizeWindows");
        }
    }

    function minimizeConversationsBlocks(neededCloseMenu) {
        var $blocks = jq(".conversation_block"),
            $this = jq(".small_chat_minimize_all_windows");
        if ($blocks.length) {
            for (var i = 0; i < $blocks.length; i++) {
                if (!jq($blocks[i]).find(".smile_icon").hasClass("display-none")) {
                    minimize($blocks[i]);
                }
            }
        }
        if ($this && $this.hasClass("small_chat_minimize_all_windows")) {
            $this.removeClass("small_chat_minimize_all_windows");
            $this.addClass("small_chat_restore_all_windows");
            $this.text(ASC.Resources.Master.ChatResource.RestoreAllWindows);
            if (neededCloseMenu) {
                $this.closest("#smallChatOptionsPopupID").css("display", "none");
            }
            sessionStorageManager.setItem("minimizeWindows", true);
        }
    }

    function sendMessage(e) {
        var k = e.keyCode,
            ctrlKey = e.ctrlKey;
        // Verify that the key entered is not a special key
        if (k == 20 /* Caps lock */
         || k == 16 /* Shift */
         || k == 17 /* Control Key */
         || k == 91 /* Windows Command Key */
         || k == 19 /* Pause Break */
         || k == 18 /* Alt Key */
         || k == 93 /* Right Click Point Key */
         || (k >= 35 && k <= 40) /* Home, End, Arrow Keys */
         || k == 45 /* Insert Key */
         || (k >= 33 && k <= 34) /*Page Down, Page Up */
         || (k >= 112 && k <= 123) /* F1 - F12 */
         || (k >= 144 && k <= 145)) { /* Num Lock, Scroll Lock */
            return true;
        }
        var inputAreaHeight = 0,
            $messageInputArea = jq(e.currentTarget),
            $conversationBlock = $messageInputArea.closest(".conversation_block"),
            userName = $conversationBlock.attr("data-username"),
            text = $messageInputArea.val(),
            isTenant = false,
            enableCtrlEnter = localStorageManager.getItem("EnableCtrlEnter"),
            newUserName,
            messageInMenu;
        if (k == 27) { /* Escape Key */
            closeConversationBlock(userName);
            var dialogsNumberInMenu = sessionStorageManager.getItem("dialogsNumberInMenu");
            if (dialogsNumberInMenu) {
                newUserName = sessionStorageManager.getItem("dn_userName" + (dialogsNumberInMenu - 1));
                messageInMenu = sessionStorageManager.getItem("messageInMenu" + newUserName);
                closeMessageDialogItem(newUserName);
                openMessageDialog(newUserName, null, null, null, messageInMenu);
            } else {
                newUserName = sessionStorageManager.getItem("userName" + (sessionStorageManager.getItem("dialogsNumber") - 1));
                jq(".conversation_block[data-username='" + newUserName + "']").find(".message_input_area").focus();
            }
            return true;
        } else if (k == 9) { /* Tab */
            var dn = sessionStorageManager.getItem("dialogsNumber");
            if (dn > 1) {
                for (var i = 0; i < dn; i++) {
                    if (sessionStorageManager.getItem("userName" + i) == userName) {
                        newUserName = i == 0 ? sessionStorageManager.getItem("userName" + (dn - 1)) : sessionStorageManager.getItem("userName" + (i - 1));
                        // setTimeout - workaround for bug with focus() in Chrome
                        setTimeout(function () {
                            var $conversationBlock = jq(".conversation_block[data-username='" + newUserName + "']")
                            if ($conversationBlock.find(".smile_icon").hasClass("display-none")) {
                                restore($conversationBlock);
                            }
                            $conversationBlock.find(".message_input_area").focus();
                        }, 1);
                        break;
                    }
                }
            }
            return false;
        }
        
        var timerId = flashBlocks[userName];
        if (timerId) {
            $conversationBlock.find(".not_read_message").removeClass("not_read_message");
            var $headerOfConversationBlock = $conversationBlock.find(".header_of_conversation_block");
            clearInterval(timerId);
            if ($headerOfConversationBlock.length) {
                if ($headerOfConversationBlock.hasClass("main-header-color-hover")) {
                    $headerOfConversationBlock.addClass("main-header-color");
                    $headerOfConversationBlock.removeClass("main-header-color-hover");
                }
            }
        }
        delete flashBlocks[userName];

        if (sessionStorageManager.getItem("WasConnected")) {
            sendTypingSignal(userName);
            $messageBusContainer = $conversationBlock.find(".message_bus_container");
            hideTypingMessageNotification($conversationBlock, $messageBusContainer);
            if (userName == sessionStorageManager.getItem("TenantGuid")) {
                isTenant = true;
            }
            if (k == ENTER_KEY_CODE && ctrlKey && !enableCtrlEnter) {
                var cursorPos = $messageInputArea.prop("selectionStart"),
                    v = $messageInputArea.val();
                $messageInputArea.val(v.substring(0, cursorPos) + "\n" + v.substring(cursorPos, v.length));
                //$messageInputArea.val($messageInputArea.val() + " ");
                setCaretPosition($messageInputArea, cursorPos + 1);
                autosize.update($messageInputArea);
                $messageInputArea.scrollTop($messageInputArea.prop("scrollHeight"));
                //sessionStorageManager.setItem("message" + userName, $messageInputArea.val());
                return true;
            } else if (k == ENTER_KEY_CODE && ((!enableCtrlEnter && !ctrlKey) || (enableCtrlEnter && ctrlKey))) {
                if ((/^\s*$/.test(text))) {
                    return true;
                }
                //Send
                socket.emit('send', isTenant ? "" : userName, text);
                putMessage({
                    IsMessageOfCurrentUser: true,
                    Name: currentAccount.ShowUserName,
                    DateTime: Teamlab.getDisplayTime(new Date()),
                    Message: addBr(text)
                }, userName);

                $conversationBlock.css({"height": "300px", "padding-bottom": "61px"});
                $messageInputArea.css("height", "29px");
                $messageInputArea.attr("data-height", "29");
                $messageInputArea.val("");
                sessionStorageManager.removeItem("message" + userName);
                return false;
            }
        }
    }

    function saveTextMessage(e) {
        var $messageInputArea = jq(e.currentTarget),
            userName = $messageInputArea.closest(".conversation_block").attr("data-username");
        sessionStorageManager.setItem("message" + userName, $messageInputArea.val());
    }

    function setCaretPosition(elem, caretPos) {
        var el = jq(elem)[0];
        el.value = el.value;
        // ^ this is used to not only get "focus", but
        // to make sure we don't have it everything -selected-
        // (it causes an issue in chrome, and having it doesn't hurt any other browser)
        if (el !== null) {
            if (el.createTextRange) {
                var range = el.createTextRange();
                range.move('character', caretPos);
                range.select();
                return true;
            }
            else {
                // (el.selectionStart === 0 added for Firefox bug)
                if (el.selectionStart || el.selectionStart === 0) {
                    el.focus();
                    el.setSelectionRange(caretPos, caretPos);
                    return true;
                }
                else {
                    el.focus();
                    return false;
                }
            }
        }
    }

    function addBr(str) {
        if (str) {
            var lines = str.trim().split(/\r\n|\n\r|\r|\n/);
            for (var i = 0; i < lines.length; i++) {
                lines[i] = Encoder.htmlEncode(lines[i]);
            }
            return lines.join("<br />");
        } else {
            return "";
        }
    }

    function sendTypingSignal(userName) {
        if (userName != sessionStorageManager.getItem("TenantGuid") && sendTypingSignalTimeout == null) {
            sendTypingSignalTimeout = setTimeout(function () {
                sendTypingSignalTimeout = null;
            }, 3000);
            socket.emit('sendTyping', userName);
        }
    }

    function chooseStatus(e) {
        chooseUserStatus(jq(e.currentTarget).attr("class").split(" ")[2]);
    }

    function chooseUserStatus(state) {
        var prevStatus = currentStatus,
            $showSmallChatIcon = jq(".show_small_chat_icon"),
            $smallChatPopupID = jq("#smallChatPopupID"),
            $smallChatTextStatus = jq(".small_chat_text_status"),
            $currentStatus = jq("." + currentStatus),
            realState;
        $smallChatPopupID.removeClass("display_block");
        $smallChatPopupID.addClass("display-none");
        $currentStatus.removeClass("disable");
        $smallChatTextStatus.removeClass(currentImage);
        $currentStatus.click(chooseStatus);
        currentStatus = state;
        sessionStorageManager.setItem("CurrentStatus", currentStatus);
        switch (state) {
            case ONLINE:
                currentImage = IMAGE_ONLINE;
                initConnect();
                break;
            case AWAY:
                currentImage = IMAGE_AWAY;
                initConnect();
                break;
            case NOT_AVAILABLE:
                currentImage = IMAGE_NOT_AVAILABLE;
                initConnect();
                break;
            default:
                currentImage = IMAGE_OFFLINE;
                if (sessionStorageManager.getItem("WasConnected")) {
                    connectionStop();
                }
        }
        $currentStatus = jq("." + currentStatus);
        $currentStatus.off("click");
        $currentStatus.addClass("disable");
        $smallChatTextStatus.addClass(currentImage);
        realState = getRealState(currentStatus);
        $smallChatTextStatus.text(realState);
        jq(".small_chat_status_menu").attr("title", realState);
        if (state == OFFLINE) {
            $showSmallChatIcon.addClass("small_chat_icon_white");
            $showSmallChatIcon.removeClass("small_chat_icon_green");
        } else {
            $showSmallChatIcon.addClass("small_chat_icon_green");
            $showSmallChatIcon.removeClass("small_chat_icon_white");
        }

        if (state != OFFLINE && sessionStorageManager.getItem("WasConnected")) {
            // sendStateToTenant
            socket.emit('sendStateToTenant', getUserNumberByState(currentStatus));
        }
    }

    function searchContactByEnter(e) {
        if (e.keyCode == ENTER_KEY_CODE) {
            clearTimeout(jq(e.currentTarget).data("timeoutId"));
            searchContact();
        } else {
            var timeoutId = setTimeout(function () {
                clearTimeout(jq(e.currentTarget).data("timeoutId"));
                searchContact();
            }, SEARCH_CONTACT_INTERVAL);
        }
    }

    function clearSearchText() {
        var $searchIcon = jq(".search_icon");
        jq(".small_chat_search_field").val("");
        searchContact();
    }

    function searchContact() {
        var template = jq(".small_chat_search_field").val().toLowerCase().trim(),
            $contactRecords = jq(".contact_record"),
            $contactBlocks = jq(".contact_block"),
            $searchIcon = jq(".search_icon"),
            $contactBlock,
            $contactRecord;

        for (var i = 0; i < $contactRecords.length; i++) {
            $contactRecord = jq($contactRecords[i]);
            $contactBlock = $contactRecord.parent();
            if ($contactRecord.text().toLowerCase().indexOf(template) == -1) {
                $contactBlock.removeClass("display_block");
                $contactBlock.addClass("display-none");
            } else {
                $contactBlock.addClass("display_block");
                $contactBlock.removeClass("display-none");
            }
        }
        if (!jq(".contact_block.display_block").length) {
            jq(".small_chat_contact_not_found_record").removeClass("display-none");
        } else {
            jq(".small_chat_contact_not_found_record").addClass("display-none");
        }
        if (template != "") {
            $searchIcon.removeClass("search_icon_image");
            $searchIcon.addClass("clear_text_image");
            $searchIcon.attr("title", ASC.Resources.Master.ChatResource.ClearText);
        } else {
            $searchIcon.removeClass("clear_text_image");
            $searchIcon.addClass("search_icon_image");
            $searchIcon.attr("title", ASC.Resources.Master.ChatResource.Search);
        }
    }

    function playSound(filename) {
        var $soundContainer = jq("#soundContainer");
        if (!$soundContainer.length) {
            $soundContainer = jq("<div>").attr("id", "soundContainer").appendTo(jq("body"));
        }
        $soundContainer.html('<audio autoplay="autoplay"><source src="' + filename + '.mp3" type="audio/mpeg" /><source src="'
            + filename + '.ogg" type="audio/ogg" /><source src="' + filename +
            '.wav" type="audio/wav" /><embed hidden="true" autostart="true" loop="false" src="' + filename + '.mp3" /></audio>');
    }

    function setChatHeight() {
        sessionStorageManager.setItem("SmallChatHeight", jq(".small_chat_main_window").css("height"));
    }

    function resizeChat() {
        if (isMobile) {
            return;
        }
        var $chat = jq(".small_chat_main_window");
        $chat.resizable({
            handles: "n",
            minHeight: 0,
            maxHeight: jq(window).height() - jq(".mainPageLayout").outerHeight(true) - HEIGHT_OFFSET,
            start: function (e, ui) {
                $chat.off("mouseenter", ".contact_block", openUserDetailList);
                $chat.resizable({
                    maxHeight: jq(window).height() - jq(".mainPageLayout").outerHeight(true) - HEIGHT_OFFSET
                });
            },
            stop: function (e, ui) {
                $chat.on("mouseenter", ".contact_block", openUserDetailList);
                $chat.resizable({
                    maxHeight: jq(window).height() - jq(".mainPageLayout").outerHeight(true) - HEIGHT_OFFSET
                });
                setChatHeight();
            }
        }).bind("resize", function () {
            jq(this).css("top", "auto");
        });

        $chat.resizable("enable");
        isResizeChat = true;
    }

    function initConnect() {
        if (!sessionStorageManager.getItem("WasConnected")) {
            connectionStart();
        }
    }

    function connectionStart() {
        if (!socket || !socket.connected() || already) {
            return;
        }
        already = true;
        
        socket.emit('connectUser', getUserNumberByStateForConnection());
    }

    function showErrorNotification() {
        var $noties = jq("body").find(".noty_text"),
            alreadyShow = false,
            error = jq(".small_chat_main_window").attr("data-error");
        if ($noties.length) {
            for (var i = 0; i < $noties.length; i++) {
                if (jq($noties[i]).text() == error) {
                    alreadyShow = true;
                }
            }
        }
        if (!alreadyShow) {
            noty({
                text: error,
                layout: "bottomRight",
                theme: "defaultTheme",
                type: "alert",
                animation: {
                    easing: "swing",
                    open: { "height": "toggle" },
                    close: { "height": "toggle" },
                    speed: "400",
                },
                timeout: 100000,
                maxVisible: 7,
                force: true
            });
        }
        ASC.Controls.JabberClient.extendChat = function () { ASC.Controls.JabberClient.open(); }
    }

    function connectionStop() {
        var $smallChatMainWindow = jq(".small_chat_main_window");
        if (!socket || !socket.connected() || already) {
            return;
        }
        already = true;
        sessionStorageManager.setItem("WasConnected", false);
        socket.emit('disconnectUser', true, function() {
            already = false;
        });
        if (pingTimerId) {
            clearInterval(pingTimerId);
            pingTimerId = null;
        }
        var $chatUserState = jq(".chat_user_state"),
            $conversationBlockUserState = jq(".conversation_block_user_state");
        $chatUserState.addClass(IMAGE_OFFLINE);
        $conversationBlockUserState.addClass(IMAGE_OFFLINE);
        $chatUserState.removeClass(IMAGE_ONLINE);
        $conversationBlockUserState.removeClass(IMAGE_ONLINE);
        $chatUserState.removeClass(IMAGE_AWAY);
        $conversationBlockUserState.removeClass(IMAGE_AWAY);
        $chatUserState.removeClass(IMAGE_NOT_AVAILABLE);
        $conversationBlockUserState.removeClass(IMAGE_NOT_AVAILABLE);

        var $onlineLists = jq(".online_user_list").children(),
            $awayLists = jq(".away_user_list").children(),
            $notAvailableLists = jq(".not_available_user_list").children(),
            $onlineContact,
            $awayContact,
            $notAvailableContact;
        for (var i = 0; i < $onlineLists.length; i++) {
            $onlineContact = jq($onlineLists[i]);
            $onlineContact.detach();
            var contactNumber = getContactNumberBeforeCurrent($onlineContact.find(".contact_record").text().toLowerCase(), "offline_user_list");
            if (contactNumber) {
                $onlineContact.insertAfter(".offline_user_list li:nth-child(" + contactNumber + ")");
            } else {
                $onlineContact.prependTo(".offline_user_list");
            }
        }
        for (var i = 0; i < $awayLists.length; i++) {
            $awayContact = jq($awayLists[i]);
            $awayContact.detach();
            var contactNumber = getContactNumberBeforeCurrent($awayContact.find(".contact_record").text().toLowerCase(), "offline_user_list");
            if (contactNumber) {
                $awayContact.insertAfter(".offline_user_list li:nth-child(" + contactNumber + ")");
            } else {
                $awayContact.prependTo(".offline_user_list");
            }
        }
        for (var i = 0; i < $notAvailableLists.length; i++) {
            $notAvailableContact = jq($notAvailableLists[i]);
            $notAvailableContact.detach();
            var contactNumber = getContactNumberBeforeCurrent($notAvailableContact.find(".contact_record").text().toLowerCase(), "offline_user_list");
            if (contactNumber) {
                $notAvailableContact.insertAfter(".offline_user_list li:nth-child(" + contactNumber + ")");
            } else {
                $notAvailableContact.prependTo(".offline_user_list");
            }
        }
    }

    function showOrHideStatusMenu() {
        var $smallChatPopupID = jq("#smallChatPopupID");
        if (!$smallChatPopupID.hasClass("display_block")) {
            $smallChatPopupID.removeClass("display-none");
            $smallChatPopupID.addClass("display_block");
        } else {
            $smallChatPopupID.removeClass("display_block");
            $smallChatPopupID.addClass("display-none");
        }
    }

    function closeNoty(e) {
        if (jq(e.target).attr("class") != "notification_close") {
            var userName = jq(e.currentTarget).find(".notification_username").attr("data-username"),
                $notifications = jq(".notification_username[data-username='" + userName + "']");
            if ($notifications.length) {
                $notifications = jq(".notification_username[data-username='" + userName + "']");
                for (var i = 0; i < $notifications.length; i++) {
                    jq($notifications[i]).closest("li").remove();
                }
                openMessageDialog(userName);
            }
         } else {
            jq(e.currentTarget).closest("li").remove();
        }
    }

    function activeWindow() {
        var $doc = jq(document);
        if (!isActive) {
            isActive = true;
            if (titleTimerId) {
                clearInterval(titleTimerId);
                titleTimerId = null;
                starsNumber = 1;
                for (var i = 0 ; i < shouldOpenUserDialogs.length; i++) {
                    openMessageDialog(shouldOpenUserDialogs[i]);
                }
                shouldOpenUserDialogs = [];
                setTimeout(function () {
                    $doc.find("title").text(originalTitle);
                }, TITLE_INTERVAL);
            }
        }
    }

    function reMap(smallObject, contract) {
        var largeObject = {};
        for (var smallProperty in contract) {
            largeObject[contract[smallProperty]] = smallObject[smallProperty];
        }
        return largeObject;
    }

    function handleMenuClickEvent(currentAccount, item) {
        var $this = jq(currentAccount),
            $smallChatCheckbox = $this.find(".small_chat_checkbox");
        if ($smallChatCheckbox.hasClass("small_chat_checkbox_enabled")) {
            $smallChatCheckbox.removeClass("small_chat_checkbox_enabled");
            $smallChatCheckbox.addClass("small_chat_checkbox_disabled");
            localStorageManager.setItem(item, false);
        } else {
            $smallChatCheckbox.addClass("small_chat_checkbox_enabled");
            $smallChatCheckbox.removeClass("small_chat_checkbox_disabled");
            localStorageManager.setItem(item, true);
        }
        //$this.closest("#smallChatOptionsPopupID").css("display", "none");
    }

    function minimizeAllWindowsIfLoseFocus(event, target) {
        if (localStorageManager.getItem("EnableMinimizeAllWindowsIfLoseFocus")) {
            if (target && jq(target).closest(".conversation_block, .small_chat_main_window").length) {
                return;
            }
            setTimeout(function () {
                if (!target && jq(".message_input_area:focus, .conversation_block:hover, .smile_menu:hover, .small_chat_main_window:hover").length) {
                    return;
                }
                minimizeConversationsBlocks();
            }, 100);
        }
    }

    function closeAllWindowsEvent(e) {
        closeAllConversationBlocks();
        closeAllMessageDialogItems();
        jq(this).closest("#smallChatOptionsPopupID").css("display", "none");
        return false;
    };

    function setSmallChatPosition() {
        var smallChatMainWindow = jq(".small_chat_main_window");
        var smallChatOptionsPopup = jq("#smallChatOptionsPopupID");
        if (!smallChatMainWindow.hasClass('small_chat_main_window_full')) {
            smallChatMainWindow.css("margin-left", (-1) * jq(document).scrollLeft());
            smallChatOptionsPopup.css("margin-left", (-1) * jq(document).scrollLeft());
        } else {
            smallChatOptionsPopup.css("margin-left", 0);
            smallChatMainWindow.css("margin-left", 0);
        }
    }

    function init() {
        
        setSmallChatPosition();
        jq(window).scroll(function (event) {
            setSmallChatPosition();
        });

        var $chat = jq(".small_chat_main_window"),
            $mainPageContent = jq(".mainPageContent"),
            $body = jq("body"),
            $window = jq(window);

        if (ASC.SocketIO && !ASC.SocketIO.disabled()) {
            socket = ASC.SocketIO.Factory.chat;
            initSocket();
        }

        sessionStorageManager.setItem("WasLoad", false);
        if (!sessionStorageManager.getItem("dialogsNumber")) {
            sessionStorageManager.setItem("dialogsNumber", 0);
        }
        if (sessionStorageManager.getItem("WasConnected")) {
            socket.connect(function () {
                connectionStart();
                showChat();
            });
        }

        $window.bind("beforeunload", function () {
            reloadPage = true;
        });
        $body.on("mousedown", ".noty_bar", closeNoty);
        $chat.off("click", ".show_small_chat_icon").
        on("click", ".show_small_chat_icon", showOrHideSmallChat).
        on("click", ".contact_block", function (e) {
            var userName = jq(e.currentTarget).attr("data-username");
            if (!isDialogOpen(userName)) {
                openMessageDialog(userName);
            } else {
                var $conversationBlock = jq(".conversation_block[data-username='" + userName + "']");
                if ($conversationBlock.find(".smile_icon").hasClass("display-none")) {
                    restore($conversationBlock);
                }
            }
        }).
        on("mouseenter", ".contact_block", openUserDetailList).
        on("mouseleave", ".contact_block", closeUserDetailList).
        on("mouseup", ".small_chat_status_menu", showOrHideStatusMenu).
        on("click", ".search_icon_image", searchContact).
        on("click", ".clear_text_image", clearSearchText).
        on("keydown", ".small_chat_search_field", searchContactByEnter);

        jq(".extend_chat_icon").on("click", extendChat);
        jq(".mainPageTableSidePanel").on("mouseenter", ".detail_user_list", function () {
            var $detailUserList = jq(this);
            $detailUserList.addClass("hover");
            $detailUserList.find(".link").addClass("text_underline");
        }).
        on("mouseleave", ".detail_user_list", function () {
            var $detailUserList = jq(this);
            $detailUserList.removeClass("hover");
            $detailUserList.find(".link").removeClass("text_underline");
            closeUserDetailList();
        });
        $mainPageContent.on("keydown", ".message_input_area", sendMessage).
        on("keyup", ".message_input_area", saveTextMessage).
        on("blur", ".message_input_area", minimizeAllWindowsIfLoseFocus).
        on("mouseover", ".conversation_block", function (e) {
            var $this = jq(e.currentTarget),
                userName = $this.attr("data-username");
            $this.find(".not_read_message").removeClass("not_read_message");
            var timerId = flashBlocks[userName];
            if (timerId) {
                clearInterval(timerId);
                var $headerOfConversationBlock = $this.find(".header_of_conversation_block");
                if ($headerOfConversationBlock.length && $headerOfConversationBlock.hasClass("main-header-color-hover")) {
                    $headerOfConversationBlock.addClass("main-header-color");
                    $headerOfConversationBlock.removeClass("main-header-color-hover");
                }
            }
            delete flashBlocks[userName];
        }).
        on("click", ".close_conversation_block", function (e) {
            closeConversationBlock(jq(e.currentTarget).closest(".conversation_block").attr("data-username"));
            if (jq(".message_dialog_btn").length) {
                var userName = jq("#messageDialogPopupID").find(".message_dialog_item").first().attr("data-username"),
                messageInMenu = sessionStorageManager.getItem("messageInMenu" + userName);
                closeMessageDialogItem(userName);
                openMessageDialog(userName, null, null, null, messageInMenu);
            }
        }).
        on("click", ".minimize_restore_conversation_block", function (e) {
            restOrMinConversationBlock(e);
        }).
        on("click", ".extend_conversation_block", extendChat).
        on("dblclick", ".header_of_conversation_block", restOrMinConversationBlock).
        on("click", ".smile", putSmileInMessage).
        on("click", ".message_dialog_close_item_icon", function (e) {
            closeMessageDialogItem(jq(e.currentTarget).closest(".message_dialog_item").attr("data-username"));
            jq("#messageDialogPopupID").css("top", parseInt(jq("#messageDialogPopupID").css("top")) + MESSAGE_WINDOW_ITEM_HEIGHT + PX);
            return false;
        }).
        on("click", ".message_dialog_item_text", function (e) {
            showMessageDialogFromMenu(jq(e.currentTarget).parent().attr("data-username"),
                sessionStorageManager.getItem("dialogsNumber"), getMaxDialogNumber());
        }).
        on("click", ".smile_icon", moveSmileMenu);
        jq("." + AWAY).click(chooseStatus);
        jq("." + NOT_AVAILABLE).click(chooseStatus);
        jq("." + OFFLINE).click(chooseStatus);

        jq(document).click(function (e) {
            var $block = jq(e.target),
                $smallChatPopupID = jq("#smallChatPopupID");
            if ($smallChatPopupID.hasClass("display_block") && !$block.hasClass("small_chat_status_menu") &&
                !$block.hasClass("small_chat_image_status") && !$block.hasClass("small_chat_text_status")) {
                $smallChatPopupID.removeClass("display_block");
                $smallChatPopupID.addClass("display-none");
            }
        });
        if (sessionStorageManager.getItem("dialogsNumber")) {
            var $allWindows = jq(".small_chat_minimize_all_windows");
            if (!$allWindows.length) {
                $allWindows = jq(".small_chat_restore_all_windows");
            }
            $allWindows.removeClass("disable");
            $body.on("click", ".small_chat_minimize_all_windows", function () {
                minimizeConversationsBlocks(true);
            }).on("click", ".small_chat_restore_all_windows", function () {
                restoreConversationsBlocks();
            });
        }
        if (sessionStorageManager.getItem("minimizeWindows")) {
            var $smallChatMinimizeAllWindows = jq(".small_chat_minimize_all_windows");
            $smallChatMinimizeAllWindows.addClass("small_chat_restore_all_windows");
            $smallChatMinimizeAllWindows.removeClass("small_chat_minimize_all_windows");
            $smallChatMinimizeAllWindows.text(ASC.Resources.Master.ChatResource.RestoreAllWindows);
        }
        if (sessionStorageManager.getItem("dialogsNumber")) {
            $body.on("click", ".small_chat_close_all_windows", closeAllWindowsEvent);
            jq(".small_chat_close_all_windows").removeClass("disable");
        }
        $window.focus(function () {
            activeWindow();
        }).blur(function () {
            if (isActive) {
                isActive = false;
            }
        }).mouseenter(function () {
            activeWindow();
        }).mouseleave(function () {
            if (isActive) {
                isActive = false;
            }
        });
        
        $window.on("resizeWinTimer", function () {
            if (sessionStorageManager.getItem("WasConnected")) {
                if ($window.outerHeight(true) - 58 >= CONVERSATION_BLOCK_HEIGHT && oldWindowHeight - 58 < CONVERSATION_BLOCK_HEIGHT) {
                    restoreConversationsBlocks();
                } else if ($window.outerHeight(true) - 58 < CONVERSATION_BLOCK_HEIGHT && oldWindowHeight - 58 >= CONVERSATION_BLOCK_HEIGHT) {
                    minimizeConversationsBlocks();
                }
                if ($window.outerHeight(true) - oldWindowHeight) {
                    oldWindowHeight = $window.outerHeight(true);
                    var maxHeightOfChat = oldWindowHeight - jq(".mainPageLayout").outerHeight(true) - HEIGHT_OFFSET;
                    $chat.resizable({
                        handles: "n",
                        minHeight: 0,
                        maxHeight: maxHeightOfChat
                    });
                    if ($chat.outerHeight(true) > maxHeightOfChat) {
                        jq(".small_chat_main_window").css("height", maxHeightOfChat + PX);
                        setChatHeight();
                    }
                }
            }

            maxDialogNumber = getMaxDialogNumber();

            var dialogsNumber = sessionStorageManager.getItem("dialogsNumber"),
                diff = dialogsNumber - maxDialogNumber,
                userName;
            if (!diff) {
                return;
            }
            if (diff > 0) {
                for (var i = 0; i < diff; i++) {
                    userName = sessionStorageManager.getItem("userName" + (dialogsNumber - 1));
                    if (userName) {
                        var message = sessionStorageManager.getItem("message" + userName);
                        closeConversationBlock(userName);
                        addToMenu({
                            UserName: userName,
                            ShowUserName: jq(".contact_block[data-username='" + userName + "']").find(".contact_record").text(),
                        }, null, message);
                        dialogsNumber = sessionStorageManager.getItem("dialogsNumber");
                    }
                }
            } else {
                var dialogsNumberInMenu;
                diff = -diff;
                for (var i = 0; i < diff; i++) {
                    dialogsNumberInMenu = sessionStorageManager.getItem("dialogsNumberInMenu");
                    if (dialogsNumberInMenu) {
                        userName = sessionStorageManager.getItem("dn_userName" + (dialogsNumberInMenu - 1));
                        if (userName) {
                            showMessageDialogFromMenu(userName, dialogsNumber, maxDialogNumber);
                        }
                    }
                }
            }

        });
        if (!isMobile) {
            $mainPageContent.on("click", ".message_bus_container", function (e) {
                if (window.getSelection().toString() == "") {
                    var $conversationBlock = jq(e.currentTarget).parent();
                    flashConversationBlock($conversationBlock.attr("data-username"));
                    $conversationBlock.find(".message_input_area").focus();
                }
            });

            jq.dropdownToggle({
                switcherSelector: ".small_chat_option_icon",
                dropdownID: "smallChatOptionsPopupID",
                addTop: -160,
                addLeft: 20,
                position: "fixed"
               
            });

            jq(".small_chat_en_dis_sounds").click(function () {
                handleMenuClickEvent(jq(this), "EnableSound");
            });

            jq(".small_chat_en_dis_ctrl_enter_sender").click(function () {
                handleMenuClickEvent(jq(this), "EnableCtrlEnter");
            });

            jq(".small_chat_minimize_all_windows_if_lose_focus").click(function () {
                handleMenuClickEvent(jq(this), "EnableMinimizeAllWindowsIfLoseFocus");
            });

            var $smallChatCheckbox = jq(".small_chat_en_dis_ctrl_enter_sender").find(".small_chat_checkbox");
            if (localStorageManager.getItem("EnableCtrlEnter")) {
                $smallChatCheckbox.removeClass("small_chat_checkbox_disabled");
                $smallChatCheckbox.addClass("small_chat_checkbox_enabled");
            } else {
                $smallChatCheckbox.removeClass("small_chat_checkbox_enabled");
                $smallChatCheckbox.addClass("small_chat_checkbox_disabled");
            }
            $smallChatCheckbox = jq(".small_chat_en_dis_sounds").find(".small_chat_checkbox");

            if (localStorageManager.getItem("EnableSound") === null) {
                localStorageManager.setItem("EnableSound", true);
            }

            if (localStorageManager.getItem("EnableSound")) {
                $smallChatCheckbox.addClass("small_chat_checkbox_enabled");
                $smallChatCheckbox.removeClass("small_chat_checkbox_disabled");
            } else {
                $smallChatCheckbox.addClass("small_chat_checkbox_disabled");
                $smallChatCheckbox.removeClass("small_chat_checkbox_enabled");
            }

            $smallChatCheckbox = jq(".small_chat_minimize_all_windows_if_lose_focus").find(".small_chat_checkbox");

            if (localStorageManager.getItem("EnableMinimizeAllWindowsIfLoseFocus")) {
                $smallChatCheckbox.removeClass("small_chat_checkbox_disabled");
                $smallChatCheckbox.addClass("small_chat_checkbox_enabled");
            } else {
                $smallChatCheckbox.removeClass("small_chat_checkbox_enabled");
                $smallChatCheckbox.addClass("small_chat_checkbox_disabled");
            }
        }
    }

    function logoutEvent() {
        if (socket.connected()) {
            connectionStop();
        }
    }

    return {
        init: init,
        openContacts: openContacts,
        extendChat: extendChat,
        minimizeAllWindowsIfLoseFocus: minimizeAllWindowsIfLoseFocus,
        logoutEvent: logoutEvent,
        getUserNumberByStateForConnection: getUserNumberByStateForConnection
    };
})();

jq(document).ready(function () {
    ASC.Controls.JabberClient.extendChat = SmallChat.extendChat;
    SmallChat.init();
});