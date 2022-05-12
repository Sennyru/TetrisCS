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
        
        /// <summary> 자신을 오른쪽(시계방향)으로 90도 회전시킨 조각을 리턴한다. </summary>
        public int[,] Rotate(bool isClockwise)
        {
            var rotatedArr = new int[shape.Height, shape.Width];
            for (int i = 0; i < shape.Height; i++)
            {
                for (int j = 0; j < shape.Width; j++)
                {
                    if (isClockwise)
                        rotatedArr[j, shape.Width-1 - i] = shape[i, j];
                    else
                        rotatedArr[j, shape.Width-1 - i] = shape[i, j]; // 여기 두줄위에 코드 복붙한 거니까 이거 왼쪽 돌리기로 수정하셈
                }
            }
            return rotatedArr;
        }

        public int this[int y, int x] => shape[y, x];

        class UnknownBlockException : Exception { }
    }
}
