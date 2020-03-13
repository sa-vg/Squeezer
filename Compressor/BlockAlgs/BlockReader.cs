using System.Collections.Generic;
using System.IO;

namespace Compressor
{
    internal class BlockReader : IBlockReader
    {
        private readonly int _blockSize;
        private readonly BinaryReader _reader;

        public BlockReader(FileStream stream, int blockSize)
        {
            _blockSize = blockSize;
            _reader = new BinaryReader(stream);
        }

        public IEnumerable<Block> ReadBlocks()
        {
            int index = 0;
            while (true)
            {
                var bytes = _reader.ReadBytes(_blockSize);
                if (bytes.Length > 0) yield return new Block(index++, bytes);
                else break;
            }
        }
    }
}