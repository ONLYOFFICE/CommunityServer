/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


/*
Copyright (c) Ascensio System SIA 2013. All rights reserved.
http://www.teamlab.com
*/

window.helpPanel = (function($) {
    var isInit = false,
        panelContent;

    var init = function() {
        if (isInit === false) {
            isInit = true;
            panelContent = $('#studio_sidePanel .help-center');
        }
    };

    var unmarkSettings = function() {
        var $sections = $(panelContent).find('.menu-sub-list').children();

        if ($(panelContent).hasClass('active')) {
            $(panelContent).toggleClass('active');
        }

        if ($(panelContent).hasClass('currentCategory')) {
            $(panelContent).toggleClass('currentCategory');
        }


        for (var i = 0, n = $sections.length; i < n; i++) {
            var $item = $($sections[i]);
            if ($item.hasClass('active')) {
                $item.toggleClass('active', false);
            }
        }
    };

    var selectItem = function(number) {
        if (number == 'all') {
            $(panelContent).toggleClass('active', true);
        } else {
            var $sections = $(panelContent).find('.menu-sub-list').children();
            $($sections[number]).toggleClass('active', true);
        }

    };

    return {
        init: init,
        selectItem: selectItem,
        unmarkSettings: unmarkSettings
    };

})(jQuery);