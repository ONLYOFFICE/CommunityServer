using System;

namespace Jurassic.Compiler
{
    
    /// <summary>
    /// Represents a variable or member access.
    /// </summary>
    internal sealed class MemberAccessExpression : OperatorExpression, IReferenceExpression
    {
        /// <summary>
        /// Creates a new instance of MemberAccessExpression.
        /// </summary>
        /// <param name="operator"> The operator to base this expression on. </param>
        public MemberAccessExpression(Operator @operator)
            : base(@operator)
        {
        }

        /// <summary>
        /// Gets an expression that evaluates to the object that is being accessed or modified.
        /// </summary>
        public Expression Base
        {
            get { return this.GetOperand(0); }
        }

        /// <summary>
        /// Gets the type that results from evaluating this expression.
        /// </summary>
        public override PrimitiveType ResultType
        {
            get { return PrimitiveType.Any; }
        }

        /// <summary>
        /// Gets the static type of the reference.
        /// </summary>
        public PrimitiveType Type
        {
            get { return PrimitiveType.Any; }
        }

        /// <summary>
        /// Generates CIL for the expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // NOTE: this is a get reference because assignment expressions do not call this method.
            GenerateGet(generator, optimizationInfo, false);
        }

        /// <summary>
        /// Pushes the value of the reference onto the stack.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="throwIfUnresolvable"> <c>true</c> to throw a ReferenceError exception if
        /// the name is unresolvable; <c>false</c> to output <c>null</c> instead. </param>
        public void GenerateGet(ILGenerator generator, OptimizationInfo optimizationInfo, bool throwIfUnresolvable)
        {
            string propertyName = null;
            bool isArrayIndex = false;

            // Right-hand-side can be a property name (a.b)
            if (this.OperatorType == OperatorType.MemberAccess)
            {
                var rhs = this.GetOperand(1) as NameExpression;
                if (rhs == null)
                    throw new JavaScriptException(optimizationInfo.Engine, "SyntaxError", "Invalid member access", optimizationInfo.SourceSpan.StartLine, optimizationInfo.Source.Path, optimizationInfo.FunctionName);
                propertyName = rhs.Name;
            }

            // Or a constant indexer (a['b'])
            if (this.OperatorType == OperatorType.Index)
            {
                var rhs = this.GetOperand(1) as LiteralExpression;
                if (rhs != null && (PrimitiveTypeUtilities.IsNumeric(rhs.ResultType) || rhs.ResultType == PrimitiveType.String))
                {
                    propertyName = TypeConverter.ToString(rhs.Value);

                    // Or a array index (a[0])
                    if (rhs.ResultType == PrimitiveType.Int32 || (propertyName != null && Library.ArrayInstance.ParseArrayIndex(propertyName) != uint.MaxValue))
                        isArrayIndex = true;
                }
            }

            if (isArrayIndex == true)
            {
                // Array indexer
                // -------------
                // xxx = object[index]

                // Load the left-hand side and convert to an object instance.
                var lhs = this.GetOperand(0);
                lhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToObject(generator, lhs.ResultType, optimizationInfo);

                // Load the right-hand side and convert to a uint32.
                var rhs = this.GetOperand(1);
                rhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToUInt32(generator, rhs.ResultType);

                // Call the indexer.
                generator.Call(ReflectionHelpers.ObjectInstance_GetPropertyValue_Int);
            }
            else if (propertyName != null)
            {
                //// Load the left-hand side and convert to an object instance.
                //var lhs = this.GetOperand(0);
                //lhs.GenerateCode(generator, optimizationInfo);
                //EmitConversion.ToObject(generator, lhs.ResultType);

                //// Call Get(string)
                //generator.LoadString(propertyName);
                //generator.Call(ReflectionHelpers.ObjectInstance_GetPropertyValue_String);



                // Named property access (e.g. x = y.property)
                // -------------------------------------------
                // __object_cacheKey = null;
                // __object_property_cachedIndex = 0;
                // ...
                // if (__object_cacheKey != object.InlineCacheKey)
                //     xxx = object.InlineGetPropertyValue("property", out __object_property_cachedIndex, out __object_cacheKey)
                // else
                //     xxx = object.InlinePropertyValues[__object_property_cachedIndex];

                // Load the left-hand side and convert to an object instance.
                var lhs = this.GetOperand(0);
                lhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToObject(generator, lhs.ResultType, optimizationInfo);

                // TODO: share these variables somehow.
                var cacheKey = generator.DeclareVariable(typeof(object));
                var cachedIndex = generator.DeclareVariable(typeof(int));

                // Store the object into a temp variable.
                var objectInstance = generator.DeclareVariable(PrimitiveType.Object);
                generator.StoreVariable(objectInstance);

                // if (__object_cacheKey != object.InlineCacheKey)
                generator.LoadVariable(cacheKey);
                generator.LoadVariable(objectInstance);
                generator.Call(ReflectionHelpers.ObjectInstance_InlineCacheKey);
                var elseClause = generator.CreateLabel();
                generator.BranchIfEqual(elseClause);

                // value = object.InlineGetProperty("property", out __object_property_cachedIndex, out __object_cacheKey)
                generator.LoadVariable(objectInstance);
                generator.LoadString(propertyName);
                generator.LoadAddressOfVariable(cachedIndex);
                generator.LoadAddressOfVariable(cacheKey);
                generator.Call(ReflectionHelpers.ObjectInstance_InlineGetPropertyValue);

                var endOfIf = generator.CreateLabel();
                generator.Branch(endOfIf);

                // else
                generator.DefineLabelPosition(elseClause);

                // value = object.InlinePropertyValues[__object_property_cachedIndex];
                generator.LoadVariable(objectInstance);
                generator.Call(ReflectionHelpers.ObjectInstance_InlinePropertyValues);
                generator.LoadVariable(cachedIndex);
                generator.LoadArrayElement(typeof(object));

                // End of the if statement
                generator.DefineLabelPosition(endOfIf);

            }
            else
            {
                // Dynamic property access
                // -----------------------
                // xxx = object.Get(x)

                // Load the left-hand side and convert to an object instance.
                var lhs = this.GetOperand(0);
                lhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToObject(generator, lhs.ResultType, optimizationInfo);

                // Load the property name and convert to a string.
                var rhs = this.GetOperand(1);
                rhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToString(generator, rhs.ResultType);

                // Call Get(string)
                generator.Call(ReflectionHelpers.ObjectInstance_GetPropertyValue_String);
            }
        }

        /// <summary>
        /// Stores the value on the top of the stack in the reference.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="valueType"> The primitive type of the value that is on the top of the stack. </param>
        /// <param name="throwIfUnresolvable"> <c>true</c> to throw a ReferenceError exception if
        /// the name is unresolvable; <c>false</c> to create a new property instead. </param>
        public void GenerateSet(ILGenerator generator, OptimizationInfo optimizationInfo, PrimitiveType valueType, bool throwIfUnresolvable)
        {
            string propertyName = null;
            bool isArrayIndex = false;
            //optimizationInfo = optimizationInfo.RemoveFlags(OptimizationFlags.SuppressReturnValue);

            // Right-hand-side can be a property name (a.b)
            if (this.OperatorType == OperatorType.MemberAccess)
            {
                var rhs = this.GetOperand(1) as NameExpression;
                if (rhs == null)
                    throw new JavaScriptException(optimizationInfo.Engine, "SyntaxError", "Invalid member access", optimizationInfo.SourceSpan.StartLine, optimizationInfo.Source.Path, optimizationInfo.FunctionName);
                propertyName = rhs.Name;
            }

            // Or a constant indexer (a['b'])
            if (this.OperatorType == OperatorType.Index)
            {
                var rhs = this.GetOperand(1) as LiteralExpression;
                if (rhs != null)
                {
                    propertyName = TypeConverter.ToString(rhs.Value);

                    // Or a array index (a[0])
                    if (rhs.ResultType == PrimitiveType.Int32 || (propertyName != null && Library.ArrayInstance.ParseArrayIndex(propertyName) != uint.MaxValue))
                        isArrayIndex = true;
                }
            }

            // Convert the value to an object and store it in a temporary variable.
            var value = generator.CreateTemporaryVariable(typeof(object));
            EmitConversion.ToAny(generator, valueType);
            generator.StoreVariable(value);

            if (isArrayIndex == true)
            {
                // Array indexer
                // -------------
                // xxx = object[index]

                // Load the left-hand side and convert to an object instance.
                var lhs = this.GetOperand(0);
                lhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToObject(generator, lhs.ResultType, optimizationInfo);

                // Load the right-hand side and convert to a uint32.
                var rhs = this.GetOperand(1);
                rhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToUInt32(generator, rhs.ResultType);

                // Call the indexer.
                generator.LoadVariable(value);
                generator.LoadBoolean(optimizationInfo.StrictMode);
                generator.Call(ReflectionHelpers.ObjectInstance_SetPropertyValue_Int);
            }
            else if (propertyName != null)
            {
                //// Load the left-hand side and convert to an object instance.
                //var lhs = this.GetOperand(0);
                //lhs.GenerateCode(generator, optimizationInfo);
                //EmitConversion.ToObject(generator, lhs.ResultType);

                //// Call the indexer.
                //generator.LoadString(propertyName);
                //generator.LoadVariable(value);
                //generator.LoadBoolean(optimizationInfo.StrictMode);
                //generator.Call(ReflectionHelpers.ObjectInstance_SetPropertyValue_String);



                // Named property modification (e.g. x.property = y)
                // -------------------------------------------------
                // __object_cacheKey = null;
                // __object_property_cachedIndex = 0;
                // ...
                // if (__object_cacheKey != object.InlineCacheKey)
                //     object.InlineSetPropertyValue("property", value, strictMode, out __object_property_cachedIndex, out __object_cacheKey)
                // else
                //     object.InlinePropertyValues[__object_property_cachedIndex] = value;

                // Load the left-hand side and convert to an object instance.
                var lhs = this.GetOperand(0);
                lhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToObject(generator, lhs.ResultType, optimizationInfo);

                // TODO: share these variables somehow.
                var cacheKey = generator.DeclareVariable(typeof(object));
                var cachedIndex = generator.DeclareVariable(typeof(int));

                // Store the object into a temp variable.
                var objectInstance = generator.DeclareVariable(PrimitiveType.Object);
                generator.StoreVariable(objectInstance);

                // if (__object_cacheKey != object.InlineCacheKey)
                generator.LoadVariable(cacheKey);
                generator.LoadVariable(objectInstance);
                generator.Call(ReflectionHelpers.ObjectInstance_InlineCacheKey);
                var elseClause = generator.CreateLabel();
                generator.BranchIfEqual(elseClause);

                // xxx = object.InlineSetPropertyValue("property", value, strictMode, out __object_property_cachedIndex, out __object_cacheKey)
                generator.LoadVariable(objectInstance);
                generator.LoadString(propertyName);
                generator.LoadVariable(value);
                generator.LoadBoolean(optimizationInfo.StrictMode);
                generator.LoadAddressOfVariable(cachedIndex);
                generator.LoadAddressOfVariable(cacheKey);
                generator.Call(ReflectionHelpers.ObjectInstance_InlineSetPropertyValue);

                var endOfIf = generator.CreateLabel();
                generator.Branch(endOfIf);

                // else
                generator.DefineLabelPosition(elseClause);

                // object.InlinePropertyValues[__object_property_cachedIndex] = value;
                generator.LoadVariable(objectInstance);
                generator.Call(ReflectionHelpers.ObjectInstance_InlinePropertyValues);
                generator.LoadVariable(cachedIndex);
                generator.LoadVariable(value);
                generator.StoreArrayElement(typeof(object));

                // End of the if statement
                generator.DefineLabelPosition(endOfIf);

            }
            else
            {
                // Dynamic property access
                // -----------------------
                // xxx = object.Get(x)

                // Load the left-hand side and convert to an object instance.
                var lhs = this.GetOperand(0);
                lhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToObject(generator, lhs.ResultType, optimizationInfo);

                // Load the property name and convert to a string.
                var rhs = this.GetOperand(1);
                rhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToString(generator, rhs.ResultType);

                // Call the indexer.
                generator.LoadVariable(value);
                generator.LoadBoolean(optimizationInfo.StrictMode);
                generator.Call(ReflectionHelpers.ObjectInstance_SetPropertyValue_String);
            }

            // The temporary variable is no longer needed.
            generator.ReleaseTemporaryVariable(value);
        }

        /// <summary>
        /// Deletes the reference and pushes <c>true</c> if the delete succeeded, or <c>false</c>
        /// if the delete failed.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public void GenerateDelete(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Load the left-hand side and convert to an object instance.
            var lhs = this.GetOperand(0);
            lhs.GenerateCode(generator, optimizationInfo);
            EmitConversion.ToObject(generator, lhs.ResultType, optimizationInfo);

            // Load the property name and convert to a string.
            var rhs = this.GetOperand(1);
            if (this.OperatorType == OperatorType.MemberAccess && rhs is NameExpression)
                generator.LoadString((rhs as NameExpression).Name);
            else
            {
                rhs.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToString(generator, rhs.ResultType);
            }

            // Call Delete()
            generator.LoadBoolean(optimizationInfo.StrictMode);
            generator.Call(ReflectionHelpers.ObjectInstance_Delete);

            // If the return value is not wanted then pop it from the stack.
            //if (optimizationInfo.SuppressReturnValue == true)
            //    generator.Pop();
        }

        /// <summary>
        /// Converts the expression to a string.
        /// </summary>
        /// <returns> A string representing this expression. </returns>
        public override string ToString()
        {
            if (this.OperatorType == OperatorType.MemberAccess)
                return string.Format("{0}.{1}", this.GetRawOperand(0), this.OperandCount >= 2 ? this.GetRawOperand(1).ToString() : "?");
            return base.ToString();
        }
    }
}