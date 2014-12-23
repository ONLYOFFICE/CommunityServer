using System;
using Jurassic.Compiler;

namespace Jurassic.Library
{

    /// <summary>
    /// Converts a series of JSON tokens into a JSON object.
    /// </summary>
    internal sealed class JSONParser
    {
        private ScriptEngine engine;
        private JSONLexer lexer;
        private Token nextToken;


        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a JSONParser instance with the given lexer supplying the tokens.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="lexer"> The lexical analyser that provides the tokens. </param>
        public JSONParser(ScriptEngine engine, JSONLexer lexer)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (lexer == null)
                throw new ArgumentNullException("lexer");
            this.engine = engine;
            this.lexer = lexer;
            this.Consume();
        }



        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets or sets a function that can be used to transform values as they are being parsed.
        /// </summary>
        public FunctionInstance ReviverFunction
        {
            get;
            set;
        }



        //     TOKEN HELPERS
        //_________________________________________________________________________________________

        /// <summary>
        /// Discards the current token and reads the next one.
        /// </summary>
        private void Consume()
        {
            do
            {
                this.nextToken = this.lexer.NextToken();
            } while ((this.nextToken is WhiteSpaceToken) == true);
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
                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Expected '{0}'", token.Text));
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
                throw new JavaScriptException(this.engine, "SyntaxError", "Expected identifier");
            }
        }




        //     PARSE METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Parses the JSON text (optionally applying the reviver function) and returns the resulting value.
        /// </summary>
        /// <returns> The result of parsing the JSON text. </returns>
        public object Parse()
        {
            // Parse the JSON text.
            object root = ParseValue();

            // We should now be at the end of the input.
            if (this.nextToken != null)
                throw new JavaScriptException(this.engine, "SyntaxError", "Expected end of input");

            // Apply the reviver function, if there is one.
            if (this.ReviverFunction != null)
            {
                var tempObject = this.engine.Object.Construct();
                tempObject[string.Empty] = root;
                return this.ReviverFunction.CallFromNative("parse", tempObject, string.Empty, root);
            }

            return root;
        }

        /// <summary>
        /// Parses a value.
        /// </summary>
        /// <returns> A JSON value. </returns>
        private object ParseValue()
        {
            object result;
            if (this.nextToken is LiteralToken)
            {
                result = ((LiteralToken)this.nextToken).Value;
                this.Consume();
            }
            else if (this.nextToken == PunctuatorToken.LeftBrace)
            {
                result = ParseObjectLiteral();
            }
            else if (this.nextToken == PunctuatorToken.LeftBracket)
            {
                result = ParseArrayLiteral();
            }
            else if (this.nextToken == null)
                throw new JavaScriptException(this.engine, "SyntaxError", "Unexpected end of input");
            else
                throw new JavaScriptException(this.engine, "SyntaxError", string.Format("Unexpected token {0}", this.nextToken));
            return result;
        }

        /// <summary>
        /// Parses an array literal (e.g. "[1, 2]").
        /// </summary>
        /// <returns> A populated array. </returns>
        private ArrayInstance ParseArrayLiteral()
        {
            // Read past the initial '[' token.
            this.Expect(PunctuatorToken.LeftBracket);

            // Loop until the next token is ']'.
            var result = this.engine.Array.New();
            uint arrayIndex = 0;
            while (this.nextToken != PunctuatorToken.RightBracket)
            {
                // Expect a comma except for the first element.
                if (arrayIndex > 0)
                    this.Expect(PunctuatorToken.Comma);

                // Read the array element value.
                object elementValue = ParseValue();

                // Set the array element.
                if (elementValue != null)
                    result[arrayIndex] = elementValue;

                // Apply the reviver function.
                if (this.ReviverFunction != null)
                {
                    var transformedValue = this.ReviverFunction.CallFromNative("parse", result, arrayIndex.ToString(), elementValue);
                    if (transformedValue != elementValue)
                    {
                        if (transformedValue == Undefined.Value || transformedValue == null)
                            // The return value is undefined - delete the property.
                            result.Delete(arrayIndex, false);
                        else
                            // The value has changed - update the holder object.
                            result[arrayIndex] = transformedValue;
                    }
                }

                arrayIndex++;
            }

            // Read the end token ']'.
            this.Expect(PunctuatorToken.RightBracket);

            // Return the array.
            return result;
        }

        /// <summary>
        /// Parses an object literal (e.g. "{a: 5}").
        /// </summary>
        /// <returns> A populated object. </returns>
        private ObjectInstance ParseObjectLiteral()
        {
            // Read past the initial '{' token.
            this.Expect(PunctuatorToken.LeftBrace);

            // If the next token is '}', then the object literal is complete.
            var result = this.engine.Object.Construct();
            bool expectComma = false;
            while (this.nextToken != PunctuatorToken.RightBrace)
            {
                // Expect a comma except for the first element.
                if (expectComma == true)
                    this.Expect(PunctuatorToken.Comma);
                expectComma = true;

                // Read the next property name.
                string propertyName;
                if (this.nextToken is LiteralToken)
                {
                    // The property name must be a string.
                    object literalValue = ((LiteralToken)this.nextToken).Value;
                    if ((literalValue is string) == false)
                        throw new JavaScriptException(this.engine, "SyntaxError", "Expected property name");
                    propertyName = (string)literalValue;
                }
                else
                    throw new JavaScriptException(this.engine, "SyntaxError", "Expected property name");
                this.Consume();

                // Read the colon.
                this.Expect(PunctuatorToken.Colon);

                // Now read the property value.
                var propertyValue = ParseValue();

                // Set the property value.
                result.FastSetProperty(propertyName, propertyValue, PropertyAttributes.FullAccess);

                // Apply the reviver function.
                if (this.ReviverFunction != null)
                {
                    var transformedValue = this.ReviverFunction.CallFromNative("parse", result, propertyName, propertyValue);
                    if (transformedValue != propertyValue)
                    {
                        if (transformedValue == Undefined.Value || transformedValue == null)
                            // The return value is undefined - delete the property.
                            result.Delete(propertyName, false);
                        else
                            // The value has changed - update the holder object.
                            result[propertyName] = transformedValue;
                    }
                }
            }

            // Read the end token '}'.
            this.Expect(PunctuatorToken.RightBrace);

            // Return the object.
            return result;
        }
    }

}