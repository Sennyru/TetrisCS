

namespace TetrisCS
{
    public struct Vector
    {
        public int x, y;

        public Vector(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary> new Vector(0, 1) <br/> 좌표계가 다르므로 유의. </summary>
        public static Vector Down => new(0, 1);
        /// <summary> new Vector(1, 0) </summary>
        public static Vector Right => new(1, 0);
        /// <summary> new Vector(-1, 0) </summary>
        public static Vector Left => new(-1, 0);

        public override string ToString() => $"({x}, {y})";
    }

    public struct Piece
    {
        public Vector pos;
        public int[,] block = new int[,]
        {
            { 0, 1, 0, },
            { 1, 1, 1, },
            { 0, 0, 0, },
        };

        public Piece(PieceType type)
        {
            pos = new Vector(Tetris.ScreenWidth / 2 - block.GetLength(1) / 2, 0);
        }

        public int Width => block.GetLength(1);
        public int Height => block.GetLength(0);

        public int this[int y, int x] => block[y, x];
    }

    public enum PieceType { T, Count }


    public class Tetris
    {
        public const int ScreenWidth = 10, ScreenHeight = 20;
        readonly Vector screenOffset = new(0, 0);

        private int[,] map = new int[ScreenHeight, ScreenWidth];
        private Piece currentPiece;
        private int gravityTick = 100;

        public Piece CurrentPiece => currentPiece;
        /// <summary> ms </summary>
        public int GravityTick { get => gravityTick; set => gravityTick = value; }


        /// <summary> 테트리스 시작 </summary>
        public void Play()
        {
            Console.CursorVisible = false;

            NextPiece();

            Thread fallThread = new(FallThread);
            fallThread.Start();
        }

        /// <summary> 다음 조각 생성 </summary>
        private void NextPiece()
        {
            currentPiece = new Piece(PieceType.T);
            SpawnPieceOnMap();
        }

        /// <summary> 조각을 새로 맵에 올린다. </summary>
        private void SpawnPieceOnMap()
        {
            for (int i = 0; i < currentPiece.Height; i++)
            {
                for (int j = 0; j < currentPiece.Width; j++)
                {
                    map[currentPiece.pos.y + i, currentPiece.pos.x + j] = currentPiece[i, j];
                }
            }
        }

        /// <summary> 매 시간마다 블록을 한 칸씩 아래로 떨어트린다 </summary>
        private void FallThread()
        {
            while (true)
            {
                Thread.Sleep(gravityTick);

                SoftDrop();
            }
        }

        public void SoftDrop()
        {
            if (CanMove())
            {
                MovePieceTo(Vector.Down);
            }
            else
            {
                NextPiece();
            }
        }

        /// <summary> 아래로 이동할 수 있는지 없는지 체크 </summary>
        private bool CanMove()
        {
            for (int j = currentPiece.Width-1; j >= 0; j--)
            {
                for (int i = currentPiece.Height - 1; i >= 0; i--)
                {
                    if (currentPiece[i, j] == 1)
                    {
                        int nextY = currentPiece.pos.y + i + 1;
                        // 다음에 바닥이나 블록에 닿는다면
                        if (nextY >= ScreenHeight || map[nextY, currentPiece.pos.x + j] == 1)
                        {
                            return false;
                        }

                        break;
                    }
                }
            }

            return true;
        }

        /// <summary> 현재 조각을 아래로 한 칸 내리기 </summary>
        /// <param name="dir"> Vector.Down </param>
        private void MovePieceTo(Vector dir)
        {
            for (int i = currentPiece.Height - 1; i >= 0; i--)
            {
                for (int j = 0; j < currentPiece.Width; j++)
                {
                    if (currentPiece[i, j] == 1)
                    {
                        map[currentPiece.pos.y + i, currentPiece.pos.x + j] = 0;
                        map[currentPiece.pos.y + i + dir.y, currentPiece.pos.x + j] = 1;
                    }
                }
            }
            currentPiece.pos.y++;
        }

        public void HardDrop()
        {
            while (CanMove())
            {
                SoftDrop();
            }
        }

        /// <summary> 현재 맵을 콘솔 화면에 그린다. </summary>
        public void DrawMapOnConsoleScreen()
        {
            for (int i = 0; i < ScreenHeight; i++)
            {
                for (int j = 0; j < ScreenWidth; j++)
                {
                    Console.SetCursorPosition(screenOffset.x + j*2, screenOffset.y + i);
                    Console.Write(map[i, j] == 1 ? '■' : '□');
                }
            }
        }

    }
}
