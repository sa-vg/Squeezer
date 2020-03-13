using System.Collections.Generic;
using System.IO;

namespace Compressor
{
    internal class CompressedBlockReader : IBlockReader
    {
        private readonly BinaryReader _reader;

        public CompressedBlockReader(FileStream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public IEnumerable<Block> ReadBlocks()
        {
            while (_reader.BaseStream.Position != _reader.BaseStream.Length)
            {
                var index = _reader.ReadInt32();
                var size = _reader.ReadInt32();
                var bytes = _reader.ReadBytes(size);
                yield return new Block(index, bytes);
            }
        }
    }
}