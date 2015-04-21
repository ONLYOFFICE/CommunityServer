using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents an assignment expression (++, --, =, +=, -=, *=, /=, %=, &=, |=, ^=, &lt;&lt;=, &gt;&gt;=, &gt;&gt;&gt;=).
    /// </summary>
    internal class AssignmentExpression : OperatorExpression
    {
        /// <summary>
        /// Creates a new instance of AssignmentExpression.
        /// </summary>
        /// <param name="operator"> The operator to base this expression on. </param>
        public AssignmentExpression(Operator @operator)
            : base(@operator)
        {
        }

        /// <summary>
        /// Creates a simple variable assignment expression.
        /// </summary>
        /// <param name="scope"> The scope the variable is defined within. </param>
        /// <param name="name"> The name of the variable to set. </param>
        /// <param name="value"> The value to set the variable to. </param>
        public AssignmentExpression(Scope scope, string name, Expression value)
            : base(Operator.Assignment)
        {
            this.Push(new NameExpression(scope, name));
            this.Push(value);
        }

        /// <summary>
        /// Gets the target of the assignment.
        /// </summary>
        public IReferenceExpression Target
        {
            get { return this.GetOperand(0) as IReferenceExpression; }
        }

        /// <summary>
        /// Gets the underlying base operator for the given compound operator.
        /// </summary>
        /// <param name="compoundOperatorType"> The type of compound operator. </param>
        /// <returns> The underlying base operator, or <c>null</c> if the type is not a compound
        /// operator. </returns>
        private static Operator GetCompoundBaseOperator(OperatorType compoundOperatorType)
        {
            switch (compoundOperatorType)
            {
                case OperatorType.CompoundAdd:
                    return Operator.Add;
                case OperatorType.CompoundBitwiseAnd:
                    return Operator.BitwiseAnd;
                case OperatorType.CompoundBitwiseOr:
                    return Operator.BitwiseOr;
                case OperatorType.CompoundBitwiseXor:
                    return Operator.BitwiseXor;
                case OperatorType.CompoundDivide:
                    return Operator.Divide;
                case OperatorType.CompoundLeftShift:
                    return Operator.LeftShift;
                case OperatorType.CompoundModulo:
                    return Operator.Modulo;
                case OperatorType.CompoundMultiply:
                    return Operator.Multiply;
                case OperatorType.CompoundSignedRightShift:
                    return Operator.SignedRightShift;
                case OperatorType.CompoundSubtract:
                    return Operator.Subtract;
                case OperatorType.CompoundUnsignedRightShift:
                    return Operator.UnsignedRightShift;
            }
            return null;
        }

        /// <summary>
        /// Gets the type that results from evaluating this expression.
        /// </summary>
        public override PrimitiveType ResultType
        {
            get
            {
                var type = this.OperatorType;
                if (type == OperatorType.PostIncrement ||
                    type == OperatorType.PostDecrement ||
                    type == OperatorType.PreIncrement ||
                    type == OperatorType.PreDecrement)
                    return PrimitiveType.Number;
                if (type == OperatorType.Assignment)
                    return this.GetOperand(1).ResultType;
                var compoundOperator = new BinaryExpression(GetCompoundBaseOperator(type), this.GetOperand(0), this.GetOperand(1));
                return compoundOperator.ResultType;
            }
        }

        /// <summary>
        /// Generates CIL for the expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // The left hand side needs to be a variable reference or member access.
            var target = this.GetOperand(0) as IReferenceExpression;
            if (target == null)
            {
                // Emit an error message.
                switch (this.OperatorType)
                {
                    case OperatorType.PostIncrement:
                    case OperatorType.PostDecrement:
                        EmitHelpers.EmitThrow(generator, "ReferenceError", "Invalid left-hand side in postfix operation", optimizationInfo);
                        break;
                    case OperatorType.PreIncrement:
                    case OperatorType.PreDecrement:
                        EmitHelpers.EmitThrow(generator, "ReferenceError", "Invalid left-hand side in prefix operation", optimizationInfo);
                        break;
                    case OperatorType.Assignment:
                    default:
                        EmitHelpers.EmitThrow(generator, "ReferenceError", "Invalid left-hand side in assignment", optimizationInfo);
                        break;
                }
                //if (optimizationInfo.SuppressReturnValue == false)
                    EmitHelpers.EmitDefaultValue(generator, this.ResultType);
                return;
            }

            // The left hand side cannot be "arguments" or "eval" in strict mode.
            if (optimizationInfo.StrictMode == true && target is NameExpression)
            {
                if (((NameExpression)target).Name == "eval")
                    throw new JavaScriptException(optimizationInfo.Engine, "SyntaxError", "The variable 'eval' cannot be modified in strict mode.", optimizationInfo.SourceSpan.StartLine, optimizationInfo.Source.Path, optimizationInfo.FunctionName);
                if (((NameExpression)target).Name == "arguments")
                    throw new JavaScriptException(optimizationInfo.Engine, "SyntaxError", "The variable 'arguments' cannot be modified in strict mode.", optimizationInfo.SourceSpan.StartLine, optimizationInfo.Source.Path, optimizationInfo.FunctionName);
            }

            switch (this.OperatorType)
            {
                case OperatorType.Assignment:
                    // Standard assignment operator.
                    GenerateAssignment(generator, optimizationInfo, target);
                    break;

                case OperatorType.PostIncrement:
                    GenerateIncrementOrDecrement(generator, optimizationInfo, target, postfix: true, increment: true);
                    break;
                case OperatorType.PostDecrement:
                    GenerateIncrementOrDecrement(generator, optimizationInfo, target, postfix: true, increment: false);
                    break;
                case OperatorType.PreIncrement:
                    GenerateIncrementOrDecrement(generator, optimizationInfo, target, postfix: false, increment: true);
                    break;
                case OperatorType.PreDecrement:
                    GenerateIncrementOrDecrement(generator, optimizationInfo, target, postfix: false, increment: false);
                    break;

                case OperatorType.CompoundAdd:
                    // Special case +=
                    GenerateCompoundAddAssignment(generator, optimizationInfo, target);
                    break;

                default:
                    // All other compound operators.
                    GenerateCompoundAssignment(generator, optimizationInfo, target);
                    break;
            }
        }

        /// <summary>
        /// Generates CIL for an assignment expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="target"> The target to modify. </param>
        private void GenerateAssignment(ILGenerator generator, OptimizationInfo optimizationInfo, IReferenceExpression target)
        {
            // Load the value to assign.
            var rhs = this.GetOperand(1);
            rhs.GenerateCode(generator, optimizationInfo);

            // Support the inferred function displayName property.
            if (rhs is FunctionExpression)
                ((FunctionExpression)rhs).GenerateDisplayName(generator, optimizationInfo, target.ToString(), false);

            // Duplicate the value so it remains on the stack afterwards.
            //if (optimizationInfo.SuppressReturnValue == false)
            generator.Duplicate();

            // Store the value.
            target.GenerateSet(generator, optimizationInfo, rhs.ResultType, optimizationInfo.StrictMode);
        }

        /// <summary>
        /// Generates CIL for an increment or decrement expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="target"> The target to modify. </param>
        /// <param name="postfix"> <c>true</c> if this is the postfix version of the operator;
        /// <c>false</c> otherwise. </param>
        /// <param name="increment"> <c>true</c> if this is the increment operator; <c>false</c> if
        /// this is the decrement operator. </param>
        private void GenerateIncrementOrDecrement(ILGenerator generator, OptimizationInfo optimizationInfo, IReferenceExpression target, bool postfix, bool increment)
        {
            // Note: increment and decrement can produce a number that is out of range if the
            // target is of type Int32.  The only time this should happen is for a loop variable
            // where the range has been carefully checked to make sure an out of range condition
            // cannot happen.

            // Get the target value.
            target.GenerateGet(generator, optimizationInfo, true);

            // Convert it to a number.
            if (target.Type != PrimitiveType.Int32)
                EmitConversion.ToNumber(generator, target.Type);

            // If this is PostIncrement or PostDecrement, duplicate the value so it can be produced as the return value.
            if (postfix == true)
                generator.Duplicate();

            // Load the increment constant.
            if (target.Type == PrimitiveType.Int32)
                generator.LoadInt32(1);
            else
                generator.LoadDouble(1.0);

            // Add or subtract the constant to the target value.
            if (increment == true)
                generator.Add();
            else
                generator.Subtract();

            // If this is PreIncrement or PreDecrement, duplicate the value so it can be produced as the return value.
            if (postfix == false)
                generator.Duplicate();

            // Store the value.
            target.GenerateSet(generator, optimizationInfo, target.Type == PrimitiveType.Int32 ? PrimitiveType.Int32 : PrimitiveType.Number, optimizationInfo.StrictMode);
        }

        /// <summary>
        /// Generates CIL for a compound assignment expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="target"> The target to modify. </param>
        private void GenerateCompoundAddAssignment(ILGenerator generator, OptimizationInfo optimizationInfo, IReferenceExpression target)
        {
            //var rhs = this.GetOperand(1);
            //if (PrimitiveTypeUtilities.IsString(rhs.ResultType) == true)
            //{
            //    // Load the value of the left-hand side and convert it to a concantenated string.
            //    target.GenerateGet(generator, optimizationInfo, true);
            //    EmitConversion.ToConcatenatedString(generator, target.Type);

            //    // Transform expressions of the form "a += b + c;" into "a += b; a += c;".
            //    List<Expression> nonAddExpressions = new List<Expression>();
            //    Stack<Expression> expressionStack = new Stack<Expression>(1);
            //    expressionStack.Push(rhs);
            //    do
            //    {
            //        var expression = expressionStack.Pop();
            //        if (expression is BinaryExpression && ((BinaryExpression)expression).OperatorType == OperatorType.Add)
            //        {
            //            expressionStack.Push(((BinaryExpression)expression).Right);
            //            expressionStack.Push(((BinaryExpression)expression).Left);
            //        }
            //        else
            //            nonAddExpressions.Add(expression);
            //    } while (expressionStack.Count > 0);

            //    foreach (var nonAddExpression in nonAddExpressions)
            //    {
            //        // Duplicate the ConcatenatedString instance.
            //        generator.Duplicate();

            //        nonAddExpression.GenerateCode(generator, optimizationInfo);
            //        var rhsType = nonAddExpression.ResultType;
            //        if (rhsType == PrimitiveType.String)
            //        {
            //            // Concatenate.
            //            generator.Call(ReflectionHelpers.ConcatenatedString_Append_String);
            //        }
            //        else if (rhsType == PrimitiveType.ConcatenatedString)
            //        {
            //            // Concatenate.
            //            generator.Call(ReflectionHelpers.ConcatenatedString_Append_ConcatenatedString);
            //        }
            //        else
            //        {
            //            // Convert the operand to an object.
            //            EmitConversion.ToAny(generator, rhsType);

            //            // Concatenate.
            //            generator.Call(ReflectionHelpers.ConcatenatedString_Append_Object);
            //        }
            //    }

            //    if (target.Type != PrimitiveType.ConcatenatedString)
            //    {
            //        // Set the original variable.
            //        generator.Duplicate();
            //        target.GenerateSet(generator, optimizationInfo, PrimitiveType.ConcatenatedString, optimizationInfo.StrictMode);
            //    }
            //}
            //else
            //{
                // Do the standard compound add.
                GenerateCompoundAssignment(generator, optimizationInfo, target);
            //}
        }

        /// <summary>
        /// Generates CIL for a compound assignment expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        /// <param name="target"> The target to modify. </param>
        private void GenerateCompoundAssignment(ILGenerator generator, OptimizationInfo optimizationInfo, IReferenceExpression target)
        {
            // Load the value to assign.
            var compoundOperator = new BinaryExpression(GetCompoundBaseOperator(this.OperatorType), this.GetOperand(0), this.GetOperand(1));
            compoundOperator.GenerateCode(generator, optimizationInfo);

            // Duplicate the value so it remains on the stack afterwards.
            //if (optimizationInfo.SuppressReturnValue == false)
            generator.Duplicate();

            // Store the value.
            target.GenerateSet(generator, optimizationInfo, compoundOperator.ResultType, optimizationInfo.StrictMode);
        }
    }

}