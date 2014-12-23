using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents information useful for optimizing a method.
    /// </summary>
    internal class MethodOptimizationHints
    {
        private HashSet<string> names = new HashSet<string>();
        private bool cached, hasArguments, hasEval, hasNestedFunction, hasThis;

        /// <summary>
        /// Called by the parser whenever a variable is encountered (variable being any identifier
        /// which is not a property name).
        /// </summary>
        /// <param name="name"> The variable name. </param>
        public void EncounteredVariable(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            this.names.Add(name);
            this.cached = false;
        }

        /// <summary>
        /// Determines if the parser encountered the given variable name while parsing the
        /// function, or if the function contains a reference to "eval" or the function contains
        /// nested functions which may reference the variable.
        /// </summary>
        /// <param name="name"> The variable name. </param>
        /// <returns> <c>true</c> if the parser encountered the given variable name or "eval" while
        /// parsing the function; <c>false</c> otherwise. </returns>
        public bool HasVariable(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (this.HasEval == true)
                return true;
            if (this.HasNestedFunction == true)
                return true;
            return this.names.Contains(name);
        }

        /// <summary>
        /// Gets a value that indicates whether the function being generated contains a reference
        /// to the arguments object.
        /// </summary>
        public bool HasArguments
        {
            get
            {
                CacheResults();
                return this.hasArguments;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the function being generated contains an eval
        /// statement.
        /// </summary>
        public bool HasEval
        {
            get
            {
                CacheResults();
                return this.hasEval;
            }
        }

        /// <summary>
        /// Caches the HasEval and HasArguments property access.
        /// </summary>
        private void CacheResults()
        {
            if (this.cached == false)
            {
                this.hasEval = this.HasNestedFunction == true || this.names.Contains("eval");
                this.hasArguments = this.hasEval == true || this.names.Contains("arguments");
                this.cached = true;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the function being generated contains a
        /// nested function declaration or expression.
        /// </summary>
        public bool HasNestedFunction
        {
            get { return this.hasNestedFunction; }
            set
            {
                this.hasNestedFunction = value;
                this.cached = false;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the function being generated contains a
        /// reference to the "this" keyword.
        /// </summary>
        public bool HasThis
        {
            get { return this.hasThis || this.HasEval == true; }
            set { this.hasThis = value; }
        }
    }

}
