using System;
using System.IO.Compression;

namespace Pipelines
{
    public class Config
    {
        public const int BlockSize = 1024 * 1024;
    }

    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var compressionMode = (CompressionMode)Enum.Parse(typeof(CompressionMode), args[2], true);
                string inputFile = args[0];
                string outputFile = args[1];

                var pipeline = Pipeline.Create(compressionMode);
                pipeline.Process(inputFile, outputFile);

                var restoredFile = inputFile.Replace(inputFile, inputFile + "_restored");

                var pipeline2 = Pipeline.Create(CompressionMode.Decompress);
                pipeline2.Process(outputFile, restoredFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid arguments {ex}");
            }
        }
    }
}
