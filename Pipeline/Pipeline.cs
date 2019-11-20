using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Pipelines
{
    public abstract class Pipeline
    {
        public static Pipeline Create(CompressionMode mode)
        {
            if(mode == CompressionMode.Compress) return new CompressionPipeline();
            return new DecompressionPipeline();
        }

        public void Process(string inputFile, string outputFile)
        {
            try
            {
                using (var inputFs = new FileStream(inputFile, FileMode.Open))
                using (var outputFs = new FileStream(outputFile, FileMode.Create))
                using (var reader = new BinaryReader(inputFs))
                using (var writer = new BinaryWriter(outputFs))
                {
                    RunPipeline(reader, writer);
                }
            }
            catch (InvalidDataException de)
            {
                Console.WriteLine("File corrupted or not supported format");
            }
            catch (IOException io)
            {
                Console.WriteLine($"Wrong file name \n {io}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error \n {ex}");
            }
        }

        private void RunPipeline(BinaryReader reader, BinaryWriter writer)
        {
            var transformBlock = TransformBlock();
            var writeBlock = WriteBlock(writer);

            var blocks = ReadBlocks(reader).
                    AsParallel().
                    AsOrdered().
                    WithExecutionMode(ParallelExecutionMode.ForceParallelism).
                    WithMergeOptions(ParallelMergeOptions.NotBuffered).
                Select(transformBlock);

            foreach (var block in blocks)
            {
                writeBlock(block);
            }
        }

        protected abstract IEnumerable<Block> ReadBlocks(BinaryReader reader);

        protected abstract Action<Block> WriteBlock(BinaryWriter writer);

        protected abstract Func<Block, Block> TransformBlock();
    }
}