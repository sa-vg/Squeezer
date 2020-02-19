using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Compressor
{
    internal class CompressionAlgs : BlockAlgs
    {
        public CompressionAlgs(int blockSize): base (blockSize) { }

        public override IEnumerable<Block> ReadBlocks(BinaryReader reader)
        {
            int index = 0;
            while (true)
            {
                var bytes = reader.ReadBytes(BlockSize);
                if (bytes.Length > 0) yield return new Block(index++, bytes);
                else break;
            }
        }

        public override Action<Block> WriteBlock(BinaryWriter writer) =>
            block =>
            {
                writer.Write(block.Index);
                writer.Write(block.Bytes.Length);
                writer.Write(block.Bytes);
            };

        public override Func<Block, Block> TransformBlock()
        {
            var localBuffer = new ThreadLocal<MemoryStream>(() => new MemoryStream());

            return block =>
            {
                var ms = localBuffer.Value;
                using (var gs = new GZipStream(ms, CompressionMode.Compress, leaveOpen: true))
                {
                    gs.Write(block.Bytes);
                    gs.Flush();
                }

                var compressedBytes = ms.ToArray();
                ms.Position = 0;
                return new Block(block.Index, compressedBytes);
            };
        }
    }
}