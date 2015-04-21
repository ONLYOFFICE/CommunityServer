using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents a reserved word in the source code.
    /// </summary>
    internal class KeywordToken : Token
    {
        /// <summary>
        /// Creates a new KeywordToken instance.
        /// </summary>
        /// <param name="name"> The keyword name. </param>
        public KeywordToken(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the identifier.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        // Keywords.
        public readonly static KeywordToken Break = new KeywordToken("break");
        public readonly static KeywordToken Case = new KeywordToken("case");
        public readonly static KeywordToken Catch = new KeywordToken("catch");
        public readonly static KeywordToken Continue = new KeywordToken("continue");
        public readonly static KeywordToken Debugger = new KeywordToken("debugger");
        public readonly static KeywordToken Default = new KeywordToken("default");
        public readonly static KeywordToken Delete = new KeywordToken("delete");
        public readonly static KeywordToken Do = new KeywordToken("do");
        public readonly static KeywordToken Else = new KeywordToken("else");
        public readonly static KeywordToken Finally = new KeywordToken("finally");
        public readonly static KeywordToken For = new KeywordToken("for");
        public readonly static KeywordToken Function = new KeywordToken("function");
        public readonly static KeywordToken If = new KeywordToken("if");
        public readonly static KeywordToken In = new KeywordToken("in");
        public readonly static KeywordToken InstanceOf = new KeywordToken("instanceof");
        public readonly static KeywordToken New = new KeywordToken("new");
        public readonly static KeywordToken Return = new KeywordToken("return");
        public readonly static KeywordToken Switch = new KeywordToken("switch");
        public readonly static KeywordToken This = new KeywordToken("this");
        public readonly static KeywordToken Throw = new KeywordToken("throw");
        public readonly static KeywordToken Try = new KeywordToken("try");
        public readonly static KeywordToken Typeof = new KeywordToken("typeof");
        public readonly static KeywordToken Var = new KeywordToken("var");
        public readonly static KeywordToken Void = new KeywordToken("void");
        public readonly static KeywordToken While = new KeywordToken("while");
        public readonly static KeywordToken With = new KeywordToken("with");

        // ECMAScript 5 reserved words.
        public readonly static KeywordToken Class = new KeywordToken("class");
        public readonly static KeywordToken Const = new KeywordToken("const");
        public readonly static KeywordToken Enum = new KeywordToken("enum");
        public readonly static KeywordToken Export = new KeywordToken("export");
        public readonly static KeywordToken Extends = new KeywordToken("extends");
        public readonly static KeywordToken Import = new KeywordToken("import");
        public readonly static KeywordToken Super = new KeywordToken("super");

        // Strict-mode reserved words.
        public readonly static KeywordToken Implements = new KeywordToken("implements");
        public readonly static KeywordToken Interface = new KeywordToken("interface");
        public readonly static KeywordToken Let = new KeywordToken("let");
        public readonly static KeywordToken Package = new KeywordToken("package");
        public readonly static KeywordToken Private = new KeywordToken("private");
        public readonly static KeywordToken Protected = new KeywordToken("protected");
        public readonly static KeywordToken Public = new KeywordToken("public");
        public readonly static KeywordToken Static = new KeywordToken("static");
        public readonly static KeywordToken Yield = new KeywordToken("yield");

        // ECMAScript 3 reserved words.
        public readonly static KeywordToken Abstract = new KeywordToken("abstract");
        public readonly static KeywordToken Boolean = new KeywordToken("boolean");
        public readonly static KeywordToken Byte = new KeywordToken("byte");
        public readonly static KeywordToken Char = new KeywordToken("char");
        public readonly static KeywordToken Double = new KeywordToken("double");
        public readonly static KeywordToken Final = new KeywordToken("final");
        public readonly static KeywordToken Float = new KeywordToken("float");
        public readonly static KeywordToken Goto = new KeywordToken("goto");
        public readonly static KeywordToken Int = new KeywordToken("int");
        public readonly static KeywordToken Long = new KeywordToken("long");
        public readonly static KeywordToken Native = new KeywordToken("native");
        public readonly static KeywordToken Short = new KeywordToken("short");
        public readonly static KeywordToken Synchronized = new KeywordToken("synchronized");
        public readonly static KeywordToken Throws = new KeywordToken("throws");
        public readonly static KeywordToken Transient = new KeywordToken("transient");
        public readonly static KeywordToken Volatile = new KeywordToken("volatile");

        // Base keywords.
        private readonly static Token[] keywords = new Token[]
        {
            Break,
            Case,
            Catch,
            Continue,
            Debugger,
            Default,
            Delete,
            Do,
            Else,
            Finally,
            For,
            Function,
            If,
            In,
            InstanceOf,
            New,
            Return,
            Switch,
            This,
            Throw,
            Try,
            Typeof,
            Var,
            Void,
            While,
            With,

            // Literal keywords.
            LiteralToken.True,
            LiteralToken.False,
            LiteralToken.Null,

            // Reserved keywords.
            Class,
            Const,
            Enum,
            Export,
            Extends,
            Import,
            Super,
        };

        // Reserved words (in strict mode).
        private readonly static Token[] strictModeReservedWords = new Token[]
        {
            Implements,
            Interface,
            Let,
            Package,
            Private,
            Protected,
            Public,
            Static,
            Yield,
        };

        // Reserved words (in ECMAScript 3).
        private readonly static Token[] ecmaScript3ReservedWords = new Token[]
        {
            Abstract,
            Boolean,
            Byte,
            Char,
            Double,
            Final,
            Float,
            Goto,
            Implements,
            Int,
            Interface,
            Long,
            Native,
            Package,
            Private,
            Protected,
            Public,
            Short,
            Static,
            Synchronized,
            Throws,
            Transient,
            Volatile,
        };

        // The actual lookup tables are the result of combining two of the lists above.
        private static Dictionary<string, Token> ecmaScript5LookupTable;
        private static Dictionary<string, Token> ecmaScript3LookupTable;
        private static Dictionary<string, Token> strictModeLookupTable;

        /// <summary>
        /// Creates a token from the given string.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <param name="compatibilityMode"> The script engine compatibility mode. </param>
        /// <param name="strictMode"> <c>true</c> if the lexer is operating in strict mode;
        /// <c>false</c> otherwise. </param>
        /// <returns> The token corresponding to the given string, or <c>null</c> if the string
        /// does not represent a valid token. </returns>
        public static Token FromString(string text, CompatibilityMode compatibilityMode, bool strictMode)
        {
            // Determine the lookup table to use.
            Dictionary<string, Token> lookupTable;
            if (compatibilityMode == CompatibilityMode.ECMAScript3)
            {
                // Initialize the ECMAScript 3 lookup table, if it hasn't already been intialized.
                if (ecmaScript3LookupTable == null)
                {
                    lookupTable = InitializeLookupTable(ecmaScript3ReservedWords);
                    System.Threading.Thread.MemoryBarrier();
                    ecmaScript3LookupTable = lookupTable;
                }
                lookupTable = ecmaScript3LookupTable;
            }
            else if (strictMode == false)
            {
                // Initialize the ECMAScript 5 lookup table, if it hasn't already been intialized.
                if (ecmaScript5LookupTable == null)
                {
                    lookupTable = InitializeLookupTable(new Token[0]);
                    System.Threading.Thread.MemoryBarrier();
                    ecmaScript5LookupTable = lookupTable;
                }
                lookupTable = ecmaScript5LookupTable;
            }
            else
            {
                // Initialize the strict mode lookup table, if it hasn't already been intialized.
                if (strictModeLookupTable == null)
                {
                    lookupTable = InitializeLookupTable(strictModeReservedWords);
                    System.Threading.Thread.MemoryBarrier();
                    strictModeLookupTable = lookupTable;
                }
                lookupTable = strictModeLookupTable;
            }

            // Look up the keyword in the lookup table.
            Token result;
            if (lookupTable.TryGetValue(text, out result) == true)
                return result;

            // If the text wasn't found, it is an identifier instead.
            return new IdentifierToken(text);
        }

        /// <summary>
        /// Initializes a lookup table by combining the base list with a second list of keywords.
        /// </summary>
        /// <param name="additionalKeywords"> A list of additional keywords. </param>
        /// <returns> A lookup table. </returns>
        private static Dictionary<string, Token> InitializeLookupTable(Token[] additionalKeywords)
        {
            var result = new Dictionary<string, Token>(keywords.Length + additionalKeywords.Length);
            foreach (var token in keywords)
                result.Add(token.Text, token);
            foreach (var token in additionalKeywords)
                result.Add(token.Text, token);
            return result;
        }

        /// <summary>
        /// Gets a string that represents the token in a parseable form.
        /// </summary>
        public override string Text
        {
            get { return this.Name; }
        }
    }

}