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


///////////////////////////////////////////////////////////////
///////			Save Bookmark begin
///////////////////////////////////////////////////////////////
function saveBookmarkButtonClick(singleBookmarkDivID, uniqueID) {
	AjaxPro.queue.abort();
	jq('#' + tagsAutocompleteImageID).hide();
	emptyUrlAlert(false);
	if ('' == getBookmarkUrlInput().val()) {
		emptyUrlAlert(true);
		return;
	}
	updateBookmarkData();
	var url = getBookmarkUrlInput().val();
	var name = getBookmarkNameInput().val();
	var description = getBookmarkDescriptionInput().val();
	var tags = getBookmarkTagsInput().val();

	disableFieldsForSaveBookmarkAjaxRequest(true, singleBookmarkDivID);
	BookmarkPage.SaveBookmarkAjax(url, name, description, tags, uniqueID,
							  function(result) {
							      if (!result.value && result.error && result.error.Message) {
							          hideAddBookmarkPanelWithAnimation(250);
							          toastr.error(result.error.Message);
							          return;
							      }
							      var v = result.value;
							      hideAddBookmarkPanelWithAnimation(250);
							      disableFieldsForSaveBookmarkAjaxRequest(false, singleBookmarkDivID);
							      if ('' != v.AddedBy) {
							          jq('#' + v.DivID).remove();
							          jq('#BookmarkedByPanel').html(jq('#BookmarkedByPanel').html() + v.AddedBy);
							          animateBookmarkChange(v.DivID);
							      }
							      setTimeout(
							  					function() {
							  					    jq('#AddBookmarkPanelToMove').appendTo(jq('#AddBookmarkPanel'));
							  					    jq('#' + singleBookmarkDivID).html(v.BookmarkString);
							  					},
							  					200
							  				);
							  }
				 );
}

function createNewBookmarkButtonClick() {
    if (jq("#SaveBookmarkButton").hasClass("disable"))
        return;

    jq('#' + tagsAutocompleteImageID).hide();
	emptyUrlAlert(false);
	if ('' == getBookmarkUrlInput().val()) {
		emptyUrlAlert(true);
		return;
	}
	
	updateBookmarkData();
	var url = getBookmarkUrlInput().val();
	var name = getBookmarkNameInput().val();
	var description = getBookmarkDescriptionInput().val();
	var tags = getBookmarkTagsInput().val();	

	disableFieldsForSaveBookmarkAjaxRequest(true, null);
	BookmarkPage.SaveBookmark(url, name, description, tags,
								function(result) {
									document.location.href = "Default.aspx";
								}
				 );
}

function addBookmarkToFavourite(url, raitingHtmlDivId, singleBookmarkDivID, uniqueID) {
	//Create thumbnail if it doesn't exist
	BookmarkPage.MakeThumbnail(url, function(result) { });

	var removeButtons = jq('div[class*="bookmarkingRemoveFromFavourites"]');
	if (removeButtons != null && removeButtons.lenght > 0) {
		jq(removeButtons[0]).hide();
	}	

    jq('#SaveBookmarkButtonCopy').attr('href', "javascript:saveBookmarkButtonClick('" + singleBookmarkDivID + "', '" + uniqueID + "');");

	var name = jq('#' + singleBookmarkDivID + 'Name').html();
	var description = jq('#' + singleBookmarkDivID + 'Description').html();
	var tags = jq('#' + singleBookmarkDivID + 'Tags').html();

	description = checkParam(description);

	description = description.replace(/<br>/gi, "\n");
	description = description.replace(/<br \/>/gi, "\n");
	description = description.replace(/<br\/>/gi, "\n");

	getBookmarkUrlInput().val(checkParam(url));
	getBookmarkNameInput().val(checkParam(name));
	getBookmarkDescriptionInput().val(checkParam(description));
	getBookmarkTagsInput().val(checkParam(tags));

	showHideCheckBookmarkUrlButtons(true);

	//Displaying raiting next to the address input during adding bookmark to the favourites
	try {
		var removeButton = null;
		var removeButtons = jq('div[class*="bookmarkingRemoveFromFavourites"]');
		if (removeButtons != null && removeButtons.length > 0) {
			removeButton = removeButtons[0];
		}
		showHideRemoveButton(removeButton, false);
		jq('#NewBookmarkRaiting').html(jq('#' + raitingHtmlDivId).html());
		showHideRemoveButton(removeButton, true);
	} catch (err) { }

	moveAddBookmarkPanel(raitingHtmlDivId + "ToAppend");
	showHideSaveBookmarkButton(false);
	jq('#BookmarkDetailsPanel').show();
	disableSaveButton(false);
	updateThumbnailAsync();

	setFocusOnFeild('BookmarkName');
}

function checkParam(param) {
	if (param == null) {
		return '';
	}
	return param;
}

function showHideRemoveButton(removeButton, flag) {
	if (removeButton != null) {
		if (flag) {
			jq(removeButton).show();
		} else {
			jq(removeButton).hide();
		}
	}
}
///////////////////////////////////////////////////////////////
///////			Save Bookmark end
///////////////////////////////////////////////////////////////


///////////////////////////////////////////////////////////////
///////			Remove Bookmark start
///////////////////////////////////////////////////////////////
function removeBookmark(userBookmarkID) {
    StudioBlockUIManager.blockUI('#removeBookmarkDialog', 400);
    jq(".studio-action-panel").hide();
    jq("#deleteBookmark").removeClass("active");
    var removeButton = jq("#BookmarkingRemoveLink");
    removeButton.off("click");
    removeButton.on("click", function () {
        BookmarkPage.RemoveBookmark(userBookmarkID, function() {
            window.location.href = "Default.aspx";
        });
    });
}

function removeBookmarkFromFavourite(userBookmarkID, singleBookmarkDivID, favouriteBookmarksMode, uniqueID) {
	disableFieldForDeleteAjaxRequest(true);
	if (favouriteBookmarksMode) {
		BookmarkPage.RemoveBookmarkFromFavouriteInFavouriteMode(userBookmarkID,
											 function(result) {
											 	disableFieldForDeleteAjaxRequest(false);
											 	showHidePanel(singleBookmarkDivID, false);
											 	if (jq(".bookmarks-row:visible").length < 2) {
											 	    document.location.reload();
											 	    return;
											 	}
											 }
					);
	} else {
	BookmarkPage.RemoveBookmarkFromFavourite(userBookmarkID, uniqueID,
											 function(result) {
											 	disableFieldForDeleteAjaxRequest(false);
											 	var v = result.value;
											 	if (v == null) {
											 		document.location.href = "Default.aspx";
											 		return;
											 	}
											 	if (v == '') {
											 	    showHidePanel(singleBookmarkDivID, false);
											 	    if (jq(".bookmarks-row:visible").length < 2) {
											 	        document.location.reload();
											 	        return;
											 	    }
											 	} else {
											 		animateBookmarkChange(singleBookmarkDivID);
											 		showHidePanel(v.ID, false, 250);
											 		setTimeout(function() { jq('#' + v.ID).remove(); }, 250);
											 		jq('#' + singleBookmarkDivID).html(v.Bookmark);
											 	}
											 }
					);
	}
}

function removeBookmarkFromFavouriteConfirmDialog(	userBookmarkID,
													bookmarkInfoUrl, singleBookmarkDivID,
													favouriteBookmarksMode, uniqueID) {
	try {

		StudioBlockUIManager.blockUI("#removeBookmarkConfirmDialog", 400);

        jq("#BookmarkingRemoveFromFavouriteLink").attr('href', "javascript: removeBookmarkFromFavourite('" + userBookmarkID +
				"', '" + singleBookmarkDivID + "', " + favouriteBookmarksMode + ", '" + uniqueID + "');");

		jq('#BookmarkToRemoveFromFavouriteName').attr('href', bookmarkInfoUrl);
		var name = jq('#' + singleBookmarkDivID + 'Name').html();
		jq('#BookmarkToRemoveFromFavouriteName').html(name);

		setFocusOnFeild('BookmarkingRemoveFromFavouriteLink');
	}
	catch (e) { };
}

function removeBookmarkFromFavouriteSpanClick(removeFlag, bookmarkID) {
	var AddToFavourite = jq('#' + bookmarkID + "AddToFavourite");
	var RemoveFromFavourite = jq('#' + bookmarkID + "RemoveFromFavourite");
	if (removeFlag) {
		AddToFavourite.hide();
		RemoveFromFavourite.show();
	} else {
		AddToFavourite.show();
		RemoveFromFavourite.hide();
	}
}
///////////////////////////////////////////////////////////////
///////			Remove Bookmark end
///////////////////////////////////////////////////////////////


///////////////////////////////////////////////////////////////
///////			Ajax Request preparation start
///////////////////////////////////////////////////////////////
function disableFieldsForSaveBookmarkAjaxRequest(flag, singleBookmarkDivID) {
	disableBookmarkingField('BookmarkUrl', flag);
	disableBookmarkingField('BookmarkName', flag);
	disableBookmarkingField('BookmarkDescription', flag);
	disableBookmarkingField('BookmarkTagsInput', flag);
	if (!flag) {
		animateBookmarkChange(singleBookmarkDivID);
		jq('#bookmarkingCreateNewBookmarkButtonsDiv').show();
		clearInputFields();
		hideAddBookmarkPanel();
	}
}
function disableBookmarkingField(elementID, flag) {
	var el = jq('#' + elementID);
	if (el != null) {
		el.attr('disabled', flag);
	}
}

function disableFieldForDeleteAjaxRequest(flag) {
	if (!flag) {
		jq.unblockUI();
		jq('#bookmarkingRemoveFromFavouriteButtonsDiv').show();
	}
}

function animateBookmarkChange(singleBookmarkDivID) {
	if (singleBookmarkDivID == null) {
		return;
	}
	try {
		var el = jq('#' + singleBookmarkDivID);
		var bg = el.css("background-color");
		bg = bg.toLowerCase();
		el.css({ "background-color": "#ffffcc" });
		if (bg == "#edf6fd" || bg == "rgb(237, 246, 253)") {
			el.animate({ backgroundColor: '##EDF6FD' }, 2000);
		} else {
			el.animate({ backgroundColor: "#ffffff" }, 2000);
		}
	} catch (err) { el.css({ "background-color": "#ffffff" }); }
}
///////////////////////////////////////////////////////////////
///////			Ajax Request preparation end
///////////////////////////////////////////////////////////////




jq(document).ready(function() {
	updateThumbnailAsync();
})

function updateThumbnailAsync() {
    //setTimeout(function() { updateThumbnailSrc(); }, 100);
    setTimeout(function () {
        jq(".bookmarkingThumbnail").each(function() {
            var th = jq(this);
            th.attr("src", th.attr("data-url"));
        });
    }, 100);
}

function emptyUrlAlert(showFlag) {
	var infoPanel = jq('div[id$="_InfoPanel"]');
	if (!showFlag) {
		infoPanel.hide();
		return;
	}
	infoPanel.parent().addClass('alert');
	infoPanel.html(jq('#EmptyBookmarkUrlErrorMessageHidden').val()).show();
}

var iterationsNumber = 0;

function updateThumbnailSrc() {
	iterationsNumber++;	
	if (iterationsNumber > 10) {
		return;
	}
	try {
		AjaxPro.onLoading = function(b) { };
		BookmarkPage.UpdateThumbnailImageSrc(function(result) {
			var res = result.value;
			if (res == null) {
				setTimeout(function() { updateThumbnailSrc(); }, 10000);
				return false;
			}
			var url = res.url;
			if (url != null && "" != url) {
				updateThumbnails(res.url, res.thumbnailUrl);
			}
		});
	} catch (err) {
		return true;
	}
	return true;
}

function updateThumbnails(url, thumbnailUrl) {
	if (thumbnailUrl == null || '' == thumbnailUrl) {
		thumbnailUrl = 'App_Themes/default/images/noimageavailable.jpg';
	}
	jq("img[alt='" + url + "']").each(function() {
		jq(this).attr("src", thumbnailUrl);
	})
}

function showBookmarks(data, divComponent) {
	divComponent.html('');
	divComponent.append(data);
}

function updateBookmarkData() {
	var url = getBookmarkUrlInput().val();
	url = modifyBookmarkURL(url);	
	getBookmarkUrlInput().val(url);	
	
	if ('' == getBookmarkNameInput().val()) {
		getBookmarkNameInput().val(getBookmarkUrlInput().val());
	}
}

function updateBookmarkURL() {
	var url = getBookmarkUrlInput().val();
	url = url.replace(new RegExp("\\s*", "g"), "");

	url = modifyBookmarkURL(url);
	
	getBookmarkUrlInput().val(url);
	return url;
}

function modifyBookmarkURL(url) {
	if (url.indexOf("http://") == 0 || url.indexOf("https://") == 0) {
		return url;
	}	
	url = "http://" + url;	
	return url;
}

function showAddBookmarkPanel() {
	cancelButtonClick();
	jq('#AddBookmarkPanel').show();
	showHideCheckBookmarkUrlButtons(false);
	jq('#NewBookmarkRaiting').html('');
	disableAddBookmarkElements(true);

	setFocusOnFeild('BookmarkUrl');
}

function hideAddBookmarkPanel() {
	jq('#AddBookmarkPanel').hide();
	disableAddBookmarkElements(false);
}

function clearInputFields() {
	getBookmarkUrlInput().val('');
	getBookmarkNameInput().val('');
	getBookmarkDescriptionInput().val('');
	getBookmarkTagsInput().val('');
}

function cancelButtonClick() {
	hideAddBookmarkPanel();
	clearInputFields();
	moveAddBookmarkToFavouritePanel();
	showHideSaveBookmarkButton(true);
	disableSaveButton(false);
	AjaxPro.queue.abort();
}

function disableSaveButton(flag) {
	jq('#SaveBookmarkButton').attr('disabled', flag);
	jq('#SaveBookmarkButtonCopy').attr('disabled', flag);
}

function copyValue(sourceElementID, destElementID) {
	jq('[id$="' + '_' + destElementID + '"]').val(jq('#' + sourceElementID).val());
}

function getBookmarkByUrl() {
	var url = updateBookmarkURL();
	emptyUrlAlert(false);
	if ('' == url || 'http://' == url || 'https://' == url) {
		emptyUrlAlert(true);
		return;
	}
	BookmarkPage.GetUserBookmarkByUrl(url,
									  function(result) {
									  	showHideCheckBookmarkUrlButtons(true);
									  	var res = result.value;
									  	if (!res.IsNewBookmark) {
									  		getBookmarkNameInput().val(res.Name);
									  		getBookmarkDescriptionInput().val(res.Description);
									  		getBookmarkTagsInput().val(res.Tags);
									  		showHideSaveBookmarkButton(false);
									  	} else {
									  		if ("" != res.Name) {
									  			getBookmarkNameInput().val(res.Name);
									  			getBookmarkDescriptionInput().val(res.Description);
									  		}
									  		showHideSaveBookmarkButton(true);
									  	}
									  	jq('#NewBookmarkRaiting').html(res.Raiting);
									  	disableAddBookmarkElements(false);

									  	setFocusOnFeild('BookmarkName');
									  }
								  );
}

function disableAddBookmarkElements(disableFlag) {
	getBookmarkUrlInput().attr('disabled', !disableFlag);
	getBookmarkNameInput().attr('disabled', disableFlag);
	getBookmarkDescriptionInput().attr('disabled', disableFlag);
	getBookmarkTagsInput().attr('disabled', disableFlag);
}

function showHideCheckBookmarkUrlButtons(showHideFlag) {
	if (showHideFlag) {
		jq('#BookmarkDetailsPanel').show();
		jq('#CheckBookmarkUrlButtonsPanel').hide();
	} else {
		jq('#BookmarkDetailsPanel').hide();
		jq('#CheckBookmarkUrlButtonsPanel').show();
	}
}


function moveAddBookmarkPanel(divToAppend) {	
	jq('#AddBookmarkPanelToMove').appendTo(jq('#' + divToAppend));
	jq('#AddBookmarkPanelToMove').show();
	jq('#AddBookmarkPanelToMove').hide();
	jq('#AddBookmarkPanelToMove').animate({ height: 'show', opacity: 'show' }, "normal");
	
	hideAddBookmarkPanel();
}

function hideAddBookmarkPanelWithAnimation() {
	jq('#AddBookmarkPanelToMove').animate({ height: 'hide', opacity: 'hide' }, 'slow');
}

function hideAddBookmarkPanelWithAnimation(delay) {
	jq('#AddBookmarkPanelToMove').animate({ height: 'hide', opacity: 'hide' }, delay);
}

function showHidePanel(panelID, flag) {
	showHidePanel(panelID, flag, 'slow');
}

function showHidePanel(panelID, flag, delay) {
	var showHide = 'show';
	if (!flag) {
		showHide = 'hide';
	}
	jq('#' + panelID).animate({ height: showHide, opacity: showHide }, delay);
}

function moveAddBookmarkToFavouritePanel() {
	jq('#AddBookmarkPanelToMove').appendTo('#AddBookmarkPanel');
}

function getBookmarkByUrlButtonClick() {
	var url = getBookmarkUrlInput().val();
	emptyUrlAlert(false);
	if ('' == url || 'http://' == url || 'https://' == url) {
		emptyUrlAlert(true);
		return;
	}
	getBookmarkUrlInput().attr('disabled', true);
	moveAddBookmarkToFavouritePanel();
	getBookmarkByUrl();
}

function showHideSaveBookmarkButton(flag) {
	if (flag) {
		jq('#SaveBookmarkButton').show();
		jq('#SaveBookmarkButtonCopy').hide();
	} else {
		jq('#SaveBookmarkButton').hide();
		jq('#SaveBookmarkButtonCopy').show();
	}
}

function navigateToBookmarkUrl(url) {
	document.location.href = url;
}

function cancelCreateNewBookmarkButtonClick() {
	cancelButtonClick();
	showAddBookmarkPanel();
}

function generateAllThumbnails(overrideFlag) {
	BookmarkPage.GenerateAllThumbnails(overrideFlag, function(res) { });
}

function updateBookmarkThumbnail() {
	var bookmarkID = jq('#SelectedBookmarkID').val();	
	BookmarkPage.UpdateBookmarkThumbnail(bookmarkID, function(res) { });
}
//---------------------------------------------------------------------
//---------Subscruptions START
//---------------------------------------------------------------------
function subscribeOnRecentBookmarks() {
	BookmarkPage.SubscribeOnRecentBookmarks(
											function(result) {
												showHideSubscribeLink(true,
												"subscribeOnRecentBookmarks",
												"unSubscribeOnRecentBookmarks");
												
											}
				);
}

function unSubscribeOnRecentBookmarks() {
	BookmarkPage.UnSubscribeOnRecentBookmarks(
											function(result) {
												showHideSubscribeLink(false,
												"subscribeOnRecentBookmarks",
												"unSubscribeOnRecentBookmarks");
											}
				 );
}

function subscribeOnBookmarkComments() {
	AjaxPro.onLoading = function(b) {
	    if (b) LoadingBanner.displayLoading();
	    else LoadingBanner.hideLoading();
	}
	BookmarkPage.SubscribeOnBookmarkComments(
											function(result) {
												showHideSubscribeLink(true,
												"subscribeOnBookmarkComments",
												"unSubscribeOnBookmarkComments");
											}
				 );
}

function unSubscribeOnBookmarkComments() {
	AjaxPro.onLoading = function(b) {
	    if (b) LoadingBanner.displayLoading();
	    else LoadingBanner.hideLoading();
	}
	BookmarkPage.UnSubscribeOnBookmarkComments(
											function(result) {
												showHideSubscribeLink(false,
												"subscribeOnBookmarkComments",
												"unSubscribeOnBookmarkComments");
											}
				 );
}

function showHideSubscribeLink(flag, subscribeLinkID, unsubscribeLinkID) {
    if (flag) {
        jq('#' + subscribeLinkID).hide();
        jq('#' + unsubscribeLinkID).show();
        jq('#statusSubscribe').show();
    } else {
        jq('#' + subscribeLinkID).show();
        jq('#' + unsubscribeLinkID).hide();
    }
}
//---------------------------------------------------------------------
//---------Subscriptions. End.
//---------------------------------------------------------------------


//---------------------------------------------------------------------
//---------Retrive bookmark fields.
//---------------------------------------------------------------------
function getBookmarkUrlInput() {
	return jq('#BookmarkUrl');
}

function getBookmarkNameInput() {
	return jq('#BookmarkName');
}

function getBookmarkDescriptionInput() {
	return jq('#BookmarkDescription');
}

function getBookmarkTagsInput() {
	return jq('#BookmarkTagsInput');
}
//---------------------------------------------------------------------
//---------Retrive bookmark fields. End.
//---------------------------------------------------------------------


function bookmarkInputUrlOnKeyDown(event) {
	//Enter key was pressed
	if (event.keyCode == 13) {
		//getBookmarkByUrlButtonClick();
		jq('#CheckBookmarkUrlLinkButton').click();
		return false;
	}
	return true;
};

function createBookmarkOnCtrlEnterKeyDown(event, textArea) {
	if (isEscapeKeyPressed(event)) {
		hideAddBookmarkPanelWithAnimation();
		return false;
	}
	//Enter key was pressed
	if (isCtrlEnterKeyPressed(event)) {
		createBookmarkActionButtonClick();
		return false;
	}

	//Enter key was pressed
	if (!textArea && event.keyCode == 13) {
		createBookmarkActionButtonClick();
		return false;
	}
	return true;
};


function setFocusOnFeild(fieldID) {
	setTimeout('jq("#' + fieldID + '").focus();', 100);
}

function isCtrlEnterKeyPressed(event) {
	//Ctrl + Enter was pressed
	if (event.ctrlKey && event.keyCode == 13) {
		return true;
	}
	return false;
}

function isEscapeKeyPressed(event) {
	//Escape key was pressed
	if (event.keyCode == 27) {
		return true;
	}
	return false;
}

function createBookmarkActionButtonClick() {
	jq('#SaveBookmarkButton').click();
	jq('#SaveBookmarkButtonCopy').click();
	eval(jq('#SaveBookmarkButtonCopy').attr('href'));
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

function changeBookmarksCountOfRows(val)
{
    var page = getURLParam("p");
    var search = getURLParam("search");
    var href = window.location.href.split("?")[0]+"?";
    href += "&size=" + val;
    if(page!=null)
        href += "&p=1";
    if(search!=null)
        href += "&search=" + search;
    window.location.href = href;
}

jq(document).ready(function() {

    if(typeof (BookmarkPage)!="undefined")
    {
        if(BookmarkPage.IsSubscribedOnBookmarkComments().value){
            jq('#subscribeOnBookmarkComments').hide();
            jq('#unSubscribeOnBookmarkComments').show();
        }
        else{
            jq('#subscribeOnBookmarkComments').show();
            jq('#unSubscribeOnBookmarkComments').hide();
        }
        
        var anchor = ASC.Controls.AnchorController.getAnchor();
        if (anchor == "addcomment" && CommentsManagerObj) {
            ckeditorConnector.load(CommentsManagerObj.AddNewComment);
        }
    }
    var textInput = jq("#BookmarkUrl");
    if (textInput.length)
        textInput.focus();

    var $actionButton = jq('.bookmarksHeaderBlock .menu-small');
    jq.dropdownToggle({
        switcherSelector: '.bookmarksHeaderBlock .menu-small',
        dropdownID: 'bookmarkActions',
        addTop: 0,
        addLeft: -11,
        showFunction: function (switcherObj, dropdownItem) {
            if (dropdownItem.is(':hidden')) {
                switcherObj.addClass('active');
            } else {
                switcherObj.removeClass('active');
            }
        },
        hideFunction: function () {
            $actionButton.removeClass('active');
        }
    });
});

function createSearchHelper() {
    var ForumTagSearchHelper = new SearchHelper(
		'BookmarkTagsInput',
		'tagAutocompleteItem',
		'tagAutocompleteSelectedItem',
		'',
		'',
		'BookmarkPage',
		'GetSuggest',
		'',
		true,
		false
	);
}