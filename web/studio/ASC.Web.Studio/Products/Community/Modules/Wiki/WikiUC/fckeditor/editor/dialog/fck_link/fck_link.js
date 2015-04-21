/*
 * FCKeditor - The text editor for Internet - http://www.fckeditor.net
 * Copyright (C) 2003-2009 Frederico Caldeira Knabben
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
 * Scripts related to the Link dialog window (see fck_link.html).
 */

var dialog	= window.parent ;
var oEditor = dialog.InnerDialogLoaded() ;

var FCK			= oEditor.FCK ;
var FCKLang		= oEditor.FCKLang ;
var FCKConfig	= oEditor.FCKConfig ;
var FCKRegexLib	= oEditor.FCKRegexLib ;
var FCKTools	= oEditor.FCKTools ;

//#### Dialog Tabs

// Set the dialog tabs.
//dialog.AddTab( 'Info', FCKLang.DlgLnkInfoTab ) ;



// Function called when a dialog tag is selected.
function OnDialogTabChange( tabCode )
{
	ShowE('divInfo'		, true ) ;

	dialog.SetAutoSize( true ) ;
}

//#### Regular Expressions library.
var oRegex = new Object() ;

oRegex.UriProtocol = /^(((http|https|ftp|news):\/\/)|mailto:)/gi ;

oRegex.UrlOnChangeProtocol = /^(http|https|ftp|news):\/\/(?=.)/gi ;

oRegex.UrlOnChangeTestOther = /^((javascript:)|[#\/\.])/gi ;

oRegex.ReserveTarget = /^_(blank|self|top|parent)$/i ;

oRegex.PopupUri = /^javascript:void\(\s*window.open\(\s*'([^']+)'\s*,\s*(?:'([^']*)'|null)\s*,\s*'([^']*)'\s*\)\s*\)\s*$/ ;

// Accessible popups
oRegex.OnClickPopup = /^\s*on[cC]lick="\s*window.open\(\s*this\.href\s*,\s*(?:'([^']*)'|null)\s*,\s*'([^']*)'\s*\)\s*;\s*return\s*false;*\s*"$/ ;

oRegex.PopupFeatures = /(?:^|,)([^=]+)=(\d+|yes|no)/gi ;





//#### Initialization Code

// oLink: The actual selected link in the editor.
var oLink = dialog.Selection.GetSelection().MoveToAncestorNode( 'A' ) ;
if ( oLink )
	FCK.Selection.SelectNode( oLink ) ;

window.onload = function()
{
	// Translate the dialog box texts.
	oEditor.FCKLanguageManager.TranslatePage(document) ;

	LoadSelection();
	// Show the initial dialog content.
	GetE('divInfo').style.display = '' ;

	// Set the actual uploader URL.

	// Activate the "OK" button.
	dialog.SetOkButton( true ) ;


}


function LoadSelection()
{

	var  innerText;
	
	if ( oLink )
	{
		innerText = oLink.innerHTML;
	}
	else if (FCK.EditorWindow.getSelection) 
	{ 
	 innerText = FCK.EditorWindow.getSelection(); 
	 
	} 
	else if (FCK.EditorWindow.selection && FCK.EditorWindow.selection.createRange) 
	{	
		var range = FCK.EditorWindow.selection.createRange(); 
		innerText  = range.text; 
    }
	else if(FCK.EditorDocument.selection && FCK.EditorDocument.selection.createRange)
	{
		var range = FCK.EditorDocument.selection.createRange(); 
		innerText  = range.text; 
	}
	else 
	{
		innerText  = "";
	} 

	SetName(innerText);

	if ( !oLink ) return ;

	var sType = 'url' ;

	// Get the actual Link href.
	var sHRef = oLink.getAttribute( '_fcksavedurl' ) ;
	if ( sHRef == null )
		sHRef = oLink.getAttribute( 'href' , 2 ) || '' ;
	
		
		SetUrl(sHRef);
}




//#### Called while the user types the URL.
function OnUrlChange()
{
	var sUrl = GetE('txtUrl').value ;
	var sProtocol = oRegex.UrlOnChangeProtocol.exec( sUrl ) ;

	if ( sProtocol )
	{
		//sUrl = sUrl.substr( sProtocol[0].length ) ;
		GetE('txtUrl').value = sUrl ;
	}
	
}


//#### The OK button was hit.
function Ok()
{
	var sUri, sSmallUrl, sProtocol, sInnerHtml;
	oEditor.FCKUndo.SaveUndoStep() ;

	sUri = GetE('txtUrl').value ;
	var UrlOnChangeProtocol = /^(http|https|ftp|news|file):\/\/(?=.)/gi ;
	
	sProtocol = UrlOnChangeProtocol.exec( sUri ) ;

	sSmallUrl = '';
	if(sProtocol)	
	{
		sSmallUrl = sUri.substr( sProtocol[0].length ) ;
	}
	else
	{
		sSmallUrl = sUri;
		sUri = 'http://' + sUri;
	}
	
	if ( sUri.length == 0 || sSmallUrl.length == 0 )
	{
		alert( FCKLang.DlnLnkMsgNoUrl ) ;
		return false ;
	}

	

	

	// If no link is selected, create a new one (it may result in more than one link creation - #220).
	var aLinks = oLink ? [ oLink ] : oEditor.FCK.CreateLink( sUri, true ) ;

	// If no selection, no links are created, so use the uri as the link text (by dom, 2006-05-26)
	var aHasSelection = ( aLinks.length > 0 ) ;
	if ( !aHasSelection )
	{
		sInnerHtml = sUri;

		
		var oLinkPathRegEx = new RegExp("//?([^?\"']+)([?].*)?$") ;
		var asLinkPath = oLinkPathRegEx.exec( sUri ) ;
		if (asLinkPath != null)
			sInnerHtml = asLinkPath[1];  // use matched path
				
		// Create a new (empty) anchor.
		aLinks = [ oEditor.FCK.InsertElement( 'a' ) ] ;
	}

	for ( var i = 0 ; i < aLinks.length ; i++ )
	{
		oLink = aLinks[i] ;

		if ( aHasSelection )
			sInnerHtml = oLink.innerHTML ;		// Save the innerHTML (IE changes it if it is like an URL).

		oLink.href = sUri ;
		SetAttribute( oLink, '_fcksavedurl', sUri ) ;

		var onclick;
		
			// Check if the previous onclick was for a popup:
			// In that case remove the onclick handler.
			onclick = oLink.getAttribute( 'onclick_fckprotectedatt' ) ;
			if ( onclick )
			{
				// Decode the protected string
				onclick = decodeURIComponent( onclick ) ;

				if( oRegex.OnClickPopup.test( onclick ) )
					SetAttribute( oLink, 'onclick_fckprotectedatt', '' ) ;
			}
		

		if(GetE('txtName').value.length > 0)
		{
			oLink.innerHTML = GetE('txtName').value ;		
		}
		else
		{
			oLink.innerHTML = sInnerHtml ;		// Set (or restore) the innerHTML
		}

		
		
		//SetAttribute( oLink, 'target', null ) ;

		// Let's set the "id" only for the first link to avoid duplication.
	

	}

	// Select the (first) link.
	try
	{oEditor.FCKSelection.SelectNode( aLinks[0] );}
	catch(e){}

	return true ;
}


function SetUrl( url )
{
	GetE('txtUrl').value = url ;
	OnUrlChange() ;
}

function SetName( name )
{
	GetE('txtName').value = name ;
}


