/*
 * WebDecrypter.cs
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
using System;
using System.Text;

namespace AjaxPro.Cryptography
{
	/// <summary>
	/// PC-Topp.NET decryptor for AuthenticationTicket, Drawings filename,...
	/// </summary>
	public class WebDecrypter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDecrypter"/> class.
        /// </summary>
		public WebDecrypter()
		{
		}

        /// <summary>
        /// Decrypts a string.
        /// </summary>
        /// <param name="Text">The encrypted string to be decrypted.</param>
        /// <param name="Key">The 8-bit string for decryption.</param>
        /// <returns>The plain text.</returns>
		public string Decrypt(string Text, string Key)
		{
			if(Key.Length != 8)
				throw new Exception("Key must be a 8-bit string!");

			byte[] IV = null;
			byte[] cipherText = null;
			byte[] key = null;
			byte[] plainText = null;

			try
			{
				Decryptor dec = new Decryptor(EncryptionAlgorithm.Des);

				IV = Encoding.ASCII.GetBytes("init vec");		// "init vec is big."

				dec.IV = IV;

				key = Encoding.ASCII.GetBytes(Key);
				cipherText = Convert.FromBase64String(Text);

				plainText = dec.Decrypt(cipherText, key);
			}
			catch(Exception ex)
			{
				throw new Exception("Exception while decrypting. " + ex.Message);
			}

			return Encoding.ASCII.GetString(plainText);
		}
	}
}
