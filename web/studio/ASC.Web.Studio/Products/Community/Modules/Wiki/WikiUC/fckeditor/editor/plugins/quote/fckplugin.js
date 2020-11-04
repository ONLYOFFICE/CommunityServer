
QuoteCommand = new FCKDialogCommand( 'Quote', FCKLang.QuoteDlgTitle, FCKPlugins.Items['quote'].Path + 'fck_quote.html', 500, 358 );
 FCKCommands.RegisterCommand( 'Quote',  QuoteCommand) ;

// Create the "Plaholder" toolbar button.
var oQuoteItem = new FCKToolbarButton( 'Quote', FCKLang.QuoteBtn ) ;
oQuoteItem.IconPath = FCKPlugins.Items['quote'].Path + 'quote.png' ;

FCKToolbarItems.RegisterItem( 'Quote', oQuoteItem ) ;

QuoteCommand.GetState = function()
		{
			return FCK_TRISTATE_OFF;
		}
		



