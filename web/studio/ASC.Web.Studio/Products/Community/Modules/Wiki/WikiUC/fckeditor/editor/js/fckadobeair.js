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
 * Compatibility code for Adobe AIR.
 */

if ( FCKBrowserInfo.IsAIR )
{
	var FCKAdobeAIR = (function()
	{
		/*
		 * ### Private functions.
		 */

		var getDocumentHead = function( doc )
		{
			var head ;
			var heads = doc.getElementsByTagName( 'head' ) ;

			if( heads && heads[0] )
				head = heads[0] ;
			else
			{
				head = doc.createElement( 'head' ) ;
				doc.documentElement.insertBefore( head, doc.documentElement.firstChild ) ;
			}

			return head ;
		} ;

		/*
		 * ### Public interface.
		 */
		return {
			FCKeditorAPI_Evaluate : function( parentWindow, script )
			{
				// TODO : This one doesn't work always. The parent window will
				// point to an anonymous function in this window. If this
				// window is destroyied the parent window will be pointing to
				// an invalid reference.

				// Evaluate the script in this window.
				eval( script ) ;

				// Point the FCKeditorAPI property of the parent window to the
				// local reference.
				parentWindow.FCKeditorAPI = window.FCKeditorAPI ;
			},

			EditingArea_Start : function( doc, html )
			{
				// Get the HTML for the <head>.
				var headInnerHtml = html.match( /<head>([\s\S]*)<\/head>/i )[1] ;

				if ( headInnerHtml && headInnerHtml.length > 0 )
				{
					// Inject the <head> HTML inside a <div>.
					// Do that before getDocumentHead because WebKit moves
					// <link css> elements to the <head> at this point.
					var div = doc.createElement( 'div' ) ;
					div.innerHTML = headInnerHtml ;

					// Move the <div> nodes to <head>.
					FCKDomTools.MoveChildren( div, getDocumentHead( doc ) ) ;
				}

				doc.body.innerHTML = html.match( /<body>([\s\S]*)<\/body>/i )[1] ;

				//prevent clicking on hyperlinks and navigating away
				doc.addEventListener('click', function( ev )
					{
						ev.preventDefault() ;
						ev.stopPropagation() ;
					}, true ) ;
			},

			Panel_Contructor : function( doc, baseLocation )
			{
				var head = getDocumentHead( doc ) ;

				// Set the <base> href.
				head.appendChild( doc.createElement('base') ).href = baseLocation ;

				doc.body.style.margin	= '0px' ;
				doc.body.style.padding	= '0px' ;
			},

			ToolbarSet_GetOutElement : function( win, outMatch )
			{
				var toolbarTarget = win.parent ;

				var targetWindowParts = outMatch[1].split( '.' ) ;
				while ( targetWindowParts.length > 0 )
				{
					var part = targetWindowParts.shift() ;
					if ( part.length > 0 )
						toolbarTarget = toolbarTarget[ part ] ;
				}

				toolbarTarget = toolbarTarget.document.getElementById( outMatch[2] ) ;
			},

			ToolbarSet_InitOutFrame : function( doc )
			{
				var head = getDocumentHead( doc ) ;

				head.appendChild( doc.createElement('base') ).href = window.document.location ;

				var targetWindow = doc.defaultView;

				targetWindow.adjust = function()
				{
					targetWindow.frameElement.height = doc.body.scrollHeight;
				} ;

				targetWindow.onresize = targetWindow.adjust ;
				targetWindow.setTimeout( targetWindow.adjust, 0 ) ;

				doc.body.style.overflow = 'hidden';
				doc.body.innerHTML = document.getElementById( 'xToolbarSpace' ).innerHTML ;
			}
		} ;
	})();

	/*
	 * ### Overrides
	 */
	( function()
	{
		// Save references for override reuse.
		var _Original_FCKPanel_Window_OnFocus	= FCKPanel_Window_OnFocus ;
		var _Original_FCKPanel_Window_OnBlur	= FCKPanel_Window_OnBlur ;
		var _Original_FCK_StartEditor			= FCK.StartEditor ;

		FCKPanel_Window_OnFocus = function( e, panel )
		{
			// Call the original implementation.
			_Original_FCKPanel_Window_OnFocus.call( this, e, panel ) ;

			if ( panel._focusTimer )
				clearTimeout( panel._focusTimer ) ;
		}

		FCKPanel_Window_OnBlur = function( e, panel )
		{
			// Delay the execution of the original function.
			panel._focusTimer = FCKTools.SetTimeout( _Original_FCKPanel_Window_OnBlur, 100, this, [ e, panel ] ) ;
		}

		FCK.StartEditor = function()
		{
			// Force pointing to the CSS files instead of using the inline CSS cached styles.
			window.FCK_InternalCSS			= FCKConfig.BasePath + 'css/fck_internal.css' ;
			window.FCK_ShowTableBordersCSS	= FCKConfig.BasePath + 'css/fck_showtableborders_gecko.css' ;

			_Original_FCK_StartEditor.apply( this, arguments ) ;
		}
	})();
}
