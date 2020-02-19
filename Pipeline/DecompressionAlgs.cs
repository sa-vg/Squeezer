using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Compressor
{
    internal class DecompressionAlgs : BlockAlgs
    {
        public DecompressionAlgs(int blockSize) : base(blockSize) { }

        public override IEnumerable<Block> ReadBlocks(BinaryReader reader)
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var index = reader.ReadInt32();
                var size = reader.ReadInt32();
                var bytes = reader.ReadBytes(size);
                yield return new Block(index, bytes);
            }
        }

        public override Action<Block> WriteBlock(BinaryWriter writer) =>
            block =>
            {
                writer.BaseStream.Position = (long)block.Index * BlockSize;
                writer.Write(block.Bytes);
            };

        public override Func<Block, Block> TransformBlock()
        {
            return block =>
            {
                var ms = new MemoryStream(block.Bytes);
                var result = new byte[BlockSize];

                int bytesRead;
                using (var gs = new GZipStream(ms, CompressionMode.Decompress)) bytesRead = gs.Read(result, 0, BlockSize);

                //last block
                if (bytesRead != BlockSize) Array.Resize(ref result, bytesRead);

                return new Block(block.Index, result);
            };
        }
    }
}