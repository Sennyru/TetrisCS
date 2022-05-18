namespace TetrisCS
{
    public delegate void TetrisEventHandler(TetrisEventArgs? e);

    public struct TetrisEventArgs : EventArgs
    {
        public int? lineClearCount { get; set; }
        public int? b2bCombo { get; set; }
    }
}
