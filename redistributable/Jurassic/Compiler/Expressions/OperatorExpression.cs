using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents a mutable operator expression.  Once all the operands are determined, the
    /// expression is converted into a real operator expression.
    /// </summary>
    internal abstract class OperatorExpression : Expression
    {
        private Expression[] operands;

        /// <summary>
        /// Creates a new instance of OperatorExpression.
        /// </summary>
        /// <param name="operator"> The operator to base this expression on. </param>
        public OperatorExpression(Operator @operator)
        {
            if (@operator == null)
                throw new ArgumentNullException("operator");
            this.Operator = @operator;
            this.operands = new Expression[@operator.Arity];
        }

        /// <summary>
        /// Creates a derived instance of OperatorExpression from the given operator.
        /// </summary>
        /// <param name="operator"> The operator to base this expression on. </param>
        /// <returns> A derived OperatorExpression instance. </returns>
        public static OperatorExpression FromOperator(Operator @operator)
        {
            if (@operator == null)
                throw new ArgumentNullException("operator");
            switch (@operator.Type)
            {
                case OperatorType.Grouping:
                    return new GroupingExpression(@operator);

                case OperatorType.FunctionCall:
                    return new FunctionCallExpression(@operator);

                case OperatorType.MemberAccess:
                case OperatorType.Index:
                    return new MemberAccessExpression(@operator);

                case OperatorType.New:
                    return new NewExpression(@operator);

                case OperatorType.PostIncrement:
                case OperatorType.PostDecrement:
                case OperatorType.PreIncrement:
                case OperatorType.PreDecrement:
                case OperatorType.Assignment:
                case OperatorType.CompoundAdd:
                case OperatorType.CompoundBitwiseAnd:
                case OperatorType.CompoundBitwiseOr:
                case OperatorType.CompoundBitwiseXor:
                case OperatorType.CompoundDivide:
                case OperatorType.CompoundLeftShift:
                case OperatorType.CompoundModulo:
                case OperatorType.CompoundMultiply:
                case OperatorType.CompoundSignedRightShift:
                case OperatorType.CompoundSubtract:
                case OperatorType.CompoundUnsignedRightShift:
                    return new AssignmentExpression(@operator);

                case OperatorType.Conditional:
                    return new TernaryExpression(@operator);

                case OperatorType.Comma:
                    return new ListExpression(@operator);
            }
            if (@operator.Arity == 1)
                return new UnaryExpression(@operator);
            if (@operator.Arity == 2)
                return new BinaryExpression(@operator);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the operator this expression refers to.
        /// </summary>
        public Operator Operator
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the type of operator this expression refers to.
        /// </summary>
        public OperatorType OperatorType
        {
            get { return this.Operator.Type; }
        }

        /// <summary>
        /// Gets the operand with the given index.  No parameter validation and grouping operator
        /// elimination is performed.
        /// </summary>
        /// <param name="index"> The index of the operand to retrieve. </param>
        /// <returns> The operand with the given index. </returns>
        public Expression GetRawOperand(int index)
        {
            return this.operands[index];
        }

        /// <summary>
        /// Gets the operand with the given index.
        /// </summary>
        /// <param name="index"> The index of the operand to retrieve. </param>
        /// <returns> The operand with the given index. </returns>
        public Expression GetOperand(int index)
        {
            if (index < 0 || index >= this.OperandCount)
                throw new ArgumentOutOfRangeException("index");
            Expression result = this.operands[index];
            if (result is GroupingExpression)
                return ((GroupingExpression)result).Operand;
            return result;
        }

        /// <summary>
        /// Gets the number of operands that have been added.
        /// </summary>
        public int OperandCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Adds an operand.
        /// </summary>
        /// <param name="operand"> The expression representing the operand to add. </param>
        public void Push(Expression operand)
        {
            if (this.OperandCount >= this.operands.Length)
                throw new InvalidOperationException("Too many operands.");
            this.operands[this.OperandCount] = operand;
            this.OperandCount ++;
        }

        /// <summary>
        /// Removes and returns the most recently added operand.
        /// </summary>
        /// <returns> The most recently added operand. </returns>
        public Expression Pop()
        {
            if (this.OperandCount == 0)
                throw new InvalidOperationException("Not enough operands.");
            this.OperandCount --;
            return this.operands[this.OperandCount];
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the second token in a ternary operator
        /// was encountered.
        /// </summary>
        public bool SecondTokenEncountered
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value that indicates whether a new operand is acceptable given the state of
        /// this operator.  For ternary operators only two operands are acceptable until the
        /// second token of the sequence is encountered.
        /// </summary>
        public bool AcceptingOperands
        {
            get
            {
                if (this.SecondTokenEncountered == true && this.Operator.HasSecondaryRHSOperand == false)
                    return false;
                return this.OperandCount <
                    (this.Operator.HasLHSOperand ? 1 : 0) + (this.Operator.HasRHSOperand ? 1 : 0) +
                    (this.Operator.HasSecondaryRHSOperand && this.SecondTokenEncountered ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets the right-most operand as an unbound operator, or <c>null</c> if the operator
        /// has no operands or the right-most operand is not an operator.
        /// </summary>
        public OperatorExpression RightBranch
        {
            get { return this.OperandCount == 0 ? null : this.operands[this.OperandCount - 1] as OperatorExpression; }
        }

        /// <summary>
        /// Gets the precedence of the operator.  For ternary operators this is -MinValue if
        /// parsing is currently between the two tokens.
        /// </summary>
        public virtual int Precedence
        {
            get { return this.SecondTokenEncountered == false ? this.Operator.SecondaryPrecedence : this.Operator.TertiaryPrecedence; }
        }

        /// <summary>
        /// Gets an enumerable list of child nodes in the abstract syntax tree.
        /// </summary>
        public override IEnumerable<AstNode> ChildNodes
        {
            get
            {
                for (int i = 0; i < this.OperandCount; i++)
                    yield return this.operands[i];
            }
        }

        /// <summary>
        /// Converts the expression to a string.
        /// </summary>
        /// <returns> A string representing this expression. </returns>
        public override string ToString()
        {
            var result = new System.Text.StringBuilder();

            // Append the first operand.
            int i = 0;
            if (this.Operator.HasLHSOperand)
            {
                result.Append(this.OperandCount > i ? this.operands[i].ToString() : string.Empty);
                i++;
                if (this.Operator.SecondaryToken == null || this.Operator == Operator.Conditional)
                    result.Append(' ');
            }

            // Append the operator.
            result.Append(this.Operator.Token.Text);

            // Append the second operand.
            if (this.Operator.HasRHSOperand)
            {
                if (this.Operator.SecondaryToken == null || this.Operator == Operator.Conditional)
                    result.Append(' ');
                result.Append(this.OperandCount > i ? this.operands[i].ToString() : string.Empty);
                i++;
                if (this.Operator == Operator.Conditional)
                    result.Append(' ');
            }

            // Append the secondary operator, if there is one.
            if (this.Operator.SecondaryToken != null)
                result.Append(this.Operator.SecondaryToken.Text);

            // Append the third operand.
            if (this.Operator.HasSecondaryRHSOperand)
            {
                if (this.Operator == Operator.Conditional)
                    result.Append(' ');
                result.Append(this.OperandCount > i ? this.operands[i].ToString() : string.Empty);
            }

            return result.ToString();
        }
    }

}