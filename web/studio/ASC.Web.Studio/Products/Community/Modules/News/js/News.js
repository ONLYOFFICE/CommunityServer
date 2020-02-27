/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


function toggleNewsControllist(element) 
{
    var divid = "textNewsDiv"+element.id.substr(2);
    
    if(document.getElementById(divid).className != "newsText") 
    {
        document.getElementById(divid).className = "newsText"; 
    } 
    else 
    {
        document.getElementById(divid).className = "newsFullText"; 
    }
}

function callbackRemove(result)
{
    if (result.value.rs1!="0")
    {
        var itemDiv = "viewItem";
        if (itemDiv!=null)
        {
            var divDel = document.getElementById(itemDiv);
            divDel.className = 'errorBox errorText';
            divDel.innerHTML = result.value.rs2;
        }
    }
}


var SubscribeOnComments = function(btn, feedId, subscribeText, unsubscribeText) {

    Teamlab.subscribeCmtEventComment({}, feedId, {feedid: feedId, isSubscribe: jq(btn).hasClass("subscribed")},
    {
        before: function(params) {
            LoadingBanner.displayLoading();
            jq('#eventsActionsMenuPanel').hide();
            jq('.menu-small').removeClass('active');

        },
        after: function(params) { LoadingBanner.hideLoading(); },
        success: function(params, result) {
            if(!result){
                jq('#statusSubscribe').removeClass('subscribed').addClass('unsubscribed');
                jq('#statusSubscribe').attr('title', subscribeText);
            } else {
                jq('#statusSubscribe').removeClass('unsubscribed').addClass('subscribed');
                jq('#statusSubscribe').attr('title', unsubscribeText);
            }
        }
    });
 
};

function callbackRemoveFromTable(result)
{
    if (result.value.rs1!="0")
    {
        var itemDiv = "item_"+result.value.rs1;
        if (itemDiv!=null)
        {
            var trDel = document.getElementById(itemDiv);
            trDel.className = 'errorBox errorText';
            var firstTD = 0;
            for(var i = 0; i < trDel.childNodes.length; i++)
            {
                if(trDel.childNodes[i].tagName == 'TD')
                {
                    if (firstTD == 0)
                    {
                        trDel.childNodes[i].innerHTML = "<td>"+result.value.rs2+"</td>"
                        firstTD = 1;
                    }
                    else
                    {
                        trDel.childNodes[i].innerHTML = "<td></td>"
                    }
                        
                }
            }
            
        }
    }
}

function ShowMore()
{
    jq('#divMore').show();
}

function HideMore()
{
    jq('#divMore').hide();
}

function NewsBlockButtons()
{
    LoadingBanner.showLoaderBtn("#actionNewsPage");
}
function NewsUnBlockButtons() {
    LoadingBanner.hideLoaderBtn("#actionNewsPage");
}
function CheckData() {
    var question = jq("input[name$='poll_question']");
    if (jq(question).val()=="") {
        ShowRequiredError(question);
        NewsUnBlockButtons();
        return;
    }
    else {
        __doPostBack('', '');
    }
}

function CheckDataNews() {
    var newsName = jq("input[id$='_feedName']");
    if (jq(newsName).val() == "") {
        ShowRequiredError(newsName);
        NewsUnBlockButtons();
        return;
    }
    else {
        __doPostBack('', '');
    }
}

function getURLParam(strParamName) {

        strParamName = strParamName.toLowerCase();

        var strReturn = "";
        var strHref = window.location.href.toLowerCase();
        var bFound = false;

        var cmpstring = strParamName + "=";
        var cmplen = cmpstring.length;

        if (strHref.indexOf("?") > -1) {
            var strQueryString = strHref.substr(strHref.indexOf("?") + 1);
            var aQueryString = strQueryString.split("&");
            for (var iParam = 0; iParam < aQueryString.length; iParam++) {
                if (aQueryString[iParam].substr(0, cmplen) == cmpstring) {
                    var aParam = aQueryString[iParam].split("=");
                    strReturn = aParam[1];
                    bFound = true;
                    break;
                }

            }
        }
        if (bFound == false) return null;

        if (strReturn.indexOf("#") > -1)
            return strReturn.split("#")[0];

        return strReturn;
}

function changeCountOfRows (val) {
    var type = getURLParam("type");
    var search = getURLParam("search");
    var href = window.location.href.split("?")[0]+"?";
    if(type!=null)
        href += "&type=" + type;
    if(search!=null)
        href += "&search=" + search;
    window.location.href = href + "&size=" + val;
}

function GetPreviewFull() {
    if (jq("#btnPreview").hasClass("disable")) return;
    
    var html = newsEditor.getData();

    AjaxPro.onLoading = function (b) {
        if (b) {
            LoadingBanner.showLoaderBtn("#actionNewsPage");
        } else {
            LoadingBanner.hideLoaderBtn("#actionNewsPage");
        }
    };

    EditNews.GetPreviewFull(html, function (result) {
        FeedPrevShow(result.value);
    });
}

function resizeContent() {

    var windowWidth = jq(window).width() - 24 * 2,
        newWidth = windowWidth,
        mainBlockWidth = parseInt(jq(".mainPageLayout").css("min-width"));
    if (windowWidth < mainBlockWidth) {
        newWidth = mainBlockWidth;
    }

    jq("#feedPrevDiv").each(
        function() {
            jq(this).css("max-width", newWidth - jq(".mainPageTableSidePanel").width() - 24 * 2 + "px");
        }
    );
};

function submitNewsData(btnObj) {
    if (jq(btnObj).hasClass("disable"))
        return;
        
    NewsBlockButtons();
    CheckDataNews();
};

function submitPollData(btnObj) {
    if (jq(btnObj).hasClass("disable"))
        return;

    NewsBlockButtons();
    CheckData();
};

jq(document).ready(function() {
    jq.dropdownToggle({
        dropdownID: "eventsActionsMenuPanel",
        switcherSelector: ".eventsHeaderBlock .menu-small",
        addLeft: -11,
        showFunction: function(switcherObj, dropdownItem) {
        jq('.eventsHeaderBlock .menu-small.active').removeClass('active');
            if (dropdownItem.is(":hidden")) {
                switcherObj.addClass('active');
            }
        },
        hideFunction: function() {
        jq('.eventsHeaderBlock .menu-small.active').removeClass('active');
        }
    });
    if (jq('#eventsActionsMenuPanel .dropdown-content a').length == 0) {
        jq('span.menu-small').hide();
    }
    var $firstInput = jq("input[id$='feedName']");
    if ($firstInput.length) {
        $firstInput.focus();
    }
    var anchor = ASC.Controls.AnchorController.getAnchor();
    if (anchor == "addcomment" && CommentsManagerObj) {
        ckeditorConnector.load(CommentsManagerObj.AddNewComment);
    }

    resizeContent();

    jq(window).resize(function () {
        resizeContent();
    });

});
