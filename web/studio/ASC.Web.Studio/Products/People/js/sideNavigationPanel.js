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
(function($) {
    var productTitle = document.title;
    var clip = null;
    // binds
    function onButtonClick(evt) {
        var $this = jq(this);

        peopleActions.callMethodByClassname($this.attr('class'), this, [evt, $this]);
        jq(document.body).trigger('click');
    }

    function onChangeGroup(evt, groupId) {
        var $container = $('#peopleSidepanel'),
            defaultHeader = ASC.People.Resources.PeopleJSResource.People;
        groupId = groupId || '@persons';
        jq(".profile-title .menu-small").hide();
        jq(".profile-title .header").html(defaultHeader);
        jq(".profile-title .header").attr("title", defaultHeader);
        switch (groupId) {
            case '@persons':
                document.title = productTitle;
                $container.find('li.menu-sub-item.active').removeClass('active');
                $container.find('li.menu-item').removeClass('active').filter('li[data-id="' + groupId + '"]:first').addClass('active open');
                break;
            case '@clients':
                break;
            case '@freelancers':
                break;
            default:
                $container.find('li.menu-item.active').removeClass('active');
                $container.find('li.menu-sub-item').removeClass('active').filter('li[data-id="' + groupId + '"]:first').addClass('active');
                var nameGroup = jq(".menu-sub-list li.active a").html();
                jq(".profile-title .header").html(nameGroup).attr("title", nameGroup);
                document.title = htmlDecode(nameGroup) + " - " + defaultHeader;
                jq(".profile-title .menu-small").show();
                jq.dropdownToggle({
                    dropdownID: "actionGroupMenu",
                    switcherSelector: ".profile-title .menu-small",
                    addTop: 0,
                    addLeft: -11,
                    showFunction: function(switcherObj, dropdownItem) {
                        if (dropdownItem.is(":hidden")) {
                            switcherObj.addClass('active');
                        } else {
                            switcherObj.removeClass('active');
                        }
                    },
                    hideFunction: function() {
                        jq(".profile-title .menu-small.active").removeClass("active");
                    }
                });
                break;
        }
    }
    function htmlDecode(value) {
        return $('<div/>').html(value).text();
    }

    function initToolbar() {
        $("#peopleSidepanel").find("a.dropdown-item:not(.invite-link)").bind("click", onButtonClick);

        jq("#actionGroupMenu").find("a.dropdown-item").bind("click", onButtonClick);

        bindClipboardEvent();
    }

    function bindClipboardEvent() {
        if (!ASC.Clipboard.enable) {
            jq("#sideNavInviteLink").on("click", function () {
                window.peopleActions.invite_link();
            });
        } else {
            jq(window).one("onOpenSideNavOtherActions", function (event, switcherObj, dropdownItem) {
                window.sideNavInviteLinkClip = ASC.Clipboard.destroy(window.sideNavInviteLinkClip);

                var url = jq("#shareInviteUserLink").val();
                sideNavInviteLinkClip = ASC.Clipboard.create(url, "sideNavInviteLink", {
                    onComplete: function () {
                        window.peopleActions.invite_link();

                        if (typeof(window.toastr) !== "undefined") {
                            toastr.success(ASC.Resources.Master.Resource.LinkCopySuccess);
                        } else {
                            jq("#shareInviteUserLink, #sideNavInviteLink").yellowFade();
                        }
                    }
                });
            });
        }
    };


    function initMenuList() {
        $(window).bind("change-group", onChangeGroup);
    }

    $(function() {
        initToolbar();
        initMenuList();
    });
})(jQuery);