using System.Text;
using TetrisCS;

class Program
{
    static readonly Tetris tetris = new();
    static readonly Vector offset = new Vector(10, 0);

    static EventQueue eventQueue;
    static int lineClearCount;
    static int b2bCombo;

    [Flags] enum EventQueue
    {
        Map = 1 << 0,
        LineClear = 1 << 1,
        Holding = 1 << 2,
    }


    static void Main()
    {
        Console.CursorVisible = false;

        tetris.MapUpdateEvent += new MapUpdateEventHandler(MapEnque);
        tetris.LineClearEvent += new LineClearEventHandler(LineClearEnque);
        tetris.HoldEvent += new HoldEventHandler(HoldingEnque);
        tetris.Play();

        Thread inputThread = new(InputThread);
        inputThread.Start();

        while (true)
        {
            DrawMap();
            ShowLineClearText();
            ShowHoldingBlock();
        }
    }

    #region Event Handlers
    /// <summary> MapUpdateEventHandler </summary>
    static void MapEnque()
    {
        eventQueue |= EventQueue.Map;
    }

    /// <summary> LineClearEventHandler </summary>
    static void LineClearEnque(int lineClearCount, int b2bCombo)
    {
        eventQueue |= EventQueue.LineClear;
        Program.lineClearCount = lineClearCount;
        Program.b2bCombo = b2bCombo;
    }

    /// <summary> HoldEventHandler </summary>
    static void HoldingEnque()
    {
        eventQueue |= EventQueue.Holding;
    }
    #endregion

    #region Tetris
    /// <summary> 테트리스 맵을 새로 그린다. </summary>
    static void DrawMap()
    {
        if (eventQueue.HasFlag(EventQueue.Map))
        {
            for (int i = 0; i < Tetris.HEIGHT; i++)
            {
                Console.SetCursorPosition(offset.x, offset.y + i);
                for (int j = 0; j < Tetris.WIDTH; j++)
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

    /// <summary> 라인 수에 따라 Tetris 등의 텍스트를 옆에 띄운다. (LineClearEventHandler) </summary>
    static void ShowLineClearText()
    {
        if (eventQueue.HasFlag(EventQueue.LineClear))
        {
            Console.SetCursorPosition(offset.x + Tetris.WIDTH * 2 + 2, offset.y + 8);
            Console.Write(lineClearCount switch
            {
                1 => "Single",
                2 => "Double",
                3 => "Triple",
                4 => "Tetris",
                _ => ""
            });
            if (b2bCombo > 0)
            {
                Console.SetCursorPosition(offset.x + Tetris.WIDTH * 2 + 2, offset.y + 9);
                Console.Write($"{b2bCombo} Combo");
            }

            Thread.Sleep(1000);
            Console.SetCursorPosition(offset.x + Tetris.WIDTH * 2 + 2, offset.y + 8);
            Console.Write("      ");
            Console.SetCursorPosition(offset.x + Tetris.WIDTH * 2 + 2, offset.y + 9);
            Console.Write("        ");

            eventQueue &= ~EventQueue.LineClear;
        }
    }

    /// <summary> 홀드 칸에 있는 조각을 보여준다. (HoldEventHandler) </summary>
    static void ShowHoldingBlock()
    {
        if (eventQueue.HasFlag(EventQueue.Holding))
        {
            if (tetris.HoldingBlock is not BlockType.None)
            {
                var holding = Block.BlockTypeToIntArray(tetris.HoldingBlock);
                for (int i = 0; i < holding.GetLength(0); i++)
                {
                    Console.SetCursorPosition(offset.x + Tetris.WIDTH * 2 + 2, offset.y + 1 + i);
                    for (int j = 0; j < holding.GetLength(1); j++)
                    {
                        Console.Write(holding[i, j] == 1 ? "■" : "　");
                    }
                }
            }
        }
    }
    #endregion

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
