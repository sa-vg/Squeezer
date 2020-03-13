using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Compressor
{
    public interface IBlockReader
    {
        IEnumerable<Block> ReadBlocks();
    }

    public interface IBlockWriter
    {
        void WriteBlock(Block block);
    }

    public interface IBlockTransformer
    {
        Block TransformBlock(Block block);
    }

    internal class BlockAlgs
    {
        public IBlockReader Reader { get; }
        public IBlockWriter Writer { get; }
        public IBlockTransformer Transformer { get; }

        public BlockAlgs(IBlockReader reader, IBlockTransformer transformer, IBlockWriter writer)
        {
            Reader = reader;
            Transformer = transformer;
            Writer = writer;
        }

        public static BlockAlgs Create(CompressionMode mode, int blockSize, FileStream inputFs, FileStream outputFs) =>
            mode switch
            {
                CompressionMode.Compress => new BlockAlgs(
                    reader: new BlockReader(inputFs, blockSize), 
                    transformer: new Compressor(), 
                    writer: new CompressedBlockWriter(outputFs)),
                CompressionMode.Decompress => new BlockAlgs(
                    reader: new CompressedBlockReader(inputFs), 
                    transformer: new Decompressor(blockSize), 
                    writer: new BlockWriter(outputFs, blockSize)),

                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
    }
}