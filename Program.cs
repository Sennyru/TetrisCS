using TetrisCS;

class Program
{
    static readonly Tetris tetris = new();
    static readonly Vector offset = new Vector(50, 0);

    static EventQueue eventQueue;
    static int lineClearCount;
    static int b2bCombo;

    [Flags] enum EventQueue
    {
        Map = 1 << 0,
        LineClear = 1 << 1,
        Hold = 1 << 2,
        Place = 1 << 3,
    }


    static void Main()
    {
        Console.CursorVisible = false;

        tetris.MapUpdateEvent += new TetrisEventHandler(MapEnque);
        tetris.LineClearEvent += new TetrisEventHandler(LineClearEnque);
        tetris.HoldEvent += new TetrisEventHandler(HoldEnque);
        tetris.PlaceEvent += new TetrisEventHandler(PlaceEnque);
        tetris.Play();

        Thread inputThread = new(InputThread);
        inputThread.Start();

        while (true)
        {
            DrawMap();
            ShowLineClearText();
            ShowHoldingBlock();
            ShowNextBlocks();
        }
    }

    /// <summary> TetrisEventHandler </summary>
    static void MapEnque(TetrisEventArgs? e)
    {
        eventQueue |= EventQueue.Map;
    }

    /// <summary> TetrisEventHandler </summary>
    static void LineClearEnque(TetrisEventArgs? e)
    {
        eventQueue |= EventQueue.LineClear;
        lineClearCount = e?.lineClearCount ?? 0;
        b2bCombo = e?.b2bCombo ?? 0;
    }

    /// <summary> TetrisEventHandler </summary>
    static void HoldEnque(TetrisEventArgs? e)
    {
        eventQueue |= EventQueue.Hold;
    }

    /// <summary> TetrisEventHandler </summary>
    static void PlaceEnque(TetrisEventArgs? e)
    {
        eventQueue |= EventQueue.Place;
    }

    /// <summary> 테트리스 맵을 새로 그린다. </summary>
    static void DrawMap()
    {
        if (eventQueue.HasFlag(EventQueue.Map))
        {
            for (int i = 0; i < Tetris.Height; i++)
            {
                Console.SetCursorPosition(offset.x, offset.y + i);
                for (int j = 0; j < Tetris.Width; j++)
                {
                    if (tetris.PositionOfCurrentBlock[i, j] == 1)
                    {
                        Console.Write("▣");
                    }
                    else
                    {
                        Console.Write(tetris.Map[i, j] >= 1 ? "■" : "□");
                    }
                }
            }

            eventQueue &= ~EventQueue.Map;
        }
    }

    /// <summary> 라인 수에 따라 Tetris 등의 텍스트를 옆에 띄운다. </summary>
    static void ShowLineClearText()
    {
        if (eventQueue.HasFlag(EventQueue.LineClear))
        {
            Console.SetCursorPosition(offset.x - 8, offset.y + Block.MaximumSquareSize + 1);
            Console.Write(lineClearCount switch
            {
                1 => "Single",
                2 => "Double",
                3 => "Triple",
                4 => "Tetris",
                _ => ""
            });
            if (b2bCombo > 1)
            {
                Console.SetCursorPosition(offset.x - 8, offset.y + Block.MaximumSquareSize + 1 + 1);
                Console.Write($"{b2bCombo} Combo");
            }

            Thread.Sleep(500);
            Console.SetCursorPosition(offset.x - 8, offset.y + Block.MaximumSquareSize + 1);
            Console.Write("      ");
            Console.SetCursorPosition(offset.x - 8, offset.y + Block.MaximumSquareSize + 1 + 1);
            Console.Write("        ");

            eventQueue &= ~EventQueue.LineClear;
        }
    }

    /// <summary> 홀드 칸에 있는 조각을 보여준다. </summary>
    static void ShowHoldingBlock()
    {
        if (eventQueue.HasFlag(EventQueue.Hold))
        {
            if (tetris.HoldingBlock is not BlockType.None)
            {
                var holding = Block.BlockTypeToIntArray(tetris.HoldingBlock);
                for (int i = 0; i < holding.GetLength(0); i++)
                {
                    Console.SetCursorPosition(offset.x - (Block.MaximumSquareSize*2 + 2), offset.y + 1 + i);
                    for (int j = 0; j < holding.GetLength(1); j++)
                    {
                        Console.Write(holding[i, j] == 1 ? "■" : "　");
                    }
                }
            }

            eventQueue &= ~EventQueue.Hold;
        }
    }

    /// <summary> 다음에 나올 블록 리스트를 보여준다. </summary>
    static void ShowNextBlocks()
    {
        if (eventQueue.HasFlag(EventQueue.Place))
        {
            for (int order = 0; order < tetris.BagSize; order++)
            {
                var block = Block.BlockTypeToIntArray(tetris.Bag[order]);
                for (int i = 0; i < Block.MaximumSquareSize; i++)
                {
                    Console.SetCursorPosition(offset.x + Tetris.Width*2 + 2, offset.y + order*Block.MaximumSquareSize + 2 + i);
                    for (int j = 0; j < Block.MaximumSquareSize; j++)
                    {
                        Console.Write((i < block.GetLength(0) && j < block.GetLength(1) && block[i, j] == 1) ? "■" : "　");
                    }
                }
            }

            eventQueue &= ~EventQueue.Place;
        }
    }

    /// <summary> 키 입력을 받는 스레드 </summary>
    static void InputThread()
    {
        while (true)
        {
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.RightArrow:
                    tetris.InputRight();
                    break;
                case ConsoleKey.LeftArrow:
                    tetris.InputLeft();
                    break;
                case ConsoleKey.DownArrow:
                    tetris.SoftDrop();
                    break;
                case ConsoleKey.Spacebar:
                    tetris.HardDrop();
                    break;
                case ConsoleKey.UpArrow:
                    tetris.RotateRight();
                    break;
                case ConsoleKey.Z:
                    tetris.RotateLeft();
                    break;
                case ConsoleKey.X:
                    tetris.RotateRight();
                    break;
                case ConsoleKey.A:
                    tetris.Rotate180();
                    break;
                case ConsoleKey.C:
                    tetris.Hold();
                    break;
            }
        }
    }
}
