using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a javascript expression statement.
    /// </summary>
    internal class ExpressionStatement : Statement
    {
        /// <summary>
        /// Creates a new ExpressionStatement instance.  By default, this expression does not
        /// contribute to the result of an eval().
        /// </summary>
        /// <param name="expression"> The underlying expression. </param>
        public ExpressionStatement(Expression expression)
            : base(null)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            this.Expression = expression;
        }

        /// <summary>
        /// Creates a new ExpressionStatement instance.  By default, this expression does
        /// contribute to the result of an eval().
        /// </summary>
        /// <param name="labels"> The labels that are associated with this statement. </param>
        /// <param name="expression"> The underlying expression. </param>
        public ExpressionStatement(IList<string> labels, Expression expression)
            : base(labels)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            this.Expression = expression;
            this.ContributesToEvalResult = true;
        }

        /// <summary>
        /// Gets or sets the underlying expression.
        /// </summary>
        public Expression Expression
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the result of this statement should be
        /// returned from an eval() call.  Does not have any effect if the context is not an
        /// EvalContext.  Defaults to <c>false</c>.
        /// </summary>
        public bool ContributesToEvalResult
        {
            get;
            set;
        }

        /// <summary>
        /// Generates CIL for the statement.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals();
            GenerateStartOfStatement(generator, optimizationInfo, statementLocals);

            if (this.ContributesToEvalResult == true && optimizationInfo.EvalResult != null)
            {
                // Emit the expression.
                this.Expression.GenerateCode(generator, optimizationInfo);

                // Store the result.
                EmitConversion.ToAny(generator, this.Expression.ResultType);
                generator.StoreVariable(optimizationInfo.EvalResult);
            }
            else
            {
                // Emit the expression.
                this.Expression.GenerateCode(generator, optimizationInfo);
                generator.Pop();
            }

            // Generate code for the end of the statement.
            GenerateEndOfStatement(generator, optimizationInfo, statementLocals);
        }

        /// <summary>
        /// Gets an enumerable list of child nodes in the abstract syntax tree.
        /// </summary>
        public override IEnumerable<AstNode> ChildNodes
        {
            get { yield return this.Expression; }
        }

        /// <summary>
        /// Converts the statement to a string.
        /// </summary>
        /// <param name="indentLevel"> The number of tabs to include before the statement. </param>
        /// <returns> A string representing this statement. </returns>
        public override string ToString(int indentLevel)
        {
            return string.Format("{0}{1};", new string('\t', indentLevel), this.Expression);
        }
    }

}