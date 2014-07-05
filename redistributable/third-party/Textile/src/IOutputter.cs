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
using System.IO;
using System.Text;
#endregion


namespace Textile
{
    /// <summary>
    /// Interface through which the HTML formatted text
    /// will be sent.
    /// </summary>
    /// Clients of the TextileFormatter class will have to provide
    /// an outputter that implements this interface. Most of the
    /// time, it'll be the WebForm itself.
    public interface IOutputter
    {
        /// <summary>
        /// Method called just before the formatted text
        /// is sent to the outputter.
        /// </summary>
        void Begin();

        /// <summary>
        /// Metohd called whenever the TextileFormatter wants to
        /// print some text.
        /// </summary>
        /// <param name="text">The formatted HTML text.</param>
        void Write(string text);
        /// <summary>
        /// Metohd called whenever the TextileFormatter wants to
        /// print some text. This should automatically print an
        /// additionnal end of line character.
        /// </summary>
        /// <param name="line">The formatted HTML text.</param>
        void WriteLine(string line);

        /// <summary>
        /// Method called at the end of the formatting.
        /// </summary>
        void End();
    }
}
