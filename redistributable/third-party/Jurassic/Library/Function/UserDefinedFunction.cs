using System;
using System.Collections.Generic;
using Jurassic.Compiler;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents a JavaScript function implemented in javascript.
    /// </summary>
    [Serializable]
    public class UserDefinedFunction : FunctionInstance
    {
        [NonSerialized]
        private GeneratedMethod generatedMethod;

        [NonSerialized]
        private FunctionDelegate body;
        


        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new instance of a user-defined function.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="argumentNames"> The names of the arguments. </param>
        /// <param name="bodyText"> The source code for the body of the function. </param>
        internal UserDefinedFunction(ObjectInstance prototype, string name, IList<string> argumentNames, string bodyText)
            : base(prototype)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (argumentNames == null)
                throw new ArgumentNullException("argumentNames");
            if (bodyText == null)
                throw new ArgumentNullException("bodyText");

            // Set up a new function scope.
            var scope = DeclarativeScope.CreateFunctionScope(this.Engine.CreateGlobalScope(), name, argumentNames);

            // Compile the code.
            var context = new FunctionMethodGenerator(this.Engine, scope, name, argumentNames, bodyText, new CompilerOptions());
            context.GenerateCode();

            // Create a new user defined function.
            Init(name, argumentNames, this.Engine.CreateGlobalScope(), bodyText, context.GeneratedMethod, context.StrictMode, true);
        }

        /// <summary>
        /// Creates a new instance of a user-defined function.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="argumentNames"> The names of the arguments. </param>
        /// <param name="parentScope"> The scope at the point the function is declared. </param>
        /// <param name="bodyText"> The source code for the function body. </param>
        /// <param name="body"> A delegate which represents the body of the function. </param>
        /// <param name="strictMode"> <c>true</c> if the function body is strict mode; <c>false</c> otherwise. </param>
        internal UserDefinedFunction(ObjectInstance prototype, string name, IList<string> argumentNames, Scope parentScope, string bodyText, FunctionDelegate body, bool strictMode)
            : base(prototype)
        {
            Init(name, argumentNames, parentScope, bodyText, new GeneratedMethod(body, null), strictMode, true);
        }

        /// <summary>
        /// Creates a new instance of a user-defined function.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="argumentNames"> The names of the arguments. </param>
        /// <param name="parentScope"> The scope at the point the function is declared. </param>
        /// <param name="bodyText"> The source code for the function body. </param>
        /// <param name="generatedMethod"> A delegate which represents the body of the function plus any dependencies. </param>
        /// <param name="strictMode"> <c>true</c> if the function body is strict mode; <c>false</c> otherwise. </param>
        public UserDefinedFunction(ObjectInstance prototype, string name, IList<string> argumentNames, Scope parentScope, string bodyText, GeneratedMethod generatedMethod, bool strictMode)
            : base(prototype)
        {
            Init(name, argumentNames, parentScope, bodyText, generatedMethod, strictMode, true);
        }

        /// <summary>
        /// Creates an empty function that is used as the prototype for all the built-in
        /// global function objects (Object, Function, Number, etc).
        /// </summary>
        /// <param name="prototype"> The prototype of the function. </param>
        /// <returns> An empty function that is used as the prototype for all the built-in
        /// global function objects. </returns>
        internal static UserDefinedFunction CreateEmptyFunction(ObjectInstance prototype)
        {
            return new UserDefinedFunction(prototype);
        }

        /// <summary>
        /// Creates an empty function.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        private UserDefinedFunction(ObjectInstance prototype)
            : base(prototype)
        {
            var body = new FunctionDelegate((engine, scope, functionObject, thisObject, argumentValues) => Undefined.Value);
            Init("Empty", new string[0], this.Engine.CreateGlobalScope(), "return undefined", new GeneratedMethod(body, null), true, false);
        }

        /// <summary>
        /// Initializes a user-defined function.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="argumentNames"> The names of the arguments. </param>
        /// <param name="parentScope"> The scope at the point the function is declared. </param>
        /// <param name="bodyText"> The source code for the function body. </param>
        /// <param name="generatedMethod"> A delegate which represents the body of the function, plus any dependencies. </param>
        /// <param name="strictMode"> <c>true</c> if the function body is strict mode; <c>false</c> otherwise. </param>
        /// <param name="hasInstancePrototype"> <c>true</c> if the function should have a valid
        /// "prototype" property; <c>false</c> if the "prototype" property should be <c>null</c>. </param>
        private void Init(string name, IList<string> argumentNames, Scope parentScope, string bodyText, GeneratedMethod generatedMethod, bool strictMode, bool hasInstancePrototype)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (argumentNames == null)
                throw new ArgumentNullException("argumentNames");
            if (bodyText == null)
                throw new ArgumentNullException("bodyText");
            if (generatedMethod == null)
                throw new ArgumentNullException("generatedMethod");
            if (parentScope == null)
                throw new ArgumentNullException("parentScope");
            this.ArgumentNames = new System.Collections.ObjectModel.ReadOnlyCollection<string>(argumentNames);
            this.BodyText = bodyText;
            this.generatedMethod = generatedMethod;
            this.body = (FunctionDelegate)this.generatedMethod.GeneratedDelegate;
            this.ParentScope = parentScope;
            this.StrictMode = strictMode;

            // Add function properties.
            this.FastSetProperty("name", name);
            this.FastSetProperty("length", argumentNames.Count);

            // The empty function doesn't have an instance prototype.
            if (hasInstancePrototype == true)
            {
                this.FastSetProperty("prototype", this.Engine.Object.Construct(), PropertyAttributes.Writable);
                this.InstancePrototype.FastSetProperty("constructor", this, PropertyAttributes.NonEnumerable);
            }
        }

        /// <summary>
        /// Compiles the function (if it hasn't already been compiled) and returns a delegate
        /// representing the compiled function.
        /// </summary>
        private FunctionDelegate Compile()
        {
            if (this.body == null)
            {
                // Compile the function.
                var scope = DeclarativeScope.CreateFunctionScope(this.Engine.CreateGlobalScope(), this.Name, this.ArgumentNames);
                var functionGenerator = new FunctionMethodGenerator(this.Engine, scope, this.Name, this.ArgumentNames, this.BodyText, new CompilerOptions());
                functionGenerator.GenerateCode();
                this.generatedMethod = functionGenerator.GeneratedMethod;
                this.body = (FunctionDelegate)this.generatedMethod.GeneratedDelegate;
            }
            return this.body;
        }



        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets a list containing the names of the function arguments, in order of definition.
        /// This list can contain duplicate names.
        /// </summary>
        public IList<string> ArgumentNames
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value that indicates whether the function was declared within strict mode code
        /// -or- the function contains a strict mode directive within the function body.
        /// </summary>
        public bool StrictMode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the scope at the point the function was declared.
        /// </summary>
        internal Scope ParentScope
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the source code for the body of the function.
        /// </summary>
        public string BodyText
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the body of the method in the form of disassembled IL code.  Will be <c>null</c>
        /// unless ScriptEngine.EnableILAnalysis has been set to <c>true</c>.
        /// </summary>
        public string DisassembledIL
        {
            get
            {
                // Compile the function.
                var body = Compile();

                // Return the disassembled IL.
                return this.generatedMethod.DisassembledIL;
            }
        }



        //     OVERRIDES
        //_________________________________________________________________________________________

        /// <summary>
        /// Calls this function, passing in the given "this" value and zero or more arguments.
        /// </summary>
        /// <param name="thisObject"> The value of the "this" keyword within the function. </param>
        /// <param name="argumentValues"> An array of argument values to pass to the function. </param>
        /// <returns> The value that was returned from the function. </returns>
        public override object CallLateBound(object thisObject, params object[] argumentValues)
        {
            // Compile the function.
            var body = Compile();

            // Call the function.
            return body(this.Engine, this.ParentScope, thisObject, this, argumentValues);
        }

        /// <summary>
        /// Returns a string representing this object.
        /// </summary>
        /// <returns> A string representing this object. </returns>
        public override string ToString()
        {
            return string.Format("function {0}({1}) {{\n{2}\n}}", this.Name, StringHelpers.Join(", ", this.ArgumentNames), this.BodyText);
        }
    }
}
