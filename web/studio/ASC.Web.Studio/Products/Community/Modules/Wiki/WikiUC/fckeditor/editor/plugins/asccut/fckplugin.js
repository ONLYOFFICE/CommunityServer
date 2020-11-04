

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




