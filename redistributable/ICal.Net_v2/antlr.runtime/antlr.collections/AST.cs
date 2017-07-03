using System.Collections;

namespace antlr.collections
{
    /*ANTLR Translator Generator
    * Project led by Terence Parr at http://www.jGuru.com
    * Software rights: http://www.antlr.org/license.html
    *
    * $Id:$
    */

    //
    // ANTLR C# Code Generator by Micheal Jordan
    //                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
    //                            Anthony Oguntimehin
    //
    // With many thanks to Eric V. Smith from the ANTLR list.
    //

    /// <summary>
    /// Minimal AST node interface used by ANTLR AST generation and tree-walker.
    /// </summary>
    public interface AST
    {
        /// <summary>
        /// Add a (rightmost) child to this node
        /// </summary>
        /// <param name="c"></param>
        void addChild(AST c);
        bool Equals(AST t);
        bool EqualsList(AST t);
        bool EqualsListPartial(AST t);
        bool EqualsTree(AST t);
        bool EqualsTreePartial(AST t);
        IEnumerator findAll(AST tree);
        IEnumerator findAllPartial(AST subtree);
        /// <summary>
        /// Get the first child of this node; null if no children
        /// </summary>
        AST getFirstChild();
        /// <summary>
        /// Get	the next sibling in line after this one
        /// </summary>
        AST getNextSibling();
        /// <summary>
        /// Get the token text for this node
        /// </summary>
        /// <returns></returns>
        string getText();
        /// <summary>
        /// Get the token type for this node
        /// </summary>
        int Type { get; set; }
        /// <summary>
        /// Get number of children of this node; if leaf, returns 0
        /// </summary>
        /// <returns>Number of children</returns>
        int getNumberOfChildren();
        void initialize(int t, string txt);
        void initialize(AST t);
        void initialize(IToken t);
        /// <summary>
        /// Set the first child of a node.
        /// </summary>
        /// <param name="c"></param>
        void setFirstChild(AST c);
        /// <summary>
        /// Set the next sibling after this one.
        /// </summary>
        /// <param name="n"></param>
        void setNextSibling(AST n);
        /// <summary>
        /// Set the token text for this node
        /// </summary>
        /// <param name="text"></param>
        void setText(string text);
        /// <summary>
        /// Set the token type for this node
        /// </summary>
        /// <param name="ttype"></param>
        void setType(int ttype);
        string ToString();
        string ToStringList();
        string ToStringTree();
        object Clone();
    }
}