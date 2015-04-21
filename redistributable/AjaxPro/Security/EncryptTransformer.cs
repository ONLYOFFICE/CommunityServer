/*
 * EncryptTransformer.cs
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
/*
 * MS	06-04-25	enums should have a zero value
 * 
 * 
 */
using System;
using System.Security.Cryptography;

namespace AjaxPro.Cryptography
{
	/// <summary>
	/// 
	/// </summary>
	public enum EncryptionAlgorithm
	{
		/// <summary>
		/// 
		/// </summary>
		Des = 0,
		
		/// <summary>
		/// 
		/// </summary>
		Rc2,
		
		/// <summary>
		/// 
		/// </summary>
		Rijndael,
		
		/// <summary>
		/// 
		/// </summary>
		TripleDes
	};

	/// <summary>
	/// 
	/// </summary>
	internal class EncryptTransformer
	{
		private EncryptionAlgorithm algorithmID;
		private byte[] initVec;
		private byte[] encKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptTransformer"/> class.
        /// </summary>
        /// <param name="algId">The alg id.</param>
		public EncryptTransformer(EncryptionAlgorithm algId)
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

					if(null == bytesKey)
					{
						encKey = des.Key;
					}
					else
					{
						des.Key = bytesKey;
						encKey = des.Key;
					}

					if(null == initVec)
					{
						initVec = des.IV;
					}
					else
					{
						des.IV = initVec;
					}
					return des.CreateEncryptor();

				case EncryptionAlgorithm.TripleDes:
					TripleDES des3 = new TripleDESCryptoServiceProvider();
					des3.Mode = CipherMode.CBC;

					if(null == bytesKey)
					{
						encKey = des3.Key;
					}
					else
					{
						des3.Key = bytesKey;
						encKey = des3.Key;
					}

					if(null == initVec)
					{
						initVec = des3.IV;
					}
					else
					{
						des3.IV = initVec;
					}
					return des3.CreateEncryptor();

				case EncryptionAlgorithm.Rc2:
					RC2 rc2 = new RC2CryptoServiceProvider();
					rc2.Mode = CipherMode.CBC;

					if(null == bytesKey)
					{
						encKey = rc2.Key;
					}
					else
					{
						rc2.Key = bytesKey;
						encKey = rc2.Key;
					}

					if(null == initVec)
					{
						initVec = rc2.IV;
					}
					else
					{
						rc2.IV = initVec;
					}
					return rc2.CreateEncryptor();

				case EncryptionAlgorithm.Rijndael:
					Rijndael rijndael = new RijndaelManaged();
					rijndael.Mode = CipherMode.CBC;

					if(null == bytesKey)
					{
						encKey = rijndael.Key;
					}
					else
					{
						rijndael.Key = bytesKey;
						encKey = rijndael.Key;
					}

					if(null == initVec)
					{
						initVec = rijndael.IV;
					}
					else
					{
						rijndael.IV = initVec;
					}
					return rijndael.CreateEncryptor();

				default:
					throw new CryptographicException("Algorithm ID '" + algorithmID + "' not supported!");
			}
		}

        /// <summary>
        /// Gets or sets the IV.
        /// </summary>
        /// <value>The IV.</value>
		internal byte[] IV
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
		internal byte[] Key
		{
			get
			{
				return encKey;
			}
		}

	}
}
