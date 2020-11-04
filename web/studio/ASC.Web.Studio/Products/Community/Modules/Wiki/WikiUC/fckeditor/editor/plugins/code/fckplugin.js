

var CodeCommand = function( )
{
	this.Name = 'Code';
	var oWindow ;

	this.aLanguages = [
		{lang:'c', icon:'c_28.png', caption: '' + FCKLang.Code_C || "C Lang"},
		{lang:'cpp', icon:'cpp_28.png', caption: '' + FCKLang.Code_CPP || "Cpp Lang"},
		{lang:'cs', icon:'cs_28.png', caption: '' + FCKLang.Code_CS || "CSharp Lang"},
		{lang:'html', icon:'html_28.png', caption: '' + FCKLang.Code_HTML || "HTML Script"},
		{lang:'xml', icon:'xml_28.png', caption: '' + FCKLang.Code_XML || "XML Script"},
		{lang:'asp', icon:'asp_28.png', caption: '' + FCKLang.Code_ASP || "Asp Script"},
		{lang:'js', icon:'js_28.png', caption: '' + FCKLang.Code_JS || "Java Script"},
		{lang:'tsql', icon:'sql_28.png', caption: '' + FCKLang.Code_TSQL || "Sql Script"},
		{lang:'vb', icon:'vb_28.png', caption: '' + FCKLang.Code_VB || "VB Lang"}
	];
	
	
	if ( FCKBrowserInfo.IsIE )
		oWindow = window ;
	else if ( FCKToolbarSet._IFrame )
		oWindow = FCKTools.GetElementWindow( FCKToolbarSet._IFrame ) ;
	else
		oWindow = window.parent ;

	this._Panel = new FCKPanel( oWindow ) ;
	this._Panel.AppendStyleSheet( FCKConfig.SkinEditorCSS ) ;
	this._Panel.MainNode.className = 'FCK_Panel' ;
	this._CreatePanelBody( this._Panel.Document, this._Panel.MainNode ) ;
	FCKToolbarItems.GetItem( this.Name ).RegisterPanel( this._Panel ) ;

	FCKTools.DisableSelection( this._Panel.Document.body ) ;
	
	
	
}


CodeCommand.prototype.Execute = function( panelX, panelY, relElement )
{
	// Show the Code Panel at the desired position.
	this._Panel.Show( panelX, panelY, relElement ) ;
	var lang = this.GetSelectedLang();
	var elId = null;
	
	elId = 'code_nocode';
	var el = this._Panel.Document.getElementById(elId);
	
	if(el != null)
	{
		if(lang == '')
		{
			el.className +=' CodeUsed';
		}
		else
		{
			el.className = el.className.replace(/[\s]?codeused/gi, '');
		}
	}
	for(var i = 0; i < this.aLanguages.length; i++)
	{
		elId = 'code_' + this.aLanguages[i].lang;
		el = this._Panel.Document.getElementById(elId);
	
		if(el != null)
		{
			if(lang == this.aLanguages[i].lang)
			{
				el.className +=' CodeUsed';
			}
			else
			{
				el.className = el.className.replace(/[\s]?codeused/gi, '');
			}
		}
	}
		
}

CodeCommand.prototype.ClearCodeArea = function()
{
	var oDiv = this.GetSelectedLangDiv();
	
	if(oDiv && oDiv.getAttribute)
	{
		FCKTempBin.RemoveElement( oDiv.getAttribute( '_fckrealelement' ) ) ;
		FCKTools.RemoveOuterTags(oDiv);
	}
}

CodeCommand.prototype.UpdateCodeLang = function(lang)
{
	var oDiv = this.GetSelectedLangDiv();
	var oRealCode = null;
	if(oDiv)
	{
		oRealCode = FCK.GetRealElement( oDiv ) ;
		oRealCode.setAttribute('lang', lang, 0 );
	}
	else
	{
		oRealCode = FCK.EditorDocument.createElement( 'Code' ) ;
		oRealCode.setAttribute( 'lang', lang, 0 ) ;
		oRealCode.innerHTML = this.GetSelected();
		
		oDiv = FCKTools.GetElementDocument( oRealCode ).createElement( 'DIV' ) ;
		oDiv.setAttribute( '_fckfakelement', 'true', 0 ) ;
		oDiv.setAttribute( '_fckrealelement', FCKTempBin.AddElement( oRealCode ), 0 ) ;
		
		oDiv = FCK.InsertElement( oDiv ) ;
	}
	
	oDiv.setAttribute( '_codeBlock', lang, 0 ) ;
	oDiv.className = 'codeFake cf_' + lang;
	oDiv.innerHTML = _encode(oRealCode.innerHTML);
	
	FCKEmbedAndObjectProcessor.RefreshView( oDiv, oRealCode ) ;
}

CodeCommand.prototype.SetCodeLang = function( language )
{
	FCKUndo.SaveUndoStep() ;
	if(language == '')
	{
		this.ClearCodeArea();
	}
	else
	{
		this.UpdateCodeLang(language);
	}
	//Magic happens here.
	FCKUndo.SaveUndoStep() ;

	FCK.Focus() ;
	FCK.Events.FireEvent( 'OnSelectionChange' ) ;
	FCK.ToolbarSet.RefreshItemsState();
}

CodeCommand.prototype.GetSelected = function()
{
	var selection = '';

	FCKSelection.GetSelection();
	
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
	}
	
	return selection;
}

CodeCommand.prototype.GetSelectedLangDiv = function()
{
	var oDiv = FCK.Selection.MoveToAncestorNode( 'DIV' );
	while(oDiv != null && oDiv.getAttribute)
	{
		if(oDiv.getAttribute('_codeBlock'))
			return oDiv;
	
		oDiv = oDiv.parentNode;
	}
	return null;
}

CodeCommand.prototype.GetSelectedLang = function()
{
	var oDiv = this.GetSelectedLangDiv();
	if(oDiv == null || !oDiv.getAttribute)
		return '';
		
	return oDiv.getAttribute('_codeBlock');
}


CodeCommand.prototype.GetState = function()
{
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
		return FCK_TRISTATE_DISABLED ;
	if(this.GetSelectedLang() != '')
		return FCK_TRISTATE_ON;
	return FCK_TRISTATE_OFF;
}

function CodeCommand_OnMouseOver()
{
	this.className = this.className.replace(/codedeselected/gi, 'CodeSelected');
}

function CodeCommand_OnMouseOut()
{
	this.className = this.className.replace(/codeselected/gi, 'CodeDeselected');
}

function CodeCommand_OnClick( ev, command, lang )
{
	this.className = 'CodeDeselected' ;
	command.SetCodeLang( lang ) ;
	command._Panel.Hide() ;
}

function CodeCommand_NoCodeOnClick( ev, command )
{
	this.className = 'CodeDeselected' ;
	command.SetCodeLang( '' ) ;
	command._Panel.Hide() ;
}



CodeCommand.prototype._CreatePanelBody = function( targetDocument, targetDiv )
{
	function CreateSelectionDiv(id)
	{
		var oDiv = targetDocument.createElement( "DIV" ) ;
		if(id)
		{
			oDiv.id = id;
		}
		oDiv.className = 'CodeDeselected' ;
		FCKTools.AddEventListenerEx( oDiv, 'mouseover', CodeCommand_OnMouseOver ) ;
		FCKTools.AddEventListenerEx( oDiv, 'mouseout', CodeCommand_OnMouseOut ) ;

		return oDiv ;
	}

	
	var oTable = targetDiv.appendChild( targetDocument.createElement( "TABLE" ) ) ;
	oTable.className = 'ForceBaseFont' ;		// Firefox 1.5 Bug.
	oTable.style.tableLayout = 'fixed' ;
	oTable.cellPadding = 0 ;
	oTable.cellSpacing = 0 ;
	oTable.border = 0 ;
	oTable.style.width = '100px' ;

	var oCell = oTable.insertRow(-1).insertCell(-1) ;
	oCell.colSpan = 3 ;

	
	var oDiv = oCell.appendChild( CreateSelectionDiv('code_nocode') ) ;
	oDiv.innerHTML =
		'<table cellspacing="0" cellpadding="0" width="100%" border="0">\
			<tr>\
				<td nowrap width="100%" align="center">' + FCKLang.CodeNoCode + '</td>\
			</tr>\
		</table>' ;
	oDiv.style.paddingRight = "2px";
	FCKTools.AddEventListenerEx( oDiv, 'click', CodeCommand_NoCodeOnClick, this ) ;

	// Dirty hack for Opera, Safari and Firefox 3.
	if ( !FCKBrowserInfo.IsIE )
		oDiv.style.width = '96%' ;

	// Create an array of codes based on the configuration file.
	

	// Create the codes table based on the array.
	var iCounter = 0 ;
	while ( iCounter < this.aLanguages.length )
	{
		var oRow = oTable.insertRow(-1) ;

		for ( var i = 0 ; i < 3 ; i++, iCounter++ )
		{
			// The div will be created even if no more codes are available.
			if ( iCounter < this.aLanguages.length )
			{
				var langName = this.aLanguages[iCounter].lang;
				var langIcon = this.aLanguages[iCounter].icon ;
				var langCaption = this.aLanguages[iCounter].caption ;
			}

			oDiv = oRow.insertCell(-1).appendChild( CreateSelectionDiv('code_' + langName) ) ;
			oDiv.innerHTML = '<div class="CodeBoxBorder"><div class="CodeBox" title="' + langCaption + '" style="background-image: url( ' + FCKPlugins.Items['code'].Path + 'icons/' + langIcon + ');"></div></div>' ;

			if ( iCounter >= this.aLanguages.length )
				oDiv.style.visibility = 'hidden' ;
			else
				FCKTools.AddEventListenerEx( oDiv, 'click', CodeCommand_OnClick, [ this, langName ] ) ;
		}
	}

	// Dirty hack for Opera, Safari and Firefox 3.
	//if ( !FCKBrowserInfo.IsIE )
	//	oDiv.style.width = '96%' ;
}




var oCodeItem = new FCKToolbarPanelButton( 'Code', FCKLang.CodeBtn, null, null, FCKPlugins.Items['code'].Path + 'code.png' ) ;
oCodeItem.ContextSensitive = true;
FCKToolbarItems.RegisterItem( 'Code', oCodeItem ) ;
FCKCommands.RegisterCommand( 'Code',  new CodeCommand()) ;


FCKEmbedAndObjectProcessor.AddCustomHandler( function( el, fakeImg )
{
		if (  !(el.nodeName.IEquals( 'code' ) && el.getAttribute('lang') ))
			return null;
		
		var oDiv = FCKTools.GetElementDocument( el ).createElement( 'DIV' ) ;
		oDiv.setAttribute( '_fckfakelement', 'true', 0 ) ;
		oDiv.setAttribute( '_fckrealelement', FCKTempBin.AddElement( el ), 0 ) ;
		
		oDiv.setAttribute( '_codeBlock', el.getAttribute('lang', 0 )) ;
		oDiv.className = 'codeFake cf_' + el.getAttribute('lang', 0 );
		oDiv.innerHTML = _encode(el.innerHTML);
		return oDiv;
} ) ;

FCK.AddGetRealElementHandle(function( fakeImg, elem )
{
	if(!elem.nodeName.IEquals( 'code' ))
		return;
	elem.innerHTML = _decode(fakeImg.innerHTML);
});

function _encode(s)
{
	//s = s.replace(/&/g, '&amp;');
	s = s.replace(/^[\s]+|[\s]+&/g, ''); //trim
	s = s.replace(/^(<br[\s]*\/?>)+/gi, '');
	s = s.replace(/(<br[\s]*\/?>)+$/gi, '');
	
	
	if(FCKBrowserInfo.IsSafari)
		return s;
	
	
	s = s.replace(/<br[\s]*\/?>/gi, '\n');
	s = s.replace(/</g, '&lt;');
	s = s.replace(/>/g, '&gt;');

	
	if(FCKBrowserInfo.IsIE)
	{
		s = s.replace(/\n/gi, '<br />');
	}
	return s;
}

function _decode(s)
{
	if(FCKBrowserInfo.IsIE)
		s = s.replace(/<br[\s]*\/?>/gi, '\n');
	s = s.replace(/\n/gi, '<br />');
	//if(!FCKBrowserInfo.IsIE)
	s = s.replace(/&nbsp;/gi, ' ');
	if(FCKBrowserInfo.IsSafari)
		return s;
	s = s.replace(/&lt;/gi, '<');
	s = s.replace(/&gt;/gi, '>');
	s = s.replace(/&amp;/gi, '&');
	
	return s;
}





		



