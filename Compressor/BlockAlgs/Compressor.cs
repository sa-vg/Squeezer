using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Compressor
{
    internal class Compressor: IBlockTransformer 
    {
        private readonly ThreadLocal<MemoryStream> _localBuffer = new ThreadLocal<MemoryStream>(() => new MemoryStream());

        public Block TransformBlock(Block block)
        {
            var ms = _localBuffer.Value;
            using (var gs = new GZipStream(ms, CompressionMode.Compress, leaveOpen: true))
            {
                gs.Write(block.Bytes);
                gs.Flush();
            }

            var compressedBytes = ms.ToArray();
            ms.Position = 0;
            return new Block(block.Index, compressedBytes);
        }
    }
}