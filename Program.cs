using TetrisCS;

namespace Program
{
    public class Program
    {
        static Tetris tetris = new();


        static void Main()
        {
            tetris.GravityTick = 500;
            tetris.Play();

            Thread inputThread = new(GetKeyDown);
            inputThread.Start();

            while (true)
            {
                tetris.DrawMapOnConsoleScreen();

                // debug
                Console.SetCursorPosition(Tetris.ScreenHeight + 2, 1);
                Console.Write(tetris.CurrentPiece.pos + "  ");
            }
        }

        static void GetKeyDown()
        {
            while (true)
            {
                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.RightArrow or ConsoleKey.D:

                        break;

                    case ConsoleKey.LeftArrow or ConsoleKey.A:

                        break;

                    case ConsoleKey.DownArrow or ConsoleKey.S:
                        tetris.SoftDrop();
                        break;

                    case ConsoleKey.Spacebar:
                        tetris.HardDrop();
                        break;

                }
            }
        }
    }
}
