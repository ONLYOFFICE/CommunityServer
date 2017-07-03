using System;
using System.Linq;
using System.Text;

namespace antlr.collections
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

    /*A BitSet to replace java.util.BitSet.
    * Primary differences are that most set operators return new sets
    * as opposed to oring and anding "in place".  Further, a number of
    * operations were added.  I cannot contain a BitSet because there
    * is no way to access the internal bits (which I need for speed)
    * and, because it is final, I cannot subclass to add functionality.
    * Consider defining set degree.  Without access to the bits, I must
    * call a method n times to test the ith bit...ack!
    *
    * Also seems like or() from util is wrong when size of incoming set is bigger
    * than this.bits.length.
    *
    * @author Terence Parr
    * @author <br><a href="mailto:pete@yamuna.demon.co.uk">Pete Wells</a>
    */

    public class BitSet
    {
        protected internal const int BITS = 64; // number of bits / long
        protected internal const int LOG_BITS = 6; // 2^6 == 64

        /*We will often need to do a mod operator (i mod nbits).  Its
        * turns out that, for powers of two, this mod operation is
        * same as (i & (nbits-1)).  Since mod is slow, we use a
        * precomputed mod mask to do the mod instead.
        */
        protected internal static readonly int MOD_MASK = BITS - 1;

        protected internal long[] dataBits;

        /*Construction from a static array of longs */
        public BitSet(long[] bits_)
        {
            dataBits = bits_;
        }

        /*Construct a bitset given the size
        * @param nbits The size of the bitset in bits
        */
        public BitSet(int nbits)
        {
            dataBits = new long[((nbits - 1) >> LOG_BITS) + 1];
        }

        private static long bitMask(int bitNumber)
        {
            int bitPosition = bitNumber & MOD_MASK; // bitNumber mod BITS
            return 1L << bitPosition;
        }

        public virtual object Clone()
        {
            var s = new BitSet(dataBits.Length);
            Array.Copy(dataBits, 0, s.dataBits, 0, dataBits.Length);
            return s;
        }

        public virtual int degree()
        {
            int deg = 0;
            for (int i = dataBits.Length - 1; i >= 0; i--)
            {
                long word = dataBits[i];
                if (word != 0L)
                {
                    for (int bit = BITS - 1; bit >= 0; bit--)
                    {
                        if ((word & (1L << bit)) != 0)
                        {
                            deg++;
                        }
                    }
                }
            }
            return deg;
        }

        protected bool Equals(BitSet other)
        {
            return dataBits.SequenceEqual(other.dataBits);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BitSet)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return dataBits.Aggregate(19, (current, bit) => current * 31 + bit.GetHashCode());
            }
        }

        public virtual bool member(int el)
        {
            int n = wordNumber(el);
            if (n >= dataBits.Length)
                return false;
            return (dataBits[n] & bitMask(el)) != 0;
        }

        private static int wordNumber(int bit) => bit >> LOG_BITS;

        public virtual int[] toArray()
        {
            int[] elems = new int[degree()];
            int en = 0;
            for (int i = 0; i < (dataBits.Length << LOG_BITS); i++)
            {
                if (member(i))
                {
                    elems[en++] = i;
                }
            }
            return elems;
        }

        public override string ToString()
        {
            return ToString(",");
        }

        /*Transform a bit set into a string by formatting each element as an integer
        * @separator The string to put in between elements
        * @return A commma-separated list of values
        */
        public virtual string ToString(string separator)
        {
            var builder = new StringBuilder(dataBits.Length * 2);

            for (int i = 0; i < dataBits.Length << LOG_BITS; i++)
            {
                if (!member(i))
                {
                    continue;
                }
                if (builder.Length > 0)
                {
                    builder.Append(separator);
                }
                builder.Append(i);
            }
            return builder.ToString();
        }
    }
}