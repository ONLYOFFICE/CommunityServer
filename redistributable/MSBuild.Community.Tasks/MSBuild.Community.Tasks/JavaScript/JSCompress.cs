#region Copyright (c) 2007 Atif Aziz. All rights reserved.
//
// Copyright (c) 2007 Atif Aziz. All rights reserved.
// http://www.raboof.com
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// 3. The name of the author may not be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  
//
#endregion



namespace MSBuild.Community.Tasks.JavaScript
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using MSBuild.Community.Tasks.Properties;

    #endregion

    /// <summary>
    /// Compresses JavaScript source by removing comments and unnecessary 
    /// whitespace. It typically reduces the size of the script by half, 
    /// resulting in faster downloads and code that is harder to read.
    /// </summary>
    /// <remarks>
    /// This task does not change the behavior of the program that it is 
    /// compressing. The resulting code will be harder to debug as well as
    /// harder to read.
    /// </remarks>

    public class JSCompress : Task 
    {
        private ITaskItem[] files;
        private string encodingName;
        private ITaskItem[] compressedFiles;

        /// <summary>
        /// Gets or sets the files to source-compress.
        /// </summary>

        [ Required ]
        public ITaskItem[] Files
        {
            get { return files; }
            set { files = value; }
        }

        /// <summary>
        /// Encoding to use to read and write files.
        /// </summary>

        public string Encoding
        {
            get { return encodingName; }
            set { encodingName = value; }
        }

        /// <summary>
        /// Gets the files that were successfully source-compressed.
        /// </summary>

        [ Output ]
        public ITaskItem[] CompressedFiles
        {
            get { return compressedFiles; }
        }


        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>

        public override bool Execute()
        {
            //
            // Nothing to work on? Bail out early.
            //

            if (files == null || files.Length == 0)
            {
                compressedFiles = new ITaskItem[0];
                return true;
            }

            //
            // Compress all files...da main loop!
            //

            ITaskItem[] inputs = Files;
            List<ITaskItem> outputList = new List<ITaskItem>(inputs.Length);

            foreach (ITaskItem input in inputs)
            {
                string path = input.ItemSpec;
                
                if (path.Length == 0)
                    continue;

                try
                {
                    //
                    // Get the original file size to compare later.
                    // If the file is empty then skip it.
                    //

                    FileInfo info = new FileInfo(path);

                    if (info.Length == 0)
                        continue;

                    //
                    // Determine if a specific encoding should be assumed.
                    //

                    Encoding encoding = null;
                    string encodingName = Encoding ?? string.Empty;

                    if (encodingName.Length > 0)
                        encoding = System.Text.Encoding.GetEncoding(encodingName);

                    //
                    // Read the source and compress to memory. This assumes 
                    // that we won't be dealing with huge JavaScript source
                    // files so doing the work in memory won't hurt too much.
                    //

                    Log.LogMessage(string.Format(Resources.JSCompressCompressing, path));

                    string source;

                    using (StreamReader reader = encoding != null ? 
                        new StreamReader(path, encoding) : new StreamReader(path))
                    {
                        StringWriter writer = new StringWriter();
                        JavaScriptCompressor.Compress(reader, writer);
                        source = writer.GetStringBuilder().ToString();

                        if (encoding == null)
                            encoding = reader.CurrentEncoding;
                    }

                    //
                    // Write out the result!
                    //

                    Log.LogMessage(MessageImportance.Low, 
                        string.Format(Resources.JSCompressWriting, path, encoding.EncodingName));

                    File.WriteAllText(path, source, encoding);

                    outputList.Add(input);

                    //
                    // Compare new file size and log the compression ratio.
                    //

                    FileInfo newInfo = new FileInfo(path);
                    double ratio = 1 - (newInfo.Length / (double) info.Length);

                    Log.LogMessage(MessageImportance.Low, string.Format(
                        Resources.JSCompressCompressed, 
                        ratio.ToString("P"), 
                        info.Length.ToString("N0"), 
                        newInfo.Length.ToString("N0")));
                }
                catch (Exception e)
                {
                    if (e is IOException || e is JavaScriptCompressor.Exception)
                    {
                        Log.LogError("Error compressing " + path + ". " + e.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            compressedFiles = outputList.ToArray();

            return !Log.HasLoggedErrors;
        }
    }
}