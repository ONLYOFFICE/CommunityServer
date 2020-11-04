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
        jq("#studio_sidePanel").find("a.dropdown-item:not(.invite-link)").bind("click", onButtonClick);

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

    function initGroupList() {
        var groupList = jq.tmpl("groupListTemplate", { groups: window.GroupManager.getAllGroups() });

        jq("#groupList").empty().append(groupList);

        if (groupList.length) {
            jq("#groupListContainer .menu-item:first").toggleClass("none-sub-list sub-list");
        }
    };

    function initMenuList() {
        $(window).bind("change-group", onChangeGroup);
    }

    $(function() {
        initToolbar();
        initGroupList();
        initMenuList();
    });
})(jQuery);