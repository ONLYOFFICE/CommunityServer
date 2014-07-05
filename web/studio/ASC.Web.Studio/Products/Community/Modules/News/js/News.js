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

function showCommentBox() {
    if (CKEDITOR && !jq.isEmptyObject(CKEDITOR.instances)) {
        CommentsManagerObj.AddNewComment();
    } else {
        setTimeout("showCommentBox();", 500);
    }
}

function GetPreviewFull () {
    var html = CKEDITOR.instances.ckEditor.getData();

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

jq(document).ready(function() {
    jq.dropdownToggle({
        dropdownID: "eventsActionsMenuPanel",
        switcherSelector: ".eventsHeaderBlock .menu-small",
        addTop: -4,
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
    var $firstInput = jq("input[id$='_feedName']");
    if ($firstInput.length) {
        $firstInput.focus();
    }
    var anchor = ASC.Controls.AnchorController.getAnchor();
    if (anchor == "addcomment" && CommentsManagerObj) {
        showCommentBox();
    }

});
