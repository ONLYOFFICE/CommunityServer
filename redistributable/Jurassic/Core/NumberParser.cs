using System;
using System.Collections.Generic;
using System.IO;

namespace Jurassic
{

    /// <summary>
    /// Parses strings into numbers.
    /// </summary>
    internal static class NumberParser
    {
        /// <summary>
        /// Converts a string to a number (used by parseFloat).
        /// </summary>
        /// <param name="input"> The string to convert. </param>
        /// <returns> The result of parsing the string as a number. </returns>
        internal static double ParseFloat(string input)
        {
            var reader = new System.IO.StringReader(input);

            // Skip whitespace and line terminators.
            while (IsWhiteSpaceOrLineTerminator(reader.Peek()))
                reader.Read();

            // The number can start with a plus or minus sign.
            bool negative = false;
            int firstChar = reader.Read();
            switch (firstChar)
            {
                case '-':
                    negative = true;
                    firstChar = reader.Read();
                    break;
                case '+':
                    firstChar = reader.Read();
                    break;
            }

            // Infinity or -Infinity are also valid.
            if (firstChar == 'I' && reader.ReadToEnd().StartsWith("nfinity", StringComparison.Ordinal) == true)
                return negative ? double.NegativeInfinity : double.PositiveInfinity;

            // Empty strings return NaN.
            if ((firstChar < '0' || firstChar > '9') && firstChar != '.')
                return double.NaN;

            // Parse the number.
            NumberParser.ParseCoreStatus status;
            double result = NumberParser.ParseCore(reader, (char)firstChar, out status, allowHex: false, allowOctal: false);

            // Handle various error cases.
            if (status == ParseCoreStatus.NoDigits)
                return double.NaN;

            return negative ? -result : result;
        }

        /// <summary>
        /// Converts a string to a number (used in type coercion).
        /// </summary>
        /// <returns> The result of parsing the string as a number. </returns>
        internal static double CoerceToNumber(string input)
        {
            var reader = new System.IO.StringReader(input);

            // Skip whitespace and line terminators.
            while (IsWhiteSpaceOrLineTerminator(reader.Peek()))
                reader.Read();

            // Empty strings return 0.
            int firstChar = reader.Read();
            if (firstChar == -1)
                return 0.0;

            // The number can start with a plus or minus sign.
            bool negative = false;
            switch (firstChar)
            {
                case '-':
                    negative = true;
                    firstChar = reader.Read();
                    break;
                case '+':
                    firstChar = reader.Read();
                    break;
            }

            // Infinity or -Infinity are also valid.
            if (firstChar == 'I')
            {
                string restOfString1 = reader.ReadToEnd();
                if (restOfString1.StartsWith("nfinity", StringComparison.Ordinal) == true)
                {
                    // Check the end of the string for junk.
                    for (int i = 7; i < restOfString1.Length; i++)
                        if (IsWhiteSpaceOrLineTerminator(restOfString1[i]) == false)
                            return double.NaN;
                    return negative ? double.NegativeInfinity : double.PositiveInfinity;
                }
            }

            // Return NaN if the first digit is not a number or a period.
            if ((firstChar < '0' || firstChar > '9') && firstChar != '.')
                return double.NaN;

            // Parse the number.
            NumberParser.ParseCoreStatus status;
            double result = NumberParser.ParseCore(reader, (char)firstChar, out status, allowHex: true, allowOctal: false);

            // Handle various error cases.
            switch (status)
            {
                case ParseCoreStatus.NoDigits:
                case ParseCoreStatus.NoExponent:
                    return double.NaN;
            }

            // Check the end of the string for junk.
            string restOfString2 = reader.ReadToEnd();
            for (int i = 0; i < restOfString2.Length; i++)
                if (IsWhiteSpaceOrLineTerminator(restOfString2[i]) == false)
                    return double.NaN;

            return negative ? -result : result;
        }

        private static readonly int[] integerPowersOfTen = new int[] {
            1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000
        };

        internal enum ParseCoreStatus
        {
            Success,
            NoDigits,   // Number consists of a period without any digits.
            NoFraction, // Number has a period, but no digits after it.
            NoExponent, // Number has 'e' but no number after it.
            ExponentHasLeadingZero,
            HexLiteral,
            InvalidHexLiteral,
            OctalLiteral,
            InvalidOctalLiteral,
        }

        /// <summary>
        /// Parses a number and returns the corresponding double-precision value.
        /// </summary>
        /// <param name="reader"> The reader to read characters from. </param>
        /// <param name="firstChar"> The first character of the number.  Must be 0-9 or a period. </param>
        /// <param name="status"> Upon returning, contains the type of error if one occurred. </param>
        /// <param name="allowHex"> </param>
        /// <param name="allowOctal"> </param>
        /// <returns> The numeric value, or <c>NaN</c> if the number is invalid. </returns>
        internal static double ParseCore(TextReader reader, char firstChar, out ParseCoreStatus status, bool allowHex = true, bool allowOctal = true)
        {
            double result;

            // A count of the number of integral and fractional digits of the input number.
            int totalDigits = 0;

            // Assume success.
            status = ParseCoreStatus.Success;

            // If the number starts with '0' then the number is a hex literal or a octal literal.
            if (firstChar == '0' && (allowHex == true || allowOctal == true))
            {
                // Read the next char - should be 'x' or 'X' if this is a hex number (could be just '0').
                int c = reader.Peek();
                if ((c == 'x' || c == 'X') && allowHex == true)
                {
                    // Hex number.
                    reader.Read();

                    result = ParseHex(reader);
                    if (double.IsNaN(result) == true)
                    {
                        status = ParseCoreStatus.InvalidHexLiteral;
                        return double.NaN;
                    }
                    status = ParseCoreStatus.HexLiteral;
                    return result;
                }
                else if (c >= '0' && c <= '9' && allowOctal == true)
                {
                    // Octal number.
                    result = ParseOctal(reader);
                    if (double.IsNaN(result) == true)
                    {
                        status = ParseCoreStatus.InvalidOctalLiteral;
                        return double.NaN;
                    }
                    status = ParseCoreStatus.OctalLiteral;
                    return result;
                }
            }

            // desired1-3 hold the integral and fractional digits of the input number.
            // desired1 holds the first set of nine digits, desired2 holds the second set of nine
            // digits, desired3 holds the rest.
            int desired1 = 0;
            int desired2 = 0;
            var desired3 = BigInteger.Zero;

            // Indicates the base-10 scale factor of the output e.g. the result is
            // desired x 10^exponentBase10.
            int exponentBase10 = 0;

            // Read the integer component.
            if (firstChar >= '0' && firstChar <= '9')
            {
                desired1 = firstChar - '0';
                totalDigits = 1;
                while (true)
                {
                    int c = reader.Peek();
                    if (c < '0' || c > '9')
                        break;
                    reader.Read();
                    
                    if (totalDigits < 9)
                        desired1 = desired1 * 10 + (c - '0');
                    else if (totalDigits < 18)
                        desired2 = desired2 * 10 + (c - '0');
                    else
                        desired3 = BigInteger.MultiplyAdd(desired3, 10, c - '0');
                    totalDigits++;
                }
            }

            if (firstChar == '.' || reader.Peek() == '.')
            {
                // Skip past the period.
                if (firstChar != '.')
                    reader.Read();

                // Read the fractional component.
                int fractionalDigits = 0;
                while (true)
                {
                    int c = reader.Peek();
                    if (c < '0' || c > '9')
                        break;
                    reader.Read();

                    if (totalDigits < 9)
                        desired1 = desired1 * 10 + (c - '0');
                    else if (totalDigits < 18)
                        desired2 = desired2 * 10 + (c - '0');
                    else
                        desired3 = BigInteger.MultiplyAdd(desired3, 10, c - '0');
                    totalDigits++;
                    fractionalDigits++;
                    exponentBase10--;
                }

                // Check if the number consists solely of a period.
                if (totalDigits == 0)
                {
                    status = ParseCoreStatus.NoDigits;
                    return double.NaN;
                }

                // Check if the number has a period but no digits afterwards.
                if (fractionalDigits == 0)
                    status = ParseCoreStatus.NoFraction;
            }

            if (reader.Peek() == 'e' || reader.Peek() == 'E')
            {
                // Skip past the 'e'.
                reader.Read();

                // Read the sign of the exponent.
                bool exponentNegative = false;
                int c = reader.Peek();
                if (c == '+')
                    reader.Read();
                else if (c == '-')
                {
                    reader.Read();
                    exponentNegative = true;
                }

                // Read the first character of the exponent.
                int firstExponentChar = reader.Read();

                // Check there is a number after the 'e'.
                int exponent = 0;
                if (firstExponentChar < '0' || firstExponentChar > '9')
                {
                    status = ParseCoreStatus.NoExponent;
                }
                else
                {
                    // Read the rest of the exponent.
                    exponent = firstExponentChar - '0';
                    int exponentDigits = 1;
                    while (true)
                    {
                        c = reader.Peek();
                        if (c < '0' || c > '9')
                            break;
                        reader.Read();
                        exponent = Math.Min(exponent * 10 + (c - '0'), 9999);
                        exponentDigits++;
                    }

                    // JSON does not allow a leading zero in front of the exponent.
                    if (firstExponentChar == '0' && exponentDigits > 1 && status == ParseCoreStatus.Success)
                        status = ParseCoreStatus.ExponentHasLeadingZero;
                }

                // Keep track of the overall base-10 exponent.
                exponentBase10 += exponentNegative ? -exponent : exponent;
            }

            // Calculate the integral and fractional portion of the number, scaled to an integer.
            if (totalDigits < 16)
            {
                // Combine desired1 and desired2 to produce an integer representing the final
                // result.
                result = (double)((long)desired1 * integerPowersOfTen[Math.Max(totalDigits - 9, 0)] + desired2);
            }
            else
            {
                // Combine desired1, desired2 and desired3 to produce an integer representing the
                // final result.
                var temp = desired3;
                desired3 = new BigInteger((long)desired1 * integerPowersOfTen[Math.Min(totalDigits - 9, 9)] + desired2);
                if (totalDigits > 18)
                {
                    desired3 = BigInteger.Multiply(desired3, BigInteger.Pow(10, totalDigits - 18));
                    desired3 = BigInteger.Add(desired3, temp);
                }
                result = desired3.ToDouble();
            }

            // Apply the base-10 exponent.
            if (exponentBase10 > 0)
                result *= Math.Pow(10, exponentBase10);
            else if (exponentBase10 < 0 && exponentBase10 >= -308)
                result /= Math.Pow(10, -exponentBase10);
            else if (exponentBase10 < -308)
            {
                // Note: 10^308 is the largest representable power of ten.
                result /= Math.Pow(10, 308);
                result /= Math.Pow(10, -exponentBase10 - 308);
            }

            // Numbers with 16 or more digits require the use of arbitrary precision arithmetic to
            // determine the correct answer.
            if (totalDigits >= 16)
                return RefineEstimate(result, exponentBase10, desired3);

            return result;
        }

        /// <summary>
        /// Converts a string to an integer (used by parseInt).
        /// </summary>
        /// <param name="radix"> The numeric base to use for parsing.  Pass zero to use base 10
        /// except when the input string starts with '0' in which case base 16 or base 8 are used
        /// instead. </param>
        /// <param name="allowOctal"> <c>true</c> if numbers with a leading zero should be parsed
        /// as octal numbers. </param>
        /// <returns> The result of parsing the string as a integer. </returns>
        internal static double ParseInt(string input, int radix, bool allowOctal)
        {
            var reader = new System.IO.StringReader(input);
            int digitCount = 0;

            // Skip whitespace and line terminators.
            while (IsWhiteSpaceOrLineTerminator(reader.Peek()))
                reader.Read();

            // Determine the sign.
            double sign = 1;
            if (reader.Peek() == '+')
            {
                reader.Read();
            }
            else if (reader.Peek() == '-')
            {
                sign = -1;
                reader.Read();
            }

            // Hex prefix should be stripped if the radix is 0, undefined or 16.
            bool stripPrefix = radix == 0 || radix == 16;

            // Default radix is 10.
            if (radix == 0)
                radix = 10;

            // Skip past the prefix, if there is one.
            if (stripPrefix == true)
            {
                if (reader.Peek() == '0')
                {
                    reader.Read();
                    digitCount = 1;     // Note: required for parsing "0z11" correctly (when radix = 0).

                    int c = reader.Peek();
                    if (c == 'x' || c == 'X')
                    {
                        // Hex number.
                        reader.Read();
                        radix = 16;
                    }

                    if (c >= '0' && c <= '9' && allowOctal == true)
                    {
                        // Octal number.
                        radix = 8;
                    }
                }
            }

            // Calculate the maximum number of digits before arbitrary precision arithmetic is
            // required.
            int maxDigits = (int)Math.Floor(53 / Math.Log(radix, 2));

            // Read numeric digits 0-9, a-z or A-Z.
            double result = 0;
            var bigResult = BigInteger.Zero;
            while (true)
            {
                int numericValue = -1;
                int c = reader.Read();
                if (c >= '0' && c <= '9')
                    numericValue = c - '0';
                if (c >= 'a' && c <= 'z')
                    numericValue = c - 'a' + 10;
                if (c >= 'A' && c <= 'Z')
                    numericValue = c - 'A' + 10;
                if (numericValue == -1 || numericValue >= radix)
                    break;
                if (digitCount == maxDigits)
                    bigResult = BigInteger.FromDouble(result);
                result = result * radix + numericValue;
                if (digitCount >= maxDigits)
                    bigResult = BigInteger.MultiplyAdd(bigResult, radix, numericValue);
                digitCount++;
            }

            // If the input is empty, then return NaN.
            if (digitCount == 0)
                return double.NaN;

            // Numbers with lots of digits require the use of arbitrary precision arithmetic to
            // determine the correct answer.
            if (digitCount > maxDigits)
                return RefineEstimate(result, 0, bigResult) * sign;

            return result * sign;
        }

        /// <summary>
        /// Parses a hexidecimal number and returns the corresponding double-precision value.
        /// </summary>
        /// <param name="reader"> The reader to read characters from. </param>
        /// <returns> The numeric value, or <c>NaN</c> if the number is invalid. </returns>
        internal static double ParseHex(TextReader reader)
        {
            double result = 0;
            int digitsRead = 0;

            // Read numeric digits 0-9, a-f or A-F.
            while (true)
            {
                int c = reader.Peek();
                if (c >= '0' && c <= '9')
                    result = result * 16 + c - '0';
                else if (c >= 'a' && c <= 'f')
                    result = result * 16 + c - 'a' + 10;
                else if (c >= 'A' && c <= 'F')
                    result = result * 16 + c - 'A' + 10;
                else
                    break;
                digitsRead++;
                reader.Read();
            }
            if (digitsRead == 0)
                return double.NaN;
            return result;
        }

        /// <summary>
        /// Parses a octal number and returns the corresponding double-precision value.
        /// </summary>
        /// <param name="reader"> The reader to read characters from. </param>
        /// <returns> The numeric value, or <c>NaN</c> if the number is invalid. </returns>
        internal static double ParseOctal(TextReader reader)
        {
            double result = 0;

            // Read numeric digits 0-7.
            while (true)
            {
                int c = reader.Peek();
                if (c >= '0' && c <= '7')
                    result = result * 8 + c - '0';
                else if (c == '8' || c == '9')
                    return double.NaN;
                else
                    break;
                reader.Read();
            }
            return result;
        }

        /// <summary>
        /// Determines if the given character is whitespace or a line terminator.
        /// </summary>
        /// <param name="c"> The unicode code point for the character. </param>
        /// <returns> <c>true</c> if the character is whitespace or a line terminator; <c>false</c>
        /// otherwise. </returns>
        private static bool IsWhiteSpaceOrLineTerminator(int c)
        {
            return c == 9 || c == 0x0b || c == 0x0c || c == ' ' || c == 0xa0 || c == 0xfeff ||
                c == 0x1680 || c == 0x180e || (c >= 0x2000 && c <= 0x200a) || c == 0x202f || c == 0x205f || c == 0x3000 ||
                c == 0x0a || c == 0x0d || c == 0x2028 || c == 0x2029;
        }

        /// <summary>
        /// Modifies the initial estimate until the closest double-precision number to the desired
        /// value is found.
        /// </summary>
        /// <param name="initialEstimate"> The initial estimate.  Assumed to be very close to the
        /// result. </param>
        /// <param name="base10Exponent"> The power-of-ten scale factor. </param>
        /// <param name="desiredValue"> The desired value, already scaled using the power-of-ten
        /// scale factor. </param>
        /// <returns> The closest double-precision number to the desired value.  If there are two
        /// such values, the one with the least significant bit set to zero is returned. </returns>
        private static double RefineEstimate(double initialEstimate, int base10Exponent, BigInteger desiredValue)
        {
            // Numbers with 16 digits or more are tricky because rounding error can cause the
            // result to be out by one or more ULPs (units in the last place).
            // The algorithm is as follows:
            // 1.  Use the initially calculated result as an estimate.
            // 2.  Create a second estimate by modifying the estimate by one ULP.
            // 3.  Calculate the actual value of both estimates to precision X (using arbitrary
            //     precision arithmetic).
            // 4.  If they are both above the desired value then decrease the estimates by 1
            //     ULP and goto step 3.
            // 5.  If they are both below the desired value then increase up the estimates by
            //     1 ULP and goto step 3.
            // 6.  One estimate must now be above the desired value and one below.
            // 7.  If one is estimate is clearly closer to the desired value than the other,
            //     then return that estimate.
            // 8.  Increase the precision by 32 bits.
            // 9.  If the precision is less than or equal to 160 bits goto step 3.
            // 10. Assume that the estimates are equally close to the desired value; return the
            //     value with the least significant bit equal to 0.
            int direction = double.IsPositiveInfinity(initialEstimate) ? -1 : 1;
            int precision = 32;

            // Calculate the candidate value by modifying the last bit.
            double result = initialEstimate;
            double result2 = AddUlps(result, direction);

            // Figure out our multiplier.  Either base10Exponent is positive, in which case we
            // multiply actual1 and actual2, or it's negative, in which case we multiply
            // desiredValue.
            BigInteger multiplier = BigInteger.One;
            if (base10Exponent < 0)
                multiplier = BigInteger.Pow(10, -base10Exponent);
            else if (base10Exponent > 0)
                desiredValue = BigInteger.Multiply(desiredValue, BigInteger.Pow(10, base10Exponent));

            while (precision <= 160)
            {
                // Scale the candidate values to a big integer.
                var actual1 = ScaleToInteger(result, multiplier, precision);
                var actual2 = ScaleToInteger(result2, multiplier, precision);

                // Calculate the differences between the candidate values and the desired value.
                var baseline = BigInteger.LeftShift(desiredValue, precision);
                var diff1 = BigInteger.Subtract(actual1, baseline);
                var diff2 = BigInteger.Subtract(actual2, baseline);

                if (diff1.Sign == direction && diff2.Sign == direction)
                {
                    // We're going the wrong way!
                    direction = -direction;
                    result2 = AddUlps(result, direction);
                }
                else if (diff1.Sign == -direction && diff2.Sign == -direction)
                {
                    // Going the right way, but need to go further.
                    result = result2;
                    result2 = AddUlps(result, direction);
                }
                else
                {
                    // Found two values that bracket the actual value.
                    // If one candidate value is closer to the actual value by at least 2 (one
                    // doesn't cut it because of the integer division) then use that value.
                    diff1 = BigInteger.Abs(diff1);
                    diff2 = BigInteger.Abs(diff2);
                    if (BigInteger.Compare(diff1, BigInteger.Subtract(diff2, BigInteger.One)) < 0)
                        return result;
                    if (BigInteger.Compare(diff2, BigInteger.Subtract(diff1, BigInteger.One)) < 0)
                        return result2;

                    // Not enough precision to determine the correct answer, or it's a halfway case.
                    // Increase the precision.
                    precision += 32;
                }

                // If result2 is NaN then we have gone too far.
                if (double.IsNaN(result2) == true)
                    return result;
            }

            // Even with heaps of precision there is no clear winner.
            // Assume this is a halfway case: choose the floating-point value with its least
            // significant bit equal to 0.
            return (BitConverter.DoubleToInt64Bits(result) & 1) == 0 ? result : result2;
        }

        /// <summary>
        /// Adds ULPs (units in the last place) to the given double-precision number.
        /// </summary>
        /// <param name="value"> The value to modify. </param>
        /// <param name="ulps"> The number of ULPs to add.  Can be negative. </param>
        /// <returns> The modified number. </returns>
        private static double AddUlps(double value, int ulps)
        {
            // Note: overflow or underflow in mantissa carries over to the exponent.
            // Overflow or underflow in exponent produces infinity or zero.
            return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(value) + ulps);
        }

        /// <summary>
        /// Scales the given double-precision number by multiplying and then shifting it.
        /// </summary>
        /// <param name="value"> The value to scale. </param>
        /// <param name="multiplier"> The multiplier. </param>
        /// <param name="shift"> The power of two scale factor. </param>
        /// <returns> A BigInteger containing the result of multiplying <paramref name="value"/> by
        /// <paramref name="multiplier"/> and then shifting left by <paramref name="shift"/> bits. </returns>
        private static BigInteger ScaleToInteger(double value, BigInteger multiplier, int shift)
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

            var result = new BigInteger(mantissa);
            result = BigInteger.Multiply(result, multiplier);
            shift += base2Exponent;
            result = BigInteger.LeftShift(result, shift);
            return result;
        }
    }

}
