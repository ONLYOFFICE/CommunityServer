/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Extensions.GetReplicaInfoResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.IO;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Extensions
{
    /// <summary>
    ///     Retrieves the replica information from a GetReplicaInfoResponse object.
    ///     An object in this class is generated from an ExtendedResponse using the
    ///     ExtendedResponseFactory class.
    ///     The getReplicaInfoResponse extension uses the following OID:
    ///     2.16.840.1.113719.1.27.100.18
    /// </summary>
    public class GetReplicaInfoResponse : LdapExtendedResponse
    {
        // Other info as returned by the server
        private readonly int partitionID;
        private readonly int replicaState;
        private readonly int modificationTime;
        private readonly int purgeTime;
        private readonly int localPartitionID;
        private readonly string partitionDN;
        private readonly int replicaType;
        private readonly int flags;

        /// <summary>
        ///     Constructs an object from the responseValue which contains the
        ///     replica information.
        ///     The constructor parses the responseValue which has the following
        ///     format:
        ///     responseValue ::=
        ///     partitionID         INTEGER
        ///     replicaState        INTEGER
        ///     modificationTime    INTEGER
        ///     purgeTime           INTEGER
        ///     localPartitionID    INTEGER
        ///     partitionDN       OCTET STRING
        ///     replicaType         INTEGER
        ///     flags               INTEGER
        /// </summary>
        /// <exception>
        ///     IOException The response value could not be decoded.
        /// </exception>
        public GetReplicaInfoResponse(RfcLdapMessage rfcMessage) : base(rfcMessage)
        {
            if (ResultCode == LdapException.SUCCESS)
            {
                // parse the contents of the reply
                var returnedValue = Value;
                if (returnedValue == null)
                    throw new IOException("No returned value");

                // Create a decoder object
                var decoder = new LBERDecoder();
                if (decoder == null)
                    throw new IOException("Decoding error");

                // Parse the parameters in the order

                var currentPtr = new MemoryStream(SupportClass.ToByteArray(returnedValue));

                // Parse partitionID
                var asn1_partitionID = (Asn1Integer) decoder.decode(currentPtr);
                if (asn1_partitionID == null)
                    throw new IOException("Decoding error");

                partitionID = asn1_partitionID.intValue();


                // Parse replicaState
                var asn1_replicaState = (Asn1Integer) decoder.decode(currentPtr);
                if (asn1_replicaState == null)
                    throw new IOException("Decoding error");

                replicaState = asn1_replicaState.intValue();

                // Parse modificationTime
                var asn1_modificationTime = (Asn1Integer) decoder.decode(currentPtr);
                if (asn1_modificationTime == null)
                    throw new IOException("Decoding error");

                modificationTime = asn1_modificationTime.intValue();

                // Parse purgeTime
                var asn1_purgeTime = (Asn1Integer) decoder.decode(currentPtr);
                if (asn1_purgeTime == null)
                    throw new IOException("Decoding error");

                purgeTime = asn1_purgeTime.intValue();

                // Parse localPartitionID
                var asn1_localPartitionID = (Asn1Integer) decoder.decode(currentPtr);
                if (asn1_localPartitionID == null)
                    throw new IOException("Decoding error");

                localPartitionID = asn1_localPartitionID.intValue();

                // Parse partitionDN
                var asn1_partitionDN = (Asn1OctetString) decoder.decode(currentPtr);
                if (asn1_partitionDN == null)
                    throw new IOException("Decoding error");

                partitionDN = asn1_partitionDN.stringValue();
                if ((object) partitionDN == null)
                    throw new IOException("Decoding error");


                // Parse replicaType
                var asn1_replicaType = (Asn1Integer) decoder.decode(currentPtr);
                if (asn1_replicaType == null)
                    throw new IOException("Decoding error");

                replicaType = asn1_replicaType.intValue();


                // Parse flags
                var asn1_flags = (Asn1Integer) decoder.decode(currentPtr);
                if (asn1_flags == null)
                    throw new IOException("Decoding error");

                flags = asn1_flags.intValue();
            }
            else
            {
                partitionID = 0;
                replicaState = 0;
                modificationTime = 0;
                purgeTime = 0;
                localPartitionID = 0;
                partitionDN = "";
                replicaType = 0;
                flags = 0;
            }
        }


        /// <summary>
        ///     Returns the numeric identifier for the partition.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the partition ID.
        /// </returns>
        public virtual int getpartitionID()
        {
            return partitionID;
        }

        /// <summary>
        ///     Returns the current state of the replica.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the current state of the replica. See
        ///     ReplicationConstants class for possible values for this field.
        /// </returns>
        /// <seealso cref="ReplicationConstants.Ldap_RS_BEGIN_ADD">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_DEAD_REPLICA">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_DYING_REPLICA">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_JS_0">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_JS_1">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_JS_2">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_LOCKED">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_MASTER_DONE">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_MASTER_START">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_SS_0">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RS_TRANSITION_ON">
        /// </seealso>
        public virtual int getreplicaState()
        {
            return replicaState;
        }


        /// <summary>
        ///     Returns the time of the most recent modification.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the last modification time.
        /// </returns>
        public virtual int getmodificationTime()
        {
            return modificationTime;
        }


        /// <summary>
        ///     Returns the most recent time in which all data has been synchronized.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the last purge time.
        /// </returns>
        public virtual int getpurgeTime()
        {
            return purgeTime;
        }

        /// <summary>
        ///     Returns the local numeric identifier for the replica.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the local ID of the partition.
        /// </returns>
        public virtual int getlocalPartitionID()
        {
            return localPartitionID;
        }

        /// <summary>
        ///     Returns the distinguished name of the partition.
        /// </summary>
        /// <returns>
        ///     String value specifying the name of the partition read.
        /// </returns>
        public virtual string getpartitionDN()
        {
            return partitionDN;
        }

        /// <summary>
        ///     Returns the replica type.
        ///     See the ReplicationConstants class for possible values for
        ///     this field.
        /// </summary>
        /// <returns>
        ///     Integer identifying the type of the replica.
        /// </returns>
        /// <seealso cref="ReplicationConstants.Ldap_RT_MASTER">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RT_SECONDARY">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RT_READONLY">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RT_SUBREF">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RT_SPARSE_WRITE">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_RT_SPARSE_READ">
        /// </seealso>
        public virtual int getreplicaType()
        {
            return replicaType;
        }

        /// <summary>
        ///     Returns flags that specify whether the replica is busy or is a boundary.
        ///     See the ReplicationConstants class for possible values for
        ///     this field.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the flags for the replica.
        /// </returns>
        /// <seealso cref="ReplicationConstants.Ldap_DS_FLAG_BUSY">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.Ldap_DS_FLAG_BOUNDARY">
        /// </seealso>
        public virtual int getflags()
        {
            return flags;
        }
    }
}