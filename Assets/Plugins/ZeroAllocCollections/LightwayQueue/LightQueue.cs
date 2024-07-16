using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using DeadlySmile.LightwayCollection.Utilities;

namespace DeadlySmile.LightwayCollection
{
    public unsafe struct LightQueue<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        private LightNode<T>* _head;
        private LightNode<T>* _tail;
        private uint _count;
        private readonly BatchMemoryPool<T> _memoryPool;

        public LightQueue(byte batchSize)
        {
            _head = null;
            _tail = null;
            _count = 0;
            _memoryPool = new(batchSize);
        }


        public uint Count => _count;
        public bool IsEmpty => _count is 0;

        public void Enqueue(T value)
        {
            LightNode<T>* nodePointer = _memoryPool.Allocate();
            *nodePointer = new()
            {
                Value = value,
                Next = null,
            };

            if (_tail is null)
            {
                _head = nodePointer;
                _tail = nodePointer;
            }
            else
            {
                _tail->Next = nodePointer;
                _tail = nodePointer;
            }
            _count++;
        }

        public T Dequeue()
        {
            if (_count is 0)
                throw new Exception("LightQueue: collection is empty when trying to dequeue a value.");

            LightNode<T>* headPtr = _head;
            T valueToReturn = headPtr->Value;
            _head = headPtr->Next;

            if (_head is null)
                _tail = null;

            _memoryPool.Free(headPtr);
            _count--;
            return valueToReturn;
        }

        public LightwayResult<T> Peek()
        {
            if (_count == 0)
                return LightwayResult<T>.Failure("LightQueue is empty.");

            return LightwayResult<T>.Success(&_head->Value);
        }

        public readonly ReadOnlySpan<T> AsSpan()
        {
            if (_count == 0)
                return ReadOnlySpan<T>.Empty;

            T[] valuesArray = ArrayPool<T>.Shared.Rent((int)_count);
            LightNode<T>* currentPtr = _head;
            for (int i = 0; i < _count; i++)
            {
                valuesArray[i] = currentPtr->Value;
                currentPtr = currentPtr->Next;
            }
            ReadOnlySpan<T> valuesToReturn = new(valuesArray);
            ArrayPool<T>.Shared.Return(valuesArray);
            return valuesArray;
        }

        public readonly T[] AsArray()
        {
            if (_count == 0)
                return Array.Empty<T>();

            T[] valuesArray = new T[_count];
            LightNode<T>* currentPtr = _head;
            for (int i = 0; i < _count; i++)
            {
                valuesArray[i] = currentPtr->Value;
                currentPtr = currentPtr->Next;
            }
            return valuesArray;
        }

        public readonly IEnumerable<T> AsLazyCollection()
        {
            if (_count == 0)
                yield return default;

            T[] concreteValues = AsArray();
            for (int i = 0; i < _count; i++)
            {
                yield return concreteValues[i];
            }
        }

        public void Dispose()
        {
            LightNode<T>* nodePtr;
            while (_head is not null)
            {
                nodePtr = _head;
                _head = nodePtr->Next;
                _memoryPool.Free(nodePtr);
            }
            _count = 0;
            _tail = null;
            _memoryPool.Dispose();
        }


        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IEnumerable<T> valCol = AsLazyCollection();
            foreach (var item in valCol)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return AsLazyCollection();
        }
    }
}