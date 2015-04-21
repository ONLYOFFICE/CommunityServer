using System;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a "new" expression.
    /// </summary>
    internal sealed class NewExpression : OperatorExpression
    {
        /// <summary>
        /// Creates a new instance of NewExpression.
        /// </summary>
        /// <param name="operator"> The operator to base this expression on. </param>
        public NewExpression(Operator @operator)
            : base(@operator)
        {
        }

        /// <summary>
        /// Gets the precedence of the operator.
        /// </summary>
        public override int Precedence
        {
            get
            {
                // The expression "new String('').toString()" is parsed as
                // "new (String('').toString())" rather than "(new String('')).toString()".
                // There is no way to express this constraint properly using the standard operator
                // rules so we artificially boost the precedence of the operator if a function
                // call is encountered.  Note: GetRawOperand() is used instead of GetOperand(0)
                // because parentheses around the function call affect the result.
                return this.OperandCount == 1 && this.GetRawOperand(0) is FunctionCallExpression ?
                    int.MaxValue : Operator.New.Precedence;
            }
        }

        /// <summary>
        /// Gets the type that results from evaluating this expression.
        /// </summary>
        public override PrimitiveType ResultType
        {
            get { return PrimitiveType.Object; }
        }

        /// <summary>
        /// Generates CIL for the expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Note: we use GetRawOperand() so that grouping operators are not ignored.
            var operand = this.GetRawOperand(0);

            // There is only one operand, and it can be either a reference or a function call.
            // We need to split the operand into a function and some arguments.
            // If the operand is a reference, it is equivalent to a function call with no arguments.
            if (operand is FunctionCallExpression)
            {
                // Emit the function instance first.
                var function = ((FunctionCallExpression)operand).Target;
                function.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToAny(generator, function.ResultType);
            }
            else
            {
                // Emit the function instance first.
                operand.GenerateCode(generator, optimizationInfo);
                EmitConversion.ToAny(generator, operand.ResultType);
            }

            // Check the object really is a function - if not, throw an exception.
            generator.IsInstance(typeof(Library.FunctionInstance));
            generator.Duplicate();
            var endOfTypeCheck = generator.CreateLabel();
            generator.BranchIfNotNull(endOfTypeCheck);

            // Throw an nicely formatted exception.
            var targetValue = generator.CreateTemporaryVariable(typeof(object));
            generator.StoreVariable(targetValue);
            EmitHelpers.LoadScriptEngine(generator);
            generator.LoadString("TypeError");
            generator.LoadString("The new operator requires a function, found a '{0}' instead");
            generator.LoadInt32(1);
            generator.NewArray(typeof(object));
            generator.Duplicate();
            generator.LoadInt32(0);
            generator.LoadVariable(targetValue);
            generator.Call(ReflectionHelpers.TypeUtilities_TypeOf);
            generator.StoreArrayElement(typeof(object));
            generator.Call(ReflectionHelpers.String_Format);
            generator.LoadInt32(optimizationInfo.SourceSpan.StartLine);
            generator.LoadStringOrNull(optimizationInfo.Source.Path);
            generator.LoadStringOrNull(optimizationInfo.FunctionName);
            generator.NewObject(ReflectionHelpers.JavaScriptException_Constructor_Error);
            generator.Throw();
            generator.DefineLabelPosition(endOfTypeCheck);
            generator.ReleaseTemporaryVariable(targetValue);

            if (operand is FunctionCallExpression)
            {
                // Emit an array containing the function arguments.
                ((FunctionCallExpression)operand).GenerateArgumentsArray(generator, optimizationInfo);
            }
            else
            {
                // Emit an empty array.
                generator.LoadInt32(0);
                generator.NewArray(typeof(object));
            }

            // Call FunctionInstance.ConstructLateBound(argumentValues)
            generator.Call(ReflectionHelpers.FunctionInstance_ConstructLateBound);
        }
    }

}