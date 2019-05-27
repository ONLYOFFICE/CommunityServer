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
// Novell.Directory.Ldap.MessageVector.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Linq;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     The <code>MessageVector</code> class implements additional semantics
    ///     to Vector needed for handling messages.
    /// </summary>
    internal class MessageVector : ArrayList
    {
        internal MessageVector(int cap, int incr) : base(cap)
        {
        }

        /// <summary>
        ///     Returns an array containing all of the elements in this MessageVector.
        ///     The elements returned are in the same order in the array as in the
        ///     Vector.  The contents of the vector are cleared.
        /// </summary>
        /// <returns>
        ///     the array containing all of the elements.
        /// </returns>
        internal virtual object[] RemoveAll()
        {
            lock (this)
            {
                var results = ToArray();
                Clear();
                return results;
            }
        }

        /// <summary>
        ///     Finds the Message object with the given MsgID, and returns the Message
        ///     object. It finds the object and returns it in an atomic operation.
        /// </summary>
        /// <param name="msgId">
        ///     The msgId of the Message object to return
        /// </param>
        /// <returns>
        ///     The Message object corresponding to this MsgId.
        ///     @throws NoSuchFieldException when no object with the corresponding
        ///     value for the MsgId field can be found.
        /// </returns>
        internal Message FindMessageById(int msgId)
        {
            lock (this)
            {
                var message = this.OfType<Message>().SingleOrDefault(m => m.MessageID == msgId);
                if (message == null)
                {
                    throw new FieldAccessException();
                }
                return message;
            }
        }
    }
}