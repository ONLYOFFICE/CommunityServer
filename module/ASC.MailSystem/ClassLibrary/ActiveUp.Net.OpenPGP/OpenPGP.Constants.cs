// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace ActiveUp.Net.Security.OpenPGP
{
    /// <summary>
    /// Constants Class
    /// </summary>
    public class Constants : NameValueCollection
    {
        // MPI counts
        // Public key packets
        public const int DSA_PUBLIC_KEY_MPI_COUNT = 4;
        public const int ELGAMAL_ENCRYPT_ONLY_PUBLIC_KEY_MPI_COUNT = 3;
        public const int RSA_PUBLIC_KEY_MPI_COUNT = 2;

        // Signature packets
        public const int DSA_SIGNATURE_MPI_COUNT = 2;

        // Secret key packets
        public const int RSA_SECRET_KEY_MPI_COUNT = 4;
        public const int DSA_SECRET_KEY_MPI_COUNT = 1;
        public const int ELGAMAL_ENCRYPT_ONLY_SECRET_KEY_MPI_COUNT = 1;
        
        // Cipher block sizes in bytes
        public const int IDEA_CIPHER_BLOCK_SIZE = 8;

        public static int GetPublicMPICount(PublicKeyAlgorithm pkAlgorithm)
        {
            if (pkAlgorithm.Equals(PublicKeyAlgorithm.DSA)) return Constants.DSA_PUBLIC_KEY_MPI_COUNT;
            else if (pkAlgorithm.Equals(PublicKeyAlgorithm.ElGamalEncryptOnly)) return Constants.ELGAMAL_ENCRYPT_ONLY_PUBLIC_KEY_MPI_COUNT;
            else if (pkAlgorithm.Equals(PublicKeyAlgorithm.RSA)) return Constants.RSA_PUBLIC_KEY_MPI_COUNT;
            else return 0;
        }
        public static int GetSecretMPICount(PublicKeyAlgorithm pkAlgorithm)
        {
            if (pkAlgorithm.Equals(PublicKeyAlgorithm.DSA)) return Constants.DSA_SECRET_KEY_MPI_COUNT;
            else if (pkAlgorithm.Equals(PublicKeyAlgorithm.ElGamalEncryptOnly)) return Constants.ELGAMAL_ENCRYPT_ONLY_SECRET_KEY_MPI_COUNT;
            else if (pkAlgorithm.Equals(PublicKeyAlgorithm.RSA)) return Constants.RSA_SECRET_KEY_MPI_COUNT;
            else return 0;
        }
        public static int GetTotalMPICount(PublicKeyAlgorithm pkAlgorithm)
        {
            return GetPublicMPICount(pkAlgorithm) + GetSecretMPICount(pkAlgorithm);
        }
        public static int GetCipherBlockSize(SymmetricKeyAlgorithm skAlgorithm)
        {
            if (skAlgorithm.Equals(SymmetricKeyAlgorithm.IDEA)) return Constants.IDEA_CIPHER_BLOCK_SIZE;
            //else if (skAlgorithm.Equals(PublicKeyAlgorithm.ElGamalEncryptOnly)) return Constants.ELGAMAL_ENCRYPT_ONLY_PUBLIC_KEY_MPI_COUNT;
            //else if (skAlgorithm.Equals(PublicKeyAlgorithm.RSA)) return Constants.RSA_PUBLIC_KEY_MPI_COUNT;
            else return 0;
        }
    }
}
