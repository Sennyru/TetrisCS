using TetrisCS;

namespace Program;

/* 
 * 콘솔 커서를 여러 스레드에서 같이 이동시키면 제대로 작동하지 않기 때문에 한 스레드(Main())에서만 콘솔 커서를 옮긴다.
 * Tetris에서 (화면을 새로 그려야 하는) 이벤트가 들어오면 창을 업데이트시켜야 한다는 플래그만 켜놓는다.
 * 그리고 while 게임 루프에서 각각의 플래그 여부를 체크해서 순서대로 업데이트를 시킨다.
 */

[Flags]
internal enum EventFlag
{
    MapUpdate       = 1 << 0,
    LineClear       = 1 << 1,
    Hold            = 1 << 2,
    Place           = 1 << 3,
    RemoveLineClear = 1 << 4,
    Debug           = 1 << 5,
}

internal static class Program
{
    private static readonly Tetris tetris = new();
    private static readonly Vector offset = new Vector(50, 0);

    private static EventFlag eventFlag;
    private static int lineClearCount;
    private static int b2bCombo;
    private static string? debugMessage;
    private static int debugCount;


    private static void Main()
    {
        Console.CursorVisible = false;

        tetris.OnMapUpdated += MapEnqueue;
        tetris.OnLineCleared += LineClearEnqueue;
        tetris.OnHold += HoldEnqueue;
        tetris.OnPlaced += PlaceEnqueue;
        tetris.OnDebugMessageSent += DebugEnqueue;
        tetris.Play();

        Thread inputThread = new(InputThread);
        inputThread.Start();

        while (tetris.Playing)
        {
            DrawMap();
            ShowLineClearText();
            ShowHoldingBlock();
            ShowNextBlocks();
            ShowDebugMessage();
        }
        GameOver();
    }


    #region Tetris Event Handlers
    private static void MapEnqueue(TetrisEventArgs? e)
    {
        eventFlag |= EventFlag.MapUpdate;
    }

    private static void LineClearEnqueue(TetrisEventArgs? e)
    {
        eventFlag |= EventFlag.LineClear;
        lineClearCount = e?.LineClearCount ?? 0;
        b2bCombo = e?.B2BCombo ?? 0;
    }

    private static void HoldEnqueue(TetrisEventArgs? e)
    {
        eventFlag |= EventFlag.Hold;
    }

    private static void PlaceEnqueue(TetrisEventArgs? e)
    {
        eventFlag |= EventFlag.Place;
    }

    private static void DebugEnqueue(TetrisEventArgs? e)
    {
        debugMessage = e?.DebugMessage ?? string.Empty;
        eventFlag |= EventFlag.Debug;
    }
    #endregion


    #region Update
    /// <summary> 테트리스 맵을 새로 그린다. </summary>
    private static void DrawMap()
    {
        if (!eventFlag.HasFlag(EventFlag.MapUpdate)) return;
        
        for (int i = 0; i < tetris.Height; i++)
        {
            Console.SetCursorPosition(offset.x, offset.y + i);
            for (int j = 0; j < tetris.Width; j++)
            {
                if (tetris.PositionOfCurrentBlock[i, j] == true)
                {
                    Console.ForegroundColor = tetris.CurrentBlock.Type.ToBlockColor();
                    Console.Write("▣");
                }
                else if (tetris.Ghost[i, j] == true)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("▩");
                }
                else
                {
                    if (tetris.Map[i, j] >= 1)
                    {
                        Console.ForegroundColor = ((BlockType)tetris.Map[i, j]).ToBlockColor();
                        Console.Write("■");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("□");
                    }
                }
            }
        }

        eventFlag &= ~EventFlag.MapUpdate;
    }

    /// <summary> 라인 수에 따라 Double 등의 텍스트를 옆에 띄운다. </summary>
    private static void ShowLineClearText()
    {
        if (eventFlag.HasFlag(EventFlag.LineClear))
        {
            eventFlag &= ~EventFlag.RemoveLineClear;

            Console.SetCursorPosition(offset.x - 8, offset.y + Block.MaximumSquareSize + 1);
            Console.ForegroundColor = ConsoleColor.White;
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

            eventFlag &= ~EventFlag.LineClear;
            Task.Delay(500).ContinueWith(t => eventFlag |= EventFlag.RemoveLineClear); // 0.5초 뒤에 텍스트 삭제
        }

        else if (eventFlag.HasFlag(EventFlag.RemoveLineClear))
        {
            Console.SetCursorPosition(offset.x - 8, offset.y + Block.MaximumSquareSize + 1);
            Console.Write("      ");
            Console.SetCursorPosition(offset.x - 8, offset.y + Block.MaximumSquareSize + 1 + 1);
            Console.Write("        ");
            eventFlag &= ~EventFlag.RemoveLineClear;
        }
    }

    /// <summary> 홀드에 있는 조각을 보여준다. </summary>
    private static void ShowHoldingBlock()
    {
        if (!eventFlag.HasFlag(EventFlag.Hold)) return;
        
        if (tetris.HoldingBlock is not BlockType.None)
        {
            var holding = Block.BlockTypeToIntArray(tetris.HoldingBlock);
            for (int y = 0; y < Block.MaximumSquareSize; y++)
            {
                Console.SetCursorPosition(offset.x - (Block.MaximumSquareSize * 2 + 2), offset.y + 1 + y);
                for (int x = 0; x < Block.MaximumSquareSize; x++)
                {
                    Console.ForegroundColor = tetris.HoldingBlock.ToBlockColor();
                    Console.Write((y < holding.GetLength(0) && x < holding.GetLength(1) && holding[y, x] == 1) ? "■" : "　");
                }
            }
        }

        eventFlag &= ~EventFlag.Hold;
    }

    /// <summary> 다음에 나올 블록 리스트를 보여준다. </summary>
    private static void ShowNextBlocks()
    {
        if (!eventFlag.HasFlag(EventFlag.Place)) return;
        
        int yOffset = 0;
        foreach (var type in tetris.NextBlockQueue)
        {
            var block = Block.BlockTypeToIntArray(type);
            for (int y = 0; y < Block.MaximumSquareSize; y++)
            {
                Console.SetCursorPosition(offset.x + tetris.Width * 2 + 2, offset.y + yOffset * Block.MaximumSquareSize + 2 + y);
                for (int x = 0; x < Block.MaximumSquareSize; x++)
                {
                    Console.ForegroundColor = type.ToBlockColor();
                    Console.Write((y < block.GetLength(0) && x < block.GetLength(1) && block[y, x] == 1) ? "■" : "　");
                }
            }
            yOffset++;
        }

        eventFlag &= ~EventFlag.Place;
    }

    private static void ShowDebugMessage()
    {
        if (!eventFlag.HasFlag(EventFlag.Debug)) return;
        
        Console.SetCursorPosition(0, debugCount++ % 30);
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write($"[{debugCount}] {debugMessage ?? string.Empty}");

        eventFlag &= ~EventFlag.Debug;
    }


    /// <summary> 게임이 끝났을 때 실행된다. </summary>
    private static void GameOver()
    {
        const string text = "ＧＡＭＥＯＶＥＲ";
        Console.SetCursorPosition(offset.x + tetris.Width - text.Length, offset.y + tetris.Height / 2);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        ExitGame();
    }

    /// <summary> 게임과 콘솔을 종료한다. </summary>
    private static void ExitGame()
    {
        Console.SetCursorPosition(0, offset.y + tetris.Height + 1);
        Console.ForegroundColor = ConsoleColor.Gray;
        Environment.Exit(0);
    }
    #endregion


    /// <summary> 키 입력을 받는 스레드 </summary>
    private static void InputThread()
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
                case ConsoleKey.Escape:
                    ExitGame();
                    break;
            }
        }
    }
}
