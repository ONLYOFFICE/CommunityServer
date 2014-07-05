namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// This class is a helper implementation for the Android platform
    /// </summary>
    internal sealed class HttpUtility
    {
        private static Hashtable entities;
        private static char[] hexChars = "0123456789abcdef".ToCharArray();
        private static object lock_ = new object();

        private static int GetChar(byte[] bytes, int offset, int length)
        {
            int num = 0;
            int num2 = length + offset;
            for (int i = offset; i < num2; i++)
            {
                int @int = GetInt(bytes[i]);
                if (@int == -1)
                {
                    return -1;
                }
                num = (num << 4) + @int;
            }
            return num;
        }

        private static int GetChar(string str, int offset, int length)
        {
            int num = 0;
            int num2 = length + offset;
            for (int i = offset; i < num2; i++)
            {
                char ch = str[i];
                if (ch > '\x007f')
                {
                    return -1;
                }
                int @int = GetInt((byte) ch);
                if (@int == -1)
                {
                    return -1;
                }
                num = (num << 4) + @int;
            }
            return num;
        }

        private static char[] GetChars(MemoryStream b, Encoding e)
        {
            return e.GetChars(b.GetBuffer(), 0, (int) b.Length);
        }

        private static int GetInt(byte b)
        {
            char ch = (char) b;
            if ((ch >= '0') && (ch <= '9'))
            {
                return (ch - '0');
            }
            if ((ch >= 'a') && (ch <= 'f'))
            {
                return ((ch - 'a') + 10);
            }
            if ((ch >= 'A') && (ch <= 'F'))
            {
                return ((ch - 'A') + 10);
            }
            return -1;
        }

        public static string HtmlAttributeEncode(string s)
        {
            if (s == null)
            {
                return null;
            }
            bool flag = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (((s[i] == '&') || (s[i] == '"')) || (s[i] == '<'))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return s;
            }
            StringBuilder builder = new StringBuilder();
            int length = s.Length;
            for (int j = 0; j < length; j++)
            {
                switch (s[j])
                {
                    case '"':
                        builder.Append("&quot;");
                        break;

                    case '&':
                        builder.Append("&amp;");
                        break;

                    case '<':
                        builder.Append("&lt;");
                        break;

                    default:
                        builder.Append(s[j]);
                        break;
                }
            }
            return builder.ToString();
        }

        public static void HtmlAttributeEncode(string s, TextWriter output)
        {
            output.Write(HtmlAttributeEncode(s));
        }

        public static string HtmlDecode(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (s.IndexOf('&') == -1)
            {
                return s;
            }
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            int length = s.Length;
            int num2 = 0;
            int num3 = 0;
            bool flag = false;
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                if (num2 == 0)
                {
                    if (ch == '&')
                    {
                        builder.Append(ch);
                        num2 = 1;
                    }
                    else
                    {
                        builder2.Append(ch);
                    }
                }
                else if (ch == '&')
                {
                    num2 = 1;
                    if (flag)
                    {
                        builder.Append(num3.ToString(CultureInfo.InvariantCulture));
                        flag = false;
                    }
                    builder2.Append(builder.ToString());
                    builder.Length = 0;
                    builder.Append('&');
                }
                else
                {
                    switch (num2)
                    {
                        case 1:
                            if (ch == ';')
                            {
                                num2 = 0;
                                builder2.Append(builder.ToString());
                                builder2.Append(ch);
                                builder.Length = 0;
                            }
                            else
                            {
                                num3 = 0;
                                if (ch != '#')
                                {
                                    num2 = 2;
                                }
                                else
                                {
                                    num2 = 3;
                                }
                                builder.Append(ch);
                            }
                            break;

                        case 2:
                            builder.Append(ch);
                            if (ch == ';')
                            {
                                string str = builder.ToString();
                                if ((str.Length > 1) && Entities.ContainsKey(str.Substring(1, str.Length - 2)))
                                {
                                    str = Entities[str.Substring(1, str.Length - 2)].ToString();
                                }
                                builder2.Append(str);
                                num2 = 0;
                                builder.Length = 0;
                            }
                            break;

                        case 3:
                            if (ch == ';')
                            {
                                if (num3 > 0xffff)
                                {
                                    builder2.Append("&#");
                                    builder2.Append(num3.ToString(CultureInfo.InvariantCulture));
                                    builder2.Append(";");
                                }
                                else
                                {
                                    builder2.Append((char) num3);
                                }
                                num2 = 0;
                                builder.Length = 0;
                                flag = false;
                            }
                            else if (char.IsDigit(ch))
                            {
                                num3 = (num3 * 10) + (ch - '0');
                                flag = true;
                            }
                            else
                            {
                                num2 = 2;
                                if (flag)
                                {
                                    builder.Append(num3.ToString(CultureInfo.InvariantCulture));
                                    flag = false;
                                }
                                builder.Append(ch);
                            }
                            break;
                    }
                }
            }
            if (builder.Length > 0)
            {
                builder2.Append(builder.ToString());
            }
            else if (flag)
            {
                builder2.Append(num3.ToString(CultureInfo.InvariantCulture));
            }
            return builder2.ToString();
        }

        public static void HtmlDecode(string s, TextWriter output)
        {
            if (s != null)
            {
                output.Write(HtmlDecode(s));
            }
        }

        public static string HtmlEncode(string s)
        {
            if (s == null)
            {
                return null;
            }
            bool flag = false;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (((ch == '&') || (ch == '"')) || (((ch == '<') || (ch == '>')) || (ch > '\x009f')))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return s;
            }
            StringBuilder builder = new StringBuilder();
            int length = s.Length;
            for (int j = 0; j < length; j++)
            {
                char ch2 = s[j];
                switch (ch2)
                {
                    case '<':
                    {
                        builder.Append("&lt;");
                        continue;
                    }
                    case '>':
                    {
                        builder.Append("&gt;");
                        continue;
                    }
                    default:
                    {
                        if (ch2 != '"')
                        {
                            if (ch2 != '&')
                            {
                                goto Label_00F4;
                            }
                            builder.Append("&amp;");
                        }
                        else
                        {
                            builder.Append("&quot;");
                        }
                        continue;
                    }
                }
            Label_00F4:
                if (s[j] > '\x009f')
                {
                    builder.Append("&#");
                    builder.Append(((int) s[j]).ToString(CultureInfo.InvariantCulture));
                    builder.Append(";");
                }
                else
                {
                    builder.Append(s[j]);
                }
            }
            return builder.ToString();
        }

        public static void HtmlEncode(string s, TextWriter output)
        {
            if (s != null)
            {
                output.Write(HtmlEncode(s));
            }
        }

        private static void InitEntities()
        {
            entities = new Hashtable();
            entities.Add("nbsp", '\x00a0');
            entities.Add("iexcl", '\x00a1');
            entities.Add("cent", '\x00a2');
            entities.Add("pound", '\x00a3');
            entities.Add("curren", '\x00a4');
            entities.Add("yen", '\x00a5');
            entities.Add("brvbar", '\x00a6');
            entities.Add("sect", '\x00a7');
            entities.Add("uml", '\x00a8');
            entities.Add("copy", '\x00a9');
            entities.Add("ordf", '\x00aa');
            entities.Add("laquo", '\x00ab');
            entities.Add("not", '\x00ac');
            entities.Add("shy", '\x00ad');
            entities.Add("reg", '\x00ae');
            entities.Add("macr", '\x00af');
            entities.Add("deg", '\x00b0');
            entities.Add("plusmn", '\x00b1');
            entities.Add("sup2", '\x00b2');
            entities.Add("sup3", '\x00b3');
            entities.Add("acute", '\x00b4');
            entities.Add("micro", '\x00b5');
            entities.Add("para", '\x00b6');
            entities.Add("middot", '\x00b7');
            entities.Add("cedil", '\x00b8');
            entities.Add("sup1", '\x00b9');
            entities.Add("ordm", '\x00ba');
            entities.Add("raquo", '\x00bb');
            entities.Add("frac14", '\x00bc');
            entities.Add("frac12", '\x00bd');
            entities.Add("frac34", '\x00be');
            entities.Add("iquest", '\x00bf');
            entities.Add("Agrave", '\x00c0');
            entities.Add("Aacute", '\x00c1');
            entities.Add("Acirc", '\x00c2');
            entities.Add("Atilde", '\x00c3');
            entities.Add("Auml", '\x00c4');
            entities.Add("Aring", '\x00c5');
            entities.Add("AElig", '\x00c6');
            entities.Add("Ccedil", '\x00c7');
            entities.Add("Egrave", '\x00c8');
            entities.Add("Eacute", '\x00c9');
            entities.Add("Ecirc", '\x00ca');
            entities.Add("Euml", '\x00cb');
            entities.Add("Igrave", '\x00cc');
            entities.Add("Iacute", '\x00cd');
            entities.Add("Icirc", '\x00ce');
            entities.Add("Iuml", '\x00cf');
            entities.Add("ETH", '\x00d0');
            entities.Add("Ntilde", '\x00d1');
            entities.Add("Ograve", '\x00d2');
            entities.Add("Oacute", '\x00d3');
            entities.Add("Ocirc", '\x00d4');
            entities.Add("Otilde", '\x00d5');
            entities.Add("Ouml", '\x00d6');
            entities.Add("times", '\x00d7');
            entities.Add("Oslash", '\x00d8');
            entities.Add("Ugrave", '\x00d9');
            entities.Add("Uacute", '\x00da');
            entities.Add("Ucirc", '\x00db');
            entities.Add("Uuml", '\x00dc');
            entities.Add("Yacute", '\x00dd');
            entities.Add("THORN", '\x00de');
            entities.Add("szlig", '\x00df');
            entities.Add("agrave", '\x00e0');
            entities.Add("aacute", '\x00e1');
            entities.Add("acirc", '\x00e2');
            entities.Add("atilde", '\x00e3');
            entities.Add("auml", '\x00e4');
            entities.Add("aring", '\x00e5');
            entities.Add("aelig", '\x00e6');
            entities.Add("ccedil", '\x00e7');
            entities.Add("egrave", '\x00e8');
            entities.Add("eacute", '\x00e9');
            entities.Add("ecirc", '\x00ea');
            entities.Add("euml", '\x00eb');
            entities.Add("igrave", '\x00ec');
            entities.Add("iacute", '\x00ed');
            entities.Add("icirc", '\x00ee');
            entities.Add("iuml", '\x00ef');
            entities.Add("eth", '\x00f0');
            entities.Add("ntilde", '\x00f1');
            entities.Add("ograve", '\x00f2');
            entities.Add("oacute", '\x00f3');
            entities.Add("ocirc", '\x00f4');
            entities.Add("otilde", '\x00f5');
            entities.Add("ouml", '\x00f6');
            entities.Add("divide", '\x00f7');
            entities.Add("oslash", '\x00f8');
            entities.Add("ugrave", '\x00f9');
            entities.Add("uacute", '\x00fa');
            entities.Add("ucirc", '\x00fb');
            entities.Add("uuml", '\x00fc');
            entities.Add("yacute", '\x00fd');
            entities.Add("thorn", '\x00fe');
            entities.Add("yuml", '\x00ff');
            entities.Add("fnof", 'ƒ');
            entities.Add("Alpha", 'Α');
            entities.Add("Beta", 'Β');
            entities.Add("Gamma", 'Γ');
            entities.Add("Delta", 'Δ');
            entities.Add("Epsilon", 'Ε');
            entities.Add("Zeta", 'Ζ');
            entities.Add("Eta", 'Η');
            entities.Add("Theta", 'Θ');
            entities.Add("Iota", 'Ι');
            entities.Add("Kappa", 'Κ');
            entities.Add("Lambda", 'Λ');
            entities.Add("Mu", 'Μ');
            entities.Add("Nu", 'Ν');
            entities.Add("Xi", 'Ξ');
            entities.Add("Omicron", 'Ο');
            entities.Add("Pi", 'Π');
            entities.Add("Rho", 'Ρ');
            entities.Add("Sigma", 'Σ');
            entities.Add("Tau", 'Τ');
            entities.Add("Upsilon", 'Υ');
            entities.Add("Phi", 'Φ');
            entities.Add("Chi", 'Χ');
            entities.Add("Psi", 'Ψ');
            entities.Add("Omega", 'Ω');
            entities.Add("alpha", 'α');
            entities.Add("beta", 'β');
            entities.Add("gamma", 'γ');
            entities.Add("delta", 'δ');
            entities.Add("epsilon", 'ε');
            entities.Add("zeta", 'ζ');
            entities.Add("eta", 'η');
            entities.Add("theta", 'θ');
            entities.Add("iota", 'ι');
            entities.Add("kappa", 'κ');
            entities.Add("lambda", 'λ');
            entities.Add("mu", 'μ');
            entities.Add("nu", 'ν');
            entities.Add("xi", 'ξ');
            entities.Add("omicron", 'ο');
            entities.Add("pi", 'π');
            entities.Add("rho", 'ρ');
            entities.Add("sigmaf", 'ς');
            entities.Add("sigma", 'σ');
            entities.Add("tau", 'τ');
            entities.Add("upsilon", 'υ');
            entities.Add("phi", 'φ');
            entities.Add("chi", 'χ');
            entities.Add("psi", 'ψ');
            entities.Add("omega", 'ω');
            entities.Add("thetasym", 'ϑ');
            entities.Add("upsih", 'ϒ');
            entities.Add("piv", 'ϖ');
            entities.Add("bull", '•');
            entities.Add("hellip", '…');
            entities.Add("prime", '′');
            entities.Add("Prime", '″');
            entities.Add("oline", '‾');
            entities.Add("frasl", '⁄');
            entities.Add("weierp", '℘');
            entities.Add("image", 'ℑ');
            entities.Add("real", 'ℜ');
            entities.Add("trade", '™');
            entities.Add("alefsym", 'ℵ');
            entities.Add("larr", '←');
            entities.Add("uarr", '↑');
            entities.Add("rarr", '→');
            entities.Add("darr", '↓');
            entities.Add("harr", '↔');
            entities.Add("crarr", '↵');
            entities.Add("lArr", '⇐');
            entities.Add("uArr", '⇑');
            entities.Add("rArr", '⇒');
            entities.Add("dArr", '⇓');
            entities.Add("hArr", '⇔');
            entities.Add("forall", '∀');
            entities.Add("part", '∂');
            entities.Add("exist", '∃');
            entities.Add("empty", '∅');
            entities.Add("nabla", '∇');
            entities.Add("isin", '∈');
            entities.Add("notin", '∉');
            entities.Add("ni", '∋');
            entities.Add("prod", '∏');
            entities.Add("sum", '∑');
            entities.Add("minus", '−');
            entities.Add("lowast", '∗');
            entities.Add("radic", '√');
            entities.Add("prop", '∝');
            entities.Add("infin", '∞');
            entities.Add("ang", '∠');
            entities.Add("and", '∧');
            entities.Add("or", '∨');
            entities.Add("cap", '∩');
            entities.Add("cup", '∪');
            entities.Add("int", '∫');
            entities.Add("there4", '∴');
            entities.Add("sim", '∼');
            entities.Add("cong", '≅');
            entities.Add("asymp", '≈');
            entities.Add("ne", '≠');
            entities.Add("equiv", '≡');
            entities.Add("le", '≤');
            entities.Add("ge", '≥');
            entities.Add("sub", '⊂');
            entities.Add("sup", '⊃');
            entities.Add("nsub", '⊄');
            entities.Add("sube", '⊆');
            entities.Add("supe", '⊇');
            entities.Add("oplus", '⊕');
            entities.Add("otimes", '⊗');
            entities.Add("perp", '⊥');
            entities.Add("sdot", '⋅');
            entities.Add("lceil", '⌈');
            entities.Add("rceil", '⌉');
            entities.Add("lfloor", '⌊');
            entities.Add("rfloor", '⌋');
            entities.Add("lang", '〈');
            entities.Add("rang", '〉');
            entities.Add("loz", '◊');
            entities.Add("spades", '♠');
            entities.Add("clubs", '♣');
            entities.Add("hearts", '♥');
            entities.Add("diams", '♦');
            entities.Add("quot", '"');
            entities.Add("amp", '&');
            entities.Add("lt", '<');
            entities.Add("gt", '>');
            entities.Add("OElig", 'Œ');
            entities.Add("oelig", 'œ');
            entities.Add("Scaron", 'Š');
            entities.Add("scaron", 'š');
            entities.Add("Yuml", 'Ÿ');
            entities.Add("circ", 'ˆ');
            entities.Add("tilde", '˜');
            entities.Add("ensp", ' ');
            entities.Add("emsp", ' ');
            entities.Add("thinsp", ' ');
            entities.Add("zwnj", '‌');
            entities.Add("zwj", '‍');
            entities.Add("lrm", '‎');
            entities.Add("rlm", '‏');
            entities.Add("ndash", '–');
            entities.Add("mdash", '—');
            entities.Add("lsquo", '‘');
            entities.Add("rsquo", '’');
            entities.Add("sbquo", '‚');
            entities.Add("ldquo", '“');
            entities.Add("rdquo", '”');
            entities.Add("bdquo", '„');
            entities.Add("dagger", '†');
            entities.Add("Dagger", '‡');
            entities.Add("permil", '‰');
            entities.Add("lsaquo", '‹');
            entities.Add("rsaquo", '›');
            entities.Add("euro", '€');
        }

        private static bool NotEncoded(char c)
        {
            return (((((c == '!') || (c == '\'')) || ((c == '(') || (c == ')'))) || (((c == '*') || (c == '-')) || (c == '.'))) || (c == '_'));
        }

        public static NameValueCollection ParseQueryString(string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }

        public static NameValueCollection ParseQueryString(string query, Encoding encoding)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if ((query.Length == 0) || ((query.Length == 1) && (query[0] == '?')))
            {
                return new NameValueCollection();
            }
            if (query[0] == '?')
            {
                query = query.Substring(1);
            }
            NameValueCollection result = new HttpQSCollection();
            ParseQueryString(query, encoding, result);
            return result;
        }

        internal static void ParseQueryString(string query, Encoding encoding, NameValueCollection result)
        {
            if (query.Length != 0)
            {
                string str = HtmlDecode(query);
                int length = str.Length;
                int startIndex = 0;
                bool flag = true;
                while (startIndex <= length)
                {
                    string str2;
                    int num3 = -1;
                    int num4 = -1;
                    for (int i = startIndex; i < length; i++)
                    {
                        if ((num3 == -1) && (str[i] == '='))
                        {
                            num3 = i + 1;
                        }
                        else if (str[i] == '&')
                        {
                            num4 = i;
                            break;
                        }
                    }
                    if (flag)
                    {
                        flag = false;
                        if (str[startIndex] == '?')
                        {
                            startIndex++;
                        }
                    }
                    if (num3 == -1)
                    {
                        str2 = null;
                        num3 = startIndex;
                    }
                    else
                    {
                        str2 = UrlDecode(str.Substring(startIndex, (num3 - startIndex) - 1), encoding);
                    }
                    if (num4 < 0)
                    {
                        startIndex = -1;
                        num4 = str.Length;
                    }
                    else
                    {
                        startIndex = num4 + 1;
                    }
                    string val = UrlDecode(str.Substring(num3, num4 - num3), encoding);
                    result.Add(str2, val);
                    if (startIndex == -1)
                    {
                        break;
                    }
                }
            }
        }

        public static string UrlDecode(string str)
        {
            return UrlDecode(str, Encoding.UTF8);
        }

        public static string UrlDecode(byte[] bytes, Encoding e)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlDecode(bytes, 0, bytes.Length, e);
        }

        public static string UrlDecode(string s, Encoding e)
        {
            if (s == null)
            {
                return null;
            }
            if ((s.IndexOf('%') == -1) && (s.IndexOf('+') == -1))
            {
                return s;
            }
            if (e == null)
            {
                e = Encoding.UTF8;
            }
            long length = s.Length;
            List<byte> buf = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                if (((ch == '%') && ((i + 2) < length)) && (s[i + 1] != '%'))
                {
                    int num2;
                    if ((s[i + 1] == 'u') && ((i + 5) < length))
                    {
                        num2 = GetChar(s, i + 2, 4);
                        if (num2 != -1)
                        {
                            WriteCharBytes(buf, (char) num2, e);
                            i += 5;
                        }
                        else
                        {
                            WriteCharBytes(buf, '%', e);
                        }
                    }
                    else
                    {
                        num2 = GetChar(s, i + 1, 2);
                        if (num2 != -1)
                        {
                            WriteCharBytes(buf, (char) num2, e);
                            i += 2;
                        }
                        else
                        {
                            WriteCharBytes(buf, '%', e);
                        }
                    }
                }
                else if (ch == '+')
                {
                    WriteCharBytes(buf, ' ', e);
                }
                else
                {
                    WriteCharBytes(buf, ch, e);
                }
            }
            byte[] bytes = buf.ToArray();
            buf = null;
            return e.GetString(bytes);
        }

        public static string UrlDecode(byte[] bytes, int offset, int count, Encoding e)
        {
            if (bytes == null)
            {
                return null;
            }
            if (count == 0)
            {
                return string.Empty;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if ((offset < 0) || (offset > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || ((offset + count) > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            StringBuilder builder = new StringBuilder();
            MemoryStream b = new MemoryStream();
            int num = count + offset;
            for (int i = offset; i < num; i++)
            {
                if (((bytes[i] == 0x25) && ((i + 2) < count)) && (bytes[i + 1] != 0x25))
                {
                    int num2;
                    if ((bytes[i + 1] == 0x75) && ((i + 5) < num))
                    {
                        if (b.Length > 0L)
                        {
                            builder.Append(GetChars(b, e));
                            b.SetLength(0L);
                        }
                        num2 = GetChar(bytes, i + 2, 4);
                        if (num2 == -1)
                        {
                            goto Label_0123;
                        }
                        builder.Append((char) num2);
                        i += 5;
                        continue;
                    }
                    num2 = GetChar(bytes, i + 1, 2);
                    if (num2 != -1)
                    {
                        b.WriteByte((byte) num2);
                        i += 2;
                        continue;
                    }
                }
            Label_0123:
                if (b.Length > 0L)
                {
                    builder.Append(GetChars(b, e));
                    b.SetLength(0L);
                }
                if (bytes[i] == 0x2b)
                {
                    builder.Append(' ');
                }
                else
                {
                    builder.Append((char) bytes[i]);
                }
            }
            if (b.Length > 0L)
            {
                builder.Append(GetChars(b, e));
            }
            b = null;
            return builder.ToString();
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlDecodeToBytes(bytes, 0, bytes.Length);
        }

        public static byte[] UrlDecodeToBytes(string str)
        {
            return UrlDecodeToBytes(str, Encoding.UTF8);
        }

        public static byte[] UrlDecodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            return UrlDecodeToBytes(e.GetBytes(str));
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                return null;
            }
            if (count == 0)
            {
                return new byte[0];
            }
            int length = bytes.Length;
            if ((offset < 0) || (offset >= length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (offset > (length - count)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            MemoryStream stream = new MemoryStream();
            int num2 = offset + count;
            for (int i = offset; i < num2; i++)
            {
                char ch = (char) bytes[i];
                if (ch == '+')
                {
                    ch = ' ';
                }
                else if ((ch == '%') && (i < (num2 - 2)))
                {
                    int num4 = GetChar(bytes, i + 1, 2);
                    if (num4 != -1)
                    {
                        ch = (char) num4;
                        i += 2;
                    }
                }
                stream.WriteByte((byte) ch);
            }
            return stream.ToArray();
        }

        public static string UrlEncode(string str)
        {
            return UrlEncode(str, Encoding.UTF8);
        }

        public static string UrlEncode(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            if (bytes.Length == 0)
            {
                return string.Empty;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, 0, bytes.Length));
        }

        public static string UrlEncode(string s, Encoding Enc)
        {
            if (s == null)
            {
                return null;
            }
            if (s == string.Empty)
            {
                return string.Empty;
            }
            bool flag = false;
            int length = s.Length;
            for (int i = 0; i < length; i++)
            {
                char c = s[i];
                if (((((c < '0') || ((c < 'A') && (c > '9'))) || ((c > 'Z') && (c < 'a'))) || (c > 'z')) && !NotEncoded(c))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return s;
            }
            byte[] bytes = new byte[Enc.GetMaxByteCount(s.Length)];
            int count = Enc.GetBytes(s, 0, s.Length, bytes, 0);
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, 0, count));
        }

        public static string UrlEncode(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                return null;
            }
            if (bytes.Length == 0)
            {
                return string.Empty;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, offset, count));
        }

        private static void UrlEncodeChar(char c, Stream result, bool isUnicode)
        {
            if (c > '\x00ff')
            {
                int num2 = c;
                result.WriteByte(0x25);
                result.WriteByte(0x75);
                int index = num2 >> 12;
                result.WriteByte((byte) hexChars[index]);
                index = (num2 >> 8) & 15;
                result.WriteByte((byte) hexChars[index]);
                index = (num2 >> 4) & 15;
                result.WriteByte((byte) hexChars[index]);
                index = num2 & 15;
                result.WriteByte((byte) hexChars[index]);
            }
            else if ((c > ' ') && NotEncoded(c))
            {
                result.WriteByte((byte) c);
            }
            else if (c == ' ')
            {
                result.WriteByte(0x2b);
            }
            else if ((((c < '0') || ((c < 'A') && (c > '9'))) || ((c > 'Z') && (c < 'a'))) || (c > 'z'))
            {
                if (isUnicode && (c > '\x007f'))
                {
                    result.WriteByte(0x25);
                    result.WriteByte(0x75);
                    result.WriteByte(0x30);
                    result.WriteByte(0x30);
                }
                else
                {
                    result.WriteByte(0x25);
                }
                int num3 = c >> 4;
                result.WriteByte((byte) hexChars[num3]);
                num3 = c & '\x000f';
                result.WriteByte((byte) hexChars[num3]);
            }
            else
            {
                result.WriteByte((byte) c);
            }
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            if (bytes.Length == 0)
            {
                return new byte[0];
            }
            return UrlEncodeToBytes(bytes, 0, bytes.Length);
        }

        public static byte[] UrlEncodeToBytes(string str)
        {
            return UrlEncodeToBytes(str, Encoding.UTF8);
        }

        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            if (str == string.Empty)
            {
                return new byte[0];
            }
            byte[] bytes = e.GetBytes(str);
            return UrlEncodeToBytes(bytes, 0, bytes.Length);
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                return null;
            }
            int length = bytes.Length;
            if (length == 0)
            {
                return new byte[0];
            }
            if ((offset < 0) || (offset >= length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (count > (length - offset)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            MemoryStream result = new MemoryStream(count);
            int num2 = offset + count;
            for (int i = offset; i < num2; i++)
            {
                UrlEncodeChar((char) bytes[i], result, false);
            }
            return result.ToArray();
        }

        public static string UrlEncodeUnicode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeUnicodeToBytes(str));
        }

        public static byte[] UrlEncodeUnicodeToBytes(string str)
        {
            if (str == null)
            {
                return null;
            }
            if (str == string.Empty)
            {
                return new byte[0];
            }
            MemoryStream result = new MemoryStream(str.Length);
            foreach (char ch in str)
            {
                UrlEncodeChar(ch, result, true);
            }
            return result.ToArray();
        }

        public static string UrlPathEncode(string s)
        {
            if ((s == null) || (s.Length == 0))
            {
                return s;
            }
            MemoryStream result = new MemoryStream();
            int length = s.Length;
            for (int i = 0; i < length; i++)
            {
                UrlPathEncodeChar(s[i], result);
            }
            return Encoding.ASCII.GetString(result.ToArray());
        }

        private static void UrlPathEncodeChar(char c, Stream result)
        {
            if ((c < '!') || (c > '~'))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(c.ToString());
                for (int i = 0; i < bytes.Length; i++)
                {
                    result.WriteByte(0x25);
                    int index = bytes[i] >> 4;
                    result.WriteByte((byte) hexChars[index]);
                    index = bytes[i] & 15;
                    result.WriteByte((byte) hexChars[index]);
                }
            }
            else if (c == ' ')
            {
                result.WriteByte(0x25);
                result.WriteByte(50);
                result.WriteByte(0x30);
            }
            else
            {
                result.WriteByte((byte) c);
            }
        }

        private static void WriteCharBytes(IList buf, char ch, Encoding e)
        {
            if (ch > '\x00ff')
            {
                char[] chars = new char[] { ch };
                foreach (byte num2 in e.GetBytes(chars))
                {
                    buf.Add(num2);
                }
            }
            else
            {
                buf.Add((byte) ch);
            }
        }

        private static Hashtable Entities
        {
            get
            {
                object obj2 = lock_;
                lock (obj2)
                {
                    if (entities == null)
                    {
                        InitEntities();
                    }
                    return entities;
                }
            }
        }

        private sealed class HttpQSCollection : NameValueCollection
        {
            public override string ToString()
            {
                int count = this.Count;
                if (count == 0)
                {
                    return string.Empty;
                }
                StringBuilder builder = new StringBuilder();
                string[] allKeys = this.AllKeys;
                for (int i = 0; i < count; i++)
                {
                    builder.AppendFormat("{0}={1}&", allKeys[i], base[allKeys[i]]);
                }
                if (builder.Length > 0)
                {
                    builder.Length--;
                }
                return builder.ToString();
            }
        }
    }
}

