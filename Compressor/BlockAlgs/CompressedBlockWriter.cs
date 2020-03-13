using System.IO;

namespace Compressor
{
    internal class CompressedBlockWriter : IBlockWriter
    {
        private readonly BinaryWriter _writer;

        public CompressedBlockWriter(FileStream stream)
        {
            _writer = new BinaryWriter(stream);
        }

        public void WriteBlock(Block block)
        {
            _writer.Write(block.Index);
            _writer.Write(block.Bytes.Length);
            _writer.Write(block.Bytes);
        }
    }
}