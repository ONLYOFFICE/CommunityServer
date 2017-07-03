using System.Text;

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

    public class NoViableAltForCharException : RecognitionException
    {
        public char foundChar;

        public NoViableAltForCharException(char c, CharScanner scanner) :
            base("NoViableAlt", scanner.getFilename(), scanner.getLine(), scanner.getColumn())
        {
            foundChar = c;
        }

        public NoViableAltForCharException(char c, string fileName, int line, int column) :
            base("NoViableAlt", fileName, line, column)
        {
            foundChar = c;
        }

        /*
        * Returns a clean error message (no line number/column information)
        */
        public override string Message
        {
            get
            {
                StringBuilder mesg = new StringBuilder("unexpected char: ");

                // I'm trying to mirror a change in the C++ stuff.
                // But java seems to lack something isprint-ish..
                // so we do it manually. This is probably too restrictive.

                if ((foundChar >= ' ') && (foundChar <= '~'))
                {
                    mesg.Append('\'');
                    mesg.Append(foundChar);
                    mesg.Append('\'');
                }
                else
                {
                    mesg.Append("0x");
                    mesg.Append(((int)foundChar).ToString("X"));
                }
                return mesg.ToString();
            }
        }
    }
}