using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jurassic.Compiler
{

    /// <summary>
    /// This class is public for technical reasons and is intended only for internal use.
    /// </summary>
    public static class BinderUtilities
    {
        /// <summary>
        /// Given a set of methods and a set of arguments, determines whether one of the methods
        /// can be unambiguously selected.  Throws an exception if this is not the case.
        /// </summary>
        /// <param name="methodHandles"> An array of handles to the candidate methods. </param>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="thisValue"> The value of the "this" keyword. </param>
        /// <param name="arguments"> An array of parameter values. </param>
        /// <returns> The index of the selected method. </returns>
        public static int ResolveOverloads(RuntimeMethodHandle[] methodHandles, ScriptEngine engine, object thisValue, object[] arguments)
        {
            // Get methods from the handles.
            var methods = new BinderMethod[methodHandles.Length];
            for (int i = 0; i < methodHandles.Length; i++)
                methods[i] = new BinderMethod(MethodBase.GetMethodFromHandle(methodHandles[i]));

            // Keep a score for each method.  Add one point if a type conversion is required, or
            // a million points if a type conversion cannot be performed.
            int[] demeritPoints = new int[methods.Length];
            const int disqualification = 65536;
            for (int i = 0; i < methods.Length; i++)
            {
                foreach (var argument in methods[i].GetArguments(arguments.Length))
                {
                    // Get the input parameter.
                    object input;
                    switch (argument.Source)
                    {
                        case BinderArgumentSource.ThisValue:
                            input = thisValue;
                            break;
                        case BinderArgumentSource.InputParameter:
                            input = arguments[argument.InputParameterIndex];
                            break;
                        default:
                            continue;
                    }

                    // Unwrap the input parameter.
                    if (input is Jurassic.Library.ClrInstanceWrapper)
                        input = ((Jurassic.Library.ClrInstanceWrapper)input).WrappedInstance;

                    // Get the type of the output parameter.
                    Type outputType = argument.Type;

                    switch (Type.GetTypeCode(outputType))
                    {
                        case TypeCode.Boolean:
                            if ((input is bool) == false)
                                demeritPoints[i] += disqualification;
                            break;

                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Decimal:
                            if (TypeUtilities.IsNumeric(input) == true)
                                demeritPoints[i] ++;
                            else
                                demeritPoints[i] += disqualification;
                            break;

                        case TypeCode.Double:
                            if (TypeUtilities.IsNumeric(input) == false)
                                demeritPoints[i] += disqualification;
                            break;

                        case TypeCode.Char:
                            if (TypeUtilities.IsString(input) == true)
                                demeritPoints[i]++;
                            else
                                demeritPoints[i] += disqualification;
                            break;

                        case TypeCode.String:
                            if (TypeUtilities.IsString(input) == false && input != Null.Value)
                                demeritPoints[i] += disqualification;
                            break;

                        case TypeCode.DateTime:
                        case TypeCode.Object:
                            if (input == null || input == Undefined.Value)
                            {
                                demeritPoints[i] += disqualification;
                            }
                            else if (input == Null.Value)
                            {
                                if (outputType.IsValueType == true)
                                    demeritPoints[i] += disqualification;
                            }
                            else if (outputType.IsAssignableFrom(input.GetType()) == false)
                            {
                                demeritPoints[i] += disqualification;
                            }
                            break;


                        case TypeCode.Empty:
                        case TypeCode.DBNull:
                            throw new NotSupportedException(string.Format("{0} is not a supported parameter type.", outputType));
                    }
                }
                
            }

            // Find the method(s) with the fewest number of demerit points.
            int lowestScore = int.MaxValue;
            var lowestIndices = new List<int>();
            for (int i = 0; i < methods.Length; i++)
            {
                if (demeritPoints[i] < lowestScore)
                {
                    lowestScore = demeritPoints[i];
                    lowestIndices.Clear();
                }
                if (demeritPoints[i] <= lowestScore)
                    lowestIndices.Add(i);
            }

            // Throw an error if the match is ambiguous.
            if (lowestIndices.Count > 1)
            {
                var ambiguousMethods = new List<BinderMethod>(lowestIndices.Count);
                foreach (var index in lowestIndices)
                    ambiguousMethods.Add(methods[index]);
                throw new JavaScriptException(engine, "TypeError", "The method call is ambiguous between the following methods: " + StringHelpers.Join(", ", ambiguousMethods));
            }

            // Throw an error is there is an invalid argument.
            if (lowestIndices.Count == 1 && lowestScore >= disqualification)
                throw new JavaScriptException(engine, "TypeError", string.Format("The best method overload {0} has some invalid arguments", methods[lowestIndices[0]]));

            return lowestIndices[0];
        }
    }

}
