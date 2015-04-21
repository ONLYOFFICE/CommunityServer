using System;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents the information needed to compile global code.
    /// </summary>
    internal class GlobalMethodGenerator : MethodGenerator
    {
        /// <summary>
        /// Creates a new GlobalMethodGenerator instance.
        /// </summary>
        /// <param name="engine"> The script engine. </param>
        /// <param name="source"> The source of javascript code. </param>
        /// <param name="options"> Options that influence the compiler. </param>
        public GlobalMethodGenerator(ScriptEngine engine, ScriptSource source, CompilerOptions options)
            : base(engine, engine.CreateGlobalScope(), source, options)
        {
        }

        /// <summary>
        /// Gets a TextReader that can read the javascript source text.
        /// </summary>
        public System.IO.TextReader Reader
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
            // Take the path of the script and replace the non-alphanumeric characters with
            // underscores.
            var sanitizedPath = new System.Text.StringBuilder(this.Source.Path);
            for (int i = 0; i < sanitizedPath.Length; i++)
            {
                char c = sanitizedPath[i];
                if ((c < '0' || c > '9') && (c < 'a' || c > 'z') && (c < 'A' || c > 'Z'))
                    sanitizedPath[i] = '_';
            }
            return string.Format("global_{0}", sanitizedPath.ToString());
        }

        /// <summary>
        /// Parses the source text into an abstract syntax tree.
        /// </summary>
        public override void Parse()
        {
            using (var lexer = new Lexer(this.Engine, this.Source))
            {
                var parser = new Parser(this.Engine, lexer, this.InitialScope, this.Options, CodeContext.Global);
                this.AbstractSyntaxTree = parser.Parse();
                this.StrictMode = parser.StrictMode;
                this.MethodOptimizationHints = parser.MethodOptimizationHints;
            }
        }

        /// <summary>
        /// Generates IL for the script.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        protected override void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo)
        {
            // Verify the scope is correct.
            VerifyScope(generator);

            // Initialize any function or variable declarations.
            this.InitialScope.GenerateDeclarations(generator, optimizationInfo);

            // Generate code for the source code.
            this.AbstractSyntaxTree.GenerateCode(generator, optimizationInfo);

            // Code in the global context always returns undefined.
            EmitHelpers.EmitUndefined(generator);
        }

        /// <summary>
        /// Executes the script.
        /// </summary>
        /// <returns> The result of evaluating the script. </returns>
        public object Execute()
        {
            // Compile the code if it hasn't already been compiled.
            if (this.GeneratedMethod == null)
                GenerateCode();

            // Execute the compiled delegate and store the result.
            object result = ((Func<ScriptEngine, Scope, object, object>)this.GeneratedMethod.GeneratedDelegate)(this.Engine, this.InitialScope, this.Engine.Global);

            // Ensure the abstract syntax tree is kept alive until the eval code finishes running.
            GC.KeepAlive(this);

            // Return the result.
            return result;
        }
    }

}