using System;
using System.Runtime.InteropServices;


namespace DeadlySmile.LightwayCollection.Utilities
{
    public unsafe class BatchMemoryPool<T> : IDisposable where T : unmanaged
    {
        private LightNode<T>* _freeListHead;
        private readonly LightNode<T>* _memory;
        private int _currentMemorySize;
        private readonly byte _batchSize;

        public BatchMemoryPool(byte batchSize)
        {
            _batchSize = batchSize;
            _currentMemorySize = _batchSize;
            _memory = (LightNode<T>*)Marshal.AllocHGlobal(_currentMemorySize * sizeof(LightNode<T>));

            InitializeFreeList(_memory, _currentMemorySize);
        }


        public LightNode<T>* Allocate()
        {
            if (_freeListHead is null)
                ResizePool();

            LightNode<T>* nodePtr = _freeListHead;
            _freeListHead = _freeListHead->Next;
            return nodePtr;
        }

        public void Free(LightNode<T>* nodePtr)
        {
            nodePtr->Next = _freeListHead;
            _freeListHead = nodePtr;
        }

        public void Free(LightNode<T> node)
        {
            Free(&node);
        }

        private void InitializeFreeList(LightNode<T>* headNodePtr, int blockSize)
        {
            for (int i = 0; i < blockSize - 1; i++)
            {
                headNodePtr[i].Next = &headNodePtr[i + 1];
            }

            headNodePtr[blockSize - 1].Next = _freeListHead;
            _freeListHead = headNodePtr;
        }

        private void ResizePool()
        {
            int newBlockSize = _currentMemorySize + _batchSize;
            LightNode<T>* newMemory = (LightNode<T>*)Marshal.AllocHGlobal(newBlockSize * sizeof(LightNode<T>));
            InitializeFreeList(newMemory, newBlockSize);
            _currentMemorySize = newBlockSize;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)_memory);
        }
    }
}