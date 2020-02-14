using System;
using System.IO.Compression;

namespace Pipelines
{
    public class Config
    {
        public static Config Default = new Config(1024*1024, Environment.ProcessorCount, 100);

        public Config(int blockSize, int degreeOfParallelism, int buffersCapacity)
        {
            BlockSize = blockSize;
            DegreeOfParallelism = degreeOfParallelism;
            BuffersCapacity = buffersCapacity;
        }

        public int BlockSize { get; }
        public int DegreeOfParallelism { get; }
        public int BuffersCapacity { get; }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var compressionMode = (CompressionMode)Enum.Parse(typeof(CompressionMode), args[2], true);
                var config = new Config(blockSize: 1024*1024, degreeOfParallelism: Environment.ProcessorCount, buffersCapacity: 100);

                var algs = WorkPlan.Create(compressionMode, config.BlockSize);
                var pipeline = new Pipeline(config, algs);

                string inputFile = args[0];
                string outputFile = args[1];
                pipeline.Process(inputFile, outputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid arguments {ex}");
            }
        }
    }
}
