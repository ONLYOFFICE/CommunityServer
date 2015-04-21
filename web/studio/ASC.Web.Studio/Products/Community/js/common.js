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


if (typeof ASC === "undefined")
    ASC = {};

if (typeof ASC.Community === "undefined")
    ASC.Community = (function() { return {} })();
    
ASC.Community.Common = (function() {
return {
    renderSideActionButton: function (linkData) {
        if(linkData.length > 0)
        {
        	jq("#studio_sidePanel #otherActions .dropdown-content").html("");
        	
        	for (var i = 0; i < linkData.length; i++) {
        		if(linkData[i].text) {
        			var container = jq("<li></li>");
        			var link = jq("<a></a>").css("cursor","pointer").addClass("dropdown-item").text(linkData[i].text);
        			if (linkData[i].id) {
        				link.attr("id", linkData[i].id);
        			}
        			if (linkData[i].href) {
        				link.attr("href", linkData[i].href);
        			}
        			container.append(link);
        			jq("#studio_sidePanel #otherActions .dropdown-content").append(container);
        			if (linkData[i].onclick) {
        				var func = linkData[i].onclick;
        				link.bind("click", func);
        			}
        		}
        	}
            
            jq("#studio_sidePanel #menuCreateNewButton").removeClass("big").addClass("middle");
            jq("#studio_sidePanel #menuOtherActionsButton").removeClass("display-none");
        }
    }
};
})(jQuery);

jq(function() {
    calculateWidthTitleBlock();
    jq(window).resize(function() {
        calculateWidthTitleBlock();
    });
    
    jq('#addUsersDashboard').bind('click', function() {
        jq('.dashboard-center-box, .backdrop').css('z-index', '500');
        ImportUsersManager.ShowImportControl();       
        return false;
    });

});

var calculateWidthTitleBlock = function() {
    var commonWidth = jq(window).width() - jq(".mainPageTableSidePanel").width();
    var titleWidth = commonWidth - 100;
    jq(".BlogsHeaderBlock").width(titleWidth);
};
