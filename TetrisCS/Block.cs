namespace TetrisCS
{
    /// <summary> 테트리스 블록 하나 </summary>
    public class Block
    {
        /// <summary> 현재 블록의 위치(좌표). 왼쪽 위를 기준으로 한다. </summary>
        public Vector pos;
        readonly BlockType type;
        int[,] shape;


        public Block(Vector pos, BlockType type, int[,]? shape = null)
        {
            this.pos = pos;
            this.type = type;
            this.shape = shape ?? BlockTypeToIntArray(type);
            Console.Write("");
        }


        #region Block Shape Arrays
        /// <summary> 블록 타입에 맞는 int[,] 배열을 얻을 수 있다. </summary>
        public static int[,] BlockTypeToIntArray(BlockType type) => type switch
        {
            BlockType.I => _i,
            BlockType.J => _j,
            BlockType.L => _l,
            BlockType.O => _o,
            BlockType.S => _s,
            BlockType.T => _t,
            BlockType.Z => _z,
            _ => new int[0, 0],
        };

        readonly static int[,] _i = new int[,]
        {
            { 0, 0, 0, 0 },
            { 1, 1, 1, 1 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
        };
        readonly static int[,] _j = new int[,]
        {
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 },
        };
        readonly static int[,] _l = new int[,]
        {
            { 0, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 0 },
        };
        readonly static int[,] _o = new int[,]
        {
            { 1, 1 },
            { 1, 1 },
        };
        readonly static int[,] _s = new int[,]
        {
            { 0, 1, 1 },
            { 1, 1, 0 },
            { 0, 0, 0 },
        };
        readonly static int[,] _t = new int[,]
        {
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 },
        };
        readonly static int[,] _z = new int[,]
        {
            { 1, 1, 0 },
            { 0, 1, 1 },
            { 0, 0, 0 },
        };
        #endregion


        /// <summary> 가장 큰 조각의 가로세로 길이 (수정 필요) </summary>
        public const int MaximumSquareSize = 4; // TODO: 수정 필요

        /// <summary> 블록 종류(타입) </summary>
        public BlockType Type => type;
        /// <summary> 현재 블록의 모양 (0 또는 1) </summary>
        public int[,] Shape { get => shape; set => shape = value; }
        /// <summary> 블록(Shape)의 가로 길이 </summary>
        public int Width => shape.GetLength(1);
        /// <summary> 블록(Shape)의 세로 길이 </summary>
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
        
        /// <summary> 자신 객체를 복사해서 새로 리턴한다. </summary>
        public Block Copy() => new Block(pos, type, shape);

        public int this[int y, int x] => shape[y, x];
    }
}
