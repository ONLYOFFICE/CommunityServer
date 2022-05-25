//
// Tuple.cs
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

namespace net.openstack.Core
{
	/// <summary>
	/// Provides static methods for creating tuple objects.
	/// </summary>
	public static class Tuple
	{
		/// <summary>
		/// Creates a new 8-tuple, or octuple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
		/// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
		/// <typeparam name="T8">The type of the eighth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <param name="item6">The value of the sixth component of the tuple.</param>
		/// <param name="item7">The value of the seventh component of the tuple.</param>
		/// <param name="item8">The value of the eighth component of the tuple.</param>
		/// <returns>An 8-tuple (octuple) whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>, <paramref name="item4"/>, <paramref name="item5"/>, <paramref name="item6"/>, <paramref name="item7"/>, <paramref name="item8"/>).</returns>
		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4,
			 T5 item5,
			 T6 item6,
			 T7 item7,
			 T8 item8) {
			return new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> (item1, item2, item3, item4, item5, item6, item7, new Tuple<T8> (item8));
		}

		/// <summary>
		/// Creates a new 7-tuple, or septuple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
		/// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <param name="item6">The value of the sixth component of the tuple.</param>
		/// <param name="item7">The value of the seventh component of the tuple.</param>
		/// <returns>A 7-tuple (septuple) whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>, <paramref name="item4"/>, <paramref name="item5"/>, <paramref name="item6"/>, <paramref name="item7"/>).</returns>
		public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4,
			 T5 item5,
			 T6 item6,
			 T7 item7) {
			return new Tuple<T1, T2, T3, T4, T5, T6, T7> (item1, item2, item3, item4, item5, item6, item7);
		}

		/// <summary>
		/// Creates a new 6-tuple, or sextuple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <param name="item6">The value of the sixth component of the tuple.</param>
		/// <returns>A 6-tuple (sextuple) whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>, <paramref name="item4"/>, <paramref name="item5"/>, <paramref name="item6"/>).</returns>
		public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4,
			 T5 item5,
			 T6 item6) {
			return new Tuple<T1, T2, T3, T4, T5, T6> (item1, item2, item3, item4, item5, item6);
		}

		/// <summary>
		/// Creates a new 5-tuple, or quintuple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <returns>A 5-tuple (quintuple) whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>, <paramref name="item4"/>, <paramref name="item5"/>).</returns>
		public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4,
			 T5 item5) {
			return new Tuple<T1, T2, T3, T4, T5> (item1, item2, item3, item4, item5);
		}

		/// <summary>
		/// Creates a new 4-tuple, or quadruple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <returns>A 4-tuple (quadruple) whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>, <paramref name="item4"/>).</returns>
		public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3,
			 T4 item4) {
			return new Tuple<T1, T2, T3, T4> (item1, item2, item3, item4);
		}

		/// <summary>
		/// Creates a new 3-tuple, or triple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <returns>A 3-tuple (triple) whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>).</returns>
		public static Tuple<T1, T2, T3> Create<T1, T2, T3>
			(
			 T1 item1,
			 T2 item2,
			 T3 item3) {
			return new Tuple<T1, T2, T3> (item1, item2, item3);
		}

		/// <summary>
		/// Creates a new 2-tuple, or pair.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <returns>A 2-tuple (pair) whose value is (<paramref name="item1"/>, <paramref name="item2"/>).</returns>
		public static Tuple<T1, T2> Create<T1, T2>
			(
			 T1 item1,
			 T2 item2) {
			return new Tuple<T1, T2> (item1, item2);
		}

		/// <summary>
		/// Creates a new 1-tuple, or singleton.
		/// </summary>
		/// <typeparam name="T1">The type of the only component of the tuple.</typeparam>
		/// <param name="item1">The value of the only component of the tuple.</param>
		/// <returns>A 1-tuple (singleton) whose value is (<paramref name="item1"/>).</returns>
		public static Tuple<T1> Create<T1>
			(
			 T1 item1) {
			return new Tuple<T1> (item1);
		}
	}		
}
	
#endif
