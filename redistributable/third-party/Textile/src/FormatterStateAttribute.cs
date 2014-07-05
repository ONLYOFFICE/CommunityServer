using System;
using System.Collections.Generic;
using System.Text;

namespace Textile
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class FormatterStateAttribute : Attribute
    {
        private string m_pattern;
        public string Pattern
        {
            get { return m_pattern; }
        }

        public FormatterStateAttribute(string pattern)
        {
            m_pattern = pattern;
        }

        public static FormatterStateAttribute Get(Type type)
        {
            object[] atts = type.GetCustomAttributes(typeof(FormatterStateAttribute), false);
            if (atts.Length == 0)
                return null;
            return (FormatterStateAttribute)atts[0];
        }
    }
}
