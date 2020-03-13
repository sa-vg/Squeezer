using System;
using System.IO;
using System.IO.Compression;

namespace Compressor
{
    internal class Decompressor : IBlockTransformer
    {
        private readonly int _blockSize;

        public Decompressor(int blockSize)
        {
            _blockSize = blockSize;
        }

        public Block TransformBlock(Block block)
        {
            var ms = new MemoryStream(block.Bytes);
            var result = new byte[_blockSize];

            int bytesRead;
            using (var gs = new GZipStream(ms, CompressionMode.Decompress)) bytesRead = gs.Read(result, 0, result.Length);

            //last block
            if (bytesRead != result.Length) Array.Resize(ref result, bytesRead);

            return new Block(block.Index, result);
        }
    }
}