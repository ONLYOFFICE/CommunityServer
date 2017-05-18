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


ASC.Community.Wiki = (function () {
    listByLiteral = {};
    listByLiteralCount = 0;
    maxColumnCount = 3;

    subscribeNotyfy = false;
    subscribePageId = '';

    function insertInListByLiteral(letter, element) {
        if (!listByLiteral.hasOwnProperty(letter)) {
            listByLiteral[letter] = [element];
            listByLiteralCount++;
        } else {
            listByLiteral[letter] = jq.merge(listByLiteral[letter], [element]);
        }
    }

    function InitListPagesByLetter() {
        var aList = wikiCategoryAlfaList.split(','),
            pagesCount = wikiPages.length,
            resultSortedList = [],
            curLetter = '',
            pageLetter = '';

        if (pagesCount == 0) {
            jq("#wikiListPagesByLetterEmpty").removeClass("display-none");
            return;
        }

        for (var i = 0, n = aList.length; i < n; i++) {
            curLetter = aList[i];
            for (var pi = 0; pi < pagesCount; pi++) {
                pageLetter = (wikiPages[pi].PageName[0] || "").toUpperCase();

                if (i === 0 && aList.indexOf(pageLetter) === -1 //special symbols
                    || pageLetter === curLetter)
                {
                    insertInListByLiteral(curLetter, wikiPages[pi]);
                }
            }
        }

        if (listByLiteralCount <= maxColumnCount) {
            for (var j in listByLiteral) {
                resultSortedList.push([{ headName: j, pages: listByLiteral[j] }]);
            }
        } else {
            var elementsInColumn = Math.round((listByLiteralCount * 3 + pagesCount) / maxColumnCount),
                currentColumnInd = 0,
                count = 0;

            resultSortedList = [[], [], []];

            for (var j in listByLiteral) {
                count += 3 + listByLiteral[j].length;

                resultSortedList[currentColumnInd].push({ headName: j, pages: listByLiteral[j] });

                if (count > elementsInColumn) {
                    count = 0;
                    currentColumnInd++;
                }
            }
        }

        jq.tmpl("wikiListPagesByLetterTmpl", { mainlist: resultSortedList }).appendTo("#listWikiPages");

    };

    function InitListPages() {
        var pagesCount = wikiPages.length;

        if (pagesCount == 0) {
            jq("#wikiListPagesEmpty").removeClass("display-none");
            return;
        }

        jq.tmpl("wikiListPagesTmpl", { list: wikiPages }).appendTo("#listWikiPages");
    };

    function BindSubscribeEvent(notyfy, pageId, unNotifyOnEditPageText, notifyOnEditPageText) {
        subscribeNotyfy = notyfy;
        subscribePageId = pageId;

        jq("#statusSubscribe").on("click", function () {
            AjaxPro.onLoading = function (b) {
                if (b) LoadingBanner.displayLoading();
                else LoadingBanner.hideLoading();
            }
            MainWikiAjaxMaster.SubscribeOnEditPage(subscribeNotyfy, pageId, function (result) {

                subscribeNotyfy = result.value;
                if (!subscribeNotyfy) {
                    jq("#statusSubscribe").removeClass("subscribed").addClass("unsubscribed");
                    jq("#statusSubscribe").attr("title", notifyOnEditPageText);
                } else {
                    jq("#statusSubscribe").removeClass("unsubscribed").addClass("subscribed");
                    jq("#statusSubscribe").attr("title", unNotifyOnEditPageText);
                }
            });
        });
    };

    function PageHistoryVersionSelected(obj) {
        var isNewClicked = obj.id.indexOf('rbNewDiff') > 0;
        var version = obj.parentNode.getAttribute('_Version');
        var spans = document.getElementsByTagName('span');
        var curVersion;

        for (var i = 0; i < spans.length; i++) {

            if (spans[i].getAttribute('_Version')) {
                curVersion = spans[i].getAttribute('_Version') * 1;

                var thisColumn = (!isNewClicked && spans[i].firstChild.id.indexOf('rbNewDiff') < 0) || (isNewClicked && spans[i].firstChild.id.indexOf('rbNewDiff') > 0)
                if (thisColumn) {
                    spans[i].firstChild.checked = (curVersion == version);
                }
                else {
                    if ((isNewClicked && curVersion >= version) ||
                    (!isNewClicked && curVersion <= version)) {
                        spans[i].style.display = 'none';
                    }
                    else {
                        spans[i].style.display = '';
                    }
                }
            }
        }
    }

    return {
        InitListPagesByLetter: InitListPagesByLetter,
        InitListPages: InitListPages,
        BindSubscribeEvent: BindSubscribeEvent,
        PageHistoryVersionSelected: PageHistoryVersionSelected
    };
})(jQuery);

if (typeof window.__doPostBack !== "function") {
    function __doPostBack(eventTarget, eventArgument) {
        var theForm = document.forms['aspnetForm'];
        if (!theForm) {
            theForm = document.aspnetForm;
        }
        if (!theForm.onsubmit || (theForm.onsubmit() != false)) {
            if (eventTarget) {
                theForm.__EVENTTARGET.value = eventTarget;
            }
            if (eventArgument) {
                theForm.__EVENTARGUMENT.value = eventArgument;
            }
            theForm.submit();
        }
    }
}
