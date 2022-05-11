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
        /// <summary> 게임에서 생성될 수 있는 블록의 종류 </summary>
        readonly BlockType[] blockTypes = { BlockType.I, BlockType.J, BlockType.L, BlockType.O, BlockType.S, BlockType.T, BlockType.Z };

        readonly System.Timers.Timer gravityTimer = new();
        readonly Random random = new();

        float gravity = 150;
        Block currentBlock;
        /// <summary> 7Bag, 여기에서 다음 블록을 하나씩 뽑는다 </summary>
        List<BlockType> bag = new();

        bool playing;


        public int[,] Map => map;
        public bool Playing => playing;
        public Block CurrentBlock => currentBlock;


        /// <summary> 테트리스를 시작한다. </summary>
        public void Play()
        {
            gravityTimer.Interval = gravity;
            gravityTimer.Elapsed += new ElapsedEventHandler(FallElapsed);

            FillBag();
            currentBlock = SpawnNewBlock();
            gravityTimer.Start();

            playing = true;
        }

        /// <summary> 가방(BlockType[] bag)을 새로 채운다. </summary>
        void FillBag()
        {
            Array.Copy(blockTypes, bag);
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
            Block block = new(new Vector(WIDTH / 2, 0), blockTypes[random.Next(0, blockTypes.Length)]);
            block.pos.x -= block.Width / 2;

            // map에 블록 집어넣기
            for (int y = 0; y < block.Height; y++)
            {
                for (int x = 0; x < block.Width; x++)
                {
                    if (block[y, x] == 1)
                    {
                        map[block.pos.y + y, block.pos.x + x] = 1;
                        positionOfCurrentBlock[block.pos.y + y, block.pos.x + x] = 1;
                    }
                }
            }

            return block;
        }
        
        /// <summary> 블록을 pos 위치에 놓을 수 있는지 검사한다. </summary>
        bool CanMove(ref Block block, Vector pos)
        {
            for (int y = 0; y < block.Height; y++)
            {
                for (int x = 0; x < block.Width; x++)
                {
                    if (block[y, x] == 1)
                    {
                        int nX = pos.x + x;
                        int nY = pos.y + y;

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
                for (int y = 0; y < currentBlock.Height; y++)
                {
                    for (int x = 0; x < currentBlock.Width; x++)
                    {
                        if (currentBlock[y, x] == 1)
                        {
                            map[currentBlock.pos.y + y, currentBlock.pos.x + x] = 0;
                            positionOfCurrentBlock[currentBlock.pos.y + y, currentBlock.pos.x + x] = 0;
                        }
                    }
                }

                // 새 위치에 블록 넣기
                for (int y = 0; y < currentBlock.Height; y++)
                {
                    for (int x = 0; x < currentBlock.Width; x++)
                    {
                        if (currentBlock[y, x] == 1)
                        {
                            map[currentBlock.pos.y + y + dir.y, currentBlock.pos.x + x + dir.x] = 1;
                            positionOfCurrentBlock[currentBlock.pos.y + y + dir.y, currentBlock.pos.x + x + dir.x] = 1;
                        }
                    }
                }

                currentBlock.pos += dir;
            }
            
            // 옮길 수 없는 경우 기본적으로 무시되나, 아래쪽으로 이동할 수 없는 경우는 블록이 땅에 닿았다는 뜻
            else if (dir == Vector.Down)
            {
                Place();
                return false;
            }

            return true;
        }

        /// <summary> 현재 블록을 회전시킨다. </summary>
        void Rotate(Vector dir)
        {
            throw new NotImplementedException();
        }

        /// <summary> 블록이 땅에 안착했을 때 실행된다. 다음 블록을 생성한다. </summary>
        void Place()
        {
            // currentBlock 초기화
            if (currentBlock.shape != null)
            {
                for (int y = 0; y < currentBlock.Height; y++)
                {
                    for (int x = 0; x < currentBlock.Width; x++)
                    {
                        if (currentBlock[y, x] == 1)
                        {
                            positionOfCurrentBlock[currentBlock.pos.y + y, currentBlock.pos.x + x] = 0;
                        }
                    }
                }
            }

            // 라인 클리어
            int lineClearCount = 0;
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    if (map[y, x] == 0)
                    {
                        goto NextLine;
                    }
                }

                // 라인 한 칸씩 내리기
                lineClearCount++;
                for (int y2 = y; y > 0; y2--)
                {
                    for (int x = 0; x < WIDTH; x++)
                    {
                        map[y2, x] = map[y2-1, x];
                    }
                }
                y--;

                NextLine:
                continue;
            }

            // 다음 블록 생성
            currentBlock = SpawnNewBlock();
        }

        /// <summary> 게임이 끝났을 때 호출 </summary>
        void GameOver()
        {
            playing = false;
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
}
