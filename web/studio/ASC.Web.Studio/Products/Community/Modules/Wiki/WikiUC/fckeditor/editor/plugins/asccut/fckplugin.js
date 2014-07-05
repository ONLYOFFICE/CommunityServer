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



var ASCAscCutCommand = function()
{
	this.Name = 'AscCut' ;
}


ASCAscCutCommand.prototype.Execute = function()
{

	FCKUndo.SaveUndoStep();

	var oDiv = ASCAscCutCommand.GetSelectedCut();
	
	if(oDiv)
	{
		FCKTools.RemoveOuterTags(oDiv);
		FCK.ToolbarSet.RefreshItemsState();
		return;
	}
		
	
	var innerHTML;
	
	var selection = '';

	if (FCK.EditorWindow.getSelection) {
		selection = FCK.EditorWindow.getSelection();

		var d = FCK.EditorDocument.createElement('DIV');
		for (var i = 0; i < selection.rangeCount; i++) {
			d.appendChild(selection.getRangeAt(i).cloneContents());
		}
		selection = d.innerHTML;

	} else if (FCK.EditorDocument.selection) {
		var range = FCK.EditorDocument.selection.createRange();

		var type = FCKSelection.GetType();
		if (type == 'Control') {
			selection = range.item(0).outerHTML;
		} else if (type == 'None') {
			selection = '';
		} else {
			selection = range.htmlText;
		}
	}

	if (selection != '') {
		selection += ''; 
	} else {
		selection += FCKLang.AscCutContents;
	}

	var html = "<div class='asccut'>";
	html += selection;
	html += "</div>";
	
	//var html = "<asc-cut>" + selection + "</asc-cut>";

	FCK.InsertHtml(html);
	FCK.Focus();
	
	//FCKEmbedAndObjectProcessor.RefreshView( fake, html ) ;

	FCK.ToolbarSet.RefreshItemsState();
	
	return; 
	
	
}

ASCAscCutCommand.prototype.GetState = function()
{
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
		return FCK_TRISTATE_DISABLED ;
	if(ASCAscCutCommand.GetSelectedCut())
		return FCK_TRISTATE_ON;
	return FCK_TRISTATE_OFF;
	
}

ASCAscCutCommand.GetSelectedCut = function()
{
	var oDiv = FCK.Selection.MoveToAncestorNode( 'DIV' );
	
	while(oDiv != null)
	{
		var className = oDiv.className;
		if(className != null)
		{	
			if(className.toLowerCase() == "asccut")
			{
				return oDiv;
			}
		}
		
		oDiv = oDiv.parentNode;
	}
	
	return null;
}


var oAscCut = new FCKToolbarButton( 'AscCut', FCKLang.AscCut, null, null, false, true);
oAscCut.IconPath = FCKPlugins.Items['asccut'].Path + 'AscCut.png' ;

FCKToolbarItems.RegisterItem( 'AscCut'	,  oAscCut) ;

FCKCommands.RegisterCommand( 'AscCut', new ASCAscCutCommand() ) ;




