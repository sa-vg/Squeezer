using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Compressor;
using Xunit;

namespace PipelineTests
{
    public class Tests
    {
        [Theory]
        [InlineData(@"C:\Temp\V.iso")]
        [InlineData(@"C:\Temp\BIAS_FX_2_Windows64bit_v2_1_7_4820.msi")]
        public void TestPipeline(string inputFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var compressedFile = Path.ChangeExtension(inputFile, "zip");
            var restoredFile = inputFile.Replace(fileName, fileName + "_restored");

            var config = Config.Default;

            Program.ProcessFile(inputFile, compressedFile, CompressionMode.Compress, config);

            Program.ProcessFile(compressedFile, restoredFile, CompressionMode.Decompress, config);

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

                if (byte1 == -1) break;
            }

            Debug.WriteLine($"Files are equal, total length {fs1.Length} {fs2.Length}");
            return true;
        }
    }
}
