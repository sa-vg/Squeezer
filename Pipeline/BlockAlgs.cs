using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Compressor
{
    public abstract class BlockAlgs
    {
        public int BlockSize { get; }

        protected BlockAlgs(int blockSize)
        {
            BlockSize = blockSize;
        }

        public static BlockAlgs Create(CompressionMode mode, int blockSize)
        {
            if (mode == CompressionMode.Compress) return new CompressionAlgs(blockSize);
            return new DecompressionAlgs(blockSize);
        }

        public abstract IEnumerable<Block> ReadBlocks(BinaryReader reader);

        public abstract Action<Block> WriteBlock(BinaryWriter writer);

        public abstract Func<Block, Block> TransformBlock();
    }
}