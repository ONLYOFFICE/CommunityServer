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
// Novell.Directory.Ldap.SupportClass.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

// Support classes replicate the functionality of the original code, but in some cases they are 
// substantially different architecturally. Although every effort is made to preserve the 
// original architecture of the application in the converted project, the user should be aware that 
// the primary goal of these support classes is to replicate functionality, and that at times 
// the architecture of the resulting solution may differ somewhat.
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

/// <summary>
///     This interface should be implemented by any class whose instances are intended
///     to be executed by a thread.
/// </summary>
public interface IThreadRunnable
{
    /// <summary>
    ///     This method has to be implemented in order that starting of the thread causes the object's
    ///     run method to be called in that separately executing thread.
    /// </summary>
    void Run();
}


public class Integer32 : object
{
    private int _wintv;

    public Integer32(int ival)
    {
        _wintv = ival;
    }

    public int intValue
    {
        get { return _wintv; }
        set { _wintv = value; }
    }
}

/// <summary>
///     Contains conversion support elements such as classes, interfaces and static methods.
/// </summary>
public class SupportClass
{
    /// <summary>
    ///     Receives a byte array and returns it transformed in an sbyte array
    /// </summary>
    /// <param name="byteArray">Byte array to process</param>
    /// <returns>The transformed array</returns>
    [CLSCompliant(false)]
    public static sbyte[] ToSByteArray(byte[] byteArray)
    {
        var sbyteArray = new sbyte[byteArray.Length];
        for (var index = 0; index < byteArray.Length; index++)
            sbyteArray[index] = (sbyte) byteArray[index];
        return sbyteArray;
    }

    /*******************************/

    /// <summary>
    ///     Converts an array of sbytes to an array of bytes
    /// </summary>
    /// <param name="sbyteArray">The array of sbytes to be converted</param>
    /// <returns>The new array of bytes</returns>
    [CLSCompliant(false)]
    public static byte[] ToByteArray(sbyte[] sbyteArray)
    {
        var byteArray = new byte[sbyteArray.Length];
        for (var index = 0; index < sbyteArray.Length; index++)
            byteArray[index] = (byte) sbyteArray[index];
        return byteArray;
    }

    /// <summary>
    ///     Converts a string to an array of bytes
    /// </summary>
    /// <param name="sourceString">The string to be converted</param>
    /// <returns>The new array of bytes</returns>
    public static byte[] ToByteArray(string sourceString)
    {
        var byteArray = new byte[sourceString.Length];
        for (var index = 0; index < sourceString.Length; index++)
            byteArray[index] = (byte) sourceString[index];
        return byteArray;
    }

    /// <summary>
    ///     Converts a array of object-type instances to a byte-type array.
    /// </summary>
    /// <param name="tempObjectArray">Array to convert.</param>
    /// <returns>An array of byte type elements.</returns>
    public static byte[] ToByteArray(object[] tempObjectArray)
    {
        var byteArray = new byte[tempObjectArray.Length];
        for (var index = 0; index < tempObjectArray.Length; index++)
            byteArray[index] = (byte) tempObjectArray[index];
        return byteArray;
    }


    /*******************************/

    /// <summary>
    ///     Reads a number of characters from the current source Stream and writes the data to the target array at the
    ///     specified index.
    /// </summary>
    /// <param name="sourceStream">The source Stream to read from.</param>
    /// <param name="target">Contains the array of characteres read from the source Stream.</param>
    /// <param name="start">The starting index of the target array.</param>
    /// <param name="count">The maximum number of characters to read from the source Stream.</param>
    /// <returns>
    ///     The number of characters read. The number will be less than or equal to count depending on the data available
    ///     in the source Stream. Returns -1 if the end of the stream is reached.
    /// </returns>
    [CLSCompliant(false)]
    public static int ReadInput(Stream sourceStream, ref sbyte[] target, int start, int count)
    {
        // Returns 0 bytes if not enough space in target
        if (target.Length == 0)
            return 0;

        var receiver = new byte[target.Length];
        var bytesRead = 0;
        var startIndex = start;
        var bytesToRead = count;
        while (bytesToRead > 0)
        {
            var n = sourceStream.Read(receiver, startIndex, bytesToRead);
            if (n == 0)
                break;
            bytesRead += n;
            startIndex += n;
            bytesToRead -= n;
        }
        // Returns -1 if EOF
        if (bytesRead == 0)
            return -1;

        for (var i = start; i < start + bytesRead; i++)
            target[i] = (sbyte) receiver[i];

        return bytesRead;
    }

    /// <summary>
    ///     Reads a number of characters from the current source TextReader and writes the data to the target array at the
    ///     specified index.
    /// </summary>
    /// <param name="sourceTextReader">The source TextReader to read from</param>
    /// <param name="target">Contains the array of characteres read from the source TextReader.</param>
    /// <param name="start">The starting index of the target array.</param>
    /// <param name="count">The maximum number of characters to read from the source TextReader.</param>
    /// <returns>
    ///     The number of characters read. The number will be less than or equal to count depending on the data available
    ///     in the source TextReader. Returns -1 if the end of the stream is reached.
    /// </returns>
    [CLSCompliant(false)]
    public static int ReadInput(TextReader sourceTextReader, ref sbyte[] target, int start, int count)
    {
        // Returns 0 bytes if not enough space in target
        if (target.Length == 0) return 0;

        var charArray = new char[target.Length];
        var bytesRead = sourceTextReader.Read(charArray, start, count);

        // Returns -1 if EOF
        if (bytesRead == 0) return -1;

        for (var index = start; index < start + bytesRead; index++)
            target[index] = (sbyte) charArray[index];

        return bytesRead;
    }

    /*******************************/

    /// <summary>
    ///     This method returns the literal value received
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static long Identity(long literal)
    {
        return literal;
    }

    /// <summary>
    ///     This method returns the literal value received
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    [CLSCompliant(false)]
    public static ulong Identity(ulong literal)
    {
        return literal;
    }

    /// <summary>
    ///     This method returns the literal value received
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static float Identity(float literal)
    {
        return literal;
    }

    /// <summary>
    ///     This method returns the literal value received
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static double Identity(double literal)
    {
        return literal;
    }

    /*******************************/

    /// <summary>
    ///     The class performs token processing from strings
    /// </summary>
    public class Tokenizer
    {
        //Element list identified
        private ArrayList elements;
        //Source string to use
        private string source;
        //The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character
        private string delimiters = " \t\n\r";

        private readonly bool returnDelims;

        /// <summary>
        ///     Initializes a new class instance with a specified string to process
        /// </summary>
        /// <param name="source">String to tokenize</param>
        public Tokenizer(string source)
        {
            elements = new ArrayList();
            elements.AddRange(source.Split(delimiters.ToCharArray()));
            RemoveEmptyStrings();
            this.source = source;
        }

        /// <summary>
        ///     Initializes a new class instance with a specified string to process
        ///     and the specified token delimiters to use
        /// </summary>
        /// <param name="source">String to tokenize</param>
        /// <param name="delimiters">String containing the delimiters</param>
        public Tokenizer(string source, string delimiters)
        {
            elements = new ArrayList();
            this.delimiters = delimiters;
            elements.AddRange(source.Split(this.delimiters.ToCharArray()));
            RemoveEmptyStrings();
            this.source = source;
        }

        public Tokenizer(string source, string delimiters, bool retDel)
        {
            elements = new ArrayList();
            this.delimiters = delimiters;
            this.source = source;
            returnDelims = retDel;
            if (returnDelims)
                Tokenize();
            else
                elements.AddRange(source.Split(this.delimiters.ToCharArray()));
            RemoveEmptyStrings();
        }

        private void Tokenize()
        {
            var tempstr = source;
            var toks = "";
            if (tempstr.IndexOfAny(delimiters.ToCharArray()) < 0 && tempstr.Length > 0)
            {
                elements.Add(tempstr);
            }
            else if (tempstr.IndexOfAny(delimiters.ToCharArray()) < 0 && tempstr.Length <= 0)
            {
                return;
            }
            while (tempstr.IndexOfAny(delimiters.ToCharArray()) >= 0)
            {
                if (tempstr.IndexOfAny(delimiters.ToCharArray()) == 0)
                {
                    if (tempstr.Length > 1)
                    {
                        elements.Add(tempstr.Substring(0, 1));
                        tempstr = tempstr.Substring(1);
                    }
                    else
                        tempstr = "";
                }
                else
                {
                    toks = tempstr.Substring(0, tempstr.IndexOfAny(delimiters.ToCharArray()));
                    elements.Add(toks);
                    elements.Add(tempstr.Substring(toks.Length, 1));
                    if (tempstr.Length > toks.Length + 1)
                    {
                        tempstr = tempstr.Substring(toks.Length + 1);
                    }
                    else
                        tempstr = "";
                }
            }
            if (tempstr.Length > 0)
            {
                elements.Add(tempstr);
            }
        }

        /// <summary>
        ///     Current token count for the source string
        /// </summary>
        public int Count
        {
            get { return elements.Count; }
        }

        /// <summary>
        ///     Determines if there are more tokens to return from the source string
        /// </summary>
        /// <returns>True or false, depending if there are more tokens</returns>
        public bool HasMoreTokens()
        {
            return elements.Count > 0;
        }

        /// <summary>
        ///     Returns the next token from the token list
        /// </summary>
        /// <returns>The string value of the token</returns>
        public string NextToken()
        {
            string result;
            if (source == "") throw new Exception();
            if (returnDelims)
            {
//						Tokenize();
                RemoveEmptyStrings();
                result = (string) elements[0];
                elements.RemoveAt(0);
                return result;
            }
            elements = new ArrayList();
            elements.AddRange(source.Split(delimiters.ToCharArray()));
            RemoveEmptyStrings();
            result = (string) elements[0];
            elements.RemoveAt(0);
            source = source.Remove(source.IndexOf(result), result.Length);
            source = source.TrimStart(delimiters.ToCharArray());
            return result;
        }

        /// <summary>
        ///     Returns the next token from the source string, using the provided
        ///     token delimiters
        /// </summary>
        /// <param name="delimiters">String containing the delimiters to use</param>
        /// <returns>The string value of the token</returns>
        public string NextToken(string delimiters)
        {
            this.delimiters = delimiters;
            return NextToken();
        }

        /// <summary>
        ///     Removes all empty strings from the token list
        /// </summary>
        private void RemoveEmptyStrings()
        {
            for (var index = 0; index < elements.Count; index++)
                if ((string) elements[index] == "")
                {
                    elements.RemoveAt(index);
                    index--;
                }
        }
    }

    /*******************************/

    /// <summary>
    ///     Provides support for DateFormat
    /// </summary>
    public class DateTimeFormatManager
    {
        public static DateTimeFormatHashTable manager = new DateTimeFormatHashTable();

        /// <summary>
        ///     Hashtable class to provide functionality for dateformat properties
        /// </summary>
        public class DateTimeFormatHashTable : Hashtable
        {
            /// <summary>
            ///     Sets the format for datetime.
            /// </summary>
            /// <param name="format">DateTimeFormat instance to set the pattern</param>
            /// <param name="newPattern">A string with the pattern format</param>
            public void SetDateFormatPattern(DateTimeFormatInfo format, string newPattern)
            {
                if (this[format] != null)
                    ((DateTimeFormatProperties) this[format]).DateFormatPattern = newPattern;
                else
                {
                    var tempProps = new DateTimeFormatProperties();
                    tempProps.DateFormatPattern = newPattern;
                    Add(format, tempProps);
                }
            }

            /// <summary>
            ///     Gets the current format pattern of the DateTimeFormat instance
            /// </summary>
            /// <param name="format">The DateTimeFormat instance which the value will be obtained</param>
            /// <returns>The string representing the current datetimeformat pattern</returns>
            public string GetDateFormatPattern(DateTimeFormatInfo format)
            {
                if (this[format] == null)
                    return "d-MMM-yy";
                return ((DateTimeFormatProperties) this[format]).DateFormatPattern;
            }

            /// <summary>
            ///     Sets the datetimeformat pattern to the giving format
            /// </summary>
            /// <param name="format">The datetimeformat instance to set</param>
            /// <param name="newPattern">The new datetimeformat pattern</param>
            public void SetTimeFormatPattern(DateTimeFormatInfo format, string newPattern)
            {
                if (this[format] != null)
                    ((DateTimeFormatProperties) this[format]).TimeFormatPattern = newPattern;
                else
                {
                    var tempProps = new DateTimeFormatProperties();
                    tempProps.TimeFormatPattern = newPattern;
                    Add(format, tempProps);
                }
            }

            /// <summary>
            ///     Gets the current format pattern of the DateTimeFormat instance
            /// </summary>
            /// <param name="format">The DateTimeFormat instance which the value will be obtained</param>
            /// <returns>The string representing the current datetimeformat pattern</returns>
            public string GetTimeFormatPattern(DateTimeFormatInfo format)
            {
                if (this[format] == null)
                    return "h:mm:ss tt";
                return ((DateTimeFormatProperties) this[format]).TimeFormatPattern;
            }

            /// <summary>
            ///     Internal class to provides the DateFormat and TimeFormat pattern properties on .NET
            /// </summary>
            private class DateTimeFormatProperties
            {
                public string DateFormatPattern = "d-MMM-yy";
                public string TimeFormatPattern = "h:mm:ss tt";
            }
        }
    }

    /*******************************/

    /// <summary>
    ///     Gets the DateTimeFormat instance and date instance to obtain the date with the format passed
    /// </summary>
    /// <param name="format">The DateTimeFormat to obtain the time and date pattern</param>
    /// <param name="date">The date instance used to get the date</param>
    /// <returns>A string representing the date with the time and date patterns</returns>
    public static string FormatDateTime(DateTimeFormatInfo format, DateTime date)
    {
        var timePattern = DateTimeFormatManager.manager.GetTimeFormatPattern(format);
        var datePattern = DateTimeFormatManager.manager.GetDateFormatPattern(format);
        return date.ToString(datePattern + " " + timePattern, format);
    }

    /*******************************/

    /// <summary>
    ///     Adds a new key-and-value pair into the hash table
    /// </summary>
    /// <param name="collection">The collection to work with</param>
    /// <param name="key">Key used to obtain the value</param>
    /// <param name="newValue">Value asociated with the key</param>
    /// <returns>The old element associated with the key</returns>
    public static object PutElement(IDictionary collection, object key, object newValue)
    {
        var element = collection[key];
        collection[key] = newValue;
        return element;
    }

    /*******************************/

    /// <summary>
    ///     This class contains static methods to manage arrays.
    /// </summary>
    public class ArrayListSupport
    {
        /// <summary>
        ///     Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <param name="collection">The collection from wich to obtain the elements.</param>
        /// <param name="objects">The array containing all the elements of the collection.</param>
        /// <returns>The array containing all the elements of the collection.</returns>
        public static object[] ToArray(ArrayList collection, object[] objects)
        {
            var index = 0;
            var tempEnumerator = collection.GetEnumerator();
            while (tempEnumerator.MoveNext())
                objects[index++] = tempEnumerator.Current;
            return objects;
        }
    }


    /*******************************/

    /// <summary>
    ///     Removes the first occurrence of an specific object from an ArrayList instance.
    /// </summary>
    /// <param name="arrayList">The ArrayList instance</param>
    /// <param name="element">The element to remove</param>
    /// <returns>True if item is found in the ArrayList; otherwise, false</returns>
    public static bool VectorRemoveElement(ArrayList arrayList, object element)
    {
        var containsItem = arrayList.Contains(element);
        arrayList.Remove(element);
        return containsItem;
    }

    /*******************************/

    /// <summary>
    ///     Support class used to handle threads
    /// </summary>
    public class ThreadClass : IThreadRunnable
    {
        /// <summary>
        ///     The instance of System.Threading.Thread
        /// </summary>
        private Thread threadField;

        private bool isStopping;

        /// <summary>
        ///     Initializes a new instance of the ThreadClass class
        /// </summary>
        public ThreadClass()
        {
            threadField = new Thread(Run);
        }

        /// <summary>
        ///     Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Name">The name of the thread</param>
        public ThreadClass(string Name)
        {
            threadField = new Thread(Run);
            this.Name = Name;
        }

        /// <summary>
        ///     Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        public ThreadClass(ThreadStart Start)
        {
            threadField = new Thread(Start);
        }

        /// <summary>
        ///     Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        /// <param name="Name">The name of the thread</param>
        public ThreadClass(ThreadStart Start, string Name)
        {
            threadField = new Thread(Start);
            this.Name = Name;
        }

        /// <summary>
        ///     This method has no functionality unless the method is overridden
        /// </summary>
        public virtual void Run()
        {
        }

        /// <summary>
        ///     Causes the operating system to change the state of the current thread instance to ThreadState.Running
        /// </summary>
        public virtual void Start()
        {
            threadField.Start();
        }

        ///// <summary>
        ///// Interrupts a thread that is in the WaitSleepJoin thread state
        ///// </summary>
        //public virtual void Interrupt()
        //{
        //	threadField.Interrupt();
        //}


        public virtual void Stop()
        {
            isStopping = true;
        }

        /// <summary>
        ///     Gets the current thread instance
        /// </summary>
        public Thread Instance
        {
            get { return threadField; }
            set { threadField = value; }
        }

        /// <summary>
        ///     Gets or sets the name of the thread
        /// </summary>
        public string Name
        {
            get { return threadField.Name; }
            set
            {
                if (threadField.Name == null)
                    threadField.Name = value;
            }
        }

        /// <summary>
        ///     Gets a value indicating the execution status of the current thread
        /// </summary>
        public bool IsAlive
        {
            get { return threadField.IsAlive; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not a thread is a background thread.
        /// </summary>
        public bool IsBackground
        {
            get { return threadField.IsBackground; }
            set { threadField.IsBackground = value; }
        }

        public bool IsStopping
        {
            get { return isStopping; }
        }

        /// <summary>
        ///     Blocks the calling thread until a thread terminates
        /// </summary>
        public void Join()
        {
            threadField.Join();
        }

        /// <summary>
        ///     Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="MiliSeconds">Time of wait in milliseconds</param>
        public void Join(int MiliSeconds)
        {
            lock (this)
            {
                threadField.Join(MiliSeconds * 10000);
            }
        }

        /// <summary>
        ///     Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="MiliSeconds">Time of wait in milliseconds</param>
        /// <param name="NanoSeconds">Time of wait in nanoseconds</param>
        public void Join(int MiliSeconds, int NanoSeconds)
        {
            lock (this)
            {
                threadField.Join(MiliSeconds * 10000 + NanoSeconds * 100);
            }
        }


        /// <summary>
        ///     Obtain a String that represents the current Object
        /// </summary>
        /// <returns>A String that represents the current Object</returns>
        public override string ToString()
        {
            return "Thread[" + Name + "]";
        }

        /// <summary>
        ///     Gets the currently running thread
        /// </summary>
        /// <returns>The currently running thread</returns>
        public static ThreadClass Current()
        {
            var CurrentThread = new ThreadClass();
            CurrentThread.Instance = Thread.CurrentThread;
            return CurrentThread;
        }
    }


    /*******************************/

    /// <summary>
    ///     This class contains different methods to manage Collections.
    /// </summary>
    public class CollectionSupport : CollectionBase
    {
        /// <summary>
        ///     Adds an specified element to the collection.
        /// </summary>
        /// <param name="element">The element to be added.</param>
        /// <returns>Returns true if the element was successfuly added. Otherwise returns false.</returns>
        public virtual bool Add(object element)
        {
            return List.Add(element) != -1;
        }

        /// <summary>
        ///     Adds all the elements contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(ICollection collection)
        {
            var result = false;
            if (collection != null)
            {
                var tempEnumerator = new ArrayList(collection).GetEnumerator();
                while (tempEnumerator.MoveNext())
                {
                    if (tempEnumerator.Current != null)
                        result = Add(tempEnumerator.Current);
                }
            }
            return result;
        }


        /// <summary>
        ///     Adds all the elements contained in the specified support class collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(CollectionSupport collection)
        {
            return AddAll((ICollection) collection);
        }

        /// <summary>
        ///     Verifies if the specified element is contained into the collection.
        /// </summary>
        /// <param name="element"> The element that will be verified.</param>
        /// <returns>Returns true if the element is contained in the collection. Otherwise returns false.</returns>
        public virtual bool Contains(object element)
        {
            return List.Contains(element);
        }

        /// <summary>
        ///     Verifies if all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
        public virtual bool ContainsAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = new ArrayList(collection).GetEnumerator();
            while (tempEnumerator.MoveNext())
                if (!(result = Contains(tempEnumerator.Current)))
                    break;
            return result;
        }

        /// <summary>
        ///     Verifies if all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
        public virtual bool ContainsAll(CollectionSupport collection)
        {
            return ContainsAll((ICollection) collection);
        }

        /// <summary>
        ///     Verifies if the collection is empty.
        /// </summary>
        /// <returns>Returns true if the collection is empty. Otherwise returns false.</returns>
        public virtual bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        ///     Removes an specified element from the collection.
        /// </summary>
        /// <param name="element">The element to be removed.</param>
        /// <returns>Returns true if the element was successfuly removed. Otherwise returns false.</returns>
        public virtual bool Remove(object element)
        {
            var result = false;
            if (Contains(element))
            {
                List.Remove(element);
                result = true;
            }
            return result;
        }

        /// <summary>
        ///     Removes all the elements contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
        public virtual bool RemoveAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = new ArrayList(collection).GetEnumerator();
            while (tempEnumerator.MoveNext())
            {
                if (Contains(tempEnumerator.Current))
                    result = Remove(tempEnumerator.Current);
            }
            return result;
        }

        /// <summary>
        ///     Removes all the elements contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
        public virtual bool RemoveAll(CollectionSupport collection)
        {
            return RemoveAll((ICollection) collection);
        }

        /// <summary>
        ///     Removes all the elements that aren't contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
        public virtual bool RetainAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = GetEnumerator();
            var tempCollection = new CollectionSupport();
            tempCollection.AddAll(collection);
            while (tempEnumerator.MoveNext())
                if (!tempCollection.Contains(tempEnumerator.Current))
                {
                    result = Remove(tempEnumerator.Current);

                    if (result)
                    {
                        tempEnumerator = GetEnumerator();
                    }
                }
            return result;
        }

        /// <summary>
        ///     Removes all the elements that aren't contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
        public virtual bool RetainAll(CollectionSupport collection)
        {
            return RetainAll((ICollection) collection);
        }

        /// <summary>
        ///     Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <returns>The array containing all the elements of the collection</returns>
        public virtual object[] ToArray()
        {
            var index = 0;
            var objects = new object[Count];
            var tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
                objects[index++] = tempEnumerator.Current;
            return objects;
        }

        /// <summary>
        ///     Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <param name="objects">The array into which the elements of the collection will be stored.</param>
        /// <returns>The array containing all the elements of the collection.</returns>
        public virtual object[] ToArray(object[] objects)
        {
            var index = 0;
            var tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
                objects[index++] = tempEnumerator.Current;
            return objects;
        }

        /// <summary>
        ///     Creates a CollectionSupport object with the contents specified in array.
        /// </summary>
        /// <param name="array">The array containing the elements used to populate the new CollectionSupport object.</param>
        /// <returns>A CollectionSupport object populated with the contents of array.</returns>
        public static CollectionSupport ToCollectionSupport(object[] array)
        {
            var tempCollectionSupport = new CollectionSupport();
            tempCollectionSupport.AddAll(array);
            return tempCollectionSupport;
        }
    }

    /*******************************/

    /// <summary>
    ///     This class contains different methods to manage list collections.
    /// </summary>
    public class ListCollectionSupport : ArrayList
    {
        /// <summary>
        ///     Creates a new instance of the class ListCollectionSupport.
        /// </summary>
        public ListCollectionSupport()
        {
        }

        /// <summary>
        ///     Creates a new instance of the class ListCollectionSupport.
        /// </summary>
        /// <param name="collection">The collection to insert into the new object.</param>
        public ListCollectionSupport(ICollection collection) : base(collection)
        {
        }

        /// <summary>
        ///     Creates a new instance of the class ListCollectionSupport with the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the new array.</param>
        public ListCollectionSupport(int capacity) : base(capacity)
        {
        }

        /// <summary>
        ///     Adds an object to the end of the List.
        /// </summary>
        /// <param name="valueToInsert">The value to insert in the array list.</param>
        /// <returns>Returns true after adding the value.</returns>
        public virtual bool Add(object valueToInsert)
        {
            Insert(Count, valueToInsert);
            return true;
        }

        /// <summary>
        ///     Adds all the elements contained into the specified collection, starting at the specified position.
        /// </summary>
        /// <param name="index">Position at which to add the first element from the specified collection.</param>
        /// <param name="list">The list used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(int index, IList list)
        {
            var result = false;
            if (list != null)
            {
                var tempEnumerator = new ArrayList(list).GetEnumerator();
                var tempIndex = index;
                while (tempEnumerator.MoveNext())
                {
                    Insert(tempIndex++, tempEnumerator.Current);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        ///     Adds all the elements contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(IList collection)
        {
            return AddAll(Count, collection);
        }

        /// <summary>
        ///     Adds all the elements contained in the specified support class collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(CollectionSupport collection)
        {
            return AddAll(Count, collection);
        }

        /// <summary>
        ///     Adds all the elements contained into the specified support class collection, starting at the specified position.
        /// </summary>
        /// <param name="index">Position at which to add the first element from the specified collection.</param>
        /// <param name="list">The list used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(int index, CollectionSupport collection)
        {
            return AddAll(index, (IList) collection);
        }

        /// <summary>
        ///     Creates a copy of the ListCollectionSupport.
        /// </summary>
        /// <returns> A copy of the ListCollectionSupport.</returns>
        public virtual object ListCollectionClone()
        {
            return MemberwiseClone();
        }


        /// <summary>
        ///     Returns an iterator of the collection.
        /// </summary>
        /// <returns>An IEnumerator.</returns>
        public virtual IEnumerator ListIterator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Removes all the elements contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
        public virtual bool RemoveAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = new ArrayList(collection).GetEnumerator();
            while (tempEnumerator.MoveNext())
            {
                result = true;
                if (Contains(tempEnumerator.Current))
                    Remove(tempEnumerator.Current);
            }
            return result;
        }

        /// <summary>
        ///     Removes all the elements contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
        public virtual bool RemoveAll(CollectionSupport collection)
        {
            return RemoveAll((ICollection) collection);
        }

        /// <summary>
        ///     Removes the value in the specified index from the list.
        /// </summary>
        /// <param name="index">The index of the value to remove.</param>
        /// <returns>Returns the value removed.</returns>
        public virtual object RemoveElement(int index)
        {
            var objectRemoved = this[index];
            RemoveAt(index);
            return objectRemoved;
        }

        /// <summary>
        ///     Removes an specified element from the collection.
        /// </summary>
        /// <param name="element">The element to be removed.</param>
        /// <returns>Returns true if the element was successfuly removed. Otherwise returns false.</returns>
        public virtual bool RemoveElement(object element)
        {
            var result = false;
            if (Contains(element))
            {
                Remove(element);
                result = true;
            }
            return result;
        }

        /// <summary>
        ///     Removes the first value from an array list.
        /// </summary>
        /// <returns>Returns the value removed.</returns>
        public virtual object RemoveFirst()
        {
            var objectRemoved = this[0];
            RemoveAt(0);
            return objectRemoved;
        }

        /// <summary>
        ///     Removes the last value from an array list.
        /// </summary>
        /// <returns>Returns the value removed.</returns>
        public virtual object RemoveLast()
        {
            var objectRemoved = this[Count - 1];
            RemoveAt(Count - 1);
            return objectRemoved;
        }

        /// <summary>
        ///     Removes all the elements that aren't contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
        public virtual bool RetainAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = GetEnumerator();
            var tempCollection = new ListCollectionSupport(collection);
            while (tempEnumerator.MoveNext())
                if (!tempCollection.Contains(tempEnumerator.Current))
                {
                    result = RemoveElement(tempEnumerator.Current);

                    if (result)
                    {
                        tempEnumerator = GetEnumerator();
                    }
                }
            return result;
        }

        /// <summary>
        ///     Removes all the elements that aren't contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
        public virtual bool RetainAll(CollectionSupport collection)
        {
            return RetainAll((ICollection) collection);
        }

        /// <summary>
        ///     Verifies if all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
        public virtual bool ContainsAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = new ArrayList(collection).GetEnumerator();
            while (tempEnumerator.MoveNext())
                if (!(result = Contains(tempEnumerator.Current)))
                    break;
            return result;
        }

        /// <summary>
        ///     Verifies if all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
        public virtual bool ContainsAll(CollectionSupport collection)
        {
            return ContainsAll((ICollection) collection);
        }

        /// <summary>
        ///     Returns a new list containing a portion of the current list between a specified range.
        /// </summary>
        /// <param name="startIndex">The start index of the range.</param>
        /// <param name="endIndex">The end index of the range.</param>
        /// <returns>A ListCollectionSupport instance containing the specified elements.</returns>
        public virtual ListCollectionSupport SubList(int startIndex, int endIndex)
        {
            var index = 0;
            var tempEnumerator = GetEnumerator();
            var result = new ListCollectionSupport();
            for (index = startIndex; index < endIndex; index++)
                result.Add(this[index]);
            return result;
        }

        /// <summary>
        ///     Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <param name="objects">The array into which the elements of the collection will be stored.</param>
        /// <returns>The array containing all the elements of the collection.</returns>
        public virtual object[] ToArray(object[] objects)
        {
            if (objects.Length < Count)
                objects = new object[Count];
            var index = 0;
            var tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
                objects[index++] = tempEnumerator.Current;
            return objects;
        }

        /// <summary>
        ///     Returns an iterator of the collection starting at the specified position.
        /// </summary>
        /// <param name="index">The position to set the iterator.</param>
        /// <returns>An IEnumerator at the specified position.</returns>
        public virtual IEnumerator ListIterator(int index)
        {
            if (index < 0 || index > Count) throw new IndexOutOfRangeException();
            var tempEnumerator = GetEnumerator();
            if (index > 0)
            {
                var i = 0;
                while (tempEnumerator.MoveNext() && i < index - 1)
                    i++;
            }
            return tempEnumerator;
        }

        /// <summary>
        ///     Gets the last value from a list.
        /// </summary>
        /// <returns>Returns the last element of the list.</returns>
        public virtual object GetLast()
        {
            if (Count == 0) throw new ArgumentOutOfRangeException();
            return this[Count - 1];
        }

        /// <summary>
        ///     Return whether this list is empty.
        /// </summary>
        /// <returns>True if the list is empty, false if it isn't.</returns>
        public virtual bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        ///     Replaces the element at the specified position in this list with the specified element.
        /// </summary>
        /// <param name="index">Index of element to replace.</param>
        /// <param name="element">Element to be stored at the specified position.</param>
        /// <returns>The element previously at the specified position.</returns>
        public virtual object Set(int index, object element)
        {
            var result = this[index];
            this[index] = element;
            return result;
        }

        /// <summary>
        ///     Returns the element at the specified position in the list.
        /// </summary>
        /// <param name="index">Index of element to return.</param>
        /// <param name="element">Element to be stored at the specified position.</param>
        /// <returns>The element at the specified position in the list.</returns>
        public virtual object Get(int index)
        {
            return this[index];
        }
    }

    /*******************************/

    /// <summary>
    ///     This class manages array operations.
    /// </summary>
    public class ArraysSupport
    {
        /// <summary>
        ///     Compares the entire members of one array whith the other one.
        /// </summary>
        /// <param name="array1">The array to be compared.</param>
        /// <param name="array2">The array to be compared with.</param>
        /// <returns>True if both arrays are equals otherwise it returns false.</returns>
        /// <remarks>Two arrays are equal if they contains the same elements in the same order.</remarks>
        public static bool IsArrayEqual(Array array1, Array array2)
        {
            if (array1.Length != array2.Length)
                return false;
            for (var i = 0; i < array1.Length; i++)
                if (!array1.GetValue(i).Equals(array2.GetValue(i)))
                    return false;
            return true;
        }

        /// <summary>
        ///     Fills the array with an specific value from an specific index to an specific index.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="fromindex">The first index to be filled.</param>
        /// <param name="toindex">The last index to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void FillArray(Array array, int fromindex, int toindex, object val)
        {
            var Temp_Object = val;
            var elementtype = array.GetType().GetElementType();
            if (elementtype != val.GetType())
                Temp_Object = Convert.ChangeType(val, elementtype);
            if (array.Length == 0)
                throw new NullReferenceException();
            if (fromindex > toindex)
                throw new ArgumentException();
            if (fromindex < 0 || array.Length < toindex)
                throw new IndexOutOfRangeException();
            for (var index = fromindex > 0 ? fromindex-- : fromindex; index < toindex; index++)
                array.SetValue(Temp_Object, index);
        }

        /// <summary>
        ///     Fills the array with an specific value.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void FillArray(Array array, object val)
        {
            FillArray(array, 0, array.Length, val);
        }
    }


    /*******************************/

    /// <summary>
    ///     This class manages a set of elements.
    /// </summary>
    public class SetSupport : ArrayList
    {
        /// <summary>
        ///     Creates a new set.
        /// </summary>
        public SetSupport()
        {
        }

        /// <summary>
        ///     Creates a new set initialized with System.Collections.ICollection object
        /// </summary>
        /// <param name="collection">System.Collections.ICollection object to initialize the set object</param>
        public SetSupport(ICollection collection) : base(collection)
        {
        }

        /// <summary>
        ///     Creates a new set initialized with a specific capacity.
        /// </summary>
        /// <param name="capacity">value to set the capacity of the set object</param>
        public SetSupport(int capacity) : base(capacity)
        {
        }

        /// <summary>
        ///     Adds an element to the set.
        /// </summary>
        /// <param name="objectToAdd">The object to be added.</param>
        /// <returns>True if the object was added, false otherwise.</returns>
        public new virtual bool Add(object objectToAdd)
        {
            if (Contains(objectToAdd))
                return false;
            base.Add(objectToAdd);
            return true;
        }

        /// <summary>
        ///     Adds all the elements contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(ICollection collection)
        {
            var result = false;
            if (collection != null)
            {
                var tempEnumerator = new ArrayList(collection).GetEnumerator();
                while (tempEnumerator.MoveNext())
                {
                    if (tempEnumerator.Current != null)
                        result = Add(tempEnumerator.Current);
                }
            }
            return result;
        }

        /// <summary>
        ///     Adds all the elements contained in the specified support class collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(CollectionSupport collection)
        {
            return AddAll((ICollection) collection);
        }

        /// <summary>
        ///     Verifies that all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>True if the collection contains all the given elements.</returns>
        public virtual bool ContainsAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = collection.GetEnumerator();
            while (tempEnumerator.MoveNext())
                if (!(result = Contains(tempEnumerator.Current)))
                    break;
            return result;
        }

        /// <summary>
        ///     Verifies if all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
        public virtual bool ContainsAll(CollectionSupport collection)
        {
            return ContainsAll((ICollection) collection);
        }

        /// <summary>
        ///     Verifies if the collection is empty.
        /// </summary>
        /// <returns>True if the collection is empty, false otherwise.</returns>
        public virtual bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        ///     Removes an element from the set.
        /// </summary>
        /// <param name="elementToRemove">The element to be removed.</param>
        /// <returns>True if the element was removed.</returns>
        public new virtual bool Remove(object elementToRemove)
        {
            var result = false;
            if (Contains(elementToRemove))
                result = true;
            base.Remove(elementToRemove);
            return result;
        }

        /// <summary>
        ///     Removes all the elements contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>True if all the elements were successfuly removed, false otherwise.</returns>
        public virtual bool RemoveAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = collection.GetEnumerator();
            while (tempEnumerator.MoveNext())
            {
                if (result == false && Contains(tempEnumerator.Current))
                    result = true;
                Remove(tempEnumerator.Current);
            }
            return result;
        }

        /// <summary>
        ///     Removes all the elements contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
        public virtual bool RemoveAll(CollectionSupport collection)
        {
            return RemoveAll((ICollection) collection);
        }

        /// <summary>
        ///     Removes all the elements that aren't contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>True if all the elements were successfully removed, false otherwise.</returns>
        public virtual bool RetainAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = collection.GetEnumerator();
            var tempSet = (SetSupport) collection;
            while (tempEnumerator.MoveNext())
                if (!tempSet.Contains(tempEnumerator.Current))
                {
                    result = Remove(tempEnumerator.Current);
                    tempEnumerator = GetEnumerator();
                }
            return result;
        }

        /// <summary>
        ///     Removes all the elements that aren't contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
        public virtual bool RetainAll(CollectionSupport collection)
        {
            return RetainAll((ICollection) collection);
        }

        /// <summary>
        ///     Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <returns>The array containing all the elements of the collection.</returns>
        public new virtual object[] ToArray()
        {
            var index = 0;
            var tempObject = new object[Count];
            var tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
                tempObject[index++] = tempEnumerator.Current;
            return tempObject;
        }

        /// <summary>
        ///     Obtains an array containing all the elements in the collection.
        /// </summary>
        /// <param name="objects">The array into which the elements of the collection will be stored.</param>
        /// <returns>The array containing all the elements of the collection.</returns>
        public virtual object[] ToArray(object[] objects)
        {
            var index = 0;
            var tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
                objects[index++] = tempEnumerator.Current;
            return objects;
        }
    }

    /*******************************/

    /// <summary>
    ///     This class manages different operation with collections.
    /// </summary>
    public class AbstractSetSupport : SetSupport
    {
    }


    /*******************************/

    /// <summary>
    ///     Removes the element with the specified key from a Hashtable instance.
    /// </summary>
    /// <param name="hashtable">The Hashtable instance</param>
    /// <param name="key">The key of the element to remove</param>
    /// <returns>The element removed</returns>
    public static object HashtableRemove(Hashtable hashtable, object key)
    {
        var element = hashtable[key];
        hashtable.Remove(key);
        return element;
    }

    /*******************************/

    /// <summary>
    ///     Sets the size of the ArrayList. If the new size is greater than the current capacity, then new null items are added
    ///     to the end of the ArrayList. If the new size is lower than the current size, then all elements after the new size
    ///     are discarded
    /// </summary>
    /// <param name="arrayList">The ArrayList to be changed</param>
    /// <param name="newSize">The new ArrayList size</param>
    public static void SetSize(ArrayList arrayList, int newSize)
    {
        if (newSize < 0) throw new ArgumentException();
        if (newSize < arrayList.Count)
            arrayList.RemoveRange(newSize, arrayList.Count - newSize);
        else
            while (newSize > arrayList.Count)
                arrayList.Add(null);
    }

    /*******************************/

    /// <summary>
    ///     Adds an element to the top end of a Stack instance.
    /// </summary>
    /// <param name="stack">The Stack instance</param>
    /// <param name="element">The element to add</param>
    /// <returns>The element added</returns>
    public static object StackPush(Stack stack, object element)
    {
        stack.Push(element);
        return element;
    }

    /*******************************/

    /// <summary>
    ///     Copies an array of chars obtained from a String into a specified array of chars
    /// </summary>
    /// <param name="sourceString">The String to get the chars from</param>
    /// <param name="sourceStart">Position of the String to start getting the chars</param>
    /// <param name="sourceEnd">Position of the String to end getting the chars</param>
    /// <param name="destinationArray">Array to return the chars</param>
    /// <param name="destinationStart">Position of the destination array of chars to start storing the chars</param>
    /// <returns>An array of chars</returns>
    public static void GetCharsFromString(string sourceString, int sourceStart, int sourceEnd,
        ref char[] destinationArray, int destinationStart)
    {
        int sourceCounter;
        int destinationCounter;
        sourceCounter = sourceStart;
        destinationCounter = destinationStart;
        while (sourceCounter < sourceEnd)
        {
            destinationArray[destinationCounter] = sourceString[sourceCounter];
            sourceCounter++;
            destinationCounter++;
        }
    }

    /*******************************/

    /// <summary>
    ///     Creates an output file stream to write to the file with the specified name.
    /// </summary>
    /// <param name="FileName">Name of the file to write.</param>
    /// <param name="Append">True in order to write to the end of the file, false otherwise.</param>
    /// <returns>New instance of FileStream with the proper file mode.</returns>
    public static FileStream GetFileStream(string FileName, bool Append)
    {
        if (Append)
            return new FileStream(FileName, FileMode.Append);
        return new FileStream(FileName, FileMode.Create);
    }


    /*******************************/

    /// <summary>
    ///     Converts an array of sbytes to an array of chars
    /// </summary>
    /// <param name="sByteArray">The array of sbytes to convert</param>
    /// <returns>The new array of chars</returns>
    [CLSCompliant(false)]
    public static char[] ToCharArray(sbyte[] sByteArray)
    {
        var charArray = new char[sByteArray.Length];
        sByteArray.CopyTo(charArray, 0);
        return charArray;
    }

    /// <summary>
    ///     Converts an array of bytes to an array of chars
    /// </summary>
    /// <param name="byteArray">The array of bytes to convert</param>
    /// <returns>The new array of chars</returns>
    public static char[] ToCharArray(byte[] byteArray)
    {
        var charArray = new char[byteArray.Length];
        byteArray.CopyTo(charArray, 0);
        return charArray;
    }

    /*******************************/

    /// <summary>
    ///     Encapsulates the functionality of message digest algorithms such as SHA-1 or MD5.
    /// </summary>
    public class MessageDigestSupport
    {
        private HashAlgorithm algorithm;
        private byte[] data;
        private int position;
        private string algorithmName;

        /// <summary>
        ///     The HashAlgorithm instance that provide the cryptographic hash algorithm
        /// </summary>
        public HashAlgorithm Algorithm
        {
            get { return algorithm; }
            set { algorithm = value; }
        }

        /// <summary>
        ///     The digest data
        /// </summary>
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary>
        ///     The name of the cryptographic hash algorithm used in the instance
        /// </summary>
        public string AlgorithmName
        {
            get { return algorithmName; }
        }

        /// <summary>
        ///     Computes the hash value for the internal data digest.
        /// </summary>
        /// <returns>The array of signed bytes with the resulting hash value</returns>
        [CLSCompliant(false)]
        public sbyte[] DigestData()
        {
            var result = ToSByteArray(Algorithm.ComputeHash(data));
            Reset();
            return result;
        }

        /// <summary>
        ///     Performs and update on the digest with the specified array and then completes the digest
        ///     computation.
        /// </summary>
        /// <param name="newData">The array of bytes for final update to the digest</param>
        /// <returns>An array of signed bytes with the resulting hash value</returns>
        [CLSCompliant(false)]
        public sbyte[] DigestData(byte[] newData)
        {
            Update(newData);
            return DigestData();
        }

        /// <summary>
        ///     Updates the digest data with the specified array of bytes by making an append
        ///     operation in the internal array of data.
        /// </summary>
        /// <param name="newData">The array of bytes for the update operation</param>
        public void Update(byte[] newData)
        {
            if (position == 0)
            {
                Data = newData;
                position = Data.Length - 1;
            }
            else
            {
                var oldData = Data;
                Data = new byte[newData.Length + position + 1];
                oldData.CopyTo(Data, 0);
                newData.CopyTo(Data, oldData.Length);

                position = Data.Length - 1;
            }
        }

        /// <summary>
        ///     Updates the digest data with the input byte by calling the method Update with an array.
        /// </summary>
        /// <param name="newData">The input byte for the update</param>
        public void Update(byte newData)
        {
            var newDataArray = new byte[1];
            newDataArray[0] = newData;
            Update(newDataArray);
        }

        /// <summary>
        ///     Updates the specified count of bytes with the input array of bytes starting at the
        ///     input offset.
        /// </summary>
        /// <param name="newData">The array of bytes for the update operation</param>
        /// <param name="offset">The initial position to start from in the array of bytes</param>
        /// <param name="count">The number of bytes fot the update</param>
        public void Update(byte[] newData, int offset, int count)
        {
            var newDataArray = new byte[count];
            Array.Copy(newData, offset, newDataArray, 0, count);
            Update(newDataArray);
        }

        /// <summary>
        ///     Resets the digest data to the initial state.
        /// </summary>
        public void Reset()
        {
            data = null;
            position = 0;
        }

        /// <summary>
        ///     Returns a string representation of the Message Digest
        /// </summary>
        /// <returns>A string representation of the object</returns>
        public override string ToString()
        {
            return Algorithm.ToString();
        }


        /// <summary>
        ///     Compares two arrays of signed bytes evaluating equivalence in digest data
        /// </summary>
        /// <param name="firstDigest">An array of signed bytes for comparison</param>
        /// <param name="secondDigest">An array of signed bytes for comparison</param>
        /// <returns>True if the input digest arrays are equal</returns>
        [CLSCompliant(false)]
        public static bool EquivalentDigest(sbyte[] firstDigest, sbyte[] secondDigest)
        {
            var result = false;
            if (firstDigest.Length == secondDigest.Length)
            {
                var index = 0;
                result = true;
                while (result && index < firstDigest.Length)
                {
                    result = firstDigest[index] == secondDigest[index];
                    index++;
                }
            }

            return result;
        }
    }

    // REMOVED Class not used

    /// *******************************/
    /// <summary>
    ///     Interface used by classes which must be single threaded.
    /// </summary>
    public interface SingleThreadModel
    {
    }


    /*******************************/

    /// <summary>
    ///     Creates an instance of a received Type.
    /// </summary>
    /// <param name="classType">The Type of the new class instance to return.</param>
    /// <returns>An Object containing the new instance.</returns>
    public static object CreateNewInstance(Type classType)
    {
        object instance = null;
        Type[] constructor = {};
        ConstructorInfo[] constructors = null;

        constructors = classType.GetConstructors();

        if (constructors.Length == 0)
            throw new UnauthorizedAccessException();
        for (var i = 0; i < constructors.Length; i++)
        {
            var parameters = constructors[i].GetParameters();

            if (parameters.Length == 0)
            {
                instance = classType.GetConstructor(constructor).Invoke(new object[] {});
                break;
            }
            if (i == constructors.Length - 1)
                throw new MethodAccessException();
        }
        return instance;
    }


    /*******************************/

    /// <summary>
    ///     Writes the exception stack trace to the received stream
    /// </summary>
    /// <param name="throwable">Exception to obtain information from</param>
    /// <param name="stream">Output sream used to write to</param>
    public static void WriteStackTrace(Exception throwable, TextWriter stream)
    {
        stream.Write(throwable.StackTrace);
        stream.Flush();
    }

    /*******************************/

    /// <summary>
    ///     Determines whether two Collections instances are equals.
    /// </summary>
    /// <param name="source">The first Collections to compare. </param>
    /// <param name="target">The second Collections to compare. </param>
    /// <returns>Return true if the first collection is the same instance as the second collection, otherwise return false.</returns>
    public static bool EqualsSupport(ICollection source, ICollection target)
    {
        var sourceEnumerator = ReverseStack(source);
        var targetEnumerator = ReverseStack(target);

        if (source.Count != target.Count)
            return false;
        while (sourceEnumerator.MoveNext() && targetEnumerator.MoveNext())
            if (!sourceEnumerator.Current.Equals(targetEnumerator.Current))
                return false;
        return true;
    }

    /// <summary>
    ///     Determines if a Collection is equal to the Object.
    /// </summary>
    /// <param name="source">The first Collections to compare.</param>
    /// <param name="target">The Object to compare.</param>
    /// <returns>Return true if the first collection contains the same values of the second Object, otherwise return false.</returns>
    public static bool EqualsSupport(ICollection source, object target)
    {
        if (target.GetType() != typeof(ICollection))
            return false;
        return EqualsSupport(source, (ICollection) target);
    }

    /// <summary>
    ///     Determines if a IDictionaryEnumerator is equal to the Object.
    /// </summary>
    /// <param name="source">The first IDictionaryEnumerator to compare.</param>
    /// <param name="target">The second Object to compare.</param>
    /// <returns>
    ///     Return true if the first IDictionaryEnumerator contains the same values of the second Object, otherwise return
    ///     false.
    /// </returns>
    public static bool EqualsSupport(IDictionaryEnumerator source, object target)
    {
        if (target.GetType() != typeof(IDictionaryEnumerator))
            return false;
        return EqualsSupport(source, (IDictionaryEnumerator) target);
    }

    /// <summary>
    ///     Determines whether two IDictionaryEnumerator instances are equals.
    /// </summary>
    /// <param name="source">The first IDictionaryEnumerator to compare.</param>
    /// <param name="target">The second IDictionaryEnumerator to compare.</param>
    /// <returns>
    ///     Return true if the first IDictionaryEnumerator contains the same values as the second IDictionaryEnumerator,
    ///     otherwise return false.
    /// </returns>
    public static bool EqualsSupport(IDictionaryEnumerator source, IDictionaryEnumerator target)
    {
        while (source.MoveNext() && target.MoveNext())
            if (source.Key.Equals(target.Key))
                if (source.Value.Equals(target.Value))
                    return true;
        return false;
    }

    /// <summary>
    ///     Reverses the Stack Collection received.
    /// </summary>
    /// <param name="collection">The collection to reverse.</param>
    /// <returns>
    ///     The collection received in reverse order if it was a System.Collections.Stack type, otherwise it does
    ///     nothing to the collection.
    /// </returns>
    public static IEnumerator ReverseStack(ICollection collection)
    {
        if (collection.GetType() == typeof(Stack))
        {
            var collectionStack = new ArrayList(collection);
            collectionStack.Reverse();
            return collectionStack.GetEnumerator();
        }
        return collection.GetEnumerator();
    }
}