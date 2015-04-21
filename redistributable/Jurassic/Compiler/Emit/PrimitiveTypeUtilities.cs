using System;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Methods related to the PrimitiveType enum.
    /// </summary>
    internal static class PrimitiveTypeUtilities
    {
        /// <summary>
        /// Converts a javascript primitive type to a .NET type.
        /// </summary>
        /// <param name="type"> The type to convert. </param>
        /// <returns> A .NET type. </returns>
        public static Type ToType(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Any:
                    return typeof(object);
                case PrimitiveType.Undefined:
                    return typeof(Undefined);
                case PrimitiveType.Null:
                    return typeof(Null);
                case PrimitiveType.Bool:
                    return typeof(bool);
                case PrimitiveType.Int32:
                    return typeof(int);
                case PrimitiveType.UInt32:
                    return typeof(uint);
                case PrimitiveType.Number:
                    return typeof(double);
                case PrimitiveType.String:
                    return typeof(string);
                case PrimitiveType.ConcatenatedString:
                    return typeof(ConcatenatedString);
                case PrimitiveType.Object:
                    return typeof(Library.ObjectInstance);
                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", type));
            }
        }

        /// <summary>
        /// Converts a .NET type to a javascript primitive type.
        /// </summary>
        /// <param name="type"> The type to convert. </param>
        /// <returns> A javascript primitive type. </returns>
        public static PrimitiveType ToPrimitiveType(Type type)
        {
            if (type == typeof(object))
                return PrimitiveType.Any;
            if (type == typeof(Undefined))
                return PrimitiveType.Undefined;
            if (type == typeof(Null))
                return PrimitiveType.Null;
            if (type == typeof(bool))
                return PrimitiveType.Bool;
            if (type == typeof(int))
                return PrimitiveType.Int32;
            if (type == typeof(uint))
                return PrimitiveType.UInt32;
            if (type == typeof(double))
                return PrimitiveType.Number;
            if (type == typeof(string))
                return PrimitiveType.String;
            if (type == typeof(ConcatenatedString))
                return PrimitiveType.ConcatenatedString;
            if (typeof(Library.ObjectInstance).IsAssignableFrom(type))
                return PrimitiveType.Object;
            throw new NotImplementedException(string.Format("Unsupported type: {0}", type));
        }

        /// <summary>
        /// Checks if the given primitive type is numeric.
        /// </summary>
        /// <param name="type"> The primitive type to check. </param>
        /// <returns> <c>true</c> if the given primitive type is numeric; <c>false</c> otherwise. </returns>
        public static bool IsNumeric(PrimitiveType type)
        {
            return type == PrimitiveType.Number || type == PrimitiveType.Int32 || type == PrimitiveType.UInt32;
        }

        /// <summary>
        /// Checks if the given primitive type is a string type.
        /// </summary>
        /// <param name="type"> The primitive type to check. </param>
        /// <returns> <c>true</c> if the given primitive type is a string type; <c>false</c>
        /// otherwise. </returns>
        public static bool IsString(PrimitiveType type)
        {
            return type == PrimitiveType.String || type == PrimitiveType.ConcatenatedString;
        }

        /// <summary>
        /// Checks if the given primitive type is a value type.
        /// </summary>
        /// <param name="type"> The primitive type to check. </param>
        /// <returns> <c>true</c> if the given primitive type is a value type; <c>false</c> otherwise. </returns>
        public static bool IsValueType(PrimitiveType type)
        {
            return type == PrimitiveType.Bool || type == PrimitiveType.Number || type == PrimitiveType.Int32 || type == PrimitiveType.UInt32;
        }

        /// <summary>
        /// Gets a type that can hold values of both the given types.
        /// </summary>
        /// <param name="a"> The first of the two types to find the LCD for. </param>
        /// <param name="b"> The second of the two types to find the LCD for. </param>
        /// <returns> A type that can hold values of both the given types. </returns>
        public static PrimitiveType GetCommonType(PrimitiveType a, PrimitiveType b)
        {
            // If the types are the same, then trivially that type will do.
            if (a == b)
                return a;

            // If both types are numeric, return the number type.
            if (IsNumeric(a) && IsNumeric(b))
                return PrimitiveType.Number;

            // Otherwise, fall back on the generic Any type.
            return PrimitiveType.Any;
        }
    }

}
