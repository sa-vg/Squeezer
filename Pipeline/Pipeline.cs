using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Pipelines
{
    public abstract class Pipeline
    {
        private readonly BlockingCollection<Block> _outputBuffer = new BlockingCollection<Block>(200);

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

        public void RunPipeline(BinaryReader reader, BinaryWriter writer)
        {
            var transformBlock = TransformBlock();

            var writerThread = CreateWriterThread(WriteBlock(writer));
            writerThread.Start();

            ReadBlocks(reader).AsParallel().
            Select(transformBlock).
            ForAll(x => _outputBuffer.Add(x));

            _outputBuffer.CompleteAdding();

            writerThread.Join();
        }

        private Thread CreateWriterThread(Action<Block> writeBlock)
        {
            return new Thread(() =>
            {
                do if (_outputBuffer.TryTake(out Block block)) writeBlock(block);
                while (!_outputBuffer.IsCompleted);
            });
        }

        public abstract IEnumerable<Block> ReadBlocks(BinaryReader reader);

        public abstract Action<Block> WriteBlock(BinaryWriter writer);

        public abstract Func<Block, Block> TransformBlock();
    }
}