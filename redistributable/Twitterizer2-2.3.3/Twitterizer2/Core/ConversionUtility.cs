//-----------------------------------------------------------------------
// <copyright file="ConversionUtility.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net/)
// 
//  Copyright (c) 2010, Patrick "Ricky" Smith (ricky@digitally-born.com)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, are 
//  permitted provided that the following conditions are met:
// 
//  - Redistributions of source code must retain the above copyright notice, this list 
//    of conditions and the following disclaimer.
//  - Redistributions in binary form must reproduce the above copyright notice, this list 
//    of conditions and the following disclaimer in the documentation and/or other 
//    materials provided with the distribution.
//  - Neither the name of the Twitterizer nor the names of its contributors may be 
//    used to endorse or promote products derived from this software without specific 
//    prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
//  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
//  POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <author>Ricky Smith</author>
// <summary>The color translation helper class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
#if !SILVERLIGHT
    using System.Drawing;
#endif
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides common color converstion methods
    /// </summary>
    /// <tocexclude />
    internal static class ConversionUtility
    {
#if !SILVERLIGHT
        /// <summary>
        /// Converts the color string to a <see cref="System.Drawing.Color"/>
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="System.Drawing.Color"/> representation of the color, or null.</returns>
        internal static Color FromTwitterString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new Color();
            }

            if (Regex.IsMatch(value, @"^#?[a-f0-9]{6}$", RegexOptions.IgnoreCase))
            {
                return ColorTranslator.FromHtml(Regex.Replace(value, "^#?([a-f0-9]{6})$", "#$1", RegexOptions.IgnoreCase));
            }

            return Color.FromName(value);
        }
#endif

        /// <summary>
        /// Reads the stream into a byte array.
        /// </summary>
        /// <param name="responseStream">The response stream.</param>
        /// <returns>A byte array.</returns>
        internal static byte[] ReadStream(Stream responseStream)
        {
            byte[] data = new byte[32768];

            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                bool exit = false;
                while (!exit)
                {
                    int read = responseStream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        data = ms.ToArray();
                        exit = true;
                    }
                    else
                    {
                        ms.Write(buffer, 0, read);
                    }
                }
            }

            return data;
        }
    }
}
