using System;

namespace Jurassic
{
    /// <summary>
    /// Represents an arbitrarily large signed integer.
    /// </summary>
    internal struct BigInteger
    {
        private uint[] bits;
        private int wordCount;
        private int sign;

        private BigInteger(uint[] bits, int wordCount, int sign)
        {
            this.bits = bits;
            this.wordCount = wordCount;
            this.sign = sign;
        }

        /// <summary>
        /// Initializes a new instance of the BigInteger structure using a 32-bit signed integer
        /// value.
        /// </summary>
        /// <param name="value"> A 32-bit signed integer. </param>
        public BigInteger(int value)
        {
            this.bits = new uint[1];
            this.bits[0] = (uint)Math.Abs(value);
            this.wordCount = 1;
            this.sign = Math.Sign(value);
        }

        /// <summary>
        /// Initializes a new instance of the BigInteger structure using a 64-bit signed integer
        /// value.
        /// </summary>
        /// <param name="value"> A 64-bit signed integer. </param>
        public BigInteger(long value)
        {
            this.bits = new uint[2];
            this.bits[0] = (uint)Math.Abs(value);
            this.bits[1] = (uint)(Math.Abs(value) >> 32);
            this.wordCount = this.bits[1] != 0 ? 2 : 1;
            this.sign = Math.Sign(value);
        }

        /// <summary>
        /// Gets a value that represents the number zero (0).
        /// </summary>
        public static readonly BigInteger Zero = new BigInteger(0);

        /// <summary>
        /// Gets a value that represents the number one (1).
        /// </summary>
        public static readonly BigInteger One = new BigInteger(1);

        /// <summary>
        /// Gets a number that indicates the sign (negative, positive or zero) of the current
        /// BigInteger object.
        /// </summary>
        public int Sign
        {
            get { return this.sign; }
        }

        public uint HighWord
        {
            get { return this.bits[this.wordCount - 1]; }
        }

        public int BitCount
        {
            get { return (32 - CountLeadingZeroBits(this.bits[this.wordCount - 1])) + (this.wordCount - 1) * 32; }
        }

        public uint[] Words
        {
            get { return this.bits; }
        }

        public int WordCount
        {
            get { return this.wordCount; }
        }

        /// <summary>
        /// Adds two BigInteger values and returns the result.
        /// </summary>
        /// <param name="left"> The first value to add. </param>
        /// <param name="right"> The second value to add. </param>
        /// <returns> The sum of <paramref name="left"/> and <paramref name="right"/>. </returns>
        public static BigInteger Add(BigInteger left, BigInteger right)
        {
            // 0 + right = right
            if (left.Sign == 0)
                return right;
            
            // left + 0 = left
            if (right.Sign == 0)
                return left;

            // If the signs of the two numbers are different, do a subtract instead.
            if (left.Sign != right.Sign)
                return Subtract(left, Negate(right));

            // From here the sign of both numbers is the same.
            int outputWordCount = Math.Max(left.wordCount, right.wordCount);
            uint[] outputBits = new uint[outputWordCount + 1];

            uint borrow = 0;
            int i = 0;
            for (; i < Math.Min(left.wordCount, right.wordCount); i++)
            {
                ulong temp = (ulong)left.bits[i] + right.bits[i] + borrow;
                borrow = (uint)(temp >> 32);
                outputBits[i] = (uint)temp;
            }
            if (left.wordCount > right.wordCount)
            {
                for (; i < left.wordCount; i++)
                {
                    ulong temp = (ulong)left.bits[i] + borrow;
                    borrow = (uint)(temp >> 32);
                    outputBits[i] = (uint)temp;
                }
            }
            else if (left.wordCount < right.wordCount)
            {
                for (; i < right.wordCount; i++)
                {
                    ulong temp = (ulong)right.bits[i] + borrow;
                    borrow = (uint)(temp >> 32);
                    outputBits[i] = (uint)temp;
                }
            }
            if (borrow != 0)
            {
                outputBits[outputWordCount] = borrow;
                outputWordCount++;
            }
            return new BigInteger(outputBits, outputWordCount, left.Sign);
        }

        /// <summary>
        /// Returns the product of two BigInteger values.
        /// </summary>
        /// <param name="left"> The first number to multiply. </param>
        /// <param name="right"> The second number to multiply. </param>
        /// <returns> The product of the <paramref name="left"/> and <paramref name="right"/>
        /// parameters. </returns>
        public static BigInteger Multiply(BigInteger left, BigInteger right)
        {
            // Check for special cases.
            if (left.wordCount == 1)
            {
                // 0 * right = 0
                if (left.bits[0] == 0)
                    return BigInteger.Zero;

                // 1 * right = right
                // -1 * right = -right
                if (left.bits[0] == 1)
                    return left.sign == -1 ? Negate(right) : right;
            }
            if (right.wordCount == 1)
            {
                // left * 0 = 0
                if (right.bits[0] == 0)
                    return BigInteger.Zero;

                // left * 1 = left
                // left * -1 = -left
                if (right.bits[0] == 1)
                    return right.sign == -1 ? Negate(left) : left;
            }

            uint[] outputBits = new uint[left.wordCount + right.wordCount];
            int outputWordCount = left.wordCount + right.wordCount - 1;

            for (int i = 0; i < left.wordCount; i++)
            {
                uint carry = 0;
                for (int j = 0; j < right.wordCount; j++)
                {
                    ulong temp = (ulong)left.bits[i] * right.bits[j] + outputBits[i + j] + carry;
                    carry = (uint)(temp >> 32);
                    outputBits[i + j] = (uint)temp;
                }
                if (carry != 0)
                {
                    outputWordCount = Math.Max(outputWordCount, i + right.wordCount + 1);
                    outputBits[i + right.wordCount] = (uint)carry;
                }
            }

            while (outputWordCount > 1 && outputBits[outputWordCount - 1] == 0)
                outputWordCount--;

            return new BigInteger(outputBits, outputWordCount, left.sign * right.sign);
        }

        /// <summary>
        /// Subtracts one BigInteger value from another and returns the result.
        /// </summary>
        /// <param name="left"> The value to subtract from. </param>
        /// <param name="right"> The value to subtract. </param>
        /// <returns> The result of subtracting <paramref name="right"/> from
        /// <paramref name="left"/>. </returns>
        public static BigInteger Subtract(BigInteger left, BigInteger right)
        {
            // 0 - right = -right
            if (left.Sign == 0)
                return Negate(right);
            
            // left - 0 = left
            if (right.Sign == 0)
                return left;

            // If the signs of the two numbers are different, do an add instead.
            if (left.Sign != right.Sign)
                return Add(left, Negate(right));

            // From here the sign of both numbers is the same.
            uint[] outputBits = new uint[Math.Max(left.wordCount, right.wordCount)];
            int outputWordCount = outputBits.Length;
            int outputSign = left.sign;
            int i;

            // Arrange it so that Abs(a) > Abs(b).
            bool swap = false;
            if (left.wordCount < right.wordCount)
                swap = true;
            else if (left.wordCount == right.wordCount)
            {
                for (i = left.wordCount - 1; i >= 0; i--)
                    if (left.bits[i] != right.bits[i])
                    {
                        if (left.bits[i] < right.bits[i])
                            swap = true;
                        break;
                    }
            }
            if (swap == true)
            {
                var temp = left;
                left = right;
                right = temp;
                outputSign = -outputSign;
            }

            ulong y, borrow = 0;
            for (i = 0; i < right.wordCount; i++)
            {
                y = (ulong)left.bits[i] - right.bits[i] - borrow;
                borrow = y >> 32 & 1;
                outputBits[i] = (uint)y;
            }
            for (; i < left.wordCount; i++)
            {
                y = (ulong)left.bits[i] - borrow;
                borrow = y >> 32 & 1;
                outputBits[i] = (uint)y;
            }
            while (outputWordCount > 1 && outputBits[outputWordCount - 1] == 0)
                outputWordCount--;
            return new BigInteger(outputBits, outputWordCount, outputSign);
        }

        /// <summary>
        /// Shifts a BigInteger value a specified number of bits to the left.
        /// </summary>
        /// <param name="value"> The value whose bits are to be shifted. </param>
        /// <param name="shift"> The number of bits to shift <paramref name="value"/> to the left.
        /// Can be negative to shift to the right. </param>
        /// <returns> A value that has been shifted to the left by the specified number of bits. </returns>
        public static BigInteger LeftShift(BigInteger value, int shift)
        {
            // Shifting by zero bits does nothing.
            if (shift == 0)
                return value;

            // Shifting left by a negative number of bits is the same as shifting right.
            if (shift < 0)
                return RightShift(value, -shift);

            int wordShift = shift / 32;
            int bitShift = shift - (wordShift * 32);

            uint[] outputBits = new uint[value.wordCount + wordShift + 1];
            int outputWordCount = outputBits.Length - 1;

            uint carry = 0;
            for (int i = 0; i < value.wordCount; i++)
            {
                uint word = value.bits[i];
                outputBits[i + wordShift] = (word << bitShift) | carry;
                carry = bitShift == 0 ? 0 : word >> (32 - bitShift);
            }
            if (carry != 0)
            {
                outputBits[outputWordCount] = carry;
                outputWordCount++;
            }

            return new BigInteger(outputBits, outputWordCount, value.sign);
        }

        /// <summary>
        /// Shifts a BigInteger value a specified number of bits to the right.
        /// </summary>
        /// <param name="value"> The value whose bits are to be shifted. </param>
        /// <param name="shift"> The number of bits to shift <paramref name="value"/> to the right. </param>
        /// <returns> A value that has been shifted to the right by the specified number of bits.
        /// Can be negative to shift to the left. </returns>
        /// <remarks> Note: unlike System.Numerics.BigInteger, negative numbers are treated
        /// identically to positive numbers. </remarks>
        public static BigInteger RightShift(BigInteger value, int shift)
        {
            // Shifting by zero bits does nothing.
            if (shift == 0)
                return value;

            // Shifting right by a negative number of bits is the same as shifting left.
            if (shift < 0)
                return LeftShift(value, -shift);

            int wordShift = shift / 32;
            int bitShift = shift - (wordShift * 32);
            if (wordShift >= value.wordCount)
                return BigInteger.Zero;

            uint[] outputBits = new uint[value.wordCount - wordShift];
            int outputWordCount = outputBits.Length - 1;

            uint carry = 0;
            for (int i = value.wordCount - 1; i >= wordShift; i--)
            {
                uint word = value.bits[i];
                outputBits[i - wordShift] = (word >> bitShift) | (carry << (32 - bitShift));
                carry = word & (((uint)1 << bitShift) - 1);
            }
            if (outputBits[outputBits.Length - 1] != 0)
                outputWordCount++;

            return new BigInteger(outputBits, outputWordCount, value.sign);
        }

        /// <summary>
        /// Multiply by m and add a.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="m"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static BigInteger MultiplyAdd(BigInteger b, int m, int a)
        {
            if (m <= 0)
                throw new ArgumentOutOfRangeException("m");
            if (a < 0)
                throw new ArgumentOutOfRangeException("a");
            if (b.sign == 0)
                return new BigInteger(a);
            uint[] outputBits = new uint[b.wordCount + 1];
            int outputWordCount = b.wordCount;
            uint carry = (uint)a;
            for (int i = 0; i < b.wordCount; i++)
            {
                ulong temp = b.bits[i] * (ulong)m + carry;
                carry = (uint)(temp >> 32);
                outputBits[i] = (uint)temp;
            }
            if (carry != 0)
            {
                outputBits[outputWordCount] = carry;
                outputWordCount++;
            }
            return new BigInteger(outputBits, outputWordCount, 1);
        }

        private readonly static int[] powersOfFive = { 5, 25, 125 };

        /// <summary>
        /// Computes b x 5 ^ k.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static BigInteger MultiplyPow5(BigInteger b, int k)
        {
            BigInteger p5;

            // Fast route if k <= 3.
            if ((k & 3) != 0)
                b = MultiplyAdd(b, powersOfFive[(k & 3) - 1], 0);
            if ((k >>= 2) == 0)
                return b;

            p5 = new BigInteger(625);
            while (true)
            {
                if ((k & 1) == 1)
                    b = Multiply(b, p5);
                if ((k >>= 1) == 0)
                    break;
                p5 = Multiply(p5, p5);
            }
            return b;
        }

        /// <summary>
        /// Returns <c>-1</c> if a &lt; b, <c>0</c> if they are the same, or <c>1</c> if a &gt; b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Compare(BigInteger a, BigInteger b)
        {
            if (a.sign != b.sign)
                return a.sign < b.sign ? -1 : 1;
            if (a.sign > 0)
            {
                // Comparison of positive numbers.
                if (a.wordCount < b.wordCount)
                    return -1;
                if (a.wordCount > b.wordCount)
                    return 1;
                for (int i = a.wordCount - 1; i >= 0; i--)
                {
                    if (a.bits[i] != b.bits[i])
                        return a.bits[i] < b.bits[i] ? -1 : 1;
                }
            }
            else if (a.sign < 0)
            {
                // Comparison of negative numbers.
                if (a.wordCount < b.wordCount)
                    return 1;
                if (a.wordCount > b.wordCount)
                    return -1;
                for (int i = a.wordCount - 1; i >= 0; i--)
                {
                    if (a.bits[i] != b.bits[i])
                        return a.bits[i] < b.bits[i] ? 1 : -1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Negates a specified BigInteger value.
        /// </summary>
        /// <param name="value"> The value to negate. </param>
        /// <returns> The result of the <paramref name="value"/> parameter multiplied by negative
        /// one (-1). </returns>
        public static BigInteger Negate(BigInteger value)
        {
            value.sign = -value.sign;
            return value;
        }

        /// <summary>
        /// Modifies the given values so they are suitable for passing to Quorem.
        /// </summary>
        /// <param name="dividend"> The number that will be divided. </param>
        /// <param name="divisor"> The number to divide by. </param>
        public static void SetupQuorum(ref BigInteger dividend, ref BigInteger divisor)
        {
            var leadingZeroCount = CountLeadingZeroBits(divisor.bits[divisor.wordCount - 1]);
            if (leadingZeroCount < 4 || leadingZeroCount > 28)
            {
                dividend = BigInteger.LeftShift(dividend, 8);
                divisor = BigInteger.LeftShift(divisor, 8);
            }
        }

        /// <summary>
        /// Modifies the given values so they are suitable for passing to Quorem.
        /// </summary>
        /// <param name="dividend"> The number that will be divided. </param>
        /// <param name="divisor"> The number to divide by. </param>
        /// <param name="other"> Another value involved in the division. </param>
        public static void SetupQuorum(ref BigInteger dividend, ref BigInteger divisor, ref BigInteger other)
        {
            var leadingZeroCount = CountLeadingZeroBits(divisor.bits[divisor.wordCount - 1]);
            if (leadingZeroCount < 4 || leadingZeroCount > 28)
            {
                dividend = BigInteger.LeftShift(dividend, 8);
                divisor = BigInteger.LeftShift(divisor, 8);
                other = BigInteger.LeftShift(other, 8);
            }
        }

        /// <summary>
        /// Calculates the integer result of dividing <paramref name="dividend"/> by
        /// <paramref name="divisor"/> then sets <paramref name="dividend"/> to the remainder.
        /// </summary>
        /// <param name="dividend"> The number that will be divided. </param>
        /// <param name="divisor"> The number to divide by. </param>
        /// <returns> The integer that results from dividing <paramref name="dividend"/> by
        /// <paramref name="divisor"/>. </returns>
        public static int Quorem(ref BigInteger dividend, BigInteger divisor)
        {
            int n = divisor.wordCount;
            if (dividend.wordCount > n)
                throw new ArgumentException("b is too large");
            if (dividend.wordCount < n)
                return 0;
            uint q = dividend.bits[dividend.wordCount - 1] / (divisor.bits[divisor.wordCount - 1] + 1);	/* ensure q <= true quotient */

            if (q != 0)
            {
                ulong borrow = 0;
                ulong carry = 0;
                for (int i = 0; i < divisor.wordCount; i++)
                {
                    ulong ys = divisor.bits[i] * (ulong)q + carry;
                    carry = ys >> 32;
                    ulong y = dividend.bits[i] - (ys & 0xFFFFFFFF) - borrow;
                    borrow = y >> 32 & 1;
                    dividend.bits[i] = (uint)y;
                }
                while (dividend.wordCount > 1 && dividend.bits[dividend.wordCount - 1] == 0)
                    dividend.wordCount--;
            }
            if (Compare(dividend, divisor) >= 0)
            {
                q++;
                ulong borrow = 0;
                ulong carry = 0;
                for (int i = 0; i < divisor.wordCount; i++)
                {
                    ulong ys = divisor.bits[i] + carry;
                    carry = ys >> 32;
                    ulong y = dividend.bits[i] - (ys & 0xFFFFFFFF) - borrow;
                    borrow = y >> 32 & 1;
                    dividend.bits[i] = (uint)y;
                }
                while (dividend.wordCount > 1 && dividend.bits[dividend.wordCount - 1] == 0)
                    dividend.wordCount--;
            }
            if (dividend.wordCount == 1 && dividend.bits[0] == 0)
                dividend.sign = 0;
            return (int)q;
        }

        /// <summary>
        /// Decrements the current value of the BigInteger object.
        /// </summary>
        public void InPlaceDecrement()
        {
            if (this.sign < 0)
                throw new InvalidOperationException("Operand must be positive.");
            uint lowWord = this.bits[0];
            if (lowWord > 1)
            {
                // Fast case: subtract from lowest word.
                this.bits[0]--;
            }
            else if (this.wordCount == 1)
            {
                // value = 0 or 1 - requires sign change.
                this.bits[0]--;
                if (lowWord == 1)
                    this.sign = 0;
                else if (lowWord == 0)
                    this.sign = -this.sign;
            }
            else
            {
                // Slow case: have to underflow.
                this.bits[0]--;
                bool carry = false;
                for (int i = 1; i < this.wordCount; i++)
                {
                    carry = this.bits[i] == 0;
                    this.bits[i]--;
                    if (carry == false)
                        break;
                }
                if (this.bits[this.wordCount - 1] == 0)
                    this.wordCount--;
            }
        }

        private readonly static long[] integerPowersOfTen = new long[]
        {
            1L,
            10L,
            100L,
            1000L,
            10000L,
            100000L,
            1000000L,
            10000000L,
            100000000L,
            1000000000L,
            10000000000L,
            100000000000L,
            1000000000000L,
            10000000000000L,
            100000000000000L,
            1000000000000000L,
            10000000000000000L,
            100000000000000000L,
            1000000000000000000L,
        };

        /// <summary>
        /// Equivalent to BigInteger.Pow but with integer arguments.
        /// </summary>
        /// <param name="radix"> The number to be raised to a power. </param>
        /// <param name="exponent"> The number that specifies the power. </param>
        /// <returns> The number <paramref name="radix"/> raised to the power
        /// <paramref name="exponent"/>. </returns>
        public static BigInteger Pow(int radix, int exponent)
        {
            if (radix < 0 || radix > 36)
                throw new ArgumentOutOfRangeException("radix");
            if (exponent < 0)
                throw new ArgumentOutOfRangeException("exponent");

            if (radix == 10 && exponent < integerPowersOfTen.Length)
            {
                // Use a table for quick lookup of powers of 10.
                return new BigInteger(integerPowersOfTen[exponent]);
            }
            else if (radix == 2)
            {
                // Power of two is easy.
                return BigInteger.LeftShift(BigInteger.One, exponent);
            }

            // Special cases.
            switch (exponent)
            {
                case 0:
                    return BigInteger.One;
                case 1:
                    return new BigInteger(radix);
                case 2:
                    return new BigInteger(radix * radix);
                case 3:
                    return new BigInteger(radix * radix * radix);
            }

            // Use recursion to calculate the result.
            if ((exponent & 1) == 1)
            {
                // Exponent is odd.
                var temp = Pow(radix, exponent / 2);
                return BigInteger.MultiplyAdd(BigInteger.Multiply(temp, temp), radix, 0);
            }
            else
            {
                // Exponent is even.
                var temp = Pow(radix, exponent / 2);
                return BigInteger.Multiply(temp, temp);
            }
        }

        /// <summary>
        /// Gets the absolute value of a BigInteger object.
        /// </summary>
        /// <param name="value"> A number. </param>
        /// <returns> The absolute value of <paramref name="value"/> </returns>
        public static BigInteger Abs(BigInteger b)
        {
            return new BigInteger(b.bits, b.wordCount, Math.Abs(b.sign));
        }

        /// <summary>
        /// Returns the logarithm of a specified number in a specified base.
        /// </summary>
        /// <param name="value"> A number whose logarithm is to be found. </param>
        /// <param name="baseValue"> The base of the logarithm. </param>
        /// <returns> The base <paramref name="baseValue"/> logarithm of <paramref name="value"/>. </returns>
        public static double Log(BigInteger value, double baseValue)
        {
            if (baseValue <= 1.0 || double.IsPositiveInfinity(baseValue) || double.IsNaN(baseValue))
                throw new ArgumentOutOfRangeException("baseValue", "Unsupported logarithmic base.");
            if (value.sign < 0)
                return double.NaN;
            if (value.sign == 0)
                return double.NegativeInfinity;
            if (value.wordCount == 1)
                return Math.Log((double)value.bits[0], baseValue);

            double d = 0.0;
            double residual = 0.5;
            int bitsInLastWord = 32 - CountLeadingZeroBits(value.bits[value.wordCount - 1]);
            int bitCount = ((value.wordCount - 1) * 32) + bitsInLastWord;
            uint highBit = ((uint)1) << (bitsInLastWord - 1);
            for (int i = value.wordCount - 1; i >= 0; i--)
            {
                while (highBit != 0)
                {
                    if ((value.bits[i] & highBit) != 0)
                        d += residual;
                    residual *= 0.5;
                    highBit = highBit >> 1;
                }
                highBit = 0x80000000;
            }
            return ((Math.Log(d) + (0.69314718055994529 * bitCount)) / Math.Log(baseValue));

        }

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified BigInteger
        /// object have the same value.
        /// </summary>
        /// <param name="obj"> The object to compare. </param>
        /// <returns> <c>true</c> if this BigInteger object and <paramref name="obj"/> have the
        /// same value; otherwise, <c>false</c>. </returns>
        public override bool Equals(object obj)
        {
            if ((obj is BigInteger) == false)
                return false;
            if (this.wordCount != ((BigInteger)obj).wordCount)
                return false;
            return Compare(this, (BigInteger)obj) == 0;
        }

        /// <summary>
        /// Returns the hash code for the current BigInteger object.
        /// </summary>
        /// <returns> A 32-bit signed integer hash code. </returns>
        public override int GetHashCode()
        {
            uint result = 0;
            for (int i = 0; i < this.wordCount; i++)
                result ^= this.bits[i];
            return (int)result;
        }

        /// <summary>
        /// Converts the string representation of a number to its BigInteger equivalent.
        /// </summary>
        /// <param name="str"> A string that contains the number to convert. </param>
        /// <returns> A value that is equivalent to the number specified in the
        /// <paramref name="value"/> parameter. </returns>
        public static BigInteger Parse(string str)
        {
            BigInteger result = BigInteger.Zero;
            bool negative = false;
            int i = 0;
            if (str[0] == '-')
            {
                negative = true;
                i = 1;
            }
            else if (str[0] == '+')
                i = 1;
            for (; i < str.Length; i++)
            {
                char c = str[i];
                if (c < '0' || c > '9')
                    throw new FormatException("Invalid character in number.");
                result = MultiplyAdd(result, 10, c - '0');
            }
            if (result.wordCount != 1 || result.bits[0] != 0)
                result.sign = negative == true ? -1 : 1;
            return result;
        }

        /// <summary>
        /// Converts the numeric value of the current BigInteger object to its equivalent string
        /// representation.
        /// </summary>
        /// <returns> The string representation of the current BigInteger value. </returns>
        public override string ToString()
        {
            if (BigInteger.Equals(this, Zero))
                return "0";

            var result = new System.Text.StringBuilder();
            var value = this;
            if (value.Sign < 0)
            {
                result.Append('-');
                value.sign = 1;
            }

            // Overestimate of Floor(Log10(value))
            int log10 = (int)Math.Floor(Log(value, 10)) + 1;

            var divisor = Pow(10, log10);
            
            // Adjust the values so that Quorum works.
            SetupQuorum(ref value, ref divisor);

            // Check for overestimate of log10.
            if (BigInteger.Compare(divisor, value) > 0)
            {
                value = BigInteger.MultiplyAdd(value, 10, 0);
                log10--;
            }

            for (int i = 0; i <= log10; i ++)
            {
                // value = value / divisor
                int digit = Quorem(ref value, divisor);

                // Append the digit.
                result.Append((char)(digit + '0'));

                // value = value * 10;
                value = BigInteger.MultiplyAdd(value, 10, 0);
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns a new instance BigInteger structure from a 64-bit double precision floating
        /// point value.
        /// </summary>
        /// <param name="value"> A 64-bit double precision floating point value. </param>
        /// <returns> The corresponding BigInteger value. </returns>
        public static BigInteger FromDouble(double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);

            // Extract the base-2 exponent.
            var base2Exponent = (int)((bits & 0x7FF0000000000000) >> 52) - 1023;

            // Extract the mantissa.
            long mantissa = bits & 0xFFFFFFFFFFFFF;
            if (base2Exponent > -1023)
            {
                mantissa |= 0x10000000000000;
                base2Exponent -= 52;
            }
            else
            {
                // Denormals.
                base2Exponent -= 51;
            }

            // Extract the sign bit.
            if (bits < 0)
                mantissa = -mantissa;

            return BigInteger.LeftShift(new BigInteger(mantissa), base2Exponent);
        }

        /// <summary>
        /// Returns a new instance BigInteger structure from a 64-bit double precision floating
        /// point value.
        /// </summary>
        /// <param name="value"> A 64-bit double precision floating point value. </param>
        /// <returns> The corresponding BigInteger value. </returns>
        public double ToDouble()
        {
            // Special case: zero.
            if (this.wordCount == 1 && this.bits[0] == 0)
                return 0.0;

            // Get the number of bits in the BigInteger.
            var bitCount = this.BitCount;

            // The top 53 bits can be packed into the double (the top-most bit is implied).
            var temp = BigInteger.RightShift(this, bitCount - 53);
            ulong doubleBits = (((ulong)temp.bits[1] << 32) | temp.bits[0]) & 0xFFFFFFFFFFFFF;

            // Base-2 exponent is however much we shifted, plus 52 (because the decimal point is
            // effectively at the 52nd bit), plus 1023 (the bias).
            doubleBits |= (ulong)(bitCount - 53 + 52 + 1023) << 52;

            // Handle the sign bit.
            if (this.sign == -1)
                doubleBits |= (ulong)1 << 63;

            // Convert the bit representation to a double.
            return BitConverter.Int64BitsToDouble((long)doubleBits);
        }

        /// <summary>
        /// Returns the number of leading zero bits in the given 32-bit integer.
        /// </summary>
        /// <param name="value"> A 32-bit integer. </param>
        /// <returns> The number of leading zero bits in the given 32-bit integer.  Returns
        /// <c>32</c> if <paramref name="value"/> is zero. </returns>
        private static int CountLeadingZeroBits(uint value)
        {
            int k = 0;

            if ((value & 0xFFFF0000) == 0)
            {
                k = 16;
                value <<= 16;
            }
            if ((value & 0xFF000000) == 0)
            {
                k += 8;
                value <<= 8;
            }
            if ((value & 0xF0000000) == 0)
            {
                k += 4;
                value <<= 4;
            }
            if ((value & 0xC0000000) == 0)
            {
                k += 2;
                value <<= 2;
            }
            if ((value & 0x80000000) == 0)
            {
                k++;
                if ((value & 0x40000000) == 0)
                    return 32;
            }
            return k;
        }
    }
}
