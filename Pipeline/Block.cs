namespace Compressor
{
    public struct Block
    {
        public int Index { get; }
        public byte[] Bytes { get; }

        public Block(int index, byte[] bytes)
        {
            Index = index;
            Bytes = bytes;
        }
    }
}