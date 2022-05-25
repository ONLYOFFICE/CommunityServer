//
// IStructuralEquatable.cs
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2009 Novell
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET35

using System;
using System.Collections;

namespace net.openstack.Core
{
	/// <summary>
	/// Defines methods to support the comparison of objects for structural equality.
	/// </summary>
	public interface IStructuralEquatable {
		/// <summary>
		/// Determines whether an object is structurally equal to the current instance.
		/// </summary>
		/// <param name="other">The object to compare with the current instance.</param>
		/// <param name="comparer">An object that determines whether the current instance and <paramref name="other"/> are equal.</param>
		/// <returns><see langword="true"/> if the two objects are equal; otherwise, <see langword="false"/>.</returns>
		bool Equals (object other, IEqualityComparer comparer);

		/// <summary>
		/// Returns a hash code for the current instance.
		/// </summary>
		/// <param name="comparer">An object that computes the hash code of the current object.</param>
		/// <returns>The hash code for the current instance.</returns>
		int GetHashCode (IEqualityComparer comparer);
	}
}

#endif
