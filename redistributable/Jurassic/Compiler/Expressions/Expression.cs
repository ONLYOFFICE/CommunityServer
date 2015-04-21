using System;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents the base class of all javascript expressions.
    /// </summary>
    internal abstract class Expression : AstNode
    {
        /// <summary>
        /// Evaluates the expression, if possible.
        /// </summary>
        /// <returns> The result of evaluating the expression, or <c>null</c> if the expression can
        /// not be evaluated. </returns>
        public virtual object Evaluate()
        {
            return null;
        }

        /// <summary>
        /// Gets the type that results from evaluating this expression.
        /// </summary>
        public virtual PrimitiveType ResultType
        {
            get { throw new NotImplementedException(); }
        }
    }

}