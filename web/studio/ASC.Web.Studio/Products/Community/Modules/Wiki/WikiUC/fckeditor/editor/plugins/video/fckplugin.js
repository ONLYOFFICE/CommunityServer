

FCKCommands.RegisterCommand( 'Video', new FCKDialogCommand( 'Video', FCKLang.VideoDlgTitle, FCKPlugins.Items['video'].Path + 'fck_flv.html', 541, 524 ) ) ;

// Create the "Plaholder" toolbar button.
var oVideoItem = new FCKToolbarButton( 'Video', FCKLang.VideoBtn ) ;
oVideoItem.IconPath = FCKPlugins.Items['video'].Path + 'video.png' ;

FCKToolbarItems.RegisterItem( 'Video', oVideoItem ) ;

FCK.ContextMenu.RegisterListener( {
        AddItems : function( menu, tag, tagName )
        {
                // under what circumstances do we display this option
                if ( tagName == 'IMG' && /*tag._fckflv*/tag.className == 'FCK__Flv'  )
                {
                        // when the option is displayed, show a separator  the command
                        menu.AddSeparator() ;
                        // the command needs the registered command name, the title for the context menu, and the icon path
                        menu.AddItem( 'Video', FCKLang.VideoDlgTitle, oVideoItem.IconPath ) ;
                }
        }}
);




