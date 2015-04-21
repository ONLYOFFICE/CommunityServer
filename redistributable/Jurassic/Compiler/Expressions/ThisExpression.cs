using System;

namespace Jurassic.Compiler
{
    
    /// <summary>
    /// Represents a reference to the "this" value.
    /// </summary>
    internal sealed class ThisExpression : Expression
    {
        /// <summary>
        /// Creates a new ThisExpression instance.
        /// </summary>
        public ThisExpression()
        {
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
            //if (optimizationInfo.SuppressReturnValue == true)
            //    return;
            EmitHelpers.LoadThis(generator);
        }

        /// <summary>
        /// Converts the expression to a string.
        /// </summary>
        /// <returns> A string representing this expression. </returns>
        public override string ToString()
        {
            return "this";
        }
    }
}