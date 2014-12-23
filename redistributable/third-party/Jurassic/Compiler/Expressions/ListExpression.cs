using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a comma-delimited list.
    /// </summary>
    internal class ListExpression : OperatorExpression
    {
        /// <summary>
        /// Creates a new instance of ListExpression.
        /// </summary>
        /// <param name="operator"> The operator to base this expression on. </param>
        public ListExpression(Operator @operator)
            : base(@operator)
        {
        }

        /// <summary>
        /// Gets an array of expressions, one for each item in the list.
        /// </summary>
        public IList<Expression> Items
        {
            get
            {
                var result = new List<Expression>();
                Expression leftHandSide = this;
                while (leftHandSide is ListExpression)
                {
                    result.Add(((ListExpression)leftHandSide).GetRawOperand(1));
                    leftHandSide = ((ListExpression)leftHandSide).GetRawOperand(0);
                }
                result.Add(leftHandSide);
                result.Reverse();
                return result;
            }
        }

        /// <summary>
        /// Evaluates the expression, if possible.
        /// </summary>
        /// <returns> The result of evaluating the expression, or <c>null</c> if the expression can
        /// not be evaluated. </returns>
        public override object Evaluate()
        {
            // Evaluate the operands.
            var left = this.GetRawOperand(0).Evaluate();
            if (left == null)
                return null;
            var right = this.GetRawOperand(1).Evaluate();
            if (right == null)
                return null;

            // Return the last value.
            return right;
        }

        /// <summary>
        /// Gets the type that results from evaluating this expression.
        /// </summary>
        public override PrimitiveType ResultType
        {
            get
            {
                return this.GetOperand(1).ResultType;
            }
        }

        /// <summary>
        /// Generates CIL for the expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Get an array of comma-delimited expressions.
            var items = this.Items;

            for (int i = 0; i < items.Count - 1; i++)
            {
                // Generate code for the item, evaluating the side-effects but not producing any values.
                items[i].GenerateCode(generator, optimizationInfo); //.AddFlags(OptimizationFlags.SuppressReturnValue));
                generator.Pop();
            }

            // Generate code for the last item and return the value.
            items[items.Count - 1].GenerateCode(generator, optimizationInfo);
        }

        /// <summary>
        /// Converts the expression to a string.
        /// </summary>
        /// <returns> A string representing this expression. </returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}", this.GetOperand(0), this.GetOperand(1));
        }
    }

}