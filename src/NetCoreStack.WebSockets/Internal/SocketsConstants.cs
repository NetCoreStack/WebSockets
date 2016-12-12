namespace NetCoreStack.WebSockets.Internal
{
    public class SocketsConstants
    {
        public static byte[] Splitter = new byte[] { 0x1f };
        public const string CompressedKey = "Compressed";
        public const int ChunkSize = 1024 * 4;
    }
}
