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


;
jq(document).ready(function () {

    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .product-menu",
        dropdownID: "studio_productListPopupPanel",
        addLeft: -15,
        toggleOnOver: true,
    });

    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .staff-profile-box",
        dropdownID: "studio_myStaffPopupPanel",
        rightPos: true
    });

    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .searchActiveBox",
        dropdownID: "studio_searchPopupPanel",
        addTop: 8,
        addLeft: 0,
        afterShowFunction: function () {
            jq("#studio_search").focus();

            var w = jq(window),
                scrWidth = w.width(),
                leftPadding = w.scrollLeft(),
                elem = jq(".studio-top-panel .searchActiveBox"),
                dropElem = jq("#studio_searchPopupPanel");

            if ((elem.offset().left + dropElem.outerWidth()) > scrWidth + leftPadding) {
                dropElem.css("left", Math.max(0, elem.offset().left - dropElem.outerWidth() + elem.outerWidth()) + "px");
            }
        }
    });

    jq("#studio_search").keydown(function (event) {
        var code;

        if (!e) {
            var e = event;
        }
        if (e.keyCode) {
            code = e.keyCode;
        } else if (e.which) {
            code = e.which;
        }

        if (code == 13) {
            Searcher.Search();
            return false;
        }

        if (code == 27) {
            jq("#studio_search").val("");
        }
    });

    jq("#studio_searchPopupPanel .search-btn").on("click", function() {
        Searcher.Search();
        return false;
    });
    UnreadMailManager.Init();


    var $aboutBtn = jq("#studio_myStaffPopupPanel .dropdown-about-btn:first");
    jq.tmpl("template-blockUIPanel", {
        id: "aboutCompanyPopup",
        headerTest: jq.trim($aboutBtn.text())
    })
    .insertAfter($aboutBtn)
    .addClass("confirmation-popup");

    jq("#aboutCompanyPopup .containerBodyBlock:first")
        .replaceWith(jq("#aboutCompanyPopupBody").removeClass("display-none").addClass("containerBodyBlock"));

    $aboutBtn.on("click", function () {
        StudioBlockUIManager.blockUI('#aboutCompanyPopup', 680, 600);
        jq('.studio-action-panel').hide();
    });


    if (jq("#debugInfoPopUpBody").length == 1) {
        var $debugBtn = jq("#studio_myStaffPopupPanel .dropdown-debuginfo-btn:first");
        jq.tmpl("template-blockUIPanel", {
            id: "debugInfoPopUp",
            headerTest: jq.trim($debugBtn.text()),
            innerHtmlText: ["<div style=\"height: 500px; overflow-y: scroll;\">", jq("#debugInfoPopUpBody").val().replace(/\n\r/g, "<br/>").replace(/\n/g, "<br/>"), "</div>"].join(''),
            OKBtn: 'Ok',
        })
        .insertAfter($debugBtn);

        jq("#debugInfoPopUp .button.blue.middle").on("click", function(){jq.unblockUI();});

        $debugBtn.on("click", function () {
            StudioBlockUIManager.blockUI('#debugInfoPopUp', 1000, 300, -300);
            jq('.studio-action-panel').hide();
        });
    }


});

var Searcher = new function () {
    this.Search = function () {
        var text = encodeURIComponent(jq("#studio_search").val());

        if (text == "") {
            return false;
        }

        var url = jq("#studio_search").attr("data-url");

        var selectedProducts = [];
        jq("#studio_searchPopupPanel .search-options-box :checkbox:checked").each(function (i, val) {
            selectedProducts.push(jq(val).attr("data-product-id"));
        });
        var productIds = selectedProducts.join(",");

        var productsUriPart = (productIds ? "&products=" + productIds : "");
        url += "?search=" + text + productsUriPart;

        window.open(url, "_self");
    };
};


var UnreadMailManager = new function () {

    this.Init = function () {
        if (StudioManager.getCurrentModule() === "mail")
            return; // Skips for mail module

        jq.dropdownToggle({
            switcherSelector: '.studio-top-panel .mailActiveBox',
            dropdownID: 'studio_dropMailPopupPanel',
            addTop: 5,
            addLeft: -392
        });

        jq('.studio-top-panel .mailActiveBox').on('click', function (event) {
            if (jq("#studio_dropMailPopupPanel").is(":visible")) {
                event.preventDefault();
                return;
            }
            var unreadMailCount = localStorageManager.isAvailable
                ? localStorageManager.getItem("MailUreadMessagesCount")
                : 0;

            if (event.which == 2 && unreadMailCount && event.which != 1) {
                return true;
            }
            if (unreadMailCount && jq(this).hasClass("has-led")) {
                showLoaderMail();
                Teamlab.getMailFilteredMessages({}, {
                    unread: true,
                    page_size: 10
                },
                {
                    success: onGetDropMail,
                    error: function(p, e) {
                        jq("#studio_dropMailPopupPanel").hide();
                        window.console.error(e);
                    }
                });
                event.preventDefault();
            } else {
                event.stopPropagation();
            }
            return true;
        });

        jq('#drop-mail-box').on("click", ".mark-all-btn", markReadMessages);

    }

    function showLoaderMail(){
        var $dropMailBox = jq('#drop-mail-box'),
            $loader = $dropMailBox.find('.loader-text-block'),
            $seeAllBtn = $dropMailBox.find('.see-all-btn'),
            $markAllBtn = $dropMailBox.find('.mark-all-btn'),
            $dropMailList = $dropMailBox.find('.list'),
            $emptyListText = $dropMailBox.find('.mail-readed-msg');
        $loader.show();
        $seeAllBtn.hide();
        $markAllBtn.hide();
        $dropMailList.hide();
        $emptyListText.hide();
    }

    function hideLoaderMail(isListEmpty) {
        var $dropMailBox = jq('#drop-mail-box'),
          $loader = $dropMailBox.find('.loader-text-block'),
          $seeAllBtn = $dropMailBox.find('.see-all-btn'),
          $markAllBtn = $dropMailBox.find('.mark-all-btn'),
          $dropMailList = $dropMailBox.find('.list'),
          $emptyListText = $dropMailBox.find('.mail-readed-msg');
        $loader.hide();
        $seeAllBtn.show();
        if (isListEmpty) {
            $dropMailList.hide();
            $emptyListText.show();
        } else {
            $markAllBtn.show();
            $dropMailList.show();
            $emptyListText.hide();
        }
    }

    function onGetDropMail(params, response) {
        var $dropMailBox = jq('#drop-mail-box'),
            dropMailList = $dropMailBox.find('.list'),
            mailActiveBox = jq(".mailActiveBox");

        if (response) {
            var mails = response;

            dropMailList.empty();

            var isListEmpty = true;

            if (mails.length > 0) {
                try {
                    var unreadMails = [];
                    var i, n;
                    for (i = 0, n = mails.length; i < n; i++) {
                        if (mails[i].isNew) {
                            if (unreadMails.length >= 10)
                                break;

                            var unreadMail = getMailTemplate(mails[i]);
                            unreadMails.push(unreadMail);
                        }
                    }

                    if (unreadMails.length > 0) {
                        isListEmpty = false;
                        jq.tmpl("dropMailsTmpl", unreadMails).appendTo(dropMailList);
                    } else {
                        hideLoaderMail(true);
                        var mailUrl =  mailActiveBox.prop("href");
                        setTimeout(function () { window.location.href = mailUrl; }, 1000);
                        return;
                    }

                } catch (e) {
                    console.error(e);
                }

            } else {
                mailActiveBox.removeClass("has-led");
            }

            hideLoaderMail(isListEmpty);
            dropMailList.scrollTop(0);
            return;
        }
    }

    function getMailTemplate(mail) {
        var tmpl = mail;
        tmpl.itemUrl = [location.protocol, '//', location.hostname, location.port ? ':' + location.port : '', "/addons/mail/#conversation/", mail.id].join('');
        tmpl.displayDate = window.ServiceFactory.getDisplayDate(window.ServiceFactory.serializeDate(mail.receivedDate));
        tmpl.displayTime = window.ServiceFactory.getDisplayTime(window.ServiceFactory.serializeDate(mail.receivedDate));

        return tmpl;
    }

    function markReadMessages() {
        var $items = jq("#drop-mail-box").find(".item"),
            ids = [];

        for (var j = 0, length = $items.length; j < length; j++) {
            ids.push(jq($items[j]).attr("data-id"));
        }

        Teamlab.markMailConversations({}, ids, "read", {
            success: function() {
                var storedCount = localStorageManager.getItem("MailUreadMessagesCount"),
                    $tpUnreadMessagesCountEl = jq("#TPUnreadMessagesCount"),
                    unread = storedCount - ids.length;

                if (storedCount !== unread) {
                    localStorageManager.setItem("MailUreadMessagesCount", unread);
                }

                if (ASC.Resources.Master.Hub.Url && ASC.Controls.MailReader) {
                    ASC.Controls.MailReader.updateFoldersOnOtherTabs(false);
                }

                if (unread > 0) {
                    $tpUnreadMessagesCountEl.html(unread > 100 ? ">100": unread);
                } else {
                    $tpUnreadMessagesCountEl.parent().toggleClass("has-led");
                }

                jq("#studio_dropMailPopupPanel").hide();
            },
            error: function (p, e) {
                jq("#studio_dropMailPopupPanel").hide();
                window.console.error(e);
            }
        });
    }
}
