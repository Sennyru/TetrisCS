namespace TetrisCS
{
    public delegate void TetrisEventHandler(TetrisEventArgs? e);

    public class TetrisEventArgs : EventArgs
    {
        public int? LineClearCount { get; set; }
        public int? B2bCombo { get; set; }
    }
}
