
AscUserCommand = new FCKDialogCommand( 'AscUser', FCKLang.AscUserDlgTitle, FCKPlugins.Items['ascuser'].Path + 'ascuser.aspx', 500, 374 );
 FCKCommands.RegisterCommand( 'AscUser',  AscUserCommand) ;

// Create the "Plaholder" toolbar button.
var oAscUserItem = new FCKToolbarButton( 'AscUser', FCKLang.AscUserBtn ) ;
oAscUserItem.IconPath = FCKPlugins.Items['ascuser'].Path + 'ascuser.png' ;

FCKToolbarItems.RegisterItem( 'AscUser', oAscUserItem ) ;

AscUserCommand.GetState = function()
		{
			return FCK_TRISTATE_OFF;
		}
		



