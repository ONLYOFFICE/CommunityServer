using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jurassic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Binds to a method group using pretty standard .NET rules.  The main difference from the
    /// JSBinder is that the number of arguments must be correct.  Additionally, it is possible to
    /// bind to overloaded methods with the same number of arguments.
    /// </summary>
    [Serializable]
    internal class ClrBinder : MethodBinder
    {
        private IEnumerable<BinderMethod> targetMethods;

        /// <summary>
        /// Creates a new ClrBinder instance.
        /// </summary>
        /// <param name="targetMethods"> A method to bind to. </param>
        public ClrBinder(MethodBase targetMethod)
            : this(new BinderMethod[] { new BinderMethod(targetMethod) })
        {
            this.targetMethods = new BinderMethod[] { new BinderMethod(targetMethod) };
        }

        /// <summary>
        /// Creates a new ClrBinder instance.
        /// </summary>
        /// <param name="targetMethods"> An enumerable list of methods to bind to. </param>
        public ClrBinder(IEnumerable<MethodBase> targetMethods)
            : this(targetMethods.Select(method => new BinderMethod(method)))
        {
        }

        /// <summary>
        /// Creates a new ClrBinder instance.
        /// </summary>
        /// <param name="targetMethods"> An enumerable list of methods to bind to. </param>
        public ClrBinder(IEnumerable<BinderMethod> targetMethods)
            : base(targetMethods)
        {
            this.targetMethods = targetMethods;
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
            // Determine the methods that have the correct number of arguments.
            var candidateMethods = new List<BinderMethod>();
            foreach (var candidateMethod in this.targetMethods)
            {
                if (candidateMethod.IsArgumentCountCompatible(argumentCount) == true)
                    candidateMethods.Add(candidateMethod);
            }

            // Zero candidates means no overload had the correct number of arguments.
            if (candidateMethods.Count == 0)
            {
                EmitHelpers.EmitThrow(generator, "TypeError", string.Format("No overload for method '{0}' takes {1} arguments", this.Name, argumentCount));
                EmitHelpers.EmitDefaultValue(generator, PrimitiveType.Any);
                generator.Complete();
                return;
            }

            // Select the method to call at run time.
            generator.LoadInt32(candidateMethods.Count);
            generator.NewArray(typeof(RuntimeMethodHandle));
            for (int i = 0; i < candidateMethods.Count; i ++)
            {
                generator.Duplicate();
                generator.LoadInt32(i);
                generator.LoadToken(candidateMethods[i]);
                generator.StoreArrayElement(typeof(RuntimeMethodHandle));
            }
            generator.LoadArgument(0);
            generator.LoadArgument(1);
            generator.LoadArgument(2);
            generator.Call(ReflectionHelpers.BinderUtilities_ResolveOverloads);

            var endOfMethod = generator.CreateLabel();
            for (int i = 0; i < candidateMethods.Count; i++)
            {
                // Check if this is the selected method.
                ILLabel endOfIf = null;
                if (i < candidateMethods.Count - 1)
                {
                    generator.Duplicate();
                    generator.LoadInt32(i);
                    endOfIf = generator.CreateLabel();
                    generator.BranchIfNotEqual(endOfIf);
                }
                generator.Pop();

                var targetMethod = candidateMethods[i];

                // Convert the arguments.
                foreach (var argument in targetMethod.GenerateArguments(generator, argumentCount))
                {
                    // Load the input parameter value.
                    switch (argument.Source)
                    {
                        case BinderArgumentSource.ScriptEngine:
                            generator.LoadArgument(0);
                            break;
                        case BinderArgumentSource.ThisValue:
                            generator.LoadArgument(1);
                            break;
                        case BinderArgumentSource.InputParameter:
                            generator.LoadArgument(2);
                            generator.LoadInt32(argument.InputParameterIndex);
                            generator.LoadArrayElement(typeof(object));
                            break;
                    }

                    // Convert to the target type.
                    EmitConversionToType(generator, argument.Type, convertToAddress: argument.Source == BinderArgumentSource.ThisValue);
                }

                // Call the target method.
                targetMethod.GenerateCall(generator);

                // Convert the return value.
                if (targetMethod.ReturnType == typeof(void))
                    EmitHelpers.EmitUndefined(generator);
                else
                    EmitConversionToObject(generator, targetMethod.ReturnType);

                // Branch to the end of the method if this was the selected method.
                if (endOfIf != null)
                {
                    generator.Branch(endOfMethod);
                    generator.DefineLabelPosition(endOfIf);
                }
            }

            generator.DefineLabelPosition(endOfMethod);
            generator.Complete();
        }

        /// <summary>
        /// Pops the value on the stack, converts it from an object to the given type, then pushes
        /// the result onto the stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="toType"> The type to convert to. </param>
        /// <param name="convertToAddress"> <c>true</c> if the value is intended for use as an
        /// instance pointer; <c>false</c> otherwise. </param>
        internal static void EmitConversionToType(ILGenerator generator, Type toType, bool convertToAddress)
        {
            // Convert Null.Value to null if the target type is a reference type.
            ILLabel endOfNullCheck = null;
            if (toType.IsValueType == false)
            {
                var startOfElse = generator.CreateLabel();
                endOfNullCheck = generator.CreateLabel();
                generator.Duplicate();
                EmitHelpers.EmitNull(generator);
                generator.BranchIfNotEqual(startOfElse);
                generator.Pop();
                generator.LoadNull();
                generator.Branch(endOfNullCheck);
                generator.DefineLabelPosition(startOfElse);
            }

            switch (Type.GetTypeCode(toType))
            {
                case TypeCode.Boolean:
                    EmitConversion.ToBool(generator, PrimitiveType.Any);
                    break;
                case TypeCode.Byte:
                    EmitConversion.ToInt32(generator, PrimitiveType.Any);
                    break;
                case TypeCode.Char:
                    EmitConversion.ToString(generator, PrimitiveType.Any);
                    generator.Duplicate();
                    generator.Call(ReflectionHelpers.String_Length);
                    generator.LoadInt32(1);
                    var endOfCharCheck = generator.CreateLabel();
                    generator.BranchIfEqual(endOfCharCheck);
                    EmitHelpers.EmitThrow(generator, "TypeError", "Cannot convert string to char - the string must be exactly one character long");
                    generator.DefineLabelPosition(endOfCharCheck);
                    generator.LoadInt32(0);
                    generator.Call(ReflectionHelpers.String_GetChars);
                    break;
                case TypeCode.DBNull:
                    throw new NotSupportedException("DBNull is not a supported parameter type.");
                case TypeCode.Decimal:
                    EmitConversion.ToNumber(generator, PrimitiveType.Any);
                    generator.NewObject(ReflectionHelpers.Decimal_Constructor_Double);
                    break;
                case TypeCode.Double:
                    EmitConversion.ToNumber(generator, PrimitiveType.Any);
                    break;
                case TypeCode.Empty:
                    throw new NotSupportedException("Empty is not a supported return type.");
                case TypeCode.Int16:
                    EmitConversion.ToInt32(generator, PrimitiveType.Any);
                    break;
                case TypeCode.Int32:
                    EmitConversion.ToInt32(generator, PrimitiveType.Any);
                    break;
                case TypeCode.Int64:
                    EmitConversion.ToNumber(generator, PrimitiveType.Any);
                    generator.ConvertToInt64();
                    break;

                case TypeCode.DateTime:
                case TypeCode.Object:
                    // Check if the type must be unwrapped.
                    generator.Duplicate();
                    generator.IsInstance(typeof(Jurassic.Library.ClrInstanceWrapper));
                    var endOfUnwrapCheck = generator.CreateLabel();
                    generator.BranchIfFalse(endOfUnwrapCheck);

                    // Unwrap the wrapped instance.
                    generator.Call(ReflectionHelpers.ClrInstanceWrapper_GetWrappedInstance);
                    generator.DefineLabelPosition(endOfUnwrapCheck);

                    // Value types must be unboxed.
                    if (toType.IsValueType == true)
                    {
                        if (convertToAddress == true)
                            // Unbox.
                            generator.Unbox(toType);
                        else
                            // Unbox and copy to the stack.
                            generator.UnboxAny(toType);

                        //// Calling methods on value required the address of the value type, not the value type itself.
                        //if (argument.Source == BinderArgumentSource.ThisValue && argument.Type.IsValueType == true)
                        //{
                        //    var temp = generator.CreateTemporaryVariable(argument.Type);
                        //    generator.StoreVariable(temp);
                        //    generator.LoadAddressOfVariable(temp);
                        //    generator.ReleaseTemporaryVariable(temp);
                        //}
                    }


                    break;

                case TypeCode.SByte:
                    EmitConversion.ToInt32(generator, PrimitiveType.Any);
                    break;
                case TypeCode.Single:
                    EmitConversion.ToNumber(generator, PrimitiveType.Any);
                    break;
                case TypeCode.String:
                    EmitConversion.ToString(generator, PrimitiveType.Any);
                    break;
                case TypeCode.UInt16:
                    EmitConversion.ToInt32(generator, PrimitiveType.Any);
                    break;
                case TypeCode.UInt32:
                    EmitConversion.ToUInt32(generator, PrimitiveType.Any);
                    break;
                case TypeCode.UInt64:
                    EmitConversion.ToNumber(generator, PrimitiveType.Any);
                    generator.ConvertToUnsignedInt64();
                    break;
            }

            // Label the end of the null check.
            if (toType.IsValueType == false)
                generator.DefineLabelPosition(endOfNullCheck);
        }
            

        /// <summary>
        /// Pops the value on the stack, converts it to an object, then pushes the result onto the
        /// stack.
        /// </summary>
        /// <param name="generator"> The IL generator. </param>
        /// <param name="fromType"> The type to convert from. </param>
        internal static void EmitConversionToObject(ILGenerator generator, Type fromType)
        {
            // If the from type is a reference type, check for null.
            ILLabel endOfNullCheck = null;
            if (fromType.IsValueType == false)
            {
                var startOfElse = generator.CreateLabel();
                endOfNullCheck = generator.CreateLabel();
                generator.Duplicate();
                generator.BranchIfNotNull(startOfElse);
                generator.Pop();
                EmitHelpers.EmitNull(generator);
                generator.Branch(endOfNullCheck);
                generator.DefineLabelPosition(startOfElse);
            }

            switch (Type.GetTypeCode(fromType))
            {
                case TypeCode.Boolean:
                    generator.Box(typeof(bool));
                    break;
                case TypeCode.Byte:
                    generator.Box(typeof(int));
                    break;
                case TypeCode.Char:
                    generator.LoadInt32(1);
                    generator.NewObject(ReflectionHelpers.String_Constructor_Char_Int);
                    break;
                
                case TypeCode.DBNull:
                    throw new NotSupportedException("DBNull is not a supported return type.");
                case TypeCode.Decimal:
                    generator.Call(ReflectionHelpers.Decimal_ToDouble);
                    generator.Box(typeof(double));
                    break;
                case TypeCode.Double:
                    generator.Box(typeof(double));
                    break;
                case TypeCode.Empty:
                    throw new NotSupportedException("Empty is not a supported return type.");
                case TypeCode.Int16:
                    generator.Box(typeof(int));
                    break;
                case TypeCode.Int32:
                    generator.Box(typeof(int));
                    break;
                case TypeCode.Int64:
                    generator.ConvertToDouble();
                    generator.Box(typeof(double));
                    break;

                case TypeCode.DateTime:
                case TypeCode.Object:
                    // Check if the type must be wrapped with a ClrInstanceWrapper.
                    // Note: if the type is a value type it cannot be a primitive or it would
                    // have been handled elsewhere in the switch.
                    ILLabel endOfWrapCheck = null;
                    if (fromType.IsValueType == false)
                    {
                        generator.Duplicate();
                        generator.Call(ReflectionHelpers.TypeUtilities_IsPrimitiveOrObject);
                        endOfWrapCheck = generator.CreateLabel();
                        generator.BranchIfTrue(endOfWrapCheck);
                    }

                    // The type must be wrapped.
                    var temp = generator.CreateTemporaryVariable(fromType);
                    generator.StoreVariable(temp);
                    generator.LoadArgument(0);
                    generator.LoadVariable(temp);
                    if (fromType.IsValueType == true)
                        generator.Box(fromType);
                    generator.ReleaseTemporaryVariable(temp);
                    generator.NewObject(ReflectionHelpers.ClrInstanceWrapper_Constructor);
                    
                    // End of wrap check.
                    if (fromType.IsValueType == false)
                        generator.DefineLabelPosition(endOfWrapCheck);
                    break;

                case TypeCode.SByte:
                    generator.Box(typeof(int));
                    break;
                case TypeCode.Single:
                    generator.Box(typeof(double));
                    break;
                case TypeCode.String:
                    break;
                case TypeCode.UInt16:
                    generator.Box(typeof(int));
                    break;
                case TypeCode.UInt32:
                    generator.Box(typeof(uint));
                    break;
                case TypeCode.UInt64:
                    generator.ConvertUnsignedToDouble();
                    generator.Box(typeof(double));
                    break;
            }

            // Label the end of the null check.
            if (fromType.IsValueType == false)
                generator.DefineLabelPosition(endOfNullCheck);
        }
    }
}
