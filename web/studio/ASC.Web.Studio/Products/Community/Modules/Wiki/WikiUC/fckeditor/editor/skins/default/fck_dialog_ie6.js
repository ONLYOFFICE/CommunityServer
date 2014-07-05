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
 */

(function()
{
	// IE6 doens't handle absolute positioning properly (it is always in quirks
	// mode). This function fixes the sizes and positions of many elements that
	// compose the skin (this is skin specific).
	var fixSizes = window.DoResizeFixes = function()
	{
		var fckDlg = window.document.body ;

		for ( var i = 0 ; i < fckDlg.childNodes.length ; i++ )
		{
			var child = fckDlg.childNodes[i] ;
			switch ( child.className )
			{
				case 'contents' :
					child.style.width = Math.max( 0, fckDlg.offsetWidth - 16 - 16 ) ;	// -left -right
					child.style.height = Math.max( 0, fckDlg.clientHeight - 20 - 2 ) ;	// -bottom -top
					break ;

				case 'blocker' :
				case 'cover' :
					child.style.width = Math.max( 0, fckDlg.offsetWidth - 16 - 16 + 4 ) ;	// -left -right + 4
					child.style.height = Math.max( 0, fckDlg.clientHeight - 20 - 2 + 4 ) ;	// -bottom -top + 4
					break ;

				case 'tr' :
					child.style.left = Math.max( 0, fckDlg.clientWidth - 16 ) ;
					break ;

				case 'tc' :
					child.style.width = Math.max( 0, fckDlg.clientWidth - 16 - 16 ) ;
					break ;

				case 'ml' :
					child.style.height = Math.max( 0, fckDlg.clientHeight - 16 - 51 ) ;
					break ;

				case 'mr' :
					child.style.left = Math.max( 0, fckDlg.clientWidth - 16 ) ;
					child.style.height = Math.max( 0, fckDlg.clientHeight - 16 - 51 ) ;
					break ;

				case 'bl' :
					child.style.top = Math.max( 0, fckDlg.clientHeight - 51 ) ;
					break ;

				case 'br' :
					child.style.left = Math.max( 0, fckDlg.clientWidth - 30 ) ;
					child.style.top = Math.max( 0, fckDlg.clientHeight - 51 ) ;
					break ;

				case 'bc' :
					child.style.width = Math.max( 0, fckDlg.clientWidth - 30 - 30 ) ;
					child.style.top = Math.max( 0, fckDlg.clientHeight - 51 ) ;
					break ;
			}
		}
	}

	var closeButtonOver = function()
	{
		this.style.backgroundPosition = '-16px -687px' ;
	} ;

	var closeButtonOut = function()
	{
		this.style.backgroundPosition = '-16px -651px' ;
	} ;

	var fixCloseButton = function()
	{
		var closeButton = document.getElementById ( 'closeButton' ) ;

		closeButton.onmouseover	= closeButtonOver ;
		closeButton.onmouseout	= closeButtonOut ;
	}

	var onLoad = function()
	{
		fixSizes() ;
		fixCloseButton() ;

		window.attachEvent( 'onresize', fixSizes ) ;
		window.detachEvent( 'onload', onLoad ) ;
	}

	window.attachEvent( 'onload', onLoad ) ;

})() ;
