using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace ServerTestApp
{
    public class DefaultCompressor : ICompressor
    {
        public async Task<byte[]> CompressAsync(byte[] input)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    await gzip.WriteAsync(input, 0, input.Length);
                }
                return memory.ToArray();
            }
        }

        public async Task<byte[]> DeCompressAsync(byte[] input)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(input), CompressionMode.Decompress))
            {
                byte[] buffer = new byte[1024 * 4];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = await stream.ReadAsync(buffer, 0, 1024 * 4);
                        if (count > 0)
                        {
                            await memory.WriteAsync(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}
