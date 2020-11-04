/*
 * Encryptor.cs
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
	/// General class for encryption.
	/// </summary>
	public class Encryptor
	{
		private EncryptTransformer transformer;
		private byte[] initVec;
		private byte[] encKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="Encryptor"/> class.
        /// </summary>
        /// <param name="algId">The algorithm to encrypt data.</param>
		public Encryptor(EncryptionAlgorithm algId)
		{
			transformer = new EncryptTransformer(algId);
		}

        /// <summary>
        /// Encrypts the specified bytes data.
        /// </summary>
        /// <param name="bytesData">The bytes data.</param>
        /// <param name="bytesKey">The bytes key.</param>
        /// <returns></returns>
		public byte[] Encrypt(byte[] bytesData, byte[] bytesKey)
		{
			MemoryStream memStreamEncryptedData = new MemoryStream();

			transformer.IV = initVec;

			ICryptoTransform transform = transformer.GetCryptoServiceProvider(bytesKey);
			CryptoStream encStream = new CryptoStream(memStreamEncryptedData, transform, CryptoStreamMode.Write);

			try
			{
				encStream.Write(bytesData, 0, bytesData.Length);
			}
			catch(Exception ex)
			{
				throw new Exception("Error while writing encrypted data to the stream: \n" + ex.Message);
			}

			encKey = transformer.Key;
			initVec = transformer.IV;

			encStream.FlushFinalBlock();
			encStream.Close();

			return memStreamEncryptedData.ToArray();
		}

        /// <summary>
        /// Gets or sets the IV.
        /// </summary>
        /// <value>The IV.</value>
		public byte[] IV
		{
			get
			{
				return initVec;
			}
			set
			{
				initVec = value;
			}
		}

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
		public byte[] Key
		{
			get
			{
				return encKey;
			}
		}

	}
}
