
using System.Timers;

namespace TetrisCS
{
    public class Tetris
    {
        #region Constants
        public const int Width = 10, Height = 20;
        #endregion
        
        #region Private Fields
        /// <summary> 테트리스 맵 </summary>
        readonly int[,] map = new int[Height, Width];
        /// <summary> currentBlock이 있는 위치 </summary>
        readonly int[,] positionOfCurrentBlock = new int[Height, Width];
        /// <summary> 게임에서 생성될 수 있는 블록의 종류 </summary>
        readonly BlockType[] blockTypes = { BlockType.I, BlockType.J, BlockType.L, BlockType.O, BlockType.S, BlockType.T, BlockType.Z };

        readonly System.Timers.Timer gravityTimer = new();
        readonly Random random = new();

        /// <summary> FallElapsed 호출 간격 (ms) </summary>
        float gravity = 500;
        /// <summary> nextBag의 크기 </summary>
        int bagSize = 4;
        Block currentBlock;
        /// <summary> 7Bag </summary>
        List<BlockType> nextBag = new();
        /// <summary> 다음에 나올 블록 목록 큐 </summary>
        List<BlockType> bag = new();
        BlockType holdingBlock = BlockType.None;
        int b2bCombo;

        bool playing;
        #endregion
        

        #region Properties
        public int[,] Map => map;
        public bool Playing => playing;
        public Block CurrentBlock => currentBlock;
        public int[,] PositionOfCurrentBlock => positionOfCurrentBlock;
        public BlockType HoldingBlock => holdingBlock;
        public List<BlockType> Bag => bag;
        public int BagSize { get => bagSize; set => bagSize = value; }
        #endregion

        #region Events
        public event MapUpdateEventHandler? MapUpdateEvent;
        public event LineClearEventHandler? LineClearEvent;
        public event HoldEventHandler? HoldEvent;
        public event PlaceEventHandler? PlaceEvent;
        #endregion


        #region Private Methods
        /// <summary> 테트리스를 시작한다. </summary>
        public void Play()
        {
            gravityTimer.Interval = gravity;
            gravityTimer.Elapsed += new ElapsedEventHandler(FallElapsed);

            for (int i = 0; i < bagSize; i++)
                InsertBag();
            currentBlock = SpawnNewBlock(GetNextBlock());
            gravityTimer.Start();

            playing = true;

            PlaceEvent?.Invoke();
        }

        /// <summary> nextBag에서 하나를 뽑아서 bag에 넣는다. </summary>
        void InsertBag()
        {
            if (nextBag.Count == 0)
                RefillNextBag();

            bag.Add(nextBag[0]);
            nextBag.RemoveAt(0);
        }

        /// <summary> nextBag을 새로 채운다. </summary>
        void RefillNextBag()
        {
            nextBag = blockTypes.OrderBy(_ => random.Next()).ToList();
        }

        /// <summary> gravity 시간마다 호출되는 함수 <br/>
        /// gravityTimer.Elapsed 이벤트에 연결되어 있다. </summary>
        void FallElapsed(object? sender, ElapsedEventArgs e)
        {
            MoveBlockTo(Vector.Down);
        }

        /// <summary> 새로운 블록을 생성하고, 그것을 map에 넣는다. </summary>
        /// <returns> 생성된 블록 </returns>
        Block SpawnNewBlock(BlockType type)
        {
            Block block = new(new Vector(Width / 2, 0), type);
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

            MapUpdateEvent?.Invoke();
            gravityTimer.Stop();
            gravityTimer.Start();

            return block;
        }

        /// <summary> bag에서 다음 블록을 뽑아오고 bag을 하나 채운다. </summary>
        BlockType GetNextBlock()
        {
            var next = bag[0];
            bag.RemoveAt(0);
            InsertBag();
            return next;
        }

        /// <summary> 블록(을 pos 위치에 놓을 수 있는지 검사한다. </summary>
        bool CanMove(int[,] block, Vector pos)
        {
            for (int y = 0; y < block.GetLength(0); y++)
            {
                for (int x = 0; x < block.GetLength(1); x++)
                {
                    if (block[y, x] == 1)
                    {
                        int nX = pos.x + x;
                        int nY = pos.y + y;

                        // 벽에 닿았다면(칸 위치가 배열을 넘어섰다면) 옮길 수 없다
                        if (0 > nY || nY >= Height || 0 > nX || nX >= Width)
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
        /// <param name="dir"> 옮기는 방향 (Right/Left/Down) </param>
        /// <param name="hardDrop"> 하드 드롭 여부 </param>
        /// <returns> 옮길 수 없어서 다음 블록이 생겼다면 false를 리턴한다. 그 외에는 true를 리턴한다. </returns>
        bool MoveBlockTo(Vector dir, bool hardDrop = false)
        {
            // 옮길 수 있는 경우
            if (CanMove(currentBlock.Shape, currentBlock.pos + dir))
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

                if (!hardDrop) MapUpdateEvent?.Invoke();

                return true;
            }
            
            // 옮길 수 없는 경우 기본적으로 무시되나, 아래쪽으로 이동할 수 없는 경우는 블록이 땅에 닿았다는 뜻
            else if (dir == Vector.Down)
            {
                Place();
                if (hardDrop) MapUpdateEvent?.Invoke();
            }

            return false;
        }

        /// <summary> 현재 블록이 회전 가능한지 체크하고, 가능하다면 회전시킨다. </summary>
        /// <param name="isClockwise"> <b>true</b> - 오른쪽으로 90도 회전 <br/>
        /// <b>false</b> - 왼쪽으로 90도 회전 <br/>
        /// <b>null</b> - 180도 회전 </param>
        void Rotate(bool? isClockwise)
        {
            var rotated = currentBlock.Rotate(isClockwise);

            // 회전이 가능하면
            if (CanMove(rotated, CurrentBlock.pos))
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

                currentBlock.Shape = rotated;

                // 새 위치에 블록 넣기
                for (int y = 0; y < currentBlock.Height; y++)
                {
                    for (int x = 0; x < currentBlock.Width; x++)
                    {
                        if (currentBlock[y, x] == 1)
                        {
                            map[currentBlock.pos.y + y, currentBlock.pos.x + x] = 1;
                            positionOfCurrentBlock[currentBlock.pos.y + y, currentBlock.pos.x + x] = 1;
                        }
                    }
                }

                MapUpdateEvent?.Invoke();
            }
        }

        /// <summary> 블록이 땅에 안착했을 때 실행된다. 다음 블록을 생성한다. </summary>
        void Place()
        {
            // currentBlock 초기화
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

            // 라인 클리어
            int lineClearCount = 0;
            for (int y = Height-1; y >= lineClearCount; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (map[y, x] == 0)
                    {
                        goto NextLine;
                    }
                }

                for (int y2 = y; y2 > 0; y2--)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        map[y2, x] = map[y2-1, x];
                    }
                }
                lineClearCount++;
                y++;

                NextLine: { }
            }
            b2bCombo = lineClearCount >= 4 ? b2bCombo + 1 : 0;

            // 게임 오버 판정
            if (lineClearCount == 0 && currentBlock.pos.y == 0)
            {
                GameOver();
            }
            else
            {
                // 다음 블록 생성
                currentBlock = SpawnNewBlock(GetNextBlock());
            }

            MapUpdateEvent?.Invoke();
            PlaceEvent?.Invoke();
            if (lineClearCount > 0) LineClearEvent?.Invoke(lineClearCount, b2bCombo);
        }

        /// <summary> 홀드 전환 </summary>
        void Holding()
        {
            var temp = holdingBlock;
            holdingBlock = currentBlock.Type;

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

            currentBlock = SpawnNewBlock(temp == BlockType.None ? GetNextBlock() : temp);

            HoldEvent?.Invoke();
            MapUpdateEvent?.Invoke();
        }

        /// <summary> 게임이 끝났을 때 호출 </summary>
        void GameOver()
        {
            playing = false;
            gravityTimer.Close();
        }
        #endregion
        
        #region Public Inputs
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
            while (MoveBlockTo(Vector.Down, true));
        }

        /// <summary> 블록을 오른쪽으로 90도 회전시키기 </summary>
        public void RotateRight()
        {
            Rotate(isClockwise: true);
        }

        /// <summary> 블록을 왼쪽으로 90도 회전시키기 </summary>
        public void RotateLeft()
        {
            Rotate(isClockwise: false);
        }

        /// <summary> 블록을 180도 회전시키기 (뒤집기) </summary>
        public void Rotate180()
        {
            Rotate(isClockwise: null);
        }

        /// <summary> 홀드 </summary>
        public void Hold()
        {
            Holding();
        }
        #endregion
        
    }
}
