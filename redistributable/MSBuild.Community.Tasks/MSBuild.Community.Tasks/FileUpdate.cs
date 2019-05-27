#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

#region 2011-12-19: Default Encoding by Manuel Sprock (Phoenix Contact)
/* 
Support for default encoding (utf-8-without-bom) added. 
There is no WebName for utf-8-without-bom. It's the default 
encoding when not specifying an encoding and using the overload
File.WriteAllText(fileName, buffer) 
Cf. http://msdn.microsoft.com/en-us/library/ms143375.aspx
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Replace text in file(s) using a Regular Expression.
    /// </summary>
    /// <example>Search for a version number and update the revision.
    /// <code><![CDATA[
    /// <FileUpdate Files="version.txt"
    ///     Regex="(\d+)\.(\d+)\.(\d+)\.(\d+)"
    ///     ReplacementText="$1.$2.$3.123" />
    /// ]]></code>
    /// </example>
    public class FileUpdate :Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileUpdate"/> class.
        /// </summary>
        public FileUpdate()
        {
            ItemsNotUpdated = null;
            AllItemsUpdated = true;

        }

        #region Properties

        private ITaskItem[] _files;

        /// <summary>
        /// Gets or sets the files to update.
        /// </summary>
        /// <value>The files.</value>
        public ITaskItem[] Files
        {
            get { return _files; }
            set { _files = value; }
        }

        private string _regex;

        /// <summary>
        /// Gets or sets the regex.
        /// </summary>
        /// <value>The regex.</value>
        public string Regex
        {
            get { return _regex; }
            set { _regex = value; }
        }

        private bool _ignoreCase;

        /// <summary>
        /// Gets or sets a value specifies case-insensitive matching. .
        /// </summary>
        /// <value><c>true</c> if [ignore case]; otherwise, <c>false</c>.</value>
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
            set { _ignoreCase = value; }
        }

        private bool _multiline;

        /// <summary>
        /// Gets or sets a value changing the meaning of ^ and $ so they match at the beginning and end, 
        /// respectively, of any line, and not just the beginning and end of the entire string.
        /// </summary>
        /// <value><c>true</c> if multiline; otherwise, <c>false</c>.</value>
        public bool Multiline
        {
            get { return _multiline; }
            set { _multiline = value; }
        }

        private bool _singleline;

        /// <summary>
        /// Gets or sets a value changing the meaning of the dot (.) so it matches 
        /// every character (instead of every character except \n). 
        /// </summary>
        /// <value><c>true</c> if singleline; otherwise, <c>false</c>.</value>
        public bool Singleline
        {
            get { return _singleline; }
            set { _singleline = value; }
        }

        private int _replacementCount = -1;

        /// <summary>
        /// Gets or sets the maximum number of times the replacement can occur.
        /// </summary>
        /// <value>The replacement count.</value>
        public int ReplacementCount
        {
            get { return _replacementCount; }
            set { _replacementCount = value; }
        }

        private bool _replacementTextEmpty;

        /// <summary>
        /// Gets or sets if replacement text is empty.
        /// </summary>
        /// <value><c>true</c> if [ignore case]; otherwise, <c>false</c>.</value>
        public bool ReplacementTextEmpty
        {
            get { return _replacementTextEmpty; }
            set { _replacementTextEmpty = value; }
        }

        private string _replacementText;

        /// <summary>
        /// Gets or sets the replacement text.
        /// </summary>
        /// <value>The replacement text.</value>
        public string ReplacementText
        {
            get { return _replacementText; }
            set { _replacementText = value; }
        }

        /// Maintain the behaviour of the original implementation for compatibility
        /// (i.e. initialize _useDefaultEncoding with false) and use utf-8-without-bom,  
        /// which is Microsoft's default encoding, only when Encoding property is set 
        /// to "utf-8-without-bom". 
        private bool _useDefaultEncoding; 

        private Encoding _encoding = System.Text.Encoding.UTF8;
        /// <summary>
        /// The character encoding used to read and write the file.
        /// </summary>
        /// <remarks>Any value returned by <see cref="System.Text.Encoding.WebName"/> is valid input.
        /// <para>The default is <c>utf-8</c></para>
        /// <para>Additionally, <c>utf-8-without-bom</c>can be used.</para></remarks>
        public string Encoding
        {
            get 
            {
                if (_useDefaultEncoding) return "utf-8-without-bom"; 
                else return _encoding.WebName; 
            }
            set
            {
                if (value.ToLower().CompareTo("utf-8-without-bom") == 0) _useDefaultEncoding = true; 
                else _encoding = System.Text.Encoding.GetEncoding(value);
            }
        }

        //--added for testing

        private bool _warnOnNoUpdate;
        /// <summary>
        /// When TRUE, a warning will be generated to show which file was not updated.
        /// </summary>
        /// <remarks>N/A</remarks>
        public bool WarnOnNoUpdate
        {
            get { return _warnOnNoUpdate; }
            set { _warnOnNoUpdate = value; }
        }

       

        #endregion

        #region Output Properties

        /// <summary>
        /// Returns list of items that were not updated
        /// </summary>
        [Output]
        public ITaskItem[] ItemsNotUpdated { get; set; }

        /// <summary>
        /// Returns true if all items were updated, else false
        /// </summary>
        [Output]
        public bool AllItemsUpdated { get; set; }

        #endregion

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            RegexOptions options = RegexOptions.None;
            
            if (_ignoreCase)
            {
                options |= RegexOptions.IgnoreCase;
            }
            
            if (_multiline)
            {
                options |= RegexOptions.Multiline;
            }
            
            if (_singleline)
            {
                options |= RegexOptions.Singleline;
            }
            
            if (_replacementCount == 0)
            {
                _replacementCount = -1;
            }

            if (_replacementTextEmpty)
            {
                _replacementText = String.Empty;
            }

            Regex replaceRegex = new Regex(_regex, options);

            try
            {
                var itemsNotUpdated = new List<ITaskItem>();

                foreach (ITaskItem item in _files)
                {
                    string fileName = item.ItemSpec;
                    Log.LogMessage("Updating File \"{0}\".", fileName);

                    string buffer = "";
                    
                    if (_useDefaultEncoding)
                    {
                        buffer = File.ReadAllText(fileName);
                    }
                    else
                    {
                        buffer = File.ReadAllText(fileName, _encoding);
                    }

                    if (!replaceRegex.IsMatch(buffer))
                    {
                        itemsNotUpdated.Add(item);

                        if (_warnOnNoUpdate)
                        {
                            Log.LogWarning(String.Format("No updates were performed on file : {0}.", fileName));
                        }
                    }                   

                    buffer = replaceRegex.Replace(buffer, _replacementText, _replacementCount);      

                    if (_useDefaultEncoding)
                    {
                        File.WriteAllText(fileName, buffer);
                    }
                    else
                    {
                        File.WriteAllText(fileName, buffer, _encoding);
                    }
                }

                if (itemsNotUpdated.Count > 0)
                {
                    ItemsNotUpdated = itemsNotUpdated.ToArray();
                    AllItemsUpdated = false;
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                AllItemsUpdated = false;
                return false;
            }
            return true;
        }
    }
}
