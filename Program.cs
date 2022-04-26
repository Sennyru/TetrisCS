using TetrisCS;

class Program
{
    static void Main()
    {
        Tetris tetris = new();
        tetris.Play();
        Console.CursorVisible = false;

        while (true)
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(tetris.GetStringMap());

            Console.SetCursorPosition(Tetris.WIDTH * 2 + 2, 1);
            Console.Write(tetris.CurrentBlock.pos);
        }
    }
}
