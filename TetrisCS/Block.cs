namespace TetrisCS
{
    /// <summary> 테트리스 블록 하나 </summary>
    public class Block
    {
        public Vector pos;

        BlockType type;
        int[,] shape;

        public Block(Vector pos, BlockType type)
        {
            this.pos = pos;
            this.type = type;

            shape = type switch
            {
                BlockType.I => new int[,]
                {
                    { 0, 0, 0, 0 },
                    { 1, 1, 1, 1 },
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 },
                },
                BlockType.J => new int[,]
                {
                    { 1, 0, 0 },
                    { 1, 1, 1 },
                    { 0, 0, 0 },
                },
                BlockType.L => new int[,]
                {
                    { 0, 0, 1 },
                    { 1, 1, 1 },
                    { 0, 0, 0 },
                },
                BlockType.O => new int[,]
                {
                    { 1, 1 },
                    { 1, 1 },
                },
                BlockType.S => new int[,]
                {
                    { 0, 1, 1 },
                    { 1, 1, 0 },
                    { 0, 0, 0 },
                },
                BlockType.T => new int[,]
                {
                    { 0, 1, 0 },
                    { 1, 1, 1 },
                    { 0, 0, 0 },
                },
                BlockType.Z => new int[,]
                {
                    { 1, 1, 0 },
                    { 0, 1, 1 },
                    { 0, 0, 0 },
                },
                _ => throw new UnknownBlockException()
            };
        }

        public int[,] Shape => shape;
        public int Width => shape.GetLength(1);
        public int Height => shape.GetLength(0);

        public int[,] Rotate()
        {
            throw new NotImplementedException();
        }

        public int this[int y, int x] => shape[y, x];

        class UnknownBlockException : Exception { }
    }
}
