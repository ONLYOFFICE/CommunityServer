using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents an instance of the JavaScript string object.
    /// </summary>
    [Serializable]
    public class StringInstance : ObjectInstance
    {
        private string value;



        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new empty string instance.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        public StringInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.value = string.Empty;
            this.FastSetProperty("length", 0);
        }

        /// <summary>
        /// Creates a new string instance.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="value"> The value to initialize the instance. </param>
        public StringInstance(ObjectInstance prototype, string value)
            : base(prototype)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            this.value = value;
            this.FastSetProperty("length", value.Length);
        }



        //     .NET ACCESSOR PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the internal class name of the object.  Used by the default toString()
        /// implementation.
        /// </summary>
        protected override string InternalClassName
        {
            get { return "String"; }
        }

        /// <summary>
        /// Gets the primitive value of this object.
        /// </summary>
        public string Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// Gets the number of characters in the string.
        /// </summary>
        public int Length
        {
            get { return this.value.Length; }
        }



        //     OVERRIDES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets a descriptor for the property with the given array index.
        /// </summary>
        /// <param name="propertyName"> The array index of the property. </param>
        /// <returns> A property descriptor containing the property value and attributes. </returns>
        /// <remarks> The prototype chain is not searched. </remarks>
        public override PropertyDescriptor GetOwnPropertyDescriptor(uint index)
        {
            if (index < this.value.Length)
            {
                var result = this.value[(int)index].ToString();
                return new PropertyDescriptor(result, PropertyAttributes.Enumerable);
            }

            // Delegate to the base class.
            return base.GetOwnPropertyDescriptor(index);
        }

        /// <summary>
        /// Gets an enumerable list of every property name and value associated with this object.
        /// </summary>
        public override IEnumerable<PropertyNameAndValue> Properties
        {
            get
            {
                // Enumerate array indices.
                for (int i = 0; i < this.value.Length; i++)
                    yield return new PropertyNameAndValue(i.ToString(),
                        new PropertyDescriptor(this.value[i].ToString(), PropertyAttributes.Enumerable));

                // Delegate to the base implementation.
                foreach (var nameAndValue in base.Properties)
                    yield return nameAndValue;
            }
        }


        
        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Returns the character at the specified index.
        /// </summary>
        /// <param name="index"> The character position (starts at 0). </param>
        /// <returns></returns>
        [JSInternalFunction(Name = "charAt", Flags = JSFunctionFlags.HasThisObject)]
        public static string CharAt(string thisObject, int index)
        {
            if (index < 0 || index >= thisObject.Length)
                return string.Empty;
            return thisObject[index].ToString();
        }

        /// <summary>
        /// Returns a number indicating the Unicode value of the character at the given index.
        /// </summary>
        /// <param name="index"> The character position (starts at 0). </param>
        /// <returns></returns>
        [JSInternalFunction(Name = "charCodeAt", Flags = JSFunctionFlags.HasThisObject)]
        public static double CharCodeAt(string thisObject, int index)
        {
            if (index < 0 || index >= thisObject.Length)
                return double.NaN;
            return (double)(int)thisObject[index];
        }

        /// <summary>
        /// Combines the text of two or more strings and returns a new string.
        /// </summary>
        /// <param name="strings"> The strings to concatenate with this string. </param>
        /// <returns> The result of combining this string with the given strings. </returns>
        [JSInternalFunction(Name = "concat", Flags = JSFunctionFlags.HasEngineParameter | JSFunctionFlags.HasThisObject)]
        public static ConcatenatedString Concat(ScriptEngine engine, object thisObject, params object[] strings)
        {
            if (thisObject is ConcatenatedString)
            {
                // Append the strings together.
                ConcatenatedString result = (ConcatenatedString)thisObject;
                if (strings.Length == 0)
                    return result;
                result = result.Concatenate(strings[0]);
                for (int i = 1; i < strings.Length; i ++)
                    result.Append(strings[i]);
                return result;
            }
            else
            {
                // Convert "this" to a string.
                TypeUtilities.VerifyThisObject(engine, thisObject, "concat");
                var thisObject2 = TypeConverter.ToString(thisObject);

                // Append the strings together.
                var result = new ConcatenatedString(thisObject2);
                foreach (object str in strings)
                    result.Append(str);
                return result;
            }
        }

        

        

        /// <summary>
        /// Returns the index within the calling String object of the first occurrence of the specified value, or -1 if not found.
        /// </summary>
        /// <param name="substring"> The substring to search for. </param>
        /// <param name="startIndex"> The character position to start searching from.  Defaults to 0. </param>
        /// <returns> The character position of the start of the substring, if it was found, or -1 if it wasn't. </returns>
        [JSInternalFunction(Name = "indexOf", Flags = JSFunctionFlags.HasThisObject, Length = 1)]
        public static int IndexOf(string thisObject, string substring, [DefaultParameterValue(0)] int startIndex = 0)
        {
            startIndex = Math.Min(Math.Max(startIndex, 0), thisObject.Length);
            return thisObject.IndexOf(substring, startIndex, StringComparison.Ordinal);
        }

        

        /// <summary>
        /// Returns the index within the calling String object of the specified value, searching
        /// backwards from the end of the string.
        /// </summary>
        /// <param name="substring"> The substring to search for. </param>
        /// <param name="startIndex"> The index of the character to start searching. </param>
        /// <returns> The index of the substring, or <c>-1</c> if not found. </returns>
        [JSInternalFunction(Name = "lastIndexOf", Flags = JSFunctionFlags.HasThisObject, Length = 1)]
        public static int LastIndexOf(string thisObject, string substring, [DefaultParameterValue(double.NaN)] double startIndex = double.NaN)
        {
            // Limit startIndex to the length of the string.  This must be done first otherwise
            // when startIndex = MaxValue it wraps around to negative.
            int startIndex2 = double.IsNaN(startIndex) ? int.MaxValue : TypeConverter.ToInteger(startIndex);
            startIndex2 = Math.Min(startIndex2, thisObject.Length - 1);
            startIndex2 = Math.Min(startIndex2 + substring.Length - 1, thisObject.Length - 1);
            if (startIndex2 < 0)
            {
                if (thisObject == string.Empty && substring == string.Empty)
                    return 0;
                return -1;
            }
            return thisObject.LastIndexOf(substring, startIndex2, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a number indicating whether a reference string comes before or after or is the
        /// same as the given string in sort order.
        /// </summary>
        /// <param name="str"> The string to compare with. </param>
        /// <returns> -1, 0 or 1 depending on whether the given string comes before or after or is
        /// the same as the given string in sort order. </returns>
        [JSInternalFunction(Name = "localeCompare", Flags = JSFunctionFlags.HasThisObject)]
        public static int LocaleCompare(string thisObject, string str)
        {
            return string.Compare(thisObject, str);
        }

        /// <summary>
        /// Finds the first match of the given substring within this string.
        /// </summary>
        /// <param name="substr"> The substring to search for. </param>
        /// <returns> An array containing the matched strings. </returns>
        [JSInternalFunction(Name = "match", Flags = JSFunctionFlags.HasEngineParameter | JSFunctionFlags.HasThisObject)]
        public static object Match(ScriptEngine engine, string thisObject, object substrOrRegExp)
        {
            if (substrOrRegExp is RegExpInstance)
                // substrOrRegExp is a regular expression.
                return ((RegExpInstance)substrOrRegExp).Match(thisObject);

            if (TypeUtilities.IsUndefined(substrOrRegExp))
                // substrOrRegExp is undefined.
                return engine.RegExp.Construct("").Match(thisObject);

            // substrOrRegExp is a string (or convertible to a string).
            return engine.RegExp.Construct(TypeConverter.ToString(substrOrRegExp)).Match(thisObject);
        }

        /// <summary>
        /// Wraps the string in double quotes (").  Any existing double quotes in the string are
        /// escaped using the backslash character.
        /// </summary>
        /// <param name="thisObject"> The string to wrap. </param>
        /// <returns> The input string wrapped with double quotes and with existing double quotes
        /// escaped. </returns>
        [JSInternalFunction(Name = "quote", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Quote(string thisObject)
        {
            var result = new System.Text.StringBuilder(thisObject.Length + 2);
            result.Append('"');
            for (int i = 0; i < thisObject.Length; i++)
            {
                char c = thisObject[i];
                if (c == '"')
                    result.Append('\\');
                result.Append(c);
            }
            result.Append('"');
            return result.ToString();
        }

        /// <summary>
        /// Substitutes the given string or regular expression with the given text or the result
        /// of a replacement function.
        /// </summary>
        /// <param name="substrOrRegExp"> The substring to replace -or- a regular expression that
        /// matches the text to replace. </param>
        /// <param name="replaceTextOrFunction"> The text to substitute -or- a function that
        /// returns the text to substitute. </param>
        /// <returns> A copy of this string with text replaced. </returns>
        [JSInternalFunction(Name = "replace", Flags = JSFunctionFlags.HasThisObject)]
        public static string Replace(string thisObject, object substrOrRegExp, object replaceTextOrFunction)
        {
            // The built-in function binding system is not powerful enough to bind the replace
            // function properly, so we bind to the correct function manually.
            if (substrOrRegExp is RegExpInstance)
            {
                if (replaceTextOrFunction is FunctionInstance)
                    return Replace(thisObject, (RegExpInstance)substrOrRegExp, (FunctionInstance)replaceTextOrFunction);
                else
                    return Replace(thisObject, (RegExpInstance)substrOrRegExp, TypeConverter.ToString(replaceTextOrFunction));
            }
            else
            {
                if (replaceTextOrFunction is FunctionInstance)
                    return Replace(thisObject, TypeConverter.ToString(substrOrRegExp), (FunctionInstance)replaceTextOrFunction);
                else
                    return Replace(thisObject, TypeConverter.ToString(substrOrRegExp), TypeConverter.ToString(replaceTextOrFunction));
            }
        }

        /// <summary>
        /// Returns a copy of this string with text replaced.
        /// </summary>
        /// <param name="substr"> The text to search for. </param>
        /// <param name="replaceText"> A string containing the text to replace for every successful
        /// match. </param>
        /// <returns> A copy of this string with text replaced. </returns>
        public static string Replace(string thisObject, string substr, string replaceText)
        {
            // Find the first occurrance of substr.
            int start = thisObject.IndexOf(substr, StringComparison.Ordinal);
            if (start == -1)
                return thisObject;
            int end = start + substr.Length;

            // Replace only the first match.
            var result = new System.Text.StringBuilder(thisObject.Length + (replaceText.Length - substr.Length));
            result.Append(thisObject, 0, start);
            result.Append(replaceText);
            result.Append(thisObject, end, thisObject.Length - end);
            return result.ToString();
        }

        /// <summary>
        /// Returns a copy of this string with text replaced using a replacement function.
        /// </summary>
        /// <param name="substr"> The text to search for. </param>
        /// <param name="replaceFunction"> A function that is called to produce the text to replace
        /// for every successful match. </param>
        /// <returns> A copy of this string with text replaced. </returns>
        public static string Replace(string thisObject, string substr, FunctionInstance replaceFunction)
        {
            // Find the first occurrance of substr.
            int start = thisObject.IndexOf(substr, StringComparison.Ordinal);
            if (start == -1)
                return thisObject;
            int end = start + substr.Length;

            // Get the replacement text from the provided function.
            var replaceText = TypeConverter.ToString(replaceFunction.CallFromNative("replace", null, substr, start, thisObject));

            // Replace only the first match.
            var result = new System.Text.StringBuilder(thisObject.Length + (replaceText.Length - substr.Length));
            result.Append(thisObject, 0, start);
            result.Append(replaceText);
            result.Append(thisObject, end, thisObject.Length - end);
            return result.ToString();
        }

        /// <summary>
        /// Returns a copy of this string with text replaced using a regular expression.
        /// </summary>
        /// <param name="regExp"> The regular expression to search for. </param>
        /// <param name="replaceText"> A string containing the text to replace for every successful match. </param>
        /// <returns> A copy of this string with text replaced using a regular expression. </returns>
        public static string Replace(string thisObject, RegExpInstance regExp, string replaceText)
        {
            return regExp.Replace(thisObject, replaceText);
        }

        /// <summary>
        /// Returns a copy of this string with text replaced using a regular expression and a
        /// replacement function.
        /// </summary>
        /// <param name="regExp"> The regular expression to search for. </param>
        /// <param name="replaceFunction"> A function that is called to produce the text to replace
        /// for every successful match. </param>
        /// <returns> A copy of this string with text replaced using a regular expression. </returns>
        public static string Replace(string thisObject, RegExpInstance regExp, FunctionInstance replaceFunction)
        {
            return regExp.Replace(thisObject, replaceFunction);
        }

        /// <summary>
        /// Returns the position of the first substring match.
        /// </summary>
        /// <param name="substrOrRegExp"> The string or regular expression to search for. </param>
        /// <returns> The character position of the first match, or -1 if no match was found. </returns>
        [JSInternalFunction(Name = "search", Flags = JSFunctionFlags.HasThisObject)]
        public static int Search(string thisObject, object substrOrRegExp)
        {
            if (substrOrRegExp is RegExpInstance)
                // substrOrRegExp is a regular expression.
                return ((RegExpInstance)substrOrRegExp).Search(thisObject);
            
            if (TypeUtilities.IsUndefined(substrOrRegExp))
                // substrOrRegExp is undefined.
                return 0;

            // substrOrRegExp is a string (or convertible to a string).
            return thisObject.IndexOf(TypeConverter.ToString(substrOrRegExp), StringComparison.Ordinal);
        }

        /// <summary>
        /// Extracts a section of the string and returns a new string.
        /// </summary>
        /// <param name="start"> The character position to start extracting. </param>
        /// <param name="end"> The character position to stop extacting. </param>
        /// <returns> A section of the string. </returns>
        [JSInternalFunction(Name = "slice", Flags = JSFunctionFlags.HasThisObject)]
        public static string Slice(string thisObject, int start, [DefaultParameterValue(int.MaxValue)] int end = int.MaxValue)
        {
            // Negative offsets are measured from the end of the string.
            if (start < 0)
                start += thisObject.Length;
            if (end < 0)
                end += thisObject.Length;

            // Constrain the parameters to within the limits of the string.
            start = Math.Min(Math.Max(start, 0), thisObject.Length);
            end = Math.Min(Math.Max(end, 0), thisObject.Length);
            if (end <= start)
                return string.Empty;

            return thisObject.Substring(start, end - start);
        }

        /// <summary>
        /// Splits this string into an array of strings by separating the string into substrings.
        /// </summary>
        /// <param name="separator"> A string or regular expression that indicates where to split the string. </param>
        /// <param name="limit"> The maximum number of array items to return.  Defaults to unlimited. </param>
        /// <returns> An array containing the split strings. </returns>
        [JSInternalFunction(Name = "split", Flags = JSFunctionFlags.HasEngineParameter | JSFunctionFlags.HasThisObject)]
        public static ArrayInstance Split(ScriptEngine engine, string thisObject, object separator, [DefaultParameterValue(4294967295.0)] double limit = uint.MaxValue)
        {
            // Limit defaults to unlimited.  Note the ToUint32() conversion.
            uint limit2 = uint.MaxValue;
            if (TypeUtilities.IsUndefined(limit) == false)
                limit2 = TypeConverter.ToUint32(limit);

            // Call separate methods, depending on whether the separator is a regular expression.
            if (separator is RegExpInstance)
                return Split(thisObject, (RegExpInstance)separator, limit2);
            else
                return Split(engine, thisObject, TypeConverter.ToString(separator), limit2);
        }

        /// <summary>
        /// Splits this string into an array of strings by separating the string into substrings.
        /// </summary>
        /// <param name="regExp"> A regular expression that indicates where to split the string. </param>
        /// <param name="limit"> The maximum number of array items to return.  Defaults to unlimited. </param>
        /// <returns> An array containing the split strings. </returns>
        public static ArrayInstance Split(string thisObject, RegExpInstance regExp, [DefaultParameterValue(uint.MaxValue)] uint limit = uint.MaxValue)
        {
            return regExp.Split(thisObject, limit);
        }

        /// <summary>
        /// Splits this string into an array of strings by separating the string into substrings.
        /// </summary>
        /// <param name="separator"> A string that indicates where to split the string. </param>
        /// <param name="limit"> The maximum number of array items to return.  Defaults to unlimited. </param>
        /// <returns> An array containing the split strings. </returns>
        public static ArrayInstance Split(ScriptEngine engine, string thisObject, string separator, [DefaultParameterValue(uint.MaxValue)] uint limit = uint.MaxValue)
        {
            if (string.IsNullOrEmpty(separator))
            {
                // If the separator is empty, split the string into individual characters.
                var result = engine.Array.New();
                for (int i = 0; i < thisObject.Length; i ++)
                    result[i] = thisObject[i].ToString();
                return result;
            }
            var splitStrings = thisObject.Split(new string[] { separator }, StringSplitOptions.None);
            if (limit < splitStrings.Length)
            {
                var splitStrings2 = new string[limit];
                Array.Copy(splitStrings, splitStrings2, (int)limit);
                splitStrings = splitStrings2;
            }
            return engine.Array.New(splitStrings);
        }

        /// <summary>
        /// Returns the characters in a string beginning at the specified location through the specified number of characters.
        /// </summary>
        /// <param name="start"> The character position to start extracting. </param>
        /// <param name="length"> The number of characters to extract. </param>
        /// <returns> A substring of this string. </returns>
        [JSInternalFunction(Name = "substr", Flags = JSFunctionFlags.HasThisObject, Deprecated = true)]
        public static string Substr(string thisObject, int start, [DefaultParameterValue(int.MaxValue)] int length = int.MaxValue)
        {
            // If start is less than zero, it is measured from the end of the string.
            if (start < 0)
                start = Math.Max(start + thisObject.Length, 0);

            // Compute the actual length.
            length = Math.Max(Math.Min(length, thisObject.Length - start), 0);
            if (length <= 0)
                return string.Empty;

            // Extract the substring.
            return thisObject.Substring(start, length);
        }

        /// <summary>
        /// Returns the characters in a string between two indexes into the string.
        /// </summary>
        /// <param name="start"> The character position to start extracting. </param>
        /// <param name="end"> The character position to stop extracting. </param>
        /// <returns> A substring of this string. </returns>
        [JSInternalFunction(Name = "substring", Flags = JSFunctionFlags.HasThisObject)]
        public static string Substring(string thisObject, int start, [DefaultParameterValue(int.MaxValue)] int end = int.MaxValue)
        {
            return Slice(thisObject, Math.Max(Math.Min(start, end), 0), Math.Max(Math.Max(start, end), 0));
        }

        /// <summary>
        /// Converts the characters within this string to lowercase while respecting the current
        /// locale.
        /// </summary>
        /// <returns> A copy of this string with the characters converted to lowercase. </returns>
        [JSInternalFunction(Name = "toLocaleLowerCase", Flags = JSFunctionFlags.HasThisObject)]
        public static string ToLocaleLowerCase(string thisObject)
        {
            return thisObject.ToLower();
        }

        /// <summary>
        /// Converts the characters within this string to uppercase while respecting the current
        /// locale.
        /// </summary>
        /// <returns> A copy of this string with the characters converted to uppercase. </returns>
        [JSInternalFunction(Name = "toLocaleUpperCase", Flags = JSFunctionFlags.HasThisObject)]
        public static string ToLocaleUpperCase(string thisObject)
        {
            return thisObject.ToUpper();
        }

        /// <summary>
        /// Returns the calling string value converted to lowercase.
        /// </summary>
        /// <returns> A copy of this string with the characters converted to lowercase. </returns>
        [JSInternalFunction(Name = "toLowerCase", Flags = JSFunctionFlags.HasThisObject)]
        public static string ToLowerCase(string thisObject)
        {
            return thisObject.ToLowerInvariant();
        }

        /// <summary>
        /// Returns a string representing the current object.
        /// </summary>
        /// <returns> A string representing the current object. </returns>
        [JSInternalFunction(Name = "toString")]
        public new string ToString()
        {
            return this.value;
        }

        /// <summary>
        /// Returns the calling string value converted to uppercase.
        /// </summary>
        /// <returns> A copy of this string with the characters converted to uppercase. </returns>
        [JSInternalFunction(Name = "toUpperCase", Flags = JSFunctionFlags.HasThisObject)]
        public static string ToUpperCase(string thisObject)
        {
            return thisObject.ToUpperInvariant();
        }

        private static char[] trimCharacters = new char[] {
            // Whitespace
            '\x09', '\x0B', '\x0C', '\x20', '\xA0', '\xFEFF',

            // Unicode space separator
            '\u1680', '\u180E', '\u2000', '\u2001',
            '\u2002', '\u2003', '\u2004', '\u2005',
            '\u2006', '\u2007', '\u2008', '\u2009',
            '\u200A', '\u202F', '\u205F', '\u3000', 

            // Line terminators
            '\x0A', '\x0D', '\u2028', '\u2029',
        };

        /// <summary>
        /// Trims whitespace from the beginning and end of the string.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "trim", Flags = JSFunctionFlags.HasThisObject)]
        public static string Trim(string thisObject)
        {
            return thisObject.Trim(trimCharacters);
        }

        /// <summary>
        /// Trims whitespace from the beginning of the string.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "trimLeft", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string TrimLeft(string thisObject)
        {
            return thisObject.TrimStart(trimCharacters);
        }

        /// <summary>
        /// Trims whitespace from the beginning of the string.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "trimRight", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string TrimRight(string thisObject)
        {
            return thisObject.TrimEnd(trimCharacters);
        }

        /// <summary>
        /// Returns the underlying primitive value of the current object.
        /// </summary>
        /// <returns> The underlying primitive value of the current object. </returns>
        [JSInternalFunction(Name = "valueOf")]
        public new string ValueOf()
        {
            return this.value;
        }



        //     JAVASCRIPT FUNCTIONS (HTML WRAPPER FUNCTIONS)
        //_________________________________________________________________________________________

        /// <summary>
        /// Wraps the string with an anchor tag.
        /// </summary>
        /// <param name="name"> The name of the anchor. </param>
        /// <returns> </returns>
        [JSInternalFunction(Name = "anchor", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Anchor(string thisObject, string name)
        {
            return string.Format(@"<a name=""{1}"">{0}</a>", thisObject, name);
        }

        /// <summary>
        /// Wraps the string with a big tag.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "big", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Big(string thisObject)
        {
            return string.Format("<big>{0}</big>", thisObject);
        }

        /// <summary>
        /// Wraps the string with a blink tag.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "blink", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Blink(string thisObject)
        {
            return string.Format("<blink>{0}</blink>", thisObject);
        }

        /// <summary>
        /// Wraps the string with a bold (b) tag.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "bold", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Bold(string thisObject)
        {
            return string.Format("<b>{0}</b>", thisObject);
        }

        /// <summary>
        /// Wraps the string with a tt tag.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "fixed", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Fixed(string thisObject)
        {
            return string.Format("<tt>{0}</tt>", thisObject);
        }

        /// <summary>
        /// Wraps the string with a font tag that specifies the given color.
        /// </summary>
        /// <param name="colorValue"> The color value or name. </param>
        /// <returns></returns>
        [JSInternalFunction(Name = "fontcolor", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string FontColor(string thisObject, string colorValue)
        {
            return string.Format(@"<font color=""{1}"">{0}</font>", thisObject, colorValue);
        }

        /// <summary>
        /// Wraps the string with a font tag that specifies the given font size.
        /// </summary>
        /// <param name="size"> The font size, specified as an integer. </param>
        /// <returns></returns>
        [JSInternalFunction(Name = "fontsize", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string FontSize(string thisObject, string size)
        {
            return string.Format(@"<font size=""{1}"">{0}</font>", thisObject, size);
        }

        /// <summary>
        /// Wraps the string with a italics (i) tag.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "italics", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Italics(string thisObject)
        {
            return string.Format("<i>{0}</i>", thisObject);
        }

        /// <summary>
        /// Wraps the string with a hyperlink.
        /// </summary>
        /// <param name="href"></param>
        /// <returns></returns>
        [JSInternalFunction(Name = "link", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Link(string thisObject, string href)
        {
            return string.Format(@"<a href=""{1}"">{0}</a>", thisObject, href);
        }

        /// <summary>
        /// Wraps the string in a <c>small</c> tag.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "small", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Small(string thisObject)
        {
            return string.Format("<small>{0}</small>", thisObject);
        }

        /// <summary>
        /// Wraps the string in a <c>strike</c> tag.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "strike", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Strike(string thisObject)
        {
            return string.Format("<strike>{0}</strike>", thisObject);
        }

        /// <summary>
        /// Wraps the string in a <c>sub</c> tag.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "sub", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Sub(string thisObject)
        {
            return string.Format("<sub>{0}</sub>", thisObject);
        }

        /// <summary>
        /// Wraps the string in a <c>sup</c> tag.
        /// </summary>
        /// <returns></returns>
        [JSInternalFunction(Name = "sup", Flags = JSFunctionFlags.HasThisObject, NonStandard = true)]
        public static string Sup(string thisObject)
        {
            return string.Format("<sup>{0}</sup>", thisObject);
        }
    }
}
