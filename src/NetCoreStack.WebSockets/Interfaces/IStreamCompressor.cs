using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.Interfaces
{
    public interface IStreamCompressor
    {
        Task<byte[]> CompressAsync(byte[] input);

        Task<byte[]> DeCompressAsync(byte[] input);
    }
}
