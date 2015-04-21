(function() {

	// Add to collection with DUP examination.
	// @param {Object} collection
	// @param {Object} element
	// @param {Object} database
	function addSafely( collection, element, database ) {
		// 1. IE doesn't support customData on text nodes;
		// 2. Text nodes never get chance to appear twice;
		if ( !element.is || !element.getCustomData( 'block_processed' ) ) {
			element.is && CKEDITOR.dom.element.setMarker( database, element, 'block_processed', true );
			collection.push( element );
		}
	}

    // Dialog reused by 'createteamlabcut' command.
	// @param {Object} editor
	function teamlabcutDialog(editor) {

	    // Definition of elements at which teamlabcut operation should stopped.
		var divLimitDefinition = (function() {

			// Customzie from specialize blockLimit elements
			var definition = CKEDITOR.tools.extend( {}, CKEDITOR.dtd.$blockLimit );

			if (editor.config.teamlabcut_wrapTable) {
				delete definition.td;
				delete definition.th;
			}
			return definition;
		})();

		// DTD of 'div' element
		var dtd = CKEDITOR.dtd.div;

		// Get the first div limit element on the element's path.
		// @param {Object} element
		function getDivContainer( element ) {
			var container = editor.elementPath( element ).blockLimit;

		    // Dont stop at 'td' and 'th' when teamlabcut should wrap entire table.
			if (editor.config.teamlabcut_wrapTable && container.is(['td', 'th'])) {
				var parentPath = editor.elementPath( container.getParent() );
				container = parentPath.blockLimit;
			}

			return container;
		}

		// Wrapping 'div' element around appropriate blocks among the selected ranges.
		// @param {Object} editor
		function createTeamlabCut(editor) {
			// new adding containers OR detected pre-existed containers.
			var containers = [];
			// node markers store.
			var database = {};
			// All block level elements which contained by the ranges.
			var containedBlocks = [],
				block;

			// Get all ranges from the selection.
			var selection = editor.getSelection(),
				ranges = selection.getRanges();
			var bookmarks = selection.createBookmarks();
			var i, iterator;

			// Calcualte a default block tag if we need to create blocks.
			var blockTag = editor.config.enterMode == CKEDITOR.ENTER_DIV ? 'div' : 'p';

			// collect all included elements from dom-iterator
			for ( i = 0; i < ranges.length; i++ ) {
				iterator = ranges[ i ].createIterator();
				while ( ( block = iterator.getNextParagraph() ) ) {
					// include contents of blockLimit elements.
					if ( block.getName() in divLimitDefinition ) {
						var j,
							childNodes = block.getChildren();
						for ( j = 0; j < childNodes.count(); j++ )
							addSafely( containedBlocks, childNodes.getItem( j ), database );
					} else {
						while ( !dtd[ block.getName() ] && !block.equals( ranges[ i ].root ) )
							block = block.getParent();
						addSafely( containedBlocks, block, database );
					}
				}
			}

			CKEDITOR.dom.element.clearAllMarkers( database );

			var blockGroups = groupByDivLimit( containedBlocks );
			var ancestor, blockEl, divElement;

			for ( i = 0; i < blockGroups.length; i++ ) {
				var currentNode = blockGroups[ i ][ 0 ];

				// Calculate the common parent node of all contained elements.
				ancestor = currentNode.getParent();
				for ( j = 1; j < blockGroups[ i ].length; j++ )
					ancestor = ancestor.getCommonAncestor( blockGroups[ i ][ j ] );

				divElement = new CKEDITOR.dom.element('div', editor.document);
			    divElement.addClass("asccut");

				// Normalize the blocks in each group to a common parent.
				for ( j = 0; j < blockGroups[ i ].length; j++ ) {
					currentNode = blockGroups[ i ][ j ];

					while ( !currentNode.getParent().equals( ancestor ) )
						currentNode = currentNode.getParent();

					// This could introduce some duplicated elements in array.
					blockGroups[ i ][ j ] = currentNode;
				}

				// Wrapped blocks counting
				var fixedBlock = null;
				for ( j = 0; j < blockGroups[ i ].length; j++ ) {
					currentNode = blockGroups[ i ][ j ];

					// Avoid DUP elements introduced by grouping.
					if ( !( currentNode.getCustomData && currentNode.getCustomData( 'block_processed' ) ) ) {
						currentNode.is && CKEDITOR.dom.element.setMarker( database, currentNode, 'block_processed', true );

						// Establish new container, wrapping all elements in this group.
						if ( !j )
							divElement.insertBefore( currentNode );

						divElement.append(currentNode);
					}
				}

				CKEDITOR.dom.element.clearAllMarkers( database );
				containers.push( divElement );
			}

			selection.selectBookmarks( bookmarks );
			return containers;
		}

		// Divide a set of nodes to different groups by their path's blocklimit element.
		// Note: the specified nodes should be in source order naturally, which mean they are supposed to producea by following class:
		//  * CKEDITOR.dom.range.Iterator
		//  * CKEDITOR.dom.domWalker
		// @returns {Array[]} the grouped nodes
		function groupByDivLimit( nodes ) {
			var groups = [],
				lastDivLimit = null,
				path, block;
			for ( var i = 0; i < nodes.length; i++ ) {
				block = nodes[ i ];
				var limit = getDivContainer( block );
				if ( !limit.equals( lastDivLimit ) ) {
					lastDivLimit = limit;
					groups.push( [] );
				}
				groups[ groups.length - 1 ].push( block );
			}
			return groups;
		}

	    // @type teamlabcutDialog
		return {
		    title: editor.lang.teamlabcut.dialogTitle,
			minWidth: 400,
			minHeight: 100,
			contents: [
				{
				id: 'info',
				elements: [
					{
					    id: 'infoText',
					    type: 'html',
					    html: '<div style="width: 400px;white-space: normal;">' + editor.lang.teamlabcut.dialogInfo + '<br/><br/>' + editor.lang.teamlabcut.dialogRemoveInfo + '</div>'
					}
				]
			}
			],
			onOk: function() {
			    createTeamlabCut(editor);
				this.hide();
			}
		};
	}

	CKEDITOR.dialog.add('createteamlabcut', function (editor) {
	    return teamlabcutDialog(editor);
	});
})();