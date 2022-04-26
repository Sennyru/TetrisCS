using System.Text;
using System.Timers;

namespace TetrisCS
{
    public class Tetris
    {
        public const int WIDTH = 10, HEIGHT = 20;

        /// <summary> 테트리스 맵 </summary>
        readonly int[,] map = new int[HEIGHT, WIDTH];
        /// <summary> currentBlock이 있는 위치 </summary>
        readonly int[,] positionOfCurrentBlock = new int[HEIGHT, WIDTH];
        readonly System.Timers.Timer gravityTimer = new();

        bool isPlaying;
        float gravity = 150;
        Block currentBlock;


        public int[,] Map => map;
        public bool Playing => isPlaying;
        public Block CurrentBlock => currentBlock;


        /// <summary> 테트리스를 시작한다. </summary>
        public void Play()
        {
            isPlaying = true;

            gravityTimer.Interval = gravity;
            gravityTimer.Elapsed += new ElapsedEventHandler(FallElapsed);

            currentBlock = SpawnNewBlock();
            gravityTimer.Start();
        }

        /// <summary> gravity 시간마다 호출되는 함수 <br/>
        /// gravityTimer.Elapsed 이벤트에 연결되어 있다. </summary>
        void FallElapsed(object? sender, ElapsedEventArgs e)
        {
            MoveBlockTo(Vector.Down);
        }

        /// <summary> 새로운 블록을 생성하고, 그것을 map에 넣는다. </summary>
        /// <returns> 생성된 블록 </returns>
        Block SpawnNewBlock()
        {
            if (currentBlock.shape != null)
            {
                for (int i = 0; i < currentBlock.Height; i++)
                {
                    for (int j = 0; j < currentBlock.Width; j++)
                    {
                        if (currentBlock[i, j] == 1)
                        {
                            positionOfCurrentBlock[currentBlock.pos.y + i, currentBlock.pos.x + j] = 0;
                        }
                    }
                }
            }
            
            if (currentBlock.pos.y <= 0)
            {
                GameOver();
            }

            Block block = new(new Vector(WIDTH / 2, 0), BlockType.T);
            block.pos.x -= block.Width / 2;

            // map에 블록 집어넣기
            for (int i = 0; i < block.Height; i++)
            {
                for (int j = 0; j < block.Width; j++)
                {
                    if (block[i, j] == 1)
                    {
                        map[block.pos.y + i, block.pos.x + j] = 1;
                        positionOfCurrentBlock[block.pos.y + i, block.pos.x + j] = 1;
                    }
                }
            }

            return block;
        }

        /// <summary> 블록을 pos 위치에 놓을 수 있는지 검사한다. </summary>
        bool CanMove(ref Block block, Vector pos)
        {
            for (int i = 0; i < block.Height; i++)
            {
                for (int j = 0; j < block.Width; j++)
                {
                    if (block[i, j] == 1)
                    {
                        int nX = pos.x + j;
                        int nY = pos.y + i;

                        // 벽에 닿았다면(칸 위치가 배열을 넘어섰다면) 옮길 수 없다
                        if (0 > nY || nY >= HEIGHT || 0 > nX || nX >= WIDTH)
                        {
                            return false;
                        }

                        // 놓을 칸에 공간이 없고(1) 자기 칸이 아니라면 옮길 수 없다
                        else if (map[nY, nX] == 1 && positionOfCurrentBlock[nY, nX] != 1)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary> 블록을 pos만큼 옮긴다. </summary>
        /// <returns> 옮길 수 없어서 다음 블록이 생겼다면 false를 리턴한다. 그 외에는 true를 리턴한다. </returns>
        bool MoveBlockTo(Vector dir)
        {
            // 옮길 수 있는 경우
            if (CanMove(ref currentBlock, currentBlock.pos + dir))
            {
                // 맵에서 블록 지우기
                for (int i = 0; i < currentBlock.Height; i++)
                {
                    for (int j = 0; j < currentBlock.Width; j++)
                    {
                        if (currentBlock[i, j] == 1)
                        {
                            map[currentBlock.pos.y + i, currentBlock.pos.x + j] = 0;
                            positionOfCurrentBlock[currentBlock.pos.y + i, currentBlock.pos.x + j] = 0;
                        }
                    }
                }

                // 새 위치에 블록 넣기
                for (int i = 0; i < currentBlock.Height; i++)
                {
                    for (int j = 0; j < currentBlock.Width; j++)
                    {
                        if (currentBlock[i, j] == 1)
                        {
                            map[currentBlock.pos.y + i + dir.y, currentBlock.pos.x + j + dir.x] = 1;
                            positionOfCurrentBlock[currentBlock.pos.y + i + dir.y, currentBlock.pos.x + j + dir.x] = 1;
                        }
                    }
                }

                currentBlock.pos += dir;
            }

            // 옮길 수 없는 경우 기본적으로 무시되나, 아래쪽으로 이동할 수 없는 경우는 블록이 땅에 닿았다는 뜻
            else if (dir == Vector.Down)
            {
                currentBlock = SpawnNewBlock();
                return false;
            }

            return true;
        }

        void Rotate(Vector dir)
        {

        }

        /// <summary> 게임이 끝났을 때 호출 </summary>
        void GameOver()
        {
            isPlaying = false;
            gravityTimer.Close();
        }


        /// <summary> 오른쪽 키 누르기 </summary>
        public void InputRight()
        {
            MoveBlockTo(Vector.Right);
        }

        /// <summary> 왼쪽 키 누르기 </summary>
        public void InputLeft()
        {
            MoveBlockTo(Vector.Left);
        }

        /// <summary> 블록 한 칸 아래로 떨어뜨리기 </summary>
        public void SoftDrop()
        {
            MoveBlockTo(Vector.Down);
        }

        /// <summary> 블록을 바닥에 한 번에 떨어뜨리기 </summary>
        public void HardDrop()
        {
            while (MoveBlockTo(Vector.Down));
        }

        /// <summary> 블록을 오른쪽으로 90도 회전시키기 </summary>
        public void RotateRight()
        {

        }

        /// <summary> 블록을 왼쪽으로 90도 회전시키기 </summary>
        public void RotateLeft()
        {

        }

        /// <summary> 블록을 180도 회전시키기 (뒤집기) </summary>
        public void Rotate180()
        {

        }

        /// <summary> 현재 맵을 사각형 문자(■, □)들로 변환한다. </summary>
        public string GetStringMap()
        {
            StringBuilder sb = new();

            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    if (positionOfCurrentBlock[i, j] == 1)
                    {
                        sb.Append('▣');
                    }
                    else
                    {
                        sb.Append(map[i, j] >= 1 ? '■' : '□');
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }


    /// <summary> (x, y) 좌표를 저장할 수 있는 구조체 </summary>
    public struct Vector
    {
        public int x, y;

        public Vector(int x, int y) { this.x = x; this.y = y; }

        /// <summary> new Vector(1, 0) </summary>
        public static Vector Right => new(1, 0);
        /// <summary> new Vector(-1, 0) </summary>
        public static Vector Left => new(-1, 0);
        /// <summary> new Vector(0, 1) <br/> 좌표계가 다르므로 유의. </summary>
        public static Vector Down => new(0, 1);

        public static Vector operator +(Vector a, Vector b) => new Vector(a.x + b.x, a.y + b.y);
        public static Vector operator -(Vector a, Vector b) => new Vector(a.x - b.x, a.y - b.y);
        public static bool operator ==(Vector a, Vector b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Vector a, Vector b) => !(a == b);
        public override bool Equals(object? obj) => this == obj as Vector?; // ???
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"({x}, {y})";
    }

    /// <summary> 테트리스 블록 하나 </summary>
    public struct Block
    {
        public Vector pos;
        public BlockType type;
        public int[,] shape;

        public Block(Vector pos, BlockType type)
        {
            this.pos = pos;
            this.type = type;
            shape = new int[,]
            {
                { 0, 1, 0 },
                { 1, 1, 1 },
                { 0, 0, 0 },
            };
        }

        public int Width => shape.GetLength(1);
        public int Height => shape.GetLength(0);

        public int this[int y, int x] => shape[y, x];
    }

    /// <summary> 블록 모양의 종류 </summary>
    public enum BlockType { Empty, I, J, L, O, S, T, Z, Garbage }

}
