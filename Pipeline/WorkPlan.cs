using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Pipelines
{
    public abstract class WorkPlan
    {
        public int BlockSize { get; }

        protected WorkPlan(int blockSize)
        {
            BlockSize = blockSize;
        }

        public static WorkPlan Create(CompressionMode mode, int blockSize)
        {
            if (mode == CompressionMode.Compress) return new CompressionPlan(blockSize);
            return new DecompressionPlan(blockSize);
        }

        public abstract IEnumerable<Block> ReadBlocks(BinaryReader reader);

        public abstract Action<Block> WriteBlock(BinaryWriter writer);

        public abstract Func<Block, Block> TransformBlock();
    }
}