/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

/**
 * Copyright (c) 2003-2014, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

// Tool scripts for the sample pages.
// This file can be ignored and is not required to make use of CKEditor.

( function() {
	CKEDITOR.on( 'instanceReady', function( ev ) {
		// Check for sample compliance.
		var editor = ev.editor,
			meta = CKEDITOR.document.$.getElementsByName( 'ckeditor-sample-required-plugins' ),
			requires = meta.length ? CKEDITOR.dom.element.get( meta[ 0 ] ).getAttribute( 'content' ).split( ',' ) : [],
			missing = [],
			i;

		if ( requires.length ) {
			for ( i = 0; i < requires.length; i++ ) {
				if ( !editor.plugins[ requires[ i ] ] )
					missing.push( '<code>' + requires[ i ] + '</code>' );
			}

			if ( missing.length ) {
				var warn = CKEDITOR.dom.element.createFromHtml(
					'<div class="warning">' +
						'<span>To fully experience this demo, the ' + missing.join( ', ' ) + ' plugin' + ( missing.length > 1 ? 's are' : ' is' ) + ' required.</span>' +
					'</div>'
				);
				warn.insertBefore( editor.container );
			}
		}

		// Set icons.
		var doc = new CKEDITOR.dom.document( document ),
			icons = doc.find( '.button_icon' );

		for ( i = 0; i < icons.count(); i++ ) {
			var icon = icons.getItem( i ),
				name = icon.getAttribute( 'data-icon' ),
				style = CKEDITOR.skin.getIconStyle( name, ( CKEDITOR.lang.dir == 'rtl' ) );

			icon.addClass( 'cke_button_icon' );
			icon.addClass( 'cke_button__' + name + '_icon' );
			icon.setAttribute( 'style', style );
			icon.setStyle( 'float', 'none' );

		}
	} );
} )();
