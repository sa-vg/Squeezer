using System;
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
        [InlineData(@"C:\Temp\New folder\JetBrains.ReSharperUltimate.2019.2.2.exe")]
        [InlineData(@"C:\Temp\VeeamBackup&Replication_9.5.4.2866.Update4b_.iso")]
        [InlineData(@"C:\Temp\New folder\VirtualBox-6.0.14-133895-Win.txt")]
        public void TestPipeline(string inputFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var compressedFile = Path.ChangeExtension(inputFile, "zip");
            var restoredFile = inputFile.Replace(fileName, fileName + "_restored");

            var pipeline1 = Pipeline.Create(CompressionMode.Compress);
            pipeline1.Process(inputFile, compressedFile);

            var pipeline2 = Pipeline.Create(CompressionMode.Decompress);
            pipeline2.Process(compressedFile, restoredFile);

            var fs1 = File.OpenRead(inputFile);
            var fs2 = File.OpenRead(restoredFile);

            var md5 = MD5.Create();
            var filesEqual = md5.ComputeHash(fs1).SequenceEqual(md5.ComputeHash(fs2));

            Assert.True(filesEqual);
        }
    }
}
