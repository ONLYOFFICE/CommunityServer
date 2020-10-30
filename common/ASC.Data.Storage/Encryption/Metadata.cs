/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ASC.Data.Storage.Encryption
{
    public class Metadata : IMetadata
    {
        private const string prefixString = "AscEncrypted";

        private const int prefixLength = 12; // prefixString length
        private const int versionLength = 1; // byte
        private const int sizeLength = 8; // long (int64)

        private const int saltLength = 32; // The salt size must be 8 bytes or larger

        private const int keySize = 256; // key size, in bits, of the secret key used for the symmetric algorithm. AES-256
        private const int blockSize = 128; // block size, in bits, of the cryptographic operation. default is 128 bits

        private const int keyLength = keySize / 8; // secret key used for the symmetric algorithm. 32 bytes
        private const int ivLength = blockSize / 8; // The initialization vector (IV) to use for the symmetric algorithm. 16 bytes

        private const int hmacKeyLength = 64; // HMACSHA256 64-byte private key is recommended.
        private const int hmacHashLength = 32; // HMACSHA256 The output hash is 256 bits (32 bytes) in length

        private const int metadataLength = prefixLength + versionLength + sizeLength + saltLength + hmacHashLength + ivLength;

        private static int? iterations; // Rfc2898DeriveBytes: The minimum recommended number of iterations is 1000.

        private int Iterations
        {
            get
            {
                if (iterations.HasValue)
                {
                    return iterations.Value;
                }

                int iterationsCount;

                if (!int.TryParse(ConfigurationManagerExtension.AppSettings["storage.encryption.iterations"], out iterationsCount))
                {
                    iterationsCount = 4096;
                }

                iterations = iterationsCount;

                return iterations.Value;
            }
        }

        private string Password;

        private byte[] Prefix;
        private byte[] Version;
        private byte[] Size;

        private byte[] Salt;

        private byte[] Key;
        private byte[] IV;

        private byte[] HmacKey;
        private byte[] HmacHash;


        public void Initialize(string password)
        {
            Password = password;

            Prefix = Encoding.UTF8.GetBytes(prefixString);
            Version = new byte[versionLength];
            Size = new byte[sizeLength];

            Salt = new byte[saltLength];

            Key = new byte[keyLength];
            IV = new byte[ivLength];

            HmacKey = new byte[hmacKeyLength];
            HmacHash = new byte[hmacHashLength];
        }

        public void Initialize(byte version, string password, long fileSize)
        {
            Password = password;

            Prefix = Encoding.UTF8.GetBytes(prefixString);
            Version = new byte[versionLength] { version };
            Size = LongToByteArray(fileSize);

            Salt = GenerateRandom(saltLength);

            Key = GenerateKey();
            IV = GenerateRandom(ivLength);

            HmacKey = GenerateHmacKey();
            HmacHash = new byte[hmacHashLength]; // Empty byte array. The real hmac will be computed after encryption
        }


        public bool TryReadFromStream(Stream stream, byte cryptVersion)
        {
            try
            {
                var readed = stream.Read(Prefix, 0, prefixLength);
                if (readed < prefixLength) return false;

                if (Encoding.UTF8.GetString(Prefix) != prefixString) return false;

                readed = stream.Read(Version, 0, versionLength);
                if (readed < versionLength) return false;

                if (Version[0] != cryptVersion) return false;

                readed = stream.Read(Size, 0, sizeLength);
                if (readed < sizeLength) return false;

                if (ByteArrayToLong(Size) < 0) return false;

                readed = stream.Read(Salt, 0, saltLength);
                if (readed < saltLength) return false;

                readed = stream.Read(HmacHash, 0, hmacHashLength);
                if (readed < hmacHashLength) return false;

                readed = stream.Read(IV, 0, ivLength);
                if (readed < ivLength) return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void WriteToStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            stream.Write(Prefix, 0, prefixLength);
            stream.Write(Version, 0, versionLength);
            stream.Write(Size, 0, sizeLength);
            stream.Write(Salt, 0, saltLength);
            stream.Write(HmacHash, 0, hmacHashLength);
            stream.Write(IV, 0, ivLength);
        }

        public SymmetricAlgorithm GetCryptographyAlgorithm()
        {
            return new RijndaelManaged
            {
                KeySize = keySize,
                BlockSize = blockSize,
                Key = Key,
                IV = IV,
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC
            };
        }

        public void ComputeAndWriteHmacHash(Stream stream)
        {
            HmacHash = ComputeHmacHash(stream);

            stream.Seek(metadataLength - ivLength - hmacHashLength, SeekOrigin.Begin); // Move position to hmac

            stream.Write(HmacHash, 0, hmacHashLength); // Replace empty hmac with computed
        }

        public void ComputeAndValidateHmacHash(Stream stream)
        {
            Key = GenerateKey();

            HmacKey = GenerateHmacKey();

            var computedHash = ComputeHmacHash(stream);

            if (!HmacHash.SequenceEqual(computedHash))
            {
                stream.Close();

                throw new IntegrityProtectionException("Invalid signature");
            }

            stream.Seek(metadataLength, SeekOrigin.Begin); // Move position to encrypted data
        }

        public byte GetCryptoVersion()
        {
            return Version[0];
        }

        public long GetFileSize()
        {
            return ByteArrayToLong(Size);
        }

        public int GetMetadataLength()
        {
            return metadataLength;
        }


        private byte[] GenerateRandom(int length)
        {
            var random = new byte[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(random);
            }

            return random;
        }

        private byte[] GenerateKey()
        {
            var key = new byte[keyLength];

            using (var deriveBytes = new Rfc2898DeriveBytes(Password, Salt, Iterations, HashAlgorithmName.SHA256))
            {
                key = deriveBytes.GetBytes(keyLength);
            }

            return key;
        }

        private byte[] GenerateHmacKey()
        {
            var hmacKey = new byte[hmacKeyLength];

            using (var sha512 = new SHA512Managed())
            {
                hmacKey = sha512.ComputeHash(Key);
            }

            return hmacKey;
        }

        private byte[] ComputeHmacHash(Stream stream)
        {
            var hmacHash = new byte[hmacHashLength];

            stream.Seek(metadataLength - ivLength, SeekOrigin.Begin); // Move position to (IV + encrypted data)

            using (var hmac = new HMACSHA256(HmacKey))
            {
                hmacHash = hmac.ComputeHash(stream); // IV needs to be part of the MAC calculation
            }

            return hmacHash;
        }

        private byte[] LongToByteArray(long value)
        {
            var result = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        private long ByteArrayToLong(byte[] value)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(value);

            try
            {
                return BitConverter.ToInt64(value, 0);
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
