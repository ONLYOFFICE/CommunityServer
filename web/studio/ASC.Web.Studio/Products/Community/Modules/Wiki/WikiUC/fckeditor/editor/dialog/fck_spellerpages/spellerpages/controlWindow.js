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

////////////////////////////////////////////////////
// controlWindow object
////////////////////////////////////////////////////
function controlWindow( controlForm ) {
	// private properties
	this._form = controlForm;

	// public properties
	this.windowType = "controlWindow";
//	this.noSuggestionSelection = "- No suggestions -";	// by FredCK
	this.noSuggestionSelection = FCKLang.DlgSpellNoSuggestions ;
	// set up the properties for elements of the given control form
	this.suggestionList  = this._form.sugg;
	this.evaluatedText   = this._form.misword;
	this.replacementText = this._form.txtsugg;
	this.undoButton      = this._form.btnUndo;

	// public methods
	this.addSuggestion = addSuggestion;
	this.clearSuggestions = clearSuggestions;
	this.selectDefaultSuggestion = selectDefaultSuggestion;
	this.resetForm = resetForm;
	this.setSuggestedText = setSuggestedText;
	this.enableUndo = enableUndo;
	this.disableUndo = disableUndo;
}

function resetForm() {
	if( this._form ) {
		this._form.reset();
	}
}

function setSuggestedText() {
	var slct = this.suggestionList;
	var txt = this.replacementText;
	var str = "";
	if( (slct.options[0].text) && slct.options[0].text != this.noSuggestionSelection ) {
		str = slct.options[slct.selectedIndex].text;
	}
	txt.value = str;
}

function selectDefaultSuggestion() {
	var slct = this.suggestionList;
	var txt = this.replacementText;
	if( slct.options.length == 0 ) {
		this.addSuggestion( this.noSuggestionSelection );
	} else {
		slct.options[0].selected = true;
	}
	this.setSuggestedText();
}

function addSuggestion( sugg_text ) {
	var slct = this.suggestionList;
	if( sugg_text ) {
		var i = slct.options.length;
		var newOption = new Option( sugg_text, 'sugg_text'+i );
		slct.options[i] = newOption;
	 }
}

function clearSuggestions() {
	var slct = this.suggestionList;
	for( var j = slct.length - 1; j > -1; j-- ) {
		if( slct.options[j] ) {
			slct.options[j] = null;
		}
	}
}

function enableUndo() {
	if( this.undoButton ) {
		if( this.undoButton.disabled == true ) {
			this.undoButton.disabled = false;
		}
	}
}

function disableUndo() {
	if( this.undoButton ) {
		if( this.undoButton.disabled == false ) {
			this.undoButton.disabled = true;
		}
	}
}
