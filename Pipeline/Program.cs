using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Compressor
{
    public class Config
    {
        public static Config Default = new Config(1024 * 1024, Environment.ProcessorCount, 100);

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
                if (args.Length != 3) throw new Exception("Wrong args count");
                var inputFile = args[0];
                var outputFile = args[1];
                if (string.IsNullOrWhiteSpace(inputFile)) throw new ArgumentException(nameof(inputFile));
                if (string.IsNullOrWhiteSpace(outputFile)) throw new ArgumentException(nameof(outputFile));

                var compressionMode = Enum.TryParse(typeof(CompressionMode), args[2], true, out object result)
                    ? (CompressionMode)result
                    : throw new ArgumentException("mode");

                var config = new Config(blockSize: 1024 * 1024, degreeOfParallelism: Environment.ProcessorCount, buffersCapacity: 100);

                ProcessFile(inputFile, outputFile, compressionMode, config);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid arguments: {ex}.");
            }

            Console.ReadLine();
        }

        public static void ProcessFile(string inputFile, string outputFile, CompressionMode compressionMode, Config config)
        {
            Console.WriteLine($"Starting {compressionMode} operation from {inputFile} to {outputFile}");

            try
            {
                using var inputFs = new FileStream(inputFile, FileMode.Open);
                using var outputFs = new FileStream(outputFile, FileMode.Create);
                using var reader = new BinaryReader(inputFs);
                using var writer = new BinaryWriter(outputFs);

                var blockAlgs = BlockAlgs.Create(compressionMode, config.BlockSize);
                var cts = new CancellationTokenSource();
                var pipeline = new Pipeline(config, cts.Token);

                pipeline.Run(blockSource: blockAlgs.ReadBlocks(reader),
                    transformAlg: blockAlgs.TransformBlock(),
                    writeAlg: blockAlgs.WriteBlock(writer));

                Console.WriteLine("Completed!");
            }
            catch (InvalidDataException de)
            {
                Console.WriteLine("File corrupted or not supported format");
            }
            catch (IOException io)
            {
                Console.WriteLine($"Wrong file name \n {io.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error \n {ex}");
            }
        }
    }


}
