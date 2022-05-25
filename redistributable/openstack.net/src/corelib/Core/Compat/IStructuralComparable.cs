//
// IStructuralComparable.cs
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
	/// Supports the structural comparison of collection objects.
	/// </summary>
	public interface IStructuralComparable {
		/// <summary>
		/// Determines whether the current collection object precedes, occurs in the same position as, or follows another object in the sort order.
		/// </summary>
		/// <param name="other">The object to compare with the current instance.</param>
		/// <param name="comparer">An object that compares members of the current collection object with the corresponding members of <paramref name="other"/>.</param>
		/// <returns>
		/// An integer that indicates the relationship of the current collection object to other, as shown in the following table.
		///
		/// <list type="table">
		/// <listheader>
		/// <term>Return value</term>
		/// <term>Description</term>
		/// </listheader>
		/// <item>
		/// <term>-1</term>
		/// <term>The current instance precedes <paramref name="other"/>.</term>
		/// </item>
		/// <item>
		/// <term>0</term>
		/// <term>The current instance and <paramref name="other"/> are equal.</term>
		/// </item>
		/// <item>
		/// <term>1</term>
		/// <term>The current instance follows <paramref name="other"/>.</term>
		/// </item>
		/// </list>
		/// </returns>
		/// <exception cref="ArgumentException">This instance and <paramref name="other"/> are not the same type.</exception>
		int CompareTo (object other, IComparer comparer);
	}
}

#endif
