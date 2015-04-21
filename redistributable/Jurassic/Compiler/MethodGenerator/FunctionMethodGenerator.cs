using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents the information needed to compile a function.
    /// </summary>
    internal class FunctionMethodGenerator : MethodGenerator
    {
        /// <summary>
        /// Creates a new FunctionMethodGenerator instance.
        /// </summary>
        /// <param name="engine"> The script engine. </param>
        /// <param name="scope"> The function scope. </param>
        /// <param name="functionName"> The name of the function. </param>
        /// <param name="includeNameInScope"> Indicates whether the name should be included in the function scope. </param>
        /// <param name="argumentNames"> The names of the arguments. </param>
        /// <param name="bodyText"> The source code of the function. </param>
        /// <param name="body"> The root of the abstract syntax tree for the body of the function. </param>
        /// <param name="scriptPath"> The URL or file system path that the script was sourced from. </param>
        /// <param name="options"> Options that influence the compiler. </param>
        public FunctionMethodGenerator(ScriptEngine engine, DeclarativeScope scope, string functionName, bool includeNameInScope, IList<string> argumentNames, string bodyText, Statement body, string scriptPath, CompilerOptions options)
            : base(engine, scope, new DummyScriptSource(scriptPath), options)
        {
            this.Name = functionName;
            this.IncludeNameInScope = includeNameInScope;
            this.ArgumentNames = argumentNames;
            this.BodyRoot = body;
            this.BodyText = bodyText;
            Validate();
        }

        /// <summary>
        /// Dummy implementation of ScriptSource.
        /// </summary>
        private class DummyScriptSource : ScriptSource
        {
            private string path;

            public DummyScriptSource(string path)
            {
                this.path = path;
            }

            public override string Path
            {
                get { return this.path; }
            }

            public override System.IO.TextReader GetReader()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates a new FunctionContext instance.
        /// </summary>
        /// <param name="engine"> The script engine. </param>
        /// <param name="scope"> The function scope. </param>
        /// <param name="functionName"> The name of the function. </param>
        /// <param name="argumentNames"> The names of the arguments. </param>
        /// <param name="body"> The source code for the body of the function. </param>
        /// <param name="options"> Options that influence the compiler. </param>
        public FunctionMethodGenerator(ScriptEngine engine, DeclarativeScope scope, string functionName, IList<string> argumentNames, string body, CompilerOptions options)
            : base(engine, scope, new StringScriptSource(body), options)
        {
            this.Name = functionName;
            this.ArgumentNames = argumentNames;
            this.BodyText = body;
        }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the display name for the function.  This is statically inferred from the
        /// context if the function is the target of an assignment or if the function is within an
        /// object literal.  Only set if the function name is empty.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the function name should be included in the function scope (i.e. if
        /// this is <c>false</c>, then you cannot reference the function by name from within the
        /// body of the function).
        /// </summary>
        internal bool IncludeNameInScope
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a list of argument names.
        /// </summary>
        public IList<string> ArgumentNames
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the root of the abstract syntax tree for the body of the function.
        /// </summary>
        public Statement BodyRoot
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
        /// Gets a name for the generated method.
        /// </summary>
        /// <returns> A name for the generated method. </returns>
        protected override string GetMethodName()
        {
            if (this.DisplayName != null)
                return this.DisplayName;
            else if (string.IsNullOrEmpty(this.Name))
                return "anonymous";
            else
                return this.Name;
        }

        /// <summary>
        /// Gets a name for the function, as it appears in the stack trace.
        /// </summary>
        /// <returns> A name for the function, as it appears in the stack trace, or <c>null</c> if
        /// this generator is generating code in the global scope. </returns>
        protected override string GetStackName()
        {
            return GetMethodName();
        }

        /// <summary>
        /// Gets an array of types - one for each parameter accepted by the method generated by
        /// this context.
        /// </summary>
        /// <returns> An array of parameter types. </returns>
        protected override Type[] GetParameterTypes()
        {
            return new Type[] {
                typeof(ScriptEngine),               // The script engine.
                typeof(Scope),                      // The parent scope.
                typeof(object),                     // The "this" object.
                typeof(Library.FunctionInstance),   // The function object.
                typeof(object[])                    // The argument values.
            };
        }

        /// <summary>
        /// Checks whether the function is valid (in strict mode the function cannot be named
        /// 'arguments' or 'eval' and the argument names cannot be duplicated).
        /// </summary>
        private void Validate()
        {
            if (this.StrictMode == true)
            {
                // If the function body is strict mode, then the function name cannot be 'eval' or 'arguments'.
                if (this.Name == "arguments" || this.Name == "eval")
                    throw new JavaScriptException(this.Engine, "SyntaxError", string.Format("Functions cannot be named '{0}' in strict mode.", this.Name));

                // If the function body is strict mode, then the argument names cannot be 'eval' or 'arguments'.
                foreach (var argumentName in this.ArgumentNames)
                    if (argumentName == "arguments" || argumentName == "eval")
                        throw new JavaScriptException(this.Engine, "SyntaxError", string.Format("Arguments cannot be named '{0}' in strict mode.", argumentName));

                // If the function body is strict mode, then the argument names cannot be duplicates.
                var duplicateCheck = new HashSet<string>();
                foreach (var argumentName in this.ArgumentNames)
                {
                    if (duplicateCheck.Contains(argumentName) == true)
                        throw new JavaScriptException(this.Engine, "SyntaxError", string.Format("Duplicate argument name '{0}' is not allowed in strict mode.", argumentName));
                    duplicateCheck.Add(argumentName);
                }
            }
        }

        /// <summary>
        /// Parses the source text into an abstract syntax tree.
        /// </summary>
        /// <returns> The root node of the abstract syntax tree. </returns>
        public override void Parse()
        {
            if (this.BodyRoot != null)
            {
                this.AbstractSyntaxTree = this.BodyRoot;
            }
            else
            {
                using (var lexer = new Lexer(this.Engine, this.Source))
                {
                    var parser = new Parser(this.Engine, lexer, this.InitialScope, this.Options, CodeContext.Function);
                    this.AbstractSyntaxTree = parser.Parse();
                    this.StrictMode = parser.StrictMode;
                    this.MethodOptimizationHints = parser.MethodOptimizationHints;
                }
                Validate();
            }
        }

        /// <summary>
        /// Retrieves a delegate for the generated method.
        /// </summary>
        /// <param name="types"> The parameter types. </param>
        /// <returns> The delegate type that matches the method parameters. </returns>
        protected override Type GetDelegate()
        {
            return typeof(Library.FunctionDelegate);
        }

        /// <summary>
        /// Generates IL for the script.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        protected override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Method signature: object FunctionDelegate(Compiler.Scope scope, object thisObject, Library.FunctionInstance functionObject, object[] arguments)

            // Initialize the scope (note: the initial scope for a function is always declarative).
            this.InitialScope.GenerateScopeCreation(generator, optimizationInfo);
            
            // Verify the scope is correct.
            VerifyScope(generator);

            // In ES3 the "this" value must be an object.  See 10.4.3 in the spec.
            if (this.StrictMode == false && this.MethodOptimizationHints.HasThis == true)
            {
                // if (thisObject == null || thisObject == Null.Value || thisObject == Undefined.Value)
                EmitHelpers.LoadThis(generator);
                generator.LoadNull();
                generator.CompareEqual();
                EmitHelpers.LoadThis(generator);
                EmitHelpers.EmitNull(generator);
                generator.CompareEqual();
                generator.BitwiseOr();
                EmitHelpers.LoadThis(generator);
                EmitHelpers.EmitUndefined(generator);
                generator.CompareEqual();
                generator.BitwiseOr();

                // {
                var startOfFalse = generator.CreateLabel();
                generator.BranchIfFalse(startOfFalse);

                // thisObject = engine.Global;
                EmitHelpers.LoadScriptEngine(generator);
                generator.Call(ReflectionHelpers.ScriptEngine_Global);
                
                // } else {
                var endOfIf = generator.CreateLabel();
                generator.Branch(endOfIf);
                generator.DefineLabelPosition(startOfFalse);
                
                // thisObject = TypeConverter.ToObject(thisObject);
                EmitHelpers.LoadThis(generator);
                EmitConversion.ToObject(generator, PrimitiveType.Any, optimizationInfo);

                // }
                generator.DefineLabelPosition(endOfIf);
                EmitHelpers.StoreThis(generator);
            }

            // Transfer the function name into the scope.
            if (string.IsNullOrEmpty(this.Name) == false &&
                this.IncludeNameInScope == true &&
                this.ArgumentNames.Contains(this.Name) == false &&
                optimizationInfo.MethodOptimizationHints.HasVariable(this.Name))
            {
                EmitHelpers.LoadFunction(generator);
                var functionName = new NameExpression(this.InitialScope, this.Name);
                functionName.GenerateSet(generator, optimizationInfo, PrimitiveType.Any, false);
            }

            // Transfer the arguments object into the scope.
            if (this.MethodOptimizationHints.HasArguments == true && this.ArgumentNames.Contains("arguments") == false)
            {
                // prototype
                EmitHelpers.LoadScriptEngine(generator);
                generator.Call(ReflectionHelpers.ScriptEngine_Object);
                generator.Call(ReflectionHelpers.FunctionInstance_InstancePrototype);
                // callee
                EmitHelpers.LoadFunction(generator);
                generator.CastClass(typeof(Library.UserDefinedFunction));
                // scope
                EmitHelpers.LoadScope(generator);
                generator.CastClass(typeof(DeclarativeScope));
                // argumentValues
                EmitHelpers.LoadArgumentsArray(generator);
                generator.NewObject(ReflectionHelpers.Arguments_Constructor);
                var arguments = new NameExpression(this.InitialScope, "arguments");
                arguments.GenerateSet(generator, optimizationInfo, PrimitiveType.Any, false);
            }

            // Transfer the argument values into the scope.
            // Note: the arguments array can be smaller than expected.
            if (this.ArgumentNames.Count > 0)
            {
                var endOfArguments = generator.CreateLabel();
                for (int i = 0; i < this.ArgumentNames.Count; i++)
                {
                    // Check if a duplicate argument name exists.
                    bool duplicate = false;
                    for (int j = i + 1; j < this.ArgumentNames.Count; j++)
                        if (this.ArgumentNames[i] == this.ArgumentNames[j])
                        {
                            duplicate = true;
                            break;
                        }
                    if (duplicate == true)
                        continue;

                    // Check if an array element exists.
                    EmitHelpers.LoadArgumentsArray(generator);
                    generator.LoadArrayLength();
                    generator.LoadInt32(i);
                    generator.BranchIfLessThanOrEqual(endOfArguments);

                    // Store the array element in the scope.
                    EmitHelpers.LoadArgumentsArray(generator);
                    generator.LoadInt32(i);
                    generator.LoadArrayElement(typeof(object));
                    var argument = new NameExpression(this.InitialScope, this.ArgumentNames[i]);
                    argument.GenerateSet(generator, optimizationInfo, PrimitiveType.Any, false);
                }
                generator.DefineLabelPosition(endOfArguments);
            }

            // Initialize any declarations.
            this.InitialScope.GenerateDeclarations(generator, optimizationInfo);

            //EmitHelpers.LoadScope(generator);
            //EmitConversion.ToObject(generator, PrimitiveType.Any);
            //generator.Pop();

            // Generate code for the body of the function.
            this.AbstractSyntaxTree.GenerateCode(generator, optimizationInfo);

            // Define the return target - this is where the return statement jumps to.
            // ReturnTarget can be null if there were no return statements.
            if (optimizationInfo.ReturnTarget != null)
                generator.DefineLabelPosition(optimizationInfo.ReturnTarget);

            // Load the return value.  If the variable is null, there were no return statements.
            if (optimizationInfo.ReturnVariable != null)
                // Return the value stored in the variable.  Will be null if execution hits the end
                // of the function without encountering any return statements.
                generator.LoadVariable(optimizationInfo.ReturnVariable);
            else
                // There were no return statements - return null.
                generator.LoadNull();
        }

        /// <summary>
        /// Converts this object to a string.
        /// </summary>
        /// <returns> A string representing this object. </returns>
        public override string ToString()
        {
            if (this.BodyRoot != null)
                return string.Format("function {0}({1}) {2}", this.Name, StringHelpers.Join(", ", this.ArgumentNames), this.BodyRoot);
            return string.Format("function {0}({1}) {{\n{2}\n}}", this.Name, StringHelpers.Join(", ", this.ArgumentNames), this.BodyText);
        }
    }

}