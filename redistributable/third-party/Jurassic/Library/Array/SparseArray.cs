using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Jurassic.Library
{
    /// <summary>
    /// Represents an array with non-consecutive elements.
    /// </summary>
    [Serializable]
    internal sealed class SparseArray
    {
        private const int NodeShift = 5;
        private const int NodeSize = 1 << NodeShift;
        private const uint NodeMask = NodeSize - 1;
        private const uint NodeInverseMask = ~NodeMask;

        [Serializable]
        private class Node
        {
            public object[] array;

            public Node()
            {
                this.array = new object[NodeSize];
            }

            public Node Clone()
            {
                Node clone = (Node)this.MemberwiseClone();
                clone.array = (object[])clone.array.Clone();
                return clone;
            }
        }

        private int depth;  // Depth of tree.
        private int mask;   // Valid index mask.
        private Node root;  // Root of tree.

        private object[] recent;        // Most recently accessed array.
        private uint recentStart = uint.MaxValue;   // The array index the most recent array starts at.


        public SparseArray()
        {
        }

        /// <summary>
        /// Creates a sparse array from the given dense array.
        /// </summary>
        /// <param name="array"> The array to copy items from. </param>
        /// <param name="length"> The number of items to copy. </param>
        /// <returns> A new sparse array containing the items from the given array. </returns>
        public static SparseArray FromDenseArray(object[] array, int length)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (length > array.Length)
                throw new ArgumentOutOfRangeException("length");
            var result = new SparseArray();
            result.CopyTo(array, 0, length);
            return result;
        }

        ///// <summary>
        ///// Gets the total number of items that can be stored in this tree without having to
        ///// increase the size of the tree.
        ///// </summary>
        //public uint Capacity
        //{
        //    get { return this.depth == 0 ? 0 : (uint)Math.Pow(NodeSize, this.depth); }
        //}

        public object this[uint index]
        {
            get
            {
                if ((index & NodeInverseMask) == this.recentStart)
                    return this.recent[index & NodeMask];
                return GetValueNonCached(index);
            }
            set
            {
                value = value ?? Undefined.Value;
                if ((index & NodeInverseMask) == this.recentStart)
                    this.recent[index & NodeMask] = value;
                else
                {
                    object[] array = FindOrCreateArray(index, writeAccess: true);
                    array[index & NodeMask] = value;
                }
            }
        }

        /// <summary>
        /// Deletes (sets to <c>null</c>) an array element.
        /// </summary>
        /// <param name="index"> The index of the array element to delete. </param>
        public void Delete(uint index)
        {
            if ((index & NodeInverseMask) == this.recentStart)
                this.recent[index & NodeMask] = null;
            else
            {
                object[] array = FindOrCreateArray(index, writeAccess: false);
                if (array == null)
                    return;
                array[index & NodeMask] = null;
            }
            uint t = 4294967295;
            double p = (double)t;
            if (p != 41)
                return;
            return;
        }

        /// <summary>
        /// Deletes (sets to <c>null</c>) a range of array elements.
        /// </summary>
        /// <param name="start"> The index of the first array element to delete. </param>
        /// <param name="length"> The number of array elements to delete. </param>
        public void DeleteRange(uint start, uint length)
        {
            if (this.root == null)
                return;
            DeleteRange(start, length, null, this.root, 0, this.depth);
        }

        /// <summary>
        /// Deletes (sets to <c>null</c>) a range of array elements.
        /// </summary>
        /// <param name="start"> The index of the first array element to delete. </param>
        /// <param name="length"> The number of array elements to delete. </param>
        /// <param name="parentNode"> The parent node of the node to delete from.  Can be <c>null</c>. </param>
        /// <param name="node"> The node to delete from. </param>
        /// <param name="nodeIndex"> The index of the node, in the parent node's array. </param>
        /// <param name="nodeDepth"> The depth of the tree, treating <paramref name="node"/> as the root. </param>
        private void DeleteRange(uint start, uint length, Node parentNode, Node node, int nodeIndex, int nodeDepth)
        {
            uint nodeLength = (NodeShift * nodeDepth) >= 32 ? uint.MaxValue : 1u << NodeShift * nodeDepth;
            uint nodeStart = nodeLength * (uint)nodeIndex;
            if (parentNode != null && (nodeStart >= start + length || nodeStart + nodeLength <= start))
            {
                // Delete the entire node.
                parentNode.array[nodeIndex] = null;
                return;
            }

            if (nodeDepth == 1)
            {
                // The node is a leaf node.
                for (int i = 0; i < NodeSize; i++)
                {
                    uint index = (uint)(nodeStart + i);
                    if (index >= start && index < start + length)
                        node.array[i] = null;
                }
            }
            else
            {
                // The node is a branch node.
                for (int i = 0; i < NodeSize; i++)
                {
                    var element = node.array[i] as Node;
                    if (element != null)
                    {
                        DeleteRange(start, length, node, element, i, nodeDepth - 1);
                    }
                }
            }
        }

        private object GetValueNonCached(uint index)
        {
            object[] array = FindOrCreateArray(index, writeAccess: false);
            if (array == null)
                return null;
            return array[index & NodeMask];
        }

        //public bool Exists(uint index)
        //{
        //    object[] array;
        //    if ((index & NodeInverseMask) == this.recentStart)
        //        array = this.recent;
        //    else
        //    {
        //        array = FindOrCreateArray(index, writeAccess: false);
        //        if (array == null)
        //            return false;
        //    }

        //    return array[index & NodeMask] != null;
        //}

        //public object RemoveAt(uint index)
        //{
        //    object[] array;
        //    if ((index & NodeInverseMask) == this.recentStart)
        //        array = this.recent;
        //    else
        //    {
        //        array = FindOrCreateArray(index, writeAccess: false);
        //        if (array == null)
        //            return null;
        //    }
        //    index &= index & NodeMask;
        //    object result = array[index];
        //    array[index] = null;
        //    if (result == NullPlaceholder.Value)
        //        return null;
        //    return result;
        //}

        private struct NodeInfo
        {
            public int depth;
            public uint index;
            public Node node;
        }

        public struct Range
        {
            public object[] Array;
            public uint StartIndex;
            public int Length { get { return this.Array.Length; } }
        }

        public IEnumerable<Range> Ranges
        {
            get
            {
                if (this.root == null)
                    yield break;

                var stack = new Stack<NodeInfo>();
                stack.Push(new NodeInfo() { depth = 1, index = 0, node = this.root });

                while (stack.Count > 0)
                {
                    NodeInfo info = stack.Pop();
                    Node node = info.node;
                    if (info.depth < this.depth)
                    {
                        for (uint i = NodeSize - 1; i != uint.MaxValue; i--)
                            if (node.array[i] != null)
                                stack.Push(new NodeInfo() { depth = info.depth + 1, index = info.index * NodeSize + i, node = (Node)node.array[i] });
                    }
                    else
                    {
                        yield return new Range() { Array = info.node.array, StartIndex = info.index * NodeSize };
                    }
                }
            }
        }

        private object[] FindOrCreateArray(uint index, bool writeAccess)
        {
            if (index < 0)
                return null;

            // Check if the index is out of bounds.
            if ((index & this.mask) != index || this.depth == 0)
            {
                if (writeAccess == false)
                    return null;

                // Create one or more new root nodes.
                do
                {
                    var newRoot = new Node();
                    newRoot.array[0] = this.root;
                    this.root = newRoot;
                    this.depth++;
                    this.mask = NodeShift * this.depth >= 32 ? -1 : (1 << NodeShift * this.depth) - 1;
                } while ((index & this.mask) != index);
            }

            // Find the node.
            Node current = this.root;
            for (int depth = this.depth - 1; depth > 0; depth--)
            {
                uint currentIndex = (index >> (depth * NodeShift)) & NodeMask;
                var newNode = (Node)current.array[currentIndex];
                if (newNode == null)
                {
                    if (writeAccess == false)
                        return null;
                    newNode = new Node();
                    current.array[currentIndex] = newNode;
                }
                current = newNode;
            }

            // Populate the MRU members.
            this.recent = current.array;
            this.recentStart = index & NodeInverseMask;

            return current.array;
        }

        /// <summary>
        /// Copies the elements of the sparse array to this sparse array, starting at a particular
        /// index.  Existing values are overwritten.
        /// </summary>
        /// <param name="source"> The sparse array to copy. </param>
        /// <param name="start"> The zero-based index at which copying begins. </param>
        /// <param name="length"> The number of elements to copy. </param>
        public void CopyTo(object[] source, uint start, int length)
        {
            int sourceOffset = 0;
            do
            {
                // Get a reference to the array to copy to.
                object[] dest = FindOrCreateArray(start, writeAccess: true);
                int destOffset = (int)(start & NodeMask);

                // Copy as much as possible.
                int copyLength = Math.Min(length - sourceOffset, dest.Length - destOffset);
                Array.Copy(source, sourceOffset, dest, destOffset, copyLength);

                start += (uint)copyLength;
                sourceOffset += copyLength;
            } while (sourceOffset < length);
        }

        /// <summary>
        /// Copies the elements of the given sparse array to this sparse array, starting at a
        /// particular index.  Existing values are overwritten.
        /// </summary>
        /// <param name="source"> The sparse array to copy. </param>
        /// <param name="start"> The zero-based index at which copying begins. </param>
        public void CopyTo(SparseArray source, uint start)
        {
            var originalStart = start;
            foreach (var sourceRange in source.Ranges)
            {
                int sourceOffset = 0;
                start = originalStart + sourceRange.StartIndex;
                do
                {
                    // Get a reference to the array to copy to.
                    object[] dest = FindOrCreateArray(start, writeAccess: true);
                    int destOffset = (int)(start & NodeMask);

                    // Copy as much as possible.
                    int copyLength = Math.Min(sourceRange.Length - sourceOffset, dest.Length - destOffset);
                    Array.Copy(sourceRange.Array, sourceOffset, dest, destOffset, copyLength);

                    start += (uint)copyLength;
                    sourceOffset += copyLength;
                } while (sourceOffset < sourceRange.Length);
            }
        }
    }
}
