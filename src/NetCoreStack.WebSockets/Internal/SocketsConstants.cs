namespace NetCoreStack.WebSockets.Internal
{
    public static class SocketsConstants
    {
        public static byte[] Splitter = new byte[] { 0x1f };
        public const int ChunkSize = 1024 * 4;
        public const string WSFQN = "X-NetCoreStack-WSHost";
        public const string CompressedKey = "Compressed";
        public const string ConnectorName = "ConnectorName";
        public const string ConnectionId = "ConnectionId";
    }
}
