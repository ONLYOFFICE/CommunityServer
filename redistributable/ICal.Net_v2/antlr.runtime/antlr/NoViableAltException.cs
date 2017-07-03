using antlr.collections;

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

    public class NoViableAltException : RecognitionException
    {
        public IToken token;
        public AST node; // handles parsing and treeparsing

        public NoViableAltException(IToken t, string fileName_) :
            base("NoViableAlt", fileName_, t.getLine(), t.getColumn())
        {
            token = t;
        }

        /*
        * Returns a clean error message (no line number/column information)
        */
        public override string Message
        {
            get
            {
                if (token != null)
                {
                    //return "unexpected token: " + token.getText();
                    return "unexpected token: " + token;
                }

                // must a tree parser error if token==null
                if (node == null)
                {
                    return "unexpected end of subtree";
                }
                return "unexpected AST node: " + node.ToString();
            }
        }
    }
}