using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAPExplorer
{
    public static class Extensions
    {
        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str.Trim());
        }
    }
}
