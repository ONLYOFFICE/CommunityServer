using System;

namespace Jurassic
{

    /// <summary>
    /// Converts numbers into strings.
    /// </summary>
    internal static class NumberFormatter
    {
        /// <summary>
        /// Used to specify the type of number formatting that should be applied.
        /// </summary>
        internal enum Style
        {
            /// <summary>
            /// Specifies that the shortest number that accurately represents the number should be
            /// displayed.  Scientific notation is used if the exponent is less than -6 or greater
            /// than twenty.  The precision parameter has no semantic meaning.
            /// </summary>
            Regular,

            /// <summary>
            /// Specifies that a fixed number of significant figures should be displayed (specified
            /// by the precision parameter).  If the number cannot be displayed using the given
            /// number of digits, scientific notation is used.
            /// </summary>
            Precision,

            /// <summary>
            /// Specifies that a fixed number of digits should be displayed after the decimal place
            /// (specified by the precision parameter).  Scientific notation is used if the
            /// exponent is greater than twenty.
            /// </summary>
            Fixed,
            
            /// <summary>
            /// Specifies that numbers should always be displayed in scientific notation.  The
            /// precision parameter specifies the number of figures to display after the decimal
            /// point.
            /// </summary>
            Exponential,
        }

        private const string exponentSymbol = "e";

        /// <summary>
        /// Converts a number to a string.
        /// </summary>
        /// <param name="value"> The value to convert to a string. </param>
        /// <param name="radix"> The base of the number system to convert to. </param>
        /// <param name="style"> The type of formatting to apply. </param>
        /// <param name="precision">
        /// This value is dependent on the formatting style:
        /// Regular - this value has no meaning.
        /// Precision - the number of significant figures to display.
        /// Fixed - the number of figures to display after the decimal point.
        /// Exponential - the number of figures to display after the decimal point.
        /// </param>
        internal static string ToString(double value, int radix, Style style, int precision = 0)
        {
            return ToString(value, radix, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, style, precision);
        }

        /// <summary>
        /// Converts a number to a string.
        /// </summary>
        /// <param name="value"> The value to convert to a string. </param>
        /// <param name="radix"> The base of the number system to convert to. </param>
        /// <param name="numberFormatInfo"> The number format style to use. </param>
        /// <param name="style"> The type of formatting to apply. </param>
        /// <param name="precision">
        /// This value is dependent on the formatting style:
        /// Regular - this value has no meaning.
        /// Precision - the number of significant figures to display.
        /// Fixed - the number of figures to display after the decimal point.
        /// Exponential - the number of figures to display after the decimal point.
        /// </param>
        internal static string ToString(double value, int radix, System.Globalization.NumberFormatInfo numberFormatInfo, Style style, int precision = 0)
        {
            // Handle NaN.
            if (double.IsNaN(value))
                return numberFormatInfo.NaNSymbol;  // "NaN"

            // Handle zero.
            if (value == 0.0)
            {
                switch (style)
                {
                    case Style.Regular:
                        return "0";
                    case Style.Precision:
                        return "0" + numberFormatInfo.NumberDecimalSeparator + new string('0', precision - 1);
                    case Style.Fixed:
                        if (precision == 0)
                            return "0";
                        return "0" + numberFormatInfo.NumberDecimalSeparator + new string('0', precision);
                    case Style.Exponential:
                        if (precision <= 0)
                            return "0" + exponentSymbol + numberFormatInfo.PositiveSign + "0";
                        return "0" + numberFormatInfo.NumberDecimalSeparator + new string('0', precision) + exponentSymbol + numberFormatInfo.PositiveSign + "0";
                }
            }

            var result = new System.Text.StringBuilder(18);

            // Handle negative numbers.
            if (value < 0.0)
            {
                value = -value;
                result.Append(numberFormatInfo.NegativeSign);
            }

            // Handle infinity.
            if (double.IsPositiveInfinity(value))
            {
                result.Append(numberFormatInfo.PositiveInfinitySymbol);     // "Infinity"
                return result.ToString();
            }

            // Extract the base-2 exponent.
            var bits = new DoubleBits() { DoubleValue = value };
            int base2Exponent = (int)(bits.LongValue >> MantissaExplicitBits);

            // Extract the mantissa.
            long mantissa = bits.LongValue & MantissaMask;

            // Correct the base-2 exponent.
            if (base2Exponent == 0)
            {
                // This is a denormalized number.
                base2Exponent = base2Exponent - ExponentBias - MantissaExplicitBits + 1;
            }
            else
            {
                // This is a normal number.
                base2Exponent = base2Exponent - ExponentBias - MantissaExplicitBits;

                // Add the implicit bit.
                mantissa |= MantissaImplicitBit;
            }

            // Remove any trailing zeros.
            int trailingZeroBits = CountTrailingZeroBits((ulong)mantissa);
            mantissa >>= trailingZeroBits;
            base2Exponent += trailingZeroBits;

            // Calculate the logarithm of the number.
            int exponent;
            if (radix == 10)
            {
                exponent = (int)Math.Floor(Math.Log10(value));

                // We need to calculate k = floor(log10(x)).
                // log(x)	~=~ log(1.5) + (x-1.5)/1.5 (taylor series approximation)
                // log10(x) ~=~ log(1.5) / log(10) + (x - 1.5) / (1.5 * log(10))
                // d = x * 2^l (1 <= x < 2)
                // log10(d) = l * log10(2) + log10(x)
                // log10(d) ~=~ l * log10(2)           + (x - 1.5) * (1 / (1.5 * log(10)))  + log(1.5) / log(10)
                // log10(d) ~=~ l * 0.301029995663981  + (x - 1.5) * 0.289529654602168      + 0.1760912590558
                // The last term (0.1760912590558) is rounded so that k = floor(log10(x)) or
                // k = floor(log10(x)) + 1 (i.e. it's the exact value or one higher).


                //double log10;
                //if ((int)(bits.LongValue >> MantissaExplicitBits) == 0)
                //{
                //    // The number is denormalized.
                //    int mantissaShift = CountLeadingZeroBits((ulong)mantissa) - (64 - MantissaImplicitBits);
                //    bits.LongValue = (mantissa << mantissaShift) & MantissaMask |
                //        ((long)ExponentBias << MantissaExplicitBits);

                //    // Calculate an overestimate of log-10 of the value.
                //    log10 = (bits.DoubleValue - 1.5) * 0.289529654602168 + 0.1760912590558 +
                //        (base2Exponent - mantissaShift) * 0.301029995663981;
                //}
                //else
                //{
                //    // Set the base-2 exponent to biased zero.
                //    bits.LongValue = (bits.LongValue & ~ExponentMask) | ((long)ExponentBias << MantissaExplicitBits);

                //    // Calculate an overestimate of log-10 of the value.
                //    log10 = (bits.DoubleValue - 1.5) * 0.289529654602168 + 0.1760912590558 + base2Exponent * 0.301029995663981;
                //}

                //// (int)Math.Floor(log10)
                //exponent = (int)log10;
                //if (log10 < 0 && log10 != exponent)
                //    exponent--;

                //if (exponent >= 0 && exponent < tens.Length)
                //{
                //    if (value < tens[exponent])
                //        exponent--;
                //}
            }
            else
                exponent = (int)Math.Floor(Math.Log(value, radix));

            if (radix == 10 && style == Style.Regular)
            {
                // Do we have a small integer?
                if (base2Exponent >= 0 && exponent <= 14)
                {
                    // Yes.
                    for (int i = exponent; i >= 0; i--)
                    {
                        double scaleFactor = tens[i];
                        int digit = (int)(value / scaleFactor);
                        result.Append((char)(digit + '0'));
                        value -= digit * scaleFactor;
                    }
                    return result.ToString();
                }
            }

            // toFixed acts like toString() if the exponent is >= 21.
            if (style == Style.Fixed && exponent >= 21)
                style = Style.Regular;

            // Calculate the exponent thresholds.
            int lowExponentThreshold = int.MinValue;
            if (radix == 10 && style != Style.Fixed)
                lowExponentThreshold = -7;
            if (style == Style.Exponential)
                lowExponentThreshold = -1;
            int highExponentThreshold = int.MaxValue;
            if (radix == 10 && style == Style.Regular)
                highExponentThreshold = 21;
            if (style == Style.Precision)
                highExponentThreshold = precision;
            if (style == Style.Exponential)
                highExponentThreshold = 0;

            // Calculate the number of bits per digit.
            double bitsPerDigit = radix == 10 ? 3.322 : Math.Log(radix, 2);

            // Calculate the maximum number of digits to output.
            // We add 7 so that there is enough precision to distinguish halfway numbers.
            int maxDigitsToOutput = radix == 10 ? 22 : (int)Math.Floor(53 / bitsPerDigit) + 7;

            // Calculate the number of integral digits, or if negative, the number of zeros after
            // the decimal point.
            int integralDigits = exponent + 1;

            // toFixed with a low precision causes rounding.
            if (style == Style.Fixed && precision <= -integralDigits)
            {
                int diff = (-integralDigits) - (precision - 1);
                maxDigitsToOutput += diff;
                exponent += diff;
                integralDigits += diff;
            }

            // Output any leading zeros.
            bool decimalPointOutput = false;
            if (integralDigits <= 0 && integralDigits > lowExponentThreshold + 1)
            {
                result.Append('0');
                if (integralDigits < 0)
                {
                    result.Append(numberFormatInfo.NumberDecimalSeparator);
                    decimalPointOutput = true;
                    result.Append('0', -integralDigits);
                }
            }

            // We need to calculate the integers "scaledValue" and "divisor" such that:
            // value = scaledValue / divisor * 10 ^ exponent
            // 1 <= scaledValue / divisor < 10

            BigInteger scaledValue = new BigInteger(mantissa);
            BigInteger divisor = BigInteger.One;
            BigInteger multiplier = BigInteger.One;
            if (exponent > 0)
            {
                // Number is >= 10.
                divisor = BigInteger.Multiply(divisor, BigInteger.Pow(radix, exponent));
            }
            else if (exponent < 0)
            {
                // Number is < 1.
                multiplier = BigInteger.Pow(radix, -exponent);
                scaledValue = BigInteger.Multiply(scaledValue, multiplier);
            }

            // Scale the divisor so it is 74 bits ((21 digits + 1 digit for rounding) * approx 3.322 bits per digit).
            int powerOfTwoScaleFactor = (radix == 10 ? 74 : (int)Math.Ceiling(maxDigitsToOutput * bitsPerDigit)) - divisor.BitCount;
            divisor = BigInteger.LeftShift(divisor, powerOfTwoScaleFactor);
            scaledValue = BigInteger.LeftShift(scaledValue, powerOfTwoScaleFactor + base2Exponent);

            // Calculate the error.
            BigInteger errorDelta = BigInteger.Zero;
            int errorPowerOfTen = int.MinValue;
            switch (style)
            {
                case Style.Regular:
                    errorDelta = ScaleToInteger(CalculateError(value), multiplier, powerOfTwoScaleFactor - 1);
                    break;
                case Style.Precision:
                    errorPowerOfTen = integralDigits - precision;
                    break;
                case Style.Fixed:
                    errorPowerOfTen = -precision;
                    break;
                case Style.Exponential:
                    if (precision < 0)
                        errorDelta = ScaleToInteger(CalculateError(value), multiplier, powerOfTwoScaleFactor - 1);
                    else
                        errorPowerOfTen = integralDigits - precision - 1;
                    break;
                default:
                    throw new ArgumentException("Unknown formatting style.", "style");
            }
            if (errorPowerOfTen != int.MinValue)
            {
                errorDelta = multiplier;
                if (errorPowerOfTen > 0)
                    errorDelta = BigInteger.Multiply(errorDelta, BigInteger.Pow(radix, errorPowerOfTen));
                errorDelta = BigInteger.LeftShift(errorDelta, powerOfTwoScaleFactor - 1);
                if (errorPowerOfTen < 0)
                {
                    // We would normally divide by the power of 10 here, but division is extremely
                    // slow so we multiply everything else instead.
                    //errorDelta = BigInteger.Divide(errorDelta, BigInteger.Pow(radix, -errorPowerOfTen));
                    var errorPowerOfTenMultiplier = BigInteger.Pow(radix, -errorPowerOfTen);
                    scaledValue = BigInteger.Multiply(scaledValue, errorPowerOfTenMultiplier);
                    divisor = BigInteger.Multiply(divisor, errorPowerOfTenMultiplier);
                    BigInteger.SetupQuorum(ref scaledValue, ref divisor, ref errorDelta);
                }
            }

            // Shrink the error in the case where ties are resolved towards the value with the 
            // least significant bit set to zero.
            if ((BitConverter.DoubleToInt64Bits(value) & 1) == 1)
                errorDelta.InPlaceDecrement();

            // Cache half the divisor.
            BigInteger halfDivisor = BigInteger.RightShift(divisor, 1);

            // Output the digits.
            int zeroCount = 0;
            int digitsOutput = 0;
            bool rounded = false, scientificNotation = false;
            for (; digitsOutput < maxDigitsToOutput && rounded == false; digitsOutput++)
            {
                // Calculate the next digit.
                var digit = (int)BigInteger.Quorem(ref scaledValue, divisor);

                if (BigInteger.Compare(scaledValue, errorDelta) <= 0 && BigInteger.Compare(scaledValue, halfDivisor) < 0)
                {
                    // Round down.
                    rounded = true;
                }
                else if (BigInteger.Compare(BigInteger.Subtract(divisor, scaledValue), errorDelta) <= 0)
                {
                    // Round up.
                    rounded = true;
                    digit++;
                    if (digit == radix)
                    {
                        digit = 1;
                        exponent++;
                        integralDigits++;
                    }
                }

                if (digit > 0 || decimalPointOutput == false)
                {
                    // Check if the decimal point should be output.
                    if (decimalPointOutput == false && (scientificNotation == true || digitsOutput == integralDigits))
                    {
                        result.Append(numberFormatInfo.NumberDecimalSeparator);
                        decimalPointOutput = true;
                    }

                    // Output any pent-up zeros.
                    if (zeroCount > 0)
                    {
                        result.Append('0', zeroCount);
                        zeroCount = 0;
                    }

                    // Output the next digit.
                    if (digit < 10)
                        result.Append((char)(digit + '0'));
                    else
                        result.Append((char)(digit - 10 + 'a'));
                }
                else
                    zeroCount++;

                // Check whether the number should be displayed in scientific notation (we cannot
                // determine this up front for large exponents because the number might get rounded
                // up to cross the threshold).
                if (digitsOutput == 0 && (exponent <= lowExponentThreshold || exponent >= highExponentThreshold))
                    scientificNotation = true;

                scaledValue = BigInteger.MultiplyAdd(scaledValue, radix, 0);
                errorDelta = BigInteger.MultiplyAdd(errorDelta, radix, 0);
            }

            // Add any extra zeros on the end, if necessary.
            if (scientificNotation == false && integralDigits > digitsOutput)
            {
                result.Append('0', integralDigits - digitsOutput);
                digitsOutput = integralDigits;
            }

            // Most of the styles output redundent zeros.
            int redundentZeroCount = 0;
            switch (style)
            {
                case Style.Precision:
                    redundentZeroCount = zeroCount + precision - digitsOutput;
                    break;
                case Style.Fixed:
                    redundentZeroCount = precision - (digitsOutput - zeroCount - integralDigits);
                    break;
                case Style.Exponential:
                    redundentZeroCount = precision - (digitsOutput - zeroCount) + 1;
                    break;
            }
            if (redundentZeroCount > 0)
            {
                if (decimalPointOutput == false)
                    result.Append(numberFormatInfo.NumberDecimalSeparator);
                result.Append('0', redundentZeroCount);
            }

            if (scientificNotation == true)
            {
                // Add the exponent on the end.
                result.Append(exponentSymbol);
                if (exponent > 0)
                    result.Append(numberFormatInfo.PositiveSign);
                result.Append(exponent);
            }

            return result.ToString();
        }



        //     CONSTANTS
        //_________________________________________________________________________________________


        private readonly static int[] powersOfFive = { 5, 25, 125 };

        // IEEE 754 double-precision constants.
        private const int MantissaExplicitBits = 52;
        private const int MantissaImplicitBits = 53;
        private const long MantissaMask = 0xFFFFFFFFFFFFF;
        private const long MantissaImplicitBit = 1L << MantissaExplicitBits;
        private const int ExponentBias = 1023;
        private const long ExponentMask = 0x7FF0000000000000;
        private const int ExponentDenormal = -1023;
        private const int ExponentSpecial = 1024;

        // Powers of ten.
        private readonly static double[] tens = new double[]
        {
		    1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9,
		    1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19,
		    1e20, 1e21, 1e22
        };



        //     PRIVATE HELPER METHODS
        //_________________________________________________________________________________________

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct DoubleBits
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public double DoubleValue;

            [System.Runtime.InteropServices.FieldOffset(0)]
            public long LongValue;
        }

        /// <summary>
        /// Calculates the minimum increment that creates a number distinct from the value that was
        /// provided.  The error for the number is plus or minus half the result of this method
        /// (note that the number returned by this method may be so small that dividing it by two
        /// produces zero).
        /// </summary>
        /// <param name="value"> The number to calculate an error value for. </param>
        /// <returns> The minimum increment that creates a number distinct from the value that was
        /// provided. </returns>
        private static double CalculateError(double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);

            // Extract the base-2 exponent.
            var base2Exponent = (int)((bits & 0x7FF0000000000000) >> 52);

            // Handle denormals.
            if (base2Exponent == 0)
                return double.Epsilon;

            // Handle very small numbers.
            if (base2Exponent < 53)
                return BitConverter.Int64BitsToDouble(1L << (base2Exponent - 1));

            // Subtract 52 from the exponent to get the error (52 is the number of bits in the mantissa).
            return BitConverter.Int64BitsToDouble((long)(base2Exponent - 52) << 52);
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

        /// <summary>
        /// Counts the number of leading zero bits in the given 64-bit value.
        /// </summary>
        /// <param name="value"> The 64-bit value. </param>
        /// <returns> The number of leading zero bits in the given 64-bit value. </returns>
        private static int CountLeadingZeroBits(ulong value)
        {
            int k = 0;

            if ((value & 0xFFFFFFFF00000000) == 0)
            {
                k = 32;
                value <<= 32;
            }
            if ((value & 0xFFFF000000000000) == 0)
            {
                k += 16;
                value <<= 16;
            }
            if ((value & 0xFF00000000000000) == 0)
            {
                k += 8;
                value <<= 8;
            }
            if ((value & 0xF000000000000000) == 0)
            {
                k += 4;
                value <<= 4;
            }
            if ((value & 0xC000000000000000) == 0)
            {
                k += 2;
                value <<= 2;
            }
            if ((value & 0x8000000000000000) == 0)
            {
                k++;
                if ((value & 0x4000000000000000) == 0)
                    return 64;
            }
            return k;
        }

        /// <summary>
        /// Counts the number of trailing zero bits in the given 64-bit value.
        /// </summary>
        /// <param name="value"> The 64-bit value. </param>
        /// <returns> The number of trailing zero bits in the given 64-bit value. </returns>
        private static int CountTrailingZeroBits(ulong value)
        {
            int k = 0;
            if ((value & 7) != 0)
            {
                if ((value & 1) != 0)
                    return 0;
                if ((value & 2) != 0)
                {
                    value >>= 1;
                    return 1;
                }
                value >>= 2;
                return 2;
            }
            k = 0;
            if ((value & 0xFFFFFFFF) == 0)
            {
                k = 32;
                value >>= 32;
            }
            if ((value & 0xFFFF) == 0)
            {
                k += 16;
                value >>= 16;
            }
            if ((value & 0xFF) == 0)
            {
                k += 8;
                value >>= 8;
            }
            if ((value & 0xF) == 0)
            {
                k += 4;
                value >>= 4;
            }
            if ((value & 0x3) == 0)
            {
                k += 2;
                value >>= 2;
            }
            if ((value & 1) == 0)
            {
                k++;
                value >>= 1;
                if (value == 0)
                    return 32;
            }
            return k;
        }


    }

}
