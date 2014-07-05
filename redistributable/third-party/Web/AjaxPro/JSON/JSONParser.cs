/*
 * JSONParser.cs
 * 
 * Copyright © 2007 Michael Schwarz (http://www.ajaxpro.info).
 * All Rights Reserved.
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without 
 * restriction, including without limitation the rights to use, 
 * copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
/*
 * MS	06-05-30	added \u0000 support for unicode characters
 * MS   06-07-09    changed "new ..." string to JavaScriptSource
 * 
 * 
 */
using System;
using System.Reflection;
using System.Data;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Text;

namespace AjaxPro
{
	/// <summary>
	/// Represents a parser for JSON strings.
	/// </summary>
	public sealed class JSONParser
	{
		/// <summary>
		/// Initialize a new instance of JSONParser.
		/// </summary>
		internal JSONParser()
		{
		}

		#region Constant Variables

		public const char JSON_OBJECT_BEGIN = '{';
		public const char JSON_OBJECT_END = '}';
		public const char JSON_ARRAY_BEGIN = '[';
		public const char JSON_ARRAY_END = ']';
		public const char JSON_PROPERTY_SEPARATOR = ':';
		public const char JSON_STRING_SINGLE_QUOTE = '\'';
		public const char JSON_STRING_DOUBLE_QUOTE = '"';
		public const char JSON_ITEMS_SEPARATOR = ',';
		public const char JSON_DECIMAL_SEPARATOR = '.';

		public const char END_OF_STRING = '\0';
		public const char NEW_LINE = '\n';
		public const char RETURN = '\r';

		#endregion

		#region Private Variables

		private int _idx = 0;
		private string _json = null;
		private char _ch = ' ';

		#endregion

		#region Read Methods

		/// <summary>
		/// Read on the JSON string until first non-whitespace character.
		/// </summary>
		internal void ReadWhiteSpaces()
		{
			while (_ch != END_OF_STRING && _ch <= ' ')
				ReadNext();
		}

		/// <summary>
		/// Read the next character from the JSON string and store it in the private variable ch.
		/// </summary>
		/// <returns>Returns false if at end of JSON string.</returns>
		internal bool ReadNext()
		{
			if (_idx >= _json.Length)
			{
				_ch = END_OF_STRING;
				return false;
			}

			_ch = _json[_idx];
			_idx++;

			return true;
		}

		internal bool CompareNext(string s)
		{
			if (_idx + s.Length > _json.Length)
				return false;

			if (_json.Substring(_idx, s.Length) == s)
				return true;

			return false;
		}

        /// <summary>
        /// Read the previous character from the JSON string and store it in the private variable ch.
        /// </summary>
        /// <returns>
        /// Returns false if at the beginning of the JSON string.
        /// </returns>
		internal bool ReadPrev()
		{
			if (_idx <= 0)
				return false;

			_idx--;
			_ch = _json[_idx];

			return true;
		}

		#endregion

		#region Read JSON Methods

        /// <summary>
        /// Read a string object from the JSON string.
        /// </summary>
        /// <returns>Returns the string.</returns>
		internal JavaScriptString ReadString()
		{
			JavaScriptString s = new JavaScriptString();

			if (_ch == JSON_STRING_DOUBLE_QUOTE)
			{
				while (ReadNext())
				{
					if (_ch == JSON_STRING_DOUBLE_QUOTE)
					{
						ReadNext();
						return s;
					}
					else if (_ch == '\\')
					{
						ReadNext();
						switch (_ch)
						{
							case 'n': s += '\n'; break;
							case 'r': s += '\r'; break;
							case 'b': s += '\b'; break;
							case 'f': s += '\f'; break;
							case 't': s += '\t'; break;
							case '\\': s += '\\'; break;

							case 'u':
								string u = "";
								for (int i = 0; i < 4; i++)
								{
									// TODO: add more checks if correct format \\u0000
									ReadNext();
									u += _ch;
								}

								s += (char)((ushort)int.Parse(u, NumberStyles.HexNumber, CultureInfo.InvariantCulture));

								break;

							default:
								s += _ch;
								break;
						}
					}
					else
					{
						s += _ch;
					}
				}
			}
			else
			{
				throw new NotSupportedException("The string could not be read.");
			}

			return s;
		}

        /// <summary>
        /// Reads the java script source.
        /// </summary>
        /// <returns></returns>
		internal JavaScriptSource ReadJavaScriptSource()
		{
			JavaScriptSource s = new JavaScriptSource();

			s.Append(ReadJavaScriptObject().ToString());

			return s;
		}

        /// <summary>
        /// Reads the java script object.
        /// </summary>
        /// <returns></returns>
		internal JavaScriptString ReadJavaScriptObject()
		{
			JavaScriptString n = new JavaScriptString();

			int b = 0;
			bool bf = false;

			while (_ch != END_OF_STRING)
			{
				if (_ch == '(')
				{
					b++;
					bf = true;
				}
				else
					if (_ch == ')') b--;

				if (bf)
				{

				}

				n += _ch;

				ReadNext();

				if (bf && b == 0)
					break;

			}

			return n;
		}

        /// <summary>
        /// Read a number object from the JSON string.
        /// </summary>
        /// <returns>Returns the number.</returns>
		internal JavaScriptNumber ReadNumber()
		{
			JavaScriptNumber n = new JavaScriptNumber();

			if (_ch == '-')		// negative numbers
			{
				n += "-";

				ReadNext();
			}

			// Searching for all numbers until the first character that is not 
			// a number.

			while (_ch >= '0' && _ch <= '9' && _ch != END_OF_STRING)		// all numbers between 0..9
			{
				n += _ch;

				ReadNext();
			}

			// In JavaScript (JSON) the decimal separator is always a point. If we
			// have a decimal number we read all the numbers after the separator.

			if (_ch == '.')
			{
				n += '.';
				ReadNext();

				while (_ch >= '0' && _ch <= '9' && _ch != END_OF_STRING)
				{
					n += _ch;
					ReadNext();
				}
			}

			if (_ch == 'e' || _ch == 'E')
			{
				n += 'e';
				ReadNext();

				if (_ch == '-' || _ch == '+')
				{
					n += _ch;
					ReadNext();
				}

				while (_ch >= '0' && _ch <= '9' && _ch != END_OF_STRING)
				{
					n += _ch;
					ReadNext();
				}
			}

			return n;
		}

        /// <summary>
        /// Read a word object from the JSON string.
        /// </summary>
        /// <returns>Returns the word.</returns>
		internal IJavaScriptObject ReadWord()
		{
			switch (_ch)
			{
				case 't':
					if (CompareNext("rue") == true)
					{
						ReadNext(); ReadNext(); ReadNext(); ReadNext();

						return new JavaScriptBoolean(true);
					}
					break;

				case 'f':
					if (CompareNext("alse") == true)
					{
						ReadNext(); ReadNext(); ReadNext(); ReadNext(); ReadNext();

						return new JavaScriptBoolean(false);
					}
					break;

				case 'n':
					if (CompareNext("ull") == true)
					{
						ReadNext(); ReadNext(); ReadNext(); ReadNext();
						return null;
					}
					else if (CompareNext("ew ") == true)
					{
						return ReadJavaScriptSource();
					}

					break;
			}

			throw new NotSupportedException("word " + _ch);
		}

        /// <summary>
        /// Read an array object from the JSON string.
        /// </summary>
        /// <returns>Returns an ArrayList with all objects.</returns>
		internal JavaScriptArray ReadArray()
		{
			JavaScriptArray a = new JavaScriptArray();

			if (_ch == JSON_ARRAY_BEGIN)
			{
				ReadNext();
				ReadWhiteSpaces();

				if (_ch == JSON_ARRAY_END)
				{
					ReadNext();
					return a;
				}

				while (_ch != END_OF_STRING)
				{
					a.Add(GetObject());
					ReadWhiteSpaces();

					if (_ch == JSON_ARRAY_END)
					{
						ReadNext();
						return a;
					}
					else if (_ch != JSON_ITEMS_SEPARATOR)
					{
						break;
					}
					ReadNext();
					ReadWhiteSpaces();
				}
			}
			else
			{
				throw new NotSupportedException("Array could not be read.");
			}

			return a;
		}

        /// <summary>
        /// Reads the next object from the JSON string.
        /// </summary>
        /// <returns>
        /// Returns an Hashtable with all properties.
        /// </returns>
		internal JavaScriptObject ReadObject()
		{
			JavaScriptObject h = new JavaScriptObject();
			string k;

			if (_ch == JSON_OBJECT_BEGIN)
			{
				ReadNext();
				ReadWhiteSpaces();

				if (_ch == JSON_OBJECT_END)
				{
					ReadNext();
					return h;
				}

				while (_ch != END_OF_STRING)
				{
					k = ReadString();
					ReadWhiteSpaces();

					if (_ch != JSON_PROPERTY_SEPARATOR)
					{
						break;
					}

					ReadNext();

					h.Add(k, GetObject());

					ReadWhiteSpaces();

					if (_ch == JSON_OBJECT_END)
					{
						ReadNext();
						return h;
					}
					else if (_ch != JSON_ITEMS_SEPARATOR)
					{
						break;
					}

					ReadNext();
					ReadWhiteSpaces();
				}
			}

			throw new NotSupportedException("obj");
		}

		#endregion

		#region JSON string and JSON object

        /// <summary>
        /// Returns a JSON object using Hashtable, ArrayList or string.
        /// </summary>
        /// <returns></returns>
		internal IJavaScriptObject GetObject()
		{
			if (_json == null)
				throw new Exception("Missing json string.");

			ReadWhiteSpaces();

			switch (_ch)
			{
				case JSON_OBJECT_BEGIN: return ReadObject();
				case JSON_ARRAY_BEGIN: return ReadArray();
				case JSON_STRING_DOUBLE_QUOTE: return ReadString();
				case '-': return ReadNumber();
				default:
					return _ch >= '0' && _ch <= '9' ? ReadNumber() : ReadWord();
			}
		}

		#endregion

        /// <summary>
        /// Reads the object that represents the JSON string.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>Returns an object.</returns>
		public IJavaScriptObject GetJSONObject(string json)
		{
			_json = json;
			_idx = 0;
			_ch = ' ';

			return GetObject();
		}
	}
}