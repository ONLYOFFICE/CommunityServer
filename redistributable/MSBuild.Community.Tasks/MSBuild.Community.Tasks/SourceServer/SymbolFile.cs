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
using System.IO;
using System.Text;



namespace MSBuild.Community.Tasks.SourceServer
{
    /// <summary>
    /// A class representing a symbol file.
    /// </summary>
    public class SymbolFile : FileBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolFile"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public SymbolFile(string fileName)
            : this(new FileInfo(fileName))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolFile"/> class.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        public SymbolFile(FileInfo fileInfo)
            : base(fileInfo)
        {
            SourceFiles = new List<SourceFile>();
        }

        /// <summary>
        /// Gets the source files.
        /// </summary>
        /// <value>The source files.</value>
        public IList<SourceFile> SourceFiles { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is indexed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is indexed; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexed { get; set; }

        /// <summary>
        /// Adds the source files.
        /// </summary>
        /// <param name="files">The files.</param>
        public void AddSourceFiles(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                SourceFile sourceFile = new SourceFile(file);
                SourceFiles.Add(sourceFile);
            }
        }
    }
}
