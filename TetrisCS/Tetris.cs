namespace TetrisCS;

public partial class Tetris
{
    #region Fields
    private readonly int width;
    private readonly int height;
    private readonly int nextQueueSize;
    private readonly BlockType[] availableBlockTypes;
    
    private readonly int[,] map;
    private readonly bool[,] positionOfCurrentBlock;
    private bool[,] ghost;
    private readonly Queue<BlockType> nextBlockQueue;
    private bool playing;
    private Block currentBlock;
    private BlockType holdingBlock;
    private float gravity;
    private int b2bCombo;

    private readonly System.Timers.Timer gravityTimer = new();
    private readonly System.Timers.Timer lockTimer = new();
    private readonly Random random = new();
    private readonly Queue<BlockType> bagBuffer = new();
    
    private event TetrisEventHandler? MapUpdateEvent, LineClearEvent, HoldEvent, PlaceEvent, DebugEvent;
    #endregion


    #region Properties
    /// <summary> 맵의 가로 길이 </summary>
    public int Width => width;
    /// <summary> 맵의 세로 길이 </summary>
    public int Height => height;
    /// <summary> 다음에 나올 블록이 표시되는 개수 </summary>
    public int NextQueueSize => nextQueueSize;
    /// <summary> 현재 테트리스 맵 </summary>
    public int[,] Map => map;
    /// <summary> 현재 조작하고 있는 블록의 위치 </summary>
    public bool[,] PositionOfCurrentBlock => positionOfCurrentBlock;
    /// <summary> 고스트 위치 </summary>
    public bool[,] Ghost => ghost;
    /// <summary> 다음에 나올 블록 큐 </summary>
    public Queue<BlockType> NextBlockQueue => nextBlockQueue;
    /// <summary> 현재 게임이 플레이 중인가? </summary>
    public bool Playing => playing;
    /// <summary> 현재 조작하고 있는 블록 객체 </summary>
    public Block CurrentBlock => currentBlock;
    /// <summary> 홀드에 있는 블록 객체 </summary>
    public BlockType HoldingBlock => holdingBlock;
    /// <summary> 현재 중력 (한 칸 아래로 내려오기까지 걸리는 시간, ms) </summary>
    public float Gravity => gravity;
    /// <summary> 현재 Back to Back 콤보 횟수 </summary>
    public int B2BCombo => b2bCombo;

    /// <summary> 맵이 업데이트됐을 때 발동되는 이벤트 </summary>
    public event TetrisEventHandler OnMapUpdated { add => MapUpdateEvent += value; remove => MapUpdateEvent -= value; }
    /// <summary> 줄을 없앴을 때 발동되는 이벤트 </summary>
    public event TetrisEventHandler OnLineCleared { add => LineClearEvent += value; remove => LineClearEvent -= value; }
    /// <summary> 홀드가 바뀌었을 때 발동되는 이벤트 </summary>
    public event TetrisEventHandler OnHold { add => HoldEvent += value; remove => HoldEvent -= value; }
    /// <summary> 블록이 바닥에 안착했을 때 발동되는 이벤트 </summary>
    public event TetrisEventHandler OnPlaced { add => PlaceEvent += value; remove => PlaceEvent -= value; }
    /// <summary> 디버그 메세지가 왔을 때 발동되는 이벤트 </summary>
    public event TetrisEventHandler OnDebugMessageSent { add => DebugEvent += value; remove => DebugEvent -= value; }
    #endregion


    public Tetris(int width = 10, int height = 20, int nextQueueSize = 4, float initialGravity = 500, BlockType[]? availableBlockTypes = null)
    {
        #region Arguments Checking
        if (width < 1)
            throw new ArgumentException("가로 길이는 최소 1 이상이어야 합니다.", nameof(width));
        if (height < 1)
            throw new ArgumentException("세로 길이는 최소 1 이상이어야 합니다.", nameof(height));
        if (nextQueueSize < 0)
            throw new ArgumentException("다음 블록의 개수는 음수가 될 수 없습니다.", nameof(nextQueueSize));
        if (initialGravity < 0)
            throw new ArgumentException("중력은 음수가 될 수 없습니다.", nameof(initialGravity));
        #endregion

        this.width = width;
        this.height = height;
        this.nextQueueSize = nextQueueSize;
        this.availableBlockTypes = availableBlockTypes ?? new BlockType[] { BlockType.I, BlockType.J, BlockType.L, BlockType.O, BlockType.S, BlockType.T, BlockType.Z };

        holdingBlock = BlockType.None;
        gravity = initialGravity;

        map = new int[height, width];
        positionOfCurrentBlock = new bool[height, width];
        ghost = new bool[height, width];
        nextBlockQueue = new();

        for (int i = 0; i < nextQueueSize; i++)
            InsertNextQueue();
        currentBlock = MakeNewBlock(GetNextBlock());

        gravityTimer.Interval = gravity;
        gravityTimer.Elapsed += (sender, e) => MoveBlockTo(Vector.Down);
        lockTimer.Interval = 500;
        lockTimer.Elapsed += (sender, e) => Place();
    }


    /// <summary> 테트리스 게임을 시작한다. </summary>
    public void Play()
    {
        gravityTimer.Start();
        playing = true;

        MapUpdateEvent?.Invoke();
        PlaceEvent?.Invoke();
    }


    #region Private Functions
    /// <summary> bagBuffer에서 블록 하나를 뽑아서 다음 블록 큐에 넣는다. 만약 bagBuffer가 비어 있다면 새로 생성한다. </summary>
    private void InsertNextQueue()
    {
        if (bagBuffer.Count == 0)
        {
            // bag 랜덤하게 채우기
            foreach (var type in availableBlockTypes.OrderBy(_ => random.Next()))
            {
                bagBuffer.Enqueue(type);
            }
        }

        nextBlockQueue.Enqueue(bagBuffer.Dequeue());
    }

    /// <summary> 다음 블록 큐에서 다음 블록을 뽑아오고, 큐에 하나를 넣는다. </summary>
    BlockType GetNextBlock()
    {
        InsertNextQueue();
        return nextBlockQueue.Dequeue();
    }

    /// <summary> 새로운 블록을 생성하고, 그것을 map에 넣는다. </summary>
    /// <param name="type"> 생성할 블록 타입 </param>
    /// <returns> 생성된 블록 </returns>
    Block MakeNewBlock(BlockType type)
    {
        Block block = new(new Vector(width / 2, 0), type);
        block.pos.x -= block.Width / 2;

        InsertBlockOnMap(block);

        gravityTimer.Stop();
        gravityTimer.Start();

        MapUpdateEvent?.Invoke();

        return block;
    }

    /// <summary> map에 블록을 넣는다. ( + positionOfCurrentBlock) </summary>
    /// <param name="block"> 넣을 블록 객체 </param>
    private void InsertBlockOnMap(Block block)
    {
        for (int y = 0; y < block.Height; y++)
        {
            for (int x = 0; x < block.Width; x++)
            {
                if (block[y, x] == 1)
                {
                    map[block.pos.y + y, block.pos.x + x] = (int)block.Type;
                    positionOfCurrentBlock[block.pos.y + y, block.pos.x + x] = true;
                }
            }
        }

        CreateGhost(block);
    }

    /// <summary> 고스트의 위치를 계산한다. </summary>
    /// <param name="block"> 계산할 블록 </param>
    /// <param name="inplace"> ghost 배열에 계산한 고스트를 넣는지 여부. 단순히 고스트 위치만 알아낼 거면 <b>false</b>. </param>
    Vector CreateGhost(Block block, bool inplace = true)
    {
        var ghostPos = block.pos;
        while (true)
        {
            if (CanMove(block.Shape, ghostPos + Vector.Down))
                ghostPos += Vector.Down;
            else break;
        }

        if (inplace)
        {
            ghost = new bool[height, width];
            for (int y = 0; y < block.Height; y++)
            {
                for (int x = 0; x < block.Width; x++)
                {
                    if (block[y, x] == 1)
                    {
                        ghost[ghostPos.y + y, ghostPos.x + x] = true;
                    }
                }
            }
        }

        return ghostPos;
    }

    /// <summary> map에서 블록을 제거한다. ( + positionOfCurrentBlock) </summary>
    /// <param name="block"> 제거할 블록 객체 </param>
    private void RemoveBlockOnMap(Block block)
    {
        for (int y = 0; y < block.Height; y++)
        {
            for (int x = 0; x < block.Width; x++)
            {
                if (block[y, x] == 1)
                {
                    map[block.pos.y + y, block.pos.x + x] = 0;
                    positionOfCurrentBlock[block.pos.y + y, block.pos.x + x] = false;
                }
            }
        }
    }

    /// <summary> 블록을 pos 위치에 놓을 수 있는지 검사한다. </summary>
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
                    if (!(0 <= nY && nY < height && 0 <= nX && nX < width))
                    {
                        return false;
                    }

                    // 놓을 칸에 공간이 없고(1) 자기 칸이 아니라면 옮길 수 없다
                    if (map[nY, nX] != 0 && positionOfCurrentBlock[nY, nX] != true)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary> 블록을 pos만큼 옮긴다. </summary>
    /// <param name="dir"> 옮기는 방향 (Right / Left / Down) </param>
    /// <param name="hardDrop"> 하드 드롭 여부 </param>
    private void MoveBlockTo(Vector dir, bool hardDrop = false)
    {
        if (hardDrop == false)
        {
            // 옮길 수 있는 경우
            if (CanMove(currentBlock.Shape, currentBlock.pos + dir))
            {
                RemoveBlockOnMap(currentBlock);
                currentBlock.pos += dir;
                InsertBlockOnMap(currentBlock);

                MapUpdateEvent?.Invoke();
            }

            // 아래로 움직일 수 없다면(바닥에 닿았다면)
            if (!CanMove(currentBlock.Shape, currentBlock.pos + Vector.Down))
            {
                gravityTimer.Stop();
                lockTimer.Start();
            }
            // 다시 아래로 움직일 수 있게 된다면
            else
            {
                if (!gravityTimer.Enabled) gravityTimer.Start();
                if (lockTimer.Enabled) lockTimer.Stop();
            }
        }
        
        // 하드 드롭
        else
        {
            Vector ghostPos = CreateGhost(currentBlock);
            RemoveBlockOnMap(currentBlock);
            currentBlock.pos = ghostPos;
            InsertBlockOnMap(currentBlock);
            Place();
        }
    }

    /// <summary> 현재 블록이 회전 가능한지 체크하고, 가능하다면 회전시킨다. </summary>
    /// <param name="isClockwise"> <b>true</b> - 오른쪽으로 90도 회전 <br/>
    /// <b>false</b> - 왼쪽으로 90도 회전 <br/>
    /// <b>null</b> - 180도 회전 </param>
    private void Rotate(bool? isClockwise)
    {
        var rotated = currentBlock.Rotate(isClockwise);

        // 회전이 가능하면
        if (CanMove(rotated, CurrentBlock.pos))
        {
            RemoveBlockOnMap(currentBlock);
            currentBlock.Shape = rotated;
            currentBlock.RotationState += isClockwise switch { true => 1, false => -1, null => 2 };
            InsertBlockOnMap(currentBlock);

            MapUpdateEvent?.Invoke();
        }
    }

    /// <summary> 블록이 땅에 안착했을 때 실행된다. 다음 블록을 생성한다. </summary>
    private void Place()
    {
        if (lockTimer.Enabled) lockTimer.Stop();

        // positionOfCurrentBlock 초기화
        for (int y = 0; y < currentBlock.Height; y++)
        {
            for (int x = 0; x < currentBlock.Width; x++)
            {
                if (currentBlock[y, x] == 1)
                {
                    positionOfCurrentBlock[currentBlock.pos.y + y, currentBlock.pos.x + x] = false;
                }
            }
        }

        // 라인 클리어
        int lineClearCount = 0;
        for (int y = height-1; y >= lineClearCount; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[y, x] == 0)
                {
                    goto NextLine;
                }
            }

            for (int y2 = y; y2 > 0; y2--)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y2, x] = map[y2-1, x];
                }
            }
            lineClearCount++;
            y++;

            NextLine: { }
        }

        b2bCombo = lineClearCount switch
        {
            0 => b2bCombo,
            >= 4 => b2bCombo + 1,
            _ => 0
        };

        // 게임 오버 판정 (라인 클리어를 못 했고 마지막 블록 위치가 맨 위라면)
        if (lineClearCount == 0 && currentBlock.pos.y == 0)
        {
            GameOver();
        }
        else
        {
            // 다음 블록 생성
            currentBlock = MakeNewBlock(GetNextBlock());
        }

        MapUpdateEvent?.Invoke();
        PlaceEvent?.Invoke();
        if (lineClearCount > 0)
            LineClearEvent?.Invoke(new TetrisEventArgs { LineClearCount = lineClearCount, B2BCombo = b2bCombo });
    }

    /// <summary> 홀드 전환 </summary>
    private void Holding()
    {
        var temp = holdingBlock;
        holdingBlock = currentBlock.Type;

        // 맵에서 블록 지우기
        RemoveBlockOnMap(currentBlock);

        currentBlock = MakeNewBlock(temp == BlockType.None ? GetNextBlock() : temp);

        HoldEvent?.Invoke();
        MapUpdateEvent?.Invoke();
    }

    /// <summary> 게임이 끝났을 때 호출 </summary>
    private void GameOver()
    {
        playing = false;
        gravityTimer.Close();
    }

    /// <summary> 디버그 이벤트를 발생시킨다. </summary>
    /// <param name="log"> 디버그 메세지 </param>
    private void Debug(string log)
    {
        DebugEvent?.Invoke(new TetrisEventArgs { DebugMessage = log });
    }
    #endregion


    static void Main() { }
}
