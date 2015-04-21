using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Jurassic.Library
{

    /// <summary>
    /// Represents a set of commands for working with the standard console.  This class is
    /// non-standard - it is based on the Firebug console API
    /// (http://getfirebug.com/wiki/index.php/Console_API).
    /// </summary>
    [Serializable]
    public class FirebugConsole : ObjectInstance
    {
        private IFirebugConsoleOutput output;
        private Dictionary<string, Stopwatch> timers;

        /// <summary>
        /// Creates a new FirebugConsole instance.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        public FirebugConsole(ScriptEngine engine)
            : base(engine.Object.InstancePrototype)
        {
            this.Output = new StandardConsoleOutput();
            this.PopulateFunctions();
        }



        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets or sets the console to output to.
        /// </summary>
        public IFirebugConsoleOutput Output
        {
            get { return this.output; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.output = value;
            }
        }



        //     API METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Logs a message to the console.  The objects provided will be converted to strings then
        /// joined together in a space separated line.  The first parameter can be a string
        /// containing the following patterns:
        ///  %s	 String
        ///  %d, %i	 Integer
        ///  %f	 Floating point number
        /// </summary>
        /// <param name="items"> The items to format. </param>
        [JSInternalFunction(Name = "log")]
        public void Log(params object[] items)
        {
            Log(FirebugConsoleMessageStyle.Regular, items);
        }

        /// <summary>
        /// Logs a message to the console.  The objects provided will be converted to strings then
        /// joined together in a space separated line.  The first parameter can be a string
        /// containing the following patterns:
        ///  %s	 String
        ///  %d, %i	 Integer
        ///  %f	 Floating point number
        /// </summary>
        /// <param name="items"> The items to format. </param>
        [JSInternalFunction(Name = "debug")]
        public void Debug(params object[] items)
        {
            Log(FirebugConsoleMessageStyle.Regular, items);
        }

        /// <summary>
        /// Logs a message to the console using a style suggesting informational content. The
        /// objects provided will be converted to strings then joined together in a space separated
        /// line.  The first parameter can be a string containing the following patterns:
        ///  %s	 String
        ///  %d, %i	 Integer
        ///  %f	 Floating point number
        /// </summary>
        /// <param name="items"> The items to format. </param>
        [JSInternalFunction(Name = "info")]
        public void Info(params object[] items)
        {
            Log(FirebugConsoleMessageStyle.Information, items);
        }

        /// <summary>
        /// Logs a message to the console using a style suggesting a warning.  The objects provided
        /// will be converted to strings then joined together in a space separated line.  The first
        /// parameter can be a string containing the following patterns:
        ///  %s	 String
        ///  %d, %i	 Integer
        ///  %f	 Floating point number
        /// </summary>
        /// <param name="items"> The items to format. </param>
        [JSInternalFunction(Name = "warn")]
        public void Warn(params object[] items)
        {
            Log(FirebugConsoleMessageStyle.Warning, items);
        }

        /// <summary>
        /// Logs a message to the console using a style suggesting an error.  The objects provided
        /// will be converted to strings then joined together in a space separated line.  The
        /// first parameter can be a string containing the following patterns:
        ///  %s	 String
        ///  %d, %i	 Integer
        ///  %f	 Floating point number
        /// </summary>
        /// <param name="items"> The items to format. </param>
        [JSInternalFunction(Name = "error")]
        public void Error(params object[] items)
        {
            Log(FirebugConsoleMessageStyle.Error, items);
        }

        /// <summary>
        /// Tests that an expression is true. If not, it will write a message to the console.
        /// </summary>
        /// <param name="expression"> The expression to test. </param>
        /// <param name="items"> The items to format. </param>
        [JSInternalFunction(Name = "assert")]
        public void Assert(bool expression, params object[] items)
        {
            if (expression == false)
            {
                if (items.Length > 0)
                {
                    object[] formattedItems = FormatObjects(items);
                    object[] formattedItems2 = new object[formattedItems.Length + 1];
                    formattedItems2[0] = "Assertion failed:";
                    Array.Copy(formattedItems, 0, formattedItems2, 1, formattedItems.Length);
                    Error(formattedItems2);
                }
                else
                    Error("Assertion failed");
            }
        }

        /// <summary>
        /// Clears the console.
        /// </summary>
        [JSInternalFunction(Name = "clear")]
        public void Clear(params object[] items)
        {
            this.output.Clear();
        }

        /// <summary>
        /// Writes a message to the console and opens a nested block to indent all future messages
        /// sent to the console.  Call console.groupEnd() to close the block.
        /// </summary>
        /// <param name="items"> The items to format. </param>
        [JSInternalFunction(Name = "group")]
        public void Group(params object[] items)
        {
            this.output.StartGroup(Format(items), false);
        }

        /// <summary>
        /// Writes a message to the console and opens a nested block to indent all future messages
        /// sent to the console.  Call console.groupEnd() to close the block.
        /// </summary>
        /// <param name="items"> The items to format. </param>
        [JSInternalFunction(Name = "groupCollapsed")]
        public void GroupCollapsed(params object[] items)
        {
            this.output.StartGroup(Format(items), true);
        }

        /// <summary>
        /// Closes the most recently opened block created by a call to console.group().
        /// </summary>
        [JSInternalFunction(Name = "groupEnd")]
        public void GroupEnd()
        {
            this.output.EndGroup();
        }


#if SILVERLIGHT

        // Silverlight does not have a StopWatch class.
        private class Stopwatch
        {
            private int tickCount;

            private Stopwatch()
            {
                this.tickCount = Environment.TickCount;
            }

            public static Stopwatch StartNew()
            {
                return new Stopwatch();
            }

            public long ElapsedMilliseconds
            {
                get { return Environment.TickCount - this.tickCount; }
            }
        }

#endif

        /// <summary>
        /// Creates a new timer under the given name. Call console.timeEnd(name) with the same name
        /// to stop the timer and print the time elapsed.
        /// </summary>
        /// <param name="name"> The name of the time to create. </param>
        [JSInternalFunction(Name = "time", Flags = JSFunctionFlags.MutatesThisObject)]
        public void Time([DefaultParameterValue("")] string name = "")
        {
            if (name == null)
                name = string.Empty;
            if (this.timers == null)
                this.timers = new Dictionary<string, Stopwatch>();
            if (this.timers.ContainsKey(name) == true)
                return;
            this.timers.Add(name, Stopwatch.StartNew());
        }

        /// <summary>
        /// Stops a timer created by a call to console.time(name) and writes the time elapsed.
        /// </summary>
        /// <param name="name"> The name of the timer to stop. </param>
        [JSInternalFunction(Name = "timeEnd", Flags = JSFunctionFlags.MutatesThisObject)]
        public void TimeEnd([DefaultParameterValue("")] string name = "")
        {
            if (name == null)
                name = string.Empty;
            if (this.timers == null || this.timers.ContainsKey(name) == false)
                return;
            var stopwatch = this.timers[name];
            if (string.IsNullOrEmpty(name))
                Log(FirebugConsoleMessageStyle.Regular, string.Format("{0}ms", stopwatch.ElapsedMilliseconds));
            else
                Log(FirebugConsoleMessageStyle.Regular, string.Format("{0}: {1}ms", name, stopwatch.ElapsedMilliseconds));
            this.timers.Remove(name);
        }



        //     PRIVATE METHODS
        //_________________________________________________________________________________________

        /// <summary>
        /// Logs a message to the console.  The objects provided will be converted to strings then
        /// joined together in a space separated line.  The first parameter can be a string
        /// containing the following patterns:
        ///  %s	 String
        ///  %d, %i	 Integer
        ///  %f	 Floating point number
        /// </summary>
        /// <param name="style"> The style of the message (this determines the icon and text
        /// color). </param>
        /// <param name="items"> The items to format. </param>
        private void Log(FirebugConsoleMessageStyle style, params object[] items)
        {
            this.output.Log(style, FormatObjects(items));
        }

        /// <summary>
        /// Formats a message.  The objects provided will be converted to strings then
        /// joined together in a space separated line.  The first parameter can be a string
        /// containing the following patterns:
        ///  %s	 String
        ///  %d, %i	 Integer
        ///  %f	 Floating point number
        ///  %o	 Object hyperlink
        /// </summary>
        /// <param name="items"> The items to format. </param>
        /// <returns> A formatted string. </returns>
        private static string Format(object[] items)
        {
            var result = new System.Text.StringBuilder();
            items = FormatObjects(items);
            for (int i = 0; i < items.Length; i++)
            {
                result.Append(' ');
                result.Append(TypeConverter.ToString(items[i]));
            }
            return result.ToString();
        }

        /// <summary>
        /// Formats a message.  The objects provided will be converted to strings then
        /// joined together in a space separated line.  The first parameter can be a string
        /// containing the following patterns:
        ///  %s	 String
        ///  %d, %i	 Integer
        ///  %f	 Floating point number
        ///  %o	 Object hyperlink
        /// </summary>
        /// <param name="items"> The items to format. </param>
        /// <returns> An array containing formatted strings interspersed with objects. </returns>
        private static object[] FormatObjects(object[] items)
        {
            if (items.Length == 0)
                return new object[0];
            var result = new List<object>();
            var formattedString = new System.Text.StringBuilder();

            // If the first item is a string, then it is assumed to be a format string.
            int itemsConsumed = 1;
            if (items[0] is string)
            {
                string formatString = (string)items[0];

                int previousPatternIndex = 0, patternIndex;
                while (items.Length > itemsConsumed)
                {
                    // Find a percent sign.
                    patternIndex = formatString.IndexOf('%', previousPatternIndex);
                    if (patternIndex == -1 || patternIndex == formatString.Length - 1)
                        break;

                    // Append the text that didn't contain a pattern to the result.
                    formattedString.Append(formatString, previousPatternIndex, patternIndex - previousPatternIndex);

                    // Extract the pattern type.
                    char patternType = formatString[patternIndex + 1];

                    // Determine the replacement string.
                    string replacement;
                    switch (patternType)
                    {
                        case 's':
                            replacement = TypeConverter.ToString(items[itemsConsumed++]);
                            break;
                        case 'd':
                        case 'i':
                            var number = TypeConverter.ToNumber(items[itemsConsumed++]);
                            replacement = (number >= 0 ? Math.Floor(number) : Math.Ceiling(number)).ToString();
                            break;
                        case 'f':
                            replacement = TypeConverter.ToNumber(items[itemsConsumed++]).ToString();
                            break;
                        case '%':
                            replacement = "%";
                            break;
                        case 'o':
                            replacement = string.Empty;
                            if (formattedString.Length > 0)
                                result.Add(formattedString.ToString());
                            result.Add(items[itemsConsumed++]);
                            formattedString.Remove(0, formattedString.Length);
                            break;
                        default:
                            replacement = "%" + patternType;
                            break;
                    }

                    // Replace the pattern with the corresponding argument.
                    formattedString.Append(replacement);

                    // Start searching just after the end of the pattern.
                    previousPatternIndex = patternIndex + 2;
                }

                // Append the text that didn't contain a pattern to the result.
                formattedString.Append(formatString, previousPatternIndex, formatString.Length - previousPatternIndex);

                // Add the formatted string to the resulting array.
                if (formattedString.Length > 0)
                    result.Add(formattedString.ToString());

                // Append the items that weren't consumed to the end of the resulting array.
                for (int i = itemsConsumed; i < items.Length; i++)
                    result.Add(items[i]);
                return result.ToArray();
            }
            else
            {
                // The first item is not a string - just return the objects verbatim.
                return items;
            }
        }
    }
}
