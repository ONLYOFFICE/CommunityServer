using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a literal expression.
    /// </summary>
    internal sealed class LiteralExpression : Expression
    {
        /// <summary>
        /// Creates a new instance of LiteralJSExpression.
        /// </summary>
        /// <param name="value"> The literal value. </param>
        public LiteralExpression(object value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the literal value.
        /// </summary>
        public object Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Evaluates the expression, if possible.
        /// </summary>
        /// <returns> The result of evaluating the expression, or <c>null</c> if the expression can
        /// not be evaluated. </returns>
        public override object Evaluate()
        {
            // Array literal.
            if (this.Value is List<Expression>)
                return null;

            // Object literal.
            if (this.Value is Dictionary<string, object>)
                return null;

            // RegExp literal.
            if (this.Value is RegularExpressionLiteral)
                return null;

            // Everything else.
            return this.Value;
        }

        /// <summary>
        /// Gets the type that results from evaluating this expression.
        /// </summary>
        public override PrimitiveType ResultType
        {
            get
            {
                // Array literal.
                if (this.Value is List<Expression>)
                    return PrimitiveType.Object;

                // Object literal.
                if (this.Value is Dictionary<string, object>)
                    return PrimitiveType.Object;

                // RegExp literal.
                if (this.Value is RegularExpressionLiteral)
                    return PrimitiveType.Object;

                // Everything else.
                return PrimitiveTypeUtilities.ToPrimitiveType(this.Value.GetType());
            }
        }

        /// <summary>
        /// Generates CIL for the expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Literals cannot have side-effects so if a return value is not expected then generate
            // nothing.
            //if (optimizationInfo.SuppressReturnValue == true) 
            //    return;

            if (this.Value is int)
                generator.LoadInt32((int)this.Value);
            else if (this.Value is double)
                generator.LoadDouble((double)this.Value);
            else if (this.Value is string)
                generator.LoadString((string)this.Value);
            else if (this.Value is bool)
                generator.LoadBoolean((bool)this.Value);
            else if (this.Value is RegularExpressionLiteral)
            {
                // RegExp
                var sharedRegExpVariable = optimizationInfo.GetRegExpVariable(generator, (RegularExpressionLiteral)this.Value);
                var label1 = generator.CreateLabel();
                var label2 = generator.CreateLabel();

                // if (sharedRegExp == null) {
                generator.LoadVariable(sharedRegExpVariable);
                generator.LoadNull();
                generator.BranchIfNotEqual(label1);

                // sharedRegExp = Global.RegExp.Construct(source, flags)
                EmitHelpers.LoadScriptEngine(generator);
                generator.Call(ReflectionHelpers.ScriptEngine_RegExp);
                generator.LoadString(((RegularExpressionLiteral)this.Value).Pattern);
                generator.LoadString(((RegularExpressionLiteral)this.Value).Flags);
                generator.Call(ReflectionHelpers.RegExp_Construct);
                generator.Duplicate();
                generator.StoreVariable(sharedRegExpVariable);

                // } else {
                generator.Branch(label2);
                generator.DefineLabelPosition(label1);

                // Global.RegExp.Construct(sharedRegExp, flags)
                EmitHelpers.LoadScriptEngine(generator);
                generator.Call(ReflectionHelpers.ScriptEngine_RegExp);
                generator.LoadVariable(sharedRegExpVariable);
                generator.LoadNull();
                generator.Call(ReflectionHelpers.RegExp_Construct);

                // }
                generator.DefineLabelPosition(label2);
            }
            else if (this.Value == Null.Value)
            {
                // Null.
                EmitHelpers.EmitNull(generator);
            }
            else if (this.Value == Undefined.Value)
            {
                // Undefined.
                EmitHelpers.EmitUndefined(generator);
            }
            else if (this.Value is List<Expression>)
            {
                // Construct an array literal.
                var arrayLiteral = (List<Expression>)this.Value;

                // Operands for ArrayConstructor.New() are: an ArrayConstructor instance (ArrayConstructor), an array (object[])
                // ArrayConstructor
                EmitHelpers.LoadScriptEngine(generator);
                generator.Call(ReflectionHelpers.ScriptEngine_Array);

                // object[]
                generator.LoadInt32(arrayLiteral.Count);
                generator.NewArray(typeof(object));
                for (int i = 0; i < arrayLiteral.Count; i ++)
                {
                    // Operands for StoreArrayElement() are: an array (object[]), index (int), value (object).
                    // Array
                    generator.Duplicate();

                    // Index
                    generator.LoadInt32(i);

                    // Value
                    var elementExpression = arrayLiteral[i];
                    if (elementExpression == null)
                        generator.LoadNull();
                    else
                    {
                        elementExpression.GenerateCode(generator, optimizationInfo);
                        EmitConversion.ToAny(generator, elementExpression.ResultType);
                    }

                    // Store the element value.
                    generator.StoreArrayElement(typeof(object));
                }

                // ArrayConstructor.New(object[])
                generator.Call(ReflectionHelpers.Array_New);
            }
            else if (this.Value is Dictionary<string, object>)
            {
                // This is an object literal.
                var properties = (Dictionary<string, object>)this.Value;

                // Create a new object.
                EmitHelpers.LoadScriptEngine(generator);
                generator.Call(ReflectionHelpers.ScriptEngine_Object);
                generator.Call(ReflectionHelpers.Object_Construct);

                foreach (var keyValuePair in properties)
                {
                    string propertyName = keyValuePair.Key;
                    object propertyValue = keyValuePair.Value;

                    generator.Duplicate();
                    generator.LoadString(propertyName);
                    if (propertyValue is Expression)
                    {
                        // Add a new property to the object.
                        var dataPropertyValue = (Expression)propertyValue;
                        dataPropertyValue.GenerateCode(generator, optimizationInfo);
                        // Support the inferred function displayName property.
                        if (dataPropertyValue is FunctionExpression)
                            ((FunctionExpression)dataPropertyValue).GenerateDisplayName(generator, optimizationInfo, propertyName, false);
                        EmitConversion.ToAny(generator, dataPropertyValue.ResultType);
                        generator.LoadBoolean(optimizationInfo.StrictMode);
                        generator.Call(ReflectionHelpers.ObjectInstance_SetPropertyValue_String);
                    }
                    else if (propertyValue is Parser.ObjectLiteralAccessor)
                    {
                        // Add a new getter/setter to the object.
                        var accessorValue = (Parser.ObjectLiteralAccessor)propertyValue;
                        if (accessorValue.Getter != null)
                        {
                            accessorValue.Getter.GenerateCode(generator, optimizationInfo);
                            // Support the inferred function displayName property.
                            accessorValue.Getter.GenerateDisplayName(generator, optimizationInfo, "get " + propertyName, true);
                            EmitConversion.ToAny(generator, accessorValue.Getter.ResultType);
                        }
                        else
                            generator.LoadNull();
                        if (accessorValue.Setter != null)
                        {
                            accessorValue.Setter.GenerateCode(generator, optimizationInfo);
                            // Support the inferred function displayName property.
                            accessorValue.Setter.GenerateDisplayName(generator, optimizationInfo, "set " + propertyName, true);
                            EmitConversion.ToAny(generator, accessorValue.Setter.ResultType);
                        }
                        else
                            generator.LoadNull();
                        generator.LoadInt32((int)Library.PropertyAttributes.FullAccess);
                        generator.NewObject(ReflectionHelpers.PropertyDescriptor_Constructor3);
                        generator.LoadBoolean(false);
                        generator.Call(ReflectionHelpers.ObjectInstance_DefineProperty);
                        generator.Pop();
                    }
                    else
                        throw new InvalidOperationException("Invalid property value type in object literal.");
                }
            }
            else
                throw new NotImplementedException("Unknown literal type.");
        }

        /// <summary>
        /// Converts the expression to a string.
        /// </summary>
        /// <returns> A string representing this expression. </returns>
        public override string ToString()
        {
            // Array literal.
            if (this.Value is List<Expression>)
            {
                var result = new System.Text.StringBuilder("[");
                foreach (var item in (List<Expression>)this.Value)
                {
                    if (result.Length > 1)
                        result.Append(", ");
                    result.Append(item);
                }
                result.Append("]");
                return result.ToString();
            }

            // Object literal.
            if (this.Value is Dictionary<string, object>)
            {
                var result = new System.Text.StringBuilder("{");
                foreach (var keyValuePair in (Dictionary<string, object>)this.Value)
                {
                    if (result.Length > 1)
                        result.Append(", ");
                    if (keyValuePair.Value is Expression)
                    {
                        result.Append(keyValuePair.Key);
                        result.Append(": ");
                        result.Append(keyValuePair.Value);

                    }
                    else if (keyValuePair.Value is Parser.ObjectLiteralAccessor)
                    {
                        var accessor = (Parser.ObjectLiteralAccessor)keyValuePair.Value;
                        if (accessor.Getter != null)
                        {
                            result.Append("get ");
                            result.Append(accessor.Getter.ToString().Substring(9));
                            if (accessor.Setter != null)
                                result.Append(", ");
                        }
                        if (accessor.Setter != null)
                        {
                            result.Append("set ");
                            result.Append(accessor.Setter.ToString().Substring(9));
                        }
                    }
                }
                result.Append("}");
                return result.ToString();
            }

            // RegExp literal.
            if (this.Value is RegularExpressionLiteral)
                return this.Value.ToString();

            // String literal.
            if (this.Value is string)
                return Library.StringInstance.Quote((string)this.Value);

            // Everything else.
            return TypeConverter.ToString(this.Value);
        }
    }


}