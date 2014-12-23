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

(function() {
    CKEDITOR.plugins.add('teamlabcut', {
		requires: 'dialog',
		lang: 'de,en,es,fr,it,ru',
		icons: 'teamlabcut',
		init: function( editor ) {
			if ( editor.blockless )
				return;

			var lang = editor.lang.teamlabcut;
			var allowed = 'div(*)';

			editor.addCommand('createteamlabcut', new CKEDITOR.dialogCommand('createteamlabcut', {
				allowedContent: allowed,
				requiredContent: 'div',
				contextSensitive: true,
				refresh: function( editor, path ) {
				    var context = editor.config.teamlabcut_wrapTable ? path.root : path.blockLimit;
				    var rangesCount = editor.getSelection().getRanges().length;

                    this.setState(('div' in context.getDtd()) && rangesCount == 1 ? CKEDITOR.TRISTATE_OFF : CKEDITOR.TRISTATE_DISABLED);
				}
			}));

			editor.addCommand('removeteamlabcut', {
				requiredContent: 'div',
				exec: function( editor ) {
					var selection = editor.getSelection(),
						ranges = selection && selection.getRanges(),
						range,
						bookmarks = selection.createBookmarks(),
						walker,
						toRemove = [];

					function findDiv( node ) {
					    var div = CKEDITOR.plugins.teamlabcut.getSurroundDiv(editor, node);
						if ( div && !div.data( 'cke-div-added' ) ) {
							toRemove.push( div );
							div.data( 'cke-div-added' );
						}
					}

					for ( var i = 0; i < ranges.length; i++ ) {
						range = ranges[ i ];
						if ( range.collapsed )
							findDiv( selection.getStartElement() );
						else {
							walker = new CKEDITOR.dom.walker( range );
							walker.evaluator = findDiv;
							walker.lastForward();
						}
					}

					for ( i = 0; i < toRemove.length; i++ )
						toRemove[ i ].remove( true );

					selection.selectBookmarks( bookmarks );
				}
			});

			editor.ui.addButton && editor.ui.addButton('TeamlabCut', {
			    label: lang.addButtonTitle,
				command: 'createteamlabcut',
				toolbar: 'paragraph'
			});

			if (editor.contextMenu) {
			    editor.addMenuGroup("teamlabcut");

			    editor.addMenuItems({
				    removeteamlabcut: {
				        label: lang.removeButtonTitle,
						command: 'removeteamlabcut',
						group: 'teamlabcut'
					}
				});

				editor.contextMenu.addListener( function( element ) {
					if ( !element || element.isReadOnly() )
						return null;

					if (CKEDITOR.plugins.teamlabcut.getSurroundDiv(editor)) {
						return {
							removeteamlabcut: CKEDITOR.TRISTATE_OFF
						};
					}

					return null;
				});
			}

			CKEDITOR.dialog.add('createteamlabcut', this.path + 'dialogs/teamlabcut.js');
		}
	});

    CKEDITOR.plugins.teamlabcut = {
		getSurroundDiv: function( editor, start ) {
			var path = editor.elementPath( start );
			var div = editor.elementPath(path.blockLimit).contains('div', 1);
			if (div && div.hasClass("asccut")) {
			    return div;
			}
			return null;
		}
	};
})();
