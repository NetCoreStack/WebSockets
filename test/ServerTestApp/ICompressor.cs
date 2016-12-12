using System.Threading.Tasks;

namespace ServerTestApp
{
    public interface ICompressor
    {
        Task<byte[]> CompressAsync(byte[] input);

        Task<byte[]> DeCompressAsync(byte[] input);
    }
}
