using System;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Outputs IL that converts between types.
    /// </summary>
    internal static class EmitConversion
    {
        /// <summary>
        /// Pops the value on the stack, converts it to the given type, then pushes the result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        /// <param name="toType"> The type to convert to. </param>
        public static void Convert(ILGenerator generator, PrimitiveType fromType, PrimitiveType toType)
        {
            Convert(generator, fromType, toType, null, null, 0);
        }

        /// <summary>
        /// Pops the value on the stack, converts it to the given type, then pushes the result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        /// <param name="toType"> The type to convert to. </param>
        /// <param name="optimizationInfo"> Information about the line number, function and path. </param>
        public static void Convert(ILGenerator generator, PrimitiveType fromType, PrimitiveType toType, OptimizationInfo optimizationInfo)
        {
            Convert(generator, fromType, toType, optimizationInfo.Source.Path, optimizationInfo.FunctionName, optimizationInfo.SourceSpan.StartLine);
        }

        /// <summary>
        /// Pops the value on the stack, converts it to the given type, then pushes the result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        /// <param name="toType"> The type to convert to. </param>
        /// <param name="path"> The path of the javascript source file that is currently executing. </param>
        /// <param name="function"> The name of the currently executing function. </param>
        /// <param name="line"> The line number of the statement that is currently executing. </param>
        public static void Convert(ILGenerator generator, PrimitiveType fromType, PrimitiveType toType, string path, string function, int line)
        {
            // Check that a conversion is actually necessary.
            if (fromType == toType)
                return;

            switch (toType)
            {
                case PrimitiveType.Any:
                    ToAny(generator, fromType);
                    break;

                case PrimitiveType.Undefined:
                    generator.Pop();
                    EmitHelpers.EmitUndefined(generator);
                    break;

                case PrimitiveType.Null:
                    generator.Pop();
                    EmitHelpers.EmitNull(generator);
                    break;

                case PrimitiveType.Bool:
                    ToBool(generator, fromType);
                    break;

                case PrimitiveType.Int32:
                    ToInt32(generator, fromType);
                    break;

                case PrimitiveType.UInt32:
                    ToUInt32(generator, fromType);
                    break;

                case PrimitiveType.Number:
                    ToNumber(generator, fromType);
                    break;

                case PrimitiveType.String:
                    ToString(generator, fromType);
                    break;

                case PrimitiveType.ConcatenatedString:
                    ToConcatenatedString(generator, fromType);
                    break;

                case PrimitiveType.Object:
                    ToObject(generator, fromType, path, function, line);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", toType));
            }
        }

        /// <summary>
        /// Pops the value on the stack, converts it to a boolean, then pushes the boolean result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        public static void ToBool(ILGenerator generator, PrimitiveType fromType)
        {
            // Check that a conversion is actually necessary.
            if (fromType == PrimitiveType.Bool)
                return;

            switch (fromType)
            {
                case PrimitiveType.Undefined:
                case PrimitiveType.Null:
                    // Converting from undefined or null produces false.
                    generator.Pop();
                    generator.LoadInt32(0);
                    break;

                case PrimitiveType.Int32:
                case PrimitiveType.UInt32:
                    // Converting from an integer produces true if the integer is non-zero.
                    generator.LoadInt32(0);
                    generator.CompareGreaterThanUnsigned();
                    break;

                case PrimitiveType.Number:
                    // Converting from a number produces true if the number is non-zero and not NaN.
                    var temp = generator.CreateTemporaryVariable(fromType);
                    generator.StoreVariable(temp);

                    // input != 0
                    generator.LoadVariable(temp);
                    generator.LoadDouble(0.0);
                    generator.CompareEqual();
                    generator.LoadInt32(0);
                    generator.CompareEqual();

                    // input == input
                    generator.LoadVariable(temp);
                    generator.Duplicate();
                    generator.CompareEqual();

                    // &&
                    generator.CompareEqual();

                    // The temporary variable is no longer needed.
                    generator.ReleaseTemporaryVariable(temp);
                    break;

                case PrimitiveType.String:
                    // Converting from a string produces true if the string is not empty.
                    generator.Call(ReflectionHelpers.String_Length);
                    generator.LoadInt32(0);
                    generator.CompareGreaterThan();
                    break;

                case PrimitiveType.ConcatenatedString:
                    // Converting from a string produces true if the string is not empty.
                    generator.Call(ReflectionHelpers.ConcatenatedString_Length);
                    generator.LoadInt32(0);
                    generator.CompareGreaterThan();
                    break;

                case PrimitiveType.Any:
                case PrimitiveType.Object:
                    // Otherwise, fall back to calling TypeConverter.ToBoolean()
                    generator.Call(ReflectionHelpers.TypeConverter_ToBoolean);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", fromType));
            }
        }

        /// <summary>
        /// Pops the value on the stack, converts it to an integer, then pushes the integer result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        public static void ToInteger(ILGenerator generator, PrimitiveType fromType)
        {
            // Check that a conversion is actually necessary.
            if (fromType == PrimitiveType.Int32 || fromType == PrimitiveType.UInt32 || fromType == PrimitiveType.Bool)
                return;

            switch (fromType)
            {
                case PrimitiveType.Undefined:
                case PrimitiveType.Null:
                    // Converting from undefined or null produces 0.
                    generator.Pop();
                    generator.LoadInt32(0);
                    break;

                case PrimitiveType.Number:
                    // Converting from a number produces the following:
                    // Any number between -2147483648 and +2147483647 -> itself
                    // Any number smaller than -2147483648 -> -2147483648
                    // Any number larger than +2147483647 -> +2147483647
                    // NaN -> 0

                    // bool isPositiveInfinity = input > 2147483647.0
                    var isPositiveInfinity = generator.CreateTemporaryVariable(typeof(bool));
                    generator.Duplicate();
                    generator.LoadDouble(2147483647.0);
                    generator.CompareGreaterThan();
                    generator.StoreVariable(isPositiveInfinity);

                    // bool notNaN = input == input
                    var notNaN = generator.CreateTemporaryVariable(typeof(bool));
                    generator.Duplicate();
                    generator.Duplicate();
                    generator.CompareEqual();
                    generator.StoreVariable(notNaN);

                    // input = (int)input
                    // Infinity -> -2147483648
                    // -Infinity -> -2147483648
                    // NaN -> -2147483648
                    generator.ConvertToInteger();

                    // input = input & -((int)notNaN)
                    generator.LoadVariable(notNaN);
                    generator.Negate();
                    generator.BitwiseAnd();

                    // input = input - (int)isPositiveInfinity
                    generator.LoadVariable(isPositiveInfinity);
                    generator.Subtract();

                    // The temporary variables are no longer needed.
                    generator.ReleaseTemporaryVariable(notNaN);
                    generator.ReleaseTemporaryVariable(isPositiveInfinity);
                    break;

                case PrimitiveType.String:
                case PrimitiveType.ConcatenatedString:
                case PrimitiveType.Any:
                case PrimitiveType.Object:
                    // Otherwise, fall back to calling TypeConverter.ToInteger()
                    generator.Call(ReflectionHelpers.TypeConverter_ToInteger);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", fromType));
            }
        }

        /// <summary>
        /// Pops the value on the stack, converts it to an integer, then pushes the integer result
        /// onto the stack.  Large numbers wrap (i.e. 4294967296 -> 0).
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        public static void ToInt32(ILGenerator generator, PrimitiveType fromType)
        {
            // Check that a conversion is actually necessary.
            if (fromType == PrimitiveType.Int32 || fromType == PrimitiveType.UInt32 || fromType == PrimitiveType.Bool)
                return;

            switch (fromType)
            {
                case PrimitiveType.Undefined:
                case PrimitiveType.Null:
                    // Converting from undefined or null produces 0.
                    generator.Pop();
                    generator.LoadInt32(0);
                    break;

                case PrimitiveType.Number:
                    // Converting from a number produces the number mod 4294967296.  NaN produces 0.
                    generator.ConvertToUnsignedInteger();
                    break;

                case PrimitiveType.String:
                case PrimitiveType.ConcatenatedString:
                case PrimitiveType.Any:
                case PrimitiveType.Object:
                    // Otherwise, fall back to calling TypeConverter.ToInt32()
                    generator.Call(ReflectionHelpers.TypeConverter_ToInt32);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", fromType));
            }
        }

        /// <summary>
        /// Pops the value on the stack, converts it to an unsigned integer, then pushes the
        /// integer result onto the stack.  Large numbers wrap (i.e. 4294967296 -> 0).
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        public static void ToUInt32(ILGenerator generator, PrimitiveType fromType)
        {
            ToInt32(generator, fromType);
        }

        /// <summary>
        /// Pops the value on the stack, converts it to a double, then pushes the double result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        public static void ToNumber(ILGenerator generator, PrimitiveType fromType)
        {
            // Check that a conversion is actually necessary.
            if (fromType == PrimitiveType.Number)
                return;

            switch (fromType)
            {
                case PrimitiveType.Undefined:
                    // Converting from undefined produces NaN.
                    generator.Pop();
                    generator.LoadDouble(double.NaN);
                    break;

                case PrimitiveType.Null:
                    // Converting from null produces 0.
                    generator.Pop();
                    generator.LoadDouble(0.0);
                    break;

                case PrimitiveType.Bool:
                    // Converting from a boolean produces 0 if the boolean is false, or 1 if the boolean is true.
                    generator.ConvertToDouble();
                    break;

                case PrimitiveType.Int32:
                    // Converting from int32 produces the same number.
                    generator.ConvertToDouble();
                    break;

                case PrimitiveType.UInt32:
                    // Converting from a number produces the following:
                    generator.ConvertUnsignedToDouble();
                    break;

                case PrimitiveType.String:
                case PrimitiveType.ConcatenatedString:
                case PrimitiveType.Any:
                case PrimitiveType.Object:
                    // Otherwise, fall back to calling TypeConverter.ToNumber()
                    generator.Call(ReflectionHelpers.TypeConverter_ToNumber);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", fromType));
            }
        }

        /// <summary>
        /// Pops the value on the stack, converts it to a string, then pushes the result onto the
        /// stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        public static void ToString(ILGenerator generator, PrimitiveType fromType)
        {
            // Check that a conversion is actually necessary.
            if (fromType == PrimitiveType.String)
                return;

            switch (fromType)
            {
                case PrimitiveType.Undefined:
                    // Converting from undefined produces "undefined".
                    generator.Pop();
                    generator.LoadString("undefined");
                    break;

                case PrimitiveType.Null:
                    // Converting from null produces "null".
                    generator.Pop();
                    generator.LoadString("null");
                    break;

                case PrimitiveType.Bool:
                    // Converting from a boolean produces "false" if the boolean is false, or "true" if the boolean is true.
                    var elseClause = generator.CreateLabel();
                    var endOfIf = generator.CreateLabel();
                    generator.BranchIfFalse(elseClause);
                    generator.LoadString("true");
                    generator.Branch(endOfIf);
                    generator.DefineLabelPosition(elseClause);
                    generator.LoadString("false");
                    generator.DefineLabelPosition(endOfIf);
                    break;

                case PrimitiveType.ConcatenatedString:
                    generator.Call(ReflectionHelpers.ConcatenatedString_ToString);
                    break;

                case PrimitiveType.Int32:
                case PrimitiveType.UInt32:
                case PrimitiveType.Number:
                case PrimitiveType.Any:
                case PrimitiveType.Object:
                    // Otherwise, fall back to calling TypeConverter.ToString()
                    if (PrimitiveTypeUtilities.IsValueType(fromType))
                        generator.Box(fromType);
                    generator.Call(ReflectionHelpers.TypeConverter_ToString);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", fromType));
            }
        }

        /// <summary>
        /// Pops the value on the stack, converts it to a concatenated string, then pushes the result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        public static void ToConcatenatedString(ILGenerator generator, PrimitiveType fromType)
        {
            // Check that a conversion is actually necessary.
            if (fromType == PrimitiveType.ConcatenatedString)
                return;

            switch (fromType)
            {
                case PrimitiveType.Undefined:
                case PrimitiveType.Null:
                case PrimitiveType.Bool:
                case PrimitiveType.String:
                    // Convert as per ToString, then create a new ConcatenatedString instance.
                    ToString(generator, fromType);
                    generator.NewObject(ReflectionHelpers.ConcatenatedString_Constructor_String);
                    break;

                case PrimitiveType.Int32:
                case PrimitiveType.UInt32:
                case PrimitiveType.Number:
                case PrimitiveType.Any:
                case PrimitiveType.Object:
                    // Otherwise, fall back to calling TypeConverter.ToConcatenatedString()
                    if (PrimitiveTypeUtilities.IsValueType(fromType))
                        generator.Box(fromType);
                    generator.Call(ReflectionHelpers.TypeConverter_ToConcatenatedString);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", fromType));
            }

            
        }

        //        /// <summary>
        ///// Pops the value on the stack, converts it to a javascript object, then pushes the result
        ///// onto the stack.
        ///// </summary>
        ///// <param name="generator"> The IL generator. </param>
        ///// <param name="fromType"> The type to convert from. </param>
        ///// <param name="path"> The path of the javascript source file that is currently executing. </param>
        ///// <param name="function"> The name of the currently executing function. </param>
        ///// <param name="line"> The line number of the statement that is currently executing. </param>
        //public static void ToObject(ILGenerator generator, PrimitiveType fromType, string path, string function, int line)
        //{
        //}

        /// <summary>
        /// Pops the value on the stack, converts it to a javascript object, then pushes the result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        /// <param name="optimizationInfo"> Information about the line number, function and path. </param>
        public static void ToObject(ILGenerator generator, PrimitiveType fromType, OptimizationInfo optimizationInfo)
        {
            ToObject(generator, fromType, optimizationInfo.Source.Path, optimizationInfo.FunctionName, optimizationInfo.SourceSpan.StartLine);
        }

        /// <summary>
        /// Pops the value on the stack, converts it to a javascript object, then pushes the result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        /// <param name="path"> The path of the javascript source file that is currently executing. </param>
        /// <param name="function"> The name of the currently executing function. </param>
        /// <param name="line"> The line number of the statement that is currently executing. </param>
        public static void ToObject(ILGenerator generator, PrimitiveType fromType, string path, string function, int line)
        {
            // Check that a conversion is actually necessary.
            if (fromType == PrimitiveType.Object)
                return;

            switch (fromType)
            {
                case PrimitiveType.Undefined:
                    // Converting from undefined always throws an exception.
                    EmitHelpers.EmitThrow(generator, "TypeError", "Undefined cannot be converted to an object", path, function, line);
                    break;

                case PrimitiveType.Null:
                    // Converting from null always throws an exception.
                    EmitHelpers.EmitThrow(generator, "TypeError", "Null cannot be converted to an object", path, function, line);
                    break;

                case PrimitiveType.Bool:
                case PrimitiveType.Int32:
                case PrimitiveType.UInt32:
                case PrimitiveType.Number:
                case PrimitiveType.String:
                case PrimitiveType.ConcatenatedString:
                case PrimitiveType.Any:
                    // Otherwise, fall back to calling TypeConverter.ToObject()
                    ToAny(generator, fromType);
                    var temp = generator.CreateTemporaryVariable(typeof(object));
                    generator.StoreVariable(temp);
                    EmitHelpers.LoadScriptEngine(generator);
                    generator.LoadVariable(temp);
                    generator.ReleaseTemporaryVariable(temp);
                    generator.LoadInt32(line);
                    generator.LoadStringOrNull(path);
                    generator.LoadStringOrNull(function);
                    generator.Call(ReflectionHelpers.TypeConverter_ToObject);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", fromType));
            }
        }

        /// <summary>
        /// Pops the value on the stack, converts it to a primitive value, then pushes the result
        /// onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        /// <param name="preferredType"> Specifies whether toString() or valueOf() should be
        /// preferred when converting to a primitive. </param>
        public static void ToPrimitive(ILGenerator generator, PrimitiveType fromType, PrimitiveTypeHint preferredType)
        {
            switch (fromType)
            {
                case PrimitiveType.Undefined:
                case PrimitiveType.Null:
                case PrimitiveType.Bool:
                case PrimitiveType.String:
                case PrimitiveType.ConcatenatedString:
                case PrimitiveType.Int32:
                case PrimitiveType.UInt32:
                case PrimitiveType.Number:
                    // These are primitives already.
                    break;

                case PrimitiveType.Any:
                case PrimitiveType.Object:
                    // Otherwise, fall back to calling TypeConverter.ToPrimitive()
                    if (PrimitiveTypeUtilities.IsValueType(fromType))
                        generator.Box(fromType);
                    generator.LoadInt32((int)preferredType);
                    generator.Call(ReflectionHelpers.TypeConverter_ToPrimitive);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Unsupported primitive type: {0}", fromType));
            }
        }

        /// <summary>
        /// Pops the value on the stack, converts it to an object, then pushes the result onto the
        /// stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        public static void ToAny(ILGenerator generator, PrimitiveType fromType)
        {
            if (PrimitiveTypeUtilities.IsValueType(fromType))
                generator.Box(fromType);
        }
    }

}
