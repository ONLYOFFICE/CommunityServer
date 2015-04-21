/*
 * DecryptTransformer.cs
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
using System.Security.Cryptography;

namespace AjaxPro.Cryptography
{
	/// <summary>
	/// 
	/// </summary>
	internal class DecryptTransformer
	{
		private EncryptionAlgorithm algorithmID;
		private byte[] initVec;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptTransformer"/> class.
        /// </summary>
        /// <param name="algId">The alg id.</param>
		public DecryptTransformer(EncryptionAlgorithm algId)
		{
			algorithmID = algId;
		}

        /// <summary>
        /// Gets the crypto service provider.
        /// </summary>
        /// <param name="bytesKey">The bytes key.</param>
        /// <returns></returns>
		internal ICryptoTransform GetCryptoServiceProvider(byte[] bytesKey)
		{
			switch(algorithmID)
			{
				case EncryptionAlgorithm.Des:
					DES des = new DESCryptoServiceProvider();
					des.Mode = CipherMode.CBC;
					des.Key = bytesKey;
					des.IV = initVec;
					return des.CreateDecryptor();

				case EncryptionAlgorithm.TripleDes:
					TripleDES des3 = new TripleDESCryptoServiceProvider();
					des3.Mode = CipherMode.CBC;
					return des3.CreateDecryptor(bytesKey, initVec);

				case EncryptionAlgorithm.Rc2:
					RC2 rc2 = new RC2CryptoServiceProvider();
					rc2.Mode = CipherMode.CBC;
					return rc2.CreateDecryptor(bytesKey, initVec);

				case EncryptionAlgorithm.Rijndael:
					Rijndael rijndael = new RijndaelManaged();
					rijndael.Mode = CipherMode.CBC;
					return rijndael.CreateDecryptor(bytesKey, initVec);

				default:
					throw new CryptographicException("Algorithm ID '" + algorithmID + "' not supported!");
			}
		}

        /// <summary>
        /// Sets the IV.
        /// </summary>
        /// <value>The IV.</value>
		internal byte[] IV
		{
			set
			{
				initVec = value;
			}
		}
	}
}
