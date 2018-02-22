using System;
using System.Linq;
using static NetCoreStack.WebSockets.Internal.NCSConstants;

namespace NetCoreStack.WebSockets
{
    internal static class ByteExtensions
    {
        public static Tuple<byte[], byte[]> Split(this byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var value = Splitter.First();
            var index = Array.IndexOf(input, value, 1);
            if (index == -1)
            {
                throw new InvalidOperationException($"Invalid data format! " +
                        $"Check the splitter pattern exist: \"{Splitter}\"");
            }

            var header = new ArraySegment<byte>(input, 0, index);
            var body = new ArraySegment<byte>(input, (index + 1), input.Length - (index + 1));

            return new Tuple<byte[], byte[]>(header.ToArray(), body.ToArray());
        }
    }
}
