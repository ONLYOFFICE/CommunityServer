using System.IO;

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

    /*A Stream of characters fed to the lexer from a InputStream that can
    * be rewound via mark()/rewind() methods.
    * <p>
    * A dynamic array is used to buffer up all the input characters.  Normally,
    * "k" characters are stored in the buffer.  More characters may be stored during
    * guess mode (testing syntactic predicate), or when LT(i>k) is referenced.
    * Consumption of characters is deferred.  In other words, reading the next
    * character is not done by conume(), but deferred until needed by LA or LT.
    * <p>
    */

    // SAS: Move most functionality into InputBuffer -- just the file-specific
    //      stuff is in here
    public class CharBuffer : InputBuffer
    {
        internal TextReader input;

        private const int BUF_SIZE = 16;
        /// <summary>
        /// Small buffer used to avoid reading individual chars
        /// </summary>
        private readonly char[] buf = new char[BUF_SIZE];


        /*Create a character buffer */
        public CharBuffer(TextReader input_)
        {
            input = input_;
        }

        /*Ensure that the character buffer is sufficiently full */
        public override void fill(int amount)
        {
            try
            {
                syncConsume();
                // Fill the buffer sufficiently to hold needed characters
                int charsToRead = (amount + markerOffset) - Buffer.Count;

                while (charsToRead > 0)
                {
                    // Read a few characters
                    var c = input.Read(buf, 0, BUF_SIZE);
                    for (int i = 0; i < c; i++)
                    {
                        // Append the next character
                        Buffer.Add(buf[i]);
                    }
                    if (c < BUF_SIZE)
                    {
                        while ((charsToRead-- > 0) && (Buffer.Count < BUF_SIZE))
                        {
                            Buffer.Add(CharScanner.EOF_CHAR);
                        }
                        break;
                    }
                    charsToRead -= c;
                }
            }
            catch (IOException io)
            {
                throw new CharStreamIOException(io);
            }
        }
    }
}