using TetrisCS;

class Program
{
    static readonly Tetris tetris = new();

    static void Main()
    {
        tetris.Play();
        Console.CursorVisible = false;

        Thread inputThread = new(InputThread);
        inputThread.Start();

        while (true)
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(tetris.GetStringMap());

            Console.SetCursorPosition(Tetris.WIDTH * 2 + 2, 1);
            Console.Write(tetris.CurrentBlock.pos + "  ");
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
            }
        }
    }
}
