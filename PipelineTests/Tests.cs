using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using Pipelines;
using Xunit;

namespace PipelineTests
{
    public class Tests
    {
        [Theory]
        [InlineData(@"C:\Temp\436.30-desktop-win10-64bit-international-whql.exe")]
        [InlineData(@"I:\Downloads\boost_1_71_0-msvc-12.0-64.exe")]
        //[InlineData(@"C:\Temp\New folder\VirtualBox-6.0.14-133895-Win.txt")]
        public void TestPipeline(string inputFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var compressedFile = Path.ChangeExtension(inputFile, "zip");
            var restoredFile = inputFile.Replace(fileName, fileName + "_restored");

            var config = Config.Default;
            var algs1 = WorkPlan.Create(CompressionMode.Compress, config.BlockSize);
            var pipeline1 = new Pipeline(config, algs1);
            pipeline1.Process(inputFile, compressedFile);

            var algs2 = WorkPlan.Create(CompressionMode.Decompress, config.BlockSize);
            var pipeline2 = new Pipeline(config, algs2);
            pipeline2.Process(compressedFile, restoredFile);

            var filesEqual = CheckFilesEqual(inputFile, restoredFile);

            Assert.True(filesEqual);
        }

        public static bool CheckFilesEqual(string a, string b)
        {
            using var fs1 = File.OpenRead(a);
            using var fs2 = File.OpenRead(b);

            while (true)
            {
                var byte1 = fs1.ReadByte();
                var byte2 = fs2.ReadByte();

                if (byte1 != byte2)
                {
                    Debug.WriteLine($"Files diffs at {fs1.Position}, total length {fs1.Length} {fs2.Length}");
                    return false;
                }

                if (byte1 == -1)
                {
                    Debug.WriteLine($"Files are equal, total length {fs1.Length} {fs2.Length}");
                    return true;
                }
            }
        }
    }
}
