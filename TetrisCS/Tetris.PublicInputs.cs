namespace TetrisCS;

public partial class Tetris
{
    #region Public Inputs
    /// <summary> 오른쪽 키 누르기 </summary>
    public void InputRight()
    {
        MoveBlockTo(Vector.Right);
    }

    /// <summary> 왼쪽 키 누르기 </summary>
    public void InputLeft()
    {
        MoveBlockTo(Vector.Left);
    }

    /// <summary> 블록 한 칸 아래로 떨어뜨리기 </summary>
    public void SoftDrop()
    {
        MoveBlockTo(Vector.Down);
    }

    /// <summary> 블록을 바닥에 한 번에 떨어뜨리기 </summary>
    public void HardDrop()
    {
        MoveBlockTo(Vector.Down, hardDrop: true);
    }

    /// <summary> 블록을 오른쪽으로 90도 회전시키기 </summary>
    public void RotateRight()
    {
        Rotate(isClockwise: true);
    }

    /// <summary> 블록을 왼쪽으로 90도 회전시키기 </summary>
    public void RotateLeft()
    {
        Rotate(isClockwise: false);
    }

    /// <summary> 블록을 180도 회전시키기 (뒤집기) </summary>
    public void Rotate180()
    {
        Rotate(isClockwise: null);
    }

    /// <summary> 홀드 </summary>
    public void Hold()
    {
        Holding();
    }
    #endregion
}
