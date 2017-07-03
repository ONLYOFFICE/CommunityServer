namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/license.html
	*
	* $Id:$
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/// <summary>
	/// A token is minimally a token type.  Subclasses can add the text matched
	/// for the token and line info. 
	/// </summary>
	public interface IToken
	{
		int		getColumn();
		void	setColumn(int c);

		int		getLine();
		void	setLine(int l);

		string	getFilename();
		void	setFilename(string name);

		string	getText();
		void	setText(string t);

		int		Type { get; set; }
	}
}