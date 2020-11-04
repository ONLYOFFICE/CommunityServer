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
 * Plugin to insert "Placeholders" in the editor.
 */

// Register the related command.]

// Register the related commands.

FCKCommands.RegisterCommand( 'Attach', new FCKDialogCommand( 'Attach', FCKLang.AttachDlgTitle, FCKPlugins.Items['attach'].Path + 'fck_attach.html', 450, 216 ) ) ;

// Create the "Plaholder" toolbar button.
var oAttachItem = new FCKToolbarButton( 'Attach', FCKLang.AttachBtn ) ;
oAttachItem.IconPath = FCKPlugins.Items['attach'].Path + 'attach.png' ;

FCKToolbarItems.RegisterItem( 'Attach', oAttachItem ) ;

FCK.ContextMenu.RegisterListener( {
        AddItems : function( menu, tag, tagName )
        {
                // under what circumstances do we display this option
				if(tag == null && FCKSelection.HasAncestorNode('A'))
				{
					tag = FCKSelection.MoveToAncestorNode('A')
					tagName = tag.tagName;
				}
                if (tagName == 'A' && tag.getAttribute( '_fckAttach' )  )
                {
                        // when the option is displayed, show a separator  the command
                        menu.AddSeparator() ;
                        // the command needs the registered command name, the title for the context menu, and the icon path
                        menu.AddItem( 'Attach', FCKLang.AttachDlgTitle, oAttachItem.IconPath ) ;
                }
        }}
);




