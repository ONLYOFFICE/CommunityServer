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
 * Scripts related to the Image dialog window (see fck_image.html).
 */

var dialog		= window.parent ;
var oEditor		= dialog.InnerDialogLoaded() ;
var FCK			= oEditor.FCK ;
var FCKLang		= oEditor.FCKLang ;
var FCKConfig	= oEditor.FCKConfig ;
var FCKDebug	= oEditor.FCKDebug ;
var FCKTools	= oEditor.FCKTools ;

var bImageButton = ( document.location.search.length > 0 && document.location.search.substr(1) == 'ImageButton' ) ;



//#### Dialog Tabs

// Set the dialog tabs.
dialog.AddTab( 'Info', FCKLang.DlgImgInfoTab ) ;


if ( FCKConfig.ImageUpload )
	dialog.AddTab( 'Upload', FCKLang.DlgLnkUpload ) ;



// Function called when a dialog tag is selected.
function OnDialogTabChange( tabCode )
{
	ShowE('divInfo'		, ( tabCode == 'Info' ) ) ;
	ShowE('divUpload'	, ( tabCode == 'Upload' ) ) ;
}

// Get the selected image (if available).
var oImage = dialog.Selection.GetSelectedElement() ;

if ( oImage && oImage.tagName != 'IMG' && !( oImage.tagName == 'INPUT' && oImage.type == 'image' ) )
	oImage = null ;

// Get the active link.
var oLink;
try {
    oLink = dialog.Selection.GetSelection().MoveToAncestorNode('A');;
} catch (e) {
    oLink = null;
}

var oImageOriginal ;

function CheckIsZoom()
{
    
	if(FCKConfig.MaxImageWidth == 0 && typeof(window.parent.parent.GetMaxImageWidth) != 'function')
	{
		return;
	}
	
	var maxWidth = FCKConfig.MaxImageWidth > 0 ? FCKConfig.MaxImageWidth : window.parent.parent.GetMaxImageWidth();
	var postedWidth = null;
	
	
	try
	{
		postedWidth = oImageOriginal.width;
	} 
	catch(ex){};
	
	
	if(postedWidth == null)
	{
		return;
	}
	
	if(postedWidth > maxWidth)
	{
		setTimeout(function()
		{
			GetE('txtWidth').value = maxWidth;
			OnSizeChanged( 'Width', maxWidth ) ;
		},0);
		
	}
	
	    dialog.document.getElementById('TabsRow').style.display = 'block';
        dialog.document.getElementById('innerContents').style.display = 'block';
        dialog.document.getElementById('PopupButtons').style.display = 'block';
        window.parent.Throbber.Hide() ;

}

function UpdateOriginal( resetSize )
{
	if ( !eImgPreview )
		return ;

	if ( GetE('txtUrl').value.length == 0 )
	{
		oImageOriginal = null ;
		return ;
	}

	oImageOriginal = document.createElement( 'IMG' ) ;	// new Image() ;

	if ( resetSize )
	{
		oImageOriginal.onload = function()
		{
			this.onload = null ;
			ResetSizes() ;
		}
	}

	oImageOriginal.src = eImgPreview.src ;
}

var bPreviewInitialized ;

window.onload = function()
{
	// Translate the dialog box texts.
	
	
	oEditor.FCKLanguageManager.TranslatePage(document) ;

	GetE('btnLockSizes').title = FCKLang.DlgImgLockRatio ;
	GetE('btnResetSize').title = FCKLang.DlgBtnResetSize ;

	// Load the selected element information (if any).
	LoadSelection() ;

	
	UpdateOriginal() ;

	// Set the actual uploader URL.
	if ( FCKConfig.ImageUpload )
		GetE('frmUpload').action = FCKConfig.ImageUploadURL ;

	dialog.SetAutoSize( true ) ;

	// Activate the "OK" button.
	dialog.SetOkButton( true ) ;

	SelectField( 'txtUrl' ) ;
}

function LoadSelection()
{
	if ( ! oImage ) return ;

	var sUrl = oImage.getAttribute( '_fcksavedurl' ) ;
	if ( sUrl == null )
		sUrl = GetAttribute( oImage, 'src', '' ) ;

	GetE('txtUrl').value    = sUrl ;
	GetE('txtAlt').value    = GetAttribute( oImage, 'alt', '' ) ;

	var iWidth, iHeight ;

	var regexSize = /^\s*(\d+)px\s*$/i ;

	if ( oImage.style.width )
	{
		var aMatchW  = oImage.style.width.match( regexSize ) ;
		if ( aMatchW )
		{
			iWidth = aMatchW[1] ;
			oImage.style.width = '' ;
			SetAttribute( oImage, 'width' , iWidth ) ;
		}
	}

	if ( oImage.style.height )
	{
		var aMatchH  = oImage.style.height.match( regexSize ) ;
		if ( aMatchH )
		{
			iHeight = aMatchH[1] ;
			oImage.style.height = '' ;
			SetAttribute( oImage, 'height', iHeight ) ;
		}
	}

	GetE('txtWidth').value	= iWidth ? iWidth : GetAttribute( oImage, "width", '' ) ;
	GetE('txtHeight').value	= iHeight ? iHeight : GetAttribute( oImage, "height", '' ) ;

	//CheckIsZoom();
	UpdatePreview() ;
}

//#### The OK button was hit.
function Ok()
{
	if ( GetE('txtUrl').value.length == 0 )
	{
		dialog.SetSelectedTab( 'Info' ) ;
		GetE('txtUrl').focus() ;

		alert( FCKLang.DlgImgAlertUrl ) ;

		return false ;
	}

	var bHasImage = ( oImage != null ) ;

	if ( bHasImage && bImageButton && oImage.tagName == 'IMG' )
	{
		if ( confirm( 'Do you want to transform the selected image on a image button?' ) )
			oImage = null ;
	}
	else if ( bHasImage && !bImageButton && oImage.tagName == 'INPUT' )
	{
		if ( confirm( 'Do you want to transform the selected image button on a simple image?' ) )
			oImage = null ;
	}

	oEditor.FCKUndo.SaveUndoStep() ;
	if ( !bHasImage )
	{
		if ( bImageButton )
		{
			oImage = FCK.EditorDocument.createElement( 'input' ) ;
			oImage.type = 'image' ;
			oImage = FCK.InsertElement( oImage ) ;
		}
		else
			oImage = FCK.InsertElement( 'img' ) ;
	}

	UpdateImage( oImage ) ;

	
	return true ;
}

function UpdateImage( e, skipId )
{
	e.src = GetE('txtUrl').value ;
	SetAttribute( e, "_fcksavedurl", GetE('txtUrl').value ) ;
	SetAttribute( e, "width" , GetE('txtWidth').value ) ;
	SetAttribute( e, "height", GetE('txtHeight').value ) ;
	SetAttribute( e, "alt"   , GetE('txtAlt').value ) ;
	SetAttribute( e, "_zoom"   , 1 ) ; //Set Zoom for any images
	
	
	if(!bImageButton){
		SetAttribute( e, "border", '0') ;
	}
	
}

var eImgPreview ;

function SetPreviewElements( imageElement )
{
	eImgPreview = imageElement ;
	
	UpdatePreview() ;
	UpdateOriginal() ;

	bPreviewInitialized = true ;
}

function UpdatePreview()
{
	if ( !eImgPreview )
		return ;

	if ( GetE('txtUrl').value.length != 0 )
	{
		UpdateImage( eImgPreview, true ) ;
	}
}

var bLockRatio = true ;

function SwitchLock( lockButton )
{
	bLockRatio = !bLockRatio ;
	lockButton.className = bLockRatio ? 'BtnLocked' : 'BtnUnlocked' ;
	lockButton.title = bLockRatio ? FCKLang.DlgImgLockRatio : FCKLang.DlgBtnResetSize ;

	if ( bLockRatio )
	{
		if ( GetE('txtWidth').value.length > 0 )
			OnSizeChanged( 'Width', GetE('txtWidth').value ) ;
		else
			OnSizeChanged( 'Height', GetE('txtHeight').value ) ;
	}
}

// Fired when the width or height input texts change
function OnSizeChanged( dimension, value )
{
	// Verifies if the aspect ration has to be maintained
	if ( oImageOriginal && bLockRatio )
	{
		var e = dimension == 'Width' ? GetE('txtHeight') : GetE('txtWidth') ;

		if ( value.length == 0 || isNaN( value ) )
		{
			e.value = '' ;
			return ;
		}

		if ( dimension == 'Width' )
			value = value == 0 ? 0 : Math.round( oImageOriginal.height * ( value  / oImageOriginal.width ) ) ;
		else
			value = value == 0 ? 0 : Math.round( oImageOriginal.width  * ( value / oImageOriginal.height ) ) ;

		if ( !isNaN( value ) )
			e.value = value ;
	}

	//CheckIsZoom();
	UpdatePreview() ;
}

// Fired when the Reset Size button is clicked
function ResetSizes()
{
    if ( ! oImageOriginal ) return ;
	if ( oEditor.FCKBrowserInfo.IsGecko && !oImageOriginal.complete )
	{
		setTimeout( ResetSizes, 50 ) ;
		return ;
	}

	GetE('txtWidth').value  = oImageOriginal.width ;
	GetE('txtHeight').value = oImageOriginal.height ;

	CheckIsZoom();
	UpdatePreview() ;
}


function SetUrl( url, width, height, alt )
{


		GetE('txtUrl').value = url ;
		GetE('txtWidth').value = width ? width : '' ;
		GetE('txtHeight').value = height ? height : '' ;

			
		if ( alt )
			GetE('txtAlt').value = alt;

        dialog.document.getElementById('TabsRow').style.display = 'none';
        dialog.document.getElementById('innerContents').style.display = 'none';
        dialog.document.getElementById('PopupButtons').style.display = 'none';
        

		UpdatePreview() ;
		
		UpdateOriginal( true ) ;
		//CheckIsZoom();

	dialog.SetSelectedTab( 'Info' ) ;
}

function OnUploadCompleted( errorNumber, fileUrl, fileName, customMsg )
{
	// Remove animation
	if(errorNumber != 0 && errorNumber != 201)
	{
	    window.parent.Throbber.Hide() ;
	    GetE( 'divUpload' ).style.display  = '' ;
    }
	switch ( errorNumber )
	{
		case 0 :	// No errors
			//alert( 'Your file has been successfully uploaded' ) ;
			break ;
		case 1 :	// Custom error
			alert( customMsg ) ;
			return ;
		case 101 :	// Custom warning
			alert( customMsg ) ;
			break ;
		case 201 :
			//alert( 'A file with the same name is already available. The uploaded file has been renamed to "' + fileName + '"' ) ;
			break ;
		case 202 :
			alert( 'Invalid file type' ) ;
			return ;
		case 203 :
			alert( "Security error. You probably don't have enough permissions to upload. Please check your server." ) ;
			return ;
		case 500 :
			alert( 'The connector is disabled' ) ;
			break ;
		default :
			alert( 'Error on file upload. Error number: ' + errorNumber ) ;
			return ;
	}

	sActualBrowser = '' ;
	SetUrl( fileUrl ) ;
	GetE('frmUpload').reset() ;
}

function CheckUpload()
{
	var sFile = GetE('txtUploadFile').value ;

	if ( sFile.length == 0 )
	{
		alert( 'Please select a file to upload' ) ;
		return false ;
	}

	if ( ( FCKConfig.ImageUploadAllowedExtensions.length > 0 && !new RegExp( FCKConfig.ImageUploadAllowedExtensions, 'i' ).test( sFile ) ) ||
		( FCKConfig.ImageUploadDeniedExtensions.length > 0 && new RegExp( FCKConfig.ImageUploadDeniedExtensions, 'i' ).test( sFile ) ) )
	{
		OnUploadCompleted( 202 ) ;
		return false ;
	}

	// Show animation
	window.parent.Throbber.Show( 100 ) ;
	GetE( 'divUpload' ).style.display  = 'none' ;

	return true ;
}
