namespace TetrisCS
{
    public delegate void TetrisEventHandler(TetrisEventArgs? e);

    public struct TetrisEventArgs
    {
        public int? lineClearCount, b2bCombo;
    }
}
