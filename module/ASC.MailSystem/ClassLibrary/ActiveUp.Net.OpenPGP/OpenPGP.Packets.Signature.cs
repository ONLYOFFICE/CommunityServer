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
using System.Text;
using System.IO;
using ActiveUp.Net.Security.OpenPGP;
using ActiveUp.Net.OpenPGP;

namespace ActiveUp.Net.Security.OpenPGP.Packets
{
    /// <summary>
    /// Signature Class
    /// </summary>
    public class Signature : Packet
    {
        public static Signature Parse(Packet input)
        {
            Signature sig = (Signature)input;
            
            MemoryStream ms = new MemoryStream(sig.RawData);
            //ActiveUp.Net.Mail.Logger.AddEntry(sig.RawData.Length.ToString());
            //ActiveUp.Net.Mail.Logger.AddEntry(ms.Length.ToString());
            sig.VersionNumber = ms.ReadByte();

            int next = ms.ReadByte();
            if (sig.VersionNumber.Equals(3))
            {
                if (!next.Equals(5)) throw new InvalidPacketSyntaxException("Invalid syntax for a version 3 signature packet. Second byte is not 5.");
                else
                {
                    sig.Type = (SignatureType)ms.ReadByte();

                    byte[] creation = new byte[4];
                    ms.Read(creation, 0, 4);
                    sig.CreationTime = Converter.UnixTimeStampToDateTime(Converter.ToInt(creation));

                    byte[] keyID = new byte[8];
                    ms.Read(keyID, 0, 8);
                    sig.IssuerKeyID = Converter.ToULong(keyID);
                }
            }
            else
            {
                // Vérifier tout avec une signature V4 !
                sig.Type = (SignatureType)next;
            }
            sig.PublicKeyAlgorithm = (PublicKeyAlgorithm)ms.ReadByte();
            sig.HashAlgorithm = (HashAlgorithm)ms.ReadByte();

            // Add version 4 subpackets
            if (sig.VersionNumber.Equals(4))
            {
                // Hashed subpackets
                byte[] hashedsublengthbytes = new byte[2];
                ms.Read(hashedsublengthbytes, 0, 2);
                short hashedsublength = Converter.ToShort(hashedsublengthbytes);
                while (ms.Position < 5 + hashedsublength)
                    sig.HashedSubPackets.Add(GetNextSignatureSubPacket(ref ms));

                // Unhashed subpackets
                byte[] unhashedsublengthbytes = new byte[2];
                ms.Read(unhashedsublengthbytes, 0, 2);
                short unhashedsublength = Converter.ToShort(unhashedsublengthbytes);
                while (ms.Position < hashedsublength + unhashedsublength)
                    sig.UnHashedSubPackets.Add(GetNextSignatureSubPacket(ref ms));

                // Fill properties
                List<SignatureSubPacket> allSubPackets = new List<SignatureSubPacket>();
                allSubPackets.AddRange(sig.HashedSubPackets);
                allSubPackets.AddRange(sig.UnHashedSubPackets);
                foreach (SignatureSubPacket spacket in allSubPackets)
                {
                    if (spacket.Type.Equals(SignatureSubPacketType.CreationTime))
                        sig.CreationTime = Converter.UnixTimeStampToDateTime(Converter.ToInt(spacket.Value));
                    else if (spacket.Type.Equals(SignatureSubPacketType.KeyExpirationTime))
                        sig.KeyExpiration = Converter.UnixTimeStampToDateTime(Converter.ToInt(spacket.Value));
                    else if (spacket.Type.Equals(SignatureSubPacketType.ExpirationTime))
                        sig.SignatureExpiration = Converter.UnixTimeStampToDateTime(Converter.ToInt(spacket.Value));
                    else if (spacket.Type.Equals(SignatureSubPacketType.ExportableCertification))
                        sig.IsExportableCertification = (spacket.Value[0] == 1);
                    else if (spacket.Type.Equals(SignatureSubPacketType.Revocable))
                        sig.IsRevocable = (spacket.Value[0] == 1);
                    else if (spacket.Type.Equals(SignatureSubPacketType.TrustSignature))
                    {
                        sig.Trust.Level = (TrustLevel)spacket.Value[0];
                        sig.Trust.Amount = spacket.Value[1];
                    }
                    else if (spacket.Type.Equals(SignatureSubPacketType.RegularExpression))
                        sig.RegularExpression = spacket.Value;
                    else if (spacket.Type.Equals(SignatureSubPacketType.RevocationKey))
                    {
                        if ((spacket.Value[0] & 128) != 128) throw new InvalidPacketSyntaxException("First bit of revocation key class is 0");
                        sig.RevocationKey.Class = (RevocationKeyClass)(spacket.Value[0] & 64);
                        sig.RevocationKey.HashAlgorithm = (HashAlgorithm)spacket.Value[1];
                        Array.Copy(spacket.Value, 2, sig.RevocationKey.FingerPrint, 0, 20);
                    }
                    else if (spacket.Type.Equals(SignatureSubPacketType.NotationData))
                    {
                        Notation n = new Notation();
                        n.IsHumanReadable = ((spacket.Value[0] & 128) == 128);
                        byte[] nameLength = new byte[2];
                        Array.Copy(spacket.Value, 4, nameLength, 0, 2);
                        Array.Copy(spacket.Value, 6, n.Name, 0, Converter.ToInt(nameLength));
                        byte[] valueLength = new byte[2];
                        Array.Copy(spacket.Value, 6 + Converter.ToInt(nameLength), valueLength, 0, 2);
                        Array.Copy(spacket.Value, 6 + Converter.ToInt(nameLength) + 2, n.Value, 0, Converter.ToInt(valueLength));
                        sig.Notations.Add(n);
                    }
                    else if (spacket.Type.Equals(SignatureSubPacketType.PolicyURL))
                        sig.PolicyURL = Encoding.ASCII.GetString(spacket.Value,0,spacket.Value.Length);
                    else if (spacket.Type.Equals(SignatureSubPacketType.SignerUserID))
                        sig.SignerUserID = Encoding.ASCII.GetString(spacket.Value,0,spacket.Value.Length);
                    else if (spacket.Type.Equals(SignatureSubPacketType.SignatureTarget))
                    {
                        sig.TargetSignatureInfo.PublicKeyAlgorithm = (PublicKeyAlgorithm)spacket.Value[0];
                        sig.TargetSignatureInfo.HashAlgorithm = (HashAlgorithm)spacket.Value[1];
                        Array.Copy(spacket.Value, 2, sig.TargetSignatureInfo.Hash, 0, spacket.Value.Length - 2);
                    }
                    else if (spacket.Type.Equals(SignatureSubPacketType.EmbeddedSignature))
                    {
                        Packet embedded = new Packet();
                        embedded.RawData = spacket.Value;
                        sig.EmbeddedSignature = Signature.Parse(embedded);
                    }
                    else if (spacket.Type.Equals(SignatureSubPacketType.IssuerKeyID))
                        sig.IssuerKeyID = Converter.ToULong(spacket.Value);
                    else if (spacket.Type.Equals(SignatureSubPacketType.PrimaryUserID))
                        sig.IsPrimaryUserID = (spacket.Value[0] == 1);
                    else if (spacket.Type.Equals(SignatureSubPacketType.KeyFlags))
                    {
                        if ((spacket.Value[0] & (byte)KeyFlag.Authentication) == (int)KeyFlag.Authentication) sig.KeyFlags.Add(KeyFlag.Authentication);
                        if ((spacket.Value[0] & (byte)KeyFlag.CertifiesOtherKeys) == (int)KeyFlag.CertifiesOtherKeys) sig.KeyFlags.Add(KeyFlag.CertifiesOtherKeys);
                        if ((spacket.Value[0] & (byte)KeyFlag.EncryptsCommunications) == (int)KeyFlag.EncryptsCommunications) sig.KeyFlags.Add(KeyFlag.EncryptsCommunications);
                        if ((spacket.Value[0] & (byte)KeyFlag.EncryptsStorage) == (int)KeyFlag.EncryptsStorage) sig.KeyFlags.Add(KeyFlag.EncryptsStorage);
                        if ((spacket.Value[0] & (byte)KeyFlag.GroupKey) == (int)KeyFlag.GroupKey) sig.KeyFlags.Add(KeyFlag.GroupKey);
                        if ((spacket.Value[0] & (byte)KeyFlag.SignsData) == (int)KeyFlag.SignsData) sig.KeyFlags.Add(KeyFlag.SignsData);
                        if ((spacket.Value[0] & (byte)KeyFlag.SplitKey) == (int)KeyFlag.SplitKey) sig.KeyFlags.Add(KeyFlag.SplitKey);
                    }
                    else if (spacket.Type.Equals(SignatureSubPacketType.Features))
                    {
                        if ((spacket.Value[0] & (byte)Feature.ModificationDetection) == (int)Feature.ModificationDetection) sig.Features.Add(Feature.ModificationDetection);
                    }
                    else if (spacket.Type.Equals(SignatureSubPacketType.KeyServerPreferences))
                    {
                        if ((spacket.Value[0] & (byte)KeyServerPreference.NoModify) == (int)KeyServerPreference.NoModify) sig.KeyServerPreferences.Add(KeyServerPreference.NoModify);
                    }
                    else if (spacket.Type.Equals(SignatureSubPacketType.ReasonForRevocation))
                    {
                        if ((spacket.Value[0] & (byte)Reason.KeyCompromised) == (int)Reason.KeyCompromised) sig.ReasonForRevocation.Reason = Reason.KeyCompromised;
                        else if ((spacket.Value[0] & (byte)Reason.KeyRetired) == (int)Reason.KeyRetired) sig.ReasonForRevocation.Reason = Reason.KeyRetired;
                        else if ((spacket.Value[0] & (byte)Reason.KeySuperceded) == (int)Reason.KeySuperceded) sig.ReasonForRevocation.Reason = Reason.KeySuperceded;
                        else if ((spacket.Value[0] & (byte)Reason.NoneSpecified) == (int)Reason.NoneSpecified) sig.ReasonForRevocation.Reason = Reason.NoneSpecified;
                        else if ((spacket.Value[0] & (byte)Reason.UserIDNoLongerValid) == (int)Reason.UserIDNoLongerValid) sig.ReasonForRevocation.Reason = Reason.UserIDNoLongerValid;
                        byte[] comment = new byte[spacket.Value.Length - 1];
                        Array.Copy(spacket.Value, 1, comment, 0, comment.Length);
                        sig.ReasonForRevocation.Comment = Encoding.UTF8.GetString(comment,0,comment.Length);
                    }
                    else if (spacket.Type.Equals(SignatureSubPacketType.PreferredCompressionAlgorithms))
                        for (int i = 0; i < spacket.Value.Length; i++) sig.PreferredCompressionAlgorithms.Add((CompressionAlgorithm)spacket.Value[i]);
                    else if (spacket.Type.Equals(SignatureSubPacketType.PreferredHashAlgorithms))
                        for (int i = 0; i < spacket.Value.Length; i++) sig.PreferredHashAlgorithms.Add((HashAlgorithm)spacket.Value[i]);
                    else if (spacket.Type.Equals(SignatureSubPacketType.PreferredSymmetricAlgorithms))
                        for (int i = 0; i < spacket.Value.Length; i++) sig.PreferredSymmetricAlgorithms.Add((SymmetricKeyAlgorithm)spacket.Value[i]);
                }
            }

            sig.QuickCheck = new byte[2];
            ms.Read(sig.QuickCheck, 0, 2);

            // MPIs
            while (ms.Position < ms.Length)
            {
                byte first = (byte)ms.ReadByte();
                byte second = (byte)ms.ReadByte();
                short length = (short)((Converter.ToShort(new byte[2] { first, second }) + 7) / 8);
                byte[] mpi = new byte[(int)length];
                ms.Read(mpi, 0, length);
                sig.MPIs.Add(mpi);
            }

            return sig;
        }
        
        private static SignatureSubPacket GetNextSignatureSubPacket(ref MemoryStream instream)
        {
            SignatureSubPacket subpacket = new SignatureSubPacket();

            byte next = (byte)instream.ReadByte();
            if (next < 192) subpacket.TypeAndBodyLength = next;
            else if (next > 191 && next < 223)
            {
                byte nextnext = (byte)instream.ReadByte();
                subpacket.TypeAndBodyLength = ((next - 192) << 8) + nextnext + 192;
            }
            else if (next == 255)
            {
                int nextnext = instream.ReadByte();
                int nextnextnext = instream.ReadByte();
                int nextnextnextnext = instream.ReadByte();
                int nextnextnextnextnext = instream.ReadByte();
                subpacket.TypeAndBodyLength = (nextnext << 24) | (nextnextnext << 16) | (nextnextnextnext << 8) | nextnextnextnextnext;
            }

            subpacket.Type = (SignatureSubPacketType)instream.ReadByte();
            subpacket.Value = new byte[subpacket.TypeAndBodyLength - 1];
            
            instream.Read(subpacket.Value, 0, subpacket.TypeAndBodyLength - 1);

            return subpacket;
        }

        private int _versionNumber;
        private DateTime _creation;
        private SignatureType _type;
        private ulong _keyID;
        private PublicKeyAlgorithm _pkAlg;
        private HashAlgorithm _hashAlg;
        private List<SignatureSubPacket> _hashedSubPackets = new List<SignatureSubPacket>();
        private List<SignatureSubPacket> _unHashedSubPackets = new List<SignatureSubPacket>();
        private byte[] _quickCheck;
        private List<byte[]> _mpis = new List<byte[]>();

        /*internal*/public int VersionNumber
        {
            get { return this._versionNumber; }
            set { this._versionNumber = value; }
        }
        /*internal*/public DateTime CreationTime
        {
            get { return this._creation; }
            set { this._creation = value; }
        }
        /*internal*/public new SignatureType Type
        {
            get { return this._type; }
            set { this._type = value; }
        }
        /*internal*/public ulong IssuerKeyID
        {
            get { return this._keyID; }
            set { this._keyID = value; }
        }
        /*internal*/public PublicKeyAlgorithm PublicKeyAlgorithm
        {
            get { return this._pkAlg; }
            set { this._pkAlg = value; }
        }
        /*internal*/public HashAlgorithm HashAlgorithm
        {
            get { return this._hashAlg; }
            set { this._hashAlg = value; }
        }
        /*internal*/public List<SignatureSubPacket> HashedSubPackets
        {
            get { return this._hashedSubPackets; }
            set { this._hashedSubPackets = value; }
        }
        /*internal*/public List<SignatureSubPacket> UnHashedSubPackets
        {
            get { return this._unHashedSubPackets; }
            set { this._unHashedSubPackets = value; }
        }
        /*internal*/public byte[] QuickCheck
        {
            get { return this._quickCheck; }
            set { this._quickCheck = value; }
        }
        /*internal*/public List<byte[]> MPIs
        {
            get { return this._mpis; }
            set { this._mpis = value; }
        }

        // Version 4 specific properties
        private DateTime _keyExpiration, _sigExpiration;
        private List<SymmetricKeyAlgorithm> _pSymAlgs = new List<SymmetricKeyAlgorithm>();
        private List<HashAlgorithm> _pHashAlgs = new List<HashAlgorithm>();
        private List<CompressionAlgorithm> _pCompAlgs = new List<CompressionAlgorithm>();
        private bool _isExportableCertification, _isRevocable;
        private byte[] _regExp;
        private RevocationKey _revocationKey;
        private string _keyServer, _policy, _signerUID;
        private List<Notation> _notations = new List<Notation>();
        private List<KeyServerPreference> _keyServerPreferences = new List<KeyServerPreference>();
        private bool _isPrimaryUserID;
        private List<Feature> _features = new List<Feature>();
        private List<KeyFlag> _keyFlags = new List<KeyFlag>();
        private TargetSignatureInfo _targetSignature;
        private Signature _embedded;
        private Trust _trust;
        private ReasonForRevocation _reason = new ReasonForRevocation();

        public DateTime KeyExpiration
        {
            get { return this._keyExpiration; }
            set { this._keyExpiration = value; }
        }
        public DateTime SignatureExpiration
        {
            get { return this._sigExpiration; }
            set { this._sigExpiration = value; }
        }
        public List<SymmetricKeyAlgorithm> PreferredSymmetricAlgorithms
        {
            get { return this._pSymAlgs; }
            set { this._pSymAlgs = value; }
        }
        public List<HashAlgorithm> PreferredHashAlgorithms
        {
            get { return this._pHashAlgs; }
            set { this._pHashAlgs = value; }
        }
        public List<CompressionAlgorithm> PreferredCompressionAlgorithms
        {
            get { return this._pCompAlgs; }
            set { this._pCompAlgs = value; }
        }
        public bool IsExportableCertification
        {
            get { return this._isExportableCertification; }
            set { this._isExportableCertification = value; }
        }
        public bool IsRevocable
        {
            get { return this._isRevocable; }
            set { this._isRevocable = value; }
        }
        public bool IsPrimaryUserID
        {
            get { return this._isPrimaryUserID; }
            set { this._isPrimaryUserID = value; }
        }
        public byte[] RegularExpression
        {
            get { return this._regExp; }
            set { this._regExp = value; }
        }
        public RevocationKey RevocationKey
        {
            get { return this._revocationKey; }
            set { this._revocationKey = value; }
        }
        public string PreferredKeyServer
        {
            get { return this._keyServer; }
            set { this._keyServer = value; }
        }
        public string PolicyURL
        {
            get { return this._policy; }
            set { this._policy = value; }
        }
        public string SignerUserID
        {
            get { return this._signerUID; }
            set { this._signerUID = value; }
        }
        public List<Notation> Notations
        {
            get { return this._notations; }
            set { this._notations = value; }
        }
        public List<KeyServerPreference> KeyServerPreferences
        {
            get { return this._keyServerPreferences; }
            set { this._keyServerPreferences = value; }
        }
        public List<Feature> Features
        {
            get { return this._features; }
            set { this._features = value; }
        }
        public List<KeyFlag> KeyFlags
        {
            get { return this._keyFlags; }
            set { this._keyFlags = value; }
        }
        public Trust Trust
        {
            get { return this._trust; }
            set { this._trust = value; }
        }
        public TargetSignatureInfo TargetSignatureInfo
        {
            get { return this._targetSignature; }
            set { this._targetSignature = value; }
        }
        public Signature EmbeddedSignature
        {
            get { return this._embedded; }
            set { this._embedded = value; }
        }
        public ReasonForRevocation ReasonForRevocation
        {
            get { return this._reason; }
            set { this._reason = value; }
        }
    }
}
