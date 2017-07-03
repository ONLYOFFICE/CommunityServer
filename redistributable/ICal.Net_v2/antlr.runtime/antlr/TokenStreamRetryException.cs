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

    /*
    * Aborted recognition of current token. Try to get one again.
    * Used by TokenStreamSelector.retry() to force nextToken()
    * of stream to re-enter and retry.
    */

    public class TokenStreamRetryException : TokenStreamException
    {
    }
}