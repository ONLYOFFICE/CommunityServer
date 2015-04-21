using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{

    /// <summary>
    /// Represents punctuation or an operator in the source code.
    /// </summary>
    internal class IdentifierToken : Token
    {
        /// <summary>
        /// Creates a new IdentifierToken instance.
        /// </summary>
        /// <param name="name"> The identifier name. </param>
        public IdentifierToken(string name)
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

        /// <summary>
        /// Gets a string that represents the token in a parseable form.
        /// </summary>
        public override string Text
        {
            get { return this.Name; }
        }
    }

}