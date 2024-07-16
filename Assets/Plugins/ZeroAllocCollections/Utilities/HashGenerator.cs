using System.Runtime.CompilerServices;

namespace DeadlySmile.LightwayCollection.Utilities
{
    public struct HashGenerator
    {
        readonly static int[] _hashLayers = new int[] { 17, 31, 37, 53 };

        public static int GenerateHash<T>(T value, int hashLayers = 4) where T : unmanaged
        {
            if (hashLayers > _hashLayers.Length)
            {
                hashLayers = _hashLayers.Length;
            }

            int hash = CombineHash(_hashLayers[0], value);
            for (int i = 1; i < hashLayers; i++)
                hash = CombineHash(hash, _hashLayers[i]);

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int CombineHash<T>(int hash, T value) where T : unmanaged
        {
            int ptrAddress = *(int*)&value;
            uint combHash = ((uint)hash << 5) | ((uint)hash >> 27);
            return ((int)combHash + hash) ^ ptrAddress;
        }
    }
}
