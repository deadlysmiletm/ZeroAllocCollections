using System.Runtime.InteropServices;

namespace DeadlySmile.LightwayCollection.Utilities
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct LightwayResult<T> where T: unmanaged
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string ErrorMsg { get; }

        private LightwayResult(bool isSuccess, T* value, string errorMsg)
        {
            IsSuccess = isSuccess;
            Value = *value;
            ErrorMsg = errorMsg;
        }

        public static LightwayResult<T> Success(T* value) => new(true, value, string.Empty);
        public static LightwayResult<T> Failure(string errorMsg) => new(false, default, errorMsg);
    }
}