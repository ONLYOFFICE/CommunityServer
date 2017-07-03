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

	/// <summary>
	/// A creator of Token object instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class and it's sub-classes exists primarily as an optimization
	/// of the reflection-based mechanism(s) previously used exclusively to 
	/// create instances of Token objects.
	/// </para>
	/// <para>
	/// Since Lexers in ANTLR use a single Token type, each TokenCreator can 
	/// create one class of Token objects (that's why it's not called TokenFactory).
	/// </para>
	/// </remarks>
	public abstract class TokenCreator
	{
		/// <summary>
		/// Returns the fully qualified name of the Token type that this
		/// class creates.
		/// </summary>
		public abstract string TokenTypeName
		{
			get;
		}

		/// <summary>
		/// Constructs a <see cref="Token"/> instance.
		/// </summary>
		public abstract IToken Create();
	}
}