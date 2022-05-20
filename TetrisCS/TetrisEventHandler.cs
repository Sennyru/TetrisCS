namespace TetrisCS
{
    public delegate void TetrisEventHandler(TetrisEventArgs? e = null);

    public class TetrisEventArgs : EventArgs
    {
        /// <summary> 제거한 줄 개수 </summary>
        public int? LineClearCount { get; set; }
        /// <summary> 현재 Back to Back 콤보 횟수 </summary>
        public int? B2bCombo { get; set; }
    }
}
