using System.Collections.Generic;
using System.Text;

namespace antlr
{
    /*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/license.html
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	// SAS: Added this class to genericise the input buffers for scanners
	//      This allows a scanner to use a binary (FileInputStream) or
	//      text (FileReader) stream of data; the generated scanner
	//      subclass will define the input stream
	//      There are two subclasses to this: CharBuffer and ByteBuffer
	
	/// <summary>
	/// Represents a stream of characters fed to the lexer from that can be rewound 
	/// via mark()/rewind() methods.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A dynamic array is used to buffer up all the input characters.  Normally,
	/// "k" characters are stored in the buffer.  More characters may be stored 
	/// during guess mode (testing syntactic predicate), or when LT(i>k) is referenced.
	/// Consumption of characters is deferred.  In other words, reading the next
	/// character is not done by conume(), but deferred until needed by LA or LT.
	/// </para>
	/// </remarks>
	public abstract class InputBuffer
	{
		// Number of active markers
		protected internal int nMarkers;
		
		// Additional offset used when markers are active
		protected internal int markerOffset;
		
		// Number of calls to consume() since last LA() or LT() call
		protected internal int numToConsume;
		
		// Circular queue
	    protected List<char> Buffer = new List<char>();
		
		/*This method updates the state of the input buffer so that
		*  the text matched since the most recent mark() is no longer
		*  held by the buffer.  So, you either do a mark/rewind for
		*  failed predicate or mark/commit to keep on parsing without
		*  rewinding the input.
		*/
		public virtual void  commit()
		{
			nMarkers--;
		}
		
		/*Mark another character for deferred consumption */
		public virtual char consume()
		{
			numToConsume++;
			return LA(1);
		}
		
		/*Ensure that the input buffer is sufficiently full */
		public abstract void  fill(int amount);
		
		public virtual string getLAChars()
		{
			StringBuilder la = new StringBuilder();

			// copy buffer contents to array before looping thru contents (it's usually faster)
			char[] fastBuf = new char[Buffer.Count-markerOffset];
			Buffer.CopyTo(fastBuf, markerOffset);

			la.Append(fastBuf);
			return la.ToString();
		}
		
		public virtual string getMarkedChars()
		{
			StringBuilder marked = new StringBuilder();

			// copy buffer contents to array before looping thru contents (it's usually faster)
			char[] fastBuf = new char[Buffer.Count-markerOffset];
			Buffer.CopyTo(fastBuf, markerOffset);

			marked.Append(fastBuf);
			return marked.ToString();
		}
		
		public virtual bool isMarked()
		{
			return (nMarkers != 0);
		}
		
		/*Get a lookahead character */
		public virtual char LA(int i)
		{
			fill(i);
			return Buffer[markerOffset + i - 1];
		}
		
		/*Return an integer marker that can be used to rewind the buffer to
		* its current state.
		*/
		public virtual int mark()
		{
			syncConsume();
			nMarkers++;
			return markerOffset;
		}
		
		/*Rewind the character buffer to a marker.
		* @param mark Marker returned previously from mark()
		*/
		public virtual void  rewind(int mark)
		{
			syncConsume();
			markerOffset = mark;
			nMarkers--;
		}
		
		/*Reset the input buffer
		*/
		public virtual void  reset()
		{
			nMarkers = 0;
			markerOffset = 0;
			numToConsume = 0;
			Buffer.Clear();
		}
		
		/*Sync up deferred consumption */
		protected internal virtual void  syncConsume()
		{
			if (numToConsume > 0)
			{
				if (nMarkers > 0)
				{
					// guess mode -- leave leading characters and bump offset.
					markerOffset += numToConsume;
				}
				else
				{
					// normal mode -- remove "consumed" characters from buffer
					Buffer.RemoveRange(0, numToConsume);
				}
				numToConsume = 0;
			}
		}
	}
}