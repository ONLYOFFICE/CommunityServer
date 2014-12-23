/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

;
(function($) {
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
        var $buttons = $("#peopleSidepanel").find("a.dropdown-item");
        $buttons.bind("click", onButtonClick);

        //var $b = actionGroupMenu
        jq("#actionGroupMenu").find("a.dropdown-item").bind("click", onButtonClick);
    }

    function initMenuList() {
        $(window).bind("change-group", onChangeGroup);
    }

    $(function() {
        initToolbar();
        initMenuList();
    });
})(jQuery);