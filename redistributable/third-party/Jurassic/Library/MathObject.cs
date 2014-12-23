using System;
using System.Collections.Generic;
using System.Text;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the built-in Math class that has mathematical constants and functions.
    /// </summary>
    [Serializable]
    public class MathObject : ObjectInstance
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Math object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal MathObject(ObjectInstance prototype)
            : base(prototype)
        {
        }



        //     .NET ACCESSOR PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the internal class name of the object.  Used by the default toString()
        /// implementation.
        /// </summary>
        protected override string InternalClassName
        {
            get { return "Math"; }
        }



        //     JAVASCRIPT FIELDS
        //_________________________________________________________________________________________

        /// <summary>
        /// The mathematical constant E, approximately 2.718.
        /// </summary>
        [JSField]
        public const double E       = 2.7182818284590452;

        /// <summary>
        /// The natural logarithm of 2, approximately 0.693.
        /// </summary>
        [JSField]
        public const double LN2     = 0.6931471805599453;

        /// <summary>
        /// The natural logarithm of 10, approximately 2.303.
        /// </summary>
        [JSField]
        public const double LN10    = 2.3025850929940456;

        /// <summary>
        /// The base 2 logarithm of E, approximately 1.442.
        /// </summary>
        [JSField]
        public const double LOG2E   = 1.4426950408889634;

        /// <summary>
        /// The base 10 logarithm of E, approximately 0.434.
        /// </summary>
        [JSField]
        public const double LOG10E  = 0.4342944819032518;

        /// <summary>
        /// The ratio of the circumference of a circle to its diameter, approximately 3.14159.
        /// </summary>
        [JSField]
        public const double PI      = 3.1415926535897932;

        /// <summary>
        /// The square root of 0.5, approximately 0.707.
        /// </summary>
        [JSField]
        public const double SQRT1_2 = 0.7071067811865475;

        /// <summary>
        /// The square root of 2, approximately 1.414.
        /// </summary>
        [JSField]
        public const double SQRT2   = 1.4142135623730950;



        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Returns the absolute value of a number.
        /// </summary>
        /// <param name="number"> The number to operate on. </param>
        /// <returns> The absolute value of the <paramref name="number"/> parameter. </returns>
        [JSInternalFunction(Name = "abs")]
        public static double Abs(double number)
        {
            return System.Math.Abs(number);
        }

        /// <summary>
        /// Returns the arccosine of a number.
        /// </summary>
        /// <param name="number"> The number to operate on. </param>
        /// <returns> The arccosine of the <paramref name="number"/> parameter.  If
        /// <paramref name="number"/> is less than -1 or greater than 1, then <c>NaN</c> is
        /// returned. </returns>
        [JSInternalFunction(Name = "acos")]
        public static double Acos(double number)
        {
            return System.Math.Acos(number);
        }

        /// <summary>
        /// Returns the arcsine of a number.
        /// </summary>
        /// <param name="number"> The number to operate on. </param>
        /// <returns> The arcsine of the <paramref name="number"/> parameter. If
        /// <paramref name="number"/> is less than -1 or greater than 1, then <c>NaN</c> is
        /// returned. </returns>
        [JSInternalFunction(Name = "asin")]
        public static double Asin(double number)
        {
            return System.Math.Asin(number);
        }

        /// <summary>
        /// Returns the arctangent of a number.
        /// </summary>
        /// <param name="number"> The number to operate on. </param>
        /// <returns> The arctangent of the <paramref name="number"/> parameter. If
        /// <paramref name="number"/> is less than -1 or greater than 1, then <c>NaN</c> is
        /// returned. </returns>
        [JSInternalFunction(Name = "atan")]
        public static double Atan(double number)
        {
            return System.Math.Atan(number);
        }

        /// <summary>
        /// Returns the counter-clockwise angle (in radians) from the X axis to the point (x,y).
        /// </summary>
        /// <param name="number"> A numeric expression representing the cartesian x-coordinate. </param>
        /// <param name="number"> A numeric expression representing the cartesian y-coordinate. </param>
        /// <returns> The angle (in radians) from the X axis to a point (x,y) (between -pi and pi). </returns>
        [JSInternalFunction(Name = "atan2")]
        public static double Atan2(double y, double x)
        {
            if (double.IsInfinity(y) || double.IsInfinity(x))
            {
                if (double.IsPositiveInfinity(y) && double.IsPositiveInfinity(x))
                    return PI / 4.0;
                if (double.IsPositiveInfinity(y) && double.IsNegativeInfinity(x))
                    return 3.0 * PI / 4.0;
                if (double.IsNegativeInfinity(y) && double.IsPositiveInfinity(x))
                    return -PI / 4.0;
                if (double.IsNegativeInfinity(y) && double.IsNegativeInfinity(x))
                    return -3.0 * PI / 4.0;
            }
            return System.Math.Atan2(y, x);
        }

        /// <summary>
        /// Returns the smallest integer greater than or equal to a number.
        /// </summary>
        /// <param name="number"> The number to operate on. </param>
        /// <returns> The smallest integer greater than or equal to the <paramref name="number"/>
        /// parameter. </returns>
        [JSInternalFunction(Name = "ceil")]
        public static double Ceil(double number)
        {
            return System.Math.Ceiling(number);
        }

        /// <summary>
        /// Returns the cosine of an angle.
        /// </summary>
        /// <param name="angle"> The angle to operate on. </param>
        /// <returns> The cosine of the <paramref name="angle"/> parameter (between -1 and 1). </returns>
        [JSInternalFunction(Name = "cos")]
        public static double Cos(double angle)
        {
            return System.Math.Cos(angle);
        }

        /// <summary>
        /// Returns e (the base of natural logarithms) raised to the specified power.
        /// </summary>
        /// <param name="number"> The exponent. </param>
        /// <returns> E (the base of natural logarithms) raised to the specified power. </returns>
        [JSInternalFunction(Name = "exp")]
        public static double Exp(double number)
        {
            return System.Math.Exp(number);
        }

        /// <summary>
        /// Returns the greatest integer less than or equal to a number.
        /// </summary>
        /// <param name="number"> The number to operate on. </param>
        /// <returns> The greatest integer less than or equal to the <paramref name="number"/> parameter. </returns>
        [JSInternalFunction(Name = "floor")]
        public static double Floor(double number)
        {
            return System.Math.Floor(number);
        }

        /// <summary>
        /// Returns the natural logarithm of a number.
        /// </summary>
        /// <param name="number"> The number to operate on. </param>
        /// <returns> The natural logarithm of the <paramref name="number"/> parameter. </returns>
        [JSInternalFunction(Name = "log")]
        public static double Log(double number)
        {
            return System.Math.Log(number);
        }

        /// <summary>
        /// Returns the largest of zero or more numbers.
        /// </summary>
        /// <param name="numbers"> The numbers to operate on. </param>
        /// <returns> The largest of zero or more numbers.  If no arguments are provided, the
        /// return value is equal to NEGATIVE_INFINITY.  If any of the arguments cannot be
        /// converted to a number, the return value is NaN. </returns>
        [JSInternalFunction(Name = "max", Length = 2)]
        public static double Max(params double[] numbers)
        {
            double result = double.NegativeInfinity;
            foreach (double number in numbers)
                if (number > result || double.IsNaN(number))
                    result = number;
            return result;
        }

        /// <summary>
        /// Returns the smallest of zero or more numbers.
        /// </summary>
        /// <param name="numbers"> The numbers to operate on. </param>
        /// <returns> The smallest of zero or more numbers.  If no arguments are provided, the
        /// return value is equal to NEGATIVE_INFINITY.  If any of the arguments cannot be
        /// converted to a number, the return value is NaN. </returns>
        [JSInternalFunction(Name = "min", Length = 2)]
        public static double Min(params double[] numbers)
        {
            double result = double.PositiveInfinity;
            foreach (double number in numbers)
                if (number < result || double.IsNaN(number))
                    result = number;
            return result;
        }

        /// <summary>
        /// Returns the value of a base expression taken to a specified power.
        /// </summary>
        /// <param name="base"> The base value of the expression. </param>
        /// <param name="exponent"> The exponent value of the expression. </param>
        /// <returns> The value of the base expression taken to the specified power. </returns>
        [JSInternalFunction(Name = "pow")]
        public static double Pow(double @base, double exponent)
        {
            if (@base == 1.0 && double.IsInfinity(exponent))
                return double.NaN;
            if (double.IsNaN(@base) && exponent == 0.0)
                return 1.0;
            return System.Math.Pow(@base, exponent);
        }

        private static object randomNumberGeneratorLock = new object();
        private static Random randomNumberGenerator;

        /// <summary>
        /// Returns a pseudorandom number between 0 and 1.
        /// </summary>
        /// <returns> A pseudorandom number between 0 and 1.  The pseudorandom number generated is
        /// from 0 (inclusive) to 1 (exclusive), that is, the returned number can be zero, but it
        /// will always be less than one. The random number generator is seeded automatically.
        /// </returns>
        [JSInternalFunction(Name = "random")]
        public static double Random()
        {
            lock (randomNumberGeneratorLock)
            {
                if (randomNumberGenerator == null)
                    randomNumberGenerator = new Random();
                return randomNumberGenerator.NextDouble();
            }
        }

        /// <summary>
        /// Returns the value of a number rounded to the nearest integer.
        /// </summary>
        /// <param name="number"> The number to operate on. </param>
        /// <returns> The required number argument is the value to be rounded to the nearest
        /// integer.  For positive numbers, if the decimal portion of number is 0.5 or greater,
        /// the return value is equal to the smallest integer greater than number. If the decimal
        /// portion is less than 0.5, the return value is the largest integer less than or equal to
        /// number.  For negative numbers, if the decimal portion is exactly -0.5, the return value
        /// is the smallest integer that is greater than the number.  For example, Math.round(8.5)
        /// returns 9, but Math.round(-8.5) returns -8. </returns>
        [JSInternalFunction(Name = "round")]
        public static double Round(double number)
        {
            if (number > 0.0)
                return System.Math.Floor(number + 0.5);
            if (number >= -0.5)
            {
                // BitConverter is used to distinguish positive and negative zero.
                if (BitConverter.DoubleToInt64Bits(number) == 0L)
                    return 0.0;
                return -0.0;
            }
            return System.Math.Floor(number + 0.5);
        }

        /// <summary>
        /// Returns the sine of an angle.
        /// </summary>
        /// <param name="angle"> The angle, in radians. </param>
        /// <returns> The sine of the <paramref name="angle"/> parameter (between -1 and 1). </returns>
        [JSInternalFunction(Name = "sin")]
        public static double Sin(double angle)
        {
            return System.Math.Sin(angle);
        }

        /// <summary>
        /// Returns the square root of a number.
        /// </summary>
        /// <param name="number"> The number to operate on. </param>
        /// <returns> The square root of the <paramref name="number"/> parameter. </returns>
        [JSInternalFunction(Name = "sqrt")]
        public static double Sqrt(double number)
        {
            return System.Math.Sqrt(number);
        }

        /// <summary>
        /// Returns the tangent of an angle.
        /// </summary>
        /// <param name="angle"> The angle, in radians. </param>
        /// <returns> The tangent of the <paramref name="angle"/> parameter (between -1 and 1). </returns>
        [JSInternalFunction(Name = "tan")]
        public static double Tan(double angle)
        {
            return System.Math.Tan(angle);
        }
    }
}
