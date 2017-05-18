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


var BlogsManager = new function() {
    this.ratingsSortField = 'Name';
    this.ratingsSortDirection = true; // true - asc; false - desc
    this.isSubscribe = false;
    this.groupid = null;
    this.userid = null;
    this.mSearchDefaultString = "";
    this.blogsEditor = null;

    this.BlockButtons = function () {
        LoadingBanner.showLoaderBtn("#actionBlogPage");
    };

    this.UnBlockButtons = function () {
        LoadingBanner.hideLoaderBtn("#actionBlogPage");
    };
    this.CheckData = function() {
        var titleText = jq("input[id$='PageContent_CommunityPageContent_txtTitle']").val();
        if (jq.trim(titleText) == "") {
            ShowRequiredError(jq("input[id$='PageContent_CommunityPageContent_txtTitle']"));
            BlogsManager.UnBlockButtons();
        }
        else {
            window.onbeforeunload = null;
            __doPostBack('', '');
        }
    };

    this.OnClickCancel = function () {
        window.onbeforeunload = null;
        BlogsManager.BlockButtons();
    };
    this.ShowPreview = function (titleid) {
        if (jq("#btnPreview").hasClass("disable")) return;

        Teamlab.getCmtPreview({},
            { title: jq('#' + titleid).val(), content: BlogsManager.blogsEditor.getData() },
            {
                before: function () { BlogsManager.BlockButtons(); },
                after: function () { BlogsManager.UnBlockButtons(); },
                success: function (params, response) {
                    jq('#previewBody').html(response.content);
                    jq('#previewTitle').html(response.title);
                    jq('#previewHolder').show();
                    var scroll_to = jq('#previewHolder').position();
                    jq.scrollTo(scroll_to.top, { speed: 500 });
                }
            });
    };


    this.HidePreview = function() {
        jq('#previewHolder').hide();
        var scroll_to = jq('#postHeader').position();
        jq.scrollTo(scroll_to.top, { speed: 500 });
    };

    this.SubscribeOnGroupBlog = function(groupID) {
        AjaxPro.onLoading = function(b) { if (b) { jq.blockUI(); } else { jq.unblockUI(); } };
        this.groupid = groupID;

        var subscribe;

        var elements = document.getElementsByName(groupID);
        if (elements[0].value == 1) {
            subscribe = true;
        }
        else {
            subscribe = false;
        }

        Default.SubscribeOnNewPostCorporate(groupID, subscribe, this.callbackSubscribeOnGroupBlog);
    };

    this.callbackSubscribeOnGroupBlog = function(result) {
        var elements = document.getElementsByName(BlogsManager.userid);
        var elementsLinks = document.getElementsByName('subscriber_' + BlogsManager.groupid);
        var subscribe = elements[0].value;

        for (var i = 0; i < elements.length; i++) {
            if (subscribe == 1)
                elements[i].value = 0;
            else
                elements[i].value = 1;

            elementsLinks[i].innerHTML = result.value;
        }
    };


    this.RatingSort = function(filedName) {
        if (this.ratingsSortField == filedName) {
            this.ratingsSortDirection = !this.ratingsSortDirection;
        }
        else {
            this.ratingsSortDirection = true;
        }

        this.ratingsSortField = filedName;

        AjaxPro.onLoading = function(b) { if (b) { jq('#blg_ratings').block(); } else { jq('#blg_ratings').unblock(); } };
        RatingList.Sort(this.ratingsSortField, this.ratingsSortDirection, this.callBackSort);
    };

    this.callBackSort = function(result) {
        if (result.value != null)
            jq('#blg_rating_list').html(result.value);
    };

    this.BlogTblSort = function(filedID) {
        if (this.ratingsSortField == filedID) {
            this.ratingsSortDirection = !this.ratingsSortDirection;
        }
        else {
            this.ratingsSortDirection = true;
        }

        this.ratingsSortField = filedID;

        AjaxPro.onLoading = function(b) { if (b) { jq('#blg_ratings').block(); } else { jq('#blg_ratings').unblock(); } };
        AllBlogs.Sort(this.ratingsSortField, this.ratingsSortDirection, this.callBackSort);
    };

    this.SubmitData = function (btnObj) {
        if (jq(btnObj).hasClass("disable"))
            return;
        
        BlogsManager.BlockButtons();
        BlogsManager.CheckData();
    };
};


BlogsManager.ratingsSortField = 'Name';
BlogsManager.ratingsSortDirection = true;

BlogSubscriber = new function() {
    this.SubscribeOnComments = function(blogID, state) {
        AjaxPro.onLoading = function(b) {
            if (b)
                LoadingBanner.displayLoading();
            else
                LoadingBanner.hideLoading();
        };

        Subscriber.SubscribeOnComments(blogID, state, function(result) {
            var res = result.value;
            jq('#blogs_subcribeOnCommentsBox').replaceWith(res.rs2);
        });
    };

    this.SubscribeOnBlogComments = function(blogID, state, link) {
        AjaxPro.onLoading = function(b) {
            if (b)
                LoadingBanner.displayLoading();
            else
                LoadingBanner.hideLoading();
        }
        
        Subscriber.SubscribeOnBlogComments(blogID, state, function(result) {
            var res = result.value;
            jq(link).replaceWith(res.rs2);
        });
        jq("#blogActionsMenuPanel").hide();
    };

    this.SubscribeOnPersonalBlog = function(userID, state) {
        AjaxPro.onLoading = function(b) {
            if (b)
                LoadingBanner.displayLoading();
            else
                LoadingBanner.hideLoading();
        }

        Subscriber.SubscribeOnPersonalBlog(userID, state, function(result) {
            var res = result.value;
            jq('#blogs_subcribeOnPersonalBlogBox').replaceWith(res.rs2);
        });
    };

    this.SubscribePersonalBlog = function(userID, state, link) {
        AjaxPro.onLoading = function(b) {
            if (b)
                LoadingBanner.displayLoading();
            else
                LoadingBanner.hideLoading();
        }

        Subscriber.SubscribePersonalBlog(userID, state, function(result) {
            var res = result.value;
            jq(link).replaceWith(res.rs2);
        });
    };

    this.SubscribeOnNewPosts = function(state) {
        AjaxPro.onLoading = function(b) {
            if (b)
                LoadingBanner.displayLoading();
            else
                LoadingBanner.hideLoading();
        }

        Subscriber.SubscribeOnNewPosts(state, function(result) {
            var res = result.value;
            jq('#blogs_subcribeOnNewPostsBox').replaceWith(res.rs2);
        });
    };

};


function blogTagsAutocompleteInputOnKeyDown(event) {
	//Enter key was pressed
	if (event.keyCode == 13) {
		return false;
	}
	return true;
};

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

function changeBlogsCountOfRows (val) {    
    var page = getURLParam("page");
    var search = getURLParam("search");
    var href = window.location.href.split("?")[0]+"?";
    href += "&size=" + val;
    if(page!=null)
        href += "&page=1";
    if(search!=null)
        href += "&search=" + search;
    window.location.href = href;
}

function resizeContent() {

    var windowWidth = jq(window).width() - 24 * 2,
           newWidth = windowWidth,
           mainBlockWidth = parseInt(jq(".mainPageLayout").css("min-width"));
    if (windowWidth < mainBlockWidth) {
        newWidth = mainBlockWidth;
    }

    jq(".BlogsBodyBlock, .ContentMainBlog, .container-list").each(
        function() {
            jq(this).css("max-width", newWidth - jq(".mainPageTableSidePanel").width() - 24 * 2 + "px");
        }
    );
       
};

jq(document).ready(function() {
    jq.dropdownToggle({
        dropdownID: "blogActionsMenuPanel",
        switcherSelector: ".BlogsHeaderBlock .menu-small",
        addLeft: -11,
        showFunction: function(switcherObj, dropdownItem) {
            jq('.BlogsHeaderBlock .menu-small.active').removeClass('active');
            if (dropdownItem.is(":hidden")) {
                switcherObj.addClass('active');
            }
        },
        hideFunction: function() {
            jq('.BlogsHeaderBlock .menu-small.active').removeClass('active');
        }
    });
    resizeContent();

    jq(window).resize(function() {
        resizeContent();
    });

    var elemParent = jq('.container-list .content-list div.asccut').parents(".content-list");
    for (var i = 0; i < elemParent.length; i++) {
        var href = jq(elemParent[i]).find('#postIndividualLink').text();
        jq(elemParent[i]).find(".comment-list").before('<div><a href="' + href + '" class="read-more"><font>' + ASC.Community.BlogsJSResource.ReadMoreLink + '</font></a></div>');
    }
    jq(".content-list p").filter(function(index) { return jq(this).html() == ("&nbsp;" && ""); }).remove();

    if (jq("#blogActionsMenuPanel .dropdown-content li").length == 0) {
        jq(".menu-small").hide();
    }
    var anchor = ASC.Controls.AnchorController.getAnchor();

    if (anchor && CommentsManagerObj) {
        CommentsManagerObj.onLoadComplete = function() {
            if (anchor == "addcomment") {
                ckeditorConnector.load(CommentsManagerObj.AddNewComment());
            } else if (anchor == "comments") {
                jq(window).scrollTop(jq('#commentsTitle').position().top, { speed: 500 });
            } else {
                jq(window).scrollTop(jq('#' + anchor).position().top, { speed: 500 });
            }
        };
    }

    if (jq("#actionBlogPage").length) {
        jq.confirmBeforeUnload();
    }
    var textInput = jq("input[id$='CommunityPageContent_txtTitle']");
    if (textInput.length)
        textInput.focus();
});
