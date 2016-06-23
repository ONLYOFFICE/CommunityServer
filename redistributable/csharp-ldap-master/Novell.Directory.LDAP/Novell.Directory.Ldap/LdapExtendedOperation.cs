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
// Novell.Directory.Ldap.LdapExtendedOperation.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap
{

    /// <summary> Encapsulates an ID which uniquely identifies a particular extended
    /// operation, known to a particular server, and the data associated
    /// with that extended operation.
    /// 
    /// </summary>
    /// <seealso cref="LdapConnection.ExtendedOperation">
    /// </seealso>
    public class LdapExtendedOperation : ICloneable
    {

        private string oid;
        private sbyte[] vals;

        /// <summary> Constructs a new object with the specified object ID and data.
        /// 
        /// </summary>
        /// <param name="oid">    The unique identifier of the operation.
        /// 
        /// </param>
        /// <param name="vals">   The operation-specific data of the operation.
        /// </param>
        [CLSCompliantAttribute(false)]
        public LdapExtendedOperation(string oid, sbyte[] vals)
        {
            this.oid = oid;
            this.vals = vals;
        }

        /// <summary> Returns a clone of this object.
        /// 
        /// </summary>
        /// <returns> clone of this object.
        /// </returns>
        public object Clone()
        {
            try
            {
                object newObj = base.MemberwiseClone();
                //				Array.Copy((System.Array)SupportClass.ToByteArray( this.vals), 0, (System.Array)SupportClass.ToByteArray( ((LdapExtendedOperation) newObj).vals), 0, this.vals.Length);
                Array.Copy((Array)this.vals, 0, (Array)((LdapExtendedOperation)newObj).vals, 0, this.vals.Length);
                return newObj;
            }
            catch (Exception)
            {
                throw new SystemException("Internal error, cannot create clone");
            }
        }

        /// <summary> Returns the unique identifier of the operation.
        /// 
        /// </summary>
        /// <returns> The OID (object ID) of the operation.
        /// </returns>
        public virtual string getID()
        {
            return oid;
        }

        /// <summary> Returns a reference to the operation-specific data.
        /// 
        /// </summary>
        /// <returns> The operation-specific data.
        /// </returns>
        [CLSCompliantAttribute(false)]
        public virtual sbyte[] getValue()
        {
            return vals;
        }

        /// <summary>  Sets the value for the operation-specific data.
        /// 
        /// </summary>
        /// <param name="newVals"> The byte array of operation-specific data.
        /// </param>
        [CLSCompliantAttribute(false)]
        protected internal virtual void setValue(sbyte[] newVals)
        {
            this.vals = newVals;
        }

        /// <summary>  Resets the OID for the operation to a new value
        /// 
        /// </summary>
        /// <param name="newoid"> The new OID for the operation
        /// </param>
        protected internal virtual void setID(string newoid)
        {
            this.oid = newoid;
        }
    }
}
