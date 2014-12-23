using System;
using System.Collections.Generic;
using System.Text;
using Jurassic.Library;

namespace Jurassic
{

    /// <summary>
    /// Contains type-related functionality that isn't conversion or comparison.
    /// </summary>
    public static class TypeUtilities
    {
        /// <summary>
        /// Gets the type name for the given object.  Used by the typeof operator.
        /// </summary>
        /// <param name="obj"> The object to get the type name for. </param>
        /// <returns> The type name for the given object. </returns>
        public static string TypeOf(object obj)
        {
            if (obj == null || obj == Undefined.Value)
                return "undefined";
            if (obj == Null.Value)
                return "object";
            if (obj is bool)
                return "boolean";
            if (obj is double || obj is int || obj is uint)
                return "number";
            if (obj is string || obj is ConcatenatedString)
                return "string";
            if (obj is FunctionInstance)
                return "function";
            if (obj is ObjectInstance)
                return "object";
            throw new InvalidOperationException("Unsupported object type.");
        }

        /// <summary>
        /// Returns <c>true</c> if the given value is undefined.
        /// </summary>
        /// <param name="obj"> The object to check. </param>
        /// <returns> <c>true</c> if the given value is undefined; <c>false</c> otherwise. </returns>
        internal static bool IsUndefined(object obj)
        {
            return obj == null || obj == Undefined.Value;
        }

        /// <summary>
        /// Returns <c>true</c> if the given value is a supported numeric type.
        /// </summary>
        /// <param name="obj"> The object to check. </param>
        /// <returns> <c>true</c> if the given value is a supported numeric type; <c>false</c>
        /// otherwise. </returns>
        internal static bool IsNumeric(object obj)
        {
            return obj is double || obj is int || obj is uint;
        }

        /// <summary>
        /// Returns <c>true</c> if the given value is a supported string type.
        /// </summary>
        /// <param name="obj"> The object to check. </param>
        /// <returns> <c>true</c> if the given value is a supported string type; <c>false</c>
        /// otherwise. </returns>
        internal static bool IsString(object obj)
        {
            return obj is string || obj is ConcatenatedString;
        }

        /// <summary>
        /// Converts the given value into a standard .NET type, suitable for returning from an API.
        /// </summary>
        /// <param name="obj"> The value to normalize. </param>
        /// <returns> The value as a standard .NET type. </returns>
        internal static object NormalizeValue(object obj)
        {
            if (obj == null)
                return Undefined.Value;
            else if (obj is double)
            {
                var numericResult = (double)obj;
                if ((double)((int)numericResult) == numericResult)
                    return (int)numericResult;
            }
            else if (obj is uint)
            {
                var uintValue = (uint)obj;
                if ((int)uintValue >= 0)
                    return (int)uintValue;
                return (double)uintValue;
            }
            else if (obj is ConcatenatedString)
                obj = ((ConcatenatedString)obj).ToString();
            else if (obj is ClrInstanceWrapper)
                obj = ((ClrInstanceWrapper)obj).WrappedInstance;
            else if (obj is ClrStaticTypeWrapper)
                obj = ((ClrStaticTypeWrapper)obj).WrappedType;
            return obj;
        }

        /// <summary>
        /// Enumerates the names of the enumerable properties on the given object, including
        /// properties defined on the object's prototype.  Used by the for-in statement.
        /// </summary>
        /// <param name="engine"> The script engine used to convert the given value to an object. </param>
        /// <param name="obj"> The object to enumerate. </param>
        /// <returns> An enumerator that iteratively returns property names. </returns>
        public static IEnumerable<string> EnumeratePropertyNames(ScriptEngine engine, object obj)
        {
            if (IsUndefined(obj) == true || obj == Null.Value)
                yield break;
            var obj2 = TypeConverter.ToObject(engine, obj);
            var names = new HashSet<string>();
            do
            {
                foreach (var property in obj2.Properties)
                {
                    // Check whether the property is shadowed.
                    if (names.Contains(property.Name) == false)
                    {
                        // Only return enumerable properties.
                        if (property.IsEnumerable == true)
                        {
                            // Make sure the property still exists.
                            if (obj2.HasProperty(property.Name) == true)
                            {
                                yield return property.Name;
                            }
                        }
                        
                        // Record the name so we can check if it was shadowed.
                        names.Add(property.Name);
                    }
                }
                obj2 = obj2.Prototype;
            } while (obj2 != null);
        }

        /// <summary>
        /// Adds two objects together, as if by the javascript addition operator.
        /// </summary>
        /// <param name="left"> The left hand side operand. </param>
        /// <param name="right"> The right hand side operand. </param>
        /// <returns> Either a number or a concatenated string. </returns>
        public static object Add(object left, object right)
        {
            var leftPrimitive = TypeConverter.ToPrimitive(left, PrimitiveTypeHint.None);
            var rightPrimitive = TypeConverter.ToPrimitive(right, PrimitiveTypeHint.None);

            if (leftPrimitive is ConcatenatedString)
            {
                return ((ConcatenatedString)leftPrimitive).Concatenate(rightPrimitive);
            }
            else if (leftPrimitive is string || rightPrimitive is string || rightPrimitive is ConcatenatedString)
            {
                return new ConcatenatedString(TypeConverter.ToString(leftPrimitive), TypeConverter.ToString(rightPrimitive));
            }

            return TypeConverter.ToNumber(leftPrimitive) + TypeConverter.ToNumber(rightPrimitive);
        }

        /// <summary>
        /// Determines if the given value is a supported JavaScript primitive.
        /// </summary>
        /// <param name="value"> The value to test. </param>
        /// <returns> <c>true</c> if the given value is a supported JavaScript primitive;
        /// <c>false</c> otherwise. </returns>
        public static bool IsPrimitive(object value)
        {
            if (value == null)
                return true;
            var type = value.GetType();
            return type == typeof(bool) ||
                type == typeof(int) || type == typeof(uint) || type == typeof(double) ||
                type == typeof(string) || type == typeof(ConcatenatedString) ||
                type == typeof(Null) || type == typeof(Undefined);
        }

        /// <summary>
        /// Determines if the given value is a supported JavaScript primitive or derives from
        /// ObjectInstance.
        /// </summary>
        /// <param name="value"> The value to test. </param>
        /// <returns> <c>true</c> if the given value is a supported JavaScript primitive or derives
        /// from ObjectInstance; <c>false</c> otherwise. </returns>
        public static bool IsPrimitiveOrObject(object value)
        {
            if (value == null)
                return true;
            var type = value.GetType();
            return type == typeof(bool) ||
                type == typeof(int) || type == typeof(uint) || type == typeof(double) ||
                type == typeof(string) || type == typeof(ConcatenatedString) ||
                type == typeof(Null) || type == typeof(Undefined) ||
                typeof(ObjectInstance).IsAssignableFrom(type);
        }

        /// <summary>
        /// Throws a TypeError when the given value is <c>null</c> or <c>undefined.</c>
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="value"> The value to check. </param>
        /// <param name="functionName"> The name of the function which is doing the check. </param>
        public static void VerifyThisObject(ScriptEngine engine, object value, string functionName)
        {
            if (value == null || value == Undefined.Value)
                throw new JavaScriptException(engine, "TypeError", string.Format("The function '{0}' does not allow the value of 'this' to be undefined", functionName));
            if (value == Null.Value)
                throw new JavaScriptException(engine, "TypeError", string.Format("The function '{0}' does not allow the value of 'this' to be null", functionName));
        }
    }

}
