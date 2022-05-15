using System.Text;
using TetrisCS;

class Program
{
    static readonly Tetris tetris = new();

    static readonly Vector offset = new Vector(10, 0);


    static void Main()
    {
        Console.CursorVisible = false;

        tetris.MapUpdateEvent += new MapUpdateEventHandler(DrawMap);
        tetris.LineClearEvent += new LineClearEventHandler(LineClearText);
        tetris.HoldEvent += new HoldEventHandler(ShowHoldingBlock);
        tetris.Play();

        Thread inputThread = new(InputThread);
        inputThread.Start();
    }

    /// <summary> 테트리스 맵을 새로 그린다. (MapUpdateEventHandler) </summary>
    static void DrawMap()
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
    }

    /// <summary> 라인 수에 따라 Tetris 등의 텍스트가 옆에 뜬다. (LineClearEventHandler) </summary>
    static void LineClearText(int lineClearCount, int b2bCombo)
    {
        Console.SetCursorPosition(offset.x + Tetris.WIDTH*2 + 2, offset.y + 8);
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
        Console.SetCursorPosition(offset.x + Tetris.WIDTH*2 + 2, offset.y + 8);
        Console.Write("      ");
        Console.SetCursorPosition(offset.x + Tetris.WIDTH * 2 + 2, offset.y + 9);
        Console.Write("        ");
    }

    /// <summary> 홀드 칸에 있는 조각을 보여준다. (HoldEventHandler) </summary>
    static void ShowHoldingBlock()
    {
        if (tetris.HoldingBlock is not BlockType.None)
        {
            var holding = Block.BlockTypeToIntArray(tetris.HoldingBlock);
            for (int i = 0; i < holding.GetLength(0); i++)
            {
                Console.SetCursorPosition(offset.x + Tetris.WIDTH*2 + 2, offset.y + 1 + i);
                for (int j = 0; j < holding.GetLength(1); j++)
                {
                    Console.Write(holding[i, j] == 1 ? "■" : "　");
                }
            }
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
