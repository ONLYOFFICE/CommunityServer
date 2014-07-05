#region Copyright © 2009 MSBuild Community Task Project. All rights reserved.
/*
Copyright © 2008 MSBuild Community Task Project. All rights reserved.

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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;



namespace MSBuild.Community.Tasks.SourceServer
{
    /// <summary>
    /// A class representing a source file.
    /// </summary>
    public class SourceFile : FileBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public SourceFile(string fileName)
            : this(new FileInfo(fileName))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceFile"/> class.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        public SourceFile(FileInfo fileInfo)
            : base(fileInfo)
        {
            Properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public IDictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is resolved.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is resolved; otherwise, <c>false</c>.
        /// </value>
        public bool IsResolved { get; set; }

        /// <summary>
        /// Creates the source string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public string ToSourceString(string format)
        {
            // {File}*{RepositorRoot}*{ItemPath}*{Revision}
            string sourceString = format.Replace("{File}", File.FullName);

            foreach (var property in Properties)
            {
                string key = string.Concat("{", property.Key, "}");
                string value = property.Value == null ? string.Empty : property.Value.ToString();

                sourceString = sourceString.Replace(key, value);
            }

            return sourceString;
        }
    }
}
