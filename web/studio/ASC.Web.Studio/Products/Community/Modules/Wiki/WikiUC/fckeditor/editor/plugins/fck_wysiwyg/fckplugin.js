/*
 * FCKeditor - The text editor for Internet - http://www.fckeditor.net
 * Copyright (C) 2003-2007 Frederico Caldeira Knabben
 *
 * == BEGIN LICENSE ==
 *
 * Licensed under the terms of any of the following licenses at your
 * choice:
 *
 *  - GNU General Public License Version 2 or later (the "GPL")
 *    http://www.gnu.org/licenses/gpl.html
 *
 *  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
 *    http://www.gnu.org/licenses/lgpl.html
 *
 *  - Mozilla Public License Version 1.1 or later (the "MPL")
 *    http://www.mozilla.org/MPL/MPL-1.1.html
 *
 * == END LICENSE ==
 *
 * Main MediaWiki integration plugin.
 *
 * Wikitext syntax reference:
 *	http://meta.wikimedia.org/wiki/Help:Wikitext_examples
 *	http://meta.wikimedia.org/wiki/Help:Advanced_editing
 *
 * MediaWiki Sandbox:
 *	http://meta.wikimedia.org/wiki/Meta:Sandbox
 */

// Rename the "Source" buttom to "Wikitext".
FCKToolbarItems.RegisterItem('Source', new FCKToolbarButton('Source', FCKLang.Wikitext, null, FCK_TOOLBARITEM_ONLYICON, true, true, FCKPlugins.Items[window.parent.WikiPlugName].Path + "wikiCommand.png"));

var wikiSourceCommand = FCKCommands.GetCommand('Source');
function CheckWikiTextButtonToolBar(elem)
{
	var divs = elem.getElementsByTagName('DIV');
	for(var i = 0; i < divs.length; i++)
	{
		if(divs[i].title == FCKLang.Wikitext)
		{
			return true;
		}
	}
	return false;
}

wikiSourceCommand.UpdateToolBar = function(extCall) {
    var _editMode = FCK.EditMode;
    if (extCall) {
        _editMode = _editMode == 1 ? 0 : 1;
    }
    var toolBar = document.getElementById('xToolbar');
    if (toolBar == null)
        return;
    var taWikiToolBarExists = false;

    for (var i = 0; i < toolBar.childNodes.length; i++) {
        elem = toolBar.childNodes[i];
        if (elem.nodeName.toUpperCase() == 'TABLE' || (elem.nodeName.toUpperCase() == 'DIV' && elem.className == 'TB_Break')) {
            if (!CheckWikiTextButtonToolBar(elem)) {
                elem.style.display = _editMode != FCK_EDITMODE_WYSIWYG ? '' : 'none';

            }
        }
        else if (elem.nodeName.toUpperCase() == 'DIV' && elem.id == 'wikiToolBar') {
            elem.style.display = _editMode != FCK_EDITMODE_WYSIWYG ? 'none' : '';
            taWikiToolBarExists = true;
        }
    }

    if (!taWikiToolBarExists) //Add custom toolbar!
    {
        var taWikiToolBar = document.createElement('DIV');
        taWikiToolBar.className = 'wiki';
        taWikiToolBar.style.display = _editMode != FCK_EDITMODE_WYSIWYG ? 'none' : '';
        taWikiToolBar.style.width = '100%';
        taWikiToolBar.style.backgroundColor = 'White';
        taWikiToolBar.style.paddingTop = '2px';
        taWikiToolBar.id = 'wikiToolBar';
        taWikiToolBar.innerHTML = window.parent.document.getElementById('taWikiTools').innerHTML;
        toolBar.appendChild(taWikiToolBar);
    }

    return !_editMode;
}

wikiSourceCommand.Execute = function()
{
	
	var _editMode = this.UpdateToolBar();
	//var _editMode;
	
	if ( FCKConfig.SourcePopup )	// Until v2.2, it was mandatory for FCKBrowserInfo.IsGecko.
	{
		var iWidth	= FCKConfig.ScreenWidth * 0.65 ;
		var iHeight	= FCKConfig.ScreenHeight * 0.65 ;
		FCKDialog.OpenDialog( 'FCKDialog_Source', FCKLang.Source, 'dialog/fck_source.html', iWidth, iHeight, null, null, true ) ;
	}
	else
	{
	    FCK.SwitchEditMode() ;
	}
	
	if(typeof(FCK.OnSwitchedEditMode) == 'function')
	{
		FCK.OnSwitchedEditMode(_editMode != FCK_EDITMODE_WYSIWYG);
	}
};









// Register our toolbar buttons.
/*var tbButton = new FCKToolbarButton( 'MW_Template', 'Template',  FCKLang.wikiBtnTemplate || 'Insert/Edit Template') ;
tbButton.IconPath = FCKConfig.PluginsPath + 'mediawiki/images/tb_icon_template.gif' ;
FCKToolbarItems.RegisterItem( 'MW_Template', tbButton ) ;*/

//Ref button
/*tbButton = new FCKToolbarButton( 'MW_Ref', 'Ref', FCKLang.wikiBtnReference || 'Insert/Edit Reference' ) ;
tbButton.IconPath = FCKConfig.PluginsPath + 'mediawiki/images/tb_icon_ref.gif' ;
FCKToolbarItems.RegisterItem( 'MW_Ref', tbButton ) ;
if ( !FCKConfig.showreferences ) {		//hack to disable MW_Ref  button
	tbButton.Create = function()		{return 0;}
	tbButton.Disable  = function()	{return 0;}
	tbButton.RefreshState  = function()	{return 0;}
}
*/
//References button
/*var FCKReferences = function( )	{	} ;
FCKReferences.prototype.GetState = function()	{ return ( FCK.EditMode == FCK_EDITMODE_WYSIWYG ? FCK_TRISTATE_OFF : FCK_TRISTATE_DISABLED) } ;
FCKCommands.RegisterCommand( 'MW_References', new FCKReferences() ) ;
tbButton = new FCKToolbarButton( 'MW_References', 'References', FCKLang.wikiBtnReferences || 'Insert <references /> tag', FCK_TOOLBARITEM_ICONTEXT,true, true, 1  );
tbButton.IconPath = FCKConfig.PluginsPath + 'mediawiki/images/tb_icon_ref.gif' ;
if ( !FCKConfig.showreferences ) {		//hack to disable MW_References  button
	tbButton.Create = function()		{return 0;}
	tbButton.Disable  = function()	{return 0;}
	tbButton.RefreshState  = function()	{return 0;}
}

FCKReferences.prototype.Execute = function()
{
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
		return ;

	FCKUndo.SaveUndoStep() ;

	var e = FCK.EditorDocument.createElement( 'span' ) ;
	e.setAttribute("_fck_mw_customtag", "true");
	e.setAttribute("_fck_mw_tagname", "references");
	e.className = "fck_mw_references";

	oFakeImage = FCK.InsertElement( FCKDocumentProcessor_CreateFakeImage( 'FCK__MWReferences', e ) ) ;
}
FCKToolbarItems.RegisterItem( 'MW_References', tbButton ) ;
*/
//Signature button
/*var FCKSignature = function( )	{} ;
FCKSignature.prototype.GetState = function()	{ return ( FCK.EditMode == FCK_EDITMODE_WYSIWYG ? FCK_TRISTATE_OFF : FCK_TRISTATE_DISABLED) } ;
FCKCommands.RegisterCommand( 'MW_Signature', new FCKSignature() ) ;
tbButton = new FCKToolbarButton( 'MW_Signature', 'Signature', FCKLang.wikiBtnSignature || 'Insert signature', FCK_TOOLBARITEM_ONLYICON,true, true, 1  );
tbButton.IconPath = FCKConfig.PluginsPath + 'mediawiki/images/tb_signature.gif' ;

FCKSignature.prototype.Execute = function()
{
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
		return ;

	FCKUndo.SaveUndoStep() ;
	var e = FCK.EditorDocument.createElement( 'span' ) ;
	e.className = "fck_mw_signature";

	oFakeImage = FCK.InsertElement( FCKDocumentProcessor_CreateFakeImage( 'FCK__MWSignature', e ) ) ;
}
FCKToolbarItems.RegisterItem( 'MW_Signature', tbButton ) ;

tbButton = new FCKToolbarButton( 'MW_Math', 'Formula', FCKLang.wikiBtnFormula || 'Insert/Edit Formula' ) ;
tbButton.IconPath = FCKConfig.PluginsPath + 'mediawiki/images/tb_icon_math.gif' ;
FCKToolbarItems.RegisterItem( 'MW_Math', tbButton ) ;

tbButton = new FCKToolbarButton( 'MW_Source', 'Source', FCKLang.wikiBtnSourceCode || 'Insert/Edit Source Code' ) ;
tbButton.IconPath = FCKConfig.PluginsPath + 'mediawiki/images/tb_icon_source.gif' ;
FCKToolbarItems.RegisterItem( 'MW_Source', tbButton ) ;
if ( !FCKConfig.showsource ) {	//hack to disable MW_Source  button
	tbButton.Create = function()		{return 0;}
	tbButton.Disable  = function()	{return 0;}
	tbButton.RefreshState  = function()	{return 0;}
}

tbButton = new FCKToolbarButton( 'MW_Special', 'Special Tag', FCKLang.wikiBtnSpecial || 'Insert/Edit Special Tag' ) ;
tbButton.IconPath = FCKConfig.PluginsPath + 'mediawiki/images/tb_icon_special.gif' ;
FCKToolbarItems.RegisterItem( 'MW_Special', tbButton ) ;

tbButton = new FCKToolbarButton( 'MW_Category', 'Categories', FCKLang.wikiBtnCategories || 'Insert/Edit categories' ) ;
tbButton.IconPath = FCKConfig.PluginsPath + 'mediawiki/images/tb_icon_category.gif' ;
FCKToolbarItems.RegisterItem( 'MW_Category', tbButton ) ;
*/
// Override some dialogs.
//FCKCommands.RegisterCommand( 'MW_Template', new FCKDialogCommand( 'MW_Template', ( FCKLang.wikiCmdTemplate || 'Template Properties' ), FCKConfig.PluginsPath + 'mediawiki/dialogs/template.html', 400, 330 ) ) ;
//FCKCommands.RegisterCommand( 'MW_Ref', new FCKDialogCommand( 'MW_Ref', ( FCKLang.wikiCmdReference || 'Reference Properties' ), FCKConfig.PluginsPath + 'mediawiki/dialogs/ref.html', 400, 250 ) ) ;
//FCKCommands.RegisterCommand( 'MW_Math', new FCKDialogCommand( 'MW_Math', ( FCKLang.wikiCmdFormula || 'Formula' ), FCKConfig.PluginsPath + 'mediawiki/dialogs/math.html', 400, 300 ) ) ;
//FCKCommands.RegisterCommand( 'MW_Special', new FCKDialogCommand( 'MW_Special', ( FCKLang.wikiCmdSpecial || 'Special Tag Properties' ), FCKConfig.PluginsPath + 'mediawiki/dialogs/special.html', 400, 330 ) ) ;
//FCKCommands.RegisterCommand( 'MW_Source', new FCKDialogCommand( 'MW_Source', ( FCKLang.wikiCmdSourceCode || 'Source Code Properties' ), FCKConfig.PluginsPath + 'mediawiki/dialogs/source.html', 720, 380 ) ) ;

var wikiTocItem = new FCKToolbarButton('WikiTOC', FCKLang.wikiTOC || 'TOC') ;
wikiTocItem.ContextSensitive = true;
wikiTocItem.IconPath = FCKPlugins.Items[window.parent.WikiPlugName].Path + 'wikiContents.png' ;
FCKToolbarItems.RegisterItem( 'WikiTOC', wikiTocItem );


var wikiTocCommand = function()
{
	this.Name = 'WikiTOC' ;
}


wikiTocCommand.prototype.GetState = function()
{
	if ( ! FCK.EditorWindow  || FCK.EditMode != FCK_EDITMODE_WYSIWYG )
			return FCK_TRISTATE_DISABLED ;
	if(this.GetSelectedTOC())
		return FCK_TRISTATE_ON ;	
	return FCK_TRISTATE_OFF ;	
}

wikiTocCommand.prototype.Execute = function()
{
	FCKUndo.SaveUndoStep();

	var oTOC = this.GetSelectedTOC();
	
	if(oTOC)
	{
		oTOC.parentNode.removeChild(oTOC);
		//FCKTools.RemoveOuterTags(oTOC);
		FCK.ToolbarSet.RefreshItemsState();
		return;
	}
	
	oTOC = FCK.EditorDocument.createElement( 'Div' ) ;
	//var html = "<div class='wikiTOC' _wikiTOC=''>";
	oTOC.setAttribute('_wikiTOC', '', 0);
	//oTOC.setAttribute('class', 'wikiTOC', 0);
	oTOC.innerHTML += FCKLang.wikiTOCHereContent || 'TOC Here';
	oTOC.className = 'wikiTOC';
	oTOC.contentEditable = false;
	//var oCaption = FCK.EditorDocument.createElement('Div')
	//oCaption.innerHTML += FCKLang.wikiTOCHereContent || 'TOC Here';
	//oCaption.setAttribute('_fckplaceholder', true);
	
	//oTOC.appendChild(oCaption);
	//oTOC += "</div>";
	
	//FCK.InsertHtml(html);
	FCK.InsertElement( oTOC ) ;
	FCK.Focus();
	
	FCK.ToolbarSet.RefreshItemsState();
	return; 
}

wikiTocCommand.prototype.GetSelectedTOC = function()
{
	var oToc = FCK.Selection.MoveToAncestorNode( 'DIV' );
	
	var wikiTOCAttr;
	while(oToc != null && oToc.getAttribute)
	{
		wikiTOCAttr = oToc.getAttribute('_wikiTOC');
		if(wikiTOCAttr != null)
		{	
				return oToc;
		}
		
		oToc = oToc.parentNode;
	}
	
	return null;
}

var _wikiToc = new wikiTocCommand();
FCKCommands.RegisterCommand( 'WikiTOC',  _wikiToc) ;



wikiListCommand.prototype = new FCKListCommand(); 
wikiListCommand.prototype.constructor = wikiListCommand; 
wikiListCommand.superclass = FCKListCommand.prototype; 
function wikiListCommand(name, tagName)
{
	this.Name = name ;
	this.TagName = tagName ;
}



wikiListCommand.prototype.GetState = function()
{
// Disabled if not WYSIWYG.
		if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG || ! FCK.EditorWindow )
			return FCK_TRISTATE_DISABLED ;

		// We'll use the style system's convention to determine list state here...
		// If the starting block is a descendant of an <ol> or <ul> node, then we're in a list.
		var startContainer = FCKSelection.GetBoundaryParentElement( true ) ;
		var listNode = startContainer ;
		var resultListItem = null;
		var resultCodeItem = false;
		
		while ( listNode )
		{
			if ( listNode.nodeName.IEquals( [ 'ul', 'ol' ] ) )
			{
				resultListItem = listNode;
			}
			
			if ( listNode.nodeName.toUpperCase()=='DIV' && listNode.getAttribute( '_wikiCodeStyle') != null)
			{
				resultCodeItem = true;
				break;
			}
			listNode = listNode.parentNode ;
		}
		
		if(resultCodeItem)
		{
			return FCK_TRISTATE_DISABLED ; 
		}
		
		if ( resultListItem && resultListItem.nodeName.IEquals( this.TagName ))
			return FCK_TRISTATE_ON ;
		else
			return FCK_TRISTATE_OFF ;
}

var wikiOrderedListCommand = new wikiListCommand( 'insertorderedlist', 'ol' );
var wikiUnorderedListCommand = new wikiListCommand( 'insertunorderedlist', 'ul' );


FCKCommands.RegisterCommand( 'InsertOrderedList',  wikiOrderedListCommand) ;
FCKCommands.RegisterCommand( 'InsertUnorderedList',  wikiUnorderedListCommand) ;

var newLinkCommand = new FCKDialogCommand( 'Link', FCKLang.DlgLnkWindowTitle, FCKPlugins.Items[window.parent.WikiPlugName].Path + 'dialogs/link.html', 500, 217 );

newLinkCommand.GetState = function()
{
	if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
		return FCK_TRISTATE_DISABLED ;
		
	var oLink;
	try
	{
		oLink = FCK.Selection.MoveToAncestorNode( 'A' ) ;
	}
	catch(e)
	{
		oLink = null;
	}
	if(oLink != null)
	{
		if(oLink.getAttribute( '_wikiIgnore' ) != null)
		{
			return FCK_TRISTATE_DISABLED;
		}
	}
	return  FCK_TRISTATE_OFF ;
}
FCKCommands.RegisterCommand( 'Link',  newLinkCommand) ;

var newUnLinkCommand = FCKCommands.GetCommand('Unlink');

if(newUnLinkCommand != null)
{
	newUnLinkCommand.GetState = function()
	{
		if ( FCK.EditMode != FCK_EDITMODE_WYSIWYG )
			return FCK_TRISTATE_DISABLED ;
			
		var oLink;
		try
		{
			oLink = FCK.Selection.MoveToAncestorNode( 'A' ) ;
		}
		catch(e)
		{
			oLink = null;
		}
		
		if(oLink != null)
		{
			if(oLink.getAttribute( '_wikiIgnore' ) != null)
			{
				return FCK_TRISTATE_DISABLED;
			}
		}
		return FCK.GetNamedCommandState( this.Name ) ;
	}
}
 
//FCKCommands.RegisterCommand( 'MW_Category', new FCKDialogCommand( 'MW_Category', ( FCKLang.wikiCmdCategories || 'Categories' ), FCKConfig.PluginsPath + 'mediawiki/dialogs/category.html', 400, 500 ) ) ;
FCKCommands.RegisterCommand( 'Image', new FCKDialogCommand( 'Image', FCKLang.DlgImgTitle, FCKPlugins.Items[window.parent.WikiPlugName].Path + 'dialogs/image.html', 500, 270 ) ) ;
var FileCommand = new FCKDialogCommand( 'File', FCKLang.wikiDlgFileTitle, FCKPlugins.Items[window.parent.WikiPlugName].Path + 'dialogs/file.html', 500, 220 );
FCKCommands.RegisterCommand( 'File',  FileCommand) ;

var FileItem = new FCKToolbarButton( 'File', FCKLang.wikiFile ) ;
FileItem.IconPath = FCKPlugins.Items[window.parent.WikiPlugName].Path + 'dialogs/file.png' ;
FileItem.GetState = function()
{
	return FCK_TRISTATE_OFF;
}
FCKToolbarItems.RegisterItem( 'File', FileItem ) ;



FCKToolbarItems.OldGetItem = FCKToolbarItems.GetItem;

FCKToolbarItems.GetItem = function( itemName )
{
	var oItem = FCKToolbarItems.LoadedItems[ itemName ] ;

	if ( oItem )
		return oItem ;

	switch ( itemName )
	{
		case 'Bold'			: oItem = new FCKToolbarButton( 'Bold'          , FCKLang.Bold, null, null, true, true, 20 ) ; break ;
		case 'Italic'		: oItem = new FCKToolbarButton( 'Italic'        , FCKLang.Italic, null, null, true, true, 21 ) ; break ;
		case 'Underline'	: oItem = new FCKToolbarButton( 'Underline'     , FCKLang.Underline, null, null, true, true, 22 ) ; break ;
		case 'StrikeThrough': oItem = new FCKToolbarButton( 'StrikeThrough' , FCKLang.StrikeThrough, null, null, true, true, 23 ) ; break ;
		case 'Link'			: oItem = new FCKToolbarButton( 'Link'          , FCKLang.InsertLinkLbl, FCKLang.InsertLink, null, true, true, 34 ) ; break ;

		default:
			return FCKToolbarItems.OldGetItem( itemName );
	}

	FCKToolbarItems.LoadedItems[ itemName ] = oItem ;

	return oItem ;
}

FCKToolbarButton.prototype.Click = function()
{
	var oToolbarButton = this._ToolbarButton || this ;

	// for some buttons, do something else instead...
	var CMode = false ;
	if ( oToolbarButton.SourceView && (FCK_EDITMODE_SOURCE == FCK.EditMode) )
	{
		if ( !window.parent.popup )
			var oDoc = window.parent;
		else
			var oDoc = window.parent.popup;

		switch (oToolbarButton.CommandName)
		{
			case 'Bold' 		: oDoc.FCKeditorInsertTags ('\'\'\'', '\'\'\'', 'Bold text', document) ; CMode = true ; break ;
			case 'Italic' 		: oDoc.FCKeditorInsertTags ('\'\'', '\'\'', 'Italic text', document) ; CMode = true ; break ;
			case 'Underline' 	: oDoc.FCKeditorInsertTags ('__', '__', 'Underlined text', document) ; CMode = true ; break ;
			case 'StrikeThrough': oDoc.FCKeditorInsertTags ('--', '--', 'Strikethrough text', document) ; CMode = true ; break ;
			case 'Link' 		: oDoc.FCKeditorInsertTags ('[[', ']]', 'Internal link', document) ; CMode = true ; break ;
		}
	}

	if ( !CMode )
		FCK.ToolbarSet.CurrentInstance.Commands.GetCommand( oToolbarButton.CommandName ).Execute() ;
}


// MediaWiki Wikitext Data Processor implementation.
FCK.DataProcessor =
{
	_inPre: false,
	_inLSpace: false,

	/*
	* Returns a string representing the HTML format of "data". The returned
	* value will be loaded in the editor.
	* The HTML must be from <html> to </html>, eventually including
	* the DOCTYPE.
	*     @param {String} data The data to be converted in the
	*            DataProcessor specific format.
	*/
	ConvertToHtml: function(data) {
		// Call the original code.
		return FCKDataProcessor.prototype.ConvertToHtml.call(this, data);
	},

	/*
	* Converts a DOM (sub-)tree to a string in the data format.
	*     @param {Object} rootNode The node that contains the DOM tree to be
	*            converted to the data format.
	*     @param {Boolean} excludeRoot Indicates that the root node must not
	*            be included in the conversion, only its children.
	*     @param {Boolean} format Indicates that the data must be formatted
	*            for human reading. Not all Data Processors may provide it.
	*/
	ConvertToDataFormat: function(rootNode, excludeRoot, ignoreIfEmptyParagraph, format) {
		// rootNode is <body>.

		try {		
			this._ShowErorMessage(false);			
		
			// Normalize the document for text node processing (except IE - #1586).
			if (!FCKBrowserInfo.IsIE)
				rootNode.normalize();

			var stringBuilder = new Array();
			
			this._AppendNode(rootNode, stringBuilder, '');
			return stringBuilder.join('').RTrim().replace(/^\n*/, "");
		} catch (err) {
			this._ShowErorMessage(true, err);			
			return '';
		}
	},
	
	_ShowErorMessage: function(showFlag, err){
		try{
			if(showFlag){
				parent.window.document.getElementById('action_loaderErrorMessage').style.display = 'block';
			} else {
				parent.window.document.getElementById('action_loaderErrorMessage').style.display = 'none';
			}
			if(err && err.message){
				parent.window.document.getElementById('action_loaderErrorMessage').innerHTML = err.message;
			}
		} catch(e){	}
	},

	/*
	* Makes any necessary changes to a piece of HTML for insertion in the
	* editor selection position.
	*     @param {String} html The HTML to be fixed.
	*/
	FixHtml: function(html) {
		return html;
	},

	// Collection of element definitions:
	//		0 : Prefix
	//		1 : Suffix
	//		2 : Ignore children
	_BasicElements: {
		body: [],
		b: ["'''", "'''"],
		strong: ["'''", "'''"],
		i: ["''", "''"],
		u: ["__", "__"],
		strike: ["--", "--"],
		em: ["''", "''"],
		p: ['\n', '\n'],
		h1: ['\n= ', ' =\n'],
		h2: ['\n== ', ' ==\n'],
		h3: ['\n=== ', ' ===\n'],
		h4: ['\n==== ', ' ====\n'],
		h5: ['\n===== ', ' =====\n'],
		h6: ['\n====== ', ' ======\n'],
		br: ['<br/>', null, true],
		hr: ['\n----\n', null, true]
	},


	_DecodeWikiPath: function(path, isWikiInfo) {
		if (isWikiInfo) {
			return decodeURI(path).replace(/_/g, ' ');
		}

		var wikiInternalStart = null;
		if (window.parent.popup) {
			wikiInternalStart = window.parent.popup.parent.wikiInternalStart;
			wikiInternalFile = window.parent.popup.parent.wikiInternalFile;
		}
		else {
			wikiInternalStart = window.parent.wikiInternalStart;
			wikiInternalFile = window.parent.wikiInternalFile;
		}

		var internalReplaced = false;
		if ((wikiInternalStart && wikiInternalStart != null) || (wikiInternalFile && wikiInternalFile != null))// && path.toLowerCase().StartsWith(wikiInternalStart.toLowerCase()))
		{
			if ((wikiInternalStart && wikiInternalStart != null) && path.indexOf(wikiInternalStart) >= 0) {
				path = path.substring(path.indexOf(wikiInternalStart));
				path = path.replace(wikiInternalStart, '');
				internalReplaced = true;
			}

			if ((wikiInternalFile && wikiInternalFile != null) && path.indexOf(wikiInternalFile) >= 0) {
				path = path.substring(path.indexOf(wikiInternalFile));
				path = path.replace(wikiInternalFile, '');
				internalReplaced = false;
			}

			if (!/^\w+:\/\//.test(path) && internalReplaced) {
			    path = path.split('&')[0];
				path = decodeURI(path);
				path = path.replace(/_/g, ' ');
			}
		}

		return path;
	},

	_HasWikiInfoDifImage: function(htmlNode) {
		var result = false;
		var child;
		for (var i = 0; i < htmlNode.childNodes.length; i++) {
			child = htmlNode.childNodes[i];
			if (child.tagName.toLowerCase() == 'img') {
				result = true;
			}
			else {
				result |= this._HasWikiInfoDifImage(child);
			}

			if (result) {
				break;
			}
		}
		return result;
	},

	_GetActualDivWithWikiInfo: function(htmlNode) {
		var result = null;
		var original = htmlNode;
		var child;
		for (var i = 0; i < htmlNode.childNodes.length; i++) {
			child = htmlNode.childNodes[i];
			if (child.tagName && child.tagName.toLowerCase() == 'div' && child.getAttribute('_wikiInfo') != null) {
				return this._GetActualDivWithWikiInfo(child);
			}

			result = this._GetActualDivWithWikiInfo(child);
			if (result != null && result.tagName && result.tagName.toLowerCase() == 'div' && result.getAttribute('_wikiInfo') != null) {
				return result;
			}
		}

		return original;
	},

	_ListLayer: [],
	// This function is based on FCKXHtml._AppendNode.
	_AppendNode: function(htmlNode, stringBuilder, prefix, linkDescNotNeed) {
		if (!htmlNode)
			return;

		switch (htmlNode.nodeType) {
			// Element Node.   
			case 1:

				if (htmlNode.tagName.toLowerCase() == 'br'
					&& htmlNode.parentNode.tagName.toLowerCase() == 'code') {
					var val = '\r';
					if (htmlNode == htmlNode.parentNode.firstChild)
						val += '\r';
					if (FCKBrowserInfo.IsIE) {
						stringBuilder.push("\r\n");
					}
					else {
						stringBuilder.push(val);
					}
					return;
				}

				// Here we found an element that is not the real element, but a
				// fake one (like the Flash placeholder image), so we must get the real one.
				if (htmlNode.getAttribute('_fckfakelement') && !htmlNode.getAttribute('_fck_mw_math'))
					return this._AppendNode(FCK.GetRealElement(htmlNode), stringBuilder);

				// Mozilla insert custom nodes in the DOM.
				if (FCKBrowserInfo.IsGecko && htmlNode.hasAttribute('_moz_editor_bogus_node'))
					return;

				// This is for elements that are instrumental to FCKeditor and
				// must be removed from the final HTML.
				if (htmlNode.getAttribute('_fcktemp'))
					return;

				// Get the element name.
				var sNodeName = htmlNode.tagName.toLowerCase();

				if (FCKBrowserInfo.IsIE) {
					// IE doens't include the scope name in the nodeName. So, add the namespace.
					if (htmlNode.scopeName && htmlNode.scopeName != 'HTML' && htmlNode.scopeName != 'FCK')
						sNodeName = htmlNode.scopeName.toLowerCase() + ':' + sNodeName;
				}
				else {
					if (sNodeName.StartsWith('fck:'))
						sNodeName = sNodeName.Remove(0, 4);
				}

				// Check if the node name is valid, otherwise ignore this tag.
				// If the nodeName starts with a slash, it is a orphan closing tag.
				// On some strange cases, the nodeName is empty, even if the node exists.
				if (!FCKRegexLib.ElementName.test(sNodeName))
					return;

				if (sNodeName == 'br' && (this._inPre || this._inLSpace)) {
					stringBuilder.push("\n");
					if (this._inLSpace)
						stringBuilder.push(" ");
					return;
				}

				// Remove the <br> if it is a bogus node.
				if (sNodeName == 'br' && htmlNode.getAttribute('type', 2) == '_moz')
					return;

				// The already processed nodes must be marked to avoid then to be duplicated (bad formatted HTML).
				// So here, the "mark" is checked... if the element is Ok, then mark it.
				if (htmlNode._fckxhtmljob && htmlNode._fckxhtmljob == FCKXHtml.CurrentJobNum)
					return;

				var basicElement = this._BasicElements[sNodeName];
				if (basicElement) {
					var basic0 = basicElement[0];
					var basic1 = basicElement[1];
					if (basicElement[0] == '\n') {
						var pr1 = stringBuilder[stringBuilder.length - 1];
						if (pr1 != '\n')
							basic0 = '\n\n';
					}

					if ((basicElement[0] == "''" || basicElement[0] == "'''") && stringBuilder.length > 2) {
						var pr1 = stringBuilder[stringBuilder.length - 1];
						var pr2 = stringBuilder[stringBuilder.length - 2];

						if (pr1 + pr2 == "'''''") {
							if (basicElement[0] == "''") {
								basic0 = '<i>';
								basic1 = '</i>';
							}
							if (basicElement[0] == "'''") {
								basic0 = '<b>';
								basic1 = '</b>';
							}
						}
					}

					if (basic0)
						stringBuilder.push(basic0);


					var len = stringBuilder.length;

					if (!basicElement[2]) {
						this._AppendChildNodes(htmlNode, stringBuilder, prefix);
						// only empty element inside, remove it to avoid quotes
						if ((stringBuilder.length == len || (stringBuilder.length == len + 1 && !stringBuilder[len].length))
							&& basicElement[0] && basicElement[0].charAt(0) == "'") {
							stringBuilder.pop();
							stringBuilder.pop();
							return;
						}
					}

					if (basic1)
						stringBuilder.push(basic1);
				}
				else {
					if (htmlNode.getAttribute('_wikiIgnore') != null)
						return;
					switch (sNodeName) {
						case 'script':
							break; //Do Nothing
						case 'ol':
						case 'ul':
							if (sNodeName.IEquals('ul', 'ol'));
							{
								this._ListLayer.push(sNodeName.IEquals('ul') ? '*' : '#');
							}

							var isFirstLevel = !htmlNode.parentNode.nodeName.IEquals('ul', 'ol', 'li', 'dl', 'dt', 'dd');

							this._AppendChildNodes(htmlNode, stringBuilder, prefix);

							if (isFirstLevel && stringBuilder[stringBuilder.length - 1] != "\n") {
								stringBuilder.push('\n');
							}

							if (sNodeName.IEquals('ul', 'ol'));
							{
								this._ListLayer.pop(sNodeName.IEquals('ul') ? '*' : '#');
							}
							break;

						case 'li':

							if (stringBuilder.length > 0) {
								var sLastStr = stringBuilder[stringBuilder.length - 1];
								if (sLastStr != ";" && sLastStr != ":" && sLastStr != "#" && sLastStr != "*")
									stringBuilder.push('\n' + prefix);
							}

							var parent = htmlNode.parentNode;
							var listType = "#";

							while (parent) {
								if (parent.nodeName.toLowerCase() == 'ul') {
									listType = "*";
									break;
								}
								else if (parent.nodeName.toLowerCase() == 'ol') {
									listType = "#";
									break;
								}
								else if (parent.nodeName.toLowerCase() != 'li')
									break;

								parent = parent.parentNode;
							}

							for (var i = 1; i < this._ListLayer.length; i++) {
								stringBuilder.push(this._ListLayer[i]);
							}
							stringBuilder.push(listType);
							this._AppendChildNodes(htmlNode, stringBuilder, prefix + listType);

							break;

						case 'a':

							var pipeline = true;
							var __innerText = htmlNode.innerHTML.replace(/<\/?(.|\n)*?>/g, '');
							__innerText = FCKTools.HTMLDecode(__innerText.replace(/\s+/g, ' '));
							// Get the actual Link href.
							var href = htmlNode.getAttribute('_fcksavedurl');
							var hrefType = htmlNode.getAttribute('_fck_mw_type') || '';
							var hrefName = htmlNode.getAttribute('name') || '';
							if (href == null)
								href = htmlNode.getAttribute('href', 2) || '';

							var isWikiUrl = true;

							if (hrefType == "media")
								stringBuilder.push('[[Media:');
							else if (htmlNode.className == "extiw") {
								stringBuilder.push('[[');
								var isWikiUrl = true;
							}
							else {
								var isWikiUrl = !(href.StartsWith('mailto:') || /^\w+:\/\//.test(href));
								if (!isWikiUrl && href == __innerText) {
									stringBuilder.push(href);
									break;
								}
								stringBuilder.push(isWikiUrl ? '[[' : '[');
							}
							//#2223
							if (htmlNode.getAttribute('_fcknotitle') && htmlNode.getAttribute('_fcknotitle') == "true") {
								var testHref = FCKConfig.ProtectedSource.Revert(href, 0);
								var testInner = FCKConfig.ProtectedSource.Revert(__innerText, 0);
								if (href.toLowerCase().StartsWith('category:'))
									testInner = 'Category:' + testInner;
								if (testHref.toLowerCase().StartsWith('rtecolon'))
									testHref = testHref.replace(/rtecolon/, ":");
								testInner = testInner.replace(/&amp;/, "&");
								if (testInner == testHref)
									pipeline = false;
							}
							if (href.toLowerCase().StartsWith('rtecolon'))		//change 'rtecolon=' => ':' in links
							{
								stringBuilder.push(':');
								href = href.substring(8);
							}


							href = this._DecodeWikiPath(href);


							if (htmlNode.getAttribute('_wikiFile') != null) {
								href = 'File:' + href;
							}




							if (href.toLowerCase().StartsWith("category:") && htmlNode.getAttribute('_wikiCategoryLink') != null) {
								stringBuilder.push(':');
							}

							//href = unescape(href);

							stringBuilder.push(href);
							if (href != __innerText && !linkDescNotNeed) {

								var numberLink = __innerText.replace(/\[[0-9]+\]/g, "");
								numberLink = numberLink.replace(/\s/g, "");
								if (pipeline && numberLink != '' && (!isWikiUrl || href != __innerText || !href.toLowerCase().StartsWith("category:"))) {
									stringBuilder.push(isWikiUrl ? '|' : ' ');
									stringBuilder.push(__innerText);
								}
							}

							stringBuilder.push(isWikiUrl ? ']]' : ']');

							break;

						case 'dl':

							this._AppendChildNodes(htmlNode, stringBuilder, prefix);
							var isFirstLevel = !htmlNode.parentNode.nodeName.IEquals('ul', 'ol', 'li', 'dl', 'dd', 'dt');
							if (isFirstLevel && stringBuilder[stringBuilder.length - 1] != "\n")
								stringBuilder.push('\n');

							break;

						case 'dt':

							if (stringBuilder.length > 1) {
								var sLastStr = stringBuilder[stringBuilder.length - 1];
								if (sLastStr != ";" && sLastStr != ":" && sLastStr != "#" && sLastStr != "*")
									stringBuilder.push('\n' + prefix);
							}
							stringBuilder.push(';');
							this._AppendChildNodes(htmlNode, stringBuilder, prefix + ";");

							break;

						case 'dd':

							if (stringBuilder.length > 1) {
								var sLastStr = stringBuilder[stringBuilder.length - 1];
								if (sLastStr != ";" && sLastStr != ":" && sLastStr != "#" && sLastStr != "*")
									stringBuilder.push('\n' + prefix);
							}
							stringBuilder.push(':');
							this._AppendChildNodes(htmlNode, stringBuilder, prefix + ":");

							break;

						case 'table':

							var attribs = this._GetAttributesStr(htmlNode);
							attribs = attribs.replace(/\s*FCK__.*?(\s|")/gi, '$1');
							stringBuilder.push('\n{|');
							if (attribs.length > 0)
								stringBuilder.push(attribs);
							stringBuilder.push('\n');

							if (htmlNode.caption && htmlNode.caption.innerHTML.length > 0) {
								stringBuilder.push('|+ ');
								this._AppendChildNodes(htmlNode.caption, stringBuilder, prefix);
								stringBuilder.push('\n');
							}

							for (var r = 0; r < htmlNode.rows.length; r++) {
								attribs = this._GetAttributesStr(htmlNode.rows[r]);

								if (r != 0) {
									stringBuilder.push('|-');
									if (attribs.length > 0)
										stringBuilder.push(attribs);
									stringBuilder.push('\n');
								}

								for (var c = 0; c < htmlNode.rows[r].cells.length; c++) {
									attribs = this._GetAttributesStr(htmlNode.rows[r].cells[c]);

									if (htmlNode.rows[r].cells[c].tagName.toLowerCase() == "th")
										stringBuilder.push('!');
									else
										stringBuilder.push('|');

									if (attribs.length > 0)
										stringBuilder.push(attribs + ' |');

									stringBuilder.push(' ');

									this._IsInsideCell = true;
									this._AppendChildNodes(htmlNode.rows[r].cells[c], stringBuilder, prefix);
									this._IsInsideCell = false;

									stringBuilder.push('\n');
								}
							}

							stringBuilder.push('|}\n');

							break;

						case 'img':

							var formula = htmlNode.getAttribute('_fck_mw_math');

							if (formula && formula.length > 0) {
								stringBuilder.push('<math>');
								stringBuilder.push(formula);
								stringBuilder.push('</math>');
								return;
							}

							var imgName = htmlNode.getAttribute('_fck_mw_filename');
							var imgCaption = htmlNode.getAttribute('alt') || '';
							var imgType = htmlNode.getAttribute('_fck_mw_type') || '';
							var imgLocation = htmlNode.getAttribute('_fck_mw_location') || '';
							var imgWidth = htmlNode.getAttribute('_fck_mw_width') || '';
							var imgHeight = htmlNode.getAttribute('_fck_mw_height') || '';
							var imgStyleWidth = (parseInt(htmlNode.style.width) || '') + '';
							var imgStyleHeight = (parseInt(htmlNode.style.height) || '') + '';
							var imgRealWidth = (htmlNode.getAttribute('width') || '') + '';
							var imgRealHeight = (htmlNode.getAttribute('height') || '') + '';

							stringBuilder.push('[[Image:');
							stringBuilder.push(imgName);

							if (imgStyleWidth.length > 0)
								imgWidth = imgStyleWidth;
							else if (imgWidth.length > 0 && imgRealWidth.length > 0)
								imgWidth = imgRealWidth;

							if (imgStyleHeight.length > 0)
								imgHeight = imgStyleHeight;
							else if (imgHeight.length > 0 && imgRealHeight.length > 0)
								imgHeight = imgRealHeight;

							if (imgType.length > 0)
								stringBuilder.push('|' + imgType);

							if (imgLocation.length > 0)
								stringBuilder.push('|' + imgLocation);

							if (imgWidth.length > 0) {
								stringBuilder.push('|' + imgWidth);

								if (imgHeight.length > 0)
									stringBuilder.push('x' + imgHeight);

								stringBuilder.push('px');
							}

							if (imgCaption.length > 0)
								stringBuilder.push('|' + imgCaption);

							stringBuilder.push(']]');

							break;

						case 'pre':
							var result;
							if (stringBuilder && stringBuilder.length != 0 && stringBuilder[stringBuilder.length - 1].replace(/[^\n]/g, '') == '') {
								stringBuilder.push('\n');
							}

							if (htmlNode.parentNode.nodeName.toUpperCase() == "NOWIKI") {
								result = '<pre>' + htmlNode.innerHTML + '</pre>';
								result = result.replace(/<br\s*\/?>/gi, '\n');
							}
							else {
								result = '\n&nbsp;' + htmlNode.innerHTML.replace(/\n/g, '\n&nbsp;') + '\n&nbsp;';
								result = result.replace(/<br\s*\/?>/gi, '\n&nbsp;');
							}

							result = result.replace(/&nbsp;/gi, ' ');




							/*var attribs = this._GetAttributesStr( htmlNode ) ;

							if ( htmlNode.className == "_fck_mw_lspace")
							{
							stringBuilder.push( "\n " ) ;
							this._inLSpace = true ;
							this._AppendChildNodes( htmlNode, stringBuilder, prefix ) ;
							this._inLSpace = false ;
							var len = stringBuilder.length ;
							if ( len>1 ) {
							var tail = stringBuilder[len-2] + stringBuilder[len-1];
							if ( len>2 ) {
							tail = stringBuilder[len-3] + tail ;
							}
							if (tail.EndsWith("\n ")) {
							stringBuilder[len-1] = stringBuilder[len-1].replace(/ $/, "");
							}
							else if ( !tail.EndsWith("\n") ) {
							stringBuilder.push( "\n" ) ;
							}
							}
							}
							else
							{
							stringBuilder.push( '<' ) ;
							stringBuilder.push( sNodeName ) ;

								if ( attribs.length > 0 )
							stringBuilder.push( attribs ) ;
							if(htmlNode.innerHTML == "")
							stringBuilder.push( ' />' ) ;
							else
							{
							stringBuilder.push( '>' ) ;
							this._inPre = true ;
							this._AppendChildNodes( htmlNode, stringBuilder, prefix ) ;
							this._inPre = false ;

									stringBuilder.push( '<\/' ) ;
							stringBuilder.push( sNodeName ) ;
							stringBuilder.push( '>' ) ;
							}
							}*/
							stringBuilder.push(result);
							stringBuilder.push('\n');
							break;
						case 'nowiki':
							if (htmlNode.firstChild && htmlNode.firstChild.nodeName.toUpperCase() == "PRE") {
								stringBuilder.push('\n<nowiki>');
								this._AppendChildNodes(htmlNode, stringBuilder, prefix);
								stringBuilder.push('</nowiki>\n');
							}
							else {
								var result = '\n<nowiki>';
								result += htmlNode.innerHTML;
								result += '</nowiki>\n';
								stringBuilder.push(result);
							}

							break;
						case 'code':
							var attribs = this._GetAttributesStr(htmlNode);
							while (stringBuilder.length > 0 && stringBuilder[stringBuilder.length - 1].replace(/\s+/g, '') == '')
								stringBuilder.pop();

							stringBuilder.push('\n<');
							stringBuilder.push(sNodeName);
							if (attribs.length > 0)
								stringBuilder.push(attribs);
							stringBuilder.push('>\n');
							this._AppendChildNodes(htmlNode, stringBuilder, prefix);
							stringBuilder.push('\n<\/');
							stringBuilder.push(sNodeName);
							stringBuilder.push('>');
							break;

						case 'span':
							switch (htmlNode.className) {
								case 'fck_mw_source':
									var refLang = htmlNode.getAttribute('lang');

									stringBuilder.push('<source');
									stringBuilder.push(' lang="' + refLang + '"');
									stringBuilder.push('>');
									stringBuilder.push(FCKTools.HTMLDecode(htmlNode.innerHTML).replace(/fckLR/gi, '\r\n'));
									stringBuilder.push('</source>');
									return;

								case 'fck_mw_ref':
									var refName = htmlNode.getAttribute('name');

									stringBuilder.push('<ref');

									if (refName && refName.length > 0)
										stringBuilder.push(' name="' + refName + '"');

									if (htmlNode.innerHTML.length == 0)
										stringBuilder.push(' />');
									else {
										stringBuilder.push('>');
										stringBuilder.push(htmlNode.innerHTML);
										stringBuilder.push('</ref>');
									}
									return;

								case 'fck_mw_references':
									stringBuilder.push('<references />');
									return;

								case 'fck_mw_signature':
									stringBuilder.push(FCKConfig.WikiSignature);
									return;

								case 'fck_mw_template':
									stringBuilder.push(FCKTools.HTMLDecode(htmlNode.innerHTML).replace(/fckLR/gi, '\r\n'));
									return;

								case 'fck_mw_magic':
									stringBuilder.push(htmlNode.innerHTML);
									return;

								case 'fck_mw_nowiki':
									sNodeName = 'nowiki';
									break;

								case 'fck_mw_html':
									sNodeName = 'html';
									break;

								case 'fck_mw_includeonly':
									sNodeName = 'includeonly';
									break;

								case 'fck_mw_noinclude':
									sNodeName = 'noinclude';
									break;

								case 'fck_mw_gallery':
									sNodeName = 'gallery';
									break;

								case 'fck_mw_onlyinclude':
									sNodeName = 'onlyinclude';

									break;
							}

							// Change the node name and fell in the "default" case.
							if (htmlNode.getAttribute('_fck_mw_customtag'))
								sNodeName = htmlNode.getAttribute('_fck_mw_tagname');

						default:
							if (sNodeName == 'div') {
								if (htmlNode.getAttribute('_wikiInfo') != null) {
									htmlNode = this._GetActualDivWithWikiInfo(htmlNode);
									var wikiInfo = htmlNode.getAttribute('_wikiInfo');

									stringBuilder.push('[[Image:');
									stringBuilder.push(this._DecodeWikiPath(wikiInfo.split(':')[0], true));
									if (wikiInfo.split(':')[3] == 1) {
										stringBuilder.push('|thumb');
									}
									else if (wikiInfo.split(':')[4] == 1) {
										stringBuilder.push('|frame');
									}

									if (wikiInfo.split(':')[5] > 0) {
										stringBuilder.push('|' + wikiInfo.split(':')[5] + 'px');
									}

									if (wikiInfo.split(':')[2] != '') {
										stringBuilder.push('|' + wikiInfo.split(':')[2]);
									}

									if (wikiInfo.split(':')[1] != '') {
										stringBuilder.push('|alt=' + this._DecodeWikiPath(wikiInfo.split(':')[1], true));
									}

									if (wikiInfo.split(':')[6] != '') {
										stringBuilder.push('|' + this._DecodeWikiPath(wikiInfo.split(':')[6], true));
									}

									stringBuilder.push(']]');
									break;
								}
								else if (htmlNode.getAttribute('_wikiTOC') != null) {
									stringBuilder.push('@@TOC@@\n');
									break;
								}
								else if (htmlNode.getAttribute('_wikiLinkTOC') != null) {
									stringBuilder.push('@@LinkTOC@@\n');
									break;
								}
								else if (htmlNode.getAttribute('_wikiCatArea') != null) {
									this._AppendChildNodes(htmlNode, stringBuilder, prefix, true);
									break;
								}
								else if (htmlNode.getAttribute('_wikiCodeStyle') != null) {
									var code = htmlNode.innerHTML.replace(/^\s+|\s+$/gi, ''); //Trim
									stringBuilder.push('\n<code lang="' + htmlNode.getAttribute('_wikiCodeStyle') + '">\n');
									//code = code.replace(/<span\s+class="lnum">.*?<\/span>/gi, '');
									code = code.replace(/<pre>&nbsp;<\/pre>/gi, '');
									code = code.replace(/<ul>([\s\S]*?)<\/ul>/gi, '$1');
									code = code.replace(/<ul>([\s\S]*?)<\/ul>/gi, '$1');
									code = code.replace(/<ol>([\s\S]*?)<\/ol>/gi, '$1');
									code = code.replace(/<ol>([\s\S]*?)<\/ol>/gi, '$1');
									code = code.replace(/<li>([\s\S]*?)<\/li>/gi, '$1');
									code = code.replace(/<pre>([\s\S]*?)<\/pre>/gi, '$1');
									code = code.replace(/<span class="?(rem|kwrd|str|op|preproc|asp|html|attr|alt)"?>(.*?)<\/span>/gi, '$2');
									//HightLight issue
									code = code.replace(/&amp;/gi, '&');
									code = code.replace(/&lt;/gi, '<');
									code = code.replace(/&gt;/gi, '>');
									code = code.replace(/^\s+|\s+$/gi, ''); //Trim
									stringBuilder.push(code);
									stringBuilder.push('\n</code>\n');
									break;
								}
							}
							var attribs = this._GetAttributesStr(htmlNode);

							stringBuilder.push('<');
							stringBuilder.push(sNodeName);

							if (attribs.length > 0)
								stringBuilder.push(attribs);

							stringBuilder.push('>');
							this._AppendChildNodes(htmlNode, stringBuilder, prefix);
							stringBuilder.push('<\/');
							stringBuilder.push(sNodeName);
							stringBuilder.push('>');
							break;
					}
				}

				htmlNode._fckxhtmljob = FCKXHtml.CurrentJobNum;
				return;

				// Text Node.
			case 3:

				var parentIsSpecialTag = htmlNode.parentNode.getAttribute('_fck_mw_customtag');
				var textValue = htmlNode.nodeValue;


				if (!parentIsSpecialTag) {
					if (FCKBrowserInfo.IsIE && this._inLSpace) {
						textValue = textValue.replace(/\r/gi, "\r ");
						if (textValue.EndsWith("\r ")) {
							textValue = textValue.replace(/\r $/, "\r");
						}
					}
					if (!FCKBrowserInfo.IsIE && this._inLSpace) {
						textValue = textValue.replace(/\n(?! )/g, "\n ");
					}

					if (!this._inLSpace && !this._inPre) {
						textValue = textValue.replace(/[\n\t]/g, ' ');
					}

					if (htmlNode.parentNode.nodeName.toUpperCase() != 'CODE')
						textValue = FCKTools.HTMLEncode(textValue);

					if (!(FCKBrowserInfo.IsIE && htmlNode.parentNode.tagName.toLowerCase() == 'code')) {
						textValue = textValue.replace(/\u00A0/g, '&nbsp;');
					}

					if ((!htmlNode.previousSibling ||
					(stringBuilder.length > 0 && stringBuilder[stringBuilder.length - 1].EndsWith('\n'))) && !this._inLSpace && !this._inPre) {
						textValue = textValue.LTrim();
					}

					if (!htmlNode.nextSibling && !this._inLSpace && !this._inPre && (!htmlNode.parentNode || !htmlNode.parentNode.nextSibling))
						textValue = textValue.RTrim();

					if (!this._inLSpace && !this._inPre)
						textValue = textValue.replace(/ {2,}/gi, ' ');

					if (this._inLSpace && textValue.length == 1 && textValue.charCodeAt(0) == 13)
						textValue = textValue + " ";

					if (!this._inLSpace && !this._inPre && textValue == " ") {
						var len = stringBuilder.length;
						if (len > 1) {
							var tail = stringBuilder[len - 2] + stringBuilder[len - 1];
							if (tail.toString().EndsWith("\n"))
								textValue = "";
						}
					}

					if (this._IsInsideCell) {
						var result, linkPattern = new RegExp("\\[\\[.*?\\]\\]", "g");
						while (result = linkPattern.exec(textValue)) {
							textValue = textValue.replace(result, result.toString().replace(/\|/gi, "<!--LINK_PIPE-->"));
						}
						textValue = textValue.replace(/\|/gi, '&#124;');
						textValue = textValue.replace(/<!--LINK_PIPE-->/gi, '|');
					}
				}
				else {
					textValue = FCKTools.HTMLDecode(textValue).replace(/fckLR/gi, '\r\n');
				}

				if (textValue != '')
					stringBuilder.push(textValue);

				return;

				// Comment
			case 8:
				// IE catches the <!DOTYPE ... > as a comment, but it has no
				// innerHTML, so we can catch it, and ignore it.
				if (htmlNode.parentNode.tagName.toLowerCase() == 'code') {
					stringBuilder.push(htmlNode.innerHTML);
					return;
				}
				if (FCKBrowserInfo.IsIE && !htmlNode.innerHTML)
					return;

				stringBuilder.push("<!--");

				try { stringBuilder.push(htmlNode.nodeValue); }
				catch (e) { /* Do nothing... probably this is a wrong format comment. */ }

				stringBuilder.push("-->");
				return;
		}
	},

	_AppendChildNodes: function(htmlNode, stringBuilder, listPrefix, isCatDiv) {
		var child = htmlNode.firstChild;

		if (isCatDiv) {
			child = child.nextSibling;
		}


		while (child) {
			if (isCatDiv) {
				if (child.nodeName != '#text')
					this._AppendNode(child, stringBuilder, listPrefix, true);
			}
			else {
				this._AppendNode(child, stringBuilder, listPrefix);
			}
			child = child.nextSibling;
		}
	},

	_GetAttributesStr: function(htmlNode) {
		var attStr = '';
		var aAttributes = htmlNode.attributes;

		for (var n = 0; n < aAttributes.length; n++) {
			var oAttribute = aAttributes[n];

			if (oAttribute.specified) {
				var sAttName = oAttribute.nodeName.toLowerCase();
				var sAttValue;

				// Ignore any attribute starting with "_fck".
				if (sAttName.StartsWith('_fck'))
					continue;
				// There is a bug in Mozilla that returns '_moz_xxx' attributes as specified.
				else if (sAttName.indexOf('_moz') == 0)
					continue;
				// For "class", nodeValue must be used.
				else if (sAttName == 'class') {
					// Get the class, removing any fckXXX we can have there.
					sAttValue = oAttribute.nodeValue.replace(/(^|\s*)fck\S+/, '').Trim();

					if (sAttValue.length == 0)
						continue;
				}
				else if (sAttName == 'style' && FCKBrowserInfo.IsIE) {
					sAttValue = htmlNode.style.cssText.toLowerCase();
				}
				// XHTML doens't support attribute minimization like "CHECKED". It must be trasformed to cheched="checked".
				else if (oAttribute.nodeValue === true)
					sAttValue = sAttName;
				else
					sAttValue = htmlNode.getAttribute(sAttName, 2); // We must use getAttribute to get it exactly as it is defined.

				// leave templates
				if (sAttName.StartsWith('{{') && sAttName.EndsWith('}}')) {
					attStr += ' ' + sAttName;
				}
				else {
					attStr += ' ' + sAttName + '="' + String(sAttValue).replace('"', '&quot;') + '"';
				}
			}
		}
		return attStr;
	}
};

// Here we change the SwitchEditMode function to make the Ajax call when
// switching from Wikitext.
(function()
{
	if (window.parent.showFCKEditor & (2|4))				//if popup or toggle link
	{
		var original_ULF = FCK.UpdateLinkedField ;
		FCK.UpdateLinkedField = function(){
			if (window.parent.showFCKEditor & 1)			//only if editor is up-to-date
			{
				original_ULF.apply();
			}
		}
	}
	var original = FCK.SwitchEditMode ;
	FCK.ready = true;
	FCK.SwitchEditMode = function()
	{
		var args = arguments ;
		FCK.ready = false;
		var loadHTMLFromAjax = function( result )
		{
			FCK.EditingArea.Textarea.value = result.value ;
			original.apply( FCK, args ) ;
			FCK.ready = true;
		}
		var edittools_markup = parent.document.getElementById ('editpage-specialchars') ;

		if ( FCK.EditMode == FCK_EDITMODE_SOURCE )
		{
			// Hide the textarea to avoid seeing the code change.
			FCK.EditingArea.Textarea.style.visibility = 'hidden' ;
			var loading = document.createElement( 'span' ) ;
			loading.innerHTML = '&nbsp;'+ (FCKLang.wikiLoadingWikitext || 'Loading Wikitext. Please wait...' )+'&nbsp;';
			loading.style.position = 'absolute' ;
			loading.style.left = '5px' ;
//			loading.style.backgroundColor = '#ff0000' ;
			FCK.EditingArea.Textarea.parentNode.appendChild( loading, FCK.EditingArea.Textarea ) ;
			// if we have a standard Edittools div, hide the one containing wikimarkup
			if (edittools_markup) {
				edittools_markup.style.display = 'none' ;
			}

			// Use Ajax to transform the Wikitext to HTML.
			if( window.parent.popup ){
				window.parent.popup.parent.FCK_sajax( 'wfSajaxWikiToHTML', FCK.EditingArea.Textarea.value, loadHTMLFromAjax ) ;
			}
			else{
				window.parent.FCK_sajax( 'wfSajaxWikiToHTML', FCK.EditingArea.Textarea.value, loadHTMLFromAjax ) ;
			}
		}
		else {
			original.apply( FCK, args ) ;
			if (edittools_markup) {
				edittools_markup.style.display = '' ;
			}
			FCK.ready = true;
		}
	}
})() ;

// MediaWiki document processor.
FCKDocumentProcessor.AppendNew().ProcessDocument = function( document )
{
	// #1011: change signatures to SPAN elements
	var aTextNodes = document.getElementsByTagName( '*' ) ;
	var i = 0 ;
	var signatureRegExp = new RegExp( FCKConfig.WikiSignature.replace( /([*:.*?();|$])/gi, "\\$1" ), "i" );
	while (element = aTextNodes[i++])
	{
		var nodes = element.childNodes ;
		var j = 0 ;
		while ( node = nodes[j++] )
		{
			if ( node.nodeType == 3 )
			{//textNode
				var index = 0 ;

				while ( aSignatures = node.nodeValue.match( signatureRegExp ) )
				{
					index = node.nodeValue.indexOf( aSignatures[0] ) ;
					if ( index != -1 )
					{
						var e = FCK.EditorDocument.createElement( 'span' ) ;
						e.className = "fck_mw_signature";
						var oFakeImage = FCKDocumentProcessor_CreateFakeImage( 'FCK__MWSignature', e ) ;

						var substr1 = FCK.EditorDocument.createTextNode( node.nodeValue.substring(0, index) ) ;
						var substr2 = FCK.EditorDocument.createTextNode( node.nodeValue.substring(index + aSignatures[0].length) ) ;

						node.parentNode.insertBefore( substr1, node ) ;
						node.parentNode.insertBefore( oFakeImage, node ) ;
						node.parentNode.insertBefore( substr2, node ) ;

						node.parentNode.removeChild( node ) ;
						if ( node )
							node.nodeValue = '' ;
					}
				}
			}
		}
	}

	// Templates and magic words.
	var aSpans = document.getElementsByTagName( 'SPAN' ) ;

	var eSpan ;
	var i = aSpans.length - 1 ;
	while ( i >= 0 && ( eSpan = aSpans[i--] ) )
	{
		var className = null ;
		switch ( eSpan.className )
		{
			case 'fck_mw_source' :
				className = 'FCK__MWSource' ;
			case 'fck_mw_ref' :
				if (className == null)
					className = 'FCK__MWRef' ;
			case 'fck_mw_references' :
				if ( className == null )
					className = 'FCK__MWReferences' ;
			case 'fck_mw_template' :
				if ( className == null ) //YC
					className = 'FCK__MWTemplate' ; //YC
			case 'fck_mw_magic' :
				if ( className == null )
					className = 'FCK__MWMagicWord' ;
			case 'fck_mw_magic' :
				if ( className == null )
					className = 'FCK__MWMagicWord' ;
			case 'fck_mw_special' : //YC
				if ( className == null )
					className = 'FCK__MWSpecial' ;
			case 'fck_mw_nowiki' :
				if ( className == null )
					className = 'FCK__MWNowiki' ;
			case 'fck_mw_html' :
				if ( className == null )
					className = 'FCK__MWHtml' ;
			case 'fck_mw_includeonly' :
				if ( className == null )
					className = 'FCK__MWIncludeonly' ;
			case 'fck_mw_gallery' :
				if ( className == null )
					className = 'FCK__MWGallery' ;
			case 'fck_mw_noinclude' :
				if ( className == null )
					className = 'FCK__MWNoinclude' ;
			case 'fck_mw_onlyinclude' :
				if ( className == null )
					className = 'FCK__MWOnlyinclude' ;

				var oImg = FCKDocumentProcessor_CreateFakeImage( className, eSpan.cloneNode(true) ) ;
				oImg.setAttribute( '_' + eSpan.className, 'true', 0 ) ;

				eSpan.parentNode.insertBefore( oImg, eSpan ) ;
				eSpan.parentNode.removeChild( eSpan ) ;
			break ;
		}
	}

	// Math tags without Tex.
	var aImgs = document.getElementsByTagName( 'IMG' ) ;
	var eImg ;
	i = aImgs.length - 1 ;
	while ( i >= 0 && ( eImg = aImgs[i--] ) )
	{
		var className = null ;
		switch ( eImg.className )
		{
			case 'FCK__MWMath' :
				eImg.src = FCKConfig.PluginsPath + 'mediawiki/images/icon_math.gif' ;
			break ;
		}
	}

	// InterWiki / InterLanguage links
	var aHrefs = document.getElementsByTagName( 'A' ) ;
	var a ;
	var i = aHrefs.length - 1 ;
	while ( i >= 0 && ( a = aHrefs[i--] ) )
	{
		if (a.className == 'extiw')
		{
			 a.href = a.title ;
			 a.setAttribute( '_fcksavedurl', a.href ) ;
		}
		else
		{
			if (a.href.toLowerCase().StartsWith( 'rtecolon' ))
			{
				a.href = ":" + a.href.substring(8);
				if (a.innerHTML.toLowerCase().StartsWith( 'rtecolon' ))
				{
					a.innerHTML = a.innerHTML.substring(8);
				}
			}
		}
	}
}

// Context menu for templates.
FCK.ContextMenu.RegisterListener({
	AddItems : function( contextMenu, tag, tagName )
	{
		if ( tagName == 'IMG' )
		{
			if ( tag.getAttribute( '_fck_mw_template' ) )
			{
				contextMenu.AddSeparator() ;
				contextMenu.AddItem( 'MW_Template', FCKLang.wikiMnuTemplate || 'Template Properties' ) ;
			}
			if ( tag.getAttribute( '_fck_mw_magic' ) )
			{
				contextMenu.AddSeparator() ;
				contextMenu.AddItem( 'MW_MagicWord', FCKLang.wikiMnuMagicWord || 'Modify Magic Word' ) ;
			}
			if ( tag.getAttribute( '_fck_mw_ref' ) )
			{
				contextMenu.AddSeparator() ;
				contextMenu.AddItem( 'MW_Ref', FCKLang.wikiMnuReference || 'Reference Properties' ) ;
			}
			if ( tag.getAttribute( '_fck_mw_html' ) )
			{
				contextMenu.AddSeparator() ;
				contextMenu.AddItem( 'MW_Special', 'Edit HTML code' ) ;
			}
			if ( tag.getAttribute( '_fck_mw_source' ) )
			{
				contextMenu.AddSeparator() ;
				contextMenu.AddItem( 'MW_Source', FCKLang.wikiMnuSourceCode || 'Source Code Properties' ) ;
			}
			if ( tag.getAttribute( '_fck_mw_math' ) )
			{
				contextMenu.AddSeparator() ;
				contextMenu.AddItem( 'MW_Math', FCKLang.wikiMnuFormula || 'Edit Formula' ) ;
			}
			if ( tag.getAttribute( '_fck_mw_special' ) || tag.getAttribute( '_fck_mw_nowiki' ) || tag.getAttribute( '_fck_mw_includeonly' ) || tag.getAttribute( '_fck_mw_noinclude' ) || tag.getAttribute( '_fck_mw_onlyinclude' ) || tag.getAttribute( '_fck_mw_gallery' )) //YC
			{
				contextMenu.AddSeparator() ;
				contextMenu.AddItem( 'MW_Special', FCKLang.wikiMnuSpecial || 'Special Tag Properties' ) ;
			}
		}
	}
}) ;
