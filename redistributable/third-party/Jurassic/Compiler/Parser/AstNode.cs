using System;
using System.Collections.Generic;

namespace Jurassic.Compiler
{
    /// <summary>
    /// Represents the base class of expressions and statements.
    /// </summary>
    internal abstract class AstNode
    {
        private static AstNode[] emptyNodeList = new AstNode[0];

        /// <summary>
        /// Gets an enumerable list of child nodes in the abstract syntax tree.
        /// </summary>
        public virtual IEnumerable<AstNode> ChildNodes
        {
            get { return emptyNodeList; }
        }

        /// <summary>
        /// Determines if this node or any of this node's children are of the given type.
        /// </summary>
        /// <typeparam name="T"> The type of AstNode to search for. </typeparam>
        /// <returns> <c>true</c> if this node or any of this node's children are of the given
        /// type; <c>false</c> otherwise. </returns>
        public bool ContainsNodeOfType<T>() where T : AstNode
        {
            if (this is T)
                return true;
            foreach (var child in this.ChildNodes)
                if (child.ContainsNodeOfType<T>())
                    return true;
            return false;
        }

        /// <summary>
        /// Generates CIL for the expression.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="optimizationInfo"> Information about any optimizations that should be performed. </param>
        public abstract void GenerateCode(ILGenerator generator, OptimizationInfo optimizationInfo);
    }
}
