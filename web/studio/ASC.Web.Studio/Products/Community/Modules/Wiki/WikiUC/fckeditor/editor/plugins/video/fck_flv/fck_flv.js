

var dialog		= window.parent ;
var oEditor		= dialog.InnerDialogLoaded() ;
var FCK			= oEditor.FCK ;
var FCKLang		= oEditor.FCKLang ;
var FCKConfig	= oEditor.FCKConfig ;
var FCKTools	= oEditor.FCKTools ;
var FCKPlugins	= oEditor.FCKPlugins ;

//#### Dialog Tabs

// Set the dialog tabs.
dialog.AddTab( 'Info', oEditor.FCKLang.DlgInfoTab ) ;

if ( FCKConfig.FlashUpload ) {
	//dialog.AddTab( 'Upload', FCKLang.DlgLnkUpload ) ;
	dialog.AddTab( 'YouTube', "YouTube" ) ;
}
	

// Function called when a dialog tag is selected.
function OnDialogTabChange( tabCode )
{
	ShowE('divInfo'		, ( tabCode == 'Info' ) ) ;
	//ShowE('divUpload'	, ( tabCode == 'Upload' ) ) ;
	ShowE('divYouTube'	, ( tabCode == 'YouTube' ) ) ;	
}

// Get the selected flash embed (if available).
var oFakeImage = dialog.Selection.GetSelectedElement() ;
var oEmbed ;

if ( oFakeImage )
{
	if ( oFakeImage.tagName == 'IMG' && oFakeImage.getAttribute('_fckflv') )
		oEmbed = FCK.GetRealElement( oFakeImage ) ;
	else
		oFakeImage = null ;
}

window.onload = function()
{
	// Translate the dialog box texts.
	oEditor.FCKLanguageManager.TranslatePage(document) ;

	// Load the selected element information (if any).
	LoadSelection() ;

	// Show/Hide the "Browse Server" button.
	
	// Set the actual uploader URL.
	//if ( FCKConfig.FlashUpload )
	//	GetE('frmUpload').action = FCKConfig.FlashUploadURL ;

	dialog.SetAutoSize( true ) ;

	// Activate the "OK" button.
	dialog.SetOkButton( true ) ;

	SelectField( 'txtUrl' ) ;
}

function LoadSelection()
{
	if ( ! oEmbed ) return ;

	//GetE('txtUrl').value    = GetAttribute( oEmbed, 'src', '' ) ;
	
	var flashvars = GetAttribute( oEmbed, 'src', '' ).split('?');
	
	if(flashvars.length < 2)
		return;
	
	flashvars = flashvars[1].split('&')
	
	for(var i = 0; i < flashvars.length; i++)
	{
		if(flashvars[i].split('=')[0] == 'movie')
		{
			GetE('txtUrl').value = flashvars[i].split('=')[1];
			break;
		}
	}
	
	/*GetE('txtWidth').value  = GetAttribute( oEmbed, 'width', '' ) ;
	GetE('txtHeight').value = GetAttribute( oEmbed, 'height', '' ) ;*/

	
	UpdatePreview() ;
}

//#### The OK button was hit.
function Ok()
{	
	if(GetE('divYouTube').style.display != 'none' && GetE('txtYouTubeUrl').value.length != 0) {
		
		GetE('txtUrl').value = GetE('txtYouTubeUrlHidden').value;
		var youtubeVideoEl = FCK.EditorDocument.createElement( 'div' ) ;
		youtubeVideoEl.innerHTML = GetE('youTubePreviewDiv').innerHTML;
		
		if ( !oFakeImage )
		{
			oFakeImage	= oEditor.FCKDocumentProcessor_CreateFakeImage( 'FCK__Flv', youtubeVideoEl ) ;
			oFakeImage.setAttribute( '_fckflv', 'true', 0 ) ;
			oFakeImage	= FCK.InsertElement( oFakeImage ) ;
		}

		oEditor.FCKEmbedAndObjectProcessor.RefreshView( oFakeImage, youtubeVideoEl ) ;

		return true ;
	}
	
	if ( GetE('txtUrl').value.length == 0 )
	{
		dialog.SetSelectedTab( 'Info' ) ;
		GetE('txtUrl').focus() ;

		alert( oEditor.FCKLang.DlgAlertUrl ) ;

		return false ;
	}

	oEditor.FCKUndo.SaveUndoStep() ;
	if ( !oEmbed )
	{
		oEmbed		= FCK.EditorDocument.createElement( 'EMBED' ) ;
		oFakeImage  = null ;
	}
	UpdateEmbed( oEmbed ) ;

	if ( !oFakeImage )
	{
		oFakeImage	= oEditor.FCKDocumentProcessor_CreateFakeImage( 'FCK__Flv', oEmbed ) ;
		oFakeImage.setAttribute( '_fckflv', 'true', 0 ) ;
		oFakeImage	= FCK.InsertElement( oFakeImage ) ;
	}

	oEditor.FCKEmbedAndObjectProcessor.RefreshView( oFakeImage, oEmbed ) ;

	return true ;
}

function UpdateEmbed( e, preview )
{
	
		
	SetAttribute( e, 'id', 'player' ) ;
	
	if(preview){
		SetAttribute( e, 'height', '100%' ) ;
		SetAttribute( e, 'width' , '100%' ) ;
	}
	else {
		SetAttribute( e, 'height', 370 ) ;
		SetAttribute( e, 'width' , 450 ) ;
	}
	

	SetAttribute( e, 'allowScriptAccess','always');
	SetAttribute( e, 'allowFullScreen','true');
	SetAttribute( e, 'quality','high');
	SetAttribute( e, 'bgcolor','#0');
	SetAttribute( e, 'name','player');
	SetAttribute( e, 'style','');
		
		
	SetAttribute( e, 'src', FCKPlugins.Items['video'].Path + 'avs4you_player.swf?movie=' + encodeURIComponent(GetE('txtUrl').value));
	SetAttribute( e, 'type'			, 'application/x-shockwave-flash' ) ;
	
	
	SetAttribute( e, 'pluginspage'	, 'http://www.adobe.com/go/getflashplayer' ) ;

}

var ePreview ;

function SetPreviewElement( previewEl )
{
	ePreview = previewEl ;

	if ( GetE('txtUrl').value.length > 0 )
		UpdatePreview() ;
}

function UpdatePreview()
{
	if ( !ePreview )
		return ;

	while ( ePreview.firstChild )
		ePreview.removeChild( ePreview.firstChild ) ;

	if ( GetE('txtUrl').value.length == 0 )
		ePreview.innerHTML = '&nbsp;' ;
	else
	{
		var oDoc	= ePreview.ownerDocument || ePreview.document ;
		var e		= oDoc.createElement( 'EMBED' ) ;

		UpdateEmbed(e, true);
		

		ePreview.appendChild( e ) ;
	}
}



function SetUrl( url )
{
	GetE('txtUrl').value = url ;

	
	UpdatePreview() ;

	dialog.SetSelectedTab( 'Info' ) ;
}

function OnUploadCompleted( errorNumber, fileUrl, fileName, customMsg )
{
	// Remove animation
	window.parent.Throbber.Hide() ;
	GetE( 'divUpload' ).style.display  = '' ;

	switch ( errorNumber )
	{
		case 0 :	// No errors
			alert( 'Your file has been successfully uploaded' ) ;
			break ;
		case 1 :	// Custom error
			alert( customMsg ) ;
			return ;
		case 101 :	// Custom warning
			alert( customMsg ) ;
			break ;
		case 201 :
			alert( 'A file with the same name is already available. The uploaded file has been renamed to "' + fileName + '"' ) ;
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

	SetUrl( fileUrl ) ;
	GetE('frmUpload').reset() ;
}

var oUploadAllowedExtRegex	= new RegExp( ".(flv)$", 'i' ) ;
var oUploadDeniedExtRegex	= new RegExp( "", 'i' ) ;

function CheckUpload()
{
	var sFile = GetE('txtUploadFile').value ;

	if ( sFile.length == 0 )
	{
		alert( 'Please select a file to upload' ) ;
		return false ;
	}

	if ( ( FCKConfig.FlashUploadAllowedExtensions.length > 0 && !oUploadAllowedExtRegex.test( sFile ) ) ||
		( FCKConfig.FlashUploadDeniedExtensions.length > 0 && oUploadDeniedExtRegex.test( sFile ) ) )
	{
		OnUploadCompleted( 202 ) ;
		return false ;
	}

	// Show animation
	window.parent.Throbber.Show( 100 ) ;
	GetE( 'divUpload' ).style.display  = 'none' ;

	return true ;
}
