namespace NetCoreStack.WebSockets.Internal
{
    public static class NCSConstants
    {
        public static byte[] Splitter = new byte[] { 0x1f };
        public const int ChunkSize = 1024 * 4;
        public const string WSFQN = "X-NetCoreStack-WSHost";
        public const string CompressedKey = "GZipCompressed";
        public const string ConnectorName = "ConnectorName";
        public const string ConnectionId = "ConnectionId";

        // Symbols
        public static readonly string CheckMarkSymbol = ((char)0x2713).ToString();
        public static readonly string WarningSymbol = ((char)0x26A0).ToString();
    }
}
