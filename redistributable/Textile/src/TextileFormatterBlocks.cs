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
#endregion


namespace Textile
{
    public partial class TextileFormatter
    {
        #region Block Modifiers Registration

        private static List<BlockModifier> s_blockModifiers = new List<BlockModifier>();
        private static List<Type> s_blockModifiersTypes = new List<Type>();

        public static void RegisterBlockModifier(BlockModifier blockModifer)
        {
            s_blockModifiers.Add(blockModifer);
            s_blockModifiersTypes.Add(blockModifer.GetType());
        }

        #endregion

        #region Block Modifiers Management

        private List<Type> m_disabledBlockModifiers = new List<Type>();

        public bool IsBlockModifierEnabled(Type type)
        {
            return !m_disabledBlockModifiers.Contains(type);
        }

        public void SwitchBlockModifier(Type type, bool onOff)
        {
            if (onOff)
                m_disabledBlockModifiers.Remove(type);
            else if (!m_disabledBlockModifiers.Contains(type))
                m_disabledBlockModifiers.Add(type);
        }

        #endregion
    }
}
