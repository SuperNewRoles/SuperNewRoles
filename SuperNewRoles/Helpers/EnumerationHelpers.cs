using System;
using System.Collections;
using System.Linq.Expressions;
using Il2CppSystem.Collections.Generic;

namespace SuperNewRoles
{

    public static class EnumerationHelpers
    {
        public static System.Collections.Generic.IEnumerable<T> GetFastEnumerator<T>(this List<T> list) where T : Il2CppSystem.Object => new Il2CppListEnumerable<T>(list);
    }

    public unsafe class Il2CppListEnumerable<T> : System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEnumerator<T> where T : Il2CppSystem.Object
    {
        private struct Il2CppListStruct
        {
#pragma warning disable CS0649
            public IntPtr _items;
            public int _size;
#pragma warning restore CS0649
        }

        private static readonly int _elemSize;
        private static readonly int _offset;
        private static readonly Func<IntPtr, T> _objFactory;

        static Il2CppListEnumerable()
        {
            _elemSize = IntPtr.Size;
            _offset = 4 * IntPtr.Size;

            System.Reflection.ConstructorInfo constructor = typeof(T).GetConstructor(new[] { typeof(IntPtr) });
            ParameterExpression ptr = Expression.Parameter(typeof(IntPtr));
            NewExpression create = Expression.New(constructor!, ptr);
            Expression<Func<IntPtr, T>> lambda = Expression.Lambda<Func<IntPtr, T>>(create, ptr);
            _objFactory = lambda.Compile();
        }

        private readonly IntPtr _arrayPointer;
        private readonly int _count;
        private int _index = -1;

        public Il2CppListEnumerable(List<T> list)
        {
            Il2CppListStruct* listStruct = (Il2CppListStruct*)list.Pointer;
            this._count = listStruct->_size;
            this._arrayPointer = listStruct->_items;
        }

        object IEnumerator.Current => this.Current;
        public T Current { get; private set; }

        public bool MoveNext()
        {
            if (++this._index >= this._count) return false;
            IntPtr refPtr = *(IntPtr*)IntPtr.Add(IntPtr.Add(this._arrayPointer, _offset), this._index * _elemSize);
            this.Current = _objFactory(refPtr);
            return true;
        }

        public void Reset()
        {
            this._index = -1;
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}