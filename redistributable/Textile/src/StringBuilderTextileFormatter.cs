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
using System.Text;
#endregion


namespace Textile
{
    public class StringBuilderTextileFormatter : IOutputter
    {
        StringBuilder m_stringBuilder = null;

        public StringBuilderTextileFormatter()
        {
        }

        public string GetFormattedText()
        {
            return m_stringBuilder.ToString();
        }

        #region IOutputter Members

        public void Begin()
        {
            m_stringBuilder = new StringBuilder();
        }

        public void End()
        {
        }

        public void Write(string text)
        {
            m_stringBuilder.Append(text);
        }

        public void WriteLine(string line)
        {
            m_stringBuilder.AppendLine(line);
        }

        #endregion
    }
}
