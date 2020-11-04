/*
 * JavaScriptConverterList.cs
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
 * MS	06-05-30	removed this code
 * MS	06-10-04	added code again for .NET 1.1 (Hashtable sort is different)
 * 
 * 
 */
#if(!NET20)
using System;
using System.Collections;

namespace AjaxPro
{
	/// <summary>
	/// Represents a IDictionary of JavaScriptConverters.
	/// </summary>
	internal class JavaScriptConverterList : IDictionary
	{
		private Hashtable keys;
		private ArrayList values;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptConverterList"/> class.
        /// </summary>
		public JavaScriptConverterList()
		{
			keys = new Hashtable();
			values = new ArrayList();
		}

        /// <summary>
        /// Gets the <see cref="AjaxPro.IJavaScriptConverter"/> with the specified key.
        /// </summary>
        /// <value></value>
		public IJavaScriptConverter this[string key]
		{
			get
			{
				return (IJavaScriptConverter)this[key];
			}
		}

        /// <summary>
        /// Gets the <see cref="AjaxPro.IJavaScriptConverter"/> at the specified index.
        /// </summary>
        /// <value></value>
		public IJavaScriptConverter this[int index]
		{
			get
			{
				object o = values[index];
				return (IJavaScriptConverter)o;
			}
		}

		

		#region IDictionary Members

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <param name="key">The <see cref="T:System.Object"></see> to use as the key of the element to add.</param>
        /// <param name="value">The <see cref="T:System.Object"></see> to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.IDictionary"></see> object. </exception>
        /// <exception cref="T:System.ArgumentNullException">key is null. </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size. </exception>
		public void Add(object key, object value)
		{
			if (value as IJavaScriptConverter == null)
				return;

			lock (this.SyncRoot)
			{
				int idx = values.Add(value);
				keys.Add(key, idx);
			}
		}

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> object is read-only. </exception>
		public void Clear()
		{
			lock (this.SyncRoot)
			{
				values.Clear();
				keys.Clear();
			}
		}

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
		public bool ContainsKey(object key)
		{
			return Contains(key);
		}

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IDictionary"></see> object contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary"></see> object.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.IDictionary"></see> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null. </exception>
		public bool Contains(object key)
		{
			return keys.Contains(key);
		}

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </returns>
		public IDictionaryEnumerator GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size; otherwise, false.</returns>
		public bool IsFixedSize
		{
			get { return false; }
		}

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.IDictionary"></see> object is read-only; otherwise, false.</returns>
		public bool IsReadOnly
		{
			get { return false; }
		}

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection"></see> object containing the keys of the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.ICollection"></see> object containing the keys of the <see cref="T:System.Collections.IDictionary"></see> object.</returns>
		public ICollection Keys
		{
			get
			{
				return keys;
			}
		}

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> object is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size. </exception>
        /// <exception cref="T:System.ArgumentNullException">key is null. </exception>
		public void Remove(object key)
		{
			lock (this.SyncRoot)
			{
				if (keys.Contains(key))
				{
					int idx = (int)keys[key];
					keys.Remove(key);
					values.RemoveAt(idx);
				}
			}
		}

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection"></see> object containing the values in the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.ICollection"></see> object containing the values in the <see cref="T:System.Collections.IDictionary"></see> object.</returns>
		public ICollection Values
		{
			get
			{
				return values;
			}
		}

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value></value>
		public object this[object key]
		{
			get
			{
				return values[(int)keys[key]];
			}
			set
			{
				lock (this.SyncRoot)
				{
					values[(int)keys[key]] = value;
				}
			}
		}

		#endregion

#region ICollection Members

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">array is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
        /// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception>
		public void CopyTo(Array array, int index)
		{
			throw new Exception("The method or operation is not implemented.");
		}

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.</returns>
		public int Count
		{
			get
			{
				return keys.Count;
			}
		}

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.</returns>
		public bool IsSynchronized
		{
			get
			{
				return values.IsSynchronized;
			}
		}

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.</returns>
		public object SyncRoot
		{
			get
			{
				return values.SyncRoot;
			}
		}

		#endregion

#region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
#endif