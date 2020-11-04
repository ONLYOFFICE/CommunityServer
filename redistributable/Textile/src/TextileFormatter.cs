#region License Statement
// Copyright (c) L.A.B.Soft.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Textile.States;
using Textile.Blocks;
#endregion


namespace Textile
{
    /// <summary>
    /// Class for formatting Textile input into HTML.
    /// </summary>
    /// This class takes raw Textile text and sends the
    /// formatted, ready to display HTML string to the
    /// outputter defined in the constructor of the
    /// class.
    public partial class TextileFormatter
	{
        static TextileFormatter()
        {
            RegisterFormatterState(typeof(HeaderFormatterState));
            RegisterFormatterState(typeof(PaddingFormatterState));
            RegisterFormatterState(typeof(BlockQuoteFormatterState));
            RegisterFormatterState(typeof(ParagraphFormatterState));
            RegisterFormatterState(typeof(FootNoteFormatterState));
            RegisterFormatterState(typeof(OrderedListFormatterState));
            RegisterFormatterState(typeof(UnorderedListFormatterState));
            RegisterFormatterState(typeof(TableFormatterState));
            RegisterFormatterState(typeof(TableRowFormatterState));
            RegisterFormatterState(typeof(CodeFormatterState));
            RegisterFormatterState(typeof(PreFormatterState));
            RegisterFormatterState(typeof(PreCodeFormatterState));
            RegisterFormatterState(typeof(NoTextileFormatterState));

            RegisterBlockModifier(new NoTextileBlockModifier());
            RegisterBlockModifier(new CodeBlockModifier());
            RegisterBlockModifier(new PreBlockModifier());
            RegisterBlockModifier(new HyperLinkBlockModifier());
            RegisterBlockModifier(new ImageBlockModifier());
            RegisterBlockModifier(new GlyphBlockModifier());
            RegisterBlockModifier(new EmphasisPhraseBlockModifier());
            RegisterBlockModifier(new StrongPhraseBlockModifier());
            RegisterBlockModifier(new ItalicPhraseBlockModifier());
            RegisterBlockModifier(new BoldPhraseBlockModifier());
            RegisterBlockModifier(new CitePhraseBlockModifier());
            RegisterBlockModifier(new DeletedPhraseBlockModifier());
            RegisterBlockModifier(new InsertedPhraseBlockModifier());
            RegisterBlockModifier(new SuperScriptPhraseBlockModifier());
            RegisterBlockModifier(new SubScriptPhraseBlockModifier());
            RegisterBlockModifier(new SpanPhraseBlockModifier());
            RegisterBlockModifier(new FootNoteReferenceBlockModifier());
            
            //TODO: capitals block modifier
        }

		/// <summary>
        /// Public constructor, where the formatter is hooked up
        /// to an outputter.
        /// </summary>
        /// <param name="output">The outputter to be used.</param>
        public TextileFormatter(IOutputter output)
        {
            m_output = output;
		}

		#region Properties for Output

		private IOutputter m_output = null;
        /// <summary>
        /// The ouputter to which the formatted text
        /// is sent to.
        /// </summary>
        public IOutputter Output
        {
            get { return m_output; }
		}

		private int m_headerOffset = 0;
		/// <summary>
		/// The offset for the header tags.
		/// </summary>
		/// When the formatted text is inserted into another page
		/// there might be a need to offset all headers (h1 becomes
		/// h4, for instance). The header offset allows this.
		public int HeaderOffset
		{
			get { return m_headerOffset; }
			set { m_headerOffset = value; }
		}

		#endregion

        #region Properties for Conversion

        public bool FormatImages
        {
            get { return IsBlockModifierEnabled(typeof(ImageBlockModifier)); }
            set { SwitchBlockModifier(typeof(ImageBlockModifier), value); }
        }

        public bool FormatLinks
        {
            get { return IsBlockModifierEnabled(typeof(HyperLinkBlockModifier)); }
            set { SwitchBlockModifier(typeof(HyperLinkBlockModifier), value); }
        }

        public bool FormatLists
        {
            get { return IsBlockModifierEnabled(typeof(OrderedListFormatterState)); }
            set
            {
                SwitchBlockModifier(typeof(OrderedListFormatterState), value);
                SwitchBlockModifier(typeof(UnorderedListFormatterState), value);
            }
        }

        public bool FormatFootNotes
        {
            get { return IsBlockModifierEnabled(typeof(FootNoteReferenceBlockModifier)); }
            set
            {
                SwitchBlockModifier(typeof(FootNoteReferenceBlockModifier), value);
                SwitchFormatterState(typeof(FootNoteFormatterState), value);
            }
        }

        public bool FormatTables
        {
            get { return IsFormatterStateEnabled(typeof(TableFormatterState)); }
            set
            {
                SwitchFormatterState(typeof(TableFormatterState), value);
                SwitchFormatterState(typeof(TableRowFormatterState), value);
            }
        }

        string m_rel = string.Empty;
		/// <summary>
		/// Attribute to add to all links.
		/// </summary>
        public string Rel
        {
            get { return m_rel; }
            set { m_rel = value; }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Utility method for quickly formatting a text without having
        /// to create a TextileFormatter with an IOutputter.
        /// </summary>
        /// <param name="input">The string to format</param>
        /// <returns>The formatted version of the string</returns>
        public static string FormatString(string input)
        {
            StringBuilderTextileFormatter s = new StringBuilderTextileFormatter();
            TextileFormatter f = new TextileFormatter(s);
            f.Format(input);
            return s.GetFormattedText();
        }

        /// <summary>
        /// Utility method for formatting a text with a given outputter.
        /// </summary>
        /// <param name="input">The string to format</param>
        /// <param name="outputter">The IOutputter to use</param>
        public static void FormatString(string input, IOutputter outputter)
        {
            TextileFormatter f = new TextileFormatter(outputter);
            f.Format(input);
        }

        #endregion
    }
}
