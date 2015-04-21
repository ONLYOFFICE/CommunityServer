using System;
using System.Collections.Generic;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents the built-in javascript RegExp object.
    /// </summary>
    [Serializable]
    public class RegExpConstructor : ClrFunction
    {
        private string lastInput;
        private System.Text.RegularExpressions.Match lastMatch;


        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new RegExp object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal RegExpConstructor(ObjectInstance prototype)
            : base(prototype, "RegExp", new RegExpInstance(prototype.Engine.Object.InstancePrototype, string.Empty))
        {
            this.InitializeDeprecatedProperties();
        }




        //     DEPRECATED PROPERTIES
        //_________________________________________________________________________________________

        // Deprecated properties:
        // $1, ..., $9          Parenthesized substring matches, if any.
        // input ($_)           The string against which a regular expression is matched.
        // lastMatch ($&)       The last matched characters.
        // lastParen ($+)       The last parenthesized substring match, if any.
        // leftContext ($`)     The substring preceding the most recent match.
        // rightContext ($')    The substring following the most recent match.

        /// <summary>
        /// Initializes the deprecated RegExp properties.
        /// </summary>
        private void InitializeDeprecatedProperties()
        {
            // Set the deprecated properties to their default values.
            this.InitializeDeprecatedProperty("$1", GetGroup1, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$2", GetGroup2, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$3", GetGroup3, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$4", GetGroup4, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$5", GetGroup5, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$6", GetGroup6, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$7", GetGroup7, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$8", GetGroup8, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$9", GetGroup9, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("input", GetInput, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$_", GetInput, PropertyAttributes.Sealed);
            this.InitializeDeprecatedProperty("lastMatch", GetLastMatch, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$&", GetLastMatch, PropertyAttributes.Sealed);
            this.InitializeDeprecatedProperty("lastParen", GetLastParen, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$+", GetLastParen, PropertyAttributes.Sealed);
            this.InitializeDeprecatedProperty("leftContext", GetLeftContext, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$`", GetLeftContext, PropertyAttributes.Sealed);
            this.InitializeDeprecatedProperty("rightContext", GetRightContext, PropertyAttributes.Enumerable);
            this.InitializeDeprecatedProperty("$'", GetRightContext, PropertyAttributes.Sealed);
        }

        /// <summary>
        /// Initializes a single deprecated property.
        /// </summary>
        /// <param name="propertyName"> The name of the property. </param>
        /// <param name="getter"> The property getter. </param>
        /// <param name="attributes"> The property attributes (determines whether the property is enumerable). </param>
        private void InitializeDeprecatedProperty(string propertyName, Func<string> getter, PropertyAttributes attributes)
        {
            this.DefineProperty(propertyName, new PropertyDescriptor(new PropertyAccessorValue(new ClrFunction(this.Engine.Function.InstancePrototype, getter), null), attributes), false);
        }

        /// <summary>
        /// Sets the deprecated RegExp properties.
        /// </summary>
        /// <param name="input"> The string against which a regular expression is matched. </param>
        /// <param name="match"> The regular expression match to base the properties on. </param>
        internal void SetDeprecatedProperties(string input, System.Text.RegularExpressions.Match match)
        {
            this.lastInput = input;
            this.lastMatch = match;
        }

        /// <summary>
        /// Gets the value of RegExp.input and RegExp.$_.
        /// </summary>
        /// <returns> The value of RegExp.input and RegExp.$_. </returns>
        public string GetInput()
        {
            if (this.lastMatch == null)
                return string.Empty;
            return this.lastInput;
        }

        /// <summary>
        /// Gets the value of RegExp.$1.
        /// </summary>
        /// <returns> The value of RegExp.$1. </returns>
        public string GetGroup1()
        {
            if (this.lastMatch == null || this.lastMatch.Groups.Count < 1)
                return string.Empty;
            return this.lastMatch.Groups[1].Value;
        }

        /// <summary>
        /// Gets the value of RegExp.$2.
        /// </summary>
        /// <returns> The value of RegExp.$2. </returns>
        public string GetGroup2()
        {
            if (this.lastMatch == null || this.lastMatch.Groups.Count < 2)
                return string.Empty;
            return this.lastMatch.Groups[2].Value;
        }

        /// <summary>
        /// Gets the value of RegExp.$3.
        /// </summary>
        /// <returns> The value of RegExp.$3. </returns>
        public string GetGroup3()
        {
            if (this.lastMatch == null || this.lastMatch.Groups.Count < 3)
                return string.Empty;
            return this.lastMatch.Groups[3].Value;
        }

        /// <summary>
        /// Gets the value of RegExp.$4.
        /// </summary>
        /// <returns> The value of RegExp.$4. </returns>
        public string GetGroup4()
        {
            if (this.lastMatch == null || this.lastMatch.Groups.Count < 4)
                return string.Empty;
            return this.lastMatch.Groups[4].Value;
        }

        /// <summary>
        /// Gets the value of RegExp.$5.
        /// </summary>
        /// <returns> The value of RegExp.$5. </returns>
        public string GetGroup5()
        {
            if (this.lastMatch == null || this.lastMatch.Groups.Count < 5)
                return string.Empty;
            return this.lastMatch.Groups[5].Value;
        }

        /// <summary>
        /// Gets the value of RegExp.$6.
        /// </summary>
        /// <returns> The value of RegExp.$6. </returns>
        public string GetGroup6()
        {
            if (this.lastMatch == null || this.lastMatch.Groups.Count < 6)
                return string.Empty;
            return this.lastMatch.Groups[6].Value;
        }

        /// <summary>
        /// Gets the value of RegExp.$7.
        /// </summary>
        /// <returns> The value of RegExp.$7. </returns>
        public string GetGroup7()
        {
            if (this.lastMatch == null || this.lastMatch.Groups.Count < 7)
                return string.Empty;
            return this.lastMatch.Groups[7].Value;
        }

        /// <summary>
        /// Gets the value of RegExp.$8.
        /// </summary>
        /// <returns> The value of RegExp.$8. </returns>
        public string GetGroup8()
        {
            if (this.lastMatch == null || this.lastMatch.Groups.Count < 8)
                return string.Empty;
            return this.lastMatch.Groups[8].Value;
        }

        /// <summary>
        /// Gets the value of RegExp.$9.
        /// </summary>
        /// <returns> The value of RegExp.$9. </returns>
        public string GetGroup9()
        {
            if (this.lastMatch == null || this.lastMatch.Groups.Count < 9)
                return string.Empty;
            return this.lastMatch.Groups[9].Value;
        }

        /// <summary>
        /// Gets the value of RegExp.lastMatch and RegExp.$&.
        /// </summary>
        /// <returns> The value of RegExp.lastMatch and RegExp.$&. </returns>
        public string GetLastMatch()
        {
            if (this.lastMatch == null)
                return string.Empty;
            return this.lastMatch.Value;
        }

        /// <summary>
        /// Gets the value of RegExp.lastParen and RegExp.$+.
        /// </summary>
        /// <returns> The value of RegExp.lastParen and RegExp.$+. </returns>
        public string GetLastParen()
        {
            if (this.lastMatch == null)
                return string.Empty;
            return this.lastMatch.Groups.Count > 1 ?
                this.lastMatch.Groups[this.lastMatch.Groups.Count - 1].Value :
                string.Empty;
        }

        /// <summary>
        /// Gets the value of RegExp.leftContext and RegExp.$`.
        /// </summary>
        /// <returns> The value of RegExp.leftContext and RegExp.$`. </returns>
        public string GetLeftContext()
        {
            if (this.lastMatch == null)
                return string.Empty;
            return this.lastInput.Substring(0, this.lastMatch.Index);
        }

        /// <summary>
        /// Gets the value of RegExp.rightContext and RegExp.$'.
        /// </summary>
        /// <returns> The value of RegExp.rightContext and RegExp.$'. </returns>
        public string GetRightContext()
        {
            if (this.lastMatch == null)
                return string.Empty;
            return this.lastInput.Substring(this.lastMatch.Index + this.lastMatch.Length);
        }




        //     JAVASCRIPT INTERNAL FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Called when the RegExp object is invoked like a function e.g. RegExp('abc', 'g') or
        /// RegExp(/abc/).  If a string is passed as the first parameter it creates a new regular
        /// expression instance.  Otherwise, if a regular expression is passed it returns the given
        /// regular expression verbatim.
        /// </summary>
        /// <param name="patternOrRegExp"> A regular expression pattern or a regular expression. </param>
        /// <param name="flags"> Available flags, which may be combined, are:
        /// g (global search for all occurrences of pattern)
        /// i (ignore case)
        /// m (multiline search)</param>
        [JSCallFunction]
        public RegExpInstance Call(object patternOrRegExp, [DefaultParameterValue(null)] string flags = null)
        {
            if (patternOrRegExp is RegExpInstance)
            {
                // RegExp(/abc/)
                if (flags != null)
                    throw new JavaScriptException(this.Engine, "TypeError", "Cannot supply flags when constructing one RegExp from another");
                return (RegExpInstance)patternOrRegExp;
            }
            else
            {
                // RegExp('abc', 'g')
                var pattern = string.Empty;
                if (TypeUtilities.IsUndefined(patternOrRegExp) == false)
                    pattern = TypeConverter.ToString(patternOrRegExp);
                return new RegExpInstance(this.InstancePrototype, pattern, flags);
            }
        }

        /// <summary>
        /// Called when the new keyword is used on the RegExp object e.g. new RegExp(/abc/).
        /// Creates a new regular expression instance.
        /// </summary>
        /// <param name="patternOrRegExp"> The regular expression pattern, or a regular expression
        /// to clone. </param>
        /// <param name="flags"> Available flags, which may be combined, are:
        /// g (global search for all occurrences of pattern)
        /// i (ignore case)
        /// m (multiline search)</param>
        [JSConstructorFunction]
        public RegExpInstance Construct(object patternOrRegExp, [DefaultParameterValue(null)] string flags = null)
        {
            if (patternOrRegExp is RegExpInstance)
            {
                // new RegExp(regExp, flags)
                if (flags != null)
                    throw new JavaScriptException(this.Engine, "TypeError", "Cannot supply flags when constructing one RegExp from another");
                return new RegExpInstance(this.InstancePrototype, (RegExpInstance)patternOrRegExp);
            }
            else
            {
                // new RegExp(pattern, flags)
                var pattern = string.Empty;
                if (TypeUtilities.IsUndefined(patternOrRegExp) == false)
                    pattern = TypeConverter.ToString(patternOrRegExp);
                return new RegExpInstance(this.InstancePrototype, pattern, flags);
            }
        }
    }
}
