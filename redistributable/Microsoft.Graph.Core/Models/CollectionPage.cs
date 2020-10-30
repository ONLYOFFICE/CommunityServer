// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;

    /// <summary>
    /// A page of results from a collection.
    /// </summary>
    /// <typeparam name="T">The type of the item contained within the collection.</typeparam>
    public class CollectionPage<T> : ICollectionPage<T>
    {
        /// <summary>
        /// Creates the collection page.
        /// </summary>
        public CollectionPage()
        {
            this.CurrentPage = new List<T>();
        }

        /// <summary>
        /// Creates the collection page.
        /// </summary>
        /// <param name="currentPage">The current page.</param>
        public CollectionPage(IList<T> currentPage)
        {
            this.CurrentPage = currentPage;
        }

        /// <summary>
        /// The current page.
        /// </summary>
        public IList<T> CurrentPage { get; private set; }

        /// <summary>
        /// Get the index of an item in the current page.
        /// </summary>
        /// <param name="item">The item to get the index for.</param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            return this.CurrentPage.IndexOf(item);
        }

        /// <summary>
        /// Insert an item into the current page.
        /// </summary>
        /// <param name="index">The index to insert the item at.</param>
        /// <param name="item">The item to insert.</param>
        public void Insert(int index, T item)
        {
            this.CurrentPage.Insert(index, item);
        }

        /// <summary>
        /// Remove the item at the given index.
        /// </summary>
        /// <param name="index">The index to remove an item at.</param>
        public void RemoveAt(int index)
        {
            this.CurrentPage.RemoveAt(index);
        }

        /// <summary>
        /// Access the item at the given index.
        /// </summary>
        /// <param name="index">The item's index.</param>
        /// <returns>The item of type T.</returns>
        public T this[int index]
        {
            get { return this.CurrentPage[index]; }
            set { this.CurrentPage[index] = value; }
        }

        /// <summary>
        /// Add an item to the current page.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            this.CurrentPage.Add(item);
        }

        /// <summary>
        /// Remove all items from the current page.
        /// </summary>
        public void Clear()
        {
            this.CurrentPage.Clear();
        }


        /// <summary>
        /// Determine whether the current page contains the given item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>True if the item is found.</returns>
        public bool Contains(T item)
        {
            return this.CurrentPage.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the current page to the given array starting at the given index.
        /// </summary>
        /// <param name="array">The array to copy elements to.</param>
        /// <param name="arrayIndex">The start index.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CurrentPage.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements in the current page.
        /// </summary>
        public int Count
        {
            get { return this.CurrentPage.Count; }
        }

        /// <summary>
        /// Determines whether the current page is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return this.CurrentPage.IsReadOnly; }
        }

        /// <summary>
        /// Removes an item from the current page.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            return this.CurrentPage.Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the current page.
        /// </summary>
        /// <returns>The enumerator for the current page.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.CurrentPage.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.CurrentPage.GetEnumerator();
        }

        /// <summary>
        /// The additional data property bag.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}
