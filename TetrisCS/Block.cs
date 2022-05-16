namespace TetrisCS
{
    /// <summary> 테트리스 블록 하나 </summary>
    public class Block
    {
        public Vector pos;
        readonly BlockType type;
        int[,] shape;

        public Block(Vector pos, BlockType type, int[,]? shape = null)
        {
            this.pos = pos;
            this.type = type;
            this.shape = shape ?? BlockTypeToIntArray(type);
        }

        public static int[,] BlockTypeToIntArray(BlockType type) => type switch
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
            BlockType.None => new int[0, 0],
            _ => throw new UnknownBlockException()
        };

        /// <summary> 가장 큰 조각의 가로세로 길이 </summary>
        public const int MaximumSquareSize = 4;

        public BlockType Type => type;
        public int[,] Shape { get => shape; set => shape = value; }
        public int Width => shape.GetLength(1);
        public int Height => shape.GetLength(0);

        /// <summary> 자신을 오른쪽 또는 왼쪽으로 회전시킨 조각을 리턴한다. </summary>
        /// <param name="isClockwise"> <b>true</b> - 오른쪽으로 90도 회전 <br/>
        /// <b>false</b> - 왼쪽으로 90도 회전 <br/>
        /// <b>null</b> - 180도 회전 </param>
        public int[,] Rotate(bool? isClockwise)
        {
            var rotatedArr = new int[Height, Width];
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    switch (isClockwise)
                    {
                        case true: rotatedArr[j, Width-1 - i] = shape[i, j]; break; // 오른쪽으로 90도 
                        case false: rotatedArr[Height-1 - j, i] = shape[i, j]; break; // 왼쪽으로 90도
                        case null: rotatedArr[Height-1 - i, Width-1 - j] = shape[i, j]; break; // 180도
                    }
                }
            }
            return rotatedArr;
        }
        
        public Block Copy() => new Block(pos, type, shape);

        public int this[int y, int x] => shape[y, x];
    }

    class UnknownBlockException : Exception { }
}
