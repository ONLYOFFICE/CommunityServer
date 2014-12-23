using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the built-in JavaScript Function object.
    /// </summary>
    [Serializable]
    public class FunctionConstructor : ClrFunction
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Function object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="instancePrototype"> The prototype for instances created by this function. </param>
        internal FunctionConstructor(ObjectInstance prototype, FunctionInstance instancePrototype)
            : base(prototype, "Function", instancePrototype)
        {
        }



        //     CONSTRUCTORS
        //_________________________________________________________________________________________

        /// <summary>
        /// Called when the Function object is invoked like a function, e.g. var x = Function("5").
        /// Creates a new function instance.
        /// </summary>
        /// <param name="argumentsAndBody"> The argument names plus the function body. </param>
        /// <returns> A new function instance. </returns>
        [JSCallFunction]
        public FunctionInstance Call(params string[] argumentsAndBody)
        {
            return this.Construct(argumentsAndBody);
        }

        /// <summary>
        /// Creates a new function instance.
        /// </summary>
        /// <param name="argumentsAndBody"> The argument names plus the function body. </param>
        /// <returns> A new function instance. </returns>
        [JSConstructorFunction]
        public FunctionInstance Construct(params string[] argumentsAndBody)
        {
            // Passing no arguments results in an empty function.
            if (argumentsAndBody.Length == 0)
                return new UserDefinedFunction(this.InstancePrototype, "anonymous", new string[0], string.Empty);

            // Split any comma-delimited names.
            var argumentNames = new List<string>();
            for (int i = 0; i < argumentsAndBody.Length - 1; i++)
            {
                var splitNames = argumentsAndBody[i].Split(',');
                if (splitNames.Length > 1 || StringInstance.Trim(splitNames[0]) != string.Empty)
                {
                    for (int j = 0; j < splitNames.Length; j++)
                    {
                        // Trim any whitespace from the start and end of the argument name.
                        string argumentName = StringInstance.Trim(splitNames[j]);
                        if (argumentName == string.Empty)
                            throw new JavaScriptException(this.Engine, "SyntaxError", "Unexpected ',' in argument");

                        // Check the name is valid and resolve any escape sequences.
                        argumentName = Compiler.Lexer.ResolveIdentifier(this.Engine, argumentName);
                        if (argumentName == null)
                            throw new JavaScriptException(this.Engine, "SyntaxError", "Expected identifier");
                        splitNames[j] = argumentName;
                    }
                }
                argumentNames.AddRange(splitNames);
            }

            // Create a new function.
            return new UserDefinedFunction(this.InstancePrototype, "anonymous", argumentNames, argumentsAndBody[argumentsAndBody.Length - 1]);
        }
    }
}
