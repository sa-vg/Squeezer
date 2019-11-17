using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Pipelines
{
    class DecompressionPipeline : Pipeline
    {
        public override IEnumerable<Block> ReadBlocks(BinaryReader reader)
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var index = reader.ReadInt32();
                var size = reader.ReadInt32();
                var bytes = reader.ReadBytes(size);
                yield return new Block(index, bytes);
            }
        }

        public override Action<Block> WriteBlock(BinaryWriter writer)
        {
            return block =>
            {
                writer.BaseStream.Position = (long)block.Index * Config.BlockSize;
                writer.Write(block.Bytes);
                ArrayPool<byte>.Shared.Return(block.Bytes);
            };
        }

        //public override Func<Block, Block> TransformBlock()
        //{
        //    return block =>
        //    {
        //        var ms = new MemoryStream(block.Bytes);
        //        var result = new byte[Config.BlockSize];

        //        int bytesRead = 0;
        //        using (var gs = new GZipStream(ms, CompressionMode.Decompress)) bytesRead = gs.Read(result, 0, Config.BlockSize);

        //        //last block
        //        if (bytesRead != Config.BlockSize) Array.Resize(ref result, bytesRead);

        //        return new Block(block.Index, result);
        //    };
        //}
        public override Func<Block, Block> TransformBlock()
        {
            return block =>
            {
                var data = Decompress(block.Bytes, Config.BlockSize);
                return new Block(block.Index, data);
            };
        }

        private unsafe byte[] Decompress(ReadOnlySpan<byte> buffer, int decompressedSize)
        {
            fixed (byte* pBuffer = &buffer[0])
            {
                using (var stream = new UnmanagedMemoryStream(pBuffer, buffer.Length))
                {
                    using (var deflateStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        var data = ArrayPool<byte>.Shared.Rent(decompressedSize);
                        var bytesRead = deflateStream.Read(data, 0, decompressedSize);
                        if (bytesRead != decompressedSize) Array.Resize(ref data, bytesRead);
                        return data;
                    }
                }
            }
        }
    }
}