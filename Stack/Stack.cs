﻿// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: Stack
**
** Purpose: An array implementation of a generic stack.
**
** Date: January 28, 2003
**
=============================================================================*/
namespace System.Collections.Generic.Ozan
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    // A simple stack of objects.  Internally it is implemented as an array,
    // so Push can be O(n).  Pop is O(1).

    [DebuggerDisplay("Count = {Count}")]
#if !SILVERLIGHT
    [Serializable()]
#endif
    [System.Runtime.InteropServices.ComVisible(false)]
    public class Stack<T> : IEnumerable<T>,
        System.Collections.ICollection,
        IReadOnlyCollection<T>
    {
        private T[] _array;     // Storage for stack elements
        private int _size;           // Number of items in the stack.
        private int _version;        // Used to keep enumerator in sync w/ collection.
#if !SILVERLIGHT
        [NonSerialized]
#endif
        private Object _syncRoot;

        private const int _defaultCapacity = 4;
        static T[] _emptyArray = new T[0];

        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.Stack"]/*' />
        public Stack()
        {
            _array = _emptyArray;
            _size = 0;
            _version = 0;
        }

        // Create a stack with a specific initial capacity.  The initial capacity
        // must be a non-negative number.
        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.Stack1"]/*' />
        public Stack(int capacity)
        {
            if (capacity < 0)
                ArgumentOutOfRangeException.ThrowIfNegative(capacity);
            _array = new T[capacity];
            _size = 0;
            _version = 0;
        }

        // Fills a Stack with the contents of a particular collection.  The items are
        // pushed onto the stack in the same order they are read by the enumerator.
        //
        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.Stack2"]/*' />
        public Stack(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                int count = c.Count;
                _array = new T[count];
                c.CopyTo(_array, 0);
                _size = count;
            }
            else
            {
                _size = 0;
                _array = new T[_defaultCapacity];

                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Push(en.Current);
                    }
                }
            }
        }

        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.Count"]/*' />
        public int Count
        {
            get { return _size; }
        }

        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.IsSynchronized"]/*' />
        bool System.Collections.ICollection.IsSynchronized
        {
            get { return false; }
        }

        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.SyncRoot"]/*' />
        Object System.Collections.ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        // Removes all Objects from the Stack.
        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.Clear"]/*' />
        public void Clear()
        {
            Array.Clear(_array, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            _size = 0;
            _version++;
        }

        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.Contains"]/*' />
        public bool Contains(T item)
        {
            int count = _size;

            EqualityComparer<T> c = EqualityComparer<T>.Default;
            while (count-- > 0)
            {
                if (((Object)item) == null)
                {
                    if (((Object)_array[count]) == null)
                        return true;
                }
                else if (_array[count] != null && c.Equals(_array[count], item))
                {
                    return true;
                }
            }
            return false;
        }

        // Copies the stack into an array.
        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.CopyTo"]/*' />
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }

            if (array.Length - arrayIndex < _size)
            {
                throw new ArgumentException();
            }

            Array.Copy(_array, 0, array, arrayIndex, _size);
            Array.Reverse(array, arrayIndex, _size);
        }

        void System.Collections.ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException();
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException();
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - arrayIndex < _size)
            {
                throw new ArgumentException();
            }

            try
            {
                Array.Copy(_array, 0, array, arrayIndex, _size);
                Array.Reverse(array, arrayIndex, _size);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException();
            }
        }

        // Returns an IEnumerator for this Stack.
        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.GetEnumerator"]/*' />
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.IEnumerable.GetEnumerator"]/*' />
        /// <internalonly/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void TrimExcess()
        {
            int threshold = (int)(((double)_array.Length) * 0.9);
            if (_size < threshold)
            {
                T[] newarray = new T[_size];
                Array.Copy(_array, 0, newarray, 0, _size);
                _array = newarray;
                _version++;
            }
        }

        // Returns the top object on the stack without removing it.  If the stack
        // is empty, Peek throws an InvalidOperationException.
        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.Peek"]/*' />
        public T Peek()
        {
            if (_size == 0)
                throw new InvalidOperationException("Stack empty");
            return _array[_size - 1];
        }

        // Pops an item from the top of the stack.  If the stack is empty, Pop
        // throws an InvalidOperationException.
        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.Pop"]/*' />
        public T Pop()
        {
            if (_size == 0)
                throw new InvalidOperationException("Stack empty");
            _version++;
            T item = _array[--_size];
            _array[_size] = default(T);     // Free memory quicker.
            return item;
        }

        // Pushes an item to the top of the stack.
        // 
        /// <include file='doc\Stack.uex' path='docs/doc[@for="Stack.Push"]/*' />
        public void Push(T item)
        {
            if (_size == _array.Length)
            {
                T[] newArray = new T[(_array.Length == 0) ? _defaultCapacity : 2 * _array.Length];
                Array.Copy(_array, 0, newArray, 0, _size);
                _array = newArray;
            }
            _array[_size++] = item;
            _version++;
        }

        // Copies the Stack to an array, in the same order Pop would return the items.
        public T[] ToArray()
        {
            T[] objArray = new T[_size];
            int i = 0;
            while (i < _size)
            {
                objArray[i] = _array[_size - i - 1];
                i++;
            }
            return objArray;
        }

        /// <include file='doc\Stack.uex' path='docs/doc[@for="StackEnumerator"]/*' />
#if !SILVERLIGHT
        [Serializable()]
#endif
        [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario")]
        public struct Enumerator : IEnumerator<T>,
            System.Collections.IEnumerator
        {
            private Stack<T> _stack;
            private int _index;
            private int _version;
            private T currentElement;

            internal Enumerator(Stack<T> stack)
            {
                _stack = stack;
                _version = _stack._version;
                _index = -2;
                currentElement = default(T);
            }

            /// <include file='doc\Stack.uex' path='docs/doc[@for="StackEnumerator.Dispose"]/*' />
            public void Dispose()
            {
                _index = -1;
            }

            /// <include file='doc\Stack.uex' path='docs/doc[@for="StackEnumerator.MoveNext"]/*' />
            public bool MoveNext()
            {
                bool retval;
                if (_version != _stack._version) throw new InvalidOperationException();
                if (_index == -2)
                {  // First call to enumerator.
                    _index = _stack._size - 1;
                    retval = (_index >= 0);
                    if (retval)
                        currentElement = _stack._array[_index];
                    return retval;
                }
                if (_index == -1)
                {  // End of enumeration.
                    return false;
                }

                retval = (--_index >= 0);
                if (retval)
                    currentElement = _stack._array[_index];
                else
                    currentElement = default(T);
                return retval;
            }

            /// <include file='doc\Stack.uex' path='docs/doc[@for="StackEnumerator.Current"]/*' />
            public T Current
            {
                get
                {
                    if (_index == -2) throw new InvalidOperationException();
                    if (_index == -1) throw new InvalidOperationException();
                    return currentElement;
                }
            }

            Object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (_index == -2) throw new InvalidOperationException();
                    if (_index == -1) throw new InvalidOperationException();
                    return currentElement;
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                if (_version != _stack._version) throw new InvalidOperationException();
                _index = -2;
                currentElement = default(T);
            }
        }
    }
}