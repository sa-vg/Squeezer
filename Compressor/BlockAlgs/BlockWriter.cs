using System.IO;

namespace Compressor
{
    internal class BlockWriter : IBlockWriter
    {
        private readonly int _blockSize;
        private readonly BinaryWriter _writer;

        public BlockWriter(FileStream stream, int blockSize)
        {
            _blockSize = blockSize;
            _writer = new BinaryWriter(stream);
        }

        public void WriteBlock(Block block)
        {
            _writer.BaseStream.Position = (long)block.Index * _blockSize;
            _writer.Write(block.Bytes);
        }
    }
}