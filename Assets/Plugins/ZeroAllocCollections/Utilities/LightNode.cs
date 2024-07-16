using System.Runtime.InteropServices;

namespace DeadlySmile.LightwayCollection.Utilities
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LightNode<T> where T: unmanaged
    {
        public T Value;
        public LightNode<T>* Next;
    }
}
