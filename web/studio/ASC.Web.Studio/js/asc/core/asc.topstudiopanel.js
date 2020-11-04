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


;
jq(document).ready(function () {

    jq("#logout_ref").on('click', function () {
        if (localStorage.getItem('onlyoffice') == 'logout') {
            localStorage.setItem('onlyoffice', 'logout_');
        } else {
            localStorage.setItem('onlyoffice', 'logout');
        }
    });
    
    jq.dropdownToggle({
        switcherSelector: ".studio-top-panel .product-menu",
        dropdownID: "studio_productListPopupPanel",
        addLeft: -15,
        toggleOnOver: true,
        beforeShowFunction: function (switcherObj, dropdownItem) {
            var wrapper = dropdownItem.find(".wrapper").removeClass("col2 col3");
            var mainCount = wrapper.find(".main-nav-items .dropdown-content li:not(.dropdown-item-seporator)").length;
            var customCount = wrapper.find(".custom-nav-items .dropdown-content li:not(.dropdown-item-seporator)").length;
            var specCount = wrapper.find(".spec-nav-items .dropdown-content li:not(.dropdown-item-seporator)").length;

            if (mainCount + customCount + specCount > 12) {
                (mainCount + customCount < 13) || (customCount + specCount < 13) ?
                    wrapper.addClass("col2") : wrapper.addClass("col3");
            }
        },
        afterShowFunction: function (switcherObj, dropdownItem) {
            var wrapper = dropdownItem.find(".wrapper");

            wrapper.find(".dropdown-content li.dropdown-item-seporator").remove();

            var seporator = jq("<li/>").addClass("dropdown-item-seporator");

            var customContent = wrapper.find(".custom-nav-items");
            var specContent = wrapper.find(".spec-nav-items");

            var customLeft = wrapper.find(".custom-nav-items").position().left;
            var specLeft = wrapper.find(".spec-nav-items").position().left;

            var customCount = customContent.find("li").length;
            var specCount = specContent.find("li").length;

            if (customCount && customLeft == 0)
                customContent.find(".dropdown-content").prepend(seporator.clone());

            if (specCount && specLeft == 0)
                specContent.find(".dropdown-content").prepend(seporator.clone());

            if (specCount && customCount && customLeft == 150 && specLeft == 150)
                specContent.find(".dropdown-content").prepend(seporator.clone());

        }
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
        StudioBlockUIManager.blockUI('#aboutCompanyPopup', 680);
        jq('.studio-action-panel').hide();
    });

    if (jq("#appsPopupBody").length == 1) {
        var $appsBtn = jq("#studio_productListPopupPanel .apps .dropdown-item:first");
        jq.tmpl("template-blockUIPanel", {
            id: "appsPopup",
            headerTest: jq.trim($appsBtn.text())
        })
        .insertAfter($appsBtn)
        .addClass("confirmation-popup");

        jq("#appsPopup .containerBodyBlock:first")
            .replaceWith(jq("#appsPopupBody").removeClass("display-none").addClass("containerBodyBlock"));

        $appsBtn.on("click", function () {
            StudioBlockUIManager.blockUI('#appsPopup', 565);
            jq('.studio-action-panel').hide();
        });
    }

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
            StudioBlockUIManager.blockUI('#debugInfoPopUp', 1000);
            jq('.studio-action-panel').hide();
        });
    }
    if (jQuery.browser.msie || /iPad|iPhone|iPod/.test(navigator.userAgent) || /Sailfish/.test(navigator.userAgent) || /^((?!chrome|android).)*safari/i.test(navigator.userAgent)) {
        svg4everybody();
    }


    var $dropGiftPopupPanel = jq("#studio_dropGiftPopupPanel");

    if ($dropGiftPopupPanel.length) {
        jq.dropdownToggle({
            switcherSelector: ".studio-top-panel .giftBox",
            dropdownID: "studio_dropGiftPopupPanel",
            addTop: 5,
            addLeft: -392
        });

        $dropGiftPopupPanel.find(".left-btn").on("click", function () {
            if (Teamlab.profile.isAdmin) {
                $dropGiftPopupPanel.hide();
                location.href = jq(this).data("url");
            } else {
                $dropGiftPopupPanel.hide();
            }
        });

        $dropGiftPopupPanel.find(".right-btn").on("click", function () {
            if (Teamlab.profile.isAdmin) {
                $dropGiftPopupPanel.hide();
            } else {
                Teamlab.markGiftAsReaded({}, {
                    success: function () {
                        $dropGiftPopupPanel.hide();
                        jq(".studio-top-panel .giftBox").remove();
                    },
                    error: function (p, e) {
                        $dropGiftPopupPanel.hide();
                        jq(".studio-top-panel .giftBox").remove();
                        window.console.error(e);
                    }
                });
            }
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

                            if (!mails[i].subject || mails[i].subject.length === 0)
                                mails[i].subject = ASC.Resources.Master.Resource.MailNoSubject;

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
