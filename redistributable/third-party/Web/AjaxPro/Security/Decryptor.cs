/*
 * Decryptor.cs
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
using System.IO;
using System.Security.Cryptography;

namespace AjaxPro.Cryptography
{
	/// <summary>
	/// General class for decryption.
	/// </summary>
	public class Decryptor
	{
		private DecryptTransformer transformer;
		private byte[] initVec;

        /// <summary>
        /// Initializes a new instance of the <see cref="Decryptor"/> class.
        /// </summary>
        /// <param name="algId">The algorithm to decrypt data.</param>
		public Decryptor(EncryptionAlgorithm algId)
		{
			transformer = new DecryptTransformer(algId);
		}

        /// <summary>
        /// Decrypts the specified bytes data.
        /// </summary>
        /// <param name="bytesData">The bytes data.</param>
        /// <param name="bytesKey">The bytes key.</param>
        /// <returns></returns>
		public byte[] Decrypt(byte[] bytesData, byte[] bytesKey)
		{
			MemoryStream memStreamDecryptedData = new MemoryStream();

			transformer.IV = initVec;

			ICryptoTransform transform = transformer.GetCryptoServiceProvider(bytesKey);
			CryptoStream decStream = new CryptoStream(memStreamDecryptedData, transform, CryptoStreamMode.Write);

			try
			{
				decStream.Write(bytesData, 0, bytesData.Length);
			}
			catch(Exception ex)
			{
				throw new Exception("Error while writing encrypted data to the stream: \n" + ex.Message);
			}

			decStream.FlushFinalBlock();
			decStream.Close();

			return memStreamDecryptedData.ToArray();
		}

        /// <summary>
        /// Sets the IV.
        /// </summary>
        /// <value>The IV.</value>
		public byte[] IV
		{
			set
			{
				initVec = value;
			}
		}
	}
}
