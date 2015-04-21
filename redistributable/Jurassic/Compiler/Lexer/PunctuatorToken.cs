using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents punctuation or an operator in the source code.
    /// </summary>
    internal class PunctuatorToken : Token
    {
        private string text;

        /// <summary>
        /// Creates a new PunctuatorToken instance.
        /// </summary>
        /// <param name="text"> The punctuator text. </param>
        private PunctuatorToken(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            this.text = text;
        }

        // The full list of punctuators.
        public readonly static PunctuatorToken LeftBrace = new PunctuatorToken("{");
        public readonly static PunctuatorToken RightBrace = new PunctuatorToken("}");
        public readonly static PunctuatorToken LeftParenthesis = new PunctuatorToken("(");
        public readonly static PunctuatorToken RightParenthesis = new PunctuatorToken(")");
        public readonly static PunctuatorToken LeftBracket = new PunctuatorToken("[");
        public readonly static PunctuatorToken RightBracket = new PunctuatorToken("]");
        public readonly static PunctuatorToken Semicolon = new PunctuatorToken(";");
        public readonly static PunctuatorToken Comma = new PunctuatorToken(",");
        public readonly static PunctuatorToken LessThan = new PunctuatorToken("<");
        public readonly static PunctuatorToken GreaterThan = new PunctuatorToken(">");
        public readonly static PunctuatorToken LessThanOrEqual = new PunctuatorToken("<=");
        public readonly static PunctuatorToken GreaterThanOrEqual = new PunctuatorToken(">=");
        public readonly static PunctuatorToken Equality = new PunctuatorToken("==");
        public readonly static PunctuatorToken Inequality = new PunctuatorToken("!=");
        public readonly static PunctuatorToken StrictEquality = new PunctuatorToken("===");
        public readonly static PunctuatorToken StrictInequality = new PunctuatorToken("!==");
        public readonly static PunctuatorToken Plus = new PunctuatorToken("+");
        public readonly static PunctuatorToken Minus = new PunctuatorToken("-");
        public readonly static PunctuatorToken Multiply = new PunctuatorToken("*");
        public readonly static PunctuatorToken Modulo = new PunctuatorToken("%");
        public readonly static PunctuatorToken Increment = new PunctuatorToken("++");
        public readonly static PunctuatorToken Decrement = new PunctuatorToken("--");
        public readonly static PunctuatorToken LeftShift = new PunctuatorToken("<<");
        public readonly static PunctuatorToken SignedRightShift = new PunctuatorToken(">>");
        public readonly static PunctuatorToken UnsignedRightShift = new PunctuatorToken(">>>");
        public readonly static PunctuatorToken BitwiseAnd = new PunctuatorToken("&");
        public readonly static PunctuatorToken BitwiseOr = new PunctuatorToken("|");
        public readonly static PunctuatorToken BitwiseXor = new PunctuatorToken("^");
        public readonly static PunctuatorToken LogicalNot = new PunctuatorToken("!");
        public readonly static PunctuatorToken BitwiseNot = new PunctuatorToken("~");
        public readonly static PunctuatorToken LogicalAnd = new PunctuatorToken("&&");
        public readonly static PunctuatorToken LogicalOr = new PunctuatorToken("||");
        public readonly static PunctuatorToken Conditional = new PunctuatorToken("?");
        public readonly static PunctuatorToken Colon = new PunctuatorToken(":");
        public readonly static PunctuatorToken Assignment = new PunctuatorToken("=");
        public readonly static PunctuatorToken CompoundAdd = new PunctuatorToken("+=");
        public readonly static PunctuatorToken CompoundSubtract = new PunctuatorToken("-=");
        public readonly static PunctuatorToken CompoundMultiply = new PunctuatorToken("*=");
        public readonly static PunctuatorToken CompoundModulo = new PunctuatorToken("%=");
        public readonly static PunctuatorToken CompoundLeftShift = new PunctuatorToken("<<=");
        public readonly static PunctuatorToken CompoundSignedRightShift = new PunctuatorToken(">>=");
        public readonly static PunctuatorToken CompoundUnsignedRightShift = new PunctuatorToken(">>>=");
        public readonly static PunctuatorToken CompoundBitwiseAnd = new PunctuatorToken("&=");
        public readonly static PunctuatorToken CompoundBitwiseOr = new PunctuatorToken("|=");
        public readonly static PunctuatorToken CompoundBitwiseXor = new PunctuatorToken("^=");

        // These are treated specially by the lexer.
        public readonly static PunctuatorToken Dot = new PunctuatorToken(".");
        public readonly static PunctuatorToken Divide = new PunctuatorToken("/");
        public readonly static PunctuatorToken CompoundDivide = new PunctuatorToken("/=");

        // Mapping from text -> punctuator.
        private readonly static Dictionary<string, PunctuatorToken> punctuatorLookupTable = new Dictionary<string, PunctuatorToken>()
        {
            {"{", LeftBrace},
            {"}", RightBrace},
            {"(", LeftParenthesis},
            {")", RightParenthesis},
            {"[", LeftBracket},
            {"]", RightBracket},
            {";", Semicolon},
            {",", Comma},
            {"<", LessThan},
            {">", GreaterThan},
            {"<=", LessThanOrEqual},
            {">=", GreaterThanOrEqual},
            {"==", Equality},
            {"!=", Inequality},
            {"===", StrictEquality},
            {"!==", StrictInequality},
            {"+", Plus},
            {"-", Minus},
            {"*", Multiply},
            {"%", Modulo},
            {"++", Increment},
            {"--", Decrement},
            {"<<", LeftShift},
            {">>", SignedRightShift},
            {">>>", UnsignedRightShift},
            {"&", BitwiseAnd},
            {"|", BitwiseOr},
            {"^", BitwiseXor},
            {"!", LogicalNot},
            {"~", BitwiseNot},
            {"&&", LogicalAnd},
            {"||", LogicalOr},
            {"?", Conditional},
            {":", Colon},
            {"=", Assignment},
            {"+=", CompoundAdd},
            {"-=", CompoundSubtract},
            {"*=", CompoundMultiply},
            {"%=", CompoundModulo},
            {"<<=", CompoundLeftShift},
            {">>=", CompoundSignedRightShift},
            {">>>=", CompoundUnsignedRightShift},
            {"&=", CompoundBitwiseAnd},
            {"|=", CompoundBitwiseOr},
            {"^=", CompoundBitwiseXor},

            // These are treated specially by the lexer.
            {".", Dot},
            {"/", Divide},
            {"/=", CompoundDivide},
        };

        /// <summary>
        /// Creates a punctuator token from the given string.
        /// </summary>
        /// <param name="text"> The punctuator text. </param>
        /// <returns> The punctuator corresponding to the given string, or <c>null</c> if the string
        /// does not represent a valid punctuator. </returns>
        public static PunctuatorToken FromString(string text)
        {
            PunctuatorToken result;
            if (punctuatorLookupTable.TryGetValue(text, out result) == false)
                return null;
            return result;
        }

        /// <summary>
        /// Gets a string that represents the token in a parseable form.
        /// </summary>
        public override string Text
        {
            get { return this.text; }
        }
    }

}