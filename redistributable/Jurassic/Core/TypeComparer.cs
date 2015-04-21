using System;
using System.Collections.Generic;
using System.Text;

namespace Jurassic
{

    /// <summary>
    /// Implements the JavaScript equality and comparison rules.
    /// </summary>
    public static class TypeComparer
    {
        /// <summary>
        /// Compares two objects for equality.  Used by the equality operator (==).
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The first object to compare. </param>
        /// <returns> <c>true</c> if the objects are identical; <c>false</c> otherwise. </returns>
        public new static bool Equals(object x, object y)
        {
            x = x ?? Undefined.Value;
            y = y ?? Undefined.Value;
            if (x is int)
                x = (double)(int)x;
            if (x is uint)
                x = (double)(uint)x;
            if (y is int)
                y = (double)(int)y;
            if (y is uint)
                y = (double)(uint)y;
            if (x is ConcatenatedString)
                x = x.ToString();
            if (y is ConcatenatedString)
                y = y.ToString();
            if (x.GetType() == y.GetType())
            {
                if (x is double && double.IsNaN((double)x) == true)
                    return false;
                return object.Equals(x, y);
            }
            if ((x == Undefined.Value && y == Null.Value) || (x == Null.Value && y == Undefined.Value))
                return true;

            // 5.5 == "5.5"
            if (TypeUtilities.IsNumeric(x) && TypeUtilities.IsString(y))
                return TypeConverter.ToNumber(x) == TypeConverter.ToNumber(y);
            if (TypeUtilities.IsString(x) && TypeUtilities.IsNumeric(y))
                return TypeConverter.ToNumber(x) == TypeConverter.ToNumber(y);

            // false == 0, true == 1
            if (x is bool)
                return Equals(TypeConverter.ToNumber(x), y);
            if (y is bool)
                return Equals(x, TypeConverter.ToNumber(y));

            // false == new Boolean(false), 1.5 == new Number(1.5)
            if (TypeUtilities.IsNumeric(x) && y is Jurassic.Library.ObjectInstance)
                return Equals(x, TypeConverter.ToPrimitive(y, PrimitiveTypeHint.None));
            if (TypeUtilities.IsString(x) && y is Jurassic.Library.ObjectInstance)
                return Equals(x, TypeConverter.ToPrimitive(y, PrimitiveTypeHint.None));
            if (x is Jurassic.Library.ObjectInstance && TypeUtilities.IsNumeric(y))
                return Equals(TypeConverter.ToPrimitive(x, PrimitiveTypeHint.None), y);
            if (x is Jurassic.Library.ObjectInstance && TypeUtilities.IsString(y))
                return Equals(TypeConverter.ToPrimitive(x, PrimitiveTypeHint.None), y);

            return false;
        }

        /// <summary>
        /// Compares two objects for equality.  Used by the strict equality operator (===).
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <c>true</c> if the objects are identical; <c>false</c> otherwise. </returns>
        public static bool StrictEquals(object x, object y)
        {
            x = x ?? Undefined.Value;
            y = y ?? Undefined.Value;
            if (x is int)
                x = (double)(int)x;
            if (x is uint)
                x = (double)(uint)x;
            if (y is int)
                y = (double)(int)y;
            if (y is uint)
                y = (double)(uint)y;
            if (x is double && double.IsNaN((double)x) == true)
                return false;
            if (x is ConcatenatedString)
                x = x.ToString();
            if (y is ConcatenatedString)
                y = y.ToString();
            return object.Equals(x, y);
        }

        /// <summary>
        /// Determines the ordering of two objects.  Used by the less than operator (&lt;).
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <c>true</c> if <paramref name="x"/> is less than <paramref name="y"/>;
        /// <c>false</c> otherwise. </returns>
        public static bool LessThan(object x, object y)
        {
            x = TypeConverter.ToPrimitive(x, PrimitiveTypeHint.Number);
            y = TypeConverter.ToPrimitive(y, PrimitiveTypeHint.Number);

            if (x is ConcatenatedString)
                x = x.ToString();
            if (y is ConcatenatedString)
                y = y.ToString();

            if (x is string && y is string)
            {
                return string.CompareOrdinal((string)x, (string)y) < 0;
            }
            else
            {
                return TypeConverter.ToNumber(x) < TypeConverter.ToNumber(y);
            }
        }

        /// <summary>
        /// Determines the ordering of two objects.  Used by the less than or equal operator (&lt;=).
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <c>true</c> if <paramref name="x"/> is less than or equal to
        /// <paramref name="y"/>; <c>false</c> otherwise. </returns>
        public static bool LessThanOrEqual(object x, object y)
        {
            x = TypeConverter.ToPrimitive(x, PrimitiveTypeHint.Number);
            y = TypeConverter.ToPrimitive(y, PrimitiveTypeHint.Number);

            if (x is ConcatenatedString)
                x = x.ToString();
            if (y is ConcatenatedString)
                y = y.ToString();

            if (x is string && y is string)
            {
                return string.CompareOrdinal((string)x, (string)y) <= 0;
            }
            else
            {
                return TypeConverter.ToNumber(x) <= TypeConverter.ToNumber(y);
            }
        }

        /// <summary>
        /// Determines the ordering of two objects.  Used by the greater than operator (&gt;).
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <c>true</c> if <paramref name="x"/> is greater than <paramref name="y"/>;
        /// <c>false</c> otherwise. </returns>
        public static bool GreaterThan(object x, object y)
        {
            x = TypeConverter.ToPrimitive(x, PrimitiveTypeHint.Number);
            y = TypeConverter.ToPrimitive(y, PrimitiveTypeHint.Number);

            if (x is ConcatenatedString)
                x = x.ToString();
            if (y is ConcatenatedString)
                y = y.ToString();

            if (x is string && y is string)
            {
                return string.CompareOrdinal((string)x, (string)y) > 0;
            }
            else
            {
                return TypeConverter.ToNumber(x) > TypeConverter.ToNumber(y);
            }
        }

        /// <summary>
        /// Determines the ordering of two objects.  Used by the greater than or equal operator (&gt;=).
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <c>true</c> if <paramref name="x"/> is greater than or equal to
        /// <paramref name="y"/>; <c>false</c> otherwise. </returns>
        public static bool GreaterThanOrEqual(object x, object y)
        {
            x = TypeConverter.ToPrimitive(x, PrimitiveTypeHint.Number);
            y = TypeConverter.ToPrimitive(y, PrimitiveTypeHint.Number);

            if (x is ConcatenatedString)
                x = x.ToString();
            if (y is ConcatenatedString)
                y = y.ToString();

            if (x is string && y is string)
            {
                return string.CompareOrdinal((string)x, (string)y) >= 0;
            }
            else
            {
                return TypeConverter.ToNumber(x) >= TypeConverter.ToNumber(y);
            }
        }

        /// <summary>
        /// Implements the SameValue algorithm.
        /// </summary>
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        /// <returns> <c>true</c> if the objects are the same value according to the SameValue
        /// algorithm. </returns>
        /// <remarks>
        /// This algorithm differs from the StrictEquals algorithm in two ways:
        /// 1. NaN compares equal with itself
        /// 2. Negative zero is considered different from positive zero.
        /// </remarks>
        public static bool SameValue(object x, object y)
        {
            if (x == null)
                x = Undefined.Value;
            if (y == null)
                y = Undefined.Value;
            if (x is int)
                x = (double)(int)x;
            if (x is uint)
                x = (double)(uint)x;
            if (y is int)
                y = (double)(int)y;
            if (y is uint)
                y = (double)(uint)y;
            if (x is double && (double) x == 0.0 && y is double && (double)y == 0.0)
                if ((1 / (double)x) != (1 / (double)y))
                    return false;
            if (x is ConcatenatedString)
                x = x.ToString();
            if (y is ConcatenatedString)
                y = y.ToString();
            return object.Equals(x, y);
        }
    }
}
