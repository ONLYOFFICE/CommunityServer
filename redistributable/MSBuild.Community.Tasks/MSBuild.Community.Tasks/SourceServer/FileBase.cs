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

using System.IO;



namespace MSBuild.Community.Tasks.SourceServer
{
    /// <summary>
    /// A base class that has a file.
    /// </summary>
    public class FileBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileBase"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public FileBase(string fileName)
            : this(new FileInfo(fileName))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBase"/> class.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        public FileBase(FileInfo fileInfo)
        {
            File = fileInfo;
        }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>The file.</value>
        public FileInfo File { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return File.ToString();
        }

        #region implicit operators
        /// <summary>
        /// Performs an implicit conversion from <see cref="FileBase"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="file">The output file.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(FileBase file)
        {
            return file.File.FullName;
        }
        #endregion
    }
}