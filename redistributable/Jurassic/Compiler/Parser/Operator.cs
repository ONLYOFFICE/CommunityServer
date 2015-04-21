using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    internal enum OperatorAssociativity
    {
        /// <summary>
        /// Indicates that multiple operators of this type are grouped left-to-right.
        /// </summary>
        LeftToRight,

        /// <summary>
        /// Indicates that multiple operators of this type are grouped right-to-left.
        /// </summary>
        RightToLeft,
    }

    [Flags]
    internal enum OperatorPlacement
    {
        /// <summary>
        /// Indicates that a value is consumed to the left of the primary token.
        /// </summary>
        HasLHSOperand = 1,
        Postfix = 1,

        /// <summary>
        /// Indicates that a value is consumed to the right of the primary token.
        /// </summary>
        HasRHSOperand = 2,
        Prefix = 2,

        /// <summary>
        /// Indicates that values to the left and right of the primary token are consumed.
        /// </summary>
        Binary = 3,

        /// <summary>
        /// Indicates that a value is consumed to the right of the secondary token.
        /// </summary>
        HasSecondaryRHSOperand = 4,

        /// <summary>
        /// Indicates that three values are consumed.
        /// </summary>
        Ternary = 7,

        /// <summary>
        /// Indicates the inner operand is optional.  Only used with the function call operator.
        /// </summary>
        InnerOperandIsOptional = 8,
    }

    /// <summary>
    /// Represents the type of operator.
    /// </summary>
    internal enum OperatorType
    {
        // Special operators.
        Grouping,
        Index,
        MemberAccess,
        FunctionCall,
        New,

        // Unary postfix operators.
        PostIncrement,
        PostDecrement,

        // Unary prefix operators.
        Delete,
        Void,
        Typeof,
        PreIncrement,
        PreDecrement,
        Plus,
        Minus,
        LogicalNot,
        BitwiseNot,

        // Binary operators.
        Multiply,
        Divide,
        Modulo,
        Add,
        Subtract,
        LeftShift,
        SignedRightShift,
        UnsignedRightShift,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        InstanceOf,
        In,
        Equal,
        NotEqual,
        StrictlyEqual,
        StrictlyNotEqual,
        BitwiseAnd,
        BitwiseXor,
        BitwiseOr,
        LogicalAnd,
        LogicalOr,
        Conditional,
        Assignment,
        CompoundAdd,
        CompoundBitwiseAnd,
        CompoundBitwiseOr,
        CompoundBitwiseXor,
        CompoundDivide,
        CompoundModulo,
        CompoundMultiply,
        CompoundLeftShift,
        CompoundSignedRightShift,
        CompoundSubtract,
        CompoundUnsignedRightShift,
        Comma,
    }

    /// <summary>
    /// Represents a JavaScript operator.
    /// </summary>
    internal sealed class Operator
    {
        private static List<Operator> allOperators = new List<Operator>();



        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new operator instance.
        /// </summary>
        /// <param name="token"> The token that corresponds to this operator.  If the operator consists
        /// of multiple tokens (e.g. ?:) then this is the first token in the sequence. </param>
        /// <param name="precedence"> An integer that indicates the order of evaluation.  Higher
        /// precedence operators are evaluated first. </param>
        /// <param name="placement"> A value that indicates where operands are allowed. </param>
        /// <param name="associativity"> Gets a value that indicates whether multiple operators of this
        /// type are grouped left-to-right or right-to-left. </param>
        /// <param name="type"> The type of operator: this decides what algorithm to use to calculate the result. </param>
        /// <param name="secondaryToken"> The second token in the sequence. </param>
        /// <param name="rhsPrecedence"> The precedence for the secondary or tertiary operand. </param>
        private Operator(Token token, int precedence, OperatorPlacement placement, OperatorAssociativity associativity, OperatorType type, Token secondaryToken = null, int rhsPrecedence = -1)
        {
            if (token == null)
                throw new ArgumentNullException("token");
            if (token == secondaryToken)
                throw new ArgumentException("Token and secondaryToken must be different.");
            if ((placement & (OperatorPlacement.HasLHSOperand | OperatorPlacement.HasRHSOperand)) == 0)
                throw new ArgumentException("An operator must take at least one operand.");
            if (secondaryToken == null && (placement & OperatorPlacement.HasSecondaryRHSOperand) != 0)
                throw new ArgumentException("HasSecondaryRHSOperand can only be specified along with a secondary token.");
            if (secondaryToken == null && (placement & OperatorPlacement.InnerOperandIsOptional) != 0)
                throw new ArgumentException("InnerOperandIsOptional can only be specified along with a secondary token.");
            if ((placement & OperatorPlacement.InnerOperandIsOptional) != 0 && (placement & OperatorPlacement.HasRHSOperand) == 0)
                throw new ArgumentException("InnerOperandIsOptional can only be specified along with HasRHSOperand.");
            this.Token = token;
            this.HasLHSOperand = (placement & OperatorPlacement.HasLHSOperand) != 0;
            this.HasRHSOperand = (placement & OperatorPlacement.HasRHSOperand) != 0;
            this.Associativity = associativity;
            this.Type = type;
            this.SecondaryToken = secondaryToken;
            this.HasSecondaryRHSOperand = (placement & OperatorPlacement.HasSecondaryRHSOperand) != 0;
            this.InnerOperandIsOptional = (placement & OperatorPlacement.InnerOperandIsOptional) != 0;
            this.Precedence = this.SecondaryPrecedence = this.TertiaryPrecedence = precedence;
            if (rhsPrecedence >= 0 && this.HasRHSOperand)
                this.SecondaryPrecedence = rhsPrecedence;
            if (rhsPrecedence >= 0 && this.HasSecondaryRHSOperand)
                this.TertiaryPrecedence = rhsPrecedence;

            // Every operator instance is stored in a list for retrieval by the parser.
            allOperators.Add(this);
        }



        //     BASIC PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the type of operator: this decides what algorithm to use to calculate the result.
        /// </summary>
        public OperatorType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the token that corresponds to this operator.  If the operator consists of multiple
        /// tokens (e.g. ?:) then this is the first token in the sequence.
        /// </summary>
        public Token Token
        {
            get;
            private set;
        }

        /// <summary>
        /// For a multi-token operator, gets the second token in the sequence.
        /// </summary>
        public Token SecondaryToken
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets an integer that indicates the order of evaluation.  Higher precedence operators are
        /// evaluated first.
        /// </summary>
        public int Precedence
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets an integer that indicates the order of evaluation for the secondary operand.
        /// Higher precedence operators are evaluated first.
        /// </summary>
        public int SecondaryPrecedence
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets an integer that indicates the order of evaluation for the tertiary operand.
        /// Higher precedence operators are evaluated first.
        /// </summary>
        public int TertiaryPrecedence
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether multiple operators of this type are grouped
        /// left-to-right or right-to-left.
        /// </summary>
        public OperatorAssociativity Associativity
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether a value is consumed to the left of the primary token.
        /// </summary>
        public bool HasLHSOperand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether a value is consumed to the right of the primary token.
        /// </summary>
        public bool HasRHSOperand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether a value is consumed to the right of the secondary token.
        /// Must be <c>false</c> if there is no secondary token.
        /// </summary>
        public bool HasSecondaryRHSOperand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates the inner operand is optional.  Only used with the function
        /// call operator.
        /// </summary>
        public bool InnerOperandIsOptional
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum number of operands required by this operator.
        /// </summary>
        public int Arity
        {
            get { return (this.HasLHSOperand ? 1 : 0) + (this.HasRHSOperand ? 1 : 0) + (this.HasSecondaryRHSOperand ? 1 : 0); }
        }

        /// <summary>
        /// Determines if the given number of operands is valid for this operator.
        /// </summary>
        /// <param name="operandCount"> The number of operands. </param>
        /// <returns> <c>true</c> if the given number of operands is valid for this operator;
        /// <c>false</c> otherwise. </returns>
        public bool IsValidNumberOfOperands(int operandCount)
        {
            if (this.InnerOperandIsOptional)
                return operandCount == ((this.HasLHSOperand ? 1 : 0) + 0 + (this.HasSecondaryRHSOperand ? 1 : 0)) ||
                    operandCount == ((this.HasLHSOperand ? 1 : 0) + 1 + (this.HasSecondaryRHSOperand ? 1 : 0));
            return operandCount == (this.HasLHSOperand ? 1 : 0) + (this.HasRHSOperand ? 1 : 0) + (this.HasSecondaryRHSOperand ? 1 : 0);
        }



        //     WELL KNOWN OPERATORS
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets an enumerable collection of every defined operator.
        /// </summary>
        public static IEnumerable<Operator> AllOperators
        {
            get { return allOperators; }
        }

        // Parenthesis.
        public static readonly Operator Grouping                    = new Operator(PunctuatorToken.LeftParenthesis, 18, OperatorPlacement.HasRHSOperand, OperatorAssociativity.LeftToRight, OperatorType.Grouping, PunctuatorToken.RightParenthesis, 0);

        // Index operator.
        public static readonly Operator Index                       = new Operator(PunctuatorToken.LeftBracket, 17, OperatorPlacement.HasLHSOperand | OperatorPlacement.HasRHSOperand, OperatorAssociativity.LeftToRight, OperatorType.Index, PunctuatorToken.RightBracket, 0);

        // Member access operator and function call operator.
        public static readonly Operator MemberAccess                = new Operator(PunctuatorToken.Dot, 17, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.MemberAccess);
        public static readonly Operator FunctionCall                = new Operator(PunctuatorToken.LeftParenthesis, 17, OperatorPlacement.HasLHSOperand | OperatorPlacement.HasRHSOperand | OperatorPlacement.InnerOperandIsOptional, OperatorAssociativity.LeftToRight, OperatorType.FunctionCall, PunctuatorToken.RightParenthesis, 0);

        // New operator.
        public static readonly Operator New                         = new Operator(KeywordToken.New, 16, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.New);

        // Postfix operators.
        public static readonly Operator PostIncrement               = new Operator(PunctuatorToken.Increment, 15, OperatorPlacement.Postfix, OperatorAssociativity.LeftToRight, OperatorType.PostIncrement);
        public static readonly Operator PostDecrement               = new Operator(PunctuatorToken.Decrement, 15, OperatorPlacement.Postfix, OperatorAssociativity.LeftToRight, OperatorType.PostDecrement);

        // Unary prefix operators.
        public static readonly Operator Delete                      = new Operator(KeywordToken.Delete, 14, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.Delete);
        public static readonly Operator Void                        = new Operator(KeywordToken.Void, 14, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.Void);
        public static readonly Operator Typeof                      = new Operator(KeywordToken.Typeof, 14, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.Typeof);
        public static readonly Operator PreIncrement                = new Operator(PunctuatorToken.Increment, 14, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.PreIncrement);
        public static readonly Operator PreDecrement                = new Operator(PunctuatorToken.Decrement, 14, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.PreDecrement);
        public static readonly Operator Plus                        = new Operator(PunctuatorToken.Plus, 14, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.Plus);
        public static readonly Operator Minus                       = new Operator(PunctuatorToken.Minus, 14, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.Minus);
        public static readonly Operator LogicalNot                  = new Operator(PunctuatorToken.LogicalNot, 14, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.LogicalNot);
        public static readonly Operator BitwiseNot                  = new Operator(PunctuatorToken.BitwiseNot, 14, OperatorPlacement.Prefix, OperatorAssociativity.RightToLeft, OperatorType.BitwiseNot);

        // Arithmetic operators.
        public static readonly Operator Multiply                    = new Operator(PunctuatorToken.Multiply, 13, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.Multiply);
        public static readonly Operator Divide                      = new Operator(PunctuatorToken.Divide, 13, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.Divide);
        public static readonly Operator Modulo                      = new Operator(PunctuatorToken.Modulo, 13, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.Modulo);
        public static readonly Operator Add                         = new Operator(PunctuatorToken.Plus, 12, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.Add);
        public static readonly Operator Subtract                    = new Operator(PunctuatorToken.Minus, 12, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.Subtract);

        // Shift operators.
        public static readonly Operator LeftShift                   = new Operator(PunctuatorToken.LeftShift, 11, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.LeftShift);
        public static readonly Operator SignedRightShift            = new Operator(PunctuatorToken.SignedRightShift, 11, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.SignedRightShift);
        public static readonly Operator UnsignedRightShift          = new Operator(PunctuatorToken.UnsignedRightShift, 11, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.UnsignedRightShift);

        // Relational operators.
        public static readonly Operator LessThan                    = new Operator(PunctuatorToken.LessThan, 10, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.LessThan);
        public static readonly Operator LessThanOrEqual             = new Operator(PunctuatorToken.LessThanOrEqual, 10, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.LessThanOrEqual);
        public static readonly Operator GreaterThan                 = new Operator(PunctuatorToken.GreaterThan, 10, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.GreaterThan);
        public static readonly Operator GreaterThanOrEqual          = new Operator(PunctuatorToken.GreaterThanOrEqual, 10, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.GreaterThanOrEqual);

        // InstanceOf and In operators.
        public static readonly Operator InstanceOf                  = new Operator(KeywordToken.InstanceOf, 10, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.InstanceOf);
        public static readonly Operator In                          = new Operator(KeywordToken.In, 10, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.In);

        // Relational operators.
        public static readonly Operator Equal                       = new Operator(PunctuatorToken.Equality, 9, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.Equal);
        public static readonly Operator NotEqual                    = new Operator(PunctuatorToken.Inequality, 9, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.NotEqual);
        public static readonly Operator StrictlyEqual               = new Operator(PunctuatorToken.StrictEquality, 9, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.StrictlyEqual);
        public static readonly Operator StrictlyNotEqual            = new Operator(PunctuatorToken.StrictInequality, 9, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.StrictlyNotEqual);

        // Bitwise operators.
        public static readonly Operator BitwiseAnd                  = new Operator(PunctuatorToken.BitwiseAnd, 8, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.BitwiseAnd);
        public static readonly Operator BitwiseXor                  = new Operator(PunctuatorToken.BitwiseXor, 7, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.BitwiseXor);
        public static readonly Operator BitwiseOr                   = new Operator(PunctuatorToken.BitwiseOr, 6, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.BitwiseOr);

        // Logical operators.
        public static readonly Operator LogicalAnd                  = new Operator(PunctuatorToken.LogicalAnd, 5, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.LogicalAnd);
        public static readonly Operator LogicalOr                   = new Operator(PunctuatorToken.LogicalOr, 4, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.LogicalOr);

        // Conditional operator.
        public static readonly Operator Conditional                 = new Operator(PunctuatorToken.Conditional, 3, OperatorPlacement.Ternary, OperatorAssociativity.RightToLeft, OperatorType.Conditional, PunctuatorToken.Colon, 2);

        // Assignment operators.
        public static readonly Operator Assignment                  = new Operator(PunctuatorToken.Assignment, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.Assignment);
        public static readonly Operator CompoundAdd                 = new Operator(PunctuatorToken.CompoundAdd, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundAdd);
        public static readonly Operator CompoundBitwiseAnd          = new Operator(PunctuatorToken.CompoundBitwiseAnd, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundBitwiseAnd);
        public static readonly Operator CompoundBitwiseOr           = new Operator(PunctuatorToken.CompoundBitwiseOr, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundBitwiseOr);
        public static readonly Operator CompoundBitwiseXor          = new Operator(PunctuatorToken.CompoundBitwiseXor, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundBitwiseXor);
        public static readonly Operator CompoundDivide              = new Operator(PunctuatorToken.CompoundDivide, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundDivide);
        public static readonly Operator CompoundModulo              = new Operator(PunctuatorToken.CompoundModulo, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundModulo);
        public static readonly Operator CompoundMultiply            = new Operator(PunctuatorToken.CompoundMultiply, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundMultiply);
        public static readonly Operator CompoundLeftShift           = new Operator(PunctuatorToken.CompoundLeftShift, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundLeftShift);
        public static readonly Operator CompoundSignedRightShift    = new Operator(PunctuatorToken.CompoundSignedRightShift, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundSignedRightShift);
        public static readonly Operator CompoundSubtract            = new Operator(PunctuatorToken.CompoundSubtract, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundSubtract);
        public static readonly Operator CompoundUnsignedRightShift  = new Operator(PunctuatorToken.CompoundUnsignedRightShift, 2, OperatorPlacement.Binary, OperatorAssociativity.RightToLeft, OperatorType.CompoundUnsignedRightShift);

        // Comma operator.
        public static readonly Operator Comma                       = new Operator(PunctuatorToken.Comma, 1, OperatorPlacement.Binary, OperatorAssociativity.LeftToRight, OperatorType.Comma);
    }

}