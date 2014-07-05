/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
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
