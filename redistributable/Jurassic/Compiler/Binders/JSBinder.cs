using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jurassic;
using Jurassic.Library;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Binds to a method group using the default javascript rules (extra parameter values are
    /// ignored, missing parameter values are replaced with "undefined").
    /// </summary>
    [Serializable]
    internal class JSBinder : MethodBinder
    {
        private JSBinderMethod[] buckets;
        
        internal const int MaximumSupportedParameterCount = 8;

        /// <summary>
        /// Creates a new JSBinder instance.
        /// </summary>
        /// <param name="targetMethods"> An array of methods to bind to. </param>
        public JSBinder(params JSBinderMethod[] targetMethods)
            : this((IEnumerable<JSBinderMethod>)targetMethods)
        {
        }

        /// <summary>
        /// Creates a new JSBinder instance.
        /// </summary>
        /// <param name="targetMethods"> An enumerable list of methods to bind to. </param>
        public JSBinder(IEnumerable<JSBinderMethod> targetMethods)
            : base(targetMethods.Select(m => (BinderMethod)m))
        {
            // Split the methods by the number of parameters they take.
            this.buckets = new JSBinderMethod[MaximumSupportedParameterCount + 1];
            for (int argumentCount = 0; argumentCount < this.buckets.Length; argumentCount++)
            {
                // Find all the methods that have the right number of parameters.
                JSBinderMethod preferred = null;
                foreach (var method in targetMethods)
                {
                    if (argumentCount >= method.RequiredParameterCount && argumentCount <= method.MaxParameterCount)
                    {
                        if (preferred != null)
                            throw new ArgumentException(string.Format("Multiple ambiguous methods detected: {0} and {1}.", method, preferred), "targetMethods");
                        preferred = method;
                    }
                }
                this.buckets[argumentCount] = preferred;
            }

            // If a bucket has no methods, search all previous buckets, then all search forward.
            for (int argumentCount = 0; argumentCount < this.buckets.Length; argumentCount++)
            {
                if (this.buckets[argumentCount] != null)
                    continue;

                // Search previous buckets.
                for (int i = argumentCount - 1; i >= 0; i --)
                    if (this.buckets[i] != null)
                    {
                        this.buckets[argumentCount] = this.buckets[i];
                        break;
                    }

                // If that didn't work, search forward.
                if (this.buckets[argumentCount] == null)
                {
                    for (int i = argumentCount + 1; i < this.buckets.Length; i++)
                        if (this.buckets[i] != null)
                        {
                            this.buckets[argumentCount] = this.buckets[i];
                            break;
                        }
                }

                // If that still didn't work, then we have a problem.
                if (this.buckets[argumentCount] == null)
                    throw new InvalidOperationException("No preferred method could be found.");
            }
        }

        /// <summary>
        /// Generates a method that does type conversion and calls the bound method.
        /// </summary>
        /// <param name="generator"> The ILGenerator used to output the body of the method. </param>
        /// <param name="argumentCount"> The number of arguments that will be passed to the delegate. </param>
        /// <returns> A delegate that does type conversion and calls the method represented by this
        /// object. </returns>
        protected override void GenerateStub(ILGenerator generator, int argumentCount)
        {
            // Here is what we are going to generate.
            //private static object SampleBinder(ScriptEngine engine, object thisObject, object[] arguments)
            //{
            //    // Target function signature: int (bool, int, string, object).
            //    bool param1;
            //    int param2;
            //    string param3;
            //    object param4;
            //    param1 = arguments[0] != 0;
            //    param2 = TypeConverter.ToInt32(arguments[1]);
            //    param3 = TypeConverter.ToString(arguments[2]);
            //    param4 = Undefined.Value;
            //    return thisObject.targetMethod(param1, param2, param3, param4);
            //}

            // Find the target method.
            var binderMethod = this.buckets[Math.Min(argumentCount, this.buckets.Length - 1)];

            // Constrain the number of apparent arguments to within the required bounds.
            int minArgumentCount = binderMethod.RequiredParameterCount;
            int maxArgumentCount = binderMethod.RequiredParameterCount + binderMethod.OptionalParameterCount;
            if (binderMethod.HasParamArray == true)
                maxArgumentCount = int.MaxValue;

            foreach (var argument in binderMethod.GenerateArguments(generator, Math.Min(Math.Max(argumentCount, minArgumentCount), maxArgumentCount)))
            {
                switch (argument.Source)
                {
                    case BinderArgumentSource.ScriptEngine:
                        // Load the "engine" parameter passed by the client.
                        generator.LoadArgument(0);
                        break;

                    case BinderArgumentSource.ThisValue:
                        // Load the "this" parameter passed by the client.
                        generator.LoadArgument(1);

                        bool inheritsFromObjectInstance = typeof(ObjectInstance).IsAssignableFrom(argument.Type);
                        if (argument.Type.IsClass == true && inheritsFromObjectInstance == false &&
                            argument.Type != typeof(string) && argument.Type != typeof(object))
                        {
                            // If the "this" object is an unsupported class, pass it through unmodified.
                            generator.CastClass(argument.Type);
                        }
                        else
                        {
                            if (argument.Type != typeof(object))
                            {
                                // If the target "this" object type is not of type object, throw an error if
                                // the value is undefined or null.
                                generator.Duplicate();
                                var temp = generator.CreateTemporaryVariable(typeof(object));
                                generator.StoreVariable(temp);
                                generator.LoadArgument(0);
                                generator.LoadVariable(temp);
                                generator.LoadString(binderMethod.Name);
                                generator.Call(ReflectionHelpers.TypeUtilities_VerifyThisObject);
                                generator.ReleaseTemporaryVariable(temp);
                            }

                            // Convert to the target type.
                            EmitTypeConversion(generator, typeof(object), argument.Type);

                            if (argument.Type != typeof(ObjectInstance) && inheritsFromObjectInstance == true)
                            {
                                // EmitConversionToObjectInstance can emit null if the toType is derived from ObjectInstance.
                                // Therefore, if the value emitted is null it means that the "thisObject" is a type derived
                                // from ObjectInstance (e.g. FunctionInstance) and the value provided is a different type
                                // (e.g. ArrayInstance).  In this case, throw an exception explaining that the function is
                                // not generic.
                                var endOfThrowLabel = generator.CreateLabel();
                                generator.Duplicate();
                                generator.BranchIfNotNull(endOfThrowLabel);
                                generator.LoadArgument(0);
                                EmitHelpers.EmitThrow(generator, "TypeError", string.Format("The method '{0}' is not generic", binderMethod.Name));
                                generator.DefineLabelPosition(endOfThrowLabel);
                            }
                        }
                        break;

                    case BinderArgumentSource.InputParameter:
                        if (argument.InputParameterIndex < argumentCount)
                        {
                            // Load the argument onto the stack.
                            generator.LoadArgument(2);
                            generator.LoadInt32(argument.InputParameterIndex);
                            generator.LoadArrayElement(typeof(object));

                            // Get some flags that apply to the parameter.
                            var parameterFlags = JSParameterFlags.None;
                            var parameterAttribute = argument.GetCustomAttribute<JSParameterAttribute>();
                            if (parameterAttribute != null)
                            {
                                if (argument.Type != typeof(ObjectInstance))
                                    throw new NotImplementedException("[JSParameter] is only supported for arguments of type ObjectInstance.");
                                parameterFlags = parameterAttribute.Flags;
                            }

                            if ((parameterFlags & JSParameterFlags.DoNotConvert) == 0)
                            {
                                // Convert the input parameter to the correct type.
                                EmitTypeConversion(generator, typeof(object), argument);
                            }
                            else
                            {
                                // Don't do argument conversion.
                                var endOfThrowLabel = generator.CreateLabel();
                                generator.IsInstance(typeof(ObjectInstance));
                                generator.Duplicate();
                                generator.BranchIfNotNull(endOfThrowLabel);
                                EmitHelpers.EmitThrow(generator, "TypeError", string.Format("Parameter {1} parameter of '{0}' must be an object", binderMethod.Name, argument.InputParameterIndex));
                                generator.DefineLabelPosition(endOfThrowLabel);
                            }
                        }
                        else
                        {
                            // The target method has more parameters than we have input values.
                            EmitUndefined(generator, argument);
                        }
                        break;
                }
            }

            // Emit the call.
            binderMethod.GenerateCall(generator);

            // Convert the return value.
            if (binderMethod.ReturnType == typeof(void))
                EmitHelpers.EmitUndefined(generator);
            else
            {
                EmitTypeConversion(generator, binderMethod.ReturnType, typeof(object));

                // Convert a null return value to Null.Value or Undefined.Value.
                var endOfSpecialCaseLabel = generator.CreateLabel();
                generator.Duplicate();
                generator.BranchIfNotNull(endOfSpecialCaseLabel);
                generator.Pop();
                if ((binderMethod.Flags & JSFunctionFlags.ConvertNullReturnValueToUndefined) != 0)
                    EmitHelpers.EmitUndefined(generator);
                else
                    EmitHelpers.EmitNull(generator);
                generator.DefineLabelPosition(endOfSpecialCaseLabel);
            }

            // End the IL.
            generator.Complete();
        }

        /// <summary>
        /// Pops the value on the stack, converts it from one type to another, then pushes the
        /// result onto the stack.  Undefined is converted to the given default value.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        /// <param name="targetParameter"> The type to convert to and the default value, if there is one. </param>
        private static void EmitTypeConversion(ILGenerator generator, Type fromType, BinderArgument argument)
        {
            // Emit either the default value if there is one, otherwise emit "undefined".
            if (argument.HasDefaultValue)
            {
                // Check if the input value is undefined.
                var elseClause = generator.CreateLabel();
                generator.Duplicate();
                generator.BranchIfNull(elseClause);
                generator.Duplicate();
                generator.LoadField(ReflectionHelpers.Undefined_Value);
                generator.CompareEqual();
                generator.BranchIfTrue(elseClause);

                // Convert as per normal.
                EmitTypeConversion(generator, fromType, argument.Type);

                // Jump to the end.
                var endOfIf = generator.CreateLabel();
                generator.Branch(endOfIf);
                generator.DefineLabelPosition(elseClause);

                // Pop the existing value and emit the default value.
                generator.Pop();
                EmitUndefined(generator, argument);

                // Define the end of the block.
                generator.DefineLabelPosition(endOfIf);
            }
            else
            {
                // Convert as per normal.
                EmitTypeConversion(generator, fromType, argument.Type);
            }
        }

        /// <summary>
        /// Pops the value on the stack, converts it from one type to another, then pushes the
        /// result onto the stack.
        /// </summary>
        /// <param name="il"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        /// <param name="toType"> The type to convert to. </param>
        private static void EmitTypeConversion(ILGenerator il, Type fromType, Type toType)
        {
            // If the source type equals the destination type, then there is nothing to do.
            if (fromType == toType)
                return;

            // Emit for each type of argument we support.
            if (toType == typeof(int))
                EmitConversion.ToInteger(il, PrimitiveTypeUtilities.ToPrimitiveType(fromType));
            else if (typeof(ObjectInstance).IsAssignableFrom(toType))
            {
                EmitConversion.Convert(il, PrimitiveTypeUtilities.ToPrimitiveType(fromType), PrimitiveType.Object);
                if (toType != typeof(ObjectInstance))
                {
                    // Convert to null if the from type isn't compatible with the to type.
                    // For example, if the target type is FunctionInstance and the from type is ArrayInstance, then pass null.
                    il.IsInstance(toType);
                }
            }
            else
                EmitConversion.Convert(il, PrimitiveTypeUtilities.ToPrimitiveType(fromType), PrimitiveTypeUtilities.ToPrimitiveType(toType));
        }

        /// <summary>
        /// Pushes the result of converting <c>undefined</c> to the given type onto the stack.
        /// </summary>
        /// <param name="il"> The IL generator. </param>
        /// <param name="targetParameter"> The type to convert to, and optionally a default value. </param>
        private static void EmitUndefined(ILGenerator il, BinderArgument argument)
        {
            // Emit either the default value if there is one, otherwise emit "undefined".
            if (argument.HasDefaultValue == true)
            {
                // Emit the default value.
                EmitHelpers.EmitValue(il, argument.DefaultValue);
            }
            else
            {
                // Convert Undefined to the target type and emit.
                EmitHelpers.EmitUndefined(il);
                EmitTypeConversion(il, typeof(object), argument.Type);
            }
        }
    }
}
