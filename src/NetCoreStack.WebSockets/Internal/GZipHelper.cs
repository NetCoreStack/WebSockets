namespace NetCoreStack.WebSockets.Internal
{
    public static class GZipHelper
    {
        /// <summary>
        /// Checks the first two bytes in a GZIP file, which must be 31 and 139.
        /// </summary>
        public static bool IsGZipHeader(byte[] arr)
        {
            return arr.Length >= 2 &&
                arr[0] == 31 &&
                arr[1] == 139;
        }
    }
}
