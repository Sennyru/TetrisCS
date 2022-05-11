namespace TetrisCS
{
    /// <summary> 블록 모양의 종류 </summary>
    public enum BlockType { I, J, L, O, S, T, Z }

    static class BlockTypeExtensions
    {
        public static int a(this BlockType b) => 1;
    }
}
