using System;
using System.Collections.Generic;
using System.Diagnostics;
using Jurassic.Library;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Converts a series of tokens into an abstract syntax tree.
    /// </summary>
    internal sealed class Parser
    {
        private ScriptEngine engine;
        private Lexer lexer;
        private SourceCodePosition positionBeforeWhitespace, positionAfterWhitespace;
        private Token nextToken;
        private bool consumedLineTerminator;
        private ParserExpressionState expressionState;
        private Scope initialScope;
        private Scope currentScope;
        private MethodOptimizationHints methodOptimizationHints;
        private List<string> labelsForCurrentStatement = new List<string>();
        private Token endToken;
        private CompilerOptions options;
        private CodeContext context;
        private bool strictMode;



        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a Parser instance with the given lexer supplying the tokens.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="lexer"> The lexical analyser that provides the tokens. </param>
        /// <param name="initialScope"> The initial variable scope. </param>
        /// <param name="options"> Options that influence the compiler. </param>
        /// <param name="context"> The context of the code (global, function or eval). </param>
        public Parser(ScriptEngine engine, Lexer lexer, Scope initialScope, CompilerOptions options, CodeContext context)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (lexer == null)
                throw new ArgumentNullException("lexer");
            if (initialScope == null)
                throw new ArgumentNullException("initialScope");
            this.engine = engine;
            this.lexer = lexer;
            this.lexer.ParserExpressionState = ParserExpressionState.Literal;
            this.currentScope = this.initialScope = initialScope;
            this.methodOptimizationHints = new MethodOptimizationHints();
            this.options = options;
            this.context = context;
            this.StrictMode = options.ForceStrictMode;
            this.Consume();
        }

        /// <summary>
        /// Creates a parser that can read the body of a function.
        /// </summary>
        /// <param name="parser"> The parser for the parent context. </param>
        /// <param name="scope"> The function scope. </param>
        /// <returns> A new parser. </returns>
        private static Parser CreateFunctionBodyParser(Parser parser, Scope scope)
        {
            var result = (Parser)parser.MemberwiseClone();
            result.currentScope = result.initialScope = scope;
            result.methodOptimizationHints = new MethodOptimizationHints();
            result.context = CodeContext.Function;
            result.endToken = PunctuatorToken.RightBrace;
            result.DirectivePrologueProcessedCallback = null;
            return result;
        }



        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the line number of the next token.
        /// </summary>
        public int LineNumber
        {
            get { return this.positionAfterWhitespace.Line; }
        }

        /// <summary>
        /// Gets the position just after the last character of the previously consumed token.
        /// </summary>
        public SourceCodePosition PositionBeforeWhitespace
        {
            get { return this.positionBeforeWhitespace; }
        }

        /// <summary>
        /// Gets the position of the first character of the next token.
        /// </summary>
        public SourceCodePosition PositionAfterWhitespace
        {
            get { return this.positionAfterWhitespace; }
        }

        /// <summary>
        /// Gets the path or URL of the source file.  Can be <c>null</c>.
        /// </summary>
        public string SourcePath
        {
            get { return this.lexer.Source.Path; }
        }

        /// <summary>
        /// Gets or sets the scope that variables are declared in.
        /// </summary>
        public Scope Scope
        {
            get { return this.currentScope; }
            set { this.currentScope = value; }
        }

        /// <summary>
        /// Gets or sets the scope that function declarations are declared in.
        /// </summary>
        public Scope InitialScope
        {
            get { return this.initialScope; }
            set { this.initialScope = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parser is operating in strict mode.
        /// </summary>
        public bool StrictMode
        {
            get { return this.strictMode; }
            set
            {
                this.strictMode = value;
                this.lexer.StrictMode = value;
            }
        }

        /// <summary>
        /// Gets or sets a callback that is called after the directive prologue has been processed.
        /// This callback will be called even if no directive prologue exists.
        /// </summary>
        public Action<Parser> DirectivePrologueProcessedCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Gets optimization information about the code that was parsed (Parse() must be called
        /// first).
        /// </summary>
        public MethodOptimizationHints MethodOptimizationHints
        {
            get { return this.methodOptimizationHints; }
        }



        //     VARIABLES
        //_________________________________________________________________________________________
     
        /// <summary>
        /// Throws an exception if the variable name is invalid.
        /// </summary>
        /// <param name="name"> The name of the variable to check. </param>
        private void ValidateVariableName(string name)
        {
            // In strict mode, the variable name cannot be "eval" or "arguments".
            if (this.StrictMode == true && (name == "eval" || name == "arguments"))
                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("The variable name cannot be '{0}' in strict mode.", name), this.LineNumber, this.SourcePath);

            // Record each occurance of a variable name.
            this.methodOptimizationHints.EncounteredVariable(name);
        }



        //     TOKEN HELPERS
        //_________________________________________________________________________________________

        /// <summary>
        /// Discards the current token and reads the next one.
        /// </summary>
        /// <param name="expressionState"> Indicates whether the next token can be a literal or an
        /// operator. </param>
        private void Consume(ParserExpressionState expressionState = ParserExpressionState.Literal)
        {
            this.expressionState = expressionState;
            this.lexer.ParserExpressionState = expressionState;
            this.consumedLineTerminator = false;
            this.positionBeforeWhitespace = new SourceCodePosition(this.lexer.LineNumber, this.lexer.ColumnNumber);
            this.positionAfterWhitespace = this.positionBeforeWhitespace;
            while (true)
            {
                this.nextToken = this.lexer.NextToken();
                if ((this.nextToken is WhiteSpaceToken) == false)
                    break;
                if (((WhiteSpaceToken)this.nextToken).LineTerminatorCount > 0)
                    this.consumedLineTerminator = true;
                this.positionAfterWhitespace = new SourceCodePosition(this.lexer.LineNumber, this.lexer.ColumnNumber);
            }
        }

        /// <summary>
        /// Indicates that the next token is identical to the given one.  Throws an exception if
        /// this is not the case.  Consumes the token.
        /// </summary>
        /// <param name="token"> The expected token. </param>
        private void Expect(Token token)
        {
            if (this.nextToken == token)
                Consume();
            else
                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Expected '{0}' but found {1}", token.Text, Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);
        }

        /// <summary>
        /// Indicates that the next token should be an identifier.  Throws an exception if this is
        /// not the case.  Consumes the token.
        /// </summary>
        /// <returns> The identifier name. </returns>
        private string ExpectIdentifier()
        {
            var token = this.nextToken;
            if (token is IdentifierToken)
            {
                Consume();
                return ((IdentifierToken)token).Name;
            }
            else
            {
                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Expected identifier but found {0}", Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the current position is a valid position to end
        /// a statement.
        /// </summary>
        /// <returns> <c>true</c> if the current position is a valid position to end a statement;
        /// <c>false</c> otherwise. </returns>
        private bool AtValidEndOfStatement()
        {
            // A statement can be terminator in four ways: by a semi-colon (;), by a right brace (}),
            // by the end of a line or by the end of the program.
            return this.nextToken == PunctuatorToken.Semicolon ||
                this.nextToken == PunctuatorToken.RightBrace ||
                this.consumedLineTerminator == true ||
                this.nextToken == null;
        }

        /// <summary>
        /// Indicates that the next token should end the current statement.  This implies that the
        /// next token is a semicolon, right brace or a line terminator.
        /// </summary>
        private void ExpectEndOfStatement()
        {
            if (this.nextToken == PunctuatorToken.Semicolon)
                Consume();
            else
            {
                // Automatic semi-colon insertion.
                // If an illegal token is found then a semicolon is automatically inserted before
                // the offending token if one or more of the following conditions is true: 
                // 1. The offending token is separated from the previous token by at least one LineTerminator.
                // 2. The offending token is '}'.
                if (this.consumedLineTerminator == true || this.nextToken == PunctuatorToken.RightBrace)
                    return;

                // If the end of the input stream of tokens is encountered and the parser is unable
                // to parse the input token stream as a single complete ECMAScript Program, then a
                // semicolon is automatically inserted at the end of the input stream.
                if (this.nextToken == null)
                    return;

                // Otherwise, throw an error.
                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Expected ';' but found {0}", Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);
            }
        }



        //     PARSE METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Parses javascript source code.
        /// </summary>
        /// <returns> An expression that can be executed to run the program represented by the
        /// source code. </returns>
        public Statement Parse()
        {
            // Read the directive prologue.
            var result = new BlockStatement(new string[0]);
            while (true)
            {
                // Check if we should stop parsing.
                if (this.nextToken == this.endToken)
                    break;

                // A directive must start with a string literal token.  Record it now so that the
                // escape sequence and line continuation information is not lost.
                var directiveToken = this.nextToken as StringLiteralToken;
                if (directiveToken == null)
                    break;

                // Directives cannot have escape sequences or line continuations.
                if (directiveToken.EscapeSequenceCount != 0 || directiveToken.LineContinuationCount != 0)
                    break;

                // If the statement starts with a string literal, it must be an expression.
                var beforeInitialToken = this.PositionAfterWhitespace;
                var expression = ParseExpression(PunctuatorToken.Semicolon);

                // The statement must be added to the AST so that eval("'test'") works.
                var initialStatement = new ExpressionStatement(this.labelsForCurrentStatement, expression);
                initialStatement.SourceSpan = new SourceCodeSpan(beforeInitialToken, this.PositionBeforeWhitespace);
                result.Statements.Add(initialStatement);

                // In order for the expression to be part of the directive prologue, it must
                // consist solely of a string literal.
                if ((expression is LiteralExpression) == false)
                    break;

                // Strict mode directive.
                if (directiveToken.Value == "use strict")
                    this.StrictMode = true;

                // Read the end of the statement.  This must happen last so that the lexer has a
                // chance to act on the strict mode flag.
                ExpectEndOfStatement();
            }

            // Call the directive prologue callback.
            if (this.DirectivePrologueProcessedCallback != null)
                this.DirectivePrologueProcessedCallback(this);

            // Read zero or more regular statements.
            while (true)
            {
                // Check if we should stop parsing.
                if (this.nextToken == this.endToken)
                    break;

                // Parse a single statement.
                result.Statements.Add(ParseStatement());
            }

            return result;
        }

        /// <summary>
        /// Parses any statement other than a function declaration.
        /// </summary>
        /// <returns> An expression that represents the statement. </returns>
        private Statement ParseStatement()
        {
            // This is a new statement so clear any labels.
            this.labelsForCurrentStatement.Clear();

            // Parse the statement.
            Statement statement = ParseStatementNoNewContext();

            return statement;
        }

        /// <summary>
        /// Parses any statement other than a function declaration, without beginning a new
        /// statement context.
        /// </summary>
        /// <returns> An expression that represents the statement. </returns>
        private Statement ParseStatementNoNewContext()
        {
            if (this.nextToken == PunctuatorToken.LeftBrace)
                return ParseBlock();
            if (this.nextToken == KeywordToken.Var)
                return ParseVar();
            if (this.nextToken == PunctuatorToken.Semicolon)
                return ParseEmpty();
            if (this.nextToken == KeywordToken.If)
                return ParseIf();
            if (this.nextToken == KeywordToken.Do)
                return ParseDo();
            if (this.nextToken == KeywordToken.While)
                return ParseWhile();
            if (this.nextToken == KeywordToken.For)
                return ParseFor();
            if (this.nextToken == KeywordToken.Continue)
                return ParseContinue();
            if (this.nextToken == KeywordToken.Break)
                return ParseBreak();
            if (this.nextToken == KeywordToken.Return)
                return ParseReturn();
            if (this.nextToken == KeywordToken.With)
                return ParseWith();
            if (this.nextToken == KeywordToken.Switch)
                return ParseSwitch();
            if (this.nextToken == KeywordToken.Throw)
                return ParseThrow();
            if (this.nextToken == KeywordToken.Try)
                return ParseTry();
            if (this.nextToken == KeywordToken.Debugger)
                return ParseDebugger();
            if (this.nextToken == KeywordToken.Function)
                return ParseFunctionDeclaration();
            if (this.nextToken == null)
                throw new JavaScriptException(this.engine, "SyntaxError", "Unexpected end of input", this.LineNumber, this.SourcePath);

            // The statement is either a label or an expression.
            return ParseLabelOrExpressionStatement();
        }

        /// <summary>
        /// Parses a block of statements.
        /// </summary>
        /// <returns> A BlockStatement containing the statements. </returns>
        /// <remarks> The value of a block statement is the value of the last statement in the block,
        /// or undefined if there are no statements in the block. </remarks>
        private BlockStatement ParseBlock()
        {
            // Consume the start brace ({).
            this.Expect(PunctuatorToken.LeftBrace);

            // Read zero or more statements.
            var result = new BlockStatement(this.labelsForCurrentStatement);
            while (true)
            {
                // Check for the end brace (}).
                if (this.nextToken == PunctuatorToken.RightBrace)
                    break;

                // Parse a single statement.
                result.Statements.Add(ParseStatement());
            }

            // Consume the end brace.
            this.Expect(PunctuatorToken.RightBrace);
            return result;
        }

        /// <summary>
        /// Parses a var statement.
        /// </summary>
        /// <returns> A var statement. </returns>
        private VarStatement ParseVar()
        {
            var result = new VarStatement(this.labelsForCurrentStatement, this.currentScope);

            // Read past the var token.
            this.Expect(KeywordToken.Var);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // There can be multiple declarations.
            while (true)
            {
                var declaration = new VariableDeclaration();

                // The next token must be a variable name.
                declaration.VariableName = this.ExpectIdentifier();
                ValidateVariableName(declaration.VariableName);

                // Add the variable to the current function's list of local variables.
                this.currentScope.DeclareVariable(declaration.VariableName,
                    this.context == CodeContext.Function ? null : new LiteralExpression(Undefined.Value),
                    writable: true, deletable: this.context == CodeContext.Eval);

                // The next token is either an equals sign (=), a semi-colon or a comma.
                if (this.nextToken == PunctuatorToken.Assignment)
                {
                    // Read past the equals token (=).
                    this.Expect(PunctuatorToken.Assignment);

                    // Read the setter expression.
                    declaration.InitExpression = ParseExpression(PunctuatorToken.Semicolon, PunctuatorToken.Comma);

                    // Record the portion of the source document that will be highlighted when debugging.
                    declaration.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);
                }

                // Add the declaration to the result.
                result.Declarations.Add(declaration);

                // Check if we are at the end of the statement.
                if (this.AtValidEndOfStatement() == true && this.nextToken != PunctuatorToken.Comma)
                    break;

                // Read past the comma token.
                this.Expect(PunctuatorToken.Comma);

                // Keep track of the start of the statement so that source debugging works correctly.
                start = this.PositionAfterWhitespace;
            }

            // Consume the end of the statement.
            this.ExpectEndOfStatement();

            return result;
        }

        /// <summary>
        /// Parses an empty statement.
        /// </summary>
        /// <returns> An empty statement. </returns>
        private Statement ParseEmpty()
        {
            var result = new EmptyStatement(this.labelsForCurrentStatement);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Read past the semicolon.
            this.Expect(PunctuatorToken.Semicolon);

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            return result;
        }

        /// <summary>
        /// Parses an if statement.
        /// </summary>
        /// <returns> An expression representing the if statement. </returns>
        private IfStatement ParseIf()
        {
            var result = new IfStatement(this.labelsForCurrentStatement);

            // Consume the if keyword.
            this.Expect(KeywordToken.If);

            // Read the left parenthesis.
            this.Expect(PunctuatorToken.LeftParenthesis);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Parse the condition.
            result.Condition = ParseExpression(PunctuatorToken.RightParenthesis);

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            // Read the right parenthesis.
            this.Expect(PunctuatorToken.RightParenthesis);

            // Read the statements that will be executed when the condition is true.
            result.IfClause = ParseStatement();

            // Optionally, read the else statement.
            if (this.nextToken == KeywordToken.Else)
            {
                // Consume the else keyword.
                this.Consume();

                // Read the statements that will be executed when the condition is false.
                result.ElseClause = ParseStatement();
            }

            return result;
        }

        /// <summary>
        /// Parses a do statement.
        /// </summary>
        /// <returns> An expression representing the do statement. </returns>
        private DoWhileStatement ParseDo()
        {
            var result = new DoWhileStatement(this.labelsForCurrentStatement);

            // Consume the do keyword.
            this.Expect(KeywordToken.Do);

            // Read the statements that will be executed in the loop body.
            result.Body = ParseStatement();

            // Read the while keyword.
            this.Expect(KeywordToken.While);

            // Read the left parenthesis.
            this.Expect(PunctuatorToken.LeftParenthesis);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Parse the condition.
            start = this.PositionAfterWhitespace;
            result.ConditionStatement = new ExpressionStatement(ParseExpression(PunctuatorToken.RightParenthesis));
            result.ConditionStatement.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            // Read the right parenthesis.
            this.Expect(PunctuatorToken.RightParenthesis);

            // Consume the end of the statement.
            this.ExpectEndOfStatement();

            return result;
        }

        /// <summary>
        /// Parses a while statement.
        /// </summary>
        /// <returns> A while statement. </returns>
        private WhileStatement ParseWhile()
        {
            var result = new WhileStatement(this.labelsForCurrentStatement);

            // Consume the while keyword.
            this.Expect(KeywordToken.While);

            // Read the left parenthesis.
            this.Expect(PunctuatorToken.LeftParenthesis);

            // Parse the condition.
            var start = this.PositionAfterWhitespace;
            result.ConditionStatement = new ExpressionStatement(ParseExpression(PunctuatorToken.RightParenthesis));
            result.ConditionStatement.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            // Read the right parenthesis.
            this.Expect(PunctuatorToken.RightParenthesis);

            // Read the statements that will be executed in the loop body.
            result.Body = ParseStatement();

            return result;
        }

        /// <summary>
        /// Parses a for statement or a for-in statement.
        /// </summary>
        /// <returns> A for statement or a for-in statement. </returns>
        private Statement ParseFor()
        {
            // Consume the for keyword.
            this.Expect(KeywordToken.For);

            // Read the left parenthesis.
            this.Expect(PunctuatorToken.LeftParenthesis);

            // The initialization statement.
            Statement initializationStatement = null;

            // The for-in expression needs a variable to assign to.  Is null for a regular for statement.
            IReferenceExpression forInReference = null;

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            if (this.nextToken == KeywordToken.Var)
            {
                // Read past the var token.
                this.Expect(KeywordToken.Var);

                // There can be multiple initializers (but not for for-in statements).
                var varStatement = new VarStatement(this.labelsForCurrentStatement, this.currentScope);
                initializationStatement = varStatement;

                // Only a simple variable name is allowed for for-in statements.
                bool cannotBeForIn = false;

                while (true)
                {
                    var declaration = new VariableDeclaration();

                    // The next token must be a variable name.
                    declaration.VariableName = this.ExpectIdentifier();
                    ValidateVariableName(declaration.VariableName);

                    // Add the variable to the current function's list of local variables.
                    this.currentScope.DeclareVariable(declaration.VariableName,
                        this.context == CodeContext.Function ? null : new LiteralExpression(Undefined.Value),
                        writable: true, deletable: this.context == CodeContext.Eval);

                    // The next token is either an equals sign (=), a semi-colon, a comma, or the "in" keyword.
                    if (this.nextToken == PunctuatorToken.Assignment)
                    {
                        // Read past the equals token (=).
                        this.Expect(PunctuatorToken.Assignment);

                        // Read the setter expression.
                        declaration.InitExpression = ParseExpression(PunctuatorToken.Semicolon, PunctuatorToken.Comma);

                        // Record the portion of the source document that will be highlighted when debugging.
                        declaration.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

                        // This is a regular for statement.
                        cannotBeForIn = true;
                    }

                    // Add the declaration to the initialization statement.
                    varStatement.Declarations.Add(declaration);

                    if (this.nextToken == PunctuatorToken.Semicolon)
                    {
                        // This is a regular for statement.
                        break;
                    }
                    else if (this.nextToken == KeywordToken.In && cannotBeForIn == false)
                    {
                        // This is a for-in statement.
                        forInReference = new NameExpression(this.currentScope, declaration.VariableName);
                        break;
                    }
                    else if (this.nextToken != PunctuatorToken.Comma)
                        throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Unexpected token {0}", Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);

                    // Read past the comma token.
                    this.Expect(PunctuatorToken.Comma);

                    // Keep track of the start of the statement so that source debugging works correctly.
                    start = this.PositionAfterWhitespace;

                    // Multiple initializers are not allowed in for-in statements.
                    cannotBeForIn = true;
                }

                // Record the portion of the source document that will be highlighted when debugging.
                varStatement.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);
            }
            else
            {
                // Not a var initializer - can be a simple variable name then "in" or any expression ending with a semi-colon.
                // The expression can be empty.
                if (this.nextToken != PunctuatorToken.Semicolon)
                {
                    // Parse an expression.
                    var initializationExpression = ParseExpression(PunctuatorToken.Semicolon, KeywordToken.In);

                    // Record debug info for the expression.
                    initializationStatement = new ExpressionStatement(initializationExpression);
                    initializationStatement.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

                    if (this.nextToken == KeywordToken.In)
                    {
                        // This is a for-in statement.
                        if ((initializationExpression is IReferenceExpression) == false)
                            throw new JavaScriptException(this.engine, "ReferenceError", "Invalid left-hand side in for-in", this.LineNumber, this.SourcePath);
                        forInReference = (IReferenceExpression)initializationExpression;
                    }
                }
            }

            if (forInReference != null)
            {
                // for (x in y)
                // for (var x in y)
                var result = new ForInStatement(this.labelsForCurrentStatement);
                result.Variable = forInReference;
                result.VariableSourceSpan = initializationStatement.SourceSpan;
                
                // Consume the "in".
                this.Expect(KeywordToken.In);

                // Parse the right-hand-side expression.
                start = this.PositionAfterWhitespace;
                result.TargetObject = ParseExpression(PunctuatorToken.RightParenthesis);
                result.TargetObjectSourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

                // Read the right parenthesis.
                this.Expect(PunctuatorToken.RightParenthesis);

                // Read the statements that will be executed in the loop body.
                result.Body = ParseStatement();

                return result;
            }
            else
            {
                var result = new ForStatement(this.labelsForCurrentStatement);

                // Set the initialization statement.
                if (initializationStatement != null)
                    result.InitStatement = initializationStatement;

                // Read the semicolon.
                this.Expect(PunctuatorToken.Semicolon);

                // Parse the optional condition expression.
                // Note: if the condition is omitted then it is considered to always be true.
                if (this.nextToken != PunctuatorToken.Semicolon)
                {
                    start = this.PositionAfterWhitespace;
                    result.ConditionStatement = new ExpressionStatement(ParseExpression(PunctuatorToken.Semicolon));
                    result.ConditionStatement.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);
                }

                // Read the semicolon.
                // Note: automatic semicolon insertion never inserts a semicolon in the header of a
                // for statement.
                this.Expect(PunctuatorToken.Semicolon);

                // Parse the optional increment expression.
                if (this.nextToken != PunctuatorToken.RightParenthesis)
                {
                    start = this.PositionAfterWhitespace;
                    result.IncrementStatement = new ExpressionStatement(ParseExpression(PunctuatorToken.RightParenthesis));
                    result.IncrementStatement.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);
                }

                // Read the right parenthesis.
                this.Expect(PunctuatorToken.RightParenthesis);

                // Read the statements that will be executed in the loop body.
                result.Body = ParseStatement();

                return result;
            }
        }

        /// <summary>
        /// Parses a continue statement.
        /// </summary>
        /// <returns> A continue statement. </returns>
        private ContinueStatement ParseContinue()
        {
            var result = new ContinueStatement(this.labelsForCurrentStatement);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Consume the continue keyword.
            this.Expect(KeywordToken.Continue);

            // The continue statement can have an optional label to jump to.
            if (this.AtValidEndOfStatement() == false)
            {
                // continue [label]

                // Read the label name.
                result.Label = this.ExpectIdentifier();
            }

            // Consume the semi-colon, if there was one.
            this.ExpectEndOfStatement();

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            return result;
        }

        /// <summary>
        /// Parses a break statement.
        /// </summary>
        /// <returns> A break statement. </returns>
        private BreakStatement ParseBreak()
        {
            var result = new BreakStatement(this.labelsForCurrentStatement);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Consume the break keyword.
            this.Expect(KeywordToken.Break);

            // The break statement can have an optional label to jump to.
            if (this.AtValidEndOfStatement() == false)
            {
                // break [label]

                // Read the label name.
                result.Label = this.ExpectIdentifier();
            }

            // Consume the semi-colon, if there was one.
            this.ExpectEndOfStatement();

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            return result;
        }

        /// <summary>
        /// Parses a return statement.
        /// </summary>
        /// <returns> A return statement. </returns>
        private ReturnStatement ParseReturn()
        {
            if (this.context != CodeContext.Function)
                throw new JavaScriptException(this.engine, "SyntaxError", "Return statements are only allowed inside functions", this.LineNumber, this.SourcePath);

            var result = new ReturnStatement(this.labelsForCurrentStatement);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Consume the return keyword.
            this.Expect(KeywordToken.Return);

            if (this.AtValidEndOfStatement() == false)
            {
                // Parse the return value expression.
                result.Value = ParseExpression(PunctuatorToken.Semicolon);
            }

            // Consume the end of the statement.
            this.ExpectEndOfStatement();

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            return result;
        }

        /// <summary>
        /// Parses a with statement.
        /// </summary>
        /// <returns> An expression representing the with statement. </returns>
        private WithStatement ParseWith()
        {
            // This statement is not allowed in strict mode.
            if (this.StrictMode == true)
                throw new JavaScriptException(this.engine, "SyntaxError", "The with statement is not supported in strict mode", this.LineNumber, this.SourcePath);

            var result = new WithStatement(this.labelsForCurrentStatement);

            // Read past the "with" token.
            this.Expect(KeywordToken.With);

            // Read a left parenthesis token "(".
            this.Expect(PunctuatorToken.LeftParenthesis);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Read an object reference.
            var objectEnvironment = ParseExpression(PunctuatorToken.RightParenthesis);

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            // Read a right parenthesis token ")".
            this.Expect(PunctuatorToken.RightParenthesis);

            // Create a new scope and assign variables within the with statement to the scope.
            result.Scope = ObjectScope.CreateWithScope(this.currentScope, objectEnvironment);
            this.currentScope = result.Scope;

            // Read the body of the with statement.
            result.Body = ParseStatement();

            // Revert the scope.
            this.currentScope = this.currentScope.ParentScope;

            return result;
        }

        /// <summary>
        /// Parses a switch statement.
        /// </summary>
        /// <returns> A switch statement. </returns>
        private SwitchStatement ParseSwitch()
        {
            var result = new SwitchStatement(this.labelsForCurrentStatement);

            // Consume the switch keyword.
            this.Expect(KeywordToken.Switch);

            // Read the left parenthesis.
            this.Expect(PunctuatorToken.LeftParenthesis);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Parse the switch expression.
            result.Value = ParseExpression(PunctuatorToken.RightParenthesis);

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            // Read the right parenthesis.
            this.Expect(PunctuatorToken.RightParenthesis);

            // Consume the start brace ({).
            this.Expect(PunctuatorToken.LeftBrace);

            SwitchCase defaultClause = null;
            while (true)
            {
                if (this.nextToken == KeywordToken.Case)
                {
                    var caseClause = new SwitchCase();

                    // Read the case keyword.
                    this.Expect(KeywordToken.Case);

                    // Parse the case expression.
                    caseClause.Value = ParseExpression(PunctuatorToken.Colon);

                    // Consume the colon.
                    this.Expect(PunctuatorToken.Colon);

                    // Zero or more statements can be added to the case statement.
                    while (this.nextToken != KeywordToken.Case && this.nextToken != KeywordToken.Default && this.nextToken != PunctuatorToken.RightBrace)
                        caseClause.BodyStatements.Add(ParseStatement());

                    // Add the case clause to the switch statement.
                    result.CaseClauses.Add(caseClause);
                }
                else if (this.nextToken == KeywordToken.Default)
                {
                    // Make sure this is the only default clause.
                    if (defaultClause != null)
                        throw new JavaScriptException(this.engine, "SyntaxError", "Only one default clause is allowed.", this.LineNumber, this.SourcePath);

                    defaultClause = new SwitchCase();

                    // Read the case keyword.
                    this.Expect(KeywordToken.Default);

                    // Consume the colon.
                    this.Expect(PunctuatorToken.Colon);

                    // Zero or more statements can be added to the case statement.
                    while (this.nextToken != KeywordToken.Case && this.nextToken != KeywordToken.Default && this.nextToken != PunctuatorToken.RightBrace)
                        defaultClause.BodyStatements.Add(ParseStatement());

                    // Add the default clause to the switch statement.
                    result.CaseClauses.Add(defaultClause);
                }
                else if (this.nextToken == PunctuatorToken.RightBrace)
                {
                    break;
                }
                else
                {
                    // Statements cannot be added directly after the switch.
                    throw new JavaScriptException(this.engine, "SyntaxError", "Expected 'case' or 'default'.", this.LineNumber, this.SourcePath);
                }
            }

            // Consume the end brace.
            this.Consume();

            return result;
        }

        /// <summary>
        /// Parses a throw statement.
        /// </summary>
        /// <returns> A throw statement. </returns>
        private ThrowStatement ParseThrow()
        {
            var result = new ThrowStatement(this.labelsForCurrentStatement);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Consume the throw keyword.
            this.Expect(KeywordToken.Throw);

            // A line terminator is not allowed here.
            if (this.consumedLineTerminator == true)
                throw new JavaScriptException(this.engine, "SyntaxError", "Illegal newline after throw", this.LineNumber, this.SourcePath);

            // Parse the expression to throw.
            result.Value = ParseExpression(PunctuatorToken.Semicolon);

            // Consume the end of the statement.
            this.ExpectEndOfStatement();

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            return result;
        }

        /// <summary>
        /// Parses a try statement.
        /// </summary>
        /// <returns> A try-catch-finally statement. </returns>
        private TryCatchFinallyStatement ParseTry()
        {
            var result = new TryCatchFinallyStatement(this.labelsForCurrentStatement);

            // Consume the try keyword.
            this.Expect(KeywordToken.Try);

            // Parse the try block.
            result.TryBlock = ParseBlock();

            // The next token is either 'catch' or 'finally'.
            if (this.nextToken == KeywordToken.Catch)
            {
                // Consume the catch token.
                this.Expect(KeywordToken.Catch);

                // Read the left parenthesis.
                this.Expect(PunctuatorToken.LeftParenthesis);

                // Read the name of the variable to assign the exception to.
                result.CatchVariableName = this.ExpectIdentifier();
                this.ValidateVariableName(result.CatchVariableName);

                // Read the right parenthesis.
                this.Expect(PunctuatorToken.RightParenthesis);

                // Create a new scope for the catch variable.
                this.currentScope = result.CatchScope = DeclarativeScope.CreateCatchScope(this.currentScope, result.CatchVariableName);

                // Parse the statements inside the catch block.
                result.CatchBlock = ParseBlock();

                // Revert the scope.
                this.currentScope = this.currentScope.ParentScope;
            }

            if (this.nextToken == KeywordToken.Finally)
            {
                // Consume the finally token.
                this.Expect(KeywordToken.Finally);

                // Read the finally statements.
                result.FinallyBlock = ParseBlock();
            }

            // There must be a catch or finally block.
            if (result.CatchBlock == null && result.FinallyBlock == null)
                throw new JavaScriptException(this.engine, "SyntaxError", "Missing catch or finally after try", this.LineNumber, this.SourcePath);

            return result;
        }

        /// <summary>
        /// Parses a debugger statement.
        /// </summary>
        /// <returns> A debugger statement. </returns>
        private DebuggerStatement ParseDebugger()
        {
            var result = new DebuggerStatement(this.labelsForCurrentStatement);

            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Consume the debugger keyword.
            this.Expect(KeywordToken.Debugger);

            // Consume the end of the statement.
            this.ExpectEndOfStatement();

            // Record the portion of the source document that will be highlighted when debugging.
            result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

            return result;
        }

        /// <summary>
        /// Parses a function declaration.
        /// </summary>
        /// <returns> A statement representing the function. </returns>
        private Statement ParseFunctionDeclaration()
        {
            // Parse the function declaration.
            var expression = ParseFunction(FunctionType.Declaration, this.initialScope);

            // Add the function to the top-level scope.
            this.initialScope.DeclareVariable(expression.FunctionName, expression, writable: true, deletable: this.context == CodeContext.Eval);

            // Function declarations do nothing at the point of declaration - everything happens
            // at the top of the function/global code.
            return new EmptyStatement(this.labelsForCurrentStatement);
        }

        private enum FunctionType
        {
            Declaration,
            Expression,
            Getter,
            Setter,
        }

        /// <summary>
        /// Parses a function declaration or a function expression.
        /// </summary>
        /// <param name="functionType"> The type of function to parse. </param>
        /// <param name="parentScope"> The parent scope for the function. </param>
        /// <returns> A function expression. </returns>
        private FunctionExpression ParseFunction(FunctionType functionType, Scope parentScope)
        {
            if (functionType != FunctionType.Getter && functionType != FunctionType.Setter)
            {
                // Consume the function keyword.
                this.Expect(KeywordToken.Function);
            }

            // Read the function name.
            var functionName = string.Empty;
            if (functionType == FunctionType.Declaration)
            {
                functionName = this.ExpectIdentifier();
            }
            else if (functionType == FunctionType.Expression)
            {
                // The function name is optional for function expressions.
                if (this.nextToken is IdentifierToken)
                    functionName = this.ExpectIdentifier();
            }
            else if (functionType == FunctionType.Getter || functionType == FunctionType.Setter)
            {
                // Getters and setters can have any name that is allowed of a property.
                bool wasIdentifier;
                functionName = ReadPropertyName(out wasIdentifier);
            }
            else
                throw new ArgumentOutOfRangeException("functionType");
            ValidateVariableName(functionName);

            // Read the left parenthesis.
            this.Expect(PunctuatorToken.LeftParenthesis);

            // Read zero or more argument names.
            var argumentNames = new List<string>();

            // Read the first argument name.
            if (this.nextToken != PunctuatorToken.RightParenthesis)
            {
                var argumentName = this.ExpectIdentifier();
                ValidateVariableName(argumentName);
                argumentNames.Add(argumentName);
            }

            while (true)
            {
                if (this.nextToken == PunctuatorToken.Comma)
                {
                    // Consume the comma.
                    this.Consume();

                    // Read and validate the argument name.
                    var argumentName = this.ExpectIdentifier();
                    ValidateVariableName(argumentName);
                    argumentNames.Add(argumentName);
                }
                else if (this.nextToken == PunctuatorToken.RightParenthesis)
                    break;
                else
                    throw new JavaScriptException(this.engine, "SyntaxError", "Expected ',' or ')'", this.LineNumber, this.SourcePath);
            }

            // Getters must have zero arguments.
            if (functionType == FunctionType.Getter && argumentNames.Count != 0)
                throw new JavaScriptException(this.engine, "SyntaxError", "Getters cannot have arguments", this.LineNumber, this.SourcePath);

            // Setters must have one argument.
            if (functionType == FunctionType.Setter && argumentNames.Count != 1)
                throw new JavaScriptException(this.engine, "SyntaxError", "Setters must have a single argument", this.LineNumber, this.SourcePath);

            // Read the right parenthesis.
            this.Expect(PunctuatorToken.RightParenthesis);

            // Record the start of the function body.
            var startPosition = this.PositionBeforeWhitespace;

            // Since the parser reads one token in advance, start capturing the function body here.
            var bodyTextBuilder = new System.Text.StringBuilder();
            var originalBodyTextBuilder = this.lexer.InputCaptureStringBuilder;
            this.lexer.InputCaptureStringBuilder = bodyTextBuilder;

            // Read the start brace.
            this.Expect(PunctuatorToken.LeftBrace);

            // This context has a nested function.
            this.methodOptimizationHints.HasNestedFunction = true;

            // Create a new scope and assign variables within the function body to the scope.
            bool includeNameInScope = functionType != FunctionType.Getter && functionType != FunctionType.Setter;
            var scope = DeclarativeScope.CreateFunctionScope(parentScope, includeNameInScope ? functionName : string.Empty, argumentNames);

            // Read the function body.
            var functionParser = Parser.CreateFunctionBodyParser(this, scope);
            var body = functionParser.Parse();

            // Transfer state back from the function parser.
            this.nextToken = functionParser.nextToken;
            this.lexer.StrictMode = this.StrictMode;
            this.lexer.InputCaptureStringBuilder = originalBodyTextBuilder;
            if (originalBodyTextBuilder != null)
                originalBodyTextBuilder.Append(bodyTextBuilder);

            SourceCodePosition endPosition;
            if (functionType == FunctionType.Expression)
            {
                // The end token '}' will be consumed by the parent function.
                if (this.nextToken != PunctuatorToken.RightBrace)
                    throw new JavaScriptException(this.engine, "SyntaxError", "Expected '}'", this.LineNumber, this.SourcePath);

                // Record the end of the function body.
                endPosition = new SourceCodePosition(this.PositionAfterWhitespace.Line, this.PositionAfterWhitespace.Column + 1);
            }
            else
            {
                // Consume the '}'.
                this.Expect(PunctuatorToken.RightBrace);

                // Record the end of the function body.
                endPosition = new SourceCodePosition(this.PositionAfterWhitespace.Line, this.PositionAfterWhitespace.Column + 1);
            }

            // Create a new function expression.
            var options = this.options.Clone();
            options.ForceStrictMode = functionParser.StrictMode;
            var context = new FunctionMethodGenerator(this.engine, scope, functionName,
                includeNameInScope, argumentNames,
                bodyTextBuilder.ToString(0, bodyTextBuilder.Length - 1), body,
                this.SourcePath, options);
            context.MethodOptimizationHints = functionParser.methodOptimizationHints;
            return new FunctionExpression(context);
        }

        /// <summary>
        /// Parses a statement consisting of an expression or starting with a label.  These two
        /// cases are disambiguated here.
        /// </summary>
        /// <returns> A statement. </returns>
        private Statement ParseLabelOrExpressionStatement()
        {
            // Keep track of the start of the statement so that source debugging works correctly.
            var start = this.PositionAfterWhitespace;

            // Parse the statement as though it was an expression - but stop if there is an unexpected colon.
            var expression = ParseExpression(PunctuatorToken.Semicolon, PunctuatorToken.Colon);

            if (this.nextToken == PunctuatorToken.Colon && expression is NameExpression)
            {
                // The expression is actually a label.

                // Extract the label name.
                var labelName = ((NameExpression)expression).Name;
                this.labelsForCurrentStatement.Add(labelName);

                // Read past the colon.
                this.Expect(PunctuatorToken.Colon);

                // Read the rest of the statement.
                return ParseStatementNoNewContext();
            }
            else
            {

                // Consume the end of the statement.
                this.ExpectEndOfStatement();

                // Create a new expression statement.
                var result = new ExpressionStatement(this.labelsForCurrentStatement, expression);

                // Record the portion of the source document that will be highlighted when debugging.
                result.SourceSpan = new SourceCodeSpan(start, this.PositionBeforeWhitespace);

                return result;
            }
        }



        //     EXPRESSION PARSER
        //_________________________________________________________________________________________

        /// <summary>
        /// Represents a key by which to look up an operator.
        /// </summary>
        private struct OperatorKey
        {
            public Token Token;
            public bool PostfixOrInfix;

            public override int GetHashCode()
            {
                return Token.GetHashCode() ^ PostfixOrInfix.GetHashCode();
            }
        }

        /// <summary>
        /// Gets or sets a mapping from token -> operator.  There can be at most two operators per
        /// token (the prefix version and the infix/postfix version).
        /// </summary>
        private static Dictionary<OperatorKey, Operator> operatorLookup;

        /// <summary>
        /// Initializes the token -> operator mapping dictionary.
        /// </summary>
        /// <returns> The token -> operator mapping dictionary. </returns>
        private static Dictionary<OperatorKey, Operator> InitializeOperatorLookup()
        {
            var result = new Dictionary<OperatorKey, Operator>(55);
            foreach (var @operator in Operator.AllOperators)
            {
                result.Add(new OperatorKey() { Token = @operator.Token, PostfixOrInfix = @operator.HasLHSOperand }, @operator);
                if (@operator.SecondaryToken != null)
                {
                    // Note: the secondary token for the grouping operator and function call operator ')' is a duplicate.
                    result[new OperatorKey() { Token = @operator.SecondaryToken, PostfixOrInfix = @operator.HasRHSOperand }] = @operator;
                    if (@operator.InnerOperandIsOptional == true)
                        result[new OperatorKey() { Token = @operator.SecondaryToken, PostfixOrInfix = false }] = @operator;
                }
            }
            return result;
        }

        /// <summary>
        /// Finds a operator given a token and an indication whether the prefix or infix/postfix
        /// version is desired.
        /// </summary>
        /// <param name="token"> The token to search for. </param>
        /// <param name="postfixOrInfix"> <c>true</c> if the infix/postfix version of the operator
        /// is desired; <c>false</c> otherwise. </param>
        /// <returns> An Operator instance, or <c>null</c> if the operator could not be found. </returns>
        private static Operator OperatorFromToken(Token token, bool postfixOrInfix)
        {
            Operator result;
            if (operatorLookup == null)
            {
                // Initialize the operator lookup table.
                var temp = InitializeOperatorLookup();
                System.Threading.Thread.MemoryBarrier();
                operatorLookup = temp;
            }
            if (operatorLookup.TryGetValue(new OperatorKey() { Token = token, PostfixOrInfix = postfixOrInfix }, out result) == false)
                return null;
            return result;
        }

        /// <summary>
        /// Parses a javascript expression.
        /// </summary>
        /// <param name="endToken"> A token that indicates the end of the expression. </param>
        /// <returns> An expression tree that represents the expression. </returns>
        private Expression ParseExpression(params Token[] endTokens)
        {
            // The root of the expression tree.
            Expression root = null;

            // The active operator, i.e. the one last encountered.
            OperatorExpression unboundOperator = null;

            while (this.nextToken != null)
            {
                if (this.nextToken is LiteralToken ||
                    this.nextToken is IdentifierToken ||
                    this.nextToken == KeywordToken.Function ||
                    this.nextToken == KeywordToken.This ||
                    this.nextToken == PunctuatorToken.LeftBrace ||
                    (this.nextToken == PunctuatorToken.LeftBracket && this.expressionState == ParserExpressionState.Literal) ||
                    (this.nextToken is KeywordToken && unboundOperator != null && unboundOperator.OperatorType == OperatorType.MemberAccess && this.expressionState == ParserExpressionState.Literal))
                {
                    // If a literal was found where an operator was expected, insert a semi-colon
                    // automatically (if this would fix the error and a line terminator was
                    // encountered) or throw an error.
                    if (this.expressionState != ParserExpressionState.Literal)
                    {
                        // Check for automatic semi-colon insertion.
                        if (Array.IndexOf(endTokens, PunctuatorToken.Semicolon) >= 0 && this.consumedLineTerminator == true)
                            break;
                        throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Expected operator but found {0}", Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);
                    }

                    // New in ECMAScript 5 is the ability to use keywords as property names.
                    if ((this.nextToken is KeywordToken || (this.nextToken is LiteralToken && ((LiteralToken)this.nextToken).IsKeyword == true)) &&
                        unboundOperator != null &&
                        unboundOperator.OperatorType == OperatorType.MemberAccess &&
                        this.expressionState == ParserExpressionState.Literal)
                    {
                        this.nextToken = new IdentifierToken(this.nextToken.Text);
                    }

                    Expression terminal;
                    if (this.nextToken is LiteralToken)
                        // If the token is a literal, convert it to a literal expression.
                        terminal = new LiteralExpression(((LiteralToken)this.nextToken).Value);
                    else if (this.nextToken is IdentifierToken)
                    {
                        // If the token is an identifier, convert it to a NameExpression.
                        var identifierName = ((IdentifierToken)this.nextToken).Name;
                        terminal = new NameExpression(this.currentScope, identifierName);

                        // Record each occurance of a variable name.
                        if (unboundOperator == null || unboundOperator.OperatorType != OperatorType.MemberAccess)
                            this.methodOptimizationHints.EncounteredVariable(identifierName);
                    }
                    else if (this.nextToken == KeywordToken.This)
                    {
                        // Convert "this" to an expression.
                        terminal = new ThisExpression();

                        // Add method optimization info.
                        this.methodOptimizationHints.HasThis = true;
                    }
                    else if (this.nextToken == PunctuatorToken.LeftBracket)
                        // Array literal.
                        terminal = ParseArrayLiteral();
                    else if (this.nextToken == PunctuatorToken.LeftBrace)
                        // Object literal.
                        terminal = ParseObjectLiteral();
                    else if (this.nextToken == KeywordToken.Function)
                        terminal = ParseFunctionExpression();
                    else
                        throw new InvalidOperationException("Unsupported literal type.");

                    // Push the literal to the most recent unbound operator, or, if there is none, to
                    // the root of the tree.
                    if (root == null)
                    {
                        // This is the first term in an expression.
                        root = terminal;
                    }
                    else
                    {
                        Debug.Assert(unboundOperator != null && unboundOperator.AcceptingOperands == true);
                        unboundOperator.Push(terminal);
                    }
                }
                else if (this.nextToken is PunctuatorToken || this.nextToken is KeywordToken)
                {
                    // The token is an operator (o1).
                    Operator newOperator = OperatorFromToken(this.nextToken, postfixOrInfix: this.expressionState == ParserExpressionState.Operator);

                    // Make sure the token is actually an operator and not just a random keyword.
                    if (newOperator == null)
                    {
                        // Check if the token is an end token, for example a semi-colon.
                        if (Array.IndexOf(endTokens, this.nextToken) >= 0)
                            break;
                        // Check for automatic semi-colon insertion.
                        if (Array.IndexOf(endTokens, PunctuatorToken.Semicolon) >= 0 && (this.consumedLineTerminator == true || this.nextToken == PunctuatorToken.RightBrace))
                            break;
                        throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Unexpected token {0} in expression.", Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);
                    }

                    // Post-fix increment and decrement cannot have a line terminator in between
                    // the operator and the operand.
                    if (this.consumedLineTerminator == true && (newOperator == Operator.PostIncrement || newOperator == Operator.PostDecrement))
                        break;

                    // There are four possibilities:
                    // 1. The token is the second of a two-part operator (for example, the ':' in a
                    //    conditional operator.  In this case, we need to go up the tree until we find
                    //    an instance of the operator and make that the active unbound operator.
                    if (this.nextToken == newOperator.SecondaryToken)
                    {
                        // Traverse down the tree looking for the parent operator that corresponds to
                        // this token.
                        OperatorExpression parentExpression = null;
                        var node = root as OperatorExpression;
                        while (node != null)
                        {
                            if (node.Operator.Token == newOperator.Token && node.SecondTokenEncountered == false)
                                parentExpression = node;
                            if (node == unboundOperator)
                                break;
                            node = node.RightBranch;
                        }

                        // If the operator was not found, then this is a mismatched token, unless
                        // it is the end token.  For example, if an unbalanced right parenthesis is
                        // found in an if statement then it is merely the end of the test expression.
                        if (parentExpression == null)
                        {
                            // Check if the token is an end token, for example a right parenthesis.
                            if (Array.IndexOf(endTokens, this.nextToken) >= 0)
                                break;
                            // Check for automatic semi-colon insertion.
                            if (Array.IndexOf(endTokens, PunctuatorToken.Semicolon) >= 0 && this.consumedLineTerminator == true)
                                break;
                            throw new JavaScriptException(this.engine, "SyntaxError", "Mismatched closing token in expression.", this.LineNumber, this.SourcePath);
                        }

                        // Mark that we have seen the closing token.
                        unboundOperator = parentExpression;
                        unboundOperator.SecondTokenEncountered = true;
                    }
                    else
                    {
                        // Check if the token is an end token, for example the comma in a variable
                        // declaration.
                        if (Array.IndexOf(endTokens, this.nextToken) >= 0)
                        {
                            // But make sure the token isn't inside an operator.
                            // For example, in the expression "var x = f(a, b)" the comma does not
                            // indicate the start of a new variable clause because it is inside the
                            // function call operator.
                            bool insideOperator = false;
                            var node = root as OperatorExpression;
                            while (node != null)
                            {
                                if (node.Operator.SecondaryToken != null && node.SecondTokenEncountered == false)
                                    insideOperator = true;
                                if (node == unboundOperator)
                                    break;
                                node = node.RightBranch;
                            }
                            if (insideOperator == false)
                                break;
                        }

                        // All the other situations involve the creation of a new operator.
                        var newExpression = OperatorExpression.FromOperator(newOperator);

                        // 2. The new operator is a prefix operator.  The new operator becomes an operand
                        //    of the previous operator.
                        if (newOperator.HasLHSOperand == false)
                        {
                            if (root == null)
                                // "!"
                                root = newExpression;
                            else if (unboundOperator != null && unboundOperator.AcceptingOperands == true)
                            {
                                // "5 + !"
                                unboundOperator.Push(newExpression);
                            }
                            else
                            {
                                // "5 !" or "5 + 5 !"
                                // Check for automatic semi-colon insertion.
                                if (Array.IndexOf(endTokens, PunctuatorToken.Semicolon) >= 0 && this.consumedLineTerminator == true)
                                    break;
                                throw new JavaScriptException(this.engine, "SyntaxError", "Invalid use of prefix operator.", this.LineNumber, this.SourcePath);
                            }
                        }
                        else
                        {
                            // Search up the tree for an operator that has a lower precedence.
                            // Because we don't store the parent link, we have to traverse down the
                            // tree and take the last one we find instead.
                            OperatorExpression lowPrecedenceOperator = null;
                            if (unboundOperator == null ||
                                (newOperator.Associativity == OperatorAssociativity.LeftToRight && unboundOperator.Precedence < newOperator.Precedence) ||
                                (newOperator.Associativity == OperatorAssociativity.RightToLeft && unboundOperator.Precedence <= newOperator.Precedence))
                            {
                                // Performance optimization: look at the previous operator first.
                                lowPrecedenceOperator = unboundOperator;
                            }
                            else
                            {
                                // Search for a lower precedence operator by traversing the tree.
                                var node = root as OperatorExpression;
                                while (node != null && node != unboundOperator)
                                {
                                    if ((newOperator.Associativity == OperatorAssociativity.LeftToRight && node.Precedence < newOperator.Precedence) ||
                                        (newOperator.Associativity == OperatorAssociativity.RightToLeft && node.Precedence <= newOperator.Precedence))
                                        lowPrecedenceOperator = node;
                                    node = node.RightBranch;
                                }
                            }

                            if (lowPrecedenceOperator == null)
                            {
                                // 3. The new operator has a lower precedence (or if the associativity is left to
                                //    right, a lower or equal precedence) than all the parent operators.  The new
                                //    operator goes to the root of the tree and the previous operator becomes the
                                //    first operand for the new operator.
                                if (root != null)
                                    newExpression.Push(root);
                                root = newExpression;
                            }
                            else
                            {
                                // 4. Otherwise, the new operator can steal the last operand from the previous
                                //    operator and then put itself in the place of that last operand.
                                if (lowPrecedenceOperator.OperandCount == 0)
                                {
                                    // "! ++"
                                    // Check for automatic semi-colon insertion.
                                    if (Array.IndexOf(endTokens, PunctuatorToken.Semicolon) >= 0 && this.consumedLineTerminator == true)
                                        break;
                                    throw new JavaScriptException(this.engine, "SyntaxError", "Invalid use of prefix operator.", this.LineNumber, this.SourcePath);
                                }
                                newExpression.Push(lowPrecedenceOperator.Pop());
                                lowPrecedenceOperator.Push(newExpression);
                            }
                        }

                        unboundOperator = newExpression;
                    }
                }
                else
                {
                    throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Unexpected token {0} in expression", Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);
                }

                // Read the next token.
                this.Consume(root != null && (unboundOperator == null || unboundOperator.AcceptingOperands == false) ? ParserExpressionState.Operator : ParserExpressionState.Literal);
            }

            // Empty expressions are invalid.
            if (root == null)
                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Expected an expression but found {0} instead", Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);

            // Check the AST is valid.
            CheckASTValidity(root);

            // A literal is the next valid expression token.
            this.expressionState = ParserExpressionState.Literal;
            this.lexer.ParserExpressionState = expressionState;

            // Resolve all the unbound operators into real operators.
            return root;
        }

        /// <summary>
        /// Checks the given AST is valid.
        /// </summary>
        /// <param name="root"> The root of the AST. </param>
        private void CheckASTValidity(Expression root)
        {
            // Push the root expression onto a stack.
            Stack<Expression> stack = new Stack<Expression>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                // Pop the next expression from the stack.
                var expression = stack.Pop() as OperatorExpression;
                
                // Only operator expressions are checked for validity.
                if (expression == null)
                    continue;

                // Check the operator expression has the right number of operands.
                if (expression.Operator.IsValidNumberOfOperands(expression.OperandCount) == false)
                    throw new JavaScriptException(this.engine, "SyntaxError", "Wrong number of operands", this.LineNumber, this.SourcePath);

                // Check the operator expression is closed.
                if (expression.Operator.SecondaryToken != null && expression.SecondTokenEncountered == false)
                    throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Missing closing token '{0}'", expression.Operator.SecondaryToken.Text), this.LineNumber, this.SourcePath);

                // Check the child nodes.
                for (int i = 0; i < expression.OperandCount; i++)
                    stack.Push(expression.GetRawOperand(i));
            }
        }

        /// <summary>
        /// Parses an array literal (e.g. "[1, 2]").
        /// </summary>
        /// <returns> A literal expression that represents the array literal. </returns>
        private LiteralExpression ParseArrayLiteral()
        {
            // Read past the initial '[' token.
            Debug.Assert(this.nextToken == PunctuatorToken.LeftBracket);
            this.Consume();

            var items = new List<Expression>();
            while (true)
            {
                // If the next token is ']', then the array literal is complete.
                if (this.nextToken == PunctuatorToken.RightBracket)
                    break;

                // If the next token is ',', then the array element is undefined.
                if (this.nextToken == PunctuatorToken.Comma)
                    items.Add(null);
                else
                    // Otherwise, read the next item in the array.
                    items.Add(ParseExpression(PunctuatorToken.Comma, PunctuatorToken.RightBracket));

                // Read past the comma.
                Debug.Assert(this.nextToken == PunctuatorToken.Comma || this.nextToken == PunctuatorToken.RightBracket);
                if (this.nextToken == PunctuatorToken.Comma)
                    this.Consume();
            }

            // The end token ']' will be consumed by the parent function.
            Debug.Assert(this.nextToken == PunctuatorToken.RightBracket);

            return new LiteralExpression(items);
        }

        /// <summary>
        /// Used to store the getter and setter for an object literal property.
        /// </summary>
        internal class ObjectLiteralAccessor
        {
            public FunctionExpression Getter;
            public FunctionExpression Setter;
        }

        /// <summary>
        /// Parses an object literal (e.g. "{a: 5}").
        /// </summary>
        /// <returns> A literal expression that represents the object literal. </returns>
        private LiteralExpression ParseObjectLiteral()
        {
            // Read past the initial '{' token.
            Debug.Assert(this.nextToken == PunctuatorToken.LeftBrace);
            this.Consume();

            var properties = new Dictionary<string, object>();
            while (true)
            {
                // If the next token is '}', then the object literal is complete.
                if (this.nextToken == PunctuatorToken.RightBrace)
                    break;

                // Read the next property name.
                bool mightBeGetOrSet;
                string propertyName = ReadPropertyName(out mightBeGetOrSet);

                // Check if this is a getter or setter.
                Expression propertyValue;
                if (this.nextToken != PunctuatorToken.Colon && mightBeGetOrSet == true && (propertyName == "get" || propertyName == "set"))
                {
                    // Parse the function name and body.
                    var function = ParseFunction(propertyName == "get" ? FunctionType.Getter : FunctionType.Setter, this.currentScope);

                    // Get the function name.
                    var getOrSet = propertyName;
                    propertyName = function.FunctionName;

                    if (getOrSet == "get")
                    {
                        // This is a getter property.
                        object existingValue;
                        if (properties.TryGetValue(propertyName, out existingValue) == false)
                            // The property has not been seen before.
                            properties.Add(propertyName, new ObjectLiteralAccessor() { Getter = function });
                        else
                        {
                            // Add to the existing property.
                            var existingAccessor = existingValue as ObjectLiteralAccessor;
                            if (existingAccessor == null)
                                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("The property '{0}' cannot have both a data property and a getter", propertyName), this.LineNumber, this.SourcePath);
                            if (existingAccessor.Getter != null)
                                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("The property '{0}' cannot have multiple getters", propertyName), this.LineNumber, this.SourcePath);
                            existingAccessor.Getter = function;
                        }
                    }
                    else
                    {
                        // This is a setter property.
                        object existingValue;
                        if (properties.TryGetValue(propertyName, out existingValue) == false)
                            // The property has not been seen before.
                            properties.Add(propertyName, new ObjectLiteralAccessor() { Setter = function });
                        else
                        {
                            // Add to the existing property.
                            var existingAccessor = existingValue as ObjectLiteralAccessor;
                            if (existingAccessor == null)
                                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("The property '{0}' cannot have both a data property and a setter", propertyName), this.LineNumber, this.SourcePath);
                            if (existingAccessor.Setter != null)
                                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("The property '{0}' cannot have multiple setters", propertyName), this.LineNumber, this.SourcePath);
                            existingAccessor.Setter = function;
                        }
                    }
                }
                else
                {
                    // This is a regular property.

                    // Read the colon.
                    this.Expect(PunctuatorToken.Colon);

                    // Now read the property value.
                    propertyValue = ParseExpression(PunctuatorToken.Comma, PunctuatorToken.RightBrace);

                    // In strict mode, properties cannot be added twice.
                    object existingValue;
                    if (properties.TryGetValue(propertyName, out existingValue) == true)
                    {
                        if (existingValue is ObjectLiteralAccessor)
                            throw new JavaScriptException(this.engine, "SyntaxError", string.Format("The property '{0}' cannot have both a data property and a getter/setter", propertyName), this.LineNumber, this.SourcePath);
                        if (this.StrictMode == true)
                            throw new JavaScriptException(this.engine, "SyntaxError", string.Format("The property '{0}' already has a value", propertyName), this.LineNumber, this.SourcePath);
                    }

                    // Add the property setter to the list.
                    properties[propertyName] = propertyValue;
                }

                // Read past the comma.
                Debug.Assert(this.nextToken == PunctuatorToken.Comma || this.nextToken == PunctuatorToken.RightBrace);
                if (this.nextToken == PunctuatorToken.Comma)
                    this.Consume();
            }

            // The end token '}' will be consumed by the parent function.
            Debug.Assert(this.nextToken == PunctuatorToken.RightBrace);

            return new LiteralExpression(properties);
        }

        /// <summary>
        /// Reads a property name, used in object literals.
        /// </summary>
        /// <param name="wasIdentifier"> Receives <c>true</c> if the property name was identifier;
        /// <c>false</c> otherwise. </param>
        /// <returns> The property name that was read. </returns>
        private string ReadPropertyName(out bool wasIdentifier)
        {
            string propertyName;
            if (this.nextToken is LiteralToken)
            {
                // The property name can be a string or a number or (in ES5) a keyword.
                if (((LiteralToken)this.nextToken).IsKeyword == true)
                {
                    // false, true or null.
                    propertyName = this.nextToken.Text;
                }
                else
                {
                    object literalValue = ((LiteralToken)this.nextToken).Value;
                    if ((literalValue is string || literalValue is double || literalValue is int) == false)
                        throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Expected property name but found {0}", Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);
                    propertyName = ((LiteralToken)this.nextToken).Value.ToString();
                }
                wasIdentifier = false;
            }
            else if (this.nextToken is IdentifierToken)
            {
                // An identifier is also okay.
                propertyName = ((IdentifierToken)this.nextToken).Name;
                wasIdentifier = true;
            }
            else if (this.nextToken is KeywordToken)
            {
                // In ES5 a keyword is also okay.
                propertyName = ((KeywordToken)this.nextToken).Name;
                wasIdentifier = false;
            }
            else
                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Expected property name but found {0}", Token.ToText(this.nextToken)), this.LineNumber, this.SourcePath);

            // Consume the token.
            this.Consume();

            // Return the property name.
            return propertyName;
        }

        /// <summary>
        /// Parses a function expression.
        /// </summary>
        /// <returns> A function expression. </returns>
        private FunctionExpression ParseFunctionExpression()
        {
            return ParseFunction(FunctionType.Expression, this.currentScope);
        }
    }

}