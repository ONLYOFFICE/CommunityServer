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
